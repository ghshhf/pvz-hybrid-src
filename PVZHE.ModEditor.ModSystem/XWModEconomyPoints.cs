using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModEconomyPoints
{
	public static readonly XWModEventPoint<XWModSunChange> BalanceChanged = XWModExtensionCatalog.Notification<XWModSunChange>("economy.balance.changed.v1", XWModAuthority.Owner);

	public static readonly XWModEventPoint<XWModSunCollection> Collected = XWModExtensionCatalog.Notification<XWModSunCollection>("economy.collected.v1", XWModAuthority.Owner);

	public static readonly XWModEventPoint<XWModSunSpend> SpendChanged = XWModExtensionCatalog.Notification<XWModSunSpend>("economy.spend.changed.v1", XWModAuthority.Owner);

	public static readonly XWModModifierPoint<XWModSunValue> CollectionValue = XWModExtensionCatalog.Modifier("economy.collection.value.v1", XWModAuthority.Owner, (XWModSunValue original, XWModSunValue value) => value != null && value.Account == original.Account && value.Reason == original.Reason);

	public static readonly XWModModifierPoint<XWModSunValue> RewardValue = XWModExtensionCatalog.Modifier("economy.reward.value.v1", XWModAuthority.Owner, (XWModSunValue original, XWModSunValue value) => value != null && value.Account == original.Account && value.Reason == original.Reason && value.Value >= 0);

	[ModuleInitializer]
	internal static void Register()
	{
		_ = BalanceChanged;
	}
}
