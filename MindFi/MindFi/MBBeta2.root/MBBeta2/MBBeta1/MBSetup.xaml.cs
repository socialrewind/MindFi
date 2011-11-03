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
//Internal API
using MBBetaAPI;

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
            //TODO: Read accounts from DBlayer
        }


        #endregion

        private void FBAccountSetupBt_Click(object sender, RoutedEventArgs e)
        {
            var AccountSetupWindow = new MBAccountSetup();
            AccountSetupWindow.Show();
        }
    }
}
