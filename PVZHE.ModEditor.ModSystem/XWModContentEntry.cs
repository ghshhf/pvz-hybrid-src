using System.Text.Json.Serialization;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModContentEntry
{
	[JsonPropertyName("category")]
	public string Category { get; set; } = "";

	[JsonPropertyName("key")]
	public string Key { get; set; } = "";

	[JsonPropertyName("path")]
	public string Path { get; set; } = "";

	[JsonPropertyName("target")]
	public XWModContentTarget Target { get; set; }
}
