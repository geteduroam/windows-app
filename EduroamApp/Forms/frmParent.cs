using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using System.Net;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Xml.Linq;
using System.Device.Location;
using System.Windows.Controls;
using Image = System.Drawing.Image;

namespace EduroamApp
{
	public partial class frmParent : Form
	{
		private int currentFormId;                                  // Id of currently selected form
		public readonly List<int> FormHistory = new List<int>();   // keeps history of previously diplayed forms, in order to backtrack correctly
		private bool reload = true;                                 // sepcifies wether a form is to be re-instantiated when loaded
		private readonly GeoCoordinateWatcher watcher;              // gets coordinates of computer
		private EapConfig eapConfig = new EapConfig();
		private uint eapType;                                   // EAP type of selected network config, determines which forms to load
		public bool ComesFromSelfExtract;
		public bool SelfExtractFlag;
		public bool SelectAlternative;

		// makes forms globally  accessible in parent form
		private frmSummary frmSummary;
		private frmSelectMethod frmSelectMethod;
		private frmDownload frmDownload;
		private frmLocal frmLocal;
		private frmConnect frmConnect;
		private frmLogin frmLogin;
		private frmRedirect frmRedirect;

		public frmParent()
		{
			// starts GeoCoordinateWatcher when app starts
			watcher = new GeoCoordinateWatcher();
			watcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
			FormClosed += frmParent_FormClosed;
			InitializeComponent();
		}

		private void frmParent_Load(object sender, EventArgs e)
		{
			eapConfig = GetSelfExtractingEap();
			// checks if file came with self extract
			if (eapConfig != null)
			{
				ComesFromSelfExtract = true;
				SelfExtractFlag = true;
				// goes to form for installation through self extract config file
				LoadFrmSummary();
			}
			else
			{
				// goes to form for selecting install method
				LoadFrmSelectMethod();
			}
		}

