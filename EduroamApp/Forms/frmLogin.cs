using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace EduroamApp
{
    public partial class frmLogin : Form
    {
        private readonly frmParent frmParent;
        private bool usernameFieldLeave;
        private bool usernameDefault = true;
        private bool passwordDefault = true;
        private bool usernameSet;
        private bool passwordSet;

        public frmLogin(frmParent parentInstance)
        {
            frmParent = parentInstance;
            InitializeComponent();
        }

        private void frm6_Load(object sender, EventArgs e)
        {
            // shows helping text by default
            txtUsername.Text = "Username";
            txtUsername.ForeColor = SystemColors.GrayText;
            txtPassword.Text = "Password";
            txtPassword.ForeColor = SystemColors.GrayText;
            txtPassword.UseSystemPasswordChar = false;

            if (!string.IsNullOrEmpty(frmParent.LblInstText))
            {
                lblInst.Text = "@" + frmParent.LblInstText;
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

            // display instution id as username suffix
            if (!txtUsername.Text.Contains("@"))
            {
                lblInst.Visible = true;
                usernameFieldLeave = true;
            }
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

            ConnectToEduroam.SetupLogin(username, password);
        }
        
        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            usernameSet = !string.IsNullOrEmpty(txtUsername.Text) && !usernameDefault && txtUsername.ContainsFocus;
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
            frmParent.BtnNextEnabled = (usernameSet && passwordSet);
        }
    }
}
