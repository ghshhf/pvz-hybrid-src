using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;
using PVZHE.ModEditor.Tools.GUI;

[GlobalClass]
[ScriptPath("res://Scene/Loading/Loading.cs")]
public class Loading : Control
{
	private enum ModInstallConfirmation
	{
		Confirmed,
		Cancelled,
		Unavailable
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName StartResourceLoadAfterFirstDraw = "StartResourceLoadAfterFirstDraw";

		public static readonly StringName MountView = "MountView";

		public static readonly StringName PresentationChanged = "PresentationChanged";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName PublishState = "PublishState";

		public static readonly StringName SetPercentage = "SetPercentage";

		public static readonly StringName LoadOver = "LoadOver";

		public static readonly StringName LoadFailed = "LoadFailed";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName IsClearCacheDialogVisible = "IsClearCacheDialogVisible";

		public static readonly StringName ViewStartRequested = "ViewStartRequested";

		public static readonly StringName OnlineButtonPressed = "OnlineButtonPressed";

		public static readonly StringName OfflineButtonPressed = "OfflineButtonPressed";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName EscapeBbcode = "EscapeBbcode";

		public static readonly StringName SafeDialogField = "SafeDialogField";

		public static readonly StringName ReportStartupModImportResult = "ReportStartupModImportResult";

		public static readonly StringName ShowStartupModImportResult = "ShowStartupModImportResult";

		public static readonly StringName LayoutStartupModResultDialog = "LayoutStartupModResultDialog";

		public static readonly StringName ShowClearLoadingCacheDialog = "ShowClearLoadingCacheDialog";

		public static readonly StringName BuildClearCacheDialogText = "BuildClearCacheDialogText";

		public static readonly StringName ClearLoadingCacheConfirmed = "ClearLoadingCacheConfirmed";

		public static readonly StringName ClearLoadedMapResourceCaches = "ClearLoadedMapResourceCaches";

		public static readonly StringName BuildLoadingText = "BuildLoadingText";

		public static readonly StringName TranslateLoadingStep = "TranslateLoadingStep";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ActiveView = "ActiveView";

		public static readonly StringName ModPresentationOwner = "ModPresentationOwner";

		public static readonly StringName CanStart = "CanStart";

		public static readonly StringName LoadingFirstFramePresentedUsec = "LoadingFirstFramePresentedUsec";

		public static readonly StringName BootstrapFirstFramePresentedUsec = "BootstrapFirstFramePresentedUsec";

		public static readonly StringName GameplayResourceLoadRequestedUsec = "GameplayResourceLoadRequestedUsec";

		public static readonly StringName _view = "_view";

		public static readonly StringName _clearCacheDialog = "_clearCacheDialog";

		public static readonly StringName _viewInitialized = "_viewInitialized";

		public static readonly StringName _loadCompleted = "_loadCompleted";

		public static readonly StringName _startupModsReady = "_startupModsReady";

		public static readonly StringName _applyingMods = "_applyingMods";

		public static readonly StringName _isStart = "_isStart";

		public static readonly StringName _viewOwner = "_viewOwner";

		public static readonly StringName _status = "_status";

		public static readonly StringName _error = "_error";

		public static readonly StringName _percentage = "_percentage";

		public static readonly StringName _startupImportMessage = "_startupImportMessage";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly SemaphoreSlim StartupModsGate = new SemaphoreSlim(1, 1);

	private XWModLoadingView _view;

	private XWModStartupContext _startup;

	private DialogBoxChoose _clearCacheDialog;

	private bool _viewInitialized;

	private bool _loadCompleted;

	private bool _startupModsReady;

	private bool _applyingMods;

	private bool _isStart;

	private string _viewOwner = "";

	private string _status = "正在准备加载";

	private string _error = "";

	private double _percentage;

	private XWLoadingState _lastState;

	private bool? _startupImportSuccess;

	private string _startupImportMessage = "";

	private XWModStartupReport _startupReport;

	public XWModLoadingView ActiveView => _view;

	public string ModPresentationOwner => _viewOwner;

	public bool CanStart
	{
		get
		{
			if (!_isStart && _error.Length == 0 && _loadCompleted && _startupModsReady)
			{
				ResourceManager instance = ResourceManager.Instance;
				if (instance != null && instance.AreFullGameplayResourcesReady && GodotObject.IsInstanceValid(_view))
				{
					return _view.PresentationComplete;
				}
			}
			return false;
		}
	}

	public ulong LoadingFirstFramePresentedUsec { get; private set; }

	public ulong BootstrapFirstFramePresentedUsec { get; private set; }

	public ulong GameplayResourceLoadRequestedUsec { get; private set; }