		public void btnNext_Click(object sender, EventArgs e)
		{
			// creates new instances of forms when going forward
			reload = true;
			// adds current form to history for easy backtracking
			FormHistory.Add(currentFormId);

			switch (currentFormId)
			{
				case 1:
					if (SelectAlternative)
					{
						pbxLogo.Image = null;
						LoadFrmSelectMethod();
						break;
					}
					eapType = frmSummary.InstallEapConfig();
					if (eapType == 13) LoadFrmConnect();
					else if (eapType == 25 || eapType == 21) LoadFrmLogin();
					else if (eapType == 500)
					{
						lblLocalFileType.Text = "CERT";
						LoadFrmLocalCert();
					}
					else if (eapType != 0) MessageBox.Show("Couldn't connect to eduroam. \nYour institution does not have a valid configuration.",
						"Configuration not valid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 2:
					SelfExtractFlag = false;
					if (frmSelectMethod.GoToForm() == 3) LoadFrmDownload();
					else
					{
						lblLocalFileType.Text = "EAPCONFIG";
						LoadFrmLocal();
					}
					break;
				case 3:
					eapConfig = frmDownload.ConnectWithDownload();
					if (eapConfig != null)
					{
						LoadFrmSummary();
					} else if (!string.IsNullOrEmpty(lblRedirect.Text))
					{
						LoadFrmRedirect();
					}
					break;
				case 4:
					eapConfig = frmLocal.ConnectWithFile();
					if (eapConfig != null) LoadFrmSummary();
					break;
				case 5:
					if (eapType != 21)
					{
						frmLogin.ConnectWithLogin(eapType);
						LoadFrmConnect();
					}
					else MessageBox.Show("Support for TTLS configuration not ready yet.", "TTLS not ready", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 6:
					break;
				case 8:
					if (frmLocal.InstallCertFile()) LoadFrmConnect();
					break;
			}

			// removes current form from history if it gets added twice
			if (FormHistory.LastOrDefault() == currentFormId) FormHistory.RemoveAt(FormHistory.Count - 1);
		}

		private void btnBack_Click(object sender, EventArgs e)
		{
			// reuses existing instances of forms when going backwards
			reload = false;
			// clears logo if going back from summary page
			if (currentFormId == 1) pbxLogo.Image = null;

			switch (FormHistory.Last())
			{
				case 1:
					if (SelfExtractFlag)
					{
						eapConfig = GetSelfExtractingEap();
					}
					LoadFrmSummary();
					break;
				case 2:
					if (ComesFromSelfExtract) SelfExtractFlag = true;
					LoadFrmSelectMethod();
					break;
				case 3:
					LoadFrmDownload();
					break;
				case 4:
					LoadFrmLocal();
					break;
				case 5:
					if (lblProfileCondition.Text == "BADPROFILE") ConnectToEduroam.CreateNewProfile();
					LoadFrmLogin();
					break;
				case 7:
					break;
				case 8:
					LoadFrmLocalCert();
					break;
			}

			// removes current form from history
			FormHistory.RemoveAt(FormHistory.Count - 1);
		}

		/// <summary>
		/// Loads new form and shows it in content panel on parent form.
		/// </summary>
		/// <param name="nextForm">Instance of form to load.</param>
		private void LoadNewForm(Form nextForm)
		{
			nextForm.TopLevel = false;
			nextForm.AutoScroll = true;
			nextForm.Dock = DockStyle.Fill;
			pnlContent.Controls.Clear();
			pnlContent.Controls.Add(nextForm);
			nextForm.Show();
		}

		/// <summary>
		/// Checks if an EAP-config file exists in the same folder as the executable
		/// </summary>
		/// <returns>EapConfig object if file exists, null if not.</returns>
		public EapConfig GetSelfExtractingEap()
		{
			string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string[] files = Directory.GetFiles(exeLocation, "*.eap-config");

			if (files.Length <= 0) return null;
			try
			{
				string eapPath = files.First();
				string eapString = File.ReadAllText(eapPath);
				eapConfig = ConnectToEduroam.GetEapConfig(eapString);
				return eapConfig;
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public GeoCoordinateWatcher GetWatcher()
		{
			return watcher;
		}

		// make form properties accessible from other forms

		public Image PbxLogo
		{
			get => pbxLogo.Image;
			set => pbxLogo.Image = value;
		}

		public string BtnNextText
		{
			get => btnNext.Text;
			set => btnNext.Text = value;
		}

		public bool BtnNextEnabled
		{
			get => btnNext.Enabled;
			set => btnNext.Enabled = value;
		}

		public bool BtnBackEnabled
		{
			get => btnBack.Enabled;
			set => btnBack.Enabled = value;
		}

		public string BtnCancelText
		{
			get => btnCancel.Text;
			set => btnCancel.Text = value;
		}

		public string LblSummary
		{
			get => lblSummary.Text;
			set => lblSummary.Text = value;
		}

		public string LblInstText
		{
			get => lblInst.Text;
			set => lblInst.Text = value;
		}

		public string LblProfileCondition
		{
			get => lblProfileCondition.Text;
			set => lblProfileCondition.Text = value;
		}

		public string LblLocalFileType
		{
			get => lblLocalFileType.Text;
			set => lblLocalFileType.Text = value;
		}

		public string LblRedirect
		{
			get => lblRedirect.Text;
			set => lblRedirect.Text = value;
		}

		/// <summary>
		/// Loads form that shows summary of selected EAP configuration.
		/// </summary>
		public void LoadFrmSummary()
		{
			frmSummary = new frmSummary(this, eapConfig);
			currentFormId = 1;
			// changes controls depending on where the summary form is called from
			if (SelfExtractFlag)
			{
				lblTitle.Text = "eduroam Setup";
				btnBack.Visible = false;
			}
			else
			{
				lblTitle.Text = "Summary";
			}
			btnNext.Text = "Next >";
			LoadNewForm(frmSummary);
		}

		/// <summary>
		/// Loads form that lets user choose how they want to get config file.
		/// </summary>
		public void LoadFrmSelectMethod()
		{
			frmSelectMethod = new frmSelectMethod(this);
			currentFormId = 2;
			lblTitle.Text = "Certificate installation";
			btnNext.Text = "Next >";
			btnNext.Enabled = true;
			// if config file exists in self extract but user wants to choose another institution
			btnBack.Visible = ComesFromSelfExtract;
			LoadNewForm(frmSelectMethod);
		}

		/// <summary>
		/// Loads form that lets user select institution and download config file.
		/// </summary>
		public void LoadFrmDownload()
		{
			frmDownload = new frmDownload(this);
			currentFormId = 3;
			lblTitle.Text = "Select your institution";
			btnNext.Text = "Next >";
			btnNext.Enabled = false;
			btnBack.Enabled = true;
			btnBack.Visible = true;
			LoadNewForm(frmDownload);
		}

		/// <summary>
		/// Loads form that lets user select config file from computer.
		/// </summary>
		public void LoadFrmLocal()
		{
			if (reload) frmLocal = new frmLocal(this);
			currentFormId = 4;
			lblTitle.Text = "Select EAP-config file";
			btnNext.Text = "Next >";
			btnNext.Enabled = true;
			btnBack.Enabled = true;
			btnBack.Visible = true;
			LoadNewForm(frmLocal);
		}

		/// <summary>
		/// Loads form that lets user log in with username+password.
		/// </summary>
		public void LoadFrmLogin()
		{
			frmLogin = new frmLogin(this);
			currentFormId = 5;
			lblTitle.Text = "Log in";
			btnNext.Text = "Connect";
			btnNext.Enabled = false;
			btnBack.Enabled = true;
			btnBack.Visible = true;
			LoadNewForm(frmLogin);
		}

		/// <summary>
		/// Loads form that shows connection status.
		/// </summary>
		public void LoadFrmConnect()
		{
			frmConnect = new frmConnect(this);
			currentFormId = 6;
			lblTitle.Text = "Connection status";
			btnNext.Text = "Next >";
			btnNext.Enabled = false;
			btnBack.Enabled = false;
			btnBack.Visible = true;
			LoadNewForm(frmConnect);
		}

		/// <summary>
		/// Loads form that shows redirect link.
		/// </summary>
		public void LoadFrmRedirect()
		{
			frmRedirect = new frmRedirect(this);
			currentFormId = 7;
			lblTitle.Text = "You are being redirected";
			btnNext.Enabled = false;
			btnBack.Enabled = true;
			btnBack.Visible = true;
			LoadNewForm(frmRedirect);
		}

		/// <summary>
		/// Loads form that lets you select a local client certificate file.
		/// </summary>
		public void LoadFrmLocalCert()
		{
			if (reload) frmLocal = new frmLocal(this);
			currentFormId = 8;
			lblTitle.Text = "Select client certificate file";
			btnNext.Text = "Next >";
			btnNext.Enabled = true;
			btnBack.Enabled = true;
			btnBack.Visible = true;
			LoadNewForm(frmLocal);
		}

		// adds lines to panels on parent form
		private void pnlNavTop_Paint(object sender, PaintEventArgs e)
		{
			Pen grayPen = new Pen(Color.LightGray);
			int width = pnlNavTop.Width;

			Point point1 = new Point(0, 0);
			Point point2 = new Point(width, 0);

			// Draw line to screen.
			e.Graphics.DrawLine(grayPen, point1, point2);
		}

		private void pnlLogoRight_Paint(object sender, PaintEventArgs e)
		{
			Pen grayPen = new Pen(Color.LightGray);
			int width = pnlLogoRight.Width;
			int height = pnlLogoRight.Height;

			Point point1 = new Point(width-1, 0);
			Point point2 = new Point(width-1, height);

			// Draw line to screen.
			e.Graphics.DrawLine(grayPen, point1, point2);
		}

		// closes form
		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		// FormClosed listener
		private void frmParent_FormClosed(object sender, FormClosedEventArgs e)
		{
			// deletes bad profile on application exit if connection was unsuccessful
			if (lblProfileCondition.Text == "BADPROFILE")
			{
				ConnectToEduroam.RemoveProfile();
			}
		}
	}
}
