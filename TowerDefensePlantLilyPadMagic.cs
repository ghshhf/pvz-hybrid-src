using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter10/LilyPadMagic/Scene/TowerDefensePlantLilyPadMagic.cs")]
public class TowerDefensePlantLilyPadMagic : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName HasMagicPlant = "HasMagicPlant";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _fireMarker = "_fireMarker";

		public static readonly StringName _active = "_active";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private FireComponent _fireComponent;

	private Marker2D _fireMarker;

	private bool _active;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_fireComponent = componentManager.GetRuntime<FireComponent>("character.fire");
			_fireMarker = GetNode<Marker2D>("SpriteGroup/TransformPoint/LilyPadMagic/PotSlot/FireMarker");
			_fireComponent.SetAlive(alive: false);
		}
	}

	public override void BatchUpdate(double delta)
	{
		if (!Engine.IsEditorHint() && inGame && !isDestroy)
		{
			FireComponent fireComponent = _fireComponent;
			if (fireComponent != null && !fireComponent.IsReleased)
			{
				bool flag = HasMagicPlant();
				_fireComponent.SetAlive(flag);
				if (_active != flag)
				{
					_active = flag;
					idleAnimeClip = (flag ? "IdleUp" : "Idle");
					base.sprite.SetAnimation(idleAnimeClip);
				}
				_fireMarker.GlobalPosition = TowerDefenseManager.Instance.GetMapCellPosCenter(base.gridPos);
			}
		}
		base.BatchUpdate(delta);
	}

	private bool HasMagicPlant()
	{
		if (!GodotObject.IsInstanceValid(cell))
		{
			return false;
		}
		foreach (TowerDefenseCharacter character in cell.GetCharacterList())
		{
			if (character != this && character is TowerDefensePlant && GodotObject.IsInstanceValid(character) && !character.die && !character.nearDie && !character.isDestroy && !character.instance.hologram && (character.instance.physiqueTypeFlags & 0x1000) != 0)
			{
				return true;
			}
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(3)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.HasMagicPlant, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasMagicPlant && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasMagicPlant());
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
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.HasMagicPlant)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._fireMarker)
		{
			_fireMarker = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName._active)
		{
			_active = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._fireMarker)
		{
			value = VariantUtils.CreateFrom(in _fireMarker);
			return true;
		}
		if (name == PropertyName._active)
		{
			value = VariantUtils.CreateFrom(in _active);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._fireMarker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._active, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._fireMarker, Variant.From(in _fireMarker));
		info.AddProperty(PropertyName._active, Variant.From(in _active));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._fireMarker, out var value))
		{
			_fireMarker = value.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName._active, out var value2))
		{
			_active = value2.As<bool>();
		}
	}
}