	public override void _Ready()
	{
		StartupLoadDiagnostics.Mark("loading.ready.begin");
		ResourceManager.Instance.OnLoadPercentage += SetPercentage;
		ResourceManager.Instance.OnLoadOver += LoadOver;
		ResourceManager.Instance.OnLoadFailed += LoadFailed;
		StartResourceLoadAfterFirstDraw();
		StartupLoadDiagnostics.Mark("loading.ready.end");
		StartupLoadDiagnostics.StopObservingTree();
	}

	private static bool IsHeadlessDisplay()
	{
		string name = DisplayServer.GetName();
		return string.IsNullOrEmpty(name) || name == "headless";
	}

	private async Task WaitFirstFrameAsync()
	{
		if (IsHeadlessDisplay())
		{
			// 无头模式（--headless）下 RenderingServer 不会派发 FramePostDraw，等待会永久挂起。
			// 退化为等待主循环帧，语义上同样是"至少跑过一帧"。
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			return;
		}
		await RenderingServer.Singleton.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
	}

	private async void StartResourceLoadAfterFirstDraw()
	{
		_ = 5;
		try
		{
			await WaitFirstFrameAsync();
			if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
			{
				return;
			}
			LoadingFirstFramePresentedUsec = Time.GetTicksUsec();
			StartupLoadDiagnostics.Mark("loading.first_frame");
			await StartupModsGate.WaitAsync();
			try
			{
				if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
				{
					return;
				}
				XWModStartupContext current = XWModStartupContext.Current;
				if (current != null)
				{
					_startup = current;
					if (!current.Applied)
					{
						await current.PreparePresentationAsync(this);
					}
				}
				else
				{
					XWModManager manager = new XWModManager();
					manager.PrepareStartupStorage();
					try
					{
						await PrepareStartupInstall();
					}
					catch (Exception ex)
					{
						ReportStartupModImportResult(success: false, ex.GetBaseException().Message);
					}
					if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
					{
						return;
					}
					_startup = manager.PrepareStartup();
					await _startup.PreparePresentationAsync(this);
				}
			}
			catch (Exception ex2)
			{
				GD.PushWarning("[Loading] Mod 启动准备失败，使用默认界面：" + ex2.GetBaseException().Message);
				_startupReport = new XWModStartupReport
				{
					LegacyCount = -1,
					BlockingReason = ex2.GetBaseException().Message
				};
			}
			finally
			{
				StartupModsGate.Release();
			}
			if (GodotObject.IsInstanceValid(this) && IsInsideTree())
			{
				if (_startup != null)
				{
					_startup.PresentationChanged += PresentationChanged;
				}
				MountView(allowMod: true);
				_viewInitialized = true;
				_status = Tr("LOAD_STARTUP");
				PublishState();
				await WaitFirstFrameAsync();
				if (GodotObject.IsInstanceValid(this) && IsInsideTree())
				{
					BootstrapFirstFramePresentedUsec = Time.GetTicksUsec();
					StartupLoadDiagnostics.Mark("loading.bootstrap_frame");
					GameplayResourceLoadRequestedUsec = Time.GetTicksUsec();
					ResourceManager.Instance.BeginGameplayAtlasPreload();
					GameSaveManager.Instance?.EnsureLoaded();
					ResourceManager.Instance.BeginLoad();
				}
			}
		}
		catch (Exception ex3)
		{
			if (GodotObject.IsInstanceValid(this) && IsInsideTree())
			{
				LoadFailed(ex3.GetBaseException().Message);
			}
		}
	}

	private async Task PrepareStartupInstall()
	{
		XWModPackageInstaller.CleanupInstallArtifacts();
		AndroidModInstallIntent.PrepareResult prepareResult = AndroidModInstallIntent.PrepareStartupIntent();
		if (prepareResult.RequiresConfirmation)
		{
			using AndroidModInstallIntent.PreparedStartupRequest request = prepareResult.Request;
			switch (await ConfirmStartupModInstall(request))
			{
			case ModInstallConfirmation.Confirmed:
			{
				AndroidModInstallIntent.StartupResult startupResult = AndroidModInstallIntent.InstallPreparedRequest(request);
				ReportStartupModImportResult(startupResult.Success, startupResult.Message);
				break;
			}
			case ModInstallConfirmation.Unavailable:
				if (GodotObject.IsInstanceValid(this) && IsInsideTree())
				{
					ReportStartupModImportResult(success: false, "无法显示 Mod 安装确认窗口，已取消本次安装。");
				}
				break;
			}
		}
		else if (prepareResult.Handled)
		{
			ReportStartupModImportResult(prepareResult.Success, prepareResult.Message);
		}
	}

