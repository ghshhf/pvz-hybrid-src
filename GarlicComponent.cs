using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public sealed class GarlicComponent : CharacterComponentRuntime
{
	public Texture2D grossoutTexture;

	public float reactionDelay = 0.5f;

	public float grossoutDuration = 0.5f;

	public float changeLineDuration = 1f;

	public float moveDownChance = 0.5f;

	public string reactionAudio = "Yuck";

	public StringName garlicStateEvent = "ToGarlic";

	public Tween.EaseType changeLineEase = Tween.EaseType.InOut;

	public Tween.TransitionType changeLineTransition;

	public TowerDefenseZombie parent;

	public int _syncMoveDir;

	public bool _syncDeserializing;

	private static readonly StringName SyncAliveKey = new StringName("_alive");

	private bool _syncPayloadInitialized;

	private int _syncPayloadMoveDir;

	private bool _syncPayloadIsChangeLine;

	private Tween _changeLineTween;

	private TaskCompletionSource<bool> _changeLineCompletion;

	private Action _changeLineFinishedHandler;

	private ulong _changeLineVersion;

	private ulong _garlicVersion;

	private bool _garlicRunning;

	private Texture2D _savedReplacement;

	private string _savedAtlasReplacementPath = string.Empty;

	private bool[] _savedFilterStates = System.Array.Empty<bool>();

	private bool _grossoutApplied;

	private bool _configured;

	private int _garlicStage;

	private SceneTreeTimer _garlicWaitTimer;

	private float _changeLineTargetY;

	private double _changeLineTotalDuration;

	private Dictionary _pendingProgress;

	private Dictionary _syncPayload => GetReusableSyncPayload();

	internal override bool WantsPhysicsProcess => _pendingProgress != null;

	protected override bool AllowPhysicsOutsideComponentBattlefield => true;

	private GarlicComponentDefinition Definition => base.ComponentDefinition as GarlicComponentDefinition;

	protected override void OnBound()
	{
		parent = base.Owner as TowerDefenseZombie;
		ApplyDefinition();
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		CancelActiveWork();
		ClearSyncPayload();
		parent = null;
		if (reason != ComponentDetachReason.TemporaryTreeExit)
		{
			ResetPersistentState();
		}
	}

	protected override void OnReleased()
	{
		CancelActiveWork();
		ClearSyncPayload();
		parent = null;
		ResetPersistentState();
		grossoutTexture = null;
		_configured = false;
	}

	private void ApplyDefinition()
	{
		if (!_configured && Definition != null)
		{
			grossoutTexture = Definition.grossoutTexture;
			reactionDelay = Definition.reactionDelay;
			grossoutDuration = Definition.grossoutDuration;
			changeLineDuration = Definition.changeLineDuration;
			moveDownChance = Definition.moveDownChance;
			reactionAudio = Definition.reactionAudio;
			garlicStateEvent = Definition.garlicStateEvent;
			changeLineEase = Definition.changeLineEase;
			changeLineTransition = Definition.changeLineTransition;
			_configured = true;
		}
	}

	private void CancelActiveWork()
	{
		_pendingProgress = null;
		_garlicStage = 0;
		_garlicWaitTimer = null;
		RefreshPhysicsProcessEligibility();
		_garlicVersion++;
		RestoreGrossoutVisual();
		CancelChangeLine();
		if (GodotObject.IsInstanceValid(parent))
		{
			parent.isGarlic = false;
		}
		_garlicRunning = false;
	}

	private void ResetPersistentState()
	{
		_syncMoveDir = 0;
		_syncDeserializing = false;
	}

	public void Garlic()
	{
		if (TryGetParent(out var zombie) && !zombie.isPause && !_garlicRunning && !zombie.isGarlic)
		{
			if (zombie.nearDie || zombie.die)
			{
				zombie.Die();
			}
			else if ((zombie.instance.unUseBuffFlags & 0x80) == 0)
			{
				_garlicRunning = true;
				GarlicAsync(++_garlicVersion);
			}
		}
	}

	private async Task GarlicAsync(ulong version, int restoredStage = 0, double remaining = 0.0)
	{
		_ = 3;
		try
		{
			TowerDefenseZombie zombie;
			while (TryGetParent(out zombie) && zombie.isRise)
			{
				SceneTree tree = zombie.GetTree();
				if (!GodotObject.IsInstanceValid(tree))
				{
					return;
				}
				await zombie.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
				if (version != _garlicVersion)
				{
					return;
				}
			}
			if (!CanContinueGarlic(version, out zombie))
			{
				return;
			}
			zombie.isGarlic = true;
			if (restoredStage == 0 && (zombie.StateMachine?.IsInitialized ?? false) && !garlicStateEvent.IsEmpty)
			{
				zombie.SendStateEvent(garlicStateEvent);
			}
			if (restoredStage < 2)
			{
				_garlicStage = 1;
				if (!(await WaitDelayAsync(version, (restoredStage == 1) ? ((float)remaining) : reactionDelay)) || !CanContinueGarlic(version, out zombie))
				{
					return;
				}
				if (!string.IsNullOrEmpty(reactionAudio) && GodotObject.IsInstanceValid(AudioManager.Instance))
				{
					AudioManager.Instance.AudioPlay(reactionAudio);
				}
			}
			_garlicStage = 2;
			ApplyGrossoutVisual(zombie);
			if (await WaitDelayAsync(version, (restoredStage == 2) ? ((float)remaining) : grossoutDuration) && CanContinueGarlic(version, out zombie))
			{
				RestoreGrossoutVisual();
				zombie.Walk();
				await ChangeLine();
			}
		}
		finally
		{
			if (version == _garlicVersion)
			{
				RestoreGrossoutVisual();
				_garlicStage = 0;
				if (GodotObject.IsInstanceValid(parent))
				{
					parent.isGarlic = false;
				}
				_garlicRunning = false;
			}
		}
	}

	private bool CanContinueGarlic(ulong version, out TowerDefenseZombie zombie)
	{
		zombie = null;
		if (version == _garlicVersion && TryGetParent(out zombie) && !zombie.nearDie)
		{
			return !zombie.die;
		}
		return false;
	}

	private async Task<bool> WaitDelayAsync(ulong version, float seconds)
	{
		if (seconds <= 0f)
		{
			return version == _garlicVersion;
		}
		if (!TryGetParent(out var zombie))
		{
			return false;
		}
		SceneTree tree = zombie.GetTree();
		if (!GodotObject.IsInstanceValid(tree))
		{
			return false;
		}
		SceneTreeTimer source = (_garlicWaitTimer = tree.CreateTimer(seconds, processAlways: false));
		await zombie.ToSignal(source, SceneTreeTimer.SignalName.Timeout);
		if (version == _garlicVersion)
		{
			_garlicWaitTimer = null;
		}
		TowerDefenseZombie zombie2;
		return version == _garlicVersion && TryGetParent(out zombie2);
	}

	private void ApplyGrossoutVisual(TowerDefenseZombie zombie)
	{
		if (!_grossoutApplied && GodotObject.IsInstanceValid(zombie.sprite))
		{
			int num = zombie.garlicFliters?.Count ?? 0;
			_savedFilterStates = new bool[num];
			for (int i = 0; i < num; i++)
			{
				_savedFilterStates[i] = zombie.sprite.GetFliter(zombie.garlicFliters[i]);
			}
			_savedReplacement = (string.IsNullOrEmpty(zombie.garlicReplace) ? null : zombie.sprite.GetReplace(zombie.garlicReplace));
			_savedAtlasReplacementPath = (string.IsNullOrEmpty(zombie.garlicReplace) ? string.Empty : zombie.sprite.GetAtlasReplacePath(zombie.garlicReplace));
			if (!string.IsNullOrEmpty(zombie.garlicReplace) && GodotObject.IsInstanceValid(grossoutTexture))
			{
				zombie.sprite.SetReplace(zombie.garlicReplace, grossoutTexture);
			}
			if (num > 0)
			{
				zombie.sprite.SetFliters((Godot.Collections.Array?)zombie.garlicFliters, open: false);
			}
			_grossoutApplied = true;
		}
	}

	private void RestoreGrossoutVisual()
	{
		if (!_grossoutApplied)
		{
			return;
		}
		_grossoutApplied = false;
		TowerDefenseZombie towerDefenseZombie = parent;
		if (!GodotObject.IsInstanceValid(towerDefenseZombie?.sprite))
		{
			_savedReplacement = null;
			_savedAtlasReplacementPath = string.Empty;
			_savedFilterStates = System.Array.Empty<bool>();
			return;
		}
		if (!string.IsNullOrEmpty(towerDefenseZombie.garlicReplace))
		{
			if (!string.IsNullOrEmpty(_savedAtlasReplacementPath))
			{
				towerDefenseZombie.sprite.SetAtlasReplace(towerDefenseZombie.garlicReplace, _savedAtlasReplacementPath);
			}
			else
			{
				towerDefenseZombie.sprite.SetReplace(towerDefenseZombie.garlicReplace, _savedReplacement);
			}
		}
		int num = Mathf.Min(_savedFilterStates.Length, towerDefenseZombie.garlicFliters?.Count ?? 0);
		for (int i = 0; i < num; i++)
		{
			if (_savedFilterStates[i])
			{
				towerDefenseZombie.sprite.SetFliter(towerDefenseZombie.garlicFliters[i], open: true);
			}
		}
		_savedReplacement = null;
		_savedAtlasReplacementPath = string.Empty;
		_savedFilterStates = System.Array.Empty<bool>();
	}

	public Task ChangeLine()
	{
		if (!TryGetParent(out var zombie) || zombie.isChangeLine || !GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			return Task.CompletedTask;
		}
		int num = ResolveMoveDirection(zombie);
		if (num == 0)
		{
			return Task.CompletedTask;
		}
		float targetY = zombie.GetLogicalGlobalPosition().Y + TowerDefenseManager.Instance.GetMapGridSize().Y * (float)num;
		return MoveToLineAsync(zombie, targetY, Mathf.Max(0.001f, changeLineDuration), num);
	}

	private async Task MoveToLineAsync(TowerDefenseZombie zombie, float targetY, double remaining, int moveDirection)
	{
		ulong version = ++_changeLineVersion;
		zombie.isChangeLine = true;
		_garlicStage = 3;
		_changeLineTargetY = targetY;
		_changeLineTotalDuration = remaining;
		try
		{
			float num = targetY - zombie.GetLogicalGlobalPosition().Y;
			float num2 = (float)Math.Max(0.001, remaining);
			_changeLineTween = zombie.CreateTween();
			_changeLineTween.SetParallel();
			_changeLineTween.SetEase(changeLineEase);
			_changeLineTween.SetTrans(changeLineTransition);
			_changeLineCompletion = new TaskCompletionSource<bool>();
			_changeLineFinishedHandler = delegate
			{
				_changeLineCompletion?.TrySetResult(result: true);
			};
			_changeLineTween.Finished += _changeLineFinishedHandler;
			float y = zombie.GetLogicalGlobalPosition().Y;
			_changeLineTween.TweenMethod(Callable.From(delegate(float value)
			{
				SetLogicalGlobalPositionY(zombie, value);
			}), y, y + num, num2);
			ShadowComponent shadowComponent = zombie.shadowComponent;
			if (shadowComponent != null && !shadowComponent.IsReleased)
			{
				zombie.shadowComponent.TweenSaveShadowPositionY(_changeLineTween, zombie.shadowComponent.saveShadowPosition.Y + num, num2);
			}
			TowerDefenseCharacter dynamicCharacter = GetDynamicCharacter(zombie, "carryCharacter");
			AddCompanionTween(dynamicCharacter, num, moveDirection, num2);
			TowerDefenseCharacter ghostCharacter = zombie.ghostCharacter;
			if (ghostCharacter != dynamicCharacter)
			{
				AddCompanionTween(ghostCharacter, num, moveDirection, num2);
			}
			RefreshChangeLineDiscard(zombie);
			zombie.gridPos = new Vector2I(zombie.gridPos.X, zombie.gridPos.Y + moveDirection);
			if (await _changeLineCompletion.Task && version == _changeLineVersion && TryGetParent(out zombie))
			{
				CleanupChangeLineTween(killTween: false);
				RefreshChangeLineDiscard(zombie);
			}
		}
		finally
		{
			if (version == _changeLineVersion && GodotObject.IsInstanceValid(parent))
			{
				parent.isChangeLine = false;
				_garlicStage = 0;
			}
		}
	}

	private int ResolveMoveDirection(TowerDefenseZombie zombie)
	{
		Vector2 vector = TowerDefenseManager.Instance.GetMapGridNum();
		int num = ((zombie.gridPos.Y <= 1) ? 1 : ((zombie.gridPos.Y >= (int)vector.Y) ? (-1) : ((!_syncDeserializing || _syncMoveDir == 0) ? ((GD.Randf() < Mathf.Clamp(moveDownChance, 0f, 1f)) ? 1 : (-1)) : _syncMoveDir)));
		_syncDeserializing = false;
		_syncMoveDir = num;
		return num;
	}

	private void AddCompanionTween(TowerDefenseCharacter companion, float moveY, int moveDirection, float duration)
	{
		if (GodotObject.IsInstanceValid(companion) && GodotObject.IsInstanceValid(_changeLineTween))
		{
			float y = companion.GetLogicalGlobalPosition().Y;
			_changeLineTween.TweenMethod(Callable.From(delegate(float value)
			{
				SetLogicalGlobalPositionY(companion, value);
			}), y, y + moveY, duration);
			companion.gridPos = new Vector2I(companion.gridPos.X, companion.gridPos.Y + moveDirection);
		}
	}

	private static void SetLogicalGlobalPositionY(TowerDefenseCharacter character, float value)
	{
		if (GodotObject.IsInstanceValid(character))
		{
			Vector2 logicalGlobalPosition = character.GetLogicalGlobalPosition();
			logicalGlobalPosition.Y = value;
			character.SetLogicalGlobalPosition(logicalGlobalPosition);
			if (character is TowerDefenseZombie zombie)
			{
				RefreshChangeLineDiscard(zombie);
			}
		}
	}

	private static TowerDefenseCharacter GetDynamicCharacter(TowerDefenseZombie zombie, StringName property)
	{
		Variant variant = zombie.Get(property);
		if (variant.VariantType != Variant.Type.Object)
		{
			return null;
		}
		return variant.AsGodotObject() as TowerDefenseCharacter;
	}

	private static void RefreshChangeLineDiscard(TowerDefenseZombie zombie)
	{
		if (zombie.inWater)
		{
			zombie.InWaterDiscardSet();
		}
		else
		{
			zombie.OutWaterDiscardSet();
		}
	}

	public void CancelChangeLine()
	{
		_changeLineVersion++;
		_changeLineCompletion?.TrySetResult(result: false);
		CleanupChangeLineTween(killTween: true);
		if (GodotObject.IsInstanceValid(parent))
		{
			RefreshChangeLineDiscard(parent);
			parent.isChangeLine = false;
		}
	}

	private void CleanupChangeLineTween(bool killTween)
	{
		if (GodotObject.IsInstanceValid(_changeLineTween) && _changeLineFinishedHandler != null)
		{
			_changeLineTween.Finished -= _changeLineFinishedHandler;
		}
		if (killTween && GodotObject.IsInstanceValid(_changeLineTween))
		{
			_changeLineTween.Kill();
		}
		_changeLineTween = null;
		_changeLineFinishedHandler = null;
		_changeLineCompletion = null;
	}

	public override Dictionary SyncSerialize()
	{
		bool flag = GodotObject.IsInstanceValid(parent) && parent.isChangeLine;
		if (_syncPayloadInitialized)
		{
			if (_syncPayload.Count == 3)
			{
				_syncPayload.Remove(SyncAliveKey);
			}
			if (_syncPayload.Count == 2 && _syncPayloadMoveDir == _syncMoveDir && _syncPayloadIsChangeLine == flag)
			{
				return _syncPayload;
			}
		}
		ClearReusableSyncPayload();
		_syncPayload["move_dir"] = _syncMoveDir;
		_syncPayload["is_change_line"] = flag;
		_syncPayloadMoveDir = _syncMoveDir;
		_syncPayloadIsChangeLine = flag;
		_syncPayloadInitialized = true;
		return _syncPayload;
	}

	public override Dictionary ExportComponentSave()
	{
		if (_pendingProgress != null)
		{
			return _pendingProgress.Duplicate(deep: true);
		}
		return new Dictionary
		{
			["stage"] = _garlicStage,
			["garlicRunning"] = _garlicRunning,
			["remaining"] = ((_garlicStage == 3 && GodotObject.IsInstanceValid(_changeLineTween)) ? Math.Max(0.0, _changeLineTotalDuration - _changeLineTween.GetTotalElapsedTime()) : (GodotObject.IsInstanceValid(_garlicWaitTimer) ? Math.Max(0.0, _garlicWaitTimer.TimeLeft) : 0.0)),
			["targetY"] = _changeLineTargetY,
			["moveDir"] = _syncMoveDir,
			["savedFilters"] = new Array<bool>(_savedFilterStates),
			["savedReplacement"] = _savedReplacement,
			["savedAtlasReplacementPath"] = _savedAtlasReplacementPath
		};
	}

	public override void ImportComponentSave(Dictionary data, TowerDefenseLevelSaveConfigCSharp owner)
	{
		CancelActiveWork();
		_pendingProgress = data.Duplicate(deep: true);
		RefreshPhysicsProcessEligibility();
	}

	internal override void PhysicsProcess(double delta, ulong physicsFrame)
	{
		if (_pendingProgress == null || !TryGetParent(out var zombie) || zombie.IsProgressRestoreInFlight || !TowerDefenseManager._IsGameRunning())
		{
			return;
		}
		Dictionary pendingProgress = _pendingProgress;
		_pendingProgress = null;
		RefreshPhysicsProcessEligibility();
		int num = pendingProgress.GetValueOrDefault("stage", 0).AsInt32();
		double num2 = pendingProgress.GetValueOrDefault("remaining", 0.0).AsDouble();
		num2 = (double.IsFinite(num2) ? Math.Max(0.0, num2) : 0.0);
		_syncMoveDir = pendingProgress.GetValueOrDefault("moveDir", 0).AsInt32();
		if ((uint)(num - 1) <= 1u)
		{
			_garlicRunning = true;
			if (num == 2)
			{
				Godot.Collections.Array array = pendingProgress.GetValueOrDefault("savedFilters", new Godot.Collections.Array()).AsGodotArray();
				_savedFilterStates = new bool[array.Count];
				for (int i = 0; i < array.Count; i++)
				{
					_savedFilterStates[i] = array[i].AsBool();
				}
				_savedReplacement = pendingProgress.GetValueOrDefault("savedReplacement").AsGodotObject() as Texture2D;
				_savedAtlasReplacementPath = pendingProgress.GetValueOrDefault("savedAtlasReplacementPath", "").AsString();
				_grossoutApplied = true;
			}
			GarlicAsync(++_garlicVersion, num, num2);
		}
		else if (num == 3)
		{
			zombie.isGarlic = false;
			float num3 = pendingProgress.GetValueOrDefault("targetY", zombie.GetLogicalGlobalPosition().Y).AsSingle();
			if (!float.IsFinite(num3))
			{
				num3 = zombie.GetLogicalGlobalPosition().Y;
			}
			MoveToLineAsync(zombie, num3, num2, 0);
		}
		else if (zombie.isGarlic || zombie.isChangeLine)
		{
			zombie.isGarlic = false;
			zombie.isChangeLine = false;
			zombie.Walk();
		}
	}

	private void ClearSyncPayload()
	{
		ClearReusableSyncPayload();
		_syncPayloadInitialized = false;
		_syncPayloadMoveDir = 0;
		_syncPayloadIsChangeLine = false;
	}

	public override void SyncDeserialize(Dictionary data)
	{
		_syncMoveDir = data.GetValueOrDefault("move_dir", 0).AsInt32();
		_syncDeserializing = _syncMoveDir != 0;
		if (data.ContainsKey("is_change_line") && GodotObject.IsInstanceValid(parent))
		{
			parent.isChangeLine = data.GetValueOrDefault("is_change_line", parent.isChangeLine).AsBool();
		}
	}

	private bool TryGetParent(out TowerDefenseZombie zombie)
	{
		zombie = parent;
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && GodotObject.IsInstanceValid(zombie) && zombie.inGame && !zombie.editorPreviewMode && GodotObject.IsInstanceValid(zombie.instance))
		{
			return GodotObject.IsInstanceValid(zombie.sprite);
		}
		return false;
	}
}
