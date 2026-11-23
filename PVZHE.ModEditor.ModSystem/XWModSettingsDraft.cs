using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModSettingsDraft
{
	internal readonly Dictionary<string, XWModSettingDefinition> Definitions;

	private readonly Dictionary<string, object> _values;

	private readonly Dictionary<string, object> _valid;

	private readonly Dictionary<string, object> _initial;

	private readonly Dictionary<string, object> _defaults;

	private readonly Dictionary<string, string> _errors = new Dictionary<string, string>(StringComparer.Ordinal);

	private readonly Dictionary<string, string> _displayText;

	internal IReadOnlyDictionary<string, object> Values => _values;

	internal IReadOnlyDictionary<string, string> Errors => _errors;

	internal int ChangedCount => Definitions.Keys.Count(IsChanged);

	internal bool HasChanges => ChangedCount > 0;

	internal bool NeedsRepair { get; }

	internal bool HasEditableItems => _values.Count > 0;

	internal bool CanSave
	{
		get
		{
			if (HasEditableItems)
			{
				if (!HasChanges)
				{
					return NeedsRepair;
				}
				return true;
			}
			return false;
		}
	}

	internal XWModSettingsDraft(XWModSettingsEditor editor)
	{
		Definitions = editor.Definitions.ToDictionary<XWModSettingDefinition, string>((XWModSettingDefinition item) => item.Key, StringComparer.Ordinal);
		_values = new Dictionary<string, object>(editor.SavedValues, StringComparer.Ordinal);
		_valid = new Dictionary<string, object>(_values, StringComparer.Ordinal);
		_initial = _values.ToDictionary<KeyValuePair<string, object>, string, object>((KeyValuePair<string, object> pair) => pair.Key, (KeyValuePair<string, object> pair) => Own(pair.Value), StringComparer.Ordinal);
		_defaults = editor.GetDefaults();
		_displayText = Definitions.Values.Where((XWModSettingDefinition item) => item.IsDisplay).ToDictionary<XWModSettingDefinition, string, string>((XWModSettingDefinition item) => item.Key, (XWModSettingDefinition item) => (!(item.Type == "richtext")) ? item.Content : XWModSettingsText.Parse(item.Content).PlainText, StringComparer.Ordinal);
		NeedsRepair = !string.IsNullOrWhiteSpace(editor.Diagnostic);
	}

	internal void Set(string key, object value)
	{
		XWModSettingDefinition xWModSettingDefinition = Definitions[key];
		if (xWModSettingDefinition.IsDisplay)
		{
			throw new ArgumentException("展示项 '" + key + "' 不可编辑。");
		}
		_values[key] = Own(value);
		if (XWModSettingsService.TryValue(xWModSettingDefinition, value, out var normalized))
		{
			_valid[key] = normalized;
			_values[key] = normalized;
			_errors.Remove(key);
			return;
		}
		Dictionary<string, string> errors = _errors;
		string value2;
		if (xWModSettingDefinition.Type == "color")
		{
			value2 = "请输入 #RRGGBB 或 #RRGGBBAA。";
		}
		else
		{
			string text;
			if (xWModSettingDefinition.Type == "multienum")
			{
				text = $"请选择 {xWModSettingDefinition.MinSelected.GetValueOrDefault()} 至 {xWModSettingDefinition.MaxSelected ?? xWModSettingDefinition.Options.Count} 项。";
			}
			else
			{
				string type = xWModSettingDefinition.Type;
				bool flag = ((type == "int" || type == "float") ? true : false);
				text = (flag ? NumericError(xWModSettingDefinition, value) : "输入值不符合类型或范围。");
			}
			value2 = text;
		}
		errors[key] = value2;
	}

	private static object Own(object value)
	{
		if (!(value is IReadOnlyCollection<string> source))
		{
			return value;
		}
		return Array.AsReadOnly(source.ToArray());
	}

	internal bool IsChanged(string key)
	{
		if (!Definitions[key].IsDisplay)
		{
			if (!_errors.ContainsKey(key))
			{
				return !XWModSettingsService.ValuesEqual(_values[key], _initial[key]);
			}
			return true;
		}
		return false;
	}

	internal bool IsDefault(string key)
	{
		if (!Definitions[key].IsDisplay)
		{
			if (!_errors.ContainsKey(key))
			{
				return XWModSettingsService.ValuesEqual(_values[key], _defaults[key]);
			}
			return false;
		}
		return true;
	}

	internal object Reset(string key)
	{
		Set(key, _defaults[key]);
		return _values[key];
	}

	internal string DefaultText(string key)
	{
		return FormatValue(Definitions[key], _defaults[key]);
	}

	internal string EnabledHint(string key)
	{
		return XWModSettingsConditions.Describe(Definitions[key].EnabledWhen, Definitions);
	}

	internal string ConditionHint(string key)
	{
		return string.Join("；", new string[2]
		{
			Visible(key) ? "" : ("显示条件：" + XWModSettingsConditions.Describe(Definitions[key].VisibleWhen, Definitions)),
			Enabled(key) ? "" : ("启用条件：" + EnabledHint(key))
		}.Where((string text) => text.Length > 0));
	}

	internal static string DisplayName(XWModSettingDefinition item)
	{
		if (!string.IsNullOrWhiteSpace(item.Label))
		{
			return item.Label;
		}
		return item.Key;
	}

	internal static string OptionLabel(XWModSettingDefinition item, string value)
	{
		XWModSettingOption xWModSettingOption = item.Options.Find((XWModSettingOption option) => option.Value == value);
		if (!string.IsNullOrWhiteSpace(xWModSettingOption?.Label))
		{
			return xWModSettingOption.Label;
		}
		return value;
	}

	internal static string FormatValue(XWModSettingDefinition item, object value)
	{
		if (!(value is bool))
		{
			if (!(value is Color color))
			{
				if (!(value is IReadOnlyList<string> readOnlyList))
				{
					if (!(value is string text))
					{
						if (value is double num)
						{
							return num.ToString("R", CultureInfo.CurrentCulture);
						}
						return Convert.ToString(value, CultureInfo.CurrentCulture);
					}
					return (item.Type == "enum") ? OptionLabel(item, text) : ((text.Length == 0) ? "空文本" : text);
				}
				return (readOnlyList.Count == 0) ? "未选择" : string.Join("、", readOnlyList.Select((string option) => OptionLabel(item, option)));
			}
			return "#" + color.ToHtml().ToLowerInvariant();
		}
		return ((bool)value) ? "开启" : "关闭";
	}

	internal static string NumericHint(XWModSettingDefinition item)
	{
		string value = ((item.Min.HasValue && item.Max.HasValue) ? (Number(item.Min.Value) + "～" + Number(item.Max.Value)) : (item.Min.HasValue ? ("不小于 " + Number(item.Min.Value)) : (item.Max.HasValue ? ("不大于 " + Number(item.Max.Value)) : "无声明范围限制")));
		return $"{((item.Type == "int") ? "整数" : "数值")}：{value} · 步长 {Number(item.Step ?? ((item.Type == "int") ? 1.0 : 0.01))}";
		static string Number(double num)
		{
			return num.ToString("R", CultureInfo.CurrentCulture);
		}
	}

	private static string NumericError(XWModSettingDefinition item, object value)
	{
		if (!double.TryParse(Convert.ToString(value, CultureInfo.CurrentCulture), NumberStyles.Float, CultureInfo.CurrentCulture, out var result) || !double.IsFinite(result))
		{
			if (!(item.Type == "int"))
			{
				return "请输入有限数值。";
			}
			return "请输入有效整数。";
		}
		if (item.Type == "int" && Math.Truncate(result) != result)
		{
			return "请输入整数，不能含小数。";
		}
		if (item.Type == "int" && (result < -2147483648.0 || result > 2147483647.0))
		{
			return $"整数必须在 {-2147483648}～{2147483647} 之间。";
		}
		return "超出允许范围。" + NumericHint(item);
	}

	internal bool Visible(string key)
	{
		return XWModSettingsConditions.Evaluate(Definitions[key].VisibleWhen, Definitions, _valid);
	}

	internal bool Enabled(string key)
	{
		return XWModSettingsConditions.Evaluate(Definitions[key].EnabledWhen, Definitions, _valid);
	}

	internal int MatchCount(string query, string group, bool changedOnly)
	{
		return Definitions.Keys.Count((string key) => Visible(key) && Matches(key, query, group, changedOnly));
	}

	internal bool Matches(string key, string query, string group, bool changedOnly = false)
	{
		XWModSettingDefinition xWModSettingDefinition = Definitions[key];
		if (changedOnly && !IsChanged(key))
		{
			return false;
		}
		if (group != null && (xWModSettingDefinition.Group ?? "") != group)
		{
			return false;
		}
		string text = string.Join(" ", new string[5]
		{
			xWModSettingDefinition.Key,
			xWModSettingDefinition.Label,
			xWModSettingDefinition.Description,
			xWModSettingDefinition.Group,
			_displayText.GetValueOrDefault(key, "")
		}.Concat(xWModSettingDefinition.Options.SelectMany((XWModSettingOption option) => new string[2] { option.Value, option.Label })));
		return (query ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).All((string term) => text.Contains(term, StringComparison.OrdinalIgnoreCase));
	}
}
