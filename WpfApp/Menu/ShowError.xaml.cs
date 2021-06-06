using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for Error.xaml
	/// </summary>
	public partial class ShowError : Page
	{
		private readonly MainWindow mainWindow;
		private readonly string errorMessage;

		public ShowError(MainWindow mainWindow, string errorMessage)
		{
			this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
			this.errorMessage = errorMessage;
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			tbError.Text = errorMessage;
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.PreviousPage();
		}
	}
}
