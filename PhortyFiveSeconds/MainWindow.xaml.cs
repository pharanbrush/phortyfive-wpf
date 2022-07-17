using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PhortyFiveSeconds;

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

	bool IsSoundEnabled { get; set; } = true;
	bool IsImageSetReady => Circulator.IsPopulated;
	static Brush? GetBrush (string key) => Application.Current.Resources[key] as Brush;
	static void SetPanelActive (UIElement element, bool active) => element.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
	static void SetCollapsiblePanelActive (UIElement activePanel, UIElement inactivePanel, bool active)
	{
		SetPanelActive(activePanel, active);
		SetPanelActive(inactivePanel, !active);
	}

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

	IEnumerable<FrameworkElement> TooltipElements
	{
		get
		{
			yield return HideBottomBarMiniButton;
			yield return ExpandBottomBarMiniButton;

			yield return SoundMiniButton;
			yield return AlwaysOnTopToggle;
			yield return AboutButton;
			yield return HelpButton;

			foreach (var button in ImageSetButtons)
			{
				yield return button;
			}
		}
	}

	bool IsAlwaysOnTop
	{
		get => this.Topmost;
		set => this.Topmost = value;
	}

	public MainWindow ()
	{
		InitializeComponent();
		SetEditableTimerPanelActive(false);

		VersionNumberOverlayLabel.Content = App.AssemblyVersionNumber;

		Timer.PlayPauseChanged += UpdatePlayPauseButtonState;
		Timer.PlayPauseChanged += UpdateTimerPlayPausedIndicator;
		Timer.Restarted += UpdateTimerIndicatorTick;
		Timer.VisualUpdate += UpdateTimerIndicatorTick;
		Timer.Elapsed += TryMoveNext;
		Timer.DurationChanged += UpdateTimerDurationIndicators;
		Timer.SetDuration(TimeSpan.FromSeconds(30));

		(int duration, string text)[] durationMenuItems = {
			(15, "15 seconds"),
			(30, "30 seconds"),
			(45, "45 seconds"),
			(60, "1 minute"),
			(2 * 60, "2 minutes"),
			(5 * 60, "5 minutes"),
			(10 * 60, "10 minutes"),
		};

		Circulator.OnCurrentNumberChanged += UpdateCurrentImage;

		Toaster = new(ToastLabel);
		ImageView = new(MainImageView, ImageFileNameLabel);
		ImageView.HandleClipboardToast(Toaster.Toast);

		soundPlayer.LoadAsync();
		Circulator.OnCurrentNumberChanged += PlaySound;
		Timer.PlayPauseChanged += PlaySound;
		Timer.DurationChanged += PlaySound;

		TimeBar.Maximum = TimerIndicatorMaximum;
		TimeBarCollapsed.Maximum = TimerIndicatorMaximum;

		foreach (var element in TooltipElements)
		{
			element.MakeTooltipImmediate();
		}

		SetSoundActive(true);

		HideBottomBarMiniButton.MouseDown += (_, _) => SetBottomBarActive(false);
		HelpOverlayPanel.MouseDown += (_, _) => SetHelpPanelActive(false);

		SoundMiniButton.Click += (_, _) => TryToggleSound();
		AlwaysOnTopToggle.Click += (_, _) => TryToggleAlwaysOnTop();		
		AboutButton.Click += (_, _) => TryOpenAboutWindow();
		HelpButton.Click += (_, _) => TryToggleHelpPanel();

		TimerSettingsUI.InitializeMenuChoices(SettingsButton, durationMenuItems, SetTimerDurationSeconds, () => SetEditableTimerPanelActive(true));
		NonEditableTimerLabel.MouseDown += (_, mouseEvent) => {
			mouseEvent.Handled = true;
			SetEditableTimerPanelActive(true);
		};
		SecondsInputTextbox.LostFocus += (_, _) => SetEditableTimerPanelActive(false);
		SecondsInputTextbox.KeyDown += (_, keyEvent) => {
			var key = keyEvent.Key;
			if (key is Key.Escape)
			{
				keyEvent.Handled = true;
				SecondsInputTextbox_Cancel();
			}
			else if (key is Key.Enter)
			{
				keyEvent.Handled = true;
				SecondsInputTextBox_Enter();
			}
		};
		this.MouseDown += (_, _) => SecondsInputTextbox_Cancel();

		OpenFolderButton.MakeTooltipImmediate();
		OpenFolderButton.SetTooltipPlacement(PlacementMode.Top);

		UpdatePlayPauseButtonState();
		UpdateTimerDurationIndicators();
		UpdateInteractibleState();
	}

	// BUTTONS
	void OpenFolderCommand (object sender, RoutedEventArgs e) => UserOpenFiles();
	void RestartTimerCommand (object sender, RoutedEventArgs e) => TryRestartTimer();
	void PreviousImageCommand (object sender, RoutedEventArgs e) => TryMovePrevious();
	void PlayPauseCommand (object sender, RoutedEventArgs e) => DoIfImageSetIsLoaded(Timer.TogglePlayPause);
	void NextImageCommand (object sender, RoutedEventArgs e) => TryMoveNext();
	void ToggleBottomBarCommand (object sender, ExecutedRoutedEventArgs e) => TryToggleBottomBar();
	void ToggleHelpCommand (object sender, ExecutedRoutedEventArgs e) => TryToggleHelpPanel();
	void ExpandBottomBarButton_MouseDown (object sender, MouseButtonEventArgs e) => SetBottomBarActive(true);
	void DropTarget_DragEnter (object sender, DragEventArgs e) => SetDropHintOverlayActive(true);
	void DropTarget_DragLeave (object sender, DragEventArgs e) => SetDropHintOverlayActive(false);

	void TryToggleSound () => SetSoundActive(!IsSoundEnabled);
	void TryToggleBottomBar () => SetBottomBarActive(CollapsedBottomBar.Visibility == Visibility.Visible);
	void TryToggleHelpPanel () => SetHelpPanelActive(HelpOverlayPanel.Visibility == Visibility.Collapsed);
	void TryToggleAlwaysOnTop () => SetAlwaysOnTop(!IsAlwaysOnTop);

	void SetDropHintOverlayActive (bool active) => SetPanelActive(DropHintOverlay, active);
	void SetHelpPanelActive (bool active) => SetPanelActive(HelpOverlayPanel, active);
	void SetBottomBarActive (bool active) => SetCollapsiblePanelActive(ActiveBottomBar, CollapsedBottomBar, active);

	void DropFilesCommand (object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

			UserAppendFiles(files);
		}

		SetDropHintOverlayActive(false);
	}

	void SecondsInputTextBox_Enter ()
	{
		var userInputString = SecondsInputTextbox.Text;
		TimerSettingsUI.TrySetDuration(userInputString);
		SetEditableTimerPanelActive(false);
	}

	void SecondsInputTextbox_Cancel ()
	{
		var unchangedTimerValue = Timer.DurationSeconds;
		SecondsInputTextbox.Text = unchangedTimerValue.ToString();
		SetEditableTimerPanelActive(false);
	}

	void DoIfImageSetIsLoaded (Action action)
	{
		if (IsImageSetReady)
			action.Invoke();
	}

	void SetTimerDurationSeconds (int durationSeconds)
	{
		Timer.SetDuration(TimeSpan.FromSeconds(durationSeconds));
		Timer.Restart();
		SetEditableTimerPanelActive(false);
	}

	void TryMoveNext ()
	{
		DoIfImageSetIsLoaded(() => {
			Circulator.MoveNext();
			Timer.Restart();
		});
	}

	void TryRestartTimer ()
	{
		DoIfImageSetIsLoaded(() => {
			Timer.Restart();
			PlaySound();
		});
	}

	void TryMovePrevious ()
	{
		DoIfImageSetIsLoaded(() => {
			Circulator.MovePrevious();
			Timer.Restart();
		});
	}

	void TryOpenAboutWindow ()
	{
		// Don't cache. You can't reopen a window once it's closed by the user.
		var aboutWindow = new AboutWindow
		{
			Owner = this,
		};
		aboutWindow.ShowDialog();
	}

	void UserOpenFiles ()
	{
		if (FileUtilities.OpenPickerForImages(out IEnumerable<string> filePaths))
		{
			LoadFilesAndStartNewSet(filePaths);
		}
		UnfocusControls();
	}

	void UserAppendFiles (string[] files)
	{
		int filesAppended = FileList.Append(FileUtilities.EnumerateImages(files));
		Toaster.Toast($"{filesAppended} {(filesAppended == 1 ? "image" : "images")} added.");
		if (filesAppended <= 0) return;
		TryStartNewSet();
	}

	void LoadFilesAndStartNewSet (IEnumerable<string> filePaths)
	{
		var imagePaths = FileUtilities.EnumerateImages(filePaths);
		FileList.Load(imagePaths);
		TryStartNewSet();
	}

	void TryStartNewSet ()
	{
		Timer.Restart();
		SetHelpPanelActive(false);

		if (!FileList.IsPopulated)
		{
			Circulator.Clear();
			return;
		}

		Circulator.StartNewOrder(FileList.Count);
		Timer.IsActive = true;
		DismissTips();
		UpdateSettingsTextBlock();

		UpdateInteractibleState();
	}

	void UpdateTimerIndicatorTick ()
	{
		double timebarValue = Timer.FractionLeft * TimerIndicatorMaximum;
		TimeBar.Value = timebarValue;
		TimeBarCollapsed.Value = timebarValue;
	}

	void UpdateCurrentImage ()
	{
		int currentIndex = Circulator.CurrentIndex;
		string currentImagePath = FileList.Get(currentIndex);
		ImageView.SetImage(currentImagePath);
	}

	void UpdateSettingsTextBlock ()
	{
		int count = FileList.Count;
		if (count <= 0)
		{
			ImagesCountLabel.Content = "No images loaded";
			return;
		}

		ImagesCountLabel.Content = $"{count} images";
	}

	void UpdateTimerDurationIndicators ()
	{
		TimerSettingsUI.UpdateSelectedState(Timer.DurationSeconds);

		string secondsText = Timer.DurationSeconds.ToString();
		SecondsInputTextbox.Text = secondsText;
		NonEditableTimerLabel.Content = $"{secondsText} seconds each";

		UpdateTimerPlayPausedIndicator();
		UpdateSettingsTextBlock();
	}

	void SetEditableTimerPanelActive (bool active)
	{
		SetCollapsiblePanelActive(EditableTimerSettingsPanel, NonEditableTimerLabel, active);

		if (active)
		{
			SecondsInputTextbox.Focus();
			SecondsInputTextbox.SelectAll();
		}
		else
		{
			UnfocusControls();
		}
	}

	void UnfocusControls ()
	{
		FocusManager.SetFocusedElement(this, this);
	}

	void SetAlwaysOnTop (bool alwaysOnTop)
	{
		IsAlwaysOnTop = alwaysOnTop;
		UpdateAlwaysOnTopButtonState();
	}

	void DismissTips ()
	{
		SetHelpPanelActive(false);
		SetDropHintOverlayActive(false);
		SetPanelActive(StartingTip, false);
	}

	void UpdateAlwaysOnTopButtonState ()
	{
		const string PinnedSymbol = "\uE842";
		const string UnpinnedSymbol = "\uE718";
		AlwaysOnTopToggle.Content = IsAlwaysOnTop ? PinnedSymbol : UnpinnedSymbol;
	}

	void UpdateTimerPlayPausedIndicator ()
	{
		var color = Timer.IsActive ? GetBrush("TimeBarActiveColor") : GetBrush("TimeBarPausedColor");
		TimeBar.Foreground = color;
		TimeBarCollapsed.Foreground = color;
	}

	void UpdatePlayPauseButtonState ()
	{
		const string PlaySymbol = "\uE768";
		const string PauseSymbol = "\uE769";

		var backgroundBrush = Timer.IsActive ? GetBrush("Button.Active.Foreground") : GetBrush("Button.Paused.Foreground");
		string label = Timer.IsActive ? PauseSymbol : PlaySymbol;

		PlayPauseButton.Background = backgroundBrush;
		PlayPauseButton.Content = label;
	}

	void UpdateInteractibleState ()
	{
		foreach (var button in ImageSetButtons)
		{
			button.IsEnabled = IsImageSetReady;
		}

		SetDropHintOverlayActive(false);
	}

	void PlaySound ()
	{
		if (IsSoundEnabled) soundPlayer.Play();
	}

	void SetSoundActive (bool active)
	{
		IsSoundEnabled = active;
		UpdateSoundButtonState();
	}

	void UpdateSoundButtonState ()
	{
		const string MutedSymbol = "\uE74F";
		const string SoundEnabledSymbol = "\uE994";
		SoundMiniButton.Content = IsSoundEnabled ? SoundEnabledSymbol : MutedSymbol;
		
	}

}
