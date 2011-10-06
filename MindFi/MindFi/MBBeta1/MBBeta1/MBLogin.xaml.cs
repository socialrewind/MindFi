using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//Configuration Files
using System.Configuration;
//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;
//API Layer
using MBBetaAPI;

namespace MBBeta1
{
    /// <summary>
    /// Interaction logic for MBLogin.xaml
    /// </summary>
    public partial class MBLogin : Window
    {
        public MBLogin()
        {
            InitializeComponent();

            ReadLastConfig();
            ReadDBConfig();

            FocusManager.SetFocusedElement(this, UserLoginTB);
            //Keyboard.Focus(UserLoginTB);

        }

        //*********** Attributes
        #region Attributes

        string CurrentCulture;
        int LoginAttempts = 1;
        string conn_prefix;
        string conn;
        string DBName;
        string DBPath;

        #endregion

        //Private Methods
        #region Private Methods

        private void ReadLastConfig()
        {
            try
            {
                string LastLoginUser = ConfigurationManager.AppSettings["LastLoginUser"];
                UserLoginTB.Text = LastLoginUser;
            }
            catch
            {
                MBError error = new MBError(this, "Login: Reading Application configuration", 1);
            }
        }

        private void ReadDBConfig()
        {
            try
            {
                DBName = ConfigurationManager.AppSettings["DBName"];
                DBPath = ConfigurationManager.AppSettings["DBPath"];

                conn_prefix = "Data Source=" + DBPath + DBName;

                //Check if file exists
                if (System.IO.File.Exists(DBPath + DBName))
                {
                    conn_prefix = conn_prefix + ";";              //Prepare for password append
                    BackupFileTB.Text = DBName;
                }
                else
                {
                    //TODO: Goto setup after error
                    MessageBoxButton MBBt = MessageBoxButton.YesNo;
                    LocTextExtension loc = new LocTextExtension("MBBeta1:LoginStrings:DBFileNotPresent");
                    string message;
                    loc.ResolveLocalizedValue(out message);
                    message += ". File: " + DBName;
                    MessageBoxResult res = MessageBox.Show(message, "Error", MBBt);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            ChangeFileBt_Click(null, null);
                            //conn_prefix = "Data Source=" + DBPath + "\\" + DBName + ";";
                            break;
                        case MessageBoxResult.No:
                            this.Close();
                            break;
                        default:
                            MessageBox.Show("default");
                            break;

                    }
                }

            }
            catch
            {
                MBError error = new MBError(this, "Login: Reading DB configuration from file.", 1);
            }
        }

        int ValidateUser()
        {
            int succces;

            conn = conn_prefix + "Password =" + PasswordLoginTB.Password;

            Login MyLogin = new Login(conn, UserLoginTB.Text, PasswordLoginTB.Password);

            succces = MyLogin.ValidateLogin();

            return succces;
        }


        #endregion

        //************** Control methods
        #region Control Methods

        private void LoginBt_Click(object sender, RoutedEventArgs e)
        {
            string AttemptsMsg;
            int result;
            LocTextExtension loc;
            bool success;

            if (LoginAttempts <= 3)
            {
                //if user is OK, login and proceed
                result = ValidateUser();
                switch (result)
                {
                    //success
                    case 0:
                        this.Cursor = Cursors.Wait;
                        //Save settings to config file
                        Configuration MBConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        MBConfig.AppSettings.Settings.Remove("DBName");
                        MBConfig.AppSettings.Settings.Remove("DBPath");
                        MBConfig.AppSettings.Settings.Remove("LastLoginUser");
                        MBConfig.AppSettings.Settings.Add("DBPath", DBPath + "\\");
                        MBConfig.AppSettings.Settings.Add("DBName", DBName);
                        MBConfig.AppSettings.Settings.Add("LastLoginUser", UserLoginTB.Text);
                        MBConfig.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");
                        var MainWindow = new MBMain(CurrentCulture, UserLoginTB.Text, conn);
                        MainWindow.Show();
                        this.Close();
                        break;
                    case 1:
                        loc = new LocTextExtension("MBBeta1:LoginStrings:Login_WrongLogin");
                        success = loc.SetBinding(ErrorMessageTB, TextBlock.TextProperty);
                        AttemptsMsg = "(" + LoginAttempts.ToString() + "/3)";
                        LoginAttemptsTB.Text = AttemptsMsg;
                        LoginAttempts++;
                        break;
                    case 2:
                        loc = new LocTextExtension("MBBeta1:LoginStrings:Login_DBError");
                        success = loc.SetBinding(ErrorMessageTB, TextBlock.TextProperty);
                        break;
                }

            }
            else
            {
                loc = new LocTextExtension("MBBeta1:LoginStrings:ManyAttempts");
                string message;
                loc.ResolveLocalizedValue(out message);
                MessageBox.Show(message);
                this.Close();
            }
        }

        private void LanguageSelectionCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (LanguageSelectionCB.SelectedIndex)
            {
                case 0:
                    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
                    CurrentCulture = "en-US";
                    break;
                case 1:
                    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("es");
                    CurrentCulture = "es";
                    break;
                //case 2:
                //    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("es-MX");
                //    break;
            }
        }

        private void Enter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }

        }

        private void PasswordLoginTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginBt_Click(null, null);
                e.Handled = true;
            }
        }

        private void ChangeFileBt_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("YES: Call Setup from here");

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = DBName; // Default file name
            dlg.DefaultExt = ".db"; // Default file extension
            dlg.Filter = "Backup Files (.db)|*.db"; // Filter files by extension
            dlg.InitialDirectory = DBPath;

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string FilePath = dlg.FileName;
                DBName = System.IO.Path.GetFileName(FilePath);
                DBPath = System.IO.Path.GetDirectoryName(FilePath);
                BackupFileTB.Text = DBName;
                conn_prefix = "Data Source=" + DBPath + "\\" + DBName + ";";
            }

        }

        #endregion

        

        

    }
}
