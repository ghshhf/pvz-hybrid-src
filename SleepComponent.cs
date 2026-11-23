using System;
using Godot;
using Godot.Collections;

public sealed class SleepComponent : CharacterComponentRuntime
{
	public TowerDefenseCharacter parent;

	public AdobeAnimateSprite sleepSprite;

	private Tween _wakeTween;

	private float _restScaleY = 1f;

	private bool _restScaleCaptured;

	private bool _parentReadyConnected;

	private bool _hasSleepAnimation;

	private bool _scriptedSleepIndicatorActive;

	private Dictionary _pendingSyncData;

	private SleepComponentDefinition Definition => base.ComponentDefinition as SleepComponentDefinition;

	public bool BlocksPlayback
	{
		get
		{
			if (!base.IsReleased)
			{
				TowerDefenseCharacter towerDefenseCharacter = parent;
				if (towerDefenseCharacter != null && towerDefenseCharacter.instance?.sleep == true)
				{
					if (_hasSleepAnimation)
					{
						SleepComponentDefinition definition = Definition;
						if (definition == null || definition.freezeTimeInIZM)
						{
							return IsIzmMode();
						}
						return false;
					}
					return true;
				}
			}
			return false;
		}
	}

	private Dictionary _syncPayload => GetReusableSyncPayload();

	protected override void OnBound()
	{
		parent = base.Owner;
		PrepareParent();
	}

	protected override void OnActivated()
	{
		PrepareParent();
		if (!ApplyPendingSyncData())
		{
			RestoreAttachedSleepState();
		}
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		CleanupAttachedState();
		_pendingSyncData = null;
		ClearReusableSyncPayload();
		parent = null;
	}

	protected override void OnReleased()
	{
		CleanupAttachedState();
		_pendingSyncData = null;
		ClearReusableSyncPayload();
		parent = null;
	}

	protected override void OnAliveChanged(bool alive)
	{
		if (alive && !ApplyPendingSyncData())
		{
			RestoreAttachedSleepState();
		}
	}

	private void RestoreAttachedSleepState()
	{
		if (TryGetParent(out var character) && character.IsNodeReady() && character.instance.sleep)
		{
			SleepEntered();
		}
	}

	private void PrepareParent()
	{
		if (GodotObject.IsInstanceValid(parent))
		{
			if (parent.IsNodeReady())
			{
				CaptureRestScale();
			}
			else if (!_parentReadyConnected)
			{
				parent.Ready += ParentReady;
				_parentReadyConnected = true;
			}
		}
	}

	private void ParentReady()
	{
		DisconnectParentReady();
		CaptureRestScale();
		if (!ApplyPendingSyncData())
		{
			RestoreAttachedSleepState();
		}
	}

	private void CaptureRestScale()
	{
		DisconnectParentReady();
		if (GodotObject.IsInstanceValid(parent?.transformPoint))
		{
			_restScaleY = parent.transformPoint.Scale.Y;
			_restScaleCaptured = true;
		}
	}

	private void DisconnectParentReady()
	{
		if (_parentReadyConnected && GodotObject.IsInstanceValid(parent))
		{
			parent.Ready -= ParentReady;
		}
		_parentReadyConnected = false;
	}

	private void CleanupAttachedState()
	{
		DisconnectParentReady();
		_scriptedSleepIndicatorActive = false;
		CancelWakeTween(restoreScale: true);
		DisposeSleepIndicator();
	}

	public void SleepEntered()
	{
		if (TryGetParent(out var character))
		{
			bool num = GodotObject.IsInstanceValid(_wakeTween);
			CancelWakeTween(restoreScale: true);
			if (!num)
			{
				CaptureRestScale();
			}
			character.componentAlive = false;
			character.instance.wakeUp = false;
			EnsureSleepIndicator();
			character.instance.sleep = true;
			_hasSleepAnimation = !string.IsNullOrWhiteSpace(character.sleepAnimeClip) && character.sleepAnimeClip != character.idleAnimeClip && character.sprite.HasClip(character.sleepAnimeClip);
			if (_hasSleepAnimation)
			{
				character.sprite.SetAnimation(character.sleepAnimeClip, loop: true, Definition?.sleepAnimationBlend ?? 0.2);
			}
			character.OnSleepStateChanged();
			character.RefreshTimeScale();
		}
	}

