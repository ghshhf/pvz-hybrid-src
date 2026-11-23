namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModProjectileHitEvent(XWModEntityHandle Source, XWModEntityHandle Target, string ProjectileKey, double LifeLost)
{
	public XWModProjectileSource Attribution { get; set; }
}
