using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using PVZHE.ModEditor.ModSystem;

public sealed class DestroyComponent : CharacterComponentRuntime
{
	private enum SpecialDestroyType
	{
		Ash,
		Smash
	}

	public const float DESTROY_DELAY = 1f;

	public const float SMASH_SCALE_Y = 0.25f;

	public float destroyDelay = 1f;

	public float smashScaleY = 0.25f;

	public StringName ashShaderParameter = "ash";

	public bool waitForPhysicsFrame = true;

	public bool pauseSpriteOnSpecialDestroy = true;

	public bool hideShadowOnSmash = true;

	public TowerDefenseCharacter parent;

	public bool isRemoteDestroy;

	private ulong _destroyVersion;

	private bool _configured;

	private DestroyComponentDefinition Definition => base.ComponentDefinition as DestroyComponentDefinition;

	protected override void OnBound()
	{
		parent = base.Owner;
		if (!_configured)
		{
			DestroyComponentDefinition definition = Definition;
			destroyDelay = definition?.destroyDelay ?? 1f;
			smashScaleY = definition?.smashScaleY ?? 0.25f;
			ashShaderParameter = definition?.ashShaderParameter ?? new StringName("ash");
			waitForPhysicsFrame = definition?.waitForPhysicsFrame ?? true;
			pauseSpriteOnSpecialDestroy = definition?.pauseSpriteOnSpecialDestroy ?? true;
			hideShadowOnSmash = definition?.hideShadowOnSmash ?? true;
			_configured = true;
		}
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		CancelPendingDestroy();
		parent = null;
	}

	protected override void OnReleased()
	{
		CancelPendingDestroy();
		parent = null;
	}

	protected override void OnAliveChanged(bool alive)
	{
		if (!alive)
		{
			CancelPendingDestroy();
		}
	}

	private void CancelPendingDestroy()
	{
		_destroyVersion++;
		isRemoteDestroy = false;
	}

	public void ShovelDestroy()
	{
		if (TryGetParent(out var character))
		{
			character.isShovel = true;
			Destroy();
		}
	}

	public void Destroy(bool freeInstance = true)
	{
		if (TryGetParent(out var _))
		{
			DestroyAsync(freeInstance, 0.0);
		}
	}

	public void DestroyWithVisualDelay(double delaySeconds)
	{
		if (TryGetParent(out var _))
		{
			DestroyAsync(freeInstance: true, Mathf.Max(0.0, delaySeconds));
		}
	}

