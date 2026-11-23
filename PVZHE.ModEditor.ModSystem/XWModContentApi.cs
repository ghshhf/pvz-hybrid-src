using System;
using Godot;
using Godot.Collections;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModContentApi
{
	public string OwnerModId { get; }

	public XWModContentApi(string ownerModId)
	{
		if (string.IsNullOrWhiteSpace(ownerModId))
		{
			throw new ArgumentException("A Mod owner is required.", "ownerModId");
		}
		OwnerModId = ownerModId.Trim().ToLowerInvariant();
	}

	public bool Provide(string category, string key, Variant value, out string diagnostic)
	{
		return XWModRuntimeRegistry.Provide(OwnerModId, category, key, value, out diagnostic);
	}

	public bool Provide(string category, string key, Variant value)
	{
		string diagnostic;
		return Provide(category, key, value, out diagnostic);
	}

	public bool ProvideLevelCatalog(string key, LevelCatalogConfig catalog, out string diagnostic)
	{
		if (string.IsNullOrWhiteSpace(key) || !GodotObject.IsInstanceValid(catalog))
		{
			diagnostic = "关卡目录需要有效的键和资源。";
			return false;
		}
		Dictionary dictionary;
		try
		{
			dictionary = XWModContentCatalog.FromResource(catalog);
		}
		catch (Exception ex)
		{
			diagnostic = "关卡目录转换失败：" + ex.Message;
			return false;
		}
		return Provide("Level", key, dictionary, out diagnostic);
	}

	public bool Override(XWModContentId target, Variant value, out string diagnostic)
	{
		return XWModRuntimeRegistry.Override(OwnerModId, target, value, out diagnostic);
	}

	public bool Override(XWModContentId target, Variant value)
	{
		string diagnostic;
		return Override(target, value, out diagnostic);
	}

	public XWModContentId Identity(string category, string key)
	{
		return new XWModContentId(OwnerModId, category, key);
	}
}
