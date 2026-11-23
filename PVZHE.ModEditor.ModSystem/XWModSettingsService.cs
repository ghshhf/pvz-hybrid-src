using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModSettingsService
{
	internal sealed record PendingState(bool PackageChanged, bool ApplyFailed, IReadOnlyList<string> ChangedKeys, string Diagnostic)
	{
		internal bool RequiresRestart
		{
			get
			{
				if (!PackageChanged && !ApplyFailed)
				{
					return ChangedKeys.Count > 0;
				}
				return true;
			}
		}

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("PackageChanged = ");
			builder.Append(PackageChanged.ToString());
			builder.Append(", ApplyFailed = ");
			builder.Append(ApplyFailed.ToString());
			builder.Append(", ChangedKeys = ");
			builder.Append(ChangedKeys);
			builder.Append(", Diagnostic = ");
			builder.Append((object?)Diagnostic);
			return true;
		}
	}

	internal sealed record Stored(Dictionary<string, JsonElement> Values, string Fingerprint, string Diagnostic);

	private static readonly ConcurrentDictionary<string, XWModSettings> Active = new ConcurrentDictionary<string, XWModSettings>(StringComparer.Ordinal);

	private static bool _saving;

	internal static string Owner(string id)
	{
		return (id ?? "").Trim().ToLowerInvariant();
	}

	internal static string Hash(byte[] bytes)
	{
		return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
	}

	internal static string StoragePath(string id)
	{
		return ProjectSettings.GlobalizePath("user://ModSettings/" + Hash(Encoding.UTF8.GetBytes(Owner(id))) + ".json");
	}

	public static bool ValidateDefinitions(XWModManifest manifest, out string diagnostic)
	{
		diagnostic = "";
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		bool flag;
		foreach (XWModSettingDefinition item in manifest?.Settings ?? new List<XWModSettingDefinition>())
		{
			string text;
			if (item == null)
			{
				text = "配置项不能为空。";
			}
			else
			{
				string text2;
				if (string.IsNullOrWhiteSpace(item.Key) || item.Key != item.Key.Trim() || !hashSet.Add(item.Key))
				{
					text2 = "配置键为空、含首尾空白或重复。";
				}
				else
				{
					switch (item.Type)
					{
					case "bool":
					case "enum":
					case "text":
					case "color":
					case "float":
					case "int":
					case "string":
					case "multienum":
					case "richtext":
						flag = true;
						break;
					default:
						flag = false;
						break;
					}
					string text3;
					if (!flag)
					{
						text3 = "不支持的配置类型。";
					}
					else
					{
						bool flag2 = !item.IsDisplay;
						if (flag2)
						{
							string applyMode = item.ApplyMode;
							bool flag3 = ((applyMode == "immediate" || applyMode == "restart") ? true : false);
							flag2 = !flag3;
						}
						text3 = (flag2 ? "applyMode 必须是 immediate 或 restart。" : "");
					}
					text2 = text3;
				}
				text = text2;
			}
			string text4 = text;
			if (text4 == "" && item.IsDisplay)
			{
				text4 = ((item.Content == null) ? "展示项必须提供字符串 content。" : (item.EnabledWhen.HasValue ? "展示项不支持 enabledWhen，请使用 visibleWhen。" : ""));
				if (text4 == "")
				{
					continue;
				}
			}
			if (text4 == "")
			{
				double? min = item.Min;
				if (min.HasValue)
				{
					double valueOrDefault = min.GetValueOrDefault();
					if (!double.IsFinite(valueOrDefault))
					{
						goto IL_0345;
					}
				}
				min = item.Max;
				if (min.HasValue)
				{
					double valueOrDefault2 = min.GetValueOrDefault();
					if (!double.IsFinite(valueOrDefault2))
					{
						goto IL_0345;
					}
				}
				min = item.Step;
				if (min.HasValue)
				{
					double valueOrDefault3 = min.GetValueOrDefault();
					if (!double.IsFinite(valueOrDefault3) || valueOrDefault3 <= 0.0)
					{
						goto IL_0345;
					}
				}
				if (item.Min.HasValue && item.Max.HasValue && item.Min > item.Max)
				{
					goto IL_0345;
				}
			}
			goto IL_034c;
			IL_034c:
			if (text4 == "" && item.Type == "int" && new double?[3] { item.Min, item.Max, item.Step }.Any((double? value) => value.HasValue && (value.Value != Math.Truncate(value.Value) || value.Value < -2147483648.0 || value.Value > 2147483647.0)))
			{
				text4 = "整数范围和步长必须是有效整数。";
			}
			if (text4 == "")
			{
				List<XWModSettingOption> options = item.Options;
				if (options != null && options.Any((XWModSettingOption option) => option == null || option.Value == null))
				{
					text4 = "选项包含无效值。";
				}
			}
			flag = text4 == "";
			if (flag)
			{
				text = item.Type;
				bool flag2 = ((text == "enum" || text == "multienum") ? true : false);
				flag = flag2;
			}
			if (flag && (item.Options == null || item.Options.Count == 0 || item.Options.Select((XWModSettingOption option) => option.Value).Distinct<string>(StringComparer.Ordinal).Count() != item.Options.Count))
			{
				text4 = "选项必须包含不重复的有效值。";
			}
			if (text4 == "" && item.Type == "multienum" && (item.MinSelected.GetValueOrDefault() < 0 || (item.MaxSelected ?? item.Options.Count) > item.Options.Count || item.MinSelected.GetValueOrDefault() > (item.MaxSelected ?? item.Options.Count)))
			{
				text4 = "选择数量必须满足 0 ≤ 最少数量 ≤ 最多数量 ≤ 选项数。";
			}
			if (text4 == "" && !TryValue(item, item.Default, out var _))
			{
				text4 = "默认值与类型、范围或选项不匹配。";
			}
			if (!(text4 != ""))
			{
				continue;
			}
			diagnostic = $"Mod '{manifest?.Id}' 配置 '{item?.Key}'：{text4}";
			flag = false;
			goto IL_0709;
			IL_0345:
			text4 = "数值范围或步长无效。";
			goto IL_034c;
		}
		Dictionary<string, XWModSettingDefinition> dictionary = (manifest?.Settings ?? new List<XWModSettingDefinition>()).ToDictionary<XWModSettingDefinition, string>((XWModSettingDefinition item) => item.Key, StringComparer.Ordinal);
		foreach (XWModSettingDefinition value in dictionary.Values)
		{
			if (XWModSettingsConditions.Validate(value.VisibleWhen, dictionary) && XWModSettingsConditions.Validate(value.EnabledWhen, dictionary))
			{
				continue;
			}
			diagnostic = $"Mod '{manifest.Id}' 配置 '{value.Key}'：条件格式、引用或比较值无效。";
			flag = false;
			goto IL_0709;
		}
		return true;
		IL_0709:
		return flag;
	}

	internal static XWModSettingDefinition Copy(XWModSettingDefinition item)
	{
		return new XWModSettingDefinition
		{
			Key = item.Key,
			Type = item.Type,
			Label = item.Label,
			Description = item.Description,
			Content = item.Content,
			Group = item.Group,
			VisibleWhen = item.VisibleWhen?.Clone(),
			EnabledWhen = item.EnabledWhen?.Clone(),
			Default = ((item.Default.ValueKind == JsonValueKind.Undefined) ? default(JsonElement) : item.Default.Clone()),
			Min = item.Min,
			Max = item.Max,
			Step = item.Step,
			MinSelected = item.MinSelected,
			MaxSelected = item.MaxSelected,
			ApplyMode = item.ApplyMode,
			AffectsGameplay = item.AffectsGameplay,
			Options = (item.IsDisplay ? new List<XWModSettingOption>() : (item.Options?.Select((XWModSettingOption option) => new XWModSettingOption
			{
				Value = option.Value,
				Label = option.Label
			}).ToList() ?? new List<XWModSettingOption>()))
		};
	}

	internal static bool TryValue(XWModSettingDefinition item, object value, out object normalized)
	{
		normalized = null;
		if (value is JsonElement jsonElement)
		{
			object obj;
			switch (jsonElement.ValueKind)
			{
			case JsonValueKind.True:
				obj = true;
				break;
			case JsonValueKind.False:
				obj = false;
				break;
			case JsonValueKind.String:
				obj = jsonElement.GetString();
				break;
			case JsonValueKind.Number:
			{
				if (jsonElement.TryGetDouble(out var value2))
				{
					obj = value2;
					break;
				}
				goto default;
			}
			case JsonValueKind.Array:
				if (item.Type == "multienum" && jsonElement.EnumerateArray().All((JsonElement element) => element.ValueKind == JsonValueKind.String))
				{
					obj = (from element in jsonElement.EnumerateArray()
						select element.GetString()).ToArray();
					break;
				}
				goto default;
			default:
				obj = null;
				break;
			}
			value = obj;
		}
		string type;
		if (item.Type == "bool" && value is bool flag)
		{
			normalized = flag;
		}
		else if (item.Type == "color")
		{
			if (value is string text && (text.Length == 7 || text.Length == 9) && text[0] == '#' && text.Skip(1).All(Uri.IsHexDigit))
			{
				value = new Color(text);
			}
			if (!(value is Color color) || !float.IsFinite(color.R) || !float.IsFinite(color.G) || !float.IsFinite(color.B) || !float.IsFinite(color.A) || color.R < 0f || color.R > 1f || color.G < 0f || color.G > 1f || color.B < 0f || color.B > 1f || color.A < 0f || color.A > 1f)
			{
				return false;
			}
			normalized = new Color("#" + color.ToHtml());
		}
		else
		{
			if (!(item.Type == "multienum") || !(value is IReadOnlyCollection<string> source))
			{
				type = item.Type;
				if ((type == "string" || type == "enum") ? true : false)
				{
					string text2 = value as string;
					if (text2 != null)
					{
						if (!(item.Type == "string"))
						{
							List<XWModSettingOption> options = item.Options;
							if (options == null || !options.Any((XWModSettingOption option) => option.Value == text2))
							{
								goto IL_0429;
							}
						}
						normalized = text2;
						goto IL_057c;
					}
				}
				goto IL_0429;
			}
			if (source.Any(delegate(string entry)
			{
				if (entry != null)
				{
					List<XWModSettingOption> options2 = item.Options;
					if (options2 == null)
					{
						return true;
					}
					return !options2.Any((XWModSettingOption option) => option.Value == entry);
				}
				return true;
			}))
			{
				return false;
			}
			string[] array = source.Distinct<string>(StringComparer.Ordinal).OrderBy<string, string>((string entry) => entry, StringComparer.Ordinal).ToArray();
			if (array.Length < item.MinSelected.GetValueOrDefault() || array.Length > (item.MaxSelected ?? item.Options.Count))
			{
				return false;
			}
			normalized = Array.AsReadOnly(array);
		}
		goto IL_057c;
		IL_057c:
		return normalized != null;
		IL_0429:
		type = item.Type;
		bool flag2 = ((type == "int" || type == "float") ? true : false);
		bool flag3 = flag2;
		if (flag3)
		{
			bool flag4 = ((value is int || value is long || value is float || value is double || value is decimal) ? true : false);
			flag3 = flag4;
		}
		if (flag3)
		{
			double num = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			if (!double.IsFinite(num) || (item.Min.HasValue && num < item.Min.Value) || (item.Max.HasValue && num > item.Max.Value))
			{
				return false;
			}
			if (item.Type == "int")
			{
				if (num < -2147483648.0 || num > 2147483647.0 || num != Math.Truncate(num))
				{
					return false;
				}
				normalized = (int)num;
			}
			else
			{
				normalized = ((num == 0.0) ? 0.0 : num);
			}
		}
		goto IL_057c;
	}

	internal static bool ValuesEqual(object left, object right)
	{
		if (!(left is IReadOnlyList<string> first) || !(right is IReadOnlyList<string> second))
		{
			return object.Equals(left, right);
		}
		return first.SequenceEqual<string>(second, StringComparer.Ordinal);
	}

	public static XWModSettingsEditor OpenEditor(XWModManifest manifest, string packageSha256)
	{
		if (string.IsNullOrWhiteSpace(manifest?.Id))
		{
			throw new ArgumentException("配置必须属于有效 Mod ID。");
		}
		if (!ValidateDefinitions(manifest, out var diagnostic))
		{
			throw new ArgumentException(diagnostic);
		}
		return new XWModSettingsEditor(Owner(manifest.Id), packageSha256 ?? "", (manifest.Settings ?? new List<XWModSettingDefinition>()).Select(Copy).ToArray());
	}

	internal static XWModSettings Activate(XWModManifest manifest, string packageSha256)
	{
		RequireMainThread();
		using (XWModEnvironmentService.TryBeginEnvironmentMutation() ?? throw new InvalidOperationException("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。"))
		{
			using XWModSettingsEditor xWModSettingsEditor = OpenEditor(manifest, packageSha256);
			Deactivate(manifest.Id);
			XWModSettings xWModSettings = new XWModSettings(xWModSettingsEditor);
			Active[xWModSettingsEditor.ModId] = xWModSettings;
			return xWModSettings;
		}
	}

	internal static XWModSettings GetActive(string id)
	{
		return Active.GetValueOrDefault(Owner(id));
	}

	internal static PendingState ComparePending(string id, string package, XWModSettingDefinition[] items, IReadOnlyDictionary<string, object> saved, string diagnostic)
	{
		XWModSettings active = GetActive(id);
		if (!items.Any((XWModSettingDefinition item) => !item.IsDisplay) || active == null)
		{
			return new PendingState(PackageChanged: false, ApplyFailed: false, Array.Empty<string>(), diagnostic);
		}
		if (string.IsNullOrWhiteSpace(package))
		{
			return new PendingState(PackageChanged: false, active.ApplyFailed, Array.Empty<string>(), "无法确认安装包哈希，请刷新后重试。" + diagnostic);
		}
		bool num = !string.Equals(active.PackageSha256, package, StringComparison.OrdinalIgnoreCase);
		object value;
		return new PendingState(ChangedKeys: Array.AsReadOnly((num || !string.IsNullOrEmpty(diagnostic)) ? Array.Empty<string>() : (from pair in saved
			where !active.Values.TryGetValue(pair.Key, out value) || !ValuesEqual(value, pair.Value)
			select pair.Key).ToArray()), PackageChanged: num, ApplyFailed: active.ApplyFailed, Diagnostic: diagnostic);
	}

	internal static PendingState ReadPendingState(XWModManifest manifest, string package)
	{
		if (manifest?.Settings == null || manifest.Settings.All((XWModSettingDefinition item) => item?.IsDisplay ?? false))
		{
			return new PendingState(PackageChanged: false, ApplyFailed: false, Array.Empty<string>(), "");
		}
		try
		{
			using XWModSettingsEditor xWModSettingsEditor = OpenEditor(manifest, package);
			return xWModSettingsEditor.Pending;
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is InvalidOperationException) ? 1 : 0) != 0)
		{
			return new PendingState(PackageChanged: false, GetActive(manifest.Id)?.ApplyFailed ?? false, Array.Empty<string>(), "无法读取配置，尚不能确认待生效状态：" + ex.GetBaseException().Message);
		}
	}

	internal static void Deactivate(string id)
	{
		RequireMainThread();
		if (Active.TryRemove(Owner(id), out var value))
		{
			value.Release();
		}
	}

	internal static void Deactivate(XWModSettings expected)
	{
		RequireMainThread();
		if (GetActive(expected.ModId) == expected)
		{
			Active.TryRemove(expected.ModId, out var _);
		}
		expected.Release();
	}

	public static string GetGameplayHash(string id)
	{
		return GetActive(id)?.GameplayHash ?? Hash(Encoding.UTF8.GetBytes("{}"));
	}

	public static bool HasApplyFailure(string id)
	{
		return GetActive(id)?.ApplyFailed ?? false;
	}

	internal static void RequireMainThread()
	{
		if (OS.GetThreadCallerId() != OS.GetMainThreadId())
		{
			throw new InvalidOperationException("Mod 配置写入必须在游戏主线程执行。");
		}
	}

	internal static Stored Read(string id)
	{
		string path = StoragePath(id);
		if (!File.Exists(path))
		{
			return new Stored(new Dictionary<string, JsonElement>(StringComparer.Ordinal), "missing", "");
		}
		byte[] array = File.ReadAllBytes(path);
		string fingerprint = Hash(array);
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(array);
			JsonElement rootElement = jsonDocument.RootElement;
			if (rootElement.GetProperty("version").GetInt32() != 1 || rootElement.GetProperty("modId").GetString() != id)
			{
				throw new InvalidDataException("配置文件身份或版本不匹配。");
			}
			Dictionary<string, JsonElement> dictionary = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
			foreach (JsonProperty item in rootElement.GetProperty("values").EnumerateObject())
			{
				dictionary.Add(item.Name, item.Value.Clone());
			}
			return new Stored(dictionary, fingerprint, "");
		}
		catch (Exception ex) when (((ex is JsonException || ex is InvalidDataException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException || ex is FormatException || ex is OverflowException) ? 1 : 0) != 0)
		{
			return new Stored(new Dictionary<string, JsonElement>(StringComparer.Ordinal), fingerprint, "配置文件损坏，当前使用默认值；保存时将备份原文件。");
		}
	}

	internal static Dictionary<string, object> Resolve(XWModSettingDefinition[] definitions, Stored stored, out string diagnostic)
	{
		diagnostic = stored.Diagnostic;
		Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
		foreach (XWModSettingDefinition xWModSettingDefinition in definitions)
		{
			if (!xWModSettingDefinition.IsDisplay)
			{
				TryValue(xWModSettingDefinition, xWModSettingDefinition.Default, out var normalized);
				object normalized2 = normalized;
				if (stored.Values.TryGetValue(xWModSettingDefinition.Key, out var value) && !TryValue(xWModSettingDefinition, value, out normalized2))
				{
					normalized2 = normalized;
					diagnostic = "部分历史配置值不再符合声明，当前使用对应默认值。";
				}
				dictionary.Add(xWModSettingDefinition.Key, normalized2);
			}
		}
		return dictionary;
	}

	internal static byte[] SerializeValues(IReadOnlyDictionary<string, object> values)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(memoryStream))
		{
			utf8JsonWriter.WriteStartObject();
			foreach (KeyValuePair<string, object> item in values.OrderBy<KeyValuePair<string, object>, string>((KeyValuePair<string, object> pair) => pair.Key, StringComparer.Ordinal))
			{
				utf8JsonWriter.WritePropertyName(item.Key);
				WriteValue(utf8JsonWriter, item.Value);
			}
			utf8JsonWriter.WriteEndObject();
		}
		return memoryStream.ToArray();
	}

	private static void WriteValue(Utf8JsonWriter writer, object value)
	{
		if (!(value is bool value2))
		{
			if (!(value is int value3))
			{
				if (!(value is double value4))
				{
					if (!(value is string value5))
					{
						if (!(value is Color color))
						{
							if (!(value is IReadOnlyList<string> readOnlyList))
							{
								throw new InvalidDataException("不支持的配置值。");
							}
							writer.WriteStartArray();
							foreach (string item in readOnlyList)
							{
								writer.WriteStringValue(item);
							}
							writer.WriteEndArray();
						}
						else
						{
							writer.WriteStringValue("#" + color.ToHtml().ToLowerInvariant());
						}
					}
					else
					{
						writer.WriteStringValue(value5);
					}
				}
				else
				{
					writer.WriteNumberValue(value4);
				}
			}
			else
			{
				writer.WriteNumberValue(value3);
			}
		}
		else
		{
			writer.WriteBooleanValue(value2);
		}
	}

	private static string Write(string id, Stored stored, Dictionary<string, object> values)
	{
		string text = StoragePath(id);
		Directory.CreateDirectory(Path.GetDirectoryName(text));
		using (new FileStream(text + ".lock", FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.None))
		{
			string text2 = text + "." + Guid.NewGuid().ToString("N") + ".tmp";
			try
			{
				using (FileStream fileStream = new FileStream(text2, FileMode.CreateNew, System.IO.FileAccess.Write, FileShare.None))
				{
					using (Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(fileStream, new JsonWriterOptions
					{
						Indented = true
					}))
					{
						utf8JsonWriter.WriteStartObject();
						utf8JsonWriter.WriteNumber("version", 1);
						utf8JsonWriter.WriteString("modId", id);
						utf8JsonWriter.WriteStartObject("values");
						foreach (KeyValuePair<string, JsonElement> item in stored.Values.Where((KeyValuePair<string, JsonElement> pair) => !values.ContainsKey(pair.Key)).OrderBy<KeyValuePair<string, JsonElement>, string>((KeyValuePair<string, JsonElement> pair) => pair.Key, StringComparer.Ordinal))
						{
							utf8JsonWriter.WritePropertyName(item.Key);
							item.Value.WriteTo(utf8JsonWriter);
						}
						foreach (KeyValuePair<string, object> item2 in values.OrderBy<KeyValuePair<string, object>, string>((KeyValuePair<string, object> pair) => pair.Key, StringComparer.Ordinal))
						{
							utf8JsonWriter.WritePropertyName(item2.Key);
							WriteValue(utf8JsonWriter, item2.Value);
						}
						utf8JsonWriter.WriteEndObject();
						utf8JsonWriter.WriteEndObject();
					}
					fileStream.Flush(flushToDisk: true);
				}
				if ((File.Exists(text) ? Hash(File.ReadAllBytes(text)) : "missing") != stored.Fingerprint)
				{
					throw new IOException("配置已被其他操作修改，请重新打开菜单。");
				}
				if (stored.Diagnostic != "" && File.Exists(text))
				{
					File.Copy(text, text + ".corrupt-" + Guid.NewGuid().ToString("N") + ".bak");
				}
				string result = Hash(File.ReadAllBytes(text2));
				File.Move(text2, text, overwrite: true);
				return result;
			}
			finally
			{
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
			}
		}
	}

	internal static bool Save(XWModSettingsEditor editor, IReadOnlyDictionary<string, object> changes, out XWModSettingsSaveResult result)
	{
		result = new XWModSettingsSaveResult("", RestartRequired: false);
		try
		{
			RequireMainThread();
		}
		catch (InvalidOperationException ex)
		{
			result = new XWModSettingsSaveResult(ex.Message, RestartRequired: false);
			return false;
		}
		if (_saving)
		{
			result = new XWModSettingsSaveResult("不能在配置变化通知中再次保存配置。", RestartRequired: false);
			return false;
		}
		_saving = true;
		try
		{
			editor.CheckAlive();
			if (changes == null)
			{
				throw new ArgumentException("缺少配置值。");
			}
			if (changes.Count == 0 && editor.Items.Length != 0 && editor.Items.All((XWModSettingDefinition item) => item.IsDisplay))
			{
				result = new XWModSettingsSaveResult("此页面仅包含只读内容，无需保存。", RestartRequired: false);
				return true;
			}
			Stored stored = Read(editor.ModId);
			if (stored.Fingerprint != editor.Fingerprint)
			{
				throw new IOException("配置已被其他操作修改，请重新打开菜单。");
			}
			string diagnostic;
			Dictionary<string, object> values = Resolve(editor.Items, stored, out diagnostic);
			foreach (KeyValuePair<string, object> pair in changes)
			{
				XWModSettingDefinition xWModSettingDefinition = editor.Items.FirstOrDefault((XWModSettingDefinition item) => item.Key == pair.Key);
				if (xWModSettingDefinition != null && xWModSettingDefinition.IsDisplay)
				{
					throw new ArgumentException("展示项 '" + pair.Key + "' 是只读内容，不能保存配置值。");
				}
				if (xWModSettingDefinition == null || !TryValue(xWModSettingDefinition, pair.Value, out var normalized))
				{
					throw new ArgumentException((xWModSettingDefinition?.Type == "multienum") ? $"配置 '{pair.Key}' 必须选择 {xWModSettingDefinition.MinSelected.GetValueOrDefault()} 至 {xWModSettingDefinition.MaxSelected ?? xWModSettingDefinition.Options.Count} 个有效选项。" : ("配置 '" + pair.Key + "' 的值无效。"));
				}
				values[pair.Key] = normalized;
			}
			XWModSettings active = GetActive(editor.ModId);
			object value;
			XWModSettingDefinition[] array = ((active != null && string.Equals(active.PackageSha256, editor.PackageSha256, StringComparison.OrdinalIgnoreCase)) ? active.Items.Where((XWModSettingDefinition item) => item.ApplyMode == "immediate" && values.ContainsKey(item.Key) && active.Values.TryGetValue(item.Key, out value) && !ValuesEqual(value, values[item.Key])).ToArray() : Array.Empty<XWModSettingDefinition>());
			bool flag = array.Any((XWModSettingDefinition item) => item.AffectsGameplay);
			using XWModEnvironmentService.EnvironmentMutation environmentMutation = (flag ? XWModEnvironmentService.TryBeginEnvironmentMutation() : null);
			if (flag && environmentMutation == null)
			{
				throw new InvalidOperationException("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			}
			string fingerprint = Write(editor.ModId, stored, values);
			editor.AcceptSaved(values, fingerprint);
			if (array.Length != 0)
			{
				active.Apply(values, array.Select((XWModSettingDefinition item) => item.Key).ToArray());
				if (flag || active.ApplyFailed)
				{
					XWModEnvironmentService.GameplaySettingsChanged(editor.ModId);
				}
			}
			bool restartRequired = editor.RestartRequired;
			XWModSettings xWModSettings = active;
			result = new XWModSettingsSaveResult((xWModSettings != null && xWModSettings.ApplyFailed) ? "配置已保存，但 Mod 应用配置时出错，请重启游戏。" : (restartRequired ? "配置已保存，部分设置重启游戏后生效。" : "配置已保存。"), restartRequired);
			return true;
		}
		catch (Exception ex2) when (((ex2 is IOException || ex2 is UnauthorizedAccessException || ex2 is ArgumentException || ex2 is InvalidOperationException) ? 1 : 0) != 0)
		{
			result = new XWModSettingsSaveResult(ex2.Message, RestartRequired: false);
			return false;
		}
		finally
		{
			_saving = false;
		}
	}
}
