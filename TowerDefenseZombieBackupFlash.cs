using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Challenge/BackupFlash/Scene/TowerDefenseZombieBackupFlash.cs")]
public class TowerDefenseZombieBackupFlash : TowerDefenseZombie, IFlashStepZombie, IDancer, INetworkSpawnStateReceiver
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public static readonly StringName GetJackson = "GetJackson";

		public static readonly StringName SetJackson = "SetJackson";

		public static readonly StringName ConnectStateSignals = "ConnectStateSignals";

		public static readonly StringName DisconnectStateSignals = "DisconnectStateSignals";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName DanceEntered = "DanceEntered";

		public static readonly StringName DanceProcessing = "DanceProcessing";

		public static readonly StringName DanceExited = "DanceExited";

		public new static readonly StringName WalkEntered = "WalkEntered";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName DieProcessing = "DieProcessing";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public new static readonly StringName Hypnoses = "Hypnoses";

		public static readonly StringName OutJackson = "OutJackson";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName OnFlashStepFell = "OnFlashStepFell";

		public new static readonly StringName NormalizeMainStateEvent = "NormalizeMainStateEvent";

		public new static readonly StringName DieEntered = "DieEntered";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public static readonly StringName ImportNetworkSpawnState = "ImportNetworkSpawnState";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName IsFlashingStep = "IsFlashingStep";

		public new static readonly StringName IsPhantomUntargetable = "IsPhantomUntargetable";

		public new static readonly StringName IsHardControlImmune = "IsHardControlImmune";

		public static readonly StringName CanFlashStep = "CanFlashStep";

		public static readonly StringName _stateSignalsConnected = "_stateSignalsConnected";

		public static readonly StringName walkTime = "walkTime";

		public static readonly StringName danceTime = "danceTime";

		public static readonly StringName jackson = "jackson";

		public static readonly StringName _pendingJacksonName = "_pendingJacksonName";

		public static readonly StringName _flashLeader = "_flashLeader";

		public static readonly StringName _pendingLeaderSyncId = "_pendingLeaderSyncId";

		public static readonly StringName _pendingLeaderName = "_pendingLeaderName";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private StateHandle _danceStateHandle;

	private bool _stateSignalsConnected;

	public int walkTime = 4;

	public int danceTime = 2;

	public TowerDefenseCharacter jackson;

	public string _pendingJacksonName = "";

	private TowerDefenseZombieDiscoFlash _flashLeader;

	private int _pendingLeaderSyncId = -1;

	private string _pendingLeaderName = "";

	public FlashStepComponent FlashStep { get; private set; }

	public bool IsFlashingStep => FlashStep?.IsFlashing ?? false;

	public override bool IsPhantomUntargetable => IsFlashingStep;

	public override bool IsHardControlImmune => IsPhantomUntargetable;

	public bool CanFlashStep => true;

	public TowerDefenseCharacter GetJackson()
	{
		return jackson;
	}

	public void SetJackson(TowerDefenseCharacter value)
	{
		if (GodotObject.IsInstanceValid(_flashLeader))
		{
			_flashLeader.RemoveFlashDancer(this);
		}
		jackson = value;
		_flashLeader = value as TowerDefenseZombieDiscoFlash;
		if (GodotObject.IsInstanceValid(_flashLeader))
		{
			_flashLeader.RegisterFlashDancer(this);
		}
	}

	private void ConnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			return;
		}
		IStateMachineController stateMachine = base.StateMachine;
		if (stateMachine != null && stateMachine.IsInitialized)
		{
			_danceStateHandle = base.StateMachine?.GetStateById("zombie.backup.dance");
			StateHandle danceStateHandle = _danceStateHandle;
			if (danceStateHandle != null && danceStateHandle.IsValid)
			{
				_danceStateHandle.Entered += DanceEntered;
				_danceStateHandle.Exited += DanceExited;
				_danceStateHandle.PhysicsProcessing += DanceProcessing;
				_stateSignalsConnected = true;
			}
		}
	}

	private void DisconnectStateSignals()
	{
		if (_stateSignalsConnected)
		{
			if (_danceStateHandle != null)
			{
				_danceStateHandle.Entered -= DanceEntered;
				_danceStateHandle.Exited -= DanceExited;
				_danceStateHandle.PhysicsProcessing -= DanceProcessing;
			}
			_danceStateHandle = null;
			_stateSignalsConnected = false;
		}
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(_flashLeader))
		{
			_flashLeader.RemoveFlashDancer(this);
		}
		base._ExitTree();
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			ConnectStateSignals();
			FlashStep = componentManager.GetRuntime<FlashStepComponent>();
		}
	}

	public void DanceEntered()
	{
		danceTime = 2;
		base.sprite.SetAnimation("ArmRise", loop: true, 0.2);
	}

	public void DanceProcessing(double delta)
	{
		base.sprite.timeScale = timeScale * 1.0;
		if (!base.sprite.pause && attackComponent.CanAttack())
		{
			Attack();
		}
	}

	public void DanceExited()
	{
	}

	public override void WalkEntered()
	{
		base.WalkEntered();
		walkTime = 4;
	}

	public override void WalkProcessing(double delta)
	{
		if (GodotObject.IsInstanceValid(jackson))
		{
			groundMoveComponent.SetAlive(((TowerDefenseZombie)jackson).groundMoveComponent?.Alive ?? false);
		}
		else
		{
			groundMoveComponent.SetAlive(value: true);
		}
		base.WalkProcessing(delta);
	}

	public override void DieProcessing(double delta)
	{
		base.DieProcessing(delta);
		base.sprite.timeScale = timeScale * 2.0;
	}

	public override void AnimeCompleted(string clip)
	{
		if (!die)
		{
			FlashStepComponent flashStep = FlashStep;
			if (flashStep != null && flashStep.AnimeCompleted(clip))
			{
				return;
			}
		}
		base.AnimeCompleted(clip);
		if ((clip == "Walk" || clip == "ArmRise") && clip != base.sprite.clip)
		{
			return;
		}
		if (!(clip == "Walk"))
		{
			if (!(clip == "ArmRise"))
			{
				return;
			}
			danceTime--;
			if (!die && !nearDie)
			{
				if (danceTime > 0)
				{
					return;
				}
				if (GodotObject.IsInstanceValid(jackson))
				{
					if (!GodotObject.IsInstanceValid(jackson))
					{
						return;
					}
					GroundMoveComponent obj = ((TowerDefenseZombie)jackson).groundMoveComponent;
					if (obj != null && obj.Alive)
					{
						return;
					}
				}
				Walk();
			}
			else
			{
				OutJackson();
				Die();
			}
		}
		else
		{
			if (GodotObject.IsInstanceValid(TowerDefenseManager.CurrentControl) && !TowerDefenseManager.CurrentControl.isGameRunning)
			{
				return;
			}
			walkTime--;
			if (!die && !nearDie)
			{
				if (walkTime > 0)
				{
					return;
				}
				if (GodotObject.IsInstanceValid(jackson))
				{
					if (!GodotObject.IsInstanceValid(jackson))
					{
						return;
					}
					GroundMoveComponent obj2 = ((TowerDefenseZombie)jackson).groundMoveComponent;
					if (obj2 != null && obj2.Alive)
					{
						return;
					}
				}
				SendStateEvent("ToDance");
			}
			else
			{
				OutJackson();
				Die();
			}
		}
	}

	public override void Hypnoses(double time = -1.0, bool canFliter = true, TowerDefenseCharacterBuffHypnoses hypnosesConfig = null)
	{
		bool flag = instance?.hypnoses ?? false;
		base.Hypnoses(time, canFliter, hypnosesConfig);
		if (GodotObject.IsInstanceValid(instance) && instance.hypnoses != flag)
		{
			OutJackson();
		}
	}

	public void OutJackson()
	{
		if (GodotObject.IsInstanceValid(this.jackson))
		{
			if (!this.jackson.instance.hypnoses && this.jackson is IJackson jackson)
			{
				jackson.RemoveDancer(this);
			}
			this.jackson = null;
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = new Dictionary();
		if (GodotObject.IsInstanceValid(jackson))
		{
			dictionary["jacksonNodeName"] = jackson.Name;
		}
		dictionary["flashLeaderName"] = (GodotObject.IsInstanceValid(_flashLeader) ? _flashLeader.Name.ToString() : _pendingLeaderName);
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		if (data.ContainsKey("jacksonNodeName"))
		{
			_pendingJacksonName = data["jacksonNodeName"].AsString();
		}
		_pendingLeaderName = data.GetValueOrDefault("flashLeaderName", "").AsString();
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (_pendingJacksonName != "")
		{
			Node2D node2D = TowerDefenseManager.GetCharacterNode();
			if (GodotObject.IsInstanceValid(node2D))
			{
				Node nodeOrNull = node2D.GetNodeOrNull(_pendingJacksonName);
				if (GodotObject.IsInstanceValid(nodeOrNull))
				{
					jackson = nodeOrNull as TowerDefenseCharacter;
				}
			}
			_pendingJacksonName = "";
		}
		if (jackson != _flashLeader)
		{
			SetJackson(jackson);
		}
		if (_pendingLeaderSyncId >= 0 && GodotObject.IsInstanceValid(TowerDefenseManager.CurrentControl) && TowerDefenseManager.CurrentControl._syncCharacters.TryGetValue(_pendingLeaderSyncId, out var value))
		{
			SetJackson(value);
			_pendingLeaderSyncId = -1;
		}
		if (_pendingLeaderName != "")
		{
			TowerDefenseZombieDiscoFlash towerDefenseZombieDiscoFlash = TowerDefenseManager.GetCharacterNode()?.GetNodeOrNull<TowerDefenseZombieDiscoFlash>(_pendingLeaderName);
			if (GodotObject.IsInstanceValid(towerDefenseZombieDiscoFlash))
			{
				SetJackson(towerDefenseZombieDiscoFlash);
				_pendingLeaderName = "";
			}
		}
	}

	public void OnFlashStepFell()
	{
		if (GodotObject.IsInstanceValid(_flashLeader))
		{
			_flashLeader.OnDancerFell(this);
		}
	}

	protected override StringName NormalizeMainStateEvent(StringName eventName)
	{
		eventName = base.NormalizeMainStateEvent(eventName);
		return FlashStep?.NormalizeEvent(eventName) ?? eventName;
	}

	public override void DieEntered()
	{
		FlashStep?.PrepareDeath();
		base.DieEntered();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return new Dictionary
		{
			["rev"] = GetNetworkSpecialStateRevision(),
			["flashLeaderSyncId"] = (GodotObject.IsInstanceValid(_flashLeader) ? _flashLeader.syncId : _pendingLeaderSyncId)
		};
	}

	public override int GetNetworkSpecialStateRevision()
	{
		if (!GodotObject.IsInstanceValid(_flashLeader))
		{
			return 0;
		}
		return _flashLeader.syncId + 1;
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		ImportNetworkSpawnState(data);
	}

	public void ImportNetworkSpawnState(Dictionary data)
	{
		_pendingLeaderSyncId = data.GetValueOrDefault("flashLeaderSyncId", data.GetValueOrDefault("dancer_parent_sync_id", -1)).AsInt32();
		if (_pendingLeaderSyncId < 0)
		{
			SetJackson(null);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(25)
		{
			new MethodInfo(MethodName.GetJackson, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetJackson, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "value", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ConnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DisconnectStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DanceEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DanceProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DanceExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WalkEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DieProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Hypnoses, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "time", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "canFliter", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "hypnosesConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null),
			new MethodInfo(MethodName.OutJackson, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnFlashStepFell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.NormalizeMainStateEvent, new PropertyInfo(Variant.Type.StringName, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.StringName, "eventName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DieEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetNetworkSpecialStateRevision, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ImportNetworkSpawnState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetJackson && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(GetJackson());
			return true;
		}
		if (method == MethodName.SetJackson && args.Count == 1)
		{
			SetJackson(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
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
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DanceEntered && args.Count == 0)
		{
			DanceEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DanceProcessing && args.Count == 1)
		{
			DanceProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DanceExited && args.Count == 0)
		{
			DanceExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WalkEntered && args.Count == 0)
		{
			WalkEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WalkProcessing && args.Count == 1)
		{
			WalkProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DieProcessing && args.Count == 1)
		{
			DieProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Hypnoses && args.Count == 3)
		{
			Hypnoses(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<TowerDefenseCharacterBuffHypnoses>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OutJackson && args.Count == 0)
		{
			OutJackson();
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
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFlashStepFell && args.Count == 0)
		{
			OnFlashStepFell();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NormalizeMainStateEvent && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<StringName>(NormalizeMainStateEvent(VariantUtils.ConvertTo<StringName>(in args[0])));
			return true;
		}
		if (method == MethodName.DieEntered && args.Count == 0)
		{
			DieEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpecialState());
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(GetNetworkSpecialStateRevision());
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState && args.Count == 1)
		{
			ImportNetworkSpecialState(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ImportNetworkSpawnState && args.Count == 1)
		{
			ImportNetworkSpawnState(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.GetJackson)
		{
			return true;
		}
		if (method == MethodName.SetJackson)
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.DanceEntered)
		{
			return true;
		}
		if (method == MethodName.DanceProcessing)
		{
			return true;
		}
		if (method == MethodName.DanceExited)
		{
			return true;
		}
		if (method == MethodName.WalkEntered)
		{
			return true;
		}
		if (method == MethodName.WalkProcessing)
		{
			return true;
		}
		if (method == MethodName.DieProcessing)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.Hypnoses)
		{
			return true;
		}
		if (method == MethodName.OutJackson)
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
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.OnFlashStepFell)
		{
			return true;
		}
		if (method == MethodName.NormalizeMainStateEvent)
		{
			return true;
		}
		if (method == MethodName.DieEntered)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.GetNetworkSpecialStateRevision)
		{
			return true;
		}
		if (method == MethodName.ImportNetworkSpecialState)
		{
			return true;
		}
		if (method == MethodName.ImportNetworkSpawnState)
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
		if (name == PropertyName.walkTime)
		{
			walkTime = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.danceTime)
		{
			danceTime = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.jackson)
		{
			jackson = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		if (name == PropertyName._pendingJacksonName)
		{
			_pendingJacksonName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._flashLeader)
		{
			_flashLeader = VariantUtils.ConvertTo<TowerDefenseZombieDiscoFlash>(in value);
			return true;
		}
		if (name == PropertyName._pendingLeaderSyncId)
		{
			_pendingLeaderSyncId = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._pendingLeaderName)
		{
			_pendingLeaderName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.IsFlashingStep)
		{
			from = IsFlashingStep;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsPhantomUntargetable)
		{
			from = IsPhantomUntargetable;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsHardControlImmune)
		{
			from = IsHardControlImmune;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CanFlashStep)
		{
			from = CanFlashStep;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._stateSignalsConnected)
		{
			value = VariantUtils.CreateFrom(in _stateSignalsConnected);
			return true;
		}
		if (name == PropertyName.walkTime)
		{
			value = VariantUtils.CreateFrom(in walkTime);
			return true;
		}
		if (name == PropertyName.danceTime)
		{
			value = VariantUtils.CreateFrom(in danceTime);
			return true;
		}
		if (name == PropertyName.jackson)
		{
			value = VariantUtils.CreateFrom(in jackson);
			return true;
		}
		if (name == PropertyName._pendingJacksonName)
		{
			value = VariantUtils.CreateFrom(in _pendingJacksonName);
			return true;
		}
		if (name == PropertyName._flashLeader)
		{
			value = VariantUtils.CreateFrom(in _flashLeader);
			return true;
		}
		if (name == PropertyName._pendingLeaderSyncId)
		{
			value = VariantUtils.CreateFrom(in _pendingLeaderSyncId);
			return true;
		}
		if (name == PropertyName._pendingLeaderName)
		{
			value = VariantUtils.CreateFrom(in _pendingLeaderName);
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
			new PropertyInfo(Variant.Type.Int, PropertyName.walkTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.danceTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.jackson, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._pendingJacksonName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._flashLeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._pendingLeaderSyncId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._pendingLeaderName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsFlashingStep, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsPhantomUntargetable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsHardControlImmune, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanFlashStep, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._stateSignalsConnected, Variant.From(in _stateSignalsConnected));
		info.AddProperty(PropertyName.walkTime, Variant.From(in walkTime));
		info.AddProperty(PropertyName.danceTime, Variant.From(in danceTime));
		info.AddProperty(PropertyName.jackson, Variant.From(in jackson));
		info.AddProperty(PropertyName._pendingJacksonName, Variant.From(in _pendingJacksonName));
		info.AddProperty(PropertyName._flashLeader, Variant.From(in _flashLeader));
		info.AddProperty(PropertyName._pendingLeaderSyncId, Variant.From(in _pendingLeaderSyncId));
		info.AddProperty(PropertyName._pendingLeaderName, Variant.From(in _pendingLeaderName));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._stateSignalsConnected, out var value))
		{
			_stateSignalsConnected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.walkTime, out var value2))
		{
			walkTime = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName.danceTime, out var value3))
		{
			danceTime = value3.As<int>();
		}
		if (info.TryGetProperty(PropertyName.jackson, out var value4))
		{
			jackson = value4.As<TowerDefenseCharacter>();
		}
		if (info.TryGetProperty(PropertyName._pendingJacksonName, out var value5))
		{
			_pendingJacksonName = value5.As<string>();
		}
		if (info.TryGetProperty(PropertyName._flashLeader, out var value6))
		{
			_flashLeader = value6.As<TowerDefenseZombieDiscoFlash>();
		}
		if (info.TryGetProperty(PropertyName._pendingLeaderSyncId, out var value7))
		{
			_pendingLeaderSyncId = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._pendingLeaderName, out var value8))
		{
			_pendingLeaderName = value8.As<string>();
		}
	}
}
