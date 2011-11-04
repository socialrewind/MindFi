using System;
using System.Collections;
using System.Windows;
/*
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
 * */
//Internal API
using MBBetaAPI;
using MBBetaAPI.AgentAPI;

namespace MBBeta2
{
    /// <summary>
    /// Interaction logic for MBSetup.xaml
    /// </summary>
    public partial class MBSetup : Window
    {
        public MBSetup(DBConnector dbParam)
        {
            InitializeComponent();
            db = dbParam;
            GetSNAccounts(db);
        }

        //********Attributes
        DBConnector db;


        //******** Private MEthods
        #region PrivateMethods

        void GetSNAccounts(DBConnector db)
        {
            DBLayer.ConnString = db.ConnString;
            string error;
            ArrayList currentAccounts = DBLayer.GetAccounts(out error);
            if (error != "")
            {
                MessageBox.Show("Error getting accounts:\n" + error);
            }
            // TODO: Review how it can be empty and not null
            if (currentAccounts != null && currentAccounts.Count > 0)
            {
                if (currentAccounts != null && SNAccount.CurrentProfile == null)
                {
                    SNAccount first = (SNAccount)currentAccounts[0];
                    SNAccount.UpdateCurrent(first);
                }

                // TODO: create labels to show multiple existing accounts, not just one
                /*
                System.Windows.Forms.LinkLabel lblToAssign = null;
                switch (SNAccount.CurrentProfile.SocialNetwork)
                {
                    case SocialNetwork.FACEBOOK:
                        lblToAssign = linkLabelFB1;
                        break;
                    case SocialNetwork.TWITTER:
                        lblToAssign = linkLabelTw1;
                        break;
                    case SocialNetwork.LINKEDIN:
                        lblToAssign = linkLabelLI1;
                        break;
                }
                if (lblToAssign != null)
                {
                    lblToAssign.Text = currentProfile.userName;
                }
                 * */
                FBAccountSetupBt.Content = SNAccount.CurrentProfile.Name;
            }

            // btnOnline.Enabled = (SNAccount.CurrentProfile != null);
        }


        #endregion

        private void FBAccountSetupBt_Click(object sender, RoutedEventArgs e)
        {
            var AccountSetupWindow = new MBAccountSetup(SocialNetwork.FACEBOOK);
            AccountSetupWindow.Show();
        }
    }
}
