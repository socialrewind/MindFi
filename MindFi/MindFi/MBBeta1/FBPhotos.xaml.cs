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

namespace MBBeta1
{
    /// <summary>
    /// Interaction logic for SNMessages.xaml
    /// </summary>
    public partial class FBPhotos : Window
    {
        public FBPhotos()
        {
            InitializeComponent();
            
        }

        //Constructor comming from Main Window
        public FBPhotos(List<int> SelectedPeopleParameter, DBConnector dbParam)
        {
            InitializeComponent();

            db = dbParam;

            SelectedPeople = SelectedPeopleParameter;
            FillPersonLightFromSelectedPeople();

            DateFilterBt_Click(null, null);
        }

        //******************* Attributes
        #region Attributes
        
        List<int> SelectedPeople;
        List<PersonLight> SelectedPeopleDetails;
        

        //Database connection
        DBConnector db;

        //Current Album
        SNPhotoAlbum SelectedAlbum;

        //TagRectangle
        Rectangle rectangle;
        TextBlock TagName;
        

        #endregion

        //************* MEthods
        #region Methods

        void FillPersonLightFromSelectedPeople()
        {
            if (SelectedPeople != null)
            {
                int total = SelectedPeople.Count;
                SelectedPeopleDetails = new List<PersonLight>(); ;
                for (int i = 0; i < total; i++)
                {
                    SelectedPeopleDetails.Add(new PersonLight(db, SelectedPeople[i]));
                }
            }
        }


        void ChangePhotoData()
        {
            SNPhoto p;

            p = SelectedAlbum.Photos[SelectedAlbum.CurrentPhoto - 1];
            p.PhotoNumber = SelectedAlbum.CurrentPhoto;
            CurrentPhoto.DataContext = p;
            PhotoDescriptionContent.Content = p;
            TagsLB.ItemsSource = p.TagsList;
            PhotoCommentsItemsControl.ItemsSource = p.CommentsList;
            PhotoLikesControl.NumberOfLikes = p.Likes.NumberOfLikes;
            PhotoLikesControl.LikesList = p.Likes.LikesList;

        }

        #endregion


        //************* Control Methods
        #region Control Methods
        private void CloseBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DateFilterBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            SNPhotoAlbum TmpPhotoAlbum;

            List<SNPhotoAlbum> AlbumList = new List<SNPhotoAlbum>();

            List<int> AlbumIDs = db.GetAlbumIDs(AlbumDF.StartDateDP.SelectedDate.Value, AlbumDF.EndDateDP.SelectedDate.Value, SelectedPeopleDetails);

            foreach(int AlbumID in AlbumIDs)
            {
                TmpPhotoAlbum = new SNPhotoAlbum(db, AlbumID);
                if (TmpPhotoAlbum.NumberOfPhotos > 0)
                    AlbumList.Add(TmpPhotoAlbum);
            }

            FBAlbumsLC.AlbumListBox.ItemsSource = AlbumList;

            this.Cursor = Cursors.Arrow;
        }

 

        #endregion

        /// <summary>
        /// Opens Album Control from select Album in Album List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FBAlbumsLC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedAlbum = new SNPhotoAlbum();
            SelectedAlbum = (SNPhotoAlbum)FBAlbumsLC.AlbumListBox.SelectedItem;

            if (SelectedAlbum != null)
            {
                FBAlbumControl.PhotoAlbumView.ItemsSource = SelectedAlbum.Photos;
                FBAlbumControl.AlbumCommentsItemsControl.ItemsSource = SelectedAlbum.CommentsList;
                FBAlbumControl.AlbumLikesControl.NumberOfLikes = SelectedAlbum.Likes.NumberOfLikes;
                FBAlbumControl.AlbumLikesControl.LikesList = SelectedAlbum.Likes.LikesList;
                FBAlbumControl.AlbumHeaderCC.Content = SelectedAlbum;
                AlbumGrid.Visibility = System.Windows.Visibility.Visible;
                AlbumGrid.IsEnabled = true;
            }

