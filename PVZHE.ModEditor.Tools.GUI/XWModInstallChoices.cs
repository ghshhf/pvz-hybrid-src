using System;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using PVZHE.ModEditor.ModSystem;
using PVZHE.ModEditor.ModSystem.Validation;

namespace PVZHE.ModEditor.Tools.GUI;

[ScriptPath("res://addons/ModEditor/Tools/GUI/XWModInstallChoices.cs")]
internal class XWModInstallChoices : VBoxContainer
{
	public new class MethodName : VBoxContainer.MethodName
	{
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
	}

	public new class SignalName : VBoxContainer.SignalName
	{
	}

	internal XWModManager.SelectionPlan Plan { get; private set; }

	internal static XWModInstallChoices MountStartup(DialogBoxChoose dialog, AndroidModInstallIntent.PreparedStartupRequest request)
	{
		dialog.GetNode<Control>("Layer/Control").Hide();
		ColorRect overlay = new ColorRect
		{
			Name = "ModInstallConfirmation",
			Color = new Color(0.15f, 0.08f, 0.02f, 0.58f),
			Theme = XWModToolsPanel.CreatePlayerManagementTheme(),
			MouseFilter = MouseFilterEnum.Stop
		};
		dialog.GetNode<CanvasLayer>("Layer").AddChild(overlay, forceReadableName: false, InternalMode.Disabled);
		overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize);
		VBoxContainer vBoxContainer = XWModToolsPanel.CreatePlayerDialogContent(overlay, "确认" + Operation(request.PreparedImport) + " Mod");
		ScrollContainer scroll = new ScrollContainer
		{
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true
		};
		vBoxContainer.AddChild(scroll, forceReadableName: false, InternalMode.Disabled);
		VBoxContainer body = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		body.AddThemeConstantOverride("separation", 12);
		scroll.AddChild(body, forceReadableName: false, InternalMode.Disabled);
		Label label = new Label
		{
			Text = Summary(request.PreparedImport),
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		body.AddChild(label, forceReadableName: false, InternalMode.Disabled);
		XWModSettingsTextView.BindWidth(label, overlay, () => Mathf.Min(640f, overlay.Size.X - 64f) - 76f);
		HBoxContainer hBoxContainer = new HBoxContainer
		{
			Alignment = AlignmentMode.End
		};
		hBoxContainer.AddThemeConstantOverride("separation", 12);
		vBoxContainer.AddChild(hBoxContainer, forceReadableName: false, InternalMode.Disabled);
		Button button = new Button
		{
			Text = "取消",
			CustomMinimumSize = new Vector2(112f, 48f),
			ThemeTypeVariation = "ModSecondaryButton"
		};
		Button button2 = new Button
		{
			Text = "安装",
			CustomMinimumSize = new Vector2(180f, 48f)
		};
		hBoxContainer.AddChild(button, forceReadableName: false, InternalMode.Disabled);
		hBoxContainer.AddChild(button2, forceReadableName: false, InternalMode.Disabled);
		XWModInstallChoices xWModInstallChoices = new XWModInstallChoices();
		body.AddChild(xWModInstallChoices, forceReadableName: false, InternalMode.Disabled);
		xWModInstallChoices.Configure(request.PreparedImport, button2, scroll, request.SourceAuthority);
		body.MinimumSizeChanged += Resize;
		overlay.Resized += Resize;
		Callable.From(Resize).CallDeferred();
		button2.Pressed += dialog.TrueButtonPressed;
		button.Pressed += dialog.FalseButtonPressed;
		Control[] array = vBoxContainer.FindChildren("*", "BaseButton", recursive: true, owned: false).OfType<Control>().ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			Control obj = array[num];
			NodePath focusNext = (array[num].FocusNeighborBottom = array[num].GetPathTo(array[(num + 1) % array.Length]));
			obj.FocusNext = focusNext;
			Control obj2 = array[num];
			focusNext = (array[num].FocusNeighborTop = array[num].GetPathTo(array[(num + array.Length - 1) % array.Length]));
			obj2.FocusPrevious = focusNext;
		}
		button.GrabFocus();
		return xWModInstallChoices;
		void Resize()
		{
			scroll.CustomMinimumSize = new Vector2(0f, Mathf.Clamp(body.GetCombinedMinimumSize().Y, 64f, Mathf.Max(64f, overlay.Size.Y - 240f)));
		}
	}

	internal static string Operation(XWModPackageInstaller.PreparedImport prepared)
	{
		if (string.IsNullOrEmpty(prepared.ExistingPackagePath))
		{
			return "安装";
		}
		XWModManager.ModEntry modEntry = new XWModManager().ScanMods().Find((XWModManager.ModEntry entry) => entry.Id.Equals(prepared.Manifest.Id, StringComparison.OrdinalIgnoreCase));
		if (modEntry?.Version == prepared.Manifest.Version)
		{
			return "重新安装";
		}
		if (Version.TryParse(modEntry?.Version, out Version result) && Version.TryParse(prepared.Manifest.Version, out Version result2) && result2 < result)
		{
			return "降级";
		}
		return "更新";
	}

	internal static string Summary(XWModPackageInstaller.PreparedImport prepared)
	{
		XWModManifest manifest = prepared.Manifest;
		XWModManager.ModEntry modEntry = new XWModManager().ScanMods().Find((XWModManager.ModEntry entry) => entry.Id.Equals(manifest.Id, StringComparison.OrdinalIgnoreCase));
		return manifest.Name + "\n作者：" + manifest.Author + "\n版本：" + ((modEntry == null) ? manifest.Version : (modEntry.Version + " → " + manifest.Version));
	}

	internal void Configure(XWModPackageInstaller.PreparedImport prepared, Button accept, Control widthSource, string source = "")
	{
		base.Name = "InstallChoices";
		base.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		AddThemeConstantOverride("separation", 12);
		CheckBox enabled = new CheckBox
		{
			Name = "EnableAfterInstall",
			Text = "安装后启用",
			ButtonPressed = XWModPackageInstaller.DefaultEnabled(prepared),
			CustomMinimumSize = new Vector2(0f, 44f)
		};
		AddChild(enabled, forceReadableName: false, InternalMode.Disabled);
		Label preview = AddText("SelectionPreview", "");
		Button detailsButton = new Button
		{
			Name = "InstallDetails",
			Text = "查看详情",
			ToggleMode = true,
			ThemeTypeVariation = "ModSecondaryButton",
			CustomMinimumSize = new Vector2(0f, 44f)
		};
		AddChild(detailsButton, forceReadableName: false, InternalMode.Disabled);
		Label details = AddText("PackageDetails", "ID：" + prepared.Manifest.Id + "\nSHA256：" + prepared.Sha256 + (string.IsNullOrWhiteSpace(source) ? "" : ("\n来源：" + source)));
		details.Visible = false;
		detailsButton.Toggled += delegate(bool show)
		{
			details.Visible = show;
			detailsButton.Text = (show ? "收起详情" : "查看详情");
		};
		string operation = Operation(prepared);
		enabled.Toggled += delegate
		{
			Refresh();
		};
		Refresh();
		Label AddText(string name, string text)
		{
			Label label = new Label
			{
				Name = name,
				Text = text,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			AddChild(label, forceReadableName: false, InternalMode.Disabled);
			XWModSettingsTextView.BindWidth(label, widthSource, () => widthSource.Size.X - 20f);
			return label;
		}
		void Refresh()
		{
			Plan = XWModPackageInstaller.Preview(prepared, enabled.ButtonPressed);
			string text = string.Join("\n", from change in Plan.Changes
				where !change.Id.Equals(prepared.Manifest.Id, StringComparison.OrdinalIgnoreCase)
				select (change.Enabled ? "一并启用" : "一并停用") + "：" + change.Name);
			preview.Text = text + ((text.Length > 0) ? "\n" : "") + ((!Plan.CanApply) ? (string.Join("\n", Plan.Issues.Select((XWValidationIssue issue) => issue.Message)) + "\n可取消勾选，选择仅安装。") : (enabled.ButtonPressed ? "将启用此 Mod 及它需要的已安装 Mod。" : "安装后保持停用，可稍后在管理页启用。"));
			accept.Disabled = !Plan.CanApply;
			accept.Text = operation + (enabled.ButtonPressed ? "并启用" : "并保持停用");
			if (operation == "安装" && !enabled.ButtonPressed)
			{
				accept.Text = "仅安装";
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
