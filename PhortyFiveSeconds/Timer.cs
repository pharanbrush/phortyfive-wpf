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
				PlayPauseChanged?.Invoke();
			}
		}
	}


	public int DurationSeconds => DurationMilliseconds / SecondsToMilliseconds;
	public event Action? PlayPauseChanged;
	public event Action? Tick;
	public event Action? Elapsed;
	public event Action? Restarted;
	public event Action? DurationChanged;

	public float FractionLeft => (float)MillisecondsLeft / (float)DurationMilliseconds;

	public void Pause () => this.IsActive = false;
	public void Resume () => this.IsActive = true;
	public void TogglePlayPause () => IsActive = !IsActive;

	public Timer ()
	{
		dispatcherTimer = new(DispatcherPriority.Normal)
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
		DurationChanged?.Invoke();
	}

	public void Restart ()
	{
		dispatcherTimer.Stop();
		dispatcherTimer.Start();
		lastTime = DateTime.Now;
		MillisecondsLeft = DurationMilliseconds;
		elapsedThisRound = false;
		Restarted?.Invoke();
	}

	void HandleTimerTick (object? sender, EventArgs e)
	{
		if (IsActive)
		{
			UpdateMillisecondsLeft();
			if (!elapsedThisRound && MillisecondsLeft < 0)
			{
				elapsedThisRound = true; // This needs to be set before invoking OnElapsed because a handler may call a restart.
				Elapsed?.Invoke();
			}
			Tick?.Invoke();
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
