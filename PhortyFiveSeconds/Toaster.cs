using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PhortyFiveSeconds;

internal class Toaster
{
	readonly Label label;
	public TimeSpan ToastDuration
	{
		get => timer.Interval;
		set => timer.Interval = value;
	}

	readonly DispatcherTimer timer;

	public Toaster (Label label)
	{
		this.label = label;
		timer = new() { Interval = new TimeSpan(0, 0, 2) };

		timer.Tick += (_, _) => {
			timer.Stop();
			HideToast();
		};
	}

	public void Toast (string toastMessage)
	{
		label.Content = toastMessage;
		label.Visibility = Visibility.Visible;
		timer.Start();
	}

	public void HideToast ()
	{
		label.Content = string.Empty;
		label.Visibility = Visibility.Hidden;
	}
}
