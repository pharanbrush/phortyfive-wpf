using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PhortyFiveSeconds
{

	

	public partial class App : Application
	{
		public static Brush? GetBrush (string key) => Current.Resources[key] as Brush;

		static public string AssemblyVersionNumber => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
	}
}
