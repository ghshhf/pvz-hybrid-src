using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter10/FumeShroomMagic/Scene/TowerDefensePlantFumeShroomMagic.cs")]
public class TowerDefensePlantFumeShroomMagic : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Attack = "Attack";

		public static readonly StringName IsValidSporeTarget = "IsValidSporeTarget";

		public static readonly StringName PlayFume = "PlayFume";

		public static readonly StringName SplitSpore = "SplitSpore";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private AttackComponent _attackComponent;

	private static PackedScene _fumeScene;

	private static PackedScene _sporeSplitFumeScene;

	private static TowerDefenseProjectileConfig _damageConfig;

	private static TowerDefenseProjectileCreateData _sporeSplitData;

	private static readonly Array<int> _sporeSplitLines = new Array<int> { -2, -1, 0, 1, 2 };

	private const double SporeSplitSpeed = 300.0;

	private const double SporeSplitRange = 0.4;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_attackComponent = componentManager.GetRuntime<AttackComponent>("character.attack.0");
			_attackComponent.OnAttack += Attack;
			_attackComponent.SetCheckAreaSegmentLengthX(0, TowerDefenseManager.Instance.GetMapGridSize().X * 6f);
		}
	}

	public override void _ExitTree()
	{
		AttackComponent attackComponent = _attackComponent;
		if (attackComponent != null && !attackComponent.IsReleased)
		{
			_attackComponent.OnAttack -= Attack;
		}
		base._ExitTree();
	}

	private void Attack()
	{
		PlayFume();
		bool hasGameplayAuthority = TowerDefenseManager.HasGameplayAuthority;
		if (_damageConfig == null)
		{
			_damageConfig = new TowerDefenseProjectileConfig
			{
				baseDamage = 40.0,
				damageFlags = 66
			};
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		List<TowerDefenseZombie> list = new List<TowerDefenseZombie>();
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		float num = towerDefenseManager.GetMapGridSize().X * 6f;
		foreach (Variant item in towerDefenseManager.GetCampTarget(base.camp))
		{
			if (!((TowerDefenseCharacter)(GodotObject)item is TowerDefenseZombie towerDefenseZombie) || !IsValidSporeTarget(towerDefenseZombie))
			{
				continue;
			}
			Vector2 logicalGlobalPosition2 = towerDefenseZombie.GetLogicalGlobalPosition();
			if (towerDefenseManager.GetMapGridPos(logicalGlobalPosition2).Y == base.gridPos.Y && !(logicalGlobalPosition2.X < logicalGlobalPosition.X) && !(logicalGlobalPosition2.X > logicalGlobalPosition.X + num))
			{
				list.Add(towerDefenseZombie);
				if (hasGameplayAuthority)
				{
					towerDefenseZombie.ProjectileHurt(null, _damageConfig);
				}
			}
		}
		if (!hasGameplayAuthority)
		{
			return;
		}
		foreach (TowerDefenseZombie item2 in list)
		{
			if (GodotObject.IsInstanceValid(item2))
			{
				SplitSpore(item2);
			}
		}
	}

	private bool IsValidSporeTarget(TowerDefenseZombie zombie)
	{
		if (!GodotObject.IsInstanceValid(zombie) || zombie.die || zombie.nearDie || zombie.isDestroy)
		{
			return false;
		}
		if (!CanTarget(zombie) || !CanCollision(zombie.instance.maskFlags))
		{
			return false;
		}
		if (zombie.instance.invincible || !zombie.instance.canBeCollection)
		{
			return false;
		}
		return zombie.IsTargetableFromLine(base.gridPos.Y);
	}

	private void PlayFume()
	{
		if (_fumeScene == null)
		{
			_fumeScene = GD.Load<PackedScene>("uid://cjg53vka0l4iu");
		}
		Node2D node2D = _fumeScene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition(GetNode<Marker2D>("SpriteGroup/TransformPoint/FumeShroomMagic/AdobeAnimateSlot/FireMarker"));
		node2D.Position = TowerDefenseGroundItemBase.characterNode.ToLocal(logicalGlobalPosition);
		TowerDefenseGroundItemBase.characterNode.AddChild(node2D, forceReadableName: false, InternalMode.Disabled);
		node2D.CreateTween().TweenCallback(Callable.From(node2D.QueueFree)).SetDelay(1.0);
		AudioManager.Instance.AudioPlay("Fume");
	}

	private void SplitSpore(TowerDefenseZombie target)
	{
		if (_sporeSplitData == null)
		{
			_sporeSplitData = new TowerDefenseProjectileCreateData("Puff")
			{
				baseDamage = 20.0,
				damageFlags = 3,
				size = TowerDefenseManager.Instance.GetMapGridSize(),
				penetrateNum = 6,
				overridePenetrateNum = true
			};
		}
		if (_sporeSplitFumeScene == null)
		{
			_sporeSplitFumeScene = GD.Load<PackedScene>("uid://dctvrjy5qe6rf");
		}
		TowerDefenseCharacterEventProjectileSplit.Run(null, target, _sporeSplitData, _sporeSplitLines, 300.0, 0.0, _useTargetGridY: true, _enemyOnly: true, _edgeCompensate: false, _sporeSplitFumeScene, 0.4, _vertical: true, base.config.collisionFlags);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(6)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Attack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IsValidSporeTarget, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "zombie", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.PlayFume, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SplitSpore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
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
		if (method == MethodName.Attack && args.Count == 0)
		{
			Attack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsValidSporeTarget && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsValidSporeTarget(VariantUtils.ConvertTo<TowerDefenseZombie>(in args[0])));
			return true;
		}
		if (method == MethodName.PlayFume && args.Count == 0)
		{
			PlayFume();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SplitSpore && args.Count == 1)
		{
			SplitSpore(VariantUtils.ConvertTo<TowerDefenseZombie>(in args[0]));
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
		if (method == MethodName.Attack)
		{
			return true;
		}
		if (method == MethodName.IsValidSporeTarget)
		{
			return true;
		}
		if (method == MethodName.PlayFume)
		{
			return true;
		}
		if (method == MethodName.SplitSpore)
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
