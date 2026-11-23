using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModBattlePresentationHost : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _paused = "_paused";

		public static readonly StringName _size = "_size";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private bool _paused;

	private Vector2 _size;

	public override void _Ready()
	{
		base.ProcessMode = ProcessModeEnum.Always;
		_paused = GetTree().Paused;
		_size = GetViewport().GetVisibleRect().Size;
	}

	public override void _Process(double delta)
	{
		if (_paused != GetTree().Paused)
		{
			_paused = GetTree().Paused;
			XWModExtensions.Notify(XWModLifecyclePoints.SimulationPause, new XWModPauseState(_paused, "simulation"), XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Local));
		}
		Vector2 size = GetViewport().GetVisibleRect().Size;
		if (!(size == _size))
		{
			_size = size;
			XWModExtensions.Notify(XWModPresentationPoints.LayoutChanged, new XWModLayoutState(size), XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Local));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(2)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._paused)
		{
			_paused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._size)
		{
			_size = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._paused)
		{
			value = VariantUtils.CreateFrom(in _paused);
			return true;
		}
		if (name == PropertyName._size)
		{
			value = VariantUtils.CreateFrom(in _size);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._paused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._size, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._paused, Variant.From(in _paused));
		info.AddProperty(PropertyName._size, Variant.From(in _size));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._paused, out var value))
		{
			_paused = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._size, out var value2))
		{
			_size = value2.As<Vector2>();
		}
	}
}
