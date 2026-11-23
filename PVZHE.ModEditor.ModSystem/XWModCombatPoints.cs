using System;
using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModCombatPoints
{
	public static readonly XWModModifierPoint<XWModEffectAdmission> EffectAdmission = XWModExtensionCatalog.Modifier("combat.effect.admission.v1", XWModAuthority.Host, (XWModEffectAdmission original, XWModEffectAdmission x) => x != null && original != null && x.Target == original.Target && x.Key == original.Key, (XWModEffectAdmission before, XWModEffectAdmission after) => !before.Blocked || after.Blocked);

	public static readonly XWModModifierPoint<XWModDamageValue> Damage = XWModExtensionCatalog.Modifier("combat.damage.modify.v1", XWModAuthority.Host, (XWModDamageValue original, XWModDamageValue x) => SameDamageIdentity(original, x) && double.IsFinite(x.Amount) && x.Amount >= 0.0 && x.Amount <= 3.4028234663852886E+38);

	public static readonly XWModModifierPoint<XWModDamageValue> Healing = XWModExtensionCatalog.Modifier("combat.healing.modify.v1", XWModAuthority.Host, (XWModDamageValue original, XWModDamageValue x) => SameDamageIdentity(original, x) && double.IsFinite(x.Amount) && Math.Abs(x.Amount) <= 3.4028234663852886E+38);

	public static readonly XWModEventPoint<XWModDamageApplied> DamageApplied = XWModExtensionCatalog.Notification<XWModDamageApplied>("combat.damage.applied.v1", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModLifeChanged> LifeChanged = XWModExtensionCatalog.Notification<XWModLifeChanged>("combat.life.changed.v1", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModLifeChanged> LifeObserved = XWModExtensionCatalog.Notification<XWModLifeChanged>("combat.life.observed.v1", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModArmorChanged> ArmorChanged = XWModExtensionCatalog.Notification<XWModArmorChanged>("combat.armor.changed.v1", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModArmorChanged> ArmorObserved = XWModExtensionCatalog.Notification<XWModArmorChanged>("combat.armor.observed.v1", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModModifierPoint<XWModEffectParameters> EffectParameters = XWModExtensionCatalog.Modifier("combat.effect.parameters.v1", XWModAuthority.Host, (XWModEffectParameters original, XWModEffectParameters x) => ValidEffect(x) && original != null && original.Target == x.Target && original.Key == x.Key && original.SupportsDuration == x.SupportsDuration && original.SupportsStrength == x.SupportsStrength && original.AllowsPermanent == x.AllowsPermanent);

	public static readonly XWModEventPoint<XWModEffectChanged> EffectChanged = XWModExtensionCatalog.Notification<XWModEffectChanged>("combat.effect.changed.v1", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModEffectChanged> EffectObserved = XWModExtensionCatalog.Notification<XWModEffectChanged>("combat.effect.observed.v1", XWModAuthority.Local, localPresentation: true);

	private static bool SameDamageIdentity(XWModDamageValue original, XWModDamageValue candidate)
	{
		if (original != null && candidate != null && original.Target == candidate.Target && original.Source == candidate.Source && original.Cause == candidate.Cause)
		{
			return original.Attribution == candidate.Attribution;
		}
		return false;
	}

	internal static bool ValidEffect(XWModEffectParameters x)
	{
		if (x != null && x.SupportsDuration == x.Duration.HasValue && x.SupportsStrength == x.Strength.HasValue && (!x.Duration.HasValue || (double.IsFinite(x.Duration.Value) && x.Duration <= 3.4028234663852886E+38 && (x.Duration >= 0.0 || (x.AllowsPermanent && x.Duration == -1.0)))))
		{
			if (x.Strength.HasValue)
			{
				if (double.IsFinite(x.Strength.Value) && x.Strength >= 0.0)
				{
					return x.Strength <= 3.4028234663852886E+38;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[ModuleInitializer]
	internal static void Register()
	{
		_ = Damage;
		_ = Healing;
		_ = DamageApplied;
		_ = LifeChanged;
		_ = LifeObserved;
		_ = EffectAdmission;
		_ = EffectParameters;
		_ = EffectChanged;
		_ = EffectObserved;
		_ = ArmorChanged;
		_ = ArmorObserved;
	}
}
