using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace MBBeta1
{
    [ValueConversion(typeof(int), typeof(DateTime))]
    public class MonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            DateTime month = new DateTime(1900, 1, 1);

            if (value != null )
            {
                int m = System.Convert.ToInt32(value);

                if (m != 0)

                    month = new DateTime(1900, m, 1);
            }

            return month;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}

