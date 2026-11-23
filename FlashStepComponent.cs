using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public sealed class FlashStepComponent : CharacterComponentRuntime, IProjectileZoneBinding, IProjectileZone
{
	public const string IdleStateId = "flash_step.idle";

	public const string FlashStateId = "flash_step.flash";

	public const string FellStateId = "flash_step.fell";

	public const string FellIdleStateId = "flash_step.fell_idle";

	public const string StandUpStateId = "flash_step.stand_up";

	private static readonly StringName IdleEvent = "ToIdle";

	private static readonly StringName FlashEvent = "ToFlash";

	private static readonly StringName FellEvent = "ToFell";

	private static readonly StringName FellIdleEvent = "ToFellIdle";

	private static readonly StringName StandUpEvent = "ToStandUp";

	private const string FlashPhaseName = "Flash";

	private const string FellPhaseName = "Fell";

	private const string FellIdlePhaseName = "FellIdle";

	private const string StandUpPhaseName = "StandUp";

	private const double FlashAnimationSpeedScale = 0.5;

	private const double FallAnimationSpeedScale = 0.75;

	private TowerDefenseZombie _parent;

	private BulletField _bulletField;

	private readonly HashSet<int> _dodgedBullets = new HashSet<int>();

	private readonly HashSet<TowerDefenseProjectile> _dodgedProjectiles = new HashSet<TowerDefenseProjectile>();

	private Rect2 _worldRect;

	private bool _nextBackward;

	private bool _backward;

	private bool _willFall;

	private bool _flashRequested;

	private bool _savedCollection;

	private bool _savedProjectileCheck;

	private bool _untargetableApplied;

	private bool _savedInvincible;

	private int _savedUnUseBuffFlags;

	private bool _hasRestoredImmunityValues;

	private double _fellRemaining;

	private float _spriteScaleX = 0.8f;

	private string _deathClip = "Death";

	private string _waterDeathClip = "Death";

	private StateHandle _idleState;

	private StateHandle _flashState;

	private StateHandle _fellState;

	private StateHandle _fellIdleState;

	private StateHandle _standUpState;

	private bool _stateSignalsConnected;

	private string _previousStateId = "flash_step.idle";

	protected override bool AllowStateMachineOutsideComponentBattlefield => true;

	public int FallChance { get; private set; } = 5;

	public bool IsBusy => IsBusyStateId(CurrentStateId);

	public bool IsFallen
	{
		get
		{
			string currentStateId = CurrentStateId;
			if (currentStateId == "flash_step.fell" || currentStateId == "flash_step.fell_idle")
			{
				return true;
			}
			return false;
		}
	}

	public bool IsFlashing
	{
		get
		{
			if (IsStateActive(_flashState))
			{
				TowerDefenseZombie parent = _parent;
				if (parent != null && !parent.die)
				{
					return !_parent.isDestroy;
				}
			}
			return false;
		}
	}

	internal override bool WantsPhysicsProcess => IsBusy;

	private FlashStepComponentDefinition Definition => (FlashStepComponentDefinition)base.ComponentDefinition;

	public Rect2 WorldRect => _worldRect;

	public int GridY
	{
		get
		{
			if (!GodotObject.IsInstanceValid(_parent))
			{
				return 0;
			}
			return _parent.gridPos.Y;
		}
	}

	public int RowSpan => 1;

	public string CurrentStateId
	{
		get
		{
			if (!IsStateActive(_standUpState))
			{
				if (!IsStateActive(_fellIdleState))
				{
					if (!IsStateActive(_fellState))
					{
						if (!IsStateActive(_flashState))
						{
							return "flash_step.idle";
						}
						return "flash_step.flash";
					}
					return "flash_step.fell";
				}
				return "flash_step.fell_idle";
			}
			return "flash_step.stand_up";
		}
	}

	private IFlashStepZombie FlashStepZombie => _parent as IFlashStepZombie;

	private static bool IsStateActive(StateHandle state)
	{
		return state?.IsActive ?? false;
	}

	private static bool IsBusyStateId(string stateId)
	{
		switch (stateId)
		{
		case "flash_step.flash":
		case "flash_step.fell":
		case "flash_step.fell_idle":
		case "flash_step.stand_up":
			return true;
		default:
			return false;
		}
	}

	private static string PhaseNameOf(string stateId)
	{
		return stateId switch
		{
			"flash_step.flash" => "Flash", 
			"flash_step.fell" => "Fell", 
			"flash_step.fell_idle" => "FellIdle", 
			"flash_step.stand_up" => "StandUp", 
			_ => string.Empty, 
		};
	}

	protected override void OnBound()
	{
		_parent = base.Owner as TowerDefenseZombie;
	}

	protected override void OnActivated()
	{
		_spriteScaleX = Mathf.Abs(_parent.sprite.Scale.X);
		_deathClip = _parent.dieAnimeClip;
		_waterDeathClip = _parent.dieWaterAnimeClip;
		BindBulletField(BulletField.Instance);
		ApplyPresentation(playAnimation: false, CurrentStateId);
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		BindBulletField(null);
		RestoreImmunity();
		_hasRestoredImmunityValues = false;
		_parent = null;
		_worldRect = default(Rect2);
	}

	protected override void OnStateRuntimeAttached()
	{
		_idleState = base.StateMachine?.GetStateById("flash_step.idle");
		_flashState = base.StateMachine?.GetStateById("flash_step.flash");
		_fellState = base.StateMachine?.GetStateById("flash_step.fell");
		_fellIdleState = base.StateMachine?.GetStateById("flash_step.fell_idle");
		_standUpState = base.StateMachine?.GetStateById("flash_step.stand_up");
		ConnectStateSignals();
	}

	protected override void OnStateRuntimeDetaching()
	{
		_flashRequested = false;
		DisconnectStateSignals();
		_idleState = null;
		_flashState = null;
		_fellState = null;
		_fellIdleState = null;
		_standUpState = null;
	}

	protected override void OnAuthoritativeStateRestorePreparing(bool remote)
	{
		_flashRequested = false;
		_previousStateId = CurrentStateId;
		RestoreImmunity();
	}

	protected override void OnAuthoritativeStateRestored(StateMachineSnapshot snapshot, bool remote)
	{
		string currentStateId = CurrentStateId;
		ApplyPresentation(_previousStateId != currentStateId, currentStateId);
		if (_previousStateId != "flash_step.idle" && !IsBusy)
		{
			ResetFacingAndDirection();
		}
		RefreshPhysicsProcessEligibility();
	}

	private void ConnectStateSignals()
	{
		if (!_stateSignalsConnected)
		{
			ConnectState(_flashState, FlashEntered);
			ConnectState(_fellState, FellEntered);
			ConnectState(_fellIdleState, FellIdleEntered);
			ConnectState(_standUpState, StandUpEntered);
			_stateSignalsConnected = true;
		}
	}

	private void DisconnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			DisconnectState(_flashState, FlashEntered);
			DisconnectState(_fellState, FellEntered);
			DisconnectState(_fellIdleState, FellIdleEntered);
			DisconnectState(_standUpState, StandUpEntered);
			_stateSignalsConnected = false;
		}
	}

	private static void ConnectState(StateHandle state, Action entered)
	{
		if (state != null && state.IsValid)
		{
			state.Entered += entered;
		}
	}

	private static void DisconnectState(StateHandle state, Action entered)
	{
		if (state != null)
		{
			state.Entered -= entered;
		}
	}

	public void FlashEntered()
	{
		_flashRequested = false;
		EnterPhase("flash_step.flash");
	}

	public void FellEntered()
	{
		EnterPhase("flash_step.fell");
	}

	public void FellIdleEntered()
	{
		EnterPhase("flash_step.fell_idle");
	}

	public void StandUpEntered()
	{
		EnterPhase("flash_step.stand_up");
	}

	private void EnterPhase(string stateId)
	{
		_parent?.SendStateEvent("ToFlash" + PhaseNameOf(stateId));
		ApplyPresentation(playAnimation: true, stateId);
		RefreshPhysicsProcessEligibility();
	}

	public void BindBulletField(BulletField bulletField)
	{
		if (_bulletField != bulletField)
		{
			if (GodotObject.IsInstanceValid(_bulletField))
			{
				_bulletField.UnregisterZone(this);
				_bulletField.OnBulletSpawned -= OnBulletSpawned;
			}
			_dodgedBullets.Clear();
			_dodgedProjectiles.Clear();
			_bulletField = (GodotObject.IsInstanceValid(bulletField) ? bulletField : null);
			if (_bulletField != null)
			{
				_bulletField.OnBulletSpawned += OnBulletSpawned;
				_bulletField.RegisterZone(this);
			}
		}
	}

	private void OnBulletSpawned(int index)
	{
		_dodgedBullets.Remove(index);
	}

	public void UpdateRect()
	{
		_dodgedProjectiles.RemoveWhere((TowerDefenseProjectile projectile) => !GodotObject.IsInstanceValid(projectile) || projectile.over || projectile.hitOver);
		_worldRect = default(Rect2);
		if (CanStartStep())
		{
			float projectileDetectionRadius = Definition.projectileDetectionRadius;
			Vector2 vector = _parent.GetLogicalGlobalPosition() + new Vector2(0f, -35f);
			_worldRect = new Rect2(vector - Vector2.One * projectileDetectionRadius, Vector2.One * projectileDetectionRadius * 2f);
		}
	}

	public void OnBulletIntersect(ref BulletData bullet, int index)
	{
		if (bullet.active && !bullet.over && bullet.camp != _parent?.camp && !_dodgedBullets.Contains(index) && TryFlash())
		{
			_dodgedBullets.Add(index);
		}
	}

	public void OnProjectileIntersect(TowerDefenseProjectile projectile)
	{
		if (GodotObject.IsInstanceValid(projectile) && !projectile.over && !projectile.hitOver && projectile.camp != _parent?.camp && !_dodgedProjectiles.Contains(projectile) && TryFlash())
		{
			_dodgedProjectiles.Add(projectile);
		}
	}

	private bool CanStartStep()
	{
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && TowerDefenseManager.HasGameplayAuthority && GodotObject.IsInstanceValid(_parent) && _parent.inGame && !_parent.isRise && _parent.CanRunAbility() && base.CanExecuteGameplay)
		{
			IStateMachineController stateMachine = base.StateMachine;
			if (stateMachine != null && stateMachine.IsInitialized && !_flashRequested && !IsBusy)
			{
				return FlashStepZombie?.CanFlashStep ?? false;
			}
		}
		return false;
	}

	public bool TryFlash()
	{
		if (!CanStartStep())
		{
			return false;
		}
		_flashRequested = true;
		bool backward = _backward;
		bool willFall = _willFall;
		int fallChance = FallChance;
		_backward = _nextBackward;
		_nextBackward = !_nextBackward;
		_willFall = GD.RandRange(1, 100) <= FallChance;
		if (!_willFall)
		{
			FallChance = Mathf.Min(100, FallChance + 5);
		}
		if (SendStateEvent(FlashEvent))
		{
			return true;
		}
		_nextBackward = !_nextBackward;
		_backward = backward;
		_willFall = willFall;
		FallChance = fallChance;
		_flashRequested = false;
		return false;
	}

	public void Fall()
	{
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && TowerDefenseManager.HasGameplayAuthority && GodotObject.IsInstanceValid(_parent) && !_parent.die && !_parent.nearDie && !_parent.isDestroy && !IsFallen)
		{
			if (!IsBusy)
			{
				_backward = false;
			}
			FallChance = 5;
			_fellRemaining = Definition.fellDuration;
			SendStateEvent(FellEvent);
			FlashStepZombie?.OnFlashStepFell();
		}
	}

	public bool AnimeCompleted(string clip)
	{
		if (!IsBusy)
		{
			return false;
		}
		if (!base.Alive || base.Lifecycle != ComponentRuntimeLifecycle.Active || _parent.die || _parent.IsRemoteNetworkReplica || !TowerDefenseManager.HasGameplayAuthority)
		{
			return true;
		}
		if (_parent.nearDie)
		{
			_parent.Die();
			return true;
		}
		if (clip != PhaseNameOf(CurrentStateId))
		{
			return true;
		}
		switch (CurrentStateId)
		{
		case "flash_step.flash":
			if (_willFall)
			{
				Fall();
			}
			else
			{
				Finish();
			}
			break;
		case "flash_step.fell":
			SendStateEvent(FellIdleEvent);
			break;
		case "flash_step.stand_up":
			Finish();
			break;
		}
		return true;
	}

	protected override void OnPhysicsProcess(double delta, ulong physicsFrame)
	{
		if (!GodotObject.IsInstanceValid(_parent) || _parent.die || _parent.isDestroy)
		{
			return;
		}
		ApplyAnimationSpeed(CurrentStateId);
		if (TowerDefenseManager.HasGameplayAuthority && _parent.CanRunAbility() && IsFallen)
		{
			_fellRemaining = Mathf.Max(0.0, _fellRemaining - delta * _parent.timeScale);
			if (_fellRemaining <= 0.0 && CurrentStateId == "flash_step.fell_idle")
			{
				SendStateEvent(StandUpEvent);
			}
		}
	}

	private void Finish()
	{
		SendStateEvent(IdleEvent);
		_backward = false;
		RestoreImmunity();
		ResetFacingAndDirection();
		_parent.dieAnimeClip = _deathClip;
		_parent.dieWaterAnimeClip = _waterDeathClip;
		ApplyAnimationSpeed("flash_step.idle");
		RefreshPhysicsProcessEligibility();
		_parent.Walk();
	}

	private void ApplyAnimationSpeed(string stateId)
	{
		double num;
		switch (stateId)
		{
		case "flash_step.flash":
			num = 0.5;
			break;
		case "flash_step.fell":
		case "flash_step.fell_idle":
		case "flash_step.stand_up":
			num = 0.75;
			break;
		default:
			num = 1.0;
			break;
		}
		double num2 = num;
		_parent.sprite.timeScale = _parent.timeScale * num2;
	}

	private void ResetFacingAndDirection()
	{
		if (GodotObject.IsInstanceValid(_parent?.sprite))
		{
			_parent.sprite.Scale = new Vector2(_spriteScaleX, _parent.sprite.Scale.Y);
			_parent.groundMoveComponent.RefreshDirectionCache();
		}
	}

	private void ApplyPresentation(bool playAnimation, string stateId)
	{
		if (!GodotObject.IsInstanceValid(_parent?.sprite))
		{
			return;
		}
		ApplyAnimationSpeed(stateId);
		if (!IsBusyStateId(stateId))
		{
			if (_parent.IsPhantomUntargetable)
			{
				ApplyImmunity();
			}
			else
			{
				RestoreImmunity();
			}
			_hasRestoredImmunityValues = false;
			_parent.dieAnimeClip = _deathClip;
			_parent.dieWaterAnimeClip = _waterDeathClip;
			return;
		}
		bool flag = stateId == "flash_step.flash";
		bool flag2 = ((stateId == "flash_step.fell" || stateId == "flash_step.fell_idle") ? true : false);
		bool flag3 = flag2;
		if (flag)
		{
			ApplyImmunity();
		}
		else
		{
			RestoreImmunity();
		}
		_hasRestoredImmunityValues = false;
		_parent.sprite.Scale = new Vector2(_backward ? (0f - _spriteScaleX) : _spriteScaleX, _parent.sprite.Scale.Y);
		_parent.groundMoveComponent.RefreshDirectionCache();
		_parent.groundMoveComponent.SetAlive(flag);
		if (playAnimation)
		{
			_parent.sprite.SetAnimation(PhaseNameOf(stateId), stateId == "flash_step.fell_idle");
		}
		_parent.dieAnimeClip = (flag3 ? "FellDeath" : _deathClip);
		_parent.dieWaterAnimeClip = (flag3 ? "FellDeath" : _waterDeathClip);
	}

	public void RestoreImmunity()
	{
		if (_untargetableApplied && GodotObject.IsInstanceValid(_parent?.instance))
		{
			_parent.instance.canBeCollection = _savedCollection;
			_parent.instance.invincible = _savedInvincible;
			_parent.instance.unUseBuffFlags = _savedUnUseBuffFlags;
			TargetRegistrationComponent targetRegistrationComponent = _parent.targetRegistrationComponent;
			if (targetRegistrationComponent != null && !targetRegistrationComponent.IsReleased)
			{
				_parent.targetRegistrationComponent.canProjectileCheck = _savedProjectileCheck;
			}
			_untargetableApplied = false;
		}
	}

	public void ApplyImmunity()
	{
		if (!_untargetableApplied && GodotObject.IsInstanceValid(_parent?.instance))
		{
			if (!_hasRestoredImmunityValues)
			{
				_savedCollection = _parent.instance.canBeCollection;
				_savedProjectileCheck = _parent.targetRegistrationComponent?.canProjectileCheck ?? true;
				_savedInvincible = _parent.instance.invincible;
				_savedUnUseBuffFlags = _parent.instance.unUseBuffFlags;
			}
			_hasRestoredImmunityValues = false;
			_untargetableApplied = true;
			_parent.instance.canBeCollection = false;
			_parent.instance.invincible = true;
			_parent.instance.unUseBuffFlags = -1;
			TargetRegistrationComponent targetRegistrationComponent = _parent.targetRegistrationComponent;
			if (targetRegistrationComponent != null && !targetRegistrationComponent.IsReleased)
			{
				_parent.targetRegistrationComponent.canProjectileCheck = false;
			}
		}
	}

	public void PrepareDeath()
	{
		if (IsFallen)
		{
			TowerDefenseZombie parent = _parent;
			string dieAnimeClip = (_parent.dieWaterAnimeClip = "FellDeath");
			parent.dieAnimeClip = dieAnimeClip;
		}
		RestoreImmunity();
	}

	public StringName NormalizeEvent(StringName eventName)
	{
		if (!IsBusy || eventName == (StringName)"ToDie" || eventName.ToString().StartsWith("ToFlash"))
		{
			return eventName;
		}
		return "FlashBusy";
	}

	public override Dictionary ExportComponentSave()
	{
		return Serialize();
	}

	public override Dictionary SyncSerialize()
	{
		return Serialize();
	}

	private Dictionary Serialize()
	{
		Dictionary reusableSyncPayload = GetReusableSyncPayload();
		string currentStateId = CurrentStateId;
		reusableSyncPayload["state"] = currentStateId;
		reusableSyncPayload["fallChance"] = FallChance;
		reusableSyncPayload["nextBackward"] = _nextBackward;
		reusableSyncPayload["backward"] = _backward;
		reusableSyncPayload["willFall"] = _willFall;
		reusableSyncPayload["fellRemaining"] = _fellRemaining;
		reusableSyncPayload["savedCollection"] = (_untargetableApplied ? _savedCollection : _parent.instance.canBeCollection);
		reusableSyncPayload["savedProjectileCheck"] = (_untargetableApplied ? _savedProjectileCheck : _parent.targetRegistrationComponent.canProjectileCheck);
		reusableSyncPayload["savedInvincible"] = (_untargetableApplied ? _savedInvincible : _parent.instance.invincible);
		reusableSyncPayload["savedUnUseBuffFlags"] = (_untargetableApplied ? _savedUnUseBuffFlags : _parent.instance.unUseBuffFlags);
		return reusableSyncPayload;
	}

	public override void ImportComponentSave(Dictionary data, TowerDefenseLevelSaveConfigCSharp owner)
	{
		Restore(data);
	}

	public override void SyncDeserialize(Dictionary data)
	{
		Restore(data);
	}

	private void Restore(Dictionary data)
	{
		FallChance = Mathf.Clamp(data.GetValueOrDefault("fallChance", 5).AsInt32(), 5, 100);
		_nextBackward = data.GetValueOrDefault("nextBackward", false).AsBool();
		_backward = data.GetValueOrDefault("backward", false).AsBool();
		_willFall = data.GetValueOrDefault("willFall", false).AsBool();
		double num = data.GetValueOrDefault("fellRemaining", 0.0).AsDouble();
		_fellRemaining = (double.IsFinite(num) ? Mathf.Clamp(num, 0.0, Definition.fellDuration) : Definition.fellDuration);
		if (data.ContainsKey("savedCollection"))
		{
			if (base.ShouldApplyLegacyStateField)
			{
				RestoreImmunity();
			}
			_savedCollection = data["savedCollection"].AsBool();
			_savedProjectileCheck = data.GetValueOrDefault("savedProjectileCheck", true).AsBool();
			_savedInvincible = data.GetValueOrDefault("savedInvincible", false).AsBool();
			_savedUnUseBuffFlags = data.GetValueOrDefault("savedUnUseBuffFlags", _parent.config.unUseBuffFlags).AsInt32();
			_hasRestoredImmunityValues = true;
		}
		if (base.ShouldApplyLegacyStateField)
		{
			string currentStateId = CurrentStateId;
			string text = data.GetValueOrDefault("state", "").AsString();
			if (!string.IsNullOrEmpty(text))
			{
				SyncForceState(base.StateMachine, text, suppressEntryEffects: true);
			}
			string currentStateId2 = CurrentStateId;
			ApplyPresentation(currentStateId != currentStateId2, currentStateId2);
			if (currentStateId != "flash_step.idle" && !IsBusy)
			{
				ResetFacingAndDirection();
			}
			RefreshPhysicsProcessEligibility();
		}
	}

	private static bool SyncForceState(IStateMachineController state, string targetState, bool suppressEntryEffects)
	{
		if (state == null || !state.IsInitialized || string.IsNullOrEmpty(targetState))
		{
			return false;
		}
		StateHandle currentStateHandle = state.CurrentStateHandle;
		StateHandle stateHandle = state.ResolveState(targetState, allowDisplayNameFallback: true);
		if (stateHandle == null || !stateHandle.IsValid)
		{
			return false;
		}
		if (currentStateHandle != null && currentStateHandle.IsValid && currentStateHandle.StableId == stateHandle.StableId)
		{
			return true;
		}
		StateMachineSnapshot stateMachineSnapshot = state.CaptureSnapshot();
		if (stateMachineSnapshot == null)
		{
			return false;
		}
		stateMachineSnapshot.ActiveStateIds.Clear();
		stateMachineSnapshot.ActiveStateIds.Add(stateHandle.StableId);
		stateMachineSnapshot.PendingTransitionIds.Clear();
		stateMachineSnapshot.PendingDelayRemaining.Clear();
		return state.RestoreSnapshot(stateMachineSnapshot, suppressEntryEffects);
	}
}
