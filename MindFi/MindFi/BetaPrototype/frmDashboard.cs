using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace MyBackup
{
    /// <summary>
    /// Main form for the new agent interface
    /// </summary>
    public partial class frmDashboard : Form
    {
        /// <summary>
        /// User logged in
        /// </summary>
        public static string LoginName;
        public static MyBackupProfile currentProfile;
        private int currentSN = 1;
        public bool online = false;

        #region "Download info"
        private string BaseDir;
        #endregion

        #region "Process control"
        private bool firstTime = true;
        #endregion

        /// <summary>
        /// Form constructor
        /// </summary>
        public frmDashboard()
        {
            InitializeComponent();
            GetPaths();
        }

        private void GetPaths()
        {
            string dbPath = DBConfig.GetLastDBFromFile();

            if (dbPath != "")
            {
                FileInfo baseFile = new FileInfo(dbPath);
                BaseDir = baseFile.DirectoryName;
            }
            else
            {
                BaseDir = "";
            }

            if (BaseDir == "")
            {
                DateTime temp = DateTime.Now;
                string tempD = temp.Year.ToString();
                if (temp.Month < 10) tempD += "0";
                tempD += temp.Month;
                if (temp.Day < 10) tempD += "0";
                tempD += temp.Day;
                AsyncReqQueue.ProfilePhotoDestinationDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                    + "\\MyBackupTests\\" + tempD + "\\fb\\ProfilePics\\";
                AsyncReqQueue.AlbumDestinationDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                    + "\\MyBackupTests\\" + tempD + "\\fb\\Albums\\";
            }
            else
            {
                AsyncReqQueue.ProfilePhotoDestinationDir = BaseDir + "\\fb\\ProfilePics\\";
                AsyncReqQueue.AlbumDestinationDir = BaseDir + "\\fb\\Albums\\";
            }
            // TODO: Save directory to database
            if (!Directory.Exists(AsyncReqQueue.ProfilePhotoDestinationDir))
            {
                Directory.CreateDirectory(AsyncReqQueue.ProfilePhotoDestinationDir);
            }
            if (!Directory.Exists(AsyncReqQueue.AlbumDestinationDir))
            {
                Directory.CreateDirectory(AsyncReqQueue.AlbumDestinationDir);
            }
        }

        /// <summary>
        /// When the form is opened, verify config and allow to call next forms appropriately
        /// </summary>
        private void frmDashboard_Load(object sender, EventArgs e)
        {
            // "maximize" form
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left | AnchorStyles.Right);
            frmLogin f1 = new frmLogin();
            DialogResult res = f1.ShowDialog();
            //MessageBox.Show("res: " + res.ToString());
            // processing using data from frmLogin
            if (res == DialogResult.OK)
            {
                SetLoginText();
            }
            else
            {
                Application.Exit();
                //this.labelWelcome.Text = "Not logged";
            }
            PopulateAccounts();
        }

        /// <summary>
        /// Generate text for all logged-in information
        /// </summary>
        private void SetLoginText()
        {
            this.labelWelcome.Text = "Logged in MyBackup as: " + frmDashboard.LoginName;
            if (FBLogin.loggedIn)
            {
                this.labelWelcome.Text += "\nLogged in Facebook as: " + FBLogin.LoginName; // + "\n" + FBLogin.accessToken;
            }
            else
            {
                this.labelWelcome.Text += "\nNot logged in Facebook";
            }
        }

        /// <summary>
        /// Processing event for adding a new social network account
        /// </summary>
        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            frmAddAccount temp = new frmAddAccount(currentSN);
            DialogResult res = temp.ShowDialog();
            temp.Dispose();
            if (res == DialogResult.Cancel)
            {
                MessageBox.Show("Account was not added");
                SetLoginText();
                this.Refresh();
                return;
            }
            // refresh data
            PopulateAccounts();
        }

        ///// <summary>
        ///// Processing event for removing social network accounts
        ///// </summary>
        //private void btnDeleteAccount_Click(object sender, EventArgs e)
        //{
        //    DialogResult res = MessageBox.Show("Are you sure to delete the selected accounts?",
        //    "Confirm delete", MessageBoxButtons.YesNo);
        //    if (res != DialogResult.Yes)
        //    {
        //        return;
        //    }
        //    // TODO: Delete an account
        //}

        /// <summary>
        /// Fills the data from the list of accounts in the database
        /// </summary>
        private void PopulateAccounts()
        {
            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                MessageBox.Show("Error getting accounts:\n" + error);
            }
            // TODO: Review how it can be empty and not null
            if (currentAccounts != null && currentProfile == null && currentAccounts.Count > 0)
            {
                SNAccount first = (SNAccount) currentAccounts[0];
                currentProfile = new MyBackupProfile();
                currentProfile.currentBackupLevel = first.currentBackupLevel;
                currentProfile.SocialNetworkAccountID = first.SNID;
                currentProfile.socialNetworkID = first.SocialNetwork;
                currentProfile.socialNetworkURL = first.URL;
                currentProfile.userName = first.Name;
            }

            System.Windows.Forms.LinkLabel lblToAssign = null;
            switch (currentProfile.socialNetworkID)
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

            btnOnline.Enabled = (currentProfile != null);
            UpdateForm();
        }

        private void GoOffline()
        {
            online = false;
            processTimer.Enabled = false;
            btnOnline.Text = "Go Online/Login";
            Refresh();
        }

        /// <summary>
        /// Processing event for logging into Facebook
        /// </summary>
        private void btnOnline_Click(object sender, EventArgs e)
        {
            if (online)
            {
                GoOffline();
            }
            else
            {
                btnOnline.Enabled = false;
                btnOnline.Text = "Logging in...";
                Refresh();
                online = true;
                FBLogin login = new FBLogin();
                login.Login();

                // Verify Login result matches current account

                processTimer.Enabled = true;
                processTimer.Interval = 5000;
                // TODO: wait or callback?
                System.Threading.Thread.Sleep(10000);
                btnOnline.Text = "Go Offline / Logout";
                btnOnline.Enabled = true;
                UpdateForm();
            }
        }

        /// <summary>
        /// General refresh for UI
        /// </summary>
        private void UpdateForm()
        {
            SetLoginText();
            this.Refresh();
        }

        /// <summary>
        /// General refresh for UI
        /// </summary>
        private void UpdateStats()
        {
            string error = "";

            // TODO: accumulate errors
            if (error == "")
            {
                this.labelStats.Text = "Total friends: " + AsyncReqQueue.nFriends
                    + ". Friends processed: " + AsyncReqQueue.nFriendsProcessed
                    + ". Friends pictures: " + AsyncReqQueue.nFriendsPictures
                    + ". Total Posts: " + AsyncReqQueue.nPosts
                    + ". Total Messages: " + AsyncReqQueue.nMessages
                    + ". Total Albums: " + AsyncReqQueue.nAlbums
                    + ". Total Photos: " + AsyncReqQueue.nPhotos;
                this.labelStats.Text += ".\nQueued: " + AsyncReqQueue.QueuedReqs;
                this.labelStats.Text += ". Sent: " + AsyncReqQueue.SentReqs;
                this.labelStats.Text += ". Retry: " + AsyncReqQueue.RetryReqs;
                this.labelStats.Text += ". Received: " + AsyncReqQueue.ReceivedReqs;
                this.labelStats.Text += ". Failed: " + AsyncReqQueue.FailedReqs;
                this.labelStats.Text += ". Parsed: " + AsyncReqQueue.ParsedReqs;
                this.labelStats.Text += ". No need parsing: " + AsyncReqQueue.NotParsedReqs;

            }
            else
            {
                this.labelStats.Text = error;
            }
        }

        /// <summary>
        /// Processing event for regular timer
        /// </summary>
        private void processTimer_Tick(object sender, EventArgs e)
        {
            int MinPriority;
            switch (currentProfile.currentBackupLevel)
            {
                case MyBackupProfile.BASIC:
                    MinPriority = 750;
                    break;
                case MyBackupProfile.EXTENDED:
                    MinPriority = 150;
                    break;
                case MyBackupProfile.STALKER:
                default:
                    MinPriority = 0;
                    break;
            }
            if (FBLogin.Me != null)
            {
                if (firstTime)
                {
                    FBPerson me = FBLogin.Me;
                    if (frmDashboard.currentProfile != null)
                    {
                        if (frmDashboard.currentProfile.socialNetworkID != SocialNetwork.FACEBOOK ||
                              (frmDashboard.currentProfile.userName != me.Name
                            && frmDashboard.currentProfile.SocialNetworkAccountID != me.SNID
                            && frmDashboard.currentProfile.socialNetworkURL != me.Link)
                            )
                        {
                            FBLogin temp = new FBLogin();
                            temp.LogOut();
                            GoOffline();
                            MessageBox.Show("You tried to login with a different account (" + me.Name
                                + ") instead of the selected account (" + frmDashboard.currentProfile.userName 
                                + "). Please correct your data; login cancelled.");
                            return;
                        }
                    }
                    firstTime = false;
                    processTimer.Interval = 1000;
                    AsyncReqQueue.InitialRequests(MinPriority);
                }
                else
                {
                    string ErrorMessage;
                    bool inProgress = AsyncReqQueue.PendingRequests(MinPriority, out ErrorMessage);
                    this.labelInfo.Text = ErrorMessage;
                    if (!inProgress)
                    {
                        GoOffline();
                    }
                }
            }

            // Get next step depending on the requests / step?
            // Get statistics
            UpdateStats();
            UpdateForm();
        }

        private void btnFBAcccount_Click(object sender, EventArgs e)
        {
            currentSN = SocialNetwork.FACEBOOK;
            btnAddAccount_Click(sender, e);
        }

        private void btnTwitterAccount_Click_1(object sender, EventArgs e)
        {
            currentSN = SocialNetwork.TWITTER;
            btnAddAccount_Click(sender, e);
        }

        private void btnLinkedInAccount_Click_1(object sender, EventArgs e)
        {
            currentSN = SocialNetwork.LINKEDIN;
            btnAddAccount_Click(sender, e);
        }

        private void linkLabelFB1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (currentProfile == null || currentSN != SocialNetwork.FACEBOOK)
            {
                currentSN = SocialNetwork.FACEBOOK;
                btnAddAccount_Click(sender, e);
            }
            else
            {
                // Edit account
                MessageBox.Show("will edit the FB account " + currentProfile.userName);
            }
        }

        private void linkLabelTw1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (currentProfile == null || currentSN != SocialNetwork.LINKEDIN)
            {
                currentSN = SocialNetwork.TWITTER;
                btnAddAccount_Click(sender, e);
            }
            else
            {
                // Edit account
                MessageBox.Show("will edit the Twitter account " + currentProfile.userName);
            }
        }

        private void linkLabelLI1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (currentProfile == null || currentSN != SocialNetwork.TWITTER)
            {
                currentSN = SocialNetwork.LINKEDIN;
                btnAddAccount_Click(sender, e);
            }
            else
            {
                // Edit account
                MessageBox.Show("will edit the LinkedIn account " + currentProfile.userName);
            }
        }

    }

}