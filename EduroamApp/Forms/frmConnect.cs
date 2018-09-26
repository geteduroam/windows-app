using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp
{
    /// <summary>
    /// This
    /// </summary>
    public partial class frmConnect : Form
    {
        readonly frmParent frmParent;

        public frmConnect(frmParent parentInstance)
        {
            frmParent = parentInstance;
            InitializeComponent();
        }

        private void frmConnect_Load(object sender, EventArgs e)
        {
            if (frmParent.EduroamAvailable)
            {
                Connect();
            }
            else
            {
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

            if (connectSuccess)
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

        private void SaveAndQuit()
        {
            pnlEduNotAvail.Visible = true;
        }
        
        
    }
}
