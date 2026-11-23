using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter10/TallnutSurf/Scene/TowerDefensePlantTallnutSurf.cs")]
public class TowerDefensePlantTallnutSurf : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName Timeout = "Timeout";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName DestroySet = "DestroySet";

		public static readonly StringName SpawnSurf = "SpawnSurf";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _surfPending = "_surfPending";

		public static readonly StringName _surfGrid = "_surfGrid";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private CharacterTimerComponent _timerComponent;

	private bool _surfPending;

	private Vector2I _surfGrid;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			_timerComponent.OnTimeout += Timeout;
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= Timeout;
		}
		base._ExitTree();
	}

	public override void BatchUpdate(double delta)
	{
		if (!Engine.IsEditorHint())
		{
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.timeScale = (inGame ? 1.0 : 0.0);
			}
		}
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || !inGame || isDestroy || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		if (_surfPending)
		{
			bool flag = CanRunAbility();
			base.sprite.timeScale = (flag ? timeScale : 0.0);
			if (flag && (base.sprite.clip != "Fire" || base.sprite.clipOver))
			{
				base.sprite.SetAnimation("Fire", loop: false);
			}
		}
		if (!_timerComponent.IsRunning("Surf"))
		{
			_timerComponent.Run("Surf", 30.0);
		}
	}

	private void Timeout(string timerName)
	{
		if (!(timerName != "Surf") && TowerDefenseManager.HasGameplayAuthority)
		{
			_timerComponent.Run("Surf", 30.0);
			if (!_surfPending && TryChooseWaterCell(out _surfGrid))
			{
				_surfPending = true;
				base.sprite.SetAnimation("Fire", loop: false);
			}
		}
	}

	private bool TryChooseWaterCell(out Vector2I result)
	{
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		List<Vector2I> list = new List<Vector2I>();
		List<Vector2I> list2 = new List<Vector2I>();
		HashSet<Vector2I> hashSet = new HashSet<Vector2I>();
		foreach (Variant item2 in towerDefenseManager.GetCampTarget(base.camp))
		{
			TowerDefenseCharacter towerDefenseCharacter = (TowerDefenseCharacter)(GodotObject)item2;
			if (towerDefenseCharacter is TowerDefenseZombie && GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				hashSet.Add(towerDefenseManager.GetMapGridPos(towerDefenseCharacter.GetLogicalGlobalPosition()));
			}
		}
		Vector2I mapGridNum = towerDefenseManager.GetMapGridNum();
		for (int i = base.gridPos.X + 1; i <= mapGridNum.X; i++)
		{
			for (int j = 1; j <= mapGridNum.Y; j++)
			{
				Vector2I item = new Vector2I(i, j);
				TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(item);
				if (mapCell != null && mapCell.isWater)
				{
					list.Add(item);
					if (hashSet.Contains(item))
					{
						list2.Add(item);
					}
				}
			}
		}
		List<Vector2I> list3 = ((list2.Count > 0) ? list2 : list);
		List<int> list4 = new List<int>();
		foreach (Vector2I item3 in list3)
		{
			if (!list4.Contains(item3.Y))
			{
				list4.Add(item3.Y);
			}
		}
		if (list4.Count == 0)
		{
			result = default(Vector2I);
			return false;
		}
		int num = list4[GD.RandRange(0, list4.Count - 1)];
		List<Vector2I> list5 = new List<Vector2I>();
		foreach (Vector2I item4 in list3)
		{
			if (item4.Y == num)
			{
				list5.Add(item4);
			}
		}
		result = list5[GD.RandRange(0, list5.Count - 1)];
		return true;
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (CanRunAbility() && !(command != "fire") && _surfPending && TowerDefenseManager.HasGameplayAuthority)
		{
			_surfPending = false;
			SpawnSurf(_surfGrid);
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Fire" && CanRunAbility())
		{
			base.sprite.SetAnimation("Idle");
		}
	}

	public override void DestroySet()
	{
		base.DestroySet();
		if (TowerDefenseManager.HasGameplayAuthority)
		{
			SpawnSurf(base.gridPos);
		}
	}

	private void SpawnSurf(Vector2I target)
	{
		TowerDefenseItemSurfWave towerDefenseItemSurfWave = TowerDefenseItemSurfWave.Create(target, base.camp);
		if (GodotObject.IsInstanceValid(towerDefenseItemSurfWave) && instance.hypnoses)
		{
			towerDefenseItemSurfWave.Hypnoses();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["surfPending"] = _surfPending,
			["surfGrid"] = _surfGrid
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_surfPending = data.GetValueOrDefault("surfPending", false).AsBool();
		_surfGrid = data.GetValueOrDefault("surfGrid", base.gridPos).AsVector2I();
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
		return new List<MethodInfo>(12)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Timeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SpawnSurf, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "target", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Timeout && args.Count == 1)
		{
			Timeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnSurf && args.Count == 1)
		{
			SpawnSurf(VariantUtils.ConvertTo<Vector2I>(in args[0]));
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
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.Timeout)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
		{
			return true;
		}
		if (method == MethodName.SpawnSurf)
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
		if (name == PropertyName._surfPending)
		{
			_surfPending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._surfGrid)
		{
			_surfGrid = VariantUtils.ConvertTo<Vector2I>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._surfPending)
		{
			value = VariantUtils.CreateFrom(in _surfPending);
			return true;
		}
		if (name == PropertyName._surfGrid)
		{
			value = VariantUtils.CreateFrom(in _surfGrid);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._surfPending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2I, PropertyName._surfGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._surfPending, Variant.From(in _surfPending));
		info.AddProperty(PropertyName._surfGrid, Variant.From(in _surfGrid));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._surfPending, out var value))
		{
			_surfPending = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._surfGrid, out var value2))
		{
			_surfGrid = value2.As<Vector2I>();
		}
	}
}
