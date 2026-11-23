using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace PVZHE.ModEditor.ModSystem;

[GlobalClass]
[ScriptPath("res://addons/ModEditor/ModSystem/XWModLoadingView.cs")]
public class XWModLoadingView : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName ActivatePresentation = "ActivatePresentation";

		public static readonly StringName BlocksStart = "BlocksStart";

		public static readonly StringName ShowCacheCleared = "ShowCacheCleared";

		public static readonly StringName SetButtonText = "SetButtonText";

		public static readonly StringName OfflinePressed = "OfflinePressed";

		public static readonly StringName OnlinePressed = "OnlinePressed";

		public static readonly StringName ClearCachePressed = "ClearCachePressed";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName StatusLabelPath = "StatusLabelPath";

		public static readonly StringName ProgressBarPath = "ProgressBarPath";

		public static readonly StringName StartButtonPath = "StartButtonPath";

		public static readonly StringName ClearCacheButtonPath = "ClearCacheButtonPath";

		public static readonly StringName OnlineButtonPath = "OnlineButtonPath";

		public static readonly StringName OfflineButtonPath = "OfflineButtonPath";

		public static readonly StringName AnimationPlayerPath = "AnimationPlayerPath";

		public static readonly StringName EnterAnimation = "EnterAnimation";

		public static readonly StringName ExitAnimation = "ExitAnimation";

		public static readonly StringName PresentationComplete = "PresentationComplete";

		public static readonly StringName StatusLabel = "StatusLabel";

		public static readonly StringName Progress = "Progress";

		public static readonly StringName StartButton = "StartButton";

		public static readonly StringName ClearCacheButton = "ClearCacheButton";

		public static readonly StringName _onlineButton = "_onlineButton";

		public static readonly StringName _offlineButton = "_offlineButton";

		public static readonly StringName _animations = "_animations";

		public static readonly StringName _bound = "_bound";

		public static readonly StringName _cacheCleared = "_cacheCleared";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public const string ScriptPath = "res://addons/ModEditor/ModSystem/XWModLoadingView.cs";

	protected Label StatusLabel;

	protected Godot.Range Progress;

	protected BaseButton StartButton;

	protected BaseButton ClearCacheButton;

	private BaseButton _onlineButton;

	private BaseButton _offlineButton;

	private AnimationPlayer _animations;

	private bool _bound;

	private bool _cacheCleared;

	[Export(PropertyHint.None, "")]
	public NodePath StatusLabelPath { get; set; } = "%StatusLabel";

	[Export(PropertyHint.None, "")]
	public NodePath ProgressBarPath { get; set; } = "%ProgressBar";

	[Export(PropertyHint.None, "")]
	public NodePath StartButtonPath { get; set; } = "%StartButton";

	[Export(PropertyHint.None, "")]
	public NodePath ClearCacheButtonPath { get; set; } = "%ClearCacheButton";

	[Export(PropertyHint.None, "")]
	public NodePath OnlineButtonPath { get; set; } = "";

	[Export(PropertyHint.None, "")]
	public NodePath OfflineButtonPath { get; set; } = "";

	[Export(PropertyHint.None, "")]
	public NodePath AnimationPlayerPath { get; set; } = "";

	[Export(PropertyHint.None, "")]
	public string EnterAnimation { get; set; } = "Enter";

	[Export(PropertyHint.None, "")]
	public string ExitAnimation { get; set; } = "Exit";

	public XWLoadingState State { get; private set; } = new XWLoadingState(XWLoadingKind.Startup, "");

	public virtual bool PresentationComplete => true;

	public event Action<bool> StartRequested;

	public event Action ClearCacheRequested;

	public bool TryBind(XWLoadingKind kind, out string error)
	{
		error = "";
		StatusLabel = Find<Label>(StatusLabelPath);
		Progress = Find<Godot.Range>(ProgressBarPath);
		StartButton = Find<BaseButton>(StartButtonPath);
		ClearCacheButton = Find<BaseButton>(ClearCacheButtonPath);
		_onlineButton = Find<BaseButton>(OnlineButtonPath);
		_offlineButton = Find<BaseButton>(OfflineButtonPath);
		_animations = Find<AnimationPlayer>(AnimationPlayerPath);
		if (StatusLabel == null || (kind == XWLoadingKind.Startup && (StartButton == null || ClearCacheButton == null)))
		{
			error = "加载界面必须绑定 StatusLabel；启动界面还须绑定 StartButton 和 ClearCacheButton。";
			return false;
		}
		if (!_bound)
		{
			if (StartButton != null)
			{
				StartButton.Pressed += OfflinePressed;
			}
			if (ClearCacheButton != null)
			{
				ClearCacheButton.Pressed += ClearCachePressed;
			}
			if (_onlineButton != null)
			{
				_onlineButton.Pressed += OnlinePressed;
			}
			if (_offlineButton != null)
			{
				_offlineButton.Pressed += OfflinePressed;
			}
			_bound = true;
		}
		ApplyState(new XWLoadingState(kind, "正在准备加载"));
		return true;
	}

	protected T Find<T>(NodePath path) where T : Node
	{
		if (!(path == null) && !path.IsEmpty)
		{
			return GetNodeOrNull<T>(path);
		}
		return null;
	}

	public virtual void ActivatePresentation()
	{
	}

	public virtual void ApplyState(XWLoadingState state)
	{
		State = state;
		if (GodotObject.IsInstanceValid(StatusLabel))
		{
			StatusLabel.Text = ((state.Error.Length > 0) ? (Tr("LOADING_FAILED") + ": " + state.Error) : state.Status);
		}
		if (GodotObject.IsInstanceValid(Progress))
		{
			double? progress = state.Progress;
			if (progress.HasValue)
			{
				double valueOrDefault = progress.GetValueOrDefault();
				Progress.Value = Progress.MinValue + Math.Clamp(valueOrDefault, 0.0, 1.0) * (Progress.MaxValue - Progress.MinValue);
			}
		}
		BaseButton[] array = new BaseButton[3] { StartButton, _onlineButton, _offlineButton };
		foreach (BaseButton baseButton in array)
		{
			if (GodotObject.IsInstanceValid(baseButton))
			{
				baseButton.Disabled = !state.CanStart;
			}
		}
		if (GodotObject.IsInstanceValid(StartButton))
		{
			SetButtonText(StartButton, Tr((state.Error.Length > 0) ? "LOADING_FAILED" : (state.CanStart ? "CLICK_TO_START" : "LOADING_IN_PROGRESS")));
		}
		if (GodotObject.IsInstanceValid(ClearCacheButton))
		{
			ClearCacheButton.Visible = state.CanClearCache;
			ClearCacheButton.Disabled = !state.CanClearCache;
			SetButtonText(ClearCacheButton, Tr(_cacheCleared ? "CLEAR_LOADING_CACHE_DONE" : "CLEAR_LOADING_CACHE"));
		}
	}

	public bool BlocksStart(InputEvent input)
	{
		if (!GodotObject.IsInstanceValid(ClearCacheButton) || !ClearCacheButton.Visible || ClearCacheButton.Disabled)
		{
			return false;
		}
		if (!ClearCacheButton.HasFocus())
		{
			if (input is InputEventMouseButton { Pressed: not false } inputEventMouseButton)
			{
				return ClearCacheButton.GetGlobalRect().HasPoint(inputEventMouseButton.Position);
			}
			return false;
		}
		return true;
	}

	public void ShowCacheCleared()
	{
		_cacheCleared = true;
		if (GodotObject.IsInstanceValid(ClearCacheButton))
		{
			SetButtonText(ClearCacheButton, Tr("CLEAR_LOADING_CACHE_DONE"));
		}
	}

	protected static void SetButtonText(BaseButton button, string text)
	{
		if (button is Button button2)
		{
			button2.Text = text;
		}
		else if (button is MainButton mainButton)
		{
			mainButton.text = text;
		}
	}

	public virtual async Task<bool> PlayMotionAsync(bool enter)
	{
		string text = (enter ? EnterAnimation : ExitAnimation);
		if (!GodotObject.IsInstanceValid(_animations) || string.IsNullOrEmpty(text) || !_animations.HasAnimation(text))
		{
			return true;
		}
		_animations.Play(text);
		ulong deadline = Time.GetTicksMsec() + 3000;
		SceneTree tree = GetTree();
		while (GodotObject.IsInstanceValid(this) && IsInsideTree() && GodotObject.IsInstanceValid(_animations) && _animations.IsPlaying())
		{
			if (Time.GetTicksMsec() >= deadline)
			{
				_animations.Stop();
				GD.PushWarning("[ModLoading] 界面" + (enter ? "进场" : "退场") + "动画超过 3 秒，已结束演出。");
				return false;
			}
			await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		}
		return GodotObject.IsInstanceValid(this) && IsInsideTree();
	}

	private void OfflinePressed()
	{
		if (State.CanStart)
		{
			this.StartRequested?.Invoke(obj: false);
		}
	}

	private void OnlinePressed()
	{
		if (State.CanStart)
		{
			this.StartRequested?.Invoke(obj: true);
		}
	}

	private void ClearCachePressed()
	{
		if (State.CanClearCache)
		{
			this.ClearCacheRequested?.Invoke();
		}
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(StartButton))
		{
			StartButton.Pressed -= OfflinePressed;
		}
		if (GodotObject.IsInstanceValid(ClearCacheButton))
		{
			ClearCacheButton.Pressed -= ClearCachePressed;
		}
		if (GodotObject.IsInstanceValid(_onlineButton))
		{
			_onlineButton.Pressed -= OnlinePressed;
		}
		if (GodotObject.IsInstanceValid(_offlineButton))
		{
			_offlineButton.Pressed -= OfflinePressed;
		}
		_bound = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(8)
		{
			new MethodInfo(MethodName.ActivatePresentation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BlocksStart, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName.ShowCacheCleared, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetButtonText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("BaseButton"), exported: false),
				new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OfflinePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnlinePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ClearCachePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ActivatePresentation && args.Count == 0)
		{
			ActivatePresentation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BlocksStart && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(BlocksStart(VariantUtils.ConvertTo<InputEvent>(in args[0])));
			return true;
		}
		if (method == MethodName.ShowCacheCleared && args.Count == 0)
		{
			ShowCacheCleared();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetButtonText && args.Count == 2)
		{
			SetButtonText(VariantUtils.ConvertTo<BaseButton>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OfflinePressed && args.Count == 0)
		{
			OfflinePressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnlinePressed && args.Count == 0)
		{
			OnlinePressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearCachePressed && args.Count == 0)
		{
			ClearCachePressed();
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
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetButtonText && args.Count == 2)
		{
			SetButtonText(VariantUtils.ConvertTo<BaseButton>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.ActivatePresentation)
		{
			return true;
		}
		if (method == MethodName.BlocksStart)
		{
			return true;
		}
		if (method == MethodName.ShowCacheCleared)
		{
			return true;
		}
		if (method == MethodName.SetButtonText)
		{
			return true;
		}
		if (method == MethodName.OfflinePressed)
		{
			return true;
		}
		if (method == MethodName.OnlinePressed)
		{
			return true;
		}
		if (method == MethodName.ClearCachePressed)
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
		if (name == PropertyName.StatusLabelPath)
		{
			StatusLabelPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.ProgressBarPath)
		{
			ProgressBarPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.StartButtonPath)
		{
			StartButtonPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.ClearCacheButtonPath)
		{
			ClearCacheButtonPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.OnlineButtonPath)
		{
			OnlineButtonPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.OfflineButtonPath)
		{
			OfflineButtonPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.AnimationPlayerPath)
		{
			AnimationPlayerPath = VariantUtils.ConvertTo<NodePath>(in value);
			return true;
		}
		if (name == PropertyName.EnterAnimation)
		{
			EnterAnimation = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.ExitAnimation)
		{
			ExitAnimation = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.StatusLabel)
		{
			StatusLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName.Progress)
		{
			Progress = VariantUtils.ConvertTo<Godot.Range>(in value);
			return true;
		}
		if (name == PropertyName.StartButton)
		{
			StartButton = VariantUtils.ConvertTo<BaseButton>(in value);
			return true;
		}
		if (name == PropertyName.ClearCacheButton)
		{
			ClearCacheButton = VariantUtils.ConvertTo<BaseButton>(in value);
			return true;
		}
		if (name == PropertyName._onlineButton)
		{
			_onlineButton = VariantUtils.ConvertTo<BaseButton>(in value);
			return true;
		}
		if (name == PropertyName._offlineButton)
		{
			_offlineButton = VariantUtils.ConvertTo<BaseButton>(in value);
			return true;
		}
		if (name == PropertyName._animations)
		{
			_animations = VariantUtils.ConvertTo<AnimationPlayer>(in value);
			return true;
		}
		if (name == PropertyName._bound)
		{
			_bound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cacheCleared)
		{
			_cacheCleared = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		NodePath from;
		if (name == PropertyName.StatusLabelPath)
		{
			from = StatusLabelPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ProgressBarPath)
		{
			from = ProgressBarPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.StartButtonPath)
		{
			from = StartButtonPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ClearCacheButtonPath)
		{
			from = ClearCacheButtonPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.OnlineButtonPath)
		{
			from = OnlineButtonPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.OfflineButtonPath)
		{
			from = OfflineButtonPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.AnimationPlayerPath)
		{
			from = AnimationPlayerPath;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		string from2;
		if (name == PropertyName.EnterAnimation)
		{
			from2 = EnterAnimation;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.ExitAnimation)
		{
			from2 = ExitAnimation;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.PresentationComplete)
		{
			value = VariantUtils.CreateFrom<bool>(PresentationComplete);
			return true;
		}
		if (name == PropertyName.StatusLabel)
		{
			value = VariantUtils.CreateFrom(in StatusLabel);
			return true;
		}
		if (name == PropertyName.Progress)
		{
			value = VariantUtils.CreateFrom(in Progress);
			return true;
		}
		if (name == PropertyName.StartButton)
		{
			value = VariantUtils.CreateFrom(in StartButton);
			return true;
		}
		if (name == PropertyName.ClearCacheButton)
		{
			value = VariantUtils.CreateFrom(in ClearCacheButton);
			return true;
		}
		if (name == PropertyName._onlineButton)
		{
			value = VariantUtils.CreateFrom(in _onlineButton);
			return true;
		}
		if (name == PropertyName._offlineButton)
		{
			value = VariantUtils.CreateFrom(in _offlineButton);
			return true;
		}
		if (name == PropertyName._animations)
		{
			value = VariantUtils.CreateFrom(in _animations);
			return true;
		}
		if (name == PropertyName._bound)
		{
			value = VariantUtils.CreateFrom(in _bound);
			return true;
		}
		if (name == PropertyName._cacheCleared)
		{
			value = VariantUtils.CreateFrom(in _cacheCleared);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.NodePath, PropertyName.StatusLabelPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.NodePath, PropertyName.ProgressBarPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.NodePath, PropertyName.StartButtonPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.NodePath, PropertyName.ClearCacheButtonPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.NodePath, PropertyName.OnlineButtonPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.NodePath, PropertyName.OfflineButtonPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.NodePath, PropertyName.AnimationPlayerPath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.EnterAnimation, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.String, PropertyName.ExitAnimation, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Bool, PropertyName.PresentationComplete, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.StatusLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.Progress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.StartButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.ClearCacheButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._onlineButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._offlineButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._animations, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._bound, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._cacheCleared, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.StatusLabelPath, Variant.From<NodePath>(StatusLabelPath));
		info.AddProperty(PropertyName.ProgressBarPath, Variant.From<NodePath>(ProgressBarPath));
		info.AddProperty(PropertyName.StartButtonPath, Variant.From<NodePath>(StartButtonPath));
		info.AddProperty(PropertyName.ClearCacheButtonPath, Variant.From<NodePath>(ClearCacheButtonPath));
		info.AddProperty(PropertyName.OnlineButtonPath, Variant.From<NodePath>(OnlineButtonPath));
		info.AddProperty(PropertyName.OfflineButtonPath, Variant.From<NodePath>(OfflineButtonPath));
		info.AddProperty(PropertyName.AnimationPlayerPath, Variant.From<NodePath>(AnimationPlayerPath));
		info.AddProperty(PropertyName.EnterAnimation, Variant.From<string>(EnterAnimation));
		info.AddProperty(PropertyName.ExitAnimation, Variant.From<string>(ExitAnimation));
		info.AddProperty(PropertyName.StatusLabel, Variant.From(in StatusLabel));
		info.AddProperty(PropertyName.Progress, Variant.From(in Progress));
		info.AddProperty(PropertyName.StartButton, Variant.From(in StartButton));
		info.AddProperty(PropertyName.ClearCacheButton, Variant.From(in ClearCacheButton));
		info.AddProperty(PropertyName._onlineButton, Variant.From(in _onlineButton));
		info.AddProperty(PropertyName._offlineButton, Variant.From(in _offlineButton));
		info.AddProperty(PropertyName._animations, Variant.From(in _animations));
		info.AddProperty(PropertyName._bound, Variant.From(in _bound));
		info.AddProperty(PropertyName._cacheCleared, Variant.From(in _cacheCleared));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.StatusLabelPath, out var value))
		{
			StatusLabelPath = value.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.ProgressBarPath, out var value2))
		{
			ProgressBarPath = value2.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.StartButtonPath, out var value3))
		{
			StartButtonPath = value3.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.ClearCacheButtonPath, out var value4))
		{
			ClearCacheButtonPath = value4.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.OnlineButtonPath, out var value5))
		{
			OnlineButtonPath = value5.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.OfflineButtonPath, out var value6))
		{
			OfflineButtonPath = value6.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.AnimationPlayerPath, out var value7))
		{
			AnimationPlayerPath = value7.As<NodePath>();
		}
		if (info.TryGetProperty(PropertyName.EnterAnimation, out var value8))
		{
			EnterAnimation = value8.As<string>();
		}
		if (info.TryGetProperty(PropertyName.ExitAnimation, out var value9))
		{
			ExitAnimation = value9.As<string>();
		}
		if (info.TryGetProperty(PropertyName.StatusLabel, out var value10))
		{
			StatusLabel = value10.As<Label>();
		}
		if (info.TryGetProperty(PropertyName.Progress, out var value11))
		{
			Progress = value11.As<Godot.Range>();
		}
		if (info.TryGetProperty(PropertyName.StartButton, out var value12))
		{
			StartButton = value12.As<BaseButton>();
		}
		if (info.TryGetProperty(PropertyName.ClearCacheButton, out var value13))
		{
			ClearCacheButton = value13.As<BaseButton>();
		}
		if (info.TryGetProperty(PropertyName._onlineButton, out var value14))
		{
			_onlineButton = value14.As<BaseButton>();
		}
		if (info.TryGetProperty(PropertyName._offlineButton, out var value15))
		{
			_offlineButton = value15.As<BaseButton>();
		}
		if (info.TryGetProperty(PropertyName._animations, out var value16))
		{
			_animations = value16.As<AnimationPlayer>();
		}
		if (info.TryGetProperty(PropertyName._bound, out var value17))
		{
			_bound = value17.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cacheCleared, out var value18))
		{
			_cacheCleared = value18.As<bool>();
		}
	}
}
