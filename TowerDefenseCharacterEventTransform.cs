using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Event/General/TowerDefenseCharacterEventTransform.cs")]
public class TowerDefenseCharacterEventTransform : TowerDefenseCharacterEventBase
{
	public new class MethodName : TowerDefenseCharacterEventBase.MethodName
	{
		public new static readonly StringName Execute = "Execute";

		public new static readonly StringName ExecuteDps = "ExecuteDps";

		public new static readonly StringName ExecuteProject = "ExecuteProject";

		public new static readonly StringName ExecuteGroundProject = "ExecuteGroundProject";

		public static readonly StringName RunPlantTransform = "RunPlantTransform";

		public static readonly StringName TransformEnemyPlantsAt = "TransformEnemyPlantsAt";

		public static readonly StringName HasEnemyPlant = "HasEnemyPlant";

		public static readonly StringName IsEnemyTransformablePlant = "IsEnemyTransformablePlant";

		public static readonly StringName IsValidTarget = "IsValidTarget";

		public static readonly StringName CanTransform = "CanTransform";

		public static readonly StringName Run = "Run";
	}

	public new class PropertyName : TowerDefenseCharacterEventBase.PropertyName
	{
		public static readonly StringName transformName = "transformName";

		public static readonly StringName effectScene = "effectScene";

		public static readonly StringName effectParticles = "effectParticles";

		public static readonly StringName networkEffectId = "networkEffectId";

		public static readonly StringName excludeImpFamily = "excludeImpFamily";

		public static readonly StringName plantTransformName = "plantTransformName";
	}

	public new class SignalName : TowerDefenseCharacterEventBase.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public string transformName = "ZombieImp";

	[Export(PropertyHint.None, "")]
	public PackedScene effectScene;

	[Export(PropertyHint.None, "")]
	public bool effectParticles;

	[Export(PropertyHint.None, "")]
	public string networkEffectId = "";

	[Export(PropertyHint.None, "")]
	public bool excludeImpFamily = true;

	[Export(PropertyHint.None, "")]
	public string plantTransformName = "";

	private const string TransformMetaKey = "CharacterTransform";

	public override void Execute(Vector2 pos, TowerDefenseCharacter target)
	{
		Run(transformName, target, effectScene, networkEffectId, excludeImpFamily, effectParticles);
	}

	public override void ExecuteDps(Vector2 pos, TowerDefenseCharacter target, double delta)
	{
		Run(transformName, target, effectScene, networkEffectId, excludeImpFamily, effectParticles);
	}

	public override void ExecuteProject(TowerDefenseProjectile projectile, TowerDefenseCharacter target)
	{
		if (!RunPlantTransform(target, projectile))
		{
			Run(transformName, target, effectScene, networkEffectId, excludeImpFamily, effectParticles);
		}
	}

	public override void ExecuteGroundProject(TowerDefenseProjectile projectile, Vector2 position, Vector2I gridPos)
	{
		TransformEnemyPlantsAt(gridPos, projectile);
		if (GodotObject.IsInstanceValid(projectile.target) && projectile.target.camp != projectile.camp)
		{
			Run(transformName, projectile.target, effectScene, networkEffectId, excludeImpFamily, effectParticles);
		}
	}

	private bool RunPlantTransform(TowerDefenseCharacter target, TowerDefenseProjectile projectile)
	{
		if (string.IsNullOrEmpty(plantTransformName))
		{
			return false;
		}
		if (!(target is TowerDefensePlant))
		{
			return false;
		}
		TransformEnemyPlantsAt(target.gridPos, projectile);
		return true;
	}

