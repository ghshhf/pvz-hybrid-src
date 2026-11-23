using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Cover/DoomShroomGar/Scene/TowerDefensePlantDoomShroomGar.cs")]
public class TowerDefensePlantDoomShroomGar : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName SleepEntered = "SleepEntered";

		public static readonly StringName ExplodeStart = "ExplodeStart";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private ExplodeComponent _explodeComponent;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_explodeComponent = componentManager.GetRuntime<ExplodeComponent>();
			_explodeComponent.OnExplodeStart += ExplodeStart;
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (IsNodeReady() && !Engine.IsEditorHint())
		{
			ExplodeComponent explodeComponent = _explodeComponent;
			if (explodeComponent != null && !explodeComponent.IsReleased)
			{
				_explodeComponent.OnExplodeStart += ExplodeStart;
			}
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		ExplodeComponent explodeComponent = _explodeComponent;
		if (explodeComponent != null && !explodeComponent.IsReleased)
		{
			_explodeComponent.OnExplodeStart -= ExplodeStart;
		}
	}

	public override void SleepEntered()
	{
		base.SleepEntered();
		instance.invincible = false;
	}

	public void ExplodeStart()
	{
		ExplodeComponent explodeComponent = _explodeComponent;
		if (explodeComponent == null || explodeComponent.IsReleased || !GodotObject.IsInstanceValid(instance) || TowerDefenseManager.Instance == null)
		{
			return;
		}
		Vector2I vector2I = (Vector2I)_explodeComponent.explodeRange.Abs();
		List<TowerDefenseCharacter> charactersForGridWindowList = TowerDefenseManager.Instance.characterRegistry.GetCharactersForGridWindowList(base.gridPos.X, base.gridPos.Y, vector2I.X, vector2I.Y, includeExtendedPlants: true);
		Dictionary<int, TowerDefenseCharacter> dictionary = new Dictionary<int, TowerDefenseCharacter>();
		foreach (TowerDefenseCharacter item in charactersForGridWindowList)
		{
			if (!GodotObject.IsInstanceValid(item) || item.die || item.nearDie || item.camp == base.camp || !(item is TowerDefenseZombie) || !GodotObject.IsInstanceValid(item.instance) || item.instance.zombiePhysique == TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS)
			{
				continue;
			}
			int y = item.gridPos.Y;
			if (!dictionary.TryGetValue(y, out var value))
			{
				dictionary[y] = item;
				continue;
			}
			float x = item.GetLogicalGlobalPosition().X;
			float x2 = value.GetLogicalGlobalPosition().X;
			if (instance.hypnoses ? (x > x2) : (x < x2))
			{
				dictionary[y] = item;
			}
		}
		int num = 0;
		foreach (KeyValuePair<int, TowerDefenseCharacter> item2 in dictionary)
		{
			item2.Deconstruct(out var _, out var value2);
			TowerDefenseCharacter towerDefenseCharacter = value2;
			if (num >= 5)
			{
				break;
			}
			if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				continue;
			}
			Vector2 logicalGlobalPosition = towerDefenseCharacter.GetLogicalGlobalPosition();
			towerDefenseCharacter.skipDestroySet = true;
			towerDefenseCharacter.Destroy();
			TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("ZombieGargantuarRedEyes");
			TowerDefenseZombie zombie = (TowerDefenseZombie)packetConfig.Create(logicalGlobalPosition, towerDefenseCharacter.gridPos);
			TowerDefenseManager.GetCharacterNode().AddChild(zombie, forceReadableName: false, InternalMode.Disabled);
			if (!instance.hypnoses)
			{
				zombie.Hypnoses();
			}
			zombie.SetMainStateMachineDispatchEnabled(enabled: false);
			Tween tween = zombie.CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Back);
			tween.TweenProperty(zombie.transformPoint, "scale", Vector2.One, 0.5).From(Vector2.One * 0.5f);
			tween.Finished += delegate
			{
				if (GodotObject.IsInstanceValid(zombie))
				{
					zombie.Walk();
					zombie.SetMainStateMachineDispatchEnabled(enabled: true);
				}
			};
			if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
			{
				int nextSyncId = TowerDefenseManager.Instance.currentControl.GetNextSyncId();
				TowerDefenseManager.Instance.currentControl.RegisterSyncCharacter(nextSyncId, zombie);
				MultiPlayerManager.Instance.SendSpawnCharacterAt("ZombieGargantuarRedEyes", towerDefenseCharacter.gridPos.X, towerDefenseCharacter.gridPos.Y, nextSyncId, 1.0, 1.0, !instance.hypnoses, 0.0, useCreate: true, logicalGlobalPosition.X, logicalGlobalPosition.Y);
			}
			num++;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(5)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SleepEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExplodeStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SleepEntered && args.Count == 0)
		{
			SleepEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExplodeStart && args.Count == 0)
		{
			ExplodeStart();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SleepEntered)
		{
			return true;
		}
		if (method == MethodName.ExplodeStart)
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
