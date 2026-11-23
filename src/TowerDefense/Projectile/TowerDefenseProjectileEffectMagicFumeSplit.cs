using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Prefab/ProjectileEffect/MagicFume/TowerDefenseProjectileEffectMagicFumeSplit.cs")]
public class TowerDefenseProjectileEffectMagicFumeSplit : TowerDefenseProjectileEffectBase
{
	public new class MethodName : TowerDefenseProjectileEffectBase.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : TowerDefenseProjectileEffectBase.PropertyName
	{
		public static readonly StringName releaseDelay = "releaseDelay";
	}

	public new class SignalName : TowerDefenseProjectileEffectBase.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public float releaseDelay = 1.1f;

	public override void _Ready()
	{
		foreach (Node child in GetChildren())
		{
			if (child is GpuParticles2D gpuParticles2D)
			{
				gpuParticles2D.Restart();
			}
		}
		CreateTween().TweenCallback(Callable.From(base.QueueFree)).SetDelay(Mathf.Max(0.1f, releaseDelay));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(1)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.releaseDelay)
		{
			releaseDelay = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.releaseDelay)
		{
			value = VariantUtils.CreateFrom(in releaseDelay);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName.releaseDelay, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.releaseDelay, Variant.From(in releaseDelay));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.releaseDelay, out var value))
		{
			releaseDelay = value.As<float>();
		}
	}
}
