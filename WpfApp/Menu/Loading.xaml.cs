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
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Page
    {
        private MainWindow mainWindow;

        public Loading(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            InitializeComponent();           
            
            
        }
    }
}