	private void MountView(bool allowMod)
	{
		if (GodotObject.IsInstanceValid(_view))
		{
			_view.StartRequested -= ViewStartRequested;
			_view.ClearCacheRequested -= ShowClearLoadingCacheDialog;
			if (_view.GetParent() == this)
			{
				RemoveChild(_view);
			}
			_view.QueueFree();
		}
		_view = null;
		_viewOwner = "";
		if (!allowMod || _startup == null || !_startup.TryCreateView(XWLoadingKind.Startup, out _view, out _viewOwner))
		{
			_view = GD.Load<PackedScene>("res://Scene/Loading/LoadingDefaultView.tscn").Instantiate<LoadingDefaultView>(PackedScene.GenEditState.Disabled);
			if (!_view.TryBind(XWLoadingKind.Startup, out var error))
			{
				throw new InvalidOperationException(error);
			}
		}
		_view.Name = "LoadingView";
		_view.ProcessMode = ProcessModeEnum.Always;
		_view.StartRequested += ViewStartRequested;
		_view.ClearCacheRequested += ShowClearLoadingCacheDialog;
		AddChild(_view, forceReadableName: false, InternalMode.Disabled);
		GetNode<Control>("BootBackground").Hide();
		_view.ActivatePresentation();
		_view.PlayMotionAsync(enter: true);
		_lastState = null;
		PublishState();
	}

	private void PresentationChanged()
	{
		if (_viewInitialized && _viewOwner.Length > 0 && _startup.GetProvider(XWLoadingKind.Startup) != _viewOwner)
		{
			MountView(allowMod: false);
		}
	}

	public override void _Process(double delta)
	{
		if (!_viewInitialized)
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(_view) || !_view.IsInsideTree())
		{
			if (_viewOwner.Length > 0)
			{
				_startup?.DisableView(XWLoadingKind.Startup, "界面节点已意外退出。");
			}
			if (!GodotObject.IsInstanceValid(_view) || !_view.IsInsideTree())
			{
				MountView(allowMod: false);
			}
		}
		PublishState();
	}

	private void PublishState()
	{
		if (!GodotObject.IsInstanceValid(_view))
		{
			GetNodeOrNull<Label>("%BootStatus")?.Set("text", (_error.Length > 0) ? _error : _status);
			return;
		}
		XWLoadingState xWLoadingState = new XWLoadingState(XWLoadingKind.Startup, CanStart ? Tr("CLICK_TO_START") : _status, _percentage, _error, CanStart, CanStart || _error.Length > 0);
		if (!(xWLoadingState == _lastState))
		{
			_view.ApplyState(xWLoadingState);
			_lastState = xWLoadingState;
		}
	}

	public void SetPercentage(double percentage, string stepName, string resourceName, int resourceIndex, int resourceTotal)
	{
		_percentage = percentage;
		_status = BuildLoadingText(stepName, resourceName, resourceIndex, resourceTotal);
		PublishState();
	}

	public void LoadOver()
	{
		if (_applyingMods)
		{
			return;
		}
		_applyingMods = true;
		_loadCompleted = true;
		_percentage = 1.0;
		_status = "正在加载 Mod";
		PublishState();
		ThumbnailBinaryResourceCache.ClearOutdatedVersionCaches();
		ThumbnailBinaryResourceCache.ClearLegacyCharacterThumbnailCategories();
		try
		{
			XWModStartupContext startup = _startup;
			if (startup != null && !startup.Applied)
			{
				_startupReport = _startup.Apply();
			}
			if (_startupReport == null)
			{
				_startupReport = new XWModStartupReport
				{
					LegacyCount = 0
				};
			}
			GD.Print($"[Loading] loaded {_startupReport.LegacyCount} enabled Mod package(s)");
		}
		catch (Exception ex)
		{
			GD.PushWarning("[Loading] Mod 启动应用失败：" + ex.GetBaseException().Message);
			_startupReport = new XWModStartupReport
			{
				LegacyCount = -1,
				BlockingReason = ex.GetBaseException().Message
			};
			if (_viewOwner.Length > 0)
			{
				_startup?.DisableView(XWLoadingKind.Startup, "正式应用失败。");
			}
		}
		finally
		{
			_startupModsReady = true;
			InternetServerManager.Instance?.CheckStartupVersion();
			PublishState();
			if (_startupImportSuccess.HasValue)
			{
				goto IL_0182;
			}
			XWModStartupReport startupReport = _startupReport;
			if (startupReport != null && startupReport.HasFailures)
			{
				goto IL_0182;
			}
			XWModStartupContext startup2 = _startup;
			if (startup2 != null && startup2.Manager.IsSafeMode)
			{
				goto IL_0182;
			}
			goto end_IL_012b;
			IL_0182:
			CallDeferred("ShowStartupModImportResult", _startupImportSuccess ?? true, _startupImportMessage);
			end_IL_012b:;
		}
		AudioManager.Instance.AudioPlay("MainMenu", AudioManagerEnum.TYPE.MUSIC);
	}

