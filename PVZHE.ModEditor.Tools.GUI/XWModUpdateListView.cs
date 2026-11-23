using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;

namespace PVZHE.ModEditor.Tools.GUI;

[ScriptPath("res://addons/ModEditor/Tools/GUI/XWModUpdateListView.cs")]
internal class XWModUpdateListView : VBoxContainer
{
	private sealed class UpdateRow
	{
		internal XWHostUpdateNotice Notice;

		internal PanelContainer Panel;

		internal BoxContainer Header;

		internal Label Name;

		internal Label Identity;

		internal Label Version;

		internal Label Preview;

		internal RichTextLabel Notes;

		internal Button Link;

		internal Button Toggle;

		internal bool Expanded;
	}

	public new class MethodName : VBoxContainer.MethodName
	{
		public static readonly StringName SetAvailableHeight = "SetAvailableHeight";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Refresh = "Refresh";

		public static readonly StringName Retire = "Retire";

		public static readonly StringName QueueLayout = "QueueLayout";

		public static readonly StringName UpdateLayout = "UpdateLayout";

		public static readonly StringName RevealFocusedControl = "RevealFocusedControl";

		public static readonly StringName Text = "Text";
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
		public static readonly StringName FallbackFocus = "FallbackFocus";

		public static readonly StringName StatusTextColor = "StatusTextColor";

		public static readonly StringName _list = "_list";

		public static readonly StringName _scroll = "_scroll";

		public static readonly StringName _summary = "_summary";

		public static readonly StringName _empty = "_empty";

		public static readonly StringName _linkError = "_linkError";

		public static readonly StringName _availableHeight = "_availableHeight";

		public static readonly StringName _layoutQueued = "_layoutQueued";

		public static readonly StringName _exiting = "_exiting";

		public static readonly StringName _revealAfterLayout = "_revealAfterLayout";
	}

	public new class SignalName : VBoxContainer.SignalName
	{
	}

	private readonly Dictionary<string, UpdateRow> _rows = new Dictionary<string, UpdateRow>();

	private VBoxContainer _list;

	private ScrollContainer _scroll;

	private Label _summary;

	private Label _empty;

	private Label _linkError;

	private float _availableHeight = 360f;

	private bool _layoutQueued;

	private bool _exiting;

	private Control _revealAfterLayout;

	internal XWModUpdateInbox Inbox { get; set; } = XWModUpdateInbox.Shared;

	internal Func<string, Error> OpenUrl { get; set; } = OS.ShellOpen;

	internal Control FallbackFocus { get; set; }

	internal Color StatusTextColor { get; set; } = new Color("513015");

	internal event Action ContentSizeChanged;

	internal static Button CreateAction(string name, string text, Action pressed, int width = 180)
	{
		Button button = new Button();
		button.Name = name;
		button.Text = text;
		button.ThemeTypeVariation = "ModSecondaryButton";
		button.CustomMinimumSize = new Vector2(width, 44f);
		button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
		button.SizeFlagsVertical = SizeFlags.ShrinkCenter;
		button.ClipText = true;
		button.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		button.Pressed += delegate
		{
			AudioManager.Instance?.AudioPlay("ButtonPress");
			pressed();
		};
		return button;
	}

	internal void SetAvailableHeight(float height)
	{
		height = Mathf.Max(0f, height);
		if (!Mathf.IsEqualApprox(_availableHeight, height))
		{
			_availableHeight = height;
			QueueLayout();
		}
	}

