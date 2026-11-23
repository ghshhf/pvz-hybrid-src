using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModPlantFootprintPlan : IDisposable
{
	private readonly record struct Refund(Vector2 Position, double Height, long Value);

	private readonly Refund[] _refunds;

	private readonly List<TowerDefenseSunBase> _refundDrops = new List<TowerDefenseSunBase>();

	private bool _refundDropsPrepared;

	private readonly TowerDefenseCellInstance[] _cells;

	private readonly TowerDefenseCellInstance[] _before;

	private readonly TowerDefenseCellInstance[] _after;

	private readonly HashSet<TowerDefenseCharacter> _removed;

	private readonly TowerDefensePacketConfig _check;

	private readonly TowerDefensePlantConfig _checkPlant;

	private readonly TowerDefenseControlNew _control = TowerDefenseManager.CurrentControl;

	private bool _attached;

	private bool _committed;

	private readonly bool _surround;

	private bool _reserved;

	private TowerDefenseCharacter _created;

	internal TowerDefensePacketConfig Packet { get; }

	internal Vector2I Anchor { get; }

	internal int Width => _cells.Length;

	internal IEnumerable<Vector2I> TargetCells => _cells.Select((TowerDefenseCellInstance cell) => cell.gridPos);

	internal long RefundTotal => _refunds.Sum((Refund item) => item.Value);

	internal bool RefundAsSunDrops { get; }

	internal bool IsPendingValid
	{
		get
		{
			if (_attached && !_committed && SameMap() && _removed.All((TowerDefenseCharacter c) => GodotObject.IsInstanceValid(c) && !c.isDestroy && !c.die) && GodotObject.IsInstanceValid(_created) && !_created.isDestroy && !_created.IsQueuedForDeletion())
			{
				return !_cells.Where((TowerDefenseCellInstance cell, int i) => !cell.MatchesFootprintState(_after[i])).Any();
			}
			return false;
		}
	}

	internal XWModPlantFootprintPlan(TowerDefensePacketConfig packet, Vector2I anchor, TowerDefenseCellInstance[] cells, HashSet<TowerDefenseCharacter> removed, XWModPlantFootprintOptions options)
	{
		Packet = packet;
		Anchor = anchor;
		_cells = cells;
		_removed = removed;
		_surround = options.Surround;
		RefundAsSunDrops = options.RefundAsSunDrops;
		_refunds = (from old in removed
			where old.config.cost > 0
			select new Refund(old.GetLogicalGlobalPosition(), old.groundHeight, old.config.cost)).ToArray();
		_before = cells.Select((TowerDefenseCellInstance c) => c.CopyFootprintState()).ToArray();
		_after = cells.Select((TowerDefenseCellInstance c) => c.CopyFootprintState()).ToArray();
		_check = (TowerDefensePacketConfig)packet.Duplicate();
		_checkPlant = (TowerDefensePlantConfig)packet.characterConfig.Duplicate();
		if (!_surround)
		{
			_checkPlant.extendGrid = new Array<Vector2I>();
		}
		_check.characterConfig = _checkPlant;
		TowerDefenseCellInstance[] after = _after;
		foreach (TowerDefenseCellInstance towerDefenseCellInstance in after)
		{
			foreach (TowerDefenseCharacter item in removed)
			{
				if (towerDefenseCellInstance.characterList.Contains(item))
				{
					towerDefenseCellInstance.RemoveCharacter(item);
				}
			}
		}
	}

	internal bool Validate()
	{
		for (int i = 0; i < _after.Length; i++)
		{
			if (!_after[i].CanPacketPlant(_check, noLimit: false, isCheck: true, allowReplacement: false))
			{
				return false;
			}
			if (!_surround)
			{
				TowerDefenseCharacter[] array = Tops(_before[i]).ToArray();
				if (array.Length > 1 || array.Any((TowerDefenseCharacter t) => !t.config.plantGridType.Contains(_checkPlant.plantGridOverrideType)))
				{
					return false;
				}
			}
		}
		return true;
	}

	private IEnumerable<TowerDefenseCharacter> Tops(TowerDefenseCellInstance state)
	{
		return (from top in _removed.Where(state.characterSlotDictionary.ContainsKey).Select(delegate(TowerDefenseCharacter old)
			{
				TowerDefenseCharacter towerDefenseCharacter = state.characterSlotDictionary[old];
				HashSet<TowerDefenseCharacter> hashSet = new HashSet<TowerDefenseCharacter>();
				while (GodotObject.IsInstanceValid(towerDefenseCharacter) && _removed.Contains(towerDefenseCharacter) && hashSet.Add(towerDefenseCharacter))
				{
					towerDefenseCharacter = state.characterSlotDictionary.GetValueOrDefault(towerDefenseCharacter);
				}
				return towerDefenseCharacter;
			})
			where GodotObject.IsInstanceValid(top) && !_removed.Contains(top)
			select top).Distinct();
	}

	internal bool Attach(TowerDefenseCharacter character)
	{
		if (_attached || _committed || !SameMap() || _removed.Any((TowerDefenseCharacter c) => !GodotObject.IsInstanceValid(c) || c.isDestroy || c.die) || _cells.Where((TowerDefenseCellInstance cell, int i) => !cell.MatchesFootprintState(_before[i])).Any())
		{
			return false;
		}
		_created = character;
		for (int num = 0; num < _after.Length; num++)
		{
			if (_surround)
			{
				_after[num].InsertFootprintSurround(character);
			}
			else
			{
				_after[num].InsertFootprintSupport(_check, character, Tops(_before[num]).FirstOrDefault());
			}
		}
		_attached = true;
		for (int num2 = 0; num2 < _cells.Length; num2++)
		{
			_cells[num2].RestoreFootprintState(_after[num2]);
		}
		character.cell = _cells[0];
		TowerDefenseCellInstance[] cells = _cells;
		for (int num3 = 0; num3 < cells.Length; num3++)
		{
			XWModPlantPlacementService.NotifySupportOccupants(cells[num3], character);
		}
		return true;
	}

	internal bool TryPrepareRefundDrops(EconomyAccountId account)
	{
		if (_refundDropsPrepared)
		{
			return true;
		}
		if (!RefundAsSunDrops || !IsPendingValid)
		{
			return false;
		}
		Refund[] refunds = _refunds;
		for (int i = 0; i < refunds.Length; i++)
		{
			Refund refund = refunds[i];
			TowerDefenseSunBase towerDefenseSunBase = TowerDefenseManager.Instance.SunCreate(account, refund.Position, refund.Value, TowerDefenseEnum.SUN_MOVING_METHOD.LAND, refund.Height);
			if (!GodotObject.IsInstanceValid(towerDefenseSunBase))
			{
				return false;
			}
			_refundDrops.Add(towerDefenseSunBase);
		}
		_refundDropsPrepared = true;
		return true;
	}

	internal bool Reserve()
	{
		if (!_reserved && SameMap())
		{
			return _reserved = XWModPlantPlacementService.Reserve(_cells);
		}
		return false;
	}

	private bool SameMap()
	{
		if (_control == TowerDefenseManager.CurrentControl)
		{
			return _cells.All((TowerDefenseCellInstance cell) => GodotObject.IsInstanceValid(cell) && cell == TowerDefenseManager.GetMapCell(cell.gridPos));
		}
		return false;
	}

	internal void Commit()
	{
		if (!_attached || _committed)
		{
			throw new InvalidOperationException("Footprint is not pending.");
		}
		MarkCommitted();
		CleanupReplacedSupports();
	}

	internal void MarkCommitted()
	{
		_committed = true;
		_refundDrops.Clear();
	}

	internal void CleanupReplacedSupports()
	{
		foreach (TowerDefenseCharacter item in _removed)
		{
			try
			{
				item.isShovel = true;
				item.suppressDeathrattles = true;
				item.instance.canBeCollection = false;
				item.Destroy();
				if (GodotObject.IsInstanceValid(item) && !item.isDestroy)
				{
					GD.PushWarning("Replaced support did not enter its destroy state; releasing the detached instance.");
					item.QueueFree();
				}
			}
			catch (Exception ex)
			{
				GD.PushWarning("Replaced support cleanup failed: " + ex.Message);
				try
				{
					if (GodotObject.IsInstanceValid(item))
					{
						item.QueueFree();
					}
				}
				catch (Exception ex2)
				{
					GD.PushWarning("Detached support release failed: " + ex2.Message);
				}
			}
		}
	}

	public void Dispose()
	{
		try
		{
			if (!_attached || _committed)
			{
				return;
			}
			if (SameMap())
			{
				for (int i = 0; i < _cells.Length; i++)
				{
					_cells[i].RestoreFootprintState(_before[i]);
				}
			}
			if (GodotObject.IsInstanceValid(_created))
			{
				_created.suppressDeathrattles = true;
				_created.isShovel = true;
				_created.QueueFree();
			}
		}
		finally
		{
			foreach (TowerDefenseSunBase refundDrop in _refundDrops)
			{
				if (GodotObject.IsInstanceValid(refundDrop))
				{
					refundDrop.Destroy();
				}
			}
			_refundDrops.Clear();
			if (_reserved)
			{
				XWModPlantPlacementService.Release(_cells);
				_reserved = false;
			}
			foreach (TowerDefenseCellInstance item in _before.Concat(_after))
			{
				item.Dispose();
			}
			_check.Dispose();
			_checkPlant.Dispose();
		}
	}
}
