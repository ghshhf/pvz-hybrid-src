using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModPlantCostRequest
{
	public GodotObject Packet { get; set; }

	public long Cost { get; set; }
}
