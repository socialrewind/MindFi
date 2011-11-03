using System;
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
using System.Windows.Forms;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBSNLogin.xaml
    /// </summary>
    public partial class MBSNLogin : Window
    {
        public MBSNLogin()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get URI to navigate to
            Uri uri = new Uri(URLBox.Text, UriKind.RelativeOrAbsolute);

            
            
            if (!uri.IsAbsoluteUri)
            {
                System.Windows.MessageBox.Show("The Address URI must be absolute eg 'http://www.eluniversal.com.mx'");
                return;
            }

            // Navigate to the desired URL by calling the .Navigate method
            this.LoginBrowser.Navigate(uri);

        }

        private void LoginBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {


            //HtmlDocument htmlDoc = LoginBrowser.Document as HtmlDocument;
            //string htmltext = htmlDoc.ActiveElement.Document.Url.ToString();
            //ResultTB.Text = htmltext;

        }
    }
}
