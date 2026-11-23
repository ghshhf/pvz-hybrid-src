using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModLevelPoints
{
	public static readonly XWModModifierPoint<XWModLevelPreparationValues> Prepare = XWModExtensionCatalog.Modifier<XWModLevelPreparationValues>("level.prepare.v3", XWModAuthority.Host, XWModLevelPreparation.Validate);

	public static readonly XWModModifierPoint<XWModModeGate> VictoryGate = XWModExtensionCatalog.Modifier("mode.victory-gate.v3", XWModAuthority.Host, (XWModModeGate original, XWModModeGate candidate) => candidate != null && candidate.Mode == original.Mode, (XWModModeGate previous, XWModModeGate candidate) => previous.Allowed || !candidate.Allowed);

	[ModuleInitializer]
	internal static void Install()
	{
		_ = Prepare;
		_ = VictoryGate;
	}
}
