using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PhortyFiveSeconds;

internal class TimerSettingsUI
{
	readonly List<(MenuItem menuItem, int seconds)> presetMenuItems = new();
	Action<int>? setDurationHandler;
	readonly ContextMenu menu = new();

	Action? OnChooseCustom;

	internal void InitializeMenuChoices (Button settingsButton, IEnumerable<(int duration, string text)> itemsToAdd, Action<int> setDurationHandler, Action onChooseCustom)
	{
		this.setDurationHandler = setDurationHandler;
		this.OnChooseCustom = onChooseCustom;

		var menuItems = menu.Items;

		menuItems.Add(new MenuItem { Header = "Timer duration", IsEnabled = false, });
		menuItems.Add(new Separator());

		foreach (var (duration, text) in itemsToAdd)
		{
			AddDurationItem(duration, text);
		}

		menuItems.Add(new Separator());

		var customItem = new MenuItem()
		{
			Header = "Custom...",
		};
		customItem.Click += (_, _) => OnChooseCustom?.Invoke();
		menuItems.Add(customItem);

		menu.PlacementTarget = settingsButton;
		menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
		settingsButton.Click += (_, _) => menu.IsOpen = true;
	}

	internal bool TrySetDuration (string durationString)
	{
		if (int.TryParse(durationString, out int duration))
		{
			duration = Math.Clamp(duration, 5, 30 * 60);
			setDurationHandler?.Invoke(duration);
			return true;
		}

		return false;
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

	internal void AddMenuItem (object menuItem)
	{
		menu.Items.Add(menuItem);
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
