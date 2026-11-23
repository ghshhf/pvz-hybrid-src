using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/HybridPark/PotatoPumpkin/Scene/TowerDefensePlantPotatoPumpkin.cs")]
public class TowerDefensePlantPotatoPumpkin : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Grow = "Grow";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _grown = "_grown";

		public static readonly StringName _contactExploded = "_contactExploded";

		public static readonly StringName _receivedNetworkState = "_receivedNetworkState";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private GrowUpComponent _growUpComponent;

	private ExplodeComponent _explodeComponent;

	private AttackComponent _attackComponent;

	private bool _grown;

	private bool _contactExploded;

	private bool _receivedNetworkState;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_growUpComponent = componentManager.GetRuntime<GrowUpComponent>();
			_explodeComponent = componentManager.GetRuntime<ExplodeComponent>();
			_attackComponent = componentManager.GetRuntime<AttackComponent>();
			GrowUpComponent growUpComponent = _growUpComponent;
			if (growUpComponent != null && !growUpComponent.IsReleased)
			{
				_growUpComponent.OnGrow += Grow;
			}
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (IsNodeReady() && !Engine.IsEditorHint())
		{
			GrowUpComponent growUpComponent = _growUpComponent;
			if (growUpComponent != null && !growUpComponent.IsReleased)
			{
				_growUpComponent.OnGrow += Grow;
			}
		}
	}

	public override void _ExitTree()
	{
		GrowUpComponent growUpComponent = _growUpComponent;
		if (growUpComponent != null && !growUpComponent.IsReleased)
		{
			_growUpComponent.OnGrow -= Grow;
		}
		base._ExitTree();
	}

	private void Grow(int reach)
	{
		_grown = true;
		instance.height = TowerDefenseEnum.CHARACTER_HEIGHT.NORMAL;
		idleAnimeClip = "Idle";
		if (_growUpComponent.IsApplyingRestoredState)
		{
			base.sprite.SetAnimation("Idle");
			return;
		}
		base.sprite.SetAnimation("Rise", loop: false);
		base.sprite.AddAnimation("Idle", 0.0);
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		if (TowerDefenseManager.HasGameplayAuthority && CanRunAbility() && _grown && !_contactExploded)
		{
			AttackComponent attackComponent = _attackComponent;
			if (attackComponent != null && !attackComponent.IsReleased && _attackComponent.CanAttackOnContact())
			{
				_contactExploded = true;
				_explodeComponent?.Explode();
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["grown"] = _grown,
			["contactExploded"] = _contactExploded
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_grown = data.GetValueOrDefault("grown", false).AsBool();
		_contactExploded = data.GetValueOrDefault("contactExploded", false).AsBool();
		idleAnimeClip = (_grown ? "Idle" : "Ready");
		instance.height = ((!_grown) ? TowerDefenseEnum.CHARACTER_HEIGHT.LOW : TowerDefenseEnum.CHARACTER_HEIGHT.NORMAL);
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportVariantSave();
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		bool num = _receivedNetworkState && !_contactExploded && data.GetValueOrDefault("contactExploded", false).AsBool();
		ImportVariantSave(data);
		_receivedNetworkState = true;
		if (num && !TowerDefenseManager.HasGameplayAuthority)
		{
			_explodeComponent?.Explode();
		}
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return (_grown ? 1 : 0) | (_contactExploded ? 2 : 0);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(10)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Grow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "reach", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Grow && args.Count == 1)
		{
			Grow(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
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
		if (method == MethodName.ExportNetworkSpecialState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpecialState());
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState && args.Count == 1)
		{
			ImportNetworkSpecialState(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(GetNetworkSpecialStateRevision());
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Grow)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
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
		if (method == MethodName.ExportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._grown)
		{
			_grown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._contactExploded)
		{
			_contactExploded = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._receivedNetworkState)
		{
			_receivedNetworkState = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._grown)
		{
			value = VariantUtils.CreateFrom(in _grown);
			return true;
		}
		if (name == PropertyName._contactExploded)
		{
			value = VariantUtils.CreateFrom(in _contactExploded);
			return true;
		}
		if (name == PropertyName._receivedNetworkState)
		{
			value = VariantUtils.CreateFrom(in _receivedNetworkState);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._grown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._contactExploded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._receivedNetworkState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._grown, Variant.From(in _grown));
		info.AddProperty(PropertyName._contactExploded, Variant.From(in _contactExploded));
		info.AddProperty(PropertyName._receivedNetworkState, Variant.From(in _receivedNetworkState));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._grown, out var value))
		{
			_grown = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._contactExploded, out var value2))
		{
			_contactExploded = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._receivedNetworkState, out var value3))
		{
			_receivedNetworkState = value3.As<bool>();
		}
	}
}
