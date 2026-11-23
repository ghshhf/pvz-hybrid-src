using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Item/SurfWave/Scene/TowerDefenseItemSurfWave.cs")]
public class TowerDefenseItemSurfWave : TowerDefenseItem
{
	public new class MethodName : TowerDefenseItem.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Create = "Create";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName PullTargets = "PullTargets";

		public static readonly StringName DamageCell = "DamageCell";

		public static readonly StringName HasGameplayAuthority = "HasGameplayAuthority";

		public static readonly StringName ReleaseTargets = "ReleaseTargets";

		public new static readonly StringName Destroy = "Destroy";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefenseItem.PropertyName
	{
		public static readonly StringName _elapsed = "_elapsed";

		public static readonly StringName _damageTicks = "_damageTicks";

		public static readonly StringName _closing = "_closing";
	}

	public new class SignalName : TowerDefenseItem.SignalName
	{
	}

	private const double Duration = 5.0;

	private double _elapsed;

	private int _damageTicks;

	private bool _closing;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			HitBoxDestroy();
			if (!base.IsProgressRestoreInFlight)
			{
				base.sprite.SetAnimation("Up", loop: false);
			}
		}
	}

	public static TowerDefenseItemSurfWave Create(Vector2I grid, TowerDefenseEnum.CHARACTER_CAMP ownerCamp)
	{
		if (!TowerDefenseManager.HasGameplayAuthority)
		{
			return null;
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(grid);
		if (mapCell == null)
		{
			return null;
		}
		Vector2 mapCellPosCenter = towerDefenseManager.GetMapCellPosCenter(grid);
		TowerDefenseItemSurfWave towerDefenseItemSurfWave = TowerDefenseCharacter.CreateCharacter("ItemSurfWave", mapCellPosCenter, grid, mapCell.GetGroundHeight()) as TowerDefenseItemSurfWave;
		if (!GodotObject.IsInstanceValid(towerDefenseItemSurfWave))
		{
			return null;
		}
		towerDefenseItemSurfWave.camp = ownerCamp;
		if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
		{
			TowerDefenseControlNew currentControl = towerDefenseManager.currentControl;
			int nextSyncId = currentControl.GetNextSyncId();
			currentControl.RegisterSyncCharacter(nextSyncId, towerDefenseItemSurfWave);
			MultiPlayerManager.Instance.SendSpawnCharacterAt("ItemSurfWave", grid.X, grid.Y, nextSyncId, 1.0, 1.0, ownerCamp != TowerDefenseEnum.CHARACTER_CAMP.PLANT, 0.0, useCreate: true, mapCellPosCenter.X, mapCellPosCenter.Y, walkAfterSpawn: false, towerDefenseItemSurfWave.groundHeight);
		}
		return towerDefenseItemSurfWave;
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || !inGame || isDestroy || !TowerDefenseManager.Instance.IsGameRunning() || _closing)
		{
			return;
		}
		_elapsed = Mathf.Min(5.0, _elapsed + delta);
		if (TowerDefenseManager.HasGameplayAuthority)
		{
			PullTargets(delta);
			while (_damageTicks < (int)_elapsed)
			{
				_damageTicks++;
				DamageCell();
			}
		}
		if (_elapsed >= 5.0)
		{
			_closing = true;
			ReleaseTargets();
			base.sprite.SetAnimation("Down", loop: false);
		}
	}

	private void PullTargets(double delta)
	{
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		Vector2 mapCellPosCenter = towerDefenseManager.GetMapCellPosCenter(base.gridPos);
		Vector2 mapGridSize = towerDefenseManager.GetMapGridSize();
		foreach (Variant item in towerDefenseManager.GetCampTarget(base.camp))
		{
			TowerDefenseCharacter towerDefenseCharacter = (TowerDefenseCharacter)(GodotObject)item;
			if (!(towerDefenseCharacter is TowerDefenseZombie) || !GodotObject.IsInstanceValid(towerDefenseCharacter) || towerDefenseCharacter.isDestroy || towerDefenseCharacter.instance.invincible || towerDefenseCharacter.instance.maskFlags == 0 || !towerDefenseCharacter.instance.canBeCollection)
			{
				continue;
			}
			Vector2 logicalGlobalPosition = towerDefenseCharacter.GetLogicalGlobalPosition();
			Vector2I mapGridPos = towerDefenseManager.GetMapGridPos(logicalGlobalPosition);
			if (Mathf.Abs(mapGridPos.X - base.gridPos.X) <= 1 && Mathf.Abs(mapGridPos.Y - base.gridPos.Y) <= 1)
			{
				if (towerDefenseCharacter.instance.zombiePhysique < TowerDefenseEnum.ZOMBIE_PHYSIQUE.HUGE)
				{
					Vector2 vector = ((mapCellPosCenter - logicalGlobalPosition) / mapGridSize).LimitLength((float)(delta * 2.0)) * mapGridSize;
					towerDefenseCharacter.SetLogicalGlobalPosition(logicalGlobalPosition + vector);
					mapGridPos = towerDefenseManager.GetMapGridPos(logicalGlobalPosition + vector);
				}
				if (mapGridPos == base.gridPos && towerDefenseCharacter.instance.zombiePhysique != TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS)
				{
					towerDefenseCharacter.BuffAdd(new TowerDefenseCharacterBuffMagicAnchor
					{
						key = "SurfWaveHold",
						time = 0.15
					});
				}
			}
		}
	}

	private void DamageCell()
	{
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		foreach (Variant item in towerDefenseManager.GetCampTarget(base.camp))
		{
			TowerDefenseCharacter towerDefenseCharacter = (TowerDefenseCharacter)(GodotObject)item;
			if (towerDefenseCharacter is TowerDefenseZombie && GodotObject.IsInstanceValid(towerDefenseCharacter) && towerDefenseManager.GetMapGridPos(towerDefenseCharacter.GetLogicalGlobalPosition()) == base.gridPos)
			{
				towerDefenseCharacter.FlagHurt(100.0, 3);
			}
		}
	}

	private static bool HasGameplayAuthority()
	{
		return TowerDefenseManager.HasGameplayAuthority;
	}

	private void ReleaseTargets()
	{
		if (!TowerDefenseManager.HasGameplayAuthority || !GodotObject.IsInstanceValid(TowerDefenseGroundItemBase.characterNode))
		{
			return;
		}
		foreach (Node child in TowerDefenseGroundItemBase.characterNode.GetChildren())
		{
			if (child is TowerDefenseItemSurfWave towerDefenseItemSurfWave && towerDefenseItemSurfWave != this && !towerDefenseItemSurfWave.isDestroy && !towerDefenseItemSurfWave._closing && towerDefenseItemSurfWave.gridPos == base.gridPos)
			{
				return;
			}
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		foreach (Variant item in towerDefenseManager.GetCampTarget(base.camp))
		{
			TowerDefenseCharacter towerDefenseCharacter = (TowerDefenseCharacter)(GodotObject)item;
			if (GodotObject.IsInstanceValid(towerDefenseCharacter) && towerDefenseManager.GetMapGridPos(towerDefenseCharacter.GetLogicalGlobalPosition()) == base.gridPos)
			{
				towerDefenseCharacter.BuffDelete("SurfWaveHold");
			}
		}
	}

	public override void Destroy(bool freeInstance = true)
	{
		ReleaseTargets();
		base.Destroy(freeInstance);
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Up" && !_closing)
		{
			base.sprite.SetAnimation("Idle");
		}
		if (clip == "Down" && TowerDefenseManager.HasGameplayAuthority)
		{
			Destroy();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["elapsed"] = _elapsed,
			["damageTicks"] = _damageTicks,
			["closing"] = _closing
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_elapsed = data.GetValueOrDefault("elapsed", 0.0).AsDouble();
		_damageTicks = data.GetValueOrDefault("damageTicks", 0).AsInt32();
		_closing = data.GetValueOrDefault("closing", false).AsBool();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportVariantSave();
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		ImportVariantSave(data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(13)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "grid", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "ownerCamp", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PullTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DamageCell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HasGameplayAuthority, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null),
			new MethodInfo(MethodName.ReleaseTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Destroy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "freeInstance", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseItemSurfWave>(Create(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1])));
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PullTargets && args.Count == 1)
		{
			PullTargets(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DamageCell && args.Count == 0)
		{
			DamageCell();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasGameplayAuthority && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasGameplayAuthority());
			return true;
		}
		if (method == MethodName.ReleaseTargets && args.Count == 0)
		{
			ReleaseTargets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Destroy && args.Count == 1)
		{
			Destroy(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportVariantSave && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportVariantSave());
			return true;
		}
		if (method == MethodName.ImportVariantSave && args.Count == 1)
		{
			ImportVariantSave(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpecialState());
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState && args.Count == 1)
		{
			ImportNetworkSpecialState(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseItemSurfWave>(Create(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1])));
			return true;
		}
		if (method == MethodName.HasGameplayAuthority && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasGameplayAuthority());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.PullTargets)
		{
			return true;
		}
		if (method == MethodName.DamageCell)
		{
			return true;
		}
		if (method == MethodName.HasGameplayAuthority)
		{
			return true;
		}
		if (method == MethodName.ReleaseTargets)
		{
			return true;
		}
		if (method == MethodName.Destroy)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ExportVariantSave)
		{
			return true;
		}
		if (method == MethodName.ImportVariantSave)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._elapsed)
		{
			_elapsed = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._damageTicks)
		{
			_damageTicks = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._closing)
		{
			_closing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._elapsed)
		{
			value = VariantUtils.CreateFrom(in _elapsed);
			return true;
		}
		if (name == PropertyName._damageTicks)
		{
			value = VariantUtils.CreateFrom(in _damageTicks);
			return true;
		}
		if (name == PropertyName._closing)
		{
			value = VariantUtils.CreateFrom(in _closing);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName._elapsed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._damageTicks, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._closing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._elapsed, Variant.From(in _elapsed));
		info.AddProperty(PropertyName._damageTicks, Variant.From(in _damageTicks));
		info.AddProperty(PropertyName._closing, Variant.From(in _closing));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._elapsed, out var value))
		{
			_elapsed = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._damageTicks, out var value2))
		{
			_damageTicks = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName._closing, out var value3))
		{
			_closing = value3.As<bool>();
		}
	}
}
