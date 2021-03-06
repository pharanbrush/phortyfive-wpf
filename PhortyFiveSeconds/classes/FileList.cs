using System;
using System.Collections.Generic;
using System.IO;

namespace PhortyFiveSeconds;

internal class FileList
{
	struct FileData
	{
		public string fileName;
		public string filePath;
	}

	readonly List<FileData> files = new();

	public int Count => files.Count;
	public bool IsPopulated => files.Count > 0;
	public string First => Get(0);

	public string Get (int index)
	{
		if (files.Count <= 0) return string.Empty;
		if (index >= files.Count) index = 0;
		return files[index].filePath;
	}

	public void Load (IEnumerable<string> filePaths)
	{
		files.Clear();
		Append(filePaths);
	}

	public int Append (IEnumerable<string> filePaths)
	{
		int filesAppendedCount = 0;
		foreach (var filePath in filePaths)
		{
			
			files.Add(new FileData
			{
				fileName = Path.GetFileName(filePath),
				filePath = filePath,
			});
			filesAppendedCount++;
		}

		return filesAppendedCount;
	}
}
