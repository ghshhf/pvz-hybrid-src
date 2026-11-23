using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter10/MagicSoul/Scene/TowerDefenseZombieMagicSoul.cs")]
public class TowerDefenseZombieMagicSoul : TowerDefenseZombie, INetworkSpawnStateReceiver
{
	public new class MethodName : TowerDefenseZombie.MethodName
	{
		public static readonly StringName BindOwner = "BindOwner";

		public static readonly StringName SetOrbitAngle = "SetOrbitAngle";

		public static readonly StringName SetupOwnerlessOrbit = "SetupOwnerlessOrbit";

		public static readonly StringName RebalanceOwnerlessOrbit = "RebalanceOwnerlessOrbit";

		public static readonly StringName ApplyShadowVisual = "ApplyShadowVisual";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName BatchUpdate = "BatchUpdate";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";

		public new static readonly StringName WalkEntered = "WalkEntered";

		public static readonly StringName TryRelocateNaturalSpawnToRandomCell = "TryRelocateNaturalSpawnToRandomCell";

		public static readonly StringName PickNaturalSpawnColumn = "PickNaturalSpawnColumn";

		public static readonly StringName EntryPositionSeed = "EntryPositionSeed";

		public static readonly StringName PickRandomEnabledLine = "PickRandomEnabledLine";

		public new static readonly StringName WalkProcessing = "WalkProcessing";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public static readonly StringName OrbitProcessing = "OrbitProcessing";

		public static readonly StringName EndSoul = "EndSoul";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName DieEntered = "DieEntered";

		public new static readonly StringName DieProcessing = "DieProcessing";

		public new static readonly StringName ExportNetworkSpawnState = "ExportNetworkSpawnState";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public static readonly StringName ImportNetworkSpawnState = "ImportNetworkSpawnState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefenseZombie.PropertyName
	{
		public static readonly StringName HasSummonOwner = "HasSummonOwner";

		public new static readonly StringName CanBeTransformed = "CanBeTransformed";

		public new static readonly StringName IsHardControlImmune = "IsHardControlImmune";

		public static readonly StringName OrbitAngle = "OrbitAngle";

		public static readonly StringName _owner = "_owner";

		public static readonly StringName _ownerKey = "_ownerKey";

		public static readonly StringName _angle = "_angle";

		public static readonly StringName _ending = "_ending";

		public static readonly StringName _orbitAnchor = "_orbitAnchor";

		public static readonly StringName _hasOrbitAnchor = "_hasOrbitAnchor";

		public static readonly StringName isShadow = "isShadow";

		public static readonly StringName _shadowVisualApplied = "_shadowVisualApplied";

		public static readonly StringName _naturalSpawnLine = "_naturalSpawnLine";

		public static readonly StringName _naturalSpawnEntryX = "_naturalSpawnEntryX";
	}

	public new class SignalName : TowerDefenseZombie.SignalName
	{
	}

	private const double OwnerLostGrace = 1.0;

	private const string OwnerlessOrbitGroup = "MagicSoulOwnerlessOrbit";

	private const ulong NaturalSpawnCellSalt = 5571884391759561541uL;

	private TowerDefenseZombieEvoker _owner;

	private string _ownerKey = "";

	private CharacterTimerComponent _timerComponent;

	private double _angle = Math.PI;

	private bool _ending;

	private Vector2 _orbitAnchor;

	private bool _hasOrbitAnchor;

	private readonly List<TowerDefenseCharacter> _targets = new List<TowerDefenseCharacter>();

	public bool isShadow;

	private bool _shadowVisualApplied;

	private int _naturalSpawnLine;

	private float _naturalSpawnEntryX;

	public bool HasSummonOwner
	{
		get
		{
			if (!GodotObject.IsInstanceValid(_owner))
			{
				return !string.IsNullOrEmpty(_ownerKey);
			}
			return true;
		}
	}

	public override bool CanBeTransformed => false;

	public override bool IsHardControlImmune => true;

	public double OrbitAngle => _angle;

	public void BindOwner(TowerDefenseZombieEvoker owner)
	{
		_owner = owner;
		_ownerKey = owner.summonOwnerKey;
		owner.RegisterSoul(this);
	}

	public void SetOrbitAngle(double angle)
	{
		_angle = angle;
	}

	private void SetupOwnerlessOrbit()
	{
		if (!_hasOrbitAnchor)
		{
			_orbitAnchor = GetLogicalGlobalPosition();
			_hasOrbitAnchor = true;
		}
		AddToGroup("MagicSoulOwnerlessOrbit");
		RebalanceOwnerlessOrbit();
	}

	private void RebalanceOwnerlessOrbit()
	{
		List<TowerDefenseZombieMagicSoul> list = new List<TowerDefenseZombieMagicSoul>();
		foreach (Node item in GetTree().GetNodesInGroup("MagicSoulOwnerlessOrbit"))
		{
			if (item is TowerDefenseZombieMagicSoul towerDefenseZombieMagicSoul && towerDefenseZombieMagicSoul != this && !towerDefenseZombieMagicSoul.HasSummonOwner && !towerDefenseZombieMagicSoul.die && !towerDefenseZombieMagicSoul.isDestroy && towerDefenseZombieMagicSoul._hasOrbitAnchor && !(towerDefenseZombieMagicSoul._orbitAnchor.DistanceTo(_orbitAnchor) > 1f))
			{
				list.Add(towerDefenseZombieMagicSoul);
			}
		}
		list.Add(this);
		list.Sort((TowerDefenseZombieMagicSoul a, TowerDefenseZombieMagicSoul b) => a.GetInstanceId().CompareTo(b.GetInstanceId()));
		double angle = list[0]._angle;
		for (int num = 0; num < list.Count; num++)
		{
			list[num]._angle = angle + (double)num * (Math.PI * 2.0) / (double)list.Count;
		}
	}

	private void ApplyShadowVisual()
	{
		if (isShadow && !_shadowVisualApplied && IsInsideTree())
		{
			_shadowVisualApplied = true;
			RefreshResourceComponentFacades();
			ShaderEffectComponent obj = shaderEffectComponent;
			if (obj != null && !obj.IsReleased)
			{
				shaderEffectComponent.SetSpriteGroupShaderParameter("hologram", true);
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint() || editorPreviewMode)
		{
			return;
		}
		instance.invincible = true;
		instance.maskFlags = 0;
		instance.canBeCollection = false;
		targetRegistrationComponent.canCarry = false;
		shadowSprite.Visible = false;
		_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout += OnTimerTimeout;
		}
		if (!HasSummonOwner)
		{
			if (base.gridPos.X < 1 && base.gridPos.Y >= 1)
			{
				_naturalSpawnLine = base.gridPos.Y;
				_naturalSpawnEntryX = GetLogicalGlobalPosition().X;
			}
			SetupOwnerlessOrbit();
		}
		if (isShadow)
		{
			Callable.From(ApplyShadowVisual).CallDeferred();
		}
	}

	public override void BatchUpdate(double delta)
	{
		base.BatchUpdate(delta);
		if (isShadow && !_shadowVisualApplied)
		{
			ApplyShadowVisual();
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (timerName == "OwnerLost")
		{
			EndSoul();
		}
	}

	public override void WalkEntered()
	{
		TryRelocateNaturalSpawnToRandomCell();
		groundMoveComponent?.SetAlive(value: false);
		base.sprite.SetAnimation("Idle");
	}

	private void TryRelocateNaturalSpawnToRandomCell()
	{
		if (_naturalSpawnLine < 1)
		{
			return;
		}
		TowerDefenseBattleFeatureMap mapFeature = TowerDefenseManager.GetMapFeature();
		if (!GodotObject.IsInstanceValid(mapFeature))
		{
			return;
		}
		Vector2I gridNum = mapFeature.config.gridNum;
		int num = _naturalSpawnLine;
		if (gridNum.X <= 0 || gridNum.Y <= 0 || num > gridNum.Y)
		{
			return;
		}
		if (num >= mapFeature.lineUse.Count || !mapFeature.lineUse[num])
		{
			num = PickRandomEnabledLine(mapFeature, gridNum.Y);
			if (num < 1)
			{
				return;
			}
		}
		Vector2I vector2I = new Vector2I(PickNaturalSpawnColumn(gridNum.X, num), num);
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(vector2I);
		if (GodotObject.IsInstanceValid(mapCell))
		{
			_naturalSpawnLine = 0;
			base.gridPos = vector2I;
			SetLogicalGlobalPosition(TowerDefenseManager.GetMapCellPlantPos(vector2I));
			base.groundHeight = mapCell.GetGroundHeight();
			base.z = base.groundHeight;
			_orbitAnchor = GetLogicalGlobalPosition();
			_hasOrbitAnchor = true;
			RebalanceOwnerlessOrbit();
		}
	}

	private int PickNaturalSpawnColumn(int columnCount, int line)
	{
		return (int)(((syncId >= 0) ? NetworkDeterministicSeed.ForCharacterEvent(syncId, line, 5571884391759561541uL) : EntryPositionSeed(line)) % (uint)columnCount) + 1;
	}

	private ulong EntryPositionSeed(int line)
	{
		long num = ((uint)Mathf.RoundToInt(_naturalSpawnEntryX * 4f) ^ ((uint)line * -7046029254386353131L)) * -4658895280553007687L;
		return (ulong)(num ^ (num >>> 31));
	}

	private static int PickRandomEnabledLine(TowerDefenseBattleFeatureMap mapFeature, int gridNumY)
	{
		uint num = GD.Randi() % (uint)gridNumY;
		for (int i = 0; i < gridNumY; i++)
		{
			int num2 = (int)((uint)((int)num + i) % (uint)gridNumY + 1);
			if (num2 < mapFeature.lineUse.Count && mapFeature.lineUse[num2])
			{
				return num2;
			}
		}
		return -1;
	}

	public override void WalkProcessing(double delta)
	{
		OrbitProcessing(delta);
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		OrbitProcessing(delta);
	}

	private void OrbitProcessing(double delta)
	{
		if (!inGame || die || isDestroy || base.sprite.pause || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		if (_ending)
		{
			EndSoul();
			return;
		}
		if (!GodotObject.IsInstanceValid(_owner) && HasSummonOwner)
		{
			foreach (Node item in GetTree().GetNodesInGroup("Evoker"))
			{
				if (item is TowerDefenseZombieEvoker towerDefenseZombieEvoker && towerDefenseZombieEvoker.summonOwnerKey == _ownerKey)
				{
					BindOwner(towerDefenseZombieEvoker);
					break;
				}
			}
			if (!GodotObject.IsInstanceValid(_owner))
			{
				CharacterTimerComponent timerComponent = _timerComponent;
				if (timerComponent != null && !timerComponent.IsReleased && !_timerComponent.IsRunning("OwnerLost"))
				{
					_timerComponent.Run("OwnerLost", 1.0);
				}
				return;
			}
		}
		CharacterTimerComponent timerComponent2 = _timerComponent;
		if (timerComponent2 != null && !timerComponent2.IsReleased && _timerComponent.IsRunning("OwnerLost"))
		{
			_timerComponent.Stop("OwnerLost");
		}
		Vector2 vector;
		if (GodotObject.IsInstanceValid(_owner))
		{
			if (_owner.die || _owner.isDestroy)
			{
				EndSoul();
				return;
			}
			base.camp = _owner.camp;
			vector = _owner.GetLogicalGlobalPosition();
		}
		else
		{
			if (!_hasOrbitAnchor)
			{
				_orbitAnchor = GetLogicalGlobalPosition();
				_hasOrbitAnchor = true;
			}
			vector = _orbitAnchor;
		}
		_angle = (_angle + delta * (Math.PI * 2.0) / 6.0) % (Math.PI * 2.0);
		Vector2 mapGridSize = TowerDefenseManager.Instance.GetMapGridSize();
		SetLogicalGlobalPosition(vector + new Vector2((float)Math.Cos(_angle) * mapGridSize.X, (float)Math.Sin(_angle) * mapGridSize.Y));
		base.gridPos = TowerDefenseManager.Instance.GetMapGridPos(GetLogicalGlobalPosition());
		Rect2 checkRect = AabbShapeUtil.RectFromCenter(GetLogicalGlobalPosition(), new Vector2(34f, 40f));
		_targets.Clear();
		TowerDefenseManager.Instance.characterRegistry.FillCharactersIntersectingRectListExcludingCamp(checkRect, base.camp, _targets);
		foreach (TowerDefenseCharacter target in _targets)
		{
			if (GodotObject.IsInstanceValid(target) && !target.die && !target.isDestroy && target.instance.maskFlags != 0)
			{
				attackComponent.target = target;
				attackComponent.AttackDpsExecute(delta, 200.0);
			}
		}
	}

	public void EndSoul()
	{
		if (!die && !isDestroy && IsInsideTree())
		{
			_ending = true;
			IStateMachineController stateMachine = base.StateMachine;
			if (stateMachine != null && stateMachine.IsInitialized)
			{
				Die();
			}
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= OnTimerTimeout;
		}
		base._ExitTree();
	}

	public override void DieEntered()
	{
		base.DieEntered();
		if (!base.PreserveCurrentPoseForAsh)
		{
			base.sprite.pause = false;
			base.sprite.timeScale = 1.0;
			base.sprite.SetAnimation("Death", loop: false);
		}
	}

	public override void DieProcessing(double delta)
	{
		base.sprite.timeScale = 1.0;
	}

	public override Dictionary ExportNetworkSpawnState()
	{
		Dictionary dictionary = new Dictionary
		{
			["soulOwnerKey"] = _ownerKey,
			["soulAngle"] = _angle,
			["soulShadow"] = isShadow
		};
		if (_hasOrbitAnchor && !GodotObject.IsInstanceValid(_owner))
		{
			dictionary["soulHasOrbitAnchor"] = true;
			dictionary["soulOrbitAnchor"] = _orbitAnchor;
		}
		return dictionary;
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportNetworkSpawnState();
	}

	public void ImportNetworkSpawnState(Dictionary data)
	{
		ImportNetworkSpecialState(data);
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		_ownerKey = data.GetValueOrDefault("soulOwnerKey", "").AsString();
		_angle = data.GetValueOrDefault("soulAngle", Math.PI).AsDouble();
		_hasOrbitAnchor = data.GetValueOrDefault("soulHasOrbitAnchor", false).AsBool();
		if (_hasOrbitAnchor)
		{
			_orbitAnchor = data.GetValueOrDefault("soulOrbitAnchor", Vector2.Zero).AsVector2();
		}
		if (data.ContainsKey("soulShadow"))
		{
			isShadow = data["soulShadow"].AsBool();
		}
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary.Merge(ExportNetworkSpawnState(), overwrite: true);
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		ImportNetworkSpecialState(data);
		ApplyShadowVisual();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(26)
		{
			new MethodInfo(MethodName.BindOwner, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "owner", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.SetOrbitAngle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "angle", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetupOwnerlessOrbit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RebalanceOwnerlessOrbit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyShadowVisual, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BatchUpdate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryRelocateNaturalSpawnToRandomCell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PickNaturalSpawnColumn, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "columnCount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "line", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.EntryPositionSeed, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "line", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PickRandomEnabledLine, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "mapFeature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Int, "gridNumY", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.WalkProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OrbitProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.EndSoul, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DieEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DieProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportNetworkSpawnState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportNetworkSpecialState, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportNetworkSpawnState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ImportNetworkSpecialState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.BindOwner && args.Count == 1)
		{
			BindOwner(VariantUtils.ConvertTo<TowerDefenseZombieEvoker>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetOrbitAngle && args.Count == 1)
		{
			SetOrbitAngle(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetupOwnerlessOrbit && args.Count == 0)
		{
			SetupOwnerlessOrbit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RebalanceOwnerlessOrbit && args.Count == 0)
		{
			RebalanceOwnerlessOrbit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyShadowVisual && args.Count == 0)
		{
			ApplyShadowVisual();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BatchUpdate && args.Count == 1)
		{
			BatchUpdate(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 1)
		{
			OnTimerTimeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WalkEntered && args.Count == 0)
		{
			WalkEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryRelocateNaturalSpawnToRandomCell && args.Count == 0)
		{
			TryRelocateNaturalSpawnToRandomCell();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PickNaturalSpawnColumn && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<int>(PickNaturalSpawnColumn(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.EntryPositionSeed && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ulong>(EntryPositionSeed(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.PickRandomEnabledLine && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<int>(PickRandomEnabledLine(VariantUtils.ConvertTo<TowerDefenseBattleFeatureMap>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.WalkProcessing && args.Count == 1)
		{
			WalkProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OrbitProcessing && args.Count == 1)
		{
			OrbitProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSoul && args.Count == 0)
		{
			EndSoul();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DieEntered && args.Count == 0)
		{
			DieEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DieProcessing && args.Count == 1)
		{
			DieProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportNetworkSpawnState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpawnState());
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportNetworkSpecialState());
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.PickRandomEnabledLine && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<int>(PickRandomEnabledLine(VariantUtils.ConvertTo<TowerDefenseBattleFeatureMap>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.BindOwner)
		{
			return true;
		}
		if (method == MethodName.SetOrbitAngle)
		{
			return true;
		}
		if (method == MethodName.SetupOwnerlessOrbit)
		{
			return true;
		}
		if (method == MethodName.RebalanceOwnerlessOrbit)
		{
			return true;
		}
		if (method == MethodName.ApplyShadowVisual)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.BatchUpdate)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
		{
			return true;
		}
		if (method == MethodName.WalkEntered)
		{
			return true;
		}
		if (method == MethodName.TryRelocateNaturalSpawnToRandomCell)
		{
			return true;
		}
		if (method == MethodName.PickNaturalSpawnColumn)
		{
			return true;
		}
		if (method == MethodName.EntryPositionSeed)
		{
			return true;
		}
		if (method == MethodName.PickRandomEnabledLine)
		{
			return true;
		}
		if (method == MethodName.WalkProcessing)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.OrbitProcessing)
		{
			return true;
		}
		if (method == MethodName.EndSoul)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.DieEntered)
		{
			return true;
		}
		if (method == MethodName.DieProcessing)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpawnState)
		{
			return true;
		}
		if (method == MethodName.ExportNetworkSpecialState)
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
		if (method == MethodName.ExportVariantSave)
		{
			return true;
		}
		if (method == MethodName.ImportVariantSave)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._owner)
		{
			_owner = VariantUtils.ConvertTo<TowerDefenseZombieEvoker>(in value);
			return true;
		}
		if (name == PropertyName._ownerKey)
		{
			_ownerKey = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._angle)
		{
			_angle = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._ending)
		{
			_ending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._orbitAnchor)
		{
			_orbitAnchor = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hasOrbitAnchor)
		{
			_hasOrbitAnchor = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.isShadow)
		{
			isShadow = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._shadowVisualApplied)
		{
			_shadowVisualApplied = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._naturalSpawnLine)
		{
			_naturalSpawnLine = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._naturalSpawnEntryX)
		{
			_naturalSpawnEntryX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.HasSummonOwner)
		{
			from = HasSummonOwner;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CanBeTransformed)
		{
			from = CanBeTransformed;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsHardControlImmune)
		{
			from = IsHardControlImmune;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.OrbitAngle)
		{
			value = VariantUtils.CreateFrom<double>(OrbitAngle);
			return true;
		}
		if (name == PropertyName._owner)
		{
			value = VariantUtils.CreateFrom(in _owner);
			return true;
		}
		if (name == PropertyName._ownerKey)
		{
			value = VariantUtils.CreateFrom(in _ownerKey);
			return true;
		}
		if (name == PropertyName._angle)
		{
			value = VariantUtils.CreateFrom(in _angle);
			return true;
		}
		if (name == PropertyName._ending)
		{
			value = VariantUtils.CreateFrom(in _ending);
			return true;
		}
		if (name == PropertyName._orbitAnchor)
		{
			value = VariantUtils.CreateFrom(in _orbitAnchor);
			return true;
		}
		if (name == PropertyName._hasOrbitAnchor)
		{
			value = VariantUtils.CreateFrom(in _hasOrbitAnchor);
			return true;
		}
		if (name == PropertyName.isShadow)
		{
			value = VariantUtils.CreateFrom(in isShadow);
			return true;
		}
		if (name == PropertyName._shadowVisualApplied)
		{
			value = VariantUtils.CreateFrom(in _shadowVisualApplied);
			return true;
		}
		if (name == PropertyName._naturalSpawnLine)
		{
			value = VariantUtils.CreateFrom(in _naturalSpawnLine);
			return true;
		}
		if (name == PropertyName._naturalSpawnEntryX)
		{
			value = VariantUtils.CreateFrom(in _naturalSpawnEntryX);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._owner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._ownerKey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._angle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._ending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._orbitAnchor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._hasOrbitAnchor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.isShadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._shadowVisualApplied, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._naturalSpawnLine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._naturalSpawnEntryX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasSummonOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanBeTransformed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsHardControlImmune, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.OrbitAngle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._owner, Variant.From(in _owner));
		info.AddProperty(PropertyName._ownerKey, Variant.From(in _ownerKey));
		info.AddProperty(PropertyName._angle, Variant.From(in _angle));
		info.AddProperty(PropertyName._ending, Variant.From(in _ending));
		info.AddProperty(PropertyName._orbitAnchor, Variant.From(in _orbitAnchor));
		info.AddProperty(PropertyName._hasOrbitAnchor, Variant.From(in _hasOrbitAnchor));
		info.AddProperty(PropertyName.isShadow, Variant.From(in isShadow));
		info.AddProperty(PropertyName._shadowVisualApplied, Variant.From(in _shadowVisualApplied));
		info.AddProperty(PropertyName._naturalSpawnLine, Variant.From(in _naturalSpawnLine));
		info.AddProperty(PropertyName._naturalSpawnEntryX, Variant.From(in _naturalSpawnEntryX));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._owner, out var value))
		{
			_owner = value.As<TowerDefenseZombieEvoker>();
		}
		if (info.TryGetProperty(PropertyName._ownerKey, out var value2))
		{
			_ownerKey = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._angle, out var value3))
		{
			_angle = value3.As<double>();
		}
		if (info.TryGetProperty(PropertyName._ending, out var value4))
		{
			_ending = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._orbitAnchor, out var value5))
		{
			_orbitAnchor = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hasOrbitAnchor, out var value6))
		{
			_hasOrbitAnchor = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.isShadow, out var value7))
		{
			isShadow = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._shadowVisualApplied, out var value8))
		{
			_shadowVisualApplied = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._naturalSpawnLine, out var value9))
		{
			_naturalSpawnLine = value9.As<int>();
		}
		if (info.TryGetProperty(PropertyName._naturalSpawnEntryX, out var value10))
		{
			_naturalSpawnEntryX = value10.As<float>();
		}
	}
}
