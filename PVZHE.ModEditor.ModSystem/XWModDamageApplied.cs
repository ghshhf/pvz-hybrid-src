using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModDamageApplied(XWModEntityHandle Target, XWModEntityHandle Source, string Cause, double Requested, double Modified, double LifeLost, double ArmorDurabilityLost, double CellShieldAbsorbed, bool NoDurabilityLost, bool Lethal)
{
	public XWModProjectileSource Attribution { get; set; }

	public string ProjectileKey { get; set; }

	public Vector2? TargetPositionAtEntry { get; internal init; }
}
