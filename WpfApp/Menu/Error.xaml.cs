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
    /// Interaction logic for Error.xaml
    /// </summary>
    public partial class Error : Page
    {
        private MainWindow mainWindow;
        private string errorMessage;

        public Error(MainWindow mainWindow, string errorMessage)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            this.errorMessage = errorMessage;
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            tbError.Text = errorMessage;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.PreviousPage();
        }
    }
}
