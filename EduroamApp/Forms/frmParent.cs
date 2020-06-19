using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Device.Location;
using System.Xml;
using Image = System.Drawing.Image;

namespace EduroamApp
{
	/// <summary>
	/// Main form.
	/// All other forms are loaded into a panel in this form.
	/// </summary>
	public partial class frmParent : Form
	{
		private enum FormId
		{
			Summary,
			SelectMethod,
			Download,
			Local,
			Login,
			Connect,
			Redirect,
			LocalCert,
			SaveAndQuit
		}

		// private variables to be used in this form
		private FormId currentFormId;                                      // Id of currently selected form
		private readonly List<FormId> historyFormId = new List<FormId>();  // Keeps history of previously diplayed forms, in order to backtrack correctly
		private bool reload = true;                                        // Specifies wether a form is to be re-instantiated when loaded
		private EapConfig eapConfig = new EapConfig();                     // Selected EAP configuration

		// makes forms globally accessible in parent form
		private frmSummary frmSummary;
		private frmSelectMethod frmSelectMethod;
		private frmDownload frmDownload;
		private frmLocal frmLocal;
		private frmConnect frmConnect;
		private frmLogin frmLogin;
		private frmRedirect frmRedirect;

		// public variables to be used across forms
		public GeoCoordinateWatcher GeoWatcher { get; set; }
		public uint EapType { get; set; }  // EAP type of selected EAP config
		public string InstId { get; set; }
		public string ProfileCondition { get; set; }
		public string LocalFileType { get; set; }
		public string RedirectUrl { get; set; }
		public bool ComesFromSelfExtract { get; set; }
		public bool SelfExtractFlag { get; set; }
		public bool SelectAlternative { get; set; }
		public bool EduroamAvailable { get; set; }
		public DateTime CertValidFrom { get; set; }

		public frmParent()
		{
			// starts GeoCoordinateWatcher when app starts
			GeoWatcher = new GeoCoordinateWatcher();
			GeoWatcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
			// adds formClosed listener
			FormClosed += frmParent_FormClosed;
			InitializeComponent();
		}

