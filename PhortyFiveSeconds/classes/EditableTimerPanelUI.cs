using System;
using System.Collections.Generic;
using System.Text;

using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PhortyFiveSeconds;

internal class EditableTimerPanelUI
{
	readonly TextBox textBox;
	readonly FrameworkElement activePanel;
	readonly Label collapsedLabel;
	readonly Action Unfocus;
	readonly Func<int> GetTimerDurationSeconds;
	readonly Action<string> SetTimerDuration;


	public string TextBoxValue
	{
		get => textBox.Text;
		set
		{
			textBox.Text = value;
			collapsedLabel.Content = $"{value} seconds each";
		}
	}

	public EditableTimerPanelUI (TextBox textBox, FrameworkElement activePanel, Label collapsedLabel, Action unfocusHandler
, Func<int> getTimerDurationSeconds, Action<string> setTimerDuration)
	{
		this.textBox = textBox;
		this.collapsedLabel = collapsedLabel;
		this.activePanel = activePanel;
		Unfocus = unfocusHandler;
		GetTimerDurationSeconds = getTimerDurationSeconds;
		SetTimerDuration = setTimerDuration;

		collapsedLabel.MouseDown += (_, mouseEvent) => {
			mouseEvent.Handled = true;
			this.SetActive(true);
		};

		textBox.LostFocus += (_, _) => SetActive(false);
		textBox.KeyDown += (_, keyEvent) => {
			var key = keyEvent.Key;
			if (key is Key.Escape)
			{
				keyEvent.Handled = true;
				UserCancel();

			}
			else if (key is Key.Enter)
			{
				keyEvent.Handled = true;
				UserConfirm();
			}
		};

		this.SetActive(false);
	}

	public void UserConfirm ()
	{
		SetTimerDuration.Invoke(TextBoxValue);
		SetActive(false);
	}

	public void UserCancel ()
	{
		TextBoxValue = GetTimerDurationSeconds.Invoke().ToString();
		SetActive(false);
	}

	public void SetActive (bool active)
	{
		WindowUtilities.SetCollapsiblePanelActive(activePanel, collapsedLabel, active);

		if (active)
		{
			textBox.Focus();
			textBox.SelectAll();
		}
		else
		{
			Unfocus.Invoke();
		}
	}
}
