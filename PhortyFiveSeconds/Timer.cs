﻿using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace PhortyFiveSeconds;
public class Timer
{
	readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(500);

	readonly DispatcherTimer dispatcherTimer;

	TimeSpan TimeLeft { get; set; } = TimeSpan.FromSeconds(30);
	TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(30);

	DateTime lastTime;
	bool elapsedThisRound = false;

	bool isActive = false;
	public bool IsActive
	{
		get => isActive;
		set
		{
			bool originalValue = isActive;
			isActive = value;
			lastTime = DateTime.Now;

			if (originalValue != isActive)
			{
				PlayPauseChanged?.Invoke();
			}
		}
	}

	public event Action? PlayPauseChanged;
	public event Action? Tick;
	public event Action? Elapsed;
	public event Action? Restarted;
	public event Action? DurationChanged;

	public int DurationSeconds => (int)Duration.TotalSeconds;
	public double FractionLeft => (TimeLeft.TotalMilliseconds / Duration.TotalMilliseconds);

	public void Pause () => this.IsActive = false;
	public void Resume () => this.IsActive = true;
	public void TogglePlayPause () => IsActive = !IsActive;

	public Timer ()
	{
		dispatcherTimer = new(DispatcherPriority.Normal)
		{
			Interval = UpdateInterval
		};
		dispatcherTimer.Tick += TimerTickEventHandler;

		lastTime = DateTime.Now;
	}

	public void SetDuration (TimeSpan newDuration)
	{
		Duration = newDuration;
		elapsedThisRound = false;
		DurationChanged?.Invoke();
	}

	public void Restart ()
	{
		dispatcherTimer.Stop();
		dispatcherTimer.Start();
		lastTime = DateTime.Now;
		TimeLeft = Duration;
		elapsedThisRound = false;
		Restarted?.Invoke();
	}

	void TimerTickEventHandler (object? sender, EventArgs e)
	{

		if (IsActive)
		{
			UpdateMillisecondsLeft();
			if (!elapsedThisRound && TimeLeft.TotalMilliseconds < 0)
			{
				elapsedThisRound = true;
				Elapsed?.Invoke();
			}
			Tick?.Invoke();
		}
	}

	void UpdateMillisecondsLeft ()
	{
		var now = DateTime.Now;
		var deltaTime = now - lastTime;
		TimeLeft -= deltaTime;

		lastTime = now;
	}
}
