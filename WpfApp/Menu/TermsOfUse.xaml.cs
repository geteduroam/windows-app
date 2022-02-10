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

			tou = tou.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

			tou = Regex.Replace(tou, "([\t ]*\r?\n){2,}", " <p>");
			tou = Regex.Replace(tou, "([\t ]*\r?\n)", "<br>");

			// Split on space (so we can check per word if it is a link)
			string[] touSplit = Regex.Split(tou, "[\\s ]+");

			for (int i = 0; i < touSplit.Length; i++)
			{
				var start = Regex.Match(touSplit[i], "^(&quot;|&lt;|&gt;|[\\.,\\?!':;])+").Value;
				var end = Regex.Match(touSplit[i], "(&quot;|&lt;|&gt;|[\\.,\\?!':;])+$").Value;
				var w = touSplit[i].Substring(start.Length, touSplit[i].Length - end.Length - start.Length);
				if (w.StartsWith("www.") || w.StartsWith("http://") || w.StartsWith("https://"))
				{
					var link = w.StartsWith("http") ? w : "http://" + w;
					
					touSplit[i] = start + "<a target=\"_blank\" href=\"" + link + "\">" + w + "</a>" + end;
				}
			}

			tou = String.Join(" ", touSplit);

			return
				"<!DOCTYPE html>" +
				"<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">" +
				"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
				"<style>" +
					"body {" +
						"margin: 2em;" +
						"justify-content: center;" +
						"font-family: \"Segoe UI\", \"Tahoma\", sans-serif;" +
						"font-size: .9em;" +
					"}" +
				"</style>" +
				tou;
		}
	}
}
