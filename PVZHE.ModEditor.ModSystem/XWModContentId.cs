using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModContentId
{
	public string ProviderModId { get; }

	public string Category { get; }

	public string Key { get; }

	public bool IsBuiltIn => ProviderModId.Length == 0;

	public bool IsValid
	{
		get
		{
			if (Category.Length > 0 && Key.Length > 0 && !ProviderModId.Contains('/') && !ProviderModId.Contains('\\'))
			{
				return !Key.StartsWith("mod/", StringComparison.Ordinal);
			}
			return false;
		}
	}

	public string RuntimeKey
	{
		get
		{
			if (!IsBuiltIn)
			{
				return "mod/" + ProviderModId + "/" + Key;
			}
			return Key;
		}
	}

	public XWModContentId(string providerModId, string category, string key)
	{
		ProviderModId = (providerModId ?? "").Trim().ToLowerInvariant();
		Category = XWModRuntimeRegistry.NormalizeCategory((category ?? "").Trim());
		Key = (key ?? "").Trim();
	}

	public static bool TryCreate(string providerModId, string category, string key, out XWModContentId identity)
	{
		identity = new XWModContentId(providerModId, category, key);
		if (!identity.IsBuiltIn)
		{
			return identity.IsValid;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(IsBuiltIn ? "builtin" : ProviderModId)}:{Category}/{Key}";
	}
}
