using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter10/MagicStoneNut/Scene/TowerDefensePlantMagicStoneNut.cs")]
public class TowerDefensePlantMagicStoneNut : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName DamagePointReach = "DamagePointReach";

		public new static readonly StringName OnCustomSwitched = "OnCustomSwitched";

		public new static readonly StringName DestroySet = "DestroySet";

		public static readonly StringName CastHaste = "CastHaste";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName magicEffect = "magicEffect";

		public static readonly StringName customMagicEffect = "customMagicEffect";

		public static readonly StringName _deathPulse = "_deathPulse";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private const double HasteTimeScale = 3.0;

	private const double HasteTime = 5.0;

	[Export(PropertyHint.None, "")]
	public PackedScene magicEffect;

	[Export(PropertyHint.None, "")]
	public PackedScene customMagicEffect;

	private bool _deathPulse;

	public override void DamagePointReach(string damagePointName)
	{
		base.DamagePointReach(damagePointName);
		if (base.currentCustom.Contains("Custom0"))
		{
			base.config.customData.SetDamagePoint(base.sprite, "Custom0", damagePointName switch
			{
				"Damage0" => 0, 
				"Damage1" => 1, 
				"Damage2" => 2, 
				_ => -1, 
			});
		}
		if (!(damagePointName == "Damage0") && !(instance.hitpoints <= 0.0) && !die && !isDestroy)
		{
			CastHaste();
		}
	}

	public override void OnCustomSwitched(string customKey)
	{
		base.OnCustomSwitched(customKey);
		if (GodotObject.IsInstanceValid(instance) && instance.hitpointsSave > 0.0)
		{
			PreviewDamagePoint(instance.hitpoints / instance.hitpointsSave);
		}
	}

	public override void DestroySet()
	{
		base.DestroySet();
		if (!_deathPulse)
		{
			_deathPulse = true;
			CastHaste();
		}
	}

	private void CastHaste()
	{
		if (Engine.IsEditorHint() || editorPreviewMode || !inGame || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		TowerDefenseEffectSpriteOnce towerDefenseEffectSpriteOnce = TowerDefenseManager.CreateEffectSpriteOnce(base.currentCustom.Contains("Custom0") ? customMagicEffect : magicEffect, base.gridPos, "Fire");
		TowerDefenseGroundItemBase.characterNode.AddChild(towerDefenseEffectSpriteOnce, forceReadableName: false, InternalMode.Disabled);
		towerDefenseEffectSpriteOnce.GlobalPosition = GetLogicalGlobalPosition();
		if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
		{
			MultiPlayerManager.Instance.SendProjectileEffectSpawn(new ProjectileEffectSpawnDto
			{
				effect_id = "magic_stone_haste",
				variant = (base.currentCustom.Contains("Custom0") ? "Custom0" : ""),
				px = towerDefenseEffectSpriteOnce.GlobalPosition.X,
				py = towerDefenseEffectSpriteOnce.GlobalPosition.Y,
				grid_x = base.gridPos.X,
				grid_y = base.gridPos.Y,
				camp = (int)base.camp
			});
		}
		foreach (Variant item in TowerDefenseManager.Instance.GetCampFriendly(base.camp))
		{
			TowerDefenseCharacter towerDefenseCharacter = (TowerDefenseCharacter)(GodotObject)item;
			if (towerDefenseCharacter is TowerDefensePlant && GodotObject.IsInstanceValid(towerDefenseCharacter) && !towerDefenseCharacter.die && !towerDefenseCharacter.isDestroy && towerDefenseCharacter.instance.IsMagicPhysique)
			{
				towerDefenseCharacter.buff.AddBuff(new TowerDefenseCharacterBuffCoffee
				{
					timeScaleValue = 3.0,
					time = 5.0
				});
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary { ["deathPulse"] = _deathPulse };
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_deathPulse = data.GetValueOrDefault("deathPulse", false).AsBool();
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
		return new List<MethodInfo>(8)
		{
			new MethodInfo(MethodName.DamagePointReach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "damagePointName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnCustomSwitched, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "customKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CastHaste, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName.DamagePointReach && args.Count == 1)
		{
			DamagePointReach(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCustomSwitched && args.Count == 1)
		{
			OnCustomSwitched(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastHaste && args.Count == 0)
		{
			CastHaste();
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
		if (method == MethodName.DamagePointReach)
		{
			return true;
		}
		if (method == MethodName.OnCustomSwitched)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
		{
			return true;
		}
		if (method == MethodName.CastHaste)
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
		if (name == PropertyName.magicEffect)
		{
			magicEffect = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName.customMagicEffect)
		{
			customMagicEffect = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._deathPulse)
		{
			_deathPulse = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.magicEffect)
		{
			value = VariantUtils.CreateFrom(in magicEffect);
			return true;
		}
		if (name == PropertyName.customMagicEffect)
		{
			value = VariantUtils.CreateFrom(in customMagicEffect);
			return true;
		}
		if (name == PropertyName._deathPulse)
		{
			value = VariantUtils.CreateFrom(in _deathPulse);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.magicEffect, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName.customMagicEffect, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._deathPulse, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.magicEffect, Variant.From(in magicEffect));
		info.AddProperty(PropertyName.customMagicEffect, Variant.From(in customMagicEffect));
		info.AddProperty(PropertyName._deathPulse, Variant.From(in _deathPulse));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.magicEffect, out var value))
		{
			magicEffect = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName.customMagicEffect, out var value2))
		{
			customMagicEffect = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._deathPulse, out var value3))
		{
			_deathPulse = value3.As<bool>();
		}
	}
}
