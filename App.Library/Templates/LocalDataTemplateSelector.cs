using System.Windows;
using System.Windows.Controls;

namespace App.Library.Templates
{
    public class LocalDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return base.SelectTemplate(item, container);
        }
    }
}