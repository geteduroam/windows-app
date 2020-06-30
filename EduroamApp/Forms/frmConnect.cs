using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using EduroamConfigure;

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

            if (frmParent.AuthMethod.EapType == EapType.TLS)
            {
                DateTime validFrom = ConnectToEduroam.CertValidFrom; // TODO: this static variable will be moved
                DateTime now = DateTime.Now;
                TimeSpan difference = validFrom - now;

                // if certificate valid from time has passed, do nothing
                if (DateTime.Compare(validFrom, now) > 0)
                {
                    // waits at connecting screen if under 9 seconds difference
                    if (difference.TotalSeconds < 8) // TODO: muyto intressante
                    {
                        await PutTaskDelay(difference.Milliseconds + 1000);
                    }
                    // displays dialog that lets user know how long they must wait, or to change their clock manually
                    else
                    {
                        // opens form as dialog
                        using frmSetTime setTimeDialog = new frmSetTime(validFrom);
                        var dialogResult = setTimeDialog.ShowDialog();
                        // cancels connection if time not set and dialog cancelled
                        if (dialogResult == DialogResult.Cancel)
                        {
                            lblStatus.Text = "Couldn't connect to eduroam.";
                            pbxStatus.Image = Properties.Resources.red_x;
                            lblConnectFailed.Text =
                                "Please ensure that the date time and time zone on your computer are set correctly.\n\n" +
                                lblConnectFailed.Text;
                            lblConnectFailed.Visible = true;
                            frmParent.BtnBackEnabled = true;
                            frmParent.ProfileCondition = "BADPROFILE";
                            return;
                        }
                    }
                }
            }

            bool connectSuccess;
            // tries to connect
            try
            {
                connectSuccess = await Task.Run(ConnectToEduroam.TryToConnect);
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
                frmParent.BtnNextText = "Close";
                frmParent.BtnNextEnabled = true;
                frmParent.BtnCancelEnabled = false;
                frmParent.BtnBackVisible = false;
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

        async Task PutTaskDelay(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        // gives user choice of wether they want to save the configuration before quitting
        private void SaveAndQuit()
        {
            frmParent.ProfileCondition = "GOODPROFILE";
            pnlEduNotAvail.Visible = true;
        }
        
        
    }
}
