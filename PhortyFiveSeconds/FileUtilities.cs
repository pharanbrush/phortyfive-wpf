using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace PhortyFiveSeconds;
static internal class FileUtilities
{
	internal static IEnumerable<string> OpenFiles ()
	{
		var openFileDialog = new OpenFileDialog
		{
			Multiselect = true,
			Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*",
		};

		openFileDialog.ShowDialog();
		return openFileDialog.FileNames;
	}

}
