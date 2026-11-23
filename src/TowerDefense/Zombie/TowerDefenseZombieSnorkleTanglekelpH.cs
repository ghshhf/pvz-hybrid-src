using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter3/Snorkle/Scene/TanglekelpH/TowerDefenseZombieSnorkleTanglekelpH.cs")]
public class TowerDefenseZombieSnorkleTanglekelpH : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName AttackEntered = "AttackEntered";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName AttackExited = "AttackExited";

		public new static readonly StringName InWater = "InWater";

		public new static readonly StringName OutWater = "OutWater";

		public new static readonly StringName DieEntered = "DieEntered";

		public static readonly StringName AnimeStarted = "AnimeStarted";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName DamagePointReach = "DamagePointReach";

		public static readonly StringName TryCharmTarget = "TryCharmTarget";

		public static readonly StringName DragAwayPlant = "DragAwayPlant";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName Purify = "Purify";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName GlobalPositionX = "GlobalPositionX";

		public static readonly StringName _charmCooldown = "_charmCooldown";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private double _charmCooldown;

	private float GlobalPositionX
	{
		get
		{
			return GetLogicalGlobalPosition().X;
		}
		set
		{
			Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
			logicalGlobalPosition.X = value;
			SetLogicalGlobalPosition(logicalGlobalPosition);
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			base.sprite.OnAnimeStarted += AnimeStarted;
		}
	}

	public override void AttackEntered()
	{
		base.AttackEntered();
		if (base.inWater)
		{
			instance.maskFlags = 9;
		}
		TryCharmTarget();
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		if (_charmCooldown > 0.0)
		{
			_charmCooldown -= delta;
		}
		AttackComponent obj = attackComponent;
		if (obj != null && obj.CanAttack())
		{
			TryCharmTarget();
		}
	}

	public override void AttackExited()
	{
		base.AttackExited();
		if (base.inWater)
		{
			instance.maskFlags = 32;
		}
	}

	public override void InWater()
	{
		base.InWater();
		instance.maskFlags = 32;
	}

	public override void OutWater()
	{
		base.groundHeight = -100.0;
		base.z = -100.0;
		base.OutWater();
		CreateTween().TweenProperty(base.sprite, "offset", new Vector2(-50f, -80f), 0.25);
		GlobalPositionX -= base.Scale.X * transformPoint.Scale.X * 30f;
		instance.maskFlags = 9;
	}

	public override void DieEntered()
	{
		base.DieEntered();
		if (!base.inWater)
		{
			base.sprite.offset = new Vector2(-50f, -80f);
		}
	}

	public void AnimeStarted(string clip)
	{
		if (clip == "Swim")
		{
			base.sprite.offset = new Vector2(-10f, -100f);
		}
	}

	public override void AnimeCompleted(string clip)
	{
		if (clip == "Jump")
		{
			instance.maskFlags = 32;
			base.sprite.offset = new Vector2(-10f, -100f);
			GlobalPositionX -= base.Scale.X * transformPoint.Scale.X * 40f;
		}
		base.AnimeCompleted(clip);
	}

	public override void DamagePointReach(string damangePointName)
	{
		base.DamagePointReach(damangePointName);
		if (damangePointName == "Head")
		{
			base.sprite.GetNode<AdobeAnimateSpriteBase>("%Head").Visible = false;
		}
	}

	private void TryCharmTarget()
	{
		if (!TowerDefenseManager.HasGameplayAuthority || _charmCooldown > 0.0 || !base.inWater)
		{
			return;
		}
		TowerDefenseCharacter towerDefenseCharacter = attackComponent?.target;
		if (!GodotObject.IsInstanceValid(towerDefenseCharacter) || !GodotObject.IsInstanceValid(towerDefenseCharacter.instance) || towerDefenseCharacter.camp == base.camp || towerDefenseCharacter is TowerDefenseGravestone || towerDefenseCharacter is TowerDefenseItem || towerDefenseCharacter.die)
		{
			return;
		}
		_charmCooldown = 1.0;
		if (towerDefenseCharacter is TowerDefensePlant plant)
		{
			if (DragAwayPlant(plant))
			{
				Destroy();
			}
		}
		else
		{
			towerDefenseCharacter.Hypnoses();
		}
	}

	private bool DragAwayPlant(TowerDefensePlant plant)
	{
		if (!GodotObject.IsInstanceValid(plant) || plant.die || plant.isDestroy)
		{
			return false;
		}
		string packetName = plant.config?.name;
		TowerDefensePacketConfig towerDefensePacketConfig = (string.IsNullOrEmpty(packetName) ? null : TowerDefenseManager.GetPacketConfig(packetName));
		if (!GodotObject.IsInstanceValid(towerDefensePacketConfig) || !(towerDefensePacketConfig.characterConfig is TowerDefensePlantConfig))
		{
			plant.Hypnoses();
			return false;
		}
		Vector2I vector2I = plant.gridPos;
		double bodyHp = (GodotObject.IsInstanceValid(plant.instance) ? plant.instance.hitpoints : 0.0);
		int sourceSyncId = plant.syncId;
		plant.SilentlyRemoveForTransformation();
		towerDefensePacketConfig.overrideHypnoses = base.camp == TowerDefenseEnum.CHARACTER_CAMP.ZOMBIE;
		TowerDefenseCharacter towerDefenseCharacter = towerDefensePacketConfig.Plant(vector2I, playAudio: false, noLimit: false, default(EconomyAccountId), skipPlacementCheck: true);
		if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
		{
			return true;
		}
		TowerDefenseCharacter target = towerDefenseCharacter;
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(target) && GodotObject.IsInstanceValid(target.instance))
			{
				if (bodyHp > 0.0)
				{
					target.instance.hitpointsSave = bodyHp;
					target.instance.hitpoints = bodyHp;
				}
				TowerDefenseManager.PublishSpawnedCharacter(packetName, target, useCreate: true);
				if (Global.IsMultiplayerMode)
				{
					MultiPlayerManager multiPlayerManager = MultiPlayerManager.Instance;
					if (multiPlayerManager != null && multiPlayerManager.isHost && sourceSyncId >= 0)
					{
						MultiPlayerManager.Instance.SendCharacterDestroy(sourceSyncId);
					}
				}
			}
		}).CallDeferred();
		return true;
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary { ["charmCooldown"] = _charmCooldown };
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_charmCooldown = data.GetValueOrDefault("charmCooldown", 0.0).AsDouble();
	}

	public override void Purify()
	{
		if (!GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		if (Global.IsMultiplayerMode && !MultiPlayerManager.Instance.isHost)
		{
			Destroy();
			return;
		}
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("PlantTanglekelpH");
		if (cell.CanPacketPlant(packetConfig))
		{
			TowerDefenseCharacter towerDefenseCharacter = packetConfig.Plant(base.gridPos);
			towerDefenseCharacter.WakeUp();
			if (instance.hypnoses)
			{
				towerDefenseCharacter.Hypnoses();
			}
			if (Global.IsMultiplayerMode && MultiPlayerManager.Instance.isHost)
			{
				TowerDefenseControlNew currentControl = TowerDefenseManager.Instance.currentControl;
				if (GodotObject.IsInstanceValid(currentControl))
				{
					int nextSyncId = currentControl.GetNextSyncId();
					currentControl.RegisterSyncCharacter(nextSyncId, towerDefenseCharacter);
					MultiPlayerManager.Instance.SendSpawnCharacterAt("PlantTanglekelpH", base.gridPos.X, base.gridPos.Y, nextSyncId);
				}
			}
		}
		Destroy();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(15)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttackEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AttackExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.InWater, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OutWater, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DieEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DamagePointReach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "damangePointName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TryCharmTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DragAwayPlant, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "plant", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Purify, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.AttackEntered && args.Count == 0)
		{
			AttackEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttackProcessing && args.Count == 1)
		{
			AttackProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttackExited && args.Count == 0)
		{
			AttackExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InWater && args.Count == 0)
		{
			InWater();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OutWater && args.Count == 0)
		{
			OutWater();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DieEntered && args.Count == 0)
		{
			DieEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeStarted && args.Count == 1)
		{
			AnimeStarted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DamagePointReach && args.Count == 1)
		{
			DamagePointReach(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryCharmTarget && args.Count == 0)
		{
			TryCharmTarget();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DragAwayPlant && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(DragAwayPlant(VariantUtils.ConvertTo<TowerDefensePlant>(in args[0])));
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
		if (method == MethodName.Purify && args.Count == 0)
		{
			Purify();
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
		if (method == MethodName.AttackEntered)
		{
			return true;
		}
		if (method == MethodName.AttackProcessing)
		{
			return true;
		}
		if (method == MethodName.AttackExited)
		{
			return true;
		}
		if (method == MethodName.InWater)
		{
			return true;
		}
		if (method == MethodName.OutWater)
		{
			return true;
		}
		if (method == MethodName.DieEntered)
		{
			return true;
		}
		if (method == MethodName.AnimeStarted)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.DamagePointReach)
		{
			return true;
		}
		if (method == MethodName.TryCharmTarget)
		{
			return true;
		}
		if (method == MethodName.DragAwayPlant)
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
		if (method == MethodName.Purify)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.GlobalPositionX)
		{
			GlobalPositionX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._charmCooldown)
		{
			_charmCooldown = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.GlobalPositionX)
		{
			value = VariantUtils.CreateFrom<float>(GlobalPositionX);
			return true;
		}
		if (name == PropertyName._charmCooldown)
		{
			value = VariantUtils.CreateFrom(in _charmCooldown);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName._charmCooldown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.GlobalPositionX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.GlobalPositionX, Variant.From<float>(GlobalPositionX));
		info.AddProperty(PropertyName._charmCooldown, Variant.From(in _charmCooldown));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.GlobalPositionX, out var value))
		{
			GlobalPositionX = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._charmCooldown, out var value2))
		{
			_charmCooldown = value2.As<double>();
		}
	}
}
