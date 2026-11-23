using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModModeService
{
	private readonly string _owner;

	internal XWModModeService(string owner)
	{
		_owner = owner;
	}

	internal static string ModeKey(TowerDefenseControlNew control, TowerDefenseBattleProcess process = null)
	{
		TowerDefenseLevelBaseConfig towerDefenseLevelBaseConfig = control?.levelConfig;
		string text = ((towerDefenseLevelBaseConfig is TowerDefenseLevelConfig towerDefenseLevelConfig) ? towerDefenseLevelConfig.processName?.ToString() : ((!(towerDefenseLevelBaseConfig is TowerDefenseLevelNewConfig towerDefenseLevelNewConfig)) ? null : towerDefenseLevelNewConfig.processName?.ToString()));
		string text2 = text;
		if (!string.IsNullOrEmpty(text2))
		{
			return text2;
		}
		return (process ?? control?.process)?.GetType().Name ?? "";
	}

	public XWModModeState GetState()
	{
		TowerDefenseControlNew currentControl = TowerDefenseManager.CurrentControl;
		if (!GodotObject.IsInstanceValid(currentControl))
		{
			return null;
		}
		return new XWModModeState(XWModExtensions.SessionId, currentControl.levelConfig?.name ?? "", ModeKey(currentControl), currentControl.isGameRunning, currentControl.isGameFail, currentControl.HasPendingBattleOperations);
	}

	public Task<XWModCommandResult<bool>> RequestOutcomeEvaluation()
	{
		return XWModExtensions.ExecuteCommand(_owner, "mode.evaluate", () => TowerDefenseManager.CurrentControl.process.EvaluateOutcome(), () => (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost) && GodotObject.IsInstanceValid(TowerDefenseManager.CurrentControl) && TowerDefenseManager.CurrentControl.isGameRunning && !TowerDefenseManager.CurrentControl.GetTree().Paused && !TowerDefenseManager.CurrentControl.HasPendingBattleOperations && GodotObject.IsInstanceValid(TowerDefenseManager.CurrentControl.process));
	}
}