	private void TransformEnemyPlantsAt(Vector2I gridPos, TowerDefenseProjectile projectile)
	{
		if (string.IsNullOrEmpty(plantTransformName) || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(gridPos);
		if (!GodotObject.IsInstanceValid(mapCell))
		{
			return;
		}
		TowerDefenseEnum.CHARACTER_CAMP casterCamp = projectile?.camp ?? TowerDefenseEnum.CHARACTER_CAMP.PLANT;
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(plantTransformName);
		if (packetConfig != null && packetConfig.characterConfig is TowerDefensePlantConfig && HasEnemyPlant(mapCell, casterCamp))
		{
			mapCell.TransformPlantsTo(plantTransformName, effectScene, (TowerDefenseCharacter character) => !IsEnemyTransformablePlant(character, casterCamp), effectParticles);
		}
	}

	private static bool HasEnemyPlant(TowerDefenseCellInstance cell, TowerDefenseEnum.CHARACTER_CAMP casterCamp)
	{
		foreach (TowerDefenseCharacter character in cell.GetCharacterList())
		{
			if (IsEnemyTransformablePlant(character, casterCamp))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsEnemyTransformablePlant(TowerDefenseCharacter character, TowerDefenseEnum.CHARACTER_CAMP casterCamp)
	{
		if (!GodotObject.IsInstanceValid(character) || !GodotObject.IsInstanceValid(character.instance))
		{
			return false;
		}
		if (!(character is TowerDefensePlant))
		{
			return false;
		}
		if (character.die || character.nearDie || character.isDestroy)
		{
			return false;
		}
		if (!character.instance.canBeCollection)
		{
			return false;
		}
		return character.camp != casterCamp;
	}

	public static bool IsValidTarget(TowerDefenseCharacter target)
	{
		if (!GodotObject.IsInstanceValid(target) || !GodotObject.IsInstanceValid(target.instance))
		{
			return false;
		}
		if (!target.die && !target.nearDie && !target.isDestroy && !target.IsQueuedForDeletion() && target.inGame && !target.editorPreviewMode && target.instance.canBeCollection && !target.instance.invincible && !target.instance.hologram && target.instance.hitpoints > 0.0)
		{
			TargetRegistrationComponent targetRegistrationComponent = target.targetRegistrationComponent;
			if (targetRegistrationComponent != null && !targetRegistrationComponent.IsReleased && target.targetRegistrationComponent.canProjectileCheck)
			{
				return target.IsHitBoxEnabled;
			}
		}
		return false;
	}

	public static bool CanTransform(TowerDefenseCharacter target, string transformName, bool excludeImpFamily)
	{
		if (!IsValidTarget(target) || !(target is TowerDefenseZombie))
		{
			return false;
		}
		bool flag = excludeImpFamily;
		if (flag)
		{
			bool flag2 = ((target is TowerDefenseZombieImpBase || target is TowerDefenseZombieImpCloud || target is TowerDefenseZombieImpDiggerSpike || target is TowerDefenseZombieImpScaredy || target is TowerDefenseZombieImpSnail) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			return false;
		}
		if (GodotObject.IsInstanceValid(target.config) && target.config.name == transformName)
		{
			return false;
		}
		if (target.instance.zombiePhysique != TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS)
		{
			return !target.HasMeta("CharacterTransform");
		}
		return false;
	}

	public static void Run(string transformName, TowerDefenseCharacter target, PackedScene effectScene, string networkEffectId = "", bool excludeImpFamily = true, bool effectParticles = false)
	{
		if (TowerDefenseManager.HasGameplayAuthority && CanTransform(target, transformName, excludeImpFamily))
		{
			target.SetMeta("CharacterTransform", true);
			if (!GodotObject.IsInstanceValid(target.TransformTo(transformName, delegate(TowerDefenseCharacter node)
			{
				TowerDefenseManager.PublishSpawnedCharacter(transformName, node, useCreate: true, 0.0, walkAfterSpawn: true);
				if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost && target.syncId >= 0)
				{
					MultiPlayerManager.Instance.SendCharacterDestroy(target.syncId);
				}
			}, effectScene, syncCamp: true, syncBodyHp: false, syncHypnoses: true, removeSource: true, effectParticles)))
			{
				target.RemoveMeta("CharacterTransform");
			}
			else if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost && networkEffectId != "")
			{
				Vector2 logicalGlobalPosition = target.GetLogicalGlobalPosition();
				MultiPlayerManager.Instance.SendProjectileEffectSpawn(new ProjectileEffectSpawnDto
				{
					effect_id = networkEffectId,
					px = logicalGlobalPosition.X,
					py = logicalGlobalPosition.Y,
					grid_x = target.gridPos.X,
					grid_y = target.gridPos.Y,
					camp = (int)target.camp
				});
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(11)
		{
			new MethodInfo(MethodName.Execute, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ExecuteDps, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExecuteProject, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projectile", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ExecuteGroundProject, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projectile", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "gridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RunPlantTransform, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Object, "projectile", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.TransformEnemyPlantsAt, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "gridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "projectile", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.HasEnemyPlant, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Int, "casterCamp", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsEnemyTransformablePlant, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Int, "casterCamp", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsValidTarget, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.CanTransform, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.String, "transformName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "excludeImpFamily", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Run, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "transformName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Object, "effectScene", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("PackedScene"), exported: false),
				new PropertyInfo(Variant.Type.String, "networkEffectId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "excludeImpFamily", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "effectParticles", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Execute && args.Count == 2)
		{
			Execute(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExecuteDps && args.Count == 3)
		{
			ExecuteDps(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExecuteProject && args.Count == 2)
		{
			ExecuteProject(VariantUtils.ConvertTo<TowerDefenseProjectile>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExecuteGroundProject && args.Count == 3)
		{
			ExecuteGroundProject(VariantUtils.ConvertTo<TowerDefenseProjectile>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Vector2I>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RunPlantTransform && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(RunPlantTransform(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<TowerDefenseProjectile>(in args[1])));
			return true;
		}
		if (method == MethodName.TransformEnemyPlantsAt && args.Count == 2)
		{
			TransformEnemyPlantsAt(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<TowerDefenseProjectile>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasEnemyPlant && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(HasEnemyPlant(VariantUtils.ConvertTo<TowerDefenseCellInstance>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1])));
			return true;
		}
		if (method == MethodName.IsEnemyTransformablePlant && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(IsEnemyTransformablePlant(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1])));
			return true;
		}
		if (method == MethodName.IsValidTarget && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsValidTarget(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.CanTransform && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<bool>(CanTransform(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.Run && args.Count == 6)
		{
			Run(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]), VariantUtils.ConvertTo<PackedScene>(in args[2]), VariantUtils.ConvertTo<string>(in args[3]), VariantUtils.ConvertTo<bool>(in args[4]), VariantUtils.ConvertTo<bool>(in args[5]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.HasEnemyPlant && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(HasEnemyPlant(VariantUtils.ConvertTo<TowerDefenseCellInstance>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1])));
			return true;
		}
		if (method == MethodName.IsEnemyTransformablePlant && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(IsEnemyTransformablePlant(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1])));
			return true;
		}
		if (method == MethodName.IsValidTarget && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsValidTarget(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.CanTransform && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<bool>(CanTransform(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.Run && args.Count == 6)
		{
			Run(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]), VariantUtils.ConvertTo<PackedScene>(in args[2]), VariantUtils.ConvertTo<string>(in args[3]), VariantUtils.ConvertTo<bool>(in args[4]), VariantUtils.ConvertTo<bool>(in args[5]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Execute)
		{
			return true;
		}
		if (method == MethodName.ExecuteDps)
		{
			return true;
		}
		if (method == MethodName.ExecuteProject)
		{
			return true;
		}
		if (method == MethodName.ExecuteGroundProject)
		{
			return true;
		}
		if (method == MethodName.RunPlantTransform)
		{
			return true;
		}
		if (method == MethodName.TransformEnemyPlantsAt)
		{
			return true;
		}
		if (method == MethodName.HasEnemyPlant)
		{
			return true;
		}
		if (method == MethodName.IsEnemyTransformablePlant)
		{
			return true;
		}
		if (method == MethodName.IsValidTarget)
		{
			return true;
		}
		if (method == MethodName.CanTransform)
		{
			return true;
		}
		if (method == MethodName.Run)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.transformName)
		{
			transformName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.effectScene)
		{
			effectScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName.effectParticles)
		{
			effectParticles = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.networkEffectId)
		{
			networkEffectId = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.excludeImpFamily)
		{
			excludeImpFamily = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.plantTransformName)
		{
			plantTransformName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.transformName)
		{
			value = VariantUtils.CreateFrom(in transformName);
			return true;
		}
		if (name == PropertyName.effectScene)
		{
			value = VariantUtils.CreateFrom(in effectScene);
			return true;
		}
		if (name == PropertyName.effectParticles)
		{
			value = VariantUtils.CreateFrom(in effectParticles);
			return true;
		}
		if (name == PropertyName.networkEffectId)
		{
			value = VariantUtils.CreateFrom(in networkEffectId);
			return true;
		}
		if (name == PropertyName.excludeImpFamily)
		{
			value = VariantUtils.CreateFrom(in excludeImpFamily);
			return true;
		}
		if (name == PropertyName.plantTransformName)
		{
			value = VariantUtils.CreateFrom(in plantTransformName);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, PropertyName.transformName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName.effectScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.effectParticles, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.networkEffectId, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.excludeImpFamily, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.plantTransformName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.transformName, Variant.From(in transformName));
		info.AddProperty(PropertyName.effectScene, Variant.From(in effectScene));
		info.AddProperty(PropertyName.effectParticles, Variant.From(in effectParticles));
		info.AddProperty(PropertyName.networkEffectId, Variant.From(in networkEffectId));
		info.AddProperty(PropertyName.excludeImpFamily, Variant.From(in excludeImpFamily));
		info.AddProperty(PropertyName.plantTransformName, Variant.From(in plantTransformName));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.transformName, out var value))
		{
			transformName = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName.effectScene, out var value2))
		{
			effectScene = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName.effectParticles, out var value3))
		{
			effectParticles = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.networkEffectId, out var value4))
		{
			networkEffectId = value4.As<string>();
		}
		if (info.TryGetProperty(PropertyName.excludeImpFamily, out var value5))
		{
			excludeImpFamily = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.plantTransformName, out var value6))
		{
			plantTransformName = value6.As<string>();
		}
	}
}
