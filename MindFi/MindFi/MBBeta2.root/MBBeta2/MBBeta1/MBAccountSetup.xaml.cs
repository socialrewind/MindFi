﻿using System;
using System.Windows;
using System.Windows.Threading;
using MBBetaAPI.AgentAPI;
//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBAccountSetup.xaml
    /// </summary>
    public partial class MBAccountSetup : Window
    {
        DispatcherTimer dispatcherTimer;
        int SN;
        bool success;
        bool firstTimeOpened = true;
        int state = 0;
        string animation = "...";

        public MBAccountSetup(int SocialNetworkID)
        {
            InitializeComponent();
            SaveBt.IsEnabled = false;

            SN = SocialNetworkID;
            // TODO: Get names from disk, localize
            switch (SocialNetworkID)
            {
                case SocialNetwork.FACEBOOK:
                    SNTB.Text = "Facebook";
                    break;
                case SocialNetwork.TWITTER:
                    SNTB.Text = "Twitter";
                    break;
                case SocialNetwork.LINKEDIN:
                    SNTB.Text = "LinkedIn";
                    break;
                case SocialNetwork.GOOGLE_PLUS:
                    SNTB.Text = "Google +";
                    break;
                default:
                    SNTB.Text = "Unknown SN";
                    break;
            }
            
            fillAmmountCB();
            BackupDateDP.SelectedDate = System.DateTime.Today.AddDays(-30);
        }

        void fillAmmountCB()
        {
            for (int i = 1; i <= 90; i++)
            {
                TimeAmmountCB.Items.Add(i);
            }
            TimeAmmountCB.SelectedIndex = 4;
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
                        this.SNTB.Text = message + animation;
                    }
                    else
                    {
                        LocTextExtension loc = new LocTextExtension("MBBeta2:SetupStrings:ASProcessing");
                        string message;
                        loc.ResolveLocalizedValue(out message);
                        this.SNTB.Text = message;
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
                        this.SNTB.Text += message + animation;
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
                    // TODO: Localize
                    this.SNTB.Text = "Getting basic data: " + AsyncReqQueue.nFriends + " friends" + animation;
                    // once logged in, just poll for the data
                    DateTime initialDate;
                    initialDate = (DateTime) (BackupDateDP.SelectedDate != null ? BackupDateDP.SelectedDate : DateTime.Now);
                    AsyncReqQueue.GetBasicData(initialDate, DateTime.Today.AddMonths(1), me.ID, me.SNID);
                    state = 4;
                    break;
                case 4:
                    AsyncReqQueue.PendingBasics();
                    // TODO: Localize
                    this.SNTB.Text = "Getting basic data: " + AsyncReqQueue.nFriends + " friends" + animation;
                    if (AsyncReqQueue.GotFriendList && AsyncReqQueue.GotOneProfilePic && AsyncReqQueue.GotWall 
                            && AsyncReqQueue.GotEvent && AsyncReqQueue.GotInbox && AsyncReqQueue.GotFriendLists )
                    {
                        dispatcherTimer.Stop();
                        //this.SNTB.Text += ":" + FBLogin.LastError;
                        // TODO: Localize
                        this.SNTB.Text = "Your basic info, profile picture, top " + AsyncReqQueue.nPosts + 
                            " wall posts and friend list (" + AsyncReqQueue.nFriends + ") have been retrieved.";
                        me = FBLogin.Me;
                        if (me.Name != null && me.Name != "")
                        {
                            this.AliasTB.Text = me.Name;
                        }
                        else
                        {
                            this.AliasTB.Text = me.SNID;
                        }
                        this.SNUrlTB.Text = me.Link;
                        this.SNIDTB.Text = me.SNID;
                        SaveBt.IsEnabled = true;
                        state = 5;
                    }
                    else if ( AsyncReqQueue.FailedRequests )
                    {
                        // TODO: Localize
                        this.SNTB.Text = "Failed to get basic info. Check you are connected to the network and try again (using login/logout buttons).";
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
            if ( SNAccount.CurrentProfile != null)
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

            string errorData = "";
            // NO LONGER USED
            ////For now
            ////me.Save(out errorData);
            //if (errorData != "")
            //{
            //    if (MessageBox.Show(errorData, "Error while saving your data, cancel Add Account?", MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            //    {
            //        Close();
            //    }
            //}
            // important: time in both is 0:0:0
            DateTime initialDate = BackupDateDP.SelectedDate.Value;
            DateTime endDate = DateTime.Today.AddMonths(1);           
            SNAccount.UpdateCurrent(new SNAccount(-1, SN, me.SNID, me.Name, me.EMail, me.Link, 
                this.BackupTypeCB.SelectedIndex + 1, 
                initialDate, endDate ) );

            if (!DBLayer.SaveAccount(me.ID, me.Name, me.EMail, SN, me.SNID, me.Link,
                   SNAccount.CurrentProfile.currentBackupLevel,
                   SNAccount.CurrentProfile.BackupPeriodStart,
                   SNAccount.CurrentProfile.BackupPeriodEnd,
                   out errorData))
            {
                // TODO: Localize
                MessageBox.Show("Error while saving account:\n" + errorData);
                return;
            }
            //MessageBox.Show("Data saved correctly");
            success = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ( !success && state >= 4)
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

    }
}
