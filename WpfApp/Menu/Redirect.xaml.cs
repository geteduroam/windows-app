using System;
using System.Collections.Generic;
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

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for Redirect.xaml
    /// </summary>
    public partial class Redirect : Page
    {
        private MainWindow mainWindow;
        public Redirect(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        // TODO: fix hyperlink and open redirect + closing application
        private void Hyperlink_redirect(object sender, RequestNavigateEventArgs e)
        {
            // tbTou.Visibility = Visibility.Collapsed;
        }
    }
}
