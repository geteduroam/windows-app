using SingleInstanceApp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, ISingleInstance
	{
		private const string SingleInstanceUid = "7aab8621-df45-4eb5-85c3-c70c06e8a22e";

		[STAThread]
		public static void Main(string[] args)
		{
			if (SingleInstance<App>.InitializeAsFirstInstance(SingleInstanceUid))
			{
				try
				{
					if (PreGuiCommandLineArgs(args))
						return;
					var app = new App();
					app.InitializeComponent();
					app.Run();
				}
				finally
				{
					SingleInstance<App>.Cleanup();
				}
			}
		}

		/// <summary>
		/// Handles command line args not related to wpf behaviour
		/// </summary>
		/// <returns>true if startup is to be aborted</returns>
		static bool PreGuiCommandLineArgs(string[] args)
		{
			// shorthand
			bool contains(string check) =>
				args.Any(param => string.Equals(param, check, StringComparison.InvariantCultureIgnoreCase));

			return false;
		}

		/// <summary>
		/// WPF startup handler, first instance runs this.
		/// Handles command line args related to wpf behaviour
		/// </summary>
		/// <param name="e"></param>
		protected override void OnStartup(StartupEventArgs e)
		{
			// shorthand
			bool contains(string check) =>
				e.Args.Any(param => string.Equals(param, check, StringComparison.InvariantCultureIgnoreCase));

			if (contains("/close"))
				Shutdown();
			// TODO
			//if (contains("/background"))

		}

		/// <summary>
		/// Signal handler from secondary instances.
		/// Handles command line arguments sent from second instance.
		/// </summary>
		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			System.Diagnostics.Debug.WriteLine("second!");
			System.Diagnostics.Debug.WriteLine(string.Join("\n", args));

			// Return value has no effect:
			// https://github.com/taylorjonl/SingleInstanceApp/blob/master/SingleInstance.cs#L261
			return true;
		}

	}
}
