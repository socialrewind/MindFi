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
//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;
//Regex
using System.Text.RegularExpressions;
//Configuration Files
using System.Configuration;
//Local
//using MBBetaAgents;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for InitialSetup.xaml
    /// </summary>
    public partial class InitialSetup : Window
    {
        public InitialSetup()
        {
            InitializeComponent();
            GenerateDefaultBackupFile();
        }


        //************ Attributes ***************
        #region Attributes
        string DBName;
        string DBPath;

        string CurrentCulture;

        bool CleanClose;

        #endregion

        //************ Private Methods *************
        #region Private Methods
        bool VerifyUserPassword()
        {
            bool status = true;
            string message;

            Regex regex = new Regex("(^[0-9a-zA-Z_]+$)");
            if (!regex.IsMatch(UserTB.Text))
            {
                LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:WrongUser");
                loc.ResolveLocalizedValue(out message);
                MessageBox.Show(message);
                status = false;
            }

            if (PasswordPB.Password != PasswordVerificationPB.Password)
            {
                LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:PasswordsDoNotMatch");
                loc.ResolveLocalizedValue(out message);
                MessageBox.Show(message);
                status = false;
            }

            regex = new Regex("(?=^.{8,}$)((?=.*\\d)|(?=.*\\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$");
            if(!regex.IsMatch(PasswordPB.Password))
            {
                LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:WrongPassword");
                loc.ResolveLocalizedValue(out message);
                MessageBox.Show(message);
                status = false;
            }

            return status;
        }

        bool VerifyNewFile()
        {

            bool result = true;

            if (DBName == null || DBName == "")
            {
                result = false;
                LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:SelectNewFile");
                string message;
                loc.ResolveLocalizedValue(out message);
                MessageBox.Show(message);
            }

            return result;
        }

        void GenerateDefaultBackupFile()
        {
            DateTime Now = DateTime.Now;
            string tempD = Now.Year.ToString();
            if (Now.Month < 10) tempD += "0";
            tempD += Now.Month;
            if (Now.Day < 10) tempD += "0";
            tempD += Now.Day;

            string MyDocBase = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\My Social Rewind (Beta)\\";

            //Create dir
            MyDocBase += tempD;
            System.IO.Directory.CreateDirectory(MyDocBase);

            //Set variables to creat file when dialog closes
            string FilePath = MyDocBase + "\\" + tempD + ".db";
            DBName = System.IO.Path.GetFileName(FilePath);
            DBPath = System.IO.Path.GetDirectoryName(FilePath);
            BackupFileTB.Text = DBName;
        }
        #endregion

        private void SelectFileBt_Click(object sender, RoutedEventArgs e)
        {
            DateTime Now = DateTime.Now;
            string tempD = Now.Year.ToString();
            if (Now.Month < 10) tempD += "0";
            tempD += Now.Month;
            if (Now.Day < 10) tempD += "0";
            tempD += Now.Day;

            string MyDocBase = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\My Social Rewind (Beta)\\";

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            MyDocBase += tempD;
            System.IO.Directory.CreateDirectory(MyDocBase);
            dlg.InitialDirectory = MyDocBase;
            dlg.FileName = MyDocBase + "\\" + tempD + ".db";
            dlg.AddExtension = true;

            LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:FileFilter");
            string LocFilter;
            loc.ResolveLocalizedValue(out LocFilter);
            dlg.Filter = LocFilter; // "Database files (*.db)|*.db|All files (*.*)|*.*";
            dlg.FilterIndex = 1;

            string LocSelect;
            loc = new LocTextExtension("MBBeta2:LoginStrings:SelectDBName");
            loc.ResolveLocalizedValue(out LocSelect);
            dlg.Title = LocSelect;  // "Select the name for the new database";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string FilePath = dlg.FileName;
                DBName = System.IO.Path.GetFileName(FilePath);
                DBPath = System.IO.Path.GetDirectoryName(FilePath);
                BackupFileTB.Text = DBName;
            }

        }

        /// <summary>
        /// Creates a new database using DBName and DBPath
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateBt_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                //Verify user and password an that file name has been selected
                if (VerifyUserPassword() && VerifyNewFile())
                {
                    if (MBBetaAPI.AgentAPI.DBLayer.CreateDB(DBPath + "\\" + DBName, UserTB.Text, PasswordPB.Password))
                    {
                        Properties.Settings.Default.NewProduct = "NO";

                        Configuration MBConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        Properties.Settings.Default.NewProduct = "NO";
                        Properties.Settings.Default.Save();
                        var LoginWindow = new MBLogin(UserTB.Text, PasswordPB.Password, DBName, DBPath, CurrentCulture);
                        LoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        LoginWindow.Show();
                        CleanClose = true;

                        LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:NewDBCreated");
                        string message;
                        loc.ResolveLocalizedValue(out message);
                        MessageBox.Show(message);

                        this.Close();
                    }
                    else
                    {
                        LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:DBCreationFailed");
                        string message;
                        loc.ResolveLocalizedValue(out message);
                        MessageBox.Show(message);
                    }

                }
            }
            catch (Exception ex)
            {
                LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:DBCreationFailed");
                string message;
                loc.ResolveLocalizedValue(out message);
                MessageBox.Show(message + "\n" + ex.ToString());
                this.Close();
            }
        }

        private void CancelBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (CleanClose)
            {
                e.Cancel = false;
                return;
            }

            MessageBoxButton MBBt = MessageBoxButton.OKCancel;
            LocTextExtension loc = new LocTextExtension("MBBeta2:LoginStrings:WarnLeavingSetup");
            string message;
            loc.ResolveLocalizedValue(out message);
            MessageBoxResult res = MessageBox.Show(message, "Warning", MBBt, MessageBoxImage.Exclamation);
            switch (res)
            {
                case MessageBoxResult.OK:
                    e.Cancel = false;
                    break;
                default:
                    e.Cancel = true;
                    break;

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

                var obj = sender as PasswordBox;
                if (obj == PasswordVerificationPB)
                {
                    CreateBt_Click(null, null);
                }

                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }

        }
        
    }
}
