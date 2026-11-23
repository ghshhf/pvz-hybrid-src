using System;
using Godot;
using Godot.Collections;

public sealed class ImpThrowerComponent : CharacterComponentRuntime
{
	private static readonly StringName SyncAliveKey = new StringName("_alive");

	private static readonly StringName HasThrownImpKey = new StringName("has_thrown_imp");

	public TowerDefenseZombieGargantuarBase parent;

	private double _syncLandPosX;

	private bool _syncDeserializing;

	private readonly Dictionary _syncPayload = new Dictionary();

	private bool _syncPayloadInitialized;

	private double _syncPayloadLandPosX;

	private bool _syncPayloadHasThrownImp;

	public bool HasThrownImp { get; private set; }

	private ImpThrowerComponentDefinition Definition => base.ComponentDefinition as ImpThrowerComponentDefinition;

	protected override void OnBound()
	{
		parent = base.Owner as TowerDefenseZombieGargantuarBase;
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		ResetRuntimeState();
		_syncPayload.Clear();
		_syncPayloadInitialized = false;
		_syncPayloadLandPosX = 0.0;
	}

	protected override void OnReleased()
	{
		ResetRuntimeState();
		_syncPayload.Clear();
		_syncPayloadInitialized = false;
		_syncPayloadLandPosX = 0.0;
	}

	private void ResetRuntimeState()
	{
		parent = null;
		HasThrownImp = false;
		_syncPayloadHasThrownImp = false;
		_syncLandPosX = 0.0;
		_syncDeserializing = false;
	}