	private void LoadFailed(string error)
	{
		_loadCompleted = false;
		_error = error;
		PublishState();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (CanStart && Input.IsActionJustPressed("Press") && !IsClearCacheDialogVisible() && !_view.BlocksStart(inputEvent))
		{
			AudioManager.Instance.AudioPlay("ButtonPress");
			OfflineButtonPressed();
		}
	}

	private bool IsClearCacheDialogVisible()
	{
		if (GodotObject.IsInstanceValid(_clearCacheDialog))
		{
			return _clearCacheDialog.Visible;
		}
		return false;
	}

	private void ViewStartRequested(bool online)
	{
		if (online)
		{
			OnlineButtonPressed();
		}
		else
		{
			OfflineButtonPressed();
		}
	}

	public void OnlineButtonPressed()
	{
		OfflineButtonPressed();
	}

	public void OfflineButtonPressed()
	{
		if (CanStart && !IsClearCacheDialogVisible())
		{
			ResourceManager.Instance.RequireFullGameplayResourcesReady("OfflineButtonPressed");
			_isStart = true;
			GameSaveManager.Instance.EnsureLoaded();
			PublishState();
			SceneManager.Instance.ChangeScene("MainMenu", stopAllAudio: false);
		}
	}

	public override void _ExitTree()
	{
		if (_startup != null)
		{
			_startup.PresentationChanged -= PresentationChanged;
		}
		if (GodotObject.IsInstanceValid(_view))
		{
			_view.StartRequested -= ViewStartRequested;
			_view.ClearCacheRequested -= ShowClearLoadingCacheDialog;
		}
		if (ResourceManager.Instance != null)
		{
			ResourceManager.Instance.OnLoadPercentage -= SetPercentage;
			ResourceManager.Instance.OnLoadOver -= LoadOver;
			ResourceManager.Instance.OnLoadFailed -= LoadFailed;
		}
	}

	private static string EscapeBbcode(string value)
	{
		return (value ?? "").Replace("[", "［", StringComparison.Ordinal).Replace("]", "］", StringComparison.Ordinal);
	}

	private async Task<ModInstallConfirmation> ConfirmStartupModInstall(AndroidModInstallIntent.PreparedStartupRequest request)
	{
		SceneTree tree = GetTree();
		await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		if (!GodotObject.IsInstanceValid(this) || !IsInsideTree() || !GodotObject.IsInstanceValid(DialogManager.Instance))
		{
			return ModInstallConfirmation.Unavailable;
		}
		DialogBoxChoose dialog = DialogManager.Instance.DialogCreate("DialogBoxChoose") as DialogBoxChoose;
		if (!GodotObject.IsInstanceValid(dialog))
		{
			return ModInstallConfirmation.Unavailable;
		}
		TaskCompletionSource<ModInstallConfirmation> completion = new TaskCompletionSource<ModInstallConfirmation>();
		XWModInstallChoices choices = XWModInstallChoices.MountStartup(dialog, request);
		dialog.OnChooseTrue += Confirmed;
		dialog.OnChooseFalse += Cancelled;
		dialog.OnClose += DialogClosed;
		dialog.TreeExited += DialogExited;
		base.TreeExited += LoadingExited;
		try
		{
			return await completion.Task;
		}
		finally
		{
			if (GodotObject.IsInstanceValid(this))
			{
				base.TreeExited -= LoadingExited;
			}
			if (GodotObject.IsInstanceValid(dialog))
			{
				dialog.OnChooseTrue -= Confirmed;
				dialog.OnChooseFalse -= Cancelled;
				dialog.OnClose -= DialogClosed;
				dialog.TreeExited -= DialogExited;
			}
		}
		void Cancelled()
		{
			completion.TrySetResult(ModInstallConfirmation.Cancelled);
		}
		void Confirmed()
		{
			request.Selection = choices.Plan;
			completion.TrySetResult(ModInstallConfirmation.Confirmed);
		}
		void DialogClosed()
		{
			completion.TrySetResult(ModInstallConfirmation.Cancelled);
		}
		void DialogExited()
		{
			completion.TrySetResult(ModInstallConfirmation.Unavailable);
		}
		void LoadingExited()
		{
			completion.TrySetResult(ModInstallConfirmation.Unavailable);
		}
	}

