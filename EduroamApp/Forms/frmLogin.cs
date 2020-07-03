using System;
using System.Drawing;
using System.Windows.Forms;
using EduroamConfigure;
using System.Linq;
using System.Threading.Tasks;
using ManagedNativeWifi;

namespace EduroamApp
{
    public partial class frmLogin : Form
    {
        private readonly frmParent frmParent;
        private readonly EapConfig.AuthenticationMethod authMethod;
        private bool usernameDefault = true;
        private bool passwordDefault = true;
        private bool passwordSet;
        private bool usernameValid = false;
        private string realm;
        private bool hint;
        public bool connected;
        public bool ignorePassChange = false;

   

        public frmLogin(frmParent parentInstance)
        {
            frmParent = parentInstance;
            authMethod = frmParent.eapConfig.AuthenticationMethods.First();
            InitializeComponent();
        }

        // TODO: if pressing enter in username field and Connect not active, display rules

        private void frmLogin_Load(object sender, EventArgs e)
        {
            if (!frmParent.EduroamAvailable)
            {
                // prompt user to save config if not
                SaveAndQuit();
            }

            realm = authMethod.ClientInnerIdentitySuffix;
            hint = authMethod.ClientInnerIdentityHint;

            usernameValid = false;
            lblRules.Visible = true;
            frmParent.BtnNextEnabled = false;
            // shows helping (placeholder) text by default
            txtUsername.Text = "Username";
            txtUsername.ForeColor = SystemColors.GrayText;
            txtPassword.Text = "Password";
            txtPassword.ForeColor = SystemColors.GrayText;
            txtPassword.UseSystemPasswordChar = false;

            if (!string.IsNullOrEmpty(frmParent.InstId))
            {
                lblInst.Text = "@" + authMethod.ClientInnerIdentitySuffix;
            }
            else
            {
                lblInst.Text = "";
            }

            // if realm is provided and subrealms not allowed always show realm at end
            if (!string.IsNullOrEmpty(realm) && hint)
            {
                lblInst.Visible = true;
            }

        }

        // removes helping text when field is in focus
        private void txtUsername_Enter(object sender, EventArgs e)
        {
            if (txtUsername.Text == "Username" && usernameDefault)
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = SystemColors.ControlText;
                usernameDefault = false;
            }
        }

        // removes helping text when field is in focus
        private void txtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == "Password" && passwordDefault)
            {
                ignorePassChange = true;
                txtPassword.Text = "";
                txtPassword.ForeColor = SystemColors.ControlText;
                //txtPassword.UseSystemPasswordChar = true;
                txtPassword.PasswordChar = '*';
                passwordDefault = false;
            }
        }

        // shows helping text when field loses focus and is empty
        private void txtUsername_Leave(object sender, EventArgs e)
        {
            lblRules.Visible = true;
            if (txtUsername.Text == "")
            {
                usernameDefault = true;
                txtUsername.Text = "Username";
                txtUsername.ForeColor = SystemColors.GrayText;
                lblRules.Text = "";
            }
            else
            {
                usernameDefault = false;
            }

            if (!txtUsername.Text.Contains('@') && !string.IsNullOrEmpty(realm) && !usernameDefault)
            {
                lblInst.Visible = true;
            }

        }

        public bool ValidateFields()
        {
            string username = txtUsername.Text;
            if ((username == "Username" && usernameDefault) || username == "" )
            {
                usernameValid = false;
                lblRules.Text = "";
                frmParent.BtnNextEnabled = false;
                return false;
            }

            
            
            // if username does not contain '@' and realm is given then show realm added to end
            if ((!username.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
            {
                username += "@" + realm;
            }

            
            string brokenRules = IdentityProviderParser.GetBrokenRules(username, realm, hint);
            usernameValid = string.IsNullOrEmpty(brokenRules);
            lblRules.Text = "";
            if (!usernameValid && !usernameDefault)
            {
                lblRules.Text = brokenRules;
            }
            else
            {
                lblRules.Text = "";
            }

            frmParent.BtnNextEnabled = passwordSet || connected;
            return (passwordSet && usernameValid) || connected;
        }

        public void ConnectClick()
        {
            if (ValidateFields())
            {
                if ((!txtUsername.Text.Contains('@') && !string.IsNullOrEmpty(realm)) || hint)
                {
                    lblInst.Visible = true;
                }
                ConnectWithLogin();
                return;
            }
            lblRules.Visible = true;
        }

  
        // shows helping text when field loses focus and is empty
        private void txtPassword_Leave(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                passwordDefault = true;
                txtPassword.ForeColor = SystemColors.GrayText;
                txtPassword.PasswordChar = '\0';
                ignorePassChange = true;
                txtPassword.Text = "Password";
            }
            else
            {
                passwordDefault = false;
            }
        }

        public void ConnectWithLogin()
        {
            string username = txtUsername.Text;
            if (lblInst.Visible)
            {
                username += lblInst.Text;
            }
            string password = txtPassword.Text;

            frmParent.BtnNextEnabled = false;
            // displays loading animation while attempt to connect
            lblStatus.Text = "Connecting...";
            pbxStatus.Image = Properties.Resources.loading_gif;
            lblStatus.Visible = true;
            pbxStatus.Visible = true;
            txtPassword.ReadOnly = true;
            txtUsername.ReadOnly = true;

            ConnectToEduroam.InstallUserProfile(username, password, frmParent.AuthMethod);
            Connect();
           

        }
        
        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            lblStatus.Visible = false;
            pbxStatus.Visible = false;
            lblRules.Visible = false;
            if (!hint) lblInst.Visible = false;
            ValidateFields();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            pbxStatus.Visible = false;
            lblStatus.Visible = false;
            if (ignorePassChange == true)
            {
                ignorePassChange = false;
                return;
            }
            passwordSet = !string.IsNullOrEmpty(txtPassword.Text) && !passwordDefault && txtPassword.ContainsFocus;
            ValidateFields();
        }



        private async void Connect()
        {

            bool eduConnected = await Task.Run(frmParent.Connect);

            if (eduConnected)
            {
                lblStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
                pbxStatus.Image = Properties.Resources.green_checkmark;
                frmParent.BtnNextText = "Close";
                frmParent.BtnBackVisible = false;
                frmParent.ProfileCondition = frmParent.ProfileStatus.Working;
            }
            else
            {
                lblStatus.Text = "Connection to eduroam failed.";
                pbxStatus.Image = Properties.Resources.red_x;
                lblConnectFailed.Visible = true;
                frmParent.BtnBackEnabled = true;
                frmParent.ProfileCondition = frmParent.ProfileStatus.Incomplete;
            }
            txtPassword.ReadOnly = false;
            txtUsername.ReadOnly = false;
            connected = eduConnected;
            frmParent.BtnNextEnabled = true;
        }

        // gives user choice of wether they want to save the configuration before quitting
        private void SaveAndQuit()
        {
            frmParent.ProfileCondition = frmParent.ProfileStatus.Working; // what?
            pnlEduNotAvail.Visible = true;
        }

    }
}
