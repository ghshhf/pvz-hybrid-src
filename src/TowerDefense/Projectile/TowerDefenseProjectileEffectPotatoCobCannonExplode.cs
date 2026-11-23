using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Prefab/ProjectileEffect/PotatoCobCannonExplode/TowerDefenseProjectileEffectPotatoCobCannonExplode.cs")]
public class TowerDefenseProjectileEffectPotatoCobCannonExplode : TowerDefenseProjectileEffectBase
{
	public new class MethodName : TowerDefenseProjectileEffectBase.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ApplySpawnCustoms = "ApplySpawnCustoms";

		public static readonly StringName Explode = "Explode";

		public static readonly StringName EffectCreate = "EffectCreate";
	}

	public new class PropertyName : TowerDefenseProjectileEffectBase.PropertyName
	{
		public static readonly StringName skinSpawnCustoms = "skinSpawnCustoms";
	}

	public new class SignalName : TowerDefenseProjectileEffectBase.SignalName
	{
	}

	private static PackedScene mineExplosion;

	private static PackedScene MINE_EXPLOSION => mineExplosion ?? (mineExplosion = GD.Load<PackedScene>("uid://dqcu2ycvf4u72"));

	[Export(PropertyHint.None, "")]
	public Dictionary skinSpawnCustoms { get; set; } = new Dictionary();

	public override async void _Ready()
	{
		if (GodotObject.IsInstanceValid(target))
		{
			Explode();
		}
		else
		{
			TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(gridPos);
			TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("PlantSunMine");
			if (GodotObject.IsInstanceValid(mapCell))
			{
				if (mapCell.CanPacketPlant(packetConfig))
				{
					TowerDefenseCharacter sunMine = packetConfig.Plant(gridPos);
					ApplySpawnCustoms(sunMine);
					if (camp == TowerDefenseEnum.CHARACTER_CAMP.ZOMBIE)
					{
						sunMine.Hypnoses();
					}
					await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
					if (sunMine is TowerDefensePlantSunMine towerDefensePlantSunMine)
					{
						towerDefensePlantSunMine.ReadyRise();
					}
					else if (sunMine is TowerDefensePlantMagnetMine towerDefensePlantMagnetMine)
					{
						towerDefensePlantMagnetMine.ReadyRise();
					}
				}
				else
				{
					Explode();
				}
			}
			else
			{
				Explode();
			}
		}
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		QueueFree();
	}

	private void ApplySpawnCustoms(TowerDefenseCharacter character)
	{
		if (!GodotObject.IsInstanceValid(character) || skinSpawnCustoms.Count == 0 || !skinSpawnCustoms.TryGetValue(projectileSkinName.ToString(), out var value))
		{
			return;
		}
		Array array = value.AsGodotArray();
		if (array == null || array.Count == 0)
		{
			return;
		}
		Array<string> array2 = new Array<string>();
		foreach (Variant item in array)
		{
			string text = item.AsString();
			if (!string.IsNullOrEmpty(text))
			{
				array2.Add(text);
			}
		}
		if (array2.Count > 0)
		{
			character.SetCustoms(array2);
		}
	}

	private void Explode()
	{
		EffectCreate();
		TowerDefenseExplode.CreateExplode(base.GlobalPosition, new Vector2(0.5f, 0.5f), base.Eventlist, new Array<TowerDefenseCharacter>(), camp, -1, suppressDeathrattles: false, useGridRange: false);
		Vector2 velocity = new Vector2((float)GD.RandRange(-50.0, 50.0), -400f);
		if (camp == TowerDefenseEnum.CHARACTER_CAMP.ZOMBIE)
		{
			TowerDefenseManager.Instance.BrainSunCreate(base.GlobalPosition, 50L, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, 0.0, velocity);
		}
		else
		{
			TowerDefenseManager.Instance.SunCreate(base.GlobalPosition, 50L, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, 0.0, velocity);
		}
	}

	public void EffectCreate()
	{
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(gridPos);
		ViewManager.Instance.CameraShake(new Vector2(GD.RandRange(-1, 1), GD.RandRange(-1, 1)), 3.0, 0.05, 4);
		TowerDefenseEffectParticlesOnce towerDefenseEffectParticlesOnce = TowerDefenseManager.CreateEffectParticlesOnce(MINE_EXPLOSION, gridPos);
		Node2D characterNode = TowerDefenseManager.GetCharacterNode();
		towerDefenseEffectParticlesOnce.GlobalPosition = base.GlobalPosition;
		if (GodotObject.IsInstanceValid(mapCell))
		{
			towerDefenseEffectParticlesOnce.GlobalPosition = new Vector2(towerDefenseEffectParticlesOnce.GlobalPosition.X, towerDefenseEffectParticlesOnce.GlobalPosition.Y - (float)(mapCell.GetGroundHeight() - 30.0));
		}
		characterNode.AddChild(towerDefenseEffectParticlesOnce, forceReadableName: false, InternalMode.Disabled);
		AudioManager.Instance.AudioPlay("MineExplosion");
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(4)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplySpawnCustoms, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.Explode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EffectCreate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.ApplySpawnCustoms && args.Count == 1)
		{
			ApplySpawnCustoms(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Explode && args.Count == 0)
		{
			Explode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EffectCreate && args.Count == 0)
		{
			EffectCreate();
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
		if (method == MethodName.ApplySpawnCustoms)
		{
			return true;
		}
		if (method == MethodName.Explode)
		{
			return true;
		}
		if (method == MethodName.EffectCreate)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.skinSpawnCustoms)
		{
			skinSpawnCustoms = VariantUtils.ConvertTo<Dictionary>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.skinSpawnCustoms)
		{
			value = VariantUtils.CreateFrom<Dictionary>(skinSpawnCustoms);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Dictionary, PropertyName.skinSpawnCustoms, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.skinSpawnCustoms, Variant.From<Dictionary>(skinSpawnCustoms));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.skinSpawnCustoms, out var value))
		{
			skinSpawnCustoms = value.As<Dictionary>();
		}
	}
}
