using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModLoadPlanVersion
{
	private sealed record VersionValue(BigInteger Major, BigInteger Minor, BigInteger Patch, string[] Prerelease);

	private static readonly Regex Pattern = new Regex("^(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*)?$", RegexOptions.CultureInvariant);

	public static bool IsValidRequirement(XWModDependency dependency)
	{
		if (dependency != null)
		{
			if (!string.IsNullOrWhiteSpace(dependency.VersionRange))
			{
				VersionValue minimum;
				VersionValue maximum;
				bool minInclusive;
				bool maxInclusive;
				if (string.IsNullOrWhiteSpace(dependency.Version))
				{
					return TryRange(dependency.VersionRange, out minimum, out maximum, out minInclusive, out maxInclusive);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool Matches(XWModDependency dependency, string actual)
	{
		if (!IsValidRequirement(dependency))
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(dependency.VersionRange))
		{
			if (!string.IsNullOrWhiteSpace(dependency.Version))
			{
				return string.Equals(dependency.Version.Trim(), actual?.Trim(), StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		if (!TryVersion(actual, out var result) || !TryRange(dependency.VersionRange, out var minimum, out var maximum, out var minInclusive, out var maxInclusive))
		{
			return false;
		}
		if (minimum == null || Compare(result, minimum) > 0 || (minInclusive && Compare(result, minimum) == 0))
		{
			if (!(maximum == null) && Compare(result, maximum) >= 0)
			{
				if (maxInclusive)
				{
					return Compare(result, maximum) == 0;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal static bool IsSemanticVersion(string input)
	{
		VersionValue result;
		if (!string.IsNullOrEmpty(input) && input == input.Trim())
		{
			return TryVersion(input, out result);
		}
		return false;
	}

	private static bool TryVersion(string input, out VersionValue result)
	{
		result = null;
		if (string.IsNullOrWhiteSpace(input) || input.Length > 256)
		{
			return false;
		}
		Match match = Pattern.Match(input.Trim());
		if (!match.Success)
		{
			return false;
		}
		string[] array = (match.Groups[4].Success ? match.Groups[4].Value.Split('.') : Array.Empty<string>());
		if (array.Any((string p) => p.Length > 1 && p[0] == '0' && p.All(char.IsAsciiDigit)))
		{
			return false;
		}
		result = new VersionValue(BigInteger.Parse(match.Groups[1].Value), BigInteger.Parse(match.Groups[2].Value), BigInteger.Parse(match.Groups[3].Value), array);
		return true;
	}

	private static bool TryRange(string text, out VersionValue minimum, out VersionValue maximum, out bool minInclusive, out bool maxInclusive)
	{
		minimum = (maximum = null);
		minInclusive = (maxInclusive = false);
		text = text?.Trim();
		if (text != null && text.Length >= 3 && text.Length <= 520 && (text[0] == '[' || text[0] == '('))
		{
			string text2 = text;
			if (text2[text2.Length - 1] != ']')
			{
				string text3 = text;
				if (text3[text3.Length - 1] != ')')
				{
					goto IL_0074;
				}
			}
			string text4 = text;
			string[] array = text4.Substring(1, text4.Length - 1 - 1).Split(',');
			if (array.Length != 2)
			{
				return false;
			}
			minInclusive = text[0] == '[';
			string text5 = text;
			maxInclusive = text5[text5.Length - 1] == ']';
			string text6 = array[0].Trim();
			string text7 = array[1].Trim();
			if (text6.Length == 0 && text7.Length == 0)
			{
				return false;
			}
			if (((text6.Length == 0) & minInclusive) || ((text7.Length == 0) & maxInclusive))
			{
				return false;
			}
			if ((text6.Length > 0 && !TryVersion(text6, out minimum)) || (text7.Length > 0 && !TryVersion(text7, out maximum)))
			{
				return false;
			}
			if (!(minimum == null) && !(maximum == null) && Compare(minimum, maximum) >= 0)
			{
				return (Compare(minimum, maximum) == 0) & minInclusive & maxInclusive;
			}
			return true;
		}
		goto IL_0074;
		IL_0074:
		return false;
	}

	private static int Compare(VersionValue a, VersionValue b)
	{
		int num = a.Major.CompareTo(b.Major);
		if (num != 0)
		{
			return num;
		}
		num = a.Minor.CompareTo(b.Minor);
		if (num != 0)
		{
			return num;
		}
		num = a.Patch.CompareTo(b.Patch);
		if (num != 0)
		{
			return num;
		}
		if (a.Prerelease.Length == 0 || b.Prerelease.Length == 0)
		{
			if (a.Prerelease.Length != b.Prerelease.Length)
			{
				if (a.Prerelease.Length != 0)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
		for (int i = 0; i < Math.Min(a.Prerelease.Length, b.Prerelease.Length); i++)
		{
			string text = a.Prerelease[i];
			string text2 = b.Prerelease[i];
			bool flag = text.All(char.IsAsciiDigit);
			bool flag2 = text2.All(char.IsAsciiDigit);
			num = ((flag && flag2) ? BigInteger.Parse(text).CompareTo(BigInteger.Parse(text2)) : ((flag == flag2) ? StringComparer.Ordinal.Compare(text, text2) : ((!flag) ? 1 : (-1))));
			if (num != 0)
			{
				return num;
			}
		}
		return a.Prerelease.Length.CompareTo(b.Prerelease.Length);
	}
}
