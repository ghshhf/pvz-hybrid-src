using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.Core;
using PVZHE.ModEditor.FileSystem;
using PVZHE.ModEditor.Issues.GUI;
using PVZHE.ModEditor.ModSystem;
using PVZHE.ModEditor.ModSystem.Overrides;
using PVZHE.ModEditor.ModSystem.References;
using PVZHE.ModEditor.ModSystem.Validation;
using PVZHE.ModEditor.Registry.BP;
using PVZHE.ModEditor.ResourceEditors;
using PVZHE.ModEditor.ScriptEditor;
using Container = Godot.Container;

namespace PVZHE.ModEditor.Tools.GUI;

[ScriptPath("res://addons/ModEditor/Tools/GUI/XWModToolsPanel.cs")]
public class XWModToolsPanel : PanelContainer
{
	private sealed class OverrideDiffRow
	{
		public XWOverrideDiff Diff;

		public XWOverrideDiff.Entry Entry;

		public string TargetPath = "";
	}

	private sealed class ReferenceGraphRow
	{
		public string SourcePath = "";

		public string TargetPath = "";

		public bool Missing;
	}

	private sealed record PlayerSettingsRow(VBoxContainer Row, Control Header, Control Input, Label Title, Button Reset, Label DisabledHint, Label Error);

	public new class MethodName : PanelContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName CloseManagementSession = "CloseManagementSession";

		public static readonly StringName TryCancelManagementConfirmation = "TryCancelManagementConfirmation";

		public static readonly StringName BindScene = "BindScene";

		public static readonly StringName ApplyScrollSafeLayout = "ApplyScrollSafeLayout";

		public static readonly StringName ConfigureScrollContainer = "ConfigureScrollContainer";

		public static readonly StringName ConfigureBoundedControl = "ConfigureBoundedControl";

		public static readonly StringName ConfigureBoundedLogControl = "ConfigureBoundedLogControl";

		public static readonly StringName RunReleaseCheck = "RunReleaseCheck";

		public static readonly StringName RunReferenceGraph = "RunReferenceGraph";

		public static readonly StringName UpdateReferenceGraphSummary = "UpdateReferenceGraphSummary";

		public static readonly StringName OpenReferenceGraphTarget = "OpenReferenceGraphTarget";

		public static readonly StringName RunScriptBuild = "RunScriptBuild";

		public static readonly StringName UpdateScriptBuildActionState = "UpdateScriptBuildActionState";

		public static readonly StringName OnModBuildFlightStateChanged = "OnModBuildFlightStateChanged";

		public static readonly StringName ShowOverrideDiff = "ShowOverrideDiff";

		public static readonly StringName ToggleOverrideDiffEntry = "ToggleOverrideDiffEntry";

		public static readonly StringName OpenOverrideDiffTarget = "OpenOverrideDiffTarget";

		public static readonly StringName OpenToolTarget = "OpenToolTarget";

		public static readonly StringName FormatGraphPath = "FormatGraphPath";

		public static readonly StringName StartSandbox = "StartSandbox";

		public static readonly StringName ShowImportHint = "ShowImportHint";

		public static readonly StringName SetupImportWizard = "SetupImportWizard";

		public static readonly StringName AddImportCategory = "AddImportCategory";

		public static readonly StringName SelectImportCategory = "SelectImportCategory";

		public static readonly StringName RunImportWizard = "RunImportWizard";

		public static readonly StringName OpenImportedResource = "OpenImportedResource";

		public static readonly StringName GetImportCategory = "GetImportCategory";

		public static readonly StringName UpdateImportSummary = "UpdateImportSummary";

		public static readonly StringName GetRelativeFolder = "GetRelativeFolder";

		public static readonly StringName ShowTemplateLibrary = "ShowTemplateLibrary";

		public static readonly StringName ShowLocalizationHint = "ShowLocalizationHint";

		public static readonly StringName RefreshLocalizationTable = "RefreshLocalizationTable";

		public static readonly StringName SaveLocalizationTable = "SaveLocalizationTable";

		public static readonly StringName PopulateLocalizationTree = "PopulateLocalizationTree";

		public static readonly StringName EditLocalizationEntry = "EditLocalizationEntry";

		public static readonly StringName ShowModManagerStatus = "ShowModManagerStatus";

		public static readonly StringName RefreshModList = "RefreshModList";

		public static readonly StringName ChooseModPackage = "ChooseModPackage";

		public static readonly StringName PrepareSelectedModPackage = "PrepareSelectedModPackage";

		public static readonly StringName EscapeBbcode = "EscapeBbcode";

		public static readonly StringName OnModSelected = "OnModSelected";

		public static readonly StringName GetSelectedModId = "GetSelectedModId";

		public static readonly StringName IsSelectedModEnabled = "IsSelectedModEnabled";

		public static readonly StringName SetSelectedModEnabled = "SetSelectedModEnabled";

		public static readonly StringName ConfirmDeleteSelectedMod = "ConfirmDeleteSelectedMod";

		public static readonly StringName SetupTemplateLibrary = "SetupTemplateLibrary";

		public static readonly StringName SelectTemplate = "SelectTemplate";

		public static readonly StringName ClearVisualChoiceChildren = "ClearVisualChoiceChildren";

		public static readonly StringName ResolveTemplateCategoryIcon = "ResolveTemplateCategoryIcon";

		public static readonly StringName ResolveImportCategoryIcon = "ResolveImportCategoryIcon";

		public static readonly StringName CreateTemplateFromSelection = "CreateTemplateFromSelection";

		public static readonly StringName SetTemplateStatus = "SetTemplateStatus";

		public static readonly StringName IsLikelyOverridePath = "IsLikelyOverridePath";

		public static readonly StringName FormatDiffValue = "FormatDiffValue";

		public static readonly StringName NormalizeProjectPath = "NormalizeProjectPath";

		public static readonly StringName SelectTab = "SelectTab";

		public static readonly StringName GetCurrentProjectPath = "GetCurrentProjectPath";

		public static readonly StringName SetResult = "SetResult";

		public static readonly StringName AddExperienceActions = "AddExperienceActions";

		public static readonly StringName AddSelectedExperienceActions = "AddSelectedExperienceActions";

		public static readonly StringName CanPlaySelectedMod = "CanPlaySelectedMod";

		public static readonly StringName RenderSelectedExperience = "RenderSelectedExperience";

		public static readonly StringName OpenPlayerHelp = "OpenPlayerHelp";

		public static readonly StringName AddProgressRecoveryActions = "AddProgressRecoveryActions";

		public static readonly StringName ConfirmProgressRepair = "ConfirmProgressRepair";

		public static readonly StringName AddPlayerManagementFilters = "AddPlayerManagementFilters";

		public static readonly StringName PlayerModFilterChanged = "PlayerModFilterChanged";

		public static readonly StringName ClearPlayerModFilters = "ClearPlayerModFilters";

		public static readonly StringName PlayerDetailWidth = "PlayerDetailWidth";

		public static readonly StringName AddPlayerManagementDetails = "AddPlayerManagementDetails";

		public static readonly StringName AddPlayerPendingBar = "AddPlayerPendingBar";

		public static readonly StringName RebuildPlayerManagementState = "RebuildPlayerManagementState";

		public static readonly StringName RenderPlayerModRows = "RenderPlayerModRows";

		public static readonly StringName RenderPlayerManagementDetails = "RenderPlayerManagementDetails";

		public static readonly StringName JumpPlayerMod = "JumpPlayerMod";

		public static readonly StringName UpdatePlayerManagementControls = "UpdatePlayerManagementControls";

		public static readonly StringName OpenPlayerPendingChanges = "OpenPlayerPendingChanges";

		public static readonly StringName SelectManagementMod = "SelectManagementMod";

		public static readonly StringName ConfigureManagementOnly = "ConfigureManagementOnly";

		public static readonly StringName ResizePlayerDirectory = "ResizePlayerDirectory";

		public static readonly StringName RenderPlayerSelection = "RenderPlayerSelection";

		public static readonly StringName UpdatePlayerActionState = "UpdatePlayerActionState";

		public static readonly StringName TryClosePlayerUtility = "TryClosePlayerUtility";

		public static readonly StringName ClosePlayerUtility = "ClosePlayerUtility";

		public static readonly StringName AddPlayerUtilityOverlay = "AddPlayerUtilityOverlay";

		public static readonly StringName OpenPlayerMore = "OpenPlayerMore";

		public static readonly StringName OpenPlayerDiagnostics = "OpenPlayerDiagnostics";

		public static readonly StringName OpenPlayerLoadPlan = "OpenPlayerLoadPlan";

		public static readonly StringName RestorePlayerManagementFocus = "RestorePlayerManagementFocus";

		public static readonly StringName UpdatePlayerTechnicalText = "UpdatePlayerTechnicalText";

		public static readonly StringName ShowManagementResult = "ShowManagementResult";

		public static readonly StringName RestartFromManagement = "RestartFromManagement";

		public static readonly StringName SetPlayerNotice = "SetPlayerNotice";

		public static readonly StringName VersionLabel = "VersionLabel";

		public static readonly StringName ShortPlayerMessage = "ShortPlayerMessage";

		public static readonly StringName PlayerLabel = "PlayerLabel";

		public static readonly StringName PlayerScroll = "PlayerScroll";

		public static readonly StringName ConfigurePlayerScrollContent = "ConfigurePlayerScrollContent";

		public static readonly StringName PlayerRule = "PlayerRule";

		public static readonly StringName CreatePlayerDialogContent = "CreatePlayerDialogContent";

		public static readonly StringName PlayerFlat = "PlayerFlat";

		public static readonly StringName CreatePlayerPaperFrame = "CreatePlayerPaperFrame";

		public static readonly StringName CreatePlayerManagementTheme = "CreatePlayerManagementTheme";

		public static readonly StringName UpdatePlayerSettingsAction = "UpdatePlayerSettingsAction";

		public static readonly StringName OpenPlayerSettings = "OpenPlayerSettings";

		public static readonly StringName SetPlayerSettingsEnabled = "SetPlayerSettingsEnabled";

		public static readonly StringName CreatePlayerSettingsIcon = "CreatePlayerSettingsIcon";

		public static readonly StringName StylePlayerSettingsControl = "StylePlayerSettingsControl";

		public static readonly StringName CreatePlayerSettingsTheme = "CreatePlayerSettingsTheme";

		public static readonly StringName PlayerSettingsCanFocus = "PlayerSettingsCanFocus";

		public static readonly StringName RequestClosePlayerSettings = "RequestClosePlayerSettings";

		public static readonly StringName DisposePlayerSettings = "DisposePlayerSettings";

		public static readonly StringName AddPlayerUpdateAction = "AddPlayerUpdateAction";

		public static readonly StringName RefreshPlayerUpdateCount = "RefreshPlayerUpdateCount";

		public static readonly StringName UnbindPlayerUpdates = "UnbindPlayerUpdates";

