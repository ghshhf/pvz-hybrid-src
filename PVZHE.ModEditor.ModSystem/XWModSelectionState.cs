using System;
using System.Collections.Generic;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSelectionState
{
	public int SchemaVersion { get; set; } = 2;

	public List<string> EnabledIds { get; set; } = new List<string>();

	public List<string> PreferredOrder { get; set; } = new List<string>();

	public XWModSelectionState Normalize()
	{
		return new XWModSelectionState
		{
			EnabledIds = NormalizeIds(EnabledIds).OrderBy<string, string>((string id) => id, StringComparer.Ordinal).ToList(),
			PreferredOrder = NormalizeIds(PreferredOrder).ToList()
		};
	}

	public XWModSelectionState Clone()
	{
		return Normalize();
	}

	internal static string Owner(string id)
	{
		return (id ?? "").Trim().ToLowerInvariant();
	}

	private static IEnumerable<string> NormalizeIds(IEnumerable<string> ids)
	{
		return (from id in (ids ?? Array.Empty<string>()).Select(Owner)
			where id.Length > 0
			select id).Distinct<string>(StringComparer.Ordinal);
	}
}
