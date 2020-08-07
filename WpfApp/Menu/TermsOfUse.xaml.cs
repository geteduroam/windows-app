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
    /// Interaction logic for TermsOfUse.xaml
    /// </summary>
    public partial class TermsOfUse : Page
    {
        private MainWindow mainWindow;
        private readonly string tou;
        public TermsOfUse(MainWindow mainWindow, string tou)
        {
            this.mainWindow = mainWindow;
            this.tou = tou;
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            this.tbTou.Text = tou;
        }
    }
}
