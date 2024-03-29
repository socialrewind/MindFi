﻿using System;
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
    public partial class SNMessages : Window
    {
        public SNMessages()
        {
            InitializeComponent();
        }

        //Constructor comming from Main Window
        public SNMessages(List<int> SelectedPeopleParameter, DBConnector dbParam)
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

        //Selected messages
        List<SNMessageStructure> Messages;
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
                    SelectedPeopleDetails.Add(new PersonLight(db, SelectedPeople[i]));
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

            ParentMessageIDs = db.GetSNParentMessageIDs(MessagesDF.StartDateDP.SelectedDate.Value, MessagesDF.EndDateDP.SelectedDate.Value, SelectedPeopleDetails);

            ParentMessages = new List<SNMessage>();
            foreach (int ParentMessageID in ParentMessageIDs)
            {
                ParentMessages.Add(new SNMessage(db, ParentMessageID));
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
                SNMessageStructure CurrentMessageStructure = new SNMessageStructure(db, ParentMessage.ID);

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