	private static string SafeDialogField(string value, int maxLength)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = value ?? "";
		foreach (char c in text)
		{
			if (c == '\n')
			{
				stringBuilder.Append('\n');
				continue;
			}
			UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if (char.IsControl(c) || unicodeCategory == UnicodeCategory.Format)
			{
				stringBuilder.Append(' ');
			}
			else
			{
				stringBuilder.Append(char.IsWhiteSpace(c) ? ' ' : c);
			}
		}
		string text2 = string.Join(" ", stringBuilder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)).Replace(" \n", "\n", StringComparison.Ordinal).Replace("\n ", "\n", StringComparison.Ordinal)
			.Trim();
		if (text2.Length > maxLength)
		{
			int num = maxLength;
			if (num > 0 && char.IsHighSurrogate(text2[num - 1]) && char.IsLowSurrogate(text2[num]))
			{
				num--;
			}
			text2 = text2.Substring(0, num) + "…";
		}
		return EscapeBbcode(text2);
	}

	private void ReportStartupModImportResult(bool success, string message)
	{
		if (success)
		{
			GD.Print("[Loading] Android Mod import completed: " + message);
		}
		else
		{
			GD.PushWarning("[Loading] Android Mod import rejected: " + message);
		}
		_startupImportSuccess = success;
		_startupImportMessage = message;
	}

	private void ShowStartupModImportResult(bool success, string message)
	{
		if (DialogManager.Instance == null)
		{
			return;
		}
		DialogBoxTips dialog = DialogManager.Instance.DialogCreate("DialogBoxTips") as DialogBoxTips;
		if (dialog == null)
		{
			return;
		}
		XWModStartupContext startup = _startup;
		int num;
		object obj;
		if (startup == null)
		{
			num = 0;
		}
		else
		{
			num = (startup.Manager.IsSafeMode ? 1 : 0);
			if (num != 0)
			{
				obj = "Mod 安全模式";
				goto IL_0076;
			}
		}
		obj = ((!_startupImportSuccess.HasValue) ? "Mod 加载结果" : (success ? "Mod 安装完成" : "Mod 安装失败"));
		goto IL_0076;
		IL_0076:
		string value = (string)obj;
		string value2 = ((!success) ? (message + "\n") : "") + (_startupReport?.PlayerSummary() ?? "加载结果暂不可用，请查看 Mod 管理。");
		string value3 = ((num != 0) ? "本次未加载 Mod，原启用配置已保留。\n\n退出安全模式：\n1. 点击下方“查看 Mod 管理”。\n2. 更多 → 加载与恢复 → 下次重新尝试。\n3. 完全退出游戏后重新打开（仅下次启动生效）。\n\n若再次进入安全模式，可选择“恢复上次成功配置”后重启。\n\n加载详情：\n" : "");
		dialog.text = $"[center][font_size=24]{value}[/font_size][/center]\n[font_size=20]{value3}{SafeDialogField(value2, 1600)}[/font_size]";
		LayoutStartupModResultDialog(dialog);
		dialog.Resized += delegate
		{
			LayoutStartupModResultDialog(dialog);
		};
		if (num == 0)
		{
			XWModStartupReport startupReport = _startupReport;
			if (startupReport == null || !startupReport.HasFailures)
			{
				return;
			}
		}
		dialog.GetNode<BaseButton>("%ConfirmButton").Set("text", "查看 Mod 管理");
		dialog.OnClose += delegate
		{
			if (DialogManager.Instance.DialogCreate("MainMenuOption") is DialogMainMenuOption dialogMainMenuOption)
			{
				dialogMainMenuOption.AllowModNavigation = false;
				Callable.From(dialogMainMenuOption.OpenManagement).CallDeferred();
			}
		};
	}

	private static void LayoutStartupModResultDialog(DialogBoxTips dialog)
	{
		Vector2 size = dialog.GetViewportRect().Size;
		Vector2 vector = new Vector2(Mathf.Min(720f, size.X - 48f), Mathf.Min(480f, size.Y - 100f));
		Control node = dialog.GetNode<Control>("Layer/Control");
		node.SetAnchorsAndOffsetsPreset(LayoutPreset.Center, LayoutPresetMode.Minsize);
		node.OffsetLeft = (0f - vector.X) / 2f;
		node.OffsetTop = (0f - vector.Y) / 2f;
		node.OffsetRight = vector.X / 2f;
		node.OffsetBottom = vector.Y / 2f;
		node.GetNode<Control>("MarginContainer2").SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		RichTextLabel node2 = dialog.GetNode<RichTextLabel>("%TextLabel");
		node2.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		node2.FitContent = false;
		node2.ScrollActive = true;
		node2.MouseFilter = MouseFilterEnum.Stop;
		node2.CustomMinimumSize = new Vector2(vector.X - 90f, vector.Y - 128f);
		Control node3 = node.GetNode<Control>("ButtonContainer");
		node3.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterBottom, LayoutPresetMode.Minsize);
		node3.OffsetLeft = (0f - vector.X) / 2f + 40f;
		node3.OffsetRight = vector.X / 2f - 50f;
		node3.OffsetTop = -70f;
		node3.OffsetBottom = -24f;
	}

	private void ShowClearLoadingCacheDialog()
	{
		if (!IsClearCacheDialogVisible() && GodotObject.IsInstanceValid(DialogManager.Instance))
		{
			_clearCacheDialog = (DialogBoxChoose)DialogManager.Instance.DialogCreate("DialogBoxChoose");
			_clearCacheDialog.text = BuildClearCacheDialogText();
			_clearCacheDialog.OnChooseTrue += ClearLoadingCacheConfirmed;
		}
	}

	private string BuildClearCacheDialogText()
	{
		return $"[center][font_size=24]{Tr("CLEAR_LOADING_CACHE")}[/font_size][/center]\n[center][font_size=16]{Tr("CLEAR_LOADING_CACHE_CONFIRM")}[/font_size][/center]";
	}

	private void ClearLoadingCacheConfirmed()
	{
		int value = CharacterBinaryResourceCache.ClearAllCaches();
		int value2 = ClearLoadedMapResourceCaches();
		GD.Print($"Loading cache cleared: characterBinaryEntries={value}, mapConfigs={value2}");
		if (GodotObject.IsInstanceValid(_view))
		{
			_view.ShowCacheCleared();
		}
	}

	private int ClearLoadedMapResourceCaches()
	{
		if (ResourceManager.Instance == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Resource value in ResourceManager.Instance.MAPS.Values)
		{
			if (value is TowerDefenseMapConfig towerDefenseMapConfig)
			{
				towerDefenseMapConfig.ClearLoadedMapResources(clearThumbnail: true);
				num++;
			}
		}
		return num;
	}

	private string BuildLoadingText(string stepName, string resourceName, int resourceIndex, int resourceTotal)
	{
		string text = TranslateLoadingStep(stepName);
		if (!string.IsNullOrEmpty(resourceName))
		{
			text = text + ": " + TranslateLoadingStep(resourceName);
		}
		if (resourceTotal > 0)
		{
			text += $" {resourceIndex}/{resourceTotal}";
		}
		return text;
	}

	private string TranslateLoadingStep(string stepName)
	{
		return TranslationServer.Translate(stepName);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(26)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartResourceLoadAfterFirstDraw, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.MountView, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "allowMod", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PresentationChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PublishState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetPercentage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "percentage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "stepName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "resourceName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "resourceIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "resourceTotal", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.LoadOver, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.LoadFailed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "error", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName.IsClearCacheDialogVisible, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ViewStartRequested, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "online", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnlineButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OfflineButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EscapeBbcode, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SafeDialogField, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "maxLength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ReportStartupModImportResult, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "success", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "message", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ShowStartupModImportResult, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "success", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "message", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.LayoutStartupModResultDialog, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "dialog", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.ShowClearLoadingCacheDialog, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BuildClearCacheDialogText, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ClearLoadingCacheConfirmed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ClearLoadedMapResourceCaches, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BuildLoadingText, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "stepName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "resourceName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "resourceIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "resourceTotal", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TranslateLoadingStep, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "stepName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
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
		if (method == MethodName.StartResourceLoadAfterFirstDraw && args.Count == 0)
		{
			StartResourceLoadAfterFirstDraw();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MountView && args.Count == 1)
		{
			MountView(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PresentationChanged && args.Count == 0)
		{
			PresentationChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PublishState && args.Count == 0)
		{
			PublishState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPercentage && args.Count == 5)
		{
			SetPercentage(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<string>(in args[2]), VariantUtils.ConvertTo<int>(in args[3]), VariantUtils.ConvertTo<int>(in args[4]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadOver && args.Count == 0)
		{
			LoadOver();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadFailed && args.Count == 1)
		{
			LoadFailed(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsClearCacheDialogVisible && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsClearCacheDialogVisible());
			return true;
		}
		if (method == MethodName.ViewStartRequested && args.Count == 1)
		{
			ViewStartRequested(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnlineButtonPressed && args.Count == 0)
		{
			OnlineButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OfflineButtonPressed && args.Count == 0)
		{
			OfflineButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EscapeBbcode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(EscapeBbcode(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.SafeDialogField && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string>(SafeDialogField(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.ReportStartupModImportResult && args.Count == 2)
		{
			ReportStartupModImportResult(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowStartupModImportResult && args.Count == 2)
		{
			ShowStartupModImportResult(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LayoutStartupModResultDialog && args.Count == 1)
		{
			LayoutStartupModResultDialog(VariantUtils.ConvertTo<DialogBoxTips>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowClearLoadingCacheDialog && args.Count == 0)
		{
			ShowClearLoadingCacheDialog();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BuildClearCacheDialogText && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(BuildClearCacheDialogText());
			return true;
		}
		if (method == MethodName.ClearLoadingCacheConfirmed && args.Count == 0)
		{
			ClearLoadingCacheConfirmed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearLoadedMapResourceCaches && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(ClearLoadedMapResourceCaches());
			return true;
		}
		if (method == MethodName.BuildLoadingText && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<string>(BuildLoadingText(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<int>(in args[3])));
			return true;
		}
		if (method == MethodName.TranslateLoadingStep && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(TranslateLoadingStep(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.EscapeBbcode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(EscapeBbcode(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.SafeDialogField && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string>(SafeDialogField(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.LayoutStartupModResultDialog && args.Count == 1)
		{
			LayoutStartupModResultDialog(VariantUtils.ConvertTo<DialogBoxTips>(in args[0]));
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
		if (method == MethodName.StartResourceLoadAfterFirstDraw)
		{
			return true;
		}
		if (method == MethodName.MountView)
		{
			return true;
		}
		if (method == MethodName.PresentationChanged)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.PublishState)
		{
			return true;
		}
		if (method == MethodName.SetPercentage)
		{
			return true;
		}
		if (method == MethodName.LoadOver)
		{
			return true;
		}
		if (method == MethodName.LoadFailed)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.IsClearCacheDialogVisible)
		{
			return true;
		}
		if (method == MethodName.ViewStartRequested)
		{
			return true;
		}
		if (method == MethodName.OnlineButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OfflineButtonPressed)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.EscapeBbcode)
		{
			return true;
		}
		if (method == MethodName.SafeDialogField)
		{
			return true;
		}
		if (method == MethodName.ReportStartupModImportResult)
		{
			return true;
		}
		if (method == MethodName.ShowStartupModImportResult)
		{
			return true;
		}
		if (method == MethodName.LayoutStartupModResultDialog)
		{
			return true;
		}
		if (method == MethodName.ShowClearLoadingCacheDialog)
		{
			return true;
		}
		if (method == MethodName.BuildClearCacheDialogText)
		{
			return true;
		}
		if (method == MethodName.ClearLoadingCacheConfirmed)
		{
			return true;
		}
		if (method == MethodName.ClearLoadedMapResourceCaches)
		{
			return true;
		}
		if (method == MethodName.BuildLoadingText)
		{
			return true;
		}
		if (method == MethodName.TranslateLoadingStep)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.LoadingFirstFramePresentedUsec)
		{
			LoadingFirstFramePresentedUsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName.BootstrapFirstFramePresentedUsec)
		{
			BootstrapFirstFramePresentedUsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName.GameplayResourceLoadRequestedUsec)
		{
			GameplayResourceLoadRequestedUsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._view)
		{
			_view = VariantUtils.ConvertTo<XWModLoadingView>(in value);
			return true;
		}
		if (name == PropertyName._clearCacheDialog)
		{
			_clearCacheDialog = VariantUtils.ConvertTo<DialogBoxChoose>(in value);
			return true;
		}
		if (name == PropertyName._viewInitialized)
		{
			_viewInitialized = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._loadCompleted)
		{
			_loadCompleted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._startupModsReady)
		{
			_startupModsReady = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._applyingMods)
		{
			_applyingMods = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isStart)
		{
			_isStart = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._viewOwner)
		{
			_viewOwner = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._status)
		{
			_status = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._error)
		{
			_error = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._percentage)
		{
			_percentage = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._startupImportMessage)
		{
			_startupImportMessage = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ActiveView)
		{
			value = VariantUtils.CreateFrom<XWModLoadingView>(ActiveView);
			return true;
		}
		if (name == PropertyName.ModPresentationOwner)
		{
			value = VariantUtils.CreateFrom<string>(ModPresentationOwner);
			return true;
		}
		if (name == PropertyName.CanStart)
		{
			value = VariantUtils.CreateFrom<bool>(CanStart);
			return true;
		}
		ulong from;
		if (name == PropertyName.LoadingFirstFramePresentedUsec)
		{
			from = LoadingFirstFramePresentedUsec;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.BootstrapFirstFramePresentedUsec)
		{
			from = BootstrapFirstFramePresentedUsec;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.GameplayResourceLoadRequestedUsec)
		{
			from = GameplayResourceLoadRequestedUsec;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._view)
		{
			value = VariantUtils.CreateFrom(in _view);
			return true;
		}
		if (name == PropertyName._clearCacheDialog)
		{
			value = VariantUtils.CreateFrom(in _clearCacheDialog);
			return true;
		}
		if (name == PropertyName._viewInitialized)
		{
			value = VariantUtils.CreateFrom(in _viewInitialized);
			return true;
		}
		if (name == PropertyName._loadCompleted)
		{
			value = VariantUtils.CreateFrom(in _loadCompleted);
			return true;
		}
		if (name == PropertyName._startupModsReady)
		{
			value = VariantUtils.CreateFrom(in _startupModsReady);
			return true;
		}
		if (name == PropertyName._applyingMods)
		{
			value = VariantUtils.CreateFrom(in _applyingMods);
			return true;
		}
		if (name == PropertyName._isStart)
		{
			value = VariantUtils.CreateFrom(in _isStart);
			return true;
		}
		if (name == PropertyName._viewOwner)
		{
			value = VariantUtils.CreateFrom(in _viewOwner);
			return true;
		}
		if (name == PropertyName._status)
		{
			value = VariantUtils.CreateFrom(in _status);
			return true;
		}
		if (name == PropertyName._error)
		{
			value = VariantUtils.CreateFrom(in _error);
			return true;
		}
		if (name == PropertyName._percentage)
		{
			value = VariantUtils.CreateFrom(in _percentage);
			return true;
		}
		if (name == PropertyName._startupImportMessage)
		{
			value = VariantUtils.CreateFrom(in _startupImportMessage);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._view, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._clearCacheDialog, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._viewInitialized, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._loadCompleted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._startupModsReady, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._applyingMods, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._isStart, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._viewOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._status, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._error, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._percentage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._startupImportMessage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.ActiveView, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.ModPresentationOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanStart, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.LoadingFirstFramePresentedUsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.BootstrapFirstFramePresentedUsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.GameplayResourceLoadRequestedUsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.LoadingFirstFramePresentedUsec, Variant.From<ulong>(LoadingFirstFramePresentedUsec));
		info.AddProperty(PropertyName.BootstrapFirstFramePresentedUsec, Variant.From<ulong>(BootstrapFirstFramePresentedUsec));
		info.AddProperty(PropertyName.GameplayResourceLoadRequestedUsec, Variant.From<ulong>(GameplayResourceLoadRequestedUsec));
		info.AddProperty(PropertyName._view, Variant.From(in _view));
		info.AddProperty(PropertyName._clearCacheDialog, Variant.From(in _clearCacheDialog));
		info.AddProperty(PropertyName._viewInitialized, Variant.From(in _viewInitialized));
		info.AddProperty(PropertyName._loadCompleted, Variant.From(in _loadCompleted));
		info.AddProperty(PropertyName._startupModsReady, Variant.From(in _startupModsReady));
		info.AddProperty(PropertyName._applyingMods, Variant.From(in _applyingMods));
		info.AddProperty(PropertyName._isStart, Variant.From(in _isStart));
		info.AddProperty(PropertyName._viewOwner, Variant.From(in _viewOwner));
		info.AddProperty(PropertyName._status, Variant.From(in _status));
		info.AddProperty(PropertyName._error, Variant.From(in _error));
		info.AddProperty(PropertyName._percentage, Variant.From(in _percentage));
		info.AddProperty(PropertyName._startupImportMessage, Variant.From(in _startupImportMessage));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.LoadingFirstFramePresentedUsec, out var value))
		{
			LoadingFirstFramePresentedUsec = value.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName.BootstrapFirstFramePresentedUsec, out var value2))
		{
			BootstrapFirstFramePresentedUsec = value2.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName.GameplayResourceLoadRequestedUsec, out var value3))
		{
			GameplayResourceLoadRequestedUsec = value3.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._view, out var value4))
		{
			_view = value4.As<XWModLoadingView>();
		}
		if (info.TryGetProperty(PropertyName._clearCacheDialog, out var value5))
		{
			_clearCacheDialog = value5.As<DialogBoxChoose>();
		}
		if (info.TryGetProperty(PropertyName._viewInitialized, out var value6))
		{
			_viewInitialized = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._loadCompleted, out var value7))
		{
			_loadCompleted = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._startupModsReady, out var value8))
		{
			_startupModsReady = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._applyingMods, out var value9))
		{
			_applyingMods = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isStart, out var value10))
		{
			_isStart = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._viewOwner, out var value11))
		{
			_viewOwner = value11.As<string>();
		}
		if (info.TryGetProperty(PropertyName._status, out var value12))
		{
			_status = value12.As<string>();
		}
		if (info.TryGetProperty(PropertyName._error, out var value13))
		{
			_error = value13.As<string>();
		}
		if (info.TryGetProperty(PropertyName._percentage, out var value14))
		{
			_percentage = value14.As<double>();
		}
		if (info.TryGetProperty(PropertyName._startupImportMessage, out var value15))
		{
			_startupImportMessage = value15.As<string>();
		}
	}
}
