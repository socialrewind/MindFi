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
//Add Namespaces for Config Files
using System.Configuration;
//Add Namespaces for collections
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;
//Other Projectsd
using MBBetaAPI;

namespace MBBeta1
{
    /// <summary>
    /// Interaction logic for MBMain.xaml
    /// </summary>
    public partial class MBMain : Window
    {
        public MBMain()
        {
            InitializeComponent();

            //Create DB Connection
            CreateDBConnection();

            //Initialize Common Code
            CC = new CommonCode();

        }


        //Constructor coming from MBLogin
        public MBMain(string culture, string user, string conn_param)
        {
            this.Cursor = Cursors.Wait;
            
            InitializeComponent();

            //Set locale
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(culture);

            conn = conn_param;
            CreateDBConnection();

            //Initialize Common Code
            CC = new CommonCode();

            //Initial Load
            InitialLoad();

            //TEst Load
            FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value);

            this.Cursor = Cursors.Arrow;

        }

        #region atributes
        // *************** Attributes  ***********************       
        List<PersonLight> FriendsList;
        ICollectionView FriendsListView;
        List<SocialGroup> FriendLists;
        Person Me;

        //db connection
        private string conn;
        DBConnector db;

        //Common code
        CommonCode CC;

        //Interface
        bool AllFriendsSelected;
        bool AdvancedSearchSelected = false;
        //Posts Display Control
        int Limit = 15;
        int Offset = 0;
        int PostCount = 0;  //Number of posts from query
        ObservableCollection<WallPostStructure> PostList;
        

        #endregion

        //**************** Methods
        #region Methods


        /// <summary>
        /// Gets the posts availabe in a time range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void FillPosts(DateTime start, DateTime end)
        {

            this.Cursor = Cursors.Wait;

            PostCount = db.CountPosts(start, end);
            PostCountTB.Text = PostCount.ToString();
            
            if (PostCount > Limit)
            {
                GetOlderPublicationsBt.IsEnabled = true;
                GetOlderPublicationsBt.Visibility = System.Windows.Visibility.Visible;
            }

            List<int> PostIDs = db.GetPosts(start, end, Offset,Limit);

            PostList = new ObservableCollection<WallPostStructure>();

            foreach (int PostID in PostIDs)
            {
                PostList.Add(new WallPostStructure(db, PostID));
            }

            PostsListWPSC.WallPostStructureListIC.ItemsSource = PostList;

            this.Cursor = Cursors.Arrow;
        }

        private void CreateDBConnection()
        {
            try
            {
                db = new DBConnector(conn);
            }
            catch
            {
                MBError error = new MBError(this, "Main: Reading DB configuration from config file.", 1);
            }
        }

        private void LoadFriends()
        {
            List<int> FriendIDs = db.GetPersonIDs();
            FriendsList = new List<PersonLight>();
            foreach (int ID in FriendIDs)
            {
                FriendsList.Add(new PersonLight(db, ID));
            }

            FriendsList.Sort(delegate(PersonLight p1, PersonLight p2) { return p1.Name.CompareTo(p2.Name); });
            FriendsListView = CollectionViewSource.GetDefaultView(FriendsList);

            FriendsLB.ItemsSource = FriendsListView;

        }

        private void InitialLoad()
        {
            this.Cursor = Cursors.Wait;

            //Load ME: It should always be 1
            Me = new Person(db, 1);
            //paint owner data
            OwnerCC.Content = Me;

            //Load Friends Data into Info Browser
            LoadFriends();
            if (FriendsList != null)
                FriendFilterText.IsEnabled = true;


            this.Cursor = Cursors.Arrow;
        }

        public List<int> GetSelectedPersons(List<PersonLight> SNFriendsList)
        {
            List<int> tmp = new List<int>();

            if (FriendsAndGroupsTC.SelectedIndex == 0)
            {
                if (SNFriendsList != null)
                {

                    foreach (PersonLight p in SNFriendsList)
                    {
                        if (p.Selected)
                        {
                            tmp.Add(p.ID);
                        }
                    }
                }
            }
            else
            {
                //SocialGroup sg = (SocialGroup)GroupsListLB.SelectedItem;
                //if (sg != null)
                //{
                //    foreach (PersonLight Member in sg.Members)
                //    {
                //        tmp.Add(Member.ID);
                //    }
                //}
            }

            return tmp;
        }

        #endregion


        //*********** Controls code **************
        #region Controls Code

        #region General Controls

        private void OwnerDetailsBt_Click(object sender, RoutedEventArgs e)
        {
            if (Me != null)
            {
                this.Cursor = Cursors.Wait;

                DetailCard DetailCardWindow = new DetailCard();
                DetailCardWindow.DataContext = Me;
                CC.PositionNewWindow(this, DetailCardWindow);

                this.Cursor = Cursors.Arrow;
            }

        }

        #endregion

        #region Navigate Controls Methods

        /// <summary>
        /// Takes control of publishing Posts by retrieving only some elements at a time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetOlderPublicationsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            Offset += Limit;        //Add n more records to be retrieved

            if (Offset + Limit >= PostCount)
            {
                GetOlderPublicationsBt.IsEnabled = false;
                GetOlderPublicationsBt.Visibility = System.Windows.Visibility.Hidden;
            }

