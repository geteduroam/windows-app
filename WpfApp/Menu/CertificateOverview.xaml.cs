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
using EduroamConfigure;
using WpfApp.Classes;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for InstallCertificates.xaml
    /// </summary>
    public partial class CertificateOverview : Page
    {
        private MainWindow mainWindow;
        private EapConfig eapConfig;
        public CertificateOverview(MainWindow mainWindow, EapConfig eapConfig)
        {
            this.mainWindow = mainWindow;
            this.eapConfig = eapConfig;
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            
            AddCertGrid("Cert1");
            AddSeparator();
            AddCertGrid("Cert2");
            AddSeparator();
            AddCertGrid("Cert3");
        }

        private void AddCertGrid(string text)
        {
            CertificateGrid grid = new CertificateGrid();
            grid.Text = text;
            grid.Margin = new Thickness(5, 5, 5, 5);
            AddToStack(grid);
        }

        private void AddSeparator()
        {
            Separator sep = new Separator();
            sep.Height = 1;
            sep.BorderThickness = new Thickness(1, 1, 1, 1);
            sep.BorderBrush = Brushes.LightGray;
            sep.Margin = new Thickness(5, 0, 5, 0);
            AddToStack(sep);
        }

        private void AddToStack(Control uc)
        {
            stpCerts.Children.Add(uc);
        }
    }
}
