using System;
using System.Collections;
using System.Windows;
using System.Windows.Threading;
using System.IO;
//Internal API
using MBBetaAPI;
using MBBetaAPI.AgentAPI;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBSetup.xaml
    /// </summary>
    public partial class MBSetup : Window
    {
        //public bool online = false;
        public bool accountAdded = false;

        #region "Download info"
        private string BaseDir;
        #endregion

        public MBSetup(string basedirparam)
        {
            InitializeComponent();
            BaseDir = basedirparam;
            AsyncReqQueue.ProfilePhotoDestinationDir = BaseDir + "\\fb\\ProfilePics\\";
            AsyncReqQueue.AlbumDestinationDir = BaseDir + "\\fb\\Albums\\";
            // TODO: Save directory to database
            if (!Directory.Exists(AsyncReqQueue.ProfilePhotoDestinationDir))
            {
                Directory.CreateDirectory(AsyncReqQueue.ProfilePhotoDestinationDir);
            }
            if (!Directory.Exists(AsyncReqQueue.AlbumDestinationDir))
            {
                Directory.CreateDirectory(AsyncReqQueue.AlbumDestinationDir);
            }

            GetSNAccounts();
        }

        //******** Private MEthods
        #region PrivateMethods

        void GetSNAccounts()
        {
            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                MessageBox.Show("Error getting accounts:\n" + error);
            }
            // TODO: Review how it can be empty and not null
            if (currentAccounts != null && currentAccounts.Count > 0)
            {
                if (currentAccounts != null && SNAccount.CurrentProfile == null)
                {
                    SNAccount first = (SNAccount)currentAccounts[0];
                    SNAccount.UpdateCurrent(first);
                }

                // TODO: create labels to show multiple existing accounts, not just one
                /*
                System.Windows.Forms.LinkLabel lblToAssign = null;
                switch (SNAccount.CurrentProfile.SocialNetwork)
                {
                    case SocialNetwork.FACEBOOK:
                        lblToAssign = linkLabelFB1;
                        break;
                    case SocialNetwork.TWITTER:
                        lblToAssign = linkLabelTw1;
                        break;
                    case SocialNetwork.LINKEDIN:
                        lblToAssign = linkLabelLI1;
                        break;
                }
                if (lblToAssign != null)
                {
                    lblToAssign.Text = currentProfile.userName;
                }
                 * */
                FBAccountSetupBt.Content = SNAccount.CurrentProfile.Name;
                // TODO: Localize
                switch ( SNAccount.CurrentProfile.currentBackupLevel )
                {
                    case SNAccount.BASIC:
                        FBBackupType.Text = "Basic";
                        break;
                    case SNAccount.EXTENDED:
                        FBBackupType.Text = "Extended";
                        break;
                    case SNAccount.STALKER:
                        FBBackupType.Text = "Stalker";
                        break;
                    default:
                        FBBackupType.Text = "";
                        break;
                }
                FBLoggedIn.Text = FBLogin.loggedIn.ToString();
                // TODO: Review
                FBFrequency.Text = SNAccount.CurrentProfile.BackupFrequency.ToString() ;
            }

            //OnlineBt.IsEnabled = (SNAccount.CurrentProfile != null);
        }


        #endregion

        private void FBAccountSetupBt_Click(object sender, RoutedEventArgs e)
        {
            var AccountSetupWindow = new MBAccountSetup(SocialNetwork.FACEBOOK);
            bool? result = AccountSetupWindow.ShowDialog();
            if (result != null && !(bool)result)
            {
                MessageBox.Show("Account was not added");
                return;
            }
            else
            {
                accountAdded = true;
            }
            // Check if refresh works
            GetSNAccounts();
        }

    }
}
