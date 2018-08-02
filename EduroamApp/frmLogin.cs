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
		public frmLogin()
		{
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
		}

		// removes helping text when field is in focus
		private void txtUsername_Enter(object sender, EventArgs e)
		{
			if (txtUsername.Text == "Username")
			{
				txtUsername.Text = "";
				txtUsername.ForeColor = SystemColors.ControlText;
			}
		}

		// removes helping text when field is in focus
		private void txtPassword_Enter(object sender, EventArgs e)
		{
			if (txtPassword.Text == "Password")
			{
				txtPassword.Text = "";
				txtPassword.ForeColor = SystemColors.ControlText;
				txtPassword.UseSystemPasswordChar = true;
			}
		}

		// shows helping text when field loses focus and is empty
		private void txtUsername_Leave(object sender, EventArgs e)
		{
			if (txtUsername.Text == "")
			{
				txtUsername.Text = "Username";
				txtUsername.ForeColor = SystemColors.GrayText;
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
			}
		}

		public bool ConnectWithLogin()
		{
			ConnectToEduroam.SetupLogin(txtUsername.Text, txtPassword.Text);
			return true;
		}
	}
}
