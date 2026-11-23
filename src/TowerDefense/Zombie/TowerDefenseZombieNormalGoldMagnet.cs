using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter1/Normal/Scene/GoldMagnet/TowerDefenseZombieNormalGoldMagnet.cs")]
public class TowerDefenseZombieNormalGoldMagnet : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName CoinGet = "CoinGet";

		public new static readonly StringName Purify = "Purify";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private MagnetCoinComponent _magnet;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_magnet = componentManager.GetRuntime<MagnetCoinComponent>();
			MagnetCoinComponent magnet = _magnet;
			if (magnet != null && !magnet.IsReleased)
			{
				_magnet.OnCoinGet += CoinGet;
			}
			AddToGroup("GoldMagnet");
		}
	}

	public override void _ExitTree()
	{
		MagnetCoinComponent magnet = _magnet;
		if (magnet != null && !magnet.IsReleased)
		{
			_magnet.OnCoinGet -= CoinGet;
		}
		base._ExitTree();
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (!Engine.IsEditorHint())
		{
			MagnetCoinComponent magnet = _magnet;
			if (magnet != null && !magnet.IsReleased && TowerDefenseManager.HasGameplayAuthority && GodotObject.IsInstanceValid(TowerDefenseManager.Instance) && TowerDefenseManager.Instance.IsGameRunning() && inGame && _magnet.CanCoinDraw())
			{
				_magnet.CoinDraw();
			}
		}
	}

	private void CoinGet(TowerDefenseCoinBase coin)
	{
		if (TowerDefenseManager.HasGameplayAuthority && GodotObject.IsInstanceValid(coin))
		{
			int num = coin.num;
			string text = num switch
			{
				50 => "CoinGold", 
				1000 => "CoinDiamond", 
				_ => "CoinSilver", 
			};
			double baseDamage = num switch
			{
				50 => 500.0, 
				1000 => 1000.0, 
				_ => 100.0, 
			};
			TowerDefenseProjectileCreateData towerDefenseProjectileCreateData = new TowerDefenseProjectileCreateData(text);
			towerDefenseProjectileCreateData.baseDamage = baseDamage;
			towerDefenseProjectileCreateData.fireMethodFlags = 32;
			BulletFieldSpawnOverrides overrides = new BulletFieldSpawnOverrides
			{
				gridYOverride = base.gridPos.Y,
				flipXOverride = (base.Scale.X < 0f)
			};
			FireComponent.CreateProjectilePositionByData(this, null, GetGroundHeight(GetLogicalGlobalPosition().Y), GetLogicalGlobalPosition(), new Vector2(-300f, 0f), towerDefenseProjectileCreateData, -1, base.camp, Vector2.Zero, overrides);
		}
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
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("PlantGoldMagnet");
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
					MultiPlayerManager.Instance.SendSpawnCharacterAt("PlantGoldMagnet", base.gridPos.X, base.gridPos.Y, nextSyncId);
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
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CoinGet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "coin", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
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
		if (method == MethodName.CoinGet && args.Count == 1)
		{
			CoinGet(VariantUtils.ConvertTo<TowerDefenseCoinBase>(in args[0]));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.CoinGet)
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
