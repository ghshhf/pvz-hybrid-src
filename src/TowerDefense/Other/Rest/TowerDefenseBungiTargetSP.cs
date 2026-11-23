using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/GraveStone/BungiTargetSP/Scene/TowerDefenseBungiTargetSP.cs")]
public class TowerDefenseBungiTargetSP : TowerDefenseGravestone
{
	public new class MethodName : TowerDefenseGravestone.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName TryStartLifetime = "TryStartLifetime";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName FinalizeProgressRestore = "FinalizeProgressRestore";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName Blow = "Blow";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName DisablePlant = "DisablePlant";

		public new static readonly StringName DestroySet = "DestroySet";
	}

	public new class PropertyName : TowerDefenseGravestone.PropertyName
	{
		public static readonly StringName lifetime = "lifetime";

		public static readonly StringName _blowStarted = "_blowStarted";

		public static readonly StringName _lifetimeTimer = "_lifetimeTimer";

		public static readonly StringName _lifetimeStarted = "_lifetimeStarted";

		public static readonly StringName _restoredLifetimeRemaining = "_restoredLifetimeRemaining";

		public static readonly StringName _blownFromSave = "_blownFromSave";

		public static readonly StringName _restoredTargetPauseStates = "_restoredTargetPauseStates";
	}

	public new class SignalName : TowerDefenseGravestone.SignalName
	{
	}

	private const string DisableMetaKey = "disabled_by_bungi_target_sp";

	private const string RefCountMetaKey = "character_disabled_ref_count";

	private const string SavedPauseMetaKey = "bungi_target_sp_saved_pause";

	public double lifetime = 15.0;

	private bool _blowStarted;

	private Timer _lifetimeTimer;

	private bool _lifetimeStarted;

	private double _restoredLifetimeRemaining = -1.0;

	private readonly HashSet<TowerDefensePlant> _disabledPlants = new HashSet<TowerDefensePlant>();

	private bool _blownFromSave;

	private Dictionary _restoredTargetPauseStates;

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint() || editorPreviewMode)
		{
			return;
		}
		instance.invincible = true;
		AddToGroup("BungiTargetSP", persistent: true);
		RemoveFromGroup("Gravestone");
		if (!base.IsProgressRestoreInFlight)
		{
			isGround = false;
			base.z = 600.0;
			Tween tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.TweenProperty(this, "z", 0.0, 0.5);
			tween.TweenCallback(Callable.From(delegate
			{
				if (GodotObject.IsInstanceValid(this))
				{
					base.z = 0.0;
					base.groundHeight = 0.0;
					isGround = true;
				}
			}));
		}
		_lifetimeTimer = new Timer
		{
			WaitTime = lifetime,
			OneShot = true
		};
		AddChild(_lifetimeTimer, forceReadableName: false, InternalMode.Disabled);
		_lifetimeTimer.Timeout += delegate
		{
			if (GodotObject.IsInstanceValid(this))
			{
				Blow();
			}
		};
		TryStartLifetime();
		if (GodotObject.IsInstanceValid(BattleEventBus.Instance))
		{
			BattleEventBus.Instance.OnBlowAllEffectEmit += Blow;
		}
	}

	private void TryStartLifetime()
	{
		if (!_lifetimeStarted && inGame && !base.IsProgressRestoreInFlight && TowerDefenseManager._IsGameRunning())
		{
			_lifetimeStarted = true;
			_lifetimeTimer.Start(Math.Max(0.001, (_restoredLifetimeRemaining >= 0.0) ? _restoredLifetimeRemaining : lifetime));
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["lifetimeRemaining"] = ((_lifetimeStarted && GodotObject.IsInstanceValid(_lifetimeTimer)) ? _lifetimeTimer.TimeLeft : ((_restoredLifetimeRemaining >= 0.0) ? _restoredLifetimeRemaining : lifetime));
		dictionary["blowStarted"] = _blowStarted;
		Dictionary dictionary2 = new Dictionary();
		if (GodotObject.IsInstanceValid(cell))
		{
			foreach (TowerDefenseCharacter character in cell.GetCharacterList())
			{
				if (GodotObject.IsInstanceValid(character) && character.HasMeta("bungi_target_sp_saved_pause"))
				{
					dictionary2[character.Name] = character.GetMeta("bungi_target_sp_saved_pause");
				}
			}
		}
		dictionary["targetPauseStates"] = dictionary2;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		double num = data.GetValueOrDefault("lifetimeRemaining", lifetime).AsDouble();
		_restoredLifetimeRemaining = (double.IsFinite(num) ? Math.Max(0.0, num) : lifetime);
		_lifetimeStarted = false;
		_blownFromSave = data.GetValueOrDefault("blowStarted", false).AsBool();
		_restoredTargetPauseStates = data.GetValueOrDefault("targetPauseStates", new Dictionary()).AsGodotDictionary();
	}

	public override void FinalizeProgressRestore()
	{
		base.FinalizeProgressRestore();
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(this) && !isDestroy)
			{
				if (GodotObject.IsInstanceValid(cell) && _restoredTargetPauseStates != null)
				{
					foreach (TowerDefenseCharacter character in cell.GetCharacterList())
					{
						if (GodotObject.IsInstanceValid(character) && character is TowerDefensePlant plant && _restoredTargetPauseStates.ContainsKey(character.Name))
						{
							character.SetMeta("bungi_target_sp_saved_pause", _restoredTargetPauseStates[character.Name]);
							DisablePlant(plant);
						}
					}
				}
				_restoredTargetPauseStates = null;
				if (_blownFromSave)
				{
					Blow();
				}
			}
		}).CallDeferred();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (GodotObject.IsInstanceValid(BattleEventBus.Instance))
		{
			BattleEventBus.Instance.OnBlowAllEffectEmit -= Blow;
		}
	}

	public override async void Blow()
	{
		if (GodotObject.IsInstanceValid(this) && !_blowStarted)
		{
			_blowStarted = true;
			HitBoxDestroy();
			Tween tween = CreateTween();
			tween.TweenProperty(this, "modulate", Colors.Transparent, 1.0);
			await ToSignal(tween, Tween.SignalName.Finished);
			Destroy();
		}
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || editorPreviewMode || !inGame || isDestroy || !TowerDefenseManager._IsGameRunning())
		{
			return;
		}
		TryStartLifetime();
		if (!GodotObject.IsInstanceValid(cell) || nearDie || die)
		{
			return;
		}
		foreach (TowerDefenseCharacter character in cell.GetCharacterList())
		{
			if (GodotObject.IsInstanceValid(character) && character is TowerDefensePlant towerDefensePlant && CanTarget(towerDefensePlant) && !(towerDefensePlant is TowerDefensePlantBowlingBase) && (character.instance.maskFlags & 2) == 0 && !towerDefensePlant.instance.invincible)
			{
				DisablePlant(towerDefensePlant);
			}
		}
	}

	private void DisablePlant(TowerDefensePlant plant)
	{
		if (_disabledPlants.Add(plant))
		{
			plant.SetMeta("disabled_by_bungi_target_sp", true);
			int num = plant.GetMeta("character_disabled_ref_count", 0).AsInt32() + 1;
			plant.SetMeta("character_disabled_ref_count", num);
		}
		if (GodotObject.IsInstanceValid(plant.sprite))
		{
			if (!plant.HasMeta("bungi_target_sp_saved_pause"))
			{
				plant.SetMeta("bungi_target_sp_saved_pause", plant.sprite.pause);
			}
			plant.sprite.pause = true;
		}
		plant.characterDisabled = true;
	}

	public override void DestroySet()
	{
		foreach (TowerDefensePlant disabledPlant in _disabledPlants)
		{
			if (!GodotObject.IsInstanceValid(disabledPlant) || !disabledPlant.HasMeta("disabled_by_bungi_target_sp"))
			{
				continue;
			}
			int num = disabledPlant.GetMeta("character_disabled_ref_count", 1).AsInt32() - 1;
			if (num <= 0)
			{
				disabledPlant.RemoveMeta("disabled_by_bungi_target_sp");
				disabledPlant.RemoveMeta("character_disabled_ref_count");
				disabledPlant.characterDisabled = false;
				if (GodotObject.IsInstanceValid(disabledPlant.sprite) && disabledPlant.HasMeta("bungi_target_sp_saved_pause"))
				{
					disabledPlant.sprite.pause = disabledPlant.GetMeta("bungi_target_sp_saved_pause").AsBool();
					disabledPlant.RemoveMeta("bungi_target_sp_saved_pause");
				}
			}
			else
			{
				disabledPlant.SetMeta("character_disabled_ref_count", num);
			}
		}
		_disabledPlants.Clear();
		base.DestroySet();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(10)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryStartLifetime, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FinalizeProgressRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Blow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DisablePlant, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "plant", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.TryStartLifetime && args.Count == 0)
		{
			TryStartLifetime();
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
		if (method == MethodName.FinalizeProgressRestore && args.Count == 0)
		{
			FinalizeProgressRestore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Blow && args.Count == 0)
		{
			Blow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisablePlant && args.Count == 1)
		{
			DisablePlant(VariantUtils.ConvertTo<TowerDefensePlant>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
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
		if (method == MethodName.TryStartLifetime)
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
		if (method == MethodName.FinalizeProgressRestore)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Blow)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.DisablePlant)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.lifetime)
		{
			lifetime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._blowStarted)
		{
			_blowStarted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._lifetimeTimer)
		{
			_lifetimeTimer = VariantUtils.ConvertTo<Timer>(in value);
			return true;
		}
		if (name == PropertyName._lifetimeStarted)
		{
			_lifetimeStarted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._restoredLifetimeRemaining)
		{
			_restoredLifetimeRemaining = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._blownFromSave)
		{
			_blownFromSave = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._restoredTargetPauseStates)
		{
			_restoredTargetPauseStates = VariantUtils.ConvertTo<Dictionary>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.lifetime)
		{
			value = VariantUtils.CreateFrom(in lifetime);
			return true;
		}
		if (name == PropertyName._blowStarted)
		{
			value = VariantUtils.CreateFrom(in _blowStarted);
			return true;
		}
		if (name == PropertyName._lifetimeTimer)
		{
			value = VariantUtils.CreateFrom(in _lifetimeTimer);
			return true;
		}
		if (name == PropertyName._lifetimeStarted)
		{
			value = VariantUtils.CreateFrom(in _lifetimeStarted);
			return true;
		}
		if (name == PropertyName._restoredLifetimeRemaining)
		{
			value = VariantUtils.CreateFrom(in _restoredLifetimeRemaining);
			return true;
		}
		if (name == PropertyName._blownFromSave)
		{
			value = VariantUtils.CreateFrom(in _blownFromSave);
			return true;
		}
		if (name == PropertyName._restoredTargetPauseStates)
		{
			value = VariantUtils.CreateFrom(in _restoredTargetPauseStates);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName.lifetime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._blowStarted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._lifetimeTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._lifetimeStarted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._restoredLifetimeRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._blownFromSave, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Dictionary, PropertyName._restoredTargetPauseStates, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.lifetime, Variant.From(in lifetime));
		info.AddProperty(PropertyName._blowStarted, Variant.From(in _blowStarted));
		info.AddProperty(PropertyName._lifetimeTimer, Variant.From(in _lifetimeTimer));
		info.AddProperty(PropertyName._lifetimeStarted, Variant.From(in _lifetimeStarted));
		info.AddProperty(PropertyName._restoredLifetimeRemaining, Variant.From(in _restoredLifetimeRemaining));
		info.AddProperty(PropertyName._blownFromSave, Variant.From(in _blownFromSave));
		info.AddProperty(PropertyName._restoredTargetPauseStates, Variant.From(in _restoredTargetPauseStates));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.lifetime, out var value))
		{
			lifetime = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._blowStarted, out var value2))
		{
			_blowStarted = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._lifetimeTimer, out var value3))
		{
			_lifetimeTimer = value3.As<Timer>();
		}
		if (info.TryGetProperty(PropertyName._lifetimeStarted, out var value4))
		{
			_lifetimeStarted = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._restoredLifetimeRemaining, out var value5))
		{
			_restoredLifetimeRemaining = value5.As<double>();
		}
		if (info.TryGetProperty(PropertyName._blownFromSave, out var value6))
		{
			_blownFromSave = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._restoredTargetPauseStates, out var value7))
		{
			_restoredTargetPauseStates = value7.As<Dictionary>();
		}
	}
}
