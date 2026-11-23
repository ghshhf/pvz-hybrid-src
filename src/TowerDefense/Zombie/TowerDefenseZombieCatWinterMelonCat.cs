using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter3/Cat/Scene/WinterMelonCat/TowerDefenseZombieCatWinterMelonCat.cs")]
public class TowerDefenseZombieCatWinterMelonCat : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName DieProcessing = "DieProcessing";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public static readonly StringName FireIceMelon = "FireIceMelon";

		public static readonly StringName PlayHeadAttackAnime = "PlayHeadAttackAnime";

		public static readonly StringName HeadAnimeCompleted = "HeadAnimeCompleted";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName Purify = "Purify";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName fireInterval = "fireInterval";

		public static readonly StringName fireNum = "fireNum";

		public static readonly StringName projectileName = "projectileName";

		public static readonly StringName _zombieSprite = "_zombieSprite";

		public static readonly StringName _currentFireNum = "_currentFireNum";

		public static readonly StringName _fireInterval = "_fireInterval";

		public static readonly StringName _fireNum = "_fireNum";

		public static readonly StringName _projectileName = "_projectileName";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const int MelonCollisionFlags = 9;

	private const int MelonProjectileIndex = 1;

	private const int MelonMarkerId = 1;

	private const string HeadFireAnimeClip = "Fire";

	private const string HeadIdleAnimeClip = "Idle";

	private const float HeadFireAnimeTimeScale = 2f;

	private const float HeadIdleAnimeTimeScale = 1f;

	private FireComponent _fireComponent;

	private ZombieCatWinterMelonCatSprite _zombieSprite;

	private int _currentFireNum;

	private double _fireInterval = 1.5;

	private int _fireNum = 2;

	private string _projectileName = "Spike";

	[Export(PropertyHint.None, "")]
	public double fireInterval
	{
		get
		{
			return _fireInterval;
		}
		set
		{
			_fireInterval = value;
			if (IsNodeReady())
			{
				FireComponent fireComponent = _fireComponent;
				if (fireComponent != null && !fireComponent.IsReleased)
				{
					_fireComponent.fireInterval = (float)value;
				}
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public int fireNum
	{
		get
		{
			return _fireNum;
		}
		set
		{
			_fireNum = value;
			if (IsNodeReady())
			{
				FireComponent fireComponent = _fireComponent;
				if (fireComponent != null && !fireComponent.IsReleased)
				{
					_fireComponent.fireNum = value;
				}
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public string projectileName
	{
		get
		{
			return _projectileName;
		}
		set
		{
			_projectileName = value;
			if (IsNodeReady())
			{
				FireComponent fireComponent = _fireComponent;
				if (fireComponent != null && !fireComponent.IsReleased && _fireComponent.fireCheckList.Count > 0)
				{
					((FireComponentProjectileSingle)_fireComponent.fireCheckList[0].projectile).projectileName = value;
				}
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_fireComponent = componentManager.GetRuntime<FireComponent>("character.fire");
			_zombieSprite = base.sprite as ZombieCatWinterMelonCatSprite;
			AdobeAnimateSpriteBase adobeAnimateSpriteBase = _zombieSprite?.head;
			if (adobeAnimateSpriteBase != null && GodotObject.IsInstanceValid(adobeAnimateSpriteBase))
			{
				adobeAnimateSpriteBase.OnAnimeCompleted += HeadAnimeCompleted;
			}
			ConfigureWaterLineVisualLayers("Zombie_whitewater");
		}
	}

	public override void _ExitTree()
	{
		AdobeAnimateSpriteBase adobeAnimateSpriteBase = _zombieSprite?.head;
		if (adobeAnimateSpriteBase != null && GodotObject.IsInstanceValid(adobeAnimateSpriteBase))
		{
			adobeAnimateSpriteBase.OnAnimeCompleted -= HeadAnimeCompleted;
		}
		base._ExitTree();
	}

	public override void WalkProcessing(double delta)
	{
		if ((double)GetGlobalPositionForPhysicsFrame(TowerDefenseProcessModeDispatch.CurrentPhysicsFrame).X > groundRight)
		{
			base.sprite.timeScale = timeScale * base.walkSpeedScale * 2.0;
		}
		else
		{
			base.sprite.timeScale = timeScale * base.walkSpeedScale;
		}
		if (!base.IsRemoteNetworkReplica && !base.sprite.pause && attackComponent.CanAttack() && base.sprite.timeScale > 0.0)
		{
			ChangeLine();
		}
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		base.sprite.timeScale = timeScale * 3.0;
	}

	public override void DieProcessing(double delta)
	{
		base.DieProcessing(delta);
		base.sprite.timeScale = timeScale * 2.0;
	}

	public override async void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (command == "fire" && !nearDie && !die && (double)GetLogicalGlobalPosition().X <= groundRight)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			FireIceMelon();
		}
	}

	private void FireIceMelon()
	{
		FireComponent fireComponent = _fireComponent;
		if (fireComponent == null || fireComponent.IsReleased || _fireComponent.fireCheckList.Count <= 1)
		{
			_currentFireNum = 0;
			return;
		}
		TowerDefenseProjectileCreateData projetile = _fireComponent.fireCheckList[1].projectile.GetProjetile();
		if (!((_currentFireNum == 0) ? _fireComponent.CanFireByData(projetile, 9) : _fireComponent.CanFireCheckOnceByData(projetile, 9)))
		{
			_currentFireNum = 0;
			return;
		}
		AudioManager.Instance.AudioPlay("ProjectileThrow");
		PlayHeadAttackAnime();
		BulletFieldSpawnOverrides overrides = new BulletFieldSpawnOverrides
		{
			gridYOverride = base.gridPos.Y,
			initialRotationOverride = (float)Math.PI * base.Scale.X
		};
		_fireComponent.CreateProjectileByData(1, new Vector2(-300f, 0f), projetile, 9, base.camp, Vector2.Zero, overrides);
		if (_currentFireNum == 0)
		{
			_fireComponent.Refresh();
		}
		_currentFireNum++;
		if (_currentFireNum >= fireNum)
		{
			_currentFireNum = 0;
		}
	}

	private void PlayHeadAttackAnime()
	{
		AdobeAnimateSpriteBase adobeAnimateSpriteBase = _zombieSprite?.head;
		if (adobeAnimateSpriteBase != null && GodotObject.IsInstanceValid(adobeAnimateSpriteBase) && adobeAnimateSpriteBase.HasClip("Fire"))
		{
			adobeAnimateSpriteBase.timeScale = 2.0;
			adobeAnimateSpriteBase.SetAnimation("Fire", loop: false, 0.20000000298023224);
		}
	}

	private void HeadAnimeCompleted(string clip)
	{
		if (!(clip != "Fire"))
		{
			AdobeAnimateSpriteBase adobeAnimateSpriteBase = _zombieSprite?.head;
			if (adobeAnimateSpriteBase != null && GodotObject.IsInstanceValid(adobeAnimateSpriteBase) && adobeAnimateSpriteBase.HasClip("Idle"))
			{
				adobeAnimateSpriteBase.timeScale = 1.0;
				adobeAnimateSpriteBase.SetAnimation("Idle", loop: true, 0.20000000298023224);
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["currentFireNum"] = _currentFireNum;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_currentFireNum = data.GetValueOrDefault("currentFireNum", 0).AsInt32();
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
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("PlantWinterMelonCat");
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
					MultiPlayerManager.Instance.SendSpawnCharacterAt("PlantWinterMelonCat", base.gridPos.X, base.gridPos.Y, nextSyncId);
				}
			}
		}
		Destroy();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(12)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DieProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.FireIceMelon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlayHeadAttackAnime, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HeadAnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.WalkProcessing && args.Count == 1)
		{
			WalkProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttackProcessing && args.Count == 1)
		{
			AttackProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DieProcessing && args.Count == 1)
		{
			DieProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FireIceMelon && args.Count == 0)
		{
			FireIceMelon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayHeadAttackAnime && args.Count == 0)
		{
			PlayHeadAttackAnime();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HeadAnimeCompleted && args.Count == 1)
		{
			HeadAnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName.WalkProcessing)
		{
			return true;
		}
		if (method == MethodName.AttackProcessing)
		{
			return true;
		}
		if (method == MethodName.DieProcessing)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.FireIceMelon)
		{
			return true;
		}
		if (method == MethodName.PlayHeadAttackAnime)
		{
			return true;
		}
		if (method == MethodName.HeadAnimeCompleted)
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
		if (method == MethodName.Purify)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.fireInterval)
		{
			fireInterval = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.fireNum)
		{
			fireNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.projectileName)
		{
			projectileName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._zombieSprite)
		{
			_zombieSprite = VariantUtils.ConvertTo<ZombieCatWinterMelonCatSprite>(in value);
			return true;
		}
		if (name == PropertyName._currentFireNum)
		{
			_currentFireNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._fireInterval)
		{
			_fireInterval = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._fireNum)
		{
			_fireNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._projectileName)
		{
			_projectileName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.fireInterval)
		{
			value = VariantUtils.CreateFrom<double>(fireInterval);
			return true;
		}
		if (name == PropertyName.fireNum)
		{
			value = VariantUtils.CreateFrom<int>(fireNum);
			return true;
		}
		if (name == PropertyName.projectileName)
		{
			value = VariantUtils.CreateFrom<string>(projectileName);
			return true;
		}
		if (name == PropertyName._zombieSprite)
		{
			value = VariantUtils.CreateFrom(in _zombieSprite);
			return true;
		}
		if (name == PropertyName._currentFireNum)
		{
			value = VariantUtils.CreateFrom(in _currentFireNum);
			return true;
		}
		if (name == PropertyName._fireInterval)
		{
			value = VariantUtils.CreateFrom(in _fireInterval);
			return true;
		}
		if (name == PropertyName._fireNum)
		{
			value = VariantUtils.CreateFrom(in _fireNum);
			return true;
		}
		if (name == PropertyName._projectileName)
		{
			value = VariantUtils.CreateFrom(in _projectileName);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._zombieSprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._currentFireNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.fireInterval, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName._fireInterval, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.fireNum, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName._fireNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.projectileName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName._projectileName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.fireInterval, Variant.From<double>(fireInterval));
		info.AddProperty(PropertyName.fireNum, Variant.From<int>(fireNum));
		info.AddProperty(PropertyName.projectileName, Variant.From<string>(projectileName));
		info.AddProperty(PropertyName._zombieSprite, Variant.From(in _zombieSprite));
		info.AddProperty(PropertyName._currentFireNum, Variant.From(in _currentFireNum));
		info.AddProperty(PropertyName._fireInterval, Variant.From(in _fireInterval));
		info.AddProperty(PropertyName._fireNum, Variant.From(in _fireNum));
		info.AddProperty(PropertyName._projectileName, Variant.From(in _projectileName));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.fireInterval, out var value))
		{
			fireInterval = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName.fireNum, out var value2))
		{
			fireNum = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName.projectileName, out var value3))
		{
			projectileName = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._zombieSprite, out var value4))
		{
			_zombieSprite = value4.As<ZombieCatWinterMelonCatSprite>();
		}
		if (info.TryGetProperty(PropertyName._currentFireNum, out var value5))
		{
			_currentFireNum = value5.As<int>();
		}
		if (info.TryGetProperty(PropertyName._fireInterval, out var value6))
		{
			_fireInterval = value6.As<double>();
		}
		if (info.TryGetProperty(PropertyName._fireNum, out var value7))
		{
			_fireNum = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._projectileName, out var value8))
		{
			_projectileName = value8.As<string>();
		}
	}
}
