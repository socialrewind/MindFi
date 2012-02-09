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
// support timers
using System.Windows.Threading;
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
        private string animation = "...";
        private string BasePath;
        private bool online = false;
        private string ErrorMessage = "";
        private string ZipOperation = "";

        #region "Process control"
        private bool firstTime = true;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        #endregion

        //Constructor coming from MBLogin
        /// <summary>
        /// Constructor coming from MBLogin
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="user"></param>
        /// <param name="conn_param"></param>
        /// <param name="level"> 2:No SN Accounts, 3: At least one SN Account</param>
        public MBMain(string culture, string user, string basePathparam, string conn_param, int level)
        {
            this.Cursor = Cursors.Wait;

            //Set locale
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(culture);

            InitializeComponent();

            conn = conn_param;
            BasePath = basePathparam;
            // CreateDBConnection();

            //Initialize Common Code
            CC = new CommonCode();

            if (level > 2)
            {
                //Initial Load
                InitialLoad();

                //TEst Load
                FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value.AddDays(1));
            }
            this.Cursor = Cursors.Arrow;
            // check
            this.Show();
            // TODO: Make sure CurrentProfile is assigned when logging in successfuly

            DBLayer.ConnString = conn; // db.ConnString;
            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                MessageBox.Show("Error getting accounts from the database:\n" + error);
            }

            // Information about backup data
            // TODO: Localize, maybe modularize
            string backupState, backupDate;
            DBLayer.GetLastBackup(out backupState, out backupDate);
            this.UpdateText.Text = backupState;
            this.UpdateTime.Text = "Updated: " + backupDate;

            // TODO: Review how it can be empty and not null
            if (currentAccounts == null || currentAccounts.Count == 0)
            {
                OpenSetupWindow();
            }
            // can change after setup runs
            if (currentAccounts != null && currentAccounts.Count != 0)
            {
                SNAccount first = (SNAccount)currentAccounts[0];
                SNAccount.UpdateCurrent(first);
            }

            CheckConnection();
        }

        #region atributes
        // *************** Attributes  ***********************       
        List<PersonLight> FriendsList;
        ICollectionView FriendsListView;
        List<SNSocialGroup> GroupsList;
        Person Me;
        //db connection
        private string conn;

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

            PostCount = DBLayer.CountPosts(start, end);
            PostCountTB.Text = PostCount.ToString();
            
            if (PostCount > Limit)
            {
                GetOlderPublicationsBt.IsEnabled = true;
                GetOlderPublicationsBt.Visibility = System.Windows.Visibility.Visible;
            }



            List<int> PostIDs = DBLayer.GetPosts(start, end, Offset, Limit);

            //PostsListWPSC.WallPostStructureListIC.ItemsSource = PostList;

            if (PostList == null)
            {
                PostList = new ObservableCollection<WallPostStructure>();
            }

            foreach (int PostID in PostIDs)
            {
                PostList.Add(new WallPostStructure(PostID));
            }


            PostsListWPSC.WallPostStructureListIC.ItemsSource = PostList;

            this.Cursor = Cursors.Arrow;
        }

        private void LoadFriends()
        {
            List<int> FriendIDs = DBLayer.GetFriendIDs();
            FriendsList = new List<PersonLight>();
            foreach (int ID in FriendIDs)
            {
                FriendsList.Add(new PersonLight(ID));
            }

            FriendsList.Sort(delegate(PersonLight p1, PersonLight p2) { return p1.Name.CompareTo(p2.Name); });
            FriendsListView = CollectionViewSource.GetDefaultView(FriendsList);

            FriendsLB.ItemsSource = FriendsListView;

        }

        private void LoadSocialGroups()
        {
            List<int> GroupIDs = DBLayer.GetSocialGroupIDs();
            GroupsList = new List<SNSocialGroup>();
            foreach (int ID in GroupIDs)
            {
                GroupsList.Add(new SNSocialGroup(ID));
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
            Me = new Person(1);
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


        private void CheckConnection()
        {
            string state;
            string fbState;

            LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:Disconnected");
            loc.ResolveLocalizedValue(out state);
            
            loc = new LocTextExtension("MBBeta2:MBStrings:DisconnectedFB");
            loc.ResolveLocalizedValue(out fbState);

            // ping SocialRewind Server
            if (MBBetaAPI.SRAPI.SRConnection.CheckInternetConnection())
            {
                loc = new LocTextExtension("MBBeta2:MBStrings:Connected");
                loc.ResolveLocalizedValue(out state);
                if (FBLogin.loggedIn)
                {
                    loc = new LocTextExtension("MBBeta2:MBStrings:ConnectedFB");
                    loc.ResolveLocalizedValue(out fbState);
                }
            }
            Connected.Text = state;
            ConnectedFB.Text = fbState;
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
                PersonWrapper dataContext = new PersonWrapper(Me);
                DetailCardWindow.DataContext = dataContext;
                CC.PositionNewWindow(this, DetailCardWindow);
                //Tests.AsyncDetailCard DetailCardWindow = new Tests.AsyncDetailCard();
                //CC.PositionNewWindow(this, DetailCardWindow);

                this.Cursor = Cursors.Arrow;
            }

        }

        private void SetupBt_Click(object sender, RoutedEventArgs e)
        {
            OpenSetupWindow();
        }

        private void OpenSetupWindow()
        {
            var MBSetupWindow = new MBSetup(BasePath);
            CC.PositionNewWindow(this, MBSetupWindow);
            if (MBSetupWindow.accountAdded)
            {
                DoRefreshData();
                GoOnline();
            }
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

        #region PostNewStatus

        private void PostNewStatusTB_GotFocus(object sender, RoutedEventArgs e)
        {
            PostNewStatusTB.Text = "";
        }


        private void PostNewStatusTB_LostFocus(object sender, RoutedEventArgs e)
        {
            LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:WhatAreYouThinking");
            loc.SetBinding(PostNewStatusTB, TextBox.TextProperty);
        }

        private void PostNewStatusTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                //The code to post an status goes here
                //MessageBox.Show("Create new post code goes here");
                FBAPI.UpdateStatus("me", PostNewStatusTB.Text, ProcessStatus );

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// process update status response
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public bool ProcessStatus(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                FBPost statusObj = new FBPost(response);
                statusObj.Parse();
                statusObj.Message = this.PostNewStatusTB.Text;
                statusObj.PostType = "status";
                statusObj.ApplicationName = "Social Rewind";
                string errorData;
                statusObj.Save(out errorData);
                if (errorData != "")
                {
                    ErrorMessage = errorData;
                }
                /*
                FBAPI.UpdateLike(statusObj.SNID, ProcessNull);
                FBAPI.AddComment(statusObj.SNID, "test of a comment", ProcessNull);
                 */

                return true;
            }
            return false;
        }

        public bool ProcessNull(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                return true;
            }
            return false;
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


            List<int> PostIDs = DBLayer.GetPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value.AddDays(1), Offset, Limit);


            foreach (int PostID in PostIDs)
            {
                tmp = new WallPostStructure(PostID);
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
            //Add 1 day, so calculation ends at 12:00 AM of next day
            DateTime EndDate = NavigateDF.EndDateDP.SelectedDate.Value.AddDays(1);
            FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, EndDate);
            
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

                Person p = new Person(selectedItem.ID);

                DetailCard DetailCardWindow = new DetailCard();
                PersonWrapper dataContext = new PersonWrapper(p);
                DetailCardWindow.DataContext = dataContext;
                CC.PositionNewWindow(this, DetailCardWindow);

                this.Cursor = Cursors.Arrow;

            }
        }

        private void FBPhotosBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var PhotosWindow = new FBPhotos(GetSelectedPersons(FriendsList));
            CC.PositionNewWindow(this, PhotosWindow);

            this.Cursor = Cursors.Arrow;
        }

        private void FBMessagesBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var MessagesWindow = new SNMessages(GetSelectedPersons(FriendsList));
            CC.PositionNewWindow(this, MessagesWindow);

            this.Cursor = Cursors.Arrow;

        }

        private void FBWallBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var WallPostWindow = new FBWallPosts(GetSelectedPersons(FriendsList));
            CC.PositionNewWindow(this, WallPostWindow);

            this.Cursor = Cursors.Arrow;
        }

        private void FBEventsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var EventsWindow = new FBEvents(GetSelectedPersons(FriendsList));
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
                // restricted restrictions - probably should only take out spaces...

                //Regex regex = new Regex("(^[0-9a-zA-Z\\s\\*]+$)");

                //if (!regex.IsMatch(SearchTB.Text))
                //{
                //    // TODO: Localize
                //    MessageBox.Show("Please use only letters and numbers");
                //}
                //else  //Perform search
                //{
                    if (AdvancedSearchSelected)
                    {
                        bool[] searchOps = new bool[5];
                        searchOps[0] = (bool)FBWallSearchCB.IsChecked;
                        searchOps[1] = (bool)FBAlbumSearchCB.IsChecked;
                        searchOps[2] = (bool)FBPhotoSearchCB.IsChecked;
                        searchOps[3] = (bool)FBMessageSearchCB.IsChecked;
                        searchOps[4] = (bool)FBEventSearchCB.IsChecked;

                        BitArray SearchOptions = new BitArray(searchOps);

                        CurrentSearch = new Search(SearchTB.Text, SearchOptions, DateSearchFilter.StartDateDP.SelectedDate.Value, DateSearchFilter.EndDateDP.SelectedDate.Value.AddDays(1));
                    }
                    else
                    {
                        CurrentSearch = new Search(SearchTB.Text);
                    }

                    fillSearchResults();
                //}
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
                    tmp.Add(new WallPostStructure(PostID));
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
                    tmp.Add(new SNPhotoAlbum(AlbumID));
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
                    SNPhoto NewPhoto = new SNPhoto(PhotoID);
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
                    tmp.Add(new SNMessage(MessageID));
                }

                var ResultsWindow = new SNMessages(tmp);
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
                    tmp.Add(new SNEvent(EventID));
                }

                var ResultsWindow = new FBEvents(tmp);
                CC.PositionNewWindow(this, ResultsWindow);
            }

            this.Cursor = Cursors.Arrow;
        }

        #endregion

        private void RefreshDataBt_Click(object sender, RoutedEventArgs e)
        {
            DoRefreshData();
        }

        private void DoRefreshData()
        {
            this.Cursor = Cursors.Wait;
            this.UpdateText.Text = "Refreshing data...";

            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                MessageBox.Show("Error getting accounts from the database:\n" + error);
            }
            // TODO: Review how it can be empty and not null
            if (currentAccounts != null && currentAccounts.Count != 0)
            {

                //Load ME: It should always be 1
                //TODO: Change to MeOnFB, MeOnTwitter, MeOnLn when more data is available
                Me = new Person(1);
                //paint owner data
                OwnerCC.Content = Me;

                //Load Friends Data into Info Browser
                LoadFriends();
                if (FriendsList != null)
                    FriendFilterText.IsEnabled = true;

                //Load Social Groups
                LoadSocialGroups();

                //TEst Load
                PostList = null;
                FillPosts(NavigateDF.StartDateDP.SelectedDate.Value, NavigateDF.EndDateDP.SelectedDate.Value.AddDays(1));
            }

            // TODO: Localize
            string backupState, backupDate;
            DBLayer.GetLastBackup(out backupState, out backupDate);
            this.UpdateText.Text = backupState;
            this.UpdateTime.Text = "Updated: " + backupDate;

            CheckConnection();
            this.Cursor = Cursors.Arrow;
        }



        #endregion

        private void BackupDataBt_Click(object sender, RoutedEventArgs e)
        {
            if (online)
            {
                GoOffline();
            }
            else
            {
                GoOnline();
            }

        }

        private void GoOffline()
        {
            online = false;
            dispatcherTimer.Stop();
        }

        private void GoOnline()
        {
            online = true;
            // display login
            DoLogin(this);
            dispatcherTimer.Tick += new EventHandler(processTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private static void DoLogin(Window myOwner)
        {
            var SNLoginWindow = new MBSNLogin(true);
            SNLoginWindow.Owner = myOwner;
            SNLoginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            SNLoginWindow.Show();
        }

        private void SNLoggedInAndInitialRequests()
        {
            FBPerson me = FBLogin.Me;
            if (SNAccount.CurrentProfile != null)
            {
                // TODO: Generalize for other social networks
                if ((SNAccount.CurrentProfile.Name != me.Name
                    && SNAccount.CurrentProfile.SNID != me.SNID
                    && SNAccount.CurrentProfile.URL != me.Link) ||
                    SNAccount.CurrentProfile.SocialNetwork != SocialNetwork.FACEBOOK
                    )
                {
                    // TODO: Localize
                    MessageBox.Show("You tried to login with a different account (" + me.Name
                        + ") instead of the selected account (" + SNAccount.CurrentProfile.Name
                        + "). Please correct your data; login cancelled.");
                    return;
                }
                firstTime = false;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                // TODO: Make sure current ID is really 1
                AsyncReqQueue.NewRequests(150, 1, SNAccount.CurrentProfile.SNID);
                this.UpdateText.Text = "Getting initial friend data" + animation;
            }
        }

        private void SNNextRequests()
        {
            if (SNAccount.CurrentProfile != null)
            {
                string ErrorMessage;
                // TODO: Make sure current ID is really 1
                bool inProgress = AsyncReqQueue.PendingRequests(150, 1, SNAccount.CurrentProfile.SNID, out ErrorMessage);
                // TODO: Localization
                switch (AsyncReqQueue.CurrentBackupState)
                {
                    case AsyncReqQueue.BACKUPFRIENDSINFO:
                        this.UpdateText.Text = "Getting friend data (" + AsyncReqQueue.nFriendsProcessed + ")" + animation;
                        break;
                    case AsyncReqQueue.BACKUPFRIENDSPROFPIC:
                        this.UpdateText.Text = "Getting friend profile pictures (" + AsyncReqQueue.nFriendsPictures + ")" + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYWALL:
                        this.UpdateText.Text = "Getting my wall posts (" + AsyncReqQueue.nPosts + ") from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYNEWS:
                        this.UpdateText.Text = "Getting news posts (" + AsyncReqQueue.nPosts + ") from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYINBOX:
                        this.UpdateText.Text = "Getting my inbox (" + AsyncReqQueue.nMessages + ") from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYEVENTS:
                        this.UpdateText.Text = "Getting my events (" + AsyncReqQueue.nEvents + ") from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYALBUMS:
                        this.UpdateText.Text = "Getting my albums (" + AsyncReqQueue.nAlbums + ") from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYNOTIFICATIONS:
                        this.UpdateText.Text = "Getting my notifications from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPFRIENDSWALLS:
                        this.UpdateText.Text = "Getting friends walls (" + AsyncReqQueue.nFriendsWalls + "/" + AsyncReqQueue.nPosts + " posts) from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                    case AsyncReqQueue.BACKUPMYPHOTOS:
                        this.UpdateText.Text = "Getting my photos (" + AsyncReqQueue.nPhotos + " ) from " +
                            SNAccount.CurrentProfile.CurrentPeriodStart.ToShortDateString() + " to " +
                            SNAccount.CurrentProfile.CurrentPeriodEnd.ToShortDateString() + animation;
                        break;
                }
                if (!inProgress)
                {
                    //MessageBox.Show("Backup finished");
                    DoRefreshData();
                    // TODO: Localize
                    this.UpdateText.Text = "Backup just finished";
                    this.UpdateTime.Text = DateTime.UtcNow.ToString();
                    GoOffline();
                    dispatcherTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Processing event for regular timer
        /// </summary>
        private void processTimer_Tick(object sender, EventArgs e)
        {
            if (!DBLayer.DatabaseBusy)
            {
                if (FBLogin.loggedIn && FBLogin.Me != null)
                {
                    if (firstTime)
                    {
                        SNLoggedInAndInitialRequests();
                    }
                    else
                    {
                        SNNextRequests();
                    }
                }
                else
                {
                    CheckConnection();
                    // some timeout so it is not checking forever...
                }
                UpdateAnimation();
                this.UpdateTime.Text = AsyncReqQueue.nNotifications.ToString() + " news";
            }
        }

        private void UpdateAnimation()
        {
            if (animation != "...")
            {
                animation += ".";
            }
            else
            {
                animation = "";
            }
        }

        #region "Zip and unzip"

        private void btnZip_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            // Get destination path
            DateTime Now = DateTime.Now;
            string tempD = Now.Year.ToString();
            if (Now.Month < 10) tempD += "0";
            tempD += Now.Month;
            if (Now.Day < 10) tempD += "0";
            tempD += Now.Day;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = ""; // current from previous file dialog
            dlg.FileName = "SocialRewindBackup" + tempD + ".zip";
            dlg.AddExtension = true;

            LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:BackupFiles");
            string LocFilter;
            loc.ResolveLocalizedValue(out LocFilter);
            dlg.Filter = LocFilter; // "Compressed backup files (*.zip)|*.zip"
            dlg.FilterIndex = 1;

            string LocSelect;
            loc = new LocTextExtension("MBBeta2:MBStrings:SelectBackupName");
            loc.ResolveLocalizedValue(out LocSelect);
            dlg.Title = LocSelect;  // "Select the name for the backup file";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string Destination = dlg.FileName;
                //MessageBox.Show("Would create backup " + Destination + " from compressing the folder " + BasePath);
                btnZip.IsEnabled = false;
                btnUnzip.IsEnabled = false;
                // TODO: Localize
                ZipOperation = "Zipping backup";
                SRZipBackup.CreateZipBackup(BasePath, Destination);

                dispatcherTimer.Tick += new EventHandler(backupTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();

            }
        }

        private void backupTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateText.Text = ZipOperation + animation;
            UpdateAnimation();
            if (!SRZipBackup.InBackup)
            {
                if (SRZipBackup.ErrorMessage == null || SRZipBackup.ErrorMessage == "")
                {
                    this.UpdateText.Text = ZipOperation + " was completed.";
                }
                else
                {
                    this.UpdateText.Text = SRZipBackup.ErrorMessage;
                }
                dispatcherTimer.Stop();
                btnZip.IsEnabled = true;
                btnUnzip.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnUnzip_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = ""; // current from previous file dialog
            dlg.AddExtension = true;

            LocTextExtension loc = new LocTextExtension("MBBeta2:MBStrings:BackupFilesAllFiles");
            string LocFilter;
            loc.ResolveLocalizedValue(out LocFilter);
            dlg.Filter = LocFilter; // "Compressed backup files (*.zip)|*.zip"
            dlg.FilterIndex = 1;

            string LocSelect;
            loc = new LocTextExtension("MBBeta2:MBStrings:SelectBackupName");
            loc.ResolveLocalizedValue(out LocSelect);
            dlg.Title = LocSelect;  // "Select the name for the backup file";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                System.Windows.Forms.FolderBrowserDialog dlg2 = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult res;
                res = dlg2.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    if ( dlg2.SelectedPath == BasePath || BasePath.Contains(dlg2.SelectedPath) )
                    {
                        MessageBox.Show("Cannot unzip to the currently used database folder (or a parent folder). Please use a different folder.");
                        return;
                    }
                    this.Cursor = Cursors.Wait;
                    btnZip.IsEnabled = false;
                    btnUnzip.IsEnabled = false;
                    ZipOperation = "Unzipping backup";
                    //MessageBox.Show("ready to unzip " + dlg.FileName + " to " + dlg2.SelectedPath);
                    SRZipBackup.UnzipBackup(dlg.FileName, dlg2.SelectedPath);

                    dispatcherTimer.Tick += new EventHandler(backupTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    dispatcherTimer.Start();
                }
            }
        }

        #endregion

    }
}
