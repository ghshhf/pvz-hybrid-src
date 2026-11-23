using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModMetadataValidation
{
	internal const int MaxFieldLength = 32;

	internal static bool IsValidAuthor(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		int num = 0;
		ReadOnlySpan<char> source = value.AsSpan();
		while (!source.IsEmpty)
		{
			if (Rune.DecodeFromUtf16(source, out var _, out var charsConsumed) != OperationStatus.Done || ++num > 32)
			{
				return false;
			}
			int num2 = charsConsumed;
			source = source.Slice(num2, source.Length - num2);
		}
		return true;
	}

	internal static bool IsValidModId(string value)
	{
		if (!string.IsNullOrEmpty(value) && value.Length <= 32)
		{
			char c = value[0];
			if (c < '0' || c > '9')
			{
				return value.All(delegate(char c2)
				{
					switch (c2)
					{
					case '-':
					case '.':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case 'A':
					case 'B':
					case 'C':
					case 'D':
					case 'E':
					case 'F':
					case 'G':
					case 'H':
					case 'I':
					case 'J':
					case 'K':
					case 'L':
					case 'M':
					case 'N':
					case 'O':
					case 'P':
					case 'Q':
					case 'R':
					case 'S':
					case 'T':
					case 'U':
					case 'V':
					case 'W':
					case 'X':
					case 'Y':
					case 'Z':
					case '_':
					case 'a':
					case 'b':
					case 'c':
					case 'd':
					case 'e':
					case 'f':
					case 'g':
					case 'h':
					case 'i':
					case 'j':
					case 'k':
					case 'l':
					case 'm':
					case 'n':
					case 'o':
					case 'p':
					case 'q':
					case 'r':
					case 's':
					case 't':
					case 'u':
					case 'v':
					case 'w':
					case 'x':
					case 'y':
					case 'z':
						return true;
					default:
						return false;
					}
				});
			}
		}
		return false;
	}

	internal static bool IsValidVersion(string value)
	{
		if (value != null)
		{
			int length = value.Length;
			if (length > 0 && length <= 32)
			{
				return XWModLoadPlanVersion.IsSemanticVersion(value);
			}
		}
		return false;
	}

	internal static bool TryValidate(string author, string modId, string version, out string diagnostic)
	{
		diagnostic = ((!IsValidAuthor(author)) ? "author（作者）必填，不能全为空白，且最多 32 个有效 Unicode 字符。" : ((!IsValidModId(modId)) ? "modid（id）必填，最多 32 个字符，仅允许 0-9、A-Z、a-z、_、.、-，且不能以数字开头。" : ((!IsValidVersion(version)) ? "version（版本）必填，最多 32 个字符，且必须符合 SemVer（例如 1.0.0 或 1.0.0-rc.1+build.1）。" : "")));
		return diagnostic.Length == 0;
	}

	internal static void RequireValid(string author, string modId, string version)
	{
		if (!TryValidate(author, modId, version, out var diagnostic))
		{
			throw new InvalidDataException(diagnostic);
		}
	}

	internal static bool TryGetJsonField(JsonElement root, string name, out JsonElement value)
	{
		value = default(JsonElement);
		bool result = false;
		if (root.ValueKind != JsonValueKind.Object)
		{
			return false;
		}
		foreach (JsonProperty item in root.EnumerateObject())
		{
			if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				value = item.Value;
				result = true;
			}
		}
		return result;
	}

	internal static string ReadJsonString(JsonElement root, string name)
	{
		if (!TryGetJsonField(root, name, out var value) || value.ValueKind != JsonValueKind.String)
		{
			return null;
		}
		try
		{
			return value.GetString();
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}
}
