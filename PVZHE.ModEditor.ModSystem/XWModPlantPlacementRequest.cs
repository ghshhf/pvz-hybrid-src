using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModPlantPlacementRequest
{
	public GodotObject Packet { get; set; }

	public GodotObject Target { get; set; }

	public Vector2I Cell { get; set; }
}
