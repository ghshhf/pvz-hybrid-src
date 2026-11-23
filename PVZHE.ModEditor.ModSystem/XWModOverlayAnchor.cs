using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModOverlayAnchor : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName Target = "Target";

		public static readonly StringName Surface = "Surface";
	}

	public new class SignalName : Node.SignalName
	{
	}

	internal Node2D Target;

	internal Control Surface;

	public override void _Process(double delta)
	{
		if (GodotObject.IsInstanceValid(Target) && GodotObject.IsInstanceValid(Surface))
		{
			Surface.Visible = Target.IsVisibleInTree();
			Vector2 origin = Target.GetGlobalTransformWithCanvas().Origin;
			Control parent = Surface.GetParent<Control>();
			Surface.Position = parent.GetGlobalTransformWithCanvas().AffineInverse() * origin;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(1)
		{
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Target)
		{
			Target = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName.Surface)
		{
			Surface = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Target)
		{
			value = VariantUtils.CreateFrom(in Target);
			return true;
		}
		if (name == PropertyName.Surface)
		{
			value = VariantUtils.CreateFrom(in Surface);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.Target, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.Surface, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Target, Variant.From(in Target));
		info.AddProperty(PropertyName.Surface, Variant.From(in Surface));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Target, out var value))
		{
			Target = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName.Surface, out var value2))
		{
			Surface = value2.As<Control>();
		}
	}
}
