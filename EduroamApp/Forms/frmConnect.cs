using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;

namespace EduroamApp
{
    /// <summary>
    /// Shows status for connection to eduroam.
    /// Lets user save configuration for later.
    /// </summary>
    public partial class frmConnect : Form
    {
        private readonly frmParent frmParent;

        public frmConnect(frmParent parentInstance)
        {
            frmParent = parentInstance;
            InitializeComponent();
        }

        private void frmConnect_Load(object sender, EventArgs e)
        {
            if (frmParent.EduroamAvailable)
            {
                // connect if eduroam is available
                Connect();
            }
            else
            {
                // prompt user to save config if not
                SaveAndQuit();
            }
        }

        private async void Connect()
        {
            // displays loading animation while attempt to connect
            lblStatus.Text = "Connecting...";
            pbxStatus.Image = Properties.Resources.loading_gif;
            lblStatus.Visible = true;
            pbxStatus.Visible = true;

            bool connectSuccess;
            // tries to connect
            try
            {
                connectSuccess = await Task.Run(ConnectToEduroam.WaitForConnect);
            }
            catch (Exception ex)
            {
                // if an exception is thrown, connection has not succeeded
                connectSuccess = false;
                MessageBox.Show("Could not connect. \nException: " + ex.Message);
            }

            // double check to validate wether eduroam really is an active connection
            var eduConnected = false;
            if (connectSuccess)
            {
                var checkConnected = NativeWifi.EnumerateConnectedNetworkSsids();
                foreach (NetworkIdentifier network in checkConnected)
                {
                    if (network.ToString() == "eduroam")
                    {
                        eduConnected = true;
                    }
                }
            }

            if (eduConnected)
            {
                lblStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
                pbxStatus.Image = Properties.Resources.green_checkmark;
                frmParent.BtnCancelText = "Close";
                frmParent.ProfileCondition = "GOODPROFILE";
            }
            else
            {
                lblStatus.Text = "Connection to eduroam failed.";
                pbxStatus.Image = Properties.Resources.red_x;
                lblConnectFailed.Visible = true;
                frmParent.BtnBackEnabled = true;
                frmParent.ProfileCondition = "BADPROFILE";
            }
        }

        // gives user choice of wether they want to save the configuration before quitting
        private void SaveAndQuit()
        {
            pnlEduNotAvail.Visible = true;
        }
        
        
    }
}
