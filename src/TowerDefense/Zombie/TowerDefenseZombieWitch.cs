using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter10/Witch/Scene/TowerDefenseZombieWitch.cs")]
public class TowerDefenseZombieWitch : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName WalkEntered = "WalkEntered";

		public new static readonly StringName Walk = "Walk";

		public static readonly StringName CanAct = "CanAct";

		public static readonly StringName SpellProcessing = "SpellProcessing";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public static readonly StringName ThrowEntered = "ThrowEntered";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public static readonly StringName ThrowBottle = "ThrowBottle";

		public static readonly StringName CompleteBottleLanding = "CompleteBottleLanding";

		public new static readonly StringName FinalizeProgressRestore = "FinalizeProgressRestore";

		public new static readonly StringName Blow = "Blow";

		public new static readonly StringName BlowBack = "BlowBack";

		public static readonly StringName QueueSwing = "QueueSwing";

		public static readonly StringName SwingEntered = "SwingEntered";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName _enteredField = "_enteredField";

		public static readonly StringName _bottlePending = "_bottlePending";

		public static readonly StringName _preserveThrowOnRestore = "_preserveThrowOnRestore";

		public static readonly StringName _pendingSwings = "_pendingSwings";

		public static readonly StringName _bottleMarker = "_bottleMarker";

		public static readonly StringName bottleTexture = "bottleTexture";

		public static readonly StringName _bottleInFlight = "_bottleInFlight";

		public static readonly StringName _bottleFlightGrid = "_bottleFlightGrid";

		public static readonly StringName _bottleFlightRemaining = "_bottleFlightRemaining";

		public static readonly StringName _bottleFlightRestorePending = "_bottleFlightRestorePending";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const double ThrowInterval = 10.0;

	private StateHandle _throwState;

	private StateHandle _swingState;

	private CharacterTimerComponent _timerComponent;

	private bool _enteredField;

	private bool _bottlePending;

	private bool _preserveThrowOnRestore;

	private int _pendingSwings;

	private Marker2D _bottleMarker;

	[Export(PropertyHint.None, "")]
	public Texture2D bottleTexture;

	private bool _bottleInFlight;

	private Vector2I _bottleFlightGrid;

	private double _bottleFlightRemaining;

	private bool _bottleFlightRestorePending;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			_bottleMarker = base.sprite.GetNode<Marker2D>("AnimeSlot/Marker2D");
			_throwState = base.StateMachine.GetStateById("zombie.witch.throw");
			_swingState = base.StateMachine.GetStateById("zombie.witch.swing");
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += OnTimerTimeout;
			}
			_throwState.Entered += ThrowEntered;
			_throwState.PhysicsProcessing += SpellProcessing;
			_swingState.Entered += SwingEntered;
			_swingState.PhysicsProcessing += SpellProcessing;
		}
	}

	public override void WalkEntered()
	{
		groundMoveComponent?.SetAlive(value: false);
		base.sprite.SetAnimation("Idle", loop: true, 0.1);
	}

	public override void Walk()
	{
		StateHandle throwState = _throwState;
		if (throwState == null || !throwState.IsActive)
		{
			StateHandle swingState = _swingState;
			if (swingState == null || !swingState.IsActive)
			{
				goto IL_0039;
			}
		}
		if (!die && !nearDie)
		{
			return;
		}
		goto IL_0039;
		IL_0039:
		base.Walk();
	}

	private bool CanAct()
	{
		if (TowerDefenseManager.HasGameplayAuthority && inGame && !die && !nearDie && !isDestroy && !base.sprite.pause)
		{
			return timeScale > 0.0;
		}
		return false;
	}

	private void SpellProcessing(double delta)
	{
		base.sprite.timeScale = timeScale;
		if (!CanAct())
		{
			return;
		}
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_enteredField |= base.IsInsideComponentBattlefield;
			if (_enteredField && !_timerComponent.IsRunning("Throw"))
			{
				_timerComponent.Run("Throw", 10.0);
			}
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (timerName != "Throw")
		{
			return;
		}
		StateHandle throwState = _throwState;
		if (throwState == null || !throwState.IsActive)
		{
			StateHandle swingState = _swingState;
			if ((swingState == null || !swingState.IsActive) && CanAct())
			{
				SendStateEvent("ToWitchThrow");
			}
		}
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		SpellProcessing(delta);
	}

	public override void WalkProcessing(double delta)
	{
		SpellProcessing(delta);
		if (!CanAct())
		{
			return;
		}
		if (_pendingSwings > 0)
		{
			SendStateEvent("ToWitchSwing");
			return;
		}
		BlowBackComponent obj = blowBackComponent;
		if (obj != null && obj.blowBack)
		{
			return;
		}
		float num = (instance.hypnoses ? 1 : (-1));
		SetLogicalGlobalPosition(GetLogicalGlobalPosition() + new Vector2((float)((double)(num * 12f) * delta * timeScale * base.walkSpeedScale), 0f));
		if (base.sprite.pause)
		{
			return;
		}
		AttackComponent obj2 = attackComponent;
		if (obj2 != null && !obj2.IsReleased && attackComponent.CanAttack())
		{
			TowerDefenseCharacter target = attackComponent.target;
			if (GodotObject.IsInstanceValid(target) && (target.instance.maskFlags & 2) != 0)
			{
				attackComponent.AttackExecute(((TowerDefenseZombieConfig)base.config).smashAttack);
				base.BlowBack(instance.hypnoses ? (-0.5) : 0.5, 0.2);
			}
		}
	}

	private void ThrowEntered()
	{
		groundMoveComponent?.SetAlive(value: false);
		if (_preserveThrowOnRestore)
		{
			_preserveThrowOnRestore = false;
			return;
		}
		_timerComponent?.Run("Throw", 10.0);
		_bottlePending = true;
		base.sprite.SetAnimation("Throw", loop: false);
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (!(command != "action") && _bottlePending)
		{
			StateHandle throwState = _throwState;
			if (throwState != null && throwState.IsActive && CanAct())
			{
				_bottlePending = false;
				ThrowBottle();
			}
		}
	}

	internal void ThrowBottle()
	{
		TowerDefenseManager manager = TowerDefenseManager.Instance;
		TowerDefenseControlNew control = manager.currentControl;
		Vector2I mapGridPos = manager.GetMapGridPos(GetLogicalGlobalPosition());
		if (!manager.CheckMapGridPosIn(mapGridPos))
		{
			return;
		}
		_bottleMarker.GetParent<AdobeAnimateSlot>().Update();
		Vector2 start = GetLogicalGlobalPosition(_bottleMarker);
		Vector2 landing = manager.GetMapCellPosCenter(mapGridPos);
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(mapGridPos);
		Vector2 vector = landing - new Vector2(0f, (float)(mapCell?.GetGroundHeight() ?? 0.0));
		Sprite2D bottle = new Sprite2D
		{
			Texture = bottleTexture,
			ZIndex = base.ZIndex,
			ZAsRelative = false
		};
		control.characterNode.AddChild(bottle, forceReadableName: false, InternalMode.Disabled);
		bottle.GlobalTransform = GetLogicalGlobalTransform(_bottleMarker);
		_bottleInFlight = true;
		_bottleFlightGrid = mapGridPos;
		_bottleFlightRemaining = 0.6499999761581421;
		Vector2 velocity = (vector - start) / 0.65f - new Vector2(0f, 292.5f);
		Tween tween = bottle.CreateTween();
		tween.TweenMethod(Callable.From(delegate(float elapsed)
		{
			bottle.GlobalPosition = start + velocity * elapsed + new Vector2(0f, 900f * elapsed * elapsed * 0.5f);
		}), 0f, 0.65f, 0.6499999761581421);
		tween.TweenCallback(Callable.From(delegate
		{
			_bottleInFlight = false;
			_bottleFlightRemaining = 0.0;
			if (manager.currentControl == control)
			{
				TowerDefenseCharacterEventPoisonBottleLand.Run(landing, base.camp);
			}
			bottle.QueueFree();
		}));
	}

	private void CompleteBottleLanding()
	{
		_bottleFlightRestorePending = false;
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		if (GodotObject.IsInstanceValid(towerDefenseManager) && !die && !isDestroy && IsInsideTree() && towerDefenseManager.CheckMapGridPosIn(_bottleFlightGrid))
		{
			TowerDefenseCharacterEventPoisonBottleLand.Run(towerDefenseManager.GetMapCellPosCenter(_bottleFlightGrid), base.camp);
		}
	}

	public override void FinalizeProgressRestore()
	{
		base.FinalizeProgressRestore();
		if (_bottleFlightRestorePending)
		{
			Callable.From(CompleteBottleLanding).CallDeferred();
		}
	}

	public override void Blow()
	{
		if (!die && !nearDie && !isDestroy)
		{
			base.BlowBack((!instance.hypnoses) ? 1 : (-1), 1.0);
			QueueSwing();
		}
	}

	public override void BlowBack(double num, double time = 1.0)
	{
		if (!die && !nearDie && !isDestroy)
		{
			base.BlowBack(num, time);
			QueueSwing();
		}
	}

	private void QueueSwing()
	{
		_pendingSwings++;
		StateHandle throwState = _throwState;
		if (throwState == null || !throwState.IsActive)
		{
			StateHandle swingState = _swingState;
			if (swingState == null || !swingState.IsActive)
			{
				SendStateEvent("ToWitchSwing");
			}
		}
	}

	private void SwingEntered()
	{
		_pendingSwings = Math.Max(0, _pendingSwings - 1);
		base.sprite.SetAnimation("Swing", loop: false);
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (!CanAct())
		{
			return;
		}
		if (clip == "Throw")
		{
			StateHandle throwState = _throwState;
			if (throwState != null && throwState.IsActive)
			{
				goto IL_0052;
			}
		}
		if (clip == "Swing")
		{
			StateHandle swingState = _swingState;
			if (swingState == null || !swingState.IsActive)
			{
				return;
			}
			goto IL_0052;
		}
		return;
		IL_0052:
		if (_pendingSwings > 0)
		{
			if (_swingState.IsActive)
			{
				SwingEntered();
			}
			else
			{
				SendStateEvent("ToWitchSwing");
			}
		}
		else
		{
			base.Walk();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["witchEnteredField"] = _enteredField;
		dictionary["witchBottlePending"] = _bottlePending;
		dictionary["witchThrowing"] = _throwState?.IsActive ?? false;
		dictionary["witchPendingSwings"] = _pendingSwings;
		dictionary["witchBottleInFlight"] = _bottleInFlight;
		dictionary["witchBottleFlightX"] = _bottleFlightGrid.X;
		dictionary["witchBottleFlightY"] = _bottleFlightGrid.Y;
		dictionary["witchBottleFlightRemaining"] = _bottleFlightRemaining;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_enteredField = data.GetValueOrDefault("witchEnteredField", false).AsBool();
		_bottlePending = data.GetValueOrDefault("witchBottlePending", false).AsBool();
		_preserveThrowOnRestore = data.GetValueOrDefault("witchThrowing", false).AsBool();
		_pendingSwings = data.GetValueOrDefault("witchPendingSwings", 0).AsInt32();
		_bottleInFlight = false;
		_bottleFlightRemaining = 0.0;
		_bottleFlightRestorePending = data.GetValueOrDefault("witchBottleInFlight", false).AsBool();
		_bottleFlightGrid = new Vector2I(data.GetValueOrDefault("witchBottleFlightX", 0).AsInt32(), data.GetValueOrDefault("witchBottleFlightY", 0).AsInt32());
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= OnTimerTimeout;
		}
		if (_throwState != null)
		{
			_throwState.Entered -= ThrowEntered;
			_throwState.PhysicsProcessing -= SpellProcessing;
		}
		if (_swingState != null)
		{
			_swingState.Entered -= SwingEntered;
			_swingState.PhysicsProcessing -= SpellProcessing;
		}
		base._ExitTree();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(21)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WalkEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Walk, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanAct, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SpellProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ThrowEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.ThrowBottle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CompleteBottleLanding, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FinalizeProgressRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Blow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BlowBack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "num", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "time", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.QueueSwing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SwingEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.WalkEntered && args.Count == 0)
		{
			WalkEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Walk && args.Count == 0)
		{
			Walk();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanAct && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanAct());
			return true;
		}
		if (method == MethodName.SpellProcessing && args.Count == 1)
		{
			SpellProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 1)
		{
			OnTimerTimeout(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName.ThrowEntered && args.Count == 0)
		{
			ThrowEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ThrowBottle && args.Count == 0)
		{
			ThrowBottle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteBottleLanding && args.Count == 0)
		{
			CompleteBottleLanding();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FinalizeProgressRestore && args.Count == 0)
		{
			FinalizeProgressRestore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Blow && args.Count == 0)
		{
			Blow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BlowBack && args.Count == 2)
		{
			BlowBack(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.QueueSwing && args.Count == 0)
		{
			QueueSwing();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SwingEntered && args.Count == 0)
		{
			SwingEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.WalkEntered)
		{
			return true;
		}
		if (method == MethodName.Walk)
		{
			return true;
		}
		if (method == MethodName.CanAct)
		{
			return true;
		}
		if (method == MethodName.SpellProcessing)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
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
		if (method == MethodName.ThrowEntered)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.ThrowBottle)
		{
			return true;
		}
		if (method == MethodName.CompleteBottleLanding)
		{
			return true;
		}
		if (method == MethodName.FinalizeProgressRestore)
		{
			return true;
		}
		if (method == MethodName.Blow)
		{
			return true;
		}
		if (method == MethodName.BlowBack)
		{
			return true;
		}
		if (method == MethodName.QueueSwing)
		{
			return true;
		}
		if (method == MethodName.SwingEntered)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
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
		if (name == PropertyName._bottlePending)
		{
			_bottlePending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._preserveThrowOnRestore)
		{
			_preserveThrowOnRestore = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pendingSwings)
		{
			_pendingSwings = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._bottleMarker)
		{
			_bottleMarker = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.bottleTexture)
		{
			bottleTexture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._bottleInFlight)
		{
			_bottleInFlight = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._bottleFlightGrid)
		{
			_bottleFlightGrid = VariantUtils.ConvertTo<Vector2I>(in value);
			return true;
		}
		if (name == PropertyName._bottleFlightRemaining)
		{
			_bottleFlightRemaining = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._bottleFlightRestorePending)
		{
			_bottleFlightRestorePending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._enteredField)
		{
			value = VariantUtils.CreateFrom(in _enteredField);
			return true;
		}
		if (name == PropertyName._bottlePending)
		{
			value = VariantUtils.CreateFrom(in _bottlePending);
			return true;
		}
		if (name == PropertyName._preserveThrowOnRestore)
		{
			value = VariantUtils.CreateFrom(in _preserveThrowOnRestore);
			return true;
		}
		if (name == PropertyName._pendingSwings)
		{
			value = VariantUtils.CreateFrom(in _pendingSwings);
			return true;
		}
		if (name == PropertyName._bottleMarker)
		{
			value = VariantUtils.CreateFrom(in _bottleMarker);
			return true;
		}
		if (name == PropertyName.bottleTexture)
		{
			value = VariantUtils.CreateFrom(in bottleTexture);
			return true;
		}
		if (name == PropertyName._bottleInFlight)
		{
			value = VariantUtils.CreateFrom(in _bottleInFlight);
			return true;
		}
		if (name == PropertyName._bottleFlightGrid)
		{
			value = VariantUtils.CreateFrom(in _bottleFlightGrid);
			return true;
		}
		if (name == PropertyName._bottleFlightRemaining)
		{
			value = VariantUtils.CreateFrom(in _bottleFlightRemaining);
			return true;
		}
		if (name == PropertyName._bottleFlightRestorePending)
		{
			value = VariantUtils.CreateFrom(in _bottleFlightRestorePending);
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
			new PropertyInfo(Variant.Type.Bool, PropertyName._bottlePending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._preserveThrowOnRestore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._pendingSwings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._bottleMarker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.bottleTexture, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._bottleInFlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2I, PropertyName._bottleFlightGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._bottleFlightRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._bottleFlightRestorePending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._enteredField, Variant.From(in _enteredField));
		info.AddProperty(PropertyName._bottlePending, Variant.From(in _bottlePending));
		info.AddProperty(PropertyName._preserveThrowOnRestore, Variant.From(in _preserveThrowOnRestore));
		info.AddProperty(PropertyName._pendingSwings, Variant.From(in _pendingSwings));
		info.AddProperty(PropertyName._bottleMarker, Variant.From(in _bottleMarker));
		info.AddProperty(PropertyName.bottleTexture, Variant.From(in bottleTexture));
		info.AddProperty(PropertyName._bottleInFlight, Variant.From(in _bottleInFlight));
		info.AddProperty(PropertyName._bottleFlightGrid, Variant.From(in _bottleFlightGrid));
		info.AddProperty(PropertyName._bottleFlightRemaining, Variant.From(in _bottleFlightRemaining));
		info.AddProperty(PropertyName._bottleFlightRestorePending, Variant.From(in _bottleFlightRestorePending));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._enteredField, out var value))
		{
			_enteredField = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._bottlePending, out var value2))
		{
			_bottlePending = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._preserveThrowOnRestore, out var value3))
		{
			_preserveThrowOnRestore = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pendingSwings, out var value4))
		{
			_pendingSwings = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._bottleMarker, out var value5))
		{
			_bottleMarker = value5.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.bottleTexture, out var value6))
		{
			bottleTexture = value6.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._bottleInFlight, out var value7))
		{
			_bottleInFlight = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._bottleFlightGrid, out var value8))
		{
			_bottleFlightGrid = value8.As<Vector2I>();
		}
		if (info.TryGetProperty(PropertyName._bottleFlightRemaining, out var value9))
		{
			_bottleFlightRemaining = value9.As<double>();
		}
		if (info.TryGetProperty(PropertyName._bottleFlightRestorePending, out var value10))
		{
			_bottleFlightRestorePending = value10.As<bool>();
		}
	}
}
