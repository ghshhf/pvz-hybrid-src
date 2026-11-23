using System;
using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModRuntimeApi
{
	public const int CurrentVersion = 3;

	public static IReadOnlyList<int> SupportedVersions { get; } = Array.AsReadOnly(new int[3] { 1, 2, 3 });

	public static bool IsSupported(int version)
	{
		if (version >= 1)
		{
			return version <= 3;
		}
		return false;
	}
}
