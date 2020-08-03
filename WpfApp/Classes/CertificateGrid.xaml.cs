using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp.Classes
{
    /// <summary>
    /// Interaction logic for CertificateGrid.xaml
    /// </summary>
    public partial class CertificateGrid : UserControl
    {
        public CertificateGrid()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CertificateGrid), new UIPropertyMetadata(null));

        public bool IsInstalled
        {

            get { return (bool)GetValue(IsInstalledProperty); }
            set { this.SetValue(IsInstalledProperty, value); }
        }

        public static readonly DependencyProperty IsInstalledProperty =
            DependencyProperty.Register("IsInstalled", typeof(bool), typeof(CertificateGrid), new UIPropertyMetadata(null));

        
        public RoutedEventHandler Click
        {
            
            get { return (RoutedEventHandler)GetValue(ClickProperty); }
            set { this.SetValue(ClickProperty, value); }
        }

        public static readonly DependencyProperty ClickProperty =
            DependencyProperty.Register("Click", typeof(RoutedEventHandler), typeof(CertificateGrid), new UIPropertyMetadata(null));

    }

}
