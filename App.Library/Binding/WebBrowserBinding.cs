using System.Windows;
using System.Windows.Controls;

namespace App.Library.Binding
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Source: https://stackoverflow.com/a/265648 by Todd White
    /// </remarks>
    public class WebBrowserBinding
    {
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserBinding), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is WebBrowser browser)
            {
                var uri = e.NewValue as string;

                if (!string.IsNullOrWhiteSpace(uri))
                {
                    browser.NavigateToString(uri);
                }
            }
        }

    }
}
