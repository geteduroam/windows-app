using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace App.Library.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.Equals("inverse", (parameter ?? "").ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }

            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}