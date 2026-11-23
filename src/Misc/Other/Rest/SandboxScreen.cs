using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[ScriptPath("res://Scene/Sandbox/SandboxScreen.cs")]
public class SandboxScreen : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName LoadCatalog = "LoadCatalog";

		public static readonly StringName FilterCatalog = "FilterCatalog";

		public static readonly StringName OnCatalogSelected = "OnCatalogSelected";

		public static readonly StringName UpdateSelectedLabel = "UpdateSelectedLabel";

		public static readonly StringName OnBoardInput = "OnBoardInput";

		public static readonly StringName SelectCharacter = "SelectCharacter";

		public static readonly StringName RefreshState = "RefreshState";

		public static readonly StringName OpenFileDialog = "OpenFileDialog";

		public static readonly StringName OnFileSelected = "OnFileSelected";

		public static readonly StringName ExitSandbox = "ExitSandbox";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName BuildLayout = "BuildLayout";

		public static readonly StringName BuildCatalog = "BuildCatalog";

		public static readonly StringName BuildStage = "BuildStage";

		public static readonly StringName BuildInspector = "BuildInspector";

		public static readonly StringName AddPanel = "AddPanel";

		public static readonly StringName BuildTheme = "BuildTheme";

		public static readonly StringName AddPanelTitle = "AddPanelTitle";

		public static readonly StringName TextureStyle = "TextureStyle";

		public static readonly StringName FlatStyle = "FlatStyle";

		public static readonly StringName ApplyPrimary = "ApplyPrimary";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName InterfaceFont = "InterfaceFont";

		public static readonly StringName Session = "Session";

		public static readonly StringName Board = "Board";

		public static readonly StringName _battleViewport = "_battleViewport";

		public static readonly StringName _search = "_search";

		public static readonly StringName _category = "_category";

		public static readonly StringName _catalog = "_catalog";

		public static readonly StringName _selectedPacket = "_selectedPacket";

		public static readonly StringName _team = "_team";

		public static readonly StringName _facing = "_facing";

		public static readonly StringName _teamButtons = "_teamButtons";

		public static readonly StringName _categoryButtons = "_categoryButtons";

		public static readonly StringName _catalogCount = "_catalogCount";

		public static readonly StringName _maps = "_maps";

		public static readonly StringName _tool = "_tool";

		public static readonly StringName _stacking = "_stacking";

		public static readonly StringName _status = "_status";

		public static readonly StringName _state = "_state";

		public static readonly StringName _selectedLabel = "_selectedLabel";

		public static readonly StringName _inspector = "_inspector";

		public static readonly StringName _inspectorTabs = "_inspectorTabs";

		public static readonly StringName _emptyInspector = "_emptyInspector";

		public static readonly StringName _editTeam = "_editTeam";

		public static readonly StringName _editFacing = "_editFacing";

		public static readonly StringName _health = "_health";

		public static readonly StringName _actionSpeed = "_actionSpeed";

		public static readonly StringName _overlapSelection = "_overlapSelection";

		public static readonly StringName _automaticFire = "_automaticFire";

		public static readonly StringName _aimButton = "_aimButton";

		public static readonly StringName _files = "_files";

		public static readonly StringName _refreshTime = "_refreshTime";

		public static readonly StringName _leaving = "_leaving";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private SubViewport _battleViewport;

	private LineEdit _search;

	private int _category;

	private SandboxCardCatalog _catalog;

	private readonly List<string> _visiblePackets = new List<string>();

	private readonly List<(string Key, TowerDefensePacketConfig Packet)> _packets = new List<(string, TowerDefensePacketConfig)>();

	private string _selectedPacket = "PlantPeaShooter";

	private int _team;

	private int _facing = 1;

	private Button[] _teamButtons;

	private Button[] _categoryButtons;

	private Label _catalogCount;

	private OptionButton _maps;

	private readonly List<string> _mapKeys = new List<string>();

	private OptionButton _tool;

	private Button _stacking;

	private Label _status;

	private Label _state;

	private Label _selectedLabel;

	private Label _inspector;

	private TabContainer _inspectorTabs;

	private Label _emptyInspector;

	private OptionButton _editTeam;

	private OptionButton _editFacing;

	private SpinBox _health;

	private SpinBox _actionSpeed;

	private OptionButton _overlapSelection;

	private readonly List<TowerDefenseCharacter> _pickCandidates = new List<TowerDefenseCharacter>();

	private CheckButton _automaticFire;

	private Button _aimButton;

	private readonly List<Button> _actionButtons = new List<Button>();

	private FileDialog _files;

	private double _refreshTime;

	private bool _leaving;

	[Export(PropertyHint.None, "")]
	public Font InterfaceFont { get; set; }

	public SandboxSession Session { get; private set; }

	public SandboxBoard Board { get; private set; }

	private void LoadCatalog()
	{
		_packets.Clear();
		foreach (KeyValuePair<string, Resource> tOWERDEFENSE_PACKET in ResourceManager.Instance.TOWERDEFENSE_PACKETS)
		{
			if (tOWERDEFENSE_PACKET.Value is TowerDefensePacketConfig towerDefensePacketConfig && SandboxPlacement.IsSupportedPacket(towerDefensePacketConfig))
			{
				_packets.Add((tOWERDEFENSE_PACKET.Key, towerDefensePacketConfig));
			}
		}
		List<string> orderedKeys = (from key in TowerDefenseManager.GetPacketBankData("GeneralPlant").GetPacketList()
			select key.AsString()).Concat(from key in TowerDefenseManager.GetPacketBankData("GeneralZombie").GetPacketList()
			select key.AsString()).ToList();
		_packets.Sort(delegate((string Key, TowerDefensePacketConfig Packet) left, (string Key, TowerDefensePacketConfig Packet) right)
		{
			int num = orderedKeys.IndexOf(left.Key);
			int num2 = orderedKeys.IndexOf(right.Key);
			return ((num < 0) ? 2147483647 : num).CompareTo((num2 < 0) ? 2147483647 : num2);
		});
		_maps.Clear();
		_mapKeys.Clear();
		foreach (KeyValuePair<string, Resource> mAP in ResourceManager.Instance.MAPS)
		{
			if (mAP.Value is TowerDefenseMapConfig towerDefenseMapConfig)
			{
				_mapKeys.Add(mAP.Key);
				string value = (string.IsNullOrWhiteSpace(towerDefenseMapConfig.translate) ? mAP.Key : Tr(towerDefenseMapConfig.translate));
				_maps.AddItem($"{value} {towerDefenseMapConfig.gridNum.X}×{towerDefenseMapConfig.gridNum.Y}");
			}
		}
		FilterCatalog();
	}

	private void FilterCatalog()
	{
		if (_catalog == null)
		{
			return;
		}
		_visiblePackets.Clear();
		string text = _search.Text.Trim();
		List<(string, TowerDefensePacketConfig)> list = new List<(string, TowerDefensePacketConfig)>();
		foreach (var packet in _packets)
		{
			bool flag = packet.Packet.characterConfig is TowerDefenseZombieConfig;
			string text2 = Tr(packet.Packet.name);
			if (!(_category == 1 && flag) && (_category != 2 || flag) && (text.Length <= 0 || text2.Contains(text, StringComparison.OrdinalIgnoreCase) || packet.Key.Contains(text, StringComparison.OrdinalIgnoreCase)))
			{
				_visiblePackets.Add(packet.Key);
				list.Add(packet);
			}
		}
		_catalog.SetItems(list);
		if (!_visiblePackets.Contains(_selectedPacket) && _visiblePackets.Count > 0)
		{
			_catalog.Select(_visiblePackets[0]);
		}
		else
		{
			UpdateSelectedLabel();
		}
	}

	private void OnCatalogSelected(string key)
	{
		if (_visiblePackets.Contains(key))
		{
			_selectedPacket = key;
			_tool.Select(0);
			UpdateSelectedLabel();
		}
	}

	private void UpdateSelectedLabel()
	{
		if (_selectedLabel != null && _packets.Count != 0)
		{
			TowerDefensePacketConfig packetConfig = TowerDefenseManager.GetPacketConfig(_selectedPacket);
			_selectedLabel.Text = Tr(packetConfig.name);
			_catalogCount.Text = $"显示 {_visiblePackets.Count} / {_packets.Count} 张角色卡";
		}
	}

	private void OnBoardInput(InputEvent inputEvent)
	{
		if (!Session.CanEdit)
		{
			return;
		}
		InputEventMouseButton mouse = inputEvent as InputEventMouseButton;
		if (mouse == null || !mouse.Pressed || (mouse.ButtonIndex != MouseButton.Left && mouse.ButtonIndex != MouseButton.Right))
		{
			return;
		}
		Board.AcceptEvent();
		if (!Board.TryGetWorldPosition(mouse.Position, out var world))
		{
			return;
		}
		Vector2I grid = TowerDefenseManager.Instance.GetMapGridPosFromMouse(world);
		if (mouse.ButtonIndex == MouseButton.Left && _tool.Selected == 3)
		{
			Session.FireAt(Board.Selected, world);
		}
		else if (mouse.ButtonIndex == MouseButton.Right || _tool.Selected != 0)
		{
			TowerDefenseCharacter[] array = (from actor in Session.Characters.Where(SandboxSession.IsAlive)
				where (actor.HasHitBox && actor.WorldHitRect.HasPoint(world)) || (actor.gridPos.Y == grid.Y && Mathf.Abs(actor.GetLogicalGlobalPosition().X - world.X) <= TowerDefenseManager.Instance.gridSize.X * 0.55f) || Board.WorldToBoard(actor.GetLogicalGlobalPosition(actor.transformPoint)).DistanceTo(mouse.Position) <= 22f
				orderby actor.GetInstanceId()
				select actor).ToArray();
			int num = (_pickCandidates.SequenceEqual(array) ? ((_pickCandidates.IndexOf(Board.Selected) + 1) % Math.Max(1, array.Length)) : 0);
			_pickCandidates.Clear();
			_pickCandidates.AddRange(array);
			_overlapSelection.Clear();
			TowerDefenseCharacter[] array2 = array;
			foreach (TowerDefenseCharacter towerDefenseCharacter in array2)
			{
				_overlapSelection.AddItem($"{((towerDefenseCharacter.camp == TowerDefenseEnum.CHARACTER_CAMP.PLANT) ? "红" : "蓝")} · {Tr(towerDefenseCharacter.packet.name)} · #{towerDefenseCharacter.GetInstanceId() % 10000}");
			}
			_overlapSelection.Visible = array.Length > 1;
			TowerDefenseCharacter character = ((array.Length != 0) ? array[num] : null);
			SelectCharacter(character);
			if (mouse.ButtonIndex == MouseButton.Left && _tool.Selected == 2)
			{
				Session.Delete(character);
			}
		}
		else
		{
			SandboxPlacement placement = new SandboxPlacement
			{
				Packet = _selectedPacket,
				Column = grid.X,
				Row = grid.Y,
				Team = _team,
				Facing = _facing
			};
			RunOperation(async delegate
			{
				TowerDefenseCharacter character2 = await Session.PlaceAsync(placement);
				SelectCharacter(character2);
			});
		}
	}

	private void SelectCharacter(TowerDefenseCharacter character)
	{
		Board.Selected = character;
		if (SandboxSession.IsAlive(character))
		{
			SandboxPlacement sandboxPlacement = Session.ReadCharacter(character);
			_editTeam.Select(sandboxPlacement.Team);
			_editFacing.Select((sandboxPlacement.Facing != 1) ? 1 : 0);
			_health.Value = sandboxPlacement.HealthScale;
			_actionSpeed.Value = sandboxPlacement.ActionSpeed;
			_automaticFire.SetPressedNoSignal(sandboxPlacement.AutoAttack);
			int num = _pickCandidates.IndexOf(character);
			if (num >= 0)
			{
				_overlapSelection.Select(num);
			}
		}
		RefreshState();
	}

	private void RefreshState()
	{
		if (_state == null || _status == null || !IsInsideTree())
		{
			return;
		}
		_status.Text = Session.Status;
		_status.TooltipText = Session.Status;
		foreach (Button actionButton in _actionButtons)
		{
			actionButton.Disabled = !Session.CanEdit;
		}
		_maps.Disabled = !Session.CanEdit;
		_stacking.Disabled = !Session.CanEdit;
		_stacking.SetPressedNoSignal(Session.AllowStacking);
		_stacking.Text = (Session.AllowStacking ? "允许叠放：开" : "允许叠放：关");
		int num = _mapKeys.IndexOf(Session.MapName);
		if (num >= 0)
		{
			_maps.Select(num);
		}
		TowerDefenseBattleProcessSandbox process = Session.Process;
		if (process == null || !process.IsReady)
		{
			_state.Text = "正在加载战场";
			return;
		}
		int num2 = 0;
		int num3 = 0;
		foreach (TowerDefenseCharacter character in Session.Characters)
		{
			if (SandboxSession.IsAlive(character))
			{
				if (character.camp == TowerDefenseEnum.CHARACTER_CAMP.PLANT)
				{
					num2++;
				}
				else
				{
					num3++;
				}
			}
		}
		string value = (Session.IsBusy ? "正在重建" : (Session.IsPreparing ? "布阵中" : (GetTree().Paused ? "已暂停" : "交战中")));
		_state.Text = $"{value}  ·  红 {num2}  /  蓝 {num3}  ·  {Session.Process.ElapsedSeconds:F1}s";
		if (SandboxSession.IsAlive(Board.Selected))
		{
			_inspectorTabs.Visible = true;
			_emptyInspector.Visible = false;
			TowerDefenseCharacter selected = Board.Selected;
			CannonComponent cannonComponent = selected.componentManager?.GetRuntime<CannonComponent>();
			bool visible = cannonComponent != null && !cannonComponent.IsReleased;
			_automaticFire.Visible = visible;
			_aimButton.Visible = visible;
			_automaticFire.Disabled = !Session.CanEdit;
			_aimButton.Disabled = !Session.CanEdit;
			_inspector.Text = $"{Tr(selected.packet.name)}\n{((selected.camp == TowerDefenseEnum.CHARACTER_CAMP.PLANT) ? "红队" : "蓝队")} · 第 {selected.gridPos.Y} 行\n生命 {selected.GetCurrentHitPoint():F0} / {selected.GetTotalHitPoint():F0}";
		}
		else
		{
			Board.Selected = null;
			_inspectorTabs.Visible = false;
			_emptyInspector.Visible = true;
			_inspector.Text = "尚未选择角色";
			_overlapSelection.Visible = false;
		}
	}

	private async void RunOperation(Func<Task> operation)
	{
		try
		{
			await operation();
		}
		catch (Exception ex)
		{
			if (IsInsideTree())
			{
				Session.SetStatus("操作失败：" + ex.Message);
				GD.PushError(ex.ToString());
			}
		}
	}

	private void OpenFileDialog(bool save)
	{
		DirAccess.MakeDirRecursiveAbsolute("user://Sandbox");
		_files.FileMode = (FileDialog.FileModeEnum)(save ? 4 : 0);
		_files.Title = (save ? "保存开战布阵方案" : "读取沙盒布阵方案");
		_files.CurrentDir = ProjectSettings.GlobalizePath("user://Sandbox");
		_files.CurrentFile = (save ? "沙盒布阵.json" : "");
		_files.PopupCentered(new Vector2I(800, 480));
	}

	private void OnFileSelected(string path)
	{
		if (_files.FileMode == FileDialog.FileModeEnum.SaveFile)
		{
			Session.Save(path);
			return;
		}
		RunOperation(() => Session.LoadAsync(path));
	}

	private void ExitSandbox()
	{
		if (!_leaving)
		{
			_leaving = true;
			Global.Instance.enterLevelMode = "";
			SceneManager.Instance.ChangeScene("LevelEditorStage");
		}
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(Session))
		{
			Session.Changed -= RefreshState;
		}
	}

	public override async void _Ready()
	{
		base.ProcessMode = ProcessModeEnum.Always;
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		BuildLayout();
		try
		{
			if (Global.IsMultiplayerMode)
			{
				Session.SetStatus("沙盒为单机模式，请先退出联机房间。");
				return;
			}
			await Session.InitializeAsync(_battleViewport);
			if (IsInsideTree())
			{
				LoadCatalog();
				RefreshState();
			}
		}
		catch (Exception ex)
		{
			Session.SetStatus("沙盒初始化失败：" + ex.Message);
			GD.PushError(ex.ToString());
		}
	}

	public override void _Process(double delta)
	{
		_refreshTime += delta;
		if (!(_refreshTime < 0.15))
		{
			_refreshTime = 0.0;
			RefreshState();
		}
	}

	private void BuildLayout()
	{
		base.Theme = BuildTheme();
		ColorRect colorRect = new ColorRect
		{
			Color = new Color("0a1813"),
			MouseFilter = MouseFilterEnum.Ignore
		};
		AddChild(colorRect, forceReadableName: false, InternalMode.Disabled);
		colorRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		Session = new SandboxSession
		{
			Name = "Session"
		};
		AddChild(Session, forceReadableName: false, InternalMode.Disabled);
		Session.Changed += RefreshState;
		_battleViewport = new SubViewport
		{
			Name = "BattleViewport",
			Size = new Vector2I(1000, 600),
			OwnWorld3D = true,
			Disable3D = true,
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
			ProcessMode = ProcessModeEnum.Pausable
		};
		AddChild(_battleViewport, forceReadableName: false, InternalMode.Disabled);
		MarginContainer marginContainer = new MarginContainer();
		AddChild(marginContainer, forceReadableName: false, InternalMode.Disabled);
		marginContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			marginContainer.AddThemeConstantOverride("margin_" + text, 8);
		}
		VBoxContainer vBoxContainer = new VBoxContainer();
		marginContainer.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		PanelContainer panelContainer = new PanelContainer
		{
			CustomMinimumSize = new Vector2(0f, 44f)
		};
		panelContainer.AddThemeStyleboxOverride("panel", FlatStyle("06392e", "789971"));
		vBoxContainer.AddChild(panelContainer, forceReadableName: false, InternalMode.Disabled);
		HBoxContainer hBoxContainer = new HBoxContainer();
		panelContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(new TextureRect
		{
			Texture = GD.Load<Texture2D>("res://Asset/Texture/GUI/LevelEditor/IZIcon.png"),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			CustomMinimumSize = new Vector2(38f, 38f),
			MouseFilter = MouseFilterEnum.Ignore
		}, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer vBoxContainer2 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(160f, 0f)
		};
		hBoxContainer.AddChild(vBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		Label label = new Label
		{
			Text = "角色沙盒"
		};
		label.AddThemeFontOverride("font", GD.Load<FontFile>("res://Asset/Font/fzjz.ttf"));
		label.AddThemeFontSizeOverride("font_size", 19);
		label.AddThemeColorOverride("font_color", new Color("ffe29a"));
		vBoxContainer2.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer2.AddChild(new Label
		{
			Text = "红蓝混编 · 自由干预 · 手动结束"
		}, forceReadableName: false, InternalMode.Disabled);
		Button button = new Button
		{
			Text = "斗蛐蛐",
			ToggleMode = true,
			ButtonPressed = true,
			CustomMinimumSize = new Vector2(64f, 24f),
			SizeFlagsVertical = SizeFlags.ShrinkCenter
		};
		ApplyPrimary(button);
		hBoxContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(new Control
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		}, forceReadableName: false, InternalMode.Disabled);
		_state = new Label
		{
			Text = "正在加载",
			HorizontalAlignment = HorizontalAlignment.Right
		};
		hBoxContainer.AddChild(_state, forceReadableName: false, InternalMode.Disabled);
		AddAction(hBoxContainer, "PauseSimulation", "暂停模拟", Session.Pause);
		AddAction(hBoxContainer, "ClearScenario", "清空并重置", delegate
		{
			RunOperation(() => Session.RebuildAsync(new SandboxScenario
			{
				Map = Session.MapName,
				AllowStacking = Session.AllowStacking
			}, run: false));
		});
		Button button2 = new Button
		{
			Name = "ExitSandbox",
			Text = "返回自制关卡",
			SizeFlagsVertical = SizeFlags.ShrinkCenter
		};
		hBoxContainer.AddChild(button2, forceReadableName: false, InternalMode.Disabled);
		button2.Pressed += ExitSandbox;
		HBoxContainer hBoxContainer2 = new HBoxContainer();
		vBoxContainer.AddChild(hBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		AddAction(hBoxContainer2, "StartSimulation", "开始", Session.Start);
		AddAction(hBoxContainer2, "StepSimulation", "单步", Session.Step);
		AddAction(hBoxContainer2, "RestartSimulation", "重开", delegate
		{
			RunOperation(() => Session.RebuildAsync(Session.CopyDraft(), run: true));
		});
		AddAction(hBoxContainer2, "ReturnPreparation", "返回布阵", delegate
		{
			RunOperation(() => Session.RebuildAsync(Session.CopyDraft(), run: false));
		});
		OptionButton optionButton = new OptionButton
		{
			Name = "SimulationSpeed"
		};
		array = new string[5] { "0.25×", "0.5×", "1×", "2×", "4×" };
		foreach (string label2 in array)
		{
			optionButton.AddItem(label2);
		}
		optionButton.Select(2);
		optionButton.ItemSelected += delegate(long index)
		{
			Session.SetSpeed((new double[5] { 0.25, 0.5, 1.0, 2.0, 4.0 })[index]);
		};
		hBoxContainer2.AddChild(new Label
		{
			Text = "速度"
		}, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer2.AddChild(optionButton, forceReadableName: false, InternalMode.Disabled);
		_stacking = new Button
		{
			Name = "AllowStacking",
			Text = "允许叠放：关",
			ToggleMode = true
		};
		_stacking.Toggled += delegate(bool enabled)
		{
			Session.SetStacking(enabled);
			_stacking.SetPressedNoSignal(Session.AllowStacking);
		};
		hBoxContainer2.AddChild(_stacking, forceReadableName: false, InternalMode.Disabled);
		AddAction(hBoxContainer2, "SaveScenario", "保存方案", delegate
		{
			OpenFileDialog(save: true);
		});
		AddAction(hBoxContainer2, "LoadScenario", "读取方案", delegate
		{
			OpenFileDialog(save: false);
		});
		AddAction(hBoxContainer2, "CaptureScenario", "当前场面设为方案", delegate
		{
			Session.Capture();
		});
		HBoxContainer hBoxContainer3 = new HBoxContainer();
		vBoxContainer.AddChild(hBoxContainer3, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer3.AddChild(new Label
		{
			Text = "沙盒工具"
		}, forceReadableName: false, InternalMode.Disabled);
		AddAction(hBoxContainer3, "ToolPlace", "放置角色", delegate
		{
			_tool.Select(0);
		});
		AddAction(hBoxContainer3, "ToolInspect", "检查角色", delegate
		{
			_tool.Select(1);
		});
		_tool = AddChoice(hBoxContainer3, "EditTool", new string[4] { "单点放置", "检查角色", "删除角色", "炮台瞄准" });
		AddAction(hBoxContainer3, "DeleteSelection", "删除选中", delegate
		{
			Session.Delete(Board.Selected);
		});
		hBoxContainer3.AddChild(new Label
		{
			Text = "  当前角色"
		}, forceReadableName: false, InternalMode.Disabled);
		_selectedLabel = new Label
		{
			Text = "从左侧目录选择一张角色卡牌",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		_selectedLabel.AddThemeColorOverride("font_color", new Color("f4dc9f"));
		hBoxContainer3.AddChild(_selectedLabel, forceReadableName: false, InternalMode.Disabled);
		HBoxContainer hBoxContainer4 = new HBoxContainer
		{
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		vBoxContainer.AddChild(hBoxContainer4, forceReadableName: false, InternalMode.Disabled);
		BuildCatalog(hBoxContainer4);
		BuildStage(hBoxContainer4);
		BuildInspector(hBoxContainer4);
		_status = new Label
		{
			Text = "正在准备……",
			TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
			CustomMinimumSize = new Vector2(0f, 22f)
		};
		vBoxContainer.AddChild(_status, forceReadableName: false, InternalMode.Disabled);
		_files = new FileDialog
		{
			Name = "ScenarioFiles",
			Access = FileDialog.AccessEnum.Filesystem,
			Filters = new string[1] { "*.json ; 沙盒布阵方案" },
			Size = new Vector2I(800, 480)
		};
		AddChild(_files, forceReadableName: false, InternalMode.Disabled);
		_files.FileSelected += OnFileSelected;
	}

	private void BuildCatalog(HBoxContainer main)
	{
		VBoxContainer vBoxContainer = AddPanel(main, 180f, "res://Asset/Texture/TowerDefense/PacketBank/PacketBankPanel.png");
		AddPanelTitle(vBoxContainer, "角色目录");
		vBoxContainer.AddChild(new Label
		{
			Text = "选择卡牌，点击草坪连续放置"
		}, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer.AddChild(new Label
		{
			Text = "放置阵营"
		}, forceReadableName: false, InternalMode.Disabled);
		HBoxContainer hBoxContainer = new HBoxContainer();
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		_teamButtons = new Button[2];
		for (int i = 0; i < 2; i++)
		{
			int team = i;
			Button button = new Button
			{
				Name = ((team == 0) ? "PlacementTeamRed" : "PlacementTeamBlue"),
				Text = ((team == 0) ? "红队 →" : "蓝队 ←"),
				ToggleMode = true,
				ButtonPressed = (team == 0),
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			button.AddThemeFontSizeOverride("font_size", 9);
			button.AddThemeStyleboxOverride("normal", FlatStyle((team == 0) ? "5b2821" : "1f3f63", (team == 0) ? "d3664f" : "64a4e5"));
			button.AddThemeStyleboxOverride("pressed", FlatStyle((team == 0) ? "5b2821" : "1f3f63", "ffdc68"));
			button.Pressed += delegate
			{
				_team = team;
				_facing = ((team == 0) ? 1 : (-1));
				_teamButtons[0].SetPressedNoSignal(team == 0);
				_teamButtons[1].SetPressedNoSignal(team == 1);
			};
			hBoxContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
			_teamButtons[i] = button;
		}
		HBoxContainer hBoxContainer2 = new HBoxContainer();
		vBoxContainer.AddChild(hBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		_categoryButtons = new Button[3];
		for (int num = 0; num < 3; num++)
		{
			int category = num;
			Button button2 = new Button();
			button2.Text = (new string[3] { "全部", "植物", "僵尸" })[num];
			button2.ToggleMode = true;
			button2.ButtonPressed = num == 0;
			button2.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			Button button3 = button2;
			button3.AddThemeStyleboxOverride("normal", FlatStyle("11271c", "324d34"));
			button3.AddThemeStyleboxOverride("pressed", FlatStyle("2f431f", "e6bd5e"));
			button3.Pressed += delegate
			{
				_category = category;
				for (int j = 0; j < _categoryButtons.Length; j++)
				{
					_categoryButtons[j].SetPressedNoSignal(j == category);
				}
				FilterCatalog();
			};
			hBoxContainer2.AddChild(button3, forceReadableName: false, InternalMode.Disabled);
			_categoryButtons[num] = button3;
		}
		_search = new LineEdit
		{
			Name = "CharacterSearch",
			PlaceholderText = "搜索名称或 ID",
			ClearButtonEnabled = false
		};
		_search.TextChanged += delegate
		{
			FilterCatalog();
		};
		vBoxContainer.AddChild(_search, forceReadableName: false, InternalMode.Disabled);
		_catalogCount = new Label
		{
			Text = "正在加载角色卡"
		};
		vBoxContainer.AddChild(_catalogCount, forceReadableName: false, InternalMode.Disabled);
		_catalog = new SandboxCardCatalog
		{
			Name = "CharacterCatalog",
			SizeFlagsVertical = SizeFlags.ExpandFill,
			CustomMinimumSize = new Vector2(160f, 100f)
		};
		_catalog.Selected += OnCatalogSelected;
		vBoxContainer.AddChild(_catalog, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer.AddChild(new Label
		{
			Text = "免费放置 · 无冷却"
		}, forceReadableName: false, InternalMode.Disabled);
	}

	private void BuildStage(HBoxContainer main)
	{
		VBoxContainer vBoxContainer = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		main.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		HBoxContainer hBoxContainer = new HBoxContainer();
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(new Label
		{
			Text = "地图"
		}, forceReadableName: false, InternalMode.Disabled);
		_maps = AddChoice(hBoxContainer, "MapChoice", new string[1] { "正在加载地图" });
		_maps.CustomMinimumSize = new Vector2(130f, 20f);
		_maps.ItemSelected += delegate(long index)
		{
			if (index >= 0 && index < _mapKeys.Count)
			{
				SandboxScenario next = Session.CopyDraft();
				next.Map = _mapKeys[(int)index];
				RunOperation(() => Session.RebuildAsync(next, run: false));
			}
		};
		Board = new SandboxBoard
		{
			Name = "Board",
			Session = Session,
			BattleViewport = _battleViewport,
			Texture = _battleViewport.GetTexture(),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Stop,
			ClipContents = true
		};
		vBoxContainer.AddChild(Board, forceReadableName: false, InternalMode.Disabled);
		Board.GuiInput += OnBoardInput;
		Label label = new Label
		{
			Text = "左键执行工具 · 右键循环选取重叠角色 · 检查器可按名称切换",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		label.AddThemeFontSizeOverride("font_size", 11);
		vBoxContainer.AddChild(label, forceReadableName: false, InternalMode.Disabled);
	}

	private void BuildInspector(HBoxContainer main)
	{
		VBoxContainer vBoxContainer = AddPanel(main, 174f, "res://Asset/Texture/TowerDefense/PacketBank/PacketBankZombiePanel.png");
		AddPanelTitle(vBoxContainer, "角色检查");
		_overlapSelection = new OptionButton
		{
			Name = "OverlapSelection",
			Visible = false,
			FitToLongestItem = false
		};
		_overlapSelection.ItemSelected += delegate(long index)
		{
			if (index >= 0 && index < _pickCandidates.Count)
			{
				SelectCharacter(_pickCandidates[(int)index]);
			}
		};
		vBoxContainer.AddChild(_overlapSelection, forceReadableName: false, InternalMode.Disabled);
		_inspector = new Label
		{
			Text = "尚未选择角色",
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			CustomMinimumSize = new Vector2(132f, 48f)
		};
		vBoxContainer.AddChild(_inspector, forceReadableName: false, InternalMode.Disabled);
		TabContainer tabContainer = (_inspectorTabs = new TabContainer
		{
			Name = "InspectorTabs",
			CustomMinimumSize = new Vector2(150f, 180f),
			Visible = false
		});
		vBoxContainer.AddChild(tabContainer, forceReadableName: false, InternalMode.Disabled);
		_emptyInspector = new Label
		{
			Text = "右键草坪上的角色以检查实例。\n\n选中后可修改属性、状态和技能。",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		vBoxContainer.AddChild(_emptyInspector, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer vBoxContainer2 = new VBoxContainer
		{
			Name = "属性"
		};
		VBoxContainer vBoxContainer3 = new VBoxContainer
		{
			Name = "状态"
		};
		VBoxContainer vBoxContainer4 = new VBoxContainer
		{
			Name = "技能"
		};
		tabContainer.AddChild(vBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		tabContainer.AddChild(vBoxContainer3, forceReadableName: false, InternalMode.Disabled);
		tabContainer.AddChild(vBoxContainer4, forceReadableName: false, InternalMode.Disabled);
		_editTeam = AddChoice(vBoxContainer2, "InstanceTeam", new string[2] { "红队", "蓝队" });
		_editFacing = AddChoice(vBoxContainer2, "InstanceFacing", new string[2] { "朝右", "朝左" });
		vBoxContainer2.AddChild(new Label
		{
			Text = "生命倍率（含防具）"
		}, forceReadableName: false, InternalMode.Disabled);
		_health = new SpinBox
		{
			Name = "InstanceHealthScale",
			MinValue = 0.1,
			MaxValue = 100.0,
			Step = 0.1,
			Value = 1.0
		};
		vBoxContainer2.AddChild(_health, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer4.AddChild(new Label
		{
			Text = "动作 / 技能速度"
		}, forceReadableName: false, InternalMode.Disabled);
		_actionSpeed = new SpinBox
		{
			Name = "InstanceActionSpeed",
			MinValue = 0.1,
			MaxValue = 10.0,
			Step = 0.1,
			Value = 1.0
		};
		vBoxContainer4.AddChild(_actionSpeed, forceReadableName: false, InternalMode.Disabled);
		AddAction(vBoxContainer2, "ApplyInstance", "应用属性", delegate
		{
			Session.Edit(Board.Selected, _editTeam.Selected, (_editFacing.Selected == 0) ? 1 : (-1), _health.Value, _actionSpeed.Value);
		});
		AddAction(vBoxContainer4, "ApplySkillSpeed", "应用速度", delegate
		{
			Session.Edit(Board.Selected, _editTeam.Selected, (_editFacing.Selected == 0) ? 1 : (-1), _health.Value, _actionSpeed.Value);
		});
		_automaticFire = new CheckButton
		{
			Name = "CannonAutomaticFire",
			Text = "自动发射",
			Visible = false
		};
		_automaticFire.Toggled += delegate(bool enabled)
		{
			Session.SetAutomaticFire(Board.Selected, enabled);
		};
		vBoxContainer4.AddChild(_automaticFire, forceReadableName: false, InternalMode.Disabled);
		_aimButton = new Button
		{
			Name = "AimCannon",
			Text = "瞄准发射",
			Visible = false
		};
		_aimButton.Pressed += delegate
		{
			_tool.Select(3);
			Session.SetStatus("左键点击草坪指定炮击位置；右键可重新选择炮台。");
		};
		vBoxContainer4.AddChild(_aimButton, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer3.AddChild(new Label
		{
			Text = "受击测试"
		}, forceReadableName: false, InternalMode.Disabled);
		OptionButton damageType = AddChoice(vBoxContainer3, "DamageType", new string[4] { "普通命中（防具优先）", "穿透本体", "爆炸伤害", "碾压伤害" });
		SpinBox damage = new SpinBox
		{
			Name = "DamageAmount",
			MinValue = 1.0,
			MaxValue = 100000.0,
			Value = 100.0
		};
		vBoxContainer3.AddChild(damage, forceReadableName: false, InternalMode.Disabled);
		AddAction(vBoxContainer3, "DamageSelection", "施加伤害", delegate
		{
			if (Session.CanEdit && SandboxSession.IsAlive(Board.Selected))
			{
				switch (damageType.Selected)
				{
				case 1:
					Board.Selected.FlagHurt(damage.Value, 18);
					break;
				case 2:
					Board.Selected.ExplodeHurt(damage.Value);
					break;
				case 3:
					Board.Selected.SmashHurt(damage.Value);
					break;
				default:
					Board.Selected.Hurt(damage.Value);
					break;
				}
				Session.SetStatus("已通过正式伤害流程执行受击测试。");
			}
		});
		vBoxContainer.AddChild(new Control
		{
			SizeFlagsVertical = SizeFlags.ExpandFill
		}, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer.AddChild(new Label
		{
			Text = "保存的是开战布阵。\n运行中的改动需先点击\n「当前场面设为方案」。",
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			CustomMinimumSize = new Vector2(132f, 52f)
		}, forceReadableName: false, InternalMode.Disabled);
	}

	private static VBoxContainer AddPanel(Control parent, float width, string texturePath)
	{
		PanelContainer panelContainer = new PanelContainer
		{
			CustomMinimumSize = new Vector2(width, 0f)
		};
		StyleBoxTexture styleBoxTexture = TextureStyle(texturePath, 8f);
		styleBoxTexture.SetTextureMarginAll(10f);
		styleBoxTexture.TextureMarginTop = 34f;
		panelContainer.AddThemeStyleboxOverride("panel", styleBoxTexture);
		parent.AddChild(panelContainer, forceReadableName: false, InternalMode.Disabled);
		MarginContainer marginContainer = new MarginContainer();
		panelContainer.AddChild(marginContainer, forceReadableName: false, InternalMode.Disabled);
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			marginContainer.AddThemeConstantOverride("margin_" + text, 6);
		}
		VBoxContainer vBoxContainer = new VBoxContainer();
		marginContainer.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		return vBoxContainer;
	}

	private void AddAction(Control parent, string name, string text, Action action)
	{
		Button button = new Button
		{
			Name = name,
			Text = text,
			CustomMinimumSize = new Vector2(0f, 20f),
			SizeFlagsVertical = SizeFlags.ShrinkCenter
		};
		if (name == "StartSimulation" || name == "ToolPlace")
		{
			ApplyPrimary(button);
		}
		button.Pressed += action;
		parent.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		_actionButtons.Add(button);
	}

	private static OptionButton AddChoice(Control parent, string name, IEnumerable<string> labels)
	{
		OptionButton optionButton = new OptionButton
		{
			Name = name,
			CustomMinimumSize = new Vector2(0f, 20f)
		};
		foreach (string label in labels)
		{
			optionButton.AddItem(label);
		}
		parent.AddChild(optionButton, forceReadableName: false, InternalMode.Disabled);
		return optionButton;
	}

	private Theme BuildTheme()
	{
		Theme theme = new Theme
		{
			DefaultFontSize = 10,
			DefaultFont = InterfaceFont
		};
		Font font = GD.Load<FontFile>("res://Asset/Font/fzjz.ttf");
		theme.SetFont("font", "Button", font);
		theme.SetFont("font", "OptionButton", font);
		string[] array = new string[9] { "Button", "OptionButton", "CheckBox", "Label", "LineEdit", "ItemList", "SpinBox", "TabBar", "TabContainer" };
		foreach (string text in array)
		{
			theme.SetFontSize("font_size", text, 10);
			theme.SetConstant("outline_size", text, 0);
			theme.SetColor("font_color", text, new Color("e4ecd8"));
			theme.SetColor("font_focus_color", text, Colors.White);
			theme.SetColor("font_hover_color", text, Colors.White);
		}
		theme.SetConstant("separation", "VBoxContainer", 4);
		theme.SetConstant("separation", "HBoxContainer", 5);
		theme.SetConstant("minimum_character_width", "LineEdit", 5);
		array = new string[3] { "LineEdit", "ItemList", "PanelContainer" };
		foreach (string text2 in array)
		{
			string[] array2 = new string[7] { "normal", "panel", "hover", "pressed", "focus", "disabled", "read_only" };
			foreach (string text3 in array2)
			{
				StyleBoxFlat styleBoxFlat = new StyleBoxFlat
				{
					BgColor = new Color((text3 == "hover" || text3 == "pressed") ? "243e2b" : "10271d"),
					BorderColor = new Color("526349"),
					ContentMarginLeft = 5f,
					ContentMarginRight = 5f,
					ContentMarginTop = 3f,
					ContentMarginBottom = 3f,
					CornerRadiusTopLeft = 2,
					CornerRadiusTopRight = 2,
					CornerRadiusBottomLeft = 2,
					CornerRadiusBottomRight = 2
				};
				styleBoxFlat.SetBorderWidthAll(1);
				theme.SetStylebox(text3, text2, styleBoxFlat);
			}
		}
		array = new string[5] { "normal", "hover", "pressed", "hover_pressed", "disabled" };
		foreach (string text4 in array)
		{
			StyleBoxTexture styleBoxTexture = TextureStyle((text4 == "pressed" || text4 == "hover_pressed") ? "res://Asset/Texture/GUI/General/General/OptionButtonDown.png" : "res://Asset/Texture/GUI/General/General/OptionButton.png", 6f);
			styleBoxTexture.TextureMarginLeft = 6f;
			styleBoxTexture.TextureMarginRight = 6f;
			styleBoxTexture.ContentMarginTop = 2f;
			styleBoxTexture.ContentMarginBottom = 4f;
			StyleBoxTexture styleBoxTexture2 = TextureStyle("res://Asset/Texture/GUI/General/EditBox/EditBox.png", 6f);
			styleBoxTexture2.SetTextureMarginAll(5f);
			switch (text4)
			{
			case "disabled":
				styleBoxTexture.ModulateColor = new Color("8c8c8c");
				styleBoxTexture2.ModulateColor = new Color("8c8c8c");
				break;
			case "hover":
			case "hover_pressed":
				styleBoxTexture.ModulateColor = new Color(1.15f, 1.15f, 1.15f);
				styleBoxTexture2.ModulateColor = new Color(1.15f, 1.15f, 1.15f);
				break;
			}
			theme.SetStylebox(text4, "Button", styleBoxTexture);
			theme.SetStylebox(text4, "OptionButton", styleBoxTexture2);
		}
		theme.SetConstant("outline_size", "Button", 1);
		theme.SetColor("font_outline_color", "Button", new Color("37384a"));
		theme.SetStylebox("focus", "Button", FlatStyle("00000000", "f4dc9f"));
		theme.SetStylebox("focus", "OptionButton", FlatStyle("00000000", "f4dc9f"));
		theme.SetStylebox("panel", "TabContainer", FlatStyle("061610", "2f4631"));
		theme.SetStylebox("tab_selected", "TabContainer", FlatStyle("2f431f", "e6bd5e"));
		theme.SetStylebox("tab_unselected", "TabContainer", FlatStyle("11271c", "324d34"));
		return theme;
	}

	private static void AddPanelTitle(Control parent, string text)
	{
		Label label = new Label
		{
			Text = text,
			CustomMinimumSize = new Vector2(0f, 28f)
		};
		label.AddThemeFontOverride("font", GD.Load<FontFile>("res://Asset/Font/fzjz.ttf"));
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeColorOverride("font_color", new Color("f4dc9f"));
		parent.AddChild(label, forceReadableName: false, InternalMode.Disabled);
	}

	private static StyleBoxTexture TextureStyle(string path, float margin)
	{
		return new StyleBoxTexture
		{
			Texture = GD.Load<Texture2D>(path),
			ContentMarginLeft = margin,
			ContentMarginRight = margin,
			ContentMarginTop = 3f,
			ContentMarginBottom = 3f
		};
	}

	private static StyleBoxFlat FlatStyle(string background, string border)
	{
		StyleBoxFlat styleBoxFlat = new StyleBoxFlat();
		styleBoxFlat.BgColor = new Color(background);
		styleBoxFlat.BorderColor = new Color(border);
		styleBoxFlat.ContentMarginLeft = 4f;
		styleBoxFlat.ContentMarginRight = 4f;
		styleBoxFlat.ContentMarginTop = 3f;
		styleBoxFlat.ContentMarginBottom = 3f;
		styleBoxFlat.SetBorderWidthAll(1);
		return styleBoxFlat;
	}

	private static void ApplyPrimary(Button button)
	{
		string[] array = new string[4] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color" };
		foreach (string text in array)
		{
			button.AddThemeColorOverride(text, new Color("f4dc9f"));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(23)
		{
			new MethodInfo(MethodName.LoadCatalog, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.FilterCatalog, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCatalogSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "key", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateSelectedLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnBoardInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName.SelectCharacter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenFileDialog, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "save", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnFileSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExitSandbox, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BuildLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BuildCatalog, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "main", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("HBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.BuildStage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "main", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("HBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.BuildInspector, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "main", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("HBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.AddPanel, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "parent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
				new PropertyInfo(Variant.Type.Float, "width", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "texturePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BuildTheme, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Theme"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AddPanelTitle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "parent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
				new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TextureStyle, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("StyleBoxTexture"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "margin", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FlatStyle, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("StyleBoxFlat"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "background", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "border", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyPrimary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Button"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.LoadCatalog && args.Count == 0)
		{
			LoadCatalog();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FilterCatalog && args.Count == 0)
		{
			FilterCatalog();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCatalogSelected && args.Count == 1)
		{
			OnCatalogSelected(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateSelectedLabel && args.Count == 0)
		{
			UpdateSelectedLabel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBoardInput && args.Count == 1)
		{
			OnBoardInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectCharacter && args.Count == 1)
		{
			SelectCharacter(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshState && args.Count == 0)
		{
			RefreshState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenFileDialog && args.Count == 1)
		{
			OpenFileDialog(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFileSelected && args.Count == 1)
		{
			OnFileSelected(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExitSandbox && args.Count == 0)
		{
			ExitSandbox();
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BuildLayout && args.Count == 0)
		{
			BuildLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BuildCatalog && args.Count == 1)
		{
			BuildCatalog(VariantUtils.ConvertTo<HBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BuildStage && args.Count == 1)
		{
			BuildStage(VariantUtils.ConvertTo<HBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BuildInspector && args.Count == 1)
		{
			BuildInspector(VariantUtils.ConvertTo<HBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddPanel && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<VBoxContainer>(AddPanel(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<string>(in args[2])));
			return true;
		}
		if (method == MethodName.BuildTheme && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Theme>(BuildTheme());
			return true;
		}
		if (method == MethodName.AddPanelTitle && args.Count == 2)
		{
			AddPanelTitle(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TextureStyle && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<StyleBoxTexture>(TextureStyle(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1])));
			return true;
		}
		if (method == MethodName.FlatStyle && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<StyleBoxFlat>(FlatStyle(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.ApplyPrimary && args.Count == 1)
		{
			ApplyPrimary(VariantUtils.ConvertTo<Button>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.AddPanel && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<VBoxContainer>(AddPanel(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<string>(in args[2])));
			return true;
		}
		if (method == MethodName.AddPanelTitle && args.Count == 2)
		{
			AddPanelTitle(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TextureStyle && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<StyleBoxTexture>(TextureStyle(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1])));
			return true;
		}
		if (method == MethodName.FlatStyle && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<StyleBoxFlat>(FlatStyle(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.ApplyPrimary && args.Count == 1)
		{
			ApplyPrimary(VariantUtils.ConvertTo<Button>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.LoadCatalog)
		{
			return true;
		}
		if (method == MethodName.FilterCatalog)
		{
			return true;
		}
		if (method == MethodName.OnCatalogSelected)
		{
			return true;
		}
		if (method == MethodName.UpdateSelectedLabel)
		{
			return true;
		}
		if (method == MethodName.OnBoardInput)
		{
			return true;
		}
		if (method == MethodName.SelectCharacter)
		{
			return true;
		}
		if (method == MethodName.RefreshState)
		{
			return true;
		}
		if (method == MethodName.OpenFileDialog)
		{
			return true;
		}
		if (method == MethodName.OnFileSelected)
		{
			return true;
		}
		if (method == MethodName.ExitSandbox)
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.BuildLayout)
		{
			return true;
		}
		if (method == MethodName.BuildCatalog)
		{
			return true;
		}
		if (method == MethodName.BuildStage)
		{
			return true;
		}
		if (method == MethodName.BuildInspector)
		{
			return true;
		}
		if (method == MethodName.AddPanel)
		{
			return true;
		}
		if (method == MethodName.BuildTheme)
		{
			return true;
		}
		if (method == MethodName.AddPanelTitle)
		{
			return true;
		}
		if (method == MethodName.TextureStyle)
		{
			return true;
		}
		if (method == MethodName.FlatStyle)
		{
			return true;
		}
		if (method == MethodName.ApplyPrimary)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.InterfaceFont)
		{
			InterfaceFont = VariantUtils.ConvertTo<Font>(in value);
			return true;
		}
		if (name == PropertyName.Session)
		{
			Session = VariantUtils.ConvertTo<SandboxSession>(in value);
			return true;
		}
		if (name == PropertyName.Board)
		{
			Board = VariantUtils.ConvertTo<SandboxBoard>(in value);
			return true;
		}
		if (name == PropertyName._battleViewport)
		{
			_battleViewport = VariantUtils.ConvertTo<SubViewport>(in value);
			return true;
		}
		if (name == PropertyName._search)
		{
			_search = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._category)
		{
			_category = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._catalog)
		{
			_catalog = VariantUtils.ConvertTo<SandboxCardCatalog>(in value);
			return true;
		}
		if (name == PropertyName._selectedPacket)
		{
			_selectedPacket = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._team)
		{
			_team = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._facing)
		{
			_facing = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._teamButtons)
		{
			_teamButtons = VariantUtils.ConvertToSystemArrayOfGodotObject<Button>(in value);
			return true;
		}
		if (name == PropertyName._categoryButtons)
		{
			_categoryButtons = VariantUtils.ConvertToSystemArrayOfGodotObject<Button>(in value);
			return true;
		}
		if (name == PropertyName._catalogCount)
		{
			_catalogCount = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._maps)
		{
			_maps = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._tool)
		{
			_tool = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._stacking)
		{
			_stacking = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._status)
		{
			_status = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._state)
		{
			_state = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._selectedLabel)
		{
			_selectedLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._inspector)
		{
			_inspector = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._inspectorTabs)
		{
			_inspectorTabs = VariantUtils.ConvertTo<TabContainer>(in value);
			return true;
		}
		if (name == PropertyName._emptyInspector)
		{
			_emptyInspector = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._editTeam)
		{
			_editTeam = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._editFacing)
		{
			_editFacing = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._health)
		{
			_health = VariantUtils.ConvertTo<SpinBox>(in value);
			return true;
		}
		if (name == PropertyName._actionSpeed)
		{
			_actionSpeed = VariantUtils.ConvertTo<SpinBox>(in value);
			return true;
		}
		if (name == PropertyName._overlapSelection)
		{
			_overlapSelection = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._automaticFire)
		{
			_automaticFire = VariantUtils.ConvertTo<CheckButton>(in value);
			return true;
		}
		if (name == PropertyName._aimButton)
		{
			_aimButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._files)
		{
			_files = VariantUtils.ConvertTo<FileDialog>(in value);
			return true;
		}
		if (name == PropertyName._refreshTime)
		{
			_refreshTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._leaving)
		{
			_leaving = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InterfaceFont)
		{
			value = VariantUtils.CreateFrom<Font>(InterfaceFont);
			return true;
		}
		if (name == PropertyName.Session)
		{
			value = VariantUtils.CreateFrom<SandboxSession>(Session);
			return true;
		}
		if (name == PropertyName.Board)
		{
			value = VariantUtils.CreateFrom<SandboxBoard>(Board);
			return true;
		}
		if (name == PropertyName._battleViewport)
		{
			value = VariantUtils.CreateFrom(in _battleViewport);
			return true;
		}
		if (name == PropertyName._search)
		{
			value = VariantUtils.CreateFrom(in _search);
			return true;
		}
		if (name == PropertyName._category)
		{
			value = VariantUtils.CreateFrom(in _category);
			return true;
		}
		if (name == PropertyName._catalog)
		{
			value = VariantUtils.CreateFrom(in _catalog);
			return true;
		}
		if (name == PropertyName._selectedPacket)
		{
			value = VariantUtils.CreateFrom(in _selectedPacket);
			return true;
		}
		if (name == PropertyName._team)
		{
			value = VariantUtils.CreateFrom(in _team);
			return true;
		}
		if (name == PropertyName._facing)
		{
			value = VariantUtils.CreateFrom(in _facing);
			return true;
		}
		if (name == PropertyName._teamButtons)
		{
			GodotObject[] teamButtons = _teamButtons;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(teamButtons);
			return true;
		}
		if (name == PropertyName._categoryButtons)
		{
			GodotObject[] teamButtons = _categoryButtons;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(teamButtons);
			return true;
		}
		if (name == PropertyName._catalogCount)
		{
			value = VariantUtils.CreateFrom(in _catalogCount);
			return true;
		}
		if (name == PropertyName._maps)
		{
			value = VariantUtils.CreateFrom(in _maps);
			return true;
		}
		if (name == PropertyName._tool)
		{
			value = VariantUtils.CreateFrom(in _tool);
			return true;
		}
		if (name == PropertyName._stacking)
		{
			value = VariantUtils.CreateFrom(in _stacking);
			return true;
		}
		if (name == PropertyName._status)
		{
			value = VariantUtils.CreateFrom(in _status);
			return true;
		}
		if (name == PropertyName._state)
		{
			value = VariantUtils.CreateFrom(in _state);
			return true;
		}
		if (name == PropertyName._selectedLabel)
		{
			value = VariantUtils.CreateFrom(in _selectedLabel);
			return true;
		}
		if (name == PropertyName._inspector)
		{
			value = VariantUtils.CreateFrom(in _inspector);
			return true;
		}
		if (name == PropertyName._inspectorTabs)
		{
			value = VariantUtils.CreateFrom(in _inspectorTabs);
			return true;
		}
		if (name == PropertyName._emptyInspector)
		{
			value = VariantUtils.CreateFrom(in _emptyInspector);
			return true;
		}
		if (name == PropertyName._editTeam)
		{
			value = VariantUtils.CreateFrom(in _editTeam);
			return true;
		}
		if (name == PropertyName._editFacing)
		{
			value = VariantUtils.CreateFrom(in _editFacing);
			return true;
		}
		if (name == PropertyName._health)
		{
			value = VariantUtils.CreateFrom(in _health);
			return true;
		}
		if (name == PropertyName._actionSpeed)
		{
			value = VariantUtils.CreateFrom(in _actionSpeed);
			return true;
		}
		if (name == PropertyName._overlapSelection)
		{
			value = VariantUtils.CreateFrom(in _overlapSelection);
			return true;
		}
		if (name == PropertyName._automaticFire)
		{
			value = VariantUtils.CreateFrom(in _automaticFire);
			return true;
		}
		if (name == PropertyName._aimButton)
		{
			value = VariantUtils.CreateFrom(in _aimButton);
			return true;
		}
		if (name == PropertyName._files)
		{
			value = VariantUtils.CreateFrom(in _files);
			return true;
		}
		if (name == PropertyName._refreshTime)
		{
			value = VariantUtils.CreateFrom(in _refreshTime);
			return true;
		}
		if (name == PropertyName._leaving)
		{
			value = VariantUtils.CreateFrom(in _leaving);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.InterfaceFont, PropertyHint.ResourceType, "Font", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName.Session, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.Board, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._battleViewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._search, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._category, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._catalog, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._selectedPacket, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._team, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._facing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Array, PropertyName._teamButtons, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Array, PropertyName._categoryButtons, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._catalogCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._maps, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._tool, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._stacking, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._status, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._state, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._selectedLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._inspector, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._inspectorTabs, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._emptyInspector, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._editTeam, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._editFacing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._health, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._actionSpeed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._overlapSelection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._automaticFire, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._aimButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._files, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._refreshTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._leaving, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.InterfaceFont, Variant.From<Font>(InterfaceFont));
		info.AddProperty(PropertyName.Session, Variant.From<SandboxSession>(Session));
		info.AddProperty(PropertyName.Board, Variant.From<SandboxBoard>(Board));
		info.AddProperty(PropertyName._battleViewport, Variant.From(in _battleViewport));
		info.AddProperty(PropertyName._search, Variant.From(in _search));
		info.AddProperty(PropertyName._category, Variant.From(in _category));
		info.AddProperty(PropertyName._catalog, Variant.From(in _catalog));
		info.AddProperty(PropertyName._selectedPacket, Variant.From(in _selectedPacket));
		info.AddProperty(PropertyName._team, Variant.From(in _team));
		info.AddProperty(PropertyName._facing, Variant.From(in _facing));
		StringName teamButtons = PropertyName._teamButtons;
		GodotObject[] teamButtons2 = _teamButtons;
		info.AddProperty(teamButtons, Variant.CreateFrom(teamButtons2));
		StringName categoryButtons = PropertyName._categoryButtons;
		teamButtons2 = _categoryButtons;
		info.AddProperty(categoryButtons, Variant.CreateFrom(teamButtons2));
		info.AddProperty(PropertyName._catalogCount, Variant.From(in _catalogCount));
		info.AddProperty(PropertyName._maps, Variant.From(in _maps));
		info.AddProperty(PropertyName._tool, Variant.From(in _tool));
		info.AddProperty(PropertyName._stacking, Variant.From(in _stacking));
		info.AddProperty(PropertyName._status, Variant.From(in _status));
		info.AddProperty(PropertyName._state, Variant.From(in _state));
		info.AddProperty(PropertyName._selectedLabel, Variant.From(in _selectedLabel));
		info.AddProperty(PropertyName._inspector, Variant.From(in _inspector));
		info.AddProperty(PropertyName._inspectorTabs, Variant.From(in _inspectorTabs));
		info.AddProperty(PropertyName._emptyInspector, Variant.From(in _emptyInspector));
		info.AddProperty(PropertyName._editTeam, Variant.From(in _editTeam));
		info.AddProperty(PropertyName._editFacing, Variant.From(in _editFacing));
		info.AddProperty(PropertyName._health, Variant.From(in _health));
		info.AddProperty(PropertyName._actionSpeed, Variant.From(in _actionSpeed));
		info.AddProperty(PropertyName._overlapSelection, Variant.From(in _overlapSelection));
		info.AddProperty(PropertyName._automaticFire, Variant.From(in _automaticFire));
		info.AddProperty(PropertyName._aimButton, Variant.From(in _aimButton));
		info.AddProperty(PropertyName._files, Variant.From(in _files));
		info.AddProperty(PropertyName._refreshTime, Variant.From(in _refreshTime));
		info.AddProperty(PropertyName._leaving, Variant.From(in _leaving));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.InterfaceFont, out var value))
		{
			InterfaceFont = value.As<Font>();
		}
		if (info.TryGetProperty(PropertyName.Session, out var value2))
		{
			Session = value2.As<SandboxSession>();
		}
		if (info.TryGetProperty(PropertyName.Board, out var value3))
		{
			Board = value3.As<SandboxBoard>();
		}
		if (info.TryGetProperty(PropertyName._battleViewport, out var value4))
		{
			_battleViewport = value4.As<SubViewport>();
		}
		if (info.TryGetProperty(PropertyName._search, out var value5))
		{
			_search = value5.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._category, out var value6))
		{
			_category = value6.As<int>();
		}
		if (info.TryGetProperty(PropertyName._catalog, out var value7))
		{
			_catalog = value7.As<SandboxCardCatalog>();
		}
		if (info.TryGetProperty(PropertyName._selectedPacket, out var value8))
		{
			_selectedPacket = value8.As<string>();
		}
		if (info.TryGetProperty(PropertyName._team, out var value9))
		{
			_team = value9.As<int>();
		}
		if (info.TryGetProperty(PropertyName._facing, out var value10))
		{
			_facing = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._teamButtons, out var value11))
		{
			_teamButtons = value11.AsGodotObjectArray<Button>();
		}
		if (info.TryGetProperty(PropertyName._categoryButtons, out var value12))
		{
			_categoryButtons = value12.AsGodotObjectArray<Button>();
		}
		if (info.TryGetProperty(PropertyName._catalogCount, out var value13))
		{
			_catalogCount = value13.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._maps, out var value14))
		{
			_maps = value14.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._tool, out var value15))
		{
			_tool = value15.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._stacking, out var value16))
		{
			_stacking = value16.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._status, out var value17))
		{
			_status = value17.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._state, out var value18))
		{
			_state = value18.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._selectedLabel, out var value19))
		{
			_selectedLabel = value19.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._inspector, out var value20))
		{
			_inspector = value20.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._inspectorTabs, out var value21))
		{
			_inspectorTabs = value21.As<TabContainer>();
		}
		if (info.TryGetProperty(PropertyName._emptyInspector, out var value22))
		{
			_emptyInspector = value22.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._editTeam, out var value23))
		{
			_editTeam = value23.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._editFacing, out var value24))
		{
			_editFacing = value24.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._health, out var value25))
		{
			_health = value25.As<SpinBox>();
		}
		if (info.TryGetProperty(PropertyName._actionSpeed, out var value26))
		{
			_actionSpeed = value26.As<SpinBox>();
		}
		if (info.TryGetProperty(PropertyName._overlapSelection, out var value27))
		{
			_overlapSelection = value27.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._automaticFire, out var value28))
		{
			_automaticFire = value28.As<CheckButton>();
		}
		if (info.TryGetProperty(PropertyName._aimButton, out var value29))
		{
			_aimButton = value29.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._files, out var value30))
		{
			_files = value30.As<FileDialog>();
		}
		if (info.TryGetProperty(PropertyName._refreshTime, out var value31))
		{
			_refreshTime = value31.As<double>();
		}
		if (info.TryGetProperty(PropertyName._leaving, out var value32))
		{
			_leaving = value32.As<bool>();
		}
	}
}
