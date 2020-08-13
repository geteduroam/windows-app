using System.Windows.Controls;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for TermsOfUse.xaml
	/// </summary>
	public partial class TermsOfUse : Page
	{
		private MainWindow mainWindow;
		private readonly string tou;
		public TermsOfUse(MainWindow mainWindow, string tou)
		{
			this.mainWindow = mainWindow;
			this.tou = tou;
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			this.tbTou.Text = tou;
		}
	}
}
