using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Other/BedHypno/Scene/TowerDefenseZombieBedHypno.cs")]
public class TowerDefenseZombieBedHypno : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public static readonly StringName IsHypnosisBlocked = "IsHypnosisBlocked";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName SpawnDeathReplacement = "SpawnDeathReplacement";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public new static readonly StringName WakeTriggersDeathrattle = "WakeTriggersDeathrattle";

		public static readonly StringName _deathReplacementSpawned = "_deathReplacementSpawned";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private bool _deathReplacementSpawned;

	private readonly HashSet<TowerDefenseCharacter> _contacts = new HashSet<TowerDefenseCharacter>();

	private const double WalkClipFrameRatio = 17.0 / 18.0;

	public override bool WakeTriggersDeathrattle => true;

	public override void WalkProcessing(double delta)
	{
		base.walkSpeedScale = ((instance.hitpoints < instance.hitpointsSave * 0.5) ? 0.26 : 0.52);
		Vector2 globalPositionForPhysicsFrame = GetGlobalPositionForPhysicsFrame(TowerDefenseProcessModeDispatch.CurrentPhysicsFrame);
		double num = base.walkSpeedScale * (17.0 / 18.0);
		if ((double)globalPositionForPhysicsFrame.X > TowerDefenseManager.Instance.GetMapGroundRight())
		{
			base.sprite.timeScale = timeScale * num * 2.0;
		}
		else
		{
			base.sprite.timeScale = timeScale * num;
		}
		if (!TowerDefenseManager.HasGameplayAuthority || !CanRunAbility())
		{
			return;
		}
		AttackComponent obj = attackComponent;
		if (obj == null || obj.IsReleased)
		{
			return;
		}
		_contacts.RemoveWhere((TowerDefenseCharacter target) => !GodotObject.IsInstanceValid(target) || target.isDestroy || !base.WorldHitRect.Intersects(target.WorldHitRect, includeBorders: true));
		if (!attackComponent.CanAttackOnContact())
		{
			return;
		}
		TowerDefenseCharacter towerDefenseCharacter = attackComponent.target;
		if (!GodotObject.IsInstanceValid(towerDefenseCharacter) || !_contacts.Add(towerDefenseCharacter))
		{
			return;
		}
		if (GodotObject.IsInstanceValid(towerDefenseCharacter.cell) && towerDefenseCharacter.cell.HasSpike())
		{
			TowerDefenseCharacter spike = towerDefenseCharacter.cell.GetSpike();
			if (spike.CanTarget(this))
			{
				towerDefenseCharacter = spike;
			}
		}
		if ((towerDefenseCharacter.instance.physiqueTypeFlags & 0x10) != 0)
		{
			if (towerDefenseCharacter.instance.spikeHurt != -1.0)
			{
				TowerDefenseCharacter towerDefenseCharacter2 = towerDefenseCharacter;
				double spikeHurt = towerDefenseCharacter.instance.spikeHurt;
				towerDefenseCharacter2.Hurt(100000.0, playSplatAudio: true, default(Vector2), createDamagePart: true, spikeHurt);
			}
			Die();
			return;
		}
		attackComponent.target = towerDefenseCharacter;
		if (towerDefenseCharacter.instance.HasBurstWheelArmor() || IsHypnosisBlocked(towerDefenseCharacter))
		{
			attackComponent.SmashAttack(((TowerDefenseZombieConfig)base.config).smashAttack);
			return;
		}
		bool hypnoses = towerDefenseCharacter.instance.hypnoses;
		towerDefenseCharacter.Hypnoses(15.0);
		if (towerDefenseCharacter.instance.hypnoses == hypnoses)
		{
			attackComponent.SmashAttack(((TowerDefenseZombieConfig)base.config).smashAttack);
		}
	}

	private static bool IsHypnosisBlocked(TowerDefenseCharacter target)
	{
		if ((target.instance.unUseBuffFlags & 8) != 0 || target.instance.hologram || target.instance.shieldValue > 0.0)
		{
			return true;
		}
		if (!(target is TowerDefensePlant) || !GodotObject.IsInstanceValid(target.cell))
		{
			return false;
		}
		TowerDefenseItemSheild itemShield = target.cell.itemShield;
		if (GodotObject.IsInstanceValid(itemShield) && !itemShield.die && !itemShield.isDestroy && GodotObject.IsInstanceValid(itemShield.instance) && itemShield.instance.hitpoints > 0.0)
		{
			return itemShield.instance.hypnoses == target.instance.hypnoses;
		}
		return false;
	}

	public override void AttackProcessing(double delta)
	{
		WalkProcessing(delta);
	}

	public override void SpawnDeathReplacement()
	{
		if (_deathReplacementSpawned)
		{
			return;
		}
		_deathReplacementSpawned = true;
		if (!TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("ZombieHypno");
		if (!GodotObject.IsInstanceValid(packetConfig))
		{
			return;
		}
		TowerDefenseCharacter replacement = (base.HasEconomyOwner ? packetConfig.Create(base.EconomyOwnerAccountId, GetLogicalGlobalPosition(), base.gridPos) : packetConfig.Create(GetLogicalGlobalPosition(), base.gridPos));
		if (!GodotObject.IsInstanceValid(replacement))
		{
			return;
		}
		double hitpointScale = instance.hitpointScale;
		Vector2 scale = transformPoint.Scale.Abs();
		bool hypnotized = instance.hypnoses;
		bool hidden = base.invisible;
		bool sandboxTeams = base.UsesSandboxTeams;
		InheritSandboxTeam(replacement);
		Callable.From(delegate
		{
			Node2D node2D = TowerDefenseManager.GetCharacterNode();
			if (GodotObject.IsInstanceValid(replacement))
			{
				if (!GodotObject.IsInstanceValid(node2D))
				{
					replacement.Free();
				}
				else
				{
					node2D.AddChild(replacement, forceReadableName: false, InternalMode.Disabled);
					replacement.instance.hitpointScale = hitpointScale;
					replacement.transformPoint.Scale = scale;
					replacement.invisible = hidden;
					if (hypnotized && !sandboxTeams)
					{
						replacement.Hypnoses();
					}
					if (replacement is TowerDefenseZombie towerDefenseZombie)
					{
						towerDefenseZombie.WalkReady();
					}
					TowerDefenseManager.PublishSpawnedCharacter("ZombieHypno", replacement, useCreate: true);
				}
			}
		}).CallDeferred();
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary { ["deathReplacementSpawned"] = _deathReplacementSpawned };
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_deathReplacementSpawned = data.GetValueOrDefault("deathReplacementSpawned", false).AsBool();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportVariantSave();
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		ImportVariantSave(data);
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return _deathReplacementSpawned ? 1 : 0;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(9)
		{
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsHypnosisBlocked, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SpawnDeathReplacement, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.WalkProcessing && args.Count == 1)
		{
			WalkProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsHypnosisBlocked && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsHypnosisBlocked(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.AttackProcessing && args.Count == 1)
		{
			AttackProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnDeathReplacement && args.Count == 0)
		{
			SpawnDeathReplacement();
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
		if (method == MethodName.GetNetworkSpecialStateRevision && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(GetNetworkSpecialStateRevision());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IsHypnosisBlocked && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsHypnosisBlocked(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.WalkProcessing)
		{
			return true;
		}
		if (method == MethodName.IsHypnosisBlocked)
		{
			return true;
		}
		if (method == MethodName.AttackProcessing)
		{
			return true;
		}
		if (method == MethodName.SpawnDeathReplacement)
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
		if (method == MethodName.GetNetworkSpecialStateRevision)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._deathReplacementSpawned)
		{
			_deathReplacementSpawned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.WakeTriggersDeathrattle)
		{
			value = VariantUtils.CreateFrom<bool>(WakeTriggersDeathrattle);
			return true;
		}
		if (name == PropertyName._deathReplacementSpawned)
		{
			value = VariantUtils.CreateFrom(in _deathReplacementSpawned);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._deathReplacementSpawned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.WakeTriggersDeathrattle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._deathReplacementSpawned, Variant.From(in _deathReplacementSpawned));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._deathReplacementSpawned, out var value))
		{
			_deathReplacementSpawned = value.As<bool>();
		}
	}
}
