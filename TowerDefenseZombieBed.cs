using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter1/Bed/Scene/TowerDefenseZombieBed.cs")]
public class TowerDefenseZombieBed : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName DieEntered = "DieEntered";

		public new static readonly StringName SpawnDeathReplacement = "SpawnDeathReplacement";

		public new static readonly StringName DestroySet = "DestroySet";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public new static readonly StringName WakeTriggersDeathrattle = "WakeTriggersDeathrattle";

		public static readonly StringName over = "over";

		public static readonly StringName _deathReplacementSpawned = "_deathReplacementSpawned";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	public bool over;

	private bool _deathReplacementSpawned;

	public override bool WakeTriggersDeathrattle => true;

	public override void WalkProcessing(double delta)
	{
		if ((double)GetGlobalPositionForPhysicsFrame(TowerDefenseProcessModeDispatch.CurrentPhysicsFrame).X > TowerDefenseManager.Instance.GetMapGroundRight())
		{
			base.sprite.timeScale = timeScale * base.walkSpeedScale * 2.0;
		}
		else
		{
			base.sprite.timeScale = timeScale * base.walkSpeedScale;
		}
		if (!attackComponent.CanAttack())
		{
			return;
		}
		if (GodotObject.IsInstanceValid(attackComponent.target) && GodotObject.IsInstanceValid(attackComponent.target.cell) && attackComponent.target.cell.HasSpike())
		{
			attackComponent.target = attackComponent.target.cell.GetSpike();
		}
		if ((attackComponent.target.instance.physiqueTypeFlags & 0x10) == 0)
		{
			attackComponent.SmashAttackCell(((TowerDefenseZombieConfig)base.config).smashAttack);
			return;
		}
		if (attackComponent.target.instance.spikeHurt != -1.0)
		{
			TowerDefenseCharacter target = attackComponent.target;
			double spikeHurt = attackComponent.target.instance.spikeHurt;
			target.Hurt(100000.0, playSplatAudio: true, default(Vector2), createDamagePart: true, spikeHurt);
		}
		Die();
	}

	public override void DieEntered()
	{
		base.DieEntered();
		DestroySet();
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
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("ZombieSleeper");
		if (!GodotObject.IsInstanceValid(packetConfig))
		{
			return;
		}
		Vector2 pos = GetLogicalGlobalPosition() + new Vector2(45f, 0f);
		TowerDefenseCharacter sleeper = (base.EconomyOwnerAccountId.IsValid ? packetConfig.Create(base.EconomyOwnerAccountId, pos, base.gridPos, 50.0) : packetConfig.Create(pos, base.gridPos, 50.0));
		if (!GodotObject.IsInstanceValid(sleeper))
		{
			return;
		}
		TowerDefenseManager.GetCharacterNode().CallDeferred("add_child", sleeper);
		if (instance.hypnoses)
		{
			Callable.From(delegate
			{
				if (GodotObject.IsInstanceValid(sleeper))
				{
					sleeper.Hypnoses();
				}
			}).CallDeferred();
		}
		double _hitpointScale = instance.hitpointScale;
		Vector2 _scale = transformPoint.Scale;
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(sleeper))
			{
				if (GodotObject.IsInstanceValid(sleeper.instance))
				{
					sleeper.instance.hitpointScale = _hitpointScale;
				}
				if (GodotObject.IsInstanceValid(sleeper.transformPoint))
				{
					sleeper.transformPoint.Scale = _scale;
				}
			}
		}).CallDeferred();
		sleeper.SetDeferred("invisible", base.invisible);
		double rotationFrom = 90f * base.Scale.X;
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(sleeper))
			{
				sleeper.CreateTween().TweenProperty(sleeper, "rotation_degrees", 0, 0.2).From(rotationFrom);
			}
		}).CallDeferred();
		GetTree().CreateTimer(0.1, processAlways: false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid(sleeper))
			{
				((TowerDefenseZombie)sleeper).Walk();
			}
		};
		Dictionary spawnState = new Dictionary
		{
			["spawn_invisible"] = base.invisible,
			["rotation_from_degrees"] = rotationFrom,
			["rotation_to_degrees"] = 0.0,
			["rotation_duration"] = 0.2,
			["walk_delay"] = 0.1
		};
		TowerDefenseManager.PublishSpawnedCharacter("ZombieSleeper", sleeper, useCreate: true, 0.0, walkAfterSpawn: true, "", spawnState);
	}

	public override async void DestroySet()
	{
		if (!over)
		{
			over = true;
			SpawnDeathReplacement();
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(4)
		{
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DieEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SpawnDeathReplacement, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.DieEntered && args.Count == 0)
		{
			DieEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnDeathReplacement && args.Count == 0)
		{
			SpawnDeathReplacement();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.WalkProcessing)
		{
			return true;
		}
		if (method == MethodName.DieEntered)
		{
			return true;
		}
		if (method == MethodName.SpawnDeathReplacement)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.over)
		{
			over = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
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
		if (name == PropertyName.over)
		{
			value = VariantUtils.CreateFrom(in over);
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
			new PropertyInfo(Variant.Type.Bool, PropertyName.over, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.WakeTriggersDeathrattle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._deathReplacementSpawned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.over, Variant.From(in over));
		info.AddProperty(PropertyName._deathReplacementSpawned, Variant.From(in _deathReplacementSpawned));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.over, out var value))
		{
			over = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._deathReplacementSpawned, out var value2))
		{
			_deathReplacementSpawned = value2.As<bool>();
		}
	}
}
