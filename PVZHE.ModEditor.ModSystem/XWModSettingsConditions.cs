using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModSettingsConditions
{
	internal static bool Validate(JsonElement? condition, IReadOnlyDictionary<string, XWModSettingDefinition> definitions)
	{
		if (!condition.HasValue)
		{
			return true;
		}
		JsonElement value = condition.Value;
		if (value.ValueKind != JsonValueKind.Object)
		{
			return false;
		}
		JsonProperty[] array = value.EnumerateObject().ToArray();
		bool flag = array.Length == 1;
		if (flag)
		{
			string name = array[0].Name;
			bool flag2 = ((name == "all" || name == "any") ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			if (array[0].Value.ValueKind == JsonValueKind.Array && array[0].Value.GetArrayLength() > 0)
			{
				return array[0].Value.EnumerateArray().All((JsonElement child) => ValidateComparison(child, definitions));
			}
			return false;
		}
		return ValidateComparison(value, definitions);
	}

	private static bool ValidateComparison(JsonElement node, IReadOnlyDictionary<string, XWModSettingDefinition> definitions)
	{
		if (node.ValueKind != JsonValueKind.Object)
		{
			return false;
		}
		if (!(from property in node.EnumerateObject()
			select property.Name).OrderBy<string, string>((string name) => name, StringComparer.Ordinal).SequenceEqual<string>(new string[3] { "key", "op", "value" }, StringComparer.Ordinal) || node.GetProperty("key").ValueKind != JsonValueKind.String || node.GetProperty("op").ValueKind != JsonValueKind.String || !definitions.TryGetValue(node.GetProperty("key").GetString(), out var value))
		{
			return false;
		}
		string text = node.GetProperty("op").GetString();
		JsonElement value2 = node.GetProperty("value");
		if ((!(text == "eq") && !(text == "neq")) || 1 == 0)
		{
			if (text == "contains" && value.Type == "multienum" && value2.ValueKind == JsonValueKind.String)
			{
				return value.Options.Any((XWModSettingOption option) => option.Value == value2.GetString());
			}
			return false;
		}
		object normalized;
		return XWModSettingsService.TryValue(value, value2, out normalized);
	}

	internal static bool Evaluate(JsonElement? condition, IReadOnlyDictionary<string, XWModSettingDefinition> definitions, IReadOnlyDictionary<string, object> values)
	{
		if (!condition.HasValue)
		{
			return true;
		}
		JsonElement value = condition.Value;
		if (value.TryGetProperty("all", out var value2))
		{
			return value2.EnumerateArray().All((JsonElement child) => Compare(child, definitions, values));
		}
		if (value.TryGetProperty("any", out var value3))
		{
			return value3.EnumerateArray().Any((JsonElement child) => Compare(child, definitions, values));
		}
		return Compare(value, definitions, values);
	}

	private static bool Compare(JsonElement node, IReadOnlyDictionary<string, XWModSettingDefinition> definitions, IReadOnlyDictionary<string, object> values)
	{
		string key = node.GetProperty("key").GetString();
		string text = node.GetProperty("op").GetString();
		JsonElement property = node.GetProperty("value");
		if (text == "contains")
		{
			return ((IReadOnlyList<string>)values[key]).Contains<string>(property.GetString(), StringComparer.Ordinal);
		}
		XWModSettingsService.TryValue(definitions[key], property, out var normalized);
		bool flag = XWModSettingsService.ValuesEqual(values[key], normalized);
		if (!(text == "eq"))
		{
			return !flag;
		}
		return flag;
	}

	internal static IEnumerable<string> Dependencies(JsonElement? condition)
	{
		if (!condition.HasValue)
		{
			return Array.Empty<string>();
		}
		JsonElement value = condition.Value;
		if (value.TryGetProperty("all", out var value2))
		{
			return from child in value2.EnumerateArray()
				select child.GetProperty("key").GetString();
		}
		if (value.TryGetProperty("any", out var value3))
		{
			return from child in value3.EnumerateArray()
				select child.GetProperty("key").GetString();
		}
		return new string[1] { value.GetProperty("key").GetString() };
	}

	internal static string Describe(JsonElement? condition, IReadOnlyDictionary<string, XWModSettingDefinition> definitions)
	{
		if (!condition.HasValue)
		{
			return "";
		}
		JsonElement value = condition.Value;
		if (value.TryGetProperty("all", out var value2))
		{
			return "全部满足：" + string.Join("；", from child in value2.EnumerateArray()
				select Describe(child, definitions));
		}
		if (value.TryGetProperty("any", out var value3))
		{
			return "任一满足：" + string.Join("；", from child in value3.EnumerateArray()
				select Describe(child, definitions));
		}
		XWModSettingDefinition item = definitions[value.GetProperty("key").GetString()];
		string text = value.GetProperty("op").GetString();
		string value4;
		if (text == "contains")
		{
			value4 = XWModSettingsDraft.OptionLabel(item, value.GetProperty("value").GetString());
		}
		else
		{
			XWModSettingsService.TryValue(item, value.GetProperty("value"), out var normalized);
			value4 = XWModSettingsDraft.FormatValue(item, normalized);
		}
		return $"“{XWModSettingsDraft.DisplayName(item)}”{((text == "contains") ? "包含" : ((text == "neq") ? "不为" : "为"))} {value4}";
	}
}
