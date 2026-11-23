using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/GraveStone/PoisonBottle/Scene/TowerDefensePoisonBottle.cs")]
public class TowerDefensePoisonBottle : TowerDefenseGravestone
{
	public new class MethodName : TowerDefenseGravestone.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ApplyWaterTexture = "ApplyWaterTexture";

		public new static readonly StringName DamagePointReach = "DamagePointReach";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName Destroy = "Destroy";
	}

	public new class PropertyName : TowerDefenseGravestone.PropertyName
	{
		public static readonly StringName waterTextures = "waterTextures";

		public static readonly StringName _consumed = "_consumed";

		public static readonly StringName _mistCreated = "_mistCreated";
	}

	public new class SignalName : TowerDefenseGravestone.SignalName
	{
	}

	private const double ContactDamage = 500.0;

	[Export(PropertyHint.None, "")]
	public Array<string> waterTextures = new Array<string>();

	private bool _consumed;

	private bool _mistCreated;

	private readonly List<TowerDefenseCharacter> _targets = new List<TowerDefenseCharacter>();

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			RemoveFromGroup("Gravestone");
			ApplyWaterTexture();
		}
	}

	private void ApplyWaterTexture()
	{
		if (GodotObject.IsInstanceValid(cell) && cell.isWater && waterTextures.Count >= 3)
		{
			shadowSprite.Visible = false;
			double num = instance.hitpoints / instance.hitpointsSave;
			int index = ((!(num > 2.0 / 3.0)) ? ((num > 1.0 / 3.0) ? 1 : 2) : 0);
			base.sprite.SetAtlasReplace("PoisonBottle_1.png", waterTextures[index]);
		}
	}

	public override void DamagePointReach(string name)
	{
		base.DamagePointReach(name);
		ApplyWaterTexture();
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		if (!inGame || die || isDestroy || _consumed || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		Rect2 checkRect = AabbShapeUtil.RectFromCenter(GetLogicalGlobalPosition(), new Vector2(36f, 36f));
		_targets.Clear();
		TowerDefenseManager.Instance.characterRegistry.FillCharactersIntersectingRectList(checkRect, _targets);
		bool flag = false;
		foreach (TowerDefenseCharacter target in _targets)
		{
			if (target == this || (target.instance.maskFlags & 0x12) != 0 || target.gridPos.Y != base.gridPos.Y || target.die || target.nearDie || target.isDestroy)
			{
				continue;
			}
			if (target.camp == base.camp)
			{
				if (target.instance.maskFlags != 0)
				{
					flag = true;
					TowerDefenseCharacterEventPoisonBottle.Run(GetLogicalGlobalPosition(), target, 500.0, base.camp);
				}
			}
			else
			{
				flag = true;
				TowerDefenseCharacterEventHurt.Run(GetLogicalGlobalPosition(), target, 500.0);
			}
		}
		if (flag)
		{
			_consumed = true;
			Destroy();
		}
	}

	public override void Destroy(bool freeInstance = true)
	{
		if (!_consumed && !_mistCreated && inGame && TowerDefenseManager.HasGameplayAuthority && GodotObject.IsInstanceValid(instance) && instance.hitpoints <= 0.0)
		{
			_mistCreated = true;
			TowerDefenseItemPoisonMist.Spawn("ItemPoisonMist", base.gridPos, base.groundHeight, instance.hypnoses);
		}
		base.Destroy(freeInstance);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(5)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyWaterTexture, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DamagePointReach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Destroy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "freeInstance", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.ApplyWaterTexture && args.Count == 0)
		{
			ApplyWaterTexture();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DamagePointReach && args.Count == 1)
		{
			DamagePointReach(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Destroy && args.Count == 1)
		{
			Destroy(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.ApplyWaterTexture)
		{
			return true;
		}
		if (method == MethodName.DamagePointReach)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.Destroy)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.waterTextures)
		{
			waterTextures = VariantUtils.ConvertToArray<string>(in value);
			return true;
		}
		if (name == PropertyName._consumed)
		{
			_consumed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._mistCreated)
		{
			_mistCreated = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.waterTextures)
		{
			value = VariantUtils.CreateFromArray(waterTextures);
			return true;
		}
		if (name == PropertyName._consumed)
		{
			value = VariantUtils.CreateFrom(in _consumed);
			return true;
		}
		if (name == PropertyName._mistCreated)
		{
			value = VariantUtils.CreateFrom(in _mistCreated);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Array, PropertyName.waterTextures, PropertyHint.TypeString, "4/0:", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._consumed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._mistCreated, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.waterTextures, Variant.CreateFrom(waterTextures));
		info.AddProperty(PropertyName._consumed, Variant.From(in _consumed));
		info.AddProperty(PropertyName._mistCreated, Variant.From(in _mistCreated));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.waterTextures, out var value))
		{
			waterTextures = value.AsGodotArray<string>();
		}
		if (info.TryGetProperty(PropertyName._consumed, out var value2))
		{
			_consumed = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._mistCreated, out var value3))
		{
			_mistCreated = value3.As<bool>();
		}
	}
}
