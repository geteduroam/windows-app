using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EduroamConfigure;

namespace WpfApp.Menu
{
	/// <summary>
	/// Interaction logic for ProfileOverview.xaml
	/// </summary>
	public partial class ProfileOverview : Page
	{
		private readonly MainWindow mainWindow;
		private EapConfig eapConfig;

		public ProfileOverview(MainWindow mainWindow, EapConfig eapConfig)
		{
			this.mainWindow = mainWindow;
			this.eapConfig = eapConfig;
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			mainWindow.lblTitle.Content = eapConfig.InstitutionInfo.DisplayName;
			/* tbDesc.Text = "dadwaddwawaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaawdwadwdadadawbbbbdwaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
				 "dwaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
				 "dwaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
			*/
			tbDesc.Text = "hello to you your family your dog your father your neighbor and also";
			return;
		}

	}
}
