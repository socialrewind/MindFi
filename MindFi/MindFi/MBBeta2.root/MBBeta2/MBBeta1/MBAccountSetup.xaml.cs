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

        public MBAccountSetup(int SocialNetworkID)
        {
            InitializeComponent();

            SNTB.Text = SocialNetworkID.ToString();
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

        }
    }
}
