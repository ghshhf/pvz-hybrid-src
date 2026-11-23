using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSettingsEditor : IDisposable
{
	internal readonly string ModId;

	internal readonly string PackageSha256;

	internal readonly XWModSettingDefinition[] Items;

	internal string Fingerprint;

	private bool _disposed;

	public IReadOnlyList<XWModSettingDefinition> Definitions => Array.AsReadOnly(Items.Select(XWModSettingsService.Copy).ToArray());

	public IReadOnlyDictionary<string, object> SavedValues { get; private set; }

	public string Diagnostic { get; private set; }

	internal XWModSettingsService.PendingState Pending => XWModSettingsService.ComparePending(ModId, PackageSha256, Items, SavedValues, Diagnostic);

	public bool RestartRequired => Pending.RequiresRestart;

	internal XWModSettingsEditor(string id, string package, XWModSettingDefinition[] items)
	{
		ModId = id;
		PackageSha256 = package;
		Items = items;
		Reload();
	}

	internal void CheckAlive()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("XWModSettingsEditor");
		}
	}

	internal void Reload()
	{
		XWModSettingsService.Stored stored = XWModSettingsService.Read(ModId);
		Fingerprint = stored.Fingerprint;
		SavedValues = new ReadOnlyDictionary<string, object>(XWModSettingsService.Resolve(Items, stored, out var diagnostic));
		Diagnostic = diagnostic;
	}

	internal void AcceptSaved(Dictionary<string, object> values, string fingerprint)
	{
		SavedValues = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(values, StringComparer.Ordinal));
		Fingerprint = fingerprint;
		Diagnostic = "";
	}

	public Dictionary<string, object> GetDefaults()
	{
		return Items.Where((XWModSettingDefinition item) => !item.IsDisplay).ToDictionary<XWModSettingDefinition, string, object>((XWModSettingDefinition item) => item.Key, delegate(XWModSettingDefinition item)
		{
			XWModSettingsService.TryValue(item, item.Default, out var normalized);
			return normalized;
		}, StringComparer.Ordinal);
	}

	public bool TrySave(IReadOnlyDictionary<string, object> changes, out XWModSettingsSaveResult result)
	{
		return XWModSettingsService.Save(this, changes, out result);
	}

	public void Dispose()
	{
		_disposed = true;
	}
}
