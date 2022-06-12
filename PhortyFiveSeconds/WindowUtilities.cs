using System;
using System.Threading.Tasks;
using System.Windows;

namespace PhortyFiveSeconds;

static internal class WindowUtilities
{
	internal static void InvokeDelayed (int delaySeconds, Action action) =>
		Task.Delay(new TimeSpan(0, 0, delaySeconds)).ContinueWith(t => action.Invoke());

	internal static void InvokeForUI (Window window, Action action)
	{
		window.Dispatcher.Invoke(action);
	}
}
