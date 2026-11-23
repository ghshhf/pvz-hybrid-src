using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[ScriptPath("res://Asset/Config/Map/MagicPool/Scene/TowerDefenseMapMagicPool.cs")]
public class TowerDefenseMapMagicPool : TowerDefenseMapRevealShaderTime
{
	public new class MethodName : TowerDefenseMapRevealShaderTime.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateForegroundTint = "UpdateForegroundTint";
	}

	public new class PropertyName : TowerDefenseMapRevealShaderTime.PropertyName
	{
		public static readonly StringName foregroundTint = "foregroundTint";

		public static readonly StringName foregroundDoor = "foregroundDoor";

		public static readonly StringName _mapControl = "_mapControl";
	}

	public new class SignalName : TowerDefenseMapRevealShaderTime.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public CanvasModulate foregroundTint;

	[Export(PropertyHint.None, "")]
	public Sprite2D foregroundDoor;

	private TowerDefenseMapControl _mapControl;

	public override void _Ready()
	{
		base._Ready();
		for (Node parent = GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent is TowerDefenseMapControl mapControl)
			{
				_mapControl = mapControl;
				break;
			}
		}
		UpdateForegroundTint();
	}

	public override void _Process(double delta)
	{
		UpdateForegroundTint();
	}

	private void UpdateForegroundTint()
	{
		if (GodotObject.IsInstanceValid(foregroundTint) && GodotObject.IsInstanceValid(foregroundDoor))
		{
			CanvasModulate canvasModulate = (GodotObject.IsInstanceValid(_mapControl) ? _mapControl.canvasModulate : null);
			foregroundTint.Visible = GodotObject.IsInstanceValid(canvasModulate) && canvasModulate.Visible;
			if (GodotObject.IsInstanceValid(canvasModulate))
			{
				foregroundTint.Color = canvasModulate.Color;
			}
			foregroundDoor.Modulate = base.Modulate * base.SelfModulate;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(3)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateForegroundTint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateForegroundTint && args.Count == 0)
		{
			UpdateForegroundTint();
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateForegroundTint)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.foregroundTint)
		{
			foregroundTint = VariantUtils.ConvertTo<CanvasModulate>(in value);
			return true;
		}
		if (name == PropertyName.foregroundDoor)
		{
			foregroundDoor = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._mapControl)
		{
			_mapControl = VariantUtils.ConvertTo<TowerDefenseMapControl>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.foregroundTint)
		{
			value = VariantUtils.CreateFrom(in foregroundTint);
			return true;
		}
		if (name == PropertyName.foregroundDoor)
		{
			value = VariantUtils.CreateFrom(in foregroundDoor);
			return true;
		}
		if (name == PropertyName._mapControl)
		{
			value = VariantUtils.CreateFrom(in _mapControl);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.foregroundTint, PropertyHint.NodeType, "CanvasModulate", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName.foregroundDoor, PropertyHint.NodeType, "Sprite2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName._mapControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.foregroundTint, Variant.From(in foregroundTint));
		info.AddProperty(PropertyName.foregroundDoor, Variant.From(in foregroundDoor));
		info.AddProperty(PropertyName._mapControl, Variant.From(in _mapControl));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.foregroundTint, out var value))
		{
			foregroundTint = value.As<CanvasModulate>();
		}
		if (info.TryGetProperty(PropertyName.foregroundDoor, out var value2))
		{
			foregroundDoor = value2.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._mapControl, out var value3))
		{
			_mapControl = value3.As<TowerDefenseMapControl>();
		}
	}
}
