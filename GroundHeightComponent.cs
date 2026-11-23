using System;
using Godot;
using Godot.Collections;
using PVZHE.ModEditor.ModSystem;

public sealed class GroundHeightComponent : CharacterComponentRuntime
{
	public float interpolationSpeed = 3f;

	public float threshold = 0.1f;

	public float waterHeight = 25f;

	public float ladderHeight = 60f;

	public bool handleWaterHeight;

	public bool handleLadder;

	public bool detectWater;

	public bool detectLadder;

	public TowerDefenseCharacter parent;

	public bool onLadder;

	public Vector2 _gridSize;

	public WaterInteractionComponent waterInteractionComponent;

	public float _waterExitCooldown;

	private Dictionary _pendingSyncData;

	private static readonly StringName SyncAliveKey = new StringName("_alive");

	private bool _syncPayloadInitialized;

	private bool _syncPayloadHasParent;

	private float _syncPayloadHeight;

	private bool _syncPayloadWater;

	private bool _syncPayloadLadder;

	private bool _configured;

	private bool _updatePending = true;

	private bool _applyingGroundHeight;

	private bool _followingModSupport;

	private bool _releasingModSupport;

	private double _supportReleaseStart;

	private double _supportReleaseElapsed;

	private const double SupportReleaseDuration = 0.2;

	protected override bool AllowPhysicsOutsideComponentBattlefield => true;

	private GroundHeightComponentDefinition Definition => base.ComponentDefinition as GroundHeightComponentDefinition;

	private bool RequiresContinuousTracking
	{
		get
		{
			if (!handleWaterHeight && !handleLadder && !detectWater && !detectLadder && !_followingModSupport)
			{
				return _releasingModSupport;
			}
			return true;
		}
	}

	internal override bool WantsPhysicsProcess
	{
		get
		{
			if (!RequiresContinuousTracking)
			{
				return _updatePending;
			}
			return true;
		}
	}

	private Dictionary _syncPayload => GetReusableSyncPayload();

