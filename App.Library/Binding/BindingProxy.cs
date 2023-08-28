using App.Library.ViewModels;

using System.Windows;

namespace App.Library.Binding
{
    /// <summary>
    /// Class that allows to bind to data in cases when DataContext is not inherited
    /// </summary>
    /// <remarks>
    /// See http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
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

        #region Overrides of Freezable

        /// <summary />
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy<T>();
        }

        #endregion
    }

    public class MainViewModelProxy : BindingProxy<MainViewModel> { }
}
