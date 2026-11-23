using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Other/KangarooBlack/Scene/TowerDefenseZombieKangarooBlack.cs")]
public class TowerDefenseZombieKangarooBlack : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName StopMoving = "StopMoving";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName WalkEntered = "WalkEntered";

		public new static readonly StringName AttackEntered = "AttackEntered";

		public new static readonly StringName OnAnimeStarted = "OnAnimeStarted";

		public new static readonly StringName InWater = "InWater";

		public new static readonly StringName SetZ = "SetZ";

		public static readonly StringName RefreshWaterLine = "RefreshWaterLine";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public static readonly StringName TickHop = "TickHop";

		public static readonly StringName StartJump = "StartJump";

		public static readonly StringName AdvanceJump = "AdvanceJump";

		public static readonly StringName Land = "Land";

		public static readonly StringName PlayIdle = "PlayIdle";

		public static readonly StringName UpdateAnimationSpeed = "UpdateAnimationSpeed";

		public static readonly StringName FindEventFrame = "FindEventFrame";

		public static readonly StringName CrushCell = "CrushCell";

		public static readonly StringName PlaySmashFeedback = "PlaySmashFeedback";

		public static readonly StringName CanThrow = "CanThrow";

		public static readonly StringName StartThrow = "StartThrow";

		public static readonly StringName EndThrow = "EndThrow";

		public static readonly StringName SetImpFilter = "SetImpFilter";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName ThrowBagImp = "ThrowBagImp";

		public new static readonly StringName SpawnDeathReplacement = "SpawnDeathReplacement";

		public static readonly StringName SpawnImpAt = "SpawnImpAt";

		public static readonly StringName HasGroundPlant = "HasGroundPlant";

		public static readonly StringName GetMapCellCenterX = "GetMapCellCenterX";

		public static readonly StringName ClipDuration = "ClipDuration";

		public new static readonly StringName DamagePointReach = "DamagePointReach";

		public static readonly StringName Enrage = "Enrage";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName impName = "impName";

		public static readonly StringName impSpawnSlot = "impSpawnSlot";

		public static readonly StringName impFireEvent = "impFireEvent";

		public static readonly StringName impFilter = "impFilter";

		public static readonly StringName impVerticalSpeed = "impVerticalSpeed";

		public static readonly StringName impLandingGridMin = "impLandingGridMin";

		public static readonly StringName impLandingGridMax = "impLandingGridMax";

		public static readonly StringName impThrowEase = "impThrowEase";

		public static readonly StringName impThrowTransition = "impThrowTransition";

		public static readonly StringName smashAudio = "smashAudio";

		public static readonly StringName cameraShakeDuration = "cameraShakeDuration";

		public static readonly StringName cameraShakeInterval = "cameraShakeInterval";

		public static readonly StringName cameraShakeCount = "cameraShakeCount";

		public static readonly StringName bagThrown = "bagThrown";

		public static readonly StringName _impFilter = "_impFilter";

		public static readonly StringName impFliters = "impFliters";

		public static readonly StringName _jumpClip = "_jumpClip";

		public static readonly StringName _throwClip = "_throwClip";

		public static readonly StringName _jumpTimer = "_jumpTimer";

		public static readonly StringName _jumping = "_jumping";

		public static readonly StringName _landed = "_landed";

		public static readonly StringName _takeoffFrame = "_takeoffFrame";

		public static readonly StringName _landingFrame = "_landingFrame";

		public static readonly StringName _lastHopFrame = "_lastHopFrame";

		public static readonly StringName _jumpStartX = "_jumpStartX";

		public static readonly StringName _jumpTargetX = "_jumpTargetX";

		public static readonly StringName _jumpTargetGrid = "_jumpTargetGrid";

		public static readonly StringName _throwing = "_throwing";

		public static readonly StringName _throwElapsed = "_throwElapsed";

		public static readonly StringName _bagThrown = "_bagThrown";

		public static readonly StringName _deathReplacementSpawned = "_deathReplacementSpawned";

		public static readonly StringName _refreshPouchVisual = "_refreshPouchVisual";

		public static readonly StringName _rageStages = "_rageStages";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private string _impFilter = "";

	public Array<string> impFliters = new Array<string>();

	private const double JumpInterval = 2.0;

	private const int ThrowColumn = 5;

	private const double ImpSpawnHeight = 40.0;

	private const double ThrowTimeoutPadding = 0.5;

	private readonly string _jumpClip = "Jump";

	private readonly string _throwClip = "Throw";

	private double _jumpTimer;

	private bool _jumping;

	private bool _landed;

	private int _takeoffFrame;

	private int _landingFrame;

	private ulong _lastHopFrame = 18446744073709551615uL;

	private float _jumpStartX;

	private float _jumpTargetX;

	private Vector2I _jumpTargetGrid;

	private bool _throwing;

	private double _throwElapsed;

	private bool _bagThrown;

	private bool _deathReplacementSpawned;

	private bool _refreshPouchVisual;

	private int _rageStages;

	[Export(PropertyHint.None, "")]
	public string impName { get; set; } = "";

	[Export(PropertyHint.None, "")]
	public AdobeAnimateSlot impSpawnSlot { get; set; }

	[Export(PropertyHint.None, "")]
	public string impFireEvent { get; set; } = "throw";

	[Export(PropertyHint.MultilineText, "")]
	public string impFilter
	{
		get
		{
			return _impFilter;
		}
		set
		{
			_impFilter = value;
			impFliters = new Array<string>(_impFilter.Split("&", StringSplitOptions.RemoveEmptyEntries));
		}
	}

	[Export(PropertyHint.None, "")]
	public double impVerticalSpeed { get; set; } = -240.0;

	[Export(PropertyHint.None, "")]
	public int impLandingGridMin { get; set; } = 1;

	[Export(PropertyHint.None, "")]
	public int impLandingGridMax { get; set; } = 1;

	[Export(PropertyHint.None, "")]
	public Tween.EaseType impThrowEase { get; set; } = Tween.EaseType.Out;

	[Export(PropertyHint.None, "")]
	public Tween.TransitionType impThrowTransition { get; set; } = Tween.TransitionType.Quad;

	[Export(PropertyHint.None, "")]
	public string smashAudio { get; set; } = "";

	[Export(PropertyHint.None, "")]
	public double cameraShakeDuration { get; set; } = 2.0;

	[Export(PropertyHint.None, "")]
	public double cameraShakeInterval { get; set; } = 0.05;

	[Export(PropertyHint.None, "")]
	public int cameraShakeCount { get; set; } = 4;

	public bool bagThrown => _bagThrown;

	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
		{
			base._Ready();
			_takeoffFrame = FindEventFrame("jump");
			_landingFrame = FindEventFrame("land");
			SetImpFilter(!_bagThrown);
			StopMoving();
		}
	}

	private void StopMoving()
	{
		GroundMoveComponent obj = groundMoveComponent;
		if (obj != null && !obj.IsReleased)
		{
			groundMoveComponent.SetAlive(value: false);
		}
	}

	public override void IdleProcessing(double delta)
	{
		TickHop(delta);
	}

	public override void WalkEntered()
	{
		StopMoving();
		if (!_jumping && !_throwing)
		{
			PlayIdle();
		}
	}

	public override void AttackEntered()
	{
		WalkEntered();
	}

	public override void OnAnimeStarted(string clipName)
	{
		base.OnAnimeStarted(clipName);
		SetImpFilter(!_bagThrown && !_deathReplacementSpawned);
		RefreshWaterLine();
		if (!die && !nearDie)
		{
			UpdateAnimationSpeed();
		}
	}

	public override void InWater()
	{
		base.InWater();
		RefreshWaterLine();
	}

	public override void SetZ()
	{
		base.SetZ();
		RefreshWaterLine();
	}

	private void RefreshWaterLine()
	{
		if (GodotObject.IsInstanceValid(base.waterLineSprite))
		{
			bool flag = GodotObject.IsInstanceValid(base.sprite) && base.sprite.clip == _jumpClip && base.sprite.frameIndex >= _takeoffFrame && base.sprite.frameIndex < _landingFrame;
			base.waterLineSprite.Visible = base.inWater && !flag && base.z <= base.groundHeight + 0.1;
		}
	}

	public override void WalkProcessing(double delta)
	{
		TickHop(delta);
	}

	public override void AttackProcessing(double delta)
	{
		if (TowerDefenseZombie.IsZombieRewindActive())
		{
			Walk();
			return;
		}
		BlowBackComponent obj = blowBackComponent;
		if (obj != null && !obj.IsReleased && blowBackComponent.blowBack)
		{
			WalkWithoutTransitionDelay();
		}
		else
		{
			TickHop(delta);
		}
	}

	private void TickHop(double delta)
	{
		if (Engine.IsEditorHint() || !GodotObject.IsInstanceValid(base.sprite))
		{
			return;
		}
		RefreshWaterLine();
		if (_refreshPouchVisual)
		{
			SetImpFilter(!_bagThrown && !_deathReplacementSpawned);
			_refreshPouchVisual = false;
		}
		if (!inGame || die || nearDie || isDestroy || isRise)
		{
			return;
		}
		UpdateAnimationSpeed();
		StopMoving();
		if (!TowerDefenseManager.HasGameplayAuthority || !CanRunAbility())
		{
			return;
		}
		ulong physicsFrames = Engine.GetPhysicsFrames();
		if (_lastHopFrame == physicsFrames)
		{
			return;
		}
		_lastHopFrame = physicsFrames;
		double num = delta * timeScale;
		if (_throwing)
		{
			_throwElapsed += num;
			if (_throwElapsed >= ClipDuration(_throwClip) + 0.5)
			{
				EndThrow();
			}
			return;
		}
		_jumpTimer += num;
		if (_jumping)
		{
			AdvanceJump();
		}
		else if (CanThrow())
		{
			StartThrow();
		}
		else if (!(_jumpTimer + 0.0001 < 2.0))
		{
			StartJump();
		}
	}

	private void StartJump()
	{
		Vector2I vector2I = base.gridPos;
		bool flag = HasGroundPlant(vector2I);
		int num = (instance.hypnoses ? 1 : (-1));
		if (TowerDefenseZombie.IsZombieRewindActive())
		{
			num = -num;
		}
		_jumpTargetGrid = (flag ? vector2I : new Vector2I(vector2I.X + num, vector2I.Y));
		_jumpStartX = GetLogicalGlobalPosition().X;
		_jumpTargetX = (flag ? _jumpStartX : GetMapCellCenterX(_jumpTargetGrid));
		_landed = false;
		_jumping = true;
		_jumpTimer = 0.0;
		base.sprite.SetAnimation(_jumpClip, loop: false);
		UpdateAnimationSpeed();
	}

	private void AdvanceJump()
	{
		if (!_landed && !(base.sprite.clip != _jumpClip))
		{
			double num = Mathf.Clamp(((double)base.sprite.frameIndex + base.sprite.elapsedTimer - (double)_takeoffFrame) / (double)Mathf.Max(1, _landingFrame - _takeoffFrame), 0.0, 1.0);
			ulong currentPhysicsFrame = TowerDefenseProcessModeDispatch.CurrentPhysicsFrame;
			Vector2 globalPositionForPhysicsFrame = GetGlobalPositionForPhysicsFrame(currentPhysicsFrame);
			globalPositionForPhysicsFrame.X = Mathf.Lerp(_jumpStartX, _jumpTargetX, (float)num);
			SetGlobalPositionForPhysicsFrame(globalPositionForPhysicsFrame, currentPhysicsFrame);
			base.gridPos = TowerDefenseManager.Instance.GetMapGridPos(globalPositionForPhysicsFrame);
		}
	}

	private void Land()
	{
		if (_jumping && !_landed && TowerDefenseManager.HasGameplayAuthority && !die && !nearDie)
		{
			_landed = true;
			base.gridPos = _jumpTargetGrid;
			Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
			logicalGlobalPosition.X = _jumpTargetX;
			SetLogicalGlobalPosition(logicalGlobalPosition);
			CrushCell(base.gridPos);
		}
	}

	private void PlayIdle()
	{
		base.sprite.SetAnimation(idleAnimeClip);
		UpdateAnimationSpeed();
	}

	private void UpdateAnimationSpeed()
	{
		base.sprite.timeScale = timeScale * ((base.sprite.clip == _jumpClip) ? (ClipDuration(_jumpClip) / 2.0) : 1.0);
	}

	private int FindEventFrame(string command)
	{
		Array<Godot.Collections.Array> events = base.sprite.flashAnimeData.events;
		for (int i = 0; i < events.Count; i++)
		{
			foreach (Variant item in events[i])
			{
				if (item.AsGodotDictionary().GetValueOrDefault("Command", "").AsString() == command)
				{
					return i;
				}
			}
		}
		return 0;
	}

	private void CrushCell(Vector2I cell)
	{
		AttackComponent obj = attackComponent;
		if (obj != null && !obj.IsReleased)
		{
			PlaySmashFeedback();
			attackComponent.SmashAttackCell(((TowerDefenseZombieConfig)base.config).smashAttack, cell);
		}
	}

	private void PlaySmashFeedback()
	{
		if (cameraShakeCount > 0 && cameraShakeDuration > 0.0 && GodotObject.IsInstanceValid(ViewManager.Instance))
		{
			Vector2 dir = new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0));
			ViewManager.Instance.CameraShake(dir, cameraShakeDuration, Mathf.Max(0.001, cameraShakeInterval), cameraShakeCount);
		}
		if (!string.IsNullOrEmpty(smashAudio) && GodotObject.IsInstanceValid(AudioManager.Instance))
		{
			AudioManager.Instance.AudioPlay(smashAudio);
		}
	}

	private bool CanThrow()
	{
		if (!_bagThrown && !_throwing && base.gridPos.X == 5)
		{
			return GodotObject.IsInstanceValid(TowerDefenseManager.GetPacketConfig(impName));
		}
		return false;
	}

	private void StartThrow()
	{
		_throwing = true;
		_throwElapsed = 0.0;
		_jumpTimer = 0.0;
		base.sprite.SetAnimation(_throwClip, loop: false, 0.2);
		UpdateAnimationSpeed();
	}

	private void EndThrow()
	{
		_throwing = false;
		_throwElapsed = 0.0;
		_jumpTimer = 0.0;
		PlayIdle();
	}

	public void SetImpFilter(bool open = false)
	{
		if (GodotObject.IsInstanceValid(base.sprite))
		{
			base.sprite.SetFliters((Godot.Collections.Array?)impFliters, open);
		}
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (command == "jump" || command == "land")
		{
			if (GodotObject.IsInstanceValid(base.waterLineSprite))
			{
				base.waterLineSprite.Visible = base.inWater && command == "land";
			}
			if (inGame && base.inWater && !die && !nearDie)
			{
				CreateSplash();
			}
		}
		if (!(command == "jump"))
		{
			if (command == "land")
			{
				Land();
			}
			else if (!string.IsNullOrEmpty(impFireEvent) && !(command != impFireEvent) && _throwing && !_bagThrown && !die && !nearDie && TowerDefenseManager.HasGameplayAuthority)
			{
				_bagThrown = true;
				ThrowBagImp();
				SetImpFilter();
			}
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (die || nearDie || isDestroy || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		if (clip == _throwClip)
		{
			if (_throwing)
			{
				EndThrow();
			}
		}
		else if (clip == _jumpClip && _jumping)
		{
			_jumping = false;
			PlayIdle();
		}
	}

	private void ThrowBagImp()
	{
		if (!GodotObject.IsInstanceValid(impSpawnSlot) || !GodotObject.IsInstanceValid(instance) || !GodotObject.IsInstanceValid(TowerDefenseManager.GetCharacterNode()))
		{
			return;
		}
		impSpawnSlot.Update();
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(impName);
		if (!GodotObject.IsInstanceValid(packetConfig) || (Global.IsMultiplayerMode && GodotObject.IsInstanceValid(MultiPlayerManager.Instance) && !MultiPlayerManager.Instance.isHost))
		{
			return;
		}
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition(impSpawnSlot);
		double height = GetGroundHeight(logicalGlobalPosition.Y) - base.groundHeight;
		Vector2 vector = new Vector2(logicalGlobalPosition.X, GetLogicalGlobalPosition().Y);
		TowerDefenseZombieImpBase towerDefenseZombieImpBase = packetConfig.Create(vector, base.gridPos, height) as TowerDefenseZombieImpBase;
		if (!GodotObject.IsInstanceValid(towerDefenseZombieImpBase))
		{
			return;
		}
		towerDefenseZombieImpBase.ySpeed = impVerticalSpeed;
		towerDefenseZombieImpBase._throw = true;
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseZombieImpBase, forceReadableName: false, InternalMode.Disabled);
		double hitpointScale = instance.hitpointScale;
		Vector2 scale = transformPoint.Scale.Abs();
		towerDefenseZombieImpBase.instance.hitpointScale = hitpointScale;
		towerDefenseZombieImpBase.transformPoint.Scale = scale;
		towerDefenseZombieImpBase.invisible = base.invisible;
		if (instance.hypnoses)
		{
			towerDefenseZombieImpBase.Hypnoses();
		}
		towerDefenseZombieImpBase.Fly();
		towerDefenseZombieImpBase.sprite.SetAnimation(towerDefenseZombieImpBase.flyAnimeClip, loop: false);
		if (towerDefenseZombieImpBase.sprite.flashAnimeData.layerDictionary.TryGetValue("Zombie_imp_body1", out var value) && towerDefenseZombieImpBase.sprite.TryGetInterpolatedSlotPose(value.AsInt32(), out var _, out var transform))
		{
			Vector2 vector2 = towerDefenseZombieImpBase.GetLogicalGlobalTransform(towerDefenseZombieImpBase.sprite) * (transform.Origin + towerDefenseZombieImpBase.sprite.offset);
			Vector2 vector3 = logicalGlobalPosition - vector2;
			vector.X += vector3.X;
			towerDefenseZombieImpBase.SetLogicalGlobalPosition(vector);
			towerDefenseZombieImpBase.z -= vector3.Y / Mathf.Max(0.001f, Mathf.Abs(towerDefenseZombieImpBase.GlobalScale.Y));
			height = towerDefenseZombieImpBase.z;
		}
		int x = Mathf.Min(impLandingGridMin, impLandingGridMax);
		int x2 = Mathf.Max(impLandingGridMin, impLandingGridMax);
		double num = GD.RandRange(GetMapCellCenterX(new Vector2I(x, base.gridPos.Y)), GetMapCellCenterX(new Vector2I(x2, base.gridPos.Y)));
		if (instance.hypnoses)
		{
			num = (double)vector.X + Math.Abs(num - (double)vector.X);
		}
		double num2 = Mathf.Max(0.001, towerDefenseZombieImpBase.GetFallTime());
		Dictionary dictionary = new Dictionary
		{
			["throw"] = true,
			["y_speed"] = impVerticalSpeed,
			["land_pos_x"] = num,
			["fall_duration"] = num2,
			["throw_ease"] = (int)impThrowEase,
			["throw_transition"] = (int)impThrowTransition
		};
		towerDefenseZombieImpBase.ImportNetworkSpawnState(dictionary);
		if (Global.IsMultiplayerMode && MultiPlayerManager.Instance.isHost)
		{
			TowerDefenseControlNew currentControl = TowerDefenseManager.Instance.currentControl;
			if (GodotObject.IsInstanceValid(currentControl))
			{
				int nextSyncId = currentControl.GetNextSyncId();
				currentControl.RegisterSyncCharacter(nextSyncId, towerDefenseZombieImpBase);
				MultiPlayerManager.Instance.SendSpawnCharacterAt(impName, base.gridPos.X, base.gridPos.Y, nextSyncId, hitpointScale, scale.X, instance.hypnoses, 0.0, useCreate: true, vector.X, vector.Y, walkAfterSpawn: false, height, "", dictionary);
			}
		}
	}

	public override void SpawnDeathReplacement()
	{
		if (_deathReplacementSpawned)
		{
			return;
		}
		_deathReplacementSpawned = true;
		_jumping = false;
		_throwing = false;
		_throwElapsed = 0.0;
		if (!_bagThrown)
		{
			SetImpFilter();
		}
		if (TowerDefenseManager.HasGameplayAuthority)
		{
			int num = (_bagThrown ? 1 : 2);
			Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
			for (int i = 0; i < num; i++)
			{
				SpawnImpAt(logicalGlobalPosition);
			}
		}
	}

	private void SpawnImpAt(Vector2 logicalPosition)
	{
		TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(impName);
		if (!GodotObject.IsInstanceValid(packetConfig))
		{
			return;
		}
		TowerDefenseZombie imp = packetConfig.Create(logicalPosition, base.gridPos, 40.0) as TowerDefenseZombie;
		if (!GodotObject.IsInstanceValid(imp))
		{
			return;
		}
		double hitpointScale = instance.hitpointScale;
		Vector2 scale = transformPoint.Scale.Abs();
		bool hypnotized = instance.hypnoses;
		bool hidden = base.invisible;
		Vector2I cell = base.gridPos;
		string packetName = impName;
		Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(imp))
			{
				Node2D node2D = TowerDefenseManager.GetCharacterNode();
				if (!GodotObject.IsInstanceValid(node2D))
				{
					imp.Free();
				}
				else
				{
					node2D.AddChild(imp, forceReadableName: false, InternalMode.Disabled);
					if (GodotObject.IsInstanceValid(imp.instance))
					{
						imp.instance.hitpointScale = hitpointScale;
					}
					if (GodotObject.IsInstanceValid(imp.transformPoint))
					{
						imp.transformPoint.Scale = scale;
					}
					imp.invisible = hidden;
					if (hypnotized)
					{
						imp.Hypnoses();
					}
					if (imp != null)
					{
						TowerDefenseZombie towerDefenseZombie = imp;
						towerDefenseZombie.WalkReady();
					}
					if (Global.IsMultiplayerMode && MultiPlayerManager.IsHost)
					{
						TowerDefenseControlNew currentControl = TowerDefenseManager.Instance.currentControl;
						int nextSyncId = currentControl.GetNextSyncId();
						currentControl.RegisterSyncCharacter(nextSyncId, imp);
						MultiPlayerManager.Instance.SendSpawnCharacterAt(packetName, cell.X, cell.Y, nextSyncId, hitpointScale, scale.X, hypnotized, 0.0, useCreate: true, logicalPosition.X, logicalPosition.Y, walkAfterSpawn: false, 40.0);
					}
				}
			}
		}).CallDeferred();
	}

	private bool HasGroundPlant(Vector2I cell)
	{
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(cell);
		if (!GodotObject.IsInstanceValid(mapCell))
		{
			return false;
		}
		List<TowerDefenseCharacter> characterList = mapCell.characterList;
		for (int i = 0; i < characterList.Count; i++)
		{
			TowerDefenseCharacter towerDefenseCharacter = characterList[i];
			if (GodotObject.IsInstanceValid(towerDefenseCharacter) && !towerDefenseCharacter.die && !towerDefenseCharacter.isDestroy && towerDefenseCharacter is TowerDefensePlant towerDefensePlant && GodotObject.IsInstanceValid(towerDefensePlant.instance) && !towerDefensePlant.instance.invincible && towerDefensePlant.instance.canBeCollection && towerDefensePlant.HasHitBox && towerDefensePlant.CanTarget(this) && CanCollision(towerDefensePlant.instance.maskFlags))
			{
				return true;
			}
		}
		return false;
	}

	private float GetMapCellCenterX(Vector2I cell)
	{
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(towerDefenseManager))
		{
			return GetLogicalGlobalPosition().X;
		}
		return towerDefenseManager.GetMapCellPosCenter(cell).X;
	}

	private double ClipDuration(string clip)
	{
		AdobeAnimateData adobeAnimateData = base.sprite?.flashAnimeData;
		if (adobeAnimateData == null || string.IsNullOrEmpty(clip))
		{
			return 1.0;
		}
		if (!adobeAnimateData.clips.TryGetValue(clip, out var value))
		{
			return 1.0;
		}
		Vector2I vector2I = value.AsVector2I();
		double num = ((adobeAnimateData.frameRate > 0.0) ? adobeAnimateData.frameRate : 12.0);
		return (double)Mathf.Max(1, vector2I.Y - vector2I.X) / num;
	}

	public override void DamagePointReach(string damagePointName)
	{
		base.DamagePointReach(damagePointName);
		if (damagePointName == null)
		{
			return;
		}
		int num;
		switch (damagePointName.Length)
		{
		default:
			return;
		case 7:
		{
			char c = damagePointName[6];
			if (c != '2')
			{
				if (c != '3' || !(damagePointName == "Armor_3"))
				{
					return;
				}
				goto IL_018c;
			}
			if (!(damagePointName == "Armor_2"))
			{
				return;
			}
			goto IL_0188;
		}
		case 10:
		{
			char c = damagePointName[0];
			if (c != 'S')
			{
				if (c != 'U')
				{
					return;
				}
				if (damagePointName == "Upperleg_2")
				{
					goto IL_0188;
				}
				if (!(damagePointName == "Upperleg_3"))
				{
					return;
				}
			}
			else
			{
				if (damagePointName == "Shoulder_2")
				{
					goto IL_0188;
				}
				if (!(damagePointName == "Shoulder_3"))
				{
					return;
				}
			}
			goto IL_018c;
		}
		case 5:
			switch (damagePointName[0])
			{
			default:
				return;
			case 'A':
				break;
			case 'B':
				goto IL_011c;
			case 'E':
				goto IL_0137;
			case 'C':
			case 'D':
				return;
			}
			if (damagePointName == "Arm_2")
			{
				goto IL_0188;
			}
			if (!(damagePointName == "Arm_3"))
			{
				return;
			}
			goto IL_018c;
		case 6:
		{
			char c = damagePointName[0];
			if (c != 'B')
			{
				if (c != 'H')
				{
					return;
				}
				if (damagePointName == "Head_2")
				{
					goto IL_0188;
				}
				if (!(damagePointName == "Head_3"))
				{
					return;
				}
			}
			else
			{
				if (damagePointName == "Body_2")
				{
					goto IL_0188;
				}
				if (!(damagePointName == "Body_3"))
				{
					return;
				}
			}
			goto IL_018c;
		}
		case 8:
		case 9:
			return;
			IL_018c:
			num = 2;
			break;
			IL_011c:
			if (damagePointName == "Bag_2")
			{
				goto IL_0188;
			}
			if (!(damagePointName == "Bag_3"))
			{
				return;
			}
			goto IL_018c;
			IL_0188:
			num = 1;
			break;
			IL_0137:
			if (damagePointName == "Ear_2")
			{
				goto IL_0188;
			}
			if (!(damagePointName == "Ear_3"))
			{
				return;
			}
			goto IL_018c;
		}
		if ((_rageStages & num) != 0)
		{
			return;
		}
		_rageStages |= num;
		if (!TowerDefenseManager.HasGameplayAuthority || !inGame)
		{
			return;
		}
		if (instance.hypnoses)
		{
			SunCreate(GetLogicalGlobalPosition(), 25L, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, new Vector2(GD.RandRange(-50, 50), -400f));
		}
		else
		{
			BrainSunCreate(GetLogicalGlobalPosition(), 25L, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, new Vector2(GD.RandRange(-50, 50), -400f));
		}
		foreach (Variant item in TowerDefenseManager.Instance.GetZombie())
		{
			TowerDefenseZombie towerDefenseZombie = (TowerDefenseZombie)(GodotObject)item;
			if (GodotObject.IsInstanceValid(towerDefenseZombie) && !towerDefenseZombie.die && !towerDefenseZombie.nearDie && !towerDefenseZombie.isDestroy && towerDefenseZombie.camp == base.camp && Mathf.Abs(towerDefenseZombie.gridPos.X - base.gridPos.X) <= 1 && Mathf.Abs(towerDefenseZombie.gridPos.Y - base.gridPos.Y) <= 1)
			{
				Enrage(towerDefenseZombie);
			}
		}
	}

	private static void Enrage(TowerDefenseCharacter target)
	{
		target.BuffAdd(new TowerDefenseCharacterBuffRedHeat
		{
			time = 5.0
		});
		target.BuffAdd(new TowerDefenseCharacterBuffOverallHaste
		{
			key = "KangarooRageHaste",
			time = 5.0,
			timeScaleValue = 2.0
		});
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary obj = base.ExportVariantSave() ?? new Dictionary();
		obj["kangarooBagThrown"] = _bagThrown;
		obj["jumping"] = _jumping;
		obj["landed"] = _landed;
		obj["jumpTimer"] = _jumpTimer;
		obj["jumpStartX"] = _jumpStartX;
		obj["jumpTargetX"] = _jumpTargetX;
		obj["jumpTargetColumn"] = _jumpTargetGrid.X;
		obj["jumpTargetRow"] = _jumpTargetGrid.Y;
		obj["throwing"] = _throwing;
		obj["throwElapsed"] = _throwElapsed;
		obj["deathReplacementSpawned"] = _deathReplacementSpawned;
		obj["rageStages"] = _rageStages;
		return obj;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_bagThrown = data.GetValueOrDefault("kangarooBagThrown", false).AsBool();
		_jumping = data.GetValueOrDefault("jumping", false).AsBool();
		_landed = data.GetValueOrDefault("landed", false).AsBool();
		_jumpTimer = data.GetValueOrDefault("jumpTimer", 0.0).AsDouble();
		_jumpStartX = data.GetValueOrDefault("jumpStartX", 0.0).AsSingle();
		_jumpTargetX = data.GetValueOrDefault("jumpTargetX", 0.0).AsSingle();
		_jumpTargetGrid = new Vector2I(data.GetValueOrDefault("jumpTargetColumn", 0).AsInt32(), data.GetValueOrDefault("jumpTargetRow", 0).AsInt32());
		_throwing = data.GetValueOrDefault("throwing", false).AsBool();
		_throwElapsed = data.GetValueOrDefault("throwElapsed", 0.0).AsDouble();
		_deathReplacementSpawned = data.GetValueOrDefault("deathReplacementSpawned", false).AsBool();
		_rageStages = data.GetValueOrDefault("rageStages", 0).AsInt32();
		SetImpFilter(!_bagThrown && !_deathReplacementSpawned);
		_refreshPouchVisual = true;
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return new Dictionary
		{
			["bagThrown"] = _bagThrown,
			["deathReplacementSpawned"] = _deathReplacementSpawned,
			["rageStages"] = _rageStages
		};
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		_bagThrown = data.GetValueOrDefault("bagThrown", _bagThrown).AsBool();
		_deathReplacementSpawned = data.GetValueOrDefault("deathReplacementSpawned", _deathReplacementSpawned).AsBool();
		_rageStages = data.GetValueOrDefault("rageStages", 0).AsInt32();
		SetImpFilter(!_bagThrown && !_deathReplacementSpawned);
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return (int)(((_bagThrown ? 1u : 0u) | (uint)(_deathReplacementSpawned ? 2 : 0)) * 4) + _rageStages;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(39)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StopMoving, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttackEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnAnimeStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clipName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.InWater, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetZ, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RefreshWaterLine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TickHop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StartJump, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AdvanceJump, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Land, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlayIdle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateAnimationSpeed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FindEventFrame, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CrushCell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlaySmashFeedback, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanThrow, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartThrow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EndThrow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetImpFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "open", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ThrowBagImp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SpawnDeathReplacement, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SpawnImpAt, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "logicalPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.HasGroundPlant, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetMapCellCenterX, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ClipDuration, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DamagePointReach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "damagePointName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Enrage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.StopMoving && args.Count == 0)
		{
			StopMoving();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WalkEntered && args.Count == 0)
		{
			WalkEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttackEntered && args.Count == 0)
		{
			AttackEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimeStarted && args.Count == 1)
		{
			OnAnimeStarted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InWater && args.Count == 0)
		{
			InWater();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetZ && args.Count == 0)
		{
			SetZ();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshWaterLine && args.Count == 0)
		{
			RefreshWaterLine();
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
		if (method == MethodName.TickHop && args.Count == 1)
		{
			TickHop(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartJump && args.Count == 0)
		{
			StartJump();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AdvanceJump && args.Count == 0)
		{
			AdvanceJump();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Land && args.Count == 0)
		{
			Land();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayIdle && args.Count == 0)
		{
			PlayIdle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateAnimationSpeed && args.Count == 0)
		{
			UpdateAnimationSpeed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FindEventFrame && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<int>(FindEventFrame(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.CrushCell && args.Count == 1)
		{
			CrushCell(VariantUtils.ConvertTo<Vector2I>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlaySmashFeedback && args.Count == 0)
		{
			PlaySmashFeedback();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanThrow && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanThrow());
			return true;
		}
		if (method == MethodName.StartThrow && args.Count == 0)
		{
			StartThrow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndThrow && args.Count == 0)
		{
			EndThrow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetImpFilter && args.Count == 1)
		{
			SetImpFilter(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ThrowBagImp && args.Count == 0)
		{
			ThrowBagImp();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnDeathReplacement && args.Count == 0)
		{
			SpawnDeathReplacement();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnImpAt && args.Count == 1)
		{
			SpawnImpAt(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasGroundPlant && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(HasGroundPlant(VariantUtils.ConvertTo<Vector2I>(in args[0])));
			return true;
		}
		if (method == MethodName.GetMapCellCenterX && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetMapCellCenterX(VariantUtils.ConvertTo<Vector2I>(in args[0])));
			return true;
		}
		if (method == MethodName.ClipDuration && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<double>(ClipDuration(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.DamagePointReach && args.Count == 1)
		{
			DamagePointReach(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enrage && args.Count == 1)
		{
			Enrage(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Enrage && args.Count == 1)
		{
			Enrage(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.StopMoving)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.WalkEntered)
		{
			return true;
		}
		if (method == MethodName.AttackEntered)
		{
			return true;
		}
		if (method == MethodName.OnAnimeStarted)
		{
			return true;
		}
		if (method == MethodName.InWater)
		{
			return true;
		}
		if (method == MethodName.SetZ)
		{
			return true;
		}
		if (method == MethodName.RefreshWaterLine)
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
		if (method == MethodName.TickHop)
		{
			return true;
		}
		if (method == MethodName.StartJump)
		{
			return true;
		}
		if (method == MethodName.AdvanceJump)
		{
			return true;
		}
		if (method == MethodName.Land)
		{
			return true;
		}
		if (method == MethodName.PlayIdle)
		{
			return true;
		}
		if (method == MethodName.UpdateAnimationSpeed)
		{
			return true;
		}
		if (method == MethodName.FindEventFrame)
		{
			return true;
		}
		if (method == MethodName.CrushCell)
		{
			return true;
		}
		if (method == MethodName.PlaySmashFeedback)
		{
			return true;
		}
		if (method == MethodName.CanThrow)
		{
			return true;
		}
		if (method == MethodName.StartThrow)
		{
			return true;
		}
		if (method == MethodName.EndThrow)
		{
			return true;
		}
		if (method == MethodName.SetImpFilter)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ThrowBagImp)
		{
			return true;
		}
		if (method == MethodName.SpawnDeathReplacement)
		{
			return true;
		}
		if (method == MethodName.SpawnImpAt)
		{
			return true;
		}
		if (method == MethodName.HasGroundPlant)
		{
			return true;
		}
		if (method == MethodName.GetMapCellCenterX)
		{
			return true;
		}
		if (method == MethodName.ClipDuration)
		{
			return true;
		}
		if (method == MethodName.DamagePointReach)
		{
			return true;
		}
		if (method == MethodName.Enrage)
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
		if (method == MethodName.ImportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.impName)
		{
			impName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.impSpawnSlot)
		{
			impSpawnSlot = VariantUtils.ConvertTo<AdobeAnimateSlot>(in value);
			return true;
		}
		if (name == PropertyName.impFireEvent)
		{
			impFireEvent = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.impFilter)
		{
			impFilter = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.impVerticalSpeed)
		{
			impVerticalSpeed = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.impLandingGridMin)
		{
			impLandingGridMin = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.impLandingGridMax)
		{
			impLandingGridMax = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.impThrowEase)
		{
			impThrowEase = VariantUtils.ConvertTo<Tween.EaseType>(in value);
			return true;
		}
		if (name == PropertyName.impThrowTransition)
		{
			impThrowTransition = VariantUtils.ConvertTo<Tween.TransitionType>(in value);
			return true;
		}
		if (name == PropertyName.smashAudio)
		{
			smashAudio = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.cameraShakeDuration)
		{
			cameraShakeDuration = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.cameraShakeInterval)
		{
			cameraShakeInterval = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.cameraShakeCount)
		{
			cameraShakeCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._impFilter)
		{
			_impFilter = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.impFliters)
		{
			impFliters = VariantUtils.ConvertToArray<string>(in value);
			return true;
		}
		if (name == PropertyName._jumpTimer)
		{
			_jumpTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._jumping)
		{
			_jumping = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._landed)
		{
			_landed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._takeoffFrame)
		{
			_takeoffFrame = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._landingFrame)
		{
			_landingFrame = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._lastHopFrame)
		{
			_lastHopFrame = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._jumpStartX)
		{
			_jumpStartX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._jumpTargetX)
		{
			_jumpTargetX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._jumpTargetGrid)
		{
			_jumpTargetGrid = VariantUtils.ConvertTo<Vector2I>(in value);
			return true;
		}
		if (name == PropertyName._throwing)
		{
			_throwing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._throwElapsed)
		{
			_throwElapsed = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._bagThrown)
		{
			_bagThrown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._deathReplacementSpawned)
		{
			_deathReplacementSpawned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._refreshPouchVisual)
		{
			_refreshPouchVisual = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._rageStages)
		{
			_rageStages = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		string from;
		if (name == PropertyName.impName)
		{
			from = impName;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.impSpawnSlot)
		{
			value = VariantUtils.CreateFrom<AdobeAnimateSlot>(impSpawnSlot);
			return true;
		}
		if (name == PropertyName.impFireEvent)
		{
			from = impFireEvent;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.impFilter)
		{
			from = impFilter;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		double from2;
		if (name == PropertyName.impVerticalSpeed)
		{
			from2 = impVerticalSpeed;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		int from3;
		if (name == PropertyName.impLandingGridMin)
		{
			from3 = impLandingGridMin;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.impLandingGridMax)
		{
			from3 = impLandingGridMax;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.impThrowEase)
		{
			value = VariantUtils.CreateFrom<Tween.EaseType>(impThrowEase);
			return true;
		}
		if (name == PropertyName.impThrowTransition)
		{
			value = VariantUtils.CreateFrom<Tween.TransitionType>(impThrowTransition);
			return true;
		}
		if (name == PropertyName.smashAudio)
		{
			from = smashAudio;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.cameraShakeDuration)
		{
			from2 = cameraShakeDuration;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.cameraShakeInterval)
		{
			from2 = cameraShakeInterval;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.cameraShakeCount)
		{
			from3 = cameraShakeCount;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.bagThrown)
		{
			value = VariantUtils.CreateFrom<bool>(bagThrown);
			return true;
		}
		if (name == PropertyName._impFilter)
		{
			value = VariantUtils.CreateFrom(in _impFilter);
			return true;
		}
		if (name == PropertyName.impFliters)
		{
			value = VariantUtils.CreateFromArray(impFliters);
			return true;
		}
		if (name == PropertyName._jumpClip)
		{
			value = VariantUtils.CreateFrom(in _jumpClip);
			return true;
		}
		if (name == PropertyName._throwClip)
		{
			value = VariantUtils.CreateFrom(in _throwClip);
			return true;
		}
		if (name == PropertyName._jumpTimer)
		{
			value = VariantUtils.CreateFrom(in _jumpTimer);
			return true;
		}
		if (name == PropertyName._jumping)
		{
			value = VariantUtils.CreateFrom(in _jumping);
			return true;
		}
		if (name == PropertyName._landed)
		{
			value = VariantUtils.CreateFrom(in _landed);
			return true;
		}
		if (name == PropertyName._takeoffFrame)
		{
			value = VariantUtils.CreateFrom(in _takeoffFrame);
			return true;
		}
		if (name == PropertyName._landingFrame)
		{
			value = VariantUtils.CreateFrom(in _landingFrame);
			return true;
		}
		if (name == PropertyName._lastHopFrame)
		{
			value = VariantUtils.CreateFrom(in _lastHopFrame);
			return true;
		}
		if (name == PropertyName._jumpStartX)
		{
			value = VariantUtils.CreateFrom(in _jumpStartX);
			return true;
		}
		if (name == PropertyName._jumpTargetX)
		{
			value = VariantUtils.CreateFrom(in _jumpTargetX);
			return true;
		}
		if (name == PropertyName._jumpTargetGrid)
		{
			value = VariantUtils.CreateFrom(in _jumpTargetGrid);
			return true;
		}
		if (name == PropertyName._throwing)
		{
			value = VariantUtils.CreateFrom(in _throwing);
			return true;
		}
		if (name == PropertyName._throwElapsed)
		{
			value = VariantUtils.CreateFrom(in _throwElapsed);
			return true;
		}
		if (name == PropertyName._bagThrown)
		{
			value = VariantUtils.CreateFrom(in _bagThrown);
			return true;
		}
		if (name == PropertyName._deathReplacementSpawned)
		{
			value = VariantUtils.CreateFrom(in _deathReplacementSpawned);
			return true;
		}
		if (name == PropertyName._refreshPouchVisual)
		{
			value = VariantUtils.CreateFrom(in _refreshPouchVisual);
			return true;
		}
		if (name == PropertyName._rageStages)
		{
			value = VariantUtils.CreateFrom(in _rageStages);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, PropertyName.impName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName.impSpawnSlot, PropertyHint.NodeType, "AdobeAnimateSlot", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.impFireEvent, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName._impFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.impFilter, PropertyHint.MultilineText, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Array, PropertyName.impFliters, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.impVerticalSpeed, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.impLandingGridMin, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.impLandingGridMax, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.impThrowEase, PropertyHint.Enum, "In,Out,InOut,OutIn", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.impThrowTransition, PropertyHint.Enum, "Linear,Sine,Quint,Quart,Quad,Expo,Elastic,Cubic,Circ,Bounce,Back,Spring", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.smashAudio, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName.cameraShakeDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName.cameraShakeInterval, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.cameraShakeCount, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName._jumpClip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._throwClip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._jumpTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._jumping, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._landed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._takeoffFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._landingFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._lastHopFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._jumpStartX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._jumpTargetX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2I, PropertyName._jumpTargetGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._throwing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._throwElapsed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._bagThrown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._deathReplacementSpawned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._refreshPouchVisual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.bagThrown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._rageStages, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.impName, Variant.From<string>(impName));
		info.AddProperty(PropertyName.impSpawnSlot, Variant.From<AdobeAnimateSlot>(impSpawnSlot));
		info.AddProperty(PropertyName.impFireEvent, Variant.From<string>(impFireEvent));
		info.AddProperty(PropertyName.impFilter, Variant.From<string>(impFilter));
		info.AddProperty(PropertyName.impVerticalSpeed, Variant.From<double>(impVerticalSpeed));
		info.AddProperty(PropertyName.impLandingGridMin, Variant.From<int>(impLandingGridMin));
		info.AddProperty(PropertyName.impLandingGridMax, Variant.From<int>(impLandingGridMax));
		info.AddProperty(PropertyName.impThrowEase, Variant.From<Tween.EaseType>(impThrowEase));
		info.AddProperty(PropertyName.impThrowTransition, Variant.From<Tween.TransitionType>(impThrowTransition));
		info.AddProperty(PropertyName.smashAudio, Variant.From<string>(smashAudio));
		info.AddProperty(PropertyName.cameraShakeDuration, Variant.From<double>(cameraShakeDuration));
		info.AddProperty(PropertyName.cameraShakeInterval, Variant.From<double>(cameraShakeInterval));
		info.AddProperty(PropertyName.cameraShakeCount, Variant.From<int>(cameraShakeCount));
		info.AddProperty(PropertyName._impFilter, Variant.From(in _impFilter));
		info.AddProperty(PropertyName.impFliters, Variant.CreateFrom(impFliters));
		info.AddProperty(PropertyName._jumpTimer, Variant.From(in _jumpTimer));
		info.AddProperty(PropertyName._jumping, Variant.From(in _jumping));
		info.AddProperty(PropertyName._landed, Variant.From(in _landed));
		info.AddProperty(PropertyName._takeoffFrame, Variant.From(in _takeoffFrame));
		info.AddProperty(PropertyName._landingFrame, Variant.From(in _landingFrame));
		info.AddProperty(PropertyName._lastHopFrame, Variant.From(in _lastHopFrame));
		info.AddProperty(PropertyName._jumpStartX, Variant.From(in _jumpStartX));
		info.AddProperty(PropertyName._jumpTargetX, Variant.From(in _jumpTargetX));
		info.AddProperty(PropertyName._jumpTargetGrid, Variant.From(in _jumpTargetGrid));
		info.AddProperty(PropertyName._throwing, Variant.From(in _throwing));
		info.AddProperty(PropertyName._throwElapsed, Variant.From(in _throwElapsed));
		info.AddProperty(PropertyName._bagThrown, Variant.From(in _bagThrown));
		info.AddProperty(PropertyName._deathReplacementSpawned, Variant.From(in _deathReplacementSpawned));
		info.AddProperty(PropertyName._refreshPouchVisual, Variant.From(in _refreshPouchVisual));
		info.AddProperty(PropertyName._rageStages, Variant.From(in _rageStages));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.impName, out var value))
		{
			impName = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName.impSpawnSlot, out var value2))
		{
			impSpawnSlot = value2.As<AdobeAnimateSlot>();
		}
		if (info.TryGetProperty(PropertyName.impFireEvent, out var value3))
		{
			impFireEvent = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName.impFilter, out var value4))
		{
			impFilter = value4.As<string>();
		}
		if (info.TryGetProperty(PropertyName.impVerticalSpeed, out var value5))
		{
			impVerticalSpeed = value5.As<double>();
		}
		if (info.TryGetProperty(PropertyName.impLandingGridMin, out var value6))
		{
			impLandingGridMin = value6.As<int>();
		}
		if (info.TryGetProperty(PropertyName.impLandingGridMax, out var value7))
		{
			impLandingGridMax = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName.impThrowEase, out var value8))
		{
			impThrowEase = value8.As<Tween.EaseType>();
		}
		if (info.TryGetProperty(PropertyName.impThrowTransition, out var value9))
		{
			impThrowTransition = value9.As<Tween.TransitionType>();
		}
		if (info.TryGetProperty(PropertyName.smashAudio, out var value10))
		{
			smashAudio = value10.As<string>();
		}
		if (info.TryGetProperty(PropertyName.cameraShakeDuration, out var value11))
		{
			cameraShakeDuration = value11.As<double>();
		}
		if (info.TryGetProperty(PropertyName.cameraShakeInterval, out var value12))
		{
			cameraShakeInterval = value12.As<double>();
		}
		if (info.TryGetProperty(PropertyName.cameraShakeCount, out var value13))
		{
			cameraShakeCount = value13.As<int>();
		}
		if (info.TryGetProperty(PropertyName._impFilter, out var value14))
		{
			_impFilter = value14.As<string>();
		}
		if (info.TryGetProperty(PropertyName.impFliters, out var value15))
		{
			impFliters = value15.AsGodotArray<string>();
		}
		if (info.TryGetProperty(PropertyName._jumpTimer, out var value16))
		{
			_jumpTimer = value16.As<double>();
		}
		if (info.TryGetProperty(PropertyName._jumping, out var value17))
		{
			_jumping = value17.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._landed, out var value18))
		{
			_landed = value18.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._takeoffFrame, out var value19))
		{
			_takeoffFrame = value19.As<int>();
		}
		if (info.TryGetProperty(PropertyName._landingFrame, out var value20))
		{
			_landingFrame = value20.As<int>();
		}
		if (info.TryGetProperty(PropertyName._lastHopFrame, out var value21))
		{
			_lastHopFrame = value21.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._jumpStartX, out var value22))
		{
			_jumpStartX = value22.As<float>();
		}
		if (info.TryGetProperty(PropertyName._jumpTargetX, out var value23))
		{
			_jumpTargetX = value23.As<float>();
		}
		if (info.TryGetProperty(PropertyName._jumpTargetGrid, out var value24))
		{
			_jumpTargetGrid = value24.As<Vector2I>();
		}
		if (info.TryGetProperty(PropertyName._throwing, out var value25))
		{
			_throwing = value25.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._throwElapsed, out var value26))
		{
			_throwElapsed = value26.As<double>();
		}
		if (info.TryGetProperty(PropertyName._bagThrown, out var value27))
		{
			_bagThrown = value27.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._deathReplacementSpawned, out var value28))
		{
			_deathReplacementSpawned = value28.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._refreshPouchVisual, out var value29))
		{
			_refreshPouchVisual = value29.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._rageStages, out var value30))
		{
			_rageStages = value30.As<int>();
		}
	}
}