		private void frmParent_Load(object sender, EventArgs e)
		{
			// sets eduroam logo
			webEduroamLogo.DocumentText = ImageFunctions.GenerateSvgLogoHtml(Properties.Resources.eduroam_logo, webEduroamLogo.Width, webEduroamLogo.Height);
			Icon = Properties.Resources.geteduroam;

			// checks if file came with self extract
			eapConfig = GetSelfExtractingEap();
			if (eapConfig != null)
			{
				// sets flags
				ComesFromSelfExtract = true;
				SelfExtractFlag = true;
				// reset web logo or else it won't load
				ResetLogo();
				// loads summary form so user can confirm installation
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
			historyFormId.Add(currentFormId);

			switch (currentFormId)
			{
				// next form depends on EAP type of selected config
				case FormId.Summary:
					if (SelectAlternative) // if user has config from self extract but wants to select another inst
					{
						ResetLogo();
						LoadFrmSelectMethod();
						break;
					}
					EapType = (uint)frmSummary.InstallEapConfig();
					switch (EapType)
					{
						case (uint)EduroamApp.EapType.TLS:
							LoadFrmConnect(); break;
						case (uint)EduroamApp.EapType.PEAP:
						case (uint)EduroamApp.EapType.TTLS:
							LoadFrmLogin(); break;
						case 500: // User needs to find user certificate
							LocalFileType = "CERT";
							LoadFrmLocalCert();
							break;
						case 600: // Eduroam not available
							LoadFrmSaveAndQuit(); break;
						case 0:
							break; // todo, this shouldn't happen?
						default:
							MessageBox.Show(
								"Couldn't connect to eduroam. \n" +
								"Your institution does not have a valid configuration.",
								"Configuration not valid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							break;
					}
					break;

				// next form depends on radio button selection
				case FormId.SelectMethod:
					SelfExtractFlag = false;
					if (frmSelectMethod.GoToForm() == 3) LoadFrmDownload();
					else
					{
						LocalFileType = "EAPCONFIG";
						LoadFrmLocal();
					}
					break;

				// next form depends on if downloaded config contains redirect url or not
				case FormId.Download:
					string profileId = frmDownload.profileId;
					eapConfig = DownloadEapConfig(profileId);
					if (eapConfig != null)
					{
						LoadFrmSummary();
					}
					else if (!string.IsNullOrEmpty(RedirectUrl))
					{
						LoadFrmRedirect();
					}
					break;

				// opens summary form if config is not null
				case FormId.Local:
					eapConfig = frmLocal.LocalEapConfig();
					if (eapConfig != null) LoadFrmSummary();
					break;

				// lets user log in and opens connection form
				case FormId.Login:
					if (EapType != 21)
					{
						frmLogin.ConnectWithLogin();
						LoadFrmConnect();
					}
					else MessageBox.Show("Support for TTLS configuration is not yet ready.", "TTLS not ready", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;

				// closes application after successful connect
				case FormId.Connect:
					Close();
					break;

				// TODO: missing case Redirect. sanity is to throw on default

				// lets user select client cert and opens connection form
				case FormId.LocalCert:
					if (frmLocal.InstallCertFile()) LoadFrmConnect();
					break;

				// closes application after saving setup
				case FormId.SaveAndQuit:
					Close();
					break;
			}

			// removes current form from history if it gets added twice
			if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);
		}

		private void btnBack_Click(object sender, EventArgs e)
		{
			// reuses existing instances of forms when going backwards
			reload = false;
			// clears logo if going back from summary page
			if (currentFormId == FormId.Summary) ResetLogo();

			switch (historyFormId.Last())
			{
				case FormId.Summary:
					if (SelfExtractFlag) // reloads the included config file if exists
					{
						eapConfig = GetSelfExtractingEap();
					}
					LoadFrmSummary();
					break;
				case FormId.SelectMethod:
					if (ComesFromSelfExtract) SelfExtractFlag = true; // enables back button if config file included in self extract
					LoadFrmSelectMethod();
					break;
				case FormId.Download:
					LoadFrmDownload();
					break;
				case FormId.Local:
					LoadFrmLocal();
					break;
				case FormId.Login:
					LoadFrmLogin();
					break;
				case FormId.LocalCert:
					LoadFrmLocalCert();
					break;
				// TODO: missing cases? sanity is to throw on default
			}

			// removes current form from history
			historyFormId.RemoveAt(historyFormId.Count - 1);
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
				eapConfig = ConnectToEduroam.ParseEapXmlData(eapString);
				return eapConfig;
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Gets EAP-config file, either directly or after browser authentication.
		/// Prepares for redirect if no EAP-config.
		/// </summary>
		/// <returns>EapConfig object.</returns>
		public EapConfig DownloadEapConfig(string profileId)
		{
			// checks if user has selected an institution and/or profile
			if (string.IsNullOrEmpty(profileId))
			{
				MessageBox.Show("Please select an institution and/or a profile.",
					"Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null; // exits function if no institution/profile selected
			};
			string redirect = IdentityProviderDownloader.GetRedirect(profileId);
			// eap config file as string
			string eapString;

			// if no redirect link
			if (string.IsNullOrEmpty(redirect))
			{
				// gets eap config file directly
				eapString = IdentityProviderDownloader.GetEapConfigString(profileId);
			}
			// if Let's Wifi redirect
			else if (redirect.Contains("#letswifi"))
			{
				// get eap config file from browser authenticate
				try
				{
					OAuth oauth = new OAuth();
					// generate authURI based on redirect
					string authUri = oauth.GetAuthUri(redirect);
					// browser authenticate
					string responseUrl = GetResponseUrl(redirect, authUri);
					// get eap-config string if available
					eapString = oauth.GetEapConfigString(responseUrl);
				}
				catch (EduroamAppUserError ex)
				{
					MessageBox.Show(ex.UserFacingMessage);
					eapString = "";
				}
				// return focus to application
				Activate();
			}
			// if other redirect
			else
			{
				// makes redirect link accessible in parent form
				RedirectUrl = redirect;
				return null;
			}

			// if not empty, creates and returns EapConfig object from Eap string
			if (string.IsNullOrEmpty(eapString))
			{
				return null;
			}

			try
			{
				// if not empty, creates and returns EapConfig object from Eap string
				return ConnectToEduroam.ParseEapXmlData(eapString);
			}
			catch (XmlException ex)
			{
				MessageBox.Show(
					"The selected institution or profile is not supported. " +
					"Please select a different institution or profile.\n" +
					"Exception: " + ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Gets a response URL after doing Browser authentication with Oauth authUri.
		/// </summary>
		/// <returns>response Url as string.</returns>
		public string GetResponseUrl(string redirectUri, string authUri)
		{
			string responseUrl; //= WebServer.NonblockingListener(redirectUri, authUri, parentLocation);
			using (var waitForm = new frmWaitDialog(redirectUri, authUri))
			{
				DialogResult result = waitForm.ShowDialog();
				if (result != DialogResult.OK)
					return "";
				responseUrl = waitForm.responseUrl;
			}
			return responseUrl;
		}

		public PictureBox PbxLogo => pbxLogo;
		public WebBrowser WebLogo => webLogo;

		public bool BtnNextEnabled
		{
			get => btnNext.Enabled;
			set => btnNext.Enabled = value;
		}

		public string BtnNextText
		{
			get => btnNext.Text;
			set => btnNext.Text = value;
		}

		public bool BtnBackEnabled
		{
			get => btnBack.Enabled;
			set => btnBack.Enabled = value;
		}

		public bool BtnBackVisible
		{
			get => btnBack.Visible;
			set => btnBack.Visible = value;
		}

		public bool BtnCancelEnabled
		{
			get => btnCancel.Enabled;
			set => btnCancel.Enabled = value;
		}


		/// <summary>
		/// Loads form that shows summary of selected EAP configuration.
		/// </summary>
		public void LoadFrmSummary()
		{
			if (reload) frmSummary = new frmSummary(this, eapConfig);
			currentFormId = FormId.Summary;
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
			if (!reload) btnNext.Enabled = true;
			btnNext.Text = eapConfig.AuthenticationMethods.First().EapType == EduroamApp.EapType.TLS ? "Connect" : "Next >";
			LoadNewForm(frmSummary);
		}

		/// <summary>
		/// Loads form that lets user choose how they want to get config file.
		/// </summary>
		public void LoadFrmSelectMethod()
		{
			frmSelectMethod = new frmSelectMethod(this);
			currentFormId = FormId.SelectMethod;
			lblTitle.Text = "Select configuration source";
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
			if (reload) frmDownload = new frmDownload(this);
			currentFormId = FormId.Download;
			lblTitle.Text = "Select your institution";
			btnNext.Enabled = !reload;
			btnNext.Text = "Next >";
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
			currentFormId = FormId.Local;
			lblTitle.Text = "Select EAP-config file";
			btnNext.Enabled = true;
			btnNext.Text = "Next >";
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
			currentFormId = FormId.Login;
			lblTitle.Text = "Log in";
			btnNext.Enabled = false;
			btnNext.Text = "Connect";
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
			currentFormId = FormId.Connect;
			lblTitle.Text = "Connection status";
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
			currentFormId = FormId.Redirect;
			lblTitle.Text = "You are being redirected";
			btnNext.Enabled = false;
			btnNext.Text = "Next >";
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
			currentFormId = FormId.LocalCert;
			lblTitle.Text = "Select client certificate file";
			btnNext.Enabled = true;
			btnNext.Text = "Connect";
			btnBack.Enabled = true;
			btnBack.Visible = true;
			LoadNewForm(frmLocal);
		}

		/// <summary>
		/// Loads form that lets user save configuration and quit.
		/// </summary>
		public void LoadFrmSaveAndQuit()
		{
			frmConnect = new frmConnect(this);
			currentFormId = FormId.SaveAndQuit;
			lblTitle.Text = "eduroam not available"; // TODO: not obvious from function name
			btnNext.Text = "Save";
			btnNext.Enabled = true;
			btnBack.Enabled = false;
			btnBack.Visible = true;
			LoadNewForm(frmConnect);
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

		/// <summary>
		/// Empties both logo controls and makes them invisible.
		/// </summary>
		public void ResetLogo()
		{
			// reset pbxLogo
			pbxLogo.Image = null;
			pbxLogo.Visible = false;

			// reset webLogo
			webLogo.Navigate("about:blank");
			if (webLogo.Document != null)
			{
				webLogo.Document.Write(string.Empty);
			}
			webLogo.Visible = false;
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
			if (ProfileCondition == "BADPROFILE")
			{
				ConnectToEduroam.RemoveProfile();
			}
		}
	}
}
