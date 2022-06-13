using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;

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

	internal static IEnumerable<string> EnumerateImages (IEnumerable<string> filePaths)
	{
		foreach (var file in filePaths)
		{
			if (IsImage(file))
			{
				yield return file;
			}
		}
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
