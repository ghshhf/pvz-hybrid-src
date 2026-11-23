using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Gold/KingKelp/Scene/TowerDefensePlantKingKelp.cs")]
public class TowerDefensePlantKingKelp : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public static readonly StringName Timeout = "Timeout";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName Summon = "Summon";

		public new static readonly StringName DestroySet = "DestroySet";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName grabScene = "grabScene";

		public static readonly StringName _shooting = "_shooting";

		public static readonly StringName _summoned = "_summoned";

		public static readonly StringName _over = "_over";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public PackedScene grabScene;

	private CharacterTimerComponent _timerComponent;

	private bool _shooting;

	private bool _summoned;

	private bool _over;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			_timerComponent.OnTimeout += Timeout;
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (IsNodeReady())
		{
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += Timeout;
			}
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= Timeout;
		}
		base._ExitTree();
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		if (TowerDefenseManager.Instance.IsGameRunning() && inGame && CanRunAbility() && !_timerComponent.IsRunning("Summon"))
		{
			_timerComponent.Run("Summon");
		}
	}

	public void Timeout(string timerName)
	{
		if (timerName == "Summon" && (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost))
		{
			_shooting = true;
			_summoned = false;
			base.sprite.SetAnimation("Shooting", loop: false, 0.2);
			_timerComponent.Run("Summon");
		}
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (command == "action")
		{
			Summon();
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Shooting")
		{
			_shooting = false;
			base.sprite.SetAnimation("Idle", loop: true, 0.2);
		}
	}

	private void Summon()
	{
		if (!_shooting || _summoned)
		{
			return;
		}
		_summoned = true;
		if (Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return;
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		int x = ((!(transformPoint.Scale.X < 0f)) ? 1 : (-1));
		for (int i = -1; i <= 1; i++)
		{
			Vector2I vector2I = base.gridPos + new Vector2I(x, i);
			if (!towerDefenseManager.CheckMapGridPosIn(vector2I))
			{
				continue;
			}
			Vector2 mapCellPlantPos = TowerDefenseManager.GetMapCellPlantPos(vector2I);
			TowerDefenseCharacter towerDefenseCharacter = (base.HasEconomyOwner ? TowerDefenseCharacter.CreateCharacter(base.EconomyOwnerAccountId, "ZombieSnorkleTanglekelp", mapCellPlantPos, vector2I, base.groundHeight) : TowerDefenseCharacter.CreateCharacter("ZombieSnorkleTanglekelp", mapCellPlantPos, vector2I, base.groundHeight));
			if (GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				towerDefenseCharacter.CallDeferred("Walk");
				if (!instance.hypnoses)
				{
					towerDefenseCharacter.Hypnoses();
				}
				if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
				{
					TowerDefenseControlNew currentControl = TowerDefenseManager.CurrentControl;
					int nextSyncId = currentControl.GetNextSyncId();
					currentControl.RegisterSyncCharacter(nextSyncId, towerDefenseCharacter);
					MultiPlayerManager.Instance.SendSpawnCharacterAt("ZombieSnorkleTanglekelp", vector2I.X, vector2I.Y, nextSyncId, instance.hitpointScale, transformPoint.Scale.X, !instance.hypnoses, 0.0, useCreate: true, mapCellPlantPos.X, mapCellPlantPos.Y, walkAfterSpawn: true, base.groundHeight);
				}
			}
		}
	}

	public override void DestroySet()
	{
		base.DestroySet();
		if (_over)
		{
			return;
		}
		_over = true;
		if (Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return;
		}
		foreach (TowerDefenseCharacter item in new List<TowerDefenseCharacter>(TowerDefenseManager.Instance.characterRegistry.GetCharactersForGridWindowList(base.gridPos.X, base.gridPos.Y, 1, 1, includeExtendedPlants: true)))
		{
			if (GodotObject.IsInstanceValid(item) && !item.isDestroy && !item.die && CheckDifferentCamp(item.camp) && item.inWater && (!(item is TowerDefenseZombie) || !GodotObject.IsInstanceValid(item.instance) || item.instance.zombiePhysique != TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS))
			{
				if (!item.config.canDragIntoWater || item.IsHardControlImmune)
				{
					item.Hurt(3000.0);
				}
				else if (item.config.dragHurt != -1.0)
				{
					double dragHurt = item.config.dragHurt;
					item.Hurt(100000.0, playSplatAudio: true, default(Vector2), createDamagePart: true, dragHurt);
				}
				else if (!GodotObject.IsInstanceValid(item.instance) || (!item.instance.TryBlockShieldEffect() && !item.instance.TryBlockLethalDamageWithArmor()))
				{
					TowerDefenseEffectSpriteOnce towerDefenseEffectSpriteOnce = TowerDefenseManager.CreateEffectSpriteOnce(grabScene, item.gridPos, "Grab");
					towerDefenseEffectSpriteOnce.GlobalPosition = item.GetLogicalGlobalPosition();
					TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseEffectSpriteOnce, forceReadableName: false, InternalMode.Disabled);
					item.CreateSplash();
					item.die = true;
					item.Destroy();
				}
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["shooting"] = _shooting,
			["summoned"] = _summoned,
			["over"] = _over
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_shooting = data.GetValueOrDefault("shooting", false).AsBool();
		_summoned = data.GetValueOrDefault("summoned", false).AsBool();
		_over = data.GetValueOrDefault("over", false).AsBool();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportVariantSave();
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		ImportVariantSave(data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(13)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Timeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Summon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Timeout && args.Count == 1)
		{
			Timeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Summon && args.Count == 0)
		{
			Summon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
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
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.Timeout)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.Summon)
		{
			return true;
		}
		if (method == MethodName.DestroySet)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.grabScene)
		{
			grabScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._shooting)
		{
			_shooting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._summoned)
		{
			_summoned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._over)
		{
			_over = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.grabScene)
		{
			value = VariantUtils.CreateFrom(in grabScene);
			return true;
		}
		if (name == PropertyName._shooting)
		{
			value = VariantUtils.CreateFrom(in _shooting);
			return true;
		}
		if (name == PropertyName._summoned)
		{
			value = VariantUtils.CreateFrom(in _summoned);
			return true;
		}
		if (name == PropertyName._over)
		{
			value = VariantUtils.CreateFrom(in _over);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.grabScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._shooting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._summoned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._over, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.grabScene, Variant.From(in grabScene));
		info.AddProperty(PropertyName._shooting, Variant.From(in _shooting));
		info.AddProperty(PropertyName._summoned, Variant.From(in _summoned));
		info.AddProperty(PropertyName._over, Variant.From(in _over));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.grabScene, out var value))
		{
			grabScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._shooting, out var value2))
		{
			_shooting = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._summoned, out var value3))
		{
			_summoned = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._over, out var value4))
		{
			_over = value4.As<bool>();
		}
	}
}
