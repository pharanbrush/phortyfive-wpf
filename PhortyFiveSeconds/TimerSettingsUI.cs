using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PhortyFiveSeconds;

internal class TimerSettingsUI
{
	readonly List<(MenuItem menuItem, int seconds)> presetMenuItems = new();
	Action<int>? setDurationHandler;
	ContextMenu? menu;

	internal void InitializeMenuChoices (Button settingsButton, IEnumerable<(int duration, string text)> itemsToAdd, Action<int> setDurationHandler)
	{
		this.setDurationHandler = setDurationHandler;

		menu = new ContextMenu();
		var menuItems = menu.Items;

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

	public void UpdateSelectedState (int currentDurationSeconds)
	{
		foreach (var (menuItem, seconds) in presetMenuItems)
		{
			bool isChosen = seconds == currentDurationSeconds;
			menuItem.IsEnabled = !isChosen;
			menuItem.IsChecked = isChosen;
		}
	}
}
