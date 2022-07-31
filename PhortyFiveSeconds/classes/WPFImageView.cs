using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PhortyFiveSeconds;

internal class WPFImageView
{
	readonly Image ImageElement;
	readonly Label FileNameLabel;
	readonly Label ImageErrorLabel;

	const string ImageUnsupportedText = "Image could not be loaded.";

	Action<string>? toastHandler;

	bool HasImage { get; set; } = false;

	string LabelText
	{
		get => FileNameLabel.Content is TextBlock textBlock ? textBlock.Text : string.Empty;

		set
		{
			if (FileNameLabel.Content is not TextBlock)
			{
				FileNameLabel.Content = new TextBlock();
			}

			if (FileNameLabel.Content is TextBlock textBlock)
			{
				textBlock.Text = value;
			}
		}
	}

	public WPFImageView (Image imageElement, Label fileNameLabel, Label errorLabel)
	{

		ImageElement = imageElement;
		ImageElement.Stretch = Stretch.Uniform;


		//ImageElement.MouseDown += (_, _) => CycleStretchMode();
		FileNameLabel = fileNameLabel;

		ImageErrorLabel = errorLabel;
		ImageErrorLabel.Visibility = Visibility.Collapsed;

		WindowUtilities.MakeTooltipImmediate(FileNameLabel);
		WindowUtilities.AllowRightClickCopy(FileNameLabel, "Copy filename");
		FileNameLabel.MouseDoubleClick += (_, _) => CopyFilenameToClipboard();


	}
	public void HandleClipboardToast (Action<string> toastHandler)
	{
		this.toastHandler = toastHandler;
	}

	void CopyFilenameToClipboard ()
	{
		WindowUtilities.CopyLabelTextToClipboard(FileNameLabel);
		toastHandler?.Invoke("Copied to clipboard.");
	}

	internal void SetImage (string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath)) return;

		ImageElement?.Dispatcher.Invoke(() => {
			try
			{
				var image = new BitmapImage(new Uri(filePath));
				ImageElement.Source = image;
				ImageElement.Visibility = Visibility.Visible;
				LabelText = Path.GetFileName(filePath);
				ImageErrorLabel.Visibility = Visibility.Collapsed;
				HasImage = true;
			}
			catch (NotSupportedException)
			{
				ImageElement.Source = null;
				ImageElement.Visibility = Visibility.Hidden;
				ImageErrorLabel.Visibility = Visibility.Visible;
				ImageErrorLabel.Content = ImageUnsupportedText;
				LabelText = Path.GetFileName(filePath);
				HasImage = false;
			}
		});
	}

	internal void CycleStretchMode ()
	{
		ImageElement.Stretch = ImageElement.Stretch switch
		{
			Stretch.None => Stretch.Uniform,
			Stretch.Uniform => Stretch.None,
			_ => Stretch.None,
		};
	}
}
