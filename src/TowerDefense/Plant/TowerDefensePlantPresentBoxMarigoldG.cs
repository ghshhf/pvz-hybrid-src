using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Colour/PresentBoxMarigoldG/Scene/TowerDefensePlantPresentBoxMarigoldG.cs")]
public class TowerDefensePlantPresentBoxMarigoldG : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ConnectRoleStateSignals = "ConnectRoleStateSignals";

		public static readonly StringName DisconnectRoleStateSignals = "DisconnectRoleStateSignals";

		public new static readonly StringName IdleEntered = "IdleEntered";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public static readonly StringName RestoreRoleState = "RestoreRoleState";

		public static readonly StringName AttractEntered = "AttractEntered";

		public static readonly StringName AttractExited = "AttractExited";

		public static readonly StringName AttractProcessing = "AttractProcessing";

		public static readonly StringName OpenEntered = "OpenEntered";

		public new static readonly StringName AnimeEvent = "AnimeEvent";

		public static readonly StringName CoinGet = "CoinGet";

		public new static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName OpenBox = "OpenBox";

		public static readonly StringName CanReplace = "CanReplace";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";

		public new static readonly StringName ExportNetworkSpecialState = "ExportNetworkSpecialState";

		public new static readonly StringName ImportNetworkSpecialState = "ImportNetworkSpecialState";

		public new static readonly StringName GetNetworkSpecialStateRevision = "GetNetworkSpecialStateRevision";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
		public static readonly StringName _roleStateSignalsConnected = "_roleStateSignalsConnected";

		public static readonly StringName _coinCount = "_coinCount";

		public static readonly StringName _attracting = "_attracting";

		public static readonly StringName _opening = "_opening";

		public static readonly StringName _opened = "_opened";
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	private MagnetCoinComponent _magnetCoinComponent;

	private StateHandle _attractState;

	private StateHandle _openState;

	private bool _roleStateSignalsConnected;

	private int _coinCount;

	private bool _attracting;

	private bool _opening;

	private bool _opened;

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			_magnetCoinComponent = componentManager.GetRuntime<MagnetCoinComponent>();
			MagnetCoinComponent magnetCoinComponent = _magnetCoinComponent;
			if (magnetCoinComponent != null && !magnetCoinComponent.IsReleased)
			{
				_magnetCoinComponent.OnCoinGet += CoinGet;
			}
			AddToGroup("GoldMagnet");
			_attractState = base.StateMachine?.GetStateById("plant.present_box_marigold_g.attract");
			_openState = base.StateMachine?.GetStateById("plant.present_box_marigold_g.open");
			ConnectRoleStateSignals();
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (IsNodeReady() && !Engine.IsEditorHint())
		{
			MagnetCoinComponent magnetCoinComponent = _magnetCoinComponent;
			if (magnetCoinComponent != null && !magnetCoinComponent.IsReleased)
			{
				_magnetCoinComponent.OnCoinGet += CoinGet;
			}
			_attractState = base.StateMachine?.GetStateById("plant.present_box_marigold_g.attract");
			_openState = base.StateMachine?.GetStateById("plant.present_box_marigold_g.open");
			ConnectRoleStateSignals();
		}
	}

	public override void _ExitTree()
	{
		MagnetCoinComponent magnetCoinComponent = _magnetCoinComponent;
		if (magnetCoinComponent != null && !magnetCoinComponent.IsReleased)
		{
			_magnetCoinComponent.OnCoinGet -= CoinGet;
		}
		DisconnectRoleStateSignals();
		base._ExitTree();
	}

	private void ConnectRoleStateSignals()
	{
		if (_roleStateSignalsConnected)
		{
			return;
		}
		StateHandle attractState = _attractState;
		if (attractState != null && attractState.IsValid)
		{
			StateHandle openState = _openState;
			if (openState != null && openState.IsValid)
			{
				_attractState.Entered += AttractEntered;
				_attractState.Exited += AttractExited;
				_attractState.PhysicsProcessing += AttractProcessing;
				_openState.Entered += OpenEntered;
				_roleStateSignalsConnected = true;
			}
		}
	}

	private void DisconnectRoleStateSignals()
	{
		if (_roleStateSignalsConnected)
		{
			_attractState.Entered -= AttractEntered;
			_attractState.Exited -= AttractExited;
			_attractState.PhysicsProcessing -= AttractProcessing;
			_openState.Entered -= OpenEntered;
			_attractState = null;
			_openState = null;
			_roleStateSignalsConnected = false;
		}
	}

	public override void IdleEntered()
	{
		base.IdleEntered();
		RestoreRoleState();
	}

	public override void IdleProcessing(double delta)
	{
		base.IdleProcessing(delta);
		if (!TowerDefenseManager.HasGameplayAuthority || !CanRunAbility() || _opened)
		{
			return;
		}
		if (_coinCount >= 8)
		{
			if (!_opening)
			{
				SendStateEvent("ToOpen");
			}
		}
		else if (!_attracting)
		{
			MagnetCoinComponent magnetCoinComponent = _magnetCoinComponent;
			if (magnetCoinComponent != null && !magnetCoinComponent.IsReleased && _magnetCoinComponent.coinList.Count == 0 && _magnetCoinComponent.CanCoinDraw())
			{
				SendStateEvent("ToAttract");
			}
		}
	}

	private void RestoreRoleState()
	{
		if (_opened)
		{
			return;
		}
		if (_opening)
		{
			if (!SendStateEvent("ToOpen"))
			{
				base.sprite.SetAnimation("Open", loop: false);
			}
		}
		else if (_attracting && !SendStateEvent("ToAttract"))
		{
			base.sprite.SetAnimation("Attract", loop: false);
		}
	}

	public void AttractEntered()
	{
		_attracting = true;
		base.sprite.SetAnimation("Attract", loop: false);
	}

	public void AttractExited()
	{
		_attracting = false;
	}

	public void AttractProcessing(double delta)
	{
		if (TowerDefenseManager.HasGameplayAuthority && CanRunAbility() && !_opened && _coinCount >= 8 && !_opening)
		{
			SendStateEvent("ToOpen");
		}
	}

	public void OpenEntered()
	{
		_opening = true;
		_attracting = false;
		base.sprite.SetAnimation("Open", loop: false);
	}

	public override void AnimeEvent(string command, Variant argument)
	{
		base.AnimeEvent(command, argument);
		if (command == "action" && TowerDefenseManager.HasGameplayAuthority && _attracting && !_opening)
		{
			_magnetCoinComponent.magnetNum = Mathf.Max(0, 8 - _coinCount - _magnetCoinComponent.coinList.Count);
			_magnetCoinComponent.CoinDraw();
		}
	}

	private void CoinGet(TowerDefenseCoinBase coin)
	{
		_coinCount = Mathf.Min(8, _coinCount + 1);
	}

	public override void AnimeCompleted(string clip)
	{
		base.AnimeCompleted(clip);
		if (!(clip == "Attract"))
		{
			if (clip == "Open")
			{
				OpenBox();
			}
		}
		else if (!_opening)
		{
			Idle();
		}
	}

	private void OpenBox()
	{
		if (!TowerDefenseManager.HasGameplayAuthority || _opened || !CanRunAbility() || !GodotObject.IsInstanceValid(cell))
		{
			return;
		}
		Array category = TowerDefenseManager.GetPacketBankData("GeneralPlant").GetCategory("Colour");
		category.Remove("PlantPresentBoxMarigoldG");
		cell.RemoveCharacter(this);
		for (int num = category.Count - 1; num >= 0; num--)
		{
			TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(category[num].AsString());
			if (!CanReplace(packetConfig) || packetConfig.characterConfig?.name == base.config.name)
			{
				category.RemoveAt(num);
			}
		}
		TowerDefensePacketConfig packetConfig2 = TowerDefenseManager.GetPacketConfig("PlantMarigoldG");
		bool num2 = GD.Randf() < 0.08f && CanReplace(packetConfig2);
		string packetName = "";
		if (num2)
		{
			packetName = "PlantMarigoldG";
		}
		else if (category.Count > 0)
		{
			packetName = category.PickRandom().AsString();
		}
		TowerDefensePacketConfig packetConfig3 = TowerDefenseManager.GetPacketConfig(packetName);
		if (CanReplace(packetConfig3))
		{
			TowerDefenseCharacter towerDefenseCharacter = (base.HasEconomyOwner ? packetConfig3.Plant(base.EconomyOwnerAccountId, base.gridPos) : packetConfig3.Plant(base.gridPos));
			if (GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				_opened = true;
				towerDefenseCharacter.CallDeferred("WakeUp");
				if (!base.UsesSandboxTeams && instance.hypnoses)
				{
					towerDefenseCharacter.Hypnoses();
				}
				InheritSandboxTeam(towerDefenseCharacter);
				TowerDefenseManager.PublishSpawnedCharacter(packetName, towerDefenseCharacter, useCreate: false);
				Destroy();
				return;
			}
		}
		cell.CharacterPlant(packet, this, noLimit: true);
		_opening = false;
	}

	private bool CanReplace(TowerDefensePacketConfig candidate)
	{
		if (GodotObject.IsInstanceValid(candidate) && cell.CanPacketPlant(candidate))
		{
			if (instance.hypnoses)
			{
				return (candidate.characterConfig.unUseBuffFlags & 8) == 0;
			}
			return true;
		}
		return false;
	}

	public override Dictionary ExportVariantSave()
	{
		return new Dictionary
		{
			["coinCount"] = _coinCount,
			["attracting"] = _attracting,
			["opening"] = _opening,
			["opened"] = _opened
		};
	}

	public override void ImportVariantSave(Dictionary data)
	{
		_coinCount = Mathf.Clamp(data.GetValueOrDefault("coinCount", 0).AsInt32(), 0, 8);
		_attracting = data.GetValueOrDefault("attracting", false).AsBool();
		_opening = data.GetValueOrDefault("opening", false).AsBool();
		_opened = data.GetValueOrDefault("opened", false).AsBool();
	}

	public override Dictionary ExportNetworkSpecialState()
	{
		return ExportVariantSave();
	}

	public override void ImportNetworkSpecialState(Dictionary data)
	{
		ImportVariantSave(data);
	}

	public override int GetNetworkSpecialStateRevision()
	{
		return _coinCount * 8 + (_attracting ? 1 : 0) + (_opening ? 2 : 0) + (_opened ? 4 : 0);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(22)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConnectRoleStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DisconnectRoleStateSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RestoreRoleState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttractEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttractExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AttractProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OpenEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "command", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "argument", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.CoinGet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "coin", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OpenBox, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanReplace, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "candidate", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectRoleStateSignals && args.Count == 0)
		{
			ConnectRoleStateSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectRoleStateSignals && args.Count == 0)
		{
			DisconnectRoleStateSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleEntered && args.Count == 0)
		{
			IdleEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestoreRoleState && args.Count == 0)
		{
			RestoreRoleState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttractEntered && args.Count == 0)
		{
			AttractEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttractExited && args.Count == 0)
		{
			AttractExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttractProcessing && args.Count == 1)
		{
			AttractProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenEntered && args.Count == 0)
		{
			OpenEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeEvent && args.Count == 2)
		{
			AnimeEvent(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CoinGet && args.Count == 1)
		{
			CoinGet(VariantUtils.ConvertTo<TowerDefenseCoinBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenBox && args.Count == 0)
		{
			OpenBox();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanReplace && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(CanReplace(VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[0])));
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
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ConnectRoleStateSignals)
		{
			return true;
		}
		if (method == MethodName.DisconnectRoleStateSignals)
		{
			return true;
		}
		if (method == MethodName.IdleEntered)
		{
			return true;
		}
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.RestoreRoleState)
		{
			return true;
		}
		if (method == MethodName.AttractEntered)
		{
			return true;
		}
		if (method == MethodName.AttractExited)
		{
			return true;
		}
		if (method == MethodName.AttractProcessing)
		{
			return true;
		}
		if (method == MethodName.OpenEntered)
		{
			return true;
		}
		if (method == MethodName.AnimeEvent)
		{
			return true;
		}
		if (method == MethodName.CoinGet)
		{
			return true;
		}
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.OpenBox)
		{
			return true;
		}
		if (method == MethodName.CanReplace)
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
		if (name == PropertyName._roleStateSignalsConnected)
		{
			_roleStateSignalsConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._coinCount)
		{
			_coinCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._attracting)
		{
			_attracting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._opening)
		{
			_opening = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._opened)
		{
			_opened = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._roleStateSignalsConnected)
		{
			value = VariantUtils.CreateFrom(in _roleStateSignalsConnected);
			return true;
		}
		if (name == PropertyName._coinCount)
		{
			value = VariantUtils.CreateFrom(in _coinCount);
			return true;
		}
		if (name == PropertyName._attracting)
		{
			value = VariantUtils.CreateFrom(in _attracting);
			return true;
		}
		if (name == PropertyName._opening)
		{
			value = VariantUtils.CreateFrom(in _opening);
			return true;
		}
		if (name == PropertyName._opened)
		{
			value = VariantUtils.CreateFrom(in _opened);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName._roleStateSignalsConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._coinCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._attracting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._opening, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._opened, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._roleStateSignalsConnected, Variant.From(in _roleStateSignalsConnected));
		info.AddProperty(PropertyName._coinCount, Variant.From(in _coinCount));
		info.AddProperty(PropertyName._attracting, Variant.From(in _attracting));
		info.AddProperty(PropertyName._opening, Variant.From(in _opening));
		info.AddProperty(PropertyName._opened, Variant.From(in _opened));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._roleStateSignalsConnected, out var value))
		{
			_roleStateSignalsConnected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._coinCount, out var value2))
		{
			_coinCount = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName._attracting, out var value3))
		{
			_attracting = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._opening, out var value4))
		{
			_opening = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._opened, out var value5))
		{
			_opened = value5.As<bool>();
		}
	}
}
