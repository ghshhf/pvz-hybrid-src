using System;
using System.IO;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModStateFile
{
	internal static bool IsStorageError(Exception error)
	{
		if (error is IOException || error is InvalidDataException || error is UnauthorizedAccessException || error is JsonException || error is ArgumentException || error is InvalidOperationException || error is NotSupportedException)
		{
			return true;
		}
		return false;
	}

	internal static void Write(string path, byte[] bytes, bool overwrite = true)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
		string text = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			using (FileStream fileStream = new FileStream(text, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				fileStream.Write(bytes);
				fileStream.Flush(flushToDisk: true);
			}
			File.Move(text, path, overwrite);
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}
}
