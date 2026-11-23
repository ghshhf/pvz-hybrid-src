using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter10/PumpkinWizard/Scene/TowerDefensePlantPumpkinWizard.cs")]
public class TowerDefensePlantPumpkinWizard : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName CheckPinataPlaced = "CheckPinataPlaced";

		public static readonly StringName HasPinataInCell = "HasPinataInCell";

		public static readonly StringName ThrowThreeMagic = "ThrowThreeMagic";

		public static readonly StringName FindTargets = "FindTargets";

		public static readonly StringName IsEnemyTransformablePlant = "IsEnemyTransformablePlant";

		public static readonly StringName Timeout = "Timeout";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName ThrowMagic = "ThrowMagic";

		public new static readonly StringName DestroySet = "DestroySet";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _castPending = "_castPending";

		public static readonly StringName _deathCast = "_deathCast";

		public static readonly StringName _pinataPresent = "_pinataPresent";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private const string TransformName = "ZombieImp";

	private const bool ExcludeImpFamily = true;

	private const string PinataName = "PlantPinata";

	private const string MiniPinataName = "PlantMiniPinata";

	private CharacterTimerComponent _timerComponent;

	private bool _castPending;

	private bool _deathCast;

	private bool _pinataPresent;

	private readonly List<TowerDefenseCharacter> _targets = new List<TowerDefenseCharacter>();

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode && inGame)
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
				_timerComponent.timeScale = ((!inGame) ? 0.0 : (buff?.GetAttackSpeedMultiplier() ?? 1.0));
			}
		}
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || !inGame || isDestroy || die || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		if (_castPending)
		{
			bool flag = CanRunAbility();
			base.sprite.timeScale = (flag ? timeScale : 0.0);
			if (flag && (base.sprite.clip != "Attack" || base.sprite.clipOver))
			{
				base.sprite.SetAnimation("Attack", loop: false);
			}
		}
		CheckPinataPlaced();
		CharacterTimerComponent timerComponent2 = _timerComponent;
		if (timerComponent2 != null && !timerComponent2.IsReleased && !_timerComponent.IsRunning("Magic"))
		{
			_timerComponent.Run("Magic", 20.0);
		}
	}

	private void CheckPinataPlaced()
	{
		if (CanRunAbility())
		{
			bool flag = HasPinataInCell();
			if (flag && !_pinataPresent)
			{
				ThrowThreeMagic();
			}
			_pinataPresent = flag;
		}
	}

	private bool HasPinataInCell()
	{
		if (!GodotObject.IsInstanceValid(cell))
		{
			return false;
		}
		foreach (TowerDefenseCharacter character in cell.GetCharacterList())
		{
			if (character != this && character is TowerDefensePlant && GodotObject.IsInstanceValid(character) && !character.die && !character.nearDie && !character.isDestroy && !character.instance.hologram)
			{
				string text = character.config?.name;
				if (text == "PlantPinata" || text == "PlantMiniPinata")
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ThrowThreeMagic()
	{
		FindTargets();
		Vector2I mapGridNum = TowerDefenseManager.Instance.GetMapGridNum();
		for (int i = 0; i < 3; i++)
		{
			if (_targets.Count > 0)
			{
				int index = GD.RandRange(0, _targets.Count - 1);
				ThrowMagic(_targets[index]);
				_targets.RemoveAt(index);
			}
			else if (mapGridNum.X > 0 && mapGridNum.Y > 0)
			{
				ThrowMagic(null, new Vector2I(GD.RandRange(1, mapGridNum.X), GD.RandRange(1, mapGridNum.Y)));
			}
		}
	}

	private void FindTargets()
	{
		_targets.Clear();
		foreach (Variant item in TowerDefenseManager.Instance.GetCampTarget(base.camp))
		{
			TowerDefenseCharacter towerDefenseCharacter = (TowerDefenseCharacter)(GodotObject)item;
			if (TowerDefenseCharacterEventTransform.CanTransform(towerDefenseCharacter, "ZombieImp", excludeImpFamily: true))
			{
				_targets.Add(towerDefenseCharacter);
			}
			else if (IsEnemyTransformablePlant(towerDefenseCharacter))
			{
				_targets.Add(towerDefenseCharacter);
			}
		}
	}

	private bool IsEnemyTransformablePlant(TowerDefenseCharacter target)
	{
		if (!GodotObject.IsInstanceValid(target) || !GodotObject.IsInstanceValid(target.instance))
		{
			return false;
		}
		if (!(target is TowerDefensePlant))
		{
			return false;
		}
		if (target.die || target.nearDie || target.isDestroy)
		{
			return false;
		}
		if (!target.instance.canBeCollection)
		{
			return false;
		}
		return target.camp != base.camp;
	}

	private void Timeout(string timerName)
	{
		if (timerName != "Magic" || !TowerDefenseManager.HasGameplayAuthority || die || isDestroy)
		{
			return;
		}
		_timerComponent.Run("Magic", 20.0);
		if (!_castPending && !instance.sleep && !base.sprite.pause)
		{
			FindTargets();
			if (_targets.Count != 0)
			{
				_castPending = true;
				base.sprite.SetAnimation("Attack", loop: false);
			}
		}
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (CanRunAbility() && !(command != "attack") && _castPending && TowerDefenseManager.HasGameplayAuthority)
		{
			_castPending = false;
			FindTargets();
			if (_targets.Count > 0)
			{
				ThrowMagic(_targets[GD.RandRange(0, _targets.Count - 1)]);
			}
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Attack" && CanRunAbility())
		{
			_castPending = false;
			base.sprite.SetAnimation("Idle");
		}
	}

	private void ThrowMagic(TowerDefenseCharacter target, Vector2I emptyGrid = default(Vector2I))
	{
		Vector2 vector = (GodotObject.IsInstanceValid(target) ? target.GetLogicalGlobalPosition() : TowerDefenseManager.GetMapCellPlantPos(emptyGrid));
		int num = 2;
		TowerDefenseProjectileCreateData projectileData = new TowerDefenseProjectileCreateData("PumpkinWizard")
		{
			baseDamage = 0.0,
			damageFlags = 0,
			fireMethodFlags = num,
			collisionFlags = 0
		};
		BulletFieldSpawnOverrides overrides = new BulletFieldSpawnOverrides
		{
			gridYOverride = TowerDefenseManager.Instance.GetMapGridPos(vector).Y,
			fireMethodFlagsOverride = num,
			catapultStartPositionOverride = GetLogicalGlobalPosition(GetNode<Marker2D>("SpriteGroup/TransformPoint/PumpkinWizard/AdobeAnimateSlot/FireMarker")),
			catapultTargetPositionOverride = vector
		};
		FireComponent.CreateProjectilePositionByData(null, target, 0.0, vector, Vector2.Zero, projectileData, 0, base.camp, default(Vector2), overrides);
	}

	public override void DestroySet()
	{
		base.DestroySet();
		if (!_deathCast && TowerDefenseManager.HasGameplayAuthority)
		{
			_deathCast = true;
			ThrowThreeMagic();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["castPending"] = _castPending,
			["deathCast"] = _deathCast,
			["pinataPresent"] = _pinataPresent
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_castPending = data.GetValueOrDefault("castPending", false).AsBool();
		_deathCast = data.GetValueOrDefault("deathCast", false).AsBool();
		_pinataPresent = data.GetValueOrDefault("pinataPresent", false).AsBool();
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
		return new List<MethodInfo>(17)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CheckPinataPlaced, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HasPinataInCell, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ThrowThreeMagic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FindTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IsEnemyTransformablePlant, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
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
			new MethodInfo(MethodName.ThrowMagic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "emptyGrid", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName.CheckPinataPlaced && args.Count == 0)
		{
			CheckPinataPlaced();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasPinataInCell && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasPinataInCell());
			return true;
		}
		if (method == MethodName.ThrowThreeMagic && args.Count == 0)
		{
			ThrowThreeMagic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FindTargets && args.Count == 0)
		{
			FindTargets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsEnemyTransformablePlant && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsEnemyTransformablePlant(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
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
		if (method == MethodName.ThrowMagic && args.Count == 2)
		{
			ThrowMagic(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<Vector2I>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
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
		if (method == MethodName.CheckPinataPlaced)
		{
			return true;
		}
		if (method == MethodName.HasPinataInCell)
		{
			return true;
		}
		if (method == MethodName.ThrowThreeMagic)
		{
			return true;
		}
		if (method == MethodName.FindTargets)
		{
			return true;
		}
		if (method == MethodName.IsEnemyTransformablePlant)
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
		if (method == MethodName.ThrowMagic)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
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
		if (name == PropertyName._castPending)
		{
			_castPending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._deathCast)
		{
			_deathCast = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pinataPresent)
		{
			_pinataPresent = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._castPending)
		{
			value = VariantUtils.CreateFrom(in _castPending);
			return true;
		}
		if (name == PropertyName._deathCast)
		{
			value = VariantUtils.CreateFrom(in _deathCast);
			return true;
		}
		if (name == PropertyName._pinataPresent)
		{
			value = VariantUtils.CreateFrom(in _pinataPresent);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._castPending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._deathCast, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._pinataPresent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._castPending, Variant.From(in _castPending));
		info.AddProperty(PropertyName._deathCast, Variant.From(in _deathCast));
		info.AddProperty(PropertyName._pinataPresent, Variant.From(in _pinataPresent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._castPending, out var value))
		{
			_castPending = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._deathCast, out var value2))
		{
			_deathCast = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pinataPresent, out var value3))
		{
			_pinataPresent = value3.As<bool>();
		}
	}
}
