using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/HybridPark/MagnetPot/Scene/TowerDefensePlantMagnetPot.cs")]
public class TowerDefensePlantMagnetPot : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName Timeout = "Timeout";

		public static readonly StringName ScanCellMagnetPlants = "ScanCellMagnetPlants";

		public static readonly StringName StartDigest = "StartDigest";

		public static readonly StringName FinishDigest = "FinishDigest";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName IsDigesting = "IsDigesting";

		public static readonly StringName _digestTimer = "_digestTimer";

		public static readonly StringName _digesting = "_digesting";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	public const double DigestTime = 20.0;

	public const string ShootClip = "Shooting";

	public const string DigestClip = "NonactiveIdle2";

	private const double ScanInterval = 0.2;

	private CharacterTimerComponent _timerComponent;

	private double _digestTimer;

	private bool _digesting;

	public bool IsDigesting => _digesting;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			if (_timerComponent != null)
			{
				_timerComponent.OnTimeout += Timeout;
			}
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= Timeout;
		}
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || !TowerDefenseManager.Instance.IsGameRunning() || !inGame)
		{
			return;
		}
		if (_digesting)
		{
			_digestTimer -= delta * timeScale;
			if (_digestTimer <= 0.0)
			{
				FinishDigest();
			}
		}
		else if (_timerComponent != null && !_timerComponent.IsRunning("Scan"))
		{
			_timerComponent.Run("Scan", 0.2);
		}
	}

	public void Timeout(string timerName)
	{
		if (timerName == "Scan")
		{
			if (TowerDefenseManager.HasGameplayAuthority && !_digesting)
			{
				ScanCellMagnetPlants();
			}
			_timerComponent.Run("Scan", 0.2);
		}
	}

	public void ScanCellMagnetPlants()
	{
		if (!GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		foreach (TowerDefenseCharacter character in cell.GetCharacterList())
		{
			if (GodotObject.IsInstanceValid(character) && character != this && character is TowerDefensePlant && character.camp == base.camp)
			{
				MagnetComponent magnetComponent = character.componentManager?.GetRuntime<MagnetComponent>();
				if (magnetComponent != null && GodotObject.IsInstanceValid(magnetComponent.breakDownArmor))
				{
					magnetComponent.BreakDownOver();
					StartDigest();
					break;
				}
			}
		}
	}

	public void StartDigest()
	{
		_digesting = true;
		_digestTimer = 20.0;
		if (GodotObject.IsInstanceValid(base.sprite))
		{
			base.sprite.SetAnimation("Shooting", loop: false);
			base.sprite.AddAnimation("NonactiveIdle2", 0.0);
		}
	}

	public void FinishDigest()
	{
		_digesting = false;
		_digestTimer = 0.0;
		if (GodotObject.IsInstanceValid(base.sprite) && !string.IsNullOrEmpty(idleAnimeClip))
		{
			base.sprite.SetAnimation(idleAnimeClip, loop: true, 0.2);
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			{ "digesting", _digesting },
			{ "digestTimer", _digestTimer }
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_digesting = data.GetValueOrDefault("digesting", false).AsBool();
		_digestTimer = data.GetValueOrDefault("digestTimer", 0.0).AsDouble();
		if (_digesting && GodotObject.IsInstanceValid(base.sprite))
		{
			base.sprite.SetAnimation("NonactiveIdle2");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(9)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Timeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ScanCellMagnetPlants, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartDigest, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FinishDigest, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Timeout && args.Count == 1)
		{
			Timeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ScanCellMagnetPlants && args.Count == 0)
		{
			ScanCellMagnetPlants();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDigest && args.Count == 0)
		{
			StartDigest();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FinishDigest && args.Count == 0)
		{
			FinishDigest();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.Timeout)
		{
			return true;
		}
		if (method == MethodName.ScanCellMagnetPlants)
		{
			return true;
		}
		if (method == MethodName.StartDigest)
		{
			return true;
		}
		if (method == MethodName.FinishDigest)
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
		if (name == PropertyName._digestTimer)
		{
			_digestTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._digesting)
		{
			_digesting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsDigesting)
		{
			value = VariantUtils.CreateFrom<bool>(IsDigesting);
			return true;
		}
		if (name == PropertyName._digestTimer)
		{
			value = VariantUtils.CreateFrom(in _digestTimer);
			return true;
		}
		if (name == PropertyName._digesting)
		{
			value = VariantUtils.CreateFrom(in _digesting);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName._digestTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._digesting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsDigesting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._digestTimer, Variant.From(in _digestTimer));
		info.AddProperty(PropertyName._digesting, Variant.From(in _digesting));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._digestTimer, out var value))
		{
			_digestTimer = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._digesting, out var value2))
		{
			_digesting = value2.As<bool>();
		}
	}
}