	public void SpawnImp()
	{
		ImpThrowerComponentDefinition definition = Definition;
		if (!base.Alive || base.Lifecycle != ComponentRuntimeLifecycle.Active || definition == null || !GodotObject.IsInstanceValid(parent) || !GodotObject.IsInstanceValid(parent.impSpawnSlot) || !GodotObject.IsInstanceValid(parent.instance) || parent.isShow)
		{
			return;
		}
		if (Global.Instance.isMultiplayerMode && GodotObject.IsInstanceValid(MultiPlayerManager.Instance) && !MultiPlayerManager.Instance.isHost)
		{
			parent.impSpawnSlot.Update();
			return;
		}
		parent.impSpawnSlot.Update();
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(parent.impName);
		if (!GodotObject.IsInstanceValid(packetConfig) || !GodotObject.IsInstanceValid(TowerDefenseGroundItemBase.characterNode))
		{
			return;
		}
		Vector2 logicalGlobalPosition = parent.GetLogicalGlobalPosition(parent.impSpawnSlot);
		double num = parent.GetGroundHeight(logicalGlobalPosition.Y) - parent.groundHeight;
		Vector2 pos = new Vector2(logicalGlobalPosition.X, parent.GetLogicalGlobalPosition().Y);
		TowerDefenseZombieImpBase imp = packetConfig.Create(pos, parent.gridPos, num) as TowerDefenseZombieImpBase;
		if (!GodotObject.IsInstanceValid(imp))
		{
			return;
		}
		imp.ySpeed = definition.impVerticalSpeed;
		imp._throw = true;
		TowerDefenseEnum.CHARACTER_CAMP camp = parent.camp;
		imp.Scale = new Vector2((float)Mathf.Sign(parent.Scale.X) * Mathf.Abs(imp.Scale.X), imp.Scale.Y);
		double num2 = 0.0;
		if (_syncDeserializing)
		{
			num2 = _syncLandPosX;
			_syncDeserializing = false;
		}
		else
		{
			int x = Mathf.Min(definition.landingGridMin, definition.landingGridMax);
			int x2 = Mathf.Max(definition.landingGridMin, definition.landingGridMax);
			num2 = GD.RandRange(TowerDefenseManager.Instance.GetMapCellPos(new Vector2I(x, 0)).X, TowerDefenseManager.Instance.GetMapCellPos(new Vector2I(x2, 0)).X);
			if (TowerDefenseManager.CurrentControl?.process is TowerDefenseBattleProcessSandbox)
			{
				float num3 = ((parent.GetLogicalGlobalTransform(parent.transformPoint).X.X < 0f) ? 1 : (-1));
				num2 = (double)pos.X + (double)num3 * Math.Abs(num2 - (double)pos.X);
			}
		}
		_syncLandPosX = num2;
		double num4 = Mathf.Max(0.001, imp.GetFallTime());
		Dictionary dictionary = new Dictionary
		{
			["throw"] = true,
			["y_speed"] = definition.impVerticalSpeed,
			["land_pos_x"] = num2,
			["fall_duration"] = num4,
			["throw_ease"] = (int)definition.throwEase,
			["throw_transition"] = (int)definition.throwTransition
		};
		imp.ImportNetworkSpawnState(dictionary);
		TowerDefenseGroundItemBase.characterNode.AddChild(imp, forceReadableName: false, Node.InternalMode.Disabled);
		HasThrownImp = true;
		double hitpointScale = parent.instance.hitpointScale;
		Vector2 scale = parent.transformPoint.Scale;
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(imp))
			{
				imp.SetHitpointAndScale(hitpointScale, scale);
			}
		}).CallDeferred();
		imp.SetDeferred("invisible", parent.invisible);
		if (parent.instance.hypnoses)
		{
			imp.Hypnoses();
		}
		imp.camp = camp;
		if (Global.Instance.isMultiplayerMode && MultiPlayerManager.Instance.isHost)
		{
			TowerDefenseControlNew currentControl = TowerDefenseManager.Instance.currentControl;
			if (GodotObject.IsInstanceValid(currentControl))
			{
				int nextSyncId = currentControl.GetNextSyncId();
				currentControl.RegisterSyncCharacter(nextSyncId, imp);
				MultiPlayerManager.Instance.SendSpawnCharacterAt(parent.impName, parent.gridPos.X, parent.gridPos.Y, nextSyncId, hitpointScale, scale.X, parent.instance.hypnoses, 0.0, useCreate: true, pos.X, pos.Y, walkAfterSpawn: false, num, "", dictionary);
			}
		}
	}

	public void SetImpFilter(bool open = false)
	{
		if (GodotObject.IsInstanceValid(parent?.sprite))
		{
			parent.sprite.SetFliters((Godot.Collections.Array?)parent.impFliters, open);
		}
	}

	public override Dictionary SyncSerialize()
	{
		if (_syncPayloadInitialized)
		{
			if (_syncPayload.Count == 3)
			{
				_syncPayload.Remove(SyncAliveKey);
			}
			if (_syncPayload.Count == 2 && _syncPayloadLandPosX == _syncLandPosX && _syncPayloadHasThrownImp == HasThrownImp)
			{
				return _syncPayload;
			}
		}
		_syncPayload.Clear();
		_syncPayload["land_pos_x"] = _syncLandPosX;
		_syncPayload[HasThrownImpKey] = HasThrownImp;
		_syncPayloadLandPosX = _syncLandPosX;
		_syncPayloadHasThrownImp = HasThrownImp;
		_syncPayloadInitialized = true;
		return _syncPayload;
	}

	public override void SyncDeserialize(Dictionary data)
	{
		HasThrownImp = data.GetValueOrDefault(HasThrownImpKey, false).AsBool();
		_syncLandPosX = 0.0;
		_syncDeserializing = false;
		if (data.ContainsKey("land_pos_x"))
		{
			_syncLandPosX = data.GetValueOrDefault("land_pos_x", 0.0).AsDouble();
			_syncDeserializing = true;
		}
	}

	public override Dictionary ExportComponentSave()
	{
		Dictionary dictionary = base.ExportComponentSave();
		dictionary[HasThrownImpKey] = HasThrownImp;
		return dictionary;
	}

	public override void ImportComponentSave(Dictionary data, TowerDefenseLevelSaveConfigCSharp owner)
	{
		HasThrownImp = data.GetValueOrDefault(HasThrownImpKey, false).AsBool();
	}
}