	public override void _Ready()
	{
		base.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		base.SizeFlagsVertical = SizeFlags.Fill;
		if (base.Theme == null)
		{
			Theme theme = (base.Theme = XWModToolsPanel.CreatePlayerManagementTheme());
		}
		AddThemeConstantOverride("separation", 8);
		_summary = Text("UpdateSummary", "", 16);
		_summary.AddThemeColorOverride("font_color", StatusTextColor);
		_summary.FocusMode = FocusModeEnum.All;
		AddChild(_summary, forceReadableName: false, InternalMode.Disabled);
		_scroll = new ScrollContainer
		{
			Name = "UpdatesScroll",
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			FollowFocus = true,
			ClipContents = true
		};
		AddChild(_scroll, forceReadableName: false, InternalMode.Disabled);
		_list = new VBoxContainer
		{
			Name = "UpdateRows",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		_list.AddThemeConstantOverride("separation", 12);
		_scroll.AddChild(_list, forceReadableName: false, InternalMode.Disabled);
		_empty = Text("NoUpdates", "本次启动尚未收到更新通知。", 20);
		_empty.AddThemeColorOverride("font_color", StatusTextColor);
		_list.AddChild(_empty, forceReadableName: false, InternalMode.Disabled);
		_linkError = Text("UpdateLinkError", "", 16);
		_linkError.AddThemeColorOverride("font_color", StatusTextColor);
		_linkError.Hide();
		AddChild(_linkError, forceReadableName: false, InternalMode.Disabled);
		base.Resized += QueueLayout;
		_list.MinimumSizeChanged += QueueLayout;
		_summary.MinimumSizeChanged += QueueLayout;
		_linkError.MinimumSizeChanged += QueueLayout;
		Inbox.Startup.MarkShown();
		Inbox.Changed += Refresh;
		Refresh();
	}

	public override void _ExitTree()
	{
		_exiting = true;
		Inbox.Changed -= Refresh;
	}

	private void Refresh()
	{
		if (!IsInsideTree() || IsQueuedForDeletion())
		{
			return;
		}
		IReadOnlyList<XWHostUpdateNotice> readOnlyList = Inbox.Snapshot();
		_summary.Text = $"共 {readOnlyList.Count} 项更新";
		_empty.Visible = readOnlyList.Count == 0;
		HashSet<string> keys = new HashSet<string>(readOnlyList.Select(Key));
		Control control = GetViewport().GuiGetFocusOwner();
		bool flag = false;
		Button button = null;
		string[] array = _rows.Keys.Where((string item) => !keys.Contains(item)).ToArray();
		foreach (string key in array)
		{
			flag |= GodotObject.IsInstanceValid(control) && _rows[key].Panel.IsAncestorOf(control);
			Retire(_rows[key].Panel);
			_rows.Remove(key);
		}
		string key2;
		for (int num2 = 0; num2 < readOnlyList.Count; _list.MoveChild(_rows[key2].Panel, num2), num2++)
		{
			XWHostUpdateNotice xWHostUpdateNotice = readOnlyList[num2];
			key2 = Key(xWHostUpdateNotice);
			if (_rows.TryGetValue(key2, out var value) && !(value.Notice != xWHostUpdateNotice))
			{
				continue;
			}
			bool flag2 = value != null && GodotObject.IsInstanceValid(control) && value.Panel.IsAncestorOf(control);
			flag = flag || flag2;
			bool flag3 = flag2 && control == value.Toggle;
			if (value != null)
			{
				Retire(value.Panel);
			}
			UpdateRow updateRow = CreateRow(xWHostUpdateNotice, num2, value?.Expanded ?? false);
			if (flag3 && updateRow.Toggle != null)
			{
				updateRow.Toggle.Show();
			}
			_list.AddChild(updateRow.Panel, forceReadableName: false, InternalMode.Disabled);
			_rows[key2] = updateRow;
			if (!flag2)
			{
				continue;
			}
			Button button2;
			if (flag3)
			{
				Button toggle = updateRow.Toggle;
				if (toggle != null && toggle.Visible)
				{
					button2 = updateRow.Toggle;
					goto IL_025c;
				}
			}
			button2 = updateRow.Link;
			goto IL_025c;
			IL_025c:
			button = button2;
		}
		if (flag)
		{
			Control control2 = button ?? _rows.Values.Select((UpdateRow row) => row.Link ?? row.Toggle).FirstOrDefault((Button button3) => GodotObject.IsInstanceValid(button3) && button3.IsVisibleInTree());
			if (control2 == null)
			{
				control2 = ((GodotObject.IsInstanceValid(FallbackFocus) && FallbackFocus.IsInsideTree()) ? FallbackFocus : _summary);
			}
			control2.GrabFocus();
			if (_list.IsAncestorOf(control2))
			{
				_revealAfterLayout = control2;
			}
		}
		QueueLayout();
	}

	private static string Key(XWHostUpdateNotice notice)
	{
		if (!notice.IsHost)
		{
			return "mod:" + notice.ModId.ToLowerInvariant();
		}
		return "host";
	}

	private void Retire(Control panel)
	{
		_list.RemoveChild(panel);
		panel.QueueFree();
	}

	private UpdateRow CreateRow(XWHostUpdateNotice notice, int index, bool expanded)
	{
		PanelContainer panel = new PanelContainer
		{
			Name = "UpdateItem" + index,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		StyleBoxFlat styleBoxFlat = new StyleBoxFlat
		{
			BgColor = new Color("f1dfba"),
			BorderColor = new Color("aa8458"),
			ContentMarginLeft = 16f,
			ContentMarginRight = 16f,
			ContentMarginTop = 12f,
			ContentMarginBottom = 12f
		};
		styleBoxFlat.SetBorderWidthAll(1);
		panel.AddThemeStyleboxOverride("panel", styleBoxFlat);
		VBoxContainer vBoxContainer = new VBoxContainer
		{
			Name = "Content",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		vBoxContainer.AddThemeConstantOverride("separation", 10);
		panel.AddChild(vBoxContainer, forceReadableName: false, InternalMode.Disabled);
		UpdateRow row = new UpdateRow
		{
			Notice = notice,
			Panel = panel,
			Expanded = expanded,
			Header = new BoxContainer
			{
				Name = "Header",
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			}
		};
		row.Header.AddThemeConstantOverride("separation", 16);
		vBoxContainer.AddChild(row.Header, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer vBoxContainer2 = new VBoxContainer
		{
			Name = "Details",
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		vBoxContainer2.AddThemeConstantOverride("separation", 3);
		row.Header.AddChild(vBoxContainer2, forceReadableName: false, InternalMode.Disabled);
		row.Name = Text("UpdateName", notice.DisplayName, 22);
		row.Name.MaxLinesVisible = 2;
		row.Name.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		row.Name.TooltipText = notice.DisplayName;
		row.Name.MouseFilter = MouseFilterEnum.Pass;
		vBoxContainer2.AddChild(row.Name, forceReadableName: false, InternalMode.Disabled);
		row.Identity = Text("UpdateIdentity", notice.IsHost ? "主程序" : ("Mod · " + notice.ModId), 15);
		row.Identity.AddThemeColorOverride("font_color", new Color("80603d"));
		row.Identity.MaxLinesVisible = 1;
		row.Identity.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		row.Identity.TooltipText = row.Identity.Text;
		row.Identity.MouseFilter = MouseFilterEnum.Pass;
		vBoxContainer2.AddChild(row.Identity, forceReadableName: false, InternalMode.Disabled);
		row.Version = Text("UpdateVersion", notice.CurrentVersion + " → " + notice.LatestVersion, 18);
		vBoxContainer2.AddChild(row.Version, forceReadableName: false, InternalMode.Disabled);
		if (!string.IsNullOrWhiteSpace(notice.Notes))
		{
			row.Preview = Text("NotesPreview", notice.Notes, 18);
			row.Preview.MaxLinesVisible = 3;
			row.Preview.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
			row.Preview.Visible = !expanded;
			vBoxContainer.AddChild(row.Preview, forceReadableName: false, InternalMode.Disabled);
			row.Notes = new RichTextLabel
			{
				Name = "UpdateNotes",
				Text = notice.Notes,
				BbcodeEnabled = false,
				FitContent = true,
				ScrollActive = false,
				SelectionEnabled = true,
				Visible = expanded,
				MouseFilter = MouseFilterEnum.Pass,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			row.Notes.AddThemeColorOverride("default_color", new Color("513015"));
			row.Notes.AddThemeFontSizeOverride("normal_font_size", 18);
			vBoxContainer.AddChild(row.Notes, forceReadableName: false, InternalMode.Disabled);
			row.Toggle = CreateAction("ToggleNotes", expanded ? "收起说明" : "展开说明", delegate
			{
				row.Expanded = !row.Expanded;
				row.Preview.Visible = !row.Expanded;
				row.Notes.Visible = row.Expanded;
				row.Toggle.Text = (row.Expanded ? "收起说明" : "展开说明");
				if (row.Toggle.HasFocus())
				{
					_revealAfterLayout = row.Toggle;
				}
				QueueLayout();
			}, 112);
			row.Toggle.AddThemeFontSizeOverride("font_size", 16);
			row.Toggle.Visible = expanded;
			vBoxContainer.AddChild(row.Toggle, forceReadableName: false, InternalMode.Disabled);
		}
		if (XWModUpdateInbox.IsWebUrl(notice.UpdateUrl))
		{
			row.Link = CreateAction("OpenUpdatePage", "打开更新页面", delegate
			{
				if (IsInsideTree() && !IsQueuedForDeletion() && panel.IsInsideTree() && !panel.IsQueuedForDeletion())
				{
					Error error = OpenUrl(notice.UpdateUrl);
					_linkError.Text = ((error == Error.Ok) ? "" : "无法打开更新页面，请稍后重试。");
					_linkError.Visible = error != Error.Ok;
					QueueLayout();
				}
			});
			row.Link.TooltipText = notice.UpdateUrl;
			row.Header.AddChild(row.Link, forceReadableName: false, InternalMode.Disabled);
		}
		return row;
	}

	private void QueueLayout()
	{
		if (!_exiting && !_layoutQueued && IsInsideTree())
		{
			_layoutQueued = true;
			Callable.From(UpdateLayout).CallDeferred();
		}
	}

	private void UpdateLayout()
	{
		_layoutQueued = false;
		if (_exiting || !IsInsideTree() || IsQueuedForDeletion())
		{
			return;
		}
		float num = Mathf.Max(1f, base.Size.X - _scroll.GetVScrollBar().GetCombinedMinimumSize().X - 6f);
		_list.CustomMaximumSize = new Vector2(num, -1f);
		_empty.CustomMaximumSize = new Vector2(num, -1f);
		Label summary = _summary;
		Vector2 customMaximumSize = (_linkError.CustomMaximumSize = new Vector2(Mathf.Max(1f, base.Size.X), -1f));
		summary.CustomMaximumSize = customMaximumSize;
		foreach (UpdateRow value in _rows.Values)
		{
			value.Header.Vertical = base.Size.X < 560f;
			float num2 = Mathf.Max(1f, num - 32f);
			if (value.Link != null)
			{
				value.Link.CustomMinimumSize = new Vector2(Mathf.Min(180f, num2), 44f);
				int num3 = ((num2 < 180f) ? 16 : 20);
				if (value.Link.GetThemeFontSize("font_size") != num3)
				{
					value.Link.AddThemeFontSizeOverride("font_size", num3);
				}
				Control control = (value.Header.Vertical ? value.Panel.GetNode<Control>("Content") : value.Header);
				if (value.Link.GetParent() != control)
				{
					bool num4 = value.Link.HasFocus();
					value.Link.Reparent(control);
					if (num4)
					{
						value.Link.GrabFocus();
						_revealAfterLayout = value.Link;
					}
				}
			}
			float x = Mathf.Max(1f, num2 - ((!value.Header.Vertical && value.Link != null) ? (value.Link.GetCombinedMinimumSize().X + 16f) : 0f));
			Label name = value.Name;
			Label identity = value.Identity;
			Vector2 vector2 = (value.Version.CustomMaximumSize = new Vector2(x, -1f));
			customMaximumSize = (identity.CustomMaximumSize = vector2);
			name.CustomMaximumSize = customMaximumSize;
			if (value.Preview != null)
			{
				value.Toggle.CustomMinimumSize = new Vector2(Mathf.Min(112f, num2), 44f);
				Label preview = value.Preview;
				customMaximumSize = (value.Notes.CustomMaximumSize = new Vector2(num2, -1f));
				preview.CustomMaximumSize = customMaximumSize;
				bool flag = value.Expanded || value.Preview.GetLineCount() > 3;
				if (!flag && value.Toggle.HasFocus())
				{
					(value.Link ?? FallbackFocus ?? _summary).GrabFocus();
				}
				value.Toggle.Visible = flag;
			}
		}
		float num5 = _summary.GetCombinedMinimumSize().Y + (float)GetThemeConstant("separation");
		if (_linkError.Visible)
		{
			num5 += _linkError.GetCombinedMinimumSize().Y + (float)GetThemeConstant("separation");
		}
		_scroll.CustomMinimumSize = new Vector2(0f, Mathf.Min(_list.GetCombinedMinimumSize().Y, Mathf.Max(0f, _availableHeight - num5)));
		this.ContentSizeChanged?.Invoke();
		if (_revealAfterLayout != null)
		{
			Control revealAfterLayout = _revealAfterLayout;
			_revealAfterLayout = null;
			RevealFocusedControl(revealAfterLayout);
		}
	}

	private async void RevealFocusedControl(Control control)
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if (!_exiting && GodotObject.IsInstanceValid(control) && control.IsInsideTree() && control.HasFocus())
		{
			_scroll.EnsureControlVisible(control);
		}
	}

	private static Label Text(string name, string text, int size)
	{
		Label label = new Label();
		label.Name = name;
		label.Text = text;
		label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		label.MouseFilter = MouseFilterEnum.Ignore;
		label.AddThemeColorOverride("font_color", new Color("513015"));
		label.AddThemeFontSizeOverride("font_size", size);
		return label;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(9)
		{
			new MethodInfo(MethodName.SetAvailableHeight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "height", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Refresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Retire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "panel", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.QueueLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.UpdateLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RevealFocusedControl, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "control", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.Text, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Label"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetAvailableHeight && args.Count == 1)
		{
			SetAvailableHeight(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
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
		if (method == MethodName.Refresh && args.Count == 0)
		{
			Refresh();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Retire && args.Count == 1)
		{
			Retire(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.QueueLayout && args.Count == 0)
		{
			QueueLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateLayout && args.Count == 0)
		{
			UpdateLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RevealFocusedControl && args.Count == 1)
		{
			RevealFocusedControl(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Text && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<Label>(Text(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Text && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<Label>(Text(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetAvailableHeight)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Refresh)
		{
			return true;
		}
		if (method == MethodName.Retire)
		{
			return true;
		}
		if (method == MethodName.QueueLayout)
		{
			return true;
		}
		if (method == MethodName.UpdateLayout)
		{
			return true;
		}
		if (method == MethodName.RevealFocusedControl)
		{
			return true;
		}
		if (method == MethodName.Text)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.FallbackFocus)
		{
			FallbackFocus = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.StatusTextColor)
		{
			StatusTextColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._list)
		{
			_list = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._scroll)
		{
			_scroll = VariantUtils.ConvertTo<ScrollContainer>(in value);
			return true;
		}
		if (name == PropertyName._summary)
		{
			_summary = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._empty)
		{
			_empty = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._linkError)
		{
			_linkError = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._availableHeight)
		{
			_availableHeight = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._layoutQueued)
		{
			_layoutQueued = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._exiting)
		{
			_exiting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._revealAfterLayout)
		{
			_revealAfterLayout = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.FallbackFocus)
		{
			value = VariantUtils.CreateFrom<Control>(FallbackFocus);
			return true;
		}
		if (name == PropertyName.StatusTextColor)
		{
			value = VariantUtils.CreateFrom<Color>(StatusTextColor);
			return true;
		}
		if (name == PropertyName._list)
		{
			value = VariantUtils.CreateFrom(in _list);
			return true;
		}
		if (name == PropertyName._scroll)
		{
			value = VariantUtils.CreateFrom(in _scroll);
			return true;
		}
		if (name == PropertyName._summary)
		{
			value = VariantUtils.CreateFrom(in _summary);
			return true;
		}
		if (name == PropertyName._empty)
		{
			value = VariantUtils.CreateFrom(in _empty);
			return true;
		}
		if (name == PropertyName._linkError)
		{
			value = VariantUtils.CreateFrom(in _linkError);
			return true;
		}
		if (name == PropertyName._availableHeight)
		{
			value = VariantUtils.CreateFrom(in _availableHeight);
			return true;
		}
		if (name == PropertyName._layoutQueued)
		{
			value = VariantUtils.CreateFrom(in _layoutQueued);
			return true;
		}
		if (name == PropertyName._exiting)
		{
			value = VariantUtils.CreateFrom(in _exiting);
			return true;
		}
		if (name == PropertyName._revealAfterLayout)
		{
			value = VariantUtils.CreateFrom(in _revealAfterLayout);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.FallbackFocus, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Color, PropertyName.StatusTextColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._list, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._scroll, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._summary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._empty, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._linkError, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._availableHeight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._layoutQueued, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._exiting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._revealAfterLayout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.FallbackFocus, Variant.From<Control>(FallbackFocus));
		info.AddProperty(PropertyName.StatusTextColor, Variant.From<Color>(StatusTextColor));
		info.AddProperty(PropertyName._list, Variant.From(in _list));
		info.AddProperty(PropertyName._scroll, Variant.From(in _scroll));
		info.AddProperty(PropertyName._summary, Variant.From(in _summary));
		info.AddProperty(PropertyName._empty, Variant.From(in _empty));
		info.AddProperty(PropertyName._linkError, Variant.From(in _linkError));
		info.AddProperty(PropertyName._availableHeight, Variant.From(in _availableHeight));
		info.AddProperty(PropertyName._layoutQueued, Variant.From(in _layoutQueued));
		info.AddProperty(PropertyName._exiting, Variant.From(in _exiting));
		info.AddProperty(PropertyName._revealAfterLayout, Variant.From(in _revealAfterLayout));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.FallbackFocus, out var value))
		{
			FallbackFocus = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.StatusTextColor, out var value2))
		{
			StatusTextColor = value2.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._list, out var value3))
		{
			_list = value3.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._scroll, out var value4))
		{
			_scroll = value4.As<ScrollContainer>();
		}
		if (info.TryGetProperty(PropertyName._summary, out var value5))
		{
			_summary = value5.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._empty, out var value6))
		{
			_empty = value6.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._linkError, out var value7))
		{
			_linkError = value7.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._availableHeight, out var value8))
		{
			_availableHeight = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._layoutQueued, out var value9))
		{
			_layoutQueued = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._exiting, out var value10))
		{
			_exiting = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._revealAfterLayout, out var value11))
		{
			_revealAfterLayout = value11.As<Control>();
		}
	}
}
