using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace App.Library.Converters
{
    // todo svg support?
    public class ImageConverter : IValueConverter
    {
        private static BitmapImage LoadImageFromBytes(byte[]? imageData)
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

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
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
                var fileName = $@"pack://application:,,,/App.Library;component/Images/{valueAsString}";
                return new BitmapImage(new Uri(fileName));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        //public static string GenerateSvgLogoHtml(byte[] logo)
        //{
        //    string base64 = System.Convert.ToBase64String(logo);
        //    return
        //        "<!DOCTYPE html>" +
        //        "<html oncontextmenu=\"return false;\" ondragstart=\"return false;\">" +
        //        "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
        //        "<style>" +
        //        "html,body {" +
        //        "margin: 0;" +
        //        "padding: 0;" +
        //        "overflow: hidden;" +
        //        "display: flex;" +
        //        "justify-content: center;" +
        //        "}" +
        //        "img {" +
        //        "max-width: 100%" +
        //        "max-height: 100%" +
        //        "width: auto;" +
        //        "height: auto;" +
        //        "}" +
        //        "</style>" +
        //        "<img src=\'data:image/svg+xml;base64," + base64 + "\'>";
        //}
    }
}