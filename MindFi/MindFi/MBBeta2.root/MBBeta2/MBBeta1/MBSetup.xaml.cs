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
        public bool online = false;

        #region "Download info"
        private string BaseDir;
        #endregion

        #region "Process control"
        private bool firstTime = true;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private int MinPriority;
        #endregion

        public MBSetup(DBConnector dbParam, string basedirparam)
        {
            InitializeComponent();
            db = dbParam;
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

            GetSNAccounts(db);
        }

        //********Attributes
        DBConnector db;


        //******** Private MEthods
        #region PrivateMethods

        void GetSNAccounts(DBConnector db)
        {
            DBLayer.ConnString = db.ConnString;
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

            OnlineBt.IsEnabled = (SNAccount.CurrentProfile != null);
        }


        #endregion

        private void FBAccountSetupBt_Click(object sender, RoutedEventArgs e)
        {
            var AccountSetupWindow = new MBAccountSetup(SocialNetwork.FACEBOOK);
            bool? result = AccountSetupWindow.ShowDialog();
            if ( result != null && !(bool)result )
            {
                MessageBox.Show("Account was not added");
                return;
            }
            // Check if refresh works
            GetSNAccounts(db);
        }

        private void OnlineBt_Click(object sender, RoutedEventArgs e)
        {
            if (online)
            {
                GoOffline();
            }
            else
            {
                OnlineBt.IsEnabled = false;
                // TODO: Localize
                OnlineBt.Content = "Logging in...";
                online = true;
                // display login
                var SNLoginWindow = new MBSNLogin(true);
                SNLoginWindow.Owner = this;
                SNLoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                SNLoginWindow.Show();
                dispatcherTimer.Tick += new EventHandler(processTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Start();
            }
        }

        private void GoOffline()
        {
            online = false;
            dispatcherTimer.Stop();
            // TODO: Localize
            OnlineBt.Content = "Go Online/Login";
        }

        /// <summary>
        /// Processing event for regular timer
        /// </summary>
        private void processTimer_Tick(object sender, EventArgs e)
        {
            if (FBLogin.loggedIn)
            {
                if (firstTime)
                {            
                    // TODO: Localize
                    OnlineBt.Content = "Go Offline / Logout";
                    OnlineBt.IsEnabled = true;

                    switch (SNAccount.CurrentProfile.currentBackupLevel)
                    {
                        case SNAccount.BASIC:
                            MinPriority = 750;
                            break;
                        case SNAccount.EXTENDED:
                            MinPriority = 150;
                            break;
                        case SNAccount.STALKER:
                        default:
                            MinPriority = 0;
                            break;
                    }
                    FBPerson me = FBLogin.Me;
                    if (SNAccount.CurrentProfile != null)
                    {
                        // TODO: Generalize for other social networks
                        if ((SNAccount.CurrentProfile.Name != me.Name
                            && SNAccount.CurrentProfile.SNID != me.SNID
                            && SNAccount.CurrentProfile.URL != me.Link) ||
                            SNAccount.CurrentProfile.SocialNetwork != SocialNetwork.FACEBOOK
                            )
                        {
                            MessageBox.Show("You tried to login with a different account (" + me.Name
                                + ") instead of the selected account (" + SNAccount.CurrentProfile.Name
                                + "). Please correct your data; login cancelled.");
                            return;
                        }
                    }
                    firstTime = false;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    AsyncReqQueue.InitialRequests(MinPriority);
                }
                else
                {
                    string ErrorMessage;
                    bool inProgress = AsyncReqQueue.PendingRequests(MinPriority, out ErrorMessage);
                    InfoTB.Text = ErrorMessage;
                    if (!inProgress)
                    {
                        GoOffline();
                    }
                }
            }

            // Get next step depending on the requests / step?
            // Get statistics
            UpdateStats();
        }

        /// <summary>
        /// General refresh for UI
        /// </summary>
        private void UpdateStats()
        {
            string error = "";
            string stats = "Statistics\n";

            // TODO: accumulate errors
            if (error == "")
            {
                stats += "Total friends: " + AsyncReqQueue.nFriends
                    + ". Friends processed: " + AsyncReqQueue.nFriendsProcessed
                    + ". Friends pictures: " + AsyncReqQueue.nFriendsPictures
                    + ". Total Posts: " + AsyncReqQueue.nPosts
                    + ". Total Messages: " + AsyncReqQueue.nMessages
                    + ". Total Albums: " + AsyncReqQueue.nAlbums
                    + ". Total Photos: " + AsyncReqQueue.nPhotos;
                stats += ".\nQueued: " + AsyncReqQueue.QueuedReqs;
                stats += ". Sent: " + AsyncReqQueue.SentReqs;
                stats += ". Retry: " + AsyncReqQueue.RetryReqs;
                stats += ". Received: " + AsyncReqQueue.ReceivedReqs;
                stats += ". Failed: " + AsyncReqQueue.FailedReqs;
                stats += ". Parsed: " + AsyncReqQueue.ParsedReqs;
                stats += ". No need parsing: " + AsyncReqQueue.NotParsedReqs;

            }
            else
            {
                stats = error;
            }
            StatsTB.Text = stats;
        }

    }
}
