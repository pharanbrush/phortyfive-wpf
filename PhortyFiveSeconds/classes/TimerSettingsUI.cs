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

		//TODO: How do I make this close on open menu? It seems to make the check on mouse up, but mouse down already closes the menu so it opens it again.
		settingsButton.Click += (_, _) => menu.IsOpen = true;
	}

	bool TryParseDurationFromString (string durationString, out int duration)
	{
		const int MinimumDuration = 5;
		const int MaximumDuration = 30 * 60;
		if (int.TryParse(durationString, out int tempDuration))
		{
			duration = Math.Clamp(tempDuration, MinimumDuration, MaximumDuration);
			return true;
		}

		duration = -1;
		return false;
	}

	internal bool TrySetDuration (string durationString)
	{
		if (TryParseDurationFromString(durationString, out int duration))
		{
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