	internal bool ControlsModSupportHeight
	{
		get
		{
			if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active)
			{
				double height;
				if (!_followingModSupport && !_releasingModSupport)
				{
					return TryGetModSupportHeight(out height);
				}
				return true;
			}
			return false;
		}
	}

	protected override void OnBound()
	{
		parent = base.Owner;
		if (parent is TowerDefenseZombie towerDefenseZombie)
		{
			WaterInteractionComponent obj = towerDefenseZombie.waterInteractionComponent;
			if (obj != null && !obj.IsReleased)
			{
				waterInteractionComponent = towerDefenseZombie.waterInteractionComponent;
			}
		}
		if (!_configured)
		{
			GroundHeightComponentDefinition definition = Definition;
			interpolationSpeed = definition?.interpolationSpeed ?? 3f;
			threshold = definition?.threshold ?? 0.1f;
			waterHeight = definition?.waterHeight ?? 25f;
			ladderHeight = definition?.ladderHeight ?? 60f;
			handleWaterHeight = definition?.handleWaterHeight ?? false;
			handleLadder = definition?.handleLadder ?? false;
			detectWater = definition?.detectWater ?? false;
			detectLadder = definition?.detectLadder ?? false;
			_configured = true;
		}
		if (GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			_gridSize = TowerDefenseManager.Instance.GetMapGridSize();
		}
		_updatePending = true;
		_followingModSupport = false;
		_releasingModSupport = false;
		ApplyPendingSyncData();
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		parent = null;
		waterInteractionComponent = null;
		_pendingSyncData = null;
		ClearSyncPayload();
		_updatePending = true;
		_applyingGroundHeight = false;
		_followingModSupport = false;
		_releasingModSupport = false;
	}

	protected override void OnReleased()
	{
		parent = null;
		waterInteractionComponent = null;
		_pendingSyncData = null;
		ClearSyncPayload();
		_updatePending = false;
		_applyingGroundHeight = false;
		_followingModSupport = false;
		_releasingModSupport = false;
	}

	protected override void OnAliveChanged(bool alive)
	{
		_updatePending = alive;
	}

	internal void NotifyOwnerGroundHeightChanged()
	{
		if (_applyingGroundHeight)
		{
			return;
		}
		if (!handleWaterHeight)
		{
			TowerDefenseCharacter towerDefenseCharacter = parent;
			if (towerDefenseCharacter != null && towerDefenseCharacter.inWater)
			{
				return;
			}
		}
		RequestUpdate();
	}

	internal void NotifyCellChanged()
	{
		RequestUpdate();
	}

	internal void NotifyEnvironmentChanged()
	{
		RequestUpdate();
	}

	internal override void PhysicsProcess(double delta, ulong physicsFrame)
	{
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && GodotObject.IsInstanceValid(parent))
		{
			BatchUpdateValidated(delta, useSharedPhysicsFrame: true, physicsFrame);
		}
	}

	public void BatchUpdate(double delta)
	{
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active && GodotObject.IsInstanceValid(parent))
		{
			BatchUpdateValidated(delta, useSharedPhysicsFrame: false, 0uL);
		}
	}

	internal void BatchUpdateValidated(double delta, bool useSharedPhysicsFrame, ulong sharedPhysicsFrame)
	{
		if (_waterExitCooldown > 0f)
		{
			_waterExitCooldown -= (float)delta;
		}
		bool flag = GodotObject.IsInstanceValid(parent.cell);
		if (flag && TryGetModSupportHeight(out var height))
		{
			_followingModSupport = true;
			_releasingModSupport = false;
			ApplyGroundHeight(height);
			return;
		}
		if (_followingModSupport)
		{
			_followingModSupport = false;
			_releasingModSupport = true;
			_supportReleaseStart = parent.groundHeight;
			_supportReleaseElapsed = 0.0;
		}
		float num;
		if (!flag)
		{
			if (ShouldPreserveHeightWithoutCell())
			{
				return;
			}
			if (detectWater && parent.inWater)
			{
				_waterExitCooldown = 0.5f;
				parent.inWater = false;
			}
			if (parent.groundHeight == 0.0)
			{
				CompleteIdleUpdate();
				return;
			}
			num = 0f;
		}
		else
		{
			if ((detectWater || detectLadder) && (ulong)((long)(useSharedPhysicsFrame ? sharedPhysicsFrame : Engine.GetPhysicsFrames()) + (long)parent.randFreshIndex) % 5uL == 0L)
			{
				DetectEnvironment();
			}
			if (!handleWaterHeight && parent.inWater && !_releasingModSupport)
			{
				CompleteIdleUpdate();
				return;
			}
			num = GetTargetHeight();
		}
		if (_releasingModSupport)
		{
			_supportReleaseElapsed = Math.Min(0.2, _supportReleaseElapsed + Math.Max(0.0, delta));
			ApplyGroundHeight(_supportReleaseStart + ((double)num - _supportReleaseStart) * (_supportReleaseElapsed / 0.2));
			if (_supportReleaseElapsed >= 0.2)
			{
				_releasingModSupport = false;
				CompleteIdleUpdate();
			}
		}
		else if (Mathf.Abs(parent.groundHeight - (double)num) > (double)threshold)
		{
			ApplyGroundHeight(Mathf.Lerp((float)parent.groundHeight, num, interpolationSpeed * (float)delta));
		}
		else
		{
			ApplyGroundHeight(num);
			CompleteIdleUpdate();
		}
	}

	private void RequestUpdate()
	{
		if (!_updatePending && !base.IsReleased)
		{
			_updatePending = true;
			RefreshPhysicsProcessEligibility();
		}
	}

	private void CompleteIdleUpdate()
	{
		if (!RequiresContinuousTracking && _updatePending)
		{
			_updatePending = false;
			RefreshPhysicsProcessEligibility();
		}
	}

	private void ApplyGroundHeight(double value)
	{
		_applyingGroundHeight = true;
		try
		{
			parent.groundHeight = value;
		}
		finally
		{
			_applyingGroundHeight = false;
		}
	}

	private bool ShouldPreserveHeightWithoutCell()
	{
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (parent is TowerDefenseZombie && GodotObject.IsInstanceValid(instance))
		{
			return (double)parent.GetGlobalPositionForPhysicsFrame(TowerDefenseProcessModeDispatch.CurrentPhysicsFrame).X <= instance.GetMapGroundLeft();
		}
		return false;
	}

	public void DetectEnvironment()
	{
		if (!GodotObject.IsInstanceValid(parent) || !GodotObject.IsInstanceValid(parent.cell))
		{
			return;
		}
		float num = (float)parent.cellPercentage;
		if (detectLadder)
		{
			onLadder = parent.config is TowerDefenseZombieConfig towerDefenseZombieConfig && (parent.instance.maskFlags & 1) != 0 && GodotObject.IsInstanceValid(parent.cell.characterLadder) && towerDefenseZombieConfig.physique < TowerDefenseEnum.ZOMBIE_PHYSIQUE.HUGE && parent.Scale.X > 0f;
		}
		if (detectWater)
		{
			bool flag = (!onLadder || num <= 0.5f) && parent.cell.isWater && (parent.instance.maskFlags & 2) == 0;
			if (parent.inWater != flag && !(_waterExitCooldown > 0f))
			{
				parent.inWater = flag;
				_waterExitCooldown = 0.5f;
			}
		}
	}

	public float GetTargetHeight()
	{
		if (TryGetModSupportHeight(out var height))
		{
			return (float)height;
		}
		if (_followingModSupport || _releasingModSupport)
		{
			WaterEnvironmentComponent waterEnvironmentComponent = parent.componentManager?.GetRuntime<WaterEnvironmentComponent>();
			if (waterEnvironmentComponent != null && waterEnvironmentComponent.TryGetActiveTargetGroundHeight(out var height2))
			{
				return height2;
			}
		}
		if (handleLadder && onLadder && parent.cellPercentage > 0.5)
		{
			return ladderHeight;
		}
		if (handleWaterHeight && parent.inWater)
		{
			if (parent is TowerDefenseZombie towerDefenseZombie)
			{
				return 0f - (float)towerDefenseZombie.waterHeight;
			}
			return 0f - waterHeight;
		}
		return (float)parent.cell.GetGroundHeight(parent.cellPercentage);
	}

	private bool TryGetModSupportHeight(out double height)
	{
		height = 0.0;
		if (parent is TowerDefensePlant)
		{
			return XWModPlantPlacementService.TryGetSupportHeight(parent.cell, parent, out height);
		}
		return false;
	}

	public override Dictionary SyncSerialize()
	{
		bool flag = GodotObject.IsInstanceValid(parent) && parent.syncId >= 0;
		float num = (flag ? Mathf.Snapped((float)parent.groundHeight, 0.1f) : 0f);
		if (_syncPayloadInitialized)
		{
			int num2 = (flag ? 3 : 0);
			if (_syncPayload.Count == num2 + 1)
			{
				_syncPayload.Remove(SyncAliveKey);
			}
			if (_syncPayload.Count == num2 && _syncPayloadHasParent == flag && (!flag || (_syncPayloadHeight == num && _syncPayloadWater == parent.inWater && _syncPayloadLadder == onLadder)))
			{
				return _syncPayload;
			}
		}
		ClearReusableSyncPayload();
		if (flag)
		{
			_syncPayload["height"] = Mathf.Snapped((float)parent.groundHeight, 0.1f);
			_syncPayload["water"] = parent.inWater;
			_syncPayload["ladder"] = onLadder;
		}
		_syncPayloadHasParent = flag;
		_syncPayloadHeight = num;
		_syncPayloadWater = flag && parent.inWater;
		_syncPayloadLadder = flag && onLadder;
		_syncPayloadInitialized = true;
		return _syncPayload;
	}

	private void ClearSyncPayload()
	{
		ClearReusableSyncPayload();
		_syncPayloadInitialized = false;
		_syncPayloadHasParent = false;
		_syncPayloadHeight = 0f;
		_syncPayloadWater = false;
		_syncPayloadLadder = false;
	}

	public override void SyncDeserialize(Dictionary data)
	{
		if (data != null)
		{
			if (!GodotObject.IsInstanceValid(parent))
			{
				_pendingSyncData = data.Duplicate(deep: true);
			}
			else
			{
				ApplySyncData(data);
			}
		}
	}

	private void ApplyPendingSyncData()
	{
		if (_pendingSyncData != null && GodotObject.IsInstanceValid(parent))
		{
			Dictionary pendingSyncData = _pendingSyncData;
			_pendingSyncData = null;
			ApplySyncData(pendingSyncData);
		}
	}

	private void ApplySyncData(Dictionary data)
	{
		if (data.ContainsKey("height"))
		{
			parent.groundHeight = data["height"].AsDouble();
		}
		onLadder = data.GetValueOrDefault("ladder", onLadder).AsBool();
		bool flag = data.GetValueOrDefault("water", parent.inWater).AsBool();
		if (parent.inWater == flag)
		{
			return;
		}
		parent.inWater = flag;
		WaterInteractionComponent obj = waterInteractionComponent;
		if (obj != null && !obj.IsReleased)
		{
			if (flag)
			{
				waterInteractionComponent.InWater();
			}
			else
			{
				waterInteractionComponent.OutWater();
			}
		}
	}
}
