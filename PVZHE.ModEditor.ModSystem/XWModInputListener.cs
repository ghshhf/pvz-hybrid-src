using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModInputListener : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _UnhandledInput = "_UnhandledInput";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName OwnerId = "OwnerId";

		public static readonly StringName Active = "Active";

		public static readonly StringName ActionName = "ActionName";

		public static readonly StringName Consume = "Consume";
	}

	public new class SignalName : Node.SignalName
	{
	}

	internal string OwnerId;

	internal XWModRuntimeSession Session;

	internal bool Active = true;

	internal string ActionName;

	internal Action Pressed;

	internal Action<Exception> Fault;

	internal bool Consume;

	public override void _UnhandledInput(InputEvent input)
	{
		if (!Active || Session != XWModRuntimeSession.Current)
		{
			return;
		}
		XWModRuntimeSession session = Session;
		if (session == null || !session.IsOwnerActive(OwnerId) || !input.IsActionPressed(ActionName) || input.IsEcho())
		{
			return;
		}
		Control control = GetViewport().GuiGetFocusOwner();
		if ((control is LineEdit || control is TextEdit) ? true : false)
		{
			return;
		}
		try
		{
			using (XWModExtensions.EnterOperation())
			{
				using (XWModExtensions.EnterCallback(OwnerId, modifier: false, XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Local, "input:" + ActionName)with
				{
					SourceMod = OwnerId
				}))
				{
					Pressed();
				}
			}
		}
		catch (Exception obj)
		{
			Active = false;
			SetProcessUnhandledInput(enable: false);
			Fault(obj);
		}
		if (Consume)
		{
			GetViewport().SetInputAsHandled();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(1)
		{
			new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.OwnerId)
		{
			OwnerId = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.Active)
		{
			Active = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.ActionName)
		{
			ActionName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.Consume)
		{
			Consume = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.OwnerId)
		{
			value = VariantUtils.CreateFrom(in OwnerId);
			return true;
		}
		if (name == PropertyName.Active)
		{
			value = VariantUtils.CreateFrom(in Active);
			return true;
		}
		if (name == PropertyName.ActionName)
		{
			value = VariantUtils.CreateFrom(in ActionName);
			return true;
		}
		if (name == PropertyName.Consume)
		{
			value = VariantUtils.CreateFrom(in Consume);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, PropertyName.OwnerId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.Active, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.ActionName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.Consume, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.OwnerId, Variant.From(in OwnerId));
		info.AddProperty(PropertyName.Active, Variant.From(in Active));
		info.AddProperty(PropertyName.ActionName, Variant.From(in ActionName));
		info.AddProperty(PropertyName.Consume, Variant.From(in Consume));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.OwnerId, out var value))
		{
			OwnerId = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName.Active, out var value2))
		{
			Active = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.ActionName, out var value3))
		{
			ActionName = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName.Consume, out var value4))
		{
			Consume = value4.As<bool>();
		}
	}
}
