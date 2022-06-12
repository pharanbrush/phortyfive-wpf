using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PhortyFiveSeconds;

internal class TimerSettingsUI
{
	readonly List<(MenuItem menuItem, int seconds)> presetMenuItems = new();
	Action<int>? setDurationHandler;
	readonly ContextMenu menu = new();

	internal void InitializeMenuChoices (Button settingsButton, IEnumerable<(int duration, string text)> itemsToAdd, Action<int> setDurationHandler)
	{
		this.setDurationHandler = setDurationHandler;
		var menuItems = menu.Items;
		menuItems.Clear();

		menuItems.Add(new MenuItem { Header = "Timer duration", IsEnabled = false, });
		menuItems.Add(new Separator());

		foreach (var (duration, text) in itemsToAdd)
		{
			AddDurationItem(duration, text);
		}

		menuItems.Add(new Separator());
		menuItems.Add(new MenuItem()
		{
			Header = "Custom...",
			IsEnabled = false,
		});

		menu.PlacementTarget = settingsButton;
		menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
		settingsButton.Click += (_, _) => menu.IsOpen = true;
	}

	internal void UpdateSelectedState (int currentDurationSeconds)
	{
		foreach (var (menuItem, seconds) in presetMenuItems)
		{
			bool isChosen = seconds == currentDurationSeconds;
			menuItem.IsEnabled = !isChosen;
			menuItem.IsChecked = isChosen;
		}
	}

	void AddDurationItem (int durationSeconds, string text)
	{
		var newItem = new MenuItem
		{
			Header = text,
		};
		newItem.Click += (_, _) => setDurationHandler?.Invoke(durationSeconds);

		menu?.Items.Add(newItem);
		presetMenuItems.Add((newItem, durationSeconds));
	}
}
