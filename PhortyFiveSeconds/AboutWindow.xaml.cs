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
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Diagnostics;

namespace PhortyFiveSeconds
{
	public partial class AboutWindow : Window
	{
		static string PharanURL => "https://pharan.gumroad.com";

		public AboutWindow ()
		{
			InitializeComponent();

			ProgramNameLabel.Content = "Phorty-Five Seconds by PharanBrush\nTimed Image Viewer for Drawing Practice";

			VersionLabel.Content = $"Build {App.AssemblyVersionNumber}";
		}

		void PharanLink_RequestNavigate (object sender, RequestNavigateEventArgs e)
		{
			TryNavigate(e.Uri.AbsoluteUri);
			e.Handled = true;
		}

		void TryNavigate (string uri)
		{
			if (string.IsNullOrWhiteSpace(uri)) return;

			var processStartInfo = new ProcessStartInfo
			{
				FileName = uri,
				UseShellExecute = true,
			};

			Process.Start(processStartInfo);
		}
	}
}
