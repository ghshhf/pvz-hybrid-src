using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;
using PVZHE.ModEditor.Tools.GUI;

[GlobalClass]
[ScriptPath("res://Prefab/GUI/DialogBox/DialogBoxGeneral/DialogBoxUpdates/DialogBoxUpdates.cs")]
public class DialogBoxUpdates : DialogPopup
{
	public new class MethodName : DialogPopup.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName QueueResize = "QueueResize";

		public static readonly StringName ResizeWindow = "ResizeWindow";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : DialogPopup.PropertyName
	{
		public static readonly StringName _frame = "_frame";

		public static readonly StringName _content = "_content";

		public static readonly StringName _updates = "_updates";

		public static readonly StringName _titlePlate = "_titlePlate";

		public static readonly StringName _close = "_close";

		public static readonly StringName _resizeQueued = "_resizeQueued";

		public static readonly StringName _exiting = "_exiting";
	}

	public new class SignalName : DialogPopup.SignalName
	{
	}

	private const int ListTopInset = 84;

	private const int ListBottomInset = 116;

	private MarginContainer _frame;

	private VBoxContainer _content;

	private XWModUpdateListView _updates;

	private PanelContainer _titlePlate;

	private Button _close;

	private bool _resizeQueued;

	private bool _exiting;

	public override void _Ready()
	{
		dragControl = null;
		base._Ready();
		textLabel.Hide();
		GetNode<Control>("Layer/Control/ButtonContainer").Hide();
		Control node = GetNode<Control>("Layer/Control");
		node.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		_frame = node.GetNode<MarginContainer>("MarginContainer2");
		_frame.GetNode<MarginContainer>("MarginContainer").Hide();
		Control control = new Control
		{
			Name = "UpdatesLayout",
			MouseFilter = MouseFilterEnum.Ignore,
			Theme = XWModToolsPanel.CreatePlayerManagementTheme()
		};
		_frame.AddChild(control, forceReadableName: false, InternalMode.Disabled);
		_titlePlate = new PanelContainer
		{
			Name = "UpdatesTitlePlate",
			MouseFilter = MouseFilterEnum.Ignore,
			AnchorRight = 1f,
			OffsetLeft = 50f,
			OffsetRight = -50f,
			OffsetTop = 36f,
			OffsetBottom = 72f,
			GrowHorizontal = GrowDirection.Both
		};
		_titlePlate.AddThemeStyleboxOverride("panel", new StyleBoxFlat
		{
			BgColor = new Color("292b3d"),
			BorderColor = new Color("565772"),
			BorderWidthBottom = 1,
			ContentMarginLeft = 16f,
			ContentMarginRight = 16f,
			ContentMarginTop = 4f,
			ContentMarginBottom = 4f
		});
		Label label = new Label
		{
			Name = "UpdatesTitle",
			Text = "检测到更新",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		label.AddThemeFontSizeOverride("font_size", 26);
		label.AddThemeColorOverride("font_color", new Color("f8df9a"));
		_titlePlate.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		control.AddChild(_titlePlate, forceReadableName: false, InternalMode.Disabled);
		_content = new VBoxContainer
		{
			Name = "UpdatesContent",
			MouseFilter = MouseFilterEnum.Ignore
		};
		control.AddChild(_content, forceReadableName: false, InternalMode.Disabled);
		_content.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		_content.OffsetLeft = 40f;
		_content.OffsetRight = -50f;
		_content.OffsetTop = 84f;
		_content.OffsetBottom = -116f;
		_updates = new XWModUpdateListView
		{
			Name = "UpdatesList",
			StatusTextColor = new Color("f8df9a")
		};
		_content.AddChild(_updates, forceReadableName: false, InternalMode.Disabled);
		_close = XWModUpdateListView.CreateAction("CloseUpdates", "稍后再说", base.CloseDialog, 160);
		_updates.FallbackFocus = _close;
		control.AddChild(_close, forceReadableName: false, InternalMode.Disabled);
		_close.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterBottom, LayoutPresetMode.Minsize);
		_close.OffsetTop = -82f;
		_close.OffsetBottom = -38f;
		GetViewport().SizeChanged += QueueResize;
		_updates.ContentSizeChanged += QueueResize;
		_content.MinimumSizeChanged += QueueResize;
		QueueResize();
		_close.GrabFocus();
	}

	private void QueueResize()
	{
		if (!_exiting && !_resizeQueued)
		{
			_resizeQueued = true;
			Callable.From(ResizeWindow).CallDeferred();
		}
	}

