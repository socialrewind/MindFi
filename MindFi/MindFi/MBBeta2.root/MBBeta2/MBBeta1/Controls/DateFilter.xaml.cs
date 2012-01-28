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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MBBeta2.Controls
{
    /// <summary>
    /// Interaction logic for DateFilter.xaml
    /// </summary>
    public partial class DateFilter : UserControl
    {
        
        public DateFilter()
        {
            InitializeComponent();

            StartDateDP.DisplayDateEnd = (System.DateTime.Today);
            StartDateDP.SelectedDate = (System.DateTime.Today.AddDays(-30));

            EndDateDP.DisplayDateStart = StartDateDP.SelectedDate;
            // TODO: Check how much we can have in the future
            EndDateDP.DisplayDateEnd = (System.DateTime.Today.AddDays(60));
            EndDateDP.SelectedDate = (System.DateTime.Today);
        }

        private void StartDateDP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (StartDateDP.SelectedDate > EndDateDP.SelectedDate)
                StartDateDP.SelectedDate = EndDateDP.SelectedDate;

            EndDateDP.DisplayDateStart = StartDateDP.SelectedDate;

        }

        private void EndDateDP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (EndDateDP.SelectedDate < StartDateDP.SelectedDate)
                EndDateDP.SelectedDate = StartDateDP.SelectedDate;

            StartDateDP.DisplayDateEnd = EndDateDP.SelectedDate;
        }


    }
}
