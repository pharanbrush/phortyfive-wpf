using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace PhortyFiveSeconds;
public class Timer
{
	const int SecondsToMilliseconds = 1000;
	const int UpdateIntervalMilliseconds = 500;

	readonly DispatcherTimer dispatcherTimer;

	int MillisecondsLeft { get; set; } = 30 * SecondsToMilliseconds;
	int DurationMilliseconds { get; set; } = 30 * SecondsToMilliseconds;
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
				OnPlayPauseChanged?.Invoke();
			}
		}
	}

	public event Action? OnPlayPauseChanged;
	public event Action? OnTick;
	public event Action? OnElapsed;
	public event Action? OnRestart;
	public event Action? OnDurationChanged;

	public int DurationSeconds => DurationMilliseconds / SecondsToMilliseconds;

	public float FractionLeft => (float)MillisecondsLeft / (float)DurationMilliseconds;

	public void Pause () => this.IsActive = false;
	public void Resume () => this.IsActive = true;
	public void TogglePlayPause () => IsActive = !IsActive;

	public Timer ()
	{
		dispatcherTimer = new()
		{
			Interval = new TimeSpan(0, 0, 0, 0, UpdateIntervalMilliseconds)
		};
		dispatcherTimer.Tick += HandleTimerTick;

		lastTime = DateTime.Now;
	}

	public void SetDuration (int seconds)
	{
		DurationMilliseconds = seconds * SecondsToMilliseconds;
		elapsedThisRound = false;
		OnDurationChanged?.Invoke();
	}

	public void Restart ()
	{
		dispatcherTimer.Stop();
		dispatcherTimer.Start();
		lastTime = DateTime.Now;
		MillisecondsLeft = DurationMilliseconds;
		elapsedThisRound = false;
		OnRestart?.Invoke();
	}

	void HandleTimerTick (object? sender, EventArgs e)
	{
		if (IsActive)
		{
			UpdateMillisecondsLeft();
			if (!elapsedThisRound && MillisecondsLeft < 0)
			{
				elapsedThisRound = true; // This needs to be set before invoking OnElapsed because a handler may call a restart.
				OnElapsed?.Invoke();
			}
			OnTick?.Invoke();
		}
	}

	void UpdateMillisecondsLeft ()
	{
		var now = DateTime.Now;
		var deltaDateTime = now - lastTime;
		var deltaTimeMilliseconds = (int)deltaDateTime.TotalMilliseconds;
		MillisecondsLeft -= deltaTimeMilliseconds;

		lastTime = now;
	}
}