	public void SleepProcessing(float _delta)
	{
		ReevaluateEnvironmentState();
	}

	private bool HasCellCoffee(TowerDefenseCharacter character)
	{
		SleepComponentDefinition definition = Definition;
		if ((definition == null || definition.coffeePreventsSleep) && GodotObject.IsInstanceValid(character.cell))
		{
			return character.cell.HasCoffee((int)character.camp);
		}
		return false;
	}

	public void ReevaluateEnvironmentState()
	{
		if (!TryGetParent(out var character) || IsRemoteSyncedClient(character))
		{
			return;
		}
		bool flag = character.CurrentStateHandle?.StableId == "character.sleep";
		if (CanSleep())
		{
			if (!character.IsSleep() || !flag)
			{
				character.Sleep();
			}
			else
			{
				EnsureSleepIndicator();
			}
			return;
		}
		if (HasCellCoffee(character))
		{
			character.instance.wakeUp = true;
		}
		if (!character.IsSleep() && !flag)
		{
			if (!_scriptedSleepIndicatorActive)
			{
				HideSleepIndicator();
			}
			return;
		}
		character.Idle();
		if (character.IsSleep())
		{
			SleepExited();
		}
		else
		{
			HideSleepIndicator();
		}
	}

	public void SleepExited()
	{
		if (TryGetParent(out var character))
		{
			character.componentAlive = true;
			CancelWakeTween(restoreScale: true);
			PlayWakeTween(character);
			HideSleepIndicator();
			character.instance.sleep = false;
			character.OnSleepStateChanged();
			character.RefreshTimeScale();
			if (character is TowerDefenseZombie { die: false } towerDefenseZombie)
			{
				towerDefenseZombie.Walk();
			}
		}
	}

	private void EnsureSleepIndicator()
	{
		PackedScene packedScene = Definition?.sleepIndicatorScene;
		if (GodotObject.IsInstanceValid(sleepSprite))
		{
			if (!sleepSprite.Visible || sleepSprite.pause)
			{
				sleepSprite.Visible = true;
				sleepSprite.pause = false;
				sleepSprite.timeScale = 1.0;
				sleepSprite.ResetAnimation();
				sleepSprite.RefreshProcessScheduling();
			}
		}
		else
		{
			if (!GodotObject.IsInstanceValid(packedScene) || !GodotObject.IsInstanceValid(parent?.spriteGroup))
			{
				return;
			}
			sleepSprite = packedScene.Instantiate(PackedScene.GenEditState.Disabled) as AdobeAnimateSprite;
			if (GodotObject.IsInstanceValid(sleepSprite))
			{
				sleepSprite.Visible = true;
				sleepSprite.pause = false;
				sleepSprite.timeScale = 1.0;
				AdobeAnimateSlot adobeAnimateSlot = null;
				if (parent is TowerDefenseZombie && GodotObject.IsInstanceValid(parent.headSlot))
				{
					parent.headSlot.Update();
					adobeAnimateSlot = parent.headSlot;
				}
				parent.AttachAnimatedStatusVisual(sleepSprite, adobeAnimateSlot, ResolveSleepIndicatorTransform(adobeAnimateSlot != null));
				sleepSprite.ResetAnimation();
				sleepSprite.RefreshProcessScheduling();
			}
		}
	}

	private Transform2D ResolveSleepIndicatorTransform(bool headAnchored)
	{
		Transform2D logicalGlobalTransform = parent.GetLogicalGlobalTransform(parent.spriteGroup);
		if (!headAnchored)
		{
			Vector2 origin = Definition?.sleepIndicatorPosition ?? new Vector2(20f, 25f);
			return logicalGlobalTransform * new Transform2D(0f, Vector2.One, 0f, origin);
		}
		logicalGlobalTransform.Origin = parent.GetLogicalGlobalPosition(parent.headSlot) + (Definition?.sleepIndicatorHeadPosition ?? new Vector2(32f, -28f));
		return logicalGlobalTransform;
	}

