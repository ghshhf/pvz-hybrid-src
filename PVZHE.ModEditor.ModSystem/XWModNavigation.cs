using System;
using System.Collections.Generic;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModNavigation
{
	private static string _selectedModId = "";

	private static bool _returnRequested;

	internal static string FilterModId { get; private set; } = "";

	internal static bool FromManagement { get; private set; }

	internal static void BeginManagement(string filter, string selected)
	{
		FilterModId = filter ?? "";
		_selectedModId = selected ?? "";
		FromManagement = true;
		_returnRequested = false;
	}

	internal static void Clear()
	{
		FilterModId = "";
		_selectedModId = "";
		FromManagement = false;
		_returnRequested = false;
	}

	internal static void ShowAll()
	{
		FilterModId = "";
	}

	internal static void RequestReturn()
	{
		_returnRequested = FromManagement;
	}

	internal static bool TakeReturn(out string selected)
	{
		selected = _selectedModId;
		bool returnRequested = _returnRequested;
		Clear();
		return returnRequested;
	}

	internal static IReadOnlyList<XWModContentCatalog.Catalog> Catalogs(string modId)
	{
		return (from catalog in XWModContentCatalog.GetCatalogs()
			where string.IsNullOrEmpty(modId) || catalog.OwnerModId.Equals(modId, StringComparison.OrdinalIgnoreCase) || catalog.EffectiveOwnerModId.Equals(modId, StringComparison.OrdinalIgnoreCase)
			select catalog).DistinctBy((XWModContentCatalog.Catalog catalog) => (catalog.OwnerModId.ToLowerInvariant(), Key: catalog.Key)).ToArray();
	}
}
