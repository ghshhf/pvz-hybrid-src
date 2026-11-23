using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModDamageRequest
{
	public TowerDefenseCharacter Target { get; set; }

	public GodotObject Source { get; set; }

	public double Amount { get; set; }

	public double DamageLimit { get; set; } = -1.0;
}
