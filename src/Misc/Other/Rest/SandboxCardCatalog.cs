using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[ScriptPath("res://Scene/Sandbox/SandboxCardCatalog.cs")]
public class SandboxCardCatalog : ScrollContainer
{
	public new class MethodName : ScrollContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName Select = "Select";

		public static readonly StringName RefreshVisible = "RefreshVisible";

		public static readonly StringName CardStyle = "CardStyle";

		public static readonly StringName IgnoreMouse = "IgnoreMouse";
	}

	public new class PropertyName : ScrollContainer.PropertyName
	{
		public static readonly StringName SelectedKey = "SelectedKey";

		public static readonly StringName ItemCount = "ItemCount";

		public static readonly StringName _content = "_content";

		public static readonly StringName _firstRow = "_firstRow";

		public static readonly StringName _lastSize = "_lastSize";
	}

	public new class SignalName : ScrollContainer.SignalName
	{
	}

	private const int Columns = 4;

	private const float RowHeight = 57f;

	private readonly List<(string Key, TowerDefensePacketConfig Packet)> _entries = new List<(string, TowerDefensePacketConfig)>();

	private readonly Dictionary<int, Button> _visible = new Dictionary<int, Button>();

	private Control _content;

	private int _firstRow = -1;

	private Vector2 _lastSize;

	public string SelectedKey { get; private set; } = "PlantPeaShooter";

	public int ItemCount => _entries.Count;

	public event Action<string> Selected;

	public override void _Ready()
	{
		base.HorizontalScrollMode = ScrollMode.Disabled;
		_content = new Control
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		AddChild(_content, forceReadableName: false, InternalMode.Disabled);
	}

	public override void _Process(double delta)
	{
		if (Math.Max(0, (int)((float)base.ScrollVertical / 57f) - 1) != _firstRow || base.Size != _lastSize)
		{
			RefreshVisible();
		}
	}

	public void SetItems(IEnumerable<(string Key, TowerDefensePacketConfig Packet)> entries)
	{
		foreach (Button value in _visible.Values)
		{
			value.QueueFree();
		}
		_visible.Clear();
		_entries.Clear();
		_entries.AddRange(entries);
		base.ScrollVertical = 0;
		_content.CustomMinimumSize = new Vector2(0f, (float)Math.Ceiling((double)_entries.Count / 4.0) * 57f);
		_firstRow = -1;
		RefreshVisible();
	}

	public void Select(string key)
	{
		SelectedKey = key;
		foreach (KeyValuePair<int, Button> item in _visible)
		{
			item.Value.SetPressedNoSignal(_entries[item.Key].Key == key);
		}
		this.Selected?.Invoke(key);
	}

	private void RefreshVisible()
	{
		if (_content == null)
		{
			return;
		}
		_firstRow = Math.Max(0, (int)((float)base.ScrollVertical / 57f) - 1);
		_lastSize = base.Size;
		int num = _firstRow * 4;
		int num2 = Math.Min(_entries.Count, ((int)(((float)base.ScrollVertical + base.Size.Y) / 57f) + 2) * 4);
		foreach (int item in new List<int>(_visible.Keys))
		{
			if (item < num || item >= num2)
			{
				_visible[item].QueueFree();
				_visible.Remove(item);
			}
		}
		float num3 = Math.Max(34f, (base.Size.X - 12f) / 4f);
		for (int i = num; i < num2; i++)
		{
			if (!_visible.TryGetValue(i, out var value))
			{
				(string Key, TowerDefensePacketConfig Packet) entry = _entries[i];
				value = new Button
				{
					Name = entry.Key,
					ToggleMode = true,
					TooltipText = Tr(entry.Packet.name) + "\n" + entry.Key,
					MouseDefaultCursorShape = CursorShape.PointingHand,
					ClipContents = true
				};
				value.AddThemeStyleboxOverride("normal", CardStyle("10271a", "2e472c"));
				value.AddThemeStyleboxOverride("hover", CardStyle("203e24", "889957"));
				value.AddThemeStyleboxOverride("pressed", CardStyle("2f431f", "e6bd5e"));
				value.Pressed += delegate
				{
					Select(entry.Key);
				};
				_content.AddChild(value, forceReadableName: false, InternalMode.Disabled);
				TowerDefenseInGamePacketShow towerDefenseInGamePacketShow = TowerDefenseManager.CreatePacketShow();
				towerDefenseInGamePacketShow.Name = "CardArt";
				towerDefenseInGamePacketShow.setPcLayout = true;
				towerDefenseInGamePacketShow.onlyDraw = true;
				towerDefenseInGamePacketShow.useCost = false;
				towerDefenseInGamePacketShow.enforceRuntimeAvailabilityOnPress = false;
				value.AddChild(towerDefenseInGamePacketShow, forceReadableName: false, InternalMode.Disabled);
				towerDefenseInGamePacketShow.InitVisualPreview(entry.Packet, entry.Packet.characterConfig.cost, -1, 1.0, 0.0);
				towerDefenseInGamePacketShow.Scale = Vector2.One * 0.68f;
				IgnoreMouse(towerDefenseInGamePacketShow);
				_visible[i] = value;
			}
			value.Position = new Vector2((float)(i % 4) * num3, (float)(i / 4) * 57f);
			value.Size = new Vector2(num3 - 2f, 54f);
			value.GetNode<Control>("CardArt").Position = new Vector2((num3 - 2f) * 0.5f, 25f);
			value.SetPressedNoSignal(_entries[i].Key == SelectedKey);
		}
	}

	private static StyleBoxFlat CardStyle(string background, string border)
	{
		StyleBoxFlat styleBoxFlat = new StyleBoxFlat();
		styleBoxFlat.BgColor = new Color(background);
		styleBoxFlat.BorderColor = new Color(border);
		styleBoxFlat.CornerRadiusTopLeft = 3;
		styleBoxFlat.CornerRadiusTopRight = 3;
		styleBoxFlat.CornerRadiusBottomLeft = 3;
		styleBoxFlat.CornerRadiusBottomRight = 3;
		styleBoxFlat.SetBorderWidthAll(1);
		return styleBoxFlat;
	}

	private static void IgnoreMouse(Node node)
	{
		if (node is Control control)
		{
			control.MouseFilter = MouseFilterEnum.Ignore;
		}
		foreach (Node child in node.GetChildren())
		{
			IgnoreMouse(child);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(6)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Select, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "key", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshVisible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CardStyle, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("StyleBoxFlat"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "background", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.String, "border", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IgnoreMouse, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Select && args.Count == 1)
		{
			Select(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshVisible && args.Count == 0)
		{
			RefreshVisible();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CardStyle && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<StyleBoxFlat>(CardStyle(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.IgnoreMouse && args.Count == 1)
		{
			IgnoreMouse(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CardStyle && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<StyleBoxFlat>(CardStyle(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.IgnoreMouse && args.Count == 1)
		{
			IgnoreMouse(VariantUtils.ConvertTo<Node>(in args[0]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.Select)
		{
			return true;
		}
		if (method == MethodName.RefreshVisible)
		{
			return true;
		}
		if (method == MethodName.CardStyle)
		{
			return true;
		}
		if (method == MethodName.IgnoreMouse)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.SelectedKey)
		{
			SelectedKey = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._content)
		{
			_content = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._firstRow)
		{
			_firstRow = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._lastSize)
		{
			_lastSize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SelectedKey)
		{
			value = VariantUtils.CreateFrom<string>(SelectedKey);
			return true;
		}
		if (name == PropertyName.ItemCount)
		{
			value = VariantUtils.CreateFrom<int>(ItemCount);
			return true;
		}
		if (name == PropertyName._content)
		{
			value = VariantUtils.CreateFrom(in _content);
			return true;
		}
		if (name == PropertyName._firstRow)
		{
			value = VariantUtils.CreateFrom(in _firstRow);
			return true;
		}
		if (name == PropertyName._lastSize)
		{
			value = VariantUtils.CreateFrom(in _lastSize);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._content, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._firstRow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._lastSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.String, PropertyName.SelectedKey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.ItemCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.SelectedKey, Variant.From<string>(SelectedKey));
		info.AddProperty(PropertyName._content, Variant.From(in _content));
		info.AddProperty(PropertyName._firstRow, Variant.From(in _firstRow));
		info.AddProperty(PropertyName._lastSize, Variant.From(in _lastSize));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.SelectedKey, out var value))
		{
			SelectedKey = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName._content, out var value2))
		{
			_content = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._firstRow, out var value3))
		{
			_firstRow = value3.As<int>();
		}
		if (info.TryGetProperty(PropertyName._lastSize, out var value4))
		{
			_lastSize = value4.As<Vector2>();
		}
	}
}
