using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModCharacterEvent
{
	public TowerDefenseCharacter Character { get; set; }

	public GodotObject Packet { get; set; }

	public Vector2 Position { get; set; }

	public Vector2 GridPosition { get; set; }
}
