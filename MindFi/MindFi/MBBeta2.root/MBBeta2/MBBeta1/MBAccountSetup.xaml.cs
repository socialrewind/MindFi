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
*/
namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBAccountSetup.xaml
    /// </summary>
    public partial class MBAccountSetup : Window
    {
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
            var SNLoginWindow = new MBSNLogin();
            SNLoginWindow.Owner = this;
            SNLoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            SNLoginWindow.Show();
        }

        private void SNLogoutnBt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveBt_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
