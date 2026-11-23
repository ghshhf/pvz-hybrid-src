using System;
using Godot;
using PVZHE.ModEditor.ModSystem;

namespace PVZHE.ModEditor.Tools.GUI;

internal static class XWModSettingsTextView
{
	internal static void BindWidth(Control view, Control widthSource, Func<float> availableWidth)
	{
		widthSource.Resized += Resize;
		view.TreeExiting += delegate
		{
			widthSource.Resized -= Resize;
		};
		Resize();
		void Resize()
		{
			view.CustomMaximumSize = new Vector2(Mathf.Max(1f, availableWidth()), -1f);
		}
	}

	internal static RichTextLabel Create(XWModSettingDefinition definition, Action<string> report, ScrollContainer touchScroll = null)
	{
		RichTextLabel view = new RichTextLabel
		{
			Name = "Content_" + definition.Key,
			BbcodeEnabled = (definition.Type == "richtext"),
			Text = ((definition.Type == "richtext") ? XWModSettingsText.Parse(definition.Content).Bbcode : definition.Content),
			FitContent = true,
			ScrollActive = false,
			SelectionEnabled = (touchScroll == null),
			ContextMenuEnabled = (touchScroll == null),
			FocusMode = Control.FocusModeEnum.All,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			MouseFilter = Control.MouseFilterEnum.Pass,
			MetaUnderlined = true
		};
		bool scrolled = false;
		if (touchScroll != null)
		{
			view.TreeEntered += delegate
			{
				touchScroll.ScrollStarted += ScrollStarted;
				view.GuiInput += ResetGesture;
			};
			view.TreeExiting += delegate
			{
				if (GodotObject.IsInstanceValid(touchScroll))
				{
					touchScroll.ScrollStarted -= ScrollStarted;
				}
				view.GuiInput -= ResetGesture;
			};
		}
		view.MetaHoverStarted += delegate(Variant meta)
		{
			view.TooltipText = meta.AsString();
		};
		view.MetaHoverEnded += delegate
		{
			view.TooltipText = "";
		};
		view.MetaClicked += delegate(Variant meta)
		{
			if (!scrolled)
			{
				if (!XWModSettingsText.TryWebUrl(meta.AsString(), out var url))
				{
					report("无法打开链接：仅支持 HTTP/HTTPS 地址。");
				}
				else
				{
					Error error = OS.ShellOpen(url);
					report((error == Error.Ok) ? "" : $"无法打开链接：{error}。");
				}
			}
		};
		return view;
		void ResetGesture(InputEvent input)
		{
			if (input is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.Pressed)
			{
				scrolled = false;
			}
		}
		void ScrollStarted()
		{
			scrolled = true;
		}
	}
}
