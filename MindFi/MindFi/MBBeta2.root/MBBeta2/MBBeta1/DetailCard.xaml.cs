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
using MBBetaAPI.AgentAPI;

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

        //BackupOptions
        bool BackupWallOption;
        bool BackupEventsOption;
        bool BackupPhotosOption;

        #endregion

        #region Control Methods

        public DetailCard(Person PersonParam, Window MainParam)
        {
            InitializeComponent();
            CurrentPerson = PersonParam;
            CC = new CommonCode();
            MainWindow = MainParam;

            if (CurrentPerson.ID == 1) 
                //Don't show backup options
            {
                BackupOptionsSP.Visibility= System.Windows.Visibility.Hidden;
            }
            else
                
            {
                //Track changes to backup options
                BackupWallOption = CurrentPerson.BackupWall;
                BackupEventsOption = CurrentPerson.BackupEvents;
                BackupPhotosOption = CurrentPerson.BackupPhotos;

                //Enable buttons if option was not taken before
                if (!BackupWallOption)
                    GetNowWallBt.Visibility = System.Windows.Visibility.Visible;
                if (!BackupEventsOption)
                    GetNowEventsBt.Visibility = System.Windows.Visibility.Visible;
                if (!BackupPhotosOption)
                    GetNowPhotosBt.Visibility = System.Windows.Visibility.Visible;
            }

            
        }

        private void CloseBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // TODO: Localize
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


        private void EventsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            List<int> SelectedPeople = new List<int>();
            SelectedPeople.Add(CurrentPerson.ID);
            var EventsWindow = new FBEvents(SelectedPeople);
            CC.PositionNewWindow(MainWindow, EventsWindow);
            //this.Close();

            this.Cursor = Cursors.Arrow;
        }

        //**** Get now buttons

        private void GetNowWallBt_Click(object sender, RoutedEventArgs e)
        {
            WallCB.IsChecked = true;
            //Code to start downloading goes here
            MessageBox.Show("Start download here..");
        }

        private void GetNowEventsBt_Click(object sender, RoutedEventArgs e)
        {
            EventsCB.IsChecked = true;
            //Code to start downloading goes here
            MessageBox.Show("Start download here..");
        }

        private void GetNowPhotosBt_Click(object sender, RoutedEventArgs e)
        {
            PhotoAlbumsCB.IsChecked = true;
            MessageBox.Show("Start download here...");
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CurrentPerson.ID != 1)
            {
                if (WallCB.IsChecked != BackupWallOption)
                {
                    BackupWallOption = WallCB.IsChecked.Value;
                    CurrentPerson.SetBackupOptions((int)Person.BackupOptions.Wall, BackupWallOption);
                }

                if (EventsCB.IsChecked != BackupEventsOption)
                {
                    BackupEventsOption = EventsCB.IsChecked.Value;
                    CurrentPerson.SetBackupOptions((int)Person.BackupOptions.Events, BackupEventsOption);
                }

                if (PhotoAlbumsCB.IsChecked != BackupPhotosOption)
                {
                    BackupPhotosOption = PhotoAlbumsCB.IsChecked.Value;
                    CurrentPerson.SetBackupOptions((int)Person.BackupOptions.Photos, BackupPhotosOption);
                }

                string error = "";
                DBLayer.UpdateBackupOptions(CurrentPerson.ID, BackupWallOption, BackupEventsOption, BackupPhotosOption, out error);
                if (error != "")
                {
                    // TODO: Localize
                    this.ErrorMessage2.Text = "Error updating backup options: " + error;
                }
            }
        }

        

 

    }
}
