using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[ScriptPath("res://Scene/Sandbox/SandboxSession.cs")]
public class SandboxSession : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _PhysicsProcess = "_PhysicsProcess";

		public static readonly StringName FitCamera = "FitCamera";

		public static readonly StringName Start = "Start";

		public static readonly StringName Pause = "Pause";

		public static readonly StringName Step = "Step";

		public static readonly StringName SetSpeed = "SetSpeed";

		public static readonly StringName SetStacking = "SetStacking";

		public static readonly StringName GetFacing = "GetFacing";

		public static readonly StringName Capture = "Capture";

		public static readonly StringName Delete = "Delete";

		public static readonly StringName Edit = "Edit";

		public static readonly StringName SetAutomaticFire = "SetAutomaticFire";

		public static readonly StringName FireAt = "FireAt";

		public static readonly StringName Save = "Save";

		public static readonly StringName SetStatus = "SetStatus";

		public static readonly StringName IsAlive = "IsAlive";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName IsPreparing = "IsPreparing";

		public static readonly StringName IsBusy = "IsBusy";

		public static readonly StringName Status = "Status";

		public static readonly StringName Speed = "Speed";

		public static readonly StringName MapName = "MapName";

		public static readonly StringName AllowStacking = "AllowStacking";

		public static readonly StringName Process = "Process";

		public static readonly StringName CanEdit = "CanEdit";

		public static readonly StringName _battleScene = "_battleScene";

		public static readonly StringName _viewport = "_viewport";

		public static readonly StringName _battle = "_battle";

		public static readonly StringName _busy = "_busy";

		public static readonly StringName _stepFrame = "_stepFrame";

		public static readonly StringName _previousTimeScale = "_previousTimeScale";

		public static readonly StringName _initialized = "_initialized";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const string BattleScenePath = "res://Scene/TowerDefesne/TowerDefenseNew/TowerDefenseControlNew.tscn";

	private PackedScene _battleScene;

	private SubViewport _viewport;

	private TowerDefenseControlNew _battle;

	private readonly Dictionary<TowerDefenseCharacter, SandboxPlacement> _placements = new Dictionary<TowerDefenseCharacter, SandboxPlacement>();

	private SandboxScenario _draft = new SandboxScenario();

	private bool _busy;

	private ulong _stepFrame;

	private double _previousTimeScale;

	private bool _initialized;

	public bool IsPreparing { get; private set; } = true;

	public bool IsBusy => _busy;

	public string Status { get; private set; } = "正在准备沙盒……";

	public double Speed { get; private set; } = 1.0;

	public string MapName => _draft.Map;

	public bool AllowStacking => _draft.AllowStacking;

	public TowerDefenseBattleProcessSandbox Process
	{
		get
		{
			if (!GodotObject.IsInstanceValid(_battle))
			{
				return null;
			}
			return _battle.process as TowerDefenseBattleProcessSandbox;
		}
	}

	public bool CanEdit
	{
		get
		{
			if (!_busy && IsInsideTree())
			{
				TowerDefenseBattleProcessSandbox process = Process;
				if (process != null && process.IsReady)
				{
					return !Global.IsMultiplayerMode;
				}
			}
			return false;
		}
	}

	public IReadOnlyList<TowerDefenseCharacter> Characters
	{
		get
		{
			if (!GodotObject.IsInstanceValid(_battle?.characterNode))
			{
				return Array.Empty<TowerDefenseCharacter>();
			}
			return _battle.characterNode.GetChildren().OfType<TowerDefenseCharacter>().ToArray();
		}
	}

	public event Action Changed;

	public override void _Ready()
	{
		base.ProcessMode = ProcessModeEnum.Always;
		base.ProcessPhysicsPriority = 2147483647;
		_previousTimeScale = Global.TimeScale;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_stepFrame != 0L && Engine.GetPhysicsFrames() >= _stepFrame)
		{
			_stepFrame = 0uL;
			GetTree().Paused = true;
			SetStatus("单步完成，模拟已暂停。");
		}
	}

	public async Task InitializeAsync(SubViewport viewport)
	{
		_viewport = viewport;
		_battleScene = GD.Load<PackedScene>("res://Scene/TowerDefesne/TowerDefenseNew/TowerDefenseControlNew.tscn");
		await ResourceManager.Instance.EnsureFullGameplayResourcesReadyAsync();
		if (IsInsideTree())
		{
			await RebuildAsync(_draft, run: false);
		}
	}

	public async Task<bool> RebuildAsync(SandboxScenario candidate, bool run)
	{
		if (_busy || Global.IsMultiplayerMode || !IsInsideTree() || !candidate.Validate(out var _))
		{
			SetStatus("无法重建：请确认地图和布阵有效，并等待当前操作完成。");
			return false;
		}
		_busy = true;
		_stepFrame = 0uL;
		SetStatus("正在恢复布阵……");
		try
		{
			GetTree().Paused = false;
			if (GodotObject.IsInstanceValid(_battle))
			{
				ObjectManager.Instance.Clear();
				_battle.Free();
				_battle = null;
			}
			_placements.Clear();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (!IsInsideTree())
			{
				return false;
			}
			SandboxScenario next = candidate.Copy();
			Json json = new Json();
			json.Parse(FileAccess.GetFileAsString("res://Asset/Config/Level/TowerDefense/Sandbox/Sandbox.json"));
			TowerDefenseLevelNewConfig towerDefenseLevelNewConfig = new TowerDefenseLevelNewConfig
			{
				data = json
			};
			towerDefenseLevelNewConfig.featureData["Map"]["MapName"] = next.Map;
			TowerDefenseManager.Instance.currentLevelConfig = towerDefenseLevelNewConfig;
			Global.Instance.enterLevelMode = "Sandbox";
			_battle = _battleScene.Instantiate<TowerDefenseControlNew>(PackedScene.GenEditState.Disabled);
			_battle.ProcessMode = ProcessModeEnum.Pausable;
			_viewport.AddChild(_battle, forceReadableName: false, InternalMode.Disabled);
			ulong deadline = Time.GetTicksMsec() + 30000;
			while (IsInsideTree())
			{
				TowerDefenseBattleProcessSandbox process = Process;
				if ((process != null && process.IsReady) || Time.GetTicksMsec() >= deadline)
				{
					break;
				}
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
			if (!IsInsideTree())
			{
				return false;
			}
			TowerDefenseBattleProcessSandbox process2 = Process;
			if (process2 == null || !process2.IsReady)
			{
				throw new InvalidOperationException("正式战场初始化超时。");
			}
			FitCamera();
			foreach (SandboxPlacement character in next.Characters)
			{
				if (CreateCharacter(character) == null)
				{
					throw new InvalidOperationException("无法放置角色：" + character.Packet);
				}
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (!IsInsideTree())
			{
				return false;
			}
			_draft = next;
			_initialized = true;
			IsPreparing = !run;
			Global.TimeScale = Speed;
			GetTree().Paused = !run;
			SetStatus(run ? "已按保存的布阵重新开战。" : "布阵就绪：选择角色，再点击草坪放置。");
			return true;
		}
		finally
		{
			_busy = false;
			this.Changed?.Invoke();
		}
	}

	private void FitCamera()
	{
		TowerDefenseMapConfig config = _battle.GetFeature<TowerDefenseBattleFeatureMap>("Map").config;
		TowerDefenseCameraControl cameraControl = _battle.GetFeature<TowerDefenseBattleFeatureCamera>("Camera").cameraControl;
		Camera2D camera = cameraControl.camera;
		camera.AnchorMode = Camera2D.AnchorModeEnum.DragCenter;
		Vector2 vector = config.gridSize * config.gridNum;
		float num = Mathf.Min((float)_viewport.Size.X / (vector.X + 160f), (float)_viewport.Size.Y / (vector.Y + 120f));
		camera.PositionSmoothingEnabled = false;
		camera.LimitLeft = -100000;
		camera.LimitTop = -100000;
		camera.LimitRight = 100000;
		camera.LimitBottom = 100000;
		camera.Zoom = Vector2.One * num;
		camera.GlobalPosition = config.gridBeginPos + vector * 0.5f;
		camera.ForceUpdateScroll();
		cameraControl.SetProcessInput(enable: false);
		cameraControl.SetProcessUnhandledInput(enable: false);
	}

	private TowerDefenseCharacter CreateCharacter(SandboxPlacement placement)
	{
		TowerDefensePacketConfig towerDefensePacketConfig = TowerDefenseManager.GetPacketConfig(placement.Packet).CreateSpawnRuntimeCopy();
		bool flag = towerDefensePacketConfig.characterConfig is TowerDefenseZombieConfig;
		towerDefensePacketConfig.packetFlip = placement.Facing != ((!flag) ? 1 : (-1));
		towerDefensePacketConfig.overrideHypnoses = false;
		TowerDefenseCharacter character = towerDefensePacketConfig.Plant(new Vector2I(placement.Column, placement.Row), playAudio: false, noLimit: true, default(EconomyAccountId), skipPlacementCheck: true);
		if (!GodotObject.IsInstanceValid(character))
		{
			return null;
		}
		character.camp = (TowerDefenseEnum.CHARACTER_CAMP)placement.Team;
		_placements[character] = placement.Copy();
		Callable.From(delegate
		{
			if (IsInsideTree() && GodotObject.IsInstanceValid(character) && !character.IsQueuedForDeletion())
			{
				character.camp = (TowerDefenseEnum.CHARACTER_CAMP)placement.Team;
				character.instance.hitpointScale = placement.HealthScale;
				character.timeScaleInit = placement.ActionSpeed;
				character.timeScale = placement.ActionSpeed;
				CannonComponent cannonComponent = character.componentManager?.GetRuntime<CannonComponent>();
				if (cannonComponent != null)
				{
					cannonComponent.autoAttack = placement.AutoAttack;
				}
				if (placement.OffsetX != 0f)
				{
					character.SetLogicalGlobalPosition(character.GetLogicalGlobalPosition() + new Vector2(placement.OffsetX, 0f));
				}
			}
		}).CallDeferred();
		return character;
	}

	public async Task<TowerDefenseCharacter> PlaceAsync(SandboxPlacement placement)
	{
		if (!CanEdit)
		{
			return null;
		}
		TowerDefenseMapConfig config = _battle.GetFeature<TowerDefenseBattleFeatureMap>("Map").config;
		if (!placement.Validate(config, out var error))
		{
			SetStatus(error);
			return null;
		}
		TowerDefenseCharacter[] array = Characters.Where(IsAlive).ToArray();
		if (array.Length >= 500)
		{
			SetStatus("已达到 500 个角色上限。");
			return null;
		}
		if (!AllowStacking)
		{
			TowerDefenseCharacterConfig characterConfig = TowerDefenseManager.GetPacketConfig(placement.Packet).characterConfig;
			HashSet<Vector2I> plantingCells = SandboxScenario.GetPlantingCells(characterConfig, new Vector2I(placement.Column, placement.Row));
			TowerDefenseCharacter[] array2 = array;
			foreach (TowerDefenseCharacter towerDefenseCharacter in array2)
			{
				Vector2I mapGridPosFromMouse = TowerDefenseManager.Instance.GetMapGridPosFromMouse(towerDefenseCharacter.GetLogicalGlobalPosition());
				if (!SandboxScenario.CanSharePlantingCell(characterConfig, placement.Team, towerDefenseCharacter.config, (int)towerDefenseCharacter.camp) && plantingCells.Overlaps(SandboxScenario.GetPlantingCells(towerDefenseCharacter.config, mapGridPosFromMouse)))
				{
					SetStatus("该格已有同层角色；底座与植物可正常共存，额外重叠请开启允许叠放。");
					return null;
				}
			}
		}
		_busy = true;
		try
		{
			TowerDefenseCharacter character = CreateCharacter(placement);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (!IsInsideTree() || !IsAlive(character))
			{
				SetStatus("该角色无法放在此处，请检查场地或所需目标。");
				return null;
			}
			if (IsPreparing)
			{
				_draft.Characters.Add(placement.Copy());
			}
			SetStatus("已放置" + ((placement.Team == 0) ? "红队" : "蓝队") + "角色；可继续点击草坪。");
			return character;
		}
		finally
		{
			_busy = false;
			this.Changed?.Invoke();
		}
	}

	public void Start()
	{
		if (CanEdit)
		{
			IsPreparing = false;
			_stepFrame = 0uL;
			GetTree().Paused = false;
			SetStatus("交战中：可以继续放置和编辑，点击返回布阵可恢复开战方案。");
		}
	}

	public void Pause()
	{
		if (CanEdit)
		{
			_stepFrame = 0uL;
			GetTree().Paused = true;
			SetStatus("模拟已暂停，可以检查或调整场上角色。");
		}
	}

	public void Step()
	{
		if (CanEdit && _stepFrame == 0L)
		{
			IsPreparing = false;
			_stepFrame = Engine.GetPhysicsFrames() + 1;
			GetTree().Paused = false;
		}
	}

	public void SetSpeed(double speed)
	{
		if (double.IsFinite(speed) && !(speed < 0.25) && !(speed > 4.0))
		{
			Speed = speed;
			Global.TimeScale = speed;
			this.Changed?.Invoke();
		}
	}

	public bool SetStacking(bool enabled)
	{
		if (!CanEdit)
		{
			return false;
		}
		SandboxScenario sandboxScenario = _draft.Copy();
		sandboxScenario.AllowStacking = enabled;
		if (!sandboxScenario.Validate(out var error))
		{
			SetStatus(error);
			return false;
		}
		_draft.AllowStacking = enabled;
		this.Changed?.Invoke();
		return true;
	}

	public SandboxScenario CopyDraft()
	{
		return _draft.Copy();
	}

	public SandboxPlacement ReadCharacter(TowerDefenseCharacter character)
	{
		SandboxPlacement value;
		SandboxPlacement obj = (_placements.TryGetValue(character, out value) ? value.Copy() : new SandboxPlacement
		{
			Packet = character.config.name
		});
		obj.Team = (int)character.camp;
		obj.Facing = GetFacing(character);
		obj.HealthScale = character.instance.hitpointScale;
		obj.ActionSpeed = character.timeScaleInit;
		obj.AutoAttack = character.componentManager?.GetRuntime<CannonComponent>()?.autoAttack ?? true;
		Vector2I mapGridPosFromMouse = TowerDefenseManager.Instance.GetMapGridPosFromMouse(character.GetLogicalGlobalPosition());
		obj.Column = mapGridPosFromMouse.X;
		obj.Row = mapGridPosFromMouse.Y;
		obj.OffsetX = character.GetLogicalGlobalPosition().X - TowerDefenseManager.GetMapCellPlantPos(mapGridPosFromMouse).X;
		return obj;
	}

	public static int GetFacing(TowerDefenseCharacter character)
	{
		return ((!(character is TowerDefenseZombie)) ? 1 : (-1)) * ((!(character.transformPoint.GlobalTransform.X.X < 0f)) ? 1 : (-1));
	}

	public bool Capture()
	{
		if (!CanEdit)
		{
			return false;
		}
		SandboxScenario sandboxScenario = _draft.Copy();
		sandboxScenario.Characters.Clear();
		foreach (TowerDefenseCharacter character in Characters)
		{
			if (IsAlive(character))
			{
				sandboxScenario.Characters.Add(ReadCharacter(character));
			}
		}
		if (!sandboxScenario.Validate(out var error))
		{
			SetStatus("未收录：" + error);
			return false;
		}
		_draft = sandboxScenario;
		SetStatus("当前场面已设为开战方案，重开时恢复完整生命。");
		return true;
	}

	public void Delete(TowerDefenseCharacter character)
	{
		if (CanEdit && IsAlive(character))
		{
			_placements.Remove(character);
			character.skipDestroySet = true;
			character.suppressDeathrattles = true;
			character.Destroy();
			if (IsPreparing)
			{
				Capture();
			}
			SetStatus("已删除选中角色。");
		}
	}

	public bool Edit(TowerDefenseCharacter character, int team, int facing, double healthScale, double actionSpeed)
	{
		if (!CanEdit || !IsAlive(character))
		{
			return false;
		}
		SandboxPlacement sandboxPlacement = ReadCharacter(character);
		sandboxPlacement.Team = team;
		sandboxPlacement.Facing = facing;
		sandboxPlacement.HealthScale = healthScale;
		sandboxPlacement.ActionSpeed = actionSpeed;
		if (!sandboxPlacement.Validate(_battle.GetFeature<TowerDefenseBattleFeatureMap>("Map").config, out var error))
		{
			SetStatus(error);
			return false;
		}
		int facing2 = ReadCharacter(character).Facing;
		character.camp = (TowerDefenseEnum.CHARACTER_CAMP)team;
		if (facing2 != facing)
		{
			character.Scale *= new Vector2(-1f, 1f);
			character.componentManager?.GetRuntime<GroundMoveComponent>()?.RefreshDirectionCache();
			character.sprite.NotifyAncestorTransformChangedForRender();
		}
		character.instance.hitpointScale = healthScale;
		character.timeScaleInit = actionSpeed;
		character.timeScale = actionSpeed;
		_placements[character] = sandboxPlacement;
		if (IsPreparing)
		{
			Capture();
		}
		SetStatus("实例参数已应用。");
		return true;
	}

	public void SetAutomaticFire(TowerDefenseCharacter character, bool enabled)
	{
		if (!CanEdit || !IsAlive(character))
		{
			return;
		}
		CannonComponent cannonComponent = character.componentManager?.GetRuntime<CannonComponent>();
		if (cannonComponent != null && !cannonComponent.IsReleased)
		{
			cannonComponent.autoAttack = enabled;
			if (IsPreparing)
			{
				Capture();
			}
			SetStatus(enabled ? "炮台会自动向敌对角色发射。" : "炮台已改为手动瞄准。");
		}
	}

	public bool FireAt(TowerDefenseCharacter character, Vector2 world)
	{
		if (CanEdit && IsAlive(character))
		{
			CannonComponent cannonComponent = character.componentManager?.GetRuntime<CannonComponent>();
			if (cannonComponent != null && !cannonComponent.IsReleased)
			{
				if (!cannonComponent.CanFire())
				{
					SetStatus("炮台尚未就绪，请开始模拟或等待装填完成。");
					return false;
				}
				cannonComponent.FireAt(world);
				SetStatus("已指定炮击位置；暂停时开始或单步即可推进发射。");
				return !cannonComponent.canFire;
			}
		}
		SetStatus("请先检查并选中一门加农炮。");
		return false;
	}

	public bool Save(string path)
	{
		try
		{
			using FileAccess fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			if (fileAccess == null)
			{
				throw new InvalidOperationException(FileAccess.GetOpenError().ToString());
			}
			fileAccess.StoreString(_draft.ToJson());
			SetStatus("开战方案已保存：" + path);
			return true;
		}
		catch (Exception ex)
		{
			SetStatus("保存失败：" + ex.Message);
			return false;
		}
	}

	public async Task<bool> LoadAsync(string path)
	{
		using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null || file.GetLength() > 1048576)
		{
			SetStatus("无法读取方案，文件不存在或超过 1 MB。");
			return false;
		}
		if (!SandboxScenario.TryRead(file.GetAsText(), out var scenario, out var error))
		{
			SetStatus(error);
			return false;
		}
		return await RebuildAsync(scenario, run: false);
	}

	public void SetStatus(string status)
	{
		Status = status;
		this.Changed?.Invoke();
	}

	public static bool IsAlive(TowerDefenseCharacter character)
	{
		if (GodotObject.IsInstanceValid(character) && !character.IsQueuedForDeletion() && !character.isDestroy && !character.die)
		{
			return GodotObject.IsInstanceValid(character.instance);
		}
		return false;
	}

	public override void _ExitTree()
	{
		_stepFrame = 0uL;
		GetTree().Paused = false;
		Global.TimeScale = _previousTimeScale;
		if (_initialized)
		{
			ObjectManager.Instance?.Clear();
		}
		_placements.Clear();
		this.Changed = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(18)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._PhysicsProcess, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FitCamera, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Pause, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Step, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetSpeed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "speed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetStacking, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetFacing, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.Capture, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Delete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.Edit, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Int, "team", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "facing", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "healthScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "actionSpeed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetAutomaticFire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FireAt, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Vector2, "world", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Save, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetStatus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "status", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsAlive, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
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
		if (method == MethodName._PhysicsProcess && args.Count == 1)
		{
			_PhysicsProcess(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FitCamera && args.Count == 0)
		{
			FitCamera();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Start && args.Count == 0)
		{
			Start();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Pause && args.Count == 0)
		{
			Pause();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Step && args.Count == 0)
		{
			Step();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSpeed && args.Count == 1)
		{
			SetSpeed(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetStacking && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(SetStacking(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.GetFacing && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<int>(GetFacing(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.Capture && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(Capture());
			return true;
		}
		if (method == MethodName.Delete && args.Count == 1)
		{
			Delete(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Edit && args.Count == 5)
		{
			ret = VariantUtils.CreateFrom<bool>(Edit(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<double>(in args[3]), VariantUtils.ConvertTo<double>(in args[4])));
			return true;
		}
		if (method == MethodName.SetAutomaticFire && args.Count == 2)
		{
			SetAutomaticFire(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FireAt && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(FireAt(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
			return true;
		}
		if (method == MethodName.Save && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(Save(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.SetStatus && args.Count == 1)
		{
			SetStatus(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsAlive && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsAlive(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
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
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetFacing && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<int>(GetFacing(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.IsAlive && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsAlive(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
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
		if (method == MethodName._PhysicsProcess)
		{
			return true;
		}
		if (method == MethodName.FitCamera)
		{
			return true;
		}
		if (method == MethodName.Start)
		{
			return true;
		}
		if (method == MethodName.Pause)
		{
			return true;
		}
		if (method == MethodName.Step)
		{
			return true;
		}
		if (method == MethodName.SetSpeed)
		{
			return true;
		}
		if (method == MethodName.SetStacking)
		{
			return true;
		}
		if (method == MethodName.GetFacing)
		{
			return true;
		}
		if (method == MethodName.Capture)
		{
			return true;
		}
		if (method == MethodName.Delete)
		{
			return true;
		}
		if (method == MethodName.Edit)
		{
			return true;
		}
		if (method == MethodName.SetAutomaticFire)
		{
			return true;
		}
		if (method == MethodName.FireAt)
		{
			return true;
		}
		if (method == MethodName.Save)
		{
			return true;
		}
		if (method == MethodName.SetStatus)
		{
			return true;
		}
		if (method == MethodName.IsAlive)
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
		if (name == PropertyName.IsPreparing)
		{
			IsPreparing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.Status)
		{
			Status = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.Speed)
		{
			Speed = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._battleScene)
		{
			_battleScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<SubViewport>(in value);
			return true;
		}
		if (name == PropertyName._battle)
		{
			_battle = VariantUtils.ConvertTo<TowerDefenseControlNew>(in value);
			return true;
		}
		if (name == PropertyName._busy)
		{
			_busy = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._stepFrame)
		{
			_stepFrame = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._previousTimeScale)
		{
			_previousTimeScale = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._initialized)
		{
			_initialized = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.IsPreparing)
		{
			from = IsPreparing;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsBusy)
		{
			from = IsBusy;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		string from2;
		if (name == PropertyName.Status)
		{
			from2 = Status;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.Speed)
		{
			value = VariantUtils.CreateFrom<double>(Speed);
			return true;
		}
		if (name == PropertyName.MapName)
		{
			from2 = MapName;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.AllowStacking)
		{
			from = AllowStacking;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Process)
		{
			value = VariantUtils.CreateFrom<TowerDefenseBattleProcessSandbox>(Process);
			return true;
		}
		if (name == PropertyName.CanEdit)
		{
			from = CanEdit;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._battleScene)
		{
			value = VariantUtils.CreateFrom(in _battleScene);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._battle)
		{
			value = VariantUtils.CreateFrom(in _battle);
			return true;
		}
		if (name == PropertyName._busy)
		{
			value = VariantUtils.CreateFrom(in _busy);
			return true;
		}
		if (name == PropertyName._stepFrame)
		{
			value = VariantUtils.CreateFrom(in _stepFrame);
			return true;
		}
		if (name == PropertyName._previousTimeScale)
		{
			value = VariantUtils.CreateFrom(in _previousTimeScale);
			return true;
		}
		if (name == PropertyName._initialized)
		{
			value = VariantUtils.CreateFrom(in _initialized);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._battleScene, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._battle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._busy, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._stepFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._previousTimeScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._initialized, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsPreparing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsBusy, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.Status, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.Speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.MapName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.AllowStacking, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.Process, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanEdit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsPreparing, Variant.From<bool>(IsPreparing));
		info.AddProperty(PropertyName.Status, Variant.From<string>(Status));
		info.AddProperty(PropertyName.Speed, Variant.From<double>(Speed));
		info.AddProperty(PropertyName._battleScene, Variant.From(in _battleScene));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._battle, Variant.From(in _battle));
		info.AddProperty(PropertyName._busy, Variant.From(in _busy));
		info.AddProperty(PropertyName._stepFrame, Variant.From(in _stepFrame));
		info.AddProperty(PropertyName._previousTimeScale, Variant.From(in _previousTimeScale));
		info.AddProperty(PropertyName._initialized, Variant.From(in _initialized));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsPreparing, out var value))
		{
			IsPreparing = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.Status, out var value2))
		{
			Status = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName.Speed, out var value3))
		{
			Speed = value3.As<double>();
		}
		if (info.TryGetProperty(PropertyName._battleScene, out var value4))
		{
			_battleScene = value4.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._viewport, out var value5))
		{
			_viewport = value5.As<SubViewport>();
		}
		if (info.TryGetProperty(PropertyName._battle, out var value6))
		{
			_battle = value6.As<TowerDefenseControlNew>();
		}
		if (info.TryGetProperty(PropertyName._busy, out var value7))
		{
			_busy = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._stepFrame, out var value8))
		{
			_stepFrame = value8.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._previousTimeScale, out var value9))
		{
			_previousTimeScale = value9.As<double>();
		}
		if (info.TryGetProperty(PropertyName._initialized, out var value10))
		{
			_initialized = value10.As<bool>();
		}
	}
}
