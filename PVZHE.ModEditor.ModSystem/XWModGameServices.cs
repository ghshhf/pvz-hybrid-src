using System;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModGameServices
{
	private readonly XWModRuntimeContext _context;

	public XWModCharacterService Characters => _context.Characters;

	public XWModCombatService Combat => _context.Combat;

	public XWModCardService Cards => _context.Cards;

	public XWModEconomyService Economy => _context.Economy;

	public XWModProjectileService Projectiles => _context.GetOrCreateService(() => new XWModProjectileService(_context.ModId));

	public XWModSeededRandom Random { get; }

	internal XWModGameServices(XWModRuntimeContext context)
	{
		_context = context;
		Random = new XWModSeededRandom(context.ModId);
	}

	public bool TryGetBattle(out XWModBattleSnapshot snapshot)
	{
		snapshot = null;
		TowerDefenseControlNew currentControl = TowerDefenseManager.CurrentControl;
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (XWModExtensions.SessionId == Guid.Empty || !GodotObject.IsInstanceValid(currentControl) || !GodotObject.IsInstanceValid(instance))
		{
			return false;
		}
		snapshot = new XWModBattleSnapshot(XWModExtensions.SessionId, XWModExtensions.NetworkSessionProvider?.Invoke() ?? "", currentControl.isGameRunning, currentControl.GetTree()?.Paused ?? false, !Global.IsMultiplayerMode || MultiPlayerManager.IsHost, instance.runGameTime, instance.gridNum);
		return true;
	}
}
