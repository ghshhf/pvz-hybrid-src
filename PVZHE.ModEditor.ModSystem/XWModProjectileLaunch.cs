using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModProjectileLaunch(XWModEntityHandle Source, string ProjectileKey, double Damage, double Speed, Vector2 Velocity)
{
	public XWModProjectileSource Attribution { get; set; }
}
