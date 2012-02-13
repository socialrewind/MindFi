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

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for DetailCard.xaml
    /// </summary>
    public partial class DetailCard : Window
    {
        //********* Attributes
        #region Attributes
        Person CurrentPerson;
        CommonCode CC;
        Window MainWindow;
        #endregion

        #region Control Methods

        public DetailCard(Person PersonParam, Window MainPaam)
        {
            InitializeComponent();
            CurrentPerson = PersonParam;
            CC = new CommonCode();
            MainWindow = MainPaam;
        }

        private void CloseBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Refresh
            this.ErrorMessage2.Text = "Information was updated " + DateTime.Now.ToShortTimeString();            
        }

        private void WallBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            List<int> SelectedPeople = new List<int>();
            SelectedPeople.Add(CurrentPerson.ID);
            var WallWindow = new FBWallPosts(SelectedPeople);
            CC.PositionNewWindow(MainWindow, WallWindow);
            //this.Close();

            this.Cursor = Cursors.Arrow;
        }

        private void PhotosBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            List<int> SelectedPeople = new List<int>();
            SelectedPeople.Add(CurrentPerson.ID);
            var PhotosWindow = new FBPhotos(SelectedPeople);
            CC.PositionNewWindow(MainWindow, PhotosWindow);
            //this.Close();

            this.Cursor = Cursors.Arrow;
        }

        private void MessagesBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            List<int> SelectedPeople = new List<int>();
            SelectedPeople.Add(CurrentPerson.ID);
            var MessagesWindow = new SNMessages(SelectedPeople);
            CC.PositionNewWindow(MainWindow, MessagesWindow);
            //this.Close();

            this.Cursor = Cursors.Arrow;
        }

        #endregion

    }
}
