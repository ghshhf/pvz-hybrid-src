using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter10/ScaredyShroomElf/Scene/TowerDefensePlantScaredyShroomElf.cs")]
public class TowerDefensePlantScaredyShroomElf : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public static readonly StringName SuppressZombieTargeting = "SuppressZombieTargeting";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName IsScared = "IsScared";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName Vanish = "Vanish";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _lifetimeRemaining = "_lifetimeRemaining";

		public static readonly StringName _lifetimeLastPhysicsFrame = "_lifetimeLastPhysicsFrame";

		public static readonly StringName _vanished = "_vanished";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private static PackedScene _imitaterCloud;

	private ScaredComponent _scaredComponent;

	private double _lifetimeRemaining = 50.0;

	private ulong _lifetimeLastPhysicsFrame = 18446744073709551615uL;

	private bool _vanished;

	private static PackedScene ImitaterCloud => _imitaterCloud ?? (_imitaterCloud = GD.Load<PackedScene>("uid://djvfnrjg7vtqn"));

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode && inGame)
		{
			_scaredComponent = componentManager.GetRuntime<ScaredComponent>("character.scared");
			SuppressZombieTargeting();
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (!Engine.IsEditorHint() && IsNodeReady() && inGame)
		{
			SuppressZombieTargeting();
		}
	}

	private void SuppressZombieTargeting()
	{
		targetRegistrationComponent?.UnregisterTarget();
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(this) && !isDestroy)
			{
				TargetRegistrationComponent obj = targetRegistrationComponent;
				if (obj != null && !obj.IsReleased)
				{
					targetRegistrationComponent.UnregisterTarget();
					targetRegistrationComponent.canProjectileCheck = false;
				}
			}
		}).CallDeferred();
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || editorPreviewMode || !inGame || _vanished || !TowerDefenseManager.HasGameplayAuthority || !TowerDefenseManager._IsGameRunning())
		{
			return;
		}
		ulong currentPhysicsFrame = TowerDefenseProcessModeDispatch.CurrentPhysicsFrame;
		if (currentPhysicsFrame != 18446744073709551615uL && _lifetimeLastPhysicsFrame == currentPhysicsFrame)
		{
			return;
		}
		_lifetimeLastPhysicsFrame = currentPhysicsFrame;
		if (CanRunAbility())
		{
			_lifetimeRemaining = Mathf.Max(0.0, _lifetimeRemaining - Math.Max(0.0, delta) * timeScale);
			if (_lifetimeRemaining <= 0.0 && !IsScared())
			{
				Vanish(releaseSpores: false);
			}
		}
	}

	private bool IsScared()
	{
		ScaredComponent scaredComponent = _scaredComponent;
		if (scaredComponent == null || scaredComponent.StateMachine?.GetStateById("scared.down")?.IsActive != true)
		{
			ScaredComponent scaredComponent2 = _scaredComponent;
			if (scaredComponent2 == null)
			{
				return false;
			}
			return scaredComponent2.StateMachine?.GetStateById("scared.scared_idle")?.IsActive == true;
		}
		return true;
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Scared" && IsScared())
		{
			Vanish(releaseSpores: true);
		}
	}

	private void Vanish(bool releaseSpores)
	{
		if (_vanished || die || isDestroy || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		_vanished = true;
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		if (releaseSpores)
		{
			TowerDefenseProjectileCreateData projectileData = new TowerDefenseProjectileCreateData("PuffShroomElfPuff")
			{
				baseDamage = 20.0,
				damageFlags = 82
			};
			float num = (float)(GetGroundHeight(logicalGlobalPosition.Y) + 10.0);
			for (int i = 0; i < 36; i++)
			{
				float num2 = Mathf.DegToRad(i * 10);
				BulletFieldSpawnOverrides overrides = new BulletFieldSpawnOverrides
				{
					checkAllOverride = true,
					initialRotationOverride = num2
				};
				FireComponent.CreateProjectilePositionByData(this, null, num, logicalGlobalPosition, Vector2.FromAngle(num2) * 200f, projectileData, -1, base.camp, default(Vector2), overrides);
			}
		}
		TowerDefenseEffectParticlesOnce towerDefenseEffectParticlesOnce = TowerDefenseManager.CreateEffectParticlesOnce(ImitaterCloud, base.gridPos);
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseEffectParticlesOnce, forceReadableName: false, InternalMode.Disabled);
		towerDefenseEffectParticlesOnce.GlobalPosition = logicalGlobalPosition;
		if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
		{
			MultiPlayerManager.Instance.SendProjectileEffectSpawn(new ProjectileEffectSpawnDto
			{
				effect_id = "scaredy_shroom_elf_smoke",
				px = logicalGlobalPosition.X,
				py = logicalGlobalPosition.Y,
				grid_x = base.gridPos.X,
				grid_y = base.gridPos.Y,
				camp = (int)base.camp
			});
		}
		Destroy();
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary { ["lifetimeRemaining"] = _lifetimeRemaining };
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_lifetimeRemaining = Mathf.Clamp(data.GetValueOrDefault("lifetimeRemaining", 50.0).AsDouble(), 0.0, 50.0);
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
		return new List<MethodInfo>(11)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SuppressZombieTargeting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsScared, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Vanish, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "releaseSpores", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SuppressZombieTargeting && args.Count == 0)
		{
			SuppressZombieTargeting();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsScared && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsScared());
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Vanish && args.Count == 1)
		{
			Vanish(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName.SuppressZombieTargeting)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.IsScared)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.Vanish)
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
		if (name == PropertyName._lifetimeRemaining)
		{
			_lifetimeRemaining = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._lifetimeLastPhysicsFrame)
		{
			_lifetimeLastPhysicsFrame = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._vanished)
		{
			_vanished = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._lifetimeRemaining)
		{
			value = VariantUtils.CreateFrom(in _lifetimeRemaining);
			return true;
		}
		if (name == PropertyName._lifetimeLastPhysicsFrame)
		{
			value = VariantUtils.CreateFrom(in _lifetimeLastPhysicsFrame);
			return true;
		}
		if (name == PropertyName._vanished)
		{
			value = VariantUtils.CreateFrom(in _vanished);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName._lifetimeRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._lifetimeLastPhysicsFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._vanished, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._lifetimeRemaining, Variant.From(in _lifetimeRemaining));
		info.AddProperty(PropertyName._lifetimeLastPhysicsFrame, Variant.From(in _lifetimeLastPhysicsFrame));
		info.AddProperty(PropertyName._vanished, Variant.From(in _vanished));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._lifetimeRemaining, out var value))
		{
			_lifetimeRemaining = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._lifetimeLastPhysicsFrame, out var value2))
		{
			_lifetimeLastPhysicsFrame = value2.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._vanished, out var value3))
		{
			_vanished = value3.As<bool>();
		}
	}
}
