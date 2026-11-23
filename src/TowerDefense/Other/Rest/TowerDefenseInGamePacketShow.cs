using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;

[GlobalClass]
[ScriptPath("res://Prefab/TowerDefense/GUI/InGame/Packet/TowerDefenseInGamePacketShow.cs")]
public class TowerDefenseInGamePacketShow : Control
{
	public delegate void PressedEventHandler(TowerDefenseInGamePacketShow packet);

	public delegate void LoveChangeEventHandler(TowerDefenseInGamePacketShow packet);

	internal readonly record struct ColumnPlacement(Vector2I GridPos, TowerDefenseCharacter Target, string Kind);

	public new class MethodName : Control.MethodName
	{
		public static readonly StringName SetScrollSelectionMode = "SetScrollSelectionMode";

		public static readonly StringName OnSelectionScrollChanged = "OnSelectionScrollChanged";

		public static readonly StringName CancelScrollSelection = "CancelScrollSelection";

		public static readonly StringName OnCardButtonPressed = "OnCardButtonPressed";

		public static readonly StringName OnCardButtonGuiInput = "OnCardButtonGuiInput";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ContainsSelectionPoint = "ContainsSelectionPoint";

		public static readonly StringName UpdateScrollSelection = "UpdateScrollSelection";

		public static readonly StringName EndScrollSelection = "EndScrollSelection";

		public static readonly StringName CompleteScrollSelection = "CompleteScrollSelection";

		public static readonly StringName SubscribeCooldownConfig = "SubscribeCooldownConfig";

		public static readonly StringName OnCooldownConfigChanged = "OnCooldownConfigChanged";

		public static readonly StringName RefreshCooldownLabel = "RefreshCooldownLabel";

		public static readonly StringName SetContainerFootprint = "SetContainerFootprint";

		public static readonly StringName ApplyContainerFootprint = "ApplyContainerFootprint";

		public static readonly StringName CanAffordItemCost = "CanAffordItemCost";

		public static readonly StringName MeetsSunBalanceRequirement = "MeetsSunBalanceRequirement";

		public static readonly StringName RefreshModCostQuote = "RefreshModCostQuote";

		public static readonly StringName ModAllowsPlacement = "ModAllowsPlacement";

		public static readonly StringName UpdateItemCostLabel = "UpdateItemCostLabel";

		public static readonly StringName SetCentralRuntimeStateRefresh = "SetCentralRuntimeStateRefresh";

		public static readonly StringName InvalidateRuntimeState = "InvalidateRuntimeState";

		public static readonly StringName RefreshRuntimeState = "RefreshRuntimeState";

		public static readonly StringName RefreshDynamicItemCost = "RefreshDynamicItemCost";

		public static readonly StringName HasRequiredPlantCover = "HasRequiredPlantCover";

		public static readonly StringName ApplyCachedRuntimeAvailability = "ApplyCachedRuntimeAvailability";

		public static readonly StringName NeedsContinuousPhysicsProcess = "NeedsContinuousPhysicsProcess";

		public static readonly StringName RefreshPhysicsProcessState = "RefreshPhysicsProcessState";

		public static readonly StringName ColorSet = "ColorSet";

		public static readonly StringName MobilePreset = "MobilePreset";

		public static readonly StringName SetPcPreset = "SetPcPreset";

		public static readonly StringName SetMobileMode = "SetMobileMode";

		public static readonly StringName RefreshPreview = "RefreshPreview";

		public static readonly StringName UpdateBackgroundTexture = "UpdateBackgroundTexture";

		public static readonly StringName UpdateSpriteLayout = "UpdateSpriteLayout";

		public static readonly StringName Clear = "Clear";

		public static readonly StringName ClearEventHandlers = "ClearEventHandlers";

		public static readonly StringName ResetForPool = "ResetForPool";

		public static readonly StringName Init = "Init";

		public static readonly StringName InitVisualPreview = "InitVisualPreview";

		public static readonly StringName ApplyInitializedUiState = "ApplyInitializedUiState";

		public static readonly StringName CreateSprite = "CreateSprite";

		public static readonly StringName OnCharacterSkinSwitched = "OnCharacterSkinSwitched";

		public static readonly StringName OnVisibilityChanged = "OnVisibilityChanged";

		public static readonly StringName ApplyDeferredVisibilityState = "ApplyDeferredVisibilityState";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _PhysicsProcess = "_PhysicsProcess";

		public static readonly StringName GrantExpireSun = "GrantExpireSun";

		public static readonly StringName Pressed = "Pressed";

		public static readonly StringName OnMouseEntered = "OnMouseEntered";

		public static readonly StringName OnMouseExited = "OnMouseExited";

		public static readonly StringName Reset = "Reset";

		public static readonly StringName CanUseFromMod = "CanUseFromMod";

		public static readonly StringName UsesCooldown = "UsesCooldown";

		public static readonly StringName ResolveModCooldown = "ResolveModCooldown";

		public static readonly StringName SetModObservableCooldown = "SetModObservableCooldown";

		public static readonly StringName NotifyModUseCancelled = "NotifyModUseCancelled";

		public static readonly StringName NotifyModRestored = "NotifyModRestored";

		public static readonly StringName StartInit = "StartInit";

		public static readonly StringName StartInitForRestore = "StartInitForRestore";

		public static readonly StringName StartInitCore = "StartInitCore";

		public static readonly StringName RestoreMagicBeanRestoreState = "RestoreMagicBeanRestoreState";

		public static readonly StringName Plant = "Plant";

		public static readonly StringName PlantFootprint = "PlantFootprint";

		public static readonly StringName PlantOnZombie = "PlantOnZombie";

		public static readonly StringName PlantOnPlant = "PlantOnPlant";

		public static readonly StringName Use = "Use";

		public static readonly StringName NotifyUseBehaviorSucceeded = "NotifyUseBehaviorSucceeded";

		public static readonly StringName ConsumePurchaseCostChanges = "ConsumePurchaseCostChanges";

		public static readonly StringName ApplyUseState = "ApplyUseState";

		public static readonly StringName LoveButtonToggled = "LoveButtonToggled";

		public static readonly StringName SetLoveButtonPressedFromSave = "SetLoveButtonPressedFromSave";

		public static readonly StringName TrySubscribeCharacterSkinSwitch = "TrySubscribeCharacterSkinSwitch";

		public static readonly StringName TryUnsubscribeCharacterSkinSwitch = "TryUnsubscribeCharacterSkinSwitch";

		public static readonly StringName TryGetMobilePresetConfig = "TryGetMobilePresetConfig";

		public static readonly StringName CanContinueAsync = "CanContinueAsync";

		public static readonly StringName HasPreviewControls = "HasPreviewControls";

		public static readonly StringName SetPreviewCreationDeferred = "SetPreviewCreationDeferred";

		public static readonly StringName Cover = "Cover";

		public static readonly StringName SetPreviewVisible = "SetPreviewVisible";

		public static readonly StringName ConfigurePreviewSprite = "ConfigurePreviewSprite";

		public static readonly StringName ConfigurePreviewNode = "ConfigurePreviewNode";

		public static readonly StringName ConfigurePreviewTreeBeforeAttach = "ConfigurePreviewTreeBeforeAttach";

		public static readonly StringName FreezePreviewTree = "FreezePreviewTree";

		public static readonly StringName FreezePreparedPreviewTree = "FreezePreparedPreviewTree";

		public static readonly StringName SynchronizeSameDataPreviewChildren = "SynchronizeSameDataPreviewChildren";

		public static readonly StringName ApplyProphecy = "ApplyProphecy";

		public static readonly StringName ProcessProphecy = "ProcessProphecy";

		public static readonly StringName UpdateProphecyBorder = "UpdateProphecyBorder";

		public static readonly StringName ConsumeProphecy = "ConsumeProphecy";

		public static readonly StringName ExportProphecySave = "ExportProphecySave";

		public static readonly StringName ImportProphecySave = "ImportProphecySave";

		public static readonly StringName FlashCostChange = "FlashCostChange";

		public static readonly StringName ResetCostChangeFlash = "ResetCostChangeFlash";

		public static readonly StringName PlayPreviewTreeFromStart = "PlayPreviewTreeFromStart";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName HasSunAccount = "HasSunAccount";

		public static readonly StringName config = "config";

		public static readonly StringName showLove = "showLove";

		public static readonly StringName showCost = "showCost";

		public static readonly StringName onlyDraw = "onlyDraw";

		public static readonly StringName alive = "alive";

		public static readonly StringName @lock = "lock";

		public static readonly StringName openShadow = "openShadow";

		public static readonly StringName start = "start";

		public static readonly StringName select = "select";

		public static readonly StringName ModUseSequence = "ModUseSequence";

		public static readonly StringName ModSelectionConsumed = "ModSelectionConsumed";

		public static readonly StringName coldDownOpen = "coldDownOpen";

		public static readonly StringName pressDelayTimer = "pressDelayTimer";

		public static readonly StringName aliveTime = "aliveTime";

		public static readonly StringName height = "height";

		public static readonly StringName IsPreviewCreationDeferred = "IsPreviewCreationDeferred";

		public static readonly StringName HasPreparedPreview = "HasPreparedPreview";

		public static readonly StringName PreparedPreviewClip = "PreparedPreviewClip";

		public static readonly StringName itemCost = "itemCost";

		public static readonly StringName IsProphesied = "IsProphesied";

		public static readonly StringName ProphecyRemaining = "ProphecyRemaining";

		public static readonly StringName _asyncLifetimeVersion = "_asyncLifetimeVersion";

		public static readonly StringName _exitedTree = "_exitedTree";

		public static readonly StringName _visibilityRefreshQueued = "_visibilityRefreshQueued";

		public static readonly StringName _queuedVisibilityRefreshVersion = "_queuedVisibilityRefreshVersion";

		public static readonly StringName _centralRuntimeStateRefresh = "_centralRuntimeStateRefresh";

		public static readonly StringName _runtimeAvailabilityDirty = "_runtimeAvailabilityDirty";

		public static readonly StringName _runtimeCostDirty = "_runtimeCostDirty";

		public static readonly StringName _cachedRuntimeAvailability = "_cachedRuntimeAvailability";

		public static readonly StringName _cachedUpgradePacket = "_cachedUpgradePacket";

		public static readonly StringName _modCostQuoteBase = "_modCostQuoteBase";

		public static readonly StringName _modCostQuoteRevision = "_modCostQuoteRevision";

		public static readonly StringName _modCostQuote = "_modCostQuote";

		public static readonly StringName _modCostQuoteReady = "_modCostQuoteReady";

		public static readonly StringName _modCostQuoteSuccess = "_modCostQuoteSuccess";

		public static readonly StringName _resolvingModCostQuote = "_resolvingModCostQuote";

		public static readonly StringName _itemCostLabelQuoteValid = "_itemCostLabelQuoteValid";

		public static readonly StringName _modCooldownPrepared = "_modCooldownPrepared";

		public static readonly StringName _modCooldownPreparedValue = "_modCooldownPreparedValue";

		public static readonly StringName previewClip = "previewClip";

		public static readonly StringName previewSpriteNode = "previewSpriteNode";

		public static readonly StringName backgroundTexture = "backgroundTexture";

		public static readonly StringName selectTexture = "selectTexture";

		public static readonly StringName layout = "layout";

		public static readonly StringName body = "body";

		public static readonly StringName itemCostLabel = "itemCostLabel";

		public static readonly StringName button = "button";

		public static readonly StringName coldDownProgressBar = "coldDownProgressBar";

		public static readonly StringName _cooldownLabel = "_cooldownLabel";

		public static readonly StringName _showPacketCooldown = "_showPacketCooldown";

		public static readonly StringName _cooldownConfigOwner = "_cooldownConfigOwner";

		public static readonly StringName _cooldownText = "_cooldownText";

		public static readonly StringName _selectionScroll = "_selectionScroll";

		public static readonly StringName _selectionWindow = "_selectionWindow";

		public static readonly StringName _scrollGestureActive = "_scrollGestureActive";

		public static readonly StringName _scrollGestureCancelled = "_scrollGestureCancelled";

		public static readonly StringName _pendingScrollClick = "_pendingScrollClick";

		public static readonly StringName _scrollPointerIndex = "_scrollPointerIndex";

		public static readonly StringName _scrollGestureVersion = "_scrollGestureVersion";

		public static readonly StringName _scrollPressPosition = "_scrollPressPosition";

		public static readonly StringName _scrollPressConfig = "_scrollPressConfig";

		public static readonly StringName loveButton = "loveButton";

		public static readonly StringName moveComponent = "moveComponent";

		public static readonly StringName _reserveContainerFootprint = "_reserveContainerFootprint";

		public static readonly StringName _config = "_config";

		public static readonly StringName _showLove = "_showLove";

		public static readonly StringName _syncingLoveButtonFromSave = "_syncingLoveButtonFromSave";

		public static readonly StringName _showCost = "_showCost";

		public static readonly StringName _onlyDraw = "_onlyDraw";

		public static readonly StringName _alive = "_alive";

		public static readonly StringName _lock = "_lock";

		public static readonly StringName plantOnce = "plantOnce";

		public static readonly StringName useCost = "useCost";

		public static readonly StringName _openShadow = "_openShadow";

		public static readonly StringName _start = "_start";

		public static readonly StringName _select = "_select";

		public static readonly StringName coldDown = "coldDown";

		public static readonly StringName _coldDownOpen = "_coldDownOpen";

		public static readonly StringName coldDownTimer = "coldDownTimer";

		public static readonly StringName _pressDelayTimer = "_pressDelayTimer";

		public static readonly StringName setMobileLayout = "setMobileLayout";

		public static readonly StringName setPcLayout = "setPcLayout";

		public static readonly StringName canPressPutBack = "canPressPutBack";

		public static readonly StringName enforceRuntimeAvailabilityOnPress = "enforceRuntimeAvailabilityOnPress";

		public static readonly StringName allowPressWhenUnavailable = "allowPressWhenUnavailable";

		public static readonly StringName isMobile = "isMobile";

		public static readonly StringName pcProgressTexture = "pcProgressTexture";

		public static readonly StringName _aliveTime = "_aliveTime";

		public static readonly StringName aliveTimer = "aliveTimer";

		public static readonly StringName blinkTimer = "blinkTimer";

		public static readonly StringName blink = "blink";

		public static readonly StringName _height = "_height";

		public static readonly StringName savePos = "savePos";

		public static readonly StringName originalSaveKey = "originalSaveKey";

		public static readonly StringName sprite = "sprite";

		public static readonly StringName _livePreviewGeneration = "_livePreviewGeneration";

		public static readonly StringName _previewHovered = "_previewHovered";

		public static readonly StringName _previewCreationDeferred = "_previewCreationDeferred";

		public static readonly StringName baseItemCost = "baseItemCost";

		public static readonly StringName _itemCostLabelInitialized = "_itemCostLabelInitialized";

		public static readonly StringName _itemCostLabelUsesRiseSuffix = "_itemCostLabelUsesRiseSuffix";

		public static readonly StringName _itemCostLabelValue = "_itemCostLabelValue";

		public static readonly StringName _costChangeFlashTween = "_costChangeFlashTween";

		public static readonly StringName _costChangeFlashBaseScale = "_costChangeFlashBaseScale";

		public static readonly StringName _costChangeFlashActive = "_costChangeFlashActive";

		public static readonly StringName _itemCost = "_itemCost";

		public static readonly StringName riseCost = "riseCost";

		public static readonly StringName costMultiple = "costMultiple";

		public static readonly StringName _footprintPlacementPending = "_footprintPlacementPending";

		public static readonly StringName _prophecyBorder = "_prophecyBorder";

		public static readonly StringName _prophecyRemaining = "_prophecyRemaining";

		public static readonly StringName _prophecyPosition = "_prophecyPosition";

		public static readonly StringName _prophecyCharmed = "_prophecyCharmed";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly Vector2 PcContainerFootprint = new Vector2(50f, 70f);

	private static readonly Vector2 MobileContainerFootprint = new Vector2(96f, 60f);

	private int _asyncLifetimeVersion;

	private bool _exitedTree = true;

	private bool _visibilityRefreshQueued;

	private int _queuedVisibilityRefreshVersion;

	private bool _centralRuntimeStateRefresh;

	private bool _runtimeAvailabilityDirty = true;

	private bool _runtimeCostDirty = true;

	private bool _cachedRuntimeAvailability = true;

	private bool _cachedUpgradePacket;

	private XWModRuntimeSession _modCostQuoteSession;

	private long _modCostQuoteBase;

	private long _modCostQuoteRevision;

	private long _modCostQuote;

	private bool _modCostQuoteReady;

	private bool _modCostQuoteSuccess;

	private bool _resolvingModCostQuote;

	private bool _itemCostLabelQuoteValid = true;

	private bool _modCooldownPrepared;

	private double _modCooldownPreparedValue;

	private static Font _fzkt;

	private static Font _pcFont;

	private static Texture2D _mobileProgressTexture;

	private static Texture2D _packetColour;

	private static Texture2D _packetDiamond;

	private static Texture2D _packetGold;

	private static Texture2D _packetNormal;

	private static Texture2D _packetStar;

	private static Texture2D _packetZombie;

	private static Texture2D _packetCover;

	private static Texture2D _packetGray;

	private const float CostChangeFlashScale = 1.14f;

	private static readonly Color CostChangeFlashColor = new Color(1f, 0.9f, 0.2f);

	private static Texture2D _packetColourMobile;

	private static Texture2D _packetCoverMobile;

	private static Texture2D _packetDiamondMobile;

	private static Texture2D _packetGoldMobile;

	private static Texture2D _packetNormalMobile;

	private static Texture2D _packetStarMobile;

	private static Texture2D _packetZombieMobile;

	public Control previewClip;

	public Control previewSpriteNode;

	public TextureRect backgroundTexture;

	public NinePatchRect selectTexture;

	public Control layout;

	public Control body;

	public Label itemCostLabel;

	public Button button;

	public TextureProgressBar coldDownProgressBar;

	public const string ShowPacketCooldownConfigKey = "ShowPacketCooldown";

	private Label _cooldownLabel;

	private bool _showPacketCooldown;

	private GameSaveManager _cooldownConfigOwner;

	private string _cooldownText;

	private const float ScrollSelectionDragDistance = 24f;

	private ScrollContainer _selectionScroll;

	private Window _selectionWindow;

	private bool _scrollGestureActive;

	private bool _scrollGestureCancelled;

	private bool _pendingScrollClick;

	private int _scrollPointerIndex = -1;

	private int _scrollGestureVersion;

	private Vector2 _scrollPressPosition;

	private TowerDefensePacketConfig _scrollPressConfig;

	public TextureButton loveButton;

	public MoveComponent moveComponent;

	private bool _reserveContainerFootprint;

	private TowerDefensePacketConfig _config;

	private readonly CardBehaviorHost _cardBehaviorHost = new CardBehaviorHost();

	private EconomyAccountId _sunAccountId;

	private bool _showLove;

	private bool _syncingLoveButtonFromSave;

	private bool _showCost = true;

	private bool _onlyDraw;

	private bool _alive = true;

	private bool _lock;

	[Export(PropertyHint.None, "")]
	public bool plantOnce;

	[Export(PropertyHint.None, "")]
	public bool useCost = true;

	private bool _openShadow;

	private bool _start;

	private bool _select;

	public double coldDown;

	private bool _coldDownOpen;

	public double coldDownTimer;

	private double _pressDelayTimer;

	[Export(PropertyHint.None, "")]
	public bool setMobileLayout;

	[Export(PropertyHint.None, "")]
	public bool setPcLayout;

	[Export(PropertyHint.None, "")]
	public bool canPressPutBack = true;

	public bool enforceRuntimeAvailabilityOnPress = true;

	public bool allowPressWhenUnavailable;

	public bool isMobile;

	public Texture2D pcProgressTexture;

	private double _aliveTime = -1.0;

	public double aliveTimer;

	public double blinkTimer;

	public bool blink;

	private double _height = -1.0;

	public Vector2 savePos = Vector2.Zero;

	public string originalSaveKey = "";

	public AdobeAnimateSprite sprite;

	private int _livePreviewGeneration;

	private bool _previewHovered;

	private bool _previewCreationDeferred;

	private HashSet<string> _extendCoverNames;

	public long baseItemCost;

	private bool _itemCostLabelInitialized;

	private bool _itemCostLabelUsesRiseSuffix;

	private long _itemCostLabelValue;

	private Tween _costChangeFlashTween;

	private Vector2 _costChangeFlashBaseScale = Vector2.One;

	private bool _costChangeFlashActive;

	private long _itemCost = 100L;

	public int riseCost = -1;

	public double costMultiple = -1.0;

	private bool _footprintPlacementPending;

	private static Texture2D _prophecyPortrait;

	private static Texture2D _prophecyLandscape;

	private TextureRect _prophecyBorder;

	private double _prophecyRemaining;

	private Vector2 _prophecyPosition;

	private bool _prophecyCharmed;

	private static Font FZKT => _fzkt ?? (_fzkt = GD.Load<Font>("uid://coqskwlqtnypf"));

	private static Font PC_FONT => _pcFont ?? (_pcFont = GD.Load<Font>("uid://dww00k8yk5k72"));

	private static Texture2D MOBILE_PROGRESS_TEXTURE => _mobileProgressTexture ?? (_mobileProgressTexture = GD.Load<Texture2D>("uid://cy8jc6hsspvfy"));

	private static Texture2D PACKET_COLOUR => _packetColour ?? (_packetColour = GD.Load<Texture2D>("uid://dricqt0scm3sm"));

	private static Texture2D PACKET_DIAMOND => _packetDiamond ?? (_packetDiamond = GD.Load<Texture2D>("uid://eutur83nlbar"));

	private static Texture2D PACKET_GOLD => _packetGold ?? (_packetGold = GD.Load<Texture2D>("uid://dfihegby6yat6"));

	private static Texture2D PACKET_NORMAL => _packetNormal ?? (_packetNormal = GD.Load<Texture2D>("uid://bwksngvkn16cd"));

	private static Texture2D PACKET_STAR => _packetStar ?? (_packetStar = GD.Load<Texture2D>("uid://bwnw5thitfc8e"));

	private static Texture2D PACKET_ZOMBIE => _packetZombie ?? (_packetZombie = GD.Load<Texture2D>("uid://btgdkkg66xc8d"));

	private static Texture2D PACKET_COVER => _packetCover ?? (_packetCover = GD.Load<Texture2D>("uid://dbkcwpq2ie7t0"));

	private static Texture2D PACKET_GRAY => _packetGray ?? (_packetGray = GD.Load<Texture2D>("uid://dfy7jg4c8v30x"));

	private static Texture2D PACKET_COLOUR_MOBILE => _packetColourMobile ?? (_packetColourMobile = GD.Load<Texture2D>("uid://cdrj5bkubm7la"));

	private static Texture2D PACKET_COVER_MOBILE => _packetCoverMobile ?? (_packetCoverMobile = GD.Load<Texture2D>("uid://3k84w2g5d10r"));

	private static Texture2D PACKET_DIAMOND_MOBILE => _packetDiamondMobile ?? (_packetDiamondMobile = GD.Load<Texture2D>("uid://1242bw5k8uwq"));

	private static Texture2D PACKET_GOLD_MOBILE => _packetGoldMobile ?? (_packetGoldMobile = GD.Load<Texture2D>("uid://2smnyulcahs3"));

	private static Texture2D PACKET_NORMAL_MOBILE => _packetNormalMobile ?? (_packetNormalMobile = GD.Load<Texture2D>("uid://bwfy3kman4ich"));

	private static Texture2D PACKET_STAR_MOBILE => _packetStarMobile ?? (_packetStarMobile = GD.Load<Texture2D>("uid://dxa7vky1ueb2o"));

	private static Texture2D PACKET_ZOMBIE_MOBILE => _packetZombieMobile ?? (_packetZombieMobile = GD.Load<Texture2D>("uid://csgt5dkel0xeb"));

	public EconomyAccountId SunAccountId => _sunAccountId;

	public bool HasSunAccount => _sunAccountId.IsValid;

	public TowerDefensePacketConfig config
	{
		get
		{
			return _config;
		}
		set
		{
			if (_config != value)
			{
				_modCostQuoteReady = false;
			}
			_config = value;
			if (backgroundTexture != null)
			{
				backgroundTexture.Visible = _config != null;
			}
			if (GodotObject.IsInstanceValid(button))
			{
				button.Visible = _config != null;
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public bool showLove
	{
		get
		{
			return _showLove;
		}
		set
		{
			_showLove = value;
			if (IsNodeReady())
			{
				loveButton.Visible = _showLove;
				if (config != null && TryGetPacketSaveValue(config.saveKey, out var packetData))
				{
					SetLoveButtonPressedFromSave(packetData.GetValueOrDefault("Love", Variant.From<bool>(false)).AsBool());
				}
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public bool showCost
	{
		get
		{
			return _showCost;
		}
		set
		{
			_showCost = value;
			if (IsNodeReady() && GodotObject.IsInstanceValid(itemCostLabel))
			{
				itemCostLabel.Visible = _showCost;
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public bool onlyDraw
	{
		get
		{
			return _onlyDraw;
		}
		set
		{
			if (_onlyDraw != value)
			{
				_onlyDraw = value;
				RefreshCooldownLabel();
				if (IsInsideTree())
				{
					RefreshPhysicsProcessState();
				}
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public bool alive
	{
		get
		{
			return _alive;
		}
		set
		{
			if (_alive != value)
			{
				_alive = value;
				ColorSet();
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public bool @lock
	{
		get
		{
			return _lock;
		}
		set
		{
			_lock = value;
			ColorSet();
		}
	}

	[Export(PropertyHint.None, "")]
	public bool openShadow
	{
		get
		{
			return _openShadow;
		}
		set
		{
			_openShadow = value;
			ColorSet();
		}
	}

	public bool start
	{
		get
		{
			return _start;
		}
		set
		{
			if (_start != value)
			{
				_start = value;
				RefreshCooldownLabel();
				InvalidateRuntimeState(includeCost: true);
			}
		}
	}

	public bool select
	{
		get
		{
			return _select;
		}
		set
		{
			_select = value;
			if (selectTexture != null && GodotObject.IsInstanceValid(selectTexture))
			{
				selectTexture.Visible = _select;
			}
		}
	}

	internal long ModUseSequence { get; private set; }

	internal bool ModSelectionConsumed { get; private set; }

	public bool coldDownOpen
	{
		get
		{
			return _coldDownOpen;
		}
		set
		{
			if (_coldDownOpen != value)
			{
				_coldDownOpen = value;
				_runtimeAvailabilityDirty = true;
				if (!_coldDownOpen && GodotObject.IsInstanceValid(coldDownProgressBar))
				{
					coldDownProgressBar.Visible = false;
				}
				RefreshCooldownLabel();
				RefreshPhysicsProcessState();
			}
		}
	}

	public double pressDelayTimer
	{
		get
		{
			return _pressDelayTimer;
		}
		set
		{
			if (!Mathf.IsEqualApprox(_pressDelayTimer, value))
			{
				_pressDelayTimer = value;
				RefreshPhysicsProcessState();
			}
		}
	}

	public double aliveTime
	{
		get
		{
			return _aliveTime;
		}
		set
		{
			if (!Mathf.IsEqualApprox(_aliveTime, value))
			{
				_aliveTime = value;
				RefreshPhysicsProcessState();
			}
		}
	}

	public double height
	{
		get
		{
			return _height;
		}
		set
		{
			if (!Mathf.IsEqualApprox(_height, value))
			{
				_height = value;
				RefreshPhysicsProcessState();
			}
		}
	}

	public bool IsPreviewCreationDeferred => _previewCreationDeferred;

	public bool HasPreparedPreview
	{
		get
		{
			if (GodotObject.IsInstanceValid(sprite) && sprite.IsFrozenPreview)
			{
				return sprite.frameIndex == sprite.clipRange.X;
			}
			return false;
		}
	}

	public string PreparedPreviewClip
	{
		get
		{
			if (!GodotObject.IsInstanceValid(sprite))
			{
				return "";
			}
			return sprite.clip ?? "";
		}
	}

	public long itemCost
	{
		get
		{
			return _itemCost;
		}
		set
		{
			bool flag = riseCost != -1;
			if (_itemCost != value || !_itemCostLabelInitialized || _itemCostLabelUsesRiseSuffix != flag)
			{
				if (_itemCost != value)
				{
					_modCostQuoteReady = false;
				}
				_itemCost = value;
				UpdateItemCostLabel();
			}
		}
	}

	private static Texture2D ProphecyPortrait => _prophecyPortrait ?? (_prophecyPortrait = GD.Load<Texture2D>("uid://ng6vh2msncos"));

	private static Texture2D ProphecyLandscape => _prophecyLandscape ?? (_prophecyLandscape = GD.Load<Texture2D>("uid://cgs40daafcbtd"));

	public bool IsProphesied => _prophecyRemaining > 0.0;

	public double ProphecyRemaining => _prophecyRemaining;

	public event PressedEventHandler OnPressed;

	public event LoveChangeEventHandler OnLoveChange;

	public void SetScrollSelectionMode(ScrollContainer scroll)
	{
		CancelScrollSelection();
		if (GodotObject.IsInstanceValid(_selectionScroll))
		{
			_selectionScroll.GetVScrollBar().ValueChanged -= OnSelectionScrollChanged;
			_selectionScroll.GetHScrollBar().ValueChanged -= OnSelectionScrollChanged;
			_selectionScroll.ScrollStarted -= CancelScrollSelection;
		}
		if (GodotObject.IsInstanceValid(_selectionWindow))
		{
			_selectionWindow.FocusExited -= CancelScrollSelection;
		}
		_selectionScroll = scroll;
		_selectionWindow = null;
		if (GodotObject.IsInstanceValid(scroll))
		{
			scroll.GetVScrollBar().ValueChanged += OnSelectionScrollChanged;
			scroll.GetHScrollBar().ValueChanged += OnSelectionScrollChanged;
			scroll.ScrollStarted += CancelScrollSelection;
			if (IsInsideTree())
			{
				_selectionWindow = GetWindow();
				_selectionWindow.FocusExited += CancelScrollSelection;
			}
		}
	}

	private void OnSelectionScrollChanged(double value)
	{
		CancelScrollSelection();
	}

	private void CancelScrollSelection()
	{
		_scrollGestureVersion++;
		_scrollGestureActive = false;
		_pendingScrollClick = false;
		_scrollPressConfig = null;
		SetProcessInput(enable: false);
	}

	private void OnCardButtonPressed()
	{
		if (!GodotObject.IsInstanceValid(_selectionScroll))
		{
			Pressed();
		}
	}

	private void OnCardButtonGuiInput(InputEvent input)
	{
		if (!GodotObject.IsInstanceValid(_selectionScroll) || button.Disabled || !IsVisibleInTree() || (long)input.Device == -1)
		{
			return;
		}
		if ((input is InputEventKey || input is InputEventJoypadButton || input is InputEventJoypadMotion || input is InputEventAction) && input.IsActionPressed("ui_accept"))
		{
			CancelScrollSelection();
			Pressed();
			button.AcceptEvent();
			return;
		}
		Vector2 position;
		int scrollPointerIndex;
		if (input is InputEventScreenTouch { Pressed: not false } inputEventScreenTouch)
		{
			position = inputEventScreenTouch.Position;
			scrollPointerIndex = inputEventScreenTouch.Index;
		}
		else
		{
			if (!(input is InputEventMouseButton { Pressed: not false } inputEventMouseButton) || inputEventMouseButton.ButtonIndex != MouseButton.Left)
			{
				return;
			}
			position = inputEventMouseButton.Position;
			scrollPointerIndex = -1;
		}
		if (!_scrollGestureActive)
		{
			CancelScrollSelection();
			_scrollGestureActive = true;
			_scrollGestureCancelled = false;
			_scrollPointerIndex = scrollPointerIndex;
			_scrollPressConfig = config;
			_scrollPressPosition = button.GetGlobalTransformWithCanvas() * position;
			SetProcessInput(enable: true);
		}
	}

	public override void _Input(InputEvent input)
	{
		if (!_scrollGestureActive || (long)input.Device == -1)
		{
			return;
		}
		if (!(input is InputEventScreenDrag inputEventScreenDrag))
		{
			if (!(input is InputEventScreenTouch inputEventScreenTouch))
			{
				if (!(input is InputEventMouseMotion inputEventMouseMotion))
				{
					if (input is InputEventMouseButton { Pressed: false } inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && _scrollPointerIndex == -1)
					{
						EndScrollSelection(inputEventMouseButton.Position);
					}
				}
				else if (_scrollPointerIndex == -1)
				{
					UpdateScrollSelection(inputEventMouseMotion.Position);
				}
			}
			else if (inputEventScreenTouch.Index == _scrollPointerIndex && !inputEventScreenTouch.Pressed)
			{
				if (inputEventScreenTouch.Canceled)
				{
					CancelScrollSelection();
				}
				else
				{
					EndScrollSelection(inputEventScreenTouch.Position);
				}
			}
		}
		else if (inputEventScreenDrag.Index == _scrollPointerIndex)
		{
			UpdateScrollSelection(inputEventScreenDrag.Position);
		}
	}

	private bool ContainsSelectionPoint(Vector2 viewportPosition)
	{
		if (!GodotObject.IsInstanceValid(button) || !GodotObject.IsInstanceValid(_selectionScroll))
		{
			return false;
		}
		Vector2 point = button.GetGlobalTransformWithCanvas().AffineInverse() * viewportPosition;
		Vector2 point2 = _selectionScroll.GetGlobalTransformWithCanvas().AffineInverse() * viewportPosition;
		if (new Rect2(Vector2.Zero, button.Size).HasPoint(point))
		{
			return new Rect2(Vector2.Zero, _selectionScroll.Size).HasPoint(point2);
		}
		return false;
	}

	private void UpdateScrollSelection(Vector2 position)
	{
		if (position.DistanceSquaredTo(_scrollPressPosition) >= 576f || !ContainsSelectionPoint(position))
		{
			_scrollGestureCancelled = true;
		}
	}

	private void EndScrollSelection(Vector2 position)
	{
		UpdateScrollSelection(position);
		_scrollGestureActive = false;
		_pendingScrollClick = true;
		CallDeferred("CompleteScrollSelection", _scrollGestureVersion);
	}

	private void CompleteScrollSelection(int version)
	{
		if (version == _scrollGestureVersion && _pendingScrollClick)
		{
			bool num = !_scrollGestureCancelled && IsInsideTree() && IsVisibleInTree() && !IsQueuedForDeletion() && GodotObject.IsInstanceValid(_selectionScroll) && config == _scrollPressConfig && !button.Disabled;
			CancelScrollSelection();
			if (num)
			{
				Pressed();
			}
		}
	}

	private void SubscribeCooldownConfig()
	{
		if (!GodotObject.IsInstanceValid(_cooldownConfigOwner) && GodotObject.IsInstanceValid(GameSaveManager.Instance))
		{
			_cooldownConfigOwner = GameSaveManager.Instance;
			_showPacketCooldown = _cooldownConfigOwner.GetConfigValue("ShowPacketCooldown").AsBool();
			_cooldownConfigOwner.ConfigValueChanged += OnCooldownConfigChanged;
		}
	}

	private void OnCooldownConfigChanged(string key, Variant value)
	{
		if (!(key != "ShowPacketCooldown"))
		{
			_showPacketCooldown = value.AsBool();
			RefreshCooldownLabel();
		}
	}

	private void RefreshCooldownLabel()
	{
		if (!GodotObject.IsInstanceValid(_cooldownLabel))
		{
			return;
		}
		bool flag = _showPacketCooldown && !onlyDraw && start && coldDownOpen && coldDownTimer > 0.0 && !HasMeta("is_upgrade_packet") && IsVisibleInTree();
		_cooldownLabel.Visible = flag;
		if (flag)
		{
			string text = coldDownTimer.ToString("0.0", CultureInfo.InvariantCulture);
			if (_cooldownText != text)
			{
				_cooldownText = text;
				_cooldownLabel.Text = text;
			}
		}
	}

	public void SetContainerFootprint(bool enabled)
	{
		_reserveContainerFootprint = enabled;
		ApplyContainerFootprint();
	}

	private void ApplyContainerFootprint()
	{
		Vector2 vector = (isMobile ? MobileContainerFootprint : PcContainerFootprint);
		base.CustomMinimumSize = (_reserveContainerFootprint ? vector : Vector2.Zero);
		if (GodotObject.IsInstanceValid(body))
		{
			body.Position = (_reserveContainerFootprint ? (vector / 2f) : Vector2.Zero);
		}
	}

	public bool TryBindSunAccount(EconomyAccountId accountId)
	{
		if (!accountId.IsValid)
		{
			return false;
		}
		if (_sunAccountId.IsValid)
		{
			return _sunAccountId == accountId;
		}
		_sunAccountId = accountId;
		return true;
	}

	private bool CanAffordItemCost()
	{
		if (!MeetsSunBalanceRequirement())
		{
			return false;
		}
		if (!useCost)
		{
			return true;
		}
		if (!TryResolveModItemCost(out var cost))
		{
			return false;
		}
		UpdateItemCostLabel();
		if (!HasSunAccount)
		{
			return TowerDefenseManager.Instance.GetSun() >= cost;
		}
		return TowerDefenseManager.Instance.CanAffordSun(_sunAccountId, cost);
	}

	private bool MeetsSunBalanceRequirement()
	{
		TowerDefensePacketConfig towerDefensePacketConfig = config;
		if (towerDefensePacketConfig == null || !towerDefensePacketConfig.disableWhenSunNegative)
		{
			return true;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(instance))
		{
			return false;
		}
		EconomyAccountId accountId = (HasSunAccount ? _sunAccountId : instance.GetLocalSunAccountId());
		if (instance.TryGetSun(accountId, out var balance))
		{
			return balance >= 0;
		}
		return false;
	}

	private bool TryBeginItemCost(bool useSun, out SunSpendReceipt receipt)
	{
		receipt = null;
		if (!_modCooldownPrepared && UsesCooldown())
		{
			try
			{
				_modCooldownPreparedValue = ResolveModCooldown(config.GetPacketCooldown(), initial: false);
			}
			catch (XWModHookCallbackException)
			{
				return false;
			}
			_modCooldownPrepared = true;
		}
		if (!useCost || !useSun)
		{
			return true;
		}
		if (!MeetsSunBalanceRequirement())
		{
			return false;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(instance))
		{
			return false;
		}
		EconomyAccountId accountId = (HasSunAccount ? _sunAccountId : instance.GetLocalSunAccountId());
		if (!TryResolveModItemCost(out var cost))
		{
			return false;
		}
		UpdateItemCostLabel();
		return instance.TryBeginSunSpend(accountId, cost, out receipt);
	}

	private bool TryResolveModItemCost(out long cost)
	{
		cost = itemCost;
		if (_resolvingModCostQuote)
		{
			return false;
		}
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		XWModHookDispatcher xWModHookDispatcher = current?.Hooks;
		if (!GodotObject.IsInstanceValid(config) || onlyDraw || !useCost || xWModHookDispatcher == null || (!xWModHookDispatcher.HasHandlers(XWModHookPoints.PlantCost.Id) && !xWModHookDispatcher.HasHandlers(XWModCardPoints.Cost.Id)))
		{
			return true;
		}
		if (_modCostQuoteReady && _modCostQuoteSession == current && _modCostQuoteBase == cost && _modCostQuoteRevision == xWModHookDispatcher.Revision)
		{
			cost = _modCostQuote;
			return _modCostQuoteSuccess;
		}
		_modCostQuoteSession = current;
		_modCostQuoteRevision = xWModHookDispatcher.Revision;
		_modCostQuoteBase = cost;
		_modCostQuote = cost;
		_modCostQuoteReady = true;
		_modCostQuoteSuccess = false;
		XWModPlantCostRequest request = new XWModPlantCostRequest
		{
			Packet = this,
			Cost = cost
		};
		_resolvingModCostQuote = true;
		try
		{
			cost = xWModHookDispatcher.Invoke(XWModHookPoints.PlantCost, request, () => request.Cost);
			if (!HasSunAccount || SunAccountId.IsLocal)
			{
				cost = XWModExtensions.Modify(XWModCardPoints.Cost, new XWModCardCost(XWModCards.GetHandle(this), config.saveKey, cost), XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Owner, "Quote")).Cost;
			}
		}
		catch (XWModHookCallbackException)
		{
			return false;
		}
		finally
		{
			_resolvingModCostQuote = false;
		}
		if (xWModHookDispatcher.Revision != _modCostQuoteRevision || itemCost != _modCostQuoteBase || !_modCostQuoteReady)
		{
			_modCostQuoteReady = false;
			XWModEnvironmentService.Invalidate("PlantCostHookMutatedQuoteInputs");
			GD.PushWarning("Mod 费用回调在报价过程中改变了价格或登记结构；本次购买已中止。");
			return false;
		}
		_modCostQuote = cost;
		return _modCostQuoteSuccess = cost != -9223372036854775808L;
	}

	public void RefreshModCostQuote()
	{
		_modCostQuoteReady = false;
		UpdateItemCostLabel(force: true);
		InvalidateRuntimeState();
	}

	private bool ModAllowsPlacement(Vector2I cell, TowerDefenseCharacter target = null)
	{
		XWModHookDispatcher xWModHookDispatcher = XWModRuntimeSession.Current?.Hooks;
		if (xWModHookDispatcher == null || !xWModHookDispatcher.HasHandlers(XWModHookPoints.PlantPlacement.Id))
		{
			return true;
		}
		XWModPlantPlacementRequest request = new XWModPlantPlacementRequest
		{
			Packet = this,
			Target = target,
			Cell = cell
		};
		return xWModHookDispatcher.Invoke(XWModHookPoints.PlantPlacement, request, () => true);
	}

	private void UpdateItemCostLabel(bool force = false)
	{
		if (!GodotObject.IsInstanceValid(itemCostLabel))
		{
			_itemCostLabelInitialized = false;
			return;
		}
		long cost;
		bool flag = TryResolveModItemCost(out cost);
		bool flag2 = riseCost != -1;
		if (force || !_itemCostLabelInitialized || _itemCostLabelValue != cost || _itemCostLabelUsesRiseSuffix != flag2 || _itemCostLabelQuoteValid != flag)
		{
			itemCostLabel.Text = ((!flag) ? "—" : (flag2 ? (cost + "+") : cost.ToString()));
			_itemCostLabelInitialized = true;
			_itemCostLabelValue = cost;
			_itemCostLabelUsesRiseSuffix = flag2;
			_itemCostLabelQuoteValid = flag;
		}
	}

	public void SetCentralRuntimeStateRefresh(bool enabled)
	{
		if (_centralRuntimeStateRefresh != enabled)
		{
			_centralRuntimeStateRefresh = enabled;
			InvalidateRuntimeState(includeCost: true);
		}
	}

	public void InvalidateRuntimeState(bool includeCost = false)
	{
		_runtimeAvailabilityDirty = true;
		if (includeCost)
		{
			_runtimeCostDirty = true;
		}
		RefreshPhysicsProcessState();
	}

	public void RefreshRuntimeState(bool includeCost = false)
	{
		if (includeCost)
		{
			_runtimeCostDirty = true;
		}
		bool flag = (_cachedUpgradePacket = HasMeta("is_upgrade_packet"));
		if (!GodotObject.IsInstanceValid(config) || (!start && !flag))
		{
			_runtimeAvailabilityDirty = false;
			_runtimeCostDirty = false;
			RefreshPhysicsProcessState();
			return;
		}
		if (_runtimeCostDirty && !flag)
		{
			RefreshDynamicItemCost();
		}
		_runtimeCostDirty = false;
		_cachedRuntimeAvailability = CanAffordItemCost() && (flag || HasRequiredPlantCover());
		_runtimeAvailabilityDirty = false;
		ApplyCachedRuntimeAvailability();
		RefreshPhysicsProcessState();
	}

	private void RefreshDynamicItemCost()
	{
		baseItemCost = config.GetCost();
		long num = baseItemCost;
		if (!TowerDefenseManager.MapIgnoresDynamicPacketCostGrowth(config._GetType()))
		{
			int characterNum = TowerDefenseManager.Instance.GetCharacterNum(config.saveKey);
			if (costMultiple != -1.0)
			{
				double num2 = (double)num * (double)Mathf.Pow((float)costMultiple, characterNum);
				num = ((!(num2 > 9.223372036854776E+18) && !double.IsNaN(num2)) ? ((long)Mathf.Floor(num2)) : 9223372036854775807L);
			}
			if (riseCost != -1)
			{
				num += (long)characterNum * (long)riseCost;
			}
		}
		itemCost = num;
	}

	private bool HasRequiredPlantCover()
	{
		Array<string> plantCover = config.GetPlantCover();
		if (plantCover != null && plantCover.Count > 0)
		{
			bool flag = false;
			foreach (string item in plantCover)
			{
				if (TowerDefenseManager.Instance.GetCharacterNum(item) > 0)
				{
					flag = true;
					break;
				}
			}
			return config.GetCoverCanDirectPlant() || flag;
		}
		if (!(config.characterConfig is TowerDefensePlantConfig { extendCoverDictionary: not null } towerDefensePlantConfig) || towerDefensePlantConfig.extendCoverDictionary.Count == 0)
		{
			return true;
		}
		if (_extendCoverNames == null)
		{
			_extendCoverNames = new HashSet<string>();
			foreach (string value in towerDefensePlantConfig.extendCoverDictionary.Values)
			{
				_extendCoverNames.Add(value);
			}
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(instance) || !GodotObject.IsInstanceValid(instance.characterRegistry))
		{
			return false;
		}
		List<TowerDefenseCharacter> cleanCharactersList = instance.characterRegistry.GetCleanCharactersList();
		for (int i = 0; i < cleanCharactersList.Count; i++)
		{
			TowerDefenseCharacter towerDefenseCharacter = cleanCharactersList[i];
			if (GodotObject.IsInstanceValid(towerDefenseCharacter) && !towerDefenseCharacter.characterFilter && towerDefenseCharacter.config != null && GodotObject.IsInstanceValid(towerDefenseCharacter.cell) && _extendCoverNames.Contains(towerDefenseCharacter.config.name) && towerDefenseCharacter.cell.CanPacketPlant(config))
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyCachedRuntimeAvailability()
	{
		if (start || _cachedUpgradePacket)
		{
			alive = _cachedRuntimeAvailability && (_cachedUpgradePacket || !coldDownOpen);
		}
	}

	private bool NeedsContinuousPhysicsProcess()
	{
		if (onlyDraw)
		{
			return false;
		}
		if (!_centralRuntimeStateRefresh)
		{
			return true;
		}
		if (_runtimeAvailabilityDirty || _runtimeCostDirty)
		{
			return true;
		}
		if (pressDelayTimer > 0.0 || coldDownOpen || aliveTime >= 0.0 || IsProphesied)
		{
			return true;
		}
		if (height >= 0.0 && GodotObject.IsInstanceValid(moveComponent))
		{
			return !moveComponent.IsQueuedForDeletion();
		}
		return false;
	}

	private void RefreshPhysicsProcessState()
	{
		if (IsInsideTree())
		{
			SetPhysicsProcess(NeedsContinuousPhysicsProcess());
		}
	}

	public void ColorSet()
	{
		Color modulate = ((alive && !openShadow && !@lock && !blink) ? Colors.White : Colors.DimGray);
		if (layout == null)
		{
			base.Modulate = modulate;
		}
		else
		{
			layout.Modulate = modulate;
		}
	}

	public void MobilePreset()
	{
		backgroundTexture.Size = new Vector2(96f, 60f);
		backgroundTexture.Position = -backgroundTexture.Size / 2f;
		previewClip.Position = new Vector2(6f, 6f);
		previewClip.Size = new Vector2(82f, 46f);
		itemCostLabel.Size = new Vector2(48f, 25f);
		itemCostLabel.Position = new Vector2(45f, 32f);
		itemCostLabel.TextureFilter = TextureFilterEnum.ParentNode;
		itemCostLabel.AddThemeColorOverride("font_color", Colors.White);
		itemCostLabel.AddThemeConstantOverride("outline_size", 5);
		itemCostLabel.AddThemeFontOverride("font", FZKT);
		itemCostLabel.AddThemeFontSizeOverride("font_size", 24);
		selectTexture.Size = new Vector2(312f, 198f);
		selectTexture.Position = new Vector2(-48f, -30f);
		button.Size = new Vector2(94f, 60f);
		button.Position = new Vector2(-48f, -30f);
		coldDownProgressBar.Size = new Vector2(95f, 59f);
		coldDownProgressBar.Position = new Vector2(-48f, -30f);
		coldDownProgressBar.TextureProgress = MOBILE_PROGRESS_TEXTURE;
		loveButton.Position = new Vector2(26f, -34f);
		ApplyContainerFootprint();
	}

	public void SetPcPreset()
	{
		backgroundTexture.Size = new Vector2(50f, 70f);
		backgroundTexture.Position = new Vector2(-24f, -32f);
		previewClip.Position = new Vector2(3f, 6f);
		previewClip.Size = new Vector2(44f, 47f);
		itemCostLabel.TextureFilter = TextureFilterEnum.Linear;
		itemCostLabel.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f));
		itemCostLabel.RemoveThemeConstantOverride("outline_size");
		itemCostLabel.AddThemeFontOverride("font", PC_FONT);
		itemCostLabel.AddThemeFontSizeOverride("font_size", 12);
		itemCostLabel.HorizontalAlignment = HorizontalAlignment.Center;
		itemCostLabel.VerticalAlignment = VerticalAlignment.Center;
		itemCostLabel.Position = new Vector2(2f, 52f);
		itemCostLabel.Size = new Vector2(35f, 17f);
		selectTexture.Size = new Vector2(170f, 237f);
		selectTexture.Position = new Vector2(-25f, -33f);
		button.Position = new Vector2(-24f, -32f);
		button.Size = new Vector2(50f, 70f);
		coldDownProgressBar.Position = new Vector2(-24f, -32f);
		coldDownProgressBar.Size = new Vector2(50f, 70f);
		if (pcProgressTexture != null)
		{
			coldDownProgressBar.TextureProgress = pcProgressTexture;
		}
		loveButton.Position = new Vector2(4f, -35f);
		ApplyContainerFootprint();
	}

	public void SetMobileMode(bool enabled)
	{
		if (!setMobileLayout && !setPcLayout && isMobile != enabled)
		{
			isMobile = enabled;
			if (isMobile)
			{
				MobilePreset();
			}
			else
			{
				SetPcPreset();
			}
			UpdateBackgroundTexture();
			UpdateSpriteLayout();
			RefreshPreview();
		}
	}

	public void RefreshPreview()
	{
		if (!CanContinueAsync(_asyncLifetimeVersion) || config == null)
		{
			return;
		}
		if (_previewCreationDeferred)
		{
			SetPreviewVisible(visible: false);
			return;
		}
		bool flag = false;
		if (!GodotObject.IsInstanceValid(sprite))
		{
			CreateSprite();
			flag = GodotObject.IsInstanceValid(sprite);
		}
		if (GodotObject.IsInstanceValid(sprite))
		{
			UpdateSpriteLayout();
			sprite.Visible = true;
			if (!flag)
			{
				FreezePreviewTree(sprite);
			}
		}
		SetPreviewVisible(HasPreparedPreview);
	}

	public void UpdateBackgroundTexture()
	{
		if (config == null || !GodotObject.IsInstanceValid(backgroundTexture))
		{
			return;
		}
		if (isMobile)
		{
			switch (config._GetType())
			{
			case TowerDefenseEnum.PACKET_TYPE.WHITE:
				backgroundTexture.Texture = PACKET_NORMAL_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.GOLD:
				backgroundTexture.Texture = PACKET_GOLD_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.DIAMOND:
				backgroundTexture.Texture = PACKET_DIAMOND_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.COLOUR:
				backgroundTexture.Texture = PACKET_COLOUR_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.STAR:
				backgroundTexture.Texture = PACKET_STAR_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.ORIGINAL:
				backgroundTexture.Texture = PACKET_NORMAL_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.ZOMBIE:
				backgroundTexture.Texture = PACKET_ZOMBIE_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.COVER:
				backgroundTexture.Texture = PACKET_COVER_MOBILE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.GRAY:
				backgroundTexture.Texture = PACKET_GRAY;
				break;
			}
		}
		else
		{
			switch (config._GetType())
			{
			case TowerDefenseEnum.PACKET_TYPE.WHITE:
				backgroundTexture.Texture = PACKET_NORMAL;
				break;
			case TowerDefenseEnum.PACKET_TYPE.GOLD:
				backgroundTexture.Texture = PACKET_GOLD;
				break;
			case TowerDefenseEnum.PACKET_TYPE.DIAMOND:
				backgroundTexture.Texture = PACKET_DIAMOND;
				break;
			case TowerDefenseEnum.PACKET_TYPE.COLOUR:
				backgroundTexture.Texture = PACKET_COLOUR;
				break;
			case TowerDefenseEnum.PACKET_TYPE.STAR:
				backgroundTexture.Texture = PACKET_STAR;
				break;
			case TowerDefenseEnum.PACKET_TYPE.ORIGINAL:
				backgroundTexture.Texture = PACKET_NORMAL;
				break;
			case TowerDefenseEnum.PACKET_TYPE.ZOMBIE:
				backgroundTexture.Texture = PACKET_ZOMBIE;
				break;
			case TowerDefenseEnum.PACKET_TYPE.COVER:
				backgroundTexture.Texture = PACKET_COVER;
				break;
			case TowerDefenseEnum.PACKET_TYPE.GRAY:
				backgroundTexture.Texture = PACKET_GRAY;
				break;
			}
		}
	}

	public void UpdateSpriteLayout()
	{
		if (config == null)
		{
			return;
		}
		TowerDefenseCharacterConfig characterConfig = config.characterConfig;
		if (GodotObject.IsInstanceValid(sprite) && isMobile)
		{
			if (characterConfig is TowerDefenseZombieConfig)
			{
				sprite.Position = config.packetAnimeOffset * 1.2f;
				sprite.Scale = config.packetAnimeScale * 1.25f;
			}
			else
			{
				sprite.Position = config.packetAnimeOffset;
				sprite.Scale = config.packetAnimeScale * 1.25f;
			}
		}
		else if (GodotObject.IsInstanceValid(sprite))
		{
			sprite.Position = config.packetAnimeOffset;
			sprite.Scale = config.packetAnimeScale;
		}
		if (GodotObject.IsInstanceValid(sprite) && config.packetFlip)
		{
			sprite.Scale = new Vector2(0f - sprite.Scale.X, sprite.Scale.Y);
		}
	}

	public void Clear()
	{
		_asyncLifetimeVersion++;
		_cardBehaviorHost.Release();
		_livePreviewGeneration++;
		_previewHovered = false;
		_runtimeAvailabilityDirty = true;
		_runtimeCostDirty = true;
		_cachedRuntimeAvailability = true;
		_cachedUpgradePacket = false;
		if (GodotObject.IsInstanceValid(config))
		{
			TryUnsubscribeCharacterSkinSwitch();
		}
		config = null;
		_extendCoverNames = null;
		if (GodotObject.IsInstanceValid(backgroundTexture))
		{
			backgroundTexture.Texture = PACKET_NORMAL;
		}
		if (GodotObject.IsInstanceValid(sprite))
		{
			sprite.ReleaseForcedCpuPoseData();
			sprite.ClearRenderClipControl();
			sprite.QueueFree();
			sprite = null;
		}
		baseItemCost = 0L;
		_itemCostLabelInitialized = false;
		itemCost = 0L;
		riseCost = -1;
		coldDown = 0.0;
		_previewCreationDeferred = false;
		SetPreviewVisible(visible: false);
	}

	public void ClearEventHandlers()
	{
		this.OnPressed = null;
		this.OnLoveChange = null;
	}

	public void ResetForPool()
	{
		SetScrollSelectionMode(null);
		if (GodotObject.IsInstanceValid(_cooldownLabel))
		{
			_cooldownLabel.Visible = false;
		}
		_cooldownText = null;
		XWModCards.Unbind(this);
		_modCooldownPrepared = false;
		_modCostQuoteReady = false;
		ApplyProphecy(0.0, Vector2.Zero);
		SetContainerFootprint(enabled: false);
		ClearEventHandlers();
		Clear();
		sprite = null;
		showLove = false;
		alive = true;
		@lock = false;
		select = false;
		onlyDraw = false;
		plantOnce = false;
		useCost = true;
		enforceRuntimeAvailabilityOnPress = true;
		allowPressWhenUnavailable = false;
		start = false;
		coldDownOpen = false;
		coldDownTimer = 0.0;
		pressDelayTimer = 0.0;
		aliveTime = -1.0;
		aliveTimer = 0.0;
		blinkTimer = 0.0;
		blink = false;
		height = -1.0;
		costMultiple = -1.0;
		originalSaveKey = "";
		_sunAccountId = default(EconomyAccountId);
		_centralRuntimeStateRefresh = false;
		_runtimeAvailabilityDirty = true;
		_runtimeCostDirty = true;
		if (IsInsideTree())
		{
			SetPhysicsProcess(enable: false);
		}
	}

	public void Init(TowerDefensePacketConfig _config, bool skipGlobalChangeCost = false)
	{
		CancelScrollSelection();
		XWModCards.Unbind(this);
		_modCooldownPrepared = false;
		_modCostQuoteReady = false;
		long startTicks = TowerDefensePerfProfiler.BeginHotPath();
		_asyncLifetimeVersion++;
		config = _config;
		long startTicks2 = TowerDefensePerfProfiler.BeginHotPath();
		_cardBehaviorHost.Bind(this, config);
		TowerDefensePerfProfiler.End("card.create.behavior", startTicks2);
		_extendCoverNames = null;
		if (originalSaveKey == "")
		{
			originalSaveKey = config.saveKey;
		}
		long startTicks3 = TowerDefensePerfProfiler.BeginHotPath();
		baseItemCost = config.GetCost(skipGlobalChangeCost);
		itemCost = baseItemCost;
		riseCost = config.GetCostRise();
		costMultiple = config.GetCostMultiple();
		InvalidateRuntimeState(includeCost: true);
		TowerDefensePerfProfiler.End("card.create.economy", startTicks3);
		long startTicks4 = TowerDefensePerfProfiler.BeginHotPath();
		if (!_previewCreationDeferred)
		{
			CreateSprite();
		}
		TowerDefensePerfProfiler.End("card.create.preview", startTicks4);
		if (!_previewCreationDeferred)
		{
			TrySubscribeCharacterSkinSwitch();
		}
		long startTicks5 = TowerDefensePerfProfiler.BeginHotPath();
		coldDown = config.GetPacketCooldown();
		ApplyInitializedUiState();
		if (!_previewCreationDeferred)
		{
			OnVisibilityChanged();
		}
		TowerDefensePerfProfiler.End("card.create.ui", startTicks5);
		TowerDefensePerfProfiler.End("card.create.total", startTicks);
	}

	public void InitVisualPreview(TowerDefensePacketConfig previewConfig, int previewCost, int previewCostRise, double previewCostMultiple, double previewCooldown)
	{
		_asyncLifetimeVersion++;
		config = previewConfig;
		if (GodotObject.IsInstanceValid(config))
		{
			if (originalSaveKey == "")
			{
				originalSaveKey = config.saveKey;
			}
			baseItemCost = previewCost;
			itemCost = previewCost;
			riseCost = previewCostRise;
			costMultiple = previewCostMultiple;
			InvalidateRuntimeState();
			CreateSprite();
			TrySubscribeCharacterSkinSwitch();
			coldDown = Math.Max(0.0, previewCooldown);
			ApplyInitializedUiState();
			OnVisibilityChanged();
		}
	}

	private void ApplyInitializedUiState()
	{
		UpdateBackgroundTexture();
		if (GodotObject.IsInstanceValid(coldDownProgressBar))
		{
			UpdateItemCostLabel(force: true);
			coldDownProgressBar.MaxValue = coldDown;
			coldDownProgressBar.Value = coldDownProgressBar.MaxValue;
		}
	}

	public void CreateSprite()
	{
		if (config == null)
		{
			return;
		}
		TowerDefenseCharacterConfig characterConfig = config.characterConfig;
		if (!GodotObject.IsInstanceValid(characterConfig))
		{
			return;
		}
		long startTicks = TowerDefensePerfProfiler.BeginHotPath();
		if (GodotObject.IsInstanceValid(sprite))
		{
			sprite.ReleaseForcedCpuPoseData();
			sprite.ClearRenderClipControl();
			sprite.QueueFree();
			sprite = null;
		}
		if (!HasPreviewControls())
		{
			TowerDefensePerfProfiler.End("card.preview.total", startTicks);
			return;
		}
		long startTicks2 = TowerDefensePerfProfiler.BeginHotPath();
		sprite = TowerDefenseManager.GetPacketSprite(config);
		TowerDefensePerfProfiler.End("card.preview.instantiate", startTicks2);
		if (!GodotObject.IsInstanceValid(sprite))
		{
			TowerDefensePerfProfiler.End("card.preview.total", startTicks);
			return;
		}
		sprite.forceCpuPoseRender = true;
		ConfigurePreviewTreeBeforeAttach(sprite);
		long startTicks3 = TowerDefensePerfProfiler.BeginHotPath();
		previewSpriteNode.AddChild(sprite, forceReadableName: false, InternalMode.Disabled);
		TowerDefensePerfProfiler.End("card.preview.attach", startTicks3);
		bool flag = false;
		if (config.GetHypnoses())
		{
			sprite.Modulate = new Color(0.72f, 0.62f, 1f, sprite.Modulate.A);
		}
		UpdateSpriteLayout();
		long startTicks4 = TowerDefensePerfProfiler.BeginHotPath();
		if (characterConfig.armorData != null && config.initArmor != null && config.initArmor.Count > 0)
		{
			foreach (string item in config.initArmor)
			{
				ArmorSlotConfig slotConfig = characterConfig.armorData.GetSlotConfig(item);
				TowerDefenseArmorTypeData typeData = characterConfig.armorData.GetTypeData(item);
				if (typeData == null)
				{
					continue;
				}
				string replaceMethod = slotConfig.replaceMethod;
				if (!(replaceMethod == "Media"))
				{
					if (replaceMethod == "Sprite")
					{
						CharacterArmorData.CreateArmorPartNode(sprite, slotConfig, typeData);
						flag = true;
					}
				}
				else
				{
					characterConfig.armorData.OpenArmorFliters(sprite, item);
					characterConfig.armorData.SetArmorReplace(sprite, item, 0);
					flag = true;
				}
			}
		}
		if (characterConfig.customData != null && TryGetPacketSaveValue(config.saveKey, out var packetData))
		{
			string text = packetData.GetValueOrDefault("Key", new Dictionary()).AsGodotDictionary().GetValueOrDefault("Custom", "")
				.AsString();
			if (text != "")
			{
				characterConfig.customData.SetCustomFliters(sprite, text);
				flag = true;
			}
		}
		TowerDefensePerfProfiler.End("card.preview.customize", startTicks4);
		long startTicks5 = TowerDefensePerfProfiler.BeginHotPath();
		if (!string.IsNullOrEmpty(config.packetAnimeClip) && config.packetAnimeClip.IndexOf('&') < 0 && sprite.clip == config.packetAnimeClip && sprite.HasClip(config.packetAnimeClip))
		{
			sprite.loop = true;
			sprite.clipOver = false;
			sprite.frameIndex = sprite.clipRange.X;
			sprite.elapsedTimer = 0.0;
			TowerDefensePerfProfiler.SampleHotPath("card.preview.reuseAuthoredClip", 1);
		}
		else
		{
			sprite.SetAnimation(config.packetAnimeClip);
			TowerDefensePerfProfiler.SampleHotPath("card.preview.selectPacketClip", 1);
		}
		TowerDefensePerfProfiler.End("card.preview.animation", startTicks5);
		long startTicks6 = TowerDefensePerfProfiler.BeginHotPath();
		FreezePreparedPreviewTree(sprite, flag, !flag);
		sprite.QueueRedraw();
		TowerDefensePerfProfiler.End("card.preview.freeze", startTicks6);
		long startTicks7 = TowerDefensePerfProfiler.BeginHotPath();
		AdobeAnimateRenderManager.WarmupRenderMount(sprite);
		TowerDefensePerfProfiler.End("card.preview.renderMount", startTicks7);
		ColorSet();
		TowerDefensePerfProfiler.End("card.preview.total", startTicks);
	}

	private void OnCharacterSkinSwitched(string packetSaveKey, string customKey)
	{
		if (!CanContinueAsync(_asyncLifetimeVersion) || !GodotObject.IsInstanceValid(config) || config.saveKey != packetSaveKey)
		{
			return;
		}
		TowerDefenseCharacterConfig characterConfig = config.characterConfig;
		if (GodotObject.IsInstanceValid(characterConfig) && characterConfig.customData != null && GodotObject.IsInstanceValid(sprite) && (!(customKey != "") || characterConfig.customData.customDictionary.ContainsKey(customKey)))
		{
			characterConfig.customData.ClearCustomFliters(sprite);
			if (customKey != "")
			{
				characterConfig.customData.SetCustomFliters(sprite, customKey);
			}
			sprite.UpdateMediaReplaceData();
			sprite.UpdateChild();
			sprite.RefreshManagedSlotSpriteCacheForRender();
			FreezePreviewTree(sprite, forcePoseRefresh: true);
		}
	}

	public void OnVisibilityChanged()
	{
		if (!IsVisibleInTree())
		{
			CancelScrollSelection();
		}
		RefreshCooldownLabel();
		_queuedVisibilityRefreshVersion = _asyncLifetimeVersion;
		if (CanContinueAsync(_queuedVisibilityRefreshVersion) && !_visibilityRefreshQueued)
		{
			_visibilityRefreshQueued = true;
			CallDeferred("ApplyDeferredVisibilityState");
		}
	}

	public void ApplyDeferredVisibilityState()
	{
		_visibilityRefreshQueued = false;
		int queuedVisibilityRefreshVersion = _queuedVisibilityRefreshVersion;
		if (!CanContinueAsync(queuedVisibilityRefreshVersion))
		{
			return;
		}
		if (_previewCreationDeferred)
		{
			SetPreviewVisible(visible: false);
		}
		else if (!IsVisibleInTree())
		{
			if (GodotObject.IsInstanceValid(sprite))
			{
				FreezePreviewTree(sprite);
			}
			SetPreviewVisible(visible: false);
		}
		else if (!_previewHovered)
		{
			if (!GodotObject.IsInstanceValid(sprite))
			{
				CreateSprite();
			}
			if (GodotObject.IsInstanceValid(sprite))
			{
				sprite.Visible = true;
				FreezePreviewTree(sprite, forcePoseRefresh: false, forceRenderResubmit: true);
			}
			SetPreviewVisible(HasPreparedPreview);
		}
	}

	public override void _EnterTree()
	{
		_exitedTree = false;
		_asyncLifetimeVersion++;
		base._EnterTree();
		SetProcessInput(enable: false);
		SubscribeCooldownConfig();
		if (!setMobileLayout && !setPcLayout && BattleEventBus.Instance != null)
		{
			BattleEventBus.Instance.OnUiSwitched += SetMobileMode;
		}
	}

	public override void _ExitTree()
	{
		SetScrollSelectionMode(null);
		if (GodotObject.IsInstanceValid(_cooldownConfigOwner))
		{
			_cooldownConfigOwner.ConfigValueChanged -= OnCooldownConfigChanged;
		}
		_cooldownConfigOwner = null;
		XWModCards.Unbind(this);
		_modCooldownPrepared = false;
		_modCostQuoteReady = false;
		_exitedTree = true;
		_asyncLifetimeVersion++;
		_livePreviewGeneration++;
		_previewHovered = false;
		if (BattleEventBus.Instance != null)
		{
			BattleEventBus.Instance.OnUiSwitched -= SetMobileMode;
		}
		TryUnsubscribeCharacterSkinSwitch();
		_visibilityRefreshQueued = false;
		base._ExitTree();
	}

	public override void _Ready()
	{
		previewClip = GetNodeOrNull<Control>("%PreviewClip");
		previewSpriteNode = GetNodeOrNull<Control>("%PreviewSpriteNode");
		backgroundTexture = GetNodeOrNull<TextureRect>("%BackgroundTexture");
		selectTexture = GetNodeOrNull<NinePatchRect>("%SelectTexture");
		layout = GetNodeOrNull<Control>("%Layout");
		body = GetNodeOrNull<Control>("%Body");
		itemCostLabel = GetNodeOrNull<Label>("%ItemCostLabel");
		button = GetNodeOrNull<Button>("%Button");
		coldDownProgressBar = GetNodeOrNull<TextureProgressBar>("%ColdDownProgressBar");
		_cooldownLabel = GetNodeOrNull<Label>("%CooldownLabel");
		if (GodotObject.IsInstanceValid(_cooldownLabel))
		{
			_cooldownLabel.AddThemeFontOverride("font", FZKT);
		}
		loveButton = GetNodeOrNull<TextureButton>("%LoveButton");
		moveComponent = GetNodeOrNull<MoveComponent>("%MoveComponent");
		savePos = base.GlobalPosition;
		button.ActionMode = BaseButton.ActionModeEnum.Press;
		base.VisibilityChanged += OnVisibilityChanged;
		button.MouseEntered += OnMouseEntered;
		button.MouseExited += OnMouseExited;
		button.Pressed += OnCardButtonPressed;
		button.GuiInput += OnCardButtonGuiInput;
		loveButton.Toggled += LoveButtonToggled;
		pcProgressTexture = coldDownProgressBar.TextureProgress;
		isMobile = setMobileLayout || (!setPcLayout && TryGetMobilePresetConfig() && SceneManager.CurrentScene != "LevelEditorStage");
		if (isMobile)
		{
			MobilePreset();
		}
		else
		{
			ApplyContainerFootprint();
		}
		coldDownProgressBar.Visible = false;
		ApplyInitializedUiState();
		if (config != null)
		{
			RefreshPreview();
		}
		RefreshPhysicsProcessState();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (onlyDraw)
		{
			return;
		}
		ProcessProphecy(delta);
		if (pressDelayTimer > 0.0)
		{
			pressDelayTimer -= delta;
		}
		if (GodotObject.IsInstanceValid(moveComponent) && height != -1.0 && (double)base.GlobalPosition.Y > (double)savePos.Y + height && moveComponent.velocity.Y >= 0f)
		{
			moveComponent.QueueFree();
		}
		if (aliveTime != -1.0)
		{
			if (!(aliveTimer < aliveTime))
			{
				if (!select)
				{
					GrantExpireSun();
					if (Global.IsMultiplayerMode && HasMeta("packet_sync_id"))
					{
						int syncId = (int)GetMeta("packet_sync_id");
						MultiPlayerManager.Instance.SendPacketPick(syncId);
					}
					QueueFree();
				}
				return;
			}
			aliveTimer += delta;
			if (aliveTimer > aliveTime - 5.0)
			{
				if (blinkTimer < 0.25)
				{
					blinkTimer += delta;
				}
				else
				{
					if (!select)
					{
						blink = !blink;
						ColorSet();
					}
					blinkTimer = 0.0;
				}
			}
		}
		bool flag = HasMeta("is_upgrade_packet");
		if (flag != _cachedUpgradePacket)
		{
			_runtimeAvailabilityDirty = true;
		}
		if (start || flag)
		{
			if (!flag && coldDownOpen)
			{
				if (!TowerDefenseManager.Instance.pausePacket)
				{
					if (!TowerDefenseManager.Instance.backPacket)
					{
						coldDownTimer = Math.Max(0.0, coldDownTimer - delta);
						if (coldDownTimer <= 0.0)
						{
							coldDownOpen = false;
							XWModCards.Notify(this, "CooldownCompleted");
						}
					}
					else if (coldDownTimer < coldDown)
					{
						coldDownTimer += delta;
					}
				}
				if (coldDownOpen && GodotObject.IsInstanceValid(coldDownProgressBar))
				{
					if (!coldDownProgressBar.Visible)
					{
						coldDownProgressBar.Visible = true;
					}
					coldDownProgressBar.Value = coldDownTimer;
				}
			}
			if (!_centralRuntimeStateRefresh)
			{
				_runtimeAvailabilityDirty = true;
				if (Engine.GetPhysicsFrames() % 10 == 0L && !flag)
				{
					_runtimeCostDirty = true;
				}
			}
			if (_runtimeAvailabilityDirty || _runtimeCostDirty)
			{
				RefreshRuntimeState();
			}
		}
		RefreshCooldownLabel();
		RefreshPhysicsProcessState();
	}

	private void GrantExpireSun()
	{
		TowerDefensePacketConfig towerDefensePacketConfig = config;
		if (!GodotObject.IsInstanceValid(towerDefensePacketConfig))
		{
			return;
		}
		int expireSun = towerDefensePacketConfig.expireSun;
		if (expireSun <= 0 || !TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (GodotObject.IsInstanceValid(instance))
		{
			bool hypnoses = towerDefensePacketConfig.GetHypnoses();
			Vector2 velocity = new Vector2((float)GD.RandRange(-50.0, 50.0), -400f);
			TowerDefenseSunBase towerDefenseSunBase = ((!HasSunAccount) ? (hypnoses ? instance.BrainSunCreate(base.GlobalPosition, expireSun, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, 0.0, velocity) : instance.SunCreate(base.GlobalPosition, expireSun, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, 0.0, velocity)) : (hypnoses ? instance.BrainSunCreate(_sunAccountId, base.GlobalPosition, expireSun, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, 0.0, velocity) : instance.SunCreate(_sunAccountId, base.GlobalPosition, expireSun, TowerDefenseEnum.SUN_MOVING_METHOD.GRAVITY, 0.0, velocity)));
			if (GodotObject.IsInstanceValid(towerDefenseSunBase))
			{
				towerDefenseSunBase.RestoreAutoCollect(savedAutoCollect: true);
			}
		}
	}

	public void Pressed()
	{
		if (onlyDraw || config == null || @lock)
		{
			return;
		}
		if (enforceRuntimeAvailabilityOnPress && !CanAffordItemCost())
		{
			alive = false;
		}
		else if ((alive || allowPressWhenUnavailable) && !(pressDelayTimer > 0.0))
		{
			pressDelayTimer = 0.2;
			AudioManager.Instance.AudioPlay("PacketPick");
			select = !select;
			if (!canPressPutBack)
			{
				button.MouseFilter = MouseFilterEnum.Ignore;
			}
			if (select)
			{
				ModSelectionConsumed = false;
				_cardBehaviorHost.NotifyPressed();
			}
			this.OnPressed?.Invoke(this);
		}
	}

	public void OnMouseEntered()
	{
		if (config != null && (!allowPressWhenUnavailable || alive))
		{
			if (_previewCreationDeferred)
			{
				SetPreviewCreationDeferred(deferred: false);
			}
			_previewHovered = true;
			_livePreviewGeneration++;
			if (!GodotObject.IsInstanceValid(sprite))
			{
				CreateSprite();
			}
			if (!GodotObject.IsInstanceValid(sprite))
			{
				SetPreviewVisible(visible: false);
				return;
			}
			sprite.SetAnimation(config.packetAnimeClip);
			sprite.Visible = true;
			PlayPreviewTreeFromStart(sprite);
			sprite.QueueRedraw();
			AdobeAnimateRenderManager.WarmupRenderMount(sprite);
			SetPreviewVisible(visible: true);
		}
	}

	public void OnMouseExited()
	{
		_previewHovered = false;
		_livePreviewGeneration++;
		if (config != null)
		{
			if (GodotObject.IsInstanceValid(sprite))
			{
				FreezePreviewTree(sprite);
			}
			SetPreviewVisible(GodotObject.IsInstanceValid(sprite));
		}
	}

	public void Reset()
	{
		if (GodotObject.IsInstanceValid(this) && !onlyDraw && config != null)
		{
			_previewHovered = false;
			_livePreviewGeneration++;
			select = false;
			pressDelayTimer = 0.0;
			if (GodotObject.IsInstanceValid(sprite))
			{
				sprite.Visible = true;
				FreezePreviewTree(sprite);
			}
			if (!canPressPutBack)
			{
				button.MouseFilter = MouseFilterEnum.Pass;
			}
			SetPreviewVisible(HasPreparedPreview);
		}
	}

	internal XWModCardState GetModCardState()
	{
		long cost;
		bool flag = TryResolveModItemCost(out cost);
		return new XWModCardState(XWModCards.GetHandle(this), config?.saveKey ?? "", cost, coldDownTimer, coldDown, coldDownOpen, flag && CanUseFromMod(), select);
	}

	internal bool CanUseFromMod()
	{
		if (!onlyDraw && start && alive && !@lock && !coldDownOpen && !HasMeta("packet_pending_plant") && (!HasSunAccount || SunAccountId.IsLocal) && GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			return CanAffordItemCost();
		}
		return false;
	}

	private bool UsesCooldown()
	{
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (GodotObject.IsInstanceValid(instance))
		{
			CommandManager instance2 = CommandManager.Instance;
			if (instance2 == null || !instance2.debugPacketColdDown)
			{
				if (instance.currentControl?.levelConfig is TowerDefenseLevelConfig towerDefenseLevelConfig)
				{
					return towerDefenseLevelConfig.packetColdDownUse;
				}
				return (instance.GetSeedBankFeature()?.config)?.packetColdDownUse ?? false;
			}
		}
		return false;
	}

	private double ResolveModCooldown(double seconds, bool initial)
	{
		if (onlyDraw || (HasSunAccount && !SunAccountId.IsLocal) || !XWModExtensions.HasHandlers(XWModCardPoints.Cooldown.Id))
		{
			return seconds;
		}
		return XWModExtensions.Modify(XWModCardPoints.Cooldown, new XWModCardCooldown(XWModCards.GetHandle(this), config?.saveKey ?? "", seconds, initial), XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Owner, initial ? "Initial" : "Use")).Seconds;
	}

	public void SetModObservableCooldown(double seconds)
	{
		if (double.IsFinite(seconds))
		{
			bool flag = coldDownOpen;
			double num = Math.Max(0.0, seconds);
			if (coldDownTimer != num || coldDownOpen != num > 0.0)
			{
				coldDownTimer = num;
				coldDownOpen = coldDownTimer > 0.0;
				XWModCards.Notify(this, (!coldDownOpen) ? "CooldownCompleted" : (flag ? "CooldownAdjusted" : "CooldownStarted"));
			}
		}
	}

	internal void NotifyModUseCancelled()
	{
		_modCooldownPrepared = false;
		XWModCards.Notify(this, "Cancelled");
	}

	internal void NotifyModRestored()
	{
		_modCooldownPrepared = false;
		_modCostQuoteReady = false;
		XWModCards.Notify(this, "Restored", XWModEventOrigin.SaveRestore);
		if (!onlyDraw)
		{
			XWModPresentation.MountCard(this, config?.saveKey ?? "");
		}
		OnVisibilityChanged();
	}

	public void StartInit()
	{
		StartInitCore(restoring: false);
	}

	internal void StartInitForRestore()
	{
		StartInitCore(restoring: true);
	}

	private void StartInitCore(bool restoring)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (!restoring)
			{
				XWModCards.Notify(this, "Bound", XWModEventOrigin.InitialBinding);
				if (!onlyDraw)
				{
					XWModPresentation.MountCard(this, config?.saveKey ?? "");
				}
			}
			alive = false;
			if (!restoring && !plantOnce && !CommandManager.Instance.debugPacketColdDown)
			{
				TowerDefenseLevelBaseConfig obj = TowerDefenseManager.Instance.currentControl?.levelConfig;
				bool flag = false;
				bool flag2 = false;
				if (obj is TowerDefenseLevelConfig towerDefenseLevelConfig)
				{
					flag = towerDefenseLevelConfig.packetColdDownStart;
					flag2 = towerDefenseLevelConfig.packetColdDownUse;
				}
				else
				{
					TowerDefenseLevelSeedBankConfig towerDefenseLevelSeedBankConfig = TowerDefenseManager.Instance.GetSeedBankFeature()?.config;
					if (towerDefenseLevelSeedBankConfig != null)
					{
						flag = towerDefenseLevelSeedBankConfig.packetColdDownStart;
						flag2 = towerDefenseLevelSeedBankConfig.packetColdDownUse;
					}
				}
				if (flag && flag2)
				{
					double num = ResolveModCooldown(config.GetStartingCooldown(), initial: true);
					if (num > 0.0)
					{
						coldDownOpen = true;
						coldDownTimer = num;
						XWModCards.Notify(this, "CooldownStarted", XWModEventOrigin.InitialBinding);
					}
				}
			}
			OnVisibilityChanged();
		}
	}

	private void RestoreMagicBeanRestoreState(TowerDefenseCharacter character)
	{
		TowerDefenseCharacterSaveConfigCSharp restoreConfig;
		if (GodotObject.IsInstanceValid(character) && HasMeta("MagicBeanRestore"))
		{
			GodotObject godotObject = GetMeta("MagicBeanRestore").AsGodotObject();
			restoreConfig = godotObject as TowerDefenseCharacterSaveConfigCSharp;
			if (restoreConfig != null)
			{
				character.PrepareForProgressRestore();
				character.Ready += RestoreOnce;
			}
		}
		void RestoreOnce()
		{
			character.Ready -= RestoreOnce;
			TowerDefensePlantMagicBean.ApplySavedState(character, restoreConfig);
		}
	}

	public TowerDefenseCharacter Plant(Vector2I gridPos, bool useSun = true, bool executeEvent = true)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (!alive)
			{
				return null;
			}
			if (XWModPlantPlacementService.IsRegistered(config))
			{
				return PlantFootprint(gridPos, useSun, executeEvent);
			}
			try
			{
				if (!ModAllowsPlacement(gridPos))
				{
					return null;
				}
			}
			catch (XWModHookCallbackException)
			{
				return null;
			}
			TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(gridPos);
			if (GodotObject.IsInstanceValid(mapCell) && mapCell.NotifyBlockedPlanting(config))
			{
				return null;
			}
			bool flag = Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage";
			SunSpendReceipt receipt = null;
			if (!flag && !TryBeginItemCost(useSun, out receipt))
			{
				return null;
			}
			TowerDefenseCharacter towerDefenseCharacter = (HasSunAccount ? config.Plant(_sunAccountId, gridPos) : config.Plant(gridPos));
			if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				receipt?.TryRollback();
				return null;
			}
			RestoreMagicBeanRestoreState(towerDefenseCharacter);
			if (Global.IsMultiplayerMode && HasMeta("packet_sync_id"))
			{
				SetMeta("packet_planted", true);
			}
			if (!flag)
			{
				CompleteUse(receipt);
				XWModCards.Notify(this, "PlantCommitted");
				BattleEventBus.Instance?.EmitPlantManuallyPlanted(towerDefenseCharacter);
			}
			if (executeEvent)
			{
				_cardBehaviorHost.NotifyUseSucceeded(towerDefenseCharacter);
			}
			return towerDefenseCharacter;
		}
	}

	private TowerDefenseCharacter PlantFootprint(Vector2I click, bool useSun, bool executeEvent)
	{
		if (_footprintPlacementPending)
		{
			return null;
		}
		_footprintPlacementPending = true;
		SunSpendReceipt receipt = null;
		SunSpendReceipt receipt2 = null;
		TowerDefenseCharacter towerDefenseCharacter = null;
		bool flag = false;
		try
		{
			if (!XWModPlantPlacementService.TryPrepare(config, click, out var plan))
			{
				return null;
			}
			using (plan)
			{
				foreach (Vector2I targetCell in plan.TargetCells)
				{
					if (!ModAllowsPlacement(targetCell))
					{
						return null;
					}
				}
			}
			bool flag2 = Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage";
			if (!XWModPlantPlacementService.TryPrepare(config, click, out var plan2))
			{
				return null;
			}
			using (plan2)
			{
				if (!plan2.Reserve())
				{
					return null;
				}
				if (!flag2 && !TryBeginItemCost(useSun, out receipt))
				{
					return null;
				}
				TowerDefenseManager instance = TowerDefenseManager.Instance;
				EconomyAccountId economyAccountId = (HasSunAccount ? _sunAccountId : instance.GetLocalSunAccountId());
				towerDefenseCharacter = config.PlantPreparedFootprint(plan2, economyAccountId);
				if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
				{
					return null;
				}
				if (!flag2 && useSun && useCost && plan2.RefundAsSunDrops)
				{
					if (!plan2.TryPrepareRefundDrops(economyAccountId))
					{
						return null;
					}
				}
				else if (!flag2 && useSun && useCost && plan2.RefundTotal > 0 && !instance.TryBeginSunSpend(economyAccountId, -plan2.RefundTotal, out receipt2, SunTransactionReason.Refund))
				{
					return null;
				}
				if ((receipt != null && !receipt.IsActive) || (receipt2 != null && !receipt2.IsActive))
				{
					return null;
				}
				if (!plan2.IsPendingValid)
				{
					return null;
				}
				if (receipt != null && !receipt.TryCommit())
				{
					return null;
				}
				if (receipt2 != null && !receipt2.TryCommit())
				{
					return null;
				}
				flag = true;
				plan2.Commit();
				if (GodotObject.IsInstanceValid(TowerDefenseInGameLevelControl.instance))
				{
					TowerDefenseInGameLevelControl.instance.hasSpawn = true;
				}
				if (!flag2)
				{
					CompleteUse(null);
					XWModCards.Notify(this, "PlantCommitted");
				}
				if (executeEvent)
				{
					_cardBehaviorHost.NotifyUseSucceeded(towerDefenseCharacter);
				}
				return towerDefenseCharacter;
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("Wide plant placement failed: " + ex.Message);
			return flag ? towerDefenseCharacter : null;
		}
		finally
		{
			if (!flag)
			{
				receipt2?.TryRollback();
				receipt?.TryRollback();
			}
			_footprintPlacementPending = false;
		}
	}

	internal List<TowerDefenseCharacter> PlantFootprintColumn(int column)
	{
		using (XWModExtensions.EnterOperation())
		{
			List<TowerDefenseCharacter> list = new List<TowerDefenseCharacter>();
			if (!alive || _footprintPlacementPending)
			{
				return list;
			}
			_footprintPlacementPending = true;
			SunSpendReceipt receipt = null;
			SunSpendReceipt receipt2 = null;
			bool flag = false;
			try
			{
				if (!XWModPlantPlacementService.TryPrepareColumn(config, column, out var batch))
				{
					return list;
				}
				HashSet<int> hashSet = new HashSet<int>();
				using (batch)
				{
					foreach (XWModPlantFootprintPlan plan in batch.Plans)
					{
						bool flag2 = true;
						foreach (Vector2I targetCell in plan.TargetCells)
						{
							if (!ModAllowsPlacement(targetCell))
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							hashSet.Add(plan.Anchor.Y);
						}
					}
				}
				if (hashSet.Count == 0)
				{
					goto IL_012c;
				}
				TowerDefensePacketConfig packet = config;
				ISet<int> allowedRows = hashSet;
				if (!XWModPlantPlacementService.TryPrepareColumn(packet, column, out var batch2, null, allowedRows))
				{
					goto IL_012c;
				}
				using (batch2)
				{
					if (batch2.Plans.Count != hashSet.Count || !batch2.Reserve())
					{
						return list;
					}
					bool flag3 = Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage";
					if (!flag3 && !TryBeginItemCost(useSun: true, out receipt))
					{
						return list;
					}
					TowerDefenseManager instance = TowerDefenseManager.Instance;
					EconomyAccountId economyAccountId = (HasSunAccount ? _sunAccountId : instance.GetLocalSunAccountId());
					foreach (XWModPlantFootprintPlan plan2 in batch2.Plans)
					{
						TowerDefenseCharacter towerDefenseCharacter = config.PlantPreparedFootprint(plan2, economyAccountId);
						if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
						{
							return list;
						}
						list.Add(towerDefenseCharacter);
					}
					if (!flag3 && useCost && batch2.RefundAsSunDrops)
					{
						if (!batch2.TryPrepareRefundDrops(economyAccountId))
						{
							return list;
						}
					}
					else if (!flag3 && useCost && batch2.RefundTotal > 0 && !instance.TryBeginSunSpend(economyAccountId, -batch2.RefundTotal, out receipt2, SunTransactionReason.Refund))
					{
						return list;
					}
					if ((receipt != null && !receipt.IsActive) || (receipt2 != null && !receipt2.IsActive) || !batch2.IsPendingValid)
					{
						return list;
					}
					if (receipt != null && !receipt.TryCommit())
					{
						return list;
					}
					flag = true;
					batch2.Commit();
					if (receipt2 != null && !receipt2.TryCommit())
					{
						GD.PushWarning("Wide plant column purchase committed, but the legacy refund receipt expired during a callback.");
					}
					if (GodotObject.IsInstanceValid(TowerDefenseInGameLevelControl.instance))
					{
						TowerDefenseInGameLevelControl.instance.hasSpawn = true;
					}
					if (!flag3)
					{
						CompleteUse(null);
						XWModCards.Notify(this, "PlantCommitted");
					}
					_cardBehaviorHost.NotifyUseSucceeded(list[list.Count - 1]);
				}
				goto end_IL_003c;
				IL_012c:
				return list;
				end_IL_003c:;
			}
			catch (Exception ex)
			{
				GD.PushWarning("Wide plant column placement failed: " + ex.Message);
			}
			finally
			{
				if (!flag)
				{
					receipt2?.TryRollback();
					receipt?.TryRollback();
					list.Clear();
				}
				_footprintPlacementPending = false;
			}
			return list;
		}
	}

	public List<TowerDefenseCharacter> PlantColumnBatch(IReadOnlyList<Vector2I> gridPositions)
	{
		using (XWModExtensions.EnterOperation())
		{
			List<ColumnPlacement> list = new List<ColumnPlacement>();
			if (gridPositions != null)
			{
				foreach (Vector2I gridPosition in gridPositions)
				{
					list.Add(new ColumnPlacement(gridPosition, null, "grid"));
				}
			}
			return PlantColumnTargets(list);
		}
	}

	internal List<TowerDefenseCharacter> PlantColumnTargets(IReadOnlyList<ColumnPlacement> placements, Action<ColumnPlacement, TowerDefenseCharacter> onCreated = null)
	{
		using (XWModExtensions.EnterOperation())
		{
			List<TowerDefenseCharacter> list = new List<TowerDefenseCharacter>();
			if (!alive || placements == null || placements.Count == 0)
			{
				return list;
			}
			XWModHookDispatcher obj = XWModRuntimeSession.Current?.Hooks;
			if (obj != null && obj.HasHandlers(XWModHookPoints.PlantPlacement.Id))
			{
				List<ColumnPlacement> list2 = new List<ColumnPlacement>(placements.Count);
				try
				{
					foreach (ColumnPlacement placement in placements)
					{
						if (!(placement.Kind != "grid") || GodotObject.IsInstanceValid(placement.Target))
						{
							Vector2I cell = ((placement.Kind == "grid") ? placement.GridPos : placement.Target.gridPos);
							if (ModAllowsPlacement(cell, placement.Target))
							{
								list2.Add(placement);
							}
						}
					}
				}
				catch (XWModHookCallbackException)
				{
					return list;
				}
				if (list2.Count == 0)
				{
					return list;
				}
				placements = list2;
			}
			bool flag = Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage";
			SunSpendReceipt receipt = null;
			if (!flag && !TryBeginItemCost(useSun: true, out receipt))
			{
				return list;
			}
			bool hypnoses = config.GetHypnoses();
			ColumnPlantGridBudget columnPlantGridBudget = new ColumnPlantGridBudget(config);
			HashSet<Vector2I> hashSet = new HashSet<Vector2I>();
			HashSet<TowerDefenseCharacter> hashSet2 = new HashSet<TowerDefenseCharacter>();
			foreach (ColumnPlacement placement2 in placements)
			{
				TowerDefenseCharacter target = placement2.Target;
				if (placement2.Kind != "grid" && (!GodotObject.IsInstanceValid(target) || target.isDestroy || target.die || target.nearDie || !GodotObject.IsInstanceValid(target.instance) || target.instance.invincible || !target.instance.canBeCollection || (placement2.Kind == "plant" && (!(target is TowerDefensePlant) || target.instance.hologram))))
				{
					continue;
				}
				Vector2I vector2I = ((placement2.Kind == "grid") ? placement2.GridPos : target.gridPos);
				TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(vector2I);
				if (!GodotObject.IsInstanceValid(mapCell) || mapCell.NotifyBlockedPlanting(config) || (placement2.Kind == "grid" && (!columnPlantGridBudget.CanPlace(vector2I) || !hashSet.Add(vector2I))) || (placement2.Kind != "grid" && !hashSet2.Add(target)))
				{
					continue;
				}
				if (placement2.Kind == "jala_vase")
				{
					if (target is TowerDefensePlantJalaVase towerDefensePlantJalaVase && towerDefensePlantJalaVase.TryAddJala(config))
					{
						list.Add(towerDefensePlantJalaVase);
						onCreated?.Invoke(placement2, towerDefensePlantJalaVase);
					}
					continue;
				}
				TowerDefenseCharacter towerDefenseCharacter;
				switch (placement2.Kind)
				{
				case "zombie":
					if (config.canPlaceOnZombie)
					{
						towerDefenseCharacter = (HasSunAccount ? config.PlantOnZombie(_sunAccountId, target, hypnoses) : config.PlantOnZombie(target, hypnoses));
						break;
					}
					goto default;
				case "plant":
					if (config.characterConfig.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.PLANT))
					{
						towerDefenseCharacter = (HasSunAccount ? config.PlantOnPlant(_sunAccountId, target) : config.PlantOnPlant(target));
						break;
					}
					goto default;
				case "grid":
					if (mapCell.CanPacketPlant(config))
					{
						towerDefenseCharacter = (HasSunAccount ? config.Plant(_sunAccountId, vector2I) : config.Plant(vector2I));
						break;
					}
					goto default;
				default:
					towerDefenseCharacter = null;
					break;
				}
				TowerDefenseCharacter towerDefenseCharacter2 = towerDefenseCharacter;
				if (GodotObject.IsInstanceValid(towerDefenseCharacter2))
				{
					if (placement2.Kind == "grid")
					{
						columnPlantGridBudget.Reserve(vector2I);
					}
					RestoreMagicBeanRestoreState(towerDefenseCharacter2);
					list.Add(towerDefenseCharacter2);
					onCreated?.Invoke(placement2, towerDefenseCharacter2);
				}
			}
			if (list.Count == 0)
			{
				receipt?.TryRollback();
				return list;
			}
			if (Global.IsMultiplayerMode && HasMeta("packet_sync_id"))
			{
				SetMeta("packet_planted", true);
			}
			if (!flag)
			{
				CompleteUse(receipt);
				XWModCards.Notify(this, "PlantCommitted");
				BattleEventBus instance = BattleEventBus.Instance;
				if (instance != null)
				{
					instance.EmitPlantManuallyPlanted(list[list.Count - 1]);
				}
			}
			_cardBehaviorHost.NotifyUseSucceeded(list[list.Count - 1]);
			return list;
		}
	}

	public TowerDefenseCharacter PlantOnZombie(TowerDefenseCharacter zombie, bool hypnoses = false, bool useSun = true, bool executeEvent = true)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (!alive)
			{
				return null;
			}
			try
			{
				if (!ModAllowsPlacement(GodotObject.IsInstanceValid(zombie) ? zombie.gridPos : default(Vector2I), zombie))
				{
					return null;
				}
			}
			catch (XWModHookCallbackException)
			{
				return null;
			}
			TowerDefenseCellInstance towerDefenseCellInstance = (GodotObject.IsInstanceValid(zombie) ? TowerDefenseManager.GetMapCell(zombie.gridPos) : null);
			if (GodotObject.IsInstanceValid(towerDefenseCellInstance) && towerDefenseCellInstance.NotifyBlockedPlanting(config))
			{
				return null;
			}
			bool flag = Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage";
			SunSpendReceipt receipt = null;
			if (!flag && !TryBeginItemCost(useSun, out receipt))
			{
				return null;
			}
			TowerDefenseCharacter towerDefenseCharacter = (HasSunAccount ? config.PlantOnZombie(_sunAccountId, zombie, hypnoses) : config.PlantOnZombie(zombie, hypnoses));
			if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				receipt?.TryRollback();
				return null;
			}
			RestoreMagicBeanRestoreState(towerDefenseCharacter);
			if (Global.IsMultiplayerMode && HasMeta("packet_sync_id"))
			{
				SetMeta("packet_planted", true);
			}
			if (!flag)
			{
				CompleteUse(receipt);
				XWModCards.Notify(this, "PlantCommitted");
				BattleEventBus.Instance?.EmitPlantManuallyPlanted(towerDefenseCharacter);
			}
			if (executeEvent)
			{
				_cardBehaviorHost.NotifyUseSucceeded(towerDefenseCharacter);
			}
			return towerDefenseCharacter;
		}
	}

	public TowerDefenseCharacter PlantOnPlant(TowerDefenseCharacter plant, bool useSun = true, bool executeEvent = true)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (!alive)
			{
				return null;
			}
			try
			{
				if (!ModAllowsPlacement(GodotObject.IsInstanceValid(plant) ? plant.gridPos : default(Vector2I), plant))
				{
					return null;
				}
			}
			catch (XWModHookCallbackException)
			{
				return null;
			}
			TowerDefenseCellInstance towerDefenseCellInstance = (GodotObject.IsInstanceValid(plant) ? TowerDefenseManager.GetMapCell(plant.gridPos) : null);
			if (GodotObject.IsInstanceValid(towerDefenseCellInstance) && towerDefenseCellInstance.NotifyBlockedPlanting(config))
			{
				return null;
			}
			bool flag = Global.IsEditor && SceneManager.CurrentScene == "LevelEditorStage";
			SunSpendReceipt receipt = null;
			if (!flag && !TryBeginItemCost(useSun, out receipt))
			{
				return null;
			}
			TowerDefenseCharacter towerDefenseCharacter = (HasSunAccount ? config.PlantOnPlant(_sunAccountId, plant) : config.PlantOnPlant(plant));
			if (!GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				receipt?.TryRollback();
				return null;
			}
			RestoreMagicBeanRestoreState(towerDefenseCharacter);
			if (Global.IsMultiplayerMode && HasMeta("packet_sync_id"))
			{
				SetMeta("packet_planted", true);
			}
			if (!flag)
			{
				CompleteUse(receipt);
				XWModCards.Notify(this, "PlantCommitted");
				BattleEventBus.Instance?.EmitPlantManuallyPlanted(towerDefenseCharacter);
			}
			if (executeEvent)
			{
				_cardBehaviorHost.NotifyUseSucceeded(towerDefenseCharacter);
			}
			return towerDefenseCharacter;
		}
	}

	public bool Use(bool useSun = true)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (!TryBeginItemCost(useSun, out var receipt))
			{
				return false;
			}
			CompleteUse(receipt);
			_cardBehaviorHost.NotifyUseSucceeded(null, includeActions: false);
			return true;
		}
	}

	public void NotifyUseBehaviorSucceeded(TowerDefenseCharacter createdCharacter = null)
	{
		_cardBehaviorHost.NotifyUseSucceeded(createdCharacter);
	}

	public bool TryBeginPendingUse(out SunSpendReceipt spendReceipt, bool useSun = true)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (!TryBeginItemCost(useSun, out spendReceipt))
			{
				return false;
			}
			ModSelectionConsumed = true;
			ApplyUseState(removePlantOnce: false);
			return true;
		}
	}

	public bool TryCommitPendingUse(SunSpendReceipt spendReceipt)
	{
		using (XWModExtensions.EnterOperation())
		{
			if (spendReceipt != null && !spendReceipt.TryCommit())
			{
				return false;
			}
			ConsumePurchaseCostChanges();
			ModUseSequence++;
			ModSelectionConsumed = true;
			XWModCards.Notify(this, "Used");
			XWModCards.Notify(this, "PlantCommitted");
			if (plantOnce)
			{
				QueueFree();
			}
			return true;
		}
	}

	private void CompleteUse(SunSpendReceipt spendReceipt)
	{
		if (spendReceipt == null || spendReceipt.TryCommit())
		{
			ConsumePurchaseCostChanges();
			ModUseSequence++;
			ModSelectionConsumed = true;
			XWModCards.Notify(this, "Used");
			ApplyUseState(removePlantOnce: true);
		}
	}

	private void ConsumePurchaseCostChanges()
	{
		_modCostQuoteReady = false;
		ConsumeProphecy();
		if (!GodotObject.IsInstanceValid(config))
		{
			return;
		}
		bool flag = false;
		for (int num = config.changeCostList.Count - 1; num >= 0; num--)
		{
			TowerDefensePacketChangeCost towerDefensePacketChangeCost = config.changeCostList[num];
			if (GodotObject.IsInstanceValid(towerDefensePacketChangeCost) && towerDefensePacketChangeCost.consumeOnPurchase)
			{
				flag |= config.ChangeCostRemove(towerDefensePacketChangeCost);
			}
		}
		if (flag)
		{
			RefreshRuntimeState(includeCost: true);
		}
		UpdateItemCostLabel();
	}

	private void ApplyUseState(bool removePlantOnce)
	{
		if (removePlantOnce && plantOnce)
		{
			QueueFree();
		}
		if (!CommandManager.Instance.debugPacketColdDown)
		{
			TowerDefenseLevelBaseConfig obj = TowerDefenseManager.Instance.currentControl?.levelConfig;
			bool flag = false;
			if (obj is TowerDefenseLevelConfig towerDefenseLevelConfig)
			{
				flag = towerDefenseLevelConfig.packetColdDownUse;
			}
			else
			{
				TowerDefenseLevelSeedBankConfig towerDefenseLevelSeedBankConfig = TowerDefenseManager.Instance.GetSeedBankFeature()?.config;
				if (towerDefenseLevelSeedBankConfig != null)
				{
					flag = towerDefenseLevelSeedBankConfig.packetColdDownUse;
				}
			}
			if (flag)
			{
				coldDownProgressBar.Visible = true;
				coldDown = (_modCooldownPrepared ? _modCooldownPreparedValue : config.GetPacketCooldown());
				_modCooldownPrepared = false;
				coldDownProgressBar.MaxValue = coldDown;
				coldDownProgressBar.Value = coldDownProgressBar.MaxValue;
				coldDownOpen = coldDown > 0.0;
				coldDownTimer = coldDown;
				XWModCards.Notify(this, coldDownOpen ? "CooldownStarted" : "CooldownCompleted");
			}
		}
		Reset();
	}

	public void LoveButtonToggled(bool toggled)
	{
		if (_syncingLoveButtonFromSave)
		{
			return;
		}
		if (config == null || !TryGetPacketSaveValue(config.saveKey, out var packetData))
		{
			loveButton.ButtonPressed = toggled;
			this.OnLoveChange?.Invoke(this);
			return;
		}
		packetData["Love"] = toggled;
		loveButton.ButtonPressed = toggled;
		if (GameSaveManager.Instance != null)
		{
			XWModPlayerProgressService.SetPacketState(config.saveKey, packetData);
		}
		this.OnLoveChange?.Invoke(this);
	}

	private void SetLoveButtonPressedFromSave(bool pressed)
	{
		if (!GodotObject.IsInstanceValid(loveButton))
		{
			return;
		}
		_syncingLoveButtonFromSave = true;
		try
		{
			loveButton.ButtonPressed = pressed;
		}
		finally
		{
			_syncingLoveButtonFromSave = false;
		}
	}

	private void TrySubscribeCharacterSkinSwitch()
	{
		if (BattleEventBus.Instance != null)
		{
			BattleEventBus.Instance.OnCharacterSkinSwitched -= OnCharacterSkinSwitched;
			BattleEventBus.Instance.OnCharacterSkinSwitched += OnCharacterSkinSwitched;
		}
	}

	private void TryUnsubscribeCharacterSkinSwitch()
	{
		if (BattleEventBus.Instance != null)
		{
			BattleEventBus.Instance.OnCharacterSkinSwitched -= OnCharacterSkinSwitched;
		}
	}

	private static bool TryGetPacketSaveValue(string saveKey, out Dictionary packetData)
	{
		packetData = new Dictionary();
		if (string.IsNullOrWhiteSpace(saveKey) || GameSaveManager.Instance == null)
		{
			return false;
		}
		packetData = XWModPlayerProgressService.GetPacketState(saveKey);
		return packetData != null;
	}

	private static bool TryGetMobilePresetConfig()
	{
		if (GameSaveManager.Instance != null)
		{
			return GameSaveManager.Instance.GetConfigValue("MobilePreset").AsBool();
		}
		return false;
	}

	private bool CanContinueAsync(int asyncVersion)
	{
		if (_exitedTree || asyncVersion != _asyncLifetimeVersion)
		{
			return false;
		}
		try
		{
			return GodotObject.IsInstanceValid(this) && !IsQueuedForDeletion() && IsInsideTree();
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	private bool HasPreviewControls()
	{
		if (GodotObject.IsInstanceValid(previewClip))
		{
			return GodotObject.IsInstanceValid(previewSpriteNode);
		}
		return false;
	}

	public void SetPreviewCreationDeferred(bool deferred, bool releaseExisting = false)
	{
		if (_previewCreationDeferred == deferred && (!deferred || !releaseExisting || !GodotObject.IsInstanceValid(sprite)))
		{
			return;
		}
		_previewCreationDeferred = deferred;
		if (deferred)
		{
			if (releaseExisting && GodotObject.IsInstanceValid(sprite))
			{
				TryUnsubscribeCharacterSkinSwitch();
				sprite.ReleaseForcedCpuPoseData();
				sprite.ClearRenderClipControl();
				sprite.QueueFree();
				sprite = null;
			}
			SetPreviewVisible(visible: false);
		}
		else if (GodotObject.IsInstanceValid(config) && IsInsideTree())
		{
			TrySubscribeCharacterSkinSwitch();
			RefreshPreview();
		}
	}

	public void Cover(TowerDefensePacketConfig _config, TowerDefensePacketOverride overrideVal = null, bool keepColddown = true, bool changePacket = true)
	{
		if (!GodotObject.IsInstanceValid(overrideVal))
		{
			overrideVal = new TowerDefensePacketOverride();
		}
		if (changePacket)
		{
			CardActionBehaviorChangePacket cardActionBehaviorChangePacket = new CardActionBehaviorChangePacket();
			cardActionBehaviorChangePacket.packetConfig = config;
			overrideVal.useSucceededActions.Add(cardActionBehaviorChangePacket);
		}
		if (keepColddown)
		{
			CardActionBehaviorSetCooldown cardActionBehaviorSetCooldown = new CardActionBehaviorSetCooldown();
			cardActionBehaviorSetCooldown.value = coldDownTimer;
			overrideVal.useSucceededActions.Add(cardActionBehaviorSetCooldown);
		}
		_config._override = overrideVal;
		_config.changeCostList = config.changeCostList;
		Init(_config);
	}

	private void SetPreviewVisible(bool visible)
	{
		if (GodotObject.IsInstanceValid(previewSpriteNode))
		{
			previewSpriteNode.Visible = visible && GodotObject.IsInstanceValid(sprite);
		}
		if (GodotObject.IsInstanceValid(previewClip))
		{
			previewClip.Visible = visible && GodotObject.IsInstanceValid(sprite);
		}
		if (GodotObject.IsInstanceValid(sprite))
		{
			sprite.Visible = visible;
		}
	}

	private void ConfigurePreviewSprite(AdobeAnimateSprite animateSprite)
	{
		animateSprite.LightMask = 0;
		animateSprite.ZIndex = 0;
		animateSprite.keepRenderSubmittedWhenPaused = true;
		animateSprite.forceLocalRender = true;
		animateSprite.SetRenderClipControl(previewClip);
		animateSprite.ProcessMode = ProcessModeEnum.Always;
	}

	private void ConfigurePreviewNode(Node node)
	{
		if (node is CanvasItem canvasItem)
		{
			canvasItem.ZIndex = 0;
		}
		if (node is AdobeAnimateSprite animateSprite)
		{
			ConfigurePreviewSprite(animateSprite);
		}
	}

	private void ConfigurePreviewTreeBeforeAttach(Node node)
	{
		if (!GodotObject.IsInstanceValid(node))
		{
			return;
		}
		ConfigurePreviewNode(node);
		foreach (Node child in node.GetChildren(includeInternal: true))
		{
			ConfigurePreviewTreeBeforeAttach(child);
		}
	}

	private void FreezePreviewTree(Node node, bool forcePoseRefresh = false, bool forceRenderResubmit = false)
	{
		if (!GodotObject.IsInstanceValid(node))
		{
			return;
		}
		ConfigurePreviewNode(node);
		AdobeAnimateSprite adobeAnimateSprite = null;
		if (node is AdobeAnimateSprite adobeAnimateSprite2)
		{
			adobeAnimateSprite = adobeAnimateSprite2;
			bool flag = forcePoseRefresh || !adobeAnimateSprite2.IsFrozenPreview;
			if (flag)
			{
				adobeAnimateSprite2.RefreshManagedSlotSpriteCacheForRender();
				adobeAnimateSprite2.ResetAnimation();
				adobeAnimateSprite2.UpdateMediaReplaceData();
				adobeAnimateSprite2.UpdateChild();
				adobeAnimateSprite2.QueueRedraw();
			}
			adobeAnimateSprite2.SetFrozenPreview(frozen: true);
			if (forceRenderResubmit && !flag)
			{
				adobeAnimateSprite2.EnsureFrozenPreviewRenderSubmission();
			}
		}
		foreach (Node child in node.GetChildren(includeInternal: true))
		{
			FreezePreviewTree(child, forcePoseRefresh, forceRenderResubmit);
		}
		if (GodotObject.IsInstanceValid(adobeAnimateSprite))
		{
			SynchronizeSameDataPreviewChildren(adobeAnimateSprite);
		}
	}

	private void FreezePreparedPreviewTree(Node node, bool forcePoseRefresh, bool poseAlreadyPrepared)
	{
		if (!GodotObject.IsInstanceValid(node))
		{
			return;
		}
		ConfigurePreviewNode(node);
		AdobeAnimateSprite adobeAnimateSprite = null;
		if (node is AdobeAnimateSprite adobeAnimateSprite2)
		{
			adobeAnimateSprite = adobeAnimateSprite2;
			if (!poseAlreadyPrepared && (forcePoseRefresh || !adobeAnimateSprite2.IsFrozenPreview))
			{
				adobeAnimateSprite2.RefreshManagedSlotSpriteCacheForRender();
				adobeAnimateSprite2.UpdateMediaReplaceData();
				adobeAnimateSprite2.UpdateChild();
				adobeAnimateSprite2.QueueRedraw();
			}
			adobeAnimateSprite2.SetFrozenPreview(frozen: true);
		}
		foreach (Node child in node.GetChildren(includeInternal: true))
		{
			FreezePreparedPreviewTree(child, forcePoseRefresh, poseAlreadyPrepared);
		}
		if (GodotObject.IsInstanceValid(adobeAnimateSprite) && !poseAlreadyPrepared)
		{
			SynchronizeSameDataPreviewChildren(adobeAnimateSprite);
		}
	}

	private void SynchronizeSameDataPreviewChildren(AdobeAnimateSprite parent)
	{
		if (!GodotObject.IsInstanceValid(parent))
		{
			return;
		}
		AdobeAnimateData flashAnimeData = parent.flashAnimeData;
		if (!GodotObject.IsInstanceValid(flashAnimeData))
		{
			return;
		}
		foreach (Node child in parent.GetChildren(includeInternal: true))
		{
			if (child is AdobeAnimateSprite adobeAnimateSprite && adobeAnimateSprite != parent && adobeAnimateSprite.flashAnimeData == flashAnimeData && !(adobeAnimateSprite.clip != parent.clip))
			{
				adobeAnimateSprite.frameIndex = parent.frameIndex;
				adobeAnimateSprite.elapsedTimer = parent.elapsedTimer;
				adobeAnimateSprite.loop = parent.loop;
				adobeAnimateSprite.clipOver = parent.clipOver;
				adobeAnimateSprite.RefreshManagedSlotSpriteCacheForRender();
				adobeAnimateSprite.UpdateMediaReplaceData();
				adobeAnimateSprite.UpdateChild();
				adobeAnimateSprite.QueueRedraw();
			}
		}
	}

	public void ApplyProphecy(double duration, Vector2 position, bool charmed = false)
	{
		_prophecyRemaining = Math.Max(0.0, duration);
		_prophecyPosition = position;
		_prophecyCharmed = charmed;
		UpdateProphecyBorder();
		RefreshPhysicsProcessState();
	}

	private void ProcessProphecy(double delta)
	{
		if (IsProphesied)
		{
			TowerDefenseManager instance = TowerDefenseManager.Instance;
			if (instance != null && instance.IsGameRunning())
			{
				_prophecyRemaining = Math.Max(0.0, _prophecyRemaining - delta);
				UpdateProphecyBorder();
			}
		}
	}

	private void UpdateProphecyBorder()
	{
		if (!GodotObject.IsInstanceValid(layout) || !GodotObject.IsInstanceValid(backgroundTexture))
		{
			return;
		}
		if (IsProphesied && !GodotObject.IsInstanceValid(_prophecyBorder))
		{
			_prophecyBorder = new TextureRect
			{
				Name = "ProphecyBorder",
				MouseFilter = MouseFilterEnum.Ignore,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				ZIndex = 10
			};
			layout.AddChild(_prophecyBorder, forceReadableName: false, InternalMode.Disabled);
		}
		if (GodotObject.IsInstanceValid(_prophecyBorder))
		{
			_prophecyBorder.Visible = IsProphesied;
			if (IsProphesied)
			{
				_prophecyBorder.Texture = (isMobile ? ProphecyLandscape : ProphecyPortrait);
				_prophecyBorder.Position = backgroundTexture.Position;
				_prophecyBorder.Size = backgroundTexture.Size;
			}
		}
	}

	private void ConsumeProphecy()
	{
		if (!IsProphesied)
		{
			return;
		}
		bool prophecyCharmed = _prophecyCharmed;
		_prophecyRemaining = 0.0;
		_prophecyCharmed = false;
		UpdateProphecyBorder();
		if (!GodotObject.IsInstanceValid(TowerDefenseManager.Instance))
		{
			return;
		}
		if (prophecyCharmed)
		{
			if (HasSunAccount)
			{
				TowerDefenseManager.Instance.SunCreate(SunAccountId, _prophecyPosition, 50L);
			}
			else
			{
				TowerDefenseManager.Instance.SunCreate(_prophecyPosition, 50L);
			}
		}
		else if (HasSunAccount)
		{
			TowerDefenseManager.Instance.BrainSunCreate(SunAccountId, _prophecyPosition, 50L);
		}
		else
		{
			TowerDefenseManager.Instance.BrainSunCreate(_prophecyPosition, 50L);
		}
	}

	public Dictionary ExportProphecySave()
	{
		return new Dictionary
		{
			["remaining"] = _prophecyRemaining,
			["x"] = _prophecyPosition.X,
			["y"] = _prophecyPosition.Y,
			["charmed"] = _prophecyCharmed
		};
	}

	public void ImportProphecySave(Dictionary data)
	{
		ApplyProphecy(data.GetValueOrDefault("remaining", 0.0).AsDouble(), new Vector2((float)data.GetValueOrDefault("x", 0.0).AsDouble(), (float)data.GetValueOrDefault("y", 0.0).AsDouble()), data.GetValueOrDefault("charmed", false).AsBool());
	}

	public void FlashCostChange()
	{
		Control control = (GodotObject.IsInstanceValid(layout) ? layout : this);
		if (GodotObject.IsInstanceValid(control))
		{
			if (!_costChangeFlashActive)
			{
				_costChangeFlashBaseScale = control.Scale;
			}
			else
			{
				control.Scale = _costChangeFlashBaseScale;
			}
			_costChangeFlashActive = true;
			_costChangeFlashTween?.Kill();
			ColorSet();
			Color modulate = control.Modulate;
			Vector2 costChangeFlashBaseScale = _costChangeFlashBaseScale;
			Vector2 vector = costChangeFlashBaseScale * 1.14f;
			Color costChangeFlashColor = CostChangeFlashColor;
			Tween tween = (_costChangeFlashTween = CreateTween());
			tween.SetTrans(Tween.TransitionType.Sine);
			tween.SetEase(Tween.EaseType.Out);
			tween.TweenProperty(control, "modulate", costChangeFlashColor, 0.08);
			tween.Parallel().TweenProperty(control, "scale", vector, 0.08);
			tween.TweenInterval(0.1);
			tween.TweenProperty(control, "modulate", modulate, 0.24);
			tween.Parallel().TweenProperty(control, "scale", costChangeFlashBaseScale, 0.24);
			tween.TweenCallback(Callable.From(ResetCostChangeFlash));
		}
	}

	private void ResetCostChangeFlash()
	{
		_costChangeFlashTween = null;
		_costChangeFlashActive = false;
		Control control = (GodotObject.IsInstanceValid(layout) ? layout : this);
		if (GodotObject.IsInstanceValid(control))
		{
			control.Scale = _costChangeFlashBaseScale;
		}
		ColorSet();
	}

	private void PlayPreviewTreeFromStart(Node node)
	{
		if (!GodotObject.IsInstanceValid(node))
		{
			return;
		}
		ConfigurePreviewNode(node);
		if (node is AdobeAnimateSprite adobeAnimateSprite)
		{
			adobeAnimateSprite.RefreshManagedSlotSpriteCacheForRender();
			adobeAnimateSprite.ResetAnimation();
			adobeAnimateSprite.UpdateMediaReplaceData();
			adobeAnimateSprite.UpdateChild();
			adobeAnimateSprite.SetFrozenPreview(frozen: false);
			adobeAnimateSprite.RefreshProcessScheduling();
			adobeAnimateSprite.QueueRedraw();
		}
		foreach (Node child in node.GetChildren(includeInternal: true))
		{
			PlayPreviewTreeFromStart(child);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(97)
		{
			new MethodInfo(MethodName.SetScrollSelectionMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "scroll", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ScrollContainer"), exported: false)
			}, null),
			new MethodInfo(MethodName.OnSelectionScrollChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CancelScrollSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCardButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCardButtonGuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName.ContainsSelectionPoint, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "viewportPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateScrollSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.EndScrollSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.CompleteScrollSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "version", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SubscribeCooldownConfig, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCooldownConfigChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "key", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Nil, "value", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshCooldownLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetContainerFootprint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyContainerFootprint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanAffordItemCost, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.MeetsSunBalanceRequirement, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RefreshModCostQuote, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ModAllowsPlacement, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateItemCostLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "force", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetCentralRuntimeStateRefresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.InvalidateRuntimeState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "includeCost", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshRuntimeState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "includeCost", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshDynamicItemCost, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HasRequiredPlantCover, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyCachedRuntimeAvailability, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.NeedsContinuousPhysicsProcess, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RefreshPhysicsProcessState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ColorSet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.MobilePreset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetPcPreset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetMobileMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshPreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateBackgroundTexture, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateSpriteLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Clear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ClearEventHandlers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ResetForPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "_config", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "skipGlobalChangeCost", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.InitVisualPreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "previewConfig", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Int, "previewCost", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "previewCostRise", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "previewCostMultiple", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "previewCooldown", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyInitializedUiState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CreateSprite, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnCharacterSkinSwitched, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "packetSaveKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "customKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnVisibilityChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyDeferredVisibilityState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._PhysicsProcess, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GrantExpireSun, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Pressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnMouseEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnMouseExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Reset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanUseFromMod, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UsesCooldown, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ResolveModCooldown, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "seconds", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "initial", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetModObservableCooldown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "seconds", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.NotifyModUseCancelled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.NotifyModRestored, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartInit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartInitForRestore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.StartInitCore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "restoring", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RestoreMagicBeanRestoreState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.Plant, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "gridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "useSun", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "executeEvent", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlantFootprint, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2I, "click", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "useSun", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "executeEvent", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlantOnZombie, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "zombie", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "hypnoses", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "useSun", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "executeEvent", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PlantOnPlant, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "plant", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "useSun", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "executeEvent", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Use, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "useSun", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.NotifyUseBehaviorSucceeded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "createdCharacter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ConsumePurchaseCostChanges, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ApplyUseState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "removePlantOnce", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.LoveButtonToggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "toggled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetLoveButtonPressedFromSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "pressed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TrySubscribeCharacterSkinSwitch, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryUnsubscribeCharacterSkinSwitch, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.TryGetMobilePresetConfig, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null),
			new MethodInfo(MethodName.CanContinueAsync, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "asyncVersion", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.HasPreviewControls, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetPreviewCreationDeferred, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "deferred", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "releaseExisting", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Cover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "_config", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Object, "overrideVal", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "keepColddown", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "changePacket", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetPreviewVisible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "visible", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigurePreviewSprite, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "animateSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigurePreviewNode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.ConfigurePreviewTreeBeforeAttach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.FreezePreviewTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "forcePoseRefresh", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "forceRenderResubmit", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FreezePreparedPreviewTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false),
				new PropertyInfo(Variant.Type.Bool, "forcePoseRefresh", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "poseAlreadyPrepared", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SynchronizeSameDataPreviewChildren, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "parent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyProphecy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "charmed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ProcessProphecy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateProphecyBorder, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ConsumeProphecy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExportProphecySave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportProphecySave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FlashCostChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ResetCostChangeFlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PlayPreviewTreeFromStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetScrollSelectionMode && args.Count == 1)
		{
			SetScrollSelectionMode(VariantUtils.ConvertTo<ScrollContainer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSelectionScrollChanged && args.Count == 1)
		{
			OnSelectionScrollChanged(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelScrollSelection && args.Count == 0)
		{
			CancelScrollSelection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardButtonPressed && args.Count == 0)
		{
			OnCardButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardButtonGuiInput && args.Count == 1)
		{
			OnCardButtonGuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ContainsSelectionPoint && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(ContainsSelectionPoint(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.UpdateScrollSelection && args.Count == 1)
		{
			UpdateScrollSelection(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndScrollSelection && args.Count == 1)
		{
			EndScrollSelection(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteScrollSelection && args.Count == 1)
		{
			CompleteScrollSelection(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SubscribeCooldownConfig && args.Count == 0)
		{
			SubscribeCooldownConfig();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCooldownConfigChanged && args.Count == 2)
		{
			OnCooldownConfigChanged(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Variant>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshCooldownLabel && args.Count == 0)
		{
			RefreshCooldownLabel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetContainerFootprint && args.Count == 1)
		{
			SetContainerFootprint(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyContainerFootprint && args.Count == 0)
		{
			ApplyContainerFootprint();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanAffordItemCost && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanAffordItemCost());
			return true;
		}
		if (method == MethodName.MeetsSunBalanceRequirement && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(MeetsSunBalanceRequirement());
			return true;
		}
		if (method == MethodName.RefreshModCostQuote && args.Count == 0)
		{
			RefreshModCostQuote();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModAllowsPlacement && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(ModAllowsPlacement(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1])));
			return true;
		}
		if (method == MethodName.UpdateItemCostLabel && args.Count == 1)
		{
			UpdateItemCostLabel(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCentralRuntimeStateRefresh && args.Count == 1)
		{
			SetCentralRuntimeStateRefresh(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InvalidateRuntimeState && args.Count == 1)
		{
			InvalidateRuntimeState(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshRuntimeState && args.Count == 1)
		{
			RefreshRuntimeState(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshDynamicItemCost && args.Count == 0)
		{
			RefreshDynamicItemCost();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasRequiredPlantCover && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasRequiredPlantCover());
			return true;
		}
		if (method == MethodName.ApplyCachedRuntimeAvailability && args.Count == 0)
		{
			ApplyCachedRuntimeAvailability();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NeedsContinuousPhysicsProcess && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(NeedsContinuousPhysicsProcess());
			return true;
		}
		if (method == MethodName.RefreshPhysicsProcessState && args.Count == 0)
		{
			RefreshPhysicsProcessState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ColorSet && args.Count == 0)
		{
			ColorSet();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MobilePreset && args.Count == 0)
		{
			MobilePreset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPcPreset && args.Count == 0)
		{
			SetPcPreset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMobileMode && args.Count == 1)
		{
			SetMobileMode(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshPreview && args.Count == 0)
		{
			RefreshPreview();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateBackgroundTexture && args.Count == 0)
		{
			UpdateBackgroundTexture();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateSpriteLayout && args.Count == 0)
		{
			UpdateSpriteLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Clear && args.Count == 0)
		{
			Clear();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearEventHandlers && args.Count == 0)
		{
			ClearEventHandlers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetForPool && args.Count == 0)
		{
			ResetForPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Init && args.Count == 2)
		{
			Init(VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitVisualPreview && args.Count == 5)
		{
			InitVisualPreview(VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<double>(in args[3]), VariantUtils.ConvertTo<double>(in args[4]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyInitializedUiState && args.Count == 0)
		{
			ApplyInitializedUiState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateSprite && args.Count == 0)
		{
			CreateSprite();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCharacterSkinSwitched && args.Count == 2)
		{
			OnCharacterSkinSwitched(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChanged && args.Count == 0)
		{
			OnVisibilityChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDeferredVisibilityState && args.Count == 0)
		{
			ApplyDeferredVisibilityState();
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
		if (method == MethodName.GrantExpireSun && args.Count == 0)
		{
			GrantExpireSun();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Pressed && args.Count == 0)
		{
			Pressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseEntered && args.Count == 0)
		{
			OnMouseEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseExited && args.Count == 0)
		{
			OnMouseExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reset && args.Count == 0)
		{
			Reset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanUseFromMod && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanUseFromMod());
			return true;
		}
		if (method == MethodName.UsesCooldown && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(UsesCooldown());
			return true;
		}
		if (method == MethodName.ResolveModCooldown && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<double>(ResolveModCooldown(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
			return true;
		}
		if (method == MethodName.SetModObservableCooldown && args.Count == 1)
		{
			SetModObservableCooldown(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NotifyModUseCancelled && args.Count == 0)
		{
			NotifyModUseCancelled();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NotifyModRestored && args.Count == 0)
		{
			NotifyModRestored();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartInit && args.Count == 0)
		{
			StartInit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartInitForRestore && args.Count == 0)
		{
			StartInitForRestore();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartInitCore && args.Count == 1)
		{
			StartInitCore(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestoreMagicBeanRestoreState && args.Count == 1)
		{
			RestoreMagicBeanRestoreState(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Plant && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(Plant(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.PlantFootprint && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(PlantFootprint(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.PlantOnZombie && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(PlantOnZombie(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
			return true;
		}
		if (method == MethodName.PlantOnPlant && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(PlantOnPlant(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.Use && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(Use(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.NotifyUseBehaviorSucceeded && args.Count == 1)
		{
			NotifyUseBehaviorSucceeded(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConsumePurchaseCostChanges && args.Count == 0)
		{
			ConsumePurchaseCostChanges();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyUseState && args.Count == 1)
		{
			ApplyUseState(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoveButtonToggled && args.Count == 1)
		{
			LoveButtonToggled(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLoveButtonPressedFromSave && args.Count == 1)
		{
			SetLoveButtonPressedFromSave(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TrySubscribeCharacterSkinSwitch && args.Count == 0)
		{
			TrySubscribeCharacterSkinSwitch();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryUnsubscribeCharacterSkinSwitch && args.Count == 0)
		{
			TryUnsubscribeCharacterSkinSwitch();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryGetMobilePresetConfig && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(TryGetMobilePresetConfig());
			return true;
		}
		if (method == MethodName.CanContinueAsync && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(CanContinueAsync(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.HasPreviewControls && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasPreviewControls());
			return true;
		}
		if (method == MethodName.SetPreviewCreationDeferred && args.Count == 2)
		{
			SetPreviewCreationDeferred(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cover && args.Count == 4)
		{
			Cover(VariantUtils.ConvertTo<TowerDefensePacketConfig>(in args[0]), VariantUtils.ConvertTo<TowerDefensePacketOverride>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPreviewVisible && args.Count == 1)
		{
			SetPreviewVisible(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigurePreviewSprite && args.Count == 1)
		{
			ConfigurePreviewSprite(VariantUtils.ConvertTo<AdobeAnimateSprite>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigurePreviewNode && args.Count == 1)
		{
			ConfigurePreviewNode(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfigurePreviewTreeBeforeAttach && args.Count == 1)
		{
			ConfigurePreviewTreeBeforeAttach(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FreezePreviewTree && args.Count == 3)
		{
			FreezePreviewTree(VariantUtils.ConvertTo<Node>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FreezePreparedPreviewTree && args.Count == 3)
		{
			FreezePreparedPreviewTree(VariantUtils.ConvertTo<Node>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SynchronizeSameDataPreviewChildren && args.Count == 1)
		{
			SynchronizeSameDataPreviewChildren(VariantUtils.ConvertTo<AdobeAnimateSprite>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyProphecy && args.Count == 3)
		{
			ApplyProphecy(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessProphecy && args.Count == 1)
		{
			ProcessProphecy(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateProphecyBorder && args.Count == 0)
		{
			UpdateProphecyBorder();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConsumeProphecy && args.Count == 0)
		{
			ConsumeProphecy();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExportProphecySave && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportProphecySave());
			return true;
		}
		if (method == MethodName.ImportProphecySave && args.Count == 1)
		{
			ImportProphecySave(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlashCostChange && args.Count == 0)
		{
			FlashCostChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetCostChangeFlash && args.Count == 0)
		{
			ResetCostChangeFlash();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayPreviewTreeFromStart && args.Count == 1)
		{
			PlayPreviewTreeFromStart(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.TryGetMobilePresetConfig && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(TryGetMobilePresetConfig());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetScrollSelectionMode)
		{
			return true;
		}
		if (method == MethodName.OnSelectionScrollChanged)
		{
			return true;
		}
		if (method == MethodName.CancelScrollSelection)
		{
			return true;
		}
		if (method == MethodName.OnCardButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnCardButtonGuiInput)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ContainsSelectionPoint)
		{
			return true;
		}
		if (method == MethodName.UpdateScrollSelection)
		{
			return true;
		}
		if (method == MethodName.EndScrollSelection)
		{
			return true;
		}
		if (method == MethodName.CompleteScrollSelection)
		{
			return true;
		}
		if (method == MethodName.SubscribeCooldownConfig)
		{
			return true;
		}
		if (method == MethodName.OnCooldownConfigChanged)
		{
			return true;
		}
		if (method == MethodName.RefreshCooldownLabel)
		{
			return true;
		}
		if (method == MethodName.SetContainerFootprint)
		{
			return true;
		}
		if (method == MethodName.ApplyContainerFootprint)
		{
			return true;
		}
		if (method == MethodName.CanAffordItemCost)
		{
			return true;
		}
		if (method == MethodName.MeetsSunBalanceRequirement)
		{
			return true;
		}
		if (method == MethodName.RefreshModCostQuote)
		{
			return true;
		}
		if (method == MethodName.ModAllowsPlacement)
		{
			return true;
		}
		if (method == MethodName.UpdateItemCostLabel)
		{
			return true;
		}
		if (method == MethodName.SetCentralRuntimeStateRefresh)
		{
			return true;
		}
		if (method == MethodName.InvalidateRuntimeState)
		{
			return true;
		}
		if (method == MethodName.RefreshRuntimeState)
		{
			return true;
		}
		if (method == MethodName.RefreshDynamicItemCost)
		{
			return true;
		}
		if (method == MethodName.HasRequiredPlantCover)
		{
			return true;
		}
		if (method == MethodName.ApplyCachedRuntimeAvailability)
		{
			return true;
		}
		if (method == MethodName.NeedsContinuousPhysicsProcess)
		{
			return true;
		}
		if (method == MethodName.RefreshPhysicsProcessState)
		{
			return true;
		}
		if (method == MethodName.ColorSet)
		{
			return true;
		}
		if (method == MethodName.MobilePreset)
		{
			return true;
		}
		if (method == MethodName.SetPcPreset)
		{
			return true;
		}
		if (method == MethodName.SetMobileMode)
		{
			return true;
		}
		if (method == MethodName.RefreshPreview)
		{
			return true;
		}
		if (method == MethodName.UpdateBackgroundTexture)
		{
			return true;
		}
		if (method == MethodName.UpdateSpriteLayout)
		{
			return true;
		}
		if (method == MethodName.Clear)
		{
			return true;
		}
		if (method == MethodName.ClearEventHandlers)
		{
			return true;
		}
		if (method == MethodName.ResetForPool)
		{
			return true;
		}
		if (method == MethodName.Init)
		{
			return true;
		}
		if (method == MethodName.InitVisualPreview)
		{
			return true;
		}
		if (method == MethodName.ApplyInitializedUiState)
		{
			return true;
		}
		if (method == MethodName.CreateSprite)
		{
			return true;
		}
		if (method == MethodName.OnCharacterSkinSwitched)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChanged)
		{
			return true;
		}
		if (method == MethodName.ApplyDeferredVisibilityState)
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._PhysicsProcess)
		{
			return true;
		}
		if (method == MethodName.GrantExpireSun)
		{
			return true;
		}
		if (method == MethodName.Pressed)
		{
			return true;
		}
		if (method == MethodName.OnMouseEntered)
		{
			return true;
		}
		if (method == MethodName.OnMouseExited)
		{
			return true;
		}
		if (method == MethodName.Reset)
		{
			return true;
		}
		if (method == MethodName.CanUseFromMod)
		{
			return true;
		}
		if (method == MethodName.UsesCooldown)
		{
			return true;
		}
		if (method == MethodName.ResolveModCooldown)
		{
			return true;
		}
		if (method == MethodName.SetModObservableCooldown)
		{
			return true;
		}
		if (method == MethodName.NotifyModUseCancelled)
		{
			return true;
		}
		if (method == MethodName.NotifyModRestored)
		{
			return true;
		}
		if (method == MethodName.StartInit)
		{
			return true;
		}
		if (method == MethodName.StartInitForRestore)
		{
			return true;
		}
		if (method == MethodName.StartInitCore)
		{
			return true;
		}
		if (method == MethodName.RestoreMagicBeanRestoreState)
		{
			return true;
		}
		if (method == MethodName.Plant)
		{
			return true;
		}
		if (method == MethodName.PlantFootprint)
		{
			return true;
		}
		if (method == MethodName.PlantOnZombie)
		{
			return true;
		}
		if (method == MethodName.PlantOnPlant)
		{
			return true;
		}
		if (method == MethodName.Use)
		{
			return true;
		}
		if (method == MethodName.NotifyUseBehaviorSucceeded)
		{
			return true;
		}
		if (method == MethodName.ConsumePurchaseCostChanges)
		{
			return true;
		}
		if (method == MethodName.ApplyUseState)
		{
			return true;
		}
		if (method == MethodName.LoveButtonToggled)
		{
			return true;
		}
		if (method == MethodName.SetLoveButtonPressedFromSave)
		{
			return true;
		}
		if (method == MethodName.TrySubscribeCharacterSkinSwitch)
		{
			return true;
		}
		if (method == MethodName.TryUnsubscribeCharacterSkinSwitch)
		{
			return true;
		}
		if (method == MethodName.TryGetMobilePresetConfig)
		{
			return true;
		}
		if (method == MethodName.CanContinueAsync)
		{
			return true;
		}
		if (method == MethodName.HasPreviewControls)
		{
			return true;
		}
		if (method == MethodName.SetPreviewCreationDeferred)
		{
			return true;
		}
		if (method == MethodName.Cover)
		{
			return true;
		}
		if (method == MethodName.SetPreviewVisible)
		{
			return true;
		}
		if (method == MethodName.ConfigurePreviewSprite)
		{
			return true;
		}
		if (method == MethodName.ConfigurePreviewNode)
		{
			return true;
		}
		if (method == MethodName.ConfigurePreviewTreeBeforeAttach)
		{
			return true;
		}
		if (method == MethodName.FreezePreviewTree)
		{
			return true;
		}
		if (method == MethodName.FreezePreparedPreviewTree)
		{
			return true;
		}
		if (method == MethodName.SynchronizeSameDataPreviewChildren)
		{
			return true;
		}
		if (method == MethodName.ApplyProphecy)
		{
			return true;
		}
		if (method == MethodName.ProcessProphecy)
		{
			return true;
		}
		if (method == MethodName.UpdateProphecyBorder)
		{
			return true;
		}
		if (method == MethodName.ConsumeProphecy)
		{
			return true;
		}
		if (method == MethodName.ExportProphecySave)
		{
			return true;
		}
		if (method == MethodName.ImportProphecySave)
		{
			return true;
		}
		if (method == MethodName.FlashCostChange)
		{
			return true;
		}
		if (method == MethodName.ResetCostChangeFlash)
		{
			return true;
		}
		if (method == MethodName.PlayPreviewTreeFromStart)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.config)
		{
			config = VariantUtils.ConvertTo<TowerDefensePacketConfig>(in value);
			return true;
		}
		if (name == PropertyName.showLove)
		{
			showLove = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.showCost)
		{
			showCost = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.onlyDraw)
		{
			onlyDraw = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.alive)
		{
			alive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.@lock)
		{
			@lock = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.openShadow)
		{
			openShadow = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.start)
		{
			start = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.select)
		{
			select = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.ModUseSequence)
		{
			ModUseSequence = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName.ModSelectionConsumed)
		{
			ModSelectionConsumed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.coldDownOpen)
		{
			coldDownOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.pressDelayTimer)
		{
			pressDelayTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.aliveTime)
		{
			aliveTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.height)
		{
			height = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.itemCost)
		{
			itemCost = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._asyncLifetimeVersion)
		{
			_asyncLifetimeVersion = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._exitedTree)
		{
			_exitedTree = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._visibilityRefreshQueued)
		{
			_visibilityRefreshQueued = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._queuedVisibilityRefreshVersion)
		{
			_queuedVisibilityRefreshVersion = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._centralRuntimeStateRefresh)
		{
			_centralRuntimeStateRefresh = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._runtimeAvailabilityDirty)
		{
			_runtimeAvailabilityDirty = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._runtimeCostDirty)
		{
			_runtimeCostDirty = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cachedRuntimeAvailability)
		{
			_cachedRuntimeAvailability = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cachedUpgradePacket)
		{
			_cachedUpgradePacket = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._modCostQuoteBase)
		{
			_modCostQuoteBase = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._modCostQuoteRevision)
		{
			_modCostQuoteRevision = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._modCostQuote)
		{
			_modCostQuote = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._modCostQuoteReady)
		{
			_modCostQuoteReady = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._modCostQuoteSuccess)
		{
			_modCostQuoteSuccess = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._resolvingModCostQuote)
		{
			_resolvingModCostQuote = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._itemCostLabelQuoteValid)
		{
			_itemCostLabelQuoteValid = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._modCooldownPrepared)
		{
			_modCooldownPrepared = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._modCooldownPreparedValue)
		{
			_modCooldownPreparedValue = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.previewClip)
		{
			previewClip = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.previewSpriteNode)
		{
			previewSpriteNode = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.backgroundTexture)
		{
			backgroundTexture = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName.selectTexture)
		{
			selectTexture = VariantUtils.ConvertTo<NinePatchRect>(in value);
			return true;
		}
		if (name == PropertyName.layout)
		{
			layout = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.body)
		{
			body = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.itemCostLabel)
		{
			itemCostLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName.button)
		{
			button = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName.coldDownProgressBar)
		{
			coldDownProgressBar = VariantUtils.ConvertTo<TextureProgressBar>(in value);
			return true;
		}
		if (name == PropertyName._cooldownLabel)
		{
			_cooldownLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._showPacketCooldown)
		{
			_showPacketCooldown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cooldownConfigOwner)
		{
			_cooldownConfigOwner = VariantUtils.ConvertTo<GameSaveManager>(in value);
			return true;
		}
		if (name == PropertyName._cooldownText)
		{
			_cooldownText = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._selectionScroll)
		{
			_selectionScroll = VariantUtils.ConvertTo<ScrollContainer>(in value);
			return true;
		}
		if (name == PropertyName._selectionWindow)
		{
			_selectionWindow = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		if (name == PropertyName._scrollGestureActive)
		{
			_scrollGestureActive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._scrollGestureCancelled)
		{
			_scrollGestureCancelled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pendingScrollClick)
		{
			_pendingScrollClick = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._scrollPointerIndex)
		{
			_scrollPointerIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._scrollGestureVersion)
		{
			_scrollGestureVersion = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._scrollPressPosition)
		{
			_scrollPressPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scrollPressConfig)
		{
			_scrollPressConfig = VariantUtils.ConvertTo<TowerDefensePacketConfig>(in value);
			return true;
		}
		if (name == PropertyName.loveButton)
		{
			loveButton = VariantUtils.ConvertTo<TextureButton>(in value);
			return true;
		}
		if (name == PropertyName.moveComponent)
		{
			moveComponent = VariantUtils.ConvertTo<MoveComponent>(in value);
			return true;
		}
		if (name == PropertyName._reserveContainerFootprint)
		{
			_reserveContainerFootprint = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._config)
		{
			_config = VariantUtils.ConvertTo<TowerDefensePacketConfig>(in value);
			return true;
		}
		if (name == PropertyName._showLove)
		{
			_showLove = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._syncingLoveButtonFromSave)
		{
			_syncingLoveButtonFromSave = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._showCost)
		{
			_showCost = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._onlyDraw)
		{
			_onlyDraw = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._alive)
		{
			_alive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._lock)
		{
			_lock = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.plantOnce)
		{
			plantOnce = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.useCost)
		{
			useCost = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._openShadow)
		{
			_openShadow = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._start)
		{
			_start = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._select)
		{
			_select = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.coldDown)
		{
			coldDown = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._coldDownOpen)
		{
			_coldDownOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.coldDownTimer)
		{
			coldDownTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._pressDelayTimer)
		{
			_pressDelayTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.setMobileLayout)
		{
			setMobileLayout = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.setPcLayout)
		{
			setPcLayout = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.canPressPutBack)
		{
			canPressPutBack = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.enforceRuntimeAvailabilityOnPress)
		{
			enforceRuntimeAvailabilityOnPress = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.allowPressWhenUnavailable)
		{
			allowPressWhenUnavailable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.isMobile)
		{
			isMobile = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.pcProgressTexture)
		{
			pcProgressTexture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._aliveTime)
		{
			_aliveTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.aliveTimer)
		{
			aliveTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.blinkTimer)
		{
			blinkTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.blink)
		{
			blink = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._height)
		{
			_height = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.savePos)
		{
			savePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.originalSaveKey)
		{
			originalSaveKey = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.sprite)
		{
			sprite = VariantUtils.ConvertTo<AdobeAnimateSprite>(in value);
			return true;
		}
		if (name == PropertyName._livePreviewGeneration)
		{
			_livePreviewGeneration = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._previewHovered)
		{
			_previewHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._previewCreationDeferred)
		{
			_previewCreationDeferred = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.baseItemCost)
		{
			baseItemCost = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._itemCostLabelInitialized)
		{
			_itemCostLabelInitialized = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._itemCostLabelUsesRiseSuffix)
		{
			_itemCostLabelUsesRiseSuffix = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._itemCostLabelValue)
		{
			_itemCostLabelValue = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._costChangeFlashTween)
		{
			_costChangeFlashTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._costChangeFlashBaseScale)
		{
			_costChangeFlashBaseScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._costChangeFlashActive)
		{
			_costChangeFlashActive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._itemCost)
		{
			_itemCost = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName.riseCost)
		{
			riseCost = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.costMultiple)
		{
			costMultiple = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._footprintPlacementPending)
		{
			_footprintPlacementPending = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._prophecyBorder)
		{
			_prophecyBorder = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._prophecyRemaining)
		{
			_prophecyRemaining = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._prophecyPosition)
		{
			_prophecyPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._prophecyCharmed)
		{
			_prophecyCharmed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.HasSunAccount)
		{
			from = HasSunAccount;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.config)
		{
			value = VariantUtils.CreateFrom<TowerDefensePacketConfig>(config);
			return true;
		}
		if (name == PropertyName.showLove)
		{
			from = showLove;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.showCost)
		{
			from = showCost;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.onlyDraw)
		{
			from = onlyDraw;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.alive)
		{
			from = alive;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.@lock)
		{
			from = @lock;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.openShadow)
		{
			from = openShadow;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.start)
		{
			from = start;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.select)
		{
			from = select;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		long from2;
		if (name == PropertyName.ModUseSequence)
		{
			from2 = ModUseSequence;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.ModSelectionConsumed)
		{
			from = ModSelectionConsumed;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.coldDownOpen)
		{
			from = coldDownOpen;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		double from3;
		if (name == PropertyName.pressDelayTimer)
		{
			from3 = pressDelayTimer;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.aliveTime)
		{
			from3 = aliveTime;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.height)
		{
			from3 = height;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.IsPreviewCreationDeferred)
		{
			from = IsPreviewCreationDeferred;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasPreparedPreview)
		{
			from = HasPreparedPreview;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.PreparedPreviewClip)
		{
			value = VariantUtils.CreateFrom<string>(PreparedPreviewClip);
			return true;
		}
		if (name == PropertyName.itemCost)
		{
			from2 = itemCost;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.IsProphesied)
		{
			from = IsProphesied;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ProphecyRemaining)
		{
			from3 = ProphecyRemaining;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName._asyncLifetimeVersion)
		{
			value = VariantUtils.CreateFrom(in _asyncLifetimeVersion);
			return true;
		}
		if (name == PropertyName._exitedTree)
		{
			value = VariantUtils.CreateFrom(in _exitedTree);
			return true;
		}
		if (name == PropertyName._visibilityRefreshQueued)
		{
			value = VariantUtils.CreateFrom(in _visibilityRefreshQueued);
			return true;
		}
		if (name == PropertyName._queuedVisibilityRefreshVersion)
		{
			value = VariantUtils.CreateFrom(in _queuedVisibilityRefreshVersion);
			return true;
		}
		if (name == PropertyName._centralRuntimeStateRefresh)
		{
			value = VariantUtils.CreateFrom(in _centralRuntimeStateRefresh);
			return true;
		}
		if (name == PropertyName._runtimeAvailabilityDirty)
		{
			value = VariantUtils.CreateFrom(in _runtimeAvailabilityDirty);
			return true;
		}
		if (name == PropertyName._runtimeCostDirty)
		{
			value = VariantUtils.CreateFrom(in _runtimeCostDirty);
			return true;
		}
		if (name == PropertyName._cachedRuntimeAvailability)
		{
			value = VariantUtils.CreateFrom(in _cachedRuntimeAvailability);
			return true;
		}
		if (name == PropertyName._cachedUpgradePacket)
		{
			value = VariantUtils.CreateFrom(in _cachedUpgradePacket);
			return true;
		}
		if (name == PropertyName._modCostQuoteBase)
		{
			value = VariantUtils.CreateFrom(in _modCostQuoteBase);
			return true;
		}
		if (name == PropertyName._modCostQuoteRevision)
		{
			value = VariantUtils.CreateFrom(in _modCostQuoteRevision);
			return true;
		}
		if (name == PropertyName._modCostQuote)
		{
			value = VariantUtils.CreateFrom(in _modCostQuote);
			return true;
		}
		if (name == PropertyName._modCostQuoteReady)
		{
			value = VariantUtils.CreateFrom(in _modCostQuoteReady);
			return true;
		}
		if (name == PropertyName._modCostQuoteSuccess)
		{
			value = VariantUtils.CreateFrom(in _modCostQuoteSuccess);
			return true;
		}
		if (name == PropertyName._resolvingModCostQuote)
		{
			value = VariantUtils.CreateFrom(in _resolvingModCostQuote);
			return true;
		}
		if (name == PropertyName._itemCostLabelQuoteValid)
		{
			value = VariantUtils.CreateFrom(in _itemCostLabelQuoteValid);
			return true;
		}
		if (name == PropertyName._modCooldownPrepared)
		{
			value = VariantUtils.CreateFrom(in _modCooldownPrepared);
			return true;
		}
		if (name == PropertyName._modCooldownPreparedValue)
		{
			value = VariantUtils.CreateFrom(in _modCooldownPreparedValue);
			return true;
		}
		if (name == PropertyName.previewClip)
		{
			value = VariantUtils.CreateFrom(in previewClip);
			return true;
		}
		if (name == PropertyName.previewSpriteNode)
		{
			value = VariantUtils.CreateFrom(in previewSpriteNode);
			return true;
		}
		if (name == PropertyName.backgroundTexture)
		{
			value = VariantUtils.CreateFrom(in backgroundTexture);
			return true;
		}
		if (name == PropertyName.selectTexture)
		{
			value = VariantUtils.CreateFrom(in selectTexture);
			return true;
		}
		if (name == PropertyName.layout)
		{
			value = VariantUtils.CreateFrom(in layout);
			return true;
		}
		if (name == PropertyName.body)
		{
			value = VariantUtils.CreateFrom(in body);
			return true;
		}
		if (name == PropertyName.itemCostLabel)
		{
			value = VariantUtils.CreateFrom(in itemCostLabel);
			return true;
		}
		if (name == PropertyName.button)
		{
			value = VariantUtils.CreateFrom(in button);
			return true;
		}
		if (name == PropertyName.coldDownProgressBar)
		{
			value = VariantUtils.CreateFrom(in coldDownProgressBar);
			return true;
		}
		if (name == PropertyName._cooldownLabel)
		{
			value = VariantUtils.CreateFrom(in _cooldownLabel);
			return true;
		}
		if (name == PropertyName._showPacketCooldown)
		{
			value = VariantUtils.CreateFrom(in _showPacketCooldown);
			return true;
		}
		if (name == PropertyName._cooldownConfigOwner)
		{
			value = VariantUtils.CreateFrom(in _cooldownConfigOwner);
			return true;
		}
		if (name == PropertyName._cooldownText)
		{
			value = VariantUtils.CreateFrom(in _cooldownText);
			return true;
		}
		if (name == PropertyName._selectionScroll)
		{
			value = VariantUtils.CreateFrom(in _selectionScroll);
			return true;
		}
		if (name == PropertyName._selectionWindow)
		{
			value = VariantUtils.CreateFrom(in _selectionWindow);
			return true;
		}
		if (name == PropertyName._scrollGestureActive)
		{
			value = VariantUtils.CreateFrom(in _scrollGestureActive);
			return true;
		}
		if (name == PropertyName._scrollGestureCancelled)
		{
			value = VariantUtils.CreateFrom(in _scrollGestureCancelled);
			return true;
		}
		if (name == PropertyName._pendingScrollClick)
		{
			value = VariantUtils.CreateFrom(in _pendingScrollClick);
			return true;
		}
		if (name == PropertyName._scrollPointerIndex)
		{
			value = VariantUtils.CreateFrom(in _scrollPointerIndex);
			return true;
		}
		if (name == PropertyName._scrollGestureVersion)
		{
			value = VariantUtils.CreateFrom(in _scrollGestureVersion);
			return true;
		}
		if (name == PropertyName._scrollPressPosition)
		{
			value = VariantUtils.CreateFrom(in _scrollPressPosition);
			return true;
		}
		if (name == PropertyName._scrollPressConfig)
		{
			value = VariantUtils.CreateFrom(in _scrollPressConfig);
			return true;
		}
		if (name == PropertyName.loveButton)
		{
			value = VariantUtils.CreateFrom(in loveButton);
			return true;
		}
		if (name == PropertyName.moveComponent)
		{
			value = VariantUtils.CreateFrom(in moveComponent);
			return true;
		}
		if (name == PropertyName._reserveContainerFootprint)
		{
			value = VariantUtils.CreateFrom(in _reserveContainerFootprint);
			return true;
		}
		if (name == PropertyName._config)
		{
			value = VariantUtils.CreateFrom(in _config);
			return true;
		}
		if (name == PropertyName._showLove)
		{
			value = VariantUtils.CreateFrom(in _showLove);
			return true;
		}
		if (name == PropertyName._syncingLoveButtonFromSave)
		{
			value = VariantUtils.CreateFrom(in _syncingLoveButtonFromSave);
			return true;
		}
		if (name == PropertyName._showCost)
		{
			value = VariantUtils.CreateFrom(in _showCost);
			return true;
		}
		if (name == PropertyName._onlyDraw)
		{
			value = VariantUtils.CreateFrom(in _onlyDraw);
			return true;
		}
		if (name == PropertyName._alive)
		{
			value = VariantUtils.CreateFrom(in _alive);
			return true;
		}
		if (name == PropertyName._lock)
		{
			value = VariantUtils.CreateFrom(in _lock);
			return true;
		}
		if (name == PropertyName.plantOnce)
		{
			value = VariantUtils.CreateFrom(in plantOnce);
			return true;
		}
		if (name == PropertyName.useCost)
		{
			value = VariantUtils.CreateFrom(in useCost);
			return true;
		}
		if (name == PropertyName._openShadow)
		{
			value = VariantUtils.CreateFrom(in _openShadow);
			return true;
		}
		if (name == PropertyName._start)
		{
			value = VariantUtils.CreateFrom(in _start);
			return true;
		}
		if (name == PropertyName._select)
		{
			value = VariantUtils.CreateFrom(in _select);
			return true;
		}
		if (name == PropertyName.coldDown)
		{
			value = VariantUtils.CreateFrom(in coldDown);
			return true;
		}
		if (name == PropertyName._coldDownOpen)
		{
			value = VariantUtils.CreateFrom(in _coldDownOpen);
			return true;
		}
		if (name == PropertyName.coldDownTimer)
		{
			value = VariantUtils.CreateFrom(in coldDownTimer);
			return true;
		}
		if (name == PropertyName._pressDelayTimer)
		{
			value = VariantUtils.CreateFrom(in _pressDelayTimer);
			return true;
		}
		if (name == PropertyName.setMobileLayout)
		{
			value = VariantUtils.CreateFrom(in setMobileLayout);
			return true;
		}
		if (name == PropertyName.setPcLayout)
		{
			value = VariantUtils.CreateFrom(in setPcLayout);
			return true;
		}
		if (name == PropertyName.canPressPutBack)
		{
			value = VariantUtils.CreateFrom(in canPressPutBack);
			return true;
		}
		if (name == PropertyName.enforceRuntimeAvailabilityOnPress)
		{
			value = VariantUtils.CreateFrom(in enforceRuntimeAvailabilityOnPress);
			return true;
		}
		if (name == PropertyName.allowPressWhenUnavailable)
		{
			value = VariantUtils.CreateFrom(in allowPressWhenUnavailable);
			return true;
		}
		if (name == PropertyName.isMobile)
		{
			value = VariantUtils.CreateFrom(in isMobile);
			return true;
		}
		if (name == PropertyName.pcProgressTexture)
		{
			value = VariantUtils.CreateFrom(in pcProgressTexture);
			return true;
		}
		if (name == PropertyName._aliveTime)
		{
			value = VariantUtils.CreateFrom(in _aliveTime);
			return true;
		}
		if (name == PropertyName.aliveTimer)
		{
			value = VariantUtils.CreateFrom(in aliveTimer);
			return true;
		}
		if (name == PropertyName.blinkTimer)
		{
			value = VariantUtils.CreateFrom(in blinkTimer);
			return true;
		}
		if (name == PropertyName.blink)
		{
			value = VariantUtils.CreateFrom(in blink);
			return true;
		}
		if (name == PropertyName._height)
		{
			value = VariantUtils.CreateFrom(in _height);
			return true;
		}
		if (name == PropertyName.savePos)
		{
			value = VariantUtils.CreateFrom(in savePos);
			return true;
		}
		if (name == PropertyName.originalSaveKey)
		{
			value = VariantUtils.CreateFrom(in originalSaveKey);
			return true;
		}
		if (name == PropertyName.sprite)
		{
			value = VariantUtils.CreateFrom(in sprite);
			return true;
		}
		if (name == PropertyName._livePreviewGeneration)
		{
			value = VariantUtils.CreateFrom(in _livePreviewGeneration);
			return true;
		}
		if (name == PropertyName._previewHovered)
		{
			value = VariantUtils.CreateFrom(in _previewHovered);
			return true;
		}
		if (name == PropertyName._previewCreationDeferred)
		{
			value = VariantUtils.CreateFrom(in _previewCreationDeferred);
			return true;
		}
		if (name == PropertyName.baseItemCost)
		{
			value = VariantUtils.CreateFrom(in baseItemCost);
			return true;
		}
		if (name == PropertyName._itemCostLabelInitialized)
		{
			value = VariantUtils.CreateFrom(in _itemCostLabelInitialized);
			return true;
		}
		if (name == PropertyName._itemCostLabelUsesRiseSuffix)
		{
			value = VariantUtils.CreateFrom(in _itemCostLabelUsesRiseSuffix);
			return true;
		}
		if (name == PropertyName._itemCostLabelValue)
		{
			value = VariantUtils.CreateFrom(in _itemCostLabelValue);
			return true;
		}
		if (name == PropertyName._costChangeFlashTween)
		{
			value = VariantUtils.CreateFrom(in _costChangeFlashTween);
			return true;
		}
		if (name == PropertyName._costChangeFlashBaseScale)
		{
			value = VariantUtils.CreateFrom(in _costChangeFlashBaseScale);
			return true;
		}
		if (name == PropertyName._costChangeFlashActive)
		{
			value = VariantUtils.CreateFrom(in _costChangeFlashActive);
			return true;
		}
		if (name == PropertyName._itemCost)
		{
			value = VariantUtils.CreateFrom(in _itemCost);
			return true;
		}
		if (name == PropertyName.riseCost)
		{
			value = VariantUtils.CreateFrom(in riseCost);
			return true;
		}
		if (name == PropertyName.costMultiple)
		{
			value = VariantUtils.CreateFrom(in costMultiple);
			return true;
		}
		if (name == PropertyName._footprintPlacementPending)
		{
			value = VariantUtils.CreateFrom(in _footprintPlacementPending);
			return true;
		}
		if (name == PropertyName._prophecyBorder)
		{
			value = VariantUtils.CreateFrom(in _prophecyBorder);
			return true;
		}
		if (name == PropertyName._prophecyRemaining)
		{
			value = VariantUtils.CreateFrom(in _prophecyRemaining);
			return true;
		}
		if (name == PropertyName._prophecyPosition)
		{
			value = VariantUtils.CreateFrom(in _prophecyPosition);
			return true;
		}
		if (name == PropertyName._prophecyCharmed)
		{
			value = VariantUtils.CreateFrom(in _prophecyCharmed);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, PropertyName._asyncLifetimeVersion, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._exitedTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._visibilityRefreshQueued, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._queuedVisibilityRefreshVersion, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._centralRuntimeStateRefresh, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._runtimeAvailabilityDirty, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._runtimeCostDirty, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._cachedRuntimeAvailability, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._cachedUpgradePacket, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._modCostQuoteBase, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._modCostQuoteRevision, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._modCostQuote, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._modCostQuoteReady, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._modCostQuoteSuccess, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._resolvingModCostQuote, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._itemCostLabelQuoteValid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._modCooldownPrepared, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._modCooldownPreparedValue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.previewClip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.previewSpriteNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.backgroundTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.selectTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.layout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.itemCostLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.button, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.coldDownProgressBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._cooldownLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._showPacketCooldown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._cooldownConfigOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._cooldownText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._selectionScroll, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._selectionWindow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._scrollGestureActive, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._scrollGestureCancelled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._pendingScrollClick, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._scrollPointerIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._scrollGestureVersion, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._scrollPressPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._scrollPressConfig, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.loveButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.moveComponent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._reserveContainerFootprint, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._config, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasSunAccount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.config, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.showLove, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._showLove, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._syncingLoveButtonFromSave, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.showCost, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._showCost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._onlyDraw, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.onlyDraw, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.alive, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._alive, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.@lock, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._lock, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.plantOnce, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.useCost, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.openShadow, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName._openShadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.start, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._start, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.select, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._select, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.ModUseSequence, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.ModSelectionConsumed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.coldDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.coldDownOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._coldDownOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.coldDownTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.pressDelayTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._pressDelayTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.setMobileLayout, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.setPcLayout, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.canPressPutBack, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.enforceRuntimeAvailabilityOnPress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.allowPressWhenUnavailable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.isMobile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.pcProgressTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.aliveTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._aliveTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.aliveTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.blinkTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.blink, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.height, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._height, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName.savePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.originalSaveKey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.sprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._livePreviewGeneration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._previewHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._previewCreationDeferred, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsPreviewCreationDeferred, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasPreparedPreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.PreparedPreviewClip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.baseItemCost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._itemCostLabelInitialized, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._itemCostLabelUsesRiseSuffix, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._itemCostLabelValue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._costChangeFlashTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._costChangeFlashBaseScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._costChangeFlashActive, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.itemCost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._itemCost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.riseCost, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.costMultiple, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._footprintPlacementPending, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._prophecyBorder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._prophecyRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._prophecyPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._prophecyCharmed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsProphesied, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.ProphecyRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.config, Variant.From<TowerDefensePacketConfig>(config));
		info.AddProperty(PropertyName.showLove, Variant.From<bool>(showLove));
		info.AddProperty(PropertyName.showCost, Variant.From<bool>(showCost));
		info.AddProperty(PropertyName.onlyDraw, Variant.From<bool>(onlyDraw));
		info.AddProperty(PropertyName.alive, Variant.From<bool>(alive));
		info.AddProperty(PropertyName.@lock, Variant.From<bool>(@lock));
		info.AddProperty(PropertyName.openShadow, Variant.From<bool>(openShadow));
		info.AddProperty(PropertyName.start, Variant.From<bool>(start));
		info.AddProperty(PropertyName.select, Variant.From<bool>(select));
		info.AddProperty(PropertyName.ModUseSequence, Variant.From<long>(ModUseSequence));
		info.AddProperty(PropertyName.ModSelectionConsumed, Variant.From<bool>(ModSelectionConsumed));
		info.AddProperty(PropertyName.coldDownOpen, Variant.From<bool>(coldDownOpen));
		info.AddProperty(PropertyName.pressDelayTimer, Variant.From<double>(pressDelayTimer));
		info.AddProperty(PropertyName.aliveTime, Variant.From<double>(aliveTime));
		info.AddProperty(PropertyName.height, Variant.From<double>(height));
		info.AddProperty(PropertyName.itemCost, Variant.From<long>(itemCost));
		info.AddProperty(PropertyName._asyncLifetimeVersion, Variant.From(in _asyncLifetimeVersion));
		info.AddProperty(PropertyName._exitedTree, Variant.From(in _exitedTree));
		info.AddProperty(PropertyName._visibilityRefreshQueued, Variant.From(in _visibilityRefreshQueued));
		info.AddProperty(PropertyName._queuedVisibilityRefreshVersion, Variant.From(in _queuedVisibilityRefreshVersion));
		info.AddProperty(PropertyName._centralRuntimeStateRefresh, Variant.From(in _centralRuntimeStateRefresh));
		info.AddProperty(PropertyName._runtimeAvailabilityDirty, Variant.From(in _runtimeAvailabilityDirty));
		info.AddProperty(PropertyName._runtimeCostDirty, Variant.From(in _runtimeCostDirty));
		info.AddProperty(PropertyName._cachedRuntimeAvailability, Variant.From(in _cachedRuntimeAvailability));
		info.AddProperty(PropertyName._cachedUpgradePacket, Variant.From(in _cachedUpgradePacket));
		info.AddProperty(PropertyName._modCostQuoteBase, Variant.From(in _modCostQuoteBase));
		info.AddProperty(PropertyName._modCostQuoteRevision, Variant.From(in _modCostQuoteRevision));
		info.AddProperty(PropertyName._modCostQuote, Variant.From(in _modCostQuote));
		info.AddProperty(PropertyName._modCostQuoteReady, Variant.From(in _modCostQuoteReady));
		info.AddProperty(PropertyName._modCostQuoteSuccess, Variant.From(in _modCostQuoteSuccess));
		info.AddProperty(PropertyName._resolvingModCostQuote, Variant.From(in _resolvingModCostQuote));
		info.AddProperty(PropertyName._itemCostLabelQuoteValid, Variant.From(in _itemCostLabelQuoteValid));
		info.AddProperty(PropertyName._modCooldownPrepared, Variant.From(in _modCooldownPrepared));
		info.AddProperty(PropertyName._modCooldownPreparedValue, Variant.From(in _modCooldownPreparedValue));
		info.AddProperty(PropertyName.previewClip, Variant.From(in previewClip));
		info.AddProperty(PropertyName.previewSpriteNode, Variant.From(in previewSpriteNode));
		info.AddProperty(PropertyName.backgroundTexture, Variant.From(in backgroundTexture));
		info.AddProperty(PropertyName.selectTexture, Variant.From(in selectTexture));
		info.AddProperty(PropertyName.layout, Variant.From(in layout));
		info.AddProperty(PropertyName.body, Variant.From(in body));
		info.AddProperty(PropertyName.itemCostLabel, Variant.From(in itemCostLabel));
		info.AddProperty(PropertyName.button, Variant.From(in button));
		info.AddProperty(PropertyName.coldDownProgressBar, Variant.From(in coldDownProgressBar));
		info.AddProperty(PropertyName._cooldownLabel, Variant.From(in _cooldownLabel));
		info.AddProperty(PropertyName._showPacketCooldown, Variant.From(in _showPacketCooldown));
		info.AddProperty(PropertyName._cooldownConfigOwner, Variant.From(in _cooldownConfigOwner));
		info.AddProperty(PropertyName._cooldownText, Variant.From(in _cooldownText));
		info.AddProperty(PropertyName._selectionScroll, Variant.From(in _selectionScroll));
		info.AddProperty(PropertyName._selectionWindow, Variant.From(in _selectionWindow));
		info.AddProperty(PropertyName._scrollGestureActive, Variant.From(in _scrollGestureActive));
		info.AddProperty(PropertyName._scrollGestureCancelled, Variant.From(in _scrollGestureCancelled));
		info.AddProperty(PropertyName._pendingScrollClick, Variant.From(in _pendingScrollClick));
		info.AddProperty(PropertyName._scrollPointerIndex, Variant.From(in _scrollPointerIndex));
		info.AddProperty(PropertyName._scrollGestureVersion, Variant.From(in _scrollGestureVersion));
		info.AddProperty(PropertyName._scrollPressPosition, Variant.From(in _scrollPressPosition));
		info.AddProperty(PropertyName._scrollPressConfig, Variant.From(in _scrollPressConfig));
		info.AddProperty(PropertyName.loveButton, Variant.From(in loveButton));
		info.AddProperty(PropertyName.moveComponent, Variant.From(in moveComponent));
		info.AddProperty(PropertyName._reserveContainerFootprint, Variant.From(in _reserveContainerFootprint));
		info.AddProperty(PropertyName._config, Variant.From(in _config));
		info.AddProperty(PropertyName._showLove, Variant.From(in _showLove));
		info.AddProperty(PropertyName._syncingLoveButtonFromSave, Variant.From(in _syncingLoveButtonFromSave));
		info.AddProperty(PropertyName._showCost, Variant.From(in _showCost));
		info.AddProperty(PropertyName._onlyDraw, Variant.From(in _onlyDraw));
		info.AddProperty(PropertyName._alive, Variant.From(in _alive));
		info.AddProperty(PropertyName._lock, Variant.From(in _lock));
		info.AddProperty(PropertyName.plantOnce, Variant.From(in plantOnce));
		info.AddProperty(PropertyName.useCost, Variant.From(in useCost));
		info.AddProperty(PropertyName._openShadow, Variant.From(in _openShadow));
		info.AddProperty(PropertyName._start, Variant.From(in _start));
		info.AddProperty(PropertyName._select, Variant.From(in _select));
		info.AddProperty(PropertyName.coldDown, Variant.From(in coldDown));
		info.AddProperty(PropertyName._coldDownOpen, Variant.From(in _coldDownOpen));
		info.AddProperty(PropertyName.coldDownTimer, Variant.From(in coldDownTimer));
		info.AddProperty(PropertyName._pressDelayTimer, Variant.From(in _pressDelayTimer));
		info.AddProperty(PropertyName.setMobileLayout, Variant.From(in setMobileLayout));
		info.AddProperty(PropertyName.setPcLayout, Variant.From(in setPcLayout));
		info.AddProperty(PropertyName.canPressPutBack, Variant.From(in canPressPutBack));
		info.AddProperty(PropertyName.enforceRuntimeAvailabilityOnPress, Variant.From(in enforceRuntimeAvailabilityOnPress));
		info.AddProperty(PropertyName.allowPressWhenUnavailable, Variant.From(in allowPressWhenUnavailable));
		info.AddProperty(PropertyName.isMobile, Variant.From(in isMobile));
		info.AddProperty(PropertyName.pcProgressTexture, Variant.From(in pcProgressTexture));
		info.AddProperty(PropertyName._aliveTime, Variant.From(in _aliveTime));
		info.AddProperty(PropertyName.aliveTimer, Variant.From(in aliveTimer));
		info.AddProperty(PropertyName.blinkTimer, Variant.From(in blinkTimer));
		info.AddProperty(PropertyName.blink, Variant.From(in blink));
		info.AddProperty(PropertyName._height, Variant.From(in _height));
		info.AddProperty(PropertyName.savePos, Variant.From(in savePos));
		info.AddProperty(PropertyName.originalSaveKey, Variant.From(in originalSaveKey));
		info.AddProperty(PropertyName.sprite, Variant.From(in sprite));
		info.AddProperty(PropertyName._livePreviewGeneration, Variant.From(in _livePreviewGeneration));
		info.AddProperty(PropertyName._previewHovered, Variant.From(in _previewHovered));
		info.AddProperty(PropertyName._previewCreationDeferred, Variant.From(in _previewCreationDeferred));
		info.AddProperty(PropertyName.baseItemCost, Variant.From(in baseItemCost));
		info.AddProperty(PropertyName._itemCostLabelInitialized, Variant.From(in _itemCostLabelInitialized));
		info.AddProperty(PropertyName._itemCostLabelUsesRiseSuffix, Variant.From(in _itemCostLabelUsesRiseSuffix));
		info.AddProperty(PropertyName._itemCostLabelValue, Variant.From(in _itemCostLabelValue));
		info.AddProperty(PropertyName._costChangeFlashTween, Variant.From(in _costChangeFlashTween));
		info.AddProperty(PropertyName._costChangeFlashBaseScale, Variant.From(in _costChangeFlashBaseScale));
		info.AddProperty(PropertyName._costChangeFlashActive, Variant.From(in _costChangeFlashActive));
		info.AddProperty(PropertyName._itemCost, Variant.From(in _itemCost));
		info.AddProperty(PropertyName.riseCost, Variant.From(in riseCost));
		info.AddProperty(PropertyName.costMultiple, Variant.From(in costMultiple));
		info.AddProperty(PropertyName._footprintPlacementPending, Variant.From(in _footprintPlacementPending));
		info.AddProperty(PropertyName._prophecyBorder, Variant.From(in _prophecyBorder));
		info.AddProperty(PropertyName._prophecyRemaining, Variant.From(in _prophecyRemaining));
		info.AddProperty(PropertyName._prophecyPosition, Variant.From(in _prophecyPosition));
		info.AddProperty(PropertyName._prophecyCharmed, Variant.From(in _prophecyCharmed));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.config, out var value))
		{
			config = value.As<TowerDefensePacketConfig>();
		}
		if (info.TryGetProperty(PropertyName.showLove, out var value2))
		{
			showLove = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.showCost, out var value3))
		{
			showCost = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.onlyDraw, out var value4))
		{
			onlyDraw = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.alive, out var value5))
		{
			alive = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.@lock, out var value6))
		{
			@lock = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.openShadow, out var value7))
		{
			openShadow = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.start, out var value8))
		{
			start = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.select, out var value9))
		{
			select = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.ModUseSequence, out var value10))
		{
			ModUseSequence = value10.As<long>();
		}
		if (info.TryGetProperty(PropertyName.ModSelectionConsumed, out var value11))
		{
			ModSelectionConsumed = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.coldDownOpen, out var value12))
		{
			coldDownOpen = value12.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.pressDelayTimer, out var value13))
		{
			pressDelayTimer = value13.As<double>();
		}
		if (info.TryGetProperty(PropertyName.aliveTime, out var value14))
		{
			aliveTime = value14.As<double>();
		}
		if (info.TryGetProperty(PropertyName.height, out var value15))
		{
			height = value15.As<double>();
		}
		if (info.TryGetProperty(PropertyName.itemCost, out var value16))
		{
			itemCost = value16.As<long>();
		}
		if (info.TryGetProperty(PropertyName._asyncLifetimeVersion, out var value17))
		{
			_asyncLifetimeVersion = value17.As<int>();
		}
		if (info.TryGetProperty(PropertyName._exitedTree, out var value18))
		{
			_exitedTree = value18.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._visibilityRefreshQueued, out var value19))
		{
			_visibilityRefreshQueued = value19.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._queuedVisibilityRefreshVersion, out var value20))
		{
			_queuedVisibilityRefreshVersion = value20.As<int>();
		}
		if (info.TryGetProperty(PropertyName._centralRuntimeStateRefresh, out var value21))
		{
			_centralRuntimeStateRefresh = value21.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._runtimeAvailabilityDirty, out var value22))
		{
			_runtimeAvailabilityDirty = value22.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._runtimeCostDirty, out var value23))
		{
			_runtimeCostDirty = value23.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cachedRuntimeAvailability, out var value24))
		{
			_cachedRuntimeAvailability = value24.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cachedUpgradePacket, out var value25))
		{
			_cachedUpgradePacket = value25.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._modCostQuoteBase, out var value26))
		{
			_modCostQuoteBase = value26.As<long>();
		}
		if (info.TryGetProperty(PropertyName._modCostQuoteRevision, out var value27))
		{
			_modCostQuoteRevision = value27.As<long>();
		}
		if (info.TryGetProperty(PropertyName._modCostQuote, out var value28))
		{
			_modCostQuote = value28.As<long>();
		}
		if (info.TryGetProperty(PropertyName._modCostQuoteReady, out var value29))
		{
			_modCostQuoteReady = value29.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._modCostQuoteSuccess, out var value30))
		{
			_modCostQuoteSuccess = value30.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._resolvingModCostQuote, out var value31))
		{
			_resolvingModCostQuote = value31.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._itemCostLabelQuoteValid, out var value32))
		{
			_itemCostLabelQuoteValid = value32.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._modCooldownPrepared, out var value33))
		{
			_modCooldownPrepared = value33.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._modCooldownPreparedValue, out var value34))
		{
			_modCooldownPreparedValue = value34.As<double>();
		}
		if (info.TryGetProperty(PropertyName.previewClip, out var value35))
		{
			previewClip = value35.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.previewSpriteNode, out var value36))
		{
			previewSpriteNode = value36.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.backgroundTexture, out var value37))
		{
			backgroundTexture = value37.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName.selectTexture, out var value38))
		{
			selectTexture = value38.As<NinePatchRect>();
		}
		if (info.TryGetProperty(PropertyName.layout, out var value39))
		{
			layout = value39.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.body, out var value40))
		{
			body = value40.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.itemCostLabel, out var value41))
		{
			itemCostLabel = value41.As<Label>();
		}
		if (info.TryGetProperty(PropertyName.button, out var value42))
		{
			button = value42.As<Button>();
		}
		if (info.TryGetProperty(PropertyName.coldDownProgressBar, out var value43))
		{
			coldDownProgressBar = value43.As<TextureProgressBar>();
		}
		if (info.TryGetProperty(PropertyName._cooldownLabel, out var value44))
		{
			_cooldownLabel = value44.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._showPacketCooldown, out var value45))
		{
			_showPacketCooldown = value45.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cooldownConfigOwner, out var value46))
		{
			_cooldownConfigOwner = value46.As<GameSaveManager>();
		}
		if (info.TryGetProperty(PropertyName._cooldownText, out var value47))
		{
			_cooldownText = value47.As<string>();
		}
		if (info.TryGetProperty(PropertyName._selectionScroll, out var value48))
		{
			_selectionScroll = value48.As<ScrollContainer>();
		}
		if (info.TryGetProperty(PropertyName._selectionWindow, out var value49))
		{
			_selectionWindow = value49.As<Window>();
		}
		if (info.TryGetProperty(PropertyName._scrollGestureActive, out var value50))
		{
			_scrollGestureActive = value50.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._scrollGestureCancelled, out var value51))
		{
			_scrollGestureCancelled = value51.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pendingScrollClick, out var value52))
		{
			_pendingScrollClick = value52.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._scrollPointerIndex, out var value53))
		{
			_scrollPointerIndex = value53.As<int>();
		}
		if (info.TryGetProperty(PropertyName._scrollGestureVersion, out var value54))
		{
			_scrollGestureVersion = value54.As<int>();
		}
		if (info.TryGetProperty(PropertyName._scrollPressPosition, out var value55))
		{
			_scrollPressPosition = value55.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scrollPressConfig, out var value56))
		{
			_scrollPressConfig = value56.As<TowerDefensePacketConfig>();
		}
		if (info.TryGetProperty(PropertyName.loveButton, out var value57))
		{
			loveButton = value57.As<TextureButton>();
		}
		if (info.TryGetProperty(PropertyName.moveComponent, out var value58))
		{
			moveComponent = value58.As<MoveComponent>();
		}
		if (info.TryGetProperty(PropertyName._reserveContainerFootprint, out var value59))
		{
			_reserveContainerFootprint = value59.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._config, out var value60))
		{
			_config = value60.As<TowerDefensePacketConfig>();
		}
		if (info.TryGetProperty(PropertyName._showLove, out var value61))
		{
			_showLove = value61.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._syncingLoveButtonFromSave, out var value62))
		{
			_syncingLoveButtonFromSave = value62.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._showCost, out var value63))
		{
			_showCost = value63.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._onlyDraw, out var value64))
		{
			_onlyDraw = value64.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._alive, out var value65))
		{
			_alive = value65.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._lock, out var value66))
		{
			_lock = value66.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.plantOnce, out var value67))
		{
			plantOnce = value67.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.useCost, out var value68))
		{
			useCost = value68.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._openShadow, out var value69))
		{
			_openShadow = value69.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._start, out var value70))
		{
			_start = value70.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._select, out var value71))
		{
			_select = value71.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.coldDown, out var value72))
		{
			coldDown = value72.As<double>();
		}
		if (info.TryGetProperty(PropertyName._coldDownOpen, out var value73))
		{
			_coldDownOpen = value73.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.coldDownTimer, out var value74))
		{
			coldDownTimer = value74.As<double>();
		}
		if (info.TryGetProperty(PropertyName._pressDelayTimer, out var value75))
		{
			_pressDelayTimer = value75.As<double>();
		}
		if (info.TryGetProperty(PropertyName.setMobileLayout, out var value76))
		{
			setMobileLayout = value76.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.setPcLayout, out var value77))
		{
			setPcLayout = value77.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.canPressPutBack, out var value78))
		{
			canPressPutBack = value78.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.enforceRuntimeAvailabilityOnPress, out var value79))
		{
			enforceRuntimeAvailabilityOnPress = value79.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.allowPressWhenUnavailable, out var value80))
		{
			allowPressWhenUnavailable = value80.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.isMobile, out var value81))
		{
			isMobile = value81.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.pcProgressTexture, out var value82))
		{
			pcProgressTexture = value82.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._aliveTime, out var value83))
		{
			_aliveTime = value83.As<double>();
		}
		if (info.TryGetProperty(PropertyName.aliveTimer, out var value84))
		{
			aliveTimer = value84.As<double>();
		}
		if (info.TryGetProperty(PropertyName.blinkTimer, out var value85))
		{
			blinkTimer = value85.As<double>();
		}
		if (info.TryGetProperty(PropertyName.blink, out var value86))
		{
			blink = value86.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._height, out var value87))
		{
			_height = value87.As<double>();
		}
		if (info.TryGetProperty(PropertyName.savePos, out var value88))
		{
			savePos = value88.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.originalSaveKey, out var value89))
		{
			originalSaveKey = value89.As<string>();
		}
		if (info.TryGetProperty(PropertyName.sprite, out var value90))
		{
			sprite = value90.As<AdobeAnimateSprite>();
		}
		if (info.TryGetProperty(PropertyName._livePreviewGeneration, out var value91))
		{
			_livePreviewGeneration = value91.As<int>();
		}
		if (info.TryGetProperty(PropertyName._previewHovered, out var value92))
		{
			_previewHovered = value92.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._previewCreationDeferred, out var value93))
		{
			_previewCreationDeferred = value93.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.baseItemCost, out var value94))
		{
			baseItemCost = value94.As<long>();
		}
		if (info.TryGetProperty(PropertyName._itemCostLabelInitialized, out var value95))
		{
			_itemCostLabelInitialized = value95.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._itemCostLabelUsesRiseSuffix, out var value96))
		{
			_itemCostLabelUsesRiseSuffix = value96.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._itemCostLabelValue, out var value97))
		{
			_itemCostLabelValue = value97.As<long>();
		}
		if (info.TryGetProperty(PropertyName._costChangeFlashTween, out var value98))
		{
			_costChangeFlashTween = value98.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._costChangeFlashBaseScale, out var value99))
		{
			_costChangeFlashBaseScale = value99.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._costChangeFlashActive, out var value100))
		{
			_costChangeFlashActive = value100.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._itemCost, out var value101))
		{
			_itemCost = value101.As<long>();
		}
		if (info.TryGetProperty(PropertyName.riseCost, out var value102))
		{
			riseCost = value102.As<int>();
		}
		if (info.TryGetProperty(PropertyName.costMultiple, out var value103))
		{
			costMultiple = value103.As<double>();
		}
		if (info.TryGetProperty(PropertyName._footprintPlacementPending, out var value104))
		{
			_footprintPlacementPending = value104.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._prophecyBorder, out var value105))
		{
			_prophecyBorder = value105.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._prophecyRemaining, out var value106))
		{
			_prophecyRemaining = value106.As<double>();
		}
		if (info.TryGetProperty(PropertyName._prophecyPosition, out var value107))
		{
			_prophecyPosition = value107.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._prophecyCharmed, out var value108))
		{
			_prophecyCharmed = value108.As<bool>();
		}
	}
}
