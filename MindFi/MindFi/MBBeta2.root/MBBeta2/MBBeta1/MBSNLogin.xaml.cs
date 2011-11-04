using System;
using System.Windows;
/*
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
*/
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

        public MBSNLogin()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            FBLogin.Login(out URL, out myCallback);

            // Get URI to navigate to
            //Uri uri = new Uri(URLBox.Text, UriKind.RelativeOrAbsolute);
            Uri uri = new Uri(URL, UriKind.RelativeOrAbsolute);
            
            // Navigate to the desired URL by calling the .Navigate method
            this.LoginBrowser.Navigate(uri);

        }

        private void LoginBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (FBLogin.CheckCallback(e.Uri.ToString()))
            {
                // TODO: Start  timer to close
                // TODO: Process Callback
            }
            else if (FBLogin.CheckErrorPage(e.Uri.ToString()))
            {
                // TODO: Error message
                MessageBox.Show("Login failed, please try again");
            }
            //HtmlDocument htmlDoc = LoginBrowser.Document as HtmlDocument;
            //string htmltext = htmlDoc.ActiveElement.Document.Url.ToString();
            //ResultTB.Text = htmltext;

        }

        private void LoginBrowser_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LoginBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

        private void LoginBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
