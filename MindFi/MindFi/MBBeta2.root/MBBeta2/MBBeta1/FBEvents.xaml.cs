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
    /// Interaction logic for FBEvents.xaml
    /// </summary>
    public partial class FBEvents : Window
    {
        //Constructors
        #region Constructors

        public FBEvents()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor coming from Main Window
        /// </summary>
        public FBEvents(List<int> SelectedPeopleParam, DBConnector dbParam)
        {
            InitializeComponent();
            db = dbParam;
            SelectedPeople = SelectedPeopleParam;

            FillPersonDetailsFromSelectedPeople();

            //fill with predefined period
            DateFilterBt_Click(null, null);

        }

        /// <summary>
        /// Constructor coming from SearchResults
        /// </summary>
        /// <param name="SearchResults"></param>
        public FBEvents(List<SNEvent> SearchResults)
        {
            InitializeComponent();

            //Hide filter controls
            EventsDF.Visibility = System.Windows.Visibility.Hidden;
            DateFilterBt.Visibility = System.Windows.Visibility.Hidden;

            EventsListCtrl.EventsList.ItemsSource = SearchResults;

        }

        #endregion

        //********** Attributes
        #region Attributes
        DBConnector db;
        List<int> SelectedPeople;
        List<PersonLight> SelectedPeopleDetails;
        List<long> SelectedPeopleSNID;

        //Events
        List<int> EventIDs;
        List<SNEvent> Events;

        #endregion

        //********** Methods
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

        void GetEvents()
        {
            EventIDs = MBBetaAPI.AgentAPI.DBLayer.GetEventIDsByPersonIDs(EventsDF.StartDateDP.SelectedDate.Value, EventsDF.EndDateDP.SelectedDate.Value, SelectedPeopleDetails);

            var UniqueEventIDs = EventIDs.Distinct();
            Events = new List<SNEvent>();

            foreach (int item in UniqueEventIDs)
            {
                SNEvent tmp = new SNEvent(db, item);
                Events.Add(tmp);
            }

            Events.Sort(
                delegate(SNEvent e1, SNEvent e2)
                {
                    int timecompare = DateTime.Compare((System.DateTime)e2.StartTime, (System.DateTime)e1.StartTime);
                    return timecompare;
                }
                );
        }

        #endregion


        //********** Control Methods
        #region ControlMethods
        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (EventDetailGrid.IsEnabled)
            {
                if (e.Key == Key.Escape)
                {
                    CloseEventDetailsBt_Click(null, null);
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.Escape)
            {
                CloseBt_Click(null, null);
                e.Handled = true;
            }
        }

        private void CloseBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DateFilterBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            GetEvents();

            EventsListCtrl.EventsList.ItemsSource = Events;

            this.Cursor = Cursors.Arrow;
        }

        private void CloseEventDetailsBt_Click(object sender, RoutedEventArgs e)
        {
            EventDetailGrid.Visibility = System.Windows.Visibility.Hidden;
            EventDetailGrid.IsEnabled = true;
            CloseBt.Visibility = System.Windows.Visibility.Visible;
        }

        private void EventsListCtrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            SNEvent CurrentEvent = (SNEvent)EventsListCtrl.EventsList.SelectedItem;

            if (CurrentEvent != null)
            {
                EventDetailGrid.Visibility = System.Windows.Visibility.Visible;
                EventDetailGrid.IsEnabled = true;
                CloseBt.Visibility = System.Windows.Visibility.Hidden;

                EventDetailsCtrl.DataContext = CurrentEvent;
            }

        }

        #endregion

        
    }
}
