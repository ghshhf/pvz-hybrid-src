using System.Linq;
using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModCharacterPoints
{
	public static readonly XWModEventPoint<XWModCharacterLifecycleEvent> Lifecycle = XWModExtensionCatalog.Notification<XWModCharacterLifecycleEvent>("character.lifecycle.v3", XWModAuthority.Local);

	public static readonly XWModEventPoint<XWModAttackEvent> Attack = XWModExtensionCatalog.Notification<XWModAttackEvent>("character.attack.v3", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModProjectileHitEvent> ProjectileHit = XWModExtensionCatalog.Notification<XWModProjectileHitEvent>("projectile.hit.v3", XWModAuthority.Host);

	public static readonly XWModModifierPoint<XWModTargetSelection> TargetSelection = XWModExtensionCatalog.Modifier("character.target.v3", XWModAuthority.Host, (XWModTargetSelection original, XWModTargetSelection x) => x != null && x.Source == original.Source && x.Candidates == original.Candidates && (x.Selected == default(XWModEntityHandle) || original.Candidates.Contains(x.Selected)));

	public static readonly XWModModifierPoint<XWModProjectileLaunch> ProjectileLaunch = XWModExtensionCatalog.Modifier("projectile.launch.v3", XWModAuthority.Host, (XWModProjectileLaunch original, XWModProjectileLaunch x) => x != null && x.Source == original.Source && x.Attribution == original.Attribution && x.ProjectileKey == original.ProjectileKey && double.IsFinite(x.Damage) && double.IsFinite(x.Speed) && x.Speed >= 0.0 && x.Velocity.IsFinite(), null, (XWModProjectileLaunch original, XWModProjectileLaunch x) => (!(x != null) || !x.Attribution.IsEmpty || original.Attribution.IsEmpty) ? x : x with
	{
		Attribution = original.Attribution
	});

	[ModuleInitializer]
	internal static void Register()
	{
		_ = Lifecycle;
		_ = Attack;
		_ = ProjectileHit;
		_ = TargetSelection;
		_ = ProjectileLaunch;
		XWModExtensionCatalog.RegisterLegacyPoint(XWModHookPoints.CharacterCreated.Id);
		XWModExtensionCatalog.RegisterLegacyPoint(XWModHookPoints.CharacterDestroyed.Id);
	}
}
