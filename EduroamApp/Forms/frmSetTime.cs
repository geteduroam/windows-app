using System;
using System.Windows.Forms;

namespace EduroamApp
{
	public partial class frmSetTime : Form
	{
		private readonly DateTime certDateTime;

		public frmSetTime(DateTime certDate)
		{
			certDateTime = certDate;
			InitializeComponent();
		}

		private void frmSetTime_Load(object sender, EventArgs e)
		{
			tmrCheckTime.Start();
			lblCertDate.Text = certDateTime.ToString();
		}

		private void tmrCheckTime_Tick(object sender, EventArgs e)
		{
			lblCurrentDate.Text = DateTime.Now.ToString();

			if (DateTime.Now > certDateTime)
			{
				DialogResult = DialogResult.OK;
			}
		}

		/// <summary>
		/// Makes the form window immovable.
		/// </summary>
		/// <param name="message"></param>
		protected override void WndProc(ref Message message)
		{
			const int WM_SYSCOMMAND = 0x0112;
			const int SC_MOVE = 0xF010;

			switch (message.Msg)
			{
				case WM_SYSCOMMAND:
					int command = message.WParam.ToInt32() & 0xfff0;
					if (command == SC_MOVE)
						return;
					break;
			}

			base.WndProc(ref message);
		}
	}



}
