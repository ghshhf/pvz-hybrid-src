using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModCardPoints
{
	public static readonly XWModEventPoint<XWModCardEvent> StateChanged = XWModExtensionCatalog.Notification<XWModCardEvent>("card.state.changed.v1", XWModAuthority.Owner);

	public static readonly XWModModifierPoint<XWModCardCooldown> Cooldown = XWModExtensionCatalog.Modifier("card.cooldown.duration.v1", XWModAuthority.Owner, (XWModCardCooldown original, XWModCardCooldown value) => value != null && value.Card == original.Card && value.ContentId == original.ContentId && value.Initial == original.Initial && double.IsFinite(value.Seconds) && value.Seconds >= 0.0);

	public static readonly XWModModifierPoint<XWModCardCost> Cost = XWModExtensionCatalog.Modifier("card.cost.quote.v1", XWModAuthority.Owner, (XWModCardCost original, XWModCardCost value) => value != null && value.Card == original.Card && value.ContentId == original.ContentId && value.Cost != -9223372036854775808L);

	[ModuleInitializer]
	internal static void Register()
	{
		_ = StateChanged;
	}
}
