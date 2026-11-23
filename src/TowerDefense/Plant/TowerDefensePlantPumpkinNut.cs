using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Other/PumpkinNut/Scene/TowerDefensePlantPumpkinNut.cs")]
public class TowerDefensePlantPumpkinNut : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName TryGrantInitialShield = "TryGrantInitialShield";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnBuffDelete = "OnBuffDelete";

		public static readonly StringName Timeout = "Timeout";

		public static readonly StringName RegenShield = "RegenShield";

		public static readonly StringName CostHealth = "CostHealth";

		public new static readonly StringName Hypnoses = "Hypnoses";

		public static readonly StringName _SyncShieldHypnoses = "_SyncShieldHypnoses";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ImportVariantSaveWhenEmpty = "ImportVariantSaveWhenEmpty";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _initialShieldGranted = "_initialShieldGranted";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private static readonly StringName SHIELD_TYPE = new StringName("Default");

	public const double SHIELD_HP = 4000.0;

	public const double SHIELD_REGEN_PER_SEC = 200.0;

	private CharacterTimerComponent _timerComponent;

	private bool _initialShieldGranted;

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			return;
		}
		if (!inGame && !editorMapPreviewMode)
		{
			PumpkinNutSprite pumpkinNutSprite = base.sprite as PumpkinNutSprite;
			if (GodotObject.IsInstanceValid(pumpkinNutSprite) && GodotObject.IsInstanceValid(pumpkinNutSprite.back))
			{
				pumpkinNutSprite.back.ZIndex = 0;
			}
		}
		_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout += Timeout;
		}
		BuffComponent buffComponent = buff;
		if (buffComponent != null && !buffComponent.IsReleased)
		{
			buff.OnBuffDelete += OnBuffDelete;
		}
		TryGrantInitialShield();
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		TryGrantInitialShield();
	}

	private void TryGrantInitialShield()
	{
		if (_initialShieldGranted || !inGame || editorPreviewMode || base.IsProgressRestoreInFlight || isDestroy || !TowerDefenseManager._IsGameRunning() || !GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		_initialShieldGranted = true;
		_timerComponent?.Run("Regen", 1.0);
		if (TowerDefenseManager.HasGameplayAuthority)
		{
			TowerDefenseItemSheild.CreateOnCellWithHP(cell, SHIELD_TYPE, 4000.0);
			if (instance.hypnoses)
			{
				Callable.From(_SyncShieldHypnoses).CallDeferred();
			}
		}
	}

	public override void _ExitTree()
	{
		BuffComponent buffComponent = buff;
		if (buffComponent != null && !buffComponent.IsReleased)
		{
			buff.OnBuffDelete -= OnBuffDelete;
		}
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= Timeout;
		}
		base._ExitTree();
	}

	private void OnBuffDelete(string key)
	{
		if (key == "Hypnoses")
		{
			_SyncShieldHypnoses();
		}
	}

	public void Timeout(string timerName)
	{
		if (timerName == "Regen" && GodotObject.IsInstanceValid(instance) && !instance.sleep)
		{
			_timerComponent.Run("Regen", 1.0);
			RegenShield();
		}
	}

	private void RegenShield()
	{
		if (!GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		TowerDefenseItemSheild itemShield = cell.itemShield;
		if (!GodotObject.IsInstanceValid(itemShield) || !GodotObject.IsInstanceValid(itemShield.instance) || itemShield.instance.hitpoints <= 0.0)
		{
			TowerDefenseItemSheild.CreateOnCellWithHP(cell, SHIELD_TYPE, 200.0);
			CostHealth(200.0);
			if (instance.hypnoses)
			{
				Callable.From(_SyncShieldHypnoses).CallDeferred();
			}
			return;
		}
		double hitpoints = itemShield.instance.hitpoints;
		if (!(hitpoints >= 4000.0))
		{
			double num = Mathf.Min(4000.0 - hitpoints, 200.0);
			itemShield.ShieldAddHitpoints(num, SHIELD_TYPE);
			CostHealth(num);
		}
	}

	private void CostHealth(double num)
	{
		if (!(num <= 0.0))
		{
			instance.SkipInvincibleDealHurt(num, playSplatAudio: false, Vector2.Zero, createDamagePart: false);
		}
	}

	public override void Hypnoses(double time = -1.0, bool canFliter = true, TowerDefenseCharacterBuffHypnoses hypnosesConfig = null)
	{
		base.Hypnoses(time, canFliter, hypnosesConfig);
		_SyncShieldHypnoses();
	}

	private void _SyncShieldHypnoses()
	{
		if (GodotObject.IsInstanceValid(cell) && GodotObject.IsInstanceValid(cell.itemShield))
		{
			TowerDefenseItemSheild itemShield = cell.itemShield;
			if (itemShield.instance.hypnoses != instance.hypnoses)
			{
				itemShield.Hypnoses();
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary { ["initialShieldGranted"] = _initialShieldGranted };
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_initialShieldGranted = data.GetValueOrDefault("initialShieldGranted", true).AsBool();
	}

	public override bool ImportVariantSaveWhenEmpty()
	{
		return true;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(13)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TryGrantInitialShield, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnBuffDelete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "key", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Timeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RegenShield, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CostHealth, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "num", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Hypnoses, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "time", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "canFliter", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "hypnosesConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null),
			new MethodInfo(MethodName._SyncShieldHypnoses, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ImportVariantSaveWhenEmpty, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryGrantInitialShield && args.Count == 0)
		{
			TryGrantInitialShield();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBuffDelete && args.Count == 1)
		{
			OnBuffDelete(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Timeout && args.Count == 1)
		{
			Timeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RegenShield && args.Count == 0)
		{
			RegenShield();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CostHealth && args.Count == 1)
		{
			CostHealth(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Hypnoses && args.Count == 3)
		{
			Hypnoses(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<TowerDefenseCharacterBuffHypnoses>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._SyncShieldHypnoses && args.Count == 0)
		{
			_SyncShieldHypnoses();
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
		if (method == MethodName.ImportVariantSaveWhenEmpty && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(ImportVariantSaveWhenEmpty());
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
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.TryGrantInitialShield)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnBuffDelete)
		{
			return true;
		}
		if (method == MethodName.Timeout)
		{
			return true;
		}
		if (method == MethodName.RegenShield)
		{
			return true;
		}
		if (method == MethodName.CostHealth)
		{
			return true;
		}
		if (method == MethodName.Hypnoses)
		{
			return true;
		}
		if (method == MethodName._SyncShieldHypnoses)
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
		if (method == MethodName.ImportVariantSaveWhenEmpty)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._initialShieldGranted)
		{
			_initialShieldGranted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._initialShieldGranted)
		{
			value = VariantUtils.CreateFrom(in _initialShieldGranted);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._initialShieldGranted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._initialShieldGranted, Variant.From(in _initialShieldGranted));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._initialShieldGranted, out var value))
		{
			_initialShieldGranted = value.As<bool>();
		}
	}
}
