using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter9/JackGene/Scene/TowerDefenseZombieJackGene.cs")]
public class TowerDefenseZombieJackGene : TowerDefenseZombie
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public static readonly StringName ConnectStateSignals = "ConnectStateSignals";

		public static readonly StringName DisconnectStateSignals = "DisconnectStateSignals";

		public new static readonly StringName PrepareForProgressRestore = "PrepareForProgressRestore";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName TryStartBattleEntry = "TryStartBattleEntry";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName BombEntered = "BombEntered";

		public static readonly StringName BombProcessing = "BombProcessing";

		public static readonly StringName BombExited = "BombExited";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName ArmorHitpointsEmpty = "ArmorHitpointsEmpty";

		public new static readonly StringName Walk = "Walk";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public static readonly StringName CreateEffect = "CreateEffect";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName _stateSignalsConnected = "_stateSignalsConnected";

		public static readonly StringName _battleEntryStarted = "_battleEntryStarted";

		public static readonly StringName _restoredFromProgress = "_restoredFromProgress";

		public static readonly StringName eventList = "eventList";

		public static readonly StringName hasJackBox = "hasJackBox";

		public static readonly StringName over = "over";

		public static readonly StringName timer = "timer";

		public static readonly StringName time = "time";

		public static readonly StringName explode = "explode";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private StateHandle _bombStateHandle;

	private bool _stateSignalsConnected;

	private bool _battleEntryStarted;

	private bool _restoredFromProgress;

	private static PackedScene _JACKBOX_EXPLOSION;

	public static AudioStreamPlayerMember JackInTheBoxPlayer;

	[Export(PropertyHint.None, "")]
	public Array<TowerDefenseCharacterEventBase> eventList = new Array<TowerDefenseCharacterEventBase>();

	public bool hasJackBox = true;

	public bool over;

	public double timer;

	public double time;

	public bool explode;

	private static PackedScene JACKBOX_EXPLOSION => _JACKBOX_EXPLOSION ?? (_JACKBOX_EXPLOSION = GD.Load<PackedScene>("uid://cxnt2jbnk48fp"));

	private void ConnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			return;
		}
		IStateMachineController stateMachine = base.StateMachine;
		if (stateMachine != null && stateMachine.IsInitialized)
		{
			_bombStateHandle = base.StateMachine?.GetStateById("zombie.jackgene.bomb");
			StateHandle bombStateHandle = _bombStateHandle;
			if (bombStateHandle != null && bombStateHandle.IsValid)
			{
				_bombStateHandle.Entered += BombEntered;
				_bombStateHandle.Exited += BombExited;
				_bombStateHandle.PhysicsProcessing += BombProcessing;
				_stateSignalsConnected = true;
			}
		}
	}

	private void DisconnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			if (_bombStateHandle != null)
			{
				_bombStateHandle.Entered -= BombEntered;
				_bombStateHandle.Exited -= BombExited;
				_bombStateHandle.PhysicsProcessing -= BombProcessing;
			}
			_bombStateHandle = null;
			_stateSignalsConnected = false;
		}
	}

	public override void PrepareForProgressRestore()
	{
		base.PrepareForProgressRestore();
		_restoredFromProgress = true;
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			ConnectStateSignals();
			AddToGroup("JackBox");
			time = GD.RandRange(9.0, 20.0);
			TryStartBattleEntry();
		}
	}

	private async void TryStartBattleEntry()
	{
		if (_battleEntryStarted || !inGame || editorPreviewMode || isDestroy || base.IsProgressRestoreInFlight || !TowerDefenseManager._IsGameRunning())
		{
			return;
		}
		_battleEntryStarted = true;
		if (!hasJackBox || nearDie || die)
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(JackInTheBoxPlayer))
		{
			JackInTheBoxPlayer = AudioManager.Instance.MemberFind("JackInTheBox");
			JackInTheBoxPlayer.MaxPolyphony = 1;
			JackInTheBoxPlayer.ProcessMode = ProcessModeEnum.Pausable;
		}
		JackInTheBoxPlayer.Play();
		if (_restoredFromProgress)
		{
			return;
		}
		await ToSignal(GetTree().CreateTimer(0.1, processAlways: false), SceneTreeTimer.SignalName.Timeout);
		if (GodotObject.IsInstanceValid(this) && IsInsideTree() && !isDestroy && TowerDefenseManager._IsGameRunning() && TowerDefenseManager.GetGameMethod() == TowerDefenseEnum.LEVEL_FINISH_METHOD.VASE)
		{
			explode = true;
			instance.unUseBuffFlags += 128;
			if (hasJackBox && !nearDie && !die)
			{
				SendStateEvent("ToBomb");
			}
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		RemoveFromGroup("JackBox");
		if (GodotObject.IsInstanceValid(JackInTheBoxPlayer) && GetTree().GetNodeCountInGroup("JackBox") <= 0)
		{
			JackInTheBoxPlayer.Stop();
		}
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (Engine.IsEditorHint() || !TowerDefenseManager.Instance.IsGameRunning() || !inGame)
		{
			return;
		}
		TryStartBattleEntry();
		if (!base.IsInsideComponentBattlefield || explode || !CanRunAbility())
		{
			return;
		}
		if (timer < time)
		{
			timer += delta * timeScale;
			return;
		}
		explode = true;
		instance.unUseBuffFlags += 128;
		if (hasJackBox)
		{
			SendStateEvent("ToBomb");
		}
	}

	public virtual void BombEntered()
	{
		AudioManager.Instance.AudioPlay("JackSurprise");
		base.sprite.SetAnimation("Bomb", loop: false, 0.2);
	}

	public virtual void BombProcessing(double delta)
	{
		base.sprite.timeScale = timeScale * 2.0;
	}

	public virtual void BombExited()
	{
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (clip == "Bomb" && !over)
		{
			over = true;
			if (hasJackBox)
			{
				CreateEffect();
				Destroy();
			}
			else
			{
				base.Walk();
			}
		}
	}

	public override void ArmorHitpointsEmpty(string armorName)
	{
		base.ArmorHitpointsEmpty(armorName);
		if (armorName == "JackGenebox")
		{
			hasJackBox = false;
		}
	}

	public override void Walk()
	{
		if (!explode)
		{
			base.Walk();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["hasJackBox"] = hasJackBox,
			["over"] = over,
			["timer"] = timer,
			["time"] = time,
			["explode"] = explode
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		hasJackBox = data.GetValueOrDefault("hasJackBox", hasJackBox).AsBool();
		over = data.GetValueOrDefault("over", over).AsBool();
		timer = Math.Max(0.0, data.GetValueOrDefault("timer", timer).AsDouble());
		time = Math.Max(0.0, data.GetValueOrDefault("time", time).AsDouble());
		explode = data.GetValueOrDefault("explode", explode).AsBool();
	}

	public void CreateEffect()
	{
		ViewManager.Instance.CameraShake(new Vector2(GD.RandRange(-1, 1), GD.RandRange(-1, 1)), 5.0, 0.05, 4);
		Vector2 logicalGlobalPosition = GetLogicalGlobalPosition();
		TowerDefenseEffectParticlesOnce towerDefenseEffectParticlesOnce = TowerDefenseManager.CreateEffectParticlesOnce(JACKBOX_EXPLOSION, base.gridPos);
		towerDefenseEffectParticlesOnce.GlobalPosition = logicalGlobalPosition;
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseEffectParticlesOnce, forceReadableName: false, InternalMode.Disabled);
		TowerDefenseExplode.CreateExplode(logicalGlobalPosition, new Vector2(1.5f, 1.5f), eventList, new Array<TowerDefenseCharacter>(), base.camp, -1, suppressDeathrattles: true);
		AudioManager.Instance.AudioPlay("ExplodeCherrybomb");
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(16)
		{
			new MethodInfo(MethodName.ConnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DisconnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PrepareForProgressRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryStartBattleEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BombEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BombProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BombExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ArmorHitpointsEmpty, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "armorName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Walk, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CreateEffect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
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
		if (method == MethodName.PrepareForProgressRestore && args.Count == 0)
		{
			PrepareForProgressRestore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryStartBattleEntry && args.Count == 0)
		{
			TryStartBattleEntry();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BombEntered && args.Count == 0)
		{
			BombEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BombProcessing && args.Count == 1)
		{
			BombProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BombExited && args.Count == 0)
		{
			BombExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ArmorHitpointsEmpty && args.Count == 1)
		{
			ArmorHitpointsEmpty(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Walk && args.Count == 0)
		{
			Walk();
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
		if (method == MethodName.CreateEffect && args.Count == 0)
		{
			CreateEffect();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.ConnectStateSignals)
		{
			return true;
		}
		if (method == MethodName.DisconnectStateSignals)
		{
			return true;
		}
		if (method == MethodName.PrepareForProgressRestore)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.TryStartBattleEntry)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.BombEntered)
		{
			return true;
		}
		if (method == MethodName.BombProcessing)
		{
			return true;
		}
		if (method == MethodName.BombExited)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.ArmorHitpointsEmpty)
		{
			return true;
		}
		if (method == MethodName.Walk)
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
		if (method == MethodName.CreateEffect)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._stateSignalsConnected)
		{
			_stateSignalsConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._battleEntryStarted)
		{
			_battleEntryStarted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._restoredFromProgress)
		{
			_restoredFromProgress = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.eventList)
		{
			eventList = VariantUtils.ConvertToArray<TowerDefenseCharacterEventBase>(in value);
			return true;
		}
		if (name == PropertyName.hasJackBox)
		{
			hasJackBox = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.over)
		{
			over = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.timer)
		{
			timer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.time)
		{
			time = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.explode)
		{
			explode = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._stateSignalsConnected)
		{
			value = VariantUtils.CreateFrom(in _stateSignalsConnected);
			return true;
		}
		if (name == PropertyName._battleEntryStarted)
		{
			value = VariantUtils.CreateFrom(in _battleEntryStarted);
			return true;
		}
		if (name == PropertyName._restoredFromProgress)
		{
			value = VariantUtils.CreateFrom(in _restoredFromProgress);
			return true;
		}
		if (name == PropertyName.eventList)
		{
			value = VariantUtils.CreateFromArray(eventList);
			return true;
		}
		if (name == PropertyName.hasJackBox)
		{
			value = VariantUtils.CreateFrom(in hasJackBox);
			return true;
		}
		if (name == PropertyName.over)
		{
			value = VariantUtils.CreateFrom(in over);
			return true;
		}
		if (name == PropertyName.timer)
		{
			value = VariantUtils.CreateFrom(in timer);
			return true;
		}
		if (name == PropertyName.time)
		{
			value = VariantUtils.CreateFrom(in time);
			return true;
		}
		if (name == PropertyName.explode)
		{
			value = VariantUtils.CreateFrom(in explode);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._stateSignalsConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._battleEntryStarted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._restoredFromProgress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Array, PropertyName.eventList, PropertyHint.TypeString, "24/17:TowerDefenseCharacterEventBase", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.hasJackBox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.over, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.explode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._stateSignalsConnected, Variant.From(in _stateSignalsConnected));
		info.AddProperty(PropertyName._battleEntryStarted, Variant.From(in _battleEntryStarted));
		info.AddProperty(PropertyName._restoredFromProgress, Variant.From(in _restoredFromProgress));
		info.AddProperty(PropertyName.eventList, Variant.CreateFrom(eventList));
		info.AddProperty(PropertyName.hasJackBox, Variant.From(in hasJackBox));
		info.AddProperty(PropertyName.over, Variant.From(in over));
		info.AddProperty(PropertyName.timer, Variant.From(in timer));
		info.AddProperty(PropertyName.time, Variant.From(in time));
		info.AddProperty(PropertyName.explode, Variant.From(in explode));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._stateSignalsConnected, out var value))
		{
			_stateSignalsConnected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._battleEntryStarted, out var value2))
		{
			_battleEntryStarted = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._restoredFromProgress, out var value3))
		{
			_restoredFromProgress = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.eventList, out var value4))
		{
			eventList = value4.AsGodotArray<TowerDefenseCharacterEventBase>();
		}
		if (info.TryGetProperty(PropertyName.hasJackBox, out var value5))
		{
			hasJackBox = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.over, out var value6))
		{
			over = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.timer, out var value7))
		{
			timer = value7.As<double>();
		}
		if (info.TryGetProperty(PropertyName.time, out var value8))
		{
			time = value8.As<double>();
		}
		if (info.TryGetProperty(PropertyName.explode, out var value9))
		{
			explode = value9.As<bool>();
		}
	}
}
