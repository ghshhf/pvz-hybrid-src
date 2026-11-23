using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Buff/TowerDefenseCharacterBuffTabooBean.cs")]
public class TowerDefenseCharacterBuffTabooBean : TowerDefenseCharacterBuffConfig
{
	public new class MethodName : TowerDefenseCharacterBuffConfig.MethodName
	{
		public new static readonly StringName _Init = "_Init";

		public new static readonly StringName Enter = "Enter";

		public new static readonly StringName EnterReadOnlyClient = "EnterReadOnlyClient";

		public new static readonly StringName Step = "Step";

		public new static readonly StringName StepReadOnlyClient = "StepReadOnlyClient";

		public new static readonly StringName SyncPresentationReadOnlyClient = "SyncPresentationReadOnlyClient";

		public new static readonly StringName Exit = "Exit";

		public new static readonly StringName ExitReadOnlyClient = "ExitReadOnlyClient";

		public new static readonly StringName Remove = "Remove";

		public new static readonly StringName Cancel = "Cancel";

		public new static readonly StringName Refresh = "Refresh";
	}

	public new class PropertyName : TowerDefenseCharacterBuffConfig.PropertyName
	{
		public new static readonly StringName FrameMeshColorMultiplier = "FrameMeshColorMultiplier";

		public new static readonly StringName AffectsHologram = "AffectsHologram";

		public static readonly StringName time = "time";

		public static readonly StringName currentTime = "currentTime";

		public static readonly StringName blink = "blink";

		public static readonly StringName produceTimeScaleValue = "produceTimeScaleValue";
	}

	public new class SignalName : TowerDefenseCharacterBuffConfig.SignalName
	{
	}

	private static readonly Color TABOO_BEAN_COLOR = new Color(0.359f, 0.359f, 0.359f);

	[Export(PropertyHint.None, "")]
	public double time = 35.0;

	[Export(PropertyHint.None, "")]
	public double currentTime;

	[Export(PropertyHint.None, "")]
	public bool blink;

	[Export(PropertyHint.None, "")]
	public double produceTimeScaleValue = 3.0;

	public override Color FrameMeshColorMultiplier => TABOO_BEAN_COLOR;

	public override double? produceTimeScaleOverride => produceTimeScaleValue;

	public override bool AffectsHologram => true;

	public override void _Init()
	{
		key = "TabooBean";
	}

	public override void Enter()
	{
	}

	public override void EnterReadOnlyClient()
	{
		character.SetSpriteGroupShaderParameter("blink", blink);
	}

	public override bool Step(double delta)
	{
		character.timeScale *= 5.0;
		currentTime += delta;
		if (!blink && currentTime >= time - 5.0)
		{
			character.SetSpriteGroupShaderParameter("blink", true);
			blink = true;
		}
		return currentTime >= time;
	}

	public override void StepReadOnlyClient(double delta)
	{
		Step(delta);
	}

	public override void SyncPresentationReadOnlyClient()
	{
		character.SetSpriteGroupShaderParameter("blink", blink);
	}

	public override void Exit()
	{
		character.Destroy();
		character.CraterCreate(nolimit: true);
	}

	public override void ExitReadOnlyClient()
	{
		character.SetSpriteGroupShaderParameter("blink", false);
	}

	public override void Remove()
	{
		Cancel();
	}

	public override void Cancel()
	{
		ExitReadOnlyClient();
	}

	public override void Refresh(TowerDefenseCharacterBuffConfig config)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(11)
		{
			new MethodInfo(MethodName._Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Enter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EnterReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Step, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StepReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SyncPresentationReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Exit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExitReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Remove, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Cancel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Refresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "config", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Init && args.Count == 0)
		{
			_Init();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enter && args.Count == 0)
		{
			Enter();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnterReadOnlyClient && args.Count == 0)
		{
			EnterReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Step && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(Step(VariantUtils.ConvertTo<double>(in args[0])));
			return true;
		}
		if (method == MethodName.StepReadOnlyClient && args.Count == 1)
		{
			StepReadOnlyClient(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SyncPresentationReadOnlyClient && args.Count == 0)
		{
			SyncPresentationReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Exit && args.Count == 0)
		{
			Exit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExitReadOnlyClient && args.Count == 0)
		{
			ExitReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Remove && args.Count == 0)
		{
			Remove();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cancel && args.Count == 0)
		{
			Cancel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Refresh && args.Count == 1)
		{
			Refresh(VariantUtils.ConvertTo<TowerDefenseCharacterBuffConfig>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Init)
		{
			return true;
		}
		if (method == MethodName.Enter)
		{
			return true;
		}
		if (method == MethodName.EnterReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Step)
		{
			return true;
		}
		if (method == MethodName.StepReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.SyncPresentationReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Exit)
		{
			return true;
		}
		if (method == MethodName.ExitReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Remove)
		{
			return true;
		}
		if (method == MethodName.Cancel)
		{
			return true;
		}
		if (method == MethodName.Refresh)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.time)
		{
			time = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.currentTime)
		{
			currentTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.blink)
		{
			blink = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.produceTimeScaleValue)
		{
			produceTimeScaleValue = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.FrameMeshColorMultiplier)
		{
			value = VariantUtils.CreateFrom<Color>(FrameMeshColorMultiplier);
			return true;
		}
		if (name == PropertyName.AffectsHologram)
		{
			value = VariantUtils.CreateFrom<bool>(AffectsHologram);
			return true;
		}
		if (name == PropertyName.time)
		{
			value = VariantUtils.CreateFrom(in time);
			return true;
		}
		if (name == PropertyName.currentTime)
		{
			value = VariantUtils.CreateFrom(in currentTime);
			return true;
		}
		if (name == PropertyName.blink)
		{
			value = VariantUtils.CreateFrom(in blink);
			return true;
		}
		if (name == PropertyName.produceTimeScaleValue)
		{
			value = VariantUtils.CreateFrom(in produceTimeScaleValue);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, PropertyName.FrameMeshColorMultiplier, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.time, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName.currentTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.blink, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName.produceTimeScaleValue, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.AffectsHologram, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.time, Variant.From(in time));
		info.AddProperty(PropertyName.currentTime, Variant.From(in currentTime));
		info.AddProperty(PropertyName.blink, Variant.From(in blink));
		info.AddProperty(PropertyName.produceTimeScaleValue, Variant.From(in produceTimeScaleValue));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.time, out var value))
		{
			time = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName.currentTime, out var value2))
		{
			currentTime = value2.As<double>();
		}
		if (info.TryGetProperty(PropertyName.blink, out var value3))
		{
			blink = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.produceTimeScaleValue, out var value4))
		{
			produceTimeScaleValue = value4.As<double>();
		}
	}
}
