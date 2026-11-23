using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[Tool]
[ScriptPath("res://Script/Component/TowerDefense/Character/FlashStepComponent/FlashStepComponentDefinition.cs")]
public class FlashStepComponentDefinition : CharacterComponentDefinition
{
	public new class MethodName : CharacterComponentDefinition.MethodName
	{
	}

	public new class PropertyName : CharacterComponentDefinition.PropertyName
	{
		public static readonly StringName projectileDetectionRadius = "projectileDetectionRadius";

		public static readonly StringName fellDuration = "fellDuration";
	}

	public new class SignalName : CharacterComponentDefinition.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public float projectileDetectionRadius { get; set; } = 90f;

	[Export(PropertyHint.None, "")]
	public double fellDuration { get; set; } = 10.0;

	public override CharacterComponentRuntime CreateRuntime()
	{
		return new FlashStepComponent();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.projectileDetectionRadius)
		{
			projectileDetectionRadius = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName.fellDuration)
		{
			fellDuration = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.projectileDetectionRadius)
		{
			value = VariantUtils.CreateFrom<float>(projectileDetectionRadius);
			return true;
		}
		if (name == PropertyName.fellDuration)
		{
			value = VariantUtils.CreateFrom<double>(fellDuration);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName.projectileDetectionRadius, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName.fellDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.projectileDetectionRadius, Variant.From<float>(projectileDetectionRadius));
		info.AddProperty(PropertyName.fellDuration, Variant.From<double>(fellDuration));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.projectileDetectionRadius, out var value))
		{
			projectileDetectionRadius = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName.fellDuration, out var value2))
		{
			fellDuration = value2.As<double>();
		}
	}
}
