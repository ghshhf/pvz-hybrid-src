using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Diamond/Relife/Scene/TowerDefensePlantRelife.cs")]
public class TowerDefensePlantRelife : TowerDefensePlant
{
	private sealed class PreparedPlant
	{
		public Vector2 Scale;

		public bool Invincible;

		public bool ComponentAlive;

		public Tween Tween;
	}

	private sealed class PreparedPlantRestore
	{
		public string Name;

		public Vector2 Scale;

		public bool Invincible;

		public bool ComponentAlive;
	}

	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectCallbacks = "ConnectCallbacks";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName HandleDestroyed = "HandleDestroyed";

		public static readonly StringName RestorePreparedPlants = "RestorePreparedPlants";

		public new static readonly StringName OnCustomSwitched = "OnCustomSwitched";

		public static readonly StringName AnimeStarted = "AnimeStarted";

		public static readonly StringName Explode = "Explode";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName FinalizeProgressRestore = "FinalizeProgressRestore";

		public static readonly StringName RollbackPreparedPlantsAfterRestore = "RollbackPreparedPlantsAfterRestore";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private ExplodeComponent _explodeComponent;

	private readonly System.Collections.Generic.Dictionary<TowerDefenseCharacter, PreparedPlant> _preparedPlants = new System.Collections.Generic.Dictionary<TowerDefenseCharacter, PreparedPlant>();

	private readonly List<PreparedPlantRestore> _preparedPlantRestore = new List<PreparedPlantRestore>();

	public override void _EnterTree()
	{
		base._EnterTree();
		if (IsNodeReady() && !Engine.IsEditorHint())
		{
			ConnectCallbacks();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			ConnectCallbacks();
			if (!inGame && !editorMapPreviewMode)
			{
				((RelifeSprite)base.sprite).back.ZIndex = 0;
			}
			if (base.currentCustom.Contains("Custom0"))
			{
				((RelifeSprite)base.sprite).back.SetFliter("Pumpkin_back", open: false);
				((RelifeSprite)base.sprite).back.SetFliter("skin1_4", open: true);
			}
		}
	}

	private void ConnectCallbacks()
	{
		_explodeComponent = componentManager.GetRuntime<ExplodeComponent>();
		_explodeComponent.OnExplode += Explode;
		base.sprite.OnAnimeStarted += AnimeStarted;
		base.OnDestroy += HandleDestroyed;
	}

	public override void _ExitTree()
	{
		RestorePreparedPlants();
		base.OnDestroy -= HandleDestroyed;
		if (GodotObject.IsInstanceValid(base.sprite))
		{
			base.sprite.OnAnimeStarted -= AnimeStarted;
		}
		ExplodeComponent explodeComponent = _explodeComponent;
		if (explodeComponent != null && !explodeComponent.IsReleased)
		{
			_explodeComponent.OnExplode -= Explode;
		}
		base._ExitTree();
	}

	private void HandleDestroyed(TowerDefenseCharacter _)
	{
		RestorePreparedPlants();
	}

	private void RestorePreparedPlants()
	{
		foreach (KeyValuePair<TowerDefenseCharacter, PreparedPlant> preparedPlant in _preparedPlants)
		{
			PreparedPlant value = preparedPlant.Value;
			if (GodotObject.IsInstanceValid(value.Tween) && value.Tween.IsValid())
			{
				value.Tween.Kill();
			}
			TowerDefenseCharacter key = preparedPlant.Key;
			if (GodotObject.IsInstanceValid(key) && !key.IsQueuedForDeletion() && !key.die && !key.isDestroy)
			{
				key.transformPoint.Scale = value.Scale;
				key.instance.invincible = value.Invincible;
				key.componentAlive = value.ComponentAlive;
				key.SetHitBoxSuppressed(HitBoxSuppressionReason.Relife, suppressed: false);
				key.SetHitBoxMonitorSuppressed(HitBoxSuppressionReason.Relife, suppressed: false);
			}
		}
		_preparedPlants.Clear();
	}

	public override void OnCustomSwitched(string customKey)
	{
		if (customKey == "Custom0")
		{
			((RelifeSprite)base.sprite).back.SetFliter("Pumpkin_back", open: false);
			((RelifeSprite)base.sprite).back.SetFliter("skin1_4", open: true);
		}
		else
		{
			((RelifeSprite)base.sprite).back.SetFliter("Pumpkin_back", open: true);
			((RelifeSprite)base.sprite).back.SetFliter("skin1_4", open: false);
		}
	}

	public void AnimeStarted(string clip)
	{
		if (clip != _explodeComponent.explodeAnimeClips)
		{
			RestorePreparedPlants();
		}
		else
		{
			if (!(clip == _explodeComponent.explodeAnimeClips) || !GodotObject.IsInstanceValid(cell))
			{
				return;
			}
			foreach (TowerDefenseCharacter character in cell.characterList)
			{
				if (GodotObject.IsInstanceValid(character) && !character.die && !character.isDestroy && !_preparedPlants.ContainsKey(character) && character is TowerDefensePlant && !(character is TowerDefensePlantBowlingBase) && !character.instance.hologram && character.config.name != base.config.name && character.instance.hypnoses == instance.hypnoses)
				{
					PreparedPlant preparedPlant = new PreparedPlant
					{
						Scale = character.transformPoint.Scale,
						Invincible = character.instance.invincible,
						ComponentAlive = character.ComponentAliveSetting
					};
					_preparedPlants.Add(character, preparedPlant);
					character.instance.invincible = true;
					character.componentAlive = false;
					character.SetHitBoxSuppressed(HitBoxSuppressionReason.Relife, suppressed: true);
					character.SetHitBoxMonitorSuppressed(HitBoxSuppressionReason.Relife, suppressed: true);
					Tween tween = (preparedPlant.Tween = CreateTween());
					tween.SetEase(Tween.EaseType.InOut);
					tween.SetTrans(Tween.TransitionType.Sine);
					tween.TweenProperty(character.transformPoint, "scale", Vector2.Zero, 0.5);
				}
			}
		}
	}