	private void ResizeWindow()
	{
		_resizeQueued = false;
		if (!_exiting && IsInsideTree() && !IsQueuedForDeletion())
		{
			Vector2 size = GetViewportRect().Size;
			float num = Mathf.Max(1f, Mathf.Min(760f, size.X - 48f));
			float num2 = Mathf.Max(0f, size.Y - 48f - 42f);
			_content.CustomMinimumSize = new Vector2(Mathf.Max(1f, num - 90f), 0f);
			_content.CustomMaximumSize = new Vector2(Mathf.Max(1f, num - 90f), -1f);
			float num3 = Mathf.Min(160f, _content.CustomMinimumSize.X);
			_close.CustomMinimumSize = new Vector2(num3, 44f);
			_close.OffsetLeft = (0f - num3) / 2f;
			_close.OffsetRight = num3 / 2f;
			_updates.SetAvailableHeight(num2 - 84f - 116f);
			float num4 = Mathf.Min(num2, _content.GetCombinedMinimumSize().Y + 84f + 116f);
			_frame.OffsetLeft = (0f - num) / 2f;
			_frame.OffsetRight = num / 2f;
			_frame.OffsetTop = (42f - num4) / 2f;
			_frame.OffsetBottom = (42f + num4) / 2f;
		}
	}

	public override void _ExitTree()
	{
		_exiting = true;
		GetViewport().SizeChanged -= QueueResize;
		if (_updates != null)
		{
			_updates.ContentSizeChanged -= QueueResize;
		}
		foreach (XWHostUpdateNotice item in XWModUpdateInbox.Shared.Snapshot())
		{
			if (item.IsHost)
			{
				Global.Instance.newVersionSkip = true;
			}
		}
		base._ExitTree();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(4)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.QueueResize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ResizeWindow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName.QueueResize && args.Count == 0)
		{
			QueueResize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResizeWindow && args.Count == 0)
		{
			ResizeWindow();
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
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.QueueResize)
		{
			return true;
		}
		if (method == MethodName.ResizeWindow)
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
		if (name == PropertyName._frame)
		{
			_frame = VariantUtils.ConvertTo<MarginContainer>(in value);
			return true;
		}
		if (name == PropertyName._content)
		{
			_content = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._updates)
		{
			_updates = VariantUtils.ConvertTo<XWModUpdateListView>(in value);
			return true;
		}
		if (name == PropertyName._titlePlate)
		{
			_titlePlate = VariantUtils.ConvertTo<PanelContainer>(in value);
			return true;
		}
		if (name == PropertyName._close)
		{
			_close = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._resizeQueued)
		{
			_resizeQueued = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._exiting)
		{
			_exiting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._frame)
		{
			value = VariantUtils.CreateFrom(in _frame);
			return true;
		}
		if (name == PropertyName._content)
		{
			value = VariantUtils.CreateFrom(in _content);
			return true;
		}
		if (name == PropertyName._updates)
		{
			value = VariantUtils.CreateFrom(in _updates);
			return true;
		}
		if (name == PropertyName._titlePlate)
		{
			value = VariantUtils.CreateFrom(in _titlePlate);
			return true;
		}
		if (name == PropertyName._close)
		{
			value = VariantUtils.CreateFrom(in _close);
			return true;
		}
		if (name == PropertyName._resizeQueued)
		{
			value = VariantUtils.CreateFrom(in _resizeQueued);
			return true;
		}
		if (name == PropertyName._exiting)
		{
			value = VariantUtils.CreateFrom(in _exiting);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._frame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._content, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._updates, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._titlePlate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._close, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._resizeQueued, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._exiting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._frame, Variant.From(in _frame));
		info.AddProperty(PropertyName._content, Variant.From(in _content));
		info.AddProperty(PropertyName._updates, Variant.From(in _updates));
		info.AddProperty(PropertyName._titlePlate, Variant.From(in _titlePlate));
		info.AddProperty(PropertyName._close, Variant.From(in _close));
		info.AddProperty(PropertyName._resizeQueued, Variant.From(in _resizeQueued));
		info.AddProperty(PropertyName._exiting, Variant.From(in _exiting));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._frame, out var value))
		{
			_frame = value.As<MarginContainer>();
		}
		if (info.TryGetProperty(PropertyName._content, out var value2))
		{
			_content = value2.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._updates, out var value3))
		{
			_updates = value3.As<XWModUpdateListView>();
		}
		if (info.TryGetProperty(PropertyName._titlePlate, out var value4))
		{
			_titlePlate = value4.As<PanelContainer>();
		}
		if (info.TryGetProperty(PropertyName._close, out var value5))
		{
			_close = value5.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._resizeQueued, out var value6))
		{
			_resizeQueued = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._exiting, out var value7))
		{
			_exiting = value7.As<bool>();
		}
	}
}
