using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace App.Library.Images
{
    public static class ImageFunctions
    {
        /// <summary>
        /// Converts base64 string to image.
        /// </summary>
        /// <param name="imageBytes">Image bytes.</param>
        /// <returns>Image.</returns>
        public static Image BytesToImage(byte[] imageBytes)
        {
            // converts byte[] to Image
            using var stream = new MemoryStream(imageBytes);
            var image = Image.FromStream(stream, true);
            return image;
        }

        public static BitmapImage? LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            var image = new BitmapImage();
            using var mem = new MemoryStream(imageData);
            mem.Position = 0;

            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = mem;
            image.EndInit();

            image.Freeze();

            return image;
        }

        /// <summary>
        /// Generates HTML code for displaying logos in SVG format.
        /// </summary>
        /// <param name="logo">SVG logo.</param>
        /// <returns>Html code.</returns>
        public static string GenerateSvgLogoHtml(byte[] logo)
        {
            var base64 = System.Convert.ToBase64String(logo);
            return
                "<!DOCTYPE html>" +
                "<html oncontextmenu=\"return false;\" ondragstart=\"return false;\">" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
                "<style>" +
                    "html,body {" +
                        "margin: 0;" +
                        "padding: 0;" +
                        "overflow: hidden;" +
                        "display: flex;" +
                        "justify-content: center;" +
                    "}" +
                    "img {" +
                        "max-width: 100%" +
                        "max-height: 100%" +
                        "width: auto;" +
                        "height: auto;" +
                    "}" +
                "</style>" +
                "<img src=\'data:image/svg+xml;base64," + base64 + "\'>";
        }
    }
}
