using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PhortyFiveSeconds;

static public class WindowUtilities
{
	public static void InvokeDelayed (int delaySeconds, Action action) =>
		Task.Delay(new TimeSpan(0, 0, delaySeconds)).ContinueWith(t => action.Invoke());

	public static void InvokeForUI (Window window, Action action) => window.Dispatcher.Invoke(action);

	public static void MakeTooltipImmediate (this FrameworkElement element)
	{
		ToolTipService.SetInitialShowDelay(element, 10);
	}

	public static void MakeTooltipsImmediate (this IEnumerable<FrameworkElement> elements)
	{
		foreach (var element in elements)
		{
			element.MakeTooltipImmediate();
		}
	}

	public static void SetTooltipPlacement (this FrameworkElement element, PlacementMode mode)
	{
		ToolTipService.SetPlacement(element, mode);
	}

	public static void SetTooltipPlacement (this IEnumerable<FrameworkElement> elements, PlacementMode mode)
	{
		foreach (var element in elements)
		{
			ToolTipService.SetPlacement(element, mode);
		}
	}

	public static void SetPanelActive (this UIElement element, bool active) => element.Visibility = active ? Visibility.Visible : Visibility.Collapsed;

	public static void SetCollapsiblePanelActive (UIElement activePanel, UIElement inactivePanel, bool active)
	{
		activePanel.SetPanelActive(active);
		inactivePanel.SetPanelActive(!active);
	}

	public static void AllowRightClickCopy (Label label, string menuItemLabel = "Copy")
	{
		ContextMenu contextMenu = label.ContextMenu;
		if (contextMenu is null)
		{
			contextMenu = new();
			label.ContextMenu = contextMenu;
		}

		MenuItem copy = new() { Header = menuItemLabel };

		copy.Click += (_, _) => CopyLabelTextToClipboard(label);
		contextMenu.Items.Add(copy);
	}

	public static void CopyLabelTextToClipboard (Label label)
	{
		var labelContent = label.Content;
		if (labelContent is null) return;
		if (labelContent is string labelText)
		{
			if (string.IsNullOrEmpty(labelText)) return;
			Clipboard.SetText(labelText);
		}
		if (labelContent is TextBlock textBlock)
		{
			string text = textBlock.Text;
			if (string.IsNullOrEmpty(text)) return;
			Clipboard.SetText(text);
		}
	}
}
