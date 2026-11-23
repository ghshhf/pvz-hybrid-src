using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModBattleEvent
{
	public GodotObject Battle { get; set; }

	public string Reason { get; set; } = "";
}