            CloseBt.IsEnabled = false;
            CloseBt.Visibility = System.Windows.Visibility.Hidden;

        }


        /// <summary>
        /// Opens Photo Slider after double click on Album Control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FBAlbumControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PhotoSliderGrid.Visibility = System.Windows.Visibility.Visible;
            PhotoSliderGrid.IsEnabled = true;
            LeftImageBt.IsEnabled = true;
            RightImageBt.IsEnabled = true;

            if (SelectedAlbum != null)
            {
                PhotoSliderHeaderContent.Content = SelectedAlbum;
                SelectedAlbum.CurrentPhoto = FBAlbumControl.PhotoAlbumView.SelectedIndex + 1;
                ChangePhotoData();

                //Manage navigation buttons
                if (SelectedAlbum.CurrentPhoto <= 1)
                {
                    LeftImageBt.IsEnabled = false;
                }
                if (SelectedAlbum.CurrentPhoto >= (SelectedAlbum.NumberOfPhotos))
                {
                    RightImageBt.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Closes (hides) Album view (grid)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAlbumBt_Click(object sender, RoutedEventArgs e)
        {
            AlbumGrid.Visibility = System.Windows.Visibility.Hidden;
            AlbumGrid.IsEnabled = false;

            CloseBt.IsEnabled = true;
            CloseBt.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Closes (hides) Photo Slider (grid)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClosePhotoSliderBt_Click(object sender, RoutedEventArgs e)
        {
            PhotoSliderGrid.Visibility = System.Windows.Visibility.Hidden;
            PhotoSliderGrid.IsEnabled = false;
        }

        private void LeftImageBt_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAlbum.CurrentPhoto > 1)
            {
                SelectedAlbum.CurrentPhoto--;
                ChangePhotoData();
            }

            if (SelectedAlbum.CurrentPhoto <= 1)
            {
                LeftImageBt.IsEnabled = false;
            }

            if (SelectedAlbum.CurrentPhoto < (SelectedAlbum.NumberOfPhotos))
            {
                RightImageBt.IsEnabled = true;
            }
        }

        private void RightImageBt_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAlbum.CurrentPhoto < (SelectedAlbum.NumberOfPhotos))
            {
                SelectedAlbum.CurrentPhoto++;
                ChangePhotoData();
            }

            if (SelectedAlbum.CurrentPhoto >= (SelectedAlbum.NumberOfPhotos))
            {
                RightImageBt.IsEnabled = false;
            }

            if (SelectedAlbum.CurrentPhoto > 1)
            {
                LeftImageBt.IsEnabled = true;
            }
        }

        private void TagsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var l = sender as ListBox;

            SNPhotoTag t = (SNPhotoTag)l.SelectedItem;

            int XOffset = (int)(PhotoCanvas.ActualWidth - CurrentPhoto.ActualWidth);
            XOffset = XOffset / 2;

            int YOffset = (int)(PhotoCanvas.ActualHeight - CurrentPhoto.ActualHeight);
            YOffset = YOffset / 2;

            int XSize = (int)((PhotoCanvas.ActualWidth - CurrentPhoto.ActualWidth) / 5);
            int YSize = XSize;

            if (t != null)
            {
                int X = (int)(((double)t.X / 100) * CurrentPhoto.ActualWidth) + XOffset - (XSize / 2);
                int Y = (int)(((double)t.Y / 100) * CurrentPhoto.ActualHeight) + YOffset - (YSize / 2);

                if (rectangle == null)
                {
                    rectangle = new Rectangle();
                    rectangle.Width = XSize;
                    rectangle.Height = YSize;
                    Canvas.SetLeft(rectangle, X);
                    Canvas.SetTop(rectangle, Y);
                    rectangle.Stroke = System.Windows.Media.Brushes.Khaki;
                    PhotoCanvas.Children.Add(rectangle);

                    TagName = new TextBlock();
                    TagName.Foreground = System.Windows.Media.Brushes.Khaki;
                    //TagName.Text = t.PersonName;
                    TagName.Text = t.SNPersonID.ToString();
                    Canvas.SetLeft(TagName, X);
                    Canvas.SetTop(TagName, Y + YSize);
                    PhotoCanvas.Children.Add(TagName);
                }
                else
                {
                    rectangle.Width = XSize;
                    rectangle.Height = YSize;
                    Canvas.SetLeft(rectangle, X);
                    Canvas.SetTop(rectangle, Y);

                    //TagName.Text = t.PersonName;
                    TagName.Text = t.SNPersonID.ToString();
                    Canvas.SetLeft(TagName, X);
                    Canvas.SetTop(TagName, Y + YSize);

                }
            }
            else
            {
                rectangle.Width = 0;
                rectangle.Height = 0;
                TagName.Text = "";
            }
        }

        private void PhotoSliderGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (PhotoSliderGrid.IsEnabled)
            {
                if (e.Key == Key.Left)
                {
                    LeftImageBt_Click(null, null);
                    e.Handled = true;
                }
                if (e.Key == Key.Right)
                {
                    RightImageBt_Click(null, null);
                    e.Handled = true;
                }
                if (e.Key == Key.Escape)
                {
                    ClosePhotoSliderBt_Click(null, null);
                    e.Handled = true;
                }

                return;
            }

            if (AlbumGrid.IsEnabled && !PhotoSliderGrid.IsEnabled)
            {
                if (e.Key == Key.Escape)
                {
                    CloseAlbumBt_Click(null, null);
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.Escape)
            {
                CloseBt_Click(null, null);
                e.Handled = true;
            }
            return;
        }

        

        
    }
}
