using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter5/PotatoCobCannon/Scene/TowerDefensePlantPotatoCobCannon.cs")]
public class TowerDefensePlantPotatoCobCannon : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnCustomSwitched = "OnCustomSwitched";

		public static readonly StringName ResolveSkinName = "ResolveSkinName";

		public static readonly StringName ResolveCustomIndex = "ResolveCustomIndex";

		public static readonly StringName ApplySkinName = "ApplySkinName";

		public static readonly StringName ResolveCobTexture = "ResolveCobTexture";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName restTime = "restTime";

		public static readonly StringName skinName = "skinName";

		public static readonly StringName _defaultCobTexture = "_defaultCobTexture";

		public static readonly StringName _restTime = "_restTime";

		public static readonly StringName _skinName = "_skinName";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private const string Skin1Name = "TurtleEgg";

	private const string Skin1CobTextureUid = "uid://dyvuq23hqmjl0";

	private static readonly string[] CustomSkinNames = new string[1] { "TurtleEgg" };

	private static readonly string[] CustomCobTextureUids = new string[1] { "uid://dyvuq23hqmjl0" };

	private const string DefaultSkinName = "Default";

	public CannonComponent cannonComponent;

	private Texture2D _defaultCobTexture;

	private double _restTime = 30.0;

	private string _skinName = "Default";

	[Export(PropertyHint.None, "")]
	public double restTime
	{
		get
		{
			return _restTime;
		}
		set
		{
			_restTime = value;
			if (IsNodeReady() && cannonComponent != null)
			{
				CannonComponent obj = cannonComponent;
				if (obj != null && !obj.IsReleased)
				{
					cannonComponent.restTime = (float)value;
				}
			}
		}
	}

	public string skinName
	{
		get
		{
			return _skinName;
		}
		set
		{
			_skinName = value;
			ApplySkinName();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			cannonComponent = componentManager.GetRuntime<CannonComponent>();
			_defaultCobTexture = (cannonComponent?.projectileNode as Sprite2D)?.Texture;
			skinName = ResolveSkinName(base.currentCustom);
		}
	}

	public override void OnCustomSwitched(string customKey)
	{
		base.OnCustomSwitched(customKey);
		skinName = ResolveSkinName(customKey);
	}

	private static string ResolveSkinName(string customKey)
	{
		int num = ResolveCustomIndex(customKey);
		if (num >= 0 && num < CustomSkinNames.Length)
		{
			return CustomSkinNames[num];
		}
		return "Default";
	}

	private static string ResolveSkinName(Array<string> customKeys)
	{
		foreach (string customKey in customKeys)
		{
			string text = ResolveSkinName(customKey);
			if (text != "Default")
			{
				return text;
			}
		}
		return "Default";
	}

	private static int ResolveCustomIndex(string customKey)
	{
		if (!string.IsNullOrEmpty(customKey) && customKey.StartsWith("Custom", StringComparison.Ordinal) && int.TryParse(customKey.Substring("Custom".Length), out var result))
		{
			return result;
		}
		return -1;
	}

	private void ApplySkinName()
	{
		if (IsNodeReady() && cannonComponent != null && !cannonComponent.IsReleased)
		{
			if (cannonComponent.projectileData != null)
			{
				cannonComponent.projectileData.skinName = new StringName(_skinName);
			}
			if (cannonComponent.projectileNode is Sprite2D sprite2D)
			{
				sprite2D.Texture = ResolveCobTexture();
			}
		}
	}

	private Texture2D ResolveCobTexture()
	{
		for (int i = 0; i < CustomSkinNames.Length; i++)
		{
			if (!(CustomSkinNames[i] != _skinName))
			{
				Texture2D texture2D = GD.Load<Texture2D>(CustomCobTextureUids[i]);
				if (texture2D == null)
				{
					break;
				}
				return texture2D;
			}
		}
		return _defaultCobTexture;
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			{ "restTime", restTime },
			{ "skinName", skinName }
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		restTime = data.GetValueOrDefault("restTime", 30).AsDouble();
		if (data.ContainsKey("skinName"))
		{
			skinName = data["skinName"].AsString();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(8)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCustomSwitched, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "customKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResolveSkinName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "customKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResolveCustomIndex, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "customKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplySkinName, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ResolveCobTexture, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnCustomSwitched && args.Count == 1)
		{
			OnCustomSwitched(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResolveSkinName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveSkinName(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ResolveCustomIndex && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<int>(ResolveCustomIndex(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ApplySkinName && args.Count == 0)
		{
			ApplySkinName();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResolveCobTexture && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(ResolveCobTexture());
			return true;
		}
		if (method == MethodName.ExportVariantSave && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportVariantSave());
			return true;
		}
		if (method == MethodName.ImportVariantSave && args.Count == 1)
		{
			ImportVariantSave(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ResolveSkinName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveSkinName(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ResolveCustomIndex && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<int>(ResolveCustomIndex(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnCustomSwitched)
		{
			return true;
		}
		if (method == MethodName.ResolveSkinName)
		{
			return true;
		}
		if (method == MethodName.ResolveCustomIndex)
		{
			return true;
		}
		if (method == MethodName.ApplySkinName)
		{
			return true;
		}
		if (method == MethodName.ResolveCobTexture)
		{
			return true;
		}
		if (method == MethodName.ExportVariantSave)
		{
			return true;
		}
		if (method == MethodName.ImportVariantSave)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.restTime)
		{
			restTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.skinName)
		{
			skinName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._defaultCobTexture)
		{
			_defaultCobTexture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._restTime)
		{
			_restTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._skinName)
		{
			_skinName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.restTime)
		{
			value = VariantUtils.CreateFrom<double>(restTime);
			return true;
		}
		if (name == PropertyName.skinName)
		{
			value = VariantUtils.CreateFrom<string>(skinName);
			return true;
		}
		if (name == PropertyName._defaultCobTexture)
		{
			value = VariantUtils.CreateFrom(in _defaultCobTexture);
			return true;
		}
		if (name == PropertyName._restTime)
		{
			value = VariantUtils.CreateFrom(in _restTime);
			return true;
		}
		if (name == PropertyName._skinName)
		{
			value = VariantUtils.CreateFrom(in _skinName);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._defaultCobTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._restTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.restTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.skinName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._skinName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.restTime, Variant.From<double>(restTime));
		info.AddProperty(PropertyName.skinName, Variant.From<string>(skinName));
		info.AddProperty(PropertyName._defaultCobTexture, Variant.From(in _defaultCobTexture));
		info.AddProperty(PropertyName._restTime, Variant.From(in _restTime));
		info.AddProperty(PropertyName._skinName, Variant.From(in _skinName));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.restTime, out var value))
		{
			restTime = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName.skinName, out var value2))
		{
			skinName = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._defaultCobTexture, out var value3))
		{
			_defaultCobTexture = value3.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._restTime, out var value4))
		{
			_restTime = value4.As<double>();
		}
		if (info.TryGetProperty(PropertyName._skinName, out var value5))
		{
			_skinName = value5.As<string>();
		}
	}
}
