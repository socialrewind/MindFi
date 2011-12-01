using System;
using System.Windows;
using System.Windows.Threading;
using MBBetaAPI.AgentAPI;

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
            var SNLoginWindow = new MBSNLogin(true);
            SNLoginWindow.Owner = this;
            SNLoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            SNLoginWindow.Show();
            // get the timer to check login status continuously
            dispatcherTimer =  new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // check if logged in
            // part 1: log in
            if (!FBLogin.loggedIn)
            {
                // TODO: Localize
                this.SNTB.Text = "Logging in";
            }
            else
            {
                // TODO: Localize
                this.SNTB.Text = "Logged in Facebook";
                // once logged in, just poll for the data
                FBPerson me = FBLogin.Me;
                if (me == null || !me.Parsed)
                {
                    this.SNTB.Text += "... waiting for parse...";
                }
                else
                {
                    dispatcherTimer.Stop();
                    this.SNTB.Text += ":" + FBLogin.LastError;

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
                }
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
                    MessageBox.Show("You tried to login with a different account (" + me.Name
                        + ") instead of the selected account (" + SNAccount.CurrentProfile.Name
                        + "). Please correct your data; login cancelled.");
                    return;
                }
            }

            string errorData = "";
            //For now
            //me.Save(out errorData);
            if (errorData != "")
            {
                if (MessageBox.Show(errorData, "Error while saving your data, cancel Add Account?", MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                {
                    Close();
                }
            }
            SNAccount.UpdateCurrent(new SNAccount(-1, SN, me.SNID, me.Name, me.EMail, me.Link, 
                this.BackupTypeCB.SelectedIndex + 1, 
                BackupDateDP.SelectedDate.Value, DateTime.UtcNow.AddMonths(1) ));

            if (!DBLayer.SaveAccount(me.ID, me.Name, me.EMail, SN, me.SNID, me.Link,
                   SNAccount.CurrentProfile.currentBackupLevel,
                   SNAccount.CurrentProfile.BackupPeriodStart,
                   SNAccount.CurrentProfile.BackupPeriodEnd,
                   out errorData))
            {
                MessageBox.Show("Error while saving account:\n" + errorData);
                return;
            }
            MessageBox.Show("Data saved correctly");
            success = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = success;
        }

    }
}
