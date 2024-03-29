﻿using System;
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
using System.IO;
//Configuration Files
using System.Configuration;
//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;
//API Layer
using MBBetaAPI;
using MBBetaAPI.AgentAPI;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBLogin.xaml
    /// </summary>
    public partial class MBLogin : Window
    {
        public MBLogin()
        {
            InitializeComponent();

            if(!IsNewProduct())
            {
            ReadLastConfig();
            ReadDBConfig();
            FocusManager.SetFocusedElement(this, UserLoginTB);
            }

        }

        /// <summary>
        /// Constructor coming from Initial Setup
        /// </summary>
        /// <param name="UserParam"></param>
        /// <param name="PasswordParam"></param>
        /// <param name="DBNameParam"></param>
        /// <param name="DBPathParam"></param>
        /// <param name="InputCulture"></param>
        public MBLogin(string UserParam, string PasswordParam, string DBNameParam, string DBPathParam, string InputCulture)
        {
            InitializeComponent();

            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(InputCulture);

            UserLoginTB.Text = UserParam;
            PasswordLoginTB.Password = PasswordParam;
            DBName = DBNameParam;
            DBPath = DBPathParam;
            FileLocation = DBPath + "\\" + DBName;

            conn = "Data Source=" + FileLocation + ";" + "Password =" + PasswordLoginTB.Password;
            BackupFileTB.Text = DBName;

            conn_prefix = "Data Source=" + DBPath + "\\" + DBName;

            //Check if file exists
            if (System.IO.File.Exists(FileLocation))
            {
                conn_prefix = conn_prefix + ";";              //Prepare for password append
                BackupFileTB.Text = DBName;
            }

        }

        //*********** Attributes
        #region Attributes

        string CurrentCulture;
        int LoginAttempts = 1;
        string conn_prefix;
        string conn;
        string DBName;
        string DBPath;
        string FileLocation;

        #endregion

        //Private Methods
        #region Private Methods

        private bool IsNewProduct()
        {
            bool NewP = false;
            try
            {
                string NewProduct = Properties.Settings.Default.NewProduct;
                if (NewProduct == "YES")
                {
                    NewP = true;
                    SetupBt_Click(null, null);
                }
            }
            catch
            {
                MBError error = new MBError(this, "Login: Reading Application configuration", 1);
            }
            return NewP;
        }

        private void ReadLastConfig()
        {
            try
            {
                string LastLoginUser = Properties.Settings.Default.LastLoginUser;
                UserLoginTB.Text = LastLoginUser;
                //Read last selected language
                CurrentCulture = Properties.Settings.Default.LastCulture;
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(CurrentCulture);
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
                DBName = Properties.Settings.Default.DBName;
                DBPath = Properties.Settings.Default.DBPath;
                FileLocation = DBPath + DBName;

                conn_prefix = "Data Source=" + DBPath + "\\" + DBName;

                //Check if file exists
                if (System.IO.File.Exists(FileLocation))
                {
                    conn_prefix = conn_prefix + ";";              //Prepare for password append
                    BackupFileTB.Text = DBName;
                }
                else
                {
                    //TODO: Goto setup after error
                    MessageBoxButton MBBt = MessageBoxButton.YesNo;
                    LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:DBFileNotPresent");
                    string message;
                    loc.ResolveLocalizedValue(out message);
                    message += ". File: " + DBName;
                    MessageBoxResult res = MessageBox.Show(message, "Error", MBBt, MessageBoxImage.Error);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            ChangeFileBt_Click(null, null);
                            break;
                        case MessageBoxResult.No:
                            break;
                        default:
                            // TODO: gt a local error, find how this can be possible or clean
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
            int level;

            conn = conn_prefix + "Password =" + PasswordLoginTB.Password;

            Login MyLogin = new Login(conn, UserLoginTB.Text, PasswordLoginTB.Password);

            level = MyLogin.ValidateLogin();

            return level;
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
            MBMain MainWindow;

            if (LoginAttempts <= 3)
            {
                //if user is OK, login and proceed
                result = ValidateUser();
                //MessageBox.Show("Result: " + result.ToString());
                switch (result)
                {
                    //level (0 - wrong File or Passwd, 1 - wrong user , 2 - no SNAccount, 3 - SNAccount)
                    case 0:
                        loc = new LocTextExtension("MBBeta2:LoginStrings:Login_WrongLogin");
                        success = loc.SetBinding(ErrorMessageTB, TextBlock.TextProperty);
                        AttemptsMsg = "(" + LoginAttempts.ToString() + "/3)";
                        LoginAttemptsTB.Text = AttemptsMsg;
                        LoginAttempts++;
                        break;
                    case 1:
                        loc = new LocTextExtension("MBBeta2:LoginStrings:Login_WrongUser");
                        success = loc.SetBinding(ErrorMessageTB, TextBlock.TextProperty);
                        AttemptsMsg = "(" + LoginAttempts.ToString() + "/3)";
                        LoginAttemptsTB.Text = AttemptsMsg;
                        LoginAttempts++;
                        break;
                        // TODO: Double check the effect was intended, when commenting everything on case 2
                    case 2:  //Enter SNAccount Setup
                    case 3:
                        this.Cursor = Cursors.Wait;
                        //Save settings to config file
                        Properties.Settings.Default.DBName = DBName;
                        if (DBPath.LastIndexOf("\\") != DBPath.Length - 1)
                        {
                            DBPath += "\\";
                        }
                        Properties.Settings.Default.DBPath = DBPath;
                        Properties.Settings.Default.LastLoginUser = UserLoginTB.Text;
                        Properties.Settings.Default.LastCulture = CurrentCulture;
                        Properties.Settings.Default.Save();

                        // Moved initialization from MBSetup, since we need these variables not only the first time
                        AsyncReqQueue.ProfilePhotoDestinationDir = DBPath + "fb\\ProfilePics\\";
                        AsyncReqQueue.AlbumDestinationDir = DBPath + "fb\\Albums\\";
                        if (!Directory.Exists(AsyncReqQueue.ProfilePhotoDestinationDir))
                        {
                            Directory.CreateDirectory(AsyncReqQueue.ProfilePhotoDestinationDir);
                        }
                        if (!Directory.Exists(AsyncReqQueue.AlbumDestinationDir))
                        {
                            Directory.CreateDirectory(AsyncReqQueue.AlbumDestinationDir);
                        }

                        //Open main with loaded info
                        this.Hide();
                        MainWindow = new MBMain(CurrentCulture, UserLoginTB.Text, DBPath, conn, result);
                        MainWindow.Show();
                        this.Close();
                        break;
                }

            }
            else
            {
                loc = new LocTextExtension("MBBeta2:LoginStrings:ManyAttempts");
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

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = DBName; // Default file name
            dlg.DefaultExt = ".db"; // Default file extension
            dlg.Filter = "Backup Files (.db)|*.db"; // Filter files by extension
            dlg.InitialDirectory = DBPath;

            // Show open file dialog box
            Nullable<bool> result2 = dlg.ShowDialog();

            // Process open file dialog box results
            if (result2 == true)
            {
                // Open document
                string FilePath = dlg.FileName;
                DBName = System.IO.Path.GetFileName(FilePath);
                DBPath = System.IO.Path.GetDirectoryName(FilePath);
                BackupFileTB.Text = DBName;
                FileLocation = DBPath + "\\" + DBName;
                conn_prefix = "Data Source=" + DBPath + "\\" + DBName + ";";
            }

        }


        private void SetupBt_Click(object sender, RoutedEventArgs e)
        {

            bool setup = false;

            //If file exists, warn the user
            if (System.IO.File.Exists(FileLocation))
            {
                MessageBoxButton MBBt = MessageBoxButton.YesNo;
                LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:WarnDBFilePresent");
                string message;
                loc.ResolveLocalizedValue(out message);
                MessageBoxResult res = MessageBox.Show(message, "Warning", MBBt, MessageBoxImage.Warning);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        //Call Initial Setup Window
                        setup = true;
                        break;
                    default:
                        break;

                }
            }
            else
            {
                //Call Setup
                setup = true;
            }

            if (setup)
            {
                var InitialSetupWindow = new InitialSetup();
                InitialSetupWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                InitialSetupWindow.Show();
                this.Close();
            }


        }

        #endregion

        

        

        

    }
}
