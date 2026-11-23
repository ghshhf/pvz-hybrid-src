using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;

[GlobalClass]
[ScriptPath("res://Core/SceneManager/SceneLoadeing/SceneLoading.cs")]
public class SceneLoading : CanvasLayer
{
	public delegate void EnterEventHandler();

	public new class MethodName : CanvasLayer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName PresentationChanged = "PresentationChanged";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName MountView = "MountView";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName SetStatus = "SetStatus";

		public static readonly StringName SetProgress = "SetProgress";

		public static readonly StringName PublishState = "PublishState";

		public static readonly StringName EnterScene = "EnterScene";

		public static readonly StringName Exit = "Exit";
	}

	public new class PropertyName : CanvasLayer.PropertyName
	{
		public static readonly StringName ModPresentationOwner = "ModPresentationOwner";

		public static readonly StringName ActiveView = "ActiveView";

		public static readonly StringName background = "background";

		public static readonly StringName stinky = "stinky";

		public static readonly StringName label = "label";

		public static readonly StringName statusLabel = "statusLabel";

		public static readonly StringName pointNum = "pointNum";

		public static readonly StringName _statusText = "_statusText";

		public static readonly StringName _view = "_view";

		public static readonly StringName _exiting = "_exiting";

		public static readonly StringName _viewOwner = "_viewOwner";
	}

	public new class SignalName : CanvasLayer.SignalName
	{
	}

	public Control background;

	public AdobeAnimateSpriteBase stinky;

	public Label label;

	public Label statusLabel;

	public int pointNum;

	private string _statusText = "正在加载场景";

	private double? _progress;

	private XWModLoadingView _view;

	private bool _exiting;

	private string _viewOwner = "";

	private XWModStartupContext _startup;

	public string ModPresentationOwner => _viewOwner;

	public XWModLoadingView ActiveView => _view;

	public event EnterEventHandler OnEnter;

	public override void _Ready()
	{
		_startup = XWModStartupContext.Current;
		if (_startup != null)
		{
			_startup.PresentationChanged += PresentationChanged;
		}
		MountView(allowMod: true);
		EnterScene();
	}

	private void PresentationChanged()
	{
		if (!_exiting && _viewOwner.Length > 0 && _startup.GetProvider(XWLoadingKind.Transition) != _viewOwner)
		{
			MountView(allowMod: false);
		}
	}

	public override void _ExitTree()
	{
		if (_startup != null)
		{
			_startup.PresentationChanged -= PresentationChanged;
		}
	}

	private void MountView(bool allowMod)
	{
		if (GodotObject.IsInstanceValid(_view))
		{
			if (_view.GetParent() == this)
			{
				RemoveChild(_view);
			}
			_view.QueueFree();
		}
		_view = null;
		_viewOwner = "";
		XWModStartupContext current = XWModStartupContext.Current;
		if (!allowMod || current == null || !current.TryCreateView(XWLoadingKind.Transition, out _view, out _viewOwner))
		{
			_view = GD.Load<PackedScene>("res://Core/SceneManager/SceneLoadeing/SceneLoadingDefaultView.tscn").Instantiate<SceneLoadingDefaultView>(PackedScene.GenEditState.Disabled);
			if (!_view.TryBind(XWLoadingKind.Transition, out var error))
			{
				throw new InvalidOperationException(error);
			}
		}
		_view.Name = "LoadingView";
		_view.ProcessMode = ProcessModeEnum.Always;
		AddChild(_view, forceReadableName: false, InternalMode.Disabled);
		if (_view is SceneLoadingDefaultView sceneLoadingDefaultView)
		{
			background = sceneLoadingDefaultView.background;
			stinky = sceneLoadingDefaultView.stinky;
			label = sceneLoadingDefaultView.label;
			statusLabel = sceneLoadingDefaultView.GetNode<Label>("%StatusLabel");
		}
		else
		{
			background = _view;
			stinky = null;
			label = null;
			statusLabel = _view.GetNodeOrNull<Label>(_view.StatusLabelPath);
		}
		PublishState();
	}

	public override void _Process(double delta)
	{
		if (!_exiting && (!GodotObject.IsInstanceValid(_view) || !_view.IsInsideTree()))
		{
			XWModStartupContext.Current?.DisableView(XWLoadingKind.Transition, "转场视图已意外退出。");
			MountView(allowMod: false);
		}
	}

	public void SetStatus(string status)
	{
		if (!string.IsNullOrWhiteSpace(status))
		{
			_statusText = status;
			PublishState();
		}
	}

	public void SetProgress(double progress)
	{
		_progress = progress;
		PublishState();
	}

	private void PublishState()
	{
		if (GodotObject.IsInstanceValid(_view))
		{
			_view.ApplyState(new XWLoadingState(XWLoadingKind.Transition, _statusText, _progress));
		}
	}

	public async void EnterScene()
	{
		XWModLoadingView initial = _view;
		await initial.PlayMotionAsync(enter: true);
		if (!GodotObject.IsInstanceValid(this) || !IsInsideTree() || _exiting)
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(initial) || !initial.IsInsideTree())
		{
			if (!GodotObject.IsInstanceValid(_view) || !_view.IsInsideTree())
			{
				MountView(allowMod: false);
			}
			await _view.PlayMotionAsync(enter: true);
		}
		if (GodotObject.IsInstanceValid(this) && IsInsideTree() && !_exiting)
		{
			this.OnEnter?.Invoke();
		}
	}

	public async void Exit()
	{
		if (!_exiting)
		{
			_exiting = true;
			if (GodotObject.IsInstanceValid(_view) && _view.IsInsideTree())
			{
				await _view.PlayMotionAsync(enter: false);
			}
			if (GodotObject.IsInstanceValid(this))
			{
				QueueFree();
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(10)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.PresentationChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.MountView, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "allowMod", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetStatus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "status", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetProgress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "progress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PublishState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EnterScene, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Exit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.PresentationChanged && args.Count == 0)
		{
			PresentationChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MountView && args.Count == 1)
		{
			MountView(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetStatus && args.Count == 1)
		{
			SetStatus(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetProgress && args.Count == 1)
		{
			SetProgress(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PublishState && args.Count == 0)
		{
			PublishState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnterScene && args.Count == 0)
		{
			EnterScene();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Exit && args.Count == 0)
		{
			Exit();
			ret = default(godot_variant);
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
		if (method == MethodName.PresentationChanged)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.MountView)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.SetStatus)
		{
			return true;
		}
		if (method == MethodName.SetProgress)
		{
			return true;
		}
		if (method == MethodName.PublishState)
		{
			return true;
		}
		if (method == MethodName.EnterScene)
		{
			return true;
		}
		if (method == MethodName.Exit)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.background)
		{
			background = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.stinky)
		{
			stinky = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName.label)
		{
			label = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName.statusLabel)
		{
			statusLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName.pointNum)
		{
			pointNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._statusText)
		{
			_statusText = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._view)
		{
			_view = VariantUtils.ConvertTo<XWModLoadingView>(in value);
			return true;
		}
		if (name == PropertyName._exiting)
		{
			_exiting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._viewOwner)
		{
			_viewOwner = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ModPresentationOwner)
		{
			value = VariantUtils.CreateFrom<string>(ModPresentationOwner);
			return true;
		}
		if (name == PropertyName.ActiveView)
		{
			value = VariantUtils.CreateFrom<XWModLoadingView>(ActiveView);
			return true;
		}
		if (name == PropertyName.background)
		{
			value = VariantUtils.CreateFrom(in background);
			return true;
		}
		if (name == PropertyName.stinky)
		{
			value = VariantUtils.CreateFrom(in stinky);
			return true;
		}
		if (name == PropertyName.label)
		{
			value = VariantUtils.CreateFrom(in label);
			return true;
		}
		if (name == PropertyName.statusLabel)
		{
			value = VariantUtils.CreateFrom(in statusLabel);
			return true;
		}
		if (name == PropertyName.pointNum)
		{
			value = VariantUtils.CreateFrom(in pointNum);
			return true;
		}
		if (name == PropertyName._statusText)
		{
			value = VariantUtils.CreateFrom(in _statusText);
			return true;
		}
		if (name == PropertyName._view)
		{
			value = VariantUtils.CreateFrom(in _view);
			return true;
		}
		if (name == PropertyName._exiting)
		{
			value = VariantUtils.CreateFrom(in _exiting);
			return true;
		}
		if (name == PropertyName._viewOwner)
		{
			value = VariantUtils.CreateFrom(in _viewOwner);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.stinky, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.statusLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.pointNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._statusText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._view, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._exiting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName._viewOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.ModPresentationOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.ActiveView, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.background, Variant.From(in background));
		info.AddProperty(PropertyName.stinky, Variant.From(in stinky));
		info.AddProperty(PropertyName.label, Variant.From(in label));
		info.AddProperty(PropertyName.statusLabel, Variant.From(in statusLabel));
		info.AddProperty(PropertyName.pointNum, Variant.From(in pointNum));
		info.AddProperty(PropertyName._statusText, Variant.From(in _statusText));
		info.AddProperty(PropertyName._view, Variant.From(in _view));
		info.AddProperty(PropertyName._exiting, Variant.From(in _exiting));
		info.AddProperty(PropertyName._viewOwner, Variant.From(in _viewOwner));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.background, out var value))
		{
			background = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.stinky, out var value2))
		{
			stinky = value2.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName.label, out var value3))
		{
			label = value3.As<Label>();
		}
		if (info.TryGetProperty(PropertyName.statusLabel, out var value4))
		{
			statusLabel = value4.As<Label>();
		}
		if (info.TryGetProperty(PropertyName.pointNum, out var value5))
		{
			pointNum = value5.As<int>();
		}
		if (info.TryGetProperty(PropertyName._statusText, out var value6))
		{
			_statusText = value6.As<string>();
		}
		if (info.TryGetProperty(PropertyName._view, out var value7))
		{
			_view = value7.As<XWModLoadingView>();
		}
		if (info.TryGetProperty(PropertyName._exiting, out var value8))
		{
			_exiting = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._viewOwner, out var value9))
		{
			_viewOwner = value9.As<string>();
		}
	}
}
