using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Diamond/ImitaterQX/Scene/TowerDefensePlantImitaterQX.cs")]
public class TowerDefensePlantImitaterQX : TowerDefensePlant, INetworkSpawnStateReceiver
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName HideOwnHealthBar = "HideOwnHealthBar";

		public static readonly StringName ApplyMimicDeviceState = "ApplyMimicDeviceState";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public static readonly StringName ConnectPlantingEvent = "ConnectPlantingEvent";

		public static readonly StringName ConnectStateSignals = "ConnectStateSignals";

		public static readonly StringName DisconnectStateSignals = "DisconnectStateSignals";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName PlantManuallyPlanted = "PlantManuallyPlanted";

		public static readonly StringName ResolvePendingTransform = "ResolvePendingTransform";

		public new static readonly StringName IdleEntered = "IdleEntered";

		public static readonly StringName TransformEntered = "TransformEntered";

		public static readonly StringName TransformedEntered = "TransformedEntered";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName ReplaceProjection = "ReplaceProjection";

		public static readonly StringName ApplyDroneState = "ApplyDroneState";

		public static readonly StringName CreateSmoke = "CreateSmoke";

		public static readonly StringName BindProjection = "BindProjection";

		public static readonly StringName UnbindProjection = "UnbindProjection";

		public static readonly StringName ApplyProjectionGridOccupancy = "ApplyProjectionGridOccupancy";

		public static readonly StringName ReleaseProjectionGridOccupancy = "ReleaseProjectionGridOccupancy";

		public static readonly StringName CanTransformInto = "CanTransformInto";

		public static readonly StringName ProjectionDestroyed = "ProjectionDestroyed";

		public static readonly StringName ProjectionExiting = "ProjectionExiting";

		public static readonly StringName UpdateProjectionFollow = "UpdateProjectionFollow";

		public static readonly StringName FollowProjection = "FollowProjection";

		public static readonly StringName OwnerDestroyed = "OwnerDestroyed";

		public static readonly StringName RemoveProjection = "RemoveProjection";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public new static readonly StringName FinalizeProgressRestore = "FinalizeProgressRestore";

		public new static readonly StringName OnHypnosisStateChanged = "OnHypnosisStateChanged";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ExportNetworkSpawnState = "ExportNetworkSpawnState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";

		public static readonly StringName ImportNetworkSpawnState = "ImportNetworkSpawnState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _eventBus = "_eventBus";

		public static readonly StringName _projection = "_projection";

		public static readonly StringName _projectionName = "_projectionName";

		public static readonly StringName _projectionSyncId = "_projectionSyncId";

		public static readonly StringName _pendingPacket = "_pendingPacket";

		public static readonly StringName _transformed = "_transformed";

		public static readonly StringName _transforming = "_transforming";

		public static readonly StringName _changingProjection = "_changingProjection";

		public static readonly StringName _resolveProjection = "_resolveProjection";

		public static readonly StringName _revision = "_revision";

		public static readonly StringName _stateSignalsConnected = "_stateSignalsConnected";

		public static readonly StringName _pendingCandidate = "_pendingCandidate";

		public static readonly StringName _hasPendingCandidate = "_hasPendingCandidate";

		public static readonly StringName _projectionLifecycleDirty = "_projectionLifecycleDirty";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private static PackedScene _imitaterCloud;

	private BattleEventBus _eventBus;

	private TowerDefenseCharacter _projection;

	private string _projectionName = "";

	private int _projectionSyncId = -1;

	private string _pendingPacket = "";

	private bool _transformed;

	private bool _transforming;

	private bool _changingProjection;

	private bool _resolveProjection;

	private int _revision;

	private StateHandle _transformState;

	private StateHandle _transformedState;

	private bool _stateSignalsConnected;

	private TowerDefenseCharacter _pendingCandidate;

	private bool _hasPendingCandidate;

	private bool _projectionLifecycleDirty;

	private static PackedScene IMITATER_CLOUD => _imitaterCloud ?? (_imitaterCloud = GD.Load<PackedScene>("uid://djvfnrjg7vtqn"));

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode && inGame)
		{
			ConnectPlantingEvent();
			base.OnDestroy += OwnerDestroyed;
			ConnectStateSignals();
			ApplyMimicDeviceState();
			HideOwnHealthBar();
			if (_transformed)
			{
				ApplyDroneState();
			}
		}
	}

	private void HideOwnHealthBar()
	{
		ShowHealthComponent obj = showHealthComponent;
		if (obj != null && !obj.IsReleased)
		{
			showHealthComponent.Visible = false;
			showHealthComponent.SetAlive(alive: false);
		}
	}

	private void ApplyMimicDeviceState()
	{
		if (GodotObject.IsInstanceValid(instance))
		{
			instance.canBeCollection = false;
			instance.maskFlags = 0;
			instance.invincible = true;
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (IsNodeReady() && !Engine.IsEditorHint() && !editorPreviewMode && inGame)
		{
			ConnectPlantingEvent();
		}
	}

	private void ConnectPlantingEvent()
	{
		if (_eventBus != BattleEventBus.Instance)
		{
			_eventBus = BattleEventBus.Instance;
			if (GodotObject.IsInstanceValid(_eventBus))
			{
				_eventBus.OnPlantManuallyPlanted += PlantManuallyPlanted;
			}
		}
	}

	private void ConnectStateSignals()
	{
		if (!_stateSignalsConnected)
		{
			_transformState = base.StateMachine?.GetStateById("plant.imitater_qx.transform");
			_transformedState = base.StateMachine?.GetStateById("plant.imitater_qx.transformed");
			StateHandle transformState = _transformState;
			if (transformState != null && transformState.IsValid)
			{
				_transformState.Entered += TransformEntered;
			}
			StateHandle transformedState = _transformedState;
			if (transformedState != null && transformedState.IsValid)
			{
				_transformedState.Entered += TransformedEntered;
			}
			_stateSignalsConnected = true;
		}
	}

	private void DisconnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			StateHandle transformState = _transformState;
			if (transformState != null && transformState.IsValid)
			{
				_transformState.Entered -= TransformEntered;
			}
			StateHandle transformedState = _transformedState;
			if (transformedState != null && transformedState.IsValid)
			{
				_transformedState.Entered -= TransformedEntered;
			}
			_transformState = null;
			_transformedState = null;
			_stateSignalsConnected = false;
		}
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(_eventBus))
		{
			_eventBus.OnPlantManuallyPlanted -= PlantManuallyPlanted;
		}
		_eventBus = null;
		_pendingCandidate = null;
		_hasPendingCandidate = false;
		DisconnectStateSignals();
		base._ExitTree();
	}

	private void PlantManuallyPlanted(TowerDefenseCharacter character)
	{
		if (!isDestroy && !die && !base.IsProgressRestoreInFlight && (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost) && GodotObject.IsInstanceValid(character) && character.config is TowerDefensePlantConfig && !(character.config.name == base.config.name) && !CheckDifferentCamp(character.camp) && (!GodotObject.IsInstanceValid(character.instance) || !character.instance.hologram))
		{
			_pendingCandidate = character;
			_hasPendingCandidate = true;
		}
	}

	private void ResolvePendingTransform()
	{
		if (!_hasPendingCandidate)
		{
			return;
		}
		TowerDefenseCharacter pendingCandidate = _pendingCandidate;
		if (!GodotObject.IsInstanceValid(pendingCandidate) || pendingCandidate.isDestroy || pendingCandidate.die)
		{
			_pendingCandidate = null;
			_hasPendingCandidate = false;
		}
		else
		{
			if (!pendingCandidate.IsInsideTree())
			{
				return;
			}
			_pendingCandidate = null;
			_hasPendingCandidate = false;
			if (!CanTransformInto(TowerDefenseManager.GetPacketConfig(pendingCandidate.packet.saveKey)))
			{
				return;
			}
			_pendingPacket = pendingCandidate.packet.saveKey;
			if (_transformed)
			{
				ReplaceProjection();
			}
			else if (!_transforming)
			{
				_transforming = true;
				_revision++;
				if (!SendStateEvent("ToTransform"))
				{
					base.sprite.SetAnimation("Explode", loop: false, 0.2);
				}
			}
		}
	}

	public override void IdleEntered()
	{
		base.IdleEntered();
		if (_transformed)
		{
			if (!SendStateEvent("ToTransformed"))
			{
				ApplyDroneState();
			}
		}
		else if (_transforming && !SendStateEvent("ToTransform"))
		{
			base.sprite.SetAnimation("Explode", loop: false);
		}
	}

	public void TransformEntered()
	{
		base.sprite.SetAnimation("Explode", loop: false, 0.2);
	}

	public void TransformedEntered()
	{
		ApplyDroneState();
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Explode" && _transforming && (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost))
		{
			ReplaceProjection();
		}
	}

	private void ReplaceProjection()
	{
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(_pendingPacket);
		if (!GodotObject.IsInstanceValid(packetConfig))
		{
			return;
		}
		RemoveProjection();
		bool transformed = _transformed;
		_transformed = true;
		_transforming = false;
		if (!transformed && !SendStateEvent("ToTransformed"))
		{
			ApplyDroneState();
		}
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		TowerDefenseCharacter towerDefenseCharacter = (base.HasEconomyOwner ? packetConfig.Create(base.EconomyOwnerAccountId, logicalGlobalPosition, base.gridPos, base.groundHeight) : packetConfig.Create(logicalGlobalPosition, base.gridPos, base.groundHeight));
		if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
		{
			Destroy();
			return;
		}
		towerDefenseCharacter.groundHeight = base.groundHeight;
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseCharacter, forceReadableName: false, InternalMode.Disabled);
		towerDefenseCharacter.instance.hologram = true;
		towerDefenseCharacter.instance.canBeCollection = false;
		towerDefenseCharacter.skipDestroySet = true;
		towerDefenseCharacter.SetSpriteGroupShaderParameter("hologram", true);
		towerDefenseCharacter.SyncHologramHypnoses(instance.hypnoses);
		towerDefenseCharacter.WakeUp();
		BindProjection(towerDefenseCharacter);
		_projectionName = towerDefenseCharacter.Name;
		if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
		{
			TowerDefenseControlNew currentControl = TowerDefenseManager.CurrentControl;
			_projectionSyncId = currentControl.GetNextSyncId();
			currentControl.RegisterSyncCharacter(_projectionSyncId, towerDefenseCharacter);
			MultiPlayerManager.Instance.SendSpawnCharacterAt(packetConfig.saveKey, base.gridPos.X, base.gridPos.Y, _projectionSyncId, 1.0, 1.0, instance.hypnoses, 0.0, useCreate: true, logicalGlobalPosition.X, logicalGlobalPosition.Y, walkAfterSpawn: false, base.groundHeight, "", new Dictionary
			{
				["hologram"] = true,
				["wake_up"] = true
			});
		}
		_pendingPacket = "";
		_revision++;
		CreateSmoke();
	}

	private void ApplyDroneState()
	{
		if (GodotObject.IsInstanceValid(instance))
		{
			instance.hologram = true;
			instance.canBeCollection = false;
			instance.maskFlags = 0;
			ShadowComponent obj = shadowComponent;
			if (obj != null && !obj.IsReleased)
			{
				shadowComponent.shadowDisabled = true;
				shadowComponent.SetShadowVisible(visible: false);
			}
			HideOwnHealthBar();
			if (GodotObject.IsInstanceValid(cell))
			{
				cell.ReleaseHologramGridOccupancy(this);
			}
			SetSpriteGroupShaderParameter("hologram", false);
			base.sprite.SetAnimation("IdleTrans");
		}
	}

	private void CreateSmoke()
	{
		TowerDefenseEffectParticlesOnce towerDefenseEffectParticlesOnce = TowerDefenseManager.CreateEffectParticlesOnce(IMITATER_CLOUD, base.gridPos);
		towerDefenseEffectParticlesOnce.GlobalPosition = GetLogicalGlobalPosition();
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseEffectParticlesOnce, forceReadableName: false, InternalMode.Disabled);
	}

	private void BindProjection(TowerDefenseCharacter projection)
	{
		UnbindProjection();
		_projection = projection;
		_projection.OnDestroy += ProjectionDestroyed;
		_projection.TreeExiting += ProjectionExiting;
		_projectionLifecycleDirty = false;
		ApplyProjectionGridOccupancy(projection);
	}

	private void UnbindProjection()
	{
		if (GodotObject.IsInstanceValid(_projection))
		{
			_projection.OnDestroy -= ProjectionDestroyed;
			_projection.TreeExiting -= ProjectionExiting;
			ReleaseProjectionGridOccupancy(_projection);
		}
		_projection = null;
	}

	private void ApplyProjectionGridOccupancy(TowerDefenseCharacter projection)
	{
		if (!GodotObject.IsInstanceValid(projection) || !GodotObject.IsInstanceValid(projection.instance) || !(projection.config is TowerDefensePlantConfig))
		{
			return;
		}
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(projection.gridPos);
		if (GodotObject.IsInstanceValid(mapCell) && GodotObject.IsInstanceValid(projection.packet))
		{
			if (_transformed && GodotObject.IsInstanceValid(instance))
			{
				instance.hologram = true;
				mapCell.ReleaseHologramGridOccupancy(this);
			}
			projection.hologramOccupiesGrid = true;
			if (!mapCell.characterList.Contains(projection))
			{
				mapCell.CharacterPlant(projection.packet, projection, noLimit: true, allowReplacement: false);
			}
		}
	}

	private void ReleaseProjectionGridOccupancy(TowerDefenseCharacter projection)
	{
		if (GodotObject.IsInstanceValid(projection))
		{
			projection.hologramOccupiesGrid = false;
			TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(projection.gridPos);
			if (GodotObject.IsInstanceValid(mapCell))
			{
				mapCell.RemoveCharacter(projection);
			}
		}
	}

	private bool CanTransformInto(TowerDefensePacketConfig packetConfig)
	{
		if (!GodotObject.IsInstanceValid(packetConfig))
		{
			return false;
		}
		if (!(packetConfig.characterConfig is TowerDefensePlantConfig))
		{
			return false;
		}
		if (packetConfig._GetType() == TowerDefenseEnum.PACKET_TYPE.COVER)
		{
			return false;
		}
		TowerDefenseCellInstance towerDefenseCellInstance = (GodotObject.IsInstanceValid(cell) ? cell : TowerDefenseManager.GetMapCell(base.gridPos));
		if (!GodotObject.IsInstanceValid(towerDefenseCellInstance))
		{
			return false;
		}
		TowerDefenseCharacter projection = _projection;
		bool flag = GodotObject.IsInstanceValid(projection) && !projection.isDestroy && towerDefenseCellInstance.characterList.Contains(projection);
		if (flag)
		{
			towerDefenseCellInstance.RemoveCharacter(projection);
		}
		bool flag2 = towerDefenseCellInstance.characterList.Contains(this);
		if (flag2)
		{
			towerDefenseCellInstance.RemoveCharacter(this);
		}
		try
		{
			return towerDefenseCellInstance.CanPacketPlant(packetConfig);
		}
		finally
		{
			if (flag2 && !_transformed && GodotObject.IsInstanceValid(this) && !isDestroy && GodotObject.IsInstanceValid(packet))
			{
				towerDefenseCellInstance.CharacterPlant(packet, this, noLimit: true, allowReplacement: false);
			}
			if (flag && GodotObject.IsInstanceValid(projection) && !projection.isDestroy && GodotObject.IsInstanceValid(projection.packet))
			{
				towerDefenseCellInstance.CharacterPlant(projection.packet, projection, noLimit: true, allowReplacement: false);
			}
		}
	}

	private void ProjectionDestroyed(TowerDefenseCharacter character)
	{
		ProjectionExiting();
	}

	private void ProjectionExiting()
	{
		if (!_changingProjection && !isDestroy && IsInsideTree())
		{
			_projectionLifecycleDirty = true;
		}
	}

	private void UpdateProjectionFollow()
	{
		if (_changingProjection || _resolveProjection || !_transformed || isDestroy)
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(_projection) || _projection.isDestroy || !_projection.IsInsideTree())
		{
			if (_projectionLifecycleDirty)
			{
				_projectionLifecycleDirty = false;
				UnbindProjection();
				if (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost)
				{
					Destroy();
				}
			}
		}
		else
		{
			_projectionLifecycleDirty = false;
			FollowProjection();
		}
	}

	private void FollowProjection()
	{
		if (GodotObject.IsInstanceValid(instance))
		{
			if (base.gridPos != _projection.gridPos)
			{
				base.gridPos = _projection.gridPos;
			}
			Vector2 logicalGlobalPosition = _projection.GetLogicalGlobalPosition();
			if (GetLogicalGlobalPosition() != logicalGlobalPosition)
			{
				SetLogicalGlobalPosition(logicalGlobalPosition);
			}
		}
	}

	private void OwnerDestroyed(TowerDefenseCharacter character)
	{
		RemoveProjection();
	}

	private void RemoveProjection()
	{
		_changingProjection = true;
		TowerDefenseCharacter projection = _projection;
		UnbindProjection();
		if (GodotObject.IsInstanceValid(projection) && !projection.isDestroy)
		{
			projection.suppressDeathrattles = true;
			projection.skipDestroySet = true;
			projection.Visible = false;
			projection.componentManager.GetRuntime<DestroyComponent>().Destroy();
		}
		_changingProjection = false;
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (!isDestroy && !base.IsProgressRestoreInFlight)
		{
			ResolvePendingTransform();
			UpdateProjectionFollow();
		}
		if (_resolveProjection && !base.IsProgressRestoreInFlight && inGame && !isDestroy)
		{
			TowerDefenseCharacter value = null;
			if (Global.IsMultiplayerMode && _projectionSyncId >= 0)
			{
				TowerDefenseManager.CurrentControl._syncCharacters.TryGetValue(_projectionSyncId, out value);
			}
			else if (!string.IsNullOrEmpty(_projectionName))
			{
				value = TowerDefenseManager.GetCharacterNode().GetNodeOrNull<TowerDefenseCharacter>(new NodePath(_projectionName));
			}
			if (GodotObject.IsInstanceValid(value))
			{
				BindProjection(value);
				_resolveProjection = false;
			}
			else if (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost)
			{
				_resolveProjection = false;
				Destroy();
			}
		}
	}

	public override void FinalizeProgressRestore()
	{
		base.FinalizeProgressRestore();
		HideOwnHealthBar();
		if (_transformed)
		{
			ApplyDroneState();
			_resolveProjection = true;
		}
	}

	protected internal override void OnHypnosisStateChanged()
	{
		base.OnHypnosisStateChanged();
		if (GodotObject.IsInstanceValid(_projection))
		{
			_projection.SyncHologramHypnoses(instance.hypnoses);
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["transformed"] = _transformed,
			["transforming"] = _transforming,
			["pendingPacket"] = _pendingPacket,
			["projectionName"] = (GodotObject.IsInstanceValid(_projection) ? _projection.Name.ToString().ValidateNodeName() : _projectionName),
			["projectionSyncId"] = _projectionSyncId,
			["revision"] = _revision
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_transformed = data.GetValueOrDefault("transformed", false).AsBool();
		_transforming = data.GetValueOrDefault("transforming", false).AsBool();
		_pendingPacket = data.GetValueOrDefault("pendingPacket", "").AsString();
		_projectionName = data.GetValueOrDefault("projectionName", "").AsString();
		_projectionSyncId = data.GetValueOrDefault("projectionSyncId", -1).AsInt32();
		_revision = data.GetValueOrDefault("revision", 0).AsInt32();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportVariantSave();
	}

	public override Dictionary ExportNetworkSpawnState()
	{
		return ExportVariantSave();
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return _revision;
	}

	public void ImportNetworkSpawnState(Dictionary data)
	{
		ImportVariantSave(data);
		ApplyMimicDeviceState();
		HideOwnHealthBar();
		_resolveProjection = _transformed;
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		int revision = _revision;
		int projectionSyncId = _projectionSyncId;
		ImportVariantSave(data);
		if (_transformed)
		{
			if (projectionSyncId != _projectionSyncId)
			{
				UnbindProjection();
				_resolveProjection = true;
			}
			if (revision != _revision)
			{
				ApplyDroneState();
				CreateSmoke();
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(38)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HideOwnHealthBar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyMimicDeviceState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConnectPlantingEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DisconnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlantManuallyPlanted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ResolvePendingTransform, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TransformEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TransformedEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ReplaceProjection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyDroneState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CreateSmoke, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BindProjection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projection", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.UnbindProjection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyProjectionGridOccupancy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projection", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ReleaseProjectionGridOccupancy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projection", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.CanTransformInto, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "packetConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null),
			new MethodInfo(MethodName.ProjectionDestroyed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ProjectionExiting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateProjectionFollow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FollowProjection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OwnerDestroyed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.RemoveProjection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FinalizeProgressRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnHypnosisStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportNetworkSpawnState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpawnState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
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
		if (method == MethodName.HideOwnHealthBar && args.Count == 0)
		{
			HideOwnHealthBar();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyMimicDeviceState && args.Count == 0)
		{
			ApplyMimicDeviceState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectPlantingEvent && args.Count == 0)
		{
			ConnectPlantingEvent();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectStateSignals && args.Count == 0)
		{
			ConnectStateSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectStateSignals && args.Count == 0)
		{
			DisconnectStateSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlantManuallyPlanted && args.Count == 1)
		{
			PlantManuallyPlanted(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResolvePendingTransform && args.Count == 0)
		{
			ResolvePendingTransform();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleEntered && args.Count == 0)
		{
			IdleEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TransformEntered && args.Count == 0)
		{
			TransformEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TransformedEntered && args.Count == 0)
		{
			TransformedEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReplaceProjection && args.Count == 0)
		{
			ReplaceProjection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDroneState && args.Count == 0)
		{
			ApplyDroneState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateSmoke && args.Count == 0)
		{
			CreateSmoke();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BindProjection && args.Count == 1)
		{
			BindProjection(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnbindProjection && args.Count == 0)
		{
			UnbindProjection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyProjectionGridOccupancy && args.Count == 1)
		{
			ApplyProjectionGridOccupancy(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleaseProjectionGridOccupancy && args.Count == 1)
		{
			ReleaseProjectionGridOccupancy(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanTransformInto && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(CanTransformInto(VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[0])));
			return true;
		}
		if (method == MethodName.ProjectionDestroyed && args.Count == 1)
		{
			ProjectionDestroyed(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProjectionExiting && args.Count == 0)
		{
			ProjectionExiting();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateProjectionFollow && args.Count == 0)
		{
			UpdateProjectionFollow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FollowProjection && args.Count == 0)
		{
			FollowProjection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OwnerDestroyed && args.Count == 1)
		{
			OwnerDestroyed(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveProjection && args.Count == 0)
		{
			RemoveProjection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FinalizeProgressRestore && args.Count == 0)
		{
			FinalizeProgressRestore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHypnosisStateChanged && args.Count == 0)
		{
			OnHypnosisStateChanged();
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
		if (method == MethodName.ExportNetworkSpecialState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpecialState());
			return true;
		}
		if (method == MethodName.ExportNetworkSpawnState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpawnState());
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(GetNetworkSpecialStateRevision());
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.HideOwnHealthBar)
		{
			return true;
		}
		if (method == MethodName.ApplyMimicDeviceState)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName.ConnectPlantingEvent)
		{
			return true;
		}
		if (method == MethodName.ConnectStateSignals)
		{
			return true;
		}
		if (method == MethodName.DisconnectStateSignals)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.PlantManuallyPlanted)
		{
			return true;
		}
		if (method == MethodName.ResolvePendingTransform)
		{
			return true;
		}
		if (method == MethodName.IdleEntered)
		{
			return true;
		}
		if (method == MethodName.TransformEntered)
		{
			return true;
		}
		if (method == MethodName.TransformedEntered)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ReplaceProjection)
		{
			return true;
		}
		if (method == MethodName.ApplyDroneState)
		{
			return true;
		}
		if (method == MethodName.CreateSmoke)
		{
			return true;
		}
		if (method == MethodName.BindProjection)
		{
			return true;
		}
		if (method == MethodName.UnbindProjection)
		{
			return true;
		}
		if (method == MethodName.ApplyProjectionGridOccupancy)
		{
			return true;
		}
		if (method == MethodName.ReleaseProjectionGridOccupancy)
		{
			return true;
		}
		if (method == MethodName.CanTransformInto)
		{
			return true;
		}
		if (method == MethodName.ProjectionDestroyed)
		{
			return true;
		}
		if (method == MethodName.ProjectionExiting)
		{
			return true;
		}
		if (method == MethodName.UpdateProjectionFollow)
		{
			return true;
		}
		if (method == MethodName.FollowProjection)
		{
			return true;
		}
		if (method == MethodName.OwnerDestroyed)
		{
			return true;
		}
		if (method == MethodName.RemoveProjection)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.FinalizeProgressRestore)
		{
			return true;
		}
		if (method == MethodName.OnHypnosisStateChanged)
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
		if (method == MethodName.ExportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpawnState)
		{
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._eventBus)
		{
			_eventBus = VariantUtils.ConvertTo<BattleEventBus>(in value);
			return true;
		}
		if (name == PropertyName._projection)
		{
			_projection = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		if (name == PropertyName._projectionName)
		{
			_projectionName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._projectionSyncId)
		{
			_projectionSyncId = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._pendingPacket)
		{
			_pendingPacket = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._transformed)
		{
			_transformed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._transforming)
		{
			_transforming = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._changingProjection)
		{
			_changingProjection = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._resolveProjection)
		{
			_resolveProjection = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._revision)
		{
			_revision = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._stateSignalsConnected)
		{
			_stateSignalsConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pendingCandidate)
		{
			_pendingCandidate = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		if (name == PropertyName._hasPendingCandidate)
		{
			_hasPendingCandidate = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._projectionLifecycleDirty)
		{
			_projectionLifecycleDirty = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._eventBus)
		{
			value = VariantUtils.CreateFrom(in _eventBus);
			return true;
		}
		if (name == PropertyName._projection)
		{
			value = VariantUtils.CreateFrom(in _projection);
			return true;
		}
		if (name == PropertyName._projectionName)
		{
			value = VariantUtils.CreateFrom(in _projectionName);
			return true;
		}
		if (name == PropertyName._projectionSyncId)
		{
			value = VariantUtils.CreateFrom(in _projectionSyncId);
			return true;
		}
		if (name == PropertyName._pendingPacket)
		{
			value = VariantUtils.CreateFrom(in _pendingPacket);
			return true;
		}
		if (name == PropertyName._transformed)
		{
			value = VariantUtils.CreateFrom(in _transformed);
			return true;
		}
		if (name == PropertyName._transforming)
		{
			value = VariantUtils.CreateFrom(in _transforming);
			return true;
		}
		if (name == PropertyName._changingProjection)
		{
			value = VariantUtils.CreateFrom(in _changingProjection);
			return true;
		}
		if (name == PropertyName._resolveProjection)
		{
			value = VariantUtils.CreateFrom(in _resolveProjection);
			return true;
		}
		if (name == PropertyName._revision)
		{
			value = VariantUtils.CreateFrom(in _revision);
			return true;
		}
		if (name == PropertyName._stateSignalsConnected)
		{
			value = VariantUtils.CreateFrom(in _stateSignalsConnected);
			return true;
		}
		if (name == PropertyName._pendingCandidate)
		{
			value = VariantUtils.CreateFrom(in _pendingCandidate);
			return true;
		}
		if (name == PropertyName._hasPendingCandidate)
		{
			value = VariantUtils.CreateFrom(in _hasPendingCandidate);
			return true;
		}
		if (name == PropertyName._projectionLifecycleDirty)
		{
			value = VariantUtils.CreateFrom(in _projectionLifecycleDirty);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._eventBus, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._projection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._projectionName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._projectionSyncId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._pendingPacket, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._transformed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._transforming, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._changingProjection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._resolveProjection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._revision, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._stateSignalsConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._pendingCandidate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._hasPendingCandidate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._projectionLifecycleDirty, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._eventBus, Variant.From(in _eventBus));
		info.AddProperty(PropertyName._projection, Variant.From(in _projection));
		info.AddProperty(PropertyName._projectionName, Variant.From(in _projectionName));
		info.AddProperty(PropertyName._projectionSyncId, Variant.From(in _projectionSyncId));
		info.AddProperty(PropertyName._pendingPacket, Variant.From(in _pendingPacket));
		info.AddProperty(PropertyName._transformed, Variant.From(in _transformed));
		info.AddProperty(PropertyName._transforming, Variant.From(in _transforming));
		info.AddProperty(PropertyName._changingProjection, Variant.From(in _changingProjection));
		info.AddProperty(PropertyName._resolveProjection, Variant.From(in _resolveProjection));
		info.AddProperty(PropertyName._revision, Variant.From(in _revision));
		info.AddProperty(PropertyName._stateSignalsConnected, Variant.From(in _stateSignalsConnected));
		info.AddProperty(PropertyName._pendingCandidate, Variant.From(in _pendingCandidate));
		info.AddProperty(PropertyName._hasPendingCandidate, Variant.From(in _hasPendingCandidate));
		info.AddProperty(PropertyName._projectionLifecycleDirty, Variant.From(in _projectionLifecycleDirty));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._eventBus, out var value))
		{
			_eventBus = value.As<BattleEventBus>();
		}
		if (info.TryGetProperty(PropertyName._projection, out var value2))
		{
			_projection = value2.As<TowerDefenseCharacter>();
		}
		if (info.TryGetProperty(PropertyName._projectionName, out var value3))
		{
			_projectionName = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._projectionSyncId, out var value4))
		{
			_projectionSyncId = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._pendingPacket, out var value5))
		{
			_pendingPacket = value5.As<string>();
		}
		if (info.TryGetProperty(PropertyName._transformed, out var value6))
		{
			_transformed = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._transforming, out var value7))
		{
			_transforming = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._changingProjection, out var value8))
		{
			_changingProjection = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._resolveProjection, out var value9))
		{
			_resolveProjection = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._revision, out var value10))
		{
			_revision = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._stateSignalsConnected, out var value11))
		{
			_stateSignalsConnected = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pendingCandidate, out var value12))
		{
			_pendingCandidate = value12.As<TowerDefenseCharacter>();
		}
		if (info.TryGetProperty(PropertyName._hasPendingCandidate, out var value13))
		{
			_hasPendingCandidate = value13.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._projectionLifecycleDirty, out var value14))
		{
			_projectionLifecycleDirty = value14.As<bool>();
		}
	}
}
