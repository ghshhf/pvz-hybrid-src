using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter10/DivineMage/Scene/TowerDefenseZombieDivineMage.cs")]
public class TowerDefenseZombieDivineMage : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public static readonly StringName HasCrystalBall = "HasCrystalBall";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName CanCast = "CanCast";

		public static readonly StringName ProcessSpell = "ProcessSpell";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName Walk = "Walk";

		public static readonly StringName CastEntered = "CastEntered";

		public static readonly StringName CastProcessing = "CastProcessing";

		public static readonly StringName CastExited = "CastExited";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public static readonly StringName CastProphecy = "CastProphecy";

		public static readonly StringName ApplyReceivedProphecy = "ApplyReceivedProphecy";

		public static readonly StringName IsCharmed = "IsCharmed";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName OnAnimeStarted = "OnAnimeStarted";

		public static readonly StringName UpdateArmLayers = "UpdateArmLayers";

		public static readonly StringName ShowShield = "ShowShield";

		public new static readonly StringName ArmorHitpointsEmpty = "ArmorHitpointsEmpty";

		public new static readonly StringName ArmorDamagePointReach = "ArmorDamagePointReach";

		public new static readonly StringName DamagePointReach = "DamagePointReach";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName shieldTexture = "shieldTexture";

		public static readonly StringName _enteredField = "_enteredField";

		public static readonly StringName _spellPending = "_spellPending";

		public static readonly StringName _preserveCastOnRestore = "_preserveCastOnRestore";

		public static readonly StringName _armLost = "_armLost";

		public static readonly StringName _prophecySerial = "_prophecySerial";

		public static readonly StringName _prophecySeed = "_prophecySeed";

		public static readonly StringName _receivedProphecySerial = "_receivedProphecySerial";

		public static readonly StringName _prophecyRemaining = "_prophecyRemaining";

		public static readonly StringName _prophecyPosition = "_prophecyPosition";

		public static readonly StringName _activeShield = "_activeShield";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const double CastInterval = 12.0;

	private const string CrystalBall = "DivineMageBall";

	private static readonly StringName[] RaisedArms = new StringName[3] { "Zombie_innerarm_screendoor", "Zombie_innerarm_screendoor_hand", "Zombie_outerarm_screendoor" };

	private static readonly StringName[] LoweredArms = new StringName[4] { "anim_innerarm1", "anim_innerarm2", "anim_innerarm3", "Zombie_outerarm_upper" };

	[Export(PropertyHint.None, "")]
	public Texture2D shieldTexture;

	private StateHandle _castState;

	private CharacterTimerComponent _timerComponent;

	private bool _enteredField;

	private bool _spellPending;

	private bool _preserveCastOnRestore;

	private bool _armLost;

	private int _prophecySerial;

	private int _prophecySeed;

	private int _receivedProphecySerial;

	private double _prophecyRemaining;

	private Vector2 _prophecyPosition;

	private Sprite2D _activeShield;

	public bool HasCrystalBall()
	{
		TowerDefenseArmorInstance armorFromName = GetArmorFromName("DivineMageBall");
		if (armorFromName != null && !armorFromName.isRemove)
		{
			return armorFromName.hitPoints > 0.0;
		}
		return false;
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			_castState = base.StateMachine.GetStateById("zombie.mage.cast");
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += OnTimerTimeout;
			}
			_castState.Entered += CastEntered;
			_castState.PhysicsProcessing += CastProcessing;
			_castState.Exited += CastExited;
			base.OnArmorHurt += ShowShield;
			UpdateArmLayers();
			ApplyReceivedProphecy();
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= OnTimerTimeout;
		}
		if (_castState != null)
		{
			_castState.Entered -= CastEntered;
			_castState.PhysicsProcessing -= CastProcessing;
			_castState.Exited -= CastExited;
			_castState = null;
		}
		base.OnArmorHurt -= ShowShield;
		base._ExitTree();
	}

	private bool CanCast()
	{
		if (CanRunAbility() && TowerDefenseManager.HasGameplayAuthority && inGame && !die && !nearDie && !isDestroy && !isRise && HasCrystalBall() && GodotObject.IsInstanceValid(base.sprite) && !base.sprite.pause)
		{
			return timeScale > 0.0;
		}
		return false;
	}

	private void ProcessSpell(double delta)
	{
		if (CanRunAbility())
		{
			_prophecyRemaining = Math.Max(0.0, _prophecyRemaining - Math.Max(0.0, delta) * timeScale);
		}
		ApplyReceivedProphecy();
		if (!CanCast())
		{
			return;
		}
		StateHandle castState = _castState;
		if (castState == null || !castState.IsValid)
		{
			return;
		}
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_enteredField |= base.IsInsideComponentBattlefield;
			if (_enteredField && !_timerComponent.IsRunning("Cast"))
			{
				_timerComponent.Run("Cast", 12.0);
			}
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (!(timerName != "Cast"))
		{
			StateHandle castState = _castState;
			if ((castState == null || !castState.IsActive) && CanCast())
			{
				SendStateEvent("ToMageCast");
			}
		}
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		ProcessSpell(delta);
	}

	public override void WalkProcessing(double delta)
	{
		base.WalkProcessing(delta);
		ProcessSpell(delta);
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		ProcessSpell(delta);
	}

	public override void Walk()
	{
		StateHandle castState = _castState;
		if (castState == null || !castState.IsActive || die || nearDie || !HasCrystalBall())
		{
			base.Walk();
		}
	}

	private void CastEntered()
	{
		groundMoveComponent?.SetAlive(value: false);
		startAttack = false;
		if (_preserveCastOnRestore)
		{
			_preserveCastOnRestore = false;
			return;
		}
		_timerComponent?.Run("Cast", 12.0);
		_spellPending = true;
		base.sprite.SetAnimation("Shooting", loop: false);
	}

	private void CastProcessing(double delta)
	{
		base.sprite.timeScale = timeScale;
		ProcessSpell(delta);
		if (TowerDefenseManager.HasGameplayAuthority && !HasCrystalBall())
		{
			base.Walk();
		}
	}

	private void CastExited()
	{
		_spellPending = false;
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (!(command != "action"))
		{
			CastProphecy();
		}
	}

	private void CastProphecy()
	{
		if (_spellPending)
		{
			StateHandle castState = _castState;
			if (castState != null && castState.IsActive && CanCast() && !(base.sprite.clip != "Shooting"))
			{
				_spellPending = false;
				_prophecySerial++;
				_prophecySeed = GD.RandRange(0, 2147483647);
				_prophecyRemaining = 10.0;
				_prophecyPosition = GetLogicalGlobalPosition();
				ApplyReceivedProphecy();
			}
		}
	}

	private void ApplyReceivedProphecy()
	{
		if (_receivedProphecySerial == _prophecySerial || _prophecyRemaining <= 0.0 || !GodotObject.IsInstanceValid(TowerDefenseManager.Instance?.GetSeedBank()))
		{
			return;
		}
		List<TowerDefenseInGamePacketShow> list = new List<TowerDefenseInGamePacketShow>();
		foreach (TowerDefenseInGamePacketShow seedBank in TowerDefenseManager.Instance.GetSeedBankList())
		{
			if (GodotObject.IsInstanceValid(seedBank) && seedBank.config != null && !seedBank.onlyDraw && !seedBank.@lock && !seedBank.HasMeta("is_upgrade_packet"))
			{
				list.Add(seedBank);
			}
		}
		_receivedProphecySerial = _prophecySerial;
		if (list.Count > 0)
		{
			list[_prophecySeed % list.Count].ApplyProphecy(_prophecyRemaining, _prophecyPosition, IsCharmed());
		}
	}

	private bool IsCharmed()
	{
		BuffComponent buffComponent = buff;
		if (buffComponent != null && !buffComponent.IsReleased)
		{
			return buffComponent.BuffHas("Hypnoses");
		}
		return false;
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (!(clip != "Shooting"))
		{
			StateHandle castState = _castState;
			if (castState != null && castState.IsActive && TowerDefenseManager.HasGameplayAuthority)
			{
				CastProphecy();
				base.Walk();
			}
		}
	}

	public override void OnAnimeStarted(string clip)
	{
		base.OnAnimeStarted(clip);
		UpdateArmLayers();
	}

	private void UpdateArmLayers()
	{
		if (GodotObject.IsInstanceValid(base.sprite))
		{
			bool flag = HasCrystalBall() && !die && !nearDie && base.sprite.clip != "Death";
			bool flag2 = flag && base.sprite.clip != "Eat";
			StringName[] raisedArms = RaisedArms;
			foreach (StringName layerName in raisedArms)
			{
				base.sprite.SetFliter(layerName, flag2);
			}
			raisedArms = LoweredArms;
			foreach (StringName layerName2 in raisedArms)
			{
				base.sprite.SetFliter(layerName2, !flag2);
			}
			base.sprite.SetFliter("Zombie_outerarm_hand", !flag2 && !_armLost);
			base.sprite.SetFliter("Zombie_outerarm_lower", !flag2 && !_armLost);
			base.sprite.SetFliter("ball", flag);
			base.sprite.SetFliter("light1", flag);
			base.sprite.SetFliter("light2", flag);
		}
	}

	private void ShowShield(int damage, Vector2 hitPosition)
	{
		if (damage > 0 && shieldTexture != null && !GodotObject.IsInstanceValid(_activeShield))
		{
			_activeShield = new Sprite2D
			{
				Texture = shieldTexture,
				ZIndex = 5
			};
			transformPoint.AddChild(_activeShield, forceReadableName: false, InternalMode.Disabled);
			_activeShield.GlobalPosition = ((hitPosition == Vector2.Zero) ? GetLogicalGlobalPosition() : hitPosition);
			Sprite2D activeShield = _activeShield;
			Tween tween = activeShield.CreateTween();
			tween.TweenProperty(activeShield, "modulate:a", 0.0, 0.3).SetDelay(0.1);
			tween.TweenCallback(Callable.From(activeShield.QueueFree));
		}
	}

	public override void ArmorHitpointsEmpty(string armorName)
	{
		base.ArmorHitpointsEmpty(armorName);
		if (!(armorName != "DivineMageBall"))
		{
			UpdateArmLayers();
			StateHandle castState = _castState;
			if (castState != null && castState.IsActive && TowerDefenseManager.HasGameplayAuthority)
			{
				base.Walk();
			}
		}
	}

	public override void ArmorDamagePointReach(string armorName, int stage)
	{
		base.ArmorDamagePointReach(armorName, stage);
		UpdateArmLayers();
	}

	public override void DamagePointReach(string name)
	{
		base.DamagePointReach(name);
		if (name == "Arm")
		{
			_armLost = true;
		}
		UpdateArmLayers();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		Dictionary dictionary = base.ExportNetworkSpecialState();
		dictionary["prophecySerial"] = _prophecySerial;
		dictionary["prophecySeed"] = _prophecySeed;
		dictionary["prophecyRemaining"] = _prophecyRemaining;
		dictionary["prophecyX"] = _prophecyPosition.X;
		dictionary["prophecyY"] = _prophecyPosition.Y;
		dictionary["mageArmLost"] = _armLost;
		return dictionary;
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		base.ImportNetworkSpecialState(data);
		_prophecySerial = data.GetValueOrDefault("prophecySerial", 0).AsInt32();
		_prophecySeed = data.GetValueOrDefault("prophecySeed", 0).AsInt32();
		_prophecyRemaining = data.GetValueOrDefault("prophecyRemaining", 0.0).AsDouble();
		_prophecyPosition = new Vector2((float)data.GetValueOrDefault("prophecyX", 0.0).AsDouble(), (float)data.GetValueOrDefault("prophecyY", 0.0).AsDouble());
		_armLost = data.GetValueOrDefault("mageArmLost", false).AsBool();
		ApplyReceivedProphecy();
		UpdateArmLayers();
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return (_prophecySerial << 1) | (_armLost ? 1 : 0);
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["mageEnteredField"] = _enteredField;
		dictionary["mageSpellPending"] = _spellPending;
		dictionary["mageCasting"] = _castState?.IsActive ?? false;
		dictionary.Merge(ExportNetworkSpecialState(), overwrite: true);
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_enteredField = data.GetValueOrDefault("mageEnteredField", false).AsBool();
		_spellPending = data.GetValueOrDefault("mageSpellPending", false).AsBool();
		_preserveCastOnRestore = data.GetValueOrDefault("mageCasting", false).AsBool();
		_receivedProphecySerial = data.GetValueOrDefault("prophecySerial", 0).AsInt32();
		ImportNetworkSpecialState(data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(29)
		{
			new MethodInfo(MethodName.HasCrystalBall, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanCast, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ProcessSpell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Walk, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CastEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CastProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CastExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.CastProphecy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyReceivedProphecy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IsCharmed, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnAnimeStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateArmLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ShowShield, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "damage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2, "hitPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ArmorHitpointsEmpty, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "armorName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ArmorDamagePointReach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "armorName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "stage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DamagePointReach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName.HasCrystalBall && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasCrystalBall());
			return true;
		}
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
		if (method == MethodName.CanCast && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanCast());
			return true;
		}
		if (method == MethodName.ProcessSpell && args.Count == 1)
		{
			ProcessSpell(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 1)
		{
			OnTimerTimeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.Walk && args.Count == 0)
		{
			Walk();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastEntered && args.Count == 0)
		{
			CastEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastProcessing && args.Count == 1)
		{
			CastProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastExited && args.Count == 0)
		{
			CastExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CastProphecy && args.Count == 0)
		{
			CastProphecy();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyReceivedProphecy && args.Count == 0)
		{
			ApplyReceivedProphecy();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsCharmed && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsCharmed());
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimeStarted && args.Count == 1)
		{
			OnAnimeStarted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateArmLayers && args.Count == 0)
		{
			UpdateArmLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowShield && args.Count == 2)
		{
			ShowShield(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ArmorHitpointsEmpty && args.Count == 1)
		{
			ArmorHitpointsEmpty(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ArmorDamagePointReach && args.Count == 2)
		{
			ArmorDamagePointReach(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DamagePointReach && args.Count == 1)
		{
			DamagePointReach(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName.GetNetworkSpecialStateRevision && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(GetNetworkSpecialStateRevision());
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
		if (method == MethodName.HasCrystalBall)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.CanCast)
		{
			return true;
		}
		if (method == MethodName.ProcessSpell)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
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
		if (method == MethodName.Walk)
		{
			return true;
		}
		if (method == MethodName.CastEntered)
		{
			return true;
		}
		if (method == MethodName.CastProcessing)
		{
			return true;
		}
		if (method == MethodName.CastExited)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.CastProphecy)
		{
			return true;
		}
		if (method == MethodName.ApplyReceivedProphecy)
		{
			return true;
		}
		if (method == MethodName.IsCharmed)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.OnAnimeStarted)
		{
			return true;
		}
		if (method == MethodName.UpdateArmLayers)
		{
			return true;
		}
		if (method == MethodName.ShowShield)
		{
			return true;
		}
		if (method == MethodName.ArmorHitpointsEmpty)
		{
			return true;
		}
		if (method == MethodName.ArmorDamagePointReach)
		{
			return true;
		}
		if (method == MethodName.DamagePointReach)
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
		if (method == MethodName.GetNetworkSpecialStateRevision)
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
		if (name == PropertyName.shieldTexture)
		{
			shieldTexture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._enteredField)
		{
			_enteredField = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._spellPending)
		{
			_spellPending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._preserveCastOnRestore)
		{
			_preserveCastOnRestore = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._armLost)
		{
			_armLost = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._prophecySerial)
		{
			_prophecySerial = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._prophecySeed)
		{
			_prophecySeed = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._receivedProphecySerial)
		{
			_receivedProphecySerial = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._prophecyRemaining)
		{
			_prophecyRemaining = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._prophecyPosition)
		{
			_prophecyPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._activeShield)
		{
			_activeShield = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.shieldTexture)
		{
			value = VariantUtils.CreateFrom(in shieldTexture);
			return true;
		}
		if (name == PropertyName._enteredField)
		{
			value = VariantUtils.CreateFrom(in _enteredField);
			return true;
		}
		if (name == PropertyName._spellPending)
		{
			value = VariantUtils.CreateFrom(in _spellPending);
			return true;
		}
		if (name == PropertyName._preserveCastOnRestore)
		{
			value = VariantUtils.CreateFrom(in _preserveCastOnRestore);
			return true;
		}
		if (name == PropertyName._armLost)
		{
			value = VariantUtils.CreateFrom(in _armLost);
			return true;
		}
		if (name == PropertyName._prophecySerial)
		{
			value = VariantUtils.CreateFrom(in _prophecySerial);
			return true;
		}
		if (name == PropertyName._prophecySeed)
		{
			value = VariantUtils.CreateFrom(in _prophecySeed);
			return true;
		}
		if (name == PropertyName._receivedProphecySerial)
		{
			value = VariantUtils.CreateFrom(in _receivedProphecySerial);
			return true;
		}
		if (name == PropertyName._prophecyRemaining)
		{
			value = VariantUtils.CreateFrom(in _prophecyRemaining);
			return true;
		}
		if (name == PropertyName._prophecyPosition)
		{
			value = VariantUtils.CreateFrom(in _prophecyPosition);
			return true;
		}
		if (name == PropertyName._activeShield)
		{
			value = VariantUtils.CreateFrom(in _activeShield);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.shieldTexture, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._enteredField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._spellPending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._preserveCastOnRestore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._armLost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._prophecySerial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._prophecySeed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._receivedProphecySerial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._prophecyRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._prophecyPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._activeShield, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.shieldTexture, Variant.From(in shieldTexture));
		info.AddProperty(PropertyName._enteredField, Variant.From(in _enteredField));
		info.AddProperty(PropertyName._spellPending, Variant.From(in _spellPending));
		info.AddProperty(PropertyName._preserveCastOnRestore, Variant.From(in _preserveCastOnRestore));
		info.AddProperty(PropertyName._armLost, Variant.From(in _armLost));
		info.AddProperty(PropertyName._prophecySerial, Variant.From(in _prophecySerial));
		info.AddProperty(PropertyName._prophecySeed, Variant.From(in _prophecySeed));
		info.AddProperty(PropertyName._receivedProphecySerial, Variant.From(in _receivedProphecySerial));
		info.AddProperty(PropertyName._prophecyRemaining, Variant.From(in _prophecyRemaining));
		info.AddProperty(PropertyName._prophecyPosition, Variant.From(in _prophecyPosition));
		info.AddProperty(PropertyName._activeShield, Variant.From(in _activeShield));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.shieldTexture, out var value))
		{
			shieldTexture = value.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._enteredField, out var value2))
		{
			_enteredField = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._spellPending, out var value3))
		{
			_spellPending = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._preserveCastOnRestore, out var value4))
		{
			_preserveCastOnRestore = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._armLost, out var value5))
		{
			_armLost = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._prophecySerial, out var value6))
		{
			_prophecySerial = value6.As<int>();
		}
		if (info.TryGetProperty(PropertyName._prophecySeed, out var value7))
		{
			_prophecySeed = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._receivedProphecySerial, out var value8))
		{
			_receivedProphecySerial = value8.As<int>();
		}
		if (info.TryGetProperty(PropertyName._prophecyRemaining, out var value9))
		{
			_prophecyRemaining = value9.As<double>();
		}
		if (info.TryGetProperty(PropertyName._prophecyPosition, out var value10))
		{
			_prophecyPosition = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._activeShield, out var value11))
		{
			_activeShield = value11.As<Sprite2D>();
		}
	}
}
