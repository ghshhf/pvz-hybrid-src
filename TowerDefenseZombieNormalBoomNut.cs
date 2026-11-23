using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter1/Normal/Scene/BoomNut/TowerDefenseZombieNormalBoomNut.cs")]
public class TowerDefenseZombieNormalBoomNut : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName TryExplodeOnContact = "TryExplodeOnContact";

		public static readonly StringName ExplodeNow = "ExplodeNow";

		public new static readonly StringName Purify = "Purify";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName _checkFrame = "_checkFrame";

		public static readonly StringName _exploded = "_exploded";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private ExplodeComponent _explodeComponent;

	private int _checkFrame;

	private bool _exploded;

	private readonly List<TowerDefenseCharacter> _overlapBuffer = new List<TowerDefenseCharacter>();

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_explodeComponent = componentManager.GetRuntime<ExplodeComponent>();
		}
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (!Engine.IsEditorHint() && GodotObject.IsInstanceValid(TowerDefenseManager.Instance) && TowerDefenseManager.Instance.IsGameRunning() && inGame)
		{
			TryExplodeOnContact();
		}
	}

	private void TryExplodeOnContact()
	{
		if (_exploded || die || isDestroy || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		ExplodeComponent explodeComponent = _explodeComponent;
		if (explodeComponent == null || explodeComponent.IsReleased || ++_checkFrame < 3)
		{
			return;
		}
		_checkFrame = 0;
		if (!TryGetActiveWorldHitRect(out var rect))
		{
			return;
		}
		TowerDefenseBattleCharacterRegistry characterRegistry = TowerDefenseManager.Instance.characterRegistry;
		if (characterRegistry == null)
		{
			return;
		}
		characterRegistry.FillCharactersIntersectingRectDirectList(rect, _overlapBuffer);
		for (int i = 0; i < _overlapBuffer.Count; i++)
		{
			TowerDefenseCharacter towerDefenseCharacter = _overlapBuffer[i];
			if (GodotObject.IsInstanceValid(towerDefenseCharacter) && towerDefenseCharacter != this && towerDefenseCharacter.camp != base.camp && !towerDefenseCharacter.die && !towerDefenseCharacter.nearDie && !towerDefenseCharacter.isDestroy && !(towerDefenseCharacter is TowerDefenseGravestone) && !(towerDefenseCharacter is TowerDefenseItem) && GodotObject.IsInstanceValid(towerDefenseCharacter.instance) && !towerDefenseCharacter.instance.invincible && CanCollision(towerDefenseCharacter.instance.maskFlags))
			{
				ExplodeNow();
				break;
			}
		}
	}

	private void ExplodeNow()
	{
		_exploded = true;
		_explodeComponent.Explode();
		Destroy();
	}

	public override void Purify()
	{
		if (!GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		if (Global.IsMultiplayerMode && !MultiPlayerManager.Instance.isHost)
		{
			Destroy();
			return;
		}
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("PlantWallnutShooter");
		if (cell.CanPacketPlant(packetConfig))
		{
			TowerDefenseCharacter towerDefenseCharacter = packetConfig.Plant(base.gridPos);
			towerDefenseCharacter.WakeUp();
			if (instance.hypnoses)
			{
				towerDefenseCharacter.Hypnoses();
			}
			if (Global.IsMultiplayerMode && MultiPlayerManager.Instance.isHost)
			{
				TowerDefenseControlNew currentControl = TowerDefenseManager.Instance.currentControl;
				if (GodotObject.IsInstanceValid(currentControl))
				{
					int nextSyncId = currentControl.GetNextSyncId();
					currentControl.RegisterSyncCharacter(nextSyncId, towerDefenseCharacter);
					MultiPlayerManager.Instance.SendSpawnCharacterAt("PlantWallnutShooter", base.gridPos.X, base.gridPos.Y, nextSyncId);
				}
			}
		}
		Destroy();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(5)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TryExplodeOnContact, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExplodeNow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Purify, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.TryExplodeOnContact && args.Count == 0)
		{
			TryExplodeOnContact();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExplodeNow && args.Count == 0)
		{
			ExplodeNow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Purify && args.Count == 0)
		{
			Purify();
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
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.TryExplodeOnContact)
		{
			return true;
		}
		if (method == MethodName.ExplodeNow)
		{
			return true;
		}
		if (method == MethodName.Purify)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._checkFrame)
		{
			_checkFrame = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._exploded)
		{
			_exploded = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._checkFrame)
		{
			value = VariantUtils.CreateFrom(in _checkFrame);
			return true;
		}
		if (name == PropertyName._exploded)
		{
			value = VariantUtils.CreateFrom(in _exploded);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, PropertyName._checkFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._exploded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._checkFrame, Variant.From(in _checkFrame));
		info.AddProperty(PropertyName._exploded, Variant.From(in _exploded));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._checkFrame, out var value))
		{
			_checkFrame = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._exploded, out var value2))
		{
			_exploded = value2.As<bool>();
		}
	}
}
