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
    /// Interaction logic for SNMessages.xaml
    /// </summary>
    public partial class SNMessages : Window
    {
        public SNMessages()
        {
            InitializeComponent();
        }

        //Constructor comming from Main Window
        public SNMessages(List<int> SelectedPeopleParameter)
        {
            InitializeComponent();

            SelectedPeople = SelectedPeopleParameter;
            FillPersonLightFromSelectedPeople();

            DateFilterBt_Click(null, null);
        }

        /// <summary>
        /// Constructor coming from Search Window
        /// </summary>
        /// <param name="SearchResults"></param>
        public SNMessages(List<SNMessage> SearchResults)
        {
            InitializeComponent();

            //Hide filters
            MessagesDF.Visibility = System.Windows.Visibility.Hidden;
            DateFilterBt.Visibility = System.Windows.Visibility.Hidden;

            ParentMessages = SearchResults;

            SNMessagesList.MessagesListBox.ItemsSource = ParentMessages;

        }


        //******************* Attributes
        #region Attributes
        
        List<int> SelectedPeople;
        List<PersonLight> SelectedPeopleDetails;

        // TODO: Use the commented fields
        //Selected messages
        // List<SNMessageStructure> Messages;
        List<SNMessage> ParentMessages;

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
                    SelectedPeopleDetails.Add(new PersonLight(SelectedPeople[i]));
                }
            }
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

            List<int> ParentMessageIDs;

            ParentMessageIDs = MBBetaAPI.AgentAPI.DBLayer.GetSNParentMessageIDs(MessagesDF.StartDateDP.SelectedDate.Value, MessagesDF.EndDateDP.SelectedDate.Value.AddDays(1), SelectedPeopleDetails);

            ParentMessages = new List<SNMessage>();
            foreach (int ParentMessageID in ParentMessageIDs)
            {
                ParentMessages.Add(new SNMessage(ParentMessageID));
            }

            SNMessagesList.MessagesListBox.ItemsSource = ParentMessages;

            this.Cursor = Cursors.Arrow;
        }

        private void SNMessagesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            SNMessage ParentMessage = (SNMessage) SNMessagesList.MessagesListBox.SelectedItem;
            if (ParentMessage != null)
            {
                SNMessageStructure CurrentMessageStructure = new SNMessageStructure(ParentMessage.ID);

                this.Cursor = Cursors.Arrow;

                if (CurrentMessageStructure.ChildMessages != null)
                {
                    ConversationList.MessagesListBox.ItemsSource = CurrentMessageStructure.ChildMessages;
                    ConversationGrid.IsEnabled = true;
                    ConversationGrid.Visibility = System.Windows.Visibility.Visible;
                    CloseBt.Visibility = System.Windows.Visibility.Hidden;
                }
            }

            this.Cursor = Cursors.Arrow;

        }

        private void CloseConversationBt_Click(object sender, RoutedEventArgs e)
        {
            ConversationGrid.IsEnabled = false;
            ConversationGrid.Visibility = System.Windows.Visibility.Hidden;
            CloseBt.Visibility = System.Windows.Visibility.Visible;
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ConversationGrid.IsEnabled)
            {
                if (e.Key == Key.Escape)
                {
                    CloseConversationBt_Click(null, null);
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

        #endregion

       
    }
}