	public void ShowScriptedSleepIndicator()
	{
		_scriptedSleepIndicatorActive = true;
		EnsureSleepIndicator();
	}

	public void HideScriptedSleepIndicator()
	{
		if (_scriptedSleepIndicatorActive)
		{
			_scriptedSleepIndicatorActive = false;
			HideSleepIndicator();
		}
	}

	private void HideSleepIndicator()
	{
		if (GodotObject.IsInstanceValid(sleepSprite))
		{
			sleepSprite.pause = true;
			sleepSprite.timeScale = 0.0;
			sleepSprite.Visible = false;
			sleepSprite.RefreshProcessScheduling();
		}
	}

	private void DisposeSleepIndicator()
	{
		if (GodotObject.IsInstanceValid(sleepSprite))
		{
			if (GodotObject.IsInstanceValid(parent))
			{
				parent.DetachAnimatedStatusVisual(sleepSprite);
			}
			sleepSprite.QueueFree();
		}
		sleepSprite = null;
	}

	private void PlayWakeTween(TowerDefenseCharacter character)
	{
		if (GodotObject.IsInstanceValid(character.transformPoint))
		{
			float num = (_restScaleCaptured ? _restScaleY : character.transformPoint.Scale.Y);
			float num2 = Definition?.wakeStepDuration ?? 0.25f;
			if (num2 <= 0f)
			{
				Vector2 scale = character.transformPoint.Scale;
				character.transformPoint.Scale = new Vector2(scale.X, num);
				return;
			}
			_wakeTween = character.CreateTween();
			_wakeTween.SetEase(Definition?.wakeEase ?? Tween.EaseType.Out);
			_wakeTween.SetTrans(Definition?.wakeTransition ?? Tween.TransitionType.Quart);
			_wakeTween.TweenProperty(character.transformPoint, "scale:y", num - (Definition?.wakeSquashAmount ?? 0.25f), num2);
			_wakeTween.TweenProperty(character.transformPoint, "scale:y", num + (Definition?.wakeStretchAmount ?? 0.1f), num2);
			_wakeTween.TweenProperty(character.transformPoint, "scale:y", num, num2);
		}
	}

	private void CancelWakeTween(bool restoreScale)
	{
		bool flag = GodotObject.IsInstanceValid(_wakeTween);
		if (flag)
		{
			_wakeTween.Kill();
		}
		_wakeTween = null;
		if (restoreScale && flag && _restScaleCaptured && GodotObject.IsInstanceValid(parent?.transformPoint))
		{
			Vector2 scale = parent.transformPoint.Scale;
			parent.transformPoint.Scale = new Vector2(scale.X, _restScaleY);
		}
	}

	public bool CanSleep()
	{
		if (TryGetParent(out var character))
		{
			BuffComponent buff = character.buff;
			if (buff != null && !buff.IsReleased && GodotObject.IsInstanceValid(character.config))
			{
				if (IsRemoteSyncedClient(character))
				{
					return character.instance.sleep;
				}
				string text = Definition?.sleepBuffName ?? "Sleep";
				bool flag = !string.IsNullOrEmpty(text) && character.buff.BuffHas(text);
				if (character.instance.wakeUp && !flag)
				{
					return false;
				}
				bool flag2 = string.Equals(character.config.sleepTime, Definition?.neverSleepValue ?? "Never", StringComparison.OrdinalIgnoreCase);
				if (!flag && flag2)
				{
					return false;
				}
				SleepComponentDefinition definition = Definition;
				if ((definition == null || definition.mapRulesPreventSleep) && TowerDefenseProcessModeDispatch.SpecialRulesPreventSleepForCurrentPhysicsFrame)
				{
					return false;
				}
				bool result = ResolveConfiguredSleep(character);
				if (flag)
				{
					result = true;
				}
				if (!GodotObject.IsInstanceValid(TowerDefenseManager.GetMapFeature()) || !GodotObject.IsInstanceValid(character.cell))
				{
					return result;
				}
				SleepComponentDefinition definition2 = Definition;
				if ((definition2 == null || definition2.matchingCellElementPreventsSleep) && (character.cell.elementFlags & character.instance.elementFlags) != 0)
				{
					result = false;
				}
				if (HasCellCoffee(character))
				{
					result = false;
					if (flag)
					{
						character.buff.DeleteBuff(text);
					}
				}
				return result;
			}
		}
		return false;
	}

