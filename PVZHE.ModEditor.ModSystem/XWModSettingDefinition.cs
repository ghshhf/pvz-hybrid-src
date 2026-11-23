using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSettingDefinition
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = "";

	[JsonPropertyName("type")]
	public string Type { get; set; } = "bool";

	[JsonPropertyName("label")]
	public string Label { get; set; } = "";

	[JsonPropertyName("description")]
	public string Description { get; set; } = "";

	[JsonPropertyName("content")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string Content { get; set; }

	[JsonIgnore]
	internal bool IsDisplay
	{
		get
		{
			string type = Type;
			if (type == "text" || type == "richtext")
			{
				return true;
			}
			return false;
		}
	}

	[JsonPropertyName("group")]
	public string Group { get; set; } = "";

	[JsonPropertyName("visibleWhen")]
	public JsonElement? VisibleWhen { get; set; }

	[JsonPropertyName("enabledWhen")]
	public JsonElement? EnabledWhen { get; set; }

	[JsonPropertyName("default")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public JsonElement Default { get; set; }

	[JsonPropertyName("min")]
	public double? Min { get; set; }

	[JsonPropertyName("max")]
	public double? Max { get; set; }

	[JsonPropertyName("step")]
	public double? Step { get; set; }

	[JsonPropertyName("options")]
	public List<XWModSettingOption> Options { get; set; } = new List<XWModSettingOption>();

	[JsonPropertyName("minSelected")]
	public int? MinSelected { get; set; }

	[JsonPropertyName("maxSelected")]
	public int? MaxSelected { get; set; }

	[JsonPropertyName("applyMode")]
	public string ApplyMode { get; set; } = "restart";

	[JsonPropertyName("affectsGameplay")]
	public bool AffectsGameplay { get; set; } = true;
}