		public static readonly StringName OpenPlayerUpdates = "OpenPlayerUpdates";
	}

	public new class PropertyName : PanelContainer.PropertyName
	{
		public static readonly StringName ManagementOnly = "ManagementOnly";

		public static readonly StringName CanUseManagement = "CanUseManagement";

		public static readonly StringName IsScriptBuildRunning = "IsScriptBuildRunning";

		public static readonly StringName ManagementConfirmationHost = "ManagementConfirmationHost";

		public static readonly StringName SelectedManagementModId = "SelectedManagementModId";

		public static readonly StringName _toolTabs = "_toolTabs";

		public static readonly StringName _result = "_result";

		public static readonly StringName _modTree = "_modTree";

		public static readonly StringName _referenceGraphTree = "_referenceGraphTree";

		public static readonly StringName _overrideDiffTree = "_overrideDiffTree";

		public static readonly StringName _modSummary = "_modSummary";

		public static readonly StringName _referenceGraphSummary = "_referenceGraphSummary";

		public static readonly StringName _overrideDiffSummary = "_overrideDiffSummary";

		public static readonly StringName _templateOption = "_templateOption";

		public static readonly StringName _templateCardGrid = "_templateCardGrid";

		public static readonly StringName _templateName = "_templateName";

		public static readonly StringName _templatePreview = "_templatePreview";

		public static readonly StringName _templateStatus = "_templateStatus";

		public static readonly StringName _importCategoryOption = "_importCategoryOption";

		public static readonly StringName _importCategoryGrid = "_importCategoryGrid";

		public static readonly StringName _importSourcePath = "_importSourcePath";

		public static readonly StringName _importResultTree = "_importResultTree";

		public static readonly StringName _importSummary = "_importSummary";

		public static readonly StringName _localizationTree = "_localizationTree";

		public static readonly StringName _localizationSummary = "_localizationSummary";

		public static readonly StringName _runScriptBuildButton = "_runScriptBuildButton";

		public static readonly StringName _importModPackageButton = "_importModPackageButton";

		public static readonly StringName _deleteModPackageButton = "_deleteModPackageButton";

		public static readonly StringName _visibilitySignalBound = "_visibilitySignalBound";

		public static readonly StringName _managementClosed = "_managementClosed";

		public static readonly StringName _modConfirmation = "_modConfirmation";

		public static readonly StringName _playerPlay = "_playerPlay";

		public static readonly StringName _playerSelectedPlay = "_playerSelectedPlay";

		public static readonly StringName _playerInstructions = "_playerInstructions";

		public static readonly StringName _playerPlayHint = "_playerPlayHint";

		public static readonly StringName _playerSearch = "_playerSearch";

		public static readonly StringName _playerFilter = "_playerFilter";

		public static readonly StringName _playerClearFilter = "_playerClearFilter";

		public static readonly StringName _playerPendingView = "_playerPendingView";

		public static readonly StringName _playerRestart = "_playerRestart";

		public static readonly StringName _playerMatched = "_playerMatched";

		public static readonly StringName _playerPendingCount = "_playerPendingCount";

		public static readonly StringName _playerPendingDetails = "_playerPendingDetails";

		public static readonly StringName _playerPackagePath = "_playerPackagePath";

		public static readonly StringName _playerRestartReason = "_playerRestartReason";

		public static readonly StringName _playerPendingBar = "_playerPendingBar";

		public static readonly StringName _playerIssueList = "_playerIssueList";

		public static readonly StringName _updatingPlayerFilters = "_updatingPlayerFilters";

		public static readonly StringName _playerExactId = "_playerExactId";

		public static readonly StringName _playerOperationEvidence = "_playerOperationEvidence";

		public static readonly StringName _playerOrderChanged = "_playerOrderChanged";

		public static readonly StringName _playerColumns = "_playerColumns";

		public static readonly StringName _playerDirectory = "_playerDirectory";

		public static readonly StringName _playerEmpty = "_playerEmpty";

		public static readonly StringName _playerList = "_playerList";

		public static readonly StringName _playerListScroll = "_playerListScroll";

		public static readonly StringName _playerDetailScroll = "_playerDetailScroll";

		public static readonly StringName _playerName = "_playerName";

		public static readonly StringName _playerAuthor = "_playerAuthor";

		public static readonly StringName _playerDescription = "_playerDescription";

		public static readonly StringName _playerEnabled = "_playerEnabled";

		public static readonly StringName _playerRunning = "_playerRunning";

		public static readonly StringName _playerMode = "_playerMode";

		public static readonly StringName _playerVersions = "_playerVersions";

		public static readonly StringName _playerFailure = "_playerFailure";

		public static readonly StringName _playerNotice = "_playerNotice";

		public static readonly StringName _playerCounts = "_playerCounts";

		public static readonly StringName _playerToggle = "_playerToggle";

		public static readonly StringName _playerEmptyImport = "_playerEmptyImport";

		public static readonly StringName _playerReturn = "_playerReturn";

		public static readonly StringName _playerMore = "_playerMore";

		public static readonly StringName _playerUtilityOverlay = "_playerUtilityOverlay";

		public static readonly StringName _playerTechnical = "_playerTechnical";

		public static readonly StringName _playerSnapshotDetails = "_playerSnapshotDetails";

		public static readonly StringName _playerOperationDetails = "_playerOperationDetails";

		public static readonly StringName _playerSnapshotNotice = "_playerSnapshotNotice";

		public static readonly StringName _playerSnapshotNoticeColor = "_playerSnapshotNoticeColor";

		public static readonly StringName _playerSettings = "_playerSettings";

		public static readonly StringName _playerSettingsSearch = "_playerSettingsSearch";

		public static readonly StringName _playerSettingsGroup = "_playerSettingsGroup";

		public static readonly StringName _updatingPlayerSettings = "_updatingPlayerSettings";

		public static readonly StringName _playerUpdates = "_playerUpdates";

		public static readonly StringName _updateNoticesBound = "_updateNoticesBound";

		public static readonly StringName _playerUtilityIsUpdates = "_playerUtilityIsUpdates";
	}

	public new class SignalName : PanelContainer.SignalName
	{
	}

	private readonly XWTemplateLibrary _templateLibrary = new XWTemplateLibrary();

	private readonly Dictionary<int, string> _templateIds = new Dictionary<int, string>();

	private readonly Dictionary<int, string> _importCategories = new Dictionary<int, string>();

	private readonly Dictionary<int, Button> _templateCards = new Dictionary<int, Button>();

	private readonly Dictionary<int, Button> _importCategoryCards = new Dictionary<int, Button>();

	private readonly List<OverrideDiffRow> _overrideDiffRows = new List<OverrideDiffRow>();

	private readonly List<ReferenceGraphRow> _referenceGraphRows = new List<ReferenceGraphRow>();

	private readonly List<XWImportWizard.ImportResult> _importResults = new List<XWImportWizard.ImportResult>();

	private readonly List<string> _localizationKeys = new List<string>();

	private XWLocalizationTable _localizationTable = new XWLocalizationTable();

	private TabContainer _toolTabs;

	private RichTextLabel _result;

	private Tree _modTree;

	private Tree _referenceGraphTree;

	private Tree _overrideDiffTree;

	private Label _modSummary;

	private Label _referenceGraphSummary;

	private Label _overrideDiffSummary;

	private OptionButton _templateOption;

	private HFlowContainer _templateCardGrid;

	private LineEdit _templateName;

	private TextEdit _templatePreview;

	private Label _templateStatus;

	private OptionButton _importCategoryOption;

	private HFlowContainer _importCategoryGrid;

	private LineEdit _importSourcePath;

	private Tree _importResultTree;

	private Label _importSummary;

	private Tree _localizationTree;

	private Label _localizationSummary;

	private Button _runScriptBuildButton;

	private Button _importModPackageButton;

	private Button _deleteModPackageButton;

	private XWModPackageInstaller.PreparedImport _pendingModImport;

	private Guid _pendingModImportToken;

	private SynchronizationContext _toolsSynchronizationContext;

	private CancellationTokenSource _buildLifetimeCts = new CancellationTokenSource();

	private bool _visibilitySignalBound;

	private bool _managementClosed;

	private Guid _filePickerToken;

	private Node _modConfirmation;

	private Action _cancelModConfirmation;

	private Button _playerPlay;

	private Button _playerSelectedPlay;

	private Label _playerInstructions;

	private Label _playerPlayHint;

	private XWModPlayerManagementState _playerManagement;

	private LineEdit _playerSearch;

	private OptionButton _playerFilter;

	private Button _playerClearFilter;

	private Button _playerPendingView;

	private Button _playerRestart;

	private Label _playerMatched;

	private Label _playerPendingCount;

	private Label _playerPendingDetails;

	private Label _playerPackagePath;

	private Label _playerRestartReason;

	private HFlowContainer _playerPendingBar;

	private VBoxContainer _playerIssueList;

	private bool _updatingPlayerFilters;

	private string _playerExactId;

	private string _playerOperationEvidence = "";

	private readonly List<XWValidationIssue> _playerOperationIssues = new List<XWValidationIssue>();

	private XWValidationIssue[] _playerCurrentIssues = Array.Empty<XWValidationIssue>();

	private bool _playerOrderChanged;

	private static readonly Color PlayerInk = new Color("513015");

	private static readonly Color PlayerMuted = new Color("80603b");

	private static readonly Color PlayerGreen = new Color("3f641e");

	private static readonly Color PlayerGold = new Color("89500e");

	private static readonly Color PlayerError = new Color("a23620");

	private readonly Dictionary<string, Button> _playerRows = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);

	private List<XWModManager.ModEntry> _playerEntries = new List<XWModManager.ModEntry>();

	private XWModManager.ModEntry _playerSelected;

	private HBoxContainer _playerColumns;

	private PanelContainer _playerDirectory;

	private CenterContainer _playerEmpty;

	private VBoxContainer _playerList;

	private ScrollContainer _playerListScroll;

	private ScrollContainer _playerDetailScroll;

	private Label _playerName;

	private Label _playerAuthor;

	private Label _playerDescription;

	private Label _playerEnabled;

	private Label _playerRunning;

	private Label _playerMode;

	private Label _playerVersions;

	private Label _playerFailure;

	private Label _playerNotice;

	private Label _playerCounts;

	private Button _playerToggle;

	private Button _playerEmptyImport;

	private Button _playerReturn;

	private Button _playerMore;

	private Control _playerUtilityOverlay;

	private RichTextLabel _playerTechnical;

	private string _playerSnapshotDetails = "";

	private string _playerOperationDetails = "";

	private string _playerSnapshotNotice = "";

	private Color _playerSnapshotNoticeColor;

	private Button _playerSettings;

	private XWModSettingsEditor _playerSettingsEditor;

	private readonly Dictionary<string, Func<object>> _playerSettingsReaders = new Dictionary<string, Func<object>>(StringComparer.Ordinal);

	private readonly Dictionary<string, Action<object>> _playerSettingsWriters = new Dictionary<string, Action<object>>(StringComparer.Ordinal);

	private XWModSettingsDraft _playerSettingsDraft;

	private readonly Dictionary<string, PlayerSettingsRow> _playerSettingsRows = new Dictionary<string, PlayerSettingsRow>(StringComparer.Ordinal);

	private readonly Dictionary<Control, FocusModeEnum> _playerSettingsFocus = new Dictionary<Control, FocusModeEnum>();

	private Action _refreshPlayerSettings;

	private LineEdit _playerSettingsSearch;

	private TabBar _playerSettingsGroup;

	private Action<bool> _clearPlayerSettingsFilters;

	private Action _rebuildPlayerSettingsFocus;

	private bool _updatingPlayerSettings;

	private Button _playerUpdates;

	private bool _updateNoticesBound;

	private bool _playerUtilityIsUpdates;

	[Export(PropertyHint.None, "")]
	public bool ManagementOnly { get; set; }

	private bool CanUseManagement
	{
		get
		{
			if (!_managementClosed && GodotObject.IsInstanceValid(this) && !IsQueuedForDeletion())
			{
				return IsInsideTree();
			}
			return false;
		}
	}

	public XWScriptCompiler.CompileResult LastScriptBuildResult { get; private set; }

	public bool IsScriptBuildRunning { get; private set; }

	internal Control ManagementConfirmationHost { get; set; }

	internal Action ManagementReturnRequested { get; set; }

	internal Action<string> ManagementPlayRequested { get; set; }

	internal string SelectedManagementModId => _playerSelected?.Id ?? "";

	internal Action ManagementCreateRequested { get; set; }

	public override void _Ready()
	{
		_toolsSynchronizationContext = SynchronizationContext.Current;
		if (ManagementOnly)
		{
			ConfigureManagementOnly();
			RefreshModList();
			return;
		}
		BindScene();
		ApplyScrollSafeLayout();
		XWModBuildSingleFlight.FlightStateChanged += OnModBuildFlightStateChanged;
		base.VisibilityChanged += UpdateScriptBuildActionState;
		_visibilitySignalBound = true;
		SetupTemplateLibrary();
		SetupImportWizard();
		RefreshModList();
		SetResult("选择一个 Mod 工具开始。");
		UpdateScriptBuildActionState();
	}

	public override void _ExitTree()
	{
		CloseManagementSession();
		CancellationTokenSource buildLifetimeCts = _buildLifetimeCts;
		_buildLifetimeCts = null;
		if (buildLifetimeCts != null)
		{
			try
			{
				buildLifetimeCts.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
			buildLifetimeCts.Dispose();
		}
		XWModBuildSingleFlight.FlightStateChanged -= OnModBuildFlightStateChanged;
		if (_visibilitySignalBound && IsConnected(CanvasItem.SignalName.VisibilityChanged, Callable.From(UpdateScriptBuildActionState)))
		{
			base.VisibilityChanged -= UpdateScriptBuildActionState;
		}
		_visibilitySignalBound = false;
		if (GodotObject.IsInstanceValid(_deleteModPackageButton) && _deleteModPackageButton.IsConnected(BaseButton.SignalName.Pressed, Callable.From(ConfirmDeleteSelectedMod)))
		{
			_deleteModPackageButton.Pressed -= ConfirmDeleteSelectedMod;
		}
		_toolsSynchronizationContext = null;
	}

	public void CloseManagementSession()
	{
		UnbindPlayerUpdates();
		_managementClosed = true;
		_filePickerToken = Guid.Empty;
		_cancelModConfirmation = null;
		if (GodotObject.IsInstanceValid(_modConfirmation))
		{
			_modConfirmation.QueueFree();
		}
		_modConfirmation = null;
		_pendingModImport?.Dispose();
		_pendingModImport = null;
		_pendingModImportToken = Guid.Empty;
		if (ManagementOnly)
		{
			ClosePlayerUtility(restoreFocus: false);
		}
	}

	public bool TryCancelManagementConfirmation()
	{
		if (_cancelModConfirmation == null)
		{
			return false;
		}
		_cancelModConfirmation();
		return true;
	}

	private void BindScene()
	{
		_toolTabs = GetNodeOrNull<TabContainer>("%ToolTabs");
		_result = GetNodeOrNull<RichTextLabel>("%Result");
		_modTree = GetNodeOrNull<Tree>("%ModTree");
		_referenceGraphTree = GetNodeOrNull<Tree>("%ReferenceGraphTree");
		_overrideDiffTree = GetNodeOrNull<Tree>("%OverrideDiffTree");
		_modSummary = GetNodeOrNull<Label>("%ModSummary");
		_referenceGraphSummary = GetNodeOrNull<Label>("%ReferenceGraphSummary");
		_overrideDiffSummary = GetNodeOrNull<Label>("%OverrideDiffSummary");
		_templateOption = GetNodeOrNull<OptionButton>("%TemplateOption");
		_templateCardGrid = GetNodeOrNull<HFlowContainer>("%TemplateCardGrid");
		_templateName = GetNodeOrNull<LineEdit>("%TemplateName");
		_templatePreview = GetNodeOrNull<TextEdit>("%TemplatePreview");
		_templateStatus = GetNodeOrNull<Label>("%TemplateStatus");
		_importCategoryOption = GetNodeOrNull<OptionButton>("%ImportCategoryOption");
		_importCategoryGrid = GetNodeOrNull<HFlowContainer>("%ImportCategoryGrid");
		_importSourcePath = GetNodeOrNull<LineEdit>("%ImportSourcePath");
		_importResultTree = GetNodeOrNull<Tree>("%ImportResultTree");
		_importSummary = GetNodeOrNull<Label>("%ImportSummary");
		_localizationTree = GetNodeOrNull<Tree>("%LocalizationTree");
		_localizationSummary = GetNodeOrNull<Label>("%LocalizationSummary");
		ConnectButton("%RefreshMods", RefreshModList);
		_importModPackageButton = GetNodeOrNull<Button>("%ImportModPackage");
		if (_importModPackageButton != null)
		{
			_importModPackageButton.Visible = true;
			_importModPackageButton.Pressed += ChooseModPackage;
		}
		ConnectButton("%EnableMod", delegate
		{
			SetSelectedModEnabled(enabled: true);
		});
		ConnectButton("%DisableMod", delegate
		{
			SetSelectedModEnabled(enabled: false);
		});
		_deleteModPackageButton = GetNodeOrNull<Button>("%DeleteMod");
		if (_deleteModPackageButton != null)
		{
			_deleteModPackageButton.Pressed += ConfirmDeleteSelectedMod;
		}
		if (_modTree != null)
		{
			_modTree.Columns = 4;
			_modTree.ColumnTitlesVisible = true;
			_modTree.HideRoot = true;
			_modTree.SetColumnTitle(0, "启用");
			_modTree.SetColumnTitle(1, "Mod");
			_modTree.SetColumnTitle(2, "版本");
			_modTree.SetColumnTitle(3, "状态");
			_modTree.ItemSelected += OnModSelected;
			_modTree.ItemActivated += delegate
			{
				SetSelectedModEnabled(!IsSelectedModEnabled());
			};
		}
		if (ManagementOnly)
		{
			return;
		}
		ConnectButton("%RunReleaseCheck", RunReleaseCheck);
		_runScriptBuildButton = GetNodeOrNull<Button>("%RunScriptBuild");
		if (_runScriptBuildButton != null)
		{
			_runScriptBuildButton.Pressed += RunScriptBuild;
		}
		ConnectButton("%RunReferenceGraph", RunReferenceGraph);
		ConnectButton("%ShowOverrideDiff", ShowOverrideDiff);
		ConnectButton("%StartSandbox", StartSandbox);
		ConnectButton("%ShowImportHint", ShowImportHint);
		ConnectButton("%ShowLocalizationHint", ShowLocalizationHint);
		ConnectButton("%RunImportWizard", RunImportWizard);
		ConnectButton("%RefreshLocalizationTable", RefreshLocalizationTable);
		ConnectButton("%SaveLocalizationTable", SaveLocalizationTable);
		ConnectButton("%CreateTemplate", CreateTemplateFromSelection);
		if (_referenceGraphTree != null)
		{
			_referenceGraphTree.Columns = 4;
			_referenceGraphTree.ColumnTitlesVisible = true;
			_referenceGraphTree.HideRoot = true;
			_referenceGraphTree.SetColumnTitle(0, "状态");
			_referenceGraphTree.SetColumnTitle(1, "源资源");
			_referenceGraphTree.SetColumnTitle(2, "目标资源");
			_referenceGraphTree.SetColumnTitle(3, "反向引用");
			_referenceGraphTree.ItemActivated += OpenReferenceGraphTarget;
		}
		if (_overrideDiffTree != null)
		{
			_overrideDiffTree.Columns = 6;
			_overrideDiffTree.ColumnTitlesVisible = true;
			_overrideDiffTree.HideRoot = true;
			_overrideDiffTree.SetColumnTitle(0, "覆盖");
			_overrideDiffTree.SetColumnTitle(1, "分类");
			_overrideDiffTree.SetColumnTitle(2, "Key");
			_overrideDiffTree.SetColumnTitle(3, "属性");
			_overrideDiffTree.SetColumnTitle(4, "原版值");
			_overrideDiffTree.SetColumnTitle(5, "Mod 值");
			_overrideDiffTree.ItemEdited += ToggleOverrideDiffEntry;
			_overrideDiffTree.ItemActivated += OpenOverrideDiffTarget;
		}
		if (_importResultTree != null)
		{
			_importResultTree.Columns = 4;
			_importResultTree.ColumnTitlesVisible = true;
			_importResultTree.HideRoot = true;
			_importResultTree.SetColumnTitle(0, "状态");
			_importResultTree.SetColumnTitle(1, "Key");
			_importResultTree.SetColumnTitle(2, "源文件");
			_importResultTree.SetColumnTitle(3, "目标文件");
			_importResultTree.ItemActivated += OpenImportedResource;
		}
		if (_localizationTree != null)
		{
			_localizationTree.Columns = 4;
			_localizationTree.ColumnTitlesVisible = true;
			_localizationTree.HideRoot = true;
			_localizationTree.SetColumnTitle(0, "Key");
			_localizationTree.SetColumnTitle(1, "简体中文");
			_localizationTree.SetColumnTitle(2, "English");
			_localizationTree.SetColumnTitle(3, "状态");
			_localizationTree.ItemEdited += EditLocalizationEntry;
		}
		if (_templateOption != null)
		{
			_templateOption.ItemSelected += SelectTemplate;
		}
		if (_importCategoryOption != null)
		{
			_importCategoryOption.ItemSelected += delegate(long index)
			{
				SelectImportCategory((int)index);
			};
		}
	}

	private void ApplyScrollSafeLayout()
	{
		base.CustomMinimumSize = Vector2.Zero;
		base.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		base.SizeFlagsVertical = SizeFlags.ExpandFill;
		ConfigureBoundedControl(GetNodeOrNull<Control>("Root"), 0f);
		ConfigureScrollContainer(GetNodeOrNull<ScrollContainer>("Root/ToolTabs/检查"));
		ConfigureScrollContainer(GetNodeOrNull<ScrollContainer>("Root/ToolTabs/模板库"));
		ConfigureScrollContainer(GetNodeOrNull<ScrollContainer>("Root/ToolTabs/Mod 管理"));
		ConfigureScrollContainer(GetNodeOrNull<ScrollContainer>("Root/ToolTabs/其它"));
		ConfigureScrollContainer(GetNodeOrNull<ScrollContainer>("%ResultScroll"));
		ConfigureBoundedControl(_referenceGraphTree, 120f);
		ConfigureBoundedControl(_modTree, 120f);
		ConfigureBoundedControl(_templatePreview, 110f);
		ConfigureBoundedControl(_importResultTree, 100f);
		ConfigureBoundedControl(_localizationTree, 100f);
		ConfigureBoundedControl(_overrideDiffTree, 110f);
		ConfigureBoundedLogControl(_result, 96f);
	}

	private static void ConfigureScrollContainer(ScrollContainer scroll)
	{
		if (scroll != null)
		{
			scroll.CustomMinimumSize = Vector2.Zero;
			scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		}
	}

	private static void ConfigureBoundedControl(Control control, float minimumHeight)
	{
		if (control != null)
		{
			control.CustomMinimumSize = new Vector2(0f, minimumHeight);
			control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			control.SizeFlagsVertical = SizeFlags.ExpandFill;
		}
	}

	private static void ConfigureBoundedLogControl(RichTextLabel label, float minimumHeight)
	{
		if (label != null)
		{
			ConfigureBoundedControl(label, minimumHeight);
			label.FitContent = false;
			label.ScrollActive = true;
		}
	}

	private void ConnectButton(string uniquePath, Action action)
	{
		Button nodeOrNull = GetNodeOrNull<Button>(uniquePath);
		if (nodeOrNull != null)
		{
			nodeOrNull.Pressed += action;
		}
	}

	private async void RunReleaseCheck()
	{
		SelectTab(0);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		List<XWValidationIssue> list = await new XWModValidationService().ValidateProjectAsync(currentProjectPath, ((XWEditorInterface.Instance?.GetEditorPanel() as ModEditorPanel)?.GetCurrentProject())?.CaptureExportInfo());
		if (XWEditorInterface.Instance?.GetIssuePanel() is XWIssuePanel xWIssuePanel)
		{
			xWIssuePanel.LoadIssues(list);
			XWEditorInterface.Instance.FocusPanel("issues");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[b]发布检查[/b]");
		if (list.Count == 0)
		{
			stringBuilder.AppendLine("未发现问题。");
		}
		else
		{
			foreach (XWValidationIssue item in list)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
				handler.AppendLiteral("- ");
				handler.AppendFormatted(item.Code);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(item.Message);
				stringBuilder2.AppendLine(ref handler);
			}
		}
		SetResult(stringBuilder.ToString());
	}

	private void RunReferenceGraph()
	{
		SelectTab(0);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		XWReferenceGraphService.ReferenceGraph referenceGraph = new XWReferenceGraphService().BuildForProject(currentProjectPath);
		PopulateReferenceGraphTree(referenceGraph, currentProjectPath);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[b]引用图[/b]");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder2);
		handler.AppendLiteral("资源: ");
		handler.AppendFormatted(referenceGraph.References.Count);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
		handler.AppendLiteral("反向引用: ");
		handler.AppendFormatted(referenceGraph.ReverseReferences.Count);
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder2);
		handler.AppendLiteral("断链: ");
		handler.AppendFormatted(referenceGraph.MissingReferences.Count);
		stringBuilder5.AppendLine(ref handler);
		foreach (KeyValuePair<string, List<string>> reference in referenceGraph.References)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
			handler.AppendLiteral("- ");
			handler.AppendFormatted(Path.GetFileName(reference.Key));
			handler.AppendLiteral(" -> ");
			handler.AppendFormatted(reference.Value.Count);
			stringBuilder6.AppendLine(ref handler);
		}
		foreach (KeyValuePair<string, List<string>> missingReference in referenceGraph.MissingReferences)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
			handler.AppendLiteral("! ");
			handler.AppendFormatted(Path.GetFileName(missingReference.Key));
			handler.AppendLiteral(" -> ");
			handler.AppendFormatted(missingReference.Value.Count);
			stringBuilder7.AppendLine(ref handler);
		}
		SetResult(stringBuilder.ToString());
	}

	private void PopulateReferenceGraphTree(XWReferenceGraphService.ReferenceGraph graph, string projectPath)
	{
		_referenceGraphRows.Clear();
		if (_referenceGraphTree == null)
		{
			return;
		}
		_referenceGraphTree.Clear();
		TreeItem root = _referenceGraphTree.CreateItem();
		if (graph == null)
		{
			UpdateReferenceGraphSummary(0, 0, 0);
			return;
		}
		foreach (KeyValuePair<string, List<string>> reference in graph.References)
		{
			string key = reference.Key;
			foreach (string item in reference.Value)
			{
				List<string> value;
				bool missing = graph.MissingReferences.TryGetValue(key, out value) && value.Contains(item);
				AddReferenceGraphRow(root, key, item, missing, graph);
			}
		}
		foreach (KeyValuePair<string, List<string>> missingReference in graph.MissingReferences)
		{
			foreach (string item2 in missingReference.Value)
			{
				if (!graph.References.TryGetValue(missingReference.Key, out var value2) || !value2.Contains(item2))
				{
					AddReferenceGraphRow(root, missingReference.Key, item2, missing: true, graph);
				}
			}
		}
		UpdateReferenceGraphSummary(graph.References.Count, graph.ReverseReferences.Count, graph.MissingReferences.Count);
	}

	private void AddReferenceGraphRow(TreeItem root, string source, string target, bool missing, XWReferenceGraphService.ReferenceGraph graph)
	{
		int count = _referenceGraphRows.Count;
		_referenceGraphRows.Add(new ReferenceGraphRow
		{
			SourcePath = source,
			TargetPath = target,
			Missing = missing
		});
		TreeItem treeItem = _referenceGraphTree.CreateItem(root);
		treeItem.SetText(0, missing ? "断链" : "正常");
		treeItem.SetText(1, FormatGraphPath(source));
		treeItem.SetText(2, FormatGraphPath(target));
		treeItem.SetText(3, graph.ReverseReferences.TryGetValue(target, out var value) ? value.Count.ToString() : "0");
		treeItem.SetMetadata(0, count);
		if (missing)
		{
			treeItem.SetCustomColor(0, new Color(1f, 0.35f, 0.25f));
		}
	}

	private void UpdateReferenceGraphSummary(int references, int reverseReferences, int missingReferences)
	{
		if (_referenceGraphSummary != null)
		{
			_referenceGraphSummary.Text = $"正向资源: {references}    反向资源: {reverseReferences}    断链来源: {missingReferences}";
		}
	}

	private void OpenReferenceGraphTarget()
	{
		TreeItem treeItem = _referenceGraphTree?.GetSelected();
		if (treeItem == null)
		{
			return;
		}
		Variant metadata = treeItem.GetMetadata(0);
		if (metadata.VariantType != Variant.Type.Int)
		{
			return;
		}
		int num = metadata.AsInt32();
		if (num >= 0 && num < _referenceGraphRows.Count)
		{
			ReferenceGraphRow referenceGraphRow = _referenceGraphRows[num];
			string text = (string.IsNullOrWhiteSpace(referenceGraphRow.TargetPath) ? referenceGraphRow.SourcePath : referenceGraphRow.TargetPath);
			if (!OpenToolTarget(text))
			{
				SetResult("[b]引用图[/b]\n无法打开: " + text);
			}
		}
	}

	private async void RunScriptBuild()
	{
		if (IsScriptBuildRunning)
		{
			return;
		}
		SelectTab(0);
		string projectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(projectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		IsScriptBuildRunning = true;
		LastScriptBuildResult = null;
		UpdateScriptBuildActionState();
		SetResult("[b]脚本编译[/b]\n正在编译...");
		try
		{
			XWScriptCompiler.CompileResult compileResult = (LastScriptBuildResult = await XWScriptCompiler.CompileModProjectAsync(projectPath, _buildLifetimeCts?.Token ?? CancellationToken.None));
			if (!GodotObject.IsInstanceValid(this))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[b]脚本编译[/b]");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder2);
			handler.AppendLiteral("目标: ");
			handler.AppendFormatted(compileResult?.TargetName ?? Path.GetFileName(projectPath));
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder2);
			handler.AppendLiteral("输出: ");
			handler.AppendFormatted(Path.Combine(compileResult?.BuildDirectory ?? "", "bin"));
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(8, 2, stringBuilder2);
			handler.AppendLiteral("结果: ");
			handler.AppendFormatted((compileResult != null && compileResult.Success) ? "成功" : "失败");
			handler.AppendLiteral(" (");
			handler.AppendFormatted(compileResult?.ElapsedSeconds ?? 0.0, "0.00");
			handler.AppendLiteral("s)");
			stringBuilder5.AppendLine(ref handler);
			if (compileResult != null && compileResult.Diagnostics?.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("[b]诊断[/b]");
				foreach (XWCodeErrorChecker.ErrorData diagnostic in compileResult.Diagnostics)
				{
					string value = ((diagnostic.SeverityLevel == XWCodeErrorChecker.Severity.Warning) ? "警告" : "错误");
					string value2 = (string.IsNullOrWhiteSpace(diagnostic.FilePath) ? "" : diagnostic.FilePath.Replace('\\', '/').GetFile());
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(6, 5, stringBuilder2);
					handler.AppendLiteral("- ");
					handler.AppendFormatted(value);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(value2);
					handler.AppendLiteral(":");
					handler.AppendFormatted(diagnostic.Line + 1);
					handler.AppendLiteral(":");
					handler.AppendFormatted(diagnostic.Column + 1);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(diagnostic.Message);
					stringBuilder6.AppendLine(ref handler);
				}
			}
			else
			{
				stringBuilder.AppendLine("未产生诊断。");
			}
			SetResult(stringBuilder.ToString());
		}
		catch (Exception ex)
		{
			if (GodotObject.IsInstanceValid(this))
			{
				SetResult("[b]脚本编译[/b]\n启动失败: " + ex.GetBaseException().Message);
			}
		}
		finally
		{
			IsScriptBuildRunning = false;
			UpdateScriptBuildActionState();
		}
	}

	public void UpdateScriptBuildActionState()
	{
		if (GodotObject.IsInstanceValid(_runScriptBuildButton))
		{
			string currentProjectPath = GetCurrentProjectPath();
			bool flag = !string.IsNullOrWhiteSpace(currentProjectPath);
			bool flag2 = flag && XWScriptCompiler.IsModBuildBusy(currentProjectPath);
			XWEditorInterface instance = XWEditorInterface.Instance;
			bool flag3 = instance != null && instance.GetScriptEditor()?.HasLiveDebugSession == true;
			_runScriptBuildButton.Disabled = !flag || IsScriptBuildRunning || flag2 || flag3;
		}
	}

	private void OnModBuildFlightStateChanged(string projectRoot, bool _)
	{
		if (!string.Equals(XWModBuildSingleFlight.NormalizeProjectRoot(GetCurrentProjectPath()), XWModBuildSingleFlight.NormalizeProjectRoot(projectRoot), StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		_toolsSynchronizationContext?.Post(delegate
		{
			if (GodotObject.IsInstanceValid(this))
			{
				UpdateScriptBuildActionState();
			}
		}, null);
	}

	private void ShowOverrideDiff()
	{
		SelectTab(3);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			PopulateOverrideDiffTree(new List<XWOverrideDiff>(), currentProjectPath);
			SetResult("[b]差异覆盖[/b]\n请先打开 Mod 工程。");
			return;
		}
		XWModManifest xWModManifest = XWModManifest.Load(Path.Combine(currentProjectPath, "mod.json"));
		if (xWModManifest == null)
		{
			PopulateOverrideDiffTree(new List<XWOverrideDiff>(), currentProjectPath);
			SetResult("[b]差异覆盖[/b]\n缺少 mod.json，无法读取 overrides。");
			return;
		}
		List<XWOverrideDiff> list = BuildOverrideDiffs(currentProjectPath, xWModManifest);
		PopulateOverrideDiffTree(list, currentProjectPath);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[b]差异覆盖[/b]");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
		handler.AppendLiteral("覆盖资源: ");
		handler.AppendFormatted(list.Count);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
		handler.AppendLiteral("覆盖项: ");
		handler.AppendFormatted(_overrideDiffRows.Count);
		stringBuilder4.AppendLine(ref handler);
		stringBuilder.AppendLine("可在表格中逐项勾选是否启用覆盖，双击行可跳到 Mod 覆盖资源。");
		SetResult(stringBuilder.ToString());
	}

	private List<XWOverrideDiff> BuildOverrideDiffs(string projectPath, XWModManifest manifest)
	{
		List<XWOverrideDiff> list = new List<XWOverrideDiff>();
		if (manifest?.Overrides == null)
		{
			return list;
		}
		foreach (KeyValuePair<string, List<string>> @override in manifest.Overrides)
		{
			string text = @override.Key ?? "";
			foreach (string item in @override.Value ?? new List<string>())
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					string text2 = FindOverrideResourcePath(projectPath, manifest, text, item);
					XWOverrideDiff xWOverrideDiff = new XWOverrideDiff
					{
						Category = text,
						BaseKey = item,
						TargetKey = item
					};
					xWOverrideDiff.Add("resource", Variant.From<string>("内置 " + text + "/" + item), Variant.From<string>(string.IsNullOrWhiteSpace(text2) ? "未找到 Mod 覆盖资源" : text2), !string.IsNullOrWhiteSpace(text2));
					list.Add(xWOverrideDiff);
				}
			}
		}
		return list;
	}

	private void PopulateOverrideDiffTree(IReadOnlyList<XWOverrideDiff> diffs, string projectPath)
	{
		_overrideDiffRows.Clear();
		if (_overrideDiffTree == null)
		{
			return;
		}
		_overrideDiffTree.Clear();
		TreeItem parent = _overrideDiffTree.CreateItem();
		foreach (XWOverrideDiff diff in diffs)
		{
			foreach (XWOverrideDiff.Entry entry in diff.Entries)
			{
				int count = _overrideDiffRows.Count;
				string text = entry.ModValue.AsString();
				if (text.StartsWith("未找到"))
				{
					text = "";
				}
				_overrideDiffRows.Add(new OverrideDiffRow
				{
					Diff = diff,
					Entry = entry,
					TargetPath = text
				});
				TreeItem treeItem = _overrideDiffTree.CreateItem(parent);
				treeItem.SetCellMode(0, TreeItem.TreeCellMode.Check);
				treeItem.SetChecked(0, entry.Enabled);
				treeItem.SetEditable(0, enabled: true);
				treeItem.SetText(1, diff.Category);
				treeItem.SetText(2, diff.TargetKey);
				treeItem.SetText(3, entry.PropertyPath);
				treeItem.SetText(4, FormatDiffValue(entry.OriginalValue));
				treeItem.SetText(5, FormatDiffValue(entry.ModValue));
				treeItem.SetMetadata(0, count);
			}
		}
		if (_overrideDiffSummary != null)
		{
			_overrideDiffSummary.Text = ((diffs.Count == 0) ? "当前 Mod 没有声明 overrides。" : $"覆盖资源: {diffs.Count}    覆盖项: {_overrideDiffRows.Count}");
		}
	}

	private void ToggleOverrideDiffEntry()
	{
		TreeItem treeItem = _overrideDiffTree?.GetSelected();
		if (treeItem == null)
		{
			return;
		}
		Variant metadata = treeItem.GetMetadata(0);
		if (metadata.VariantType == Variant.Type.Int)
		{
			int num = metadata.AsInt32();
			if (num >= 0 && num < _overrideDiffRows.Count)
			{
				OverrideDiffRow overrideDiffRow = _overrideDiffRows[num];
				overrideDiffRow.Entry.Enabled = treeItem.IsChecked(0);
				overrideDiffRow.Diff.SetEnabled(overrideDiffRow.Entry.PropertyPath, overrideDiffRow.Entry.Enabled);
				SetResult($"[b]差异覆盖[/b]\n{overrideDiffRow.Diff.Category}/{overrideDiffRow.Diff.TargetKey} {overrideDiffRow.Entry.PropertyPath} 已{(overrideDiffRow.Entry.Enabled ? "启用" : "关闭")}覆盖。");
			}
		}
	}

	private void OpenOverrideDiffTarget()
	{
		TreeItem treeItem = _overrideDiffTree?.GetSelected();
		if (treeItem == null)
		{
			return;
		}
		Variant metadata = treeItem.GetMetadata(0);
		if (metadata.VariantType != Variant.Type.Int)
		{
			return;
		}
		int num = metadata.AsInt32();
		if (num >= 0 && num < _overrideDiffRows.Count)
		{
			string targetPath = _overrideDiffRows[num].TargetPath;
			if (string.IsNullOrWhiteSpace(targetPath))
			{
				SetResult("[b]差异覆盖[/b]\n这个覆盖项还没有对应的 Mod 资源文件。");
			}
			else if (!OpenToolTarget(targetPath))
			{
				SetResult("[b]差异覆盖[/b]\n无法打开: " + targetPath);
			}
		}
	}

	private static bool OpenToolTarget(string targetPath)
	{
		if (string.IsNullOrWhiteSpace(targetPath))
		{
			return false;
		}
		string text = NormalizeProjectPath(targetPath);
		if (Path.GetExtension(text).ToLowerInvariant() == ".cs")
		{
			XWScriptEditor xWScriptEditor = XWEditorInterface.Instance?.GetScriptEditor();
			if (xWScriptEditor == null)
			{
				return false;
			}
			if (!xWScriptEditor.TryOpenFileAt(text, 0, 0))
			{
				return false;
			}
			XWEditorInterface.Instance?.FocusPanel("script_editor");
			return true;
		}
		if (XWResourceEditorRegistry.TryOpenPath(text))
		{
			return true;
		}
		if (ResourceLoader.Exists(text))
		{
			Resource resource = ResourceLoader.Load<Resource>(text, null, ResourceLoader.CacheMode.Reuse);
			XWEditorInterface.Instance?.EditResource(resource, XWResourceEditContext.ForRoot(resource, text, "resource_editor"));
			return true;
		}
		return false;
	}

	private static string FormatGraphPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return "";
		}
		string text = NormalizeProjectPath(path).Replace('\\', '/');
		if (text.StartsWith("res://") || text.StartsWith("uid://"))
		{
			return text;
		}
		string fileName = Path.GetFileName(text);
		if (!string.IsNullOrWhiteSpace(fileName))
		{
			return fileName;
		}
		return text;
	}

	private async void StartSandbox()
	{
		SelectTab(3);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		string modId = Path.GetFileName(currentProjectPath);
		XWModManifest xWModManifest = XWModManifest.Load(Path.Combine(currentProjectPath, "mod.json"));
		if (!string.IsNullOrWhiteSpace(xWModManifest?.Id))
		{
			modId = xWModManifest.Id;
		}
		await new XWModSandboxSession().StartAsync(modId);
		SetResult("[b]测试沙盒[/b]\n" + modId + " 已临时启用，不会写入正式启用列表。");
	}

	private void ShowImportHint()
	{
		SelectTab(3);
		SetResult("[b]导入向导[/b]\n输入一个或多个本地文件路径，导入后会按扩展名复制到 Assets/Images、Assets/Audio、Assets/Fonts 等目录，生成 key，并写入 mod.json。");
	}

	private void SetupImportWizard()
	{
		if (_importCategoryOption != null)
		{
			_importCategories.Clear();
			_importCategoryCards.Clear();
			_importCategoryOption.Clear();
			ClearVisualChoiceChildren(_importCategoryGrid);
			AddImportCategory(0, "图片", "Images");
			AddImportCategory(1, "音效", "Sfx");
			AddImportCategory(2, "BGM", "BGM");
			AddImportCategory(3, "字体", "Fonts");
			AddImportCategory(4, "场景", "Scenes");
			AddImportCategory(5, "UI", "UI");
			_importCategoryOption.Select(0);
			SelectImportCategory(0);
		}
	}

	private void AddImportCategory(int index, string label, string folder)
	{
		_importCategoryOption.AddItem(label + " / " + folder, index);
		_importCategories[index] = folder;
		if (_importCategoryGrid != null)
		{
			Button button = new Button
			{
				Name = "ImportCategoryCard_" + folder,
				Text = label,
				TooltipText = "导入到 Assets/" + folder,
				ToggleMode = true,
				CustomMinimumSize = new Vector2(108f, 42f),
				Icon = ResourceLoader.Load<Texture2D>(ResolveImportCategoryIcon(folder), null, ResourceLoader.CacheMode.Reuse)
			};
			button.Pressed += delegate
			{
				SelectImportCategory(index);
			};
			_importCategoryGrid.AddChild(button, forceReadableName: false, InternalMode.Disabled);
			_importCategoryCards[index] = button;
		}
	}

	private void SelectImportCategory(int index)
	{
		if (!_importCategories.ContainsKey(index))
		{
			index = 0;
		}
		_importCategoryOption?.Select(index);
		foreach (KeyValuePair<int, Button> importCategoryCard in _importCategoryCards)
		{
			importCategoryCard.Value.SetPressedNoSignal(importCategoryCard.Key == index);
		}
	}

	private void RunImportWizard()
	{
		SelectTab(3);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		List<string> list = ParseImportSources(_importSourcePath?.Text ?? "");
		if (list.Count == 0)
		{
			UpdateImportSummary(0, 0);
			SetResult("[b]导入向导[/b]\n请输入要导入的文件路径；多个路径可以换行或用分号分隔。");
			return;
		}
		List<XWImportWizard.ImportResult> list2 = new XWImportWizard().ImportFilesAndUpdateManifest(list, currentProjectPath, GetImportCategory());
		PopulateImportResults(list2);
		int num = 0;
		foreach (XWImportWizard.ImportResult item in list2)
		{
			if (item.Success)
			{
				num++;
			}
		}
		SetResult($"[b]导入向导[/b]\n已导入: {num}/{list2.Count}\n{FormatImportTargetSummary(list2)}");
	}

	private void PopulateImportResults(IReadOnlyList<XWImportWizard.ImportResult> results)
	{
		_importResults.Clear();
		if (_importResultTree == null)
		{
			return;
		}
		_importResultTree.Clear();
		TreeItem parent = _importResultTree.CreateItem();
		int num = 0;
		foreach (XWImportWizard.ImportResult result in results)
		{
			int count = _importResults.Count;
			_importResults.Add(result);
			if (result.Success)
			{
				num++;
			}
			TreeItem treeItem = _importResultTree.CreateItem(parent);
			treeItem.SetText(0, result.Success ? "成功" : "失败");
			treeItem.SetText(1, result.ResourceKey);
			treeItem.SetText(2, FormatGraphPath(result.SourcePath));
			treeItem.SetText(3, result.Success ? FormatGraphPath(result.TargetPath) : result.Error);
			treeItem.SetMetadata(0, count);
			if (!result.Success)
			{
				treeItem.SetCustomColor(0, new Color(1f, 0.35f, 0.25f));
			}
		}
		UpdateImportSummary(num, results.Count - num, FormatImportTargetFolders(results));
	}

	private void OpenImportedResource()
	{
		TreeItem treeItem = _importResultTree?.GetSelected();
		if (treeItem == null)
		{
			return;
		}
		Variant metadata = treeItem.GetMetadata(0);
		if (metadata.VariantType != Variant.Type.Int)
		{
			return;
		}
		int num = metadata.AsInt32();
		if (num >= 0 && num < _importResults.Count)
		{
			XWImportWizard.ImportResult importResult = _importResults[num];
			if (importResult.Success && !OpenToolTarget(importResult.TargetPath))
			{
				SetResult("[b]导入向导[/b]\n无法打开: " + importResult.TargetPath);
			}
		}
	}

	private string GetImportCategory()
	{
		int key = _importCategoryOption?.Selected ?? 0;
		if (!_importCategories.TryGetValue(key, out var value))
		{
			return "Imported";
		}
		return value;
	}

	private void UpdateImportSummary(int success, int failed, string target = "Assets/*")
	{
		if (_importSummary != null)
		{
			_importSummary.Text = $"成功: {success}    失败: {failed}    目标: {target}";
		}
	}

	private string FormatImportTargetSummary(IReadOnlyList<XWImportWizard.ImportResult> results)
	{
		return "目标目录: " + FormatImportTargetFolders(results);
	}

	private string FormatImportTargetFolders(IReadOnlyList<XWImportWizard.ImportResult> results)
	{
		List<string> list = new List<string>();
		string currentProjectPath = GetCurrentProjectPath();
		foreach (XWImportWizard.ImportResult result in results)
		{
			if (result.Success)
			{
				string relativeFolder = GetRelativeFolder(XWImportWizard.ToModRelativePath(result.TargetPath, currentProjectPath));
				if (!string.IsNullOrWhiteSpace(relativeFolder) && !list.Contains(relativeFolder))
				{
					list.Add(relativeFolder);
				}
			}
		}
		if (list.Count != 0)
		{
			return string.Join(", ", list);
		}
		return "Assets/*";
	}

	private static string GetRelativeFolder(string relativePath)
	{
		if (string.IsNullOrWhiteSpace(relativePath))
		{
			return "";
		}
		string text = relativePath.Replace('\\', '/').Trim('/');
		int num = text.LastIndexOf('/');
		if (num > 0)
		{
			return text.Substring(0, num);
		}
		return "";
	}

	private void ShowTemplateLibrary()
	{
		SelectTab(1);
		XWTemplateLibrary xWTemplateLibrary = new XWTemplateLibrary();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[b]模板库[/b]");
		foreach (XWTemplateLibrary.TemplateInfo template in xWTemplateLibrary.Templates)
		{
			XWTemplatePresentation xWTemplatePresentation = XWTemplatePresentation.Resolve(template);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder2);
			handler.AppendLiteral("- ");
			handler.AppendFormatted(xWTemplatePresentation.CategoryLabel);
			handler.AppendLiteral("：");
			handler.AppendFormatted(xWTemplatePresentation.DisplayNameLabel);
			stringBuilder2.AppendLine(ref handler);
		}
		SetResult(stringBuilder.ToString());
	}

	private void ShowLocalizationHint()
	{
		SelectTab(3);
		RefreshLocalizationTable();
	}

	private void RefreshLocalizationTable()
	{
		SelectTab(3);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		_localizationTable = XWLocalizationTable.LoadFromProject(currentProjectPath);
		EnsureLocalizationRows(currentProjectPath, _localizationTable);
		PopulateLocalizationTree();
		List<string> missingTranslations = _localizationTable.GetMissingTranslations(XWLocalizationTable.DefaultLocales);
		SetResult($"[b]多语言[/b]\n文本 key: {_localizationTable.Entries.Count}\n缺失翻译: {missingTranslations.Count}");
	}

	private void SaveLocalizationTable()
	{
		SelectTab(3);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetResult("请先打开 Mod 工程。");
			return;
		}
		_localizationTable.SaveToProject(currentProjectPath, XWLocalizationTable.DefaultLocales);
		PopulateLocalizationTree();
		List<string> missingTranslations = _localizationTable.GetMissingTranslations(XWLocalizationTable.DefaultLocales);
		SetResult($"[b]多语言[/b]\n已保存到 Localization/translations.csv。\n缺失翻译: {missingTranslations.Count}");
	}

	private void PopulateLocalizationTree()
	{
		_localizationKeys.Clear();
		if (_localizationTree == null)
		{
			return;
		}
		_localizationTree.Clear();
		TreeItem parent = _localizationTree.CreateItem();
		List<string> list = new List<string>(_localizationTable.Entries.Keys);
		list.Sort(StringComparer.OrdinalIgnoreCase);
		foreach (string item in list)
		{
			XWLocalizationTable.Entry entry = _localizationTable.Entries[item];
			int count = _localizationKeys.Count;
			_localizationKeys.Add(item);
			TreeItem treeItem = _localizationTree.CreateItem(parent);
			treeItem.SetText(0, item);
			treeItem.SetText(1, entry.Values.TryGetValue("zh_CN", out var value) ? value : "");
			treeItem.SetText(2, entry.Values.TryGetValue("en_US", out var value2) ? value2 : "");
			treeItem.SetText(3, IsLocalizationEntryMissing(entry) ? "缺失" : "完整");
			treeItem.SetEditable(1, enabled: true);
			treeItem.SetEditable(2, enabled: true);
			treeItem.SetMetadata(0, count);
			if (IsLocalizationEntryMissing(entry))
			{
				treeItem.SetCustomColor(3, new Color(1f, 0.62f, 0.25f));
			}
		}
		List<string> missingTranslations = _localizationTable.GetMissingTranslations(XWLocalizationTable.DefaultLocales);
		if (_localizationSummary != null)
		{
			_localizationSummary.Text = $"Key: {_localizationTable.Entries.Count}    缺失: {missingTranslations.Count}    文件: Localization/*.csv";
		}
	}

	private void EditLocalizationEntry()
	{
		TreeItem treeItem = _localizationTree?.GetSelected();
		if (treeItem == null)
		{
			return;
		}
		Variant metadata = treeItem.GetMetadata(0);
		if (metadata.VariantType != Variant.Type.Int)
		{
			return;
		}
		int num = metadata.AsInt32();
		if (num < 0 || num >= _localizationKeys.Count)
		{
			return;
		}
		int selectedColumn = _localizationTree.GetSelectedColumn();
		string text = selectedColumn switch
		{
			1 => "zh_CN", 
			2 => "en_US", 
			_ => "", 
		};
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		string key = _localizationKeys[num];
		_localizationTable.Set(key, text, treeItem.GetText(selectedColumn));
		if (_localizationTable.Entries.TryGetValue(key, out var value))
		{
			treeItem.SetText(3, IsLocalizationEntryMissing(value) ? "缺失" : "完整");
			if (IsLocalizationEntryMissing(value))
			{
				treeItem.SetCustomColor(3, new Color(1f, 0.62f, 0.25f));
			}
			else
			{
				treeItem.ClearCustomColor(3);
			}
		}
		List<string> missingTranslations = _localizationTable.GetMissingTranslations(XWLocalizationTable.DefaultLocales);
		if (_localizationSummary != null)
		{
			_localizationSummary.Text = $"Key: {_localizationTable.Entries.Count}    缺失: {missingTranslations.Count}    未保存";
		}
	}

	private static bool IsLocalizationEntryMissing(XWLocalizationTable.Entry entry)
	{
		string[] defaultLocales = XWLocalizationTable.DefaultLocales;
		foreach (string key in defaultLocales)
		{
			if (!entry.Values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
			{
				return true;
			}
		}
		return false;
	}

	private static void EnsureLocalizationRows(string projectPath, XWLocalizationTable table)
	{
		if (table != null && !string.IsNullOrWhiteSpace(projectPath))
		{
			XWModManifest xWModManifest = XWModManifest.Load(Path.Combine(projectPath, "mod.json"));
			if (xWModManifest != null)
			{
				AddLocalizationRowsFromKeys(table, xWModManifest.Provides, "name");
				AddLocalizationRowsFromKeys(table, xWModManifest.Provides, "description");
				AddLocalizationRowsFromKeys(table, xWModManifest.Overrides, "name");
				AddLocalizationRowsFromKeys(table, xWModManifest.Overrides, "description");
			}
		}
	}

	private static void AddLocalizationRowsFromKeys(XWLocalizationTable table, Dictionary<string, List<string>> groups, string suffix)
	{
		if (groups == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<string>> group in groups)
		{
			string value = XWImportWizard.SanitizeKey(group.Key);
			foreach (string item in group.Value ?? new List<string>())
			{
				string value2 = XWImportWizard.SanitizeKey(item);
				if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(value2))
				{
					continue;
				}
				string key = $"{value}.{value2}.{suffix}";
				string[] defaultLocales = XWLocalizationTable.DefaultLocales;
				foreach (string text in defaultLocales)
				{
					if (!table.Entries.TryGetValue(key, out var value3) || !value3.Values.ContainsKey(text))
					{
						table.Set(key, text, "");
					}
				}
			}
		}
	}

	private static List<string> ParseImportSources(string text)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrWhiteSpace(text))
		{
			return list;
		}
		string[] array = text.Split(new char[3] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i].Trim().Trim('"');
			if (!string.IsNullOrWhiteSpace(text2))
			{
				list.Add(text2);
			}
		}
		return list;
	}

	private void ShowModConfirmation(string title, string message, string acceptText, Action confirmed, Action canceled = null, bool preservePlayerUtility = false, string cancelText = "取消", bool allowAccept = true, Action<VBoxContainer, Button> buildContent = null)
	{
		if (!CanUseManagement || _modConfirmation != null)
		{
			canceled?.Invoke();
			return;
		}
		Node presentation;
		if (ManagementOnly || buildContent != null)
		{
			if (!preservePlayerUtility)
			{
				ClosePlayerUtility(restoreFocus: false);
			}
			presentation = new ColorRect
			{
				Name = "ModConfirmation",
				Color = new Color(0.15f, 0.08f, 0.02f, 0.58f),
				MouseFilter = MouseFilterEnum.Stop
			};
		}
		else
		{
			presentation = new ConfirmationDialog
			{
				Title = title,
				DialogText = message,
				OkButtonText = acceptText
			};
		}
		_modConfirmation = presentation;
		UpdatePlayerActionState();
		_cancelModConfirmation = delegate
		{
			Finish(accept: false);
		};
		if (presentation is ConfirmationDialog confirmationDialog)
		{
			confirmationDialog.GetCancelButton().Text = cancelText;
			confirmationDialog.GetOkButton().Visible = allowAccept;
			confirmationDialog.Confirmed += delegate
			{
				Finish(accept: true);
			};
			confirmationDialog.Canceled += delegate
			{
				Finish(accept: false);
			};
			AddChild(confirmationDialog, forceReadableName: false, InternalMode.Disabled);
			confirmationDialog.PopupCentered(new Vector2I(620, 360));
			return;
		}
		ColorRect overlay = (ColorRect)presentation;
		(GodotObject.IsInstanceValid(ManagementConfirmationHost) ? ManagementConfirmationHost : this).AddChild(overlay, forceReadableName: false, InternalMode.Disabled);
		overlay.Theme = CreatePlayerManagementTheme();
		overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		VBoxContainer vBoxContainer = CreatePlayerDialogContent(overlay, title);
		ScrollContainer scroll = new ScrollContainer
		{
			CustomMinimumSize = new Vector2(0f, 64f),
			SizeFlagsVertical = SizeFlags.ExpandFill,
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
		};
		vBoxContainer.AddChild(scroll, forceReadableName: false, InternalMode.Disabled);
		Label label = PlayerLabel(message, 20, PlayerInk);
		label.Name = "ConfirmationMessage";
		VBoxContainer body = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		body.AddThemeConstantOverride("separation", 12);
		scroll.AddChild(body, forceReadableName: false, InternalMode.Disabled);
		body.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(label, overlay, () => Mathf.Min(640f, overlay.Size.X - 64f) - 76f);
		body.MinimumSizeChanged += FitMessage;
		overlay.Resized += FitMessage;
		Callable.From(FitMessage).CallDeferred();
		HBoxContainer hBoxContainer = new HBoxContainer
		{
			Alignment = BoxContainer.AlignmentMode.End
		};
		hBoxContainer.AddThemeConstantOverride("separation", 12);
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		Button button = new Button
		{
			Name = "Cancel",
			Text = cancelText,
			ThemeTypeVariation = "ModSecondaryButton",
			CustomMinimumSize = new Vector2(112f, 48f)
		};
		Button button2 = new Button
		{
			Name = "Confirm",
			Text = acceptText,
			Visible = allowAccept,
			CustomMinimumSize = new Vector2(112f, 48f)
		};
		hBoxContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(button2, forceReadableName: false, InternalMode.Disabled);
		buildContent?.Invoke(body, button2);
		NodePath focusNext = (button.FocusPrevious = button.GetPathTo(allowAccept ? button2 : button));
		button.FocusNext = focusNext;
		focusNext = (button.FocusNeighborRight = button.FocusNext);
		button.FocusNeighborLeft = focusNext;
		focusNext = (button.FocusNeighborBottom = button.GetPathTo(button));
		button.FocusNeighborTop = focusNext;
		focusNext = (button2.FocusPrevious = button2.GetPathTo(button));
		button2.FocusNext = focusNext;
		focusNext = (button2.FocusNeighborRight = button2.FocusNext);
		button2.FocusNeighborLeft = focusNext;
		focusNext = (button2.FocusNeighborBottom = button2.GetPathTo(button2));
		button2.FocusNeighborTop = focusNext;
		if (buildContent != null)
		{
			Control[] array = body.FindChildren("*", "BaseButton", recursive: true, owned: false).OfType<Control>().Concat(new Control[2] { button, button2 })
				.ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				Control obj = array[num];
				focusNext = (obj.FocusNeighborTop = obj.GetPathTo(array[(num + array.Length - 1) % array.Length]));
				obj.FocusPrevious = focusNext;
				focusNext = (obj.FocusNeighborBottom = obj.GetPathTo(array[(num + 1) % array.Length]));
				obj.FocusNext = focusNext;
			}
		}
		button.Pressed += delegate
		{
			Finish(accept: false);
		};
		button2.Pressed += delegate
		{
			Finish(accept: true);
		};
		button.GrabFocus();
		void Finish(bool accept)
		{
			if (_modConfirmation == presentation)
			{
				_modConfirmation = null;
				_cancelModConfirmation = null;
				if (GodotObject.IsInstanceValid(presentation))
				{
					if (presentation is CanvasItem canvasItem)
					{
						canvasItem.Hide();
					}
					if (presentation is Window window)
					{
						window.Hide();
					}
					presentation.QueueFree();
				}
				if (CanUseManagement)
				{
					UpdatePlayerActionState();
					if (ManagementOnly)
					{
						AudioManager.Instance?.AudioPlay("ButtonPress");
					}
					if (accept)
					{
						confirmed();
					}
					else
					{
						canceled?.Invoke();
					}
					if (ManagementOnly && CanUseManagement && _modConfirmation == null && !preservePlayerUtility)
					{
						RestorePlayerManagementFocus();
					}
				}
			}
		}
		void FitMessage()
		{
			if (_modConfirmation == presentation && GodotObject.IsInstanceValid(scroll))
			{
				float max = Mathf.Max(64f, overlay.Size.Y - 240f);
				scroll.CustomMinimumSize = new Vector2(0f, Mathf.Clamp(body.GetCombinedMinimumSize().Y, 64f, max));
			}
		}
	}

	private void ShowModManagerStatus()
	{
		SelectTab(2);
		RefreshModList();
	}

	private void RefreshModList()
	{
		if (!CanUseManagement)
		{
			return;
		}
		XWModManager xWModManager = new XWModManager();
		List<XWModManager.ModEntry> managementSnapshot = xWModManager.GetManagementSnapshot();
		IReadOnlyList<string> readOnlyList = xWModManager.LoadEnabledIds();
		List<XWValidationIssue> list = xWModManager.ValidateEnableSet(readOnlyList);
		if (ManagementOnly)
		{
			RefreshPlayerManagement(managementSnapshot, readOnlyList.Count, list, XWModEnvironmentService.GetStatus(), xWModManager.ModsDirectory);
			return;
		}
		PopulateModTree(managementSnapshot);
		if (!ManagementOnly)
		{
			UpdateIssuePanel(list);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[b]Mod 管理[/b]");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
		handler.AppendLiteral("安装目录: ");
		handler.AppendFormatted(xWModManager.ModsDirectory);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
		handler.AppendLiteral("已启用: ");
		handler.AppendFormatted(readOnlyList.Count);
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
		handler.AppendLiteral("已发现: ");
		handler.AppendFormatted(managementSnapshot.Count);
		stringBuilder5.AppendLine(ref handler);
		XWModEnvironmentStatus status = XWModEnvironmentService.GetStatus();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder2);
		handler.AppendLiteral("实际运行环境: ");
		handler.AppendFormatted(status.Ready ? "已应用" : "应用失败或尚未应用");
		stringBuilder6.AppendLine(ref handler);
		foreach (XWModManager.ModEntry item in managementSnapshot)
		{
			string effectiveState = item.EffectiveState;
			string value = (item.HasManifest ? "" : " (缺少 mod.json)");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(6, 4, stringBuilder2);
			handler.AppendLiteral("- [");
			handler.AppendFormatted(effectiveState);
			handler.AppendLiteral("] ");
			handler.AppendFormatted(item.DisplayName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted(item.Version);
			handler.AppendFormatted(value);
			stringBuilder7.AppendLine(ref handler);
			if (!string.IsNullOrWhiteSpace(item.LastApplyFailure))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
				handler.AppendLiteral("  最近失败：");
				handler.AppendFormatted(item.LastApplyFailure);
				stringBuilder8.AppendLine(ref handler);
			}
		}
		if (list.Count > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("[b]启用检查[/b]");
			foreach (XWValidationIssue item2 in list)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder9 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
				handler.AppendLiteral("- ");
				handler.AppendFormatted(item2.Code);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(item2.Message);
				stringBuilder9.AppendLine(ref handler);
			}
		}
		SetResult(stringBuilder.ToString());
		if (_modSummary != null)
		{
			_modSummary.Text = $"目录: {xWModManager.ModsDirectory}\n已发现: {managementSnapshot.Count}    已启用: {readOnlyList.Count}    问题: {list.Count}\n实际运行: {(status.Ready ? "已应用" : "应用失败或尚未应用")}";
			if (ManagementOnly)
			{
				_modSummary.Text = $"已安装/发现：{managementSnapshot.Count}    已启用：{readOnlyList.Count}    问题：{list.Count}    实际运行：{(status.Ready ? "已应用" : "应用失败或尚未应用")}";
			}
		}
	}

	private void ChooseModPackage()
	{
		if (!CanUseManagement || _filePickerToken != Guid.Empty || _modConfirmation != null)
		{
			return;
		}
		Guid request = Guid.NewGuid();
		_filePickerToken = request;
		_importModPackageButton.Disabled = true;
		UpdatePlayerActionState();
		Error error = DisplayServer.FileDialogShow("导入 Mod", "", "", showHidden: false, DisplayServer.FileDialogMode.OpenFile, new string[1] { "*.pmod ; Mod package" }, Callable.From(delegate(bool status, string[] paths, int filterIndex)
		{
			if (CanUseManagement && !(_filePickerToken != request))
			{
				_filePickerToken = Guid.Empty;
				_importModPackageButton.Disabled = false;
				UpdatePlayerActionState();
				PrepareSelectedModPackage(status, paths, filterIndex);
			}
		}));
		if (error != Error.Ok && CanUseManagement && _filePickerToken == request)
		{
			_filePickerToken = Guid.Empty;
			_importModPackageButton.Disabled = false;
			UpdatePlayerActionState();
			ShowManagementResult("无法导入", "系统文件选择器未能打开。", failure: true, error.ToString());
		}
	}

	private void PrepareSelectedModPackage(bool status, string[] paths, int selectedFilterIndex)
	{
		if (!CanUseManagement || !status || paths == null || paths.Length == 0)
		{
			return;
		}
		if (_pendingModImport != null)
		{
			ShowManagementResult("等待确认", "请先完成或取消当前导入。");
			return;
		}
		if (!XWModPackageInstaller.TryPrepare(paths[0], "", "", out var prepared, out var diagnostic))
		{
			ShowManagementResult("导入失败", "安装包未通过检查。", failure: true, diagnostic);
			return;
		}
		if (!CanUseManagement)
		{
			prepared.Dispose();
			return;
		}
		_pendingModImport = prepared;
		Guid importToken = Guid.NewGuid();
		_pendingModImportToken = importToken;
		_ = prepared.Manifest;
		XWModInstallChoices choices = null;
		ShowModConfirmation("确认" + XWModInstallChoices.Operation(prepared) + " Mod", XWModInstallChoices.Summary(prepared), "安装", delegate
		{
			InstallPreparedMod(prepared, importToken, choices.Plan);
		}, delegate
		{
			CancelPreparedMod(prepared, importToken);
		}, preservePlayerUtility: false, "取消", allowAccept: true, delegate(VBoxContainer body, Button accept)
		{
			choices = new XWModInstallChoices();
			body.AddChild(choices, forceReadableName: false, InternalMode.Disabled);
			choices.Configure(prepared, accept, body.GetParent<Control>());
		});
	}

	private void InstallPreparedMod(XWModPackageInstaller.PreparedImport prepared, Guid importToken, XWModManager.SelectionPlan selection)
	{
		if (!CanUseManagement || _pendingModImport != prepared || _pendingModImportToken != importToken)
		{
			prepared?.Dispose();
			return;
		}
		_pendingModImport = null;
		_pendingModImportToken = Guid.Empty;
		if (prepared == null)
		{
			return;
		}
		XWModManifest manifest = prepared.Manifest;
		using (prepared)
		{
			XWModPackageInstaller.InstallResult installResult = XWModPackageInstaller.Install(prepared, selection);
			if (!installResult.Success)
			{
				ShowManagementResult("导入失败", "安装未完成，请查看详情。", failure: true, installResult.Message + "\n" + FormatManagementIssues(installResult.Issues));
				return;
			}
			RefreshModList();
			if (ManagementOnly)
			{
				JumpPlayerMod(manifest.Id);
			}
			ShowManagementResult("安装完成", (!installResult.Enabled) ? (manifest.Name + " 已安装并保持停用，可稍后启用。") : (ManagementOnly ? (manifest.Name + " 已安装并启用。") : (manifest.Name + " 已安装，重启后可使用。")), failure: false, "", installResult.RestartRequired);
		}
	}

	private void CancelPreparedMod(XWModPackageInstaller.PreparedImport prepared, Guid importToken)
	{
		if (_pendingModImport == prepared && _pendingModImportToken == importToken)
		{
			_pendingModImport?.Dispose();
			_pendingModImport = null;
			_pendingModImportToken = Guid.Empty;
		}
	}

	private static string EscapeBbcode(string value)
	{
		return (value ?? "").Replace("[", "［", StringComparison.Ordinal).Replace("]", "］", StringComparison.Ordinal);
	}

	private void PopulateModTree(IReadOnlyList<XWModManager.ModEntry> entries)
	{
		if (_modTree == null)
		{
			return;
		}
		_modTree.Clear();
		TreeItem parent = _modTree.CreateItem();
		foreach (XWModManager.ModEntry entry in entries)
		{
			TreeItem treeItem = _modTree.CreateItem(parent);
			treeItem.SetCellMode(0, TreeItem.TreeCellMode.Check);
			treeItem.SetChecked(0, entry.Enabled);
			treeItem.SetMetadata(0, Variant.From<string>(entry.Id));
			treeItem.SetMetadata(1, Variant.From<string>(entry.DirectoryPath));
			treeItem.SetText(1, entry.DisplayName);
			treeItem.SetText(2, string.IsNullOrWhiteSpace(entry.Version) ? "-" : entry.Version);
			treeItem.SetText(3, (string.IsNullOrEmpty(entry.PackagePath) || entry.HasManifest) ? entry.EffectiveState : "缺少 mod.json");
			if (!entry.HasManifest)
			{
				treeItem.SetCustomColor(3, new Color(1f, 0.62f, 0.25f));
			}
		}
	}

	private void OnModSelected()
	{
		if (!CanUseManagement)
		{
			return;
		}
		string modId = GetSelectedModId();
		if (!string.IsNullOrWhiteSpace(modId))
		{
			string value = _modTree.GetSelected()?.GetMetadata(1).AsString() ?? "";
			XWModManager.ModEntry modEntry = new XWModManager().GetManagementSnapshot().Find((XWModManager.ModEntry item) => string.Equals(item.Id, modId, StringComparison.OrdinalIgnoreCase));
			SetResult($"[b]Mod 管理[/b]\n已选择: {modId}\n安装包: {value}\n安装版本: {modEntry?.InstalledVersion}\n运行版本: {modEntry?.RunningVersion}\n实际模式: {modEntry?.EffectiveMode}\n状态: {modEntry?.EffectiveState}\n安装 SHA256: {modEntry?.PackageSha256}\n运行 SHA256: {modEntry?.RunningSha256}\n最近失败: {modEntry?.LastApplyFailure}\n{modEntry?.Diagnostic}");
		}
	}

	private string GetSelectedModId()
	{
		if (!ManagementOnly)
		{
			return (_modTree?.GetSelected())?.GetMetadata(0).AsString() ?? "";
		}
		return _playerSelected?.Id ?? "";
	}

	private bool IsSelectedModEnabled()
	{
		if (ManagementOnly)
		{
			return _playerSelected?.Enabled ?? false;
		}
		return (_modTree?.GetSelected())?.IsChecked(0) ?? false;
	}

	private void SetSelectedModEnabled(bool enabled)
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty)
		{
			return;
		}
		SelectTab(2);
		string modId = GetSelectedModId();
		if (string.IsNullOrWhiteSpace(modId))
		{
			ShowManagementResult("请选择 Mod", "先在列表中选择一个 Mod。");
			return;
		}
		XWModManager manager = new XWModManager();
		XWModManager.SelectionPlan plan = manager.PreviewSelection(modId, enabled);
		if (!plan.CanApply)
		{
			RememberPlayerOperationIssues(modId, plan.Issues, success: false);
			ShowManagementResult("状态未更改", string.Join("\n", plan.Issues.Select((XWValidationIssue issue) => issue.Message)), failure: true, FormatManagementIssues(plan.Issues));
		}
		else if (plan.HasRelatedChanges)
		{
			ShowModConfirmation(enabled ? "一起启用所需 Mod" : "一起停用关联 Mod", "将" + (enabled ? "启用" : "停用") + "以下 Mod：\n" + string.Join("\n", plan.Changes.Select((XWModManager.SelectionChange change) => "• " + change.Name)), enabled ? "一并启用" : "一并停用", Apply);
		}
		else
		{
			Apply();
		}
		void Apply()
		{
			XWModManager.SetEnabledResult setEnabledResult;
			try
			{
				setEnabledResult = manager.ApplySelection(plan);
			}
			catch (Exception ex) when (((ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException || ex is InvalidOperationException) ? 1 : 0) != 0)
			{
				ShowManagementResult("状态未更改", "无法保存：" + ex.GetBaseException().Message, failure: true);
				return;
			}
			RefreshModList();
			RememberPlayerOperationIssues(modId, setEnabledResult.Issues, setEnabledResult.Success);
			XWModToolsPanel xWModToolsPanel = this;
			string title = (setEnabledResult.Success ? "更改已保存" : "状态未更改");
			object message;
			if (!setEnabledResult.Success)
			{
				message = string.Join("\n", setEnabledResult.Issues.Select((XWValidationIssue issue) => issue.Message));
			}
			else
			{
				if (ManagementOnly)
				{
					XWModPlayerManagementState playerManagement = _playerManagement;
					if (playerManagement != null && playerManagement.PendingCount == 0)
					{
						message = "当前没有待应用的更改。";
						goto IL_0134;
					}
				}
				message = "重启后应用，可继续调整其他 Mod。";
			}
			goto IL_0134;
			IL_0134:
			xWModToolsPanel.ShowManagementResult(title, (string)message, !setEnabledResult.Success, FormatManagementIssues(setEnabledResult.Issues));
		}
	}

	private void ConfirmDeleteSelectedMod()
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty)
		{
			return;
		}
		string modId = GetSelectedModId();
		if (string.IsNullOrWhiteSpace(modId))
		{
			ShowManagementResult("请选择 Mod", "先在列表中选择一个 Mod。");
			return;
		}
		XWModManager.ModEntry[] array = (from item in new XWModManager().ScanMods(includeHashes: true)
			where string.Equals(item.Id, modId, StringComparison.OrdinalIgnoreCase)
			select item).ToArray();
		TreeItem treeItem = _modTree?.GetSelected();
		string selectedPath = ((!ManagementOnly) ? ((treeItem == null) ? "" : treeItem.GetMetadata(1).AsString()) : (_playerSelected?.PackagePath ?? ""));
		XWModManager.ModEntry[] array2 = (string.IsNullOrWhiteSpace(selectedPath) ? XWModManager.PreferValidCandidates(array) : array.Where((XWModManager.ModEntry item) => string.Equals(item.PackagePath, selectedPath, StringComparison.OrdinalIgnoreCase)).ToArray());
		XWModManager.ModEntry entry = ((array2.Length == 1) ? array2[0] : null);
		if (entry != null)
		{
			ShowModConfirmation("确认删除 Mod", $"将删除安装包：{entry.DisplayName} {entry.Version}\n\nMod 玩家进度会保留。确认继续？", "删除", delegate
			{
				XWModManager.DeleteResult deleteResult = new XWModManager().TryDeleteMod(entry.Id, entry.PackageSha256);
				RefreshModList();
				ShowManagementResult((!deleteResult.Success) ? "删除失败" : (deleteResult.PendingRestart ? "已安排删除" : "删除完成"), (!deleteResult.Success) ? "安装包未能删除，请查看详情。" : (deleteResult.PendingRestart ? "重启后删除安装包，玩家进度会保留。" : "已删除安装包，玩家进度已保留。"), !deleteResult.Success, deleteResult.Message, deleteResult.Success && deleteResult.PendingRestart);
			});
		}
	}

	private void SetupTemplateLibrary()
	{
		if (_templateOption == null)
		{
			return;
		}
		_templateIds.Clear();
		_templateCards.Clear();
		_templateOption.Clear();
		ClearVisualChoiceChildren(_templateCardGrid);
		ButtonGroup buttonGroup = new ButtonGroup
		{
			AllowUnpress = false
		};
		for (int i = 0; i < _templateLibrary.Templates.Count; i++)
		{
			XWTemplateLibrary.TemplateInfo templateInfo = _templateLibrary.Templates[i];
			XWTemplatePresentation xWTemplatePresentation = XWTemplatePresentation.Resolve(templateInfo);
			Texture2D texture2D = ResourceLoader.Load<Texture2D>(ResolveTemplateCategoryIcon(templateInfo.Category), null, ResourceLoader.CacheMode.Reuse);
			_templateOption.AddItem(xWTemplatePresentation.CategoryLabel + " / " + xWTemplatePresentation.DisplayNameLabel, i);
			if (GodotObject.IsInstanceValid(texture2D))
			{
				_templateOption.SetItemIcon(i, texture2D);
			}
			_templateIds[i] = templateInfo.Id;
			if (_templateCardGrid != null)
			{
				int cardIndex = i;
				Button button = new Button
				{
					Name = $"TemplateCard_{i}",
					Text = xWTemplatePresentation.CategoryLabel + "\n" + xWTemplatePresentation.DisplayNameLabel,
					TooltipText = xWTemplatePresentation.PreviewTextLabel + "\n默认目录：" + xWTemplatePresentation.DefaultFolderLabel,
					ToggleMode = true,
					ButtonGroup = buttonGroup,
					CustomMinimumSize = new Vector2(176f, 72f),
					Icon = texture2D,
					ExpandIcon = false
				};
				button.Pressed += delegate
				{
					SelectTemplate(cardIndex);
				};
				_templateCardGrid.AddChild(button, forceReadableName: false, InternalMode.Disabled);
				_templateCards[i] = button;
			}
		}
		if (_templateLibrary.Templates.Count > 0)
		{
			SelectTemplate(0L);
		}
	}

	private void SelectTemplate(long index)
	{
		int num = (int)index;
		if (!_templateIds.TryGetValue(num, out var value))
		{
			return;
		}
		XWTemplateLibrary.TemplateInfo templateInfo = FindTemplate(value);
		if (templateInfo == null)
		{
			return;
		}
		XWTemplatePresentation xWTemplatePresentation = XWTemplatePresentation.Resolve(templateInfo);
		if (_templateName != null && string.IsNullOrWhiteSpace(_templateName.Text))
		{
			_templateName.PlaceholderText = "例如：" + xWTemplatePresentation.SuggestedName;
		}
		if (_templatePreview != null)
		{
			_templatePreview.Text = $"{xWTemplatePresentation.CategoryLabel} / {xWTemplatePresentation.DisplayNameLabel}\n{xWTemplatePresentation.DescriptionLabel}\n\n用途预览：{xWTemplatePresentation.PreviewTextLabel}\n\n技术结构：\n{templateInfo.Content}";
		}
		if (_templateStatus != null)
		{
			_templateStatus.Text = "默认目录：" + xWTemplatePresentation.DefaultFolderLabel;
		}
		_templateOption?.Select(num);
		foreach (KeyValuePair<int, Button> templateCard in _templateCards)
		{
			templateCard.Value.SetPressedNoSignal(templateCard.Key == num);
		}
	}

	private static void ClearVisualChoiceChildren(Node parent)
	{
		if (parent == null)
		{
			return;
		}
		foreach (Node child in parent.GetChildren())
		{
			child.QueueFree();
		}
	}

	private static string ResolveTemplateCategoryIcon(string category)
	{
		if (category != null)
		{
			int length = category.Length;
			if (length <= 5)
			{
				if (length != 3)
				{
					if (length == 5)
					{
						char c = category[0];
						if (c != 'A')
						{
							if (c == 'L' && category == "Level")
							{
								goto IL_0159;
							}
						}
						else if (category == "Audio")
						{
							goto IL_0161;
						}
					}
				}
				else
				{
					char c = category[0];
					if (c != 'B')
					{
						if (c != 'G')
						{
							if (c == 'M' && category == "Map")
							{
								goto IL_0159;
							}
						}
						else if (category == "GUI")
						{
							return "res://addons/ModEditor/Icons/ResourceGUI.svg";
						}
					}
					else if (category == "BGM")
					{
						goto IL_0161;
					}
				}
			}
			else if (length != 9)
			{
				if (length != 10)
				{
					if (length == 14 && category == "CharacterEvent")
					{
						goto IL_0141;
					}
				}
				else if (category == "Projectile")
				{
					return "res://addons/ModEditor/Icons/ResourceProjectile.svg";
				}
			}
			else
			{
				char c = category[1];
				if (c != 'h')
				{
					if (c != 'l')
					{
						if (c == 'o' && category == "Component")
						{
							goto IL_0141;
						}
					}
					else if (category == "Blueprint")
					{
						return "res://addons/ModEditor/Icons/ClassIcon/VisualShader.svg";
					}
				}
				else if (category == "Character")
				{
					goto IL_0141;
				}
			}
		}
		return "res://addons/ModEditor/Icons/ScriptCreate.svg";
		IL_0159:
		return "res://addons/ModEditor/Icons/ResourceLevel.svg";
		IL_0161:
		return "res://addons/ModEditor/Icons/ResourceAudio.svg";
		IL_0141:
		return "res://addons/ModEditor/Icons/ResourceCharacter.svg";
	}

	private static string ResolveImportCategoryIcon(string folder)
	{
		switch (folder)
		{
		case "Images":
			return "res://addons/ModEditor/Icons/ClassIcon/Image.svg";
		case "Sfx":
		case "BGM":
			return "res://addons/ModEditor/Icons/ClassIcon/AudioStream.svg";
		case "Fonts":
			return "res://addons/ModEditor/Icons/ClassIcon/Font.svg";
		case "Scenes":
			return "res://addons/ModEditor/Icons/PackedScene.svg";
		case "UI":
			return "res://addons/ModEditor/Icons/ResourceGUI.svg";
		default:
			return "res://addons/ModEditor/Icons/File.svg";
		}
	}

	private void CreateTemplateFromSelection()
	{
		SelectTab(1);
		string currentProjectPath = GetCurrentProjectPath();
		if (string.IsNullOrEmpty(currentProjectPath))
		{
			SetTemplateStatus("请先打开 Mod 工程。");
			return;
		}
		int key = _templateOption?.Selected ?? (-1);
		if (!_templateIds.TryGetValue(key, out var value))
		{
			SetTemplateStatus("请先选择模板。");
			return;
		}
		XWTemplatePresentation xWTemplatePresentation = XWTemplatePresentation.Resolve(FindTemplate(value));
		string text = _templateName?.Text?.Trim() ?? "";
		bool num = string.IsNullOrWhiteSpace(text);
		string resourceName = (num ? xWTemplatePresentation.SuggestedName : text);
		string displayName = (num ? xWTemplatePresentation.DisplayNameLabel : text);
		XWTemplateLibrary.TemplateCreateResult templateCreateResult = _templateLibrary.CreateFromTemplate(value, currentProjectPath, resourceName, displayName);
		if (!templateCreateResult.Success)
		{
			SetTemplateStatus(templateCreateResult.Error);
			SetResult("[b]模板库[/b]\n创建失败: " + templateCreateResult.Error);
			return;
		}
		SetTemplateStatus("已创建: " + templateCreateResult.CreatedPath);
		SetResult("[b]模板库[/b]\n已创建: " + templateCreateResult.CreatedPath);
		IEnumerable<string> paths;
		if (templateCreateResult.CreatedPaths.Count <= 0)
		{
			IEnumerable<string> enumerable = new string[1] { templateCreateResult.CreatedPath };
			paths = enumerable;
		}
		else
		{
			IEnumerable<string> enumerable = templateCreateResult.CreatedPaths;
			paths = enumerable;
		}
		XWModManifestSyncService.RegisterPaths(currentProjectPath, paths);
		foreach (string createdPath in templateCreateResult.CreatedPaths)
		{
			if (createdPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				XWBPCSharpMemberRegistry.Instance.NotifyScriptSaved(createdPath);
			}
		}
		XWFileSystem.GetSingleton()?.ScanChanges();
		if (!string.IsNullOrWhiteSpace(templateCreateResult.Warning))
		{
			XWEditorInterface.Instance?.ShowToast("模板已创建，但存在清理提示：" + templateCreateResult.Warning, 1);
		}
		if (XWFileSystemPanel.Instance != null)
		{
			XWFileSystemPanel.Instance.CompleteTemplateCreationFromTool(templateCreateResult);
		}
		else if (templateCreateResult.CreatedPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
		{
			XWEditorInterface.Instance?.GetScriptEditor()?.OpenFileAt(templateCreateResult.CreatedPath, 0, 0);
		}
	}

	private XWTemplateLibrary.TemplateInfo FindTemplate(string templateId)
	{
		foreach (XWTemplateLibrary.TemplateInfo template in _templateLibrary.Templates)
		{
			if (template.Id == templateId)
			{
				return template;
			}
		}
		return null;
	}

	private void SetTemplateStatus(string text)
	{
		if (_templateStatus != null)
		{
			_templateStatus.Text = text;
		}
		SetResult("[b]模板库[/b]\n" + text);
	}

	private static string FindOverrideResourcePath(string projectPath, XWModManifest manifest, string category, string key)
	{
		if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(key))
		{
			return "";
		}
		foreach (string resource in manifest.Resources)
		{
			if (IsLikelyOverridePath(resource, category, key))
			{
				string text = Path.Combine(projectPath, resource.Replace('/', Path.DirectorySeparatorChar));
				if (File.Exists(text))
				{
					return text;
				}
			}
		}
		foreach (string overrideSearchFolder in GetOverrideSearchFolders(projectPath, category))
		{
			if (!Directory.Exists(overrideSearchFolder))
			{
				continue;
			}
			foreach (string item in Directory.EnumerateFiles(overrideSearchFolder, "*.*", SearchOption.AllDirectories))
			{
				if (!item.Contains($"{Path.DirectorySeparatorChar}.build{Path.DirectorySeparatorChar}") && IsLikelyOverridePath(item, category, key))
				{
					return item;
				}
			}
		}
		return "";
	}

	private static IEnumerable<string> GetOverrideSearchFolders(string projectPath, string category)
	{
		if (!string.IsNullOrWhiteSpace(category))
		{
			yield return Path.Combine(projectPath, "Resources", category);
		}
		yield return Path.Combine(projectPath, "Resources");
		yield return projectPath;
	}

	private static bool IsLikelyOverridePath(string path, string category, string key)
	{
		if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(key))
		{
			return false;
		}
		string text = path.Replace('\\', '/').ToLowerInvariant();
		string text2 = (category ?? "").ToLowerInvariant();
		string text3 = key.Replace('\\', '/').Trim('/').ToLowerInvariant();
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text3.Replace('/', Path.DirectorySeparatorChar));
		string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text.Replace('/', Path.DirectorySeparatorChar));
		if (!(fileNameWithoutExtension2 == text3) && !(fileNameWithoutExtension2 == fileNameWithoutExtension) && !text.Contains("/" + text3 + ".") && !text.Contains("/" + fileNameWithoutExtension + "."))
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(text2))
		{
			return text.Contains("/" + text2.ToLowerInvariant() + "/");
		}
		return true;
	}

	private static string FormatDiffValue(Variant value)
	{
		Variant.Type variantType = value.VariantType;
		if ((ulong)variantType <= 4uL)
		{
			switch (variantType)
			{
			case Variant.Type.Nil:
				return "空";
			case Variant.Type.Bool:
				return value.AsBool() ? "启用" : "关闭";
			case Variant.Type.String:
				goto IL_005c;
			case Variant.Type.Int:
				return value.AsInt64().ToString();
			case Variant.Type.Float:
				return value.AsDouble().ToString("0.###");
			}
		}
		switch (variantType)
		{
		case Variant.Type.StringName:
		case Variant.Type.NodePath:
			break;
		case Variant.Type.Object:
			return value.AsGodotObject()?.GetType().Name ?? "空";
		default:
			return value.ToString();
		}
		goto IL_005c;
		IL_005c:
		return value.AsString();
	}

	private static string NormalizeProjectPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || path.StartsWith("res://") || path.StartsWith("uid://"))
		{
			return path;
		}
		string text = path.Replace('\\', '/');
		string text2 = ProjectSettings.GlobalizePath("res://").Replace('\\', '/');
		if (!text2.EndsWith("/"))
		{
			text2 += "/";
		}
		if (text.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
		{
			return "res://" + text.Substring(text2.Length);
		}
		return path;
	}

	private static void UpdateIssuePanel(List<XWValidationIssue> issues)
	{
		if (XWEditorInterface.Instance?.GetIssuePanel() is XWIssuePanel xWIssuePanel)
		{
			xWIssuePanel.LoadIssues(issues);
		}
	}

	private void SelectTab(int index)
	{
		if (_toolTabs != null && index >= 0 && index < _toolTabs.GetTabCount())
		{
			_toolTabs.CurrentTab = index;
		}
	}

	private string GetCurrentProjectPath()
	{
		if (XWEditorInterface.Instance?.GetEditorPanel() is ModEditorPanel modEditorPanel)
		{
			return modEditorPanel.GetCurrentProject()?.ProjectPath ?? "";
		}
		return "";
	}

	private void SetResult(string text)
	{
		if (_result != null)
		{
			_result.Clear();
			_result.AppendText(text);
		}
	}

	private void AddExperienceActions(BoxContainer root)
	{
		if (ManagementPlayRequested == null)
		{
			return;
		}
		_playerPlay = PlayerButton("PlayModLevels", "全部 Mod 关卡", delegate
		{
			if (CanUseManagement)
			{
				if (XWModContentCatalog.GetCatalogs().Count == 0)
				{
					SetPlayerNotice("还没有可玩的 Mod 关卡。安装并启用后，请重启游戏。", PlayerGold, showDetails: false);
				}
				else
				{
					ManagementPlayRequested("");
				}
			}
		}, 180, "ModSecondaryButton");
		root.AddChild(_playerPlay, forceReadableName: false, InternalMode.Disabled);
	}

	private void AddSelectedExperienceActions(VBoxContainer detail)
	{
		detail.AddChild(PlayerLabel("怎么玩", 20, PlayerInk), forceReadableName: false, InternalMode.Disabled);
		_playerInstructions = PlayerLabel("", 18, PlayerInk);
		_playerInstructions.Name = "PlayInstructions";
		detail.AddChild(_playerInstructions, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(_playerInstructions, this, PlayerDetailWidth);
		_playerPlayHint = PlayerLabel("", 16, PlayerMuted);
		_playerPlayHint.Name = "PlayHint";
		detail.AddChild(_playerPlayHint, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(_playerPlayHint, this, PlayerDetailWidth);
		_playerSelectedPlay = PlayerButton("PlaySelectedMod", "开始游玩", delegate
		{
			if (CanPlaySelectedMod())
			{
				ManagementPlayRequested?.Invoke(_playerSelected.Id);
			}
		}, 160);
		_playerSelectedPlay.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
		detail.AddChild(_playerSelectedPlay, forceReadableName: false, InternalMode.Disabled);
	}

	private bool CanPlaySelectedMod()
	{
		XWModManager.ModEntry playerSelected = _playerSelected;
		if (playerSelected != null && playerSelected.Loaded && playerSelected.Enabled && !playerSelected.PendingDelete && playerSelected.EffectiveState != "已加载旧版本" && !PlayerHasFailure(playerSelected))
		{
			return XWModNavigation.Catalogs(playerSelected.Id).Count > 0;
		}
		return false;
	}

	private void RenderSelectedExperience()
	{
		XWModManager.ModEntry playerSelected = _playerSelected;
		if (playerSelected != null)
		{
			_playerInstructions.Text = (string.IsNullOrWhiteSpace(playerSelected.Manifest?.PlayInstructions) ? "作者未提供使用说明，请参考上方简介。" : playerSelected.Manifest.PlayInstructions);
			_playerPlayHint.Text = ((!playerSelected.Enabled) ? "启用后重启游戏，即可使用此 Mod。" : ((!playerSelected.Loaded || playerSelected.EffectiveState == "已加载旧版本") ? "重启游戏后，应用此 Mod 的更改。" : ((XWModNavigation.Catalogs(playerSelected.Id).Count == 0) ? "这个 Mod 没有独立关卡入口，请查看作者简介或说明。" : "")));
			_playerPlayHint.Visible = _playerPlayHint.Text.Length > 0;
			_playerSelectedPlay.Visible = ManagementPlayRequested != null && CanPlaySelectedMod();
		}
	}

	private void OpenPlayerHelp()
	{
		ShowModConfirmation("如何安装和使用 Mod", "1. 下载作者提供的 .pmod 文件，无需解压。\n" + (Global.IsMobile ? "2. 在文件管理器中选择该文件，用本游戏打开；也可使用管理页的安装按钮。\n" : "2. 点击“安装 Mod”，选择下载好的文件。\n") + "3. 确认安装并启用；如果需要其他 Mod，按提示补齐。\n4. 点击“重启并应用”。重启后选中 Mod，查看“怎么玩”或点击“开始游玩”。\n\n有些 Mod 会修改已有玩法，没有独立关卡入口；具体用法请查看作者说明。", "知道了", delegate
		{
		}, null, preservePlayerUtility: false, "返回", allowAccept: false);
	}

	private void AddProgressRecoveryActions(VBoxContainer actions)
	{
		string owner = _playerSelected?.Id;
		bool flag = !string.IsNullOrWhiteSpace(owner) && !string.IsNullOrWhiteSpace(GameSaveManager.Instance?.GetUserCurrent());
		Button button = PlayerButton("RestoreModProgress", "从备份恢复进度", delegate
		{
			ConfirmProgressRepair(owner, reset: false);
		}, 200, "ModSecondaryButton");
		button.Disabled = !flag || XWModPlayerProgressService.GetStatus(owner) != XWModPlayerProgressService.ProgressStatus.Recoverable;
		actions.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		Button button2 = PlayerButton("ResetModProgress", "重置 Mod 进度", delegate
		{
			ConfirmProgressRepair(owner, reset: true);
		}, 200, "ModDangerButton");
		button2.Disabled = !flag;
		actions.AddChild(button2, forceReadableName: false, InternalMode.Disabled);
	}

	private void ConfirmProgressRepair(string owner, bool reset)
	{
		ClosePlayerUtility(restoreFocus: false);
		string account = GameSaveManager.Instance?.GetUserCurrent();
		ShowModConfirmation(reset ? "重置 Mod 进度" : "恢复 Mod 进度", reset ? "将先备份原文件，再重置当前账号在此 Mod 中的通关、解锁和收藏。中途战斗存档、其他 Mod 和原版进度保留。" : "将先保护当前文件，再使用最后有效备份恢复此 Mod 的进度配置。", reset ? "备份并重置" : "恢复", delegate
		{
			if (account != GameSaveManager.Instance?.GetUserCurrent())
			{
				SetPlayerNotice("账号已改变，请重新选择 Mod 后操作。", PlayerError, showDetails: true);
			}
			else
			{
				string reason;
				bool flag = (reset ? XWModPlayerProgressService.TryResetProgress(owner, out reason) : XWModPlayerProgressService.TryRestoreBackup(owner, out reason));
				RefreshModList();
				SetPlayerNotice(reason, flag ? PlayerGreen : PlayerError, !flag);
			}
		});
	}

	private static void AppendExtensionDiagnostics(StringBuilder text, string owner)
	{
		XWModExtensionDiagnostic[] array = (from item in XWModExtensions.GetDiagnostics()
			where string.Equals(item.OwnerId, owner, StringComparison.OrdinalIgnoreCase)
			select item).ToArray();
		StringBuilder stringBuilder = text;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(24, 2, stringBuilder);
		handler.AppendLiteral("\n扩展接口 API ");
		handler.AppendFormatted(3);
		handler.AppendLiteral("；已接入 ");
		handler.AppendFormatted(XWModExtensionCatalog.Installed.Count);
		handler.AppendLiteral(" 个类型化扩展点。");
		stringBuilder2.AppendLine(ref handler);
		if (array.Length == 0)
		{
			text.AppendLine("此 Mod 尚无扩展调用记录。接口可用不代表当前平台已完成实机验收。");
			return;
		}
		text.AppendLine("当前会话扩展调用（耗时为采样累计，并非每帧总耗时）：");
		XWModExtensionDiagnostic[] array2 = array;
		foreach (XWModExtensionDiagnostic xWModExtensionDiagnostic in array2)
		{
			stringBuilder = text;
			StringBuilder stringBuilder3 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(24, 6, stringBuilder);
			handler.AppendFormatted(xWModExtensionDiagnostic.PointId);
			handler.AppendLiteral("：调用 ");
			handler.AppendFormatted(xWModExtensionDiagnostic.InvocationCount);
			handler.AppendLiteral("，失败 ");
			handler.AppendFormatted(xWModExtensionDiagnostic.FaultCount);
			handler.AppendLiteral("，拒绝 ");
			handler.AppendFormatted(xWModExtensionDiagnostic.RejectedCount);
			handler.AppendLiteral("，采样 ");
			handler.AppendFormatted(xWModExtensionDiagnostic.SampleCount);
			handler.AppendLiteral(" 次 / ");
			handler.AppendFormatted(xWModExtensionDiagnostic.SampledMilliseconds, "F3");
			handler.AppendLiteral(" ms");
			stringBuilder3.AppendLine(ref handler);
			if (!string.IsNullOrEmpty(xWModExtensionDiagnostic.LastDiagnostic))
			{
				text.AppendLine("  " + xWModExtensionDiagnostic.LastDiagnostic);
			}
		}
	}

	private static Button PlayerCompactButton(string name, string text, Action action, int width = 96)
	{
		Button button = PlayerButton(name, text, action, width, "ModSecondaryButton");
		button.CustomMinimumSize = new Vector2(width, 36f);
		button.AddThemeFontSizeOverride("font_size", 18);
		return button;
	}

	private void AddPlayerManagementFilters(VBoxContainer root)
	{
		HFlowContainer hFlowContainer = new HFlowContainer
		{
			Name = "ModFilters"
		};
		hFlowContainer.AddThemeConstantOverride("h_separation", 8);
		root.AddChild(hFlowContainer, forceReadableName: false, InternalMode.Disabled);
		_playerSearch = new LineEdit
		{
			Name = "ModSearch",
			PlaceholderText = "搜索名称、作者或 ID…",
			CustomMinimumSize = new Vector2(250f, 36f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		_playerSearch.AddThemeFontSizeOverride("font_size", 18);
		hFlowContainer.AddChild(_playerSearch, forceReadableName: false, InternalMode.Disabled);
		_playerFilter = new OptionButton
		{
			Name = "ModStateFilter",
			CustomMinimumSize = new Vector2(140f, 36f)
		};
		string[] array = new string[5] { "全部", "下次启用", "当前运行", "待重启", "有问题" };
		foreach (string label in array)
		{
			_playerFilter.AddItem(label);
		}
		_playerFilter.AddThemeFontSizeOverride("font_size", 18);
		hFlowContainer.AddChild(_playerFilter, forceReadableName: false, InternalMode.Disabled);
		_playerClearFilter = PlayerCompactButton("ClearModFilters", "清除", ClearPlayerModFilters, 64);
		hFlowContainer.AddChild(_playerClearFilter, forceReadableName: false, InternalMode.Disabled);
		_playerMatched = PlayerLabel("", 16, PlayerMuted, wrap: false);
		_playerMatched.VerticalAlignment = VerticalAlignment.Center;
		_playerMatched.CustomMinimumSize = new Vector2(90f, 36f);
		_playerMatched.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		hFlowContainer.AddChild(_playerMatched, forceReadableName: false, InternalMode.Disabled);
		_playerSearch.TextChanged += delegate
		{
			PlayerModFilterChanged();
		};
		_playerFilter.ItemSelected += delegate
		{
			PlayerModFilterChanged();
		};
	}

	private void PlayerModFilterChanged()
	{
		if (!_updatingPlayerFilters && CanUseManagement)
		{
			_playerExactId = null;
			RenderPlayerModRows();
			_playerListScroll.ScrollVertical = 0;
		}
	}

	private void ClearPlayerModFilters()
	{
		_updatingPlayerFilters = true;
		_playerSearch.Text = "";
		_playerFilter.Select(0);
		_playerExactId = null;
		_updatingPlayerFilters = false;
		RenderPlayerModRows();
		_playerSearch.GrabFocus();
	}

	private float PlayerDetailWidth()
	{
		return Mathf.Max(1f, base.Size.X - Mathf.Clamp(base.Size.X * 0.3f, 240f, 300f) - 24f - _playerDetailScroll.GetVScrollBar().GetCombinedMinimumSize().X);
	}

	private void AddPlayerManagementDetails(VBoxContainer detail)
	{
		Label[] array = new Label[7] { _playerName, _playerAuthor, _playerDescription, _playerEnabled, _playerRunning, _playerVersions, _playerMode };
		for (int i = 0; i < array.Length; i++)
		{
			XWModSettingsTextView.BindWidth(array[i], this, PlayerDetailWidth);
		}
		_playerPackagePath = PlayerLabel("", 16, PlayerMuted);
		_playerPackagePath.Name = "SelectedPackagePath";
		detail.AddChild(_playerPackagePath, forceReadableName: false, InternalMode.Disabled);
		_playerPendingDetails = PlayerLabel("", 16, PlayerGold);
		_playerPendingDetails.Name = "SelectedPendingChanges";
		detail.AddChild(_playerPendingDetails, forceReadableName: false, InternalMode.Disabled);
		_playerIssueList = new VBoxContainer
		{
			Name = "SelectedModIssues",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		detail.AddChild(_playerIssueList, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(_playerPackagePath, this, PlayerDetailWidth);
		XWModSettingsTextView.BindWidth(_playerPendingDetails, this, PlayerDetailWidth);
	}

	private void AddPlayerPendingBar(VBoxContainer root)
	{
		_playerPendingBar = new HFlowContainer
		{
			Name = "PendingChangesBar",
			Visible = false
		};
		_playerPendingBar.AddThemeConstantOverride("h_separation", 12);
		root.AddChild(_playerPendingBar, forceReadableName: false, InternalMode.Disabled);
		_playerPendingCount = PlayerLabel("", 18, PlayerGold, wrap: false);
		_playerPendingCount.CustomMinimumSize = new Vector2(160f, 36f);
		_playerPendingCount.VerticalAlignment = VerticalAlignment.Center;
		_playerPendingBar.AddChild(_playerPendingCount, forceReadableName: false, InternalMode.Disabled);
		_playerPendingView = PlayerCompactButton("ViewPendingChanges", "查看变更", OpenPlayerPendingChanges, 112);
		_playerPendingBar.AddChild(_playerPendingView, forceReadableName: false, InternalMode.Disabled);
		_playerRestart = PlayerCompactButton("RestartFromManagement", "重启并应用", RestartFromManagement, 144);
		_playerPendingBar.AddChild(_playerRestart, forceReadableName: false, InternalMode.Disabled);
		_playerRestartReason = PlayerLabel("", 14, PlayerMuted);
		root.AddChild(_playerRestartReason, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(_playerRestartReason, this, () => base.Size.X);
	}

	private void UpdatePlayerManagementState(IEnumerable<XWValidationIssue> issues, string[] actual, string[] planned)
	{
		string text = XWModPlayerManagementState.Evidence(_playerEntries);
		if (_playerOperationEvidence != text)
		{
			_playerOperationIssues.Clear();
			_playerOperationDetails = "";
			_playerOperationEvidence = text;
		}
		_playerCurrentIssues = issues.DistinctBy((XWValidationIssue issue) => (Code: issue.Code, Level: issue.Level, ResourceKey: issue.ResourceKey, Message: issue.Message)).ToArray();
		_playerOrderChanged = actual != null && planned != null && !actual.Where((string id) => planned.Contains<string>(id, StringComparer.OrdinalIgnoreCase)).SequenceEqual<string>(planned.Where((string id) => actual.Contains<string>(id, StringComparer.OrdinalIgnoreCase)), StringComparer.OrdinalIgnoreCase);
		RebuildPlayerManagementState();
	}

	private void RebuildPlayerManagementState()
	{
		_playerManagement = new XWModPlayerManagementState(_playerEntries, _playerCurrentIssues.Concat(_playerOperationIssues), _playerOrderChanged);
		_playerPendingBar.Visible = _playerManagement.PendingCount > 0;
		_playerPendingCount.Text = $"待应用 {_playerManagement.PendingCount} 项";
	}

	private void RememberPlayerOperationIssues(string modId, IEnumerable<XWValidationIssue> issues, bool success)
	{
		if (!ManagementOnly)
		{
			return;
		}
		_playerOperationEvidence = XWModPlayerManagementState.Evidence(_playerEntries);
		_playerOperationIssues.Clear();
		if (!success)
		{
			foreach (XWValidationIssue item in issues.Where((XWValidationIssue issue) => (issue.AffectedModIds ?? new List<string>()).Contains<string>(modId, StringComparer.OrdinalIgnoreCase) || (issue.RelatedModIds ?? new List<string>()).Contains<string>(modId, StringComparer.OrdinalIgnoreCase)))
			{
				_playerOperationIssues.Add(new XWValidationIssue
				{
					Code = item.Code,
					Level = item.Level,
					Message = "上次操作未成功：" + item.Message,
					ResourceKey = item.ResourceKey,
					AffectedModIds = (item.AffectedModIds ?? new List<string>()).Append(modId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList(),
					RelatedModIds = (from id in (item.RelatedModIds ?? new List<string>()).Concat(item.AffectedModIds ?? new List<string>())
						where !string.Equals(id, modId, StringComparison.OrdinalIgnoreCase)
						select id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList()
				});
			}
		}
		if (!success && _playerOperationIssues.Count == 0)
		{
			_playerOperationIssues.Add(new XWValidationIssue
			{
				Code = XWValidationIssue.IssueCode.RuntimeLoadFailed,
				Level = XWValidationIssue.Severity.Error,
				Message = "上次操作未成功，请查看完整诊断中的启用集合问题。",
				AffectedModIds = new List<string> { modId }
			});
		}
		RebuildPlayerManagementState();
		RenderPlayerModRows();
	}

	private void RenderPlayerModRows()
	{
		if (_playerManagement == null)
		{
			return;
		}
		string selectedKey = PlayerEntryKey(_playerSelected);
		foreach (Node child in _playerList.GetChildren())
		{
			_playerList.RemoveChild(child);
			child.QueueFree();
		}
		_playerRows.Clear();
		IReadOnlyList<XWModPlayerManagementState.Item> readOnlyList = _playerManagement.Match(_playerSearch.Text, (XWModPlayerManagementState.Filter)_playerFilter.Selected, _playerExactId);
		_playerSelected = readOnlyList.FirstOrDefault((XWModPlayerManagementState.Item item) => PlayerEntryKey(item.Entry).Equals(selectedKey, StringComparison.OrdinalIgnoreCase))?.Entry ?? readOnlyList.FirstOrDefault()?.Entry;
		if (PlayerEntryKey(_playerSelected) != selectedKey)
		{
			_playerDetailScroll.ScrollVertical = 0;
		}
		_playerMatched.Text = $"显示 {readOnlyList.Count}/{_playerEntries.Count}";
		_playerDetailScroll.Visible = _playerSelected != null;
		foreach (XWModPlayerManagementState.Item item in readOnlyList)
		{
			XWModManager.ModEntry entry = item.Entry;
			Button button = new Button
			{
				Name = "ModRow",
				ThemeTypeVariation = "ModListRow",
				ToggleMode = true,
				CustomMinimumSize = new Vector2(0f, 72f),
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				TooltipText = $"{entry.DisplayName}\n{entry.Id}\n{entry.PackagePath}"
			};
			ColorRect colorRect = new ColorRect
			{
				Name = "Bookmark",
				Color = new Color("ad6b22"),
				MouseFilter = MouseFilterEnum.Ignore
			};
			button.AddChild(colorRect, forceReadableName: false, InternalMode.Disabled);
			colorRect.SetAnchorsAndOffsetsPreset(LayoutPreset.LeftWide, LayoutPresetMode.Minsize);
			colorRect.OffsetRight = 5f;
			colorRect.OffsetTop = 8f;
			colorRect.OffsetBottom = -8f;
			MarginContainer marginContainer = new MarginContainer
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			string[] array = new string[2] { "left", "right" };
			foreach (string text in array)
			{
				marginContainer.AddThemeConstantOverride("margin_" + text, 14);
			}
			array = new string[2] { "top", "bottom" };
			foreach (string text2 in array)
			{
				marginContainer.AddThemeConstantOverride("margin_" + text2, 8);
			}
			button.AddChild(marginContainer, forceReadableName: false, InternalMode.Disabled);
			marginContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
			VBoxContainer vBoxContainer = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,
				Alignment = BoxContainer.AlignmentMode.Center
			};
			vBoxContainer.AddThemeConstantOverride("separation", 3);
			marginContainer.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
			Label label = PlayerLabel(entry.DisplayName, 20, PlayerInk, wrap: false);
			label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
			vBoxContainer.AddChild(label, forceReadableName: false, InternalMode.Disabled);
			string text3 = (item.HasProblems ? ("有问题" + (item.Pending ? " · 待重启" : "")) : (item.Pending ? "待重启" : PlayerState(entry)));
			Label label2 = PlayerLabel(VersionLabel(entry.Version) + " · " + text3, 16, item.HasProblems ? PlayerError : (item.Pending ? PlayerGold : PlayerStateColor(entry)), wrap: false);
			label2.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
			vBoxContainer.AddChild(label2, forceReadableName: false, InternalMode.Disabled);
			button.Pressed += delegate
			{
				SelectPlayerMod(entry);
			};
			button.FocusEntered += delegate
			{
				if (!Global.IsMobile)
				{
					SelectPlayerMod(entry);
				}
			};
			_playerList.AddChild(button, forceReadableName: false, InternalMode.Disabled);
			_playerRows[PlayerEntryKey(entry)] = button;
		}
		if (readOnlyList.Count == 0 && _playerEntries.Count > 0)
		{
			_playerList.AddChild(PlayerLabel("没有匹配的 Mod", 20, PlayerMuted), forceReadableName: false, InternalMode.Disabled);
			_playerList.AddChild(PlayerCompactButton("ClearNoMatches", "清除筛选", ClearPlayerModFilters, 120), forceReadableName: false, InternalMode.Disabled);
		}
		ConfigurePlayerScrollContent(_playerList);
		RenderPlayerSelection();
	}

	private void RenderPlayerManagementDetails()
	{
		foreach (Node child in _playerIssueList.GetChildren())
		{
			_playerIssueList.RemoveChild(child);
			child.QueueFree();
		}
		XWModPlayerManagementState.Item item = _playerManagement?.Find(_playerSelected);
		_playerPendingDetails.Text = ((item == null) ? "" : string.Join("\n", item.PendingReasons.Concat((!item.OrderPending) ? ((IEnumerable<string>)Array.Empty<string>()) : ((IEnumerable<string>)new string[1] { "加载顺序待调整，重启后生效" }))));
		_playerPendingDetails.Visible = !string.IsNullOrEmpty(_playerPendingDetails.Text);
		_playerPackagePath.Text = ((_playerSelected == null) ? "" : ("ID：" + _playerSelected.Id + "\n" + _playerSelected.PackagePath));
		_playerPackagePath.Visible = false;
		if (item == null)
		{
			return;
		}
		foreach (XWValidationIssue issue in item.Issues)
		{
			Label label = PlayerLabel(((issue.Level == XWValidationIssue.Severity.Error) ? "问题：" : ((issue.Level == XWValidationIssue.Severity.Warning) ? "提醒：" : "说明：")) + issue.Message, 16, (issue.Level == XWValidationIssue.Severity.Error) ? PlayerError : PlayerGold);
			label.Name = "ModIssue";
			_playerIssueList.AddChild(label, forceReadableName: false, InternalMode.Disabled);
			XWModSettingsTextView.BindWidth(label, this, PlayerDetailWidth);
			foreach (string id in (issue.RelatedModIds ?? new List<string>()).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				if (string.Equals(id, _playerSelected.Id, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				XWModManager.ModEntry[] source = _playerEntries.Where((XWModManager.ModEntry entry) => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase)).ToArray();
				Label label2 = PlayerLabel("关联 Mod：" + id, 16, PlayerMuted);
				_playerIssueList.AddChild(label2, forceReadableName: false, InternalMode.Disabled);
				XWModSettingsTextView.BindWidth(label2, this, PlayerDetailWidth);
				HFlowContainer hFlowContainer = new HFlowContainer();
				_playerIssueList.AddChild(hFlowContainer, forceReadableName: false, InternalMode.Disabled);
				if (source.Any((XWModManager.ModEntry entry) => !string.IsNullOrEmpty(entry.PackagePath)))
				{
					Button button = PlayerCompactButton("RelatedMod", "定位关联 Mod", delegate
					{
						JumpPlayerMod(id);
					}, 144);
					button.TooltipText = id + "\n" + string.Join("\n", source.Select((XWModManager.ModEntry entry) => entry.DisplayName));
					hFlowContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
				}
				else
				{
					hFlowContainer.AddChild(PlayerCompactButton("ImportDependency", "导入依赖", ChooseModPackage, 112), forceReadableName: false, InternalMode.Disabled);
					hFlowContainer.AddChild(PlayerCompactButton("CopyDependencyId", "复制依赖 ID", delegate
					{
						DisplayServer.ClipboardSet(id);
					}, 144), forceReadableName: false, InternalMode.Disabled);
				}
			}
		}
		ConfigurePlayerScrollContent(_playerIssueList);
	}

	private void JumpPlayerMod(string id)
	{
		if (!CanUseManagement)
		{
			return;
		}
		ClosePlayerUtility(restoreFocus: false);
		_updatingPlayerFilters = true;
		_playerFilter.Select(0);
		XWModManager.ModEntry[] array = _playerEntries.Where((XWModManager.ModEntry entry) => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase)).ToArray();
		_playerExactId = ((array.Length > 1) ? id : null);
		_playerSearch.Text = ((array.Length > 1) ? id : "");
		_playerSelected = array.FirstOrDefault();
		_updatingPlayerFilters = false;
		RenderPlayerModRows();
		if (_playerSelected != null && _playerRows.TryGetValue(PlayerEntryKey(_playerSelected), out var row))
		{
			Callable.From(delegate
			{
				if (GodotObject.IsInstanceValid(row) && row.IsVisibleInTree())
				{
					row.GrabFocus();
					_playerListScroll.EnsureControlVisible(row);
				}
			}).CallDeferred();
		}
		if (array.Length > 1)
		{
			SetPlayerNotice("同一 ID 存在多个安装包，请按详情中的路径确认。", PlayerGold, showDetails: false);
		}
	}

	private void UpdatePlayerManagementControls(bool busy)
	{
		if (_playerSearch == null)
		{
			return;
		}
		_playerSearch.Editable = !busy;
		_playerFilter.Disabled = busy;
		_playerClearFilter.Disabled = busy;
		_playerPendingView.Disabled = busy;
		string diagnostic;
		bool flag = XWGameRestartService.CanRestart(out diagnostic);
		_playerRestart.Disabled = busy || !flag;
		_playerRestart.TooltipText = (flag ? "重启并应用已保存的更改" : diagnostic);
		_playerRestartReason.Text = (flag ? "" : diagnostic);
		Label playerRestartReason = _playerRestartReason;
		int visible;
		if (!flag)
		{
			XWModPlayerManagementState playerManagement = _playerManagement;
			visible = ((playerManagement != null && playerManagement.PendingCount > 0) ? 1 : 0);
		}
		else
		{
			visible = 0;
		}
		playerRestartReason.Visible = (byte)visible != 0;
		foreach (Button item in _playerIssueList.FindChildren("*", "Button", recursive: true, owned: false).OfType<Button>())
		{
			item.Disabled = busy;
		}
	}

	private void OpenPlayerPendingChanges()
	{
		if (!CanUseManagement || _modConfirmation != null || _playerUtilityOverlay != null || _playerManagement == null)
		{
			return;
		}
		ColorRect overlay = AddPlayerUtilityOverlay("PendingChangesOverlay", new Color(0.15f, 0.08f, 0.02f, 0.58f));
		VBoxContainer content = CreatePlayerDialogContent(overlay, "待应用变更", 760);
		ScrollContainer scroll = PlayerScroll("PendingChangesScroll");
		scroll.CustomMinimumSize = new Vector2(0f, Mathf.Clamp(GetViewportRect().Size.Y - 260f, 100f, 280f));
		content.AddChild(scroll, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer list = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		list.AddThemeConstantOverride("separation", 10);
		scroll.AddChild(list, forceReadableName: false, InternalMode.Disabled);
		foreach (IGrouping<string, XWModPlayerManagementState.Item> group in _playerManagement.Items.Where((XWModPlayerManagementState.Item item) => item.PendingReasons.Count > 0).GroupBy<XWModPlayerManagementState.Item, string>((XWModPlayerManagementState.Item item) => item.Entry.Id, StringComparer.OrdinalIgnoreCase))
		{
			AddText(group.First().Entry.DisplayName + " · " + group.Key, PlayerInk);
			AddText(string.Join("\n", group.SelectMany((XWModPlayerManagementState.Item item) => item.PendingReasons).Distinct()), PlayerGold);
			list.AddChild(PlayerCompactButton("LocatePendingMod", "定位 Mod", delegate
			{
				JumpPlayerMod(group.Key);
			}, 112), forceReadableName: false, InternalMode.Disabled);
			list.AddChild(PlayerRule(), forceReadableName: false, InternalMode.Disabled);
		}
		if (_playerManagement.OrderChanged)
		{
			AddText("加载顺序已调整，重启后生效。", PlayerGold);
		}
		AddText("以上为已保存的更改。存在问题的 Mod 仍需先处理，重启不保证其加载成功。", PlayerMuted);
		ConfigurePlayerScrollContent(list);
		Button button = PlayerCompactButton("ClosePendingChanges", "返回管理", delegate
		{
			ClosePlayerUtility();
		}, 112);
		content.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		button.GrabFocus();
		void AddText(string text, Color color)
		{
			Label label = PlayerLabel(text, 18, color);
			list.AddChild(label, forceReadableName: false, InternalMode.Disabled);
			XWModSettingsTextView.BindWidth(label, overlay, () => Mathf.Min(760f, overlay.Size.X - 64f) - content.GetParent<PanelContainer>().GetThemeStylebox("panel").GetMinimumSize()
				.X - scroll.GetVScrollBar().GetCombinedMinimumSize().X);
		}
	}

	internal void SelectManagementMod(string id)
	{
		JumpPlayerMod(id);
	}

	private void ConfigureManagementOnly()
	{
		GetNode<Control>("Root").Hide();
		base.CustomMinimumSize = Vector2.Zero;
		base.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		base.SizeFlagsVertical = SizeFlags.ExpandFill;
		AddThemeStyleboxOverride("panel", new StyleBoxEmpty());
		VBoxContainer vBoxContainer = new VBoxContainer
		{
			Name = "PlayerLayout"
		};
		vBoxContainer.AddThemeConstantOverride("separation", 12);
		AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		HBoxContainer hBoxContainer = new HBoxContainer
		{
			Name = "ManagementToolbar",
			CustomMinimumSize = new Vector2(0f, 48f)
		};
		hBoxContainer.AddThemeConstantOverride("separation", 12);
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		_playerCounts = PlayerLabel("", 16, PlayerMuted, wrap: false);
		_playerCounts.Name = "InstalledCounts";
		_playerCounts.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		_playerCounts.VerticalAlignment = VerticalAlignment.Center;
		hBoxContainer.AddChild(_playerCounts, forceReadableName: false, InternalMode.Disabled);
		_importModPackageButton = PlayerButton("ImportMod", "安装 Mod", ChooseModPackage, 144);
		hBoxContainer.AddChild(_importModPackageButton, forceReadableName: false, InternalMode.Disabled);
		AddPlayerUpdateAction(hBoxContainer);
		AddExperienceActions(hBoxContainer);
		AddPlayerManagementFilters(vBoxContainer);
		_playerColumns = new HBoxContainer
		{
			Name = "Columns",
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		_playerColumns.AddThemeConstantOverride("separation", 24);
		vBoxContainer.AddChild(_playerColumns, forceReadableName: false, InternalMode.Disabled);
		_playerDirectory = new PanelContainer
		{
			Name = "Directory",
			CustomMinimumSize = new Vector2(300f, 0f)
		};
		StyleBoxFlat styleBoxFlat = PlayerFlat(new Color(0.5f, 0.28f, 0.08f, 0.05f), new Color(0.5f, 0.28f, 0.08f, 0.25f), 0, 0);
		styleBoxFlat.BorderWidthRight = 1;
		styleBoxFlat.ContentMarginRight = 16f;
		styleBoxFlat.ContentMarginTop = 8f;
		styleBoxFlat.ContentMarginBottom = 8f;
		_playerDirectory.AddThemeStyleboxOverride("panel", styleBoxFlat);
		_playerColumns.AddChild(_playerDirectory, forceReadableName: false, InternalMode.Disabled);
		_playerListScroll = PlayerScroll("ModListScroll");
		_playerDirectory.AddChild(_playerListScroll, forceReadableName: false, InternalMode.Disabled);
		_playerList = new VBoxContainer
		{
			Name = "ModList",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		_playerList.AddThemeConstantOverride("separation", 8);
		_playerListScroll.AddChild(_playerList, forceReadableName: false, InternalMode.Disabled);
		_playerDetailScroll = PlayerScroll("DetailScroll");
		_playerColumns.AddChild(_playerDetailScroll, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer vBoxContainer2 = new VBoxContainer
		{
			Name = "SelectedMod",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		vBoxContainer2.AddThemeConstantOverride("separation", 12);
		_playerDetailScroll.AddChild(vBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		_playerName = PlayerLabel("", 26, PlayerInk);
		_playerName.Name = "ModName";
		_playerName.MaxLinesVisible = 2;
		_playerName.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		vBoxContainer2.AddChild(_playerName, forceReadableName: false, InternalMode.Disabled);
		_playerAuthor = PlayerLabel("", 16, PlayerMuted);
		vBoxContainer2.AddChild(_playerAuthor, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer2.AddChild(PlayerRule(), forceReadableName: false, InternalMode.Disabled);
		_playerDescription = PlayerLabel("", 20, PlayerInk);
		_playerDescription.Name = "ModDescription";
		vBoxContainer2.AddChild(_playerDescription, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(_playerDescription, this, PlayerDetailWidth);
		vBoxContainer2.AddChild(PlayerRule(), forceReadableName: false, InternalMode.Disabled);
		AddSelectedExperienceActions(vBoxContainer2);
		vBoxContainer2.AddChild(new Control
		{
			CustomMinimumSize = new Vector2(0f, 8f),
			MouseFilter = MouseFilterEnum.Ignore
		}, forceReadableName: false, InternalMode.Disabled);
		_playerEnabled = PlayerLabel("", 20, PlayerInk);
		_playerEnabled.Name = "EnabledSetting";
		_playerRunning = PlayerLabel("", 20, PlayerInk);
		_playerRunning.Name = "RunningState";
		_playerVersions = PlayerLabel("", 16, PlayerMuted);
		_playerMode = PlayerLabel("", 16, PlayerMuted);
		_playerFailure = PlayerLabel("", 16, PlayerError);
		Label[] array = new Label[5] { _playerEnabled, _playerRunning, _playerVersions, _playerMode, _playerFailure };
		foreach (Label node in array)
		{
			vBoxContainer2.AddChild(node, forceReadableName: false, InternalMode.Disabled);
		}
		AddPlayerManagementDetails(vBoxContainer2);
		ConfigurePlayerScrollContent(vBoxContainer2);
		_playerEmpty = new CenterContainer
		{
			Name = "EmptyState",
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		vBoxContainer.AddChild(_playerEmpty, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer vBoxContainer3 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(400f, 0f)
		};
		vBoxContainer3.AddThemeConstantOverride("separation", 20);
		_playerEmpty.AddChild(vBoxContainer3, forceReadableName: false, InternalMode.Disabled);
		Label label = PlayerLabel("还没有安装 Mod", 26, PlayerInk);
		label.HorizontalAlignment = HorizontalAlignment.Center;
		vBoxContainer3.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		Label label2 = PlayerLabel("选择下载好的 .pmod 文件，安装并启用后重启游戏。", 20, PlayerMuted);
		label2.HorizontalAlignment = HorizontalAlignment.Center;
		vBoxContainer3.AddChild(label2, forceReadableName: false, InternalMode.Disabled);
		_playerEmptyImport = PlayerButton("EmptyImport", "安装 Mod", ChooseModPackage, 180);
		_playerEmptyImport.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		vBoxContainer3.AddChild(_playerEmptyImport, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer3.AddChild(PlayerButton("EmptyHelp", "如何安装和使用", OpenPlayerHelp, 180, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		AddPlayerPendingBar(vBoxContainer);
		HBoxContainer hBoxContainer2 = new HBoxContainer
		{
			Name = "ManagementFooter",
			CustomMinimumSize = new Vector2(0f, 48f)
		};
		hBoxContainer2.AddThemeConstantOverride("separation", 16);
		vBoxContainer.AddChild(hBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		_playerReturn = PlayerButton("BackToOptions", "返回选项", delegate
		{
			ManagementReturnRequested?.Invoke();
		}, 144, "ModSecondaryButton");
		hBoxContainer2.AddChild(_playerReturn, forceReadableName: false, InternalMode.Disabled);
		_playerNotice = PlayerLabel("", 16, PlayerGold, wrap: false);
		_playerNotice.Name = "OperationNotice";
		_playerNotice.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		_playerNotice.VerticalAlignment = VerticalAlignment.Center;
		hBoxContainer2.AddChild(_playerNotice, forceReadableName: false, InternalMode.Disabled);
		_playerToggle = PlayerButton("ToggleEnabled", "启用", delegate
		{
			SetSelectedModEnabled(!IsSelectedModEnabled());
		}, 144);
		hBoxContainer2.AddChild(_playerToggle, forceReadableName: false, InternalMode.Disabled);
		_playerSettings = PlayerButton("ConfigureMod", "配置", OpenPlayerSettings, 96, "ModSecondaryButton");
		hBoxContainer2.AddChild(_playerSettings, forceReadableName: false, InternalMode.Disabled);
		_playerMore = PlayerButton("MoreActions", "更多", OpenPlayerMore, 104, "ModSecondaryButton");
		hBoxContainer2.AddChild(_playerMore, forceReadableName: false, InternalMode.Disabled);
		base.Resized += ResizePlayerDirectory;
		ResizePlayerDirectory();
		_playerReturn.GrabFocus();
	}

	private void ResizePlayerDirectory()
	{
		if (GodotObject.IsInstanceValid(_playerDirectory))
		{
			_playerDirectory.CustomMinimumSize = new Vector2(Mathf.Clamp(base.Size.X * 0.3f, 240f, 300f), 0f);
		}
	}

	private void RefreshPlayerManagement(List<XWModManager.ModEntry> entries, int enabledCount, List<XWValidationIssue> issues, XWModEnvironmentStatus environment, string directory)
	{
		string selectedKey = PlayerEntryKey(_playerSelected);
		bool flag = _playerRows.Values.Any((Button row) => row.HasFocus());
		int scrollVertical = _playerListScroll.ScrollVertical;
		_playerEntries = entries;
		_playerColumns.Visible = entries.Count > 0;
		_playerEmpty.Visible = entries.Count == 0;
		_importModPackageButton.Visible = entries.Count > 0;
		_playerToggle.Visible = entries.Count > 0;
		_playerSelected = entries.Find((XWModManager.ModEntry entry) => PlayerEntryKey(entry).Equals(selectedKey, StringComparison.OrdinalIgnoreCase)) ?? entries.FirstOrDefault();
		int value = entries.Count((XWModManager.ModEntry entry) => !string.IsNullOrEmpty(entry.PackagePath));
		_playerCounts.Text = $"已安装 {value}    当前运行 {entries.Count((XWModManager.ModEntry entry) => entry.Loaded)}    下次启用 {enabledCount}";
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
		handler.AppendLiteral("安装目录：");
		handler.AppendFormatted(directory);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(8, 2, stringBuilder2);
		handler.AppendLiteral("运行环境：");
		handler.AppendFormatted(environment.Ready ? "已应用" : "尚未应用");
		handler.AppendLiteral(" / ");
		handler.AppendFormatted(environment.Reason);
		stringBuilder4.AppendLine(ref handler);
		foreach (XWValidationIssue issue in issues)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(issue.Code);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(issue.Message);
			stringBuilder5.AppendLine(ref handler);
		}
		XWModManager xWModManager = new XWModManager();
		TryPreviewPlayerLoadPlan(xWModManager, null, out var plan, out var diagnostic);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
		handler.AppendLiteral("安全模式：");
		handler.AppendFormatted(xWModManager.IsSafeMode);
		handler.AppendLiteral("\n");
		handler.AppendFormatted(xWModManager.RecoveryDiagnostic);
		stringBuilder6.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder7 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
		handler.AppendLiteral("当前运行环境 SHA256：");
		handler.AppendFormatted(environment.Snapshot?.Sha256 ?? "尚无可用快照");
		stringBuilder7.AppendLine(ref handler);
		XWModSnapshotEntry[] array = environment.Snapshot?.Mods.OrderBy((XWModSnapshotEntry mod) => mod.LoadOrderIndex).ToArray();
		stringBuilder.AppendLine("当前实际顺序：" + ((array == null) ? "尚无可用快照" : string.Join(" → ", array.Select((XWModSnapshotEntry mod) => mod.Id))));
		if (array != null)
		{
			XWModSnapshotEntry[] array2 = array;
			foreach (XWModSnapshotEntry xWModSnapshotEntry in array2)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(13, 3, stringBuilder2);
				handler.AppendLiteral("运行包 ");
				handler.AppendFormatted(xWModSnapshotEntry.Id);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(xWModSnapshotEntry.Version);
				handler.AppendLiteral(" SHA256=");
				handler.AppendFormatted(xWModSnapshotEntry.PackageSha256);
				stringBuilder8.AppendLine(ref handler);
			}
		}
		stringBuilder.AppendLine((plan == null) ? ("下次计划无法读取：" + diagnostic) : DescribePlayerLoadPlan(plan));
		UpdatePlayerManagementState(issues.Concat(plan?.Issues ?? Array.Empty<XWValidationIssue>()), array?.Select((XWModSnapshotEntry mod) => mod.Id).ToArray() ?? XWModEnvironmentService.GetAppliedOrder(), plan?.LoadOrder.Select((XWModManager.ModEntry mod) => mod.Id).ToArray());
		_playerSnapshotDetails = stringBuilder.ToString();
		bool flag2 = !environment.Ready;
		if (flag2)
		{
			bool flag3;
			switch (environment.Reason)
			{
			case "LifecycleFailed":
			case "ApplyFailed":
			case "StorageRecoveryBlocked":
			case "InvalidEnabledState":
			case "NonFormalRuntime":
				flag3 = true;
				break;
			default:
				flag3 = false;
				break;
			}
			flag2 = flag3;
		}
		bool flag4 = flag2;
		flag2 = !environment.Ready;
		if (flag2)
		{
			bool flag3;
			switch (environment.Reason)
			{
			case "EnabledSetChanged":
			case "LifecycleRollback":
			case "InstalledPackageChanged":
			case "ManagedRuntimeChanged":
			case "RuntimeUnloaded":
			case "RuntimeApplied":
			case "PackageExtracted":
				flag3 = true;
				break;
			default:
				flag3 = false;
				break;
			}
			flag2 = flag3;
		}
		bool flag5 = flag2;
		bool flag6 = plan == null || flag4 || issues.Any((XWValidationIssue issue) => issue.Level == XWValidationIssue.Severity.Error) || _playerManagement.Items.Any((XWModPlayerManagementState.Item item) => item.Issues.Any((XWValidationIssue issue) => issue.Level == XWValidationIssue.Severity.Error));
		bool flag7 = _playerManagement.PendingCount > 0 || flag5;
		_playerSnapshotNotice = (xWModManager.IsSafeMode ? "安全模式：本次未加载 Mod，请在“更多 → 加载与恢复”中处理。" : (flag6 ? "部分 Mod 存在问题，请查看状态和详情。" : (flag7 ? "有更改尚未应用，请重启游戏。" : ((!environment.Ready) ? "Mod 运行环境尚未就绪，请查看详情。" : ""))));
		_playerSnapshotNoticeColor = (flag6 ? PlayerError : PlayerGold);
		SetPlayerNotice(_playerSnapshotNotice, _playerSnapshotNoticeColor, flag6 || (!environment.Ready && !flag7));
		RenderPlayerModRows();
		_playerListScroll.SetDeferred(ScrollContainer.PropertyName.ScrollVertical, PlayerEntryKey(_playerSelected).Equals(selectedKey, StringComparison.OrdinalIgnoreCase) ? scrollVertical : 0);
		if (flag && _playerSelected != null && _playerRows.TryGetValue(PlayerEntryKey(_playerSelected), out var value2))
		{
			value2.GrabFocus();
		}
	}

	private void SelectPlayerMod(XWModManager.ModEntry entry)
	{
		if (CanUseManagement && _modConfirmation == null && !(_filePickerToken != Guid.Empty) && _playerUtilityOverlay == null)
		{
			bool num = PlayerEntryKey(entry) != PlayerEntryKey(_playerSelected);
			_playerSelected = entry;
			if (num)
			{
				_playerDetailScroll.ScrollVertical = 0;
			}
			RenderPlayerSelection();
		}
	}

	private void RenderPlayerSelection()
	{
		XWModManager.ModEntry playerSelected = _playerSelected;
		foreach (KeyValuePair<string, Button> playerRow in _playerRows)
		{
			playerRow.Value.SetPressedNoSignal(playerRow.Key.Equals(PlayerEntryKey(playerSelected), StringComparison.OrdinalIgnoreCase));
			playerRow.Value.GetNode<ColorRect>("Bookmark").Visible = playerRow.Value.ButtonPressed;
		}
		if (playerSelected != null)
		{
			_playerName.Text = playerSelected.DisplayName;
			_playerName.TooltipText = playerSelected.DisplayName;
			_playerName.MouseFilter = MouseFilterEnum.Pass;
			_playerAuthor.Text = (string.IsNullOrWhiteSpace(playerSelected.Manifest?.Author) ? VersionLabel(playerSelected.Version) : (VersionLabel(playerSelected.Version) + "  ·  " + playerSelected.Manifest.Author));
			_playerDescription.Text = (string.IsNullOrWhiteSpace(playerSelected.Manifest?.Description) ? "作者尚未填写介绍。" : playerSelected.Manifest.Description);
			_playerEnabled.Visible = false;
			_playerRunning.Text = PlayerState(playerSelected);
			_playerRunning.AddThemeColorOverride("font_color", PlayerStateColor(playerSelected));
			_playerVersions.Visible = playerSelected.Loaded && (playerSelected.InstalledVersion != playerSelected.RunningVersion || playerSelected.EffectiveState == "已加载旧版本");
			_playerVersions.Text = "安装版本：" + VersionLabel(playerSelected.InstalledVersion) + "    运行版本：" + VersionLabel(playerSelected.RunningVersion);
			_playerMode.Visible = false;
			_playerMode.Text = "运行内容：" + playerSelected.EffectiveMode;
			_playerFailure.Visible = false;
			_playerToggle.Text = (playerSelected.Enabled ? "停用" : "启用");
		}
		RenderPlayerManagementDetails();
		RenderSelectedExperience();
		UpdatePlayerActionState();
		UpdatePlayerTechnicalText();
	}

	private void UpdatePlayerActionState()
	{
		if (!ManagementOnly || !GodotObject.IsInstanceValid(_playerMore))
		{
			return;
		}
		bool flag = _filePickerToken != Guid.Empty || _modConfirmation != null || _playerUtilityOverlay != null || XWGameRestartService.IsRestartRequested || !CanUseManagement;
		_importModPackageButton.Disabled = flag;
		_playerEmptyImport.Disabled = flag;
		_playerReturn.Disabled = flag;
		_playerMore.Disabled = flag;
		if (GodotObject.IsInstanceValid(_playerUpdates))
		{
			_playerUpdates.Disabled = flag;
		}
		if (_playerPlay != null)
		{
			_playerPlay.Disabled = flag;
		}
		if (_playerSelectedPlay != null)
		{
			_playerSelectedPlay.Disabled = flag || !CanPlaySelectedMod();
		}
		foreach (Button value in _playerRows.Values)
		{
			value.Disabled = flag;
		}
		_playerToggle.Disabled = flag || _playerSelected == null || (!_playerSelected.Enabled && (!_playerSelected.HasManifest || _playerSelected.PendingDelete));
		UpdatePlayerSettingsAction(flag);
		UpdatePlayerManagementControls(flag);
	}

	internal bool TryClosePlayerUtility()
	{
		OptionButton playerFilter = _playerFilter;
		if (playerFilter != null && playerFilter.GetPopup().Visible)
		{
			_playerFilter.GetPopup().Hide();
			_playerFilter.GrabFocus();
			return true;
		}
		if (_playerSettingsEditor == null)
		{
			return ClosePlayerUtility();
		}
		return RequestClosePlayerSettings();
	}

	private bool ClosePlayerUtility(bool restoreFocus = true)
	{
		if (_playerUtilityOverlay == null)
		{
			return false;
		}
		bool flag = _playerSettingsEditor != null;
		bool playerUtilityIsUpdates = _playerUtilityIsUpdates;
		_playerUtilityIsUpdates = false;
		DisposePlayerSettings();
		if (GodotObject.IsInstanceValid(_playerUtilityOverlay))
		{
			_playerUtilityOverlay.Hide();
			_playerUtilityOverlay.QueueFree();
		}
		_playerUtilityOverlay = null;
		_playerTechnical = null;
		if (CanUseManagement)
		{
			UpdatePlayerActionState();
			if (restoreFocus)
			{
				if (flag && _playerSettings.Visible && !_playerSettings.Disabled)
				{
					_playerSettings.GrabFocus();
				}
				else if (playerUtilityIsUpdates && GodotObject.IsInstanceValid(_playerUpdates) && !_playerUpdates.Disabled)
				{
					_playerUpdates.GrabFocus();
				}
				else
				{
					_playerMore.GrabFocus();
				}
			}
		}
		return true;
	}

	private ColorRect AddPlayerUtilityOverlay(string name, Color shade)
	{
		ColorRect colorRect = (ColorRect)(_playerUtilityOverlay = new ColorRect
		{
			Name = name,
			Color = shade,
			MouseFilter = MouseFilterEnum.Stop
		});
		(GodotObject.IsInstanceValid(ManagementConfirmationHost) ? ManagementConfirmationHost : this).AddChild(colorRect, forceReadableName: false, InternalMode.Disabled);
		colorRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		UpdatePlayerActionState();
		return colorRect;
	}

	private void OpenPlayerMore()
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty || _playerUtilityOverlay != null)
		{
			return;
		}
		ColorRect overlay = AddPlayerUtilityOverlay("MoreActionsOverlay", Colors.Transparent);
		overlay.GuiInput += delegate(InputEvent input)
		{
			if (_playerUtilityOverlay == overlay && input is InputEventMouseButton { Pressed: not false } inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				ClosePlayerUtility();
			}
		};
		PanelContainer panelContainer = new PanelContainer
		{
			Name = "MoreMenu",
			MouseFilter = MouseFilterEnum.Stop
		};
		StyleBoxTexture styleBoxTexture = CreatePlayerPaperFrame();
		float num = (styleBoxTexture.ContentMarginBottom = 12f);
		float num3 = (styleBoxTexture.ContentMarginTop = num);
		float contentMarginLeft = (styleBoxTexture.ContentMarginRight = num3);
		styleBoxTexture.ContentMarginLeft = contentMarginLeft;
		panelContainer.AddThemeStyleboxOverride("panel", styleBoxTexture);
		overlay.AddChild(panelContainer, forceReadableName: false, InternalMode.Disabled);
		panelContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight, LayoutPresetMode.Minsize);
		panelContainer.OffsetLeft = -260f;
		panelContainer.OffsetRight = -36f;
		float num6 = Mathf.Clamp(base.Size.Y - 160f, 96f, 400f);
		panelContainer.OffsetTop = -120f - num6;
		panelContainer.OffsetBottom = -96f;
		VBoxContainer vBoxContainer = new VBoxContainer();
		vBoxContainer.AddThemeConstantOverride("separation", 8);
		ScrollContainer scrollContainer = PlayerScroll("MoreActionsScroll");
		scrollContainer.CustomMinimumSize = new Vector2(200f, num6);
		panelContainer.AddChild(scrollContainer, forceReadableName: false, InternalMode.Disabled);
		scrollContainer.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		Button button = PlayerButton("RefreshMods", "刷新", delegate
		{
			ClosePlayerUtility(restoreFocus: false);
			RefreshModList();
		}, 200, "ModSecondaryButton");
		vBoxContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer.AddChild(PlayerButton("ModHelp", "如何安装和使用", delegate
		{
			ClosePlayerUtility(restoreFocus: false);
			OpenPlayerHelp();
		}, 200, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		if (!Global.IsMobile && ManagementCreateRequested != null)
		{
			vBoxContainer.AddChild(PlayerButton("CreateMod", "制作 Mod（F3）", delegate
			{
				ClosePlayerUtility(restoreFocus: false);
				ManagementCreateRequested();
			}, 200, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		}
		Button node = PlayerButton("ViewDiagnostics", "查看诊断", delegate
		{
			if (CanUseManagement && _playerUtilityOverlay == overlay)
			{
				ClosePlayerUtility(restoreFocus: false);
				OpenPlayerDiagnostics();
			}
		}, 200, "ModSecondaryButton");
		vBoxContainer.AddChild(node, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer.AddChild(PlayerButton("LoadPlan", "加载与恢复", delegate
		{
			if (CanUseManagement && _playerUtilityOverlay == overlay)
			{
				ClosePlayerUtility(restoreFocus: false);
				OpenPlayerLoadPlan();
			}
		}, 200, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		Button button2 = PlayerButton("DeletePackage", "删除安装包", delegate
		{
			if (CanUseManagement && _playerUtilityOverlay == overlay)
			{
				ClosePlayerUtility(restoreFocus: false);
				ConfirmDeleteSelectedMod();
			}
		}, 200, "ModDangerButton");
		button2.Disabled = _playerSelected == null || string.IsNullOrEmpty(_playerSelected.PackagePath) || _playerSelected.PendingDelete;
		vBoxContainer.AddChild(button2, forceReadableName: false, InternalMode.Disabled);
		AddProgressRecoveryActions(vBoxContainer);
		ConfigurePlayerScrollContent(vBoxContainer);
		button.GrabFocus();
	}

	private void OpenPlayerDiagnostics()
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty || _playerUtilityOverlay != null)
		{
			return;
		}
		ColorRect overlay = AddPlayerUtilityOverlay("DiagnosticsOverlay", new Color(0.15f, 0.08f, 0.02f, 0.58f));
		VBoxContainer vBoxContainer = CreatePlayerDialogContent(overlay, "Mod 诊断", 760);
		_playerTechnical = new RichTextLabel
		{
			Name = "TechnicalText",
			BbcodeEnabled = false,
			SelectionEnabled = true,
			CustomMinimumSize = new Vector2(0f, Mathf.Min(300f, GetViewportRect().Size.Y - 240f)),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			ScrollActive = true,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		_playerTechnical.AddThemeFontSizeOverride("normal_font_size", 16);
		vBoxContainer.AddChild(_playerTechnical, forceReadableName: false, InternalMode.Disabled);
		UpdatePlayerTechnicalText();
		Button button = PlayerButton("CloseDiagnostics", "返回管理", delegate
		{
			if (_playerUtilityOverlay == overlay)
			{
				ClosePlayerUtility();
			}
		}, 144, "ModSecondaryButton");
		button.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		HBoxContainer hBoxContainer = new HBoxContainer();
		hBoxContainer.AddThemeConstantOverride("separation", 12);
		hBoxContainer.AddChild(PlayerButton("CopyDiagnostics", "复制诊断", delegate
		{
			if (_playerUtilityOverlay == overlay)
			{
				DisplayServer.ClipboardSet(_playerTechnical.Text);
			}
		}, 144, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		button.GrabFocus();
	}

	private static bool TryPreviewPlayerLoadPlan(XWModManager manager, IEnumerable<string> order, out XWModLoadPlan plan, out string diagnostic)
	{
		try
		{
			plan = manager.PreviewLoadPlan(order);
			diagnostic = "";
			return true;
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException) ? 1 : 0) != 0)
		{
			plan = null;
			diagnostic = ex.Message;
			return false;
		}
	}

	private static string DescribePlayerLoadPlan(XWModLoadPlan plan, bool technical = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (technical)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
			handler.AppendLiteral("下次启动计划 SHA256：");
			handler.AppendFormatted(plan.PlanHash);
			stringBuilder2.AppendLine(ref handler);
		}
		stringBuilder.AppendLine("有效加载顺序：" + ((plan.LoadOrder.Count == 0) ? "无" : string.Join(" → ", plan.LoadOrder.Select((XWModManager.ModEntry entry) => entry.Id))));
		foreach (XWValidationIssue issue in plan.Issues)
		{
			stringBuilder.AppendLine(technical ? $"{issue.Level} / {issue.Code} / {issue.ResourceKey}：{issue.Message}" : issue.Message);
		}
		return stringBuilder.ToString();
	}

	private void OpenPlayerLoadPlan()
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty || _playerUtilityOverlay != null)
		{
			return;
		}
		XWModManager manager = new XWModManager();
		XWModSelectionState state;
		string error;
		bool selectionReadable = manager.TryGetSelection(out state, out error);
		List<string> draft = (selectionReadable ? new List<string>(state.PreferredOrder) : new List<string>());
		XWModLoadPlan initialPlan = null;
		string previewError = "";
		bool safeMode = manager.IsSafeMode;
		string recoveryDiagnostic = manager.RecoveryDiagnostic;
		if (selectionReadable)
		{
			TryPreviewPlayerLoadPlan(manager, null, out initialPlan, out previewError);
		}
		if (initialPlan != null)
		{
			foreach (XWModManager.ModEntry item in initialPlan.LoadOrder)
			{
				if (!draft.Contains<string>(item.Id, StringComparer.OrdinalIgnoreCase))
				{
					draft.Add(item.Id);
				}
			}
		}
		foreach (string item2 in selectionReadable ? state.EnabledIds : new List<string>())
		{
			if (!draft.Contains<string>(item2, StringComparer.OrdinalIgnoreCase))
			{
				draft.Add(item2);
			}
		}
		ColorRect overlay = AddPlayerUtilityOverlay("LoadPlanOverlay", new Color(0.15f, 0.08f, 0.02f, 0.58f));
		VBoxContainer vBoxContainer = CreatePlayerDialogContent(overlay, "加载顺序与启动恢复", 760);
		vBoxContainer.AddChild(PlayerLabel("调整的是优先顺序；依赖必须先加载。内容覆盖以后加载者为准。保存后重启生效。", 16, PlayerMuted), forceReadableName: false, InternalMode.Disabled);
		HBoxContainer hBoxContainer = new HBoxContainer();
		hBoxContainer.AddThemeConstantOverride("separation", 8);
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		OptionButton choices = new OptionButton
		{
			Name = "PreferredModOrder",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		hBoxContainer.AddChild(choices, forceReadableName: false, InternalMode.Disabled);
		RichTextLabel preview = new RichTextLabel
		{
			Name = "LoadPlanPreview",
			BbcodeEnabled = false,
			SelectionEnabled = true,
			CustomMinimumSize = new Vector2(0f, Mathf.Max(100f, Mathf.Min(220f, GetViewportRect().Size.Y - 400f))),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			ScrollActive = true,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		preview.AddThemeFontSizeOverride("normal_font_size", 16);
		Button up = null;
		Button down = null;
		up = PlayerButton("MoveModUp", "上移", delegate
		{
			Move(-1);
		}, 80, "ModSecondaryButton");
		down = PlayerButton("MoveModDown", "下移", delegate
		{
			Move(1);
		}, 80, "ModSecondaryButton");
		hBoxContainer.AddChild(up, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(down, forceReadableName: false, InternalMode.Disabled);
		choices.ItemSelected += delegate
		{
			up.Disabled = choices.Selected <= 0;
			down.Disabled = choices.Selected >= draft.Count - 1;
		};
		vBoxContainer.AddChild(preview, forceReadableName: false, InternalMode.Disabled);
		if (safeMode || !selectionReadable)
		{
			HBoxContainer hBoxContainer2 = new HBoxContainer();
			hBoxContainer2.AddThemeConstantOverride("separation", 12);
			hBoxContainer2.AddChild(PlayerButton("RetryStartup", "下次重新尝试", delegate
			{
				Recover(restore: false);
			}, 180, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
			hBoxContainer2.AddChild(PlayerButton("RestoreLastGood", "恢复上次成功配置", delegate
			{
				Recover(restore: true);
			}, 220, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
			vBoxContainer.AddChild(hBoxContainer2, forceReadableName: false, InternalMode.Disabled);
			vBoxContainer.AddChild(PlayerButton("ResetSelectionRecovery", "备份并清空启用列表", delegate
			{
				if (_playerUtilityOverlay == overlay)
				{
					ClosePlayerUtility(restoreFocus: false);
					ShowModConfirmation("重置 Mod 启用设置", "将备份当前启用设置，再清空启用列表和加载顺序。安装包、Mod 配置与玩家存档会保留。\n下次重启将不启用任何 Mod，之后可以重新选择。确认继续？", "备份并清空", delegate
					{
						string diagnostic;
						bool flag = manager.TryResetSelectionForRecovery(out diagnostic);
						RefreshModList();
						ShowManagementResult(flag ? "启用列表已清空" : "恢复失败", flag ? "请重启游戏；本次运行状态保持不变。" : "恢复未完成，请查看诊断。", !flag, diagnostic, flag);
					});
				}
			}, 240, "ModDangerButton"), forceReadableName: false, InternalMode.Disabled);
		}
		HBoxContainer hBoxContainer3 = new HBoxContainer();
		hBoxContainer3.AddThemeConstantOverride("separation", 12);
		hBoxContainer3.AddChild(PlayerButton("ResetOrder", "恢复默认顺序", delegate
		{
			draft.Clear();
			Render();
		}, 180, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		hBoxContainer3.AddChild(PlayerButton("CancelOrder", "取消", delegate
		{
			if (_playerUtilityOverlay == overlay)
			{
				ClosePlayerUtility();
			}
		}, 96, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		hBoxContainer3.AddChild(PlayerButton("SaveOrder", "保存顺序", delegate
		{
			if (_playerUtilityOverlay == overlay)
			{
				if (!manager.TryGetSelection(out var state2, out var diagnostic))
				{
					preview.Text = diagnostic;
				}
				else
				{
					bool flag = !state2.PreferredOrder.SequenceEqual<string>(draft, StringComparer.OrdinalIgnoreCase);
					if (!manager.TrySetPreferredOrder(draft, out diagnostic))
					{
						preview.Text = diagnostic;
					}
					else
					{
						ClosePlayerUtility(restoreFocus: false);
						RefreshModList();
						ShowManagementResult("顺序已保存", flag ? "请重启游戏后应用加载顺序。" : "加载顺序未变化。", failure: false, diagnostic, flag);
					}
				}
			}
		}, 144), forceReadableName: false, InternalMode.Disabled);
		foreach (Node child in hBoxContainer3.GetChildren())
		{
			if (child is Button button && button.Name != (StringName)"CancelOrder")
			{
				button.Disabled = !selectionReadable;
			}
		}
		vBoxContainer.AddChild(hBoxContainer3, forceReadableName: false, InternalMode.Disabled);
		Render();
		choices.GrabFocus();
		void Move(int delta)
		{
			if (_playerUtilityOverlay == overlay)
			{
				int selected = choices.Selected;
				int num = selected + delta;
				if (selected >= 0 && num >= 0 && num < draft.Count)
				{
					string selected2 = draft[selected];
					List<string> list = draft;
					int index = selected;
					List<string> list2 = draft;
					int index2 = num;
					string value = draft[num];
					string value2 = draft[selected];
					list[index] = value;
					list2[index2] = value2;
					Render(selected2);
				}
			}
		}
		void Recover(bool restore)
		{
			if (_playerUtilityOverlay == overlay)
			{
				if (!(restore ? manager.TryRestoreLastGood(out var diagnostic) : manager.TryRequestStartupRetry(out diagnostic)))
				{
					preview.Text = diagnostic;
				}
				else
				{
					ClosePlayerUtility(restoreFocus: false);
					RefreshModList();
					ShowManagementResult("恢复设置已保存", "请重启游戏；本次仍保持安全模式。", failure: false, diagnostic, restartRequired: true);
				}
			}
		}
		void Render(string selected = null)
		{
			choices.Clear();
			foreach (string item3 in draft)
			{
				choices.AddItem(item3);
			}
			int val = ((selected != null) ? draft.FindIndex((string id) => string.Equals(id, selected, StringComparison.OrdinalIgnoreCase)) : 0);
			if (draft.Count > 0)
			{
				choices.Select(Math.Max(0, val));
			}
			choices.Disabled = draft.Count == 0;
			up.Disabled = choices.Selected <= 0;
			down.Disabled = choices.Selected < 0 || choices.Selected >= draft.Count - 1;
			string text = ((!selectionReadable) ? ("加载设置无法读取：" + error) : ((initialPlan != null) ? DescribePlayerLoadPlan(initialPlan.Reorder(draft), technical: false) : ("加载计划无法读取：" + previewError)));
			preview.Text = (safeMode ? ("本次安全模式：" + recoveryDiagnostic + "\n\n") : "") + text;
		}
	}

	private void RestorePlayerManagementFocus()
	{
		if (CanUseManagement)
		{
			if (_playerToggle.Visible && !_playerToggle.Disabled)
			{
				_playerToggle.GrabFocus();
			}
			else if (_playerEmptyImport.Visible && _playerEmpty.Visible && !_playerEmptyImport.Disabled)
			{
				_playerEmptyImport.GrabFocus();
			}
			else if (!_playerMore.Disabled)
			{
				_playerMore.GrabFocus();
			}
		}
	}

	private void UpdatePlayerTechnicalText()
	{
		if (_playerTechnical != null)
		{
			StringBuilder stringBuilder = new StringBuilder(_playerSnapshotDetails);
			XWModManager.ModEntry playerSelected = _playerSelected;
			if (playerSelected != null)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(13, 2, stringBuilder2);
				handler.AppendLiteral("\nMod ID：");
				handler.AppendFormatted(playerSelected.Id);
				handler.AppendLiteral("\n安装包：");
				handler.AppendFormatted(playerSelected.PackagePath);
				stringBuilder3.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(11, 2, stringBuilder2);
				handler.AppendLiteral("安装版本：");
				handler.AppendFormatted(playerSelected.InstalledVersion);
				handler.AppendLiteral("\n运行版本：");
				handler.AppendFormatted(playerSelected.RunningVersion);
				stringBuilder4.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(21, 2, stringBuilder2);
				handler.AppendLiteral("安装 SHA256：");
				handler.AppendFormatted(playerSelected.PackageSha256);
				handler.AppendLiteral("\n运行 SHA256：");
				handler.AppendFormatted(playerSelected.RunningSha256);
				stringBuilder5.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(12, 3, stringBuilder2);
				handler.AppendLiteral("实际模式：");
				handler.AppendFormatted(playerSelected.EffectiveMode);
				handler.AppendLiteral("\n最近失败：");
				handler.AppendFormatted(playerSelected.LastApplyFailure);
				handler.AppendLiteral("\n");
				handler.AppendFormatted(playerSelected.Diagnostic);
				stringBuilder6.AppendLine(ref handler);
				AppendExtensionDiagnostics(stringBuilder, playerSelected.Id);
			}
			if (!string.IsNullOrWhiteSpace(_playerOperationDetails))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder7 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
				handler.AppendLiteral("\n最近操作：\n");
				handler.AppendFormatted(_playerOperationDetails);
				stringBuilder7.AppendLine(ref handler);
			}
			_playerTechnical.Text = stringBuilder.ToString();
		}
	}

	private void ShowManagementResult(string title, string message, bool failure = false, string details = "", bool restartRequired = false)
	{
		if (!ManagementOnly)
		{
			SetResult("[b]" + EscapeBbcode(title) + "[/b]\n" + EscapeBbcode(message) + (string.IsNullOrWhiteSpace(details) ? "" : ("\n" + EscapeBbcode(details))));
		}
		else if (CanUseManagement)
		{
			_playerOperationDetails = $"{title}\n{message}\n{details}";
			string message2 = title + "：" + ShortPlayerMessage(message);
			if (!failure && !string.IsNullOrWhiteSpace(_playerSnapshotNotice))
			{
				message2 = title + "。" + _playerSnapshotNotice;
			}
			SetPlayerNotice(message2, failure ? PlayerError : ((_playerSnapshotNotice.Length > 0) ? _playerSnapshotNoticeColor : PlayerGreen), failure || !string.IsNullOrWhiteSpace(details));
			UpdatePlayerTechnicalText();
		}
	}

	private void RestartFromManagement()
	{
		if (ManagementOnly && CanUseManagement && !XWGameRestartService.IsRestartRequested)
		{
			if (!XWGameRestartService.TryRestart(out var diagnostic))
			{
				ShowManagementResult("未能重启", diagnostic, failure: true);
				return;
			}
			SetPlayerNotice("正在重启游戏…", PlayerGold, showDetails: false);
			UpdatePlayerActionState();
		}
	}

	private void SetPlayerNotice(string message, Color color, bool showDetails)
	{
		_playerNotice.Text = message;
		_playerNotice.TooltipText = message + (showDetails ? "\n更多 → 查看诊断" : "");
		_playerNotice.MouseFilter = MouseFilterEnum.Pass;
		_playerNotice.AddThemeColorOverride("font_color", color);
	}

	private static string PlayerEntryKey(XWModManager.ModEntry entry)
	{
		return XWModPlayerManagementState.Key(entry);
	}

	private static string FormatManagementIssues(IEnumerable<XWValidationIssue> issues)
	{
		return string.Join("\n", issues.Select((XWValidationIssue issue) => $"{issue.Code}: {issue.Message}"));
	}

	private static string VersionLabel(string version)
	{
		if (!string.IsNullOrWhiteSpace(version))
		{
			return "v" + version;
		}
		return "版本未提供";
	}

	private static bool PlayerHasFailure(XWModManager.ModEntry entry)
	{
		bool flag = !entry.HasManifest || (entry.Enabled && !string.IsNullOrWhiteSpace(entry.LastApplyFailure));
		if (!flag)
		{
			string effectiveState = entry.EffectiveState;
			bool flag2 = ((effectiveState == "加载失败" || effectiveState == "回滚受阻") ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private static string PlayerState(XWModManager.ModEntry entry)
	{
		if (entry.HasManifest)
		{
			if (!entry.PendingDelete)
			{
				if (entry.Enabled || !(entry.EffectiveState != "回滚受阻"))
				{
					if (!PlayerHasFailure(entry))
					{
						if (!entry.Loaded)
						{
							if (!entry.Enabled)
							{
								return "已安装，未启用";
							}
							return "重启后启用";
						}
						if (!(entry.EffectiveState == "已加载旧版本"))
						{
							return "正在使用";
						}
						return "正在使用旧版本，重启后更新";
					}
					return entry.EffectiveState;
				}
				if (!entry.Loaded)
				{
					return "已安装，未启用";
				}
				return "正在使用，重启后停用";
			}
			return "待删除，重启后移除";
		}
		if (!string.IsNullOrEmpty(entry.PackagePath))
		{
			return "安装包异常";
		}
		return "缺少安装包";
	}

	private static string PlayerRunningState(XWModManager.ModEntry entry)
	{
		if (entry.HasManifest && !entry.PendingDelete && !PlayerHasFailure(entry))
		{
			if (!entry.Loaded)
			{
				if (!entry.Enabled)
				{
					return "未加载";
				}
				return "未加载，重启后启用";
			}
			return PlayerState(entry);
		}
		return PlayerState(entry);
	}

	private static Color PlayerStateColor(XWModManager.ModEntry entry)
	{
		if (!PlayerHasFailure(entry))
		{
			if (!entry.PendingDelete && entry.Loaded == entry.Enabled && !(entry.EffectiveState == "已加载旧版本"))
			{
				if (!entry.Loaded)
				{
					return PlayerMuted;
				}
				return PlayerGreen;
			}
			return PlayerGold;
		}
		return PlayerError;
	}

	private static string ShortPlayerMessage(string message)
	{
		string text = (message ?? "").Replace("\r", "").Split('\n', 2)[0];
		if (text.Length <= 72)
		{
			return text;
		}
		return text.Substring(0, 72) + "…";
	}

	private static Label PlayerLabel(string text, int size, Color color, bool wrap = true)
	{
		Label label = new Label();
		label.Text = text;
		label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		label.MouseFilter = MouseFilterEnum.Ignore;
		label.AutowrapMode = (TextServer.AutowrapMode)(wrap ? 3 : 0);
		label.AddThemeFontSizeOverride("font_size", size);
		label.AddThemeColorOverride("font_color", color);
		return label;
	}

	private static Button PlayerButton(string name, string text, Action pressed, int width, string variation = "")
	{
		Button button = new Button();
		button.Name = name;
		button.Text = text;
		button.CustomMinimumSize = new Vector2(width, 48f);
		button.ThemeTypeVariation = variation;
		button.Pressed += delegate
		{
			AudioManager.Instance?.AudioPlay("ButtonPress");
			pressed();
		};
		return button;
	}

	private static ScrollContainer PlayerScroll(string name)
	{
		return new ScrollContainer
		{
			Name = name,
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			FollowFocus = true,
			ClipContents = true
		};
	}

	private static void ConfigurePlayerScrollContent(Node content)
	{
		if ((content is Window || content is ScrollContainer || content is LineEdit || content is TextEdit || content is Slider || content is SpinBox) ? true : false)
		{
			return;
		}
		if (content is BaseButton baseButton)
		{
			if (baseButton.MouseFilter != MouseFilterEnum.Ignore)
			{
				baseButton.MouseFilter = MouseFilterEnum.Pass;
			}
			if (Global.IsMobile)
			{
				baseButton.ActionMode = BaseButton.ActionModeEnum.Release;
			}
		}
		else if (content is Container { MouseFilter: MouseFilterEnum.Stop } container)
		{
			container.MouseFilter = MouseFilterEnum.Pass;
		}
		foreach (Node child in content.GetChildren())
		{
			ConfigurePlayerScrollContent(child);
		}
	}

	private static Control PlayerRule()
	{
		return new ColorRect
		{
			Color = new Color(0.5f, 0.28f, 0.08f, 0.25f),
			CustomMinimumSize = new Vector2(0f, 1f),
			MouseFilter = MouseFilterEnum.Ignore
		};
	}

	internal static VBoxContainer CreatePlayerDialogContent(Control overlay, string title, int preferredWidth = 640)
	{
		CenterContainer centerContainer = new CenterContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		overlay.AddChild(centerContainer, forceReadableName: false, InternalMode.Disabled);
		centerContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		PanelContainer frame = new PanelContainer
		{
			Name = "PaperDialogFrame",
			MouseFilter = MouseFilterEnum.Stop
		};
		frame.AddThemeStyleboxOverride("panel", CreatePlayerPaperFrame());
		centerContainer.AddChild(frame, forceReadableName: false, InternalMode.Disabled);
		overlay.Resized += ResizeFrame;
		ResizeFrame();
		VBoxContainer vBoxContainer = new VBoxContainer();
		vBoxContainer.AddThemeConstantOverride("separation", 18);
		frame.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		Label label = PlayerLabel(title, 26, PlayerInk);
		label.Name = "DialogHeading";
		label.HorizontalAlignment = HorizontalAlignment.Center;
		vBoxContainer.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		return vBoxContainer;
		void ResizeFrame()
		{
			frame.CustomMinimumSize = new Vector2(Mathf.Min(preferredWidth, overlay.Size.X - 64f), 0f);
		}
	}

	private static StyleBoxFlat PlayerFlat(Color background, Color border, int borderWidth, int padding)
	{
		StyleBoxFlat styleBoxFlat = new StyleBoxFlat();
		styleBoxFlat.BgColor = background;
		styleBoxFlat.BorderColor = border;
		styleBoxFlat.CornerRadiusTopLeft = 4;
		styleBoxFlat.CornerRadiusTopRight = 4;
		styleBoxFlat.CornerRadiusBottomLeft = 4;
		styleBoxFlat.CornerRadiusBottomRight = 4;
		styleBoxFlat.ContentMarginLeft = padding;
		styleBoxFlat.ContentMarginRight = padding;
		styleBoxFlat.ContentMarginTop = padding;
		styleBoxFlat.ContentMarginBottom = padding;
		styleBoxFlat.SetBorderWidthAll(borderWidth);
		return styleBoxFlat;
	}

	internal static StyleBoxTexture CreatePlayerPaperFrame()
	{
		return new StyleBoxTexture
		{
			Texture = GD.Load<Texture2D>("res://Asset/Texture/GUI/TowerDefense/Level/Option/LevelOptionButton.png"),
			TextureMarginLeft = 14f,
			TextureMarginRight = 14f,
			TextureMarginTop = 12f,
			TextureMarginBottom = 18f,
			ContentMarginLeft = 28f,
			ContentMarginRight = 28f,
			ContentMarginTop = 28f,
			ContentMarginBottom = 28f
		};
	}

	internal static Theme CreatePlayerManagementTheme()
	{
		Theme theme = new Theme
		{
			DefaultFont = GD.Load<Font>("res://Asset/Font/fzkt.ttf"),
			DefaultFontSize = 20
		};
		StyleBoxTexture styleBoxTexture = new StyleBoxTexture
		{
			Texture = GD.Load<Texture2D>("res://Asset/Texture/GUI/General/General/SeedChooserButton.png"),
			TextureMarginLeft = 8f,
			TextureMarginRight = 8f,
			TextureMarginTop = 6f,
			TextureMarginBottom = 6f,
			ContentMarginLeft = 16f,
			ContentMarginRight = 16f,
			ContentMarginTop = 8f,
			ContentMarginBottom = 8f
		};
		StyleBoxTexture styleBoxTexture2 = (StyleBoxTexture)styleBoxTexture.Duplicate();
		styleBoxTexture2.ModulateColor = new Color(0.82f, 0.82f, 0.82f);
		StyleBoxTexture styleBoxTexture3 = (StyleBoxTexture)styleBoxTexture.Duplicate();
		styleBoxTexture3.ModulateColor = new Color(1.08f, 1.08f, 1.08f);
		StyleBoxTexture styleBoxTexture4 = (StyleBoxTexture)styleBoxTexture.Duplicate();
		styleBoxTexture4.Texture = GD.Load<Texture2D>("res://Asset/Texture/GUI/General/General/SeedChooserButtonDisabled.png");
		theme.SetStylebox("normal", "Button", styleBoxTexture);
		theme.SetStylebox("hover", "Button", styleBoxTexture3);
		theme.SetStylebox("pressed", "Button", styleBoxTexture2);
		theme.SetStylebox("disabled", "Button", styleBoxTexture4);
		theme.SetStylebox("focus", "Button", PlayerFlat(Colors.Transparent, new Color("f5d482"), 2, 0));
		theme.SetColor("font_color", "Button", new Color("a8e665"));
		theme.SetColor("font_hover_color", "Button", new Color("d7ffab"));
		theme.SetColor("font_pressed_color", "Button", new Color("bce879"));
		theme.SetColor("font_focus_color", "Button", new Color("c7f293"));
		theme.SetColor("font_disabled_color", "Button", new Color("b29872"));
		theme.SetColor("font_outline_color", "Button", new Color("47240f"));
		theme.SetFontSize("font_size", "Button", 20);
		theme.SetConstant("outline_size", "Button", 2);
		string[] array = new string[3] { "ModSecondaryButton", "ModDangerButton", "ModListRow" };
		foreach (string text in array)
		{
			theme.SetTypeVariation(text, "Button");
		}
		array = new string[4] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" };
		foreach (string text2 in array)
		{
			theme.SetColor(text2, "ModSecondaryButton", new Color("f8dfad"));
			theme.SetColor(text2, "ModDangerButton", new Color("f0b090"));
		}
		StyleBoxFlat texture = PlayerFlat(new Color(1f, 0.88f, 0.62f, 0.22f), new Color(0.5f, 0.28f, 0.08f, 0.2f), 1, 0);
		theme.SetStylebox("normal", "ModListRow", texture);
		theme.SetStylebox("disabled", "ModListRow", texture);
		theme.SetStylebox("hover", "ModListRow", PlayerFlat(new Color("f2d6a4"), new Color("b1874e"), 1, 0));
		theme.SetStylebox("pressed", "ModListRow", PlayerFlat(new Color("f5d99b"), new Color("ac772f"), 1, 0));
		theme.SetStylebox("hover_pressed", "ModListRow", PlayerFlat(new Color("ffe5ae"), new Color("ac772f"), 1, 0));
		theme.SetStylebox("focus", "ModListRow", PlayerFlat(Colors.Transparent, new Color("956025"), 2, 0));
		theme.SetColor("font_color", "Label", PlayerInk);
		theme.SetColor("default_color", "RichTextLabel", PlayerInk);
		theme.SetStylebox("scroll", "VScrollBar", PlayerFlat(new Color(0.5f, 0.28f, 0.08f, 0.12f), Colors.Transparent, 0, 5));
		theme.SetStylebox("grabber", "VScrollBar", PlayerFlat(new Color("ba884f"), new Color("996637"), 1, 5));
		theme.SetStylebox("grabber_highlight", "VScrollBar", PlayerFlat(new Color("d5a262"), PlayerGold, 1, 5));
		theme.SetStylebox("grabber_pressed", "VScrollBar", PlayerFlat(new Color("996637"), PlayerGold, 1, 5));
		return theme;
	}

	private void UpdatePlayerSettingsAction(bool busy)
	{
		if (!GodotObject.IsInstanceValid(_playerSettings))
		{
			return;
		}
		XWModManifest xWModManifest = _playerSelected?.Manifest;
		_playerSettings.Visible = xWModManifest != null && xWModManifest.Settings?.Count > 0;
		string diagnostic = "";
		bool flag = xWModManifest != null && XWModSettingsService.ValidateDefinitions(xWModManifest, out diagnostic);
		_playerSettings.Disabled = busy || !flag || _playerSelected == null || !_playerSelected.HasManifest || _playerSelected.PendingDelete || string.IsNullOrEmpty(_playerSelected.PackagePath);
		Button playerSettings = _playerSettings;
		object tooltipText;
		if (flag)
		{
			XWModManager.ModEntry playerSelected = _playerSelected;
			tooltipText = ((playerSelected != null && playerSelected.PendingDelete) ? "安装包待删除，无法修改配置。" : ((xWModManifest != null && xWModManifest.Settings?.All((XWModSettingDefinition item) => item.IsDisplay) == true) ? "查看此 Mod 的说明与公告；无需启用。" : "配置此 Mod；未启用时也可以保存。"));
		}
		else
		{
			tooltipText = diagnostic;
		}
		playerSettings.TooltipText = (string)tooltipText;
	}

	private void OpenPlayerSettings()
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty || _playerUtilityOverlay != null || _playerSelected == null || _playerSettings.Disabled)
		{
			return;
		}
		XWModManager.ModEntry selected = _playerSelected;
		XWModSettingsEditor editor;
		try
		{
			XWModManager.ModEntry modEntry = FindPlayerSettingsPackage(selected);
			if (modEntry == null)
			{
				ShowManagementResult("无法打开配置", "安装包已变化或待删除，请刷新列表后重试。", failure: true);
				return;
			}
			editor = XWModSettingsService.OpenEditor(modEntry.Manifest, modEntry.PackageSha256);
		}
		catch (Exception ex)
		{
			ShowManagementResult("无法打开配置", ex.GetBaseException().Message, failure: true);
			return;
		}
		_playerSettingsEditor = editor;
		_playerSettingsDraft = new XWModSettingsDraft(editor);
		bool hasEditableItems = _playerSettingsDraft.HasEditableItems;
		Theme theme = CreatePlayerSettingsTheme();
		ColorRect overlay = AddPlayerUtilityOverlay("ModSettingsOverlay", new Color(0.15f, 0.08f, 0.02f, 0.58f));
		overlay.Theme = CreatePlayerManagementTheme();
		overlay.TreeExiting += delegate
		{
			if (_playerSettingsEditor == editor)
			{
				DisposePlayerSettings();
			}
		};
		PanelContainer frame = new PanelContainer
		{
			Name = "ModSettingsFrame",
			MouseFilter = MouseFilterEnum.Stop
		};
		frame.AddThemeStyleboxOverride("panel", CreatePlayerPaperFrame());
		overlay.AddChild(frame, forceReadableName: false, InternalMode.Disabled);
		frame.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		overlay.Resized += ResizeSettings;
		ResizeSettings();
		VBoxContainer vBoxContainer = new VBoxContainer();
		vBoxContainer.AddThemeConstantOverride("separation", 12);
		frame.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		Label label = PlayerLabel(selected.DisplayName + " · 配置", 26, PlayerInk);
		label.MaxLinesVisible = 2;
		label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		vBoxContainer.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		if (hasEditableItems)
		{
			vBoxContainer.AddChild(PlayerLabel("配置保存在本机，所有游戏账号共用。", 16, PlayerMuted), forceReadableName: false, InternalMode.Disabled);
		}
		VBoxContainer vBoxContainer2 = new VBoxContainer
		{
			Theme = theme
		};
		vBoxContainer.AddChild(vBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		LineEdit search = new LineEdit
		{
			Name = "SettingsSearch",
			PlaceholderText = "搜索配置…",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		StylePlayerSettingsControl(search);
		HBoxContainer hBoxContainer = new HBoxContainer();
		vBoxContainer2.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(search, forceReadableName: false, InternalMode.Disabled);
		CheckBox onlyChanged = CreatePlayerSettingsChoice(new XWModSettingOption
		{
			Label = "只看已修改"
		}, selected: false);
		onlyChanged.Name = "SettingsOnlyChanged";
		onlyChanged.Visible = hasEditableItems;
		onlyChanged.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		onlyChanged.AddThemeFontSizeOverride("font_size", 16);
		hBoxContainer.AddChild(onlyChanged, forceReadableName: false, InternalMode.Disabled);
		_playerSettingsSearch = search;
		TabBar groups = new TabBar
		{
			Name = "SettingsGroupTabs",
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			TabAlignment = TabBar.AlignmentMode.Left,
			ClipTabs = true,
			ScrollingEnabled = true,
			MaxTabWidth = 180,
			CustomMinimumSize = new Vector2(0f, 36f),
			FocusMode = FocusModeEnum.All
		};
		groups.AddThemeStyleboxOverride("tab_unselected", PlayerFlat(new Color("d3b285"), new Color("956025"), 1, 8));
		groups.AddThemeStyleboxOverride("tab_hovered", PlayerFlat(new Color("f3d2a1"), new Color("956025"), 1, 8));
		StyleBoxFlat styleBoxFlat = PlayerFlat(new Color("f8dfad"), new Color("956025"), 1, 8);
		styleBoxFlat.BorderWidthBottom = 3;
		groups.AddThemeStyleboxOverride("tab_selected", styleBoxFlat);
		groups.AddThemeStyleboxOverride("tab_disabled", PlayerFlat(new Color("d3b285"), Colors.Transparent, 0, 8));
		groups.AddThemeStyleboxOverride("tab_focus", PlayerFlat(Colors.Transparent, PlayerInk, 1, 0));
		string[] array = new string[3] { "font_selected_color", "font_unselected_color", "font_hovered_color" };
		foreach (string text in array)
		{
			groups.AddThemeColorOverride(text, PlayerInk);
		}
		groups.AddThemeColorOverride("font_disabled_color", PlayerMuted);
		groups.AddThemeConstantOverride("outline_size", 0);
		groups.AddTab("全部");
		string[] groupNames = editor.Definitions.Select((XWModSettingDefinition item) => item.Group ?? "").Distinct<string>(StringComparer.Ordinal).ToArray();
		array = groupNames;
		foreach (string text2 in array)
		{
			groups.AddTab(string.IsNullOrWhiteSpace(text2) ? "未分组" : text2);
		}
		vBoxContainer2.AddChild(groups, forceReadableName: false, InternalMode.Disabled);
		_playerSettingsGroup = groups;
		Dictionary<int, int> scrollPositions = new Dictionary<int, int>();
		int currentTab = 0;
		int scrollRevision = 0;
		ScrollContainer scroll = PlayerScroll("SettingsScroll");
		scroll.FollowFocus = true;
		vBoxContainer.AddChild(scroll, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer fields = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		fields.AddThemeConstantOverride("separation", 14);
		scroll.AddChild(fields, forceReadableName: false, InternalMode.Disabled);
		Dictionary<string, Label> groupHeadings = new Dictionary<string, Label>(StringComparer.Ordinal);
		foreach (IGrouping<string, XWModSettingDefinition> item in from xWModSettingDefinition in editor.Definitions
			group xWModSettingDefinition by xWModSettingDefinition.Group ?? "")
		{
			if (!string.IsNullOrWhiteSpace(item.Key))
			{
				Label label2 = PlayerLabel(item.Key, 22, PlayerInk);
				fields.AddChild(label2, forceReadableName: false, InternalMode.Disabled);
				groupHeadings.Add(item.Key, label2);
			}
			foreach (XWModSettingDefinition item2 in item)
			{
				XWModSettingDefinition definition = item2;
				VBoxContainer vBoxContainer3 = new VBoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				vBoxContainer3.AddThemeConstantOverride("separation", 5);
				fields.AddChild(vBoxContainer3, forceReadableName: false, InternalMode.Disabled);
				if (definition.IsDisplay)
				{
					HBoxContainer hBoxContainer2 = new HBoxContainer();
					Label label3 = PlayerLabel(definition.Label ?? "", 20, PlayerInk);
					hBoxContainer2.Visible = !string.IsNullOrWhiteSpace(definition.Label);
					hBoxContainer2.AddChild(label3, forceReadableName: false, InternalMode.Disabled);
					vBoxContainer3.AddChild(hBoxContainer2, forceReadableName: false, InternalMode.Disabled);
					XWModSettingsTextView.BindWidth(label3, overlay, SettingsTextWidth);
					if (!string.IsNullOrWhiteSpace(definition.Description))
					{
						Label label4 = PlayerLabel(definition.Description, 16, PlayerMuted);
						vBoxContainer3.AddChild(label4, forceReadableName: false, InternalMode.Disabled);
						XWModSettingsTextView.BindWidth(label4, overlay, SettingsTextWidth);
					}
					VBoxContainer vBoxContainer4 = new VBoxContainer
					{
						SizeFlagsHorizontal = SizeFlags.ExpandFill
					};
					vBoxContainer3.AddChild(vBoxContainer4, forceReadableName: false, InternalMode.Disabled);
					Label linkError = PlayerLabel("", 16, PlayerError);
					linkError.Visible = false;
					RichTextLabel richTextLabel = XWModSettingsTextView.Create(definition, delegate(string message)
					{
						linkError.Text = message;
						linkError.Visible = message.Length > 0;
					}, Global.IsMobile ? scroll : null);
					richTextLabel.AddThemeColorOverride("default_color", PlayerInk);
					richTextLabel.AddThemeFontSizeOverride("normal_font_size", 18);
					richTextLabel.AddThemeConstantOverride("outline_size", 0);
					richTextLabel.AddThemeConstantOverride("shadow_outline_size", 0);
					richTextLabel.AddThemeColorOverride("font_shadow_color", Colors.Transparent);
					vBoxContainer4.AddChild(richTextLabel, forceReadableName: false, InternalMode.Disabled);
					XWModSettingsTextView.BindWidth(richTextLabel, overlay, SettingsTextWidth);
					_playerSettingsFocus[richTextLabel] = richTextLabel.FocusMode;
					vBoxContainer3.AddChild(linkError, forceReadableName: false, InternalMode.Disabled);
					_playerSettingsRows.Add(definition.Key, new PlayerSettingsRow(vBoxContainer3, hBoxContainer2, vBoxContainer4, label3, null, null, linkError));
					vBoxContainer3.AddChild(PlayerRule(), forceReadableName: false, InternalMode.Disabled);
					continue;
				}
				BoxContainer line = new BoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					Vertical = true
				};
				line.AddThemeConstantOverride("separation", 10);
				vBoxContainer3.AddChild(line, forceReadableName: false, InternalMode.Disabled);
				HBoxContainer hBoxContainer3 = new HBoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				hBoxContainer3.AddThemeConstantOverride("separation", 4);
				line.AddChild(hBoxContainer3, forceReadableName: false, InternalMode.Disabled);
				Label label5 = PlayerLabel(XWModSettingsDraft.DisplayName(definition), 20, PlayerInk);
				label5.SizeFlagsHorizontal = SizeFlags.ExpandFill;
				label5.MouseFilter = MouseFilterEnum.Pass;
				hBoxContainer3.AddChild(label5, forceReadableName: false, InternalMode.Disabled);
				Label label6 = PlayerLabel("", 15, PlayerMuted);
				label6.Name = "StatusHint_" + definition.Key;
				label6.Visible = false;
				bool flag = definition.ApplyMode == "immediate";
				string text3 = (flag ? "保存后生效；安装包与运行版本不一致时仍需重启。" : "保存后，需要重启游戏才能生效。");
				if (!definition.AffectsGameplay)
				{
					text3 += " 仅影响本地显示。";
				}
				text3 = text3 + " 默认值：" + _playerSettingsDraft.DefaultText(definition.Key);
				hBoxContainer3.AddChild(CreatePlayerSettingsIcon("ApplyMode_" + definition.Key, flag ? "SettingsImmediate" : "SettingsRestart", text3, label6), forceReadableName: false, InternalMode.Disabled);
				if (definition.AffectsGameplay)
				{
					hBoxContainer3.AddChild(CreatePlayerSettingsIcon("Gameplay_" + definition.Key, "SettingsNetwork", "影响玩法，联机玩家需要使用一致的配置。", label6), forceReadableName: false, InternalMode.Disabled);
				}
				HBoxContainer hBoxContainer4 = new HBoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				line.AddChild(hBoxContainer4, forceReadableName: false, InternalMode.Disabled);
				VBoxContainer vBoxContainer5 = new VBoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					Theme = theme
				};
				hBoxContainer4.AddChild(vBoxContainer5, forceReadableName: false, InternalMode.Disabled);
				AddPlayerSettingsField(vBoxContainer5, definition, editor.SavedValues[definition.Key]);
				Button reset = CreatePlayerSettingsIcon("Reset_" + definition.Key, "SettingsReset", "恢复此项默认值：" + _playerSettingsDraft.DefaultText(definition.Key), label6);
				hBoxContainer4.AddChild(reset, forceReadableName: false, InternalMode.Disabled);
				label5.TooltipText = "默认值：" + _playerSettingsDraft.DefaultText(definition.Key);
				reset.Pressed += delegate
				{
					if (_playerSettingsEditor == editor && !reset.Disabled)
					{
						_updatingPlayerSettings = true;
						try
						{
							_playerSettingsWriters[definition.Key](_playerSettingsDraft.Reset(definition.Key));
						}
						finally
						{
							_updatingPlayerSettings = false;
						}
						_refreshPlayerSettings?.Invoke();
					}
				};
				fields.Resized += LayoutField;
				LayoutField();
				if (!string.IsNullOrWhiteSpace(definition.Description))
				{
					vBoxContainer3.AddChild(PlayerLabel(definition.Description, 16, PlayerMuted), forceReadableName: false, InternalMode.Disabled);
				}
				string type = definition.Type;
				if ((type == "int" || type == "float") ? true : false)
				{
					vBoxContainer3.AddChild(PlayerLabel(XWModSettingsDraft.NumericHint(definition), 15, PlayerMuted), forceReadableName: false, InternalMode.Disabled);
				}
				vBoxContainer3.AddChild(label6, forceReadableName: false, InternalMode.Disabled);
				Label label7 = PlayerLabel("", 15, PlayerMuted);
				vBoxContainer3.AddChild(label7, forceReadableName: false, InternalMode.Disabled);
				Label label8 = PlayerLabel("", 16, PlayerError);
				vBoxContainer3.AddChild(label8, forceReadableName: false, InternalMode.Disabled);
				_playerSettingsRows.Add(definition.Key, new PlayerSettingsRow(vBoxContainer3, hBoxContainer3, vBoxContainer5, label5, reset, label7, label8));
				WirePlayerSettingsInput(vBoxContainer5, definition);
				vBoxContainer3.AddChild(PlayerRule(), forceReadableName: false, InternalMode.Disabled);
				void LayoutField()
				{
					line.Vertical = definition.Type == "multienum" || fields.Size.X < 620f;
				}
			}
		}
		VBoxContainer empty = new VBoxContainer();
		empty.AddChild(PlayerLabel("没有符合条件的配置项。", 18, PlayerMuted), forceReadableName: false, InternalMode.Disabled);
		Button clear = PlayerButton("ClearSettingsFilter", "清除筛选", delegate
		{
			_clearPlayerSettingsFilters?.Invoke(obj: true);
		}, 100, "ModSecondaryButton");
		empty.AddChild(clear, forceReadableName: false, InternalMode.Disabled);
		fields.AddChild(empty, forceReadableName: false, InternalMode.Disabled);
		ConfigurePlayerScrollContent(fields);
		Label summary = PlayerLabel("", 16, PlayerMuted);
		summary.Visible = hasEditableItems;
		summary.Name = "SettingsChanges";
		vBoxContainer.AddChild(summary, forceReadableName: false, InternalMode.Disabled);
		Button save = null;
		_refreshPlayerSettings = delegate
		{
			if (!_updatingPlayerSettings && _playerSettingsEditor == editor)
			{
				Control focus = GetViewport().GuiGetFocusOwner();
				bool flag2 = focus != null && overlay.IsAncestorOf(focus);
				string focusedKey = _playerSettingsRows.FirstOrDefault((KeyValuePair<string, PlayerSettingsRow> keyValuePair) => focus != null && keyValuePair.Value.Row.IsAncestorOf(focus)).Key;
				string text4 = ((groups.CurrentTab > 0) ? groupNames[groups.CurrentTab - 1] : null);
				foreach (KeyValuePair<string, PlayerSettingsRow> playerSettingsRow in _playerSettingsRows)
				{
					playerSettingsRow.Value.Row.Visible = _playerSettingsDraft.Visible(playerSettingsRow.Key) && _playerSettingsDraft.Matches(playerSettingsRow.Key, search.Text, text4, onlyChanged.ButtonPressed);
					bool flag3 = _playerSettingsDraft.Enabled(playerSettingsRow.Key);
					SetPlayerSettingsEnabled(playerSettingsRow.Value.Input, flag3, playerSettingsRow.Value.Row.Visible);
					if (!_playerSettingsDraft.Definitions[playerSettingsRow.Key].IsDisplay)
					{
						foreach (Button item3 in PlayerSettingsControls(playerSettingsRow.Value.Header).OfType<Button>())
						{
							item3.Disabled = !flag3;
							item3.FocusMode = (FocusModeEnum)((flag3 && playerSettingsRow.Value.Row.Visible) ? 2 : 0);
						}
						playerSettingsRow.Value.Title.Text = XWModSettingsDraft.DisplayName(_playerSettingsDraft.Definitions[playerSettingsRow.Key]) + (_playerSettingsDraft.IsChanged(playerSettingsRow.Key) ? " · 已修改" : "");
						playerSettingsRow.Value.Reset.Disabled = !flag3 || _playerSettingsDraft.IsDefault(playerSettingsRow.Key);
						playerSettingsRow.Value.Reset.FocusMode = (FocusModeEnum)((flag3 && playerSettingsRow.Value.Row.Visible && !playerSettingsRow.Value.Reset.Disabled) ? 2 : 0);
						playerSettingsRow.Value.DisabledHint.Text = (flag3 ? "" : ("启用条件：" + _playerSettingsDraft.EnabledHint(playerSettingsRow.Key)));
						playerSettingsRow.Value.DisabledHint.Visible = !flag3;
						playerSettingsRow.Value.Error.Text = _playerSettingsDraft.Errors.GetValueOrDefault(playerSettingsRow.Key, "");
						playerSettingsRow.Value.Error.Visible = playerSettingsRow.Value.Error.Text.Length > 0;
					}
				}
				foreach (KeyValuePair<string, Label> pair in groupHeadings)
				{
					pair.Value.Visible = text4 == null && _playerSettingsRows.Any((KeyValuePair<string, PlayerSettingsRow> row) => _playerSettingsDraft.Definitions[row.Key].Group == pair.Key && row.Value.Row.Visible);
				}
				empty.Visible = !_playerSettingsRows.Any((KeyValuePair<string, PlayerSettingsRow> keyValuePair) => keyValuePair.Value.Row.Visible);
				for (int num2 = 0; num2 < groups.TabCount; num2++)
				{
					string text5 = ((num2 == 0) ? null : groupNames[num2 - 1]);
					string value = ((num2 == 0) ? "全部" : (string.IsNullOrWhiteSpace(text5) ? "未分组" : text5));
					string text6 = $"{value} ({_playerSettingsDraft.MatchCount(search.Text, text5, onlyChanged.ButtonPressed)})";
					groups.SetTabTitle(num2, text6);
					groups.SetTabTooltip(num2, text6);
				}
				summary.Text = (_playerSettingsDraft.HasChanges ? $"已修改 {_playerSettingsDraft.ChangedCount} 项，尚未保存。" : "没有未保存的修改。");
				if (_playerSettingsDraft.Errors.Count > 0)
				{
					summary.Text += $" {_playerSettingsDraft.Errors.Count} 项需要修正。";
				}
				if (save != null)
				{
					save.Disabled = !_playerSettingsDraft.CanSave;
				}
				_rebuildPlayerSettingsFocus?.Invoke();
				if (focusedKey != null && !PlayerSettingsCanFocus(focus))
				{
					((from keyValuePair in _playerSettingsRows.SkipWhile((KeyValuePair<string, PlayerSettingsRow> keyValuePair) => keyValuePair.Key != focusedKey).Skip(1)
						where keyValuePair.Value.Row.Visible && _playerSettingsDraft.Enabled(keyValuePair.Key)
						select keyValuePair).SelectMany((KeyValuePair<string, PlayerSettingsRow> keyValuePair) => PlayerSettingsControls(keyValuePair.Value.Input)).FirstOrDefault(PlayerSettingsCanFocus) ?? search).GrabFocus();
				}
				else if (flag2 && !PlayerSettingsCanFocus(focus))
				{
					search.GrabFocus();
				}
			}
		};
		_clearPlayerSettingsFilters = delegate(bool resetScroll)
		{
			_updatingPlayerSettings = true;
			try
			{
				search.Text = "";
				onlyChanged.SetPressedNoSignal(pressed: false);
				groups.CurrentTab = 0;
				currentTab = 0;
			}
			finally
			{
				_updatingPlayerSettings = false;
			}
			FilterChanged(resetScroll);
			groups.EnsureTabVisible(0);
		};
		search.TextChanged += delegate
		{
			FilterChanged();
		};
		onlyChanged.Toggled += delegate
		{
			FilterChanged();
		};
		groups.TabChanged += delegate(long index)
		{
			if (!_updatingPlayerSettings)
			{
				scrollPositions[currentTab] = scroll.ScrollVertical;
				currentTab = (int)index;
				_refreshPlayerSettings?.Invoke();
				RestoreScroll(scrollPositions.GetValueOrDefault(currentTab));
				groups.EnsureTabVisible((int)index);
			}
		};
		Label notice = PlayerLabel(editor.Diagnostic ?? "", 16, PlayerGold);
		notice.Visible = hasEditableItems;
		notice.Name = "SettingsNotice";
		notice.MouseFilter = MouseFilterEnum.Pass;
		notice.MaxLinesVisible = 3;
		notice.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		if (editor.RestartRequired)
		{
			notice.Text += "\n有配置需要重启游戏后生效。";
		}
		notice.TooltipText = notice.Text;
		vBoxContainer.AddChild(notice, forceReadableName: false, InternalMode.Disabled);
		HFlowContainer actions = new HFlowContainer
		{
			Name = "SettingsActions",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		actions.AddThemeConstantOverride("h_separation", 12);
		actions.AddThemeConstantOverride("v_separation", 8);
		vBoxContainer.AddChild(actions, forceReadableName: false, InternalMode.Disabled);
		if (hasEditableItems)
		{
			actions.AddChild(PlayerButton("ResetSettings", "恢复默认", delegate
			{
				if (_playerSettingsEditor == editor)
				{
					_updatingPlayerSettings = true;
					try
					{
						foreach (string key in _playerSettingsWriters.Keys)
						{
							_playerSettingsWriters[key](_playerSettingsDraft.Reset(key));
						}
					}
					finally
					{
						_updatingPlayerSettings = false;
					}
					_refreshPlayerSettings?.Invoke();
					notice.Text = "已恢复默认草稿，保存后生效。";
					notice.AddThemeColorOverride("font_color", PlayerGold);
				}
			}, 120, "ModSecondaryButton"), forceReadableName: false, InternalMode.Disabled);
		}
		Button button = PlayerButton("CancelSettings", hasEditableItems ? "取消" : "关闭", delegate
		{
			if (_playerUtilityOverlay == overlay)
			{
				TryClosePlayerUtility();
			}
		}, 96, "ModSecondaryButton");
		actions.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		if (hasEditableItems)
		{
			save = PlayerButton("SaveSettings", "保存", delegate
			{
				SavePlayerSettings(selected, editor, notice);
			}, 96);
			actions.AddChild(save, forceReadableName: false, InternalMode.Disabled);
		}
		_rebuildPlayerSettingsFocus = delegate
		{
			List<Control> list = new List<Control> { search, onlyChanged, groups };
			foreach (PlayerSettingsRow value2 in _playerSettingsRows.Values)
			{
				list.AddRange(PlayerSettingsControls(value2.Header));
				list.AddRange(PlayerSettingsControls(value2.Input));
				if (value2.Reset != null)
				{
					list.Add(value2.Reset);
				}
			}
			list.Add(clear);
			list.AddRange(PlayerSettingsControls(actions));
			Control[] array2 = list.Where(PlayerSettingsCanFocus).ToArray();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].FocusNext = array2[i].GetPathTo(array2[(i + 1) % array2.Length]);
				array2[i].FocusPrevious = array2[i].GetPathTo(array2[(i + array2.Length - 1) % array2.Length]);
			}
		};
		_refreshPlayerSettings();
		Viewport viewport = GetViewport();
		Control lastSettingsFocus = button;
		viewport.GuiFocusChanged += KeepSettingsFocus;
		overlay.TreeExiting += delegate
		{
			viewport.GuiFocusChanged -= KeepSettingsFocus;
		};
		button.GrabFocus();
		void FilterChanged(bool resetScroll = true)
		{
			if (!_updatingPlayerSettings)
			{
				scrollPositions.Clear();
				_refreshPlayerSettings?.Invoke();
				if (resetScroll)
				{
					RestoreScroll(0);
				}
				else
				{
					int num2 = scrollRevision + 1;
					scrollRevision = num2;
				}
			}
		}
		void KeepSettingsFocus(Control focused)
		{
			if (_playerSettingsEditor == editor && overlay.IsVisibleInTree() && _modConfirmation == null && GodotObject.IsInstanceValid(focused))
			{
				if (overlay.IsAncestorOf(focused))
				{
					lastSettingsFocus = focused;
				}
				else
				{
					(PlayerSettingsCanFocus(lastSettingsFocus) ? lastSettingsFocus : search).GrabFocus();
				}
			}
		}
		void ResizeSettings()
		{
			float num2 = Mathf.Max(12f, (overlay.Size.X - 760f) / 2f);
			frame.OffsetLeft = num2;
			frame.OffsetRight = 0f - num2;
			frame.OffsetTop = 16f;
			frame.OffsetBottom = -16f;
		}
		void RestoreScroll(int position)
		{
			int revision = ++scrollRevision;
			SceneTree tree = GetTree();
			tree.ProcessFrame += ApplyScroll;
			overlay.TreeExiting += CancelRestore;
			void ApplyScroll()
			{
				CancelRestore();
				if (revision == scrollRevision && _playerSettingsEditor == editor && GodotObject.IsInstanceValid(scroll))
				{
					VScrollBar vScrollBar = scroll.GetVScrollBar();
					scroll.ScrollVertical = (int)Math.Clamp(position, 0.0, Math.Max(0.0, vScrollBar.MaxValue - vScrollBar.Page));
				}
			}
			void CancelRestore()
			{
				tree.ProcessFrame -= ApplyScroll;
				if (GodotObject.IsInstanceValid(overlay))
				{
					overlay.TreeExiting -= CancelRestore;
				}
			}
		}
		float SettingsTextWidth()
		{
			return Mathf.Min(760f, overlay.Size.X - 24f) - frame.GetThemeStylebox("panel").GetMinimumSize().X - scroll.GetVScrollBar().GetCombinedMinimumSize().X;
		}
	}

	private void AddPlayerSettingsField(VBoxContainer field, XWModSettingDefinition definition, object value)
	{
		string type = definition.Type;
		if (type == null)
		{
			return;
		}
		Dictionary<string, CheckBox> checks;
		Label count;
		XWModSettingsNumberInput number;
		switch (type.Length)
		{
		case 4:
			switch (type[0])
			{
			case 'b':
				if (type == "bool")
				{
					CheckBox toggle = CreatePlayerSettingsChoice(new XWModSettingOption
					{
						Label = "开启"
					}, (bool)value);
					field.AddChild(toggle, forceReadableName: false, InternalMode.Disabled);
					_playerSettingsReaders[definition.Key] = () => toggle.ButtonPressed;
					_playerSettingsWriters[definition.Key] = delegate(object next)
					{
						toggle.SetPressedNoSignal((bool)next);
					};
				}
				break;
			case 'e':
			{
				if (!(type == "enum"))
				{
					break;
				}
				OptionButton choice = new OptionButton
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					FitToLongestItem = false
				};
				foreach (XWModSettingOption option in definition.Options)
				{
					choice.AddItem(string.IsNullOrWhiteSpace(option.Label) ? option.Value : option.Label);
				}
				choice.Select(definition.Options.FindIndex((XWModSettingOption option) => option.Value == (string)value));
				field.AddChild(choice, forceReadableName: false, InternalMode.Disabled);
				_playerSettingsReaders[definition.Key] = () => definition.Options[choice.Selected].Value;
				_playerSettingsWriters[definition.Key] = delegate(object next)
				{
					choice.Select(definition.Options.FindIndex((XWModSettingOption option) => option.Value == (string)next));
				};
				break;
			}
			}
			break;
		case 5:
			switch (type[0])
			{
			default:
				return;
			case 'f':
				break;
			case 'c':
			{
				if (!(type == "color"))
				{
					return;
				}
				HBoxContainer hBoxContainer = new HBoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				field.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
				ColorPickerButton picker = new ColorPickerButton
				{
					Color = (Color)value,
					EditAlpha = true,
					CustomMinimumSize = new Vector2(48f, 36f)
				};
				hBoxContainer.AddChild(picker, forceReadableName: false, InternalMode.Disabled);
				LineEdit colorValue = new LineEdit
				{
					Text = "#" + picker.Color.ToHtml().ToLowerInvariant(),
					PlaceholderText = "#RRGGBBAA",
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				hBoxContainer.AddChild(colorValue, forceReadableName: false, InternalMode.Disabled);
				bool synchronizingColor = false;
				picker.ColorChanged += delegate(Color color)
				{
					if (!synchronizingColor)
					{
						colorValue.Text = "#" + color.ToHtml().ToLowerInvariant();
					}
				};
				colorValue.TextChanged += delegate(string value2)
				{
					if (!XWModSettingsService.TryValue(definition, value2, out var normalized))
					{
						return;
					}
					synchronizingColor = true;
					try
					{
						picker.Color = (Color)normalized;
					}
					finally
					{
						synchronizingColor = false;
					}
				};
				colorValue.FocusExited += delegate
				{
					if (XWModSettingsService.TryValue(definition, colorValue.Text, out var normalized))
					{
						colorValue.Text = "#" + ((Color)normalized).ToHtml().ToLowerInvariant();
					}
				};
				_playerSettingsReaders[definition.Key] = () => colorValue.Text;
				_playerSettingsWriters[definition.Key] = delegate(object next)
				{
					picker.Color = (Color)next;
					colorValue.Text = "#" + picker.Color.ToHtml().ToLowerInvariant();
				};
				return;
			}
			}
			if (!(type == "float"))
			{
				break;
			}
			goto IL_01a2;
		case 3:
			if (!(type == "int"))
			{
				break;
			}
			goto IL_01a2;
		case 6:
			if (type == "string")
			{
				LineEdit text = new LineEdit
				{
					Text = (string)value,
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				field.AddChild(text, forceReadableName: false, InternalMode.Disabled);
				_playerSettingsReaders[definition.Key] = () => text.Text;
				_playerSettingsWriters[definition.Key] = delegate(object next)
				{
					text.Text = (string)next;
				};
			}
			break;
		case 9:
		{
			if (!(type == "multienum"))
			{
				break;
			}
			HashSet<string> hashSet = new HashSet<string>((IReadOnlyList<string>)value, StringComparer.Ordinal);
			checks = new Dictionary<string, CheckBox>(StringComparer.Ordinal);
			count = PlayerLabel("", 16, PlayerMuted);
			field.AddChild(count, forceReadableName: false, InternalMode.Disabled);
			foreach (XWModSettingOption option2 in definition.Options)
			{
				CheckBox checkBox = CreatePlayerSettingsChoice(option2, hashSet.Contains(option2.Value));
				checks.Add(option2.Value, checkBox);
				field.AddChild(checkBox, forceReadableName: false, InternalMode.Disabled);
				checkBox.Toggled += delegate
				{
					UpdateCount();
				};
			}
			UpdateCount();
			_playerSettingsReaders[definition.Key] = () => (from pair in checks
				where pair.Value.ButtonPressed
				select pair.Key).ToArray();
			_playerSettingsWriters[definition.Key] = delegate(object next)
			{
				HashSet<string> hashSet2 = new HashSet<string>((IReadOnlyList<string>)next, StringComparer.Ordinal);
				foreach (KeyValuePair<string, CheckBox> item in checks)
				{
					item.Value.SetPressedNoSignal(hashSet2.Contains(item.Key));
				}
				UpdateCount();
			};
			break;
		}
		case 7:
		case 8:
			break;
			IL_01a2:
			number = new XWModSettingsNumberInput(definition, value);
			field.AddChild(number.View, forceReadableName: false, InternalMode.Disabled);
			_playerSettingsReaders[definition.Key] = () => number.DraftValue;
			_playerSettingsWriters[definition.Key] = number.SetDraft;
			number.Changed += delegate
			{
				if (!_updatingPlayerSettings)
				{
					_playerSettingsDraft?.Set(definition.Key, number.DraftValue);
					_refreshPlayerSettings?.Invoke();
				}
			};
			break;
		}
		void UpdateCount()
		{
			int num = checks.Values.Count((CheckBox check) => check.ButtonPressed);
			int valueOrDefault = definition.MinSelected.GetValueOrDefault();
			int num2 = definition.MaxSelected ?? definition.Options.Count;
			count.Text = $"已选 {num} 项，允许 {valueOrDefault} 至 {num2} 项";
			count.AddThemeColorOverride("font_color", (num < valueOrDefault || num > num2) ? PlayerError : PlayerMuted);
		}
	}

	private static IEnumerable<Control> PlayerSettingsControls(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Window)
			{
				continue;
			}
			if (child is Control control)
			{
				yield return control;
			}
			foreach (Control item in PlayerSettingsControls(child))
			{
				yield return item;
			}
		}
		if (node is SpinBox spinBox)
		{
			yield return spinBox.GetLineEdit();
		}
	}

	private void WirePlayerSettingsInput(Control input, XWModSettingDefinition definition)
	{
		foreach (Control item in PlayerSettingsControls(input))
		{
			_playerSettingsFocus[item] = item.FocusMode;
			if (!(item is CheckBox))
			{
				StylePlayerSettingsControl(item);
			}
			string type = definition.Type;
			if ((type == "int" || type == "float") ? true : false)
			{
				continue;
			}
			if (item is LineEdit lineEdit)
			{
				lineEdit.TextChanged += delegate
				{
					Changed();
				};
			}
			if (item is CheckBox checkBox)
			{
				checkBox.Toggled += delegate
				{
					Changed();
				};
			}
			if (item is OptionButton optionButton)
			{
				optionButton.ItemSelected += delegate
				{
					Changed();
				};
			}
			if (item is ColorPickerButton colorPickerButton)
			{
				colorPickerButton.ColorChanged += delegate
				{
					Changed();
				};
			}
		}
		void Changed()
		{
			if (_playerSettingsDraft != null && !_updatingPlayerSettings)
			{
				_playerSettingsDraft.Set(definition.Key, _playerSettingsReaders[definition.Key]());
				_refreshPlayerSettings?.Invoke();
			}
		}
	}

	private void SetPlayerSettingsEnabled(Control input, bool enabled, bool visible)
	{
		foreach (Control item in PlayerSettingsControls(input))
		{
			item.FocusMode = ((enabled && visible) ? _playerSettingsFocus.GetValueOrDefault(item) : FocusModeEnum.None);
			if (item is BaseButton baseButton)
			{
				baseButton.Disabled = !enabled;
			}
			if (item is LineEdit lineEdit)
			{
				lineEdit.Editable = enabled;
			}
			if (item is SpinBox spinBox)
			{
				spinBox.Editable = enabled;
			}
			if (item is Slider slider)
			{
				slider.Editable = enabled && visible;
			}
			if (!enabled || !visible)
			{
				if (item is ColorPickerButton colorPickerButton)
				{
					colorPickerButton.GetPopup().Hide();
				}
				if (item is OptionButton optionButton)
				{
					optionButton.GetPopup().Hide();
				}
			}
		}
	}

	private static Button CreatePlayerSettingsIcon(string name, string icon, string description, Label hint)
	{
		Button button = new Button
		{
			Name = name,
			Icon = GD.Load<Texture2D>("res://addons/ModEditor/Tools/GUI/Icons/" + icon + ".svg"),
			TooltipText = description,
			FocusMode = FocusModeEnum.All,
			CustomMinimumSize = new Vector2(36f, 36f),
			SizeFlagsVertical = SizeFlags.ShrinkBegin
		};
		string[] array = new string[5] { "normal", "disabled", "hover", "pressed", "hover_pressed" };
		foreach (string text in array)
		{
			Button button2 = button;
			StringName name2 = text;
			bool flag;
			switch (text)
			{
			case "hover":
			case "pressed":
			case "hover_pressed":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			button2.AddThemeStyleboxOverride(name2, PlayerFlat(flag ? new Color(0.4f, 0.25f, 0.1f, 0.1f) : Colors.Transparent, Colors.Transparent, 0, 6));
		}
		button.AddThemeStyleboxOverride("focus", PlayerFlat(Colors.Transparent, PlayerInk, 1, 0));
		array = new string[5] { "icon_normal_color", "icon_hover_color", "icon_pressed_color", "icon_hover_pressed_color", "icon_focus_color" };
		foreach (string text2 in array)
		{
			button.AddThemeColorOverride(text2, PlayerInk);
		}
		button.AddThemeColorOverride("icon_disabled_color", PlayerMuted);
		button.FocusEntered += ShowHint;
		button.Pressed += ShowHint;
		button.FocusExited += delegate
		{
			hint.Hide();
		};
		return button;
		void ShowHint()
		{
			hint.Text = description;
			hint.Show();
		}
	}

	private static void StylePlayerSettingsControl(Control control)
	{
		if (control is HSlider hSlider)
		{
			hSlider.AddThemeStyleboxOverride("slider", PlayerFlat(new Color("d3b285"), new Color("956025"), 1, 3));
			hSlider.AddThemeStyleboxOverride("grabber_area", PlayerFlat(new Color("b1874e"), Colors.Transparent, 0, 3));
			hSlider.AddThemeStyleboxOverride("grabber_area_highlight", PlayerFlat(new Color("956025"), Colors.Transparent, 0, 3));
			hSlider.AddThemeStyleboxOverride("focus", PlayerFlat(Colors.Transparent, PlayerInk, 1, 0));
			hSlider.AddThemeIconOverride("grabber", GD.Load<Texture2D>("res://addons/ModEditor/Tools/GUI/Icons/SettingsSlider.svg"));
			hSlider.AddThemeIconOverride("grabber_highlight", GD.Load<Texture2D>("res://addons/ModEditor/Tools/GUI/Icons/SettingsSliderHighlight.svg"));
			hSlider.AddThemeIconOverride("grabber_disabled", GD.Load<Texture2D>("res://addons/ModEditor/Tools/GUI/Icons/SettingsSliderDisabled.svg"));
		}
		else
		{
			if ((!(control is LineEdit) && !(control is BaseButton)) || 1 == 0)
			{
				return;
			}
			string[] array = new string[6] { "normal", "hover", "pressed", "hover_pressed", "disabled", "read_only" };
			foreach (string text in array)
			{
				StringName name = text;
				string code;
				if ((text == "hover" || text == "hover_pressed") ? true : false)
				{
					code = "f3d2a1";
				}
				else
				{
					bool flag = ((text == "disabled" || text == "read_only") ? true : false);
					code = (flag ? "d3b285" : "eac28b");
				}
				control.AddThemeStyleboxOverride(name, PlayerFlat(new Color(code), new Color("956025"), 1, 6));
			}
			control.AddThemeStyleboxOverride("focus", PlayerFlat(Colors.Transparent, PlayerInk, 1, 0));
			array = new string[6] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color", "font_uneditable_color" };
			foreach (string text2 in array)
			{
				control.AddThemeColorOverride(text2, PlayerInk);
			}
			control.AddThemeColorOverride("font_disabled_color", PlayerMuted);
			control.AddThemeColorOverride("font_placeholder_color", PlayerMuted);
			control.AddThemeColorOverride("caret_color", PlayerInk);
			control.AddThemeConstantOverride("outline_size", 0);
			control.AddThemeFontSizeOverride("font_size", 18);
			control.CustomMinimumSize = new Vector2(control.CustomMinimumSize.X, 36f);
			if (control is OptionButton optionButton)
			{
				optionButton.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
				PopupMenu popup = optionButton.GetPopup();
				popup.AddThemeStyleboxOverride("panel", PlayerFlat(new Color("eac28b"), new Color("956025"), 1, 6));
				popup.AddThemeStyleboxOverride("hover", PlayerFlat(new Color("ddb17a"), Colors.Transparent, 0, 6));
				popup.AddThemeColorOverride("font_color", PlayerInk);
				popup.AddThemeColorOverride("font_hover_color", PlayerInk);
				popup.AddThemeColorOverride("font_disabled_color", PlayerMuted);
				popup.AddThemeConstantOverride("outline_size", 0);
				popup.AddThemeFontSizeOverride("font_size", 18);
			}
		}
	}

	private static Theme CreatePlayerSettingsTheme()
	{
		Theme theme = new Theme
		{
			DefaultFont = GD.Load<Font>("res://Asset/Font/fzkt.ttf"),
			DefaultFontSize = 18
		};
		string[] array = new string[5] { "Button", "LineEdit", "OptionButton", "CheckButton", "CheckBox" };
		foreach (string text in array)
		{
			string[] array2 = new string[6] { "normal", "hover", "pressed", "hover_pressed", "disabled", "read_only" };
			foreach (string text2 in array2)
			{
				Theme theme2 = theme;
				StringName name = text2;
				StringName themeType = text;
				string code;
				if ((text2 == "hover" || text2 == "hover_pressed") ? true : false)
				{
					code = "f3d2a1";
				}
				else
				{
					bool flag = ((text2 == "disabled" || text2 == "read_only") ? true : false);
					code = (flag ? "d3b285" : "eac28b");
				}
				theme2.SetStylebox(name, themeType, PlayerFlat(new Color(code), new Color("956025"), 1, 6));
			}
			theme.SetStylebox("focus", text, PlayerFlat(Colors.Transparent, PlayerInk, 1, 0));
			array2 = new string[5] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color", "font_hover_pressed_color" };
			foreach (string text3 in array2)
			{
				theme.SetColor(text3, text, PlayerInk);
			}
			theme.SetColor("font_disabled_color", text, PlayerMuted);
			theme.SetConstant("outline_size", text, 0);
		}
		array = new string[3] { "PanelContainer", "PopupPanel", "PopupMenu" };
		foreach (string text4 in array)
		{
			theme.SetStylebox("panel", text4, PlayerFlat(new Color("eac28b"), new Color("956025"), 1, 8));
		}
		theme.SetColor("font_color", "Label", PlayerInk);
		theme.SetColor("font_color", "PopupMenu", PlayerInk);
		theme.SetColor("font_hover_color", "PopupMenu", PlayerInk);
		theme.SetStylebox("hover", "PopupMenu", PlayerFlat(new Color("f3d2a1"), Colors.Transparent, 0, 6));
		return theme;
	}

	private static CheckBox CreatePlayerSettingsChoice(XWModSettingOption option, bool selected)
	{
		CheckBox checkBox = new CheckBox
		{
			Text = (string.IsNullOrWhiteSpace(option.Label) ? option.Value : option.Label),
			TooltipText = (string.IsNullOrWhiteSpace(option.Label) ? option.Value : option.Label),
			ButtonPressed = selected,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			CustomMinimumSize = new Vector2(0f, 36f),
			TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
		};
		StyleBoxEmpty stylebox = new StyleBoxEmpty
		{
			ContentMarginLeft = 4f,
			ContentMarginRight = 4f,
			ContentMarginTop = 4f,
			ContentMarginBottom = 4f
		};
		string[] array = new string[3] { "normal", "pressed", "disabled" };
		foreach (string text in array)
		{
			checkBox.AddThemeStyleboxOverride(text, stylebox);
		}
		StyleBoxFlat stylebox2 = PlayerFlat(new Color(0.4f, 0.25f, 0.1f, 0.08f), Colors.Transparent, 0, 4);
		checkBox.AddThemeStyleboxOverride("hover", stylebox2);
		checkBox.AddThemeStyleboxOverride("hover_pressed", stylebox2);
		checkBox.AddThemeStyleboxOverride("focus", PlayerFlat(Colors.Transparent, new Color("956025"), 1, 0));
		array = new string[5] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" };
		foreach (string text2 in array)
		{
			checkBox.AddThemeColorOverride(text2, PlayerInk);
		}
		checkBox.AddThemeColorOverride("font_disabled_color", PlayerMuted);
		checkBox.AddThemeConstantOverride("outline_size", 0);
		checkBox.AddThemeConstantOverride("h_separation", 8);
		checkBox.AddThemeFontSizeOverride("font_size", 20);
		Texture2D texture = GD.Load<Texture2D>("res://addons/ModEditor/Tools/GUI/Icons/SettingsChecked.svg");
		Texture2D texture2 = GD.Load<Texture2D>("res://addons/ModEditor/Tools/GUI/Icons/SettingsUnchecked.svg");
		checkBox.AddThemeIconOverride("checked", texture);
		checkBox.AddThemeIconOverride("checked_disabled", texture);
		checkBox.AddThemeIconOverride("unchecked", texture2);
		checkBox.AddThemeIconOverride("unchecked_disabled", texture2);
		return checkBox;
	}

	private static XWModManager.ModEntry FindPlayerSettingsPackage(XWModManager.ModEntry expected)
	{
		return new XWModManager().GetManagementSnapshot().Find((XWModManager.ModEntry entry) => entry.HasManifest && !entry.PendingDelete && string.Equals(entry.Id, expected.Id, StringComparison.OrdinalIgnoreCase) && string.Equals(entry.PackagePath, expected.PackagePath, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entry.PackageSha256) && string.Equals(entry.PackageSha256, expected.PackageSha256, StringComparison.OrdinalIgnoreCase));
	}

	private static bool PlayerSettingsCanFocus(Control control)
	{
		if (GodotObject.IsInstanceValid(control) && control.IsVisibleInTree() && control.FocusMode == FocusModeEnum.All && !(control is BaseButton { Disabled: not false }) && !(control is LineEdit { Editable: false }))
		{
			if (control is Slider slider)
			{
				return slider.Editable;
			}
			return true;
		}
		return false;
	}

	private bool RequestClosePlayerSettings()
	{
		if (_modConfirmation != null)
		{
			return true;
		}
		foreach (Control item in _playerSettingsRows.Values.SelectMany((PlayerSettingsRow row) => PlayerSettingsControls(row.Input)))
		{
			Window window = ((item is ColorPickerButton colorPickerButton) ? ((Popup)colorPickerButton.GetPopup()) : ((Popup)((item is OptionButton optionButton) ? optionButton.GetPopup() : ((item is RichTextLabel richTextLabel) ? richTextLabel.GetMenu() : null))));
			if (window != null && window.Visible)
			{
				window.Hide();
				if (PlayerSettingsCanFocus(item))
				{
					item.GrabFocus();
				}
				return true;
			}
		}
		foreach (KeyValuePair<string, Func<object>> playerSettingsReader in _playerSettingsReaders)
		{
			_playerSettingsDraft.Set(playerSettingsReader.Key, playerSettingsReader.Value());
		}
		_refreshPlayerSettings?.Invoke();
		if (!_playerSettingsDraft.HasChanges)
		{
			return ClosePlayerUtility();
		}
		XWModSettingsEditor editor = _playerSettingsEditor;
		Control focus = GetViewport().GuiGetFocusOwner();
		ShowModConfirmation("放弃未保存的修改？", $"有 {_playerSettingsDraft.ChangedCount} 项修改尚未保存。", "丢弃修改", delegate
		{
			if (_playerSettingsEditor == editor)
			{
				ClosePlayerUtility();
			}
		}, delegate
		{
			if (_playerSettingsEditor == editor)
			{
				_rebuildPlayerSettingsFocus?.Invoke();
				(PlayerSettingsCanFocus(focus) ? focus : _playerSettingsSearch)?.GrabFocus();
			}
		}, preservePlayerUtility: true, "继续编辑");
		return true;
	}

	private void SavePlayerSettings(XWModManager.ModEntry selected, XWModSettingsEditor editor, Label notice)
	{
		if (!CanUseManagement || _playerSettingsEditor != editor)
		{
			return;
		}
		try
		{
			if (FindPlayerSettingsPackage(selected) == null)
			{
				throw new InvalidOperationException("安装包已变化或待删除，请取消并刷新列表后重新配置。");
			}
			foreach (KeyValuePair<string, Func<object>> playerSettingsReader in _playerSettingsReaders)
			{
				_playerSettingsDraft.Set(playerSettingsReader.Key, playerSettingsReader.Value());
			}
			_refreshPlayerSettings?.Invoke();
			if (!_playerSettingsDraft.CanSave)
			{
				return;
			}
			if (_playerSettingsDraft.Errors.Count > 0)
			{
				string text = _playerSettingsDraft.Errors.Keys.First();
				_clearPlayerSettingsFilters?.Invoke(obj: false);
				XWModSettingDefinition item = _playerSettingsDraft.Definitions[text];
				if (_playerSettingsDraft.Visible(text) && _playerSettingsDraft.Enabled(text))
				{
					PlayerSettingsControls(_playerSettingsRows[text].Input).FirstOrDefault(PlayerSettingsCanFocus)?.GrabFocus();
				}
				throw new InvalidOperationException($"配置 '{XWModSettingsDraft.DisplayName(item)} ({text})'：{_playerSettingsDraft.Errors[text]}" + " " + _playerSettingsDraft.ConditionHint(text));
			}
			if (!editor.TrySave(_playerSettingsDraft.Values, out var result))
			{
				notice.Text = result.Message;
				notice.TooltipText = result.Message;
				notice.AddThemeColorOverride("font_color", PlayerError);
				return;
			}
			string message = result.Message;
			bool restartRequired = result.RestartRequired;
			ClosePlayerUtility();
			if (ManagementOnly)
			{
				RefreshModList();
			}
			ShowManagementResult("配置已保存", (!string.IsNullOrWhiteSpace(message)) ? message : (restartRequired ? "请重启游戏使配置生效。" : "配置已保存。"), failure: false, "", restartRequired);
			if (restartRequired)
			{
				SetPlayerNotice("配置已保存，请重启游戏使配置生效。", PlayerGold, showDetails: false);
			}
		}
		catch (Exception ex)
		{
			notice.Text = ex.GetBaseException().Message;
			notice.TooltipText = notice.Text;
			notice.AddThemeColorOverride("font_color", PlayerError);
		}
	}

	private void DisposePlayerSettings()
	{
		_playerSettingsEditor?.Dispose();
		_playerSettingsEditor = null;
		_playerSettingsReaders.Clear();
		_playerSettingsWriters.Clear();
		_playerSettingsRows.Clear();
		_playerSettingsFocus.Clear();
		_playerSettingsDraft = null;
		_refreshPlayerSettings = null;
		_playerSettingsSearch = null;
		_playerSettingsGroup = null;
		_clearPlayerSettingsFilters = null;
		_rebuildPlayerSettingsFocus = null;
		_updatingPlayerSettings = false;
	}

	private void AddPlayerUpdateAction(HBoxContainer toolbar)
	{
		_playerUpdates = PlayerButton("ViewUpdates", "更新（0）", OpenPlayerUpdates, 144, "ModSecondaryButton");
		toolbar.AddChild(_playerUpdates, forceReadableName: false, InternalMode.Disabled);
		XWModUpdateInbox.Shared.Changed += RefreshPlayerUpdateCount;
		_updateNoticesBound = true;
		RefreshPlayerUpdateCount();
	}

	private void RefreshPlayerUpdateCount()
	{
		if (CanUseManagement && GodotObject.IsInstanceValid(_playerUpdates))
		{
			_playerUpdates.Text = $"更新（{XWModUpdateInbox.Shared.Count}）";
		}
	}

	private void UnbindPlayerUpdates()
	{
		if (_updateNoticesBound)
		{
			XWModUpdateInbox.Shared.Changed -= RefreshPlayerUpdateCount;
			_updateNoticesBound = false;
		}
	}

	private void OpenPlayerUpdates()
	{
		if (!CanUseManagement || _modConfirmation != null || _filePickerToken != Guid.Empty || _playerUtilityOverlay != null)
		{
			return;
		}
		ColorRect overlay = AddPlayerUtilityOverlay("UpdatesOverlay", new Color(0.15f, 0.08f, 0.02f, 0.58f));
		_playerUtilityIsUpdates = true;
		VBoxContainer content = CreatePlayerDialogContent(overlay, "更新列表", 760);
		XWModUpdateListView list = new XWModUpdateListView
		{
			Name = "UpdatesList"
		};
		content.AddChild(list, forceReadableName: false, InternalMode.Disabled);
		Button close = XWModUpdateListView.CreateAction("CloseUpdates", "返回管理", delegate
		{
			if (_playerUtilityOverlay == overlay)
			{
				ClosePlayerUtility();
			}
		}, 160);
		close.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		list.FallbackFocus = close;
		content.AddChild(close, forceReadableName: false, InternalMode.Disabled);
		overlay.Resized += ResizeList;
		list.ContentSizeChanged += ResizeList;
		list.TreeExiting += delegate
		{
			overlay.Resized -= ResizeList;
			list.ContentSizeChanged -= ResizeList;
		};
		ResizeList();
		close.GrabFocus();
		void ResizeList()
		{
			if (GodotObject.IsInstanceValid(overlay) && !overlay.IsQueuedForDeletion())
			{
				float num = Mathf.Max(1f, Mathf.Min(760f, overlay.Size.X - 64f));
				content.CustomMaximumSize = new Vector2(Mathf.Max(1f, num - 56f), -1f);
				close.CustomMinimumSize = new Vector2(Mathf.Min(160f, content.CustomMaximumSize.X), 44f);
				list.SetAvailableHeight(overlay.Size.Y - 48f - 56f - content.GetNode<Label>("DialogHeading").GetCombinedMinimumSize().Y - close.GetCombinedMinimumSize().Y - 36f);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(118)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CloseManagementSession, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryCancelManagementConfirmation, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BindScene, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyScrollSafeLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConfigureScrollContainer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "scroll", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ScrollContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigureBoundedControl, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "control", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
				new PropertyInfo(Variant.Type.Float, "minimumHeight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigureBoundedLogControl, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "label", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("RichTextLabel"), exported: false),
				new PropertyInfo(Variant.Type.Float, "minimumHeight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RunReleaseCheck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RunReferenceGraph, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateReferenceGraphSummary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "references", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "reverseReferences", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "missingReferences", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OpenReferenceGraphTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RunScriptBuild, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateScriptBuildActionState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnModBuildFlightStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "projectRoot", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ShowOverrideDiff, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ToggleOverrideDiffEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenOverrideDiffTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenToolTarget, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "targetPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FormatGraphPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StartSandbox, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ShowImportHint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetupImportWizard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AddImportCategory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "label", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "folder", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SelectImportCategory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RunImportWizard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenImportedResource, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetImportCategory, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateImportSummary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "success", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "failed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "target", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetRelativeFolder, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "relativePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ShowTemplateLibrary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ShowLocalizationHint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RefreshLocalizationTable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SaveLocalizationTable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PopulateLocalizationTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EditLocalizationEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ShowModManagerStatus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RefreshModList, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ChooseModPackage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PrepareSelectedModPackage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "status", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.PackedStringArray, "paths", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "selectedFilterIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.EscapeBbcode, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnModSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetSelectedModId, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IsSelectedModEnabled, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetSelectedModEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ConfirmDeleteSelectedMod, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetupTemplateLibrary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SelectTemplate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ClearVisualChoiceChildren, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "parent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.ResolveTemplateCategoryIcon, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "category", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResolveImportCategoryIcon, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "folder", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CreateTemplateFromSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetTemplateStatus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsLikelyOverridePath, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "category", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "key", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FormatDiffValue, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Nil, "value", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.NormalizeProjectPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SelectTab, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetCurrentProjectPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetResult, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AddExperienceActions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "root", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("BoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.AddSelectedExperienceActions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "detail", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.CanPlaySelectedMod, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RenderSelectedExperience, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenPlayerHelp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AddProgressRecoveryActions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "actions", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.ConfirmProgressRepair, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "owner", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "reset", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AddPlayerManagementFilters, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "root", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.PlayerModFilterChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ClearPlayerModFilters, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlayerDetailWidth, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AddPlayerManagementDetails, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "detail", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.AddPlayerPendingBar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "root", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.RebuildPlayerManagementState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RenderPlayerModRows, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RenderPlayerManagementDetails, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.JumpPlayerMod, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "id", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdatePlayerManagementControls, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "busy", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OpenPlayerPendingChanges, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SelectManagementMod, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "id", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigureManagementOnly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ResizePlayerDirectory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RenderPlayerSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdatePlayerActionState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryClosePlayerUtility, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ClosePlayerUtility, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "restoreFocus", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.AddPlayerUtilityOverlay, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ColorRect"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Color, "shade", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OpenPlayerMore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenPlayerDiagnostics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenPlayerLoadPlan, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RestorePlayerManagementFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdatePlayerTechnicalText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ShowManagementResult, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "title", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "message", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "failure", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "details", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "restartRequired", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RestartFromManagement, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetPlayerNotice, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "message", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Color, "color", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "showDetails", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.VersionLabel, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "version", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ShortPlayerMessage, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "message", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlayerLabel, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Label"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Color, "color", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "wrap", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlayerScroll, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ScrollContainer"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigurePlayerScrollContent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "content", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.PlayerRule, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null),
			new MethodInfo(MethodName.CreatePlayerDialogContent, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "overlay", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
				new PropertyInfo(Variant.Type.String, "title", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "preferredWidth", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlayerFlat, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("StyleBoxFlat"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Color, "background", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Color, "border", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "borderWidth", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "padding", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CreatePlayerPaperFrame, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("StyleBoxTexture"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null),
			new MethodInfo(MethodName.CreatePlayerManagementTheme, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Theme"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null),
			new MethodInfo(MethodName.UpdatePlayerSettingsAction, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "busy", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OpenPlayerSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetPlayerSettingsEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "visible", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CreatePlayerSettingsIcon, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Button"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "icon", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "description", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "hint", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Label"), exported: false)
			}, null),
			new MethodInfo(MethodName.StylePlayerSettingsControl, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "control", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.CreatePlayerSettingsTheme, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Theme"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null),
			new MethodInfo(MethodName.PlayerSettingsCanFocus, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "control", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.RequestClosePlayerSettings, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.DisposePlayerSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AddPlayerUpdateAction, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "toolbar", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("HBoxContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshPlayerUpdateCount, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UnbindPlayerUpdates, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OpenPlayerUpdates, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CloseManagementSession && args.Count == 0)
		{
			CloseManagementSession();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryCancelManagementConfirmation && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(TryCancelManagementConfirmation());
			return true;
		}
		if (method == MethodName.BindScene && args.Count == 0)
		{
			BindScene();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyScrollSafeLayout && args.Count == 0)
		{
			ApplyScrollSafeLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigureScrollContainer && args.Count == 1)
		{
			ConfigureScrollContainer(VariantUtils.ConvertTo<ScrollContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigureBoundedControl && args.Count == 2)
		{
			ConfigureBoundedControl(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigureBoundedLogControl && args.Count == 2)
		{
			ConfigureBoundedLogControl(VariantUtils.ConvertTo<RichTextLabel>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RunReleaseCheck && args.Count == 0)
		{
			RunReleaseCheck();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RunReferenceGraph && args.Count == 0)
		{
			RunReferenceGraph();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateReferenceGraphSummary && args.Count == 3)
		{
			UpdateReferenceGraphSummary(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenReferenceGraphTarget && args.Count == 0)
		{
			OpenReferenceGraphTarget();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RunScriptBuild && args.Count == 0)
		{
			RunScriptBuild();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScriptBuildActionState && args.Count == 0)
		{
			UpdateScriptBuildActionState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnModBuildFlightStateChanged && args.Count == 2)
		{
			OnModBuildFlightStateChanged(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowOverrideDiff && args.Count == 0)
		{
			ShowOverrideDiff();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleOverrideDiffEntry && args.Count == 0)
		{
			ToggleOverrideDiffEntry();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenOverrideDiffTarget && args.Count == 0)
		{
			OpenOverrideDiffTarget();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenToolTarget && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(OpenToolTarget(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.FormatGraphPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatGraphPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.StartSandbox && args.Count == 0)
		{
			StartSandbox();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowImportHint && args.Count == 0)
		{
			ShowImportHint();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetupImportWizard && args.Count == 0)
		{
			SetupImportWizard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddImportCategory && args.Count == 3)
		{
			AddImportCategory(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<string>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectImportCategory && args.Count == 1)
		{
			SelectImportCategory(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RunImportWizard && args.Count == 0)
		{
			RunImportWizard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenImportedResource && args.Count == 0)
		{
			OpenImportedResource();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetImportCategory && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetImportCategory());
			return true;
		}
		if (method == MethodName.UpdateImportSummary && args.Count == 3)
		{
			UpdateImportSummary(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]), VariantUtils.ConvertTo<string>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetRelativeFolder && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetRelativeFolder(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ShowTemplateLibrary && args.Count == 0)
		{
			ShowTemplateLibrary();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowLocalizationHint && args.Count == 0)
		{
			ShowLocalizationHint();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshLocalizationTable && args.Count == 0)
		{
			RefreshLocalizationTable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveLocalizationTable && args.Count == 0)
		{
			SaveLocalizationTable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PopulateLocalizationTree && args.Count == 0)
		{
			PopulateLocalizationTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EditLocalizationEntry && args.Count == 0)
		{
			EditLocalizationEntry();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowModManagerStatus && args.Count == 0)
		{
			ShowModManagerStatus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshModList && args.Count == 0)
		{
			RefreshModList();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ChooseModPackage && args.Count == 0)
		{
			ChooseModPackage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PrepareSelectedModPackage && args.Count == 3)
		{
			PrepareSelectedModPackage(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<string[]>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EscapeBbcode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(EscapeBbcode(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.OnModSelected && args.Count == 0)
		{
			OnModSelected();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetSelectedModId && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetSelectedModId());
			return true;
		}
		if (method == MethodName.IsSelectedModEnabled && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsSelectedModEnabled());
			return true;
		}
		if (method == MethodName.SetSelectedModEnabled && args.Count == 1)
		{
			SetSelectedModEnabled(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfirmDeleteSelectedMod && args.Count == 0)
		{
			ConfirmDeleteSelectedMod();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetupTemplateLibrary && args.Count == 0)
		{
			SetupTemplateLibrary();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectTemplate && args.Count == 1)
		{
			SelectTemplate(VariantUtils.ConvertTo<long>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearVisualChoiceChildren && args.Count == 1)
		{
			ClearVisualChoiceChildren(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResolveTemplateCategoryIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveTemplateCategoryIcon(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ResolveImportCategoryIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveImportCategoryIcon(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.CreateTemplateFromSelection && args.Count == 0)
		{
			CreateTemplateFromSelection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTemplateStatus && args.Count == 1)
		{
			SetTemplateStatus(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsLikelyOverridePath && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<bool>(IsLikelyOverridePath(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<string>(in args[2])));
			return true;
		}
		if (method == MethodName.FormatDiffValue && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatDiffValue(VariantUtils.ConvertTo<Variant>(in args[0])));
			return true;
		}
		if (method == MethodName.NormalizeProjectPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(NormalizeProjectPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.SelectTab && args.Count == 1)
		{
			SelectTab(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCurrentProjectPath && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetCurrentProjectPath());
			return true;
		}
		if (method == MethodName.SetResult && args.Count == 1)
		{
			SetResult(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddExperienceActions && args.Count == 1)
		{
			AddExperienceActions(VariantUtils.ConvertTo<BoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddSelectedExperienceActions && args.Count == 1)
		{
			AddSelectedExperienceActions(VariantUtils.ConvertTo<VBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanPlaySelectedMod && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanPlaySelectedMod());
			return true;
		}
		if (method == MethodName.RenderSelectedExperience && args.Count == 0)
		{
			RenderSelectedExperience();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPlayerHelp && args.Count == 0)
		{
			OpenPlayerHelp();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddProgressRecoveryActions && args.Count == 1)
		{
			AddProgressRecoveryActions(VariantUtils.ConvertTo<VBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfirmProgressRepair && args.Count == 2)
		{
			ConfirmProgressRepair(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddPlayerManagementFilters && args.Count == 1)
		{
			AddPlayerManagementFilters(VariantUtils.ConvertTo<VBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayerModFilterChanged && args.Count == 0)
		{
			PlayerModFilterChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearPlayerModFilters && args.Count == 0)
		{
			ClearPlayerModFilters();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayerDetailWidth && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(PlayerDetailWidth());
			return true;
		}
		if (method == MethodName.AddPlayerManagementDetails && args.Count == 1)
		{
			AddPlayerManagementDetails(VariantUtils.ConvertTo<VBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddPlayerPendingBar && args.Count == 1)
		{
			AddPlayerPendingBar(VariantUtils.ConvertTo<VBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RebuildPlayerManagementState && args.Count == 0)
		{
			RebuildPlayerManagementState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RenderPlayerModRows && args.Count == 0)
		{
			RenderPlayerModRows();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RenderPlayerManagementDetails && args.Count == 0)
		{
			RenderPlayerManagementDetails();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.JumpPlayerMod && args.Count == 1)
		{
			JumpPlayerMod(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePlayerManagementControls && args.Count == 1)
		{
			UpdatePlayerManagementControls(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPlayerPendingChanges && args.Count == 0)
		{
			OpenPlayerPendingChanges();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectManagementMod && args.Count == 1)
		{
			SelectManagementMod(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigureManagementOnly && args.Count == 0)
		{
			ConfigureManagementOnly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResizePlayerDirectory && args.Count == 0)
		{
			ResizePlayerDirectory();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RenderPlayerSelection && args.Count == 0)
		{
			RenderPlayerSelection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePlayerActionState && args.Count == 0)
		{
			UpdatePlayerActionState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryClosePlayerUtility && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(TryClosePlayerUtility());
			return true;
		}
		if (method == MethodName.ClosePlayerUtility && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(ClosePlayerUtility(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.AddPlayerUtilityOverlay && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<ColorRect>(AddPlayerUtilityOverlay(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
			return true;
		}
		if (method == MethodName.OpenPlayerMore && args.Count == 0)
		{
			OpenPlayerMore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPlayerDiagnostics && args.Count == 0)
		{
			OpenPlayerDiagnostics();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPlayerLoadPlan && args.Count == 0)
		{
			OpenPlayerLoadPlan();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestorePlayerManagementFocus && args.Count == 0)
		{
			RestorePlayerManagementFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePlayerTechnicalText && args.Count == 0)
		{
			UpdatePlayerTechnicalText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowManagementResult && args.Count == 5)
		{
			ShowManagementResult(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<string>(in args[3]), VariantUtils.ConvertTo<bool>(in args[4]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestartFromManagement && args.Count == 0)
		{
			RestartFromManagement();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPlayerNotice && args.Count == 3)
		{
			SetPlayerNotice(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.VersionLabel && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(VersionLabel(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ShortPlayerMessage && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ShortPlayerMessage(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.PlayerLabel && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<Label>(PlayerLabel(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
			return true;
		}
		if (method == MethodName.PlayerScroll && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ScrollContainer>(PlayerScroll(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ConfigurePlayerScrollContent && args.Count == 1)
		{
			ConfigurePlayerScrollContent(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayerRule && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Control>(PlayerRule());
			return true;
		}
		if (method == MethodName.CreatePlayerDialogContent && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<VBoxContainer>(CreatePlayerDialogContent(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2])));
			return true;
		}
		if (method == MethodName.PlayerFlat && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<StyleBoxFlat>(PlayerFlat(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<int>(in args[3])));
			return true;
		}
		if (method == MethodName.CreatePlayerPaperFrame && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<StyleBoxTexture>(CreatePlayerPaperFrame());
			return true;
		}
		if (method == MethodName.CreatePlayerManagementTheme && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Theme>(CreatePlayerManagementTheme());
			return true;
		}
		if (method == MethodName.UpdatePlayerSettingsAction && args.Count == 1)
		{
			UpdatePlayerSettingsAction(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPlayerSettings && args.Count == 0)
		{
			OpenPlayerSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPlayerSettingsEnabled && args.Count == 3)
		{
			SetPlayerSettingsEnabled(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreatePlayerSettingsIcon && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<Button>(CreatePlayerSettingsIcon(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<string>(in args[2]), VariantUtils.ConvertTo<Label>(in args[3])));
			return true;
		}
		if (method == MethodName.StylePlayerSettingsControl && args.Count == 1)
		{
			StylePlayerSettingsControl(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreatePlayerSettingsTheme && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Theme>(CreatePlayerSettingsTheme());
			return true;
		}
		if (method == MethodName.PlayerSettingsCanFocus && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(PlayerSettingsCanFocus(VariantUtils.ConvertTo<Control>(in args[0])));
			return true;
		}
		if (method == MethodName.RequestClosePlayerSettings && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(RequestClosePlayerSettings());
			return true;
		}
		if (method == MethodName.DisposePlayerSettings && args.Count == 0)
		{
			DisposePlayerSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddPlayerUpdateAction && args.Count == 1)
		{
			AddPlayerUpdateAction(VariantUtils.ConvertTo<HBoxContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshPlayerUpdateCount && args.Count == 0)
		{
			RefreshPlayerUpdateCount();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnbindPlayerUpdates && args.Count == 0)
		{
			UnbindPlayerUpdates();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPlayerUpdates && args.Count == 0)
		{
			OpenPlayerUpdates();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ConfigureScrollContainer && args.Count == 1)
		{
			ConfigureScrollContainer(VariantUtils.ConvertTo<ScrollContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigureBoundedControl && args.Count == 2)
		{
			ConfigureBoundedControl(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigureBoundedLogControl && args.Count == 2)
		{
			ConfigureBoundedLogControl(VariantUtils.ConvertTo<RichTextLabel>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenToolTarget && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(OpenToolTarget(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.FormatGraphPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatGraphPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetRelativeFolder && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetRelativeFolder(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.EscapeBbcode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(EscapeBbcode(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ClearVisualChoiceChildren && args.Count == 1)
		{
			ClearVisualChoiceChildren(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResolveTemplateCategoryIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveTemplateCategoryIcon(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ResolveImportCategoryIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveImportCategoryIcon(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.IsLikelyOverridePath && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<bool>(IsLikelyOverridePath(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<string>(in args[2])));
			return true;
		}
		if (method == MethodName.FormatDiffValue && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatDiffValue(VariantUtils.ConvertTo<Variant>(in args[0])));
			return true;
		}
		if (method == MethodName.NormalizeProjectPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(NormalizeProjectPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.VersionLabel && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(VersionLabel(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ShortPlayerMessage && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ShortPlayerMessage(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.PlayerLabel && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<Label>(PlayerLabel(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
			return true;
		}
		if (method == MethodName.PlayerScroll && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ScrollContainer>(PlayerScroll(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ConfigurePlayerScrollContent && args.Count == 1)
		{
			ConfigurePlayerScrollContent(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayerRule && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Control>(PlayerRule());
			return true;
		}
		if (method == MethodName.CreatePlayerDialogContent && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<VBoxContainer>(CreatePlayerDialogContent(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2])));
			return true;
		}
		if (method == MethodName.PlayerFlat && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<StyleBoxFlat>(PlayerFlat(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<int>(in args[3])));
			return true;
		}
		if (method == MethodName.CreatePlayerPaperFrame && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<StyleBoxTexture>(CreatePlayerPaperFrame());
			return true;
		}
		if (method == MethodName.CreatePlayerManagementTheme && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Theme>(CreatePlayerManagementTheme());
			return true;
		}
		if (method == MethodName.CreatePlayerSettingsIcon && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<Button>(CreatePlayerSettingsIcon(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<string>(in args[2]), VariantUtils.ConvertTo<Label>(in args[3])));
			return true;
		}
		if (method == MethodName.StylePlayerSettingsControl && args.Count == 1)
		{
			StylePlayerSettingsControl(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreatePlayerSettingsTheme && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Theme>(CreatePlayerSettingsTheme());
			return true;
		}
		if (method == MethodName.PlayerSettingsCanFocus && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(PlayerSettingsCanFocus(VariantUtils.ConvertTo<Control>(in args[0])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.CloseManagementSession)
		{
			return true;
		}
		if (method == MethodName.TryCancelManagementConfirmation)
		{
			return true;
		}
		if (method == MethodName.BindScene)
		{
			return true;
		}
		if (method == MethodName.ApplyScrollSafeLayout)
		{
			return true;
		}
		if (method == MethodName.ConfigureScrollContainer)
		{
			return true;
		}
		if (method == MethodName.ConfigureBoundedControl)
		{
			return true;
		}
		if (method == MethodName.ConfigureBoundedLogControl)
		{
			return true;
		}
		if (method == MethodName.RunReleaseCheck)
		{
			return true;
		}
		if (method == MethodName.RunReferenceGraph)
		{
			return true;
		}
		if (method == MethodName.UpdateReferenceGraphSummary)
		{
			return true;
		}
		if (method == MethodName.OpenReferenceGraphTarget)
		{
			return true;
		}
		if (method == MethodName.RunScriptBuild)
		{
			return true;
		}
		if (method == MethodName.UpdateScriptBuildActionState)
		{
			return true;
		}
		if (method == MethodName.OnModBuildFlightStateChanged)
		{
			return true;
		}
		if (method == MethodName.ShowOverrideDiff)
		{
			return true;
		}
		if (method == MethodName.ToggleOverrideDiffEntry)
		{
			return true;
		}
		if (method == MethodName.OpenOverrideDiffTarget)
		{
			return true;
		}
		if (method == MethodName.OpenToolTarget)
		{
			return true;
		}
		if (method == MethodName.FormatGraphPath)
		{
			return true;
		}
		if (method == MethodName.StartSandbox)
		{
			return true;
		}
		if (method == MethodName.ShowImportHint)
		{
			return true;
		}
		if (method == MethodName.SetupImportWizard)
		{
			return true;
		}
		if (method == MethodName.AddImportCategory)
		{
			return true;
		}
		if (method == MethodName.SelectImportCategory)
		{
			return true;
		}
		if (method == MethodName.RunImportWizard)
		{
			return true;
		}
		if (method == MethodName.OpenImportedResource)
		{
			return true;
		}
		if (method == MethodName.GetImportCategory)
		{
			return true;
		}
		if (method == MethodName.UpdateImportSummary)
		{
			return true;
		}
		if (method == MethodName.GetRelativeFolder)
		{
			return true;
		}
		if (method == MethodName.ShowTemplateLibrary)
		{
			return true;
		}
		if (method == MethodName.ShowLocalizationHint)
		{
			return true;
		}
		if (method == MethodName.RefreshLocalizationTable)
		{
			return true;
		}
		if (method == MethodName.SaveLocalizationTable)
		{
			return true;
		}
		if (method == MethodName.PopulateLocalizationTree)
		{
			return true;
		}
		if (method == MethodName.EditLocalizationEntry)
		{
			return true;
		}
		if (method == MethodName.ShowModManagerStatus)
		{
			return true;
		}
		if (method == MethodName.RefreshModList)
		{
			return true;
		}
		if (method == MethodName.ChooseModPackage)
		{
			return true;
		}
		if (method == MethodName.PrepareSelectedModPackage)
		{
			return true;
		}
		if (method == MethodName.EscapeBbcode)
		{
			return true;
		}
		if (method == MethodName.OnModSelected)
		{
			return true;
		}
		if (method == MethodName.GetSelectedModId)
		{
			return true;
		}
		if (method == MethodName.IsSelectedModEnabled)
		{
			return true;
		}
		if (method == MethodName.SetSelectedModEnabled)
		{
			return true;
		}
		if (method == MethodName.ConfirmDeleteSelectedMod)
		{
			return true;
		}
		if (method == MethodName.SetupTemplateLibrary)
		{
			return true;
		}
		if (method == MethodName.SelectTemplate)
		{
			return true;
		}
		if (method == MethodName.ClearVisualChoiceChildren)
		{
			return true;
		}
		if (method == MethodName.ResolveTemplateCategoryIcon)
		{
			return true;
		}
		if (method == MethodName.ResolveImportCategoryIcon)
		{
			return true;
		}
		if (method == MethodName.CreateTemplateFromSelection)
		{
			return true;
		}
		if (method == MethodName.SetTemplateStatus)
		{
			return true;
		}
		if (method == MethodName.IsLikelyOverridePath)
		{
			return true;
		}
		if (method == MethodName.FormatDiffValue)
		{
			return true;
		}
		if (method == MethodName.NormalizeProjectPath)
		{
			return true;
		}
		if (method == MethodName.SelectTab)
		{
			return true;
		}
		if (method == MethodName.GetCurrentProjectPath)
		{
			return true;
		}
		if (method == MethodName.SetResult)
		{
			return true;
		}
		if (method == MethodName.AddExperienceActions)
		{
			return true;
		}
		if (method == MethodName.AddSelectedExperienceActions)
		{
			return true;
		}
		if (method == MethodName.CanPlaySelectedMod)
		{
			return true;
		}
		if (method == MethodName.RenderSelectedExperience)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerHelp)
		{
			return true;
		}
		if (method == MethodName.AddProgressRecoveryActions)
		{
			return true;
		}
		if (method == MethodName.ConfirmProgressRepair)
		{
			return true;
		}
		if (method == MethodName.AddPlayerManagementFilters)
		{
			return true;
		}
		if (method == MethodName.PlayerModFilterChanged)
		{
			return true;
		}
		if (method == MethodName.ClearPlayerModFilters)
		{
			return true;
		}
		if (method == MethodName.PlayerDetailWidth)
		{
			return true;
		}
		if (method == MethodName.AddPlayerManagementDetails)
		{
			return true;
		}
		if (method == MethodName.AddPlayerPendingBar)
		{
			return true;
		}
		if (method == MethodName.RebuildPlayerManagementState)
		{
			return true;
		}
		if (method == MethodName.RenderPlayerModRows)
		{
			return true;
		}
		if (method == MethodName.RenderPlayerManagementDetails)
		{
			return true;
		}
		if (method == MethodName.JumpPlayerMod)
		{
			return true;
		}
		if (method == MethodName.UpdatePlayerManagementControls)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerPendingChanges)
		{
			return true;
		}
		if (method == MethodName.SelectManagementMod)
		{
			return true;
		}
		if (method == MethodName.ConfigureManagementOnly)
		{
			return true;
		}
		if (method == MethodName.ResizePlayerDirectory)
		{
			return true;
		}
		if (method == MethodName.RenderPlayerSelection)
		{
			return true;
		}
		if (method == MethodName.UpdatePlayerActionState)
		{
			return true;
		}
		if (method == MethodName.TryClosePlayerUtility)
		{
			return true;
		}
		if (method == MethodName.ClosePlayerUtility)
		{
			return true;
		}
		if (method == MethodName.AddPlayerUtilityOverlay)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerMore)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerDiagnostics)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerLoadPlan)
		{
			return true;
		}
		if (method == MethodName.RestorePlayerManagementFocus)
		{
			return true;
		}
		if (method == MethodName.UpdatePlayerTechnicalText)
		{
			return true;
		}
		if (method == MethodName.ShowManagementResult)
		{
			return true;
		}
		if (method == MethodName.RestartFromManagement)
		{
			return true;
		}
		if (method == MethodName.SetPlayerNotice)
		{
			return true;
		}
		if (method == MethodName.VersionLabel)
		{
			return true;
		}
		if (method == MethodName.ShortPlayerMessage)
		{
			return true;
		}
		if (method == MethodName.PlayerLabel)
		{
			return true;
		}
		if (method == MethodName.PlayerScroll)
		{
			return true;
		}
		if (method == MethodName.ConfigurePlayerScrollContent)
		{
			return true;
		}
		if (method == MethodName.PlayerRule)
		{
			return true;
		}
		if (method == MethodName.CreatePlayerDialogContent)
		{
			return true;
		}
		if (method == MethodName.PlayerFlat)
		{
			return true;
		}
		if (method == MethodName.CreatePlayerPaperFrame)
		{
			return true;
		}
		if (method == MethodName.CreatePlayerManagementTheme)
		{
			return true;
		}
		if (method == MethodName.UpdatePlayerSettingsAction)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerSettings)
		{
			return true;
		}
		if (method == MethodName.SetPlayerSettingsEnabled)
		{
			return true;
		}
		if (method == MethodName.CreatePlayerSettingsIcon)
		{
			return true;
		}
		if (method == MethodName.StylePlayerSettingsControl)
		{
			return true;
		}
		if (method == MethodName.CreatePlayerSettingsTheme)
		{
			return true;
		}
		if (method == MethodName.PlayerSettingsCanFocus)
		{
			return true;
		}
		if (method == MethodName.RequestClosePlayerSettings)
		{
			return true;
		}
		if (method == MethodName.DisposePlayerSettings)
		{
			return true;
		}
		if (method == MethodName.AddPlayerUpdateAction)
		{
			return true;
		}
		if (method == MethodName.RefreshPlayerUpdateCount)
		{
			return true;
		}
		if (method == MethodName.UnbindPlayerUpdates)
		{
			return true;
		}
		if (method == MethodName.OpenPlayerUpdates)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ManagementOnly)
		{
			ManagementOnly = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.IsScriptBuildRunning)
		{
			IsScriptBuildRunning = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.ManagementConfirmationHost)
		{
			ManagementConfirmationHost = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._toolTabs)
		{
			_toolTabs = VariantUtils.ConvertTo<TabContainer>(in value);
			return true;
		}
		if (name == PropertyName._result)
		{
			_result = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._modTree)
		{
			_modTree = VariantUtils.ConvertTo<Tree>(in value);
			return true;
		}
		if (name == PropertyName._referenceGraphTree)
		{
			_referenceGraphTree = VariantUtils.ConvertTo<Tree>(in value);
			return true;
		}
		if (name == PropertyName._overrideDiffTree)
		{
			_overrideDiffTree = VariantUtils.ConvertTo<Tree>(in value);
			return true;
		}
		if (name == PropertyName._modSummary)
		{
			_modSummary = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._referenceGraphSummary)
		{
			_referenceGraphSummary = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._overrideDiffSummary)
		{
			_overrideDiffSummary = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._templateOption)
		{
			_templateOption = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._templateCardGrid)
		{
			_templateCardGrid = VariantUtils.ConvertTo<HFlowContainer>(in value);
			return true;
		}
		if (name == PropertyName._templateName)
		{
			_templateName = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._templatePreview)
		{
			_templatePreview = VariantUtils.ConvertTo<TextEdit>(in value);
			return true;
		}
		if (name == PropertyName._templateStatus)
		{
			_templateStatus = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._importCategoryOption)
		{
			_importCategoryOption = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._importCategoryGrid)
		{
			_importCategoryGrid = VariantUtils.ConvertTo<HFlowContainer>(in value);
			return true;
		}
		if (name == PropertyName._importSourcePath)
		{
			_importSourcePath = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._importResultTree)
		{
			_importResultTree = VariantUtils.ConvertTo<Tree>(in value);
			return true;
		}
		if (name == PropertyName._importSummary)
		{
			_importSummary = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._localizationTree)
		{
			_localizationTree = VariantUtils.ConvertTo<Tree>(in value);
			return true;
		}
		if (name == PropertyName._localizationSummary)
		{
			_localizationSummary = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._runScriptBuildButton)
		{
			_runScriptBuildButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._importModPackageButton)
		{
			_importModPackageButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._deleteModPackageButton)
		{
			_deleteModPackageButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._visibilitySignalBound)
		{
			_visibilitySignalBound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._managementClosed)
		{
			_managementClosed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._modConfirmation)
		{
			_modConfirmation = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName._playerPlay)
		{
			_playerPlay = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerSelectedPlay)
		{
			_playerSelectedPlay = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerInstructions)
		{
			_playerInstructions = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerPlayHint)
		{
			_playerPlayHint = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerSearch)
		{
			_playerSearch = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._playerFilter)
		{
			_playerFilter = VariantUtils.ConvertTo<OptionButton>(in value);
			return true;
		}
		if (name == PropertyName._playerClearFilter)
		{
			_playerClearFilter = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerPendingView)
		{
			_playerPendingView = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerRestart)
		{
			_playerRestart = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerMatched)
		{
			_playerMatched = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerPendingCount)
		{
			_playerPendingCount = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerPendingDetails)
		{
			_playerPendingDetails = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerPackagePath)
		{
			_playerPackagePath = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerRestartReason)
		{
			_playerRestartReason = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerPendingBar)
		{
			_playerPendingBar = VariantUtils.ConvertTo<HFlowContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerIssueList)
		{
			_playerIssueList = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._updatingPlayerFilters)
		{
			_updatingPlayerFilters = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._playerExactId)
		{
			_playerExactId = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._playerOperationEvidence)
		{
			_playerOperationEvidence = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._playerOrderChanged)
		{
			_playerOrderChanged = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._playerColumns)
		{
			_playerColumns = VariantUtils.ConvertTo<HBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerDirectory)
		{
			_playerDirectory = VariantUtils.ConvertTo<PanelContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerEmpty)
		{
			_playerEmpty = VariantUtils.ConvertTo<CenterContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerList)
		{
			_playerList = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerListScroll)
		{
			_playerListScroll = VariantUtils.ConvertTo<ScrollContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerDetailScroll)
		{
			_playerDetailScroll = VariantUtils.ConvertTo<ScrollContainer>(in value);
			return true;
		}
		if (name == PropertyName._playerName)
		{
			_playerName = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerAuthor)
		{
			_playerAuthor = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerDescription)
		{
			_playerDescription = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerEnabled)
		{
			_playerEnabled = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerRunning)
		{
			_playerRunning = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerMode)
		{
			_playerMode = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerVersions)
		{
			_playerVersions = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerFailure)
		{
			_playerFailure = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerNotice)
		{
			_playerNotice = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerCounts)
		{
			_playerCounts = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._playerToggle)
		{
			_playerToggle = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerEmptyImport)
		{
			_playerEmptyImport = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerReturn)
		{
			_playerReturn = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerMore)
		{
			_playerMore = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerUtilityOverlay)
		{
			_playerUtilityOverlay = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._playerTechnical)
		{
			_playerTechnical = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._playerSnapshotDetails)
		{
			_playerSnapshotDetails = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._playerOperationDetails)
		{
			_playerOperationDetails = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._playerSnapshotNotice)
		{
			_playerSnapshotNotice = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._playerSnapshotNoticeColor)
		{
			_playerSnapshotNoticeColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._playerSettings)
		{
			_playerSettings = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._playerSettingsSearch)
		{
			_playerSettingsSearch = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._playerSettingsGroup)
		{
			_playerSettingsGroup = VariantUtils.ConvertTo<TabBar>(in value);
			return true;
		}
		if (name == PropertyName._updatingPlayerSettings)
		{
			_updatingPlayerSettings = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._playerUpdates)
		{
			_playerUpdates = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._updateNoticesBound)
		{
			_updateNoticesBound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._playerUtilityIsUpdates)
		{
			_playerUtilityIsUpdates = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.ManagementOnly)
		{
			from = ManagementOnly;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CanUseManagement)
		{
			from = CanUseManagement;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsScriptBuildRunning)
		{
			from = IsScriptBuildRunning;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ManagementConfirmationHost)
		{
			value = VariantUtils.CreateFrom<Control>(ManagementConfirmationHost);
			return true;
		}
		if (name == PropertyName.SelectedManagementModId)
		{
			value = VariantUtils.CreateFrom<string>(SelectedManagementModId);
			return true;
		}
		if (name == PropertyName._toolTabs)
		{
			value = VariantUtils.CreateFrom(in _toolTabs);
			return true;
		}
		if (name == PropertyName._result)
		{
			value = VariantUtils.CreateFrom(in _result);
			return true;
		}
		if (name == PropertyName._modTree)
		{
			value = VariantUtils.CreateFrom(in _modTree);
			return true;
		}
		if (name == PropertyName._referenceGraphTree)
		{
			value = VariantUtils.CreateFrom(in _referenceGraphTree);
			return true;
		}
		if (name == PropertyName._overrideDiffTree)
		{
			value = VariantUtils.CreateFrom(in _overrideDiffTree);
			return true;
		}
		if (name == PropertyName._modSummary)
		{
			value = VariantUtils.CreateFrom(in _modSummary);
			return true;
		}
		if (name == PropertyName._referenceGraphSummary)
		{
			value = VariantUtils.CreateFrom(in _referenceGraphSummary);
			return true;
		}
		if (name == PropertyName._overrideDiffSummary)
		{
			value = VariantUtils.CreateFrom(in _overrideDiffSummary);
			return true;
		}
		if (name == PropertyName._templateOption)
		{
			value = VariantUtils.CreateFrom(in _templateOption);
			return true;
		}
		if (name == PropertyName._templateCardGrid)
		{
			value = VariantUtils.CreateFrom(in _templateCardGrid);
			return true;
		}
		if (name == PropertyName._templateName)
		{
			value = VariantUtils.CreateFrom(in _templateName);
			return true;
		}
		if (name == PropertyName._templatePreview)
		{
			value = VariantUtils.CreateFrom(in _templatePreview);
			return true;
		}
		if (name == PropertyName._templateStatus)
		{
			value = VariantUtils.CreateFrom(in _templateStatus);
			return true;
		}
		if (name == PropertyName._importCategoryOption)
		{
			value = VariantUtils.CreateFrom(in _importCategoryOption);
			return true;
		}
		if (name == PropertyName._importCategoryGrid)
		{
			value = VariantUtils.CreateFrom(in _importCategoryGrid);
			return true;
		}
		if (name == PropertyName._importSourcePath)
		{
			value = VariantUtils.CreateFrom(in _importSourcePath);
			return true;
		}
		if (name == PropertyName._importResultTree)
		{
			value = VariantUtils.CreateFrom(in _importResultTree);
			return true;
		}
		if (name == PropertyName._importSummary)
		{
			value = VariantUtils.CreateFrom(in _importSummary);
			return true;
		}
		if (name == PropertyName._localizationTree)
		{
			value = VariantUtils.CreateFrom(in _localizationTree);
			return true;
		}
		if (name == PropertyName._localizationSummary)
		{
			value = VariantUtils.CreateFrom(in _localizationSummary);
			return true;
		}
		if (name == PropertyName._runScriptBuildButton)
		{
			value = VariantUtils.CreateFrom(in _runScriptBuildButton);
			return true;
		}
		if (name == PropertyName._importModPackageButton)
		{
			value = VariantUtils.CreateFrom(in _importModPackageButton);
			return true;
		}
		if (name == PropertyName._deleteModPackageButton)
		{
			value = VariantUtils.CreateFrom(in _deleteModPackageButton);
			return true;
		}
		if (name == PropertyName._visibilitySignalBound)
		{
			value = VariantUtils.CreateFrom(in _visibilitySignalBound);
			return true;
		}
		if (name == PropertyName._managementClosed)
		{
			value = VariantUtils.CreateFrom(in _managementClosed);
			return true;
		}
		if (name == PropertyName._modConfirmation)
		{
			value = VariantUtils.CreateFrom(in _modConfirmation);
			return true;
		}
		if (name == PropertyName._playerPlay)
		{
			value = VariantUtils.CreateFrom(in _playerPlay);
			return true;
		}
		if (name == PropertyName._playerSelectedPlay)
		{
			value = VariantUtils.CreateFrom(in _playerSelectedPlay);
			return true;
		}
		if (name == PropertyName._playerInstructions)
		{
			value = VariantUtils.CreateFrom(in _playerInstructions);
			return true;
		}
		if (name == PropertyName._playerPlayHint)
		{
			value = VariantUtils.CreateFrom(in _playerPlayHint);
			return true;
		}
		if (name == PropertyName._playerSearch)
		{
			value = VariantUtils.CreateFrom(in _playerSearch);
			return true;
		}
		if (name == PropertyName._playerFilter)
		{
			value = VariantUtils.CreateFrom(in _playerFilter);
			return true;
		}
		if (name == PropertyName._playerClearFilter)
		{
			value = VariantUtils.CreateFrom(in _playerClearFilter);
			return true;
		}
		if (name == PropertyName._playerPendingView)
		{
			value = VariantUtils.CreateFrom(in _playerPendingView);
			return true;
		}
		if (name == PropertyName._playerRestart)
		{
			value = VariantUtils.CreateFrom(in _playerRestart);
			return true;
		}
		if (name == PropertyName._playerMatched)
		{
			value = VariantUtils.CreateFrom(in _playerMatched);
			return true;
		}
		if (name == PropertyName._playerPendingCount)
		{
			value = VariantUtils.CreateFrom(in _playerPendingCount);
			return true;
		}
		if (name == PropertyName._playerPendingDetails)
		{
			value = VariantUtils.CreateFrom(in _playerPendingDetails);
			return true;
		}
		if (name == PropertyName._playerPackagePath)
		{
			value = VariantUtils.CreateFrom(in _playerPackagePath);
			return true;
		}
		if (name == PropertyName._playerRestartReason)
		{
			value = VariantUtils.CreateFrom(in _playerRestartReason);
			return true;
		}
		if (name == PropertyName._playerPendingBar)
		{
			value = VariantUtils.CreateFrom(in _playerPendingBar);
			return true;
		}
		if (name == PropertyName._playerIssueList)
		{
			value = VariantUtils.CreateFrom(in _playerIssueList);
			return true;
		}
		if (name == PropertyName._updatingPlayerFilters)
		{
			value = VariantUtils.CreateFrom(in _updatingPlayerFilters);
			return true;
		}
		if (name == PropertyName._playerExactId)
		{
			value = VariantUtils.CreateFrom(in _playerExactId);
			return true;
		}
		if (name == PropertyName._playerOperationEvidence)
		{
			value = VariantUtils.CreateFrom(in _playerOperationEvidence);
			return true;
		}
		if (name == PropertyName._playerOrderChanged)
		{
			value = VariantUtils.CreateFrom(in _playerOrderChanged);
			return true;
		}
		if (name == PropertyName._playerColumns)
		{
			value = VariantUtils.CreateFrom(in _playerColumns);
			return true;
		}
		if (name == PropertyName._playerDirectory)
		{
			value = VariantUtils.CreateFrom(in _playerDirectory);
			return true;
		}
		if (name == PropertyName._playerEmpty)
		{
			value = VariantUtils.CreateFrom(in _playerEmpty);
			return true;
		}
		if (name == PropertyName._playerList)
		{
			value = VariantUtils.CreateFrom(in _playerList);
			return true;
		}
		if (name == PropertyName._playerListScroll)
		{
			value = VariantUtils.CreateFrom(in _playerListScroll);
			return true;
		}
		if (name == PropertyName._playerDetailScroll)
		{
			value = VariantUtils.CreateFrom(in _playerDetailScroll);
			return true;
		}
		if (name == PropertyName._playerName)
		{
			value = VariantUtils.CreateFrom(in _playerName);
			return true;
		}
		if (name == PropertyName._playerAuthor)
		{
			value = VariantUtils.CreateFrom(in _playerAuthor);
			return true;
		}
		if (name == PropertyName._playerDescription)
		{
			value = VariantUtils.CreateFrom(in _playerDescription);
			return true;
		}
		if (name == PropertyName._playerEnabled)
		{
			value = VariantUtils.CreateFrom(in _playerEnabled);
			return true;
		}
		if (name == PropertyName._playerRunning)
		{
			value = VariantUtils.CreateFrom(in _playerRunning);
			return true;
		}
		if (name == PropertyName._playerMode)
		{
			value = VariantUtils.CreateFrom(in _playerMode);
			return true;
		}
		if (name == PropertyName._playerVersions)
		{
			value = VariantUtils.CreateFrom(in _playerVersions);
			return true;
		}
		if (name == PropertyName._playerFailure)
		{
			value = VariantUtils.CreateFrom(in _playerFailure);
			return true;
		}
		if (name == PropertyName._playerNotice)
		{
			value = VariantUtils.CreateFrom(in _playerNotice);
			return true;
		}
		if (name == PropertyName._playerCounts)
		{
			value = VariantUtils.CreateFrom(in _playerCounts);
			return true;
		}
		if (name == PropertyName._playerToggle)
		{
			value = VariantUtils.CreateFrom(in _playerToggle);
			return true;
		}
		if (name == PropertyName._playerEmptyImport)
		{
			value = VariantUtils.CreateFrom(in _playerEmptyImport);
			return true;
		}
		if (name == PropertyName._playerReturn)
		{
			value = VariantUtils.CreateFrom(in _playerReturn);
			return true;
		}
		if (name == PropertyName._playerMore)
		{
			value = VariantUtils.CreateFrom(in _playerMore);
			return true;
		}
		if (name == PropertyName._playerUtilityOverlay)
		{
			value = VariantUtils.CreateFrom(in _playerUtilityOverlay);
			return true;
		}
		if (name == PropertyName._playerTechnical)
		{
			value = VariantUtils.CreateFrom(in _playerTechnical);
			return true;
		}
		if (name == PropertyName._playerSnapshotDetails)
		{
			value = VariantUtils.CreateFrom(in _playerSnapshotDetails);
			return true;
		}
		if (name == PropertyName._playerOperationDetails)
		{
			value = VariantUtils.CreateFrom(in _playerOperationDetails);
			return true;
		}
		if (name == PropertyName._playerSnapshotNotice)
		{
			value = VariantUtils.CreateFrom(in _playerSnapshotNotice);
			return true;
		}
		if (name == PropertyName._playerSnapshotNoticeColor)
		{
			value = VariantUtils.CreateFrom(in _playerSnapshotNoticeColor);
			return true;
		}
		if (name == PropertyName._playerSettings)
		{
			value = VariantUtils.CreateFrom(in _playerSettings);
			return true;
		}
		if (name == PropertyName._playerSettingsSearch)
		{
			value = VariantUtils.CreateFrom(in _playerSettingsSearch);
			return true;
		}
		if (name == PropertyName._playerSettingsGroup)
		{
			value = VariantUtils.CreateFrom(in _playerSettingsGroup);
			return true;
		}
		if (name == PropertyName._updatingPlayerSettings)
		{
			value = VariantUtils.CreateFrom(in _updatingPlayerSettings);
			return true;
		}
		if (name == PropertyName._playerUpdates)
		{
			value = VariantUtils.CreateFrom(in _playerUpdates);
			return true;
		}
		if (name == PropertyName._updateNoticesBound)
		{
			value = VariantUtils.CreateFrom(in _updateNoticesBound);
			return true;
		}
		if (name == PropertyName._playerUtilityIsUpdates)
		{
			value = VariantUtils.CreateFrom(in _playerUtilityIsUpdates);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName.ManagementOnly, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName._toolTabs, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._result, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._modTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._referenceGraphTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._overrideDiffTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._modSummary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._referenceGraphSummary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._overrideDiffSummary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._templateOption, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._templateCardGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._templateName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._templatePreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._templateStatus, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._importCategoryOption, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._importCategoryGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._importSourcePath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._importResultTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._importSummary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._localizationTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._localizationSummary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._runScriptBuildButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._importModPackageButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._deleteModPackageButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._visibilitySignalBound, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._managementClosed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._modConfirmation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanUseManagement, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsScriptBuildRunning, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerSelectedPlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerInstructions, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPlayHint, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerSearch, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerClearFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPendingView, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerRestart, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerMatched, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPendingCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPendingDetails, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPackagePath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerRestartReason, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerPendingBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerIssueList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._updatingPlayerFilters, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._playerExactId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._playerOperationEvidence, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._playerOrderChanged, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.ManagementConfirmationHost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.SelectedManagementModId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerColumns, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerDirectory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerEmpty, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerListScroll, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerDetailScroll, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerAuthor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerDescription, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerRunning, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerVersions, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerFailure, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerNotice, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerCounts, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerToggle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerEmptyImport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerReturn, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerMore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerUtilityOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerTechnical, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._playerSnapshotDetails, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._playerOperationDetails, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._playerSnapshotNotice, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Color, PropertyName._playerSnapshotNoticeColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerSettings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerSettingsSearch, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerSettingsGroup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._updatingPlayerSettings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._playerUpdates, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._updateNoticesBound, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._playerUtilityIsUpdates, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.ManagementOnly, Variant.From<bool>(ManagementOnly));
		info.AddProperty(PropertyName.IsScriptBuildRunning, Variant.From<bool>(IsScriptBuildRunning));
		info.AddProperty(PropertyName.ManagementConfirmationHost, Variant.From<Control>(ManagementConfirmationHost));
		info.AddProperty(PropertyName._toolTabs, Variant.From(in _toolTabs));
		info.AddProperty(PropertyName._result, Variant.From(in _result));
		info.AddProperty(PropertyName._modTree, Variant.From(in _modTree));
		info.AddProperty(PropertyName._referenceGraphTree, Variant.From(in _referenceGraphTree));
		info.AddProperty(PropertyName._overrideDiffTree, Variant.From(in _overrideDiffTree));
		info.AddProperty(PropertyName._modSummary, Variant.From(in _modSummary));
		info.AddProperty(PropertyName._referenceGraphSummary, Variant.From(in _referenceGraphSummary));
		info.AddProperty(PropertyName._overrideDiffSummary, Variant.From(in _overrideDiffSummary));
		info.AddProperty(PropertyName._templateOption, Variant.From(in _templateOption));
		info.AddProperty(PropertyName._templateCardGrid, Variant.From(in _templateCardGrid));
		info.AddProperty(PropertyName._templateName, Variant.From(in _templateName));
		info.AddProperty(PropertyName._templatePreview, Variant.From(in _templatePreview));
		info.AddProperty(PropertyName._templateStatus, Variant.From(in _templateStatus));
		info.AddProperty(PropertyName._importCategoryOption, Variant.From(in _importCategoryOption));
		info.AddProperty(PropertyName._importCategoryGrid, Variant.From(in _importCategoryGrid));
		info.AddProperty(PropertyName._importSourcePath, Variant.From(in _importSourcePath));
		info.AddProperty(PropertyName._importResultTree, Variant.From(in _importResultTree));
		info.AddProperty(PropertyName._importSummary, Variant.From(in _importSummary));
		info.AddProperty(PropertyName._localizationTree, Variant.From(in _localizationTree));
		info.AddProperty(PropertyName._localizationSummary, Variant.From(in _localizationSummary));
		info.AddProperty(PropertyName._runScriptBuildButton, Variant.From(in _runScriptBuildButton));
		info.AddProperty(PropertyName._importModPackageButton, Variant.From(in _importModPackageButton));
		info.AddProperty(PropertyName._deleteModPackageButton, Variant.From(in _deleteModPackageButton));
		info.AddProperty(PropertyName._visibilitySignalBound, Variant.From(in _visibilitySignalBound));
		info.AddProperty(PropertyName._managementClosed, Variant.From(in _managementClosed));
		info.AddProperty(PropertyName._modConfirmation, Variant.From(in _modConfirmation));
		info.AddProperty(PropertyName._playerPlay, Variant.From(in _playerPlay));
		info.AddProperty(PropertyName._playerSelectedPlay, Variant.From(in _playerSelectedPlay));
		info.AddProperty(PropertyName._playerInstructions, Variant.From(in _playerInstructions));
		info.AddProperty(PropertyName._playerPlayHint, Variant.From(in _playerPlayHint));
		info.AddProperty(PropertyName._playerSearch, Variant.From(in _playerSearch));
		info.AddProperty(PropertyName._playerFilter, Variant.From(in _playerFilter));
		info.AddProperty(PropertyName._playerClearFilter, Variant.From(in _playerClearFilter));
		info.AddProperty(PropertyName._playerPendingView, Variant.From(in _playerPendingView));
		info.AddProperty(PropertyName._playerRestart, Variant.From(in _playerRestart));
		info.AddProperty(PropertyName._playerMatched, Variant.From(in _playerMatched));
		info.AddProperty(PropertyName._playerPendingCount, Variant.From(in _playerPendingCount));
		info.AddProperty(PropertyName._playerPendingDetails, Variant.From(in _playerPendingDetails));
		info.AddProperty(PropertyName._playerPackagePath, Variant.From(in _playerPackagePath));
		info.AddProperty(PropertyName._playerRestartReason, Variant.From(in _playerRestartReason));
		info.AddProperty(PropertyName._playerPendingBar, Variant.From(in _playerPendingBar));
		info.AddProperty(PropertyName._playerIssueList, Variant.From(in _playerIssueList));
		info.AddProperty(PropertyName._updatingPlayerFilters, Variant.From(in _updatingPlayerFilters));
		info.AddProperty(PropertyName._playerExactId, Variant.From(in _playerExactId));
		info.AddProperty(PropertyName._playerOperationEvidence, Variant.From(in _playerOperationEvidence));
		info.AddProperty(PropertyName._playerOrderChanged, Variant.From(in _playerOrderChanged));
		info.AddProperty(PropertyName._playerColumns, Variant.From(in _playerColumns));
		info.AddProperty(PropertyName._playerDirectory, Variant.From(in _playerDirectory));
		info.AddProperty(PropertyName._playerEmpty, Variant.From(in _playerEmpty));
		info.AddProperty(PropertyName._playerList, Variant.From(in _playerList));
		info.AddProperty(PropertyName._playerListScroll, Variant.From(in _playerListScroll));
		info.AddProperty(PropertyName._playerDetailScroll, Variant.From(in _playerDetailScroll));
		info.AddProperty(PropertyName._playerName, Variant.From(in _playerName));
		info.AddProperty(PropertyName._playerAuthor, Variant.From(in _playerAuthor));
		info.AddProperty(PropertyName._playerDescription, Variant.From(in _playerDescription));
		info.AddProperty(PropertyName._playerEnabled, Variant.From(in _playerEnabled));
		info.AddProperty(PropertyName._playerRunning, Variant.From(in _playerRunning));
		info.AddProperty(PropertyName._playerMode, Variant.From(in _playerMode));
		info.AddProperty(PropertyName._playerVersions, Variant.From(in _playerVersions));
		info.AddProperty(PropertyName._playerFailure, Variant.From(in _playerFailure));
		info.AddProperty(PropertyName._playerNotice, Variant.From(in _playerNotice));
		info.AddProperty(PropertyName._playerCounts, Variant.From(in _playerCounts));
		info.AddProperty(PropertyName._playerToggle, Variant.From(in _playerToggle));
		info.AddProperty(PropertyName._playerEmptyImport, Variant.From(in _playerEmptyImport));
		info.AddProperty(PropertyName._playerReturn, Variant.From(in _playerReturn));
		info.AddProperty(PropertyName._playerMore, Variant.From(in _playerMore));
		info.AddProperty(PropertyName._playerUtilityOverlay, Variant.From(in _playerUtilityOverlay));
		info.AddProperty(PropertyName._playerTechnical, Variant.From(in _playerTechnical));
		info.AddProperty(PropertyName._playerSnapshotDetails, Variant.From(in _playerSnapshotDetails));
		info.AddProperty(PropertyName._playerOperationDetails, Variant.From(in _playerOperationDetails));
		info.AddProperty(PropertyName._playerSnapshotNotice, Variant.From(in _playerSnapshotNotice));
		info.AddProperty(PropertyName._playerSnapshotNoticeColor, Variant.From(in _playerSnapshotNoticeColor));
		info.AddProperty(PropertyName._playerSettings, Variant.From(in _playerSettings));
		info.AddProperty(PropertyName._playerSettingsSearch, Variant.From(in _playerSettingsSearch));
		info.AddProperty(PropertyName._playerSettingsGroup, Variant.From(in _playerSettingsGroup));
		info.AddProperty(PropertyName._updatingPlayerSettings, Variant.From(in _updatingPlayerSettings));
		info.AddProperty(PropertyName._playerUpdates, Variant.From(in _playerUpdates));
		info.AddProperty(PropertyName._updateNoticesBound, Variant.From(in _updateNoticesBound));
		info.AddProperty(PropertyName._playerUtilityIsUpdates, Variant.From(in _playerUtilityIsUpdates));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ManagementOnly, out var value))
		{
			ManagementOnly = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.IsScriptBuildRunning, out var value2))
		{
			IsScriptBuildRunning = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.ManagementConfirmationHost, out var value3))
		{
			ManagementConfirmationHost = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._toolTabs, out var value4))
		{
			_toolTabs = value4.As<TabContainer>();
		}
		if (info.TryGetProperty(PropertyName._result, out var value5))
		{
			_result = value5.As<RichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._modTree, out var value6))
		{
			_modTree = value6.As<Tree>();
		}
		if (info.TryGetProperty(PropertyName._referenceGraphTree, out var value7))
		{
			_referenceGraphTree = value7.As<Tree>();
		}
		if (info.TryGetProperty(PropertyName._overrideDiffTree, out var value8))
		{
			_overrideDiffTree = value8.As<Tree>();
		}
		if (info.TryGetProperty(PropertyName._modSummary, out var value9))
		{
			_modSummary = value9.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._referenceGraphSummary, out var value10))
		{
			_referenceGraphSummary = value10.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._overrideDiffSummary, out var value11))
		{
			_overrideDiffSummary = value11.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._templateOption, out var value12))
		{
			_templateOption = value12.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._templateCardGrid, out var value13))
		{
			_templateCardGrid = value13.As<HFlowContainer>();
		}
		if (info.TryGetProperty(PropertyName._templateName, out var value14))
		{
			_templateName = value14.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._templatePreview, out var value15))
		{
			_templatePreview = value15.As<TextEdit>();
		}
		if (info.TryGetProperty(PropertyName._templateStatus, out var value16))
		{
			_templateStatus = value16.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._importCategoryOption, out var value17))
		{
			_importCategoryOption = value17.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._importCategoryGrid, out var value18))
		{
			_importCategoryGrid = value18.As<HFlowContainer>();
		}
		if (info.TryGetProperty(PropertyName._importSourcePath, out var value19))
		{
			_importSourcePath = value19.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._importResultTree, out var value20))
		{
			_importResultTree = value20.As<Tree>();
		}
		if (info.TryGetProperty(PropertyName._importSummary, out var value21))
		{
			_importSummary = value21.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._localizationTree, out var value22))
		{
			_localizationTree = value22.As<Tree>();
		}
		if (info.TryGetProperty(PropertyName._localizationSummary, out var value23))
		{
			_localizationSummary = value23.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._runScriptBuildButton, out var value24))
		{
			_runScriptBuildButton = value24.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._importModPackageButton, out var value25))
		{
			_importModPackageButton = value25.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._deleteModPackageButton, out var value26))
		{
			_deleteModPackageButton = value26.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._visibilitySignalBound, out var value27))
		{
			_visibilitySignalBound = value27.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._managementClosed, out var value28))
		{
			_managementClosed = value28.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._modConfirmation, out var value29))
		{
			_modConfirmation = value29.As<Node>();
		}
		if (info.TryGetProperty(PropertyName._playerPlay, out var value30))
		{
			_playerPlay = value30.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerSelectedPlay, out var value31))
		{
			_playerSelectedPlay = value31.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerInstructions, out var value32))
		{
			_playerInstructions = value32.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerPlayHint, out var value33))
		{
			_playerPlayHint = value33.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerSearch, out var value34))
		{
			_playerSearch = value34.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._playerFilter, out var value35))
		{
			_playerFilter = value35.As<OptionButton>();
		}
		if (info.TryGetProperty(PropertyName._playerClearFilter, out var value36))
		{
			_playerClearFilter = value36.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerPendingView, out var value37))
		{
			_playerPendingView = value37.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerRestart, out var value38))
		{
			_playerRestart = value38.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerMatched, out var value39))
		{
			_playerMatched = value39.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerPendingCount, out var value40))
		{
			_playerPendingCount = value40.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerPendingDetails, out var value41))
		{
			_playerPendingDetails = value41.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerPackagePath, out var value42))
		{
			_playerPackagePath = value42.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerRestartReason, out var value43))
		{
			_playerRestartReason = value43.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerPendingBar, out var value44))
		{
			_playerPendingBar = value44.As<HFlowContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerIssueList, out var value45))
		{
			_playerIssueList = value45.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._updatingPlayerFilters, out var value46))
		{
			_updatingPlayerFilters = value46.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._playerExactId, out var value47))
		{
			_playerExactId = value47.As<string>();
		}
		if (info.TryGetProperty(PropertyName._playerOperationEvidence, out var value48))
		{
			_playerOperationEvidence = value48.As<string>();
		}
		if (info.TryGetProperty(PropertyName._playerOrderChanged, out var value49))
		{
			_playerOrderChanged = value49.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._playerColumns, out var value50))
		{
			_playerColumns = value50.As<HBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerDirectory, out var value51))
		{
			_playerDirectory = value51.As<PanelContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerEmpty, out var value52))
		{
			_playerEmpty = value52.As<CenterContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerList, out var value53))
		{
			_playerList = value53.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerListScroll, out var value54))
		{
			_playerListScroll = value54.As<ScrollContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerDetailScroll, out var value55))
		{
			_playerDetailScroll = value55.As<ScrollContainer>();
		}
		if (info.TryGetProperty(PropertyName._playerName, out var value56))
		{
			_playerName = value56.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerAuthor, out var value57))
		{
			_playerAuthor = value57.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerDescription, out var value58))
		{
			_playerDescription = value58.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerEnabled, out var value59))
		{
			_playerEnabled = value59.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerRunning, out var value60))
		{
			_playerRunning = value60.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerMode, out var value61))
		{
			_playerMode = value61.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerVersions, out var value62))
		{
			_playerVersions = value62.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerFailure, out var value63))
		{
			_playerFailure = value63.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerNotice, out var value64))
		{
			_playerNotice = value64.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerCounts, out var value65))
		{
			_playerCounts = value65.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._playerToggle, out var value66))
		{
			_playerToggle = value66.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerEmptyImport, out var value67))
		{
			_playerEmptyImport = value67.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerReturn, out var value68))
		{
			_playerReturn = value68.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerMore, out var value69))
		{
			_playerMore = value69.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerUtilityOverlay, out var value70))
		{
			_playerUtilityOverlay = value70.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._playerTechnical, out var value71))
		{
			_playerTechnical = value71.As<RichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._playerSnapshotDetails, out var value72))
		{
			_playerSnapshotDetails = value72.As<string>();
		}
		if (info.TryGetProperty(PropertyName._playerOperationDetails, out var value73))
		{
			_playerOperationDetails = value73.As<string>();
		}
		if (info.TryGetProperty(PropertyName._playerSnapshotNotice, out var value74))
		{
			_playerSnapshotNotice = value74.As<string>();
		}
		if (info.TryGetProperty(PropertyName._playerSnapshotNoticeColor, out var value75))
		{
			_playerSnapshotNoticeColor = value75.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._playerSettings, out var value76))
		{
			_playerSettings = value76.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._playerSettingsSearch, out var value77))
		{
			_playerSettingsSearch = value77.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._playerSettingsGroup, out var value78))
		{
			_playerSettingsGroup = value78.As<TabBar>();
		}
		if (info.TryGetProperty(PropertyName._updatingPlayerSettings, out var value79))
		{
			_updatingPlayerSettings = value79.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._playerUpdates, out var value80))
		{
			_playerUpdates = value80.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._updateNoticesBound, out var value81))
		{
			_updateNoticesBound = value81.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._playerUtilityIsUpdates, out var value82))
		{
			_playerUtilityIsUpdates = value82.As<bool>();
		}
	}
}