	private bool ResolveConfiguredSleep(TowerDefenseCharacter character)
	{
		string sleepTime = character.config.sleepTime;
		if (string.Equals(sleepTime, Definition?.neverSleepValue ?? "Never", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (string.Equals(sleepTime, Definition?.daySleepValue ?? "Day", StringComparison.OrdinalIgnoreCase))
		{
			if (TowerDefenseManager.GetMapIsNight())
			{
				return false;
			}
			if (GodotObject.IsInstanceValid(character.cell))
			{
				return (character.cell.elementFlags & 8) == 0;
			}
			return true;
		}
		if (string.Equals(sleepTime, Definition?.nightSleepValue ?? "Night", StringComparison.OrdinalIgnoreCase))
		{
			if (!TowerDefenseManager.GetMapIsNight())
			{
				return false;
			}
			if (GodotObject.IsInstanceValid(character.cell))
			{
				return (character.cell.elementFlags & 4) == 0;
			}
			return true;
		}
		return true;
	}

	private bool TryGetParent(out TowerDefenseCharacter character)
	{
		character = parent;
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && GodotObject.IsInstanceValid(character) && GodotObject.IsInstanceValid(character.instance))
		{
			return GodotObject.IsInstanceValid(character.sprite);
		}
		return false;
	}

	public override Dictionary SyncSerialize()
	{
		_syncPayload.Clear();
		if (!GodotObject.IsInstanceValid(parent?.instance) || parent.syncId < 0)
		{
			return _syncPayload;
		}
		_syncPayload["sleep"] = parent.instance.sleep;
		_syncPayload["wake"] = parent.instance.wakeUp;
		_syncPayload["component_alive"] = parent.componentAlive;
		_syncPayload["time_scale_init"] = parent.timeScaleInit;
		_syncPayload["time_scale_save"] = parent.timeScaleSave;
		return _syncPayload;
	}

	public override void SyncDeserialize(Dictionary data)
	{
		if (data != null)
		{
			if (!GodotObject.IsInstanceValid(parent) || !parent.IsNodeReady() || !TryGetParent(out var _))
			{
				_pendingSyncData = data.Duplicate(deep: true);
			}
			else
			{
				ApplySyncData(data);
			}
		}
	}

	private bool ApplyPendingSyncData()
	{
		if (_pendingSyncData == null || !TryGetParent(out var _))
		{
			return false;
		}
		Dictionary pendingSyncData = _pendingSyncData;
		_pendingSyncData = null;
		ApplySyncData(pendingSyncData);
		return true;
	}

	private void ApplySyncData(Dictionary data)
	{
		if (TryGetParent(out var character))
		{
			bool sleep = character.instance.sleep;
			bool flag = data.GetValueOrDefault("sleep", sleep).AsBool();
			character.instance.wakeUp = data.GetValueOrDefault("wake", character.instance.wakeUp).AsBool();
			if (flag && !sleep)
			{
				SleepEntered();
			}
			else if (!flag && sleep)
			{
				SleepExited();
			}
			else if (flag)
			{
				EnsureSleepIndicator();
				character.OnSleepStateChanged();
			}
			else
			{
				HideSleepIndicator();
				character.OnSleepStateChanged();
			}
			character.componentAlive = data.GetValueOrDefault("component_alive", character.componentAlive).AsBool();
			character.timeScaleInit = data.GetValueOrDefault("time_scale_init", character.timeScaleInit).AsDouble();
			character.timeScaleSave = data.GetValueOrDefault("time_scale_save", character.timeScaleSave).AsDouble();
			character.instance.sleep = flag;
			character.RefreshTimeScale();
		}
	}

	private bool IsRemoteSyncedClient(TowerDefenseCharacter character)
	{
		SleepComponentDefinition definition = Definition;
		if ((definition == null || definition.hostAuthoritativeState) && Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return character.syncId >= 0;
		}
		return false;
	}

	private static bool IsIzmMode()
	{
		if (GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			return TowerDefenseManager.Instance.IsIZMMode();
		}
		return false;
	}
}
