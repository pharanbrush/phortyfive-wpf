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

namespace PhortyFiveSeconds.components
{
	public partial class HotkeyListItem : UserControl
	{
		
		public string CommandName
		{
			get => (string)GetValue(CommandLabelProperty);
			set => SetValue(CommandLabelProperty, value);
		}

		public string Modifier
		{
			get => (string)GetValue(ModifierProperty);
			set => SetValue(ModifierProperty, value);
		}


		public string Key
		{
			get => (string)GetValue(KeyProperty);
			set => SetValue(KeyProperty, value);
		}

		public static readonly DependencyProperty CommandLabelProperty =
			DependencyProperty.Register(nameof(CommandName), typeof(string), typeof(HotkeyListItem), new PropertyMetadata(string.Empty));


		public static readonly DependencyProperty ModifierProperty =
			DependencyProperty.Register(nameof(Modifier), typeof(string), typeof(HotkeyListItem), new PropertyMetadata(string.Empty));


		// Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty KeyProperty =
			DependencyProperty.Register(nameof(Key), typeof(string), typeof(HotkeyListItem), new PropertyMetadata(string.Empty));

		public HotkeyListItem ()
		{
			InitializeComponent();
		}
	}
}
