using System.Text.Json.Serialization;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModLoadingPresentation
{
	[JsonPropertyName("startupScene")]
	public string StartupScene { get; set; } = "";

	[JsonPropertyName("transitionScene")]
	public string TransitionScene { get; set; } = "";

	internal XWModLoadingPresentation Clone()
	{
		return new XWModLoadingPresentation
		{
			StartupScene = StartupScene,
			TransitionScene = TransitionScene
		};
	}
}
