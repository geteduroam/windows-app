using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace App.Library.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueAsString = value?.ToString();

            if (value != null
                && !string.IsNullOrEmpty(valueAsString))
            {
                var isRelativePath = string.IsNullOrEmpty(Path.GetPathRoot(valueAsString));

                if (isRelativePath)
                {
                    valueAsString = Path.Combine(Directory.GetCurrentDirectory(), valueAsString);
                }

                if (File.Exists(valueAsString))
                {
                    return new BitmapImage(new Uri(valueAsString));
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}