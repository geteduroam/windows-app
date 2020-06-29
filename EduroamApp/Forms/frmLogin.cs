using System;
using System.Drawing;
using System.Windows.Forms;
using EduroamConfigure;
using System.Linq;
using System.Diagnostics;

namespace EduroamApp
{
    public partial class frmLogin : Form
    {
        private readonly frmParent frmParent;
        private readonly EapConfig.AuthenticationMethod authMethod;
        private bool usernameFieldLeave;
        private bool usernameDefault = true;
        private bool passwordDefault = true;
        private bool usernameSet;
        private bool passwordSet;
        private bool usernameValid = false;

        public frmLogin(frmParent parentInstance)
        {
            frmParent = parentInstance;
            authMethod = frmParent.eapConfig.AuthenticationMethods.First();
            InitializeComponent();
        }

        private void frm6_Load(object sender, EventArgs e)
        {
            // shows helping (placeholder) text by default
            txtUsername.Text = "Username";
            txtUsername.ForeColor = SystemColors.GrayText;
            txtPassword.Text = "Password";
            txtPassword.ForeColor = SystemColors.GrayText;
            txtPassword.UseSystemPasswordChar = false;

            if (!string.IsNullOrEmpty(frmParent.InstId))
            {
                // lblInst.Text = "@" + frmParent.InstId;
                lblInst.Text = "@" + authMethod.ClientInnerIdentitySuffix;
            }
            else
            {
                lblInst.Text = "";
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
                txtPassword.Text = "";
                txtPassword.ForeColor = SystemColors.ControlText;
                txtPassword.UseSystemPasswordChar = true;
                passwordDefault = false;
            }
        }

        // shows helping text when field loses focus and is empty
        private void txtUsername_Leave(object sender, EventArgs e)
        {
            if (txtUsername.Text == "")
            {
                txtUsername.Text = "Username";
                txtUsername.ForeColor = SystemColors.GrayText;
                usernameDefault = true;
            }
            else
            {
                usernameDefault = false;
            }
            
            string username = txtUsername.Text;
            string realm = authMethod.ClientInnerIdentitySuffix;
            bool hint = authMethod.ClientInnerIdentityHint;

            // use realm as suffix
            if (!username.Contains('@'))
            {
                username += "@" + realm;
                lblInst.Visible = true;
                usernameFieldLeave = true;
            }
            string brokenRules = IdentityProviderParser.GetBrokenRules(username, realm, hint);
            bool valid = string.IsNullOrEmpty(brokenRules);
            lblRules.Text = "";
            if (!valid)
            { 
                lblRules.Text = "Error:\n" + brokenRules;
            }
            usernameValid = valid;
            ValidateFields();
            
        }

        // shows helping text when field loses focus and is empty
        private void txtPassword_Leave(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                txtPassword.Text = "Password";
                txtPassword.ForeColor = SystemColors.GrayText;
                txtPassword.UseSystemPasswordChar = false;
                passwordDefault = true;
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

            ConnectToEduroam.SetupLogin(username, password, frmParent.AuthMethod);
        }
        
        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            usernameSet = !string.IsNullOrEmpty(txtUsername.Text) && !usernameDefault && txtUsername.ContainsFocus;
            // set to false in case user changes a previously validated username
            usernameValid = false;
            ValidateFields();
            if (usernameFieldLeave) lblInst.Visible = !txtUsername.Text.Contains("@");
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            passwordSet = !string.IsNullOrEmpty(txtPassword.Text) && !passwordDefault && txtPassword.ContainsFocus;
            ValidateFields();
        }

        private void ValidateFields()
        {
            frmParent.BtnNextEnabled = (usernameSet && passwordSet && usernameValid);
        }
    }
}
