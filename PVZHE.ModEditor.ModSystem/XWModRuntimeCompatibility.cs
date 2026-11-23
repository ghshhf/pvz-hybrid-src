using System;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModRuntimeCompatibility
{
	public static bool HasManagedCode(XWModManifest manifest)
	{
		if (manifest != null)
		{
			if (string.IsNullOrWhiteSpace(manifest.RuntimeAssembly))
			{
				return !string.IsNullOrWhiteSpace(manifest.RuntimeEntryType);
			}
			return true;
		}
		return false;
	}

	public static bool ValidatePackage(XWModManifest manifest, out string diagnostic)
	{
		diagnostic = "";
		if (manifest == null || manifest.SchemaVersion < 1 || manifest.SchemaVersion > 3)
		{
			diagnostic = $"Mod 清单缺失或 schemaVersion 不受支持：{manifest?.SchemaVersion}。";
			return false;
		}
		if (!XWModSettingsService.ValidateDefinitions(manifest, out diagnostic))
		{
			return false;
		}
		if (!HasManagedCode(manifest))
		{
			return true;
		}
		string text = ((manifest.SchemaVersion < 2 || manifest.SchemaVersion > 3) ? $"schemaVersion={manifest.SchemaVersion}, supported=2..{3}" : ((manifest.RuntimeApiVersion < 1 || manifest.RuntimeApiVersion > 3) ? $"runtimeApiVersion={manifest.RuntimeApiVersion}, supported=1..{3}" : (string.IsNullOrWhiteSpace(manifest.RuntimeAssembly) ? "runtimeEntryType requires runtimeAssembly" : ((!string.Equals(manifest.RuntimeAssemblyPolicy?.Trim(), "required", StringComparison.OrdinalIgnoreCase) && !string.Equals(manifest.RuntimeAssemblyPolicy?.Trim(), "optional", StringComparison.OrdinalIgnoreCase)) ? ("runtimeAssemblyPolicy='" + manifest.RuntimeAssemblyPolicy + "', expected=required|optional") : ""))));
		if (text.Length == 0)
		{
			return true;
		}
		diagnostic = $"Mod '{manifest.Id}' 托管兼容声明无效：{text}；请使用当前工具链重新编译并重新导出。";
		return false;
	}
}
