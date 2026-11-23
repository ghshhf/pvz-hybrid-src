using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter1/Balloon/Scene/Blover/TowerDefenseZombieBalloonBlover.cs")]
public class TowerDefenseZombieBalloonBlover : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName StartCloverTimerIfNeeded = "StartCloverTimerIfNeeded";

		public static readonly StringName ConnectStateSignals = "ConnectStateSignals";

		public static readonly StringName FlyEntered = "FlyEntered";

		public static readonly StringName FlyProcessing = "FlyProcessing";

		public static readonly StringName FlyExited = "FlyExited";

		public static readonly StringName PopEntered = "PopEntered";

		public static readonly StringName PopProcessing = "PopProcessing";

		public static readonly StringName PopExited = "PopExited";

		public static readonly StringName FlyAttackEntered = "FlyAttackEntered";

		public static readonly StringName FlyAttackProcessing = "FlyAttackProcessing";

		public static readonly StringName FlyAttackExited = "FlyAttackExited";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName DieProcessing = "DieProcessing";

		public new static readonly StringName DieEntered = "DieEntered";

		public new static readonly StringName Walk = "Walk";

		public new static readonly StringName Blow = "Blow";

		public static readonly StringName StartLogicalBlowTween = "StartLogicalBlowTween";

		public static readonly StringName CompleteLogicalBlowTween = "CompleteLogicalBlowTween";

		public static readonly StringName CancelLogicalBlowTween = "CancelLogicalBlowTween";

		public static readonly StringName CompleteBlowAfterRestore = "CompleteBlowAfterRestore";

		public new static readonly StringName FinalizeProgressRestore = "FinalizeProgressRestore";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName ArmorHitpointsEmpty = "ArmorHitpointsEmpty";

		public static readonly StringName OnBlowAll = "OnBlowAll";

		public static readonly StringName OnBlowLine = "OnBlowLine";

		public static readonly StringName OnCloverTimeout = "OnCloverTimeout";

		public static readonly StringName BlowSelfAway = "BlowSelfAway";

		public static readonly StringName ActivateClover = "ActivateClover";

		public static readonly StringName RefreshCloverAnimation = "RefreshCloverAnimation";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";

		public new static readonly StringName Purify = "Purify";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName _stateSignalsConnected = "_stateSignalsConnected";

		public static readonly StringName _cloverStarted = "_cloverStarted";

		public static readonly StringName _cloverBlowRemaining = "_cloverBlowRemaining";

		public static readonly StringName _cloverDeparturePending = "_cloverDeparturePending";

		public static readonly StringName _logicalBlowTween = "_logicalBlowTween";

		public static readonly StringName _blowInFlight = "_blowInFlight";

		public static readonly StringName _blowTarget = "_blowTarget";

		public static readonly StringName _blowRestorePending = "_blowRestorePending";

		public static readonly StringName pop = "pop";

		public static readonly StringName speed = "speed";

		public static readonly StringName audioPlay = "audioPlay";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private StateHandle _flyStateHandle;

	private StateHandle _flyAttackStateHandle;

	private StateHandle _popStateHandle;

	private bool _stateSignalsConnected;

	private CharacterTimerComponent _timer;

	private BloverComponent _blover;

	private bool _cloverStarted;

	private double _cloverBlowRemaining;

	private bool _cloverDeparturePending;

	private Tween _logicalBlowTween;

	private bool _blowInFlight;

	private Vector2 _blowTarget;

	private bool _blowRestorePending;

	public bool pop;

	public double speed = 30.0;

	public bool audioPlay;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			ConnectStateSignals();
			_timer = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timer = _timer;
			if (timer != null && !timer.IsReleased)
			{
				_timer.OnTimeout += OnCloverTimeout;
			}
			_blover = componentManager.GetRuntime<BloverComponent>("character.blover");
			if (GodotObject.IsInstanceValid(BattleEventBus.Instance))
			{
				BattleEventBus.Instance.OnBlowAllEffectEmit += OnBlowAll;
				BattleEventBus.Instance.OnBlowLineEffectEmit += OnBlowLine;
			}
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timer = _timer;
		if (timer != null && !timer.IsReleased)
		{
			_timer.OnTimeout -= OnCloverTimeout;
		}
		if (GodotObject.IsInstanceValid(BattleEventBus.Instance))
		{
			BattleEventBus.Instance.OnBlowAllEffectEmit -= OnBlowAll;
			BattleEventBus.Instance.OnBlowLineEffectEmit -= OnBlowLine;
		}
		CancelLogicalBlowTween();
		base._ExitTree();
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (!TowerDefenseCharacter.CachedEditorHint && inGame && !base.isPause && !die && !isDestroy && !GetTree().Paused)
		{
			if (TowerDefenseManager._IsGameRunning() && TowerDefenseManager.HasGameplayAuthority)
			{
				StartCloverTimerIfNeeded();
			}
			if (_cloverBlowRemaining > 0.0)
			{
				_cloverBlowRemaining = Math.Max(0.0, _cloverBlowRemaining - Math.Max(0.0, delta));
				RefreshCloverAnimation();
			}
			if (_cloverBlowRemaining <= 0.0 && _cloverDeparturePending && TowerDefenseManager.HasGameplayAuthority)
			{
				_cloverDeparturePending = false;
				BlowSelfAway();
			}
		}
	}

	private void StartCloverTimerIfNeeded()
	{
		if (!_cloverStarted)
		{
			CharacterTimerComponent timer = _timer;
			if (timer != null && !timer.IsReleased && inGame)
			{
				_timer.Run("Clover", 8.0);
				_cloverStarted = true;
			}
		}
	}

	private void ConnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			return;
		}
		IStateMachineController stateMachine = base.StateMachine;
		if (stateMachine == null || !stateMachine.IsInitialized)
		{
			return;
		}
		_flyStateHandle = base.StateMachine?.GetStateById("zombie.balloon.fly");
		_flyAttackStateHandle = base.StateMachine?.GetStateById("zombie.balloon.fly_attack");
		_popStateHandle = base.StateMachine?.GetStateById("zombie.balloon.pop");
		StateHandle flyStateHandle = _flyStateHandle;
		if (flyStateHandle == null || !flyStateHandle.IsValid)
		{
			return;
		}
		StateHandle flyAttackStateHandle = _flyAttackStateHandle;
		if (flyAttackStateHandle != null && flyAttackStateHandle.IsValid)
		{
			StateHandle popStateHandle = _popStateHandle;
			if (popStateHandle != null && popStateHandle.IsValid)
			{
				_flyStateHandle.Entered += FlyEntered;
				_flyStateHandle.Exited += FlyExited;
				_flyStateHandle.PhysicsProcessing += FlyProcessing;
				_flyAttackStateHandle.Entered += FlyAttackEntered;
				_flyAttackStateHandle.Exited += FlyAttackExited;
				_flyAttackStateHandle.PhysicsProcessing += FlyAttackProcessing;
				_popStateHandle.Entered += PopEntered;
				_popStateHandle.Exited += PopExited;
				_popStateHandle.PhysicsProcessing += PopProcessing;
				_stateSignalsConnected = true;
			}
		}
	}

	public virtual void FlyEntered()
	{
		base.sprite.SetAnimation("Idle", loop: true, 0.2);
	}

	public virtual void FlyProcessing(double delta)
	{
		base.sprite.timeScale = timeScale;
		if (base.IsRemoteNetworkReplica)
		{
			return;
		}
		ulong currentPhysicsFrame = TowerDefenseProcessModeDispatch.CurrentPhysicsFrame;
		Vector2 globalPositionForPhysicsFrame = GetGlobalPositionForPhysicsFrame(currentPhysicsFrame);
		if (!base.sprite.pause)
		{
			globalPositionForPhysicsFrame.X -= (float)(speed * delta * base.sprite.timeScale * (double)transformPoint.Scale.X * (double)base.Scale.X * (double)((!base.sprite.playBack) ? 1 : (-1)));
			SetGlobalPositionForPhysicsFrame(globalPositionForPhysicsFrame, currentPhysicsFrame);
		}
		if ((double)globalPositionForPhysicsFrame.X < TowerDefenseManager.Instance.GetMapGroundLeft() - 20.0)
		{
			instance.ArmorDelete("Balloon");
			return;
		}
		if (!audioPlay && (double)globalPositionForPhysicsFrame.X < groundRight)
		{
			AudioManager.Instance.AudioPlay("BalloonInflate");
			audioPlay = true;
		}
		if (!base.sprite.pause && attackComponent.CanAttack())
		{
			SendStateEvent("ToFlyAttack");
		}
	}

	public virtual void FlyExited()
	{
	}

	public virtual void PopEntered()
	{
		base.sprite.SetAnimation("Pop", loop: false, 0.2);
	}

	public virtual void PopProcessing(double delta)
	{
		base.sprite.timeScale = timeScale * 2.0;
	}

	public virtual void PopExited()
	{
	}

	public void FlyAttackEntered()
	{
		base.sprite.SetAnimation("FlyEat", loop: true, 0.2);
		ActivateAttackAfterStateDelay(_flyAttackStateHandle);
	}

	public virtual void FlyAttackProcessing(double delta)
	{
		if (!attackComponent.CanAttack())
		{
			SendStateEvent("ToFly");
		}
		else if (startAttack && !nearDie && !base.sprite.pause && base.sprite.timeScale > 0.0 && base.useAttackDps)
		{
			attackComponent.AttackDpsExecute(delta, ((TowerDefenseZombieConfig)base.config).attack);
		}
		base.sprite.timeScale = timeScale * 2.0;
	}

	public virtual void FlyAttackExited()
	{
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		base.sprite.timeScale = timeScale * 2.0;
	}

	public override void DieProcessing(double delta)
	{
		base.DieProcessing(delta);
		base.sprite.timeScale = timeScale * 2.0;
	}

	public override void DieEntered()
	{
		CancelLogicalBlowTween();
		base.DieEntered();
	}

	public override void Walk()
	{
		if (pop)
		{
			SendStateEvent("ToWalk");
		}
		else
		{
			SendStateEvent("ToFly");
		}
	}

	public override void Blow()
	{
		if (!pop)
		{
			HitBoxDestroy();
			StartLogicalBlowTween();
		}
	}

	private void StartLogicalBlowTween()
	{
		CancelLogicalBlowTween();
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		Vector2 targetPosition = logicalGlobalPosition + new Vector2(TowerDefenseManager.Instance.GetMapGridSize().Y * (float)TowerDefenseManager.Instance.GetMapGridNum().Y * 2f, 0f);
		Tween tween = CreateTween();
		_logicalBlowTween = tween;
		_blowInFlight = true;
		_blowTarget = targetPosition;
		tween.TweenMethod(Callable.From<Vector2>(base.SetLogicalGlobalPosition), logicalGlobalPosition, targetPosition, 1.0);
		tween.TweenCallback(Callable.From(delegate
		{
			CompleteLogicalBlowTween(tween, targetPosition);
		}));
	}

	private void CompleteLogicalBlowTween(Tween tween, Vector2 targetPosition)
	{
		if (_logicalBlowTween == tween && !isDestroy)
		{
			_logicalBlowTween = null;
			_blowInFlight = false;
			SetLogicalGlobalPosition(targetPosition);
			Destroy();
		}
	}

	private void CancelLogicalBlowTween()
	{
		if (GodotObject.IsInstanceValid(_logicalBlowTween))
		{
			_logicalBlowTween.Kill();
		}
		_logicalBlowTween = null;
		_blowInFlight = false;
	}

	private void CompleteBlowAfterRestore()
	{
		_blowRestorePending = false;
		if (!isDestroy && !die)
		{
			SetLogicalGlobalPosition(_blowTarget);
			Destroy();
		}
	}

	public override void FinalizeProgressRestore()
	{
		base.FinalizeProgressRestore();
		RefreshCloverAnimation();
		if (_blowRestorePending)
		{
			Callable.From(CompleteBlowAfterRestore).CallDeferred();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["pop"] = pop,
			["speed"] = speed,
			["cloverStarted"] = _cloverStarted,
			["cloverBlowRemaining"] = _cloverBlowRemaining,
			["cloverDeparturePending"] = _cloverDeparturePending,
			["balloonBlowInFlight"] = _blowInFlight,
			["balloonBlowTargetX"] = _blowTarget.X,
			["balloonBlowTargetY"] = _blowTarget.Y
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		pop = data.GetValueOrDefault("pop", false).AsBool();
		speed = data.GetValueOrDefault("speed", 30.0).AsDouble();
		_cloverStarted = data.GetValueOrDefault("cloverStarted", false).AsBool();
		_cloverBlowRemaining = Math.Max(0.0, data.GetValueOrDefault("cloverBlowRemaining", 0.0).AsDouble());
		_cloverDeparturePending = data.GetValueOrDefault("cloverDeparturePending", false).AsBool();
		_blowInFlight = false;
		_blowRestorePending = data.GetValueOrDefault("balloonBlowInFlight", false).AsBool();
		_blowTarget = new Vector2(data.GetValueOrDefault("balloonBlowTargetX", 0f).AsSingle(), data.GetValueOrDefault("balloonBlowTargetY", 0f).AsSingle());
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Pop" && !die && !nearDie)
		{
			Walk();
		}
	}

	public override void ArmorHitpointsEmpty(string armorName)
	{
		base.ArmorHitpointsEmpty(armorName);
		if (!(armorName != "Balloon"))
		{
			pop = true;
			AudioManager.Instance.AudioPlay("BalloonPop");
			instance.collisionFlags = 1;
			instance.maskFlags = 9;
			instance.unUseBuffFlags = 512;
			if (GetHasArmor("SpecialHelmet"))
			{
				instance.armorOverrideUnUseBuffFlagSave = 512;
			}
			SendStateEvent("ToPop");
		}
	}

	private void OnBlowAll()
	{
		ActivateClover(triggerOffensiveBlow: false);
	}

	private void OnBlowLine(int line)
	{
		if (line == base.gridPos.Y)
		{
			ActivateClover(triggerOffensiveBlow: false);
		}
	}

	private void OnCloverTimeout(string timerName)
	{
		if (!(timerName != "Clover") && TowerDefenseManager.HasGameplayAuthority && !die && !isDestroy && !_blowInFlight)
		{
			_cloverDeparturePending = true;
			ActivateClover(triggerOffensiveBlow: true);
		}
	}

	private void BlowSelfAway()
	{
		if (!isDestroy && !die)
		{
			HitBoxDestroy();
			StartLogicalBlowTween();
		}
	}

	private void ActivateClover(bool triggerOffensiveBlow)
	{
		if (!TowerDefenseManager.HasGameplayAuthority || die || isDestroy || _blowInFlight)
		{
			return;
		}
		instance.unUseBuffFlags |= 512;
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(towerDefenseManager))
		{
			return;
		}
		if (_cloverBlowRemaining <= 0.0)
		{
			BloverComponent blover = _blover;
			_cloverBlowRemaining = Math.Max(0.0, (blover != null && !blover.IsReleased) ? _blover.blowTime : 2.0);
			RefreshCloverAnimation();
		}
		if (triggerOffensiveBlow)
		{
			BloverComponent blover2 = _blover;
			if (blover2 != null && !blover2.IsReleased)
			{
				_blover.ExecuteTargetList(towerDefenseManager.GetCharacterTarget(this));
			}
			BloverComponent.BlowEnemyFloatingPlants(this, -1);
		}
	}

	private void RefreshCloverAnimation()
	{
		if (base.sprite is ZombieBalloonBloverSprite zombieBalloonBloverSprite && GodotObject.IsInstanceValid(zombieBalloonBloverSprite.plantHead))
		{
			AdobeAnimateSpriteBase plantHead = zombieBalloonBloverSprite.plantHead;
			plantHead.timeScale = timeScale * 2.0;
			bool flag = _cloverBlowRemaining > 0.0;
			string text = ((!flag) ? "Idle" : (plantHead.HasClip("Loop") ? "Loop" : "Blow"));
			if (plantHead.clip != text || !plantHead.loop || plantHead.clipOver)
			{
				plantHead.SetAnimation(text, loop: true, flag ? 0.2 : 0.0);
			}
		}
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return new Dictionary
		{
			["rev"] = GetNetworkSpecialStateRevision(),
			["cloverBlowRemaining"] = _cloverBlowRemaining,
			["cloverDeparturePending"] = _cloverDeparturePending
		};
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		_cloverBlowRemaining = Math.Max(0.0, data.GetValueOrDefault("cloverBlowRemaining", 0.0).AsDouble());
		_cloverDeparturePending = data.GetValueOrDefault("cloverDeparturePending", false).AsBool();
		RefreshCloverAnimation();
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return ((_cloverBlowRemaining > 0.0) ? 1 : 0) | (_cloverDeparturePending ? 2 : 0);
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
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig("PlantBlover");
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
					MultiPlayerManager.Instance.SendSpawnCharacterAt("PlantBlover", base.gridPos.X, base.gridPos.Y, nextSyncId);
				}
			}
		}
		Destroy();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(38)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StartCloverTimerIfNeeded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FlyEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FlyProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FlyExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PopEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PopProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PopExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FlyAttackEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FlyAttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FlyAttackExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DieProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DieEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Walk, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Blow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartLogicalBlowTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CompleteLogicalBlowTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "tween", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Tween"), exported: false),
				new PropertyInfo(Variant.Type.Vector2, "targetPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CancelLogicalBlowTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CompleteBlowAfterRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FinalizeProgressRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ArmorHitpointsEmpty, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "armorName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnBlowAll, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnBlowLine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "line", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnCloverTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BlowSelfAway, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ActivateClover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "triggerOffensiveBlow", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshCloverAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName.StartCloverTimerIfNeeded && args.Count == 0)
		{
			StartCloverTimerIfNeeded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectStateSignals && args.Count == 0)
		{
			ConnectStateSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlyEntered && args.Count == 0)
		{
			FlyEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlyProcessing && args.Count == 1)
		{
			FlyProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlyExited && args.Count == 0)
		{
			FlyExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PopEntered && args.Count == 0)
		{
			PopEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PopProcessing && args.Count == 1)
		{
			PopProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PopExited && args.Count == 0)
		{
			PopExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlyAttackEntered && args.Count == 0)
		{
			FlyAttackEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlyAttackProcessing && args.Count == 1)
		{
			FlyAttackProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlyAttackExited && args.Count == 0)
		{
			FlyAttackExited();
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
		if (method == MethodName.DieEntered && args.Count == 0)
		{
			DieEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Walk && args.Count == 0)
		{
			Walk();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Blow && args.Count == 0)
		{
			Blow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartLogicalBlowTween && args.Count == 0)
		{
			StartLogicalBlowTween();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteLogicalBlowTween && args.Count == 2)
		{
			CompleteLogicalBlowTween(VariantUtils.ConvertTo<Tween>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelLogicalBlowTween && args.Count == 0)
		{
			CancelLogicalBlowTween();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteBlowAfterRestore && args.Count == 0)
		{
			CompleteBlowAfterRestore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FinalizeProgressRestore && args.Count == 0)
		{
			FinalizeProgressRestore();
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
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ArmorHitpointsEmpty && args.Count == 1)
		{
			ArmorHitpointsEmpty(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBlowAll && args.Count == 0)
		{
			OnBlowAll();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBlowLine && args.Count == 1)
		{
			OnBlowLine(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCloverTimeout && args.Count == 1)
		{
			OnCloverTimeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BlowSelfAway && args.Count == 0)
		{
			BlowSelfAway();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ActivateClover && args.Count == 1)
		{
			ActivateClover(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshCloverAnimation && args.Count == 0)
		{
			RefreshCloverAnimation();
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
		if (method == MethodName.StartCloverTimerIfNeeded)
		{
			return true;
		}
		if (method == MethodName.ConnectStateSignals)
		{
			return true;
		}
		if (method == MethodName.FlyEntered)
		{
			return true;
		}
		if (method == MethodName.FlyProcessing)
		{
			return true;
		}
		if (method == MethodName.FlyExited)
		{
			return true;
		}
		if (method == MethodName.PopEntered)
		{
			return true;
		}
		if (method == MethodName.PopProcessing)
		{
			return true;
		}
		if (method == MethodName.PopExited)
		{
			return true;
		}
		if (method == MethodName.FlyAttackEntered)
		{
			return true;
		}
		if (method == MethodName.FlyAttackProcessing)
		{
			return true;
		}
		if (method == MethodName.FlyAttackExited)
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
		if (method == MethodName.DieEntered)
		{
			return true;
		}
		if (method == MethodName.Walk)
		{
			return true;
		}
		if (method == MethodName.Blow)
		{
			return true;
		}
		if (method == MethodName.StartLogicalBlowTween)
		{
			return true;
		}
		if (method == MethodName.CompleteLogicalBlowTween)
		{
			return true;
		}
		if (method == MethodName.CancelLogicalBlowTween)
		{
			return true;
		}
		if (method == MethodName.CompleteBlowAfterRestore)
		{
			return true;
		}
		if (method == MethodName.FinalizeProgressRestore)
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
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ArmorHitpointsEmpty)
		{
			return true;
		}
		if (method == MethodName.OnBlowAll)
		{
			return true;
		}
		if (method == MethodName.OnBlowLine)
		{
			return true;
		}
		if (method == MethodName.OnCloverTimeout)
		{
			return true;
		}
		if (method == MethodName.BlowSelfAway)
		{
			return true;
		}
		if (method == MethodName.ActivateClover)
		{
			return true;
		}
		if (method == MethodName.RefreshCloverAnimation)
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
		if (method == MethodName.Purify)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._stateSignalsConnected)
		{
			_stateSignalsConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cloverStarted)
		{
			_cloverStarted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cloverBlowRemaining)
		{
			_cloverBlowRemaining = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._cloverDeparturePending)
		{
			_cloverDeparturePending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._logicalBlowTween)
		{
			_logicalBlowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._blowInFlight)
		{
			_blowInFlight = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._blowTarget)
		{
			_blowTarget = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._blowRestorePending)
		{
			_blowRestorePending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.pop)
		{
			pop = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.speed)
		{
			speed = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.audioPlay)
		{
			audioPlay = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._stateSignalsConnected)
		{
			value = VariantUtils.CreateFrom(in _stateSignalsConnected);
			return true;
		}
		if (name == PropertyName._cloverStarted)
		{
			value = VariantUtils.CreateFrom(in _cloverStarted);
			return true;
		}
		if (name == PropertyName._cloverBlowRemaining)
		{
			value = VariantUtils.CreateFrom(in _cloverBlowRemaining);
			return true;
		}
		if (name == PropertyName._cloverDeparturePending)
		{
			value = VariantUtils.CreateFrom(in _cloverDeparturePending);
			return true;
		}
		if (name == PropertyName._logicalBlowTween)
		{
			value = VariantUtils.CreateFrom(in _logicalBlowTween);
			return true;
		}
		if (name == PropertyName._blowInFlight)
		{
			value = VariantUtils.CreateFrom(in _blowInFlight);
			return true;
		}
		if (name == PropertyName._blowTarget)
		{
			value = VariantUtils.CreateFrom(in _blowTarget);
			return true;
		}
		if (name == PropertyName._blowRestorePending)
		{
			value = VariantUtils.CreateFrom(in _blowRestorePending);
			return true;
		}
		if (name == PropertyName.pop)
		{
			value = VariantUtils.CreateFrom(in pop);
			return true;
		}
		if (name == PropertyName.speed)
		{
			value = VariantUtils.CreateFrom(in speed);
			return true;
		}
		if (name == PropertyName.audioPlay)
		{
			value = VariantUtils.CreateFrom(in audioPlay);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._stateSignalsConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._cloverStarted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._cloverBlowRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._cloverDeparturePending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._logicalBlowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._blowInFlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._blowTarget, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._blowRestorePending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.pop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.audioPlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._stateSignalsConnected, Variant.From(in _stateSignalsConnected));
		info.AddProperty(PropertyName._cloverStarted, Variant.From(in _cloverStarted));
		info.AddProperty(PropertyName._cloverBlowRemaining, Variant.From(in _cloverBlowRemaining));
		info.AddProperty(PropertyName._cloverDeparturePending, Variant.From(in _cloverDeparturePending));
		info.AddProperty(PropertyName._logicalBlowTween, Variant.From(in _logicalBlowTween));
		info.AddProperty(PropertyName._blowInFlight, Variant.From(in _blowInFlight));
		info.AddProperty(PropertyName._blowTarget, Variant.From(in _blowTarget));
		info.AddProperty(PropertyName._blowRestorePending, Variant.From(in _blowRestorePending));
		info.AddProperty(PropertyName.pop, Variant.From(in pop));
		info.AddProperty(PropertyName.speed, Variant.From(in speed));
		info.AddProperty(PropertyName.audioPlay, Variant.From(in audioPlay));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._stateSignalsConnected, out var value))
		{
			_stateSignalsConnected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cloverStarted, out var value2))
		{
			_cloverStarted = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cloverBlowRemaining, out var value3))
		{
			_cloverBlowRemaining = value3.As<double>();
		}
		if (info.TryGetProperty(PropertyName._cloverDeparturePending, out var value4))
		{
			_cloverDeparturePending = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._logicalBlowTween, out var value5))
		{
			_logicalBlowTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._blowInFlight, out var value6))
		{
			_blowInFlight = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._blowTarget, out var value7))
		{
			_blowTarget = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._blowRestorePending, out var value8))
		{
			_blowRestorePending = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.pop, out var value9))
		{
			pop = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.speed, out var value10))
		{
			speed = value10.As<double>();
		}
		if (info.TryGetProperty(PropertyName.audioPlay, out var value11))
		{
			audioPlay = value11.As<bool>();
		}
	}
}
