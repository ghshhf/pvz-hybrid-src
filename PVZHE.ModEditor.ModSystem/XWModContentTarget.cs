using System.Text.Json.Serialization;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModContentTarget
{
	[JsonPropertyName("providerModId")]
	public string ProviderModId { get; set; } = "";

	[JsonPropertyName("category")]
	public string Category { get; set; } = "";

	[JsonPropertyName("key")]
	public string Key { get; set; } = "";
}
