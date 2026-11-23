using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModCardService
{
	private readonly string _owner;

	internal XWModCardService(string owner)
	{
		_owner = owner;
	}

	public IReadOnlyList<XWModCardState> GetCards()
	{
		return XWModCards.GetCards();
	}

	public bool TryGetCard(XWModCardHandle handle, out XWModCardState state)
	{
		state = (XWModCards.TryResolve(handle, out var card) ? card.GetModCardState() : null);
		return state != null;
	}

	public Task<XWModCommandResult<XWModCardUseResult>> Use(XWModCardHandle handle, int column, int row)
	{
		TowerDefenseInGamePacketShow card;
		return XWModExtensions.ExecuteCommand(_owner, "card.use", delegate
		{
			if (!XWModCards.TryResolve(handle, out card))
			{
				return XWModCardUseResult.Rejected;
			}
			PacketPickControl packetPickControl = TowerDefenseManager.Instance.GetPacketPickControl();
			return GodotObject.IsInstanceValid(packetPickControl) ? packetPickControl.TryUseModCard(card, new Vector2I(column, row)) : XWModCardUseResult.Rejected;
		}, () => XWModCards.TryResolve(handle, out card) && card.CanUseFromMod(), handle.Id.ToString());
	}

	public Task<XWModCommandResult<bool>> Cancel(XWModCardHandle handle)
	{
		TowerDefenseInGamePacketShow card;
		return XWModExtensions.ExecuteCommand(_owner, "card.cancel", delegate
		{
			if (!XWModCards.TryResolve(handle, out card))
			{
				return false;
			}
			PacketPickControl packetPickControl = TowerDefenseManager.Instance.GetPacketPickControl();
			if (!GodotObject.IsInstanceValid(packetPickControl) || packetPickControl.packetPick != card)
			{
				return false;
			}
			packetPickControl.PacketPickRelease();
			return true;
		}, () => XWModCards.TryResolve(handle, out card) && card.select && !card.HasMeta("packet_pending_plant"), handle.Id.ToString());
	}
}
