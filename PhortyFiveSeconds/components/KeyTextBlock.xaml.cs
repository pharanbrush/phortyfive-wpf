using System.Windows;
using System.Windows.Controls;

namespace PhortyFiveSeconds.components
{
	public partial class KeyTextBlock : UserControl
	{
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(nameof(Text), typeof(string), typeof(KeyTextBlock), new PropertyMetadata(string.Empty));

		public KeyTextBlock ()
		{
			InitializeComponent();
			//if (string.IsNullOrEmpty(Text))
			//{
			//	KeyBoarder.Visibility = Visibility.Collapsed;
			//}
		}
	}
}
