using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter8/BungiSquash/Scene/TowerDefensePlantBungiSquash.cs")]
public class TowerDefensePlantBungiSquash : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName PreSpawn = "PreSpawn";

		public static readonly StringName ApplyPreSpawnAirdrop = "ApplyPreSpawnAirdrop";

		public static readonly StringName OnLanded = "OnLanded";

		public static readonly StringName SummonBungi = "SummonBungi";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private SquashComponent _squashComponent;

	private AttackComponent _attackComponent;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_squashComponent = componentManager.GetRuntime<SquashComponent>();
			_attackComponent = componentManager.GetRuntime<AttackComponent>("character.attack.0");
			if (TowerDefenseManager.Instance.IsGameRunning())
			{
				instance.biteHurt = 0.0;
				base.z = 600.0;
				isGround = false;
				base.sprite.SetAnimation("JumpUp", loop: false);
				base.OnLand += OnLanded;
			}
		}
	}

	public override void PreSpawn()
	{
		base.PreSpawn();
		if (IsNodeReady())
		{
			ApplyPreSpawnAirdrop();
		}
		else
		{
			Callable.From(ApplyPreSpawnAirdrop).CallDeferred();
		}
	}

	private void ApplyPreSpawnAirdrop()
	{
		if (GodotObject.IsInstanceValid(this) && IsNodeReady())
		{
			instance.biteHurt = 0.0;
			base.z = 600.0;
			isGround = false;
			base.sprite.SetAnimation("JumpUp", loop: false);
			base.OnLand += OnLanded;
		}
	}

	public void OnLanded()
	{
		if (GodotObject.IsInstanceValid(this))
		{
			base.OnLand -= OnLanded;
		}
		if (isDestroy || isShovel)
		{
			return;
		}
		base.sprite.SetAnimation("JumpDown", loop: false);
		SummonBungi();
		GetTree().CreateTimer(0.5, processAlways: false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid(this))
			{
				Destroy();
			}
		};
	}

	public void SummonBungi()
	{
		if (!TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		TowerDefenseZombieBungi towerDefenseZombieBungi = (base.EconomyOwnerAccountId.IsValid ? (TowerDefenseCharacter.CreateCharacter(base.EconomyOwnerAccountId, "ZombieBungi", logicalGlobalPosition, base.gridPos, 0.0) as TowerDefenseZombieBungi) : (TowerDefenseCharacter.CreateCharacter("ZombieBungi", logicalGlobalPosition, base.gridPos, 0.0) as TowerDefenseZombieBungi));
		if (GodotObject.IsInstanceValid(towerDefenseZombieBungi))
		{
			Dictionary dictionary = new Dictionary { ["skipBungeeTarget"] = true };
			towerDefenseZombieBungi.ImportNetworkSpawnState(dictionary);
			if (!instance.hypnoses)
			{
				towerDefenseZombieBungi.Hypnoses();
			}
			TowerDefenseManager.PublishSpawnedCharacter("ZombieBungi", towerDefenseZombieBungi, useCreate: true, 0.0, walkAfterSpawn: true, "", dictionary);
			towerDefenseZombieBungi.CallDeferred("Walk");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(5)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PreSpawn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyPreSpawnAirdrop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnLanded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SummonBungi, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.PreSpawn && args.Count == 0)
		{
			PreSpawn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyPreSpawnAirdrop && args.Count == 0)
		{
			ApplyPreSpawnAirdrop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLanded && args.Count == 0)
		{
			OnLanded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SummonBungi && args.Count == 0)
		{
			SummonBungi();
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
		if (method == MethodName.PreSpawn)
		{
			return true;
		}
		if (method == MethodName.ApplyPreSpawnAirdrop)
		{
			return true;
		}
		if (method == MethodName.OnLanded)
		{
			return true;
		}
		if (method == MethodName.SummonBungi)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
