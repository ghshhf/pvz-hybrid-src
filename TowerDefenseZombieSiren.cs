using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter10/Siren/Scene/TowerDefenseZombieSiren.cs")]
public class TowerDefenseZombieSiren : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName CanSing = "CanSing";

		public static readonly StringName ProcessSongTimer = "ProcessSongTimer";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName AttackProcessing = "AttackProcessing";

		public new static readonly StringName Walk = "Walk";

		public new static readonly StringName Attack = "Attack";

		public new static readonly StringName WalkEntered = "WalkEntered";

		public new static readonly StringName InWater = "InWater";

		public new static readonly StringName OutWater = "OutWater";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";

		public static readonly StringName SingEntered = "SingEntered";

		public static readonly StringName PlaySongAnimation = "PlaySongAnimation";

		public static readonly StringName SingProcessing = "SingProcessing";

		public static readonly StringName SingExited = "SingExited";

		public static readonly StringName FindSongTargets = "FindSongTargets";

		public static readonly StringName IsSongTarget = "IsSongTarget";

		public static readonly StringName ApplySongEffects = "ApplySongEffects";

		public static readonly StringName ApplyDizziness = "ApplyDizziness";

		public static readonly StringName ApplyHaste = "ApplyHaste";

		public static readonly StringName HealAllies = "HealAllies";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName SongRemaining = "SongRemaining";

		public static readonly StringName _enteredField = "_enteredField";

		public static readonly StringName _preserveSongOnRestore = "_preserveSongOnRestore";

		public static readonly StringName _songPhase = "_songPhase";

		public static readonly StringName _healPulses = "_healPulses";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const double SingInterval = 12.0;

	private const double SingDuration = 5.0;

	private const string DizzinessBuffKey = "Dizziness";

	private readonly List<TowerDefenseCharacter> _candidates = new List<TowerDefenseCharacter>();

	private StateHandle _singState;

	private CharacterTimerComponent _timerComponent;

	private bool _enteredField;

	private bool _preserveSongOnRestore;

	private int _songPhase;

	private int _healPulses;

	private double SongRemaining
	{
		get
		{
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent == null || timerComponent.IsReleased || !_timerComponent.timerCurrent.TryGetValue("Sing", out var value))
			{
				return 0.0;
			}
			return Math.Max(0.0, 5.0 - value);
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			_singState = base.StateMachine.GetStateById("zombie.siren.sing");
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += OnTimerTimeout;
			}
			_singState.Entered += SingEntered;
			_singState.PhysicsProcessing += SingProcessing;
			_singState.Exited += SingExited;
		}
	}

	private bool CanSing()
	{
		if (TowerDefenseManager.HasGameplayAuthority && CanRunAbility() && inGame && !isRise)
		{
			return GodotObject.IsInstanceValid(base.sprite);
		}
		return false;
	}

	private void ProcessSongTimer()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent == null || timerComponent.IsReleased)
		{
			return;
		}
		bool flag = CanSing();
		_timerComponent.timeScale = (flag ? 1.0 : 0.0);
		if (flag)
		{
			_enteredField |= base.IsInsideComponentBattlefield;
			if (_enteredField && !_timerComponent.IsRunning("Cast"))
			{
				_timerComponent.Run("Cast", 12.0);
			}
		}
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		ProcessSongTimer();
	}

	public override void WalkProcessing(double delta)
	{
		base.WalkProcessing(delta);
		ProcessSongTimer();
	}

	public override void AttackProcessing(double delta)
	{
		base.AttackProcessing(delta);
		ProcessSongTimer();
	}

	public override void Walk()
	{
		StateHandle singState = _singState;
		if (singState == null || !singState.IsActive || die || nearDie)
		{
			base.Walk();
		}
	}

	public override void Attack()
	{
		StateHandle singState = _singState;
		if (singState == null || !singState.IsActive || die || nearDie)
		{
			base.Attack();
		}
	}

	public override void WalkEntered()
	{
		if (!base.inWater)
		{
			WaterInteractionComponent obj = waterInteractionComponent;
			if (obj != null && obj.outFromWater)
			{
				waterInteractionComponent.outFromWater = false;
				base.sprite.SetAnimation(base.outSwimAnimeClip, loop: false);
				base.sprite.AddAnimation(base.walkAnimeClip, 0.0);
				ActivateGroundMovementAfterStateDelay(base.WalkStateHandle);
				return;
			}
		}
		base.WalkEntered();
	}

	public override void InWater()
	{
		base.InWater();
		StateHandle singState = _singState;
		if (singState != null && singState.IsActive)
		{
			PlaySongAnimation();
		}
	}

	public override void OutWater()
	{
		base.OutWater();
		StateHandle singState = _singState;
		if (singState != null && singState.IsActive)
		{
			PlaySongAnimation();
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (!CanSing())
		{
			return;
		}
		switch (timerName)
		{
		case "Cast":
		{
			StateHandle singState3 = _singState;
			if (singState3 == null || !singState3.IsActive)
			{
				SendStateEvent("ToSirenSing");
			}
			break;
		}
		case "Sing":
		{
			StateHandle singState2 = _singState;
			if (singState2 != null && singState2.IsActive && _songPhase == 1)
			{
				if (_healPulses < 5)
				{
					HealAllies();
				}
				_timerComponent.Stop("Heal");
				_songPhase = 2;
				PlaySongAnimation();
			}
			break;
		}
		case "Heal":
		{
			StateHandle singState = _singState;
			if (singState != null && singState.IsActive && _songPhase == 1 && _healPulses < 5)
			{
				HealAllies();
				_timerComponent.Run("Heal", 1.0);
			}
			break;
		}
		}
	}

	private void SingEntered()
	{
		groundMoveComponent?.SetAlive(value: false);
		startAttack = false;
		if (_preserveSongOnRestore)
		{
			_preserveSongOnRestore = false;
			return;
		}
		_timerComponent?.Run("Cast", 12.0);
		_songPhase = 0;
		_healPulses = 0;
		PlaySongAnimation();
	}

	private void PlaySongAnimation()
	{
		string text = ((_songPhase == 0) ? "Up" : ((_songPhase == 1) ? "Sing" : "Down"));
		base.sprite.SetAnimation(base.inWater ? ("Water" + text) : text, _songPhase == 1);
	}

	private void SingProcessing(double delta)
	{
		groundMoveComponent?.SetAlive(value: false);
		base.sprite.timeScale = timeScale;
		ProcessSongTimer();
		if (_songPhase == 1 && CanSing())
		{
			ApplySongEffects();
		}
	}

	private void SingExited()
	{
		_timerComponent?.Stop("Sing");
		_timerComponent?.Stop("Heal");
		_songPhase = 0;
	}

	private void FindSongTargets()
	{
		_candidates.Clear();
		_candidates.AddRange(TowerDefenseManager.Instance.characterRegistry.GetCharactersForGridWindowList(base.gridPos.X, base.gridPos.Y, 1, 1));
	}

	private bool IsSongTarget(TowerDefenseCharacter target)
	{
		if (GodotObject.IsInstanceValid(target) && target != this && target.inGame && !target.die && !target.nearDie && !target.isDestroy && GodotObject.IsInstanceValid(target.instance))
		{
			return target.instance.maskFlags != 0;
		}
		return false;
	}

	private void ApplySongEffects()
	{
		double songRemaining = SongRemaining;
		if (songRemaining <= 0.0)
		{
			return;
		}
		FindSongTargets();
		foreach (TowerDefenseCharacter candidate in _candidates)
		{
			if (!IsSongTarget(candidate))
			{
				continue;
			}
			if (candidate.camp == base.camp)
			{
				if (candidate is TowerDefenseZombie)
				{
					ApplyHaste(candidate, songRemaining);
				}
			}
			else
			{
				ApplyDizziness(candidate, songRemaining);
			}
		}
	}

	private static void ApplyDizziness(TowerDefenseCharacter target, double duration)
	{
		if (target.BuffGet("Dizziness") is TowerDefenseCharacterBuffDizziness towerDefenseCharacterBuffDizziness)
		{
			towerDefenseCharacterBuffDizziness.time = Math.Max(towerDefenseCharacterBuffDizziness.time, towerDefenseCharacterBuffDizziness.currentTime + duration);
			return;
		}
		target.BuffAdd(new TowerDefenseCharacterBuffDizziness
		{
			time = duration
		});
	}

	private static void ApplyHaste(TowerDefenseCharacter target, double duration)
	{
		if (target.buff.BuffGet("SirenHaste") is TowerDefenseCharacterBuffOverallHaste towerDefenseCharacterBuffOverallHaste)
		{
			towerDefenseCharacterBuffOverallHaste.Extend(duration);
			return;
		}
		target.buff.AddBuff(new TowerDefenseCharacterBuffOverallHaste
		{
			key = "SirenHaste",
			timeScaleValue = 1.5,
			time = duration
		});
	}

	private void HealAllies()
	{
		_healPulses++;
		FindSongTargets();
		foreach (TowerDefenseCharacter candidate in _candidates)
		{
			if (IsSongTarget(candidate) && candidate is TowerDefenseZombie && candidate.camp == base.camp && (candidate.instance.physiqueTypeFlags & 0x1000) != 0)
			{
				double val = Math.Max(40.0, candidate.instance.hitpointsSave * 0.02);
				candidate.Health(Math.Min(val, Math.Max(0.0, candidate.instance.hitpointsSave - candidate.instance.hitpoints)));
			}
		}
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		StateHandle singState = _singState;
		if (singState != null && singState.IsActive && CanSing())
		{
			if ((clip == "Up" || clip == "WaterUp") && _songPhase == 0)
			{
				_songPhase = 1;
				_timerComponent.Run("Sing", 5.0);
				_timerComponent.Run("Heal", 1.0);
				PlaySongAnimation();
				ApplySongEffects();
			}
			else if ((clip == "Down" || clip == "WaterDown") && _songPhase == 2)
			{
				base.Walk();
			}
		}
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (!(command != "attack") && CanSing() && startAttack)
		{
			StateHandle attackStateHandle = base.AttackStateHandle;
			if (attackStateHandle != null && attackStateHandle.IsActive && (!(base.sprite.clip != base.attackAnimeClip) || !(base.sprite.clip != base.attackWaterAnimeClip)))
			{
				attackComponent.AttackExecute(((TowerDefenseZombieConfig)base.config).smashAttack);
			}
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["sirenEnteredField"] = _enteredField;
		dictionary["sirenSongPhase"] = _songPhase;
		dictionary["sirenHealPulses"] = _healPulses;
		dictionary["sirenSinging"] = _singState?.IsActive ?? false;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_enteredField = data.GetValueOrDefault("sirenEnteredField", false).AsBool();
		_songPhase = data.GetValueOrDefault("sirenSongPhase", 0).AsInt32();
		_healPulses = data.GetValueOrDefault("sirenHealPulses", 0).AsInt32();
		_preserveSongOnRestore = data.GetValueOrDefault("sirenSinging", false).AsBool();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return new Dictionary { ["sirenSongPhase"] = _songPhase };
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		_songPhase = data.GetValueOrDefault("sirenSongPhase", 0).AsInt32();
		_preserveSongOnRestore = true;
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= OnTimerTimeout;
		}
		if (_singState != null)
		{
			_singState.Entered -= SingEntered;
			_singState.PhysicsProcessing -= SingProcessing;
			_singState.Exited -= SingExited;
			_singState = null;
		}
		_candidates.Clear();
		base._ExitTree();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(29)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanSing, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ProcessSongTimer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AttackProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Walk, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Attack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WalkEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.InWater, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OutWater, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SingEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlaySongAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SingProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SingExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FindSongTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IsSongTarget, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ApplySongEffects, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyDizziness, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyHaste, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.HealAllies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
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
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.CanSing && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanSing());
			return true;
		}
		if (method == MethodName.ProcessSongTimer && args.Count == 0)
		{
			ProcessSongTimer();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.Walk && args.Count == 0)
		{
			Walk();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Attack && args.Count == 0)
		{
			Attack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WalkEntered && args.Count == 0)
		{
			WalkEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InWater && args.Count == 0)
		{
			InWater();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OutWater && args.Count == 0)
		{
			OutWater();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 1)
		{
			OnTimerTimeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SingEntered && args.Count == 0)
		{
			SingEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlaySongAnimation && args.Count == 0)
		{
			PlaySongAnimation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SingProcessing && args.Count == 1)
		{
			SingProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SingExited && args.Count == 0)
		{
			SingExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FindSongTargets && args.Count == 0)
		{
			FindSongTargets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsSongTarget && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsSongTarget(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.ApplySongEffects && args.Count == 0)
		{
			ApplySongEffects();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDizziness && args.Count == 2)
		{
			ApplyDizziness(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyHaste && args.Count == 2)
		{
			ApplyHaste(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HealAllies && args.Count == 0)
		{
			HealAllies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ApplyDizziness && args.Count == 2)
		{
			ApplyDizziness(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyHaste && args.Count == 2)
		{
			ApplyHaste(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
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
		if (method == MethodName.CanSing)
		{
			return true;
		}
		if (method == MethodName.ProcessSongTimer)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
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
		if (method == MethodName.Walk)
		{
			return true;
		}
		if (method == MethodName.Attack)
		{
			return true;
		}
		if (method == MethodName.WalkEntered)
		{
			return true;
		}
		if (method == MethodName.InWater)
		{
			return true;
		}
		if (method == MethodName.OutWater)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
		{
			return true;
		}
		if (method == MethodName.SingEntered)
		{
			return true;
		}
		if (method == MethodName.PlaySongAnimation)
		{
			return true;
		}
		if (method == MethodName.SingProcessing)
		{
			return true;
		}
		if (method == MethodName.SingExited)
		{
			return true;
		}
		if (method == MethodName.FindSongTargets)
		{
			return true;
		}
		if (method == MethodName.IsSongTarget)
		{
			return true;
		}
		if (method == MethodName.ApplySongEffects)
		{
			return true;
		}
		if (method == MethodName.ApplyDizziness)
		{
			return true;
		}
		if (method == MethodName.ApplyHaste)
		{
			return true;
		}
		if (method == MethodName.HealAllies)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._enteredField)
		{
			_enteredField = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._preserveSongOnRestore)
		{
			_preserveSongOnRestore = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._songPhase)
		{
			_songPhase = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._healPulses)
		{
			_healPulses = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SongRemaining)
		{
			value = VariantUtils.CreateFrom<double>(SongRemaining);
			return true;
		}
		if (name == PropertyName._enteredField)
		{
			value = VariantUtils.CreateFrom(in _enteredField);
			return true;
		}
		if (name == PropertyName._preserveSongOnRestore)
		{
			value = VariantUtils.CreateFrom(in _preserveSongOnRestore);
			return true;
		}
		if (name == PropertyName._songPhase)
		{
			value = VariantUtils.CreateFrom(in _songPhase);
			return true;
		}
		if (name == PropertyName._healPulses)
		{
			value = VariantUtils.CreateFrom(in _healPulses);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._enteredField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._preserveSongOnRestore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._songPhase, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._healPulses, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.SongRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._enteredField, Variant.From(in _enteredField));
		info.AddProperty(PropertyName._preserveSongOnRestore, Variant.From(in _preserveSongOnRestore));
		info.AddProperty(PropertyName._songPhase, Variant.From(in _songPhase));
		info.AddProperty(PropertyName._healPulses, Variant.From(in _healPulses));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._enteredField, out var value))
		{
			_enteredField = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._preserveSongOnRestore, out var value2))
		{
			_preserveSongOnRestore = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._songPhase, out var value3))
		{
			_songPhase = value3.As<int>();
		}
		if (info.TryGetProperty(PropertyName._healPulses, out var value4))
		{
			_healPulses = value4.As<int>();
		}
	}
}