	public void AbsorbDestroy(Vector2 center, double durationSeconds)
	{
		if (!TryGetParent(out var character) || character.isDestroy || !HasDestroyAuthority(character))
		{
			return;
		}
		double num = Mathf.Clamp(durationSeconds, 0.05, 10.0);
		SendNetworkOperation("absorb", 0L, new Dictionary
		{
			["x"] = center.X,
			["y"] = center.Y,
			["duration"] = num
		});
		character.suppressDeathrattles = true;
		character.skipDestroySet = true;
		character.CancelCellMoveTween();
		DestroyWithVisualDelay(num + 0.05);
		if (GodotObject.IsInstanceValid(character.transformPoint))
		{
			if (GodotObject.IsInstanceValid(character.sprite))
			{
				character.sprite.pause = true;
				character.sprite.forceLocalRender = true;
			}
			if (GodotObject.IsInstanceValid(character.shadowSprite))
			{
				character.shadowSprite.Visible = false;
			}
			Node2D visual = character.transformPoint;
			Vector2 startOffset = visual.GlobalPosition - center;
			Vector2 startScale = visual.Scale;
			float startRotation = visual.Rotation;
			Color startModulate = visual.Modulate;
			character.CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In)
				.TweenMethod(Callable.From(delegate(float progress)
				{
					float num2 = 1f - progress;
					float num3 = (float)Math.PI * 2f * progress;
					float num4 = num2 * num2;
					visual.GlobalPosition = center + startOffset.Rotated(num3) * num4;
					visual.Scale = startScale * num2;
					visual.Rotation = startRotation + num3 * 2f;
					visual.Modulate = new Color(startModulate, startModulate.A * num2);
				}), 0f, 1f, num);
		}
	}

	public void BlowAwayDestroy(Vector2 destination, double durationSeconds)
	{
		if (!TryGetParent(out var character) || character.isDestroy || !HasDestroyAuthority(character))
		{
			return;
		}
		double num = Mathf.Clamp(durationSeconds, 0.05, 10.0);
		SendNetworkOperation("blowAway", 0L, new Dictionary
		{
			["x"] = destination.X,
			["y"] = destination.Y,
			["duration"] = num
		});
		character.CancelCellMoveTween();
		character.skipDestroySet = true;
		Vector2 vector = destination - character.GetLogicalGlobalPosition();
		DestroyWithVisualDelay(num + 0.05);
		if (GodotObject.IsInstanceValid(character.transformPoint))
		{
			if (GodotObject.IsInstanceValid(character.sprite))
			{
				character.sprite.forceLocalRender = true;
			}
			if (GodotObject.IsInstanceValid(character.shadowSprite))
			{
				character.shadowSprite.Visible = false;
			}
			Node2D transformPoint = character.transformPoint;
			character.CreateTween().SetPauseMode(Tween.TweenPauseMode.Stop).TweenProperty(transformPoint, "global_position", transformPoint.GlobalPosition + vector, num);
		}
	}

	public override void ApplyNetworkOperation(string operationName, long sequence, Dictionary data)
	{
		if ((!(operationName != "absorb") || !(operationName != "blowAway")) && sequence >= 0 && data != null && TryGetParent(out var character) && !character.isDestroy)
		{
			isRemoteDestroy = true;
			Vector2 vector = new Vector2(data["x"].AsSingle(), data["y"].AsSingle());
			if (operationName == "blowAway")
			{
				BlowAwayDestroy(vector, data["duration"].AsDouble());
			}
			else
			{
				AbsorbDestroy(vector, data["duration"].AsDouble());
			}
		}
	}

	private async Task DestroyAsync(bool freeInstance, double visualReleaseDelaySeconds)
	{
		if (!TryGetParent(out var character) || character.isDestroy || !HasDestroyAuthority(character))
		{
			return;
		}
		ulong version = ++_destroyVersion;
		SceneTree tree = GetParentTreeOrNull(character);
		NotifyOwnerBeforeDestroy(character);
		MarkDestroyState(character);
		HitBoxDestroy();
		if (Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage")
		{
			character.EmitDestroy();
			UnregisterCharacter(character);
			QueueFreeParent(version);
			return;
		}
		float num = (GodotObject.IsInstanceValid(character.transformPoint) ? character.transformPoint.Scale.X : 1f);
		float num2 = (GodotObject.IsInstanceValid(character.instance) ? ((float)character.instance.hitpointScale) : 1f);
		character.EmitDestroy();
		if (GodotObject.IsInstanceValid(BattleEventBus.Instance))
		{
			BattleEventBus.Instance.EmitCharacterDestroy(character.packet, character.GetLogicalGlobalPosition(), character.gridPos, character.camp, num, num2);
		}
		if (freeInstance && !character.suppressDeathrattles)
		{
			ExecuteDeathEvents(character);
		}
		if (character.isExplode && (character.config == null || character.config.ashScene == null))
		{
			await SpecialDestroyAsync(SpecialDestroyType.Ash, tree, version, emitDestroy: false);
			return;
		}
		if (character.isSmash)
		{
			await SpecialDestroyAsync(SpecialDestroyType.Smash, tree, version, emitDestroy: false);
			return;
		}
		if (freeInstance)
		{
			UnregisterCharacter(character);
		}
		if (await ApplyDestroySetAsync(tree, version) && await WaitVisualReleaseDelayAsync(tree, version, visualReleaseDelaySeconds) && freeInstance)
		{
			QueueFreeParent(version);
		}
	}

	public void AshDestroy()
	{
		if (TryGetParent(out var _))
		{
			SpecialDestroyEntryAsync(SpecialDestroyType.Ash);
		}
	}

	public void SmashDestroy()
	{
		if (TryGetParent(out var _))
		{
			SpecialDestroyEntryAsync(SpecialDestroyType.Smash);
		}
	}

	private async Task SpecialDestroyEntryAsync(SpecialDestroyType type)
	{
		if (TryGetParent(out var character) && !character.isDestroy && HasDestroyAuthority(character))
		{
			ulong version = ++_destroyVersion;
			SceneTree parentTreeOrNull = GetParentTreeOrNull(character);
			NotifyOwnerBeforeDestroy(character);
			MarkDestroyState(character);
			HitBoxDestroy();
			await SpecialDestroyAsync(type, parentTreeOrNull, version, emitDestroy: true);
		}
	}

	private async Task SpecialDestroyAsync(SpecialDestroyType type, SceneTree tree, ulong version, bool emitDestroy)
	{
		if (!IsCurrent(version, out var character))
		{
			return;
		}
		if (emitDestroy)
		{
			character.EmitDestroy();
			if (!character.suppressDeathrattles && TowerDefenseManager.HasGameplayAuthority && (!Global.IsEditor || !(SceneManager.CurrentScene == "LevelEditorStage")))
			{
				ExecuteDeathEvents(character);
			}
		}
		UnregisterCharacter(character);
		if (!(await ApplyDestroySetAsync(tree, version)) || !IsCurrent(version, out character))
		{
			return;
		}
		if (character.inWater || (type == SpecialDestroyType.Smash && IsVase(character)))
		{
			QueueFreeParent(version);
			return;
		}
		if (type == SpecialDestroyType.Ash)
		{
			ApplyAshVisual(character);
		}
		else
		{
			ApplySmashVisual(character);
		}
		if (await WaitDestroyDelayAsync(tree, version))
		{
			QueueFreeParent(version);
		}
	}

	private async Task<bool> ApplyDestroySetAsync(SceneTree tree, ulong version)
	{
		if (!IsCurrent(version, out var character))
		{
			return false;
		}
		if ((GodotObject.IsInstanceValid(character.instance) && character.instance.hologram) || character.skipDestroySet || (character.suppressDeathrattles && !character.PreserveDeathTransformation) || !GodotObject.IsInstanceValid(tree))
		{
			return true;
		}
		if (FallingObjectReplicationPolicy.ShouldSuppressRemoteDestroySet(character))
		{
			return true;
		}
		using (FallingObjectReplicationPolicy.BeginCapture(character))
		{
			character.DestroySet();
		}
		TowerDefenseCharacter character2;
		if (!waitForPhysicsFrame)
		{
			return IsCurrent(version, out character2);
		}
		await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
		return IsCurrent(version, out character2);
	}

	private async Task<bool> WaitDestroyDelayAsync(SceneTree tree, ulong version)
	{
		float num = Mathf.Max(0f, destroyDelay);
		TowerDefenseCharacter character;
		if (num <= 0f || !GodotObject.IsInstanceValid(tree))
		{
			return IsCurrent(version, out character);
		}
		SceneTreeTimer source = tree.CreateTimer(num, processAlways: false);
		await tree.ToSignal(source, SceneTreeTimer.SignalName.Timeout);
		return IsCurrent(version, out character);
	}

	private async Task<bool> WaitVisualReleaseDelayAsync(SceneTree tree, ulong version, double delaySeconds)
	{
		TowerDefenseCharacter character;
		if (delaySeconds <= 0.0 || !GodotObject.IsInstanceValid(tree))
		{
			return IsCurrent(version, out character);
		}
		SceneTreeTimer source = tree.CreateTimer(delaySeconds, processAlways: false);
		await tree.ToSignal(source, SceneTreeTimer.SignalName.Timeout);
		return IsCurrent(version, out character);
	}

	private void ApplyAshVisual(TowerDefenseCharacter character)
	{
		if (!ashShaderParameter.IsEmpty)
		{
			character.SetSpriteGroupShaderParameter(ashShaderParameter.ToString(), true);
		}
		if (pauseSpriteOnSpecialDestroy && GodotObject.IsInstanceValid(character.sprite))
		{
			character.sprite.pause = true;
		}
	}

	private void ApplySmashVisual(TowerDefenseCharacter character)
	{
		if (pauseSpriteOnSpecialDestroy && GodotObject.IsInstanceValid(character.sprite))
		{
			character.sprite.pause = true;
		}
		if (hideShadowOnSmash && GodotObject.IsInstanceValid(character.shadowSprite))
		{
			character.shadowSprite.Visible = false;
		}
		if (GodotObject.IsInstanceValid(character.transformPoint))
		{
			character.transformPoint.Scale = new Vector2(character.transformPoint.Scale.X, Mathf.Max(0.01f, smashScaleY));
		}
	}

	private static bool IsVase(TowerDefenseCharacter character)
	{
		if (GodotObject.IsInstanceValid(character.instance))
		{
			return (character.instance.physiqueTypeFlags & 0x20) != 0;
		}
		return false;
	}

	private static void ExecuteDeathEvents(TowerDefenseCharacter character)
	{
		if (TowerDefenseManager.HasGameplayAuthority)
		{
			character.ExecuteReplacementDeathEvents();
		}
		int num = character.dieEvent?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			TowerDefenseCharacterEventBase towerDefenseCharacterEventBase = character.dieEvent[i];
			if (GodotObject.IsInstanceValid(towerDefenseCharacterEventBase) && !TowerDefenseCharacter.IsReplacementDeathEvent(towerDefenseCharacterEventBase))
			{
				towerDefenseCharacterEventBase.Execute(character.GetLogicalGlobalPosition(), character);
			}
		}
	}

	private static void MarkDestroyState(TowerDefenseCharacter character)
	{
		character.isDestroy = true;
		character.die = true;
		if (GodotObject.IsInstanceValid(character.instance))
		{
			character.instance.die = true;
		}
		XWModCharacters.Lifecycle(character, character.isShovel ? "shoveled" : "removed");
	}

	private static void NotifyOwnerBeforeDestroy(TowerDefenseCharacter character)
	{
		if (GodotObject.IsInstanceValid(character?.componentManager))
		{
			character.componentManager.NotifyOwnerBeforeDestroy();
		}
	}

	private static void UnregisterCharacter(TowerDefenseCharacter character)
	{
		if (GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			TowerDefenseManager.Instance.CharacterUnregister(character);
		}
		character.RemoveFromGroup("Character");
	}

	private bool HasDestroyAuthority(TowerDefenseCharacter character)
	{
		if (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost || isRemoteDestroy)
		{
			return true;
		}
		if (character is TowerDefensePlant)
		{
			return false;
		}
		if (character is TowerDefenseZombie towerDefenseZombie)
		{
			return towerDefenseZombie.syncId < 0;
		}
		return true;
	}

	private static SceneTree GetParentTreeOrNull(TowerDefenseCharacter character)
	{
		if (!GodotObject.IsInstanceValid(character) || !character.IsInsideTree())
		{
			return null;
		}
		SceneTree tree = character.GetTree();
		if (!GodotObject.IsInstanceValid(tree))
		{
			return null;
		}
		return tree;
	}

	private void QueueFreeParent(ulong version)
	{
		if (IsCurrent(version, out var character) && !character.IsQueuedForDeletion())
		{
			character.QueueFree();
		}
	}

	public void HitBoxDestroy()
	{
		if (TryGetParent(out var character))
		{
			character.DestroyHitBoxRuntime();
			TargetRegistrationComponent targetRegistrationComponent = character.targetRegistrationComponent;
			if (targetRegistrationComponent != null && !targetRegistrationComponent.IsReleased)
			{
				character.targetRegistrationComponent.UnregisterTarget();
				character.targetRegistrationComponent.canProjectileCheck = false;
			}
		}
	}

	private bool IsCurrent(ulong version, out TowerDefenseCharacter character)
	{
		character = null;
		if (version == _destroyVersion)
		{
			return TryGetParent(out character);
		}
		return false;
	}

	private bool TryGetParent(out TowerDefenseCharacter character)
	{
		character = parent;
		if (base.Alive && base.Lifecycle == ComponentRuntimeLifecycle.Active)
		{
			return GodotObject.IsInstanceValid(character);
		}
		return false;
	}
}
