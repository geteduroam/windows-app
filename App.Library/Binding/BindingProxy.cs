using App.Library.ViewModels;

using System.Windows;

namespace App.Library.Binding
{
    /// <summary>
    /// Class that allows to bind to data in cases when DataContext is not inherited
    /// </summary>
    /// <remarks>
    ///  Source: http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/ by Thomas Levesque
    /// </remarks>
    public class BindingProxy<T> : Freezable
    {
        /// <summary />
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("DataCtx", typeof(object), typeof(BindingProxy<T>),
            new UIPropertyMetadata(null));

        /// <summary >
        ///     DataContext
        /// </summary>
        public T DataCtx
        {
            get { return (T)this.GetValue(DataProperty); }
            set { this.SetValue(DataProperty, value); }
        }

        /// <summary />
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy<T>();
        }
    }

    public class MainViewModelProxy : BindingProxy<MainViewModel> { }
}
