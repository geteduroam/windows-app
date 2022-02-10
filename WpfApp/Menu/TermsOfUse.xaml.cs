using System;
using System.Text.RegularExpressions;
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
			webTou.NavigateToString(GetTouHtml(tou));
		}

		private string GetTouHtml(string tou)
		{
			var maybeBom = tou.Substring(0, 3);
			var bom = new string(new char[] { (char)0xEF, (char)0xBB, (char)0xBF });

			if (maybeBom.Equals(bom))
			{
				tou = tou.Substring(3);
			}

			bool fixLinks = !Regex.IsMatch(tou, "(^|\\s)<a[\\S\t ]*>");
			bool fixNewlines = !Regex.IsMatch(tou, "(^|\\s)<(p|br)[\\S\t ]*>");

			if (fixNewlines)
			{
				tou = Regex.Replace(tou, "([\t ]*\r?\n){2,}", " <p>");
				tou = Regex.Replace(tou, "([\t ]*\r?\n)", "<br>");
			}


			if (fixLinks)
			{
				// Split on space (so we can check per word if it is a link)
				string[] touSplit = Regex.Split(tou, "[\\s ]+");

				for (int i = 0; i < touSplit.Length; i++)
				{
					var w = touSplit[i];
					if (w.StartsWith("www.") || w.StartsWith("http://") || w.StartsWith("https://"))
					{
						var link = w.StartsWith("http") ? w : "http://" + w;
						touSplit[i] = "<a target=\"_blank\" href=\"" + link + "\">" + w + "</a>";
					}
				}

				tou = String.Join(" ", touSplit);
			}

			return
				"<!DOCTYPE html>" +
				"<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">" +
				"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
				"<style>" +
					"body {" +
						"margin: 2em;" +
						"justify-content: center;" +
						"font-family: \"Segoe UI\", \"Tahoma\", sans-serif;" +
					"}" +
				"</style>" +
				tou;
		}
	}
}
