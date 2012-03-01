using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace MBBeta2
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class LikePluralConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            bool plural = false;

            if (value != null)
            {
                int n = System.Convert.ToInt32(value);

                if (n == 1)
                    plural = false;
                else
                    plural = true;

            }

            return plural;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
