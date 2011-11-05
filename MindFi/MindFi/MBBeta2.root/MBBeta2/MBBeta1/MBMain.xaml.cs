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
//Regex
using System.Text.RegularExpressions;
//Other Projects
using MBBetaAPI;
using MBBetaAPI.AgentAPI;

namespace MBBeta2
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
        /// <summary>
        /// Constructor coming from MBLogin
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="user"></param>
        /// <param name="conn_param"></param>
        /// <param name="level"> 2:No SN Accounts, 3: At least one SN Account</param>
        public MBMain(string culture, string user, string conn_param, int level)
        {
            this.Cursor = Cursors.Wait;

            //Set locale
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(culture);

            InitializeComponent();

            conn = conn_param;
            CreateDBConnection();

            //Initialize Common Code
            CC = new CommonCode();

            if (level > 2)
            {
                //Initial Load
                InitialLoad();

                //TEst Load
                FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value);
            }
            this.Cursor = Cursors.Arrow;
            // check
            this.Show();
            // TODO: Make sure CurrentProfile is assigned when logging in successfuly
            DBLayer.ConnString = db.ConnString;
            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                MessageBox.Show("Error getting accounts from the database:\n" + error);
            }
            // TODO: Review how it can be empty and not null
            if (currentAccounts == null || currentAccounts.Count == 0)
            {
                OpenSetupWindow();
            }

        }

        #region atributes
        // *************** Attributes  ***********************       
        List<PersonLight> FriendsList;
        ICollectionView FriendsListView;
        List<SNSocialGroup> GroupsList;
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

        //Search Control
        Search CurrentSearch;
        

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

            //PostsListWPSC.WallPostStructureListIC.ItemsSource = PostList;

            if (PostList == null)
            {
                PostList = new ObservableCollection<WallPostStructure>();
            }

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
            List<int> FriendIDs = db.GetFriendIDs();
            FriendsList = new List<PersonLight>();
            foreach (int ID in FriendIDs)
            {
                FriendsList.Add(new PersonLight(db, ID));
            }

            FriendsList.Sort(delegate(PersonLight p1, PersonLight p2) { return p1.Name.CompareTo(p2.Name); });
            FriendsListView = CollectionViewSource.GetDefaultView(FriendsList);

            FriendsLB.ItemsSource = FriendsListView;

        }

        private void LoadSocialGroups()
        {
            List<int> GroupIDs = db.GetSocialGroupIDs();
            GroupsList = new List<SNSocialGroup>();
            foreach (int ID in GroupIDs)
            {
                GroupsList.Add(new SNSocialGroup(db, ID));
            }

            GroupsList.Sort(delegate(SNSocialGroup g1,  SNSocialGroup g2) { return g1.Name.CompareTo(g2.Name); });
            //FriendsListView = CollectionViewSource.GetDefaultView(FriendsList);

            

            SocialGroupsLB.ItemsSource = GroupsList;

        }

        private void InitialLoad()
        {
            this.Cursor = Cursors.Wait;

            //Load ME: It should always be 1
            //TODO: Change to MeOnFB, MeOnTwitter, MeOnLn when more data is available
            Me = new Person(db, 1);
            //paint owner data
            OwnerCC.Content = Me;

            //Load Friends Data into Info Browser
            LoadFriends();
            if (FriendsList != null)
                FriendFilterText.IsEnabled = true;

            //Load Social Groups
            LoadSocialGroups();

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
                SNSocialGroup sg = (SNSocialGroup)SocialGroupsLB.SelectedItem;
                if (sg != null)
                {
                    foreach (int PersonID in sg.MemberIDs)
                    {
                        tmp.Add(PersonID);
                    }
                }
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

        private void SetupBt_Click(object sender, RoutedEventArgs e)
        {
            OpenSetupWindow();
        }

        private void OpenSetupWindow()
        {
            var MBSetupWindow = new MBSetup(db);
            CC.PositionNewWindow(this, MBSetupWindow);
        }

        private void Enter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
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

            WallPostStructure tmp;

            Offset += Limit;        //Add n more records to be retrieved

            if (Offset + Limit >= PostCount)
            {
                GetOlderPublicationsBt.IsEnabled = false;
                GetOlderPublicationsBt.Visibility = System.Windows.Visibility.Hidden;
            }

            
            List<int> PostIDs = db.GetPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value, Offset, Limit);


            foreach (int PostID in PostIDs)
            {
                tmp = new WallPostStructure(db, PostID);
                PostList.Add(tmp);
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
            PostList = null;
            FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value);

        }


        private void RecordsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Limit = (int)RecordsSlider.Value;
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
            LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:FriendFilterShowAll");
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
            LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:FriendFilterShowSelected");
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

        private void FBPhotosBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var PhotosWindow = new FBPhotos(GetSelectedPersons(FriendsList), db);
            CC.PositionNewWindow(this, PhotosWindow);

            this.Cursor = Cursors.Arrow;
        }

        private void FBMessagesBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var MessagesWindow = new SNMessages(GetSelectedPersons(FriendsList), db);
            CC.PositionNewWindow(this, MessagesWindow);

            this.Cursor = Cursors.Arrow;

        }

        private void FBWallBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var WallPostWindow = new FBWallPosts(GetSelectedPersons(FriendsList), db);
            CC.PositionNewWindow(this, WallPostWindow);

            this.Cursor = Cursors.Arrow;
        }

        private void FBEventsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var EventsWindow = new FBEvents(GetSelectedPersons(FriendsList), db);
            CC.PositionNewWindow(this, EventsWindow);

            this.Cursor = Cursors.Arrow;
        }
        #endregion

        #region Search Control Methods


        void fillSearchResults()
        {
            ParamText.Text = SearchTB.Text;

            FBWPCountTB.Text = CurrentSearch.MatchingPostIDs.Count().ToString();
            FBAlbumsCountTB.Text = CurrentSearch.MatchingAlbumIDs.Count().ToString();
            FBPhotosCountTB.Text = CurrentSearch.MatchingPhotoIDs.Count().ToString();
            FBMessagesCountTB.Text = CurrentSearch.MatchingMessageIDs.Count().ToString();
            FBEventsCountTB.Text = CurrentSearch.MatchingEventIDs.Count().ToString();
        }

        private void AdvancedSearchTB_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancedSearchSelected)
            {
                AdvancedSearchGrid.IsEnabled = false;
                AdvancedSearchGrid.Visibility = System.Windows.Visibility.Collapsed;
                //Change Selected Button Text
                LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:EnableAdvancedSearch");
                loc.SetBinding(AdvancedSearchTB, Button.ContentProperty);
                AdvancedSearchSelected = false;
            }
            else
            {
                AdvancedSearchGrid.IsEnabled = true;
                AdvancedSearchGrid.Visibility = System.Windows.Visibility.Visible;
                //Change Selected Button Text
                LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:DisableAdvancedSearch");
                loc.SetBinding(AdvancedSearchTB, Button.ContentProperty);
                AdvancedSearchSelected = true;
            }
        }

        private void SearchBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            CurrentSearch = null;

            if (SearchTB.Text != "")
            {
                Regex regex = new Regex("(^[0-9a-zA-Z\\s\\*]+$)");

                if (!regex.IsMatch(SearchTB.Text))
                {
                    MessageBox.Show("Please use only letters and numbers");
                }
                else  //Perform search
                {
                    if (AdvancedSearchSelected)
                    {
                        bool[] searchOps = new bool[5];
                        searchOps[0] = (bool)FBWallSearchCB.IsChecked;
                        searchOps[1] = (bool)FBAlbumSearchCB.IsChecked;
                        searchOps[2] = (bool)FBPhotoSearchCB.IsChecked;
                        searchOps[3] = (bool)FBMessageSearchCB.IsChecked;
                        searchOps[4] = (bool)FBEventSearchCB.IsChecked;

                        BitArray SearchOptions = new BitArray(searchOps);

                        CurrentSearch = new Search(db, SearchTB.Text, SearchOptions, DateSearchFilter.StartDateDP.SelectedDate.Value, DateSearchFilter.EndDateDP.SelectedDate.Value);
                    }
                    else
                    {
                        CurrentSearch = new Search(db, SearchTB.Text);
                    }

                    fillSearchResults();
                }
            }

            SearchResultsGrid.Visibility = System.Windows.Visibility.Visible;

            this.Cursor = Cursors.Arrow;

        }

        private void FBWallSearchResultsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            if (CurrentSearch !=null)
            if (CurrentSearch.MatchingPostIDs.Count() > 0)
            {
                List<WallPostStructure> tmp = new List<WallPostStructure>();

                foreach (int PostID in CurrentSearch.MatchingPostIDs)
                {
                    tmp.Add(new WallPostStructure(db, PostID));
                }

                var PostResultsWindow = new FBWallPosts(tmp);
                CC.PositionNewWindow(this, PostResultsWindow);
            }

            this.Cursor = Cursors.Arrow;
        }


        private void FBAlbumsSearchResultsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            if (CurrentSearch != null)
            if (CurrentSearch.MatchingAlbumIDs.Count() > 0)
            {

                List<SNPhotoAlbum> tmp = new List<SNPhotoAlbum>();
                foreach (int AlbumID in CurrentSearch.MatchingAlbumIDs)
                {
                    tmp.Add(new SNPhotoAlbum(db, AlbumID));
                }

                var ResultsWindow = new FBPhotos(tmp);
                CC.PositionNewWindow(this, ResultsWindow);
            }

            this.Cursor = Cursors.Arrow;
        }

        private void FBSearchResultsPhotosBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            if (CurrentSearch != null)
            if (CurrentSearch.MatchingPhotoIDs.Count() > 0)
            {

                //Create temp album to show matching photos
                List<SNPhotoAlbum> tmp = new List<SNPhotoAlbum>();

                //Create an dummy album
                SNPhotoAlbum pa = new SNPhotoAlbum(1);
                pa.Name = "Search results for " + CurrentSearch.SearchText;
                pa.NumberOfPhotos = CurrentSearch.MatchingPhotoIDs.Count();
                //pa.CurrentPhoto = 1;
                pa.Likes = new LikeStructure();
                pa.PhotoRibbon = new List<SNPhoto>();
                pa.Photos = new List<SNPhoto>();

                int i = 0;
                foreach (int PhotoID in CurrentSearch.MatchingPhotoIDs)
                {
                    SNPhoto NewPhoto = new SNPhoto(db, PhotoID);
                    if (i <= 5)
                        pa.PhotoRibbon.Add(NewPhoto);
                    pa.Photos.Add(NewPhoto);
                    i++;
                }

                tmp.Add(pa);

                var ResultsWindow = new FBPhotos(tmp);
                CC.PositionNewWindow(this, ResultsWindow);
            }

            this.Cursor = Cursors.Arrow;
        }

        private void FBSearchREsultsMessagesBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            if (CurrentSearch != null)
            if (CurrentSearch.MatchingMessageIDs.Count() > 0)
            {

                List<SNMessage> tmp = new List<SNMessage>();
                foreach (int MessageID in CurrentSearch.MatchingMessageIDs)
                {
                    tmp.Add(new SNMessage(db, MessageID));
                }

                var ResultsWindow = new SNMessages(db, tmp);
                CC.PositionNewWindow(this, ResultsWindow);
            }

            this.Cursor = Cursors.Arrow;
        }

        private void FBSearhcResultsEventsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            if (CurrentSearch != null)
            if (CurrentSearch.MatchingEventIDs.Count() > 0)
            {

                List<SNEvent> tmp = new List<SNEvent>();

                foreach (int EventID in CurrentSearch.MatchingEventIDs)
                {
                    tmp.Add(new SNEvent(db, EventID));
                }

                var ResultsWindow = new FBEvents(tmp);
                CC.PositionNewWindow(this, ResultsWindow);
            }

            this.Cursor = Cursors.Arrow;
        }

        #endregion



        #endregion

    }
}
