using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModUpdateCheckRequest
{
	internal const string Endpoint = "https://api.pvzhe.com/check_update_mods";

	internal const int MaxSubmissionCount = 100;

	internal string Filter { get; }

	internal IReadOnlyList<XWOfficialModDescriptor> ReportedMods { get; }

	internal byte[] Body { get; }

	private XWModUpdateCheckRequest(string filter, List<XWOfficialModDescriptor> owners)
	{
		Filter = filter;
		ReportedMods = owners.AsReadOnly();
		using MemoryStream memoryStream = new MemoryStream();
		using (Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(memoryStream))
		{
			utf8JsonWriter.WriteStartObject();
			utf8JsonWriter.WriteString("filter", filter);
			utf8JsonWriter.WriteStartObject("mods");
			foreach (XWOfficialModDescriptor item in owners.DistinctBy((XWOfficialModDescriptor owner) => owner.SubmissionId))
			{
				utf8JsonWriter.WriteString(item.SubmissionId, item.CurrentVersion);
			}
			utf8JsonWriter.WriteEndObject();
			utf8JsonWriter.WriteEndObject();
		}
		Body = memoryStream.ToArray();
	}

	internal static IReadOnlyList<XWModUpdateCheckRequest> Create(IReadOnlyList<ModLoader.LoadedMod> loaded, string filter = "release")
	{
		string text = filter;
		if ((!(text == "release") && !(text == "all")) || 1 == 0)
		{
			throw new ArgumentException("筛选必须为 release 或 all。", "filter");
		}
		List<XWOfficialModDescriptor> list = new List<XWOfficialModDescriptor>();
		for (int i = 0; i < loaded.Count; i++)
		{
			ModLoader.LoadedMod loadedMod = loaded[i];
			string text2 = loadedMod.Manifest?.PostId;
			if (loadedMod.Applied)
			{
				XWModRuntimeSession current = XWModRuntimeSession.Current;
				if ((current == null || !current.Faults.ContainsKey(loadedMod.LoadedId)) && XWModManifest.IsValidPostId(text2) && XWModLoadPlanVersion.IsSemanticVersion(loadedMod.LoadedVersion))
				{
					list.Add(new XWOfficialModDescriptor(loadedMod, loadedMod.Manifest.Author, loadedMod.LoadedId, loadedMod.LoadedVersion, string.IsNullOrWhiteSpace(loadedMod.Manifest.Name) ? loadedMod.LoadedId : loadedMod.Manifest.Name, XWModRuntimeSession.Current?.GetUpdateLoadOrder(loadedMod.LoadedId, i) ?? i, text2));
				}
			}
		}
		List<Dictionary<string, List<XWOfficialModDescriptor>>> list2 = new List<Dictionary<string, List<XWOfficialModDescriptor>>>();
		foreach (IGrouping<(string, string), XWOfficialModDescriptor> group in from owner in list
			group owner by (SubmissionId: owner.SubmissionId, CurrentVersion: owner.CurrentVersion))
		{
			Dictionary<string, List<XWOfficialModDescriptor>> dictionary = list2.FirstOrDefault((Dictionary<string, List<XWOfficialModDescriptor>> batch) => batch.Count < 100 && !batch.ContainsKey(group.Key.Item1));
			if (dictionary == null)
			{
				list2.Add(dictionary = new Dictionary<string, List<XWOfficialModDescriptor>>());
			}
			dictionary.Add(group.Key.Item1, group.ToList());
		}
		return list2.Select((Dictionary<string, List<XWOfficialModDescriptor>> batch) => new XWModUpdateCheckRequest(filter, batch.Values.SelectMany((List<XWOfficialModDescriptor> result) => result).ToList())).ToArray();
	}

	internal bool ReportResponse(JsonElement root, XWModUpdateInbox inbox)
	{
		if (root.ValueKind != JsonValueKind.Object || !TryString(root, "filter", out var value) || value != Filter || !root.TryGetProperty("mods", out var value2) || value2.ValueKind != JsonValueKind.Object)
		{
			return false;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		foreach (JsonProperty item in value2.EnumerateObject())
		{
			if (!hashSet.Add(item.Name))
			{
				return false;
			}
		}
		foreach (IGrouping<string, XWOfficialModDescriptor> item2 in from owner in ReportedMods
			group owner by owner.SubmissionId)
		{
			XWModUpdateNotice notice = null;
			if (value2.TryGetProperty(item2.Key, out var value3))
			{
				if (value3.ValueKind != JsonValueKind.Object || !TryString(value3, "newVersion", out var value4) || !XWModLoadPlanVersion.IsSemanticVersion(value4) || !TryString(value3, "updateInfo", out var value5))
				{
					continue;
				}
				notice = new XWModUpdateNotice(value4, value5, "https://www.pvzhe.com/mods/" + item2.Key);
			}
			foreach (XWOfficialModDescriptor item3 in item2)
			{
				inbox.ReportOfficial(item3, notice);
			}
		}
		return true;
	}

	private static bool TryString(JsonElement item, string name, out string value)
	{
		value = null;
		if (!item.TryGetProperty(name, out var value2) || value2.ValueKind != JsonValueKind.String)
		{
			return false;
		}
		try
		{
			value = value2.GetString();
			return true;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}
}
