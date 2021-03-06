using EduroamConfigure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Classes
{
	/// <summary>
	/// Interaction logic for CertificateGrid.xaml
	/// </summary>
	public partial class CertificateGrid : UserControl, IObservable<ConnectToEduroam.CertificateInstaller>
	{

		private readonly List<IObserver<ConnectToEduroam.CertificateInstaller>> observers;
		public CertificateGrid()
		{
			observers = new List<IObserver<ConnectToEduroam.CertificateInstaller>>();
			InitializeComponent();
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { this.SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(CertificateGrid), new UIPropertyMetadata(null));

		public bool IsInstalled
		{

			get { return (bool)GetValue(IsInstalledProperty); }
			set { this.SetValue(IsInstalledProperty, value); }
		}

		public static readonly DependencyProperty IsInstalledProperty =
			DependencyProperty.Register("IsInstalled", typeof(bool), typeof(CertificateGrid), new UIPropertyMetadata(null));


		public ConnectToEduroam.CertificateInstaller Installer
		{

			get { return (ConnectToEduroam.CertificateInstaller)GetValue(InstallerProperty); }
			set
			{
				if (value == null) throw new ArgumentNullException("Installer");
				this.SetValue(InstallerProperty, value);
				this.IsInstalled = value.IsInstalled;
				this.Text = value.ToString();
			}
		}

		public static readonly DependencyProperty InstallerProperty =
			DependencyProperty.Register("Installer", typeof(ConnectToEduroam.CertificateInstaller), typeof(CertificateGrid), new UIPropertyMetadata(null));

		private void btnInstall_Click(object sender, RoutedEventArgs e)
		{
			this.Installer.AttemptInstallCertificate();
			this.IsInstalled = Installer.IsInstalled;
			if (Installer.IsInstalledByUs)
			{
				// Any CA that we have installed must also be removed by us when it is not needed anymore
				// so install the geteduroam app when we have installed a CA
				_ = Task.Run(App.Installer.EnsureIsInstalled);
			}
			NotifySubscribers();
		}

		private void NotifySubscribers()
		{
			foreach (IObserver<ConnectToEduroam.CertificateInstaller> observer in observers)
			{
				observer.OnNext(Installer);
			}
		}

		public IDisposable Subscribe(IObserver<ConnectToEduroam.CertificateInstaller> observer)
		{
			if (!observers.Contains(observer))
			{
				this.observers.Add(observer);
			}
			return new Unsubscriber<ConnectToEduroam.CertificateInstaller>(observers, observer);
		}

		internal class Unsubscriber<CertificateInstaller> : IDisposable
		{
			private readonly List<IObserver<CertificateInstaller>> _observers;
			private readonly IObserver<CertificateInstaller> _observer;

			internal Unsubscriber(List<IObserver<CertificateInstaller>> observers, IObserver<CertificateInstaller> observer)
			{
				this._observers = observers;
				this._observer = observer;
			}

			public void Dispose()
			{
				if (_observers.Contains(_observer))
					_observers.Remove(_observer);
			}
		}
	}



}
