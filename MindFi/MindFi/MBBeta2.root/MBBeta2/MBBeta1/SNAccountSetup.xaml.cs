using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.IO;
//Internal API
using MBBetaAPI;
using MBBetaAPI.AgentAPI;
//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for SNAccountSetup.xaml
    /// </summary>
    public partial class SNAccountSetup : Window
    {
        public bool accountAdded = false;
        DispatcherTimer dispatcherTimer;
        int SN;
        bool success;
        bool firstTimeOpened = true;
        int state = 0;
        string animation = "...";

        #region "Download info"
        private string BaseDir;
        #endregion

        public SNAccountSetup(string basedirparam)
        {
            InitializeComponent();
            BaseDir = basedirparam;
            SN = SocialNetwork.FACEBOOK;
            GetSNAccounts();

            SaveBt.IsEnabled = false;
            if ( SNAccount.CurrentProfile == null )
            {
                BackupDateDP.SelectedDate = System.DateTime.Today.AddDays(-30);
                BackupEndTB.Visibility = Visibility.Hidden;
                endBackupTB.Visibility = Visibility.Hidden;
            }
            else
            {
                BackupDateDP.SelectedDate = SNAccount.CurrentProfile.BackupPeriodStart;
                BackupDateDP.DisplayDateEnd = BackupDateDP.SelectedDate;

                BackupEndTB.Visibility = Visibility.Visible;
                string backupState, backupDate;
                DBLayer.GetLastBackup(out backupState, out backupDate);
                endBackupTB.Text = backupDate;
            }                             

            //BackupOptions
            MeWallCB.IsChecked = AsyncReqQueue.BackupMyWall;
            MeNewsCB.IsChecked = AsyncReqQueue.BackupMyNews;
            MeInboxCB.IsChecked = AsyncReqQueue.BackupMyInbox;
            MeEventsCB.IsChecked = AsyncReqQueue.BackupMyEvents;
            MePhotosCB.IsChecked = AsyncReqQueue.BackupMyPhotos;
            FriendsEventsCB.IsChecked = AsyncReqQueue.BackupFriendsEvents;
            FriendsPhotosCB.IsChecked = AsyncReqQueue.BackupFriendsAlbums;
            FriendsWallCB.IsChecked = AsyncReqQueue.BackupFriendsWall;

        }


        //******** Private MEthods
        #region PrivateMethods

        void GetSNAccounts()
        {
            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                // TODO: Localize
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

                SNUrlTB.Text = SNAccount.CurrentProfile.URL;                
            }
        }

        private void SNLoginBt_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            var SNLoginWindow = new MBSNLogin(true);
            SNLoginWindow.Owner = this;
            SNLoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            SNLoginWindow.Show();
            // get the timer to check login status continuously
            state = 1;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            FBPerson me = null;
            switch (state)
            {
                case 1: // waiting for login
                    // check if logged in
                    // part 1: log in
                    if (!FBLogin.loggedIn)
                    {
                        // TODO: Localize
                        LocTextExtension loc = new LocTextExtension("MBBeta2:SetupStrings:ASLogging");
                        string message;
                        loc.ResolveLocalizedValue(out message);
                        this.StatusTB.Text = message + animation;
                    }
                    else
                    {
                        LocTextExtension loc = new LocTextExtension("MBBeta2:SetupStrings:ASProcessing");
                        string message;
                        loc.ResolveLocalizedValue(out message);
                        this.StatusTB.Text = message;
                        state = 2;
                    }
                    break;
                case 2:
                    // once logged in, just poll for the data
                    me = FBLogin.Me;
                    if (me == null || !me.Parsed)
                    {
                        LocTextExtension loc = new LocTextExtension("MBBeta2:SetupStrings:WaitingParse");
                        string message;
                        loc.ResolveLocalizedValue(out message);
                        this.StatusTB.Text += message + animation;
                    }
                    else
                    {
                        state = 3;
                    }
                    break;
                case 3:
                    me = FBLogin.Me;
                    if (me == null || !me.Parsed)
                    {
                        state = 2;
                    }
                    else
                    {
                        if (SNAccount.CurrentProfile != null)
                        {
                            if (SaveBt != null)
                            {
                                SaveBt.IsEnabled = true;
                            }
                        }
                        else
                        {
                            // TODO: Localize
                            this.StatusTB.Text = "Getting basic data: " + AsyncReqQueue.nFriends + " friends" + animation;
                            //Store Backup Options
                            AsyncReqQueue.BackupMyWall = (bool)MeWallCB.IsChecked;
                            AsyncReqQueue.BackupMyNews = (bool)MeNewsCB.IsChecked;
                            AsyncReqQueue.BackupMyInbox = (bool)MeInboxCB.IsChecked;
                            AsyncReqQueue.BackupMyEvents = (bool)MeEventsCB.IsChecked;
                            AsyncReqQueue.BackupMyPhotos = (bool)MePhotosCB.IsChecked;
                            AsyncReqQueue.BackupFriendsEvents = (bool)FriendsEventsCB.IsChecked;
                            AsyncReqQueue.BackupFriendsAlbums = (bool)FriendsPhotosCB.IsChecked;
                            AsyncReqQueue.BackupFriendsWall = (bool)FriendsWallCB.IsChecked;
                            // once logged in, just poll for the data
                            DateTime initialDate;
                            initialDate = (DateTime)(BackupDateDP.SelectedDate != null ? BackupDateDP.SelectedDate : DateTime.Now);
                            AsyncReqQueue.GetBasicData(initialDate, DateTime.Today.AddMonths(1), me.ID, me.SNID);
                            state = 4;
                        }
                    }
                    break;
                case 4:
                    AsyncReqQueue.PendingBasics();
                    // TODO: Localize
                    this.StatusTB.Text = "Getting basic data: " + AsyncReqQueue.nFriends + " friends" + animation;
                    if (AsyncReqQueue.GotFriendList && AsyncReqQueue.GotOneProfilePic 
                            && ( AsyncReqQueue.GotWall || !AsyncReqQueue.BackupMyWall )
                            && (AsyncReqQueue.GotEvent || !AsyncReqQueue.BackupMyEvents )
                            && (AsyncReqQueue.GotInbox || !AsyncReqQueue.BackupMyInbox) 
                            && AsyncReqQueue.GotFriendLists)
                    {
                        dispatcherTimer.Stop();
                        this.StatusTB.Text += ":" + FBLogin.LastError;
                        // TODO: Localize
                        this.StatusTB.Text = "Your basic info, profile picture and friend list (" + AsyncReqQueue.nFriends + ") have been retrieved.";
                        me = FBLogin.Me;
                        this.SNUrlTB.Text = me.Link;
                        SaveBt.IsEnabled = true;
                        state = 5;
                    }
                    else if (AsyncReqQueue.FailedRequests)
                    {
                        // TODO: Localize
                        this.StatusTB.Text = "Failed to get basic info. Check you are connected to the network and try again (using login/logout buttons).";
                        state = 5;
                    }
                    break;
                // case 5 - basic Wall posts? timeline?
            }
            if (animation != "...")
            {
                animation += ".";
            }
            else
            {
                animation = "";
            }
        }

        private void SNLogoutnBt_Click(object sender, RoutedEventArgs e)
        {
            var SNLoginWindow = new MBSNLogin(false);
            SNLoginWindow.Owner = this;
            SNLoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            SNLoginWindow.Show();
            // TODO: Timer to close, depending on result
        }

        private void SaveBt_Click(object sender, RoutedEventArgs e)
        {
            // TODO: support multiple profiles
            // For now, verifying only if there is no currentProfile
            FBPerson me = FBLogin.Me;
            if (SNAccount.CurrentProfile != null && me != null)
            {
                if ((SNAccount.CurrentProfile.Name != me.Name
                    && SNAccount.CurrentProfile.SNID != me.SNID
                    && SNAccount.CurrentProfile.URL != me.Link) ||
                    SNAccount.CurrentProfile.SocialNetwork != SN
                    )
                {
                    // TODO: Localize
                    MessageBox.Show("You tried to login with a different account (" + me.Name
                        + ") instead of the selected account (" + SNAccount.CurrentProfile.Name
                        + "). Please correct your data; login cancelled.");
                    return;
                }
            }

            //Store Backup Options
            AsyncReqQueue.BackupMyWall = (bool)MeWallCB.IsChecked;
            AsyncReqQueue.BackupMyNews = (bool)MeNewsCB.IsChecked;
            AsyncReqQueue.BackupMyInbox = (bool)MeInboxCB.IsChecked;
            AsyncReqQueue.BackupMyEvents = (bool)MeEventsCB.IsChecked;
            AsyncReqQueue.BackupMyPhotos = (bool)MePhotosCB.IsChecked;
            AsyncReqQueue.BackupFriendsEvents = (bool)FriendsEventsCB.IsChecked;
            AsyncReqQueue.BackupFriendsAlbums = (bool)FriendsPhotosCB.IsChecked;
            AsyncReqQueue.BackupFriendsWall = (bool)FriendsWallCB.IsChecked;

            string errorData = "";
            // important: time in both is 0:0:0
            DateTime initialDate = BackupDateDP.SelectedDate.Value;
            DateTime endDate = DateTime.Today.AddMonths(1);
            if (SNAccount.CurrentProfile == null)
            {
                SNAccount.UpdateCurrent(new SNAccount(-1, SN, me.SNID, me.Name, me.EMail, me.Link,
                    AsyncReqQueue.BackupMyWall,
                    AsyncReqQueue.BackupMyNews,
                    AsyncReqQueue.BackupMyInbox,
                    AsyncReqQueue.BackupMyEvents,
                    AsyncReqQueue.BackupMyPhotos,
                    AsyncReqQueue.BackupFriendsEvents,
                    AsyncReqQueue.BackupFriendsAlbums,
                    AsyncReqQueue.BackupFriendsWall,
                    initialDate, endDate));

                if (!DBLayer.SaveAccount(me.ID, me.Name, me.EMail, SN, me.SNID, me.Link,
                       //SNAccount.CurrentProfile.currentBackupLevel,
                       AsyncReqQueue.BackupMyWall,
                       AsyncReqQueue.BackupMyNews,
                       AsyncReqQueue.BackupMyInbox,
                       AsyncReqQueue.BackupMyEvents,
                       AsyncReqQueue.BackupMyPhotos,
                       AsyncReqQueue.BackupFriendsEvents,
                       AsyncReqQueue.BackupFriendsAlbums,
                       AsyncReqQueue.BackupFriendsWall,
                       SNAccount.CurrentProfile.BackupPeriodStart,
                       SNAccount.CurrentProfile.BackupPeriodEnd,
                       out errorData))
                {
                    // TODO: Localize
                    MessageBox.Show("Error while saving account:\n" + errorData);
                    return;
                }
                accountAdded = true;
            }
            else
            {
                SNAccount.CurrentProfile.BackupPeriodStart = initialDate;
                //SNAccount.CurrentProfile.currentBackupLevel = this.BackupTypeCB.SelectedIndex + 1;
                // make sure it updates existing accounts
                if (!DBLayer.UpdateAccount(SN, me.SNID,
                       AsyncReqQueue.BackupMyWall,
                       AsyncReqQueue.BackupMyNews,
                       AsyncReqQueue.BackupMyInbox,
                       AsyncReqQueue.BackupMyEvents,
                       AsyncReqQueue.BackupMyPhotos,
                       AsyncReqQueue.BackupFriendsEvents,
                       AsyncReqQueue.BackupFriendsAlbums,
                       AsyncReqQueue.BackupFriendsWall,
                       SNAccount.CurrentProfile.BackupPeriodStart,
                       out errorData))
                {
                    // TODO: Localize
                    MessageBox.Show("Error while saving account:\n" + errorData);
                    return;
                }
            }

            success = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!success && state >= 4)
            {
                // TODO: Localize
                MessageBoxResult temp = MessageBox.Show("Are you sure to exit without saving available data?", "Confirmation", MessageBoxButton.YesNo);
                if (temp != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            this.DialogResult = success;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (firstTimeOpened)
            {
                DoLogin();
            }
            firstTimeOpened = false;
        }

        #endregion

        private void BackupDateDP_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Enable Save
            if (SNAccount.CurrentProfile != null && SaveBt !=null && FBLogin.Me != null )
            {
                SaveBt.IsEnabled = true;
            }
        }

        private void BackupTypeCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Enable Save
            if (SNAccount.CurrentProfile != null && SaveBt != null && FBLogin.Me != null )
            {
                SaveBt.IsEnabled = true;
            }
        }

        private void MePhotosCB_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
