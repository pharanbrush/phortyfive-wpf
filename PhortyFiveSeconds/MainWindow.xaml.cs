using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PhortyFiveSeconds;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	const int TimerIndicatorMaximum = 200;

	readonly FileList FileList = new();
	readonly Circulator Circulator = new();
	readonly Timer Timer = new();
	readonly WPFImageView ImageView;
	readonly TimerSettingsUI TimerSettingsUI = new();
	readonly Toaster Toaster;
	readonly SoundPlayer soundPlayer = new(Properties.Resources.ClackSound);

	bool IsImageSetLoaded => Circulator.IsPopulated;
	static Brush? GetBrush (string key) => Application.Current.Resources[key] as Brush;

	static string AssemblyVersionNumber => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

	IEnumerable<Button> ImageSetButtons
	{
		get
		{
			yield return PlayPauseButton;
			yield return NextButton;
			yield return PrevButton;
			yield return RestartTimerButton;
		}
	}

	public MainWindow ()
	{
		InitializeComponent();
		Toaster = new(ToastLabel);

		ImageView = new(MainImageView, ImageFileNameLabel);
		ImageView.HandleClipboardToast(Toaster.Toast);

		soundPlayer.LoadAsync();
		VersionNumberOverlayLabel.Content = AssemblyVersionNumber;

		Timer.OnPlayPauseChanged += UpdatePlayPauseButtonState;
		Timer.OnPlayPauseChanged += UpdateTimerPlayPausedIndicator;
		Timer.OnRestart += UpdateTimerIndicatorTick;
		Timer.OnTick += UpdateTimerIndicatorTick;
		Timer.OnElapsed += TryMoveNext;
		Timer.OnDurationChanged += UpdateTimerDurationIndicators;
		Timer.SetDuration(30);

		TimeBar.Maximum = TimerIndicatorMaximum;

		(int duration, string text)[] durationMenuItems = {
			(15, "15 seconds"),
			(30, "30 seconds"),
			(45, "45 seconds"),
			(60, "1 minute"),
			(2 * 60, "2 minutes"),
			(5 * 60, "5 minutes"),
			(10 * 60, "10 minutes"),
		};

		TimerSettingsUI.InitializeMenuChoices(SettingsButton, durationMenuItems, SetTimerDuration);
		Circulator.OnCurrentNumberChanged += UpdateCurrentImage;
		Circulator.OnCurrentNumberChanged += PlaySound;
		soundPlayer.LoadAsync();
		OpenFolderButton.MakeTooltipImmediate();
		OpenFolderButton.SetTooltipPlacement(PlacementMode.Top);

		foreach (var button in ImageSetButtons)
		{
			button.MakeTooltipImmediate();
		}

		UpdatePlayPauseButtonState();
		UpdateTimerDurationIndicators();
		UpdateInteractibleState();
	}

	void OpenFolderButton_Click (object sender, RoutedEventArgs e) => UserOpenFiles();
	void RestartTimerButton_Click (object sender, RoutedEventArgs e) => DoIfValidState(Timer.Restart);
	void PrevButton_Click (object sender, RoutedEventArgs e) => TryMovePrevious();
	void PlayPauseButton_Click (object sender, RoutedEventArgs e) => DoIfValidState(Timer.TogglePlayPause);
	void NextButton_Click (object sender, RoutedEventArgs e) => TryMoveNext();

	void DoIfValidState (Action action)
	{
		if (IsImageSetLoaded)
			action.Invoke();
	}

	void SetTimerDuration (int durationSeconds)
	{
		Timer.SetDuration(durationSeconds);
		Timer.Restart();
	}

	void TryMoveNext ()
	{
		DoIfValidState(() => {
			Circulator.MoveNext();
			Timer.Restart();
		});
	}

	void TryMovePrevious ()
	{
		DoIfValidState(() => {
			Circulator.MovePrevious();
			Timer.Restart();
		});
	}

	void UserOpenFiles ()
	{
		if (FileUtilities.OpenPickerForImages(out IEnumerable<string> filePaths))
		{
			LoadFilesAndStartNewSet(filePaths);
		}
	}

	void LoadFilesAndStartNewSet (IEnumerable<string> filePaths)
	{
		var imagePaths = FileUtilities.EnumerateImages(filePaths);
		FileList.Load(imagePaths);
		Timer.Restart();

		if (FileList.IsPopulated)
		{
			TryStartNewSet();
		}

		UpdateInteractibleState();
	}

	void TryStartNewSet ()
	{
		if (!FileList.IsPopulated)
		{
			Circulator.Clear();
			return;
		}

		Circulator.StartNewOrder(FileList.Count);
		Timer.IsActive = true;
		DismissTips();
		UpdateSettingsTextBlock();
	}

	void UpdateTimerIndicatorTick ()
	{
		TimeBar.Value = Timer.FractionLeft * TimerIndicatorMaximum;
	}

	void UpdateCurrentImage ()
	{
		int currentIndex = Circulator.CurrentIndex;
		string currentImagePath = FileList.Get(currentIndex);
		ImageView.SetImage(currentImagePath);
	}

	void UpdateSettingsTextBlock ()
	{
		DebugText.Content = $"{FileList.Count} images : {Timer.DurationSeconds} seconds each";
	}

	void UpdateTimerDurationIndicators ()
	{
		TimerSettingsUI.UpdateSelectedState(Timer.DurationSeconds);
		UpdateTimerPlayPausedIndicator();
		UpdateSettingsTextBlock();
	}

	void UpdateTimerPlayPausedIndicator ()
	{
		var color = Timer.IsActive ? GetBrush("TimeBarActiveColor") : GetBrush("TimeBarPausedColor");
		TimeBar.Foreground = color;
	}

	void UpdatePlayPauseButtonState ()
	{
		const string PlaySymbol = "\uE768";
		const string PauseSymbol = "\uE769";

		var backgroundBrush = Timer.IsActive ? GetBrush("TimerActiveColor") : GetBrush("TimerPausedColor");
		string label = Timer.IsActive ? PauseSymbol : PlaySymbol;

		PlayPauseButton.Background = backgroundBrush;
		PlayPauseButton.Content = label;
	}

	void DismissTips ()
	{
		this.StartingTip.Visibility = Visibility.Hidden;
	}

	void UpdateInteractibleState ()
	{
		foreach (var button in ImageSetButtons)
		{
			button.IsEnabled = IsImageSetReady;
		}
	}

	void PlaySound ()
	{
		soundPlayer.Play();
	}
}
