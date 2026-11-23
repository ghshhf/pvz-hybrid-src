using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Chapter9/MagicBean/Scene/TowerDefensePlantMagicBean.cs")]
public class TowerDefensePlantMagicBean : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Explode = "Explode";

		public static readonly StringName RestoreCardTarget = "RestoreCardTarget";

		public static readonly StringName ApplySavedState = "ApplySavedState";

		public static readonly StringName PickTarget = "PickTarget";

		public static readonly StringName FindPlantPlacementTarget = "FindPlantPlacementTarget";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private const double CardAliveTime = 30.0;

	private ExplodeComponent _explodeComponent;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_explodeComponent = componentManager.GetRuntime<ExplodeComponent>();
			_explodeComponent.OnExplode += Explode;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		ExplodeComponent explodeComponent = _explodeComponent;
		if (explodeComponent != null && !explodeComponent.IsReleased)
		{
			_explodeComponent.OnExplode -= Explode;
		}
	}

	public void Explode()
	{
		if (!TowerDefenseManager.HasGameplayAuthority || !GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		TowerDefenseCharacter towerDefenseCharacter = PickTarget();
		if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
		{
			return;
		}
		if (towerDefenseCharacter is TowerDefenseZombie || (towerDefenseCharacter.instance != null && towerDefenseCharacter.instance.sleep))
		{
			towerDefenseCharacter.WakeUp();
		}
		if (GodotObject.IsInstanceValid(cell))
		{
			foreach (TowerDefenseCharacter character in cell.characterList)
			{
				if (GodotObject.IsInstanceValid(character) && character != towerDefenseCharacter && character != this && character.instance != null && character.instance.sleep)
				{
					character.WakeUp();
				}
			}
		}
		TowerDefensePacketConfig towerDefensePacketConfig = towerDefenseCharacter.packet;
		if (!GodotObject.IsInstanceValid(towerDefensePacketConfig))
		{
			return;
		}
		TowerDefenseCharacterSaveConfigCSharp towerDefenseCharacterSaveConfigCSharp = new TowerDefenseCharacterSaveConfigCSharp();
		towerDefenseCharacterSaveConfigCSharp.SaveCharacter(towerDefenseCharacter);
		Vector2I vector2I = towerDefenseCharacter.gridPos;
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		double num = GetGroundHeight(logicalGlobalPosition.Y);
		towerDefenseCharacter.skipDestroySet = true;
		cell.RemoveCharacter(towerDefenseCharacter);
		towerDefenseCharacter.Destroy(freeInstance: false);
		towerDefenseCharacter.QueueFree();
		TowerDefenseInGamePacketShow towerDefenseInGamePacketShow = SpawnPacket(towerDefensePacketConfig, logicalGlobalPosition + new Vector2(0f, 0f - (float)num), 30.0, isFall: false);
		if (GodotObject.IsInstanceValid(towerDefenseInGamePacketShow))
		{
			towerDefenseInGamePacketShow.SetMeta("MagicBeanRestore", towerDefenseCharacterSaveConfigCSharp);
			TowerDefenseInGamePacketShow cardRef = towerDefenseInGamePacketShow;
			TowerDefensePacketConfig restorePacket = towerDefensePacketConfig;
			Vector2I restoreGrid = vector2I;
			TowerDefenseCharacterSaveConfigCSharp restoreSave = towerDefenseCharacterSaveConfigCSharp;
			GetTree().CreateTimer(30.0).Timeout += delegate
			{
				RestoreCardTarget(cardRef, restorePacket, restoreGrid, restoreSave);
			};
		}
	}

	private static void RestoreCardTarget(TowerDefenseInGamePacketShow card, TowerDefensePacketConfig packetConfig, Vector2I gridPos, TowerDefenseCharacterSaveConfigCSharp saveConfig)
	{
		if (!TowerDefenseManager.HasGameplayAuthority || !GodotObject.IsInstanceValid(card) || card.IsQueuedForDeletion())
		{
			return;
		}
		PacketPickControl packetPickControl = TowerDefenseManager.Instance?.GetPacketPickControl();
		if (GodotObject.IsInstanceValid(packetPickControl) && GodotObject.IsInstanceValid(packetPickControl.packetPick))
		{
			packetPickControl.Release();
		}
		card.QueueFree();
		TowerDefenseCharacter restored = packetConfig.Plant(gridPos, playAudio: false, noLimit: true);
		if (GodotObject.IsInstanceValid(restored) && saveConfig != null)
		{
			restored.PrepareForProgressRestore();
			Callable.From(delegate
			{
				ApplySavedState(restored, saveConfig);
			}).CallDeferred();
		}
	}

	internal static void ApplySavedState(TowerDefenseCharacter restored, TowerDefenseCharacterSaveConfigCSharp saveConfig)
	{
		if (!GodotObject.IsInstanceValid(restored))
		{
			return;
		}
		if (restored.instance == null)
		{
			restored.ProcessMode = ProcessModeEnum.Inherit;
			return;
		}
		foreach (string item in saveConfig.currentArmor)
		{
			if (!string.IsNullOrEmpty(item) && !restored.instance.ArmorHas(item))
			{
				restored.instance.ArmorAdd(item);
			}
		}
		saveConfig.RestoreCharacter(restored);
		restored.ImportVariantSave(saveConfig.variantSave);
		foreach (TowerDefenseArmorInstance armor in restored.instance.armorList)
		{
			if (!armor.isRemove)
			{
				armor.SetDamageStage(armor.stageIndex);
			}
		}
		restored.ProcessMode = ProcessModeEnum.Inherit;
		IStateMachineController stateMachine = restored.StateMachine;
		if (stateMachine != null && stateMachine.IsInitialized)
		{
			restored.SetMainStateMachineDispatchEnabled(enabled: true);
		}
		Dictionary data = saveConfig.instanceSave;
		if (data == null)
		{
			return;
		}
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(restored) && restored.instance != null)
			{
				if (data.ContainsKey("hitpointsSave"))
				{
					restored.instance.hitpointsSave = data["hitpointsSave"].AsDouble();
				}
				if (data.ContainsKey("hitpoints"))
				{
					restored.instance.hitpoints = data["hitpoints"].AsDouble();
				}
			}
		}).CallDeferred();
	}

	private TowerDefenseCharacter PickTarget()
	{
		if (GodotObject.IsInstanceValid(targetZombie) && !targetZombie.isDestroy && !targetZombie.die && !targetZombie.nearDie && targetZombie.instance != null && targetZombie.instance.zombiePhysique != TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS)
		{
			return targetZombie;
		}
		return FindPlantPlacementTarget(cell, this);
	}

	internal static TowerDefenseCharacter FindPlantPlacementTarget(TowerDefenseCellInstance targetCell, TowerDefenseCharacter excluded = null)
	{
		if (!GodotObject.IsInstanceValid(targetCell))
		{
			return null;
		}
		TowerDefenseCharacter result = null;
		int num = 2147483647;
		foreach (TowerDefenseCharacter character in targetCell.GetCharacterList())
		{
			if (GodotObject.IsInstanceValid(character) && character != excluded && character is TowerDefensePlant && !character.isDestroy && !character.die && !character.nearDie && GodotObject.IsInstanceValid(character.instance) && character.instance.canBeCollection && !character.instance.invincible && !character.instance.hologram && GodotObject.IsInstanceValid(character.config) && !character.config.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.AIR))
			{
				TowerDefenseCharacterConfig towerDefenseCharacterConfig = character.config;
				int num2;
				if (towerDefenseCharacterConfig.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.SURROUND))
				{
					num2 = 1;
				}
				else
				{
					TowerDefenseEnum.PLANTGRIDTYPE plantGridOverrideType = towerDefenseCharacterConfig.plantGridOverrideType;
					bool flag = (uint)(plantGridOverrideType - 5) <= 1u;
					num2 = (flag ? 2 : 0);
				}
				int num3 = num2;
				if (num3 < num)
				{
					result = character;
					num = num3;
				}
			}
		}
		return result;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(7)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Explode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RestoreCardTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
				new PropertyInfo(Variant.Type.Object, "packetConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "gridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "saveConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null),
			new MethodInfo(MethodName.ApplySavedState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "restored", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Object, "saveConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null),
			new MethodInfo(MethodName.PickTarget, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FindPlantPlacementTarget, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "targetCell", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Object, "excluded", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
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
		if (method == MethodName.Explode && args.Count == 0)
		{
			Explode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestoreCardTarget && args.Count == 4)
		{
			RestoreCardTarget(VariantUtils.ConvertTo<TowerDefenseInGamePacketShow>(in args[0]), VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[1]), VariantUtils.ConvertTo<Vector2I>(in args[2]), VariantUtils.ConvertTo<TowerDefenseCharacterSaveConfigCSharp>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplySavedState && args.Count == 2)
		{
			ApplySavedState(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacterSaveConfigCSharp>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PickTarget && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(PickTarget());
			return true;
		}
		if (method == MethodName.FindPlantPlacementTarget && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(FindPlantPlacementTarget(VariantUtils.ConvertTo<TowerDefenseCellInstance>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.RestoreCardTarget && args.Count == 4)
		{
			RestoreCardTarget(VariantUtils.ConvertTo<TowerDefenseInGamePacketShow>(in args[0]), VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[1]), VariantUtils.ConvertTo<Vector2I>(in args[2]), VariantUtils.ConvertTo<TowerDefenseCharacterSaveConfigCSharp>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplySavedState && args.Count == 2)
		{
			ApplySavedState(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacterSaveConfigCSharp>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FindPlantPlacementTarget && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(FindPlantPlacementTarget(VariantUtils.ConvertTo<TowerDefenseCellInstance>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1])));
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
		if (method == MethodName.Explode)
		{
			return true;
		}
		if (method == MethodName.RestoreCardTarget)
		{
			return true;
		}
		if (method == MethodName.ApplySavedState)
		{
			return true;
		}
		if (method == MethodName.PickTarget)
		{
			return true;
		}
		if (method == MethodName.FindPlantPlacementTarget)
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
