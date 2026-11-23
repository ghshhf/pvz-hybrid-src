using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWOfficialModUpdateRequest
{
	internal const string ModsHeader = "X-PVZHE-Client-Mods";

	internal const string CountHeader = "X-PVZHE-Client-Mods-Length";

	internal const int MaxHeaderLength = 512;

	internal string[] Headers { get; }

	internal string Mods { get; }

	internal int LoadedCount { get; }

	internal IReadOnlyList<XWOfficialModDescriptor> ReportedMods { get; }

	private XWOfficialModUpdateRequest(string[] headers, string mods, int count, IReadOnlyList<XWOfficialModDescriptor> reported)
	{
		Headers = headers;
		Mods = mods;
		LoadedCount = count;
		ReportedMods = reported;
	}

	internal static XWOfficialModUpdateRequest Capture(string[] headers)
	{
		return Create(headers, ModLoader.GetLoadedMods());
	}

	internal static XWOfficialModUpdateRequest Create(string[] headers, IReadOnlyList<ModLoader.LoadedMod> loaded)
	{
		ModLoader.LoadedMod[] array = loaded.Where(delegate(ModLoader.LoadedMod mod)
		{
			if (mod.Applied)
			{
				XWModRuntimeSession current = XWModRuntimeSession.Current;
				if (current == null)
				{
					return true;
				}
				return !current.Faults.ContainsKey(mod.LoadedId);
			}
			return false;
		}).ToArray();
		StringBuilder stringBuilder = new StringBuilder(512);
		List<XWOfficialModDescriptor> list = new List<XWOfficialModDescriptor>();
		for (int num = 0; num < array.Length; num++)
		{
			ModLoader.LoadedMod loadedMod = array[num];
			string text = loadedMod.Manifest?.Author;
			string loadedId = loadedMod.LoadedId;
			string loadedVersion = loadedMod.LoadedVersion;
			if (XWModMetadataValidation.TryValidate(text, loadedId, loadedVersion, out var _))
			{
				string text2 = Uri.EscapeDataString(text) + ":" + Uri.EscapeDataString(loadedId) + "@" + Uri.EscapeDataString(loadedVersion);
				int num2 = ((stringBuilder.Length != 0) ? 2 : 0);
				if (stringBuilder.Length + num2 + text2.Length > 512)
				{
					break;
				}
				if (num2 != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(text2);
				string displayName = (string.IsNullOrWhiteSpace(loadedMod.Manifest?.Name) ? loadedId : loadedMod.Manifest.Name);
				int loadOrder = XWModRuntimeSession.Current?.GetUpdateLoadOrder(loadedId, num) ?? num;
				list.Add(new XWOfficialModDescriptor(loadedMod, text, loadedId, loadedVersion, displayName, loadOrder));
			}
		}
		List<string> list2 = (headers ?? Array.Empty<string>()).Where((string header) => !IsModHeader(header)).ToList();
		string text3 = stringBuilder.ToString();
		list2.Add("X-PVZHE-Client-Mods:" + text3);
		list2.Add("X-PVZHE-Client-Mods-Length:" + array.Length.ToString(CultureInfo.InvariantCulture));
		return new XWOfficialModUpdateRequest(list2.ToArray(), text3, array.Length, list.AsReadOnly());
	}

	private static bool IsModHeader(string header)
	{
		int num = header?.IndexOf(':') ?? (-1);
		if (num < 0)
		{
			return false;
		}
		string text = header.Substring(0, num).Trim();
		if (!text.Equals("X-PVZHE-Client-Mods", StringComparison.OrdinalIgnoreCase))
		{
			return text.Equals("X-PVZHE-Client-Mods-Length", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	internal static bool IsValidVersion(string value)
	{
		return XWModMetadataValidation.IsValidVersion(value);
	}
}
