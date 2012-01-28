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
using MBBetaAPI;

namespace MBBeta2.Tests
{
    /// <summary>
    /// Interaction logic for AsyncDetailCard.xaml
    /// </summary>
    public partial class AsyncDetailCard : Window
    {
        public AsyncDetailCard()
        {
            InitializeComponent();
        }

        private void CloseBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FriendshipBt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MyID_TextChanged(object sender, TextChangedEventArgs e)
        {
            // query the data, first with basic constructor
            int ID;
            string stringID = MyID.Text;
            if (int.TryParse(stringID, out ID))
            {
                try
                {
                    Person who = new Person(ID);
                    this.DataContext = who;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting person: " + ex.ToString() );
                }
            }
            else
            {
                MessageBox.Show("Please write a numeric ID");
                MyID.Text = "";
            }
        }
    }
}