	public void Explode()
	{
		foreach (PreparedPlant value in _preparedPlants.Values)
		{
			if (GodotObject.IsInstanceValid(value.Tween) && value.Tween.IsValid())
			{
				value.Tween.Kill();
			}
		}
		if (GodotObject.IsInstanceValid(cell))
		{
			foreach (TowerDefenseCharacter item in new List<TowerDefenseCharacter>(cell.characterList))
			{
				if (GodotObject.IsInstanceValid(item) && !item.die && !item.isDestroy && item is TowerDefensePlant && !(item is TowerDefensePlantBowlingBase) && !item.instance.hologram && item.config.name != base.config.name && item.instance.hypnoses == instance.hypnoses)
				{
					item.Recycle(1.0);
				}
			}
		}
		RestorePreparedPlants();
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		if (_preparedPlants.Count == 0)
		{
			return dictionary;
		}
		Array array = new Array();
		foreach (KeyValuePair<TowerDefenseCharacter, PreparedPlant> preparedPlant in _preparedPlants)
		{
			TowerDefenseCharacter key = preparedPlant.Key;
			if (GodotObject.IsInstanceValid(key))
			{
				PreparedPlant value = preparedPlant.Value;
				array.Add(new Dictionary
				{
					["name"] = key.Name.ToString(),
					["scaleX"] = value.Scale.X,
					["scaleY"] = value.Scale.Y,
					["invincible"] = value.Invincible,
					["componentAlive"] = value.ComponentAlive
				});
			}
		}
		if (array.Count > 0)
		{
			dictionary["relifePrepared"] = array;
		}
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_preparedPlantRestore.Clear();
		foreach (Variant item in data.GetValueOrDefault("relifePrepared", new Array()).AsGodotArray())
		{
			Dictionary dictionary = item.AsGodotDictionary();
			_preparedPlantRestore.Add(new PreparedPlantRestore
			{
				Name = dictionary.GetValueOrDefault("name", "").AsString(),
				Scale = new Vector2(dictionary.GetValueOrDefault("scaleX", 1f).AsSingle(), dictionary.GetValueOrDefault("scaleY", 1f).AsSingle()),
				Invincible = dictionary.GetValueOrDefault("invincible", false).AsBool(),
				ComponentAlive = dictionary.GetValueOrDefault("componentAlive", true).AsBool()
			});
		}
	}

	public override void FinalizeProgressRestore()
	{
		base.FinalizeProgressRestore();
		if (_preparedPlantRestore.Count > 0)
		{
			Callable.From(RollbackPreparedPlantsAfterRestore).CallDeferred();
		}
	}

	private void RollbackPreparedPlantsAfterRestore()
	{
		if (_preparedPlantRestore.Count == 0)
		{
			return;
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(towerDefenseManager))
		{
			return;
		}
		Array character = towerDefenseManager.GetCharacter();
		foreach (PreparedPlantRestore item in _preparedPlantRestore)
		{
			TowerDefenseCharacter towerDefenseCharacter = null;
			for (int i = 0; i < character.Count; i++)
			{
				if (character[i].AsGodotObject() is TowerDefenseCharacter towerDefenseCharacter2 && towerDefenseCharacter2.Name.ToString() == item.Name)
				{
					towerDefenseCharacter = towerDefenseCharacter2;
					break;
				}
			}
			if (GodotObject.IsInstanceValid(towerDefenseCharacter) && !towerDefenseCharacter.die && !towerDefenseCharacter.isDestroy)
			{
				towerDefenseCharacter.transformPoint.Scale = item.Scale;
				towerDefenseCharacter.instance.invincible = item.Invincible;
				towerDefenseCharacter.componentAlive = item.ComponentAlive;
				towerDefenseCharacter.SetHitBoxSuppressed(HitBoxSuppressionReason.Relife, suppressed: false);
				towerDefenseCharacter.SetHitBoxMonitorSuppressed(HitBoxSuppressionReason.Relife, suppressed: false);
			}
		}
		_preparedPlantRestore.Clear();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(13)
		{
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConnectCallbacks, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HandleDestroyed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.RestorePreparedPlants, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCustomSwitched, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "customKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Explode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FinalizeProgressRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RollbackPreparedPlantsAfterRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectCallbacks && args.Count == 0)
		{
			ConnectCallbacks();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HandleDestroyed && args.Count == 1)
		{
			HandleDestroyed(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestorePreparedPlants && args.Count == 0)
		{
			RestorePreparedPlants();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCustomSwitched && args.Count == 1)
		{
			OnCustomSwitched(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeStarted && args.Count == 1)
		{
			AnimeStarted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Explode && args.Count == 0)
		{
			Explode();
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
		if (method == MethodName.RollbackPreparedPlantsAfterRestore && args.Count == 0)
		{
			RollbackPreparedPlantsAfterRestore();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ConnectCallbacks)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.HandleDestroyed)
		{
			return true;
		}
		if (method == MethodName.RestorePreparedPlants)
		{
			return true;
		}
		if (method == MethodName.OnCustomSwitched)
		{
			return true;
		}
		if (method == MethodName.AnimeStarted)
		{
			return true;
		}
		if (method == MethodName.Explode)
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
		if (method == MethodName.RollbackPreparedPlantsAfterRestore)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
