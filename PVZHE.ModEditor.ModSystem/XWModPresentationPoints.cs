using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModPresentationPoints
{
	public static readonly XWModEventPoint<XWModUiSurface> MainMenu = XWModExtensionCatalog.Notification<XWModUiSurface>("ui.main-menu.mount.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModUiSurface> BattleHud = XWModExtensionCatalog.Notification<XWModUiSurface>("ui.battle-hud.mount.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModUiSurface> CharacterOverlay = XWModExtensionCatalog.Notification<XWModUiSurface>("ui.character-overlay.mount.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModUiSurface> CardAugment = XWModExtensionCatalog.Notification<XWModUiSurface>("ui.card.mount.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModLayoutState> LayoutChanged = XWModExtensionCatalog.Notification<XWModLayoutState>("ui.layout.changed.v3", XWModAuthority.Local, localPresentation: true);

	[ModuleInitializer]
	internal static void Install()
	{
		_ = MainMenu;
	}
}
