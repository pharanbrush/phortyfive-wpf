using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;


namespace PhortyFiveSeconds;
static internal class FileUtilities
{
	static readonly string[] AllowableImageTypes = { "png", "jpg", "gif" };

	static string ImageTypesFilter
	{
		get
		{
			StringBuilder sb = new();
			foreach (string type in AllowableImageTypes)
			{
				sb.Append($"*.{type};");
			}

			return sb.ToString();
		}
	}

	internal static bool OpenPickerForImages (out IEnumerable<string> outputFileNames)
	{
		var imageFilter = ImageTypesFilter;
		var openFileDialog = new OpenFileDialog
		{
			Multiselect = true,
			Filter = $"Image files ({imageFilter})|{imageFilter}|All files (*.*)|*.*",
		};

		bool userPressedOk = openFileDialog.ShowDialog() ?? false;

		outputFileNames = openFileDialog.FileNames;
		return userPressedOk;
	}

	public static IEnumerable<string> EnumerateImages (IEnumerable<string> filePaths)
	{
		foreach (var file in filePaths)
		{
			if (IsImage(file))
			{
				yield return file;
			}
		}
	}

	public static bool OpenPickerForImageFolder (out IEnumerable<string> outputFileNames)
	{
		using var dialog = new FolderBrowserDialog();
		var result = dialog.ShowDialog();
		bool userPressedOk = result is DialogResult.OK;
		outputFileNames = null;

		if (userPressedOk)
		{
			string folderPath = dialog.SelectedPath;
			outputFileNames = FileUtilities.EnumerateImagesFromFolder(folderPath);
		}

		return userPressedOk;
	}

	public static IEnumerable<string> EnumerateImagesFromFolder (string folderPath)
	{
		var files = Directory.GetFiles(folderPath);
		return EnumerateImages(files);
	}

	internal static bool IsImage (string fileName)
	{
		var extension = Path.GetExtension(fileName);
		foreach (var type in AllowableImageTypes)
		{
			if (extension.EndsWith(type, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}



}
