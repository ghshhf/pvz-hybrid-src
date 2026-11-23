using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSettings
{
	private readonly string _id;

	internal readonly XWModSettingDefinition[] Items;

	internal readonly string PackageSha256;

	private bool _released;

	internal string ModId => _id;

	internal IReadOnlyDictionary<string, object> Values { get; private set; }

	internal string GameplayHash { get; private set; }

	internal bool ApplyFailed { get; private set; }

	public event Action<IReadOnlyList<string>> Changed;

	internal XWModSettings(XWModSettingsEditor editor)
	{
		_id = editor.ModId;
		PackageSha256 = editor.PackageSha256;
		Items = editor.Items.Select(XWModSettingsService.Copy).ToArray();
		Values = editor.SavedValues;
		UpdateHash();
	}

	public T Get<T>(string key)
	{
		if (_released)
		{
			throw new ObjectDisposedException("XWModSettings");
		}
		if (!Values.TryGetValue(key, out var value))
		{
			throw new KeyNotFoundException("未声明的 Mod 配置：" + key);
		}
		if (value is T)
		{
			return (T)value;
		}
		throw new InvalidCastException($"配置 '{key}' 不是 {typeof(T).Name}。");
	}

	public bool TrySave(IReadOnlyDictionary<string, object> changes, out XWModSettingsSaveResult result)
	{
		if (_released)
		{
			result = new XWModSettingsSaveResult("Mod 配置实例已卸载。", RestartRequired: false);
			return false;
		}
		try
		{
			using XWModSettingsEditor xWModSettingsEditor = new XWModSettingsEditor(_id, PackageSha256, Items.Select(XWModSettingsService.Copy).ToArray());
			return xWModSettingsEditor.TrySave(changes, out result);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			result = new XWModSettingsSaveResult(ex.Message, RestartRequired: false);
			return false;
		}
	}

	internal void Apply(Dictionary<string, object> values, string[] keys)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(Values, StringComparer.Ordinal);
		foreach (string key in keys)
		{
			dictionary[key] = values[key];
		}
		Values = new ReadOnlyDictionary<string, object>(dictionary);
		UpdateHash();
		if (this.Changed == null)
		{
			return;
		}
		Delegate[] invocationList = this.Changed.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action<IReadOnlyList<string>> action = (Action<IReadOnlyList<string>>)invocationList[i];
			try
			{
				action(Array.AsReadOnly(keys));
			}
			catch (Exception ex)
			{
				ApplyFailed = true;
				GD.PushWarning($"[Mod:{_id}] 配置已保存，但应用回调失败：{ex.GetBaseException().Message}；请重启游戏。");
			}
		}
	}

	private void UpdateHash()
	{
		GameplayHash = XWModSettingsService.Hash(XWModSettingsService.SerializeValues(Items.Where((XWModSettingDefinition item) => !item.IsDisplay && item.AffectsGameplay).ToDictionary<XWModSettingDefinition, string, object>((XWModSettingDefinition item) => item.Key, (XWModSettingDefinition item) => Values[item.Key], StringComparer.Ordinal)));
	}

	internal void Release()
	{
		_released = true;
		this.Changed = null;
	}
}
