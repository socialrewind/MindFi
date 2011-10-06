﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;

namespace MBBeta1.Converters
{
    public class TabSizeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter,            
            System.Globalization.CultureInfo culture)
        {            
            TabControl tabControl = values[0] as TabControl;            
            double width = tabControl.ActualWidth / tabControl.Items.Count;            
            //Subtract 1, otherwise we could overflow to two rows.            
            return (width <= 1) ? 0 : (width - 2.1);        
        }        
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,            
            System.Globalization.CultureInfo culture)        
        {            
            throw new NotSupportedException();       
        }

    }
}
