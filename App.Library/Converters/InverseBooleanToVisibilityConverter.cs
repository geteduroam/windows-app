using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace App.Library.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private readonly BooleanToVisibilityConverter converter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = this.converter.Convert(value, targetType, parameter, culture) as Visibility?;
            return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = this.converter.ConvertBack(value, targetType, parameter, culture) as bool?;
            return result != true;
        }
    }
}
