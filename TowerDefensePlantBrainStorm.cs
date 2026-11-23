using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Diamond/BrainStorm/Scene/TowerDefensePlantBrainStorm.cs")]
public class TowerDefensePlantBrainStorm : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Appear = "Appear";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName DestroySet = "DestroySet";

		public static readonly StringName PlayDisappearanceEffect = "PlayDisappearanceEffect";

		public static readonly StringName Collapse = "Collapse";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName effectScene = "effectScene";

		public static readonly StringName _appeared = "_appeared";

		public static readonly StringName _over = "_over";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private const double AbsorbDuration = 1.35;

	private static readonly Vector2 ExplodeCenter = new Vector2(40f, 22f);

	[Export(PropertyHint.None, "")]
	public PackedScene effectScene;

	private bool _appeared;

	private bool _over;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			instance.invincible = true;
			instance.canBeCollection = false;
			if (!editorPreviewMode && inGame && !base.IsProgressRestoreInFlight)
			{
				Callable.From(Appear).CallDeferred();
			}
		}
	}

	private void Appear()
	{
		if (!_appeared && !isDestroy && IsInsideTree())
		{
			_appeared = true;
			base.sprite.SetAnimation("Explode", loop: false);
			Collapse();
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Explode" && _appeared && !isDestroy && !die)
		{
			base.sprite.SetAnimation(idleAnimeClip);
		}
	}

	public override void DestroySet()
	{
		base.DestroySet();
		if (!_over)
		{
			_over = true;
			PlayDisappearanceEffect();
			Collapse();
		}
	}

	private void PlayDisappearanceEffect()
	{
		TowerDefenseEffectSpriteOnce towerDefenseEffectSpriteOnce = TowerDefenseManager.CreateEffectSpriteOnce(effectScene, base.gridPos, "Explode");
		base.config.customData.ClearCustomFliters(towerDefenseEffectSpriteOnce.sprite);
		foreach (string item in base.currentCustom)
		{
			base.config.customData.SetCustomFliters(towerDefenseEffectSpriteOnce.sprite, item);
		}
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseEffectSpriteOnce, forceReadableName: false, InternalMode.Disabled);
		towerDefenseEffectSpriteOnce.GlobalTransform = base.sprite.GlobalTransform;
		base.sprite.Visible = false;
	}

	private void Collapse()
	{
		if (Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return;
		}
		Vector2 center = base.sprite.ToGlobal(base.sprite.offset + ExplodeCenter);
		foreach (TowerDefenseCharacter item in new List<TowerDefenseCharacter>(TowerDefenseManager.Instance.characterRegistry.GetCharactersForGridWindowList(base.gridPos.X, base.gridPos.Y, 1, 1, includeExtendedPlants: true)))
		{
			if (GodotObject.IsInstanceValid(item) && !item.isDestroy && !item.die && CheckDifferentCamp(item.camp) && GodotObject.IsInstanceValid(item.instance) && item.instance.maskFlags != 0)
			{
				item.componentManager.GetRuntime<DestroyComponent>().AbsorbDestroy(center, 1.35);
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["appeared"] = _appeared,
			["over"] = _over
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_appeared = data.GetValueOrDefault("appeared", _appeared).AsBool();
		_over = data.GetValueOrDefault("over", _over).AsBool();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(8)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Appear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlayDisappearanceEffect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Collapse, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.Appear && args.Count == 0)
		{
			Appear();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayDisappearanceEffect && args.Count == 0)
		{
			PlayDisappearanceEffect();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Collapse && args.Count == 0)
		{
			Collapse();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Appear)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
		{
			return true;
		}
		if (method == MethodName.PlayDisappearanceEffect)
		{
			return true;
		}
		if (method == MethodName.Collapse)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.effectScene)
		{
			effectScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._appeared)
		{
			_appeared = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._over)
		{
			_over = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.effectScene)
		{
			value = VariantUtils.CreateFrom(in effectScene);
			return true;
		}
		if (name == PropertyName._appeared)
		{
			value = VariantUtils.CreateFrom(in _appeared);
			return true;
		}
		if (name == PropertyName._over)
		{
			value = VariantUtils.CreateFrom(in _over);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.effectScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._appeared, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._over, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.effectScene, Variant.From(in effectScene));
		info.AddProperty(PropertyName._appeared, Variant.From(in _appeared));
		info.AddProperty(PropertyName._over, Variant.From(in _over));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.effectScene, out var value))
		{
			effectScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._appeared, out var value2))
		{
			_appeared = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._over, out var value3))
		{
			_over = value3.As<bool>();
		}
	}
}
