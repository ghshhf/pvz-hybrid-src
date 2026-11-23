using Godot;

public sealed class GargantuarSmashComponent : CharacterComponentRuntime
{
	public string smashAudio = "GargantuarThump";

	public double cameraShakeDuration = 2.0;

	public double cameraShakeInterval = 0.05;

	public int cameraShakeCount = 4;

	private TowerDefenseZombieGargantuarBase _parent;

	private bool _configured;

	private double _runtimeDamageMultiplier = 1.0;

	public double RuntimeDamageMultiplier => _runtimeDamageMultiplier;

	public GargantuarSmashTargetMode TargetMode { get; private set; }

	private GargantuarSmashComponentDefinition Definition => base.ComponentDefinition as GargantuarSmashComponentDefinition;

	protected override void OnBound()
	{
		_parent = base.Owner as TowerDefenseZombieGargantuarBase;
		if (!_configured)
		{
			GargantuarSmashComponentDefinition definition = Definition;
			smashAudio = definition?.smashAudio ?? "GargantuarThump";
			TargetMode = definition?.targetMode ?? GargantuarSmashTargetMode.Grid;
			cameraShakeDuration = definition?.cameraShakeDuration ?? 2.0;
			cameraShakeInterval = definition?.cameraShakeInterval ?? 0.05;
			cameraShakeCount = definition?.cameraShakeCount ?? 4;
			_configured = true;
		}
	}

	protected override void OnAliveChanged(bool alive)
	{
		if (!alive)
		{
			ResetRuntimeDamageMultiplier();
		}
	}

	protected override void OnDetaching(ComponentDetachReason reason)
	{
		ResetRuntimeDamageMultiplier();
		_parent = null;
	}

	protected override void OnReleased()
	{
		ResetRuntimeDamageMultiplier();
		_parent = null;
	}

	internal void ResetRuntimeDamageMultiplier()
	{
		_runtimeDamageMultiplier = 1.0;
	}

	public bool TrySetRuntimeDamageMultiplier(double multiplier, out string error)
	{
		error = "";
		if (base.IsReleased || !GodotObject.IsInstanceValid(_parent) || !(_parent.config is TowerDefenseZombieConfig towerDefenseZombieConfig))
		{
			error = "Smash component is not bound to a live owner.";
			return false;
		}
		if ((!base.Alive || _parent.die || _parent.isDestroy) && multiplier != 1.0)
		{
			error = "An inactive or dead smash owner only accepts resetting its multiplier to one.";
			return false;
		}
		double num = towerDefenseZombieConfig.smashAttack * multiplier;
		if (!double.IsFinite(multiplier) || multiplier <= 0.0 || !double.IsFinite(num) || num < 0.0 || num > 3.4028234663852886E+38)
		{
			error = "Smash multiplier must be finite and positive, with a representable damage input.";
			return false;
		}
		_runtimeDamageMultiplier = multiplier;
		return true;
	}

	public void SmashAttack(int gridRadius = 0, Vector2I? gridCenter = null)
	{
		if (!base.Alive || base.Lifecycle != ComponentRuntimeLifecycle.Active || !GodotObject.IsInstanceValid(_parent))
		{
			return;
		}
		if (cameraShakeCount > 0 && cameraShakeDuration > 0.0 && GodotObject.IsInstanceValid(ViewManager.Instance))
		{
			Vector2 dir = new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0));
			ViewManager.Instance.CameraShake(dir, cameraShakeDuration, Mathf.Max(0.001, cameraShakeInterval), cameraShakeCount);
		}
		if (!string.IsNullOrEmpty(smashAudio) && GodotObject.IsInstanceValid(AudioManager.Instance))
		{
			AudioManager.Instance.AudioPlay(smashAudio);
		}
		if (Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return;
		}
		AttackComponent attackComponent = _parent.attackComponent;
		if (attackComponent == null || attackComponent.IsReleased || !(_parent.config is TowerDefenseZombieConfig towerDefenseZombieConfig))
		{
			return;
		}
		double num = towerDefenseZombieConfig.smashAttack * _runtimeDamageMultiplier;
		if (double.IsFinite(num) && !(num < 0.0) && !(num > 3.4028234663852886E+38))
		{
			if (TargetMode == GargantuarSmashTargetMode.WorldSpaceSingleTarget)
			{
				_parent.attackComponent.SmashCurrentTarget(num);
			}
			else if (gridRadius <= 0)
			{
				_parent.attackComponent.SmashAttackCell(num);
			}
			else
			{
				_parent.attackComponent.SmashAttackGridArea(num, gridCenter ?? _parent.gridPos, gridRadius);
			}
		}
	}
}
