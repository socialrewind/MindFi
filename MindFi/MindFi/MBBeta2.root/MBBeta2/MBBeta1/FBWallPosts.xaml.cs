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
    /// Interaction logic for FBWallPosts.xaml
    /// </summary>
    public partial class FBWallPosts : Window
    {
        public FBWallPosts()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor tha starts the window with parameters coming from main window
        /// </summary>
        /// <param name="selectedPeopleParameter"></param>
        /// <param name="dbParam"></param>
        public FBWallPosts(List<int> selectedPeopleParameter, DBConnector dbParam)
        {
            InitializeComponent();

            db = dbParam;
            SelectedPeople = selectedPeopleParameter;

            FillPersonDetailsFromSelectedPeople();

            //fill with predefined period
            DateFilterBt_Click(null, null);

        }

        /// <summary>
        /// Constructor coming from Search Results
        /// </summary>
        /// <param name="CurrentSearch"></param>
        public FBWallPosts(List<WallPostStructure> SearchPosts)
        {
            InitializeComponent();

            this.Cursor = Cursors.Wait;

            Posts = SearchPosts;

            //Hide navigation options from user
            PostsDF.Visibility = System.Windows.Visibility.Hidden;
            DateFilterBt.Visibility = System.Windows.Visibility.Hidden;

            BrowsePostsCtrl.WallPostStructureListIC.ItemsSource = Posts;

            this.Cursor = Cursors.Arrow;

        }

        //Atributes
        #region Attributes
        
        DBConnector db;

        List<int> SelectedPeople;
        List<long> SelectedPeopleSNID;
        List<PersonLight> SelectedPeopleDetails;

        List<int> PostIDs;
        List<WallPostStructure> Posts;

        #endregion

        //Methods
        #region Methods

        void FillPersonDetailsFromSelectedPeople()
        {
            if (SelectedPeople != null)
            {
                int total = SelectedPeople.Count;

                SelectedPeopleDetails = new List<PersonLight>();
                SelectedPeopleSNID = new List<long>();

                for (int i = 0; i < total; i++)
                {
                    SelectedPeopleDetails.Add(new PersonLight(db, (int)SelectedPeople[i]));
                    SelectedPeopleSNID.Add(SelectedPeopleDetails[i].SNID);
                }
            }
        }


        void GetPostIDs()
        {


            PostIDs = MBBetaAPI.AgentAPI.DBLayer.GetPostsByPersonID(PostsDF.StartDateDP.SelectedDate.Value, PostsDF.EndDateDP.SelectedDate.Value, SelectedPeopleSNID);

            var UniquePostIDs = PostIDs.Distinct();
            Posts = new List<WallPostStructure>();

            foreach (int item in UniquePostIDs)
            {
                WallPostStructure tmp = new WallPostStructure(db, item);
                Posts.Add(tmp);
            }

            Posts.Sort(
                delegate(WallPostStructure p1, WallPostStructure p2)
                {
                    int timecompare = DateTime.Compare((System.DateTime)p2.ParentPost.Date, (System.DateTime)p1.ParentPost.Date);

                    return timecompare;
                }
                );

        }
        #endregion

        //Control Methods
        #region Control Methods


        private void CloseBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DateFilterBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            GetPostIDs();

            BrowsePostsCtrl.WallPostStructureListIC.ItemsSource = Posts;

            this.Cursor = Cursors.Arrow;
        }

        #endregion
    }
}
