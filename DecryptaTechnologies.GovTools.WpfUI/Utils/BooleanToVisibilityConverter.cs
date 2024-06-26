﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

public class BooleanToVisibilityConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = false;
        if (value is bool)
        {
            flag = (bool)value;
        }
        else if (value is bool?)
        {
            bool? nullable = (bool?)value;
            flag = nullable.HasValue ? nullable.Value : false;
        }
        return (flag ? Visibility.Visible : Visibility.Collapsed);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