            List<int> PostIDs = db.GetPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value, Offset, Limit);

            foreach (int PostID in PostIDs)
            {
                PostList.Add(new WallPostStructure(db, PostID));
            }

            PostsListWPSC.WallPostStructureListIC.ItemsSource = PostList;


            this.Cursor = Cursors.Arrow;
        }


        /// <summary>
        /// Changes the current date filter for Navigate Network Updates View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateDateFilterBt_Click(object sender, RoutedEventArgs e)
        {

            //Set offset of first post to retrieve from DB query
            Offset = 0;

            //Ask for Posts and display them
            FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value);

        }

        #endregion

        #region Browse Control Methods

        private void ClearFriendFilterBt_Click(object sender, RoutedEventArgs e)
        {
            FriendFilterText.Text = "";
        }


        private void SelectAllCBox_Checked(object sender, RoutedEventArgs e)
        {

            AllFriendsSelected = true;

            if (FriendsList != null)
            {

                foreach (PersonLight p in FriendsList)
                {
                    p.Selected = true;
                }

                FriendsListView.Refresh();

            }

        }


        private void SelectAllCBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AllFriendsSelected)
            {
                if (FriendsList != null)
                {
                    foreach (PersonLight p in FriendsList)
                    {
                        p.Selected = false;
                    }
                    FriendsListView.Refresh();
                    SelectAllCBox.IsChecked = false;
                }
            }
        }

        private void ClearSelectedBt_Click(object sender, RoutedEventArgs e)
        {
            if (FriendsList != null)
            {
                foreach (PersonLight p in FriendsList)
                {
                    p.Selected = false;
                }
                FriendsListView.Refresh();
                SelectAllCBox.IsChecked = false;
            }
        }

        private void FriendSelectedCB_UnChecked(object sender, RoutedEventArgs e)
        {
            AllFriendsSelected = false;
            SelectAllCBox.IsChecked = false;
        }

        private void ShowSelectedTB_Checked(object sender, RoutedEventArgs e)
        {
            if (FriendsList != null)
            {

                FriendsListView.Filter = delegate(object obj)
                {

                    PersonLight f = obj as PersonLight;

                    if (f == null)
                        return false;

                    return f.Selected;

                };

                FriendsListView.Refresh();
            }

            //Change Selected Button Text
            LocTextExtension loc = new LocTextExtension("MBBeta1:MBStrings:FriendFilterShowAll");
            loc.SetBinding(ShowSelectedTB, Button.ContentProperty);
        }

        private void ShowSelectedTB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (FriendsList != null)
            {
                FriendsListView.Filter = null;
                FriendsListView.Refresh();
                ShowSelectedTextBox.DataContext = null;

            }

            //Change Selected Button Text
            LocTextExtension loc = new LocTextExtension("MBBeta1:MBStrings:FriendFilterShowSelected");
            loc.SetBinding(ShowSelectedTB, Button.ContentProperty);
        }



        private void FriendFilterText_TextChanged(object sender, TextChangedEventArgs e)
        {

            string filterText = FriendFilterText.Text;

            FriendsListView.Filter = delegate(object obj)
            {
                if (String.IsNullOrEmpty(filterText))
                    return true;

                //current object of filter function

                PersonLight f = obj as PersonLight;

                if (f == null)
                    return false;

                //Get the name
                string str = f.Name;

                if (String.IsNullOrEmpty(str))
                    return false;

                //See if filtertext is found as a subset of FirstName
                int index = str.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);

                if (index > -1)
                {
                    return true;
                }

                // if index is gt -1 return true, else false
                return index > -1;

            };

            FriendsListView.Refresh();
        }




        


        private void FriendsLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FriendsLB.SelectedIndex != -1)
            {
                this.Cursor = Cursors.Wait;

                PersonLight selectedItem = (PersonLight)FriendsLB.SelectedItem;

                Person p = new Person(db, selectedItem.ID);

                DetailCard DetailCardWindow = new DetailCard();
                DetailCardWindow.DataContext = p;
                CC.PositionNewWindow(this, DetailCardWindow);


                this.Cursor = Cursors.Arrow;

            }
        }

       


        #endregion

        #region Search Control Methods

        private void AdvancedSearchTB_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancedSearchSelected)
            {
                AdvancedSearchGrid.IsEnabled = false;
                AdvancedSearchGrid.Visibility = System.Windows.Visibility.Collapsed;
                //Change Selected Button Text
                LocTextExtension loc = new LocTextExtension("MBBeta1:MBStrings:EnableAdvancedSearch");
                loc.SetBinding(AdvancedSearchTB, Button.ContentProperty);
                AdvancedSearchSelected = false;
            }
            else
            {
                AdvancedSearchGrid.IsEnabled = true;
                AdvancedSearchGrid.Visibility = System.Windows.Visibility.Visible;
                //Change Selected Button Text
                LocTextExtension loc = new LocTextExtension("MBBeta1:MBStrings:DisableAdvancedSearch");
                loc.SetBinding(AdvancedSearchTB, Button.ContentProperty);
                AdvancedSearchSelected = true;
            }
        }
        #endregion

        private void FBMessagesBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var MessagesWindow = new SNMessages(GetSelectedPersons(FriendsList), db);
            CC.PositionNewWindow(this, MessagesWindow);

            this.Cursor = Cursors.Arrow;
            
        }

        #endregion

        private void FBPhotosBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var PhotosWindow = new FBPhotos(GetSelectedPersons(FriendsList), db);
            CC.PositionNewWindow(this, PhotosWindow);

            this.Cursor = Cursors.Arrow;
        }


    }
}
