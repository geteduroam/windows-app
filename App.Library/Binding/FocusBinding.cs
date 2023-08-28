using System.Windows;

namespace App.Library.Binding
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Source: https://stackoverflow.com/a/1356781 by Anvaka
    /// </remarks>
    public static class FocusBinding
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof(bool), typeof(FocusBinding),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
            }
        }
    }
}
