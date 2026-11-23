using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Diamond/CubeBox/Scene/TowerDefensePlantCubeBox.cs")]
public class TowerDefensePlantCubeBox : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName PreSpawn = "PreSpawn";

		public static readonly StringName OnExplode = "OnExplode";

		public static readonly StringName SummonAllHolograms = "SummonAllHolograms";

		public static readonly StringName TrySummonHologram = "TrySummonHologram";

		public static readonly StringName ApplyHologramState = "ApplyHologramState";

		public static readonly StringName SyncHologramSpawn = "SyncHologramSpawn";

		public new static readonly StringName OnHypnosisStateChanged = "OnHypnosisStateChanged";

		public static readonly StringName RemoveHologram = "RemoveHologram";

		public static readonly StringName ForceRefreshSprites = "ForceRefreshSprites";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public static readonly StringName RestorePendingHolograms = "RestorePendingHolograms";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _converted = "_converted";

		public static readonly StringName _ownCellSummoned = "_ownCellSummoned";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private const double HologramDuration = 30.0;

	private ExplodeComponent _explodeComponent;

	private bool _converted;

	private bool _ownCellSummoned;

	private List<TowerDefenseCharacter> _pendingHolograms = new List<TowerDefenseCharacter>();

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			_explodeComponent = componentManager.GetRuntime<ExplodeComponent>();
			ExplodeComponent explodeComponent = _explodeComponent;
			if (explodeComponent != null && !explodeComponent.IsReleased)
			{
				_explodeComponent.OnExplode += OnExplode;
			}
		}
	}

	public override void _ExitTree()
	{
		ExplodeComponent explodeComponent = _explodeComponent;
		if (explodeComponent != null && !explodeComponent.IsReleased)
		{
			_explodeComponent.OnExplode -= OnExplode;
		}
		base._ExitTree();
	}

	public override void PreSpawn()
	{
		base.PreSpawn();
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(this) && !isDestroy && !_converted)
			{
				_explodeComponent?.Explode();
				Destroy();
			}
		}).CallDeferred();
	}

	private void OnExplode()
	{
		if (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost)
		{
			SummonAllHolograms();
		}
	}

	private void SummonAllHolograms()
	{
		if (_converted)
		{
			return;
		}
		_converted = true;
		_ownCellSummoned = true;
		if (GodotObject.IsInstanceValid(cell))
		{
			cell.RemoveCharacter(this);
		}
		List<TowerDefenseCharacter> list = new List<TowerDefenseCharacter>();
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Vector2I targetGridPos = base.gridPos + new Vector2I(i, j);
				TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(targetGridPos);
				if (GodotObject.IsInstanceValid(mapCell))
				{
					TowerDefenseCharacter item = TrySummonHologram(mapCell, targetGridPos);
					if (GodotObject.IsInstanceValid(item))
					{
						list.Add(item);
					}
				}
			}
		}
		ScheduleExpire(list);
	}

	private TowerDefenseCharacter TrySummonHologram(TowerDefenseCellInstance cell, Vector2I targetGridPos)
	{
		TowerDefensePacketBankData packetBankData = TowerDefenseManager.GetPacketBankData(((double)GD.Randf() > 0.5) ? "PlantPresentBox" : "GeneralPlant");
		if (!GodotObject.IsInstanceValid(packetBankData))
		{
			return null;
		}
		Array plantList = packetBankData.GetPlantList();
		if (plantList.Count == 0)
		{
			return null;
		}
		if (GodotObject.IsInstanceValid(packet))
		{
			plantList.Remove(packet.saveKey);
		}
		if (plantList.Count == 0)
		{
			return null;
		}
		string text = plantList.PickRandom().AsString();
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(text);
		while (plantList.Count > 1 && GodotObject.IsInstanceValid(packetConfig) && ((instance.hypnoses && (packetConfig.characterConfig.unUseBuffFlags & 8) != 0) || !cell.CanPacketPlant(packetConfig)))
		{
			plantList.Remove(text);
			text = plantList.PickRandom().AsString();
			packetConfig = TowerDefenseManager.GetPacketConfig(text);
		}
		if (plantList.Count == 0 || !GodotObject.IsInstanceValid(packetConfig) || !cell.CanPacketPlant(packetConfig) || (instance.hypnoses && (packetConfig.characterConfig.unUseBuffFlags & 8) != 0))
		{
			return null;
		}
		bool hypnoses = instance.hypnoses;
		TowerDefenseCharacter plant = packetConfig.Plant(targetGridPos, playAudio: false);
		if (!GodotObject.IsInstanceValid(plant))
		{
			return null;
		}
		_pendingHolograms.Add(plant);
		if (!TowerDefenseManager.Instance.IsGameRunning() && TowerDefenseManager.CurrentControl.GetFeature("PreSpawn") is TowerDefenseBattleFeaturePreSpawn towerDefenseBattleFeaturePreSpawn)
		{
			towerDefenseBattleFeaturePreSpawn.preSpawnList.Add(plant);
		}
		SyncHologramSpawn(plant, text, targetGridPos, hypnoses);
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(plant))
			{
				if (hypnoses)
				{
					plant.Hypnoses();
				}
				ApplyHologramState(plant);
				plant.CallDeferred("WakeUp");
			}
		}).CallDeferred();
		return plant;
	}

	private static void ApplyHologramState(TowerDefenseCharacter character)
	{
		character.SetSpriteGroupShaderParameter("hologram", true);
		character.instance.hologram = true;
		character.instance.canBeCollection = false;
		character.skipDestroySet = true;
		ForceRefreshSprites(character.spriteGroup);
	}

	private void SyncHologramSpawn(TowerDefenseCharacter plant, string plantName, Vector2I targetGridPos, bool hypnoses)
	{
		if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
		{
			TowerDefenseControlNew currentControl = TowerDefenseManager.CurrentControl;
			if (GodotObject.IsInstanceValid(currentControl))
			{
				int nextSyncId = currentControl.GetNextSyncId();
				currentControl.RegisterSyncCharacter(nextSyncId, plant);
				Dictionary spawnState = new Dictionary
				{
					["hologram"] = true,
					["plant_play_audio"] = false
				};
				MultiPlayerManager.Instance.SendSpawnCharacterAt(plantName, targetGridPos.X, targetGridPos.Y, nextSyncId, 1.0, 1.0, hypnoses, 0.0, useCreate: false, 0.0, 0.0, walkAfterSpawn: false, 0.0, "", spawnState);
			}
		}
	}

	private void ScheduleExpire(List<TowerDefenseCharacter> batch)
	{
		if (batch == null || batch.Count == 0)
		{
			return;
		}
		foreach (TowerDefenseCharacter item in batch)
		{
			ExpireHologramAsync(item);
		}
	}

	private static async Task ExpireHologramAsync(TowerDefenseCharacter character)
	{
		bool flag = GodotObject.IsInstanceValid(character);
		if (flag)
		{
			flag = await character.WaitForAbilityTimeAsync(30.0);
		}
		if (flag)
		{
			RemoveHologram(character);
		}
	}

	protected internal override void OnHypnosisStateChanged()
	{
		base.OnHypnosisStateChanged();
		bool hypnoses = instance.hypnoses;
		foreach (TowerDefenseCharacter pendingHologram in _pendingHolograms)
		{
			if (GodotObject.IsInstanceValid(pendingHologram) && !pendingHologram.isDestroy)
			{
				pendingHologram.SyncHologramHypnoses(hypnoses);
			}
		}
	}

	private static void ExpireHolograms(List<TowerDefenseCharacter> holograms)
	{
		GD.Print($"[CubeBox] ExpireHolograms count={holograms.Count}");
		foreach (TowerDefenseCharacter hologram in holograms)
		{
			if (GodotObject.IsInstanceValid(hologram) && !hologram.isDestroy)
			{
				RemoveHologram(hologram);
			}
		}
	}

	private static void RemoveHologram(TowerDefenseCharacter character)
	{
		character.EmitDestroy();
		if (GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			TowerDefenseManager.Instance.CharacterUnregister(character);
		}
		character.RemoveFromGroup("Character");
		character.QueueFree();
	}

	private static void ForceRefreshSprites(Node root)
	{
		if (GodotObject.IsInstanceValid(root))
		{
			if (root is AdobeAnimateSpriteBase adobeAnimateSpriteBase)
			{
				adobeAnimateSpriteBase.SetRenderColorMultiplier(new Color(1f, 1f, 1f, 0.999f));
				adobeAnimateSpriteBase.SetRenderColorMultiplier(Colors.White);
			}
			for (int i = 0; i < root.GetChildCount(); i++)
			{
				ForceRefreshSprites(root.GetChild(i));
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			{ "converted", _converted },
			{ "ownCellSummoned", _ownCellSummoned }
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_converted = data.GetValueOrDefault("converted", false).AsBool();
		_ownCellSummoned = data.GetValueOrDefault("ownCellSummoned", false).AsBool();
		if (_converted || _ownCellSummoned)
		{
			Callable.From(RestorePendingHolograms).CallDeferred();
		}
	}

	private void RestorePendingHolograms()
	{
		if (Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return;
		}
		List<TowerDefenseCharacter> cleanCharactersList = TowerDefenseManager.Instance.characterRegistry.GetCleanCharactersList();
		for (int i = 0; i < cleanCharactersList.Count; i++)
		{
			TowerDefenseCharacter towerDefenseCharacter = cleanCharactersList[i];
			if (towerDefenseCharacter != this && GodotObject.IsInstanceValid(towerDefenseCharacter) && towerDefenseCharacter is TowerDefensePlant towerDefensePlant && towerDefensePlant.instance.hologram && !towerDefensePlant.isDestroy && towerDefensePlant.camp == base.camp)
			{
				_pendingHolograms.Add(towerDefensePlant);
			}
		}
		if (_pendingHolograms.Count > 0)
		{
			ScheduleExpire(new List<TowerDefenseCharacter>(_pendingHolograms));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(14)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PreSpawn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnExplode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SummonAllHolograms, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TrySummonHologram, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "targetGridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyHologramState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.SyncHologramSpawn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "plant", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.String, "plantName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "targetGridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "hypnoses", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnHypnosisStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RemoveHologram, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ForceRefreshSprites, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "root", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RestorePendingHolograms, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.PreSpawn && args.Count == 0)
		{
			PreSpawn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnExplode && args.Count == 0)
		{
			OnExplode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SummonAllHolograms && args.Count == 0)
		{
			SummonAllHolograms();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TrySummonHologram && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(TrySummonHologram(VariantUtils.ConvertTo<TowerDefenseCellInstance>(in args[0]), VariantUtils.ConvertTo<Vector2I>(in args[1])));
			return true;
		}
		if (method == MethodName.ApplyHologramState && args.Count == 1)
		{
			ApplyHologramState(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SyncHologramSpawn && args.Count == 4)
		{
			SyncHologramSpawn(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<Vector2I>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHypnosisStateChanged && args.Count == 0)
		{
			OnHypnosisStateChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveHologram && args.Count == 1)
		{
			RemoveHologram(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceRefreshSprites && args.Count == 1)
		{
			ForceRefreshSprites(VariantUtils.ConvertTo<Node>(in args[0]));
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
		if (method == MethodName.RestorePendingHolograms && args.Count == 0)
		{
			RestorePendingHolograms();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ApplyHologramState && args.Count == 1)
		{
			ApplyHologramState(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveHologram && args.Count == 1)
		{
			RemoveHologram(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceRefreshSprites && args.Count == 1)
		{
			ForceRefreshSprites(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.PreSpawn)
		{
			return true;
		}
		if (method == MethodName.OnExplode)
		{
			return true;
		}
		if (method == MethodName.SummonAllHolograms)
		{
			return true;
		}
		if (method == MethodName.TrySummonHologram)
		{
			return true;
		}
		if (method == MethodName.ApplyHologramState)
		{
			return true;
		}
		if (method == MethodName.SyncHologramSpawn)
		{
			return true;
		}
		if (method == MethodName.OnHypnosisStateChanged)
		{
			return true;
		}
		if (method == MethodName.RemoveHologram)
		{
			return true;
		}
		if (method == MethodName.ForceRefreshSprites)
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
		if (method == MethodName.RestorePendingHolograms)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._converted)
		{
			_converted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._ownCellSummoned)
		{
			_ownCellSummoned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._converted)
		{
			value = VariantUtils.CreateFrom(in _converted);
			return true;
		}
		if (name == PropertyName._ownCellSummoned)
		{
			value = VariantUtils.CreateFrom(in _ownCellSummoned);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._converted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._ownCellSummoned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._converted, Variant.From(in _converted));
		info.AddProperty(PropertyName._ownCellSummoned, Variant.From(in _ownCellSummoned));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._converted, out var value))
		{
			_converted = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._ownCellSummoned, out var value2))
		{
			_ownCellSummoned = value2.As<bool>();
		}
	}
}
