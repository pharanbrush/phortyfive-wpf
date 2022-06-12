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

	Action<string>? toastHandler;

	string LabelText
	{
		get => FileNameLabel.Content.ToString() ?? string.Empty;
		set => FileNameLabel.Content = value;
	}

	public WPFImageView (Image imageElement, Label fileNameLabel)
	{
		
		ImageElement = imageElement;
		ImageElement.MouseDown += (_,_) => CycleStretchMode();

		FileNameLabel = fileNameLabel;
		FileNameLabel.MouseDoubleClick += (_,_) => CopyFilenameToClipboard();
	}

	public void HandleClipboardToast (Action<string> toastHandler)
	{
		this.toastHandler = toastHandler;
	}

	void CopyFilenameToClipboard () {
		string labelText = this.LabelText;
		if (string.IsNullOrEmpty(labelText)) return;

		Clipboard.SetText(this.LabelText);

		toastHandler?.Invoke("Copied to clipboard.");
	}

	internal void SetImage (string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath)) return;

		ImageElement?.Dispatcher.Invoke(() => {
			ImageElement.Source = new BitmapImage(new Uri(filePath));
			ImageElement.Visibility = Visibility.Visible;
			LabelText = Path.GetFileName(filePath);
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
