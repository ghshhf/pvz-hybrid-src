using System.Linq;
using System.Runtime.CompilerServices;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModLifecyclePoints
{
	public static readonly XWModEventPoint<XWModBattleState> Ready = XWModExtensionCatalog.Notification<XWModBattleState>("battle.ready.v3", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModBattleState> Restored = XWModExtensionCatalog.Notification<XWModBattleState>("battle.restored.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModBattleState> Result = XWModExtensionCatalog.Notification<XWModBattleState>("battle.result.v3", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModBattleState> SettlementCompleted = XWModExtensionCatalog.Notification<XWModBattleState>("battle.settlement.completed.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModBattleState> Exited = XWModExtensionCatalog.Notification<XWModBattleState>("battle.exited.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModPauseState> SimulationPause = XWModExtensionCatalog.Notification<XWModPauseState>("battle.pause.simulation.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModPauseState> NetworkPauseUi = XWModExtensionCatalog.Notification<XWModPauseState>("battle.pause.network-ui.v3", XWModAuthority.Local, localPresentation: true);

	public static readonly XWModEventPoint<XWModWaveState> WaveReady = XWModExtensionCatalog.Notification<XWModWaveState>("wave.ready.v3", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModWaveState> WaveBegin = XWModExtensionCatalog.Notification<XWModWaveState>("wave.begin.v3", XWModAuthority.Host);

	public static readonly XWModEventPoint<XWModWaveState> WaveSpawnCompleted = XWModExtensionCatalog.Notification<XWModWaveState>("wave.spawn.completed.v3", XWModAuthority.Host);

	public static readonly XWModModifierPoint<XWModWaveReinforcements> Reinforcements = XWModExtensionCatalog.Modifier("wave.reinforcements.v3", XWModAuthority.Host, (XWModWaveReinforcements original, XWModWaveReinforcements candidate) => candidate != null && candidate.Wave == original.Wave && ValidateReinforcements(candidate));

	internal static bool ValidateReinforcements(XWModWaveReinforcements value)
	{
		if (value != null && value.Wave > 0 && value.Entries != null && value.Entries.Count <= 128 && value.Entries.All((XWModReinforcement x) => x != null && !string.IsNullOrWhiteSpace(x.ContentKey) && x.ContentKey.Length <= 256 && x.Lane > 0 && x.Count > 0 && x.Count <= 128))
		{
			return value.Entries.Sum((XWModReinforcement x) => x.Count) <= 128;
		}
		return false;
	}

	[ModuleInitializer]
	internal static void Install()
	{
		_ = Ready;
		XWModExtensionCatalog.RegisterLegacyPoint(XWModHookPoints.BattleStarted.Id);
		XWModExtensionCatalog.RegisterLegacyPoint(XWModHookPoints.BattleEnded.Id);
	}
}
