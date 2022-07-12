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
	readonly MenuItem alwaysOnTopMenuItem;
	readonly MenuItem soundMenuItem;

	bool IsSoundEnabled { get; set; } = true;
	bool IsImageSetReady => Circulator.IsPopulated;
	static Brush? GetBrush (string key) => Application.Current.Resources[key] as Brush;

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

	bool IsAlwaysOnTop
	{
		get => this.Topmost;
		set => this.Topmost = value;
	}

	public MainWindow ()
	{
		InitializeComponent();
		SetTimerSettingsPanelVisible(false);

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
		ExpandBottomBarButton.MakeTooltipImmediate();

		var aboutItem = new MenuItem { Header = "About...", };
		aboutItem.Click += (_, _) => TryOpenAboutWindow();

		alwaysOnTopMenuItem = new MenuItem { Header = "Always on top", };
		alwaysOnTopMenuItem.Click += (_, _) => TryToggleAlwaysOnTop();
		soundMenuItem = new MenuItem { Header = "Sounds", };
		soundMenuItem.Click += (_, _) => TryToggleSound();

		SetSoundActive(true);

		var hideBottomBarMenuItem = new MenuItem { Header = "Hide bottom bar (H)", };
		hideBottomBarMenuItem.Click += (_, _) => SetBottomBarActive(false);

		TimerSettingsUI.AddMenuItem(aboutItem);
		TimerSettingsUI.AddMenuItem(new Separator());
		TimerSettingsUI.AddMenuItem(alwaysOnTopMenuItem);
		TimerSettingsUI.AddMenuItem(soundMenuItem);
		TimerSettingsUI.AddMenuItem(hideBottomBarMenuItem);
		TimerSettingsUI.AddMenuItem(new Separator());

		TimerSettingsUI.InitializeMenuChoices(SettingsButton, durationMenuItems, SetTimerDurationSeconds, () => SetTimerSettingsPanelVisible(true));
		NonEditableTimerLabel.MouseDown += (_, mouseEvent) => {
			mouseEvent.Handled = true;
			SetTimerSettingsPanelVisible(true);
		};
		SecondsInputTextbox.LostFocus += (_, _) => SetTimerSettingsPanelVisible(false);
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

		foreach (var button in ImageSetButtons)
		{
			button.MakeTooltipImmediate();
		}

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
	void ToggleBottomBarCommand (object sender, ExecutedRoutedEventArgs e) => ToggleBottomBar();

	void DropPanel_Drop (object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

			UserAppendFiles(files);
		}
	}

	void SecondsInputTextBox_Enter ()
	{
		var userInputString = SecondsInputTextbox.Text;
		TimerSettingsUI.TrySetDuration(userInputString);
		SetTimerSettingsPanelVisible(false);
	}

	void SecondsInputTextbox_Cancel ()
	{
		var unchangedTimerValue = Timer.DurationSeconds;
		SecondsInputTextbox.Text = unchangedTimerValue.ToString();
		SetTimerSettingsPanelVisible(false);
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
		SetTimerSettingsPanelVisible(false);
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

	void SetBottomBarActive (bool active)
	{
		CollapsedBottomBar.Visibility = active ? Visibility.Collapsed : Visibility.Visible;
		ActiveBottomBar.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
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

	void SetTimerSettingsPanelVisible (bool visible)
	{
		var hide = Visibility.Collapsed;
		var show = Visibility.Visible;
		EditableTimerSettingsPanel.Visibility = visible ? show : hide;
		NonEditableTimerLabel.Visibility = visible ? hide : show;

		if (visible)
		{
			SecondsInputTextbox.Focus();
			SecondsInputTextbox.SelectAll();
		}
		else
		{
			SecondsInputTextbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}
	}

	void TryToggleAlwaysOnTop ()
	{
		TrySetAlwaysOnTop(!IsAlwaysOnTop);
	}

	void TrySetAlwaysOnTop (bool alwaysOnTop)
	{
		IsAlwaysOnTop = alwaysOnTop;
		alwaysOnTopMenuItem.IsChecked = IsAlwaysOnTop;
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
		if (IsSoundEnabled) soundPlayer.Play();
	}

	void TryToggleSound ()
	{
		SetSoundActive(!IsSoundEnabled);
	}

	void SetSoundActive (bool active)
	{
		IsSoundEnabled = active;
		soundMenuItem.IsChecked = IsSoundEnabled;
	}

	private void ExpandBottomBarButton_MouseDown (object sender, MouseButtonEventArgs e)
	{
		SetBottomBarActive(true);
	}

	void ToggleBottomBar ()
	{
		SetBottomBarActive(CollapsedBottomBar.Visibility == Visibility.Visible);
	}
}
