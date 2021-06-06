using System;
using System.Windows.Controls;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for Loading.xaml
	/// </summary>
	public partial class Loading : Page
	{
		private readonly MainWindow mainWindow;

		public Loading(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow ?? throw new ArgumentNullException(paramName: nameof(mainWindow));
			InitializeComponent();


		}
	}
}
