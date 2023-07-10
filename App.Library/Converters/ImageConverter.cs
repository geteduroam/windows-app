using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace App.Library.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueAsString = value?.ToString();

            if (parameter != null
                && parameter is string)
            {
                valueAsString = parameter.ToString();
            }

            if (value != null
                && !string.IsNullOrEmpty(valueAsString))
            {
                var fileName = $@"pack://application:,,,/App.Library;component/Images/{valueAsString}";
                return new BitmapImage(new Uri(fileName));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}