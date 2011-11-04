using System;
using System.Windows;
using System.Windows.Threading;
using MBBetaAPI.AgentAPI;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBSNLogin.xaml
    /// </summary>
    public partial class MBSNLogin : Window
    {
        // TODO: Support multiple Social networks
        CallBack myCallback;
        string URL;
        bool success = false;

        public MBSNLogin(bool InOrOut)
        {
            InitializeComponent();
            if (InOrOut)
            {
                FBLogin.Login(out URL, out myCallback);
            }
            else
            {
                FBLogin.LogOut(out URL);
            }
            Uri uri = new Uri(URL, UriKind.RelativeOrAbsolute);
            this.LoginBrowser.Navigate(uri);
        }

        private void LoginBrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (FBLogin.CheckCallback(this.LoginBrowser.Document.Url.ToString()))
            {
                success = true;
                // get the timer to close after 5 seconds...
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Start();
            }
                // TODO: seems this is not really in use, consider Logout case
            else if (FBLogin.CheckErrorPage(this.LoginBrowser.Document.Url.ToString()))
            {
                // TODO: Error message
                MessageBox.Show("Login failed, please try again");
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!success)
            {
                string URL;
                FBLogin.LogOut(out URL);
                Uri uri = new Uri(URL, UriKind.RelativeOrAbsolute);
                this.LoginBrowser.Navigate(uri);
            }
            // callback?
            if (myCallback != null)
            {
                myCallback( 0, success, FBLogin.token, null, "");
            }

        }

    }
}
