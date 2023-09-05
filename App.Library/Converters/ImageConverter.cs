using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace App.Library.Converters
{
    // todo svg support?
    public class ImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is byte[] bytes)
            {
                return LoadImageFromBytes(bytes);
            }

            var valueAsString = value?.ToString();

            if (parameter != null
                && parameter is string)
            {
                valueAsString = parameter.ToString();
            }

            if (value != null
                && !string.IsNullOrEmpty(valueAsString))
            {
                var sourceAssembly = this.AppSpecific ? Assembly.GetEntryAssembly()!.FullName : Assembly.GetExecutingAssembly()!.FullName;
                var fileName = $@"pack://application:,,,/{sourceAssembly};component/{valueAsString}";
                return new BitmapImage(new Uri(fileName));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// App specific determines if an image is taken from the App assembly (e.g. geteduroam or getgovroam)
        ///  or from the App.Library assembly
        /// </summary>
        public bool AppSpecific { get; set; }

        private static BitmapImage? LoadImageFromBytes(byte[]? imageData)
        {
            if (imageData == null
                || imageData.Length == 0)
            {
                return null;
            }

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }
    }
}