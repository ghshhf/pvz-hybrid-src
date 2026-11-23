using System.Text.Json.Serialization;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSettingOption
{
	[JsonPropertyName("value")]
	public string Value { get; set; } = "";

	[JsonPropertyName("label")]
	public string Label { get; set; } = "";
}
