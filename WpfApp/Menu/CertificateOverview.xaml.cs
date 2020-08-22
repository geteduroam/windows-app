using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EduroamConfigure;
using WpfApp.Classes;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for InstallCertificates.xaml
    /// </summary>
    public partial class CertificateOverview : Page, IObserver<ConnectToEduroam.CertificateInstaller>
    {
        private IDisposable unsubscriber;
        private readonly MainWindow mainWindow;
        private readonly EapConfig eapConfig;
        private List<ConnectToEduroam.CertificateInstaller> installers;
        public CertificateOverview(MainWindow mainWindow, EapConfig eapConfig)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
            this.eapConfig = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
            InitializeComponent();
            Load();
        }
        /// <summary>
        /// Loads relevant elements of the page
        /// </summary>
        private void Load()
        {
            tbInfo.Text = "In order to continue you have to install the listed certificates";

            ConnectToEduroam.EnumerateCAInstallers(eapConfig)
                .Any(installer => installer.IsInstalledByUs || !installer.IsInstalled);

            installers = ConnectToEduroam.EnumerateCAInstallers(eapConfig).ToList();
            foreach (ConnectToEduroam.CertificateInstaller installer in installers )
            {
                AddSeparator();
                AddCertGrid(installer);
            }
            // remove the first separator
            if (stpCerts.Children.Count != 0)
                stpCerts.Children.RemoveAt(0);

            VerifyNextButton();
        }

        /// <summary>
        /// Adds a CertificateGrid controller to the stpCerts stackpanel
        /// </summary>
        /// <param name="installer"></param>
        private void AddCertGrid( ConnectToEduroam.CertificateInstaller installer)
        {
            CertificateGrid grid = new CertificateGrid
            {
                Margin = new Thickness(5, 5, 5, 5),
                Installer = installer,
            };
            // Subscribe to CertificateGrid so 'Install certificate' events gets noticed by this page.
            // TODO: Pass function to CertificateGrid that can notify the page instead of using 
            // subcription for obesrver/observable stuff.
            unsubscriber = grid.Subscribe(this);
            AddToStack(grid);
        }

        /// <summary>
        /// Adds a Seperator ( a straight, gray horizontal line) to the stpCerts stackpanel
        /// </summary>
        private void AddSeparator()
        {
            Separator sep = new Separator
            {
                Height = 1,
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = Brushes.LightGray,
                Margin = new Thickness(5, 0, 5, 0)
            };
            AddToStack(sep);
        }

        /// <summary>
        /// Decides if the Next button should be clickable
        /// </summary>
        private void VerifyNextButton()
        {
            // false if going back from login after failing to log in
            bool isInstalled = VerifyInstallers();
            mainWindow.btnNext.IsEnabled = isInstalled;
            if (isInstalled)
            {
                tbInfo.Text = "All Certificates are installed";
            }
        }

        /// <summary>
        /// checks if all certificates are installed
        /// </summary>
        /// <returns>true if all certs are installed. Else return is false</returns>
        private bool VerifyInstallers()
        {
            foreach (ConnectToEduroam.CertificateInstaller installer in installers)
            {
                if (!installer.IsInstalled) return false;
            }
            return true;
        }

        /// <summary>
        /// adds a Control object to the stpCerts stackpanel
        /// </summary>
        /// <param name="c"></param>
        private void AddToStack(Control c)
        {
            stpCerts.Children.Add(c);
        }

        /// <summary>
        /// Triggered by CertificateGrid objects that the page object subscribes to.
        /// Lets the page know a 'Install' button has been pressed.
        /// </summary>
        /// <param name="value"></param>
        public void OnNext(ConnectToEduroam.CertificateInstaller value)
        {
            VerifyNextButton();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
