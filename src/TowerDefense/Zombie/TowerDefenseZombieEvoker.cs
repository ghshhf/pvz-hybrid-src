using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter10/Evoker/Scene/TowerDefenseZombieEvoker.cs")]
public class TowerDefenseZombieEvoker : TowerDefenseZombie, INetworkSpawnStateReceiver
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ApplyShadowInstanceFlags = "ApplyShadowInstanceFlags";

		public new static readonly StringName Rise = "Rise";

		public static readonly StringName ApplyShadowSetup = "ApplyShadowSetup";

		public static readonly StringName ApplyShadowSpeed = "ApplyShadowSpeed";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName CanCast = "CanCast";

		public static readonly StringName HasFullSouls = "HasFullSouls";

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

		public static readonly StringName SpawnSoul = "SpawnSoul";

		public static readonly StringName InheritCharmTo = "InheritCharmTo";

		public static readonly StringName SummonShadow = "SummonShadow";

		public static readonly StringName ProcessShadowSouls = "ProcessShadowSouls";

		public static readonly StringName EndShadow = "EndShadow";

		public static readonly StringName RegisterSoul = "RegisterSoul";

		public static readonly StringName EnsureSoulOrbitBalance = "EnsureSoulOrbitBalance";

		public static readonly StringName ReleaseSouls = "ReleaseSouls";

		public static readonly StringName ReleaseShadows = "ReleaseShadows";

		public static readonly StringName ReleaseSoulsSilently = "ReleaseSoulsSilently";

		public static readonly StringName ReleaseShadowsSilently = "ReleaseShadowsSilently";

		public new static readonly StringName DieEntered = "DieEntered";

		public new static readonly StringName Destroy = "Destroy";

		public new static readonly StringName SilentlyRemove = "SilentlyRemove";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName ExportNetworkSpawnState = "ExportNetworkSpawnState";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public static readonly StringName ImportNetworkSpawnState = "ImportNetworkSpawnState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName isShadow = "isShadow";

		public static readonly StringName SoulCount = "SoulCount";

		public static readonly StringName _enteredField = "_enteredField";

		public static readonly StringName _spellPending = "_spellPending";

		public static readonly StringName _preserveCastOnRestore = "_preserveCastOnRestore";

		public static readonly StringName _isShadow = "_isShadow";

		public static readonly StringName shadowSourceHitpointScale = "shadowSourceHitpointScale";

		public static readonly StringName shadowSourceTimeScaleInit = "shadowSourceTimeScaleInit";

		public static readonly StringName _shadowSetupApplied = "_shadowSetupApplied";

		public static readonly StringName _shadowSoulsSpawned = "_shadowSoulsSpawned";

		public static readonly StringName _balancedSoulCount = "_balancedSoulCount";

		public static readonly StringName summonOwnerKey = "summonOwnerKey";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const double CastInterval = 5.0;

	private const int MaxSouls = 3;

	private const double ShadowHitpointScale = 0.5;

	private const double ShadowSpeedScale = 2.0;

	private StateHandle _castState;

	private CharacterTimerComponent _timerComponent;

	private bool _enteredField;

	private bool _spellPending;

	private bool _preserveCastOnRestore;

	private readonly List<TowerDefenseZombieMagicSoul> _souls = new List<TowerDefenseZombieMagicSoul>();

	private readonly List<TowerDefenseZombieEvoker> _shadows = new List<TowerDefenseZombieEvoker>();

	private bool _isShadow;

	public double shadowSourceHitpointScale = 1.0;

	public double shadowSourceTimeScaleInit = 1.0;

	private bool _shadowSetupApplied;

	private bool _shadowSoulsSpawned;

	private int _balancedSoulCount;

	public string summonOwnerKey = Guid.NewGuid().ToString("N");

	public bool isShadow
	{
		get
		{
			return _isShadow;
		}
		set
		{
			_isShadow = value;
			if (value)
			{
				ApplyShadowInstanceFlags();
			}
		}
	}

	public int SoulCount
	{
		get
		{
			_souls.RemoveAll((TowerDefenseZombieMagicSoul s) => !GodotObject.IsInstanceValid(s) || s.die || s.isDestroy);
			return _souls.Count;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			AddToGroup("Evoker");
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += OnTimerTimeout;
			}
			_castState = base.StateMachine.GetStateById("zombie.evoker.cast");
			_castState.Entered += CastEntered;
			_castState.PhysicsProcessing += CastProcessing;
			_castState.Exited += CastExited;
			if (isShadow)
			{
				ApplyShadowInstanceFlags();
				StartSpawnFadeIn();
				Callable.From(ApplyShadowSetup).CallDeferred();
			}
		}
	}

	private void ApplyShadowInstanceFlags()
	{
		if (isShadow)
		{
			fadeOutOnDeath = true;
			if (GodotObject.IsInstanceValid(instance))
			{
				DisableDamagePoints();
			}
		}
	}

	public override void Rise(double duration = -1.0, double delay = 0.0, bool createDirt = true, bool changeState = true, double from = 150.0, bool emitRiseOver = true)
	{
		if (!isShadow)
		{
			base.Rise(duration, delay, createDirt, changeState, from, emitRiseOver);
		}
	}

	private void ApplyShadowSetup()
	{
		if (!isShadow || !IsInsideTree())
		{
			return;
		}
		ApplyShadowSpeed();
		if (!_shadowSetupApplied)
		{
			_shadowSetupApplied = true;
			ApplyShadowInstanceFlags();
			if (GodotObject.IsInstanceValid(instance))
			{
				instance.collisionFlags = 0;
				instance.hitpointScale = shadowSourceHitpointScale * 0.5;
			}
			RefreshResourceComponentFacades();
			ShaderEffectComponent obj = shaderEffectComponent;
			if (obj != null && !obj.IsReleased)
			{
				shaderEffectComponent.SetSpriteGroupShaderParameter("hologram", true);
			}
		}
	}

	private void ApplyShadowSpeed()
	{
		if (isShadow)
		{
			timeScaleInit = shadowSourceTimeScaleInit * 2.0;
		}
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (isShadow)
		{
			ApplyShadowSpeed();
			if (!_shadowSetupApplied)
			{
				ApplyShadowSetup();
			}
		}
	}

	private bool CanCast()
	{
		if (CanRunAbility() && TowerDefenseManager.HasGameplayAuthority && inGame && !isRise)
		{
			return GodotObject.IsInstanceValid(base.sprite);
		}
		return false;
	}

	private bool HasFullSouls()
	{
		return SoulCount >= 3;
	}

	private void ProcessSpell()
	{
		if (isShadow)
		{
			ProcessShadowSouls();
		}
		else
		{
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
					_timerComponent.Run("Cast", 5.0);
				}
			}
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (!(timerName != "Cast"))
		{
			EnsureSoulOrbitBalance();
			StateHandle castState = _castState;
			if ((castState == null || !castState.IsActive) && CanCast() && !isShadow)
			{
				SendStateEvent("ToEvokerCast");
			}
		}
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		ProcessSpell();
	}

	public override void WalkProcessing(double delta)
	{
		base.WalkProcessing(delta);
		ProcessSpell();
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		ProcessSpell();
	}

	public override void Walk()
	{
		StateHandle castState = _castState;
		if (castState == null || !castState.IsActive || die || nearDie)
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
		_timerComponent?.Run("Cast", 5.0);
		_spellPending = true;
		base.sprite.SetAnimation("Magic", loop: false);
	}

	private void CastProcessing(double delta)
	{
		base.sprite.timeScale = timeScale;
		ProcessSpell();
	}

	private void CastExited()
	{
		_spellPending = false;
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (command != "magic" || !_spellPending)
		{
			return;
		}
		StateHandle castState = _castState;
		if (castState != null && castState.IsActive && CanCast())
		{
			_spellPending = false;
			if (HasFullSouls())
			{
				SummonShadow();
			}
			else
			{
				SpawnSoul();
			}
		}
	}

	private void SpawnSoul()
	{
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("ZombieMagicSoul");
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		Vector2 pos = GetLogicalGlobalPosition() + new Vector2(0f - towerDefenseManager.GetMapGridSize().X, 0f);
		TowerDefenseZombieMagicSoul towerDefenseZombieMagicSoul = packetConfig.Create(pos, towerDefenseManager.GetMapGridPos(pos), base.groundHeight) as TowerDefenseZombieMagicSoul;
		if (GodotObject.IsInstanceValid(towerDefenseZombieMagicSoul))
		{
			towerDefenseZombieMagicSoul.isShadow = isShadow;
			towerDefenseZombieMagicSoul.BindOwner(this);
			TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseZombieMagicSoul, forceReadableName: false, InternalMode.Disabled);
			towerDefenseZombieMagicSoul.Walk();
			InheritCharmTo(towerDefenseZombieMagicSoul);
			TowerDefenseManager.PublishSpawnedCharacter("ZombieMagicSoul", towerDefenseZombieMagicSoul, useCreate: true, 0.0, walkAfterSpawn: true, "", towerDefenseZombieMagicSoul.ExportNetworkSpawnState());
		}
	}

	private void InheritCharmTo(TowerDefenseZombie summon)
	{
		if (GodotObject.IsInstanceValid(summon))
		{
			if (GodotObject.IsInstanceValid(instance) && instance.hypnoses)
			{
				summon.RefreshResourceComponentFacades();
				summon.Hypnoses();
			}
			else
			{
				summon.camp = base.camp;
			}
		}
	}

	private void SummonShadow()
	{
		if (!TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("ZombieEvoker");
		if (!GodotObject.IsInstanceValid(packetConfig))
		{
			return;
		}
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		TowerDefenseZombieEvoker towerDefenseZombieEvoker = packetConfig.Create(logicalGlobalPosition, base.gridPos, base.groundHeight) as TowerDefenseZombieEvoker;
		if (GodotObject.IsInstanceValid(towerDefenseZombieEvoker))
		{
			double num = (GodotObject.IsInstanceValid(instance) ? instance.hitpointScale : 1.0);
			Vector2 scale = (GodotObject.IsInstanceValid(transformPoint) ? transformPoint.Scale : Vector2.One);
			towerDefenseZombieEvoker.isShadow = true;
			towerDefenseZombieEvoker.shadowSourceHitpointScale = num;
			towerDefenseZombieEvoker.shadowSourceTimeScaleInit = timeScaleInit;
			TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseZombieEvoker, forceReadableName: false, InternalMode.Disabled);
			towerDefenseZombieEvoker.SetHitpointAndScale(num * 0.5, scale);
			InheritCharmTo(towerDefenseZombieEvoker);
			if (!_shadows.Contains(towerDefenseZombieEvoker))
			{
				_shadows.Add(towerDefenseZombieEvoker);
			}
			towerDefenseZombieEvoker.CallDeferred("Walk");
			TowerDefenseManager.PublishSpawnedCharacter("ZombieEvoker", towerDefenseZombieEvoker, useCreate: true, 0.0, walkAfterSpawn: true, "", towerDefenseZombieEvoker.ExportNetworkSpawnState());
		}
	}

	private void ProcessShadowSouls()
	{
		if (isShadow && !_shadowSoulsSpawned && CanRunAbility() && TowerDefenseManager.HasGameplayAuthority && inGame && !isRise && GodotObject.IsInstanceValid(base.sprite) && base.IsInsideComponentBattlefield)
		{
			_shadowSoulsSpawned = true;
			for (int i = 0; i < 3; i++)
			{
				SpawnSoul();
			}
		}
	}

	public void EndShadow()
	{
		if (!die && !isDestroy && IsInsideTree())
		{
			IStateMachineController stateMachine = base.StateMachine;
			if (stateMachine != null && stateMachine.IsInitialized)
			{
				Die();
			}
			else
			{
				Destroy();
			}
		}
	}

	public void RegisterSoul(TowerDefenseZombieMagicSoul soul)
	{
		if (!_souls.Contains(soul))
		{
			_souls.Add(soul);
		}
		EnsureSoulOrbitBalance();
	}

	private void EnsureSoulOrbitBalance()
	{
		int soulCount = SoulCount;
		if (soulCount == _balancedSoulCount)
		{
			return;
		}
		_balancedSoulCount = soulCount;
		if (soulCount > 0)
		{
			double orbitAngle = _souls[0].OrbitAngle;
			for (int i = 0; i < soulCount; i++)
			{
				_souls[i].SetOrbitAngle(orbitAngle + (double)i * (Math.PI * 2.0) / (double)soulCount);
			}
		}
	}

	private void ReleaseSouls()
	{
		TowerDefenseZombieMagicSoul[] array = _souls.ToArray();
		foreach (TowerDefenseZombieMagicSoul towerDefenseZombieMagicSoul in array)
		{
			if (GodotObject.IsInstanceValid(towerDefenseZombieMagicSoul))
			{
				towerDefenseZombieMagicSoul.EndSoul();
			}
		}
		_souls.Clear();
	}

	private void ReleaseShadows()
	{
		TowerDefenseZombieEvoker[] array = _shadows.ToArray();
		foreach (TowerDefenseZombieEvoker towerDefenseZombieEvoker in array)
		{
			if (GodotObject.IsInstanceValid(towerDefenseZombieEvoker))
			{
				towerDefenseZombieEvoker.EndShadow();
			}
		}
		_shadows.Clear();
	}

	private void ReleaseSoulsSilently()
	{
		TowerDefenseZombieMagicSoul[] array = _souls.ToArray();
		foreach (TowerDefenseZombieMagicSoul towerDefenseZombieMagicSoul in array)
		{
			if (GodotObject.IsInstanceValid(towerDefenseZombieMagicSoul))
			{
				towerDefenseZombieMagicSoul.SilentlyRemoveAsOrphan();
			}
		}
		_souls.Clear();
	}

	private void ReleaseShadowsSilently()
	{
		TowerDefenseZombieEvoker[] array = _shadows.ToArray();
		foreach (TowerDefenseZombieEvoker towerDefenseZombieEvoker in array)
		{
			if (GodotObject.IsInstanceValid(towerDefenseZombieEvoker))
			{
				towerDefenseZombieEvoker.SilentlyRemoveAsOrphan();
			}
		}
		_shadows.Clear();
	}

	public override void DieEntered()
	{
		ReleaseSouls();
		ReleaseShadows();
		base.DieEntered();
	}

	public override void Destroy(bool freeInstance = true)
	{
		ReleaseSouls();
		ReleaseShadows();
		base.Destroy(freeInstance);
	}

	protected override void SilentlyRemove()
	{
		ReleaseSouls();
		ReleaseShadows();
		base.SilentlyRemove();
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Magic")
		{
			StateHandle castState = _castState;
			if (castState != null && castState.IsActive && CanCast())
			{
				base.Walk();
			}
		}
	}

	public override Dictionary ExportNetworkSpawnState()
	{
		return new Dictionary
		{
			["evokerOwnerKey"] = summonOwnerKey,
			["evokerShadow"] = isShadow,
			["evokerShadowHitpointScale"] = shadowSourceHitpointScale,
			["evokerShadowTimeScaleInit"] = shadowSourceTimeScaleInit
		};
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportNetworkSpawnState();
	}

	public void ImportNetworkSpawnState(Dictionary data)
	{
		ImportNetworkSpecialState(data);
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		if (data.ContainsKey("evokerOwnerKey"))
		{
			summonOwnerKey = data["evokerOwnerKey"].AsString();
		}
		if (data.ContainsKey("evokerShadow"))
		{
			isShadow = data["evokerShadow"].AsBool();
		}
		shadowSourceHitpointScale = data.GetValueOrDefault("evokerShadowHitpointScale", shadowSourceHitpointScale).AsDouble();
		shadowSourceTimeScaleInit = data.GetValueOrDefault("evokerShadowTimeScaleInit", shadowSourceTimeScaleInit).AsDouble();
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary.Merge(ExportNetworkSpawnState(), overwrite: true);
		dictionary["evokerEnteredField"] = _enteredField;
		dictionary["evokerSpellPending"] = _spellPending;
		dictionary["evokerCasting"] = _castState?.IsActive ?? false;
		dictionary["evokerShadowSouls"] = _shadowSoulsSpawned;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		ImportNetworkSpecialState(data);
		_enteredField = data.GetValueOrDefault("evokerEnteredField", false).AsBool();
		_spellPending = data.GetValueOrDefault("evokerSpellPending", false).AsBool();
		_preserveCastOnRestore = data.GetValueOrDefault("evokerCasting", false).AsBool();
		_shadowSoulsSpawned = data.GetValueOrDefault("evokerShadowSouls", false).AsBool();
		ApplyShadowInstanceFlags();
		ApplyShadowSetup();
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			ReleaseSoulsSilently();
			ReleaseShadowsSilently();
		}
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
		}
		base._ExitTree();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(40)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyShadowInstanceFlags, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Rise, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "delay", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "createDirt", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "changeState", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "from", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "emitRiseOver", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyShadowSetup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyShadowSpeed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CanCast, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HasFullSouls, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ProcessSpell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
			new MethodInfo(MethodName.SpawnSoul, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.InheritCharmTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "summon", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.SummonShadow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ProcessShadowSouls, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EndShadow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RegisterSoul, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "soul", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.EnsureSoulOrbitBalance, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ReleaseSouls, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ReleaseShadows, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ReleaseSoulsSilently, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ReleaseShadowsSilently, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DieEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Destroy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "freeInstance", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SilentlyRemove, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpawnState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpawnState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.ApplyShadowInstanceFlags && args.Count == 0)
		{
			ApplyShadowInstanceFlags();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Rise && args.Count == 6)
		{
			Rise(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3]), VariantUtils.ConvertTo<double>(in args[4]), VariantUtils.ConvertTo<bool>(in args[5]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyShadowSetup && args.Count == 0)
		{
			ApplyShadowSetup();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyShadowSpeed && args.Count == 0)
		{
			ApplyShadowSpeed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanCast && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanCast());
			return true;
		}
		if (method == MethodName.HasFullSouls && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasFullSouls());
			return true;
		}
		if (method == MethodName.ProcessSpell && args.Count == 0)
		{
			ProcessSpell();
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
		if (method == MethodName.SpawnSoul && args.Count == 0)
		{
			SpawnSoul();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InheritCharmTo && args.Count == 1)
		{
			InheritCharmTo(VariantUtils.ConvertTo<TowerDefenseZombie>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SummonShadow && args.Count == 0)
		{
			SummonShadow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessShadowSouls && args.Count == 0)
		{
			ProcessShadowSouls();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndShadow && args.Count == 0)
		{
			EndShadow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RegisterSoul && args.Count == 1)
		{
			RegisterSoul(VariantUtils.ConvertTo<TowerDefenseZombieMagicSoul>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnsureSoulOrbitBalance && args.Count == 0)
		{
			EnsureSoulOrbitBalance();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleaseSouls && args.Count == 0)
		{
			ReleaseSouls();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleaseShadows && args.Count == 0)
		{
			ReleaseShadows();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleaseSoulsSilently && args.Count == 0)
		{
			ReleaseSoulsSilently();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleaseShadowsSilently && args.Count == 0)
		{
			ReleaseShadowsSilently();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DieEntered && args.Count == 0)
		{
			DieEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Destroy && args.Count == 1)
		{
			Destroy(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SilentlyRemove && args.Count == 0)
		{
			SilentlyRemove();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportNetworkSpawnState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpawnState());
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpecialState());
			return true;
		}
		if (method == MethodName.ImportNetworkSpawnState && args.Count == 1)
		{
			ImportNetworkSpawnState(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState && args.Count == 1)
		{
			ImportNetworkSpecialState(VariantUtils.ConvertTo<Dictionary>(in args[0]));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.ApplyShadowInstanceFlags)
		{
			return true;
		}
		if (method == MethodName.Rise)
		{
			return true;
		}
		if (method == MethodName.ApplyShadowSetup)
		{
			return true;
		}
		if (method == MethodName.ApplyShadowSpeed)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.CanCast)
		{
			return true;
		}
		if (method == MethodName.HasFullSouls)
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
		if (method == MethodName.SpawnSoul)
		{
			return true;
		}
		if (method == MethodName.InheritCharmTo)
		{
			return true;
		}
		if (method == MethodName.SummonShadow)
		{
			return true;
		}
		if (method == MethodName.ProcessShadowSouls)
		{
			return true;
		}
		if (method == MethodName.EndShadow)
		{
			return true;
		}
		if (method == MethodName.RegisterSoul)
		{
			return true;
		}
		if (method == MethodName.EnsureSoulOrbitBalance)
		{
			return true;
		}
		if (method == MethodName.ReleaseSouls)
		{
			return true;
		}
		if (method == MethodName.ReleaseShadows)
		{
			return true;
		}
		if (method == MethodName.ReleaseSoulsSilently)
		{
			return true;
		}
		if (method == MethodName.ReleaseShadowsSilently)
		{
			return true;
		}
		if (method == MethodName.DieEntered)
		{
			return true;
		}
		if (method == MethodName.Destroy)
		{
			return true;
		}
		if (method == MethodName.SilentlyRemove)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpawnState)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.ImportNetworkSpawnState)
		{
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState)
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.isShadow)
		{
			isShadow = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._isShadow)
		{
			_isShadow = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.shadowSourceHitpointScale)
		{
			shadowSourceHitpointScale = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.shadowSourceTimeScaleInit)
		{
			shadowSourceTimeScaleInit = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._shadowSetupApplied)
		{
			_shadowSetupApplied = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._shadowSoulsSpawned)
		{
			_shadowSoulsSpawned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._balancedSoulCount)
		{
			_balancedSoulCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.summonOwnerKey)
		{
			summonOwnerKey = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.isShadow)
		{
			value = VariantUtils.CreateFrom<bool>(isShadow);
			return true;
		}
		if (name == PropertyName.SoulCount)
		{
			value = VariantUtils.CreateFrom<int>(SoulCount);
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
		if (name == PropertyName._isShadow)
		{
			value = VariantUtils.CreateFrom(in _isShadow);
			return true;
		}
		if (name == PropertyName.shadowSourceHitpointScale)
		{
			value = VariantUtils.CreateFrom(in shadowSourceHitpointScale);
			return true;
		}
		if (name == PropertyName.shadowSourceTimeScaleInit)
		{
			value = VariantUtils.CreateFrom(in shadowSourceTimeScaleInit);
			return true;
		}
		if (name == PropertyName._shadowSetupApplied)
		{
			value = VariantUtils.CreateFrom(in _shadowSetupApplied);
			return true;
		}
		if (name == PropertyName._shadowSoulsSpawned)
		{
			value = VariantUtils.CreateFrom(in _shadowSoulsSpawned);
			return true;
		}
		if (name == PropertyName._balancedSoulCount)
		{
			value = VariantUtils.CreateFrom(in _balancedSoulCount);
			return true;
		}
		if (name == PropertyName.summonOwnerKey)
		{
			value = VariantUtils.CreateFrom(in summonOwnerKey);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._enteredField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._spellPending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._preserveCastOnRestore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.isShadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._isShadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.shadowSourceHitpointScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.shadowSourceTimeScaleInit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._shadowSetupApplied, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._shadowSoulsSpawned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._balancedSoulCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.summonOwnerKey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.SoulCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.isShadow, Variant.From<bool>(isShadow));
		info.AddProperty(PropertyName._enteredField, Variant.From(in _enteredField));
		info.AddProperty(PropertyName._spellPending, Variant.From(in _spellPending));
		info.AddProperty(PropertyName._preserveCastOnRestore, Variant.From(in _preserveCastOnRestore));
		info.AddProperty(PropertyName._isShadow, Variant.From(in _isShadow));
		info.AddProperty(PropertyName.shadowSourceHitpointScale, Variant.From(in shadowSourceHitpointScale));
		info.AddProperty(PropertyName.shadowSourceTimeScaleInit, Variant.From(in shadowSourceTimeScaleInit));
		info.AddProperty(PropertyName._shadowSetupApplied, Variant.From(in _shadowSetupApplied));
		info.AddProperty(PropertyName._shadowSoulsSpawned, Variant.From(in _shadowSoulsSpawned));
		info.AddProperty(PropertyName._balancedSoulCount, Variant.From(in _balancedSoulCount));
		info.AddProperty(PropertyName.summonOwnerKey, Variant.From(in summonOwnerKey));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.isShadow, out var value))
		{
			isShadow = value.As<bool>();
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
		if (info.TryGetProperty(PropertyName._isShadow, out var value5))
		{
			_isShadow = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.shadowSourceHitpointScale, out var value6))
		{
			shadowSourceHitpointScale = value6.As<double>();
		}
		if (info.TryGetProperty(PropertyName.shadowSourceTimeScaleInit, out var value7))
		{
			shadowSourceTimeScaleInit = value7.As<double>();
		}
		if (info.TryGetProperty(PropertyName._shadowSetupApplied, out var value8))
		{
			_shadowSetupApplied = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._shadowSoulsSpawned, out var value9))
		{
			_shadowSoulsSpawned = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._balancedSoulCount, out var value10))
		{
			_balancedSoulCount = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName.summonOwnerKey, out var value11))
		{
			summonOwnerKey = value11.As<string>();
		}
	}
}
