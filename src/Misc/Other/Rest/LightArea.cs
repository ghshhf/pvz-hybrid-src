using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Buff/LightArea/LightArea.cs")]
public class LightArea : AabbArea2D
{
	public new class MethodName : AabbArea2D.MethodName
	{
		public new static readonly StringName _PhysicsProcess = "_PhysicsProcess";

		public static readonly StringName RevealCharacter = "RevealCharacter";
	}

	public new class PropertyName : AabbArea2D.PropertyName
	{
		public static readonly StringName _lightOwner = "_lightOwner";
	}

	public new class SignalName : AabbArea2D.SignalName
	{
	}

	private TowerDefenseCharacter _lightOwner;

	private readonly List<TowerDefenseCharacter> _targets = new List<TowerDefenseCharacter>();

	public override bool TryGetWorldRect(out Rect2 rect)
	{
		rect = default(Rect2);
		if (!base.Enabled)
		{
			return false;
		}
		if (!GodotObject.IsInstanceValid(_lightOwner))
		{
			for (Node parent = GetParent(); parent != null; parent = parent.GetParent())
			{
				if (parent is TowerDefenseCharacter lightOwner)
				{
					_lightOwner = lightOwner;
					break;
				}
			}
		}
		if (!GodotObject.IsInstanceValid(_lightOwner))
		{
			return false;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		rect = AabbShapeUtil.RectFromCenter(instance.GetMapCellPosCenter(_lightOwner.gridPos), instance.GetMapGridSize() * new Vector2(7f, 5f)).Grow(-0.01f);
		return true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!TryGetWorldRect(out var _))
		{
			return;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (GodotObject.IsInstanceValid(instance) && instance.characterRegistry != null)
		{
			_targets.Clear();
			_targets.AddRange(instance.characterRegistry.GetCharactersForGridWindowList(_lightOwner.gridPos.X, _lightOwner.gridPos.Y, 3, 2, includeExtendedPlants: true));
			for (int i = 0; i < _targets.Count; i++)
			{
				RevealCharacter(_targets[i]);
			}
		}
	}

	private void RevealCharacter(TowerDefenseCharacter tdChar)
	{
		if (GodotObject.IsInstanceValid(tdChar) && GodotObject.IsInstanceValid(tdChar.sprite) && tdChar.sprite.invisible)
		{
			tdChar.sprite.invisible = false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(2)
		{
			new MethodInfo(MethodName._PhysicsProcess, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RevealCharacter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "tdChar", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._PhysicsProcess && args.Count == 1)
		{
			_PhysicsProcess(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RevealCharacter && args.Count == 1)
		{
			RevealCharacter(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._PhysicsProcess)
		{
			return true;
		}
		if (method == MethodName.RevealCharacter)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._lightOwner)
		{
			_lightOwner = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._lightOwner)
		{
			value = VariantUtils.CreateFrom(in _lightOwner);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._lightOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._lightOwner, Variant.From(in _lightOwner));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._lightOwner, out var value))
		{
			_lightOwner = value.As<TowerDefenseCharacter>();
		}
	}
}
