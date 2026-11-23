using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;

[ScriptPath("res://Core/DialogManager/XWUpdateNoticeCoordinator.cs")]
internal class XWUpdateNoticeCoordinator : Node
{
	public new class MethodName : Node.MethodName
	{
		public static readonly StringName Attach = "Attach";

		public static readonly StringName Detach = "Detach";

		public static readonly StringName MainMenuEntered = "MainMenuEntered";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName IsStableMainMenu = "IsStableMainMenu";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _menu = "_menu";

		public static readonly StringName _menuEntered = "_menuEntered";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private MainMenu _menu;

	private bool _menuEntered;

	internal void Attach(MainMenu menu)
	{
		_menu = menu;
		_menuEntered = false;
	}

	internal void Detach(MainMenu menu)
	{
		if (_menu == menu)
		{
			_menu = null;
			_menuEntered = false;
		}
	}

	internal void MainMenuEntered(MainMenu menu)
	{
		if (_menu == menu)
		{
			_menuEntered = true;
			XWModUpdateInbox.Shared.Startup.EnterMainMenu();
		}
	}

	public override void _Process(double delta)
	{
		XWModUpdateInbox shared = XWModUpdateInbox.Shared;
		shared.Pump();
		bool menuIdle = _menuEntered && GodotObject.IsInstanceValid(_menu) && _menu.IsInsideTree() && !_menu.IsQueuedForDeletion() && !_menu.wait && !_menu.selectAnimationPlayer.IsPlaying() && IsStableMainMenu(_menu) && GameSaveManager.Instance.GetUserCurrent() != "" && !DialogManager.Instance.HasOpenDialogs;
		if (shared.Startup.TryShowAutomatically(menuIdle, shared.Count))
		{
			DialogManager.Instance.DialogCreate("Updates");
		}
	}

	internal static bool IsStableMainMenu(Control menu)
	{
		if (SceneManager.CurrentScene == "MainMenu" && !SceneManager.Instance.isLoading)
		{
			return menu.CanProcess();
		}
		return false;
	}

	public override void _ExitTree()
	{
		XWModUpdateInbox.Shared.Shutdown();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(6)
		{
			new MethodInfo(MethodName.Attach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "menu", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.Detach, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "menu", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName.MainMenuEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "menu", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsStableMainMenu, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "menu", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
			}, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Attach && args.Count == 1)
		{
			Attach(VariantUtils.ConvertTo<MainMenu>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Detach && args.Count == 1)
		{
			Detach(VariantUtils.ConvertTo<MainMenu>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MainMenuEntered && args.Count == 1)
		{
			MainMenuEntered(VariantUtils.ConvertTo<MainMenu>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsStableMainMenu && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsStableMainMenu(VariantUtils.ConvertTo<Control>(in args[0])));
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
		if (method == MethodName.IsStableMainMenu && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsStableMainMenu(VariantUtils.ConvertTo<Control>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Attach)
		{
			return true;
		}
		if (method == MethodName.Detach)
		{
			return true;
		}
		if (method == MethodName.MainMenuEntered)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.IsStableMainMenu)
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
		if (name == PropertyName._menu)
		{
			_menu = VariantUtils.ConvertTo<MainMenu>(in value);
			return true;
		}
		if (name == PropertyName._menuEntered)
		{
			_menuEntered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._menu)
		{
			value = VariantUtils.CreateFrom(in _menu);
			return true;
		}
		if (name == PropertyName._menuEntered)
		{
			value = VariantUtils.CreateFrom(in _menuEntered);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._menu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._menuEntered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._menu, Variant.From(in _menu));
		info.AddProperty(PropertyName._menuEntered, Variant.From(in _menuEntered));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._menu, out var value))
		{
			_menu = value.As<MainMenu>();
		}
		if (info.TryGetProperty(PropertyName._menuEntered, out var value2))
		{
			_menuEntered = value2.As<bool>();
		}
	}
}
