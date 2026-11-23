using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Prefab/GUI/DragMenu/Select/DragMenuSelectItem.cs")]
public class DragMenuSelectItem : Control
{
	public delegate void SelectEventHandler(int index);

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName BeginPressFeedback = "BeginPressFeedback";

		public static readonly StringName ReleasePressFeedback = "ReleasePressFeedback";

		public static readonly StringName OnFeedbackVisibilityChanged = "OnFeedbackVisibilityChanged";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName ResetPressFeedback = "ResetPressFeedback";

		public static readonly StringName OnSpriteTextureChanged = "OnSpriteTextureChanged";

		public static readonly StringName OnButtonPressed = "OnButtonPressed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName textureDisplaySize = "textureDisplaySize";

		public static readonly StringName buttonNode = "buttonNode";

		public static readonly StringName graphics = "graphics";

		public static readonly StringName sprite = "sprite";

		public static readonly StringName button = "button";

		public static readonly StringName index = "index";

		public static readonly StringName @lock = "lock";

		public static readonly StringName _pressFeedbackActive = "_pressFeedbackActive";

		public static readonly StringName _pressFeedbackReleased = "_pressFeedbackReleased";

		public static readonly StringName _pressFeedbackUntil = "_pressFeedbackUntil";

		public static readonly StringName _unpressedModulate = "_unpressedModulate";

		public static readonly StringName _unpressedScale = "_unpressedScale";

		public static readonly StringName _unpressedButtonScale = "_unpressedButtonScale";

		public static readonly StringName _unpressedButtonPosition = "_unpressedButtonPosition";

		public static readonly StringName _feedbackWindow = "_feedbackWindow";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public Control graphics;

	public Sprite2D sprite;

	public Button button;

	public int index = -1;

	public bool @lock;

	private const ulong PressVisibleMilliseconds = 160uL;

	private bool _pressFeedbackActive;

	private bool _pressFeedbackReleased;

	private ulong _pressFeedbackUntil;

	private Color _unpressedModulate;

	private Vector2 _unpressedScale;

	private Vector2 _unpressedButtonScale;

	private Vector2 _unpressedButtonPosition;

	private Window _feedbackWindow;

	[Export(PropertyHint.None, "")]
	public Vector2 textureDisplaySize { get; set; } = Vector2.Zero;

	public Button buttonNode => button;

	public event SelectEventHandler OnSelect;

	public override void _Ready()
	{
		graphics = GetNode<Control>("%Graphics");
		sprite = GetNode<Sprite2D>("%Sprite");
		button = GetNode<Button>("%Button");
		sprite.TextureChanged += OnSpriteTextureChanged;
		button.Pressed += OnButtonPressed;
		button.ButtonDown += BeginPressFeedback;
		button.ButtonUp += ReleasePressFeedback;
		button.MouseExited += ReleasePressFeedback;
		base.VisibilityChanged += OnFeedbackVisibilityChanged;
		SetProcess(enable: false);
	}

	public override void _EnterTree()
	{
		_feedbackWindow = GetWindow();
		_feedbackWindow.FocusExited += ResetPressFeedback;
	}

	public override void _ExitTree()
	{
		ResetPressFeedback();
		if (GodotObject.IsInstanceValid(_feedbackWindow))
		{
			_feedbackWindow.FocusExited -= ResetPressFeedback;
		}
		_feedbackWindow = null;
	}

	private void BeginPressFeedback()
	{
		if (!@lock && !button.Disabled && IsVisibleInTree())
		{
			if (!_pressFeedbackActive)
			{
				_unpressedScale = graphics.Scale;
				_unpressedModulate = graphics.Modulate;
				_unpressedButtonScale = button.Scale;
				_unpressedButtonPosition = button.Position;
			}
			_pressFeedbackActive = true;
			_pressFeedbackReleased = false;
			_pressFeedbackUntil = Time.GetTicksMsec() + 160;
			graphics.Scale = _unpressedScale * 0.94f;
			button.Scale = _unpressedButtonScale / 0.94f;
			button.Position = _unpressedButtonPosition / 0.94f;
			graphics.Modulate = new Color(_unpressedModulate.R * 0.62f, _unpressedModulate.G * 0.62f, _unpressedModulate.B * 0.62f, _unpressedModulate.A);
			SetProcess(enable: true);
		}
	}

	private void ReleasePressFeedback()
	{
		_pressFeedbackReleased = true;
	}

	private void OnFeedbackVisibilityChanged()
	{
		if (!IsVisibleInTree())
		{
			ResetPressFeedback();
		}
	}

	public override void _Process(double delta)
	{
		if (button.Disabled || @lock || (_pressFeedbackReleased && Time.GetTicksMsec() >= _pressFeedbackUntil))
		{
			ResetPressFeedback();
		}
	}

	private void ResetPressFeedback()
	{
		if (_pressFeedbackActive && GodotObject.IsInstanceValid(graphics))
		{
			graphics.Scale = _unpressedScale;
			graphics.Modulate = _unpressedModulate;
			if (GodotObject.IsInstanceValid(button))
			{
				button.Scale = _unpressedButtonScale;
				button.Position = _unpressedButtonPosition;
			}
		}
		_pressFeedbackActive = false;
		_pressFeedbackReleased = false;
		SetProcess(enable: false);
	}

	public void OnSpriteTextureChanged()
	{
		if (GodotObject.IsInstanceValid(sprite.Texture))
		{
			Vector2 size = sprite.Texture.GetSize();
			Vector2 vector = ((textureDisplaySize.X > 0f && textureDisplaySize.Y > 0f) ? textureDisplaySize : size);
			sprite.Scale = vector / size;
			button.Size = vector;
			button.Position = -button.Size / 2f;
		}
	}

	public void OnButtonPressed()
	{
		this.OnSelect?.Invoke(index);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(10)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BeginPressFeedback, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ReleasePressFeedback, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnFeedbackVisibilityChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResetPressFeedback, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnSpriteTextureChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.BeginPressFeedback && args.Count == 0)
		{
			BeginPressFeedback();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleasePressFeedback && args.Count == 0)
		{
			ReleasePressFeedback();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFeedbackVisibilityChanged && args.Count == 0)
		{
			OnFeedbackVisibilityChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetPressFeedback && args.Count == 0)
		{
			ResetPressFeedback();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSpriteTextureChanged && args.Count == 0)
		{
			OnSpriteTextureChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnButtonPressed && args.Count == 0)
		{
			OnButtonPressed();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.BeginPressFeedback)
		{
			return true;
		}
		if (method == MethodName.ReleasePressFeedback)
		{
			return true;
		}
		if (method == MethodName.OnFeedbackVisibilityChanged)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.ResetPressFeedback)
		{
			return true;
		}
		if (method == MethodName.OnSpriteTextureChanged)
		{
			return true;
		}
		if (method == MethodName.OnButtonPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.textureDisplaySize)
		{
			textureDisplaySize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.graphics)
		{
			graphics = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.sprite)
		{
			sprite = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName.button)
		{
			button = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName.index)
		{
			index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.@lock)
		{
			@lock = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pressFeedbackActive)
		{
			_pressFeedbackActive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pressFeedbackReleased)
		{
			_pressFeedbackReleased = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pressFeedbackUntil)
		{
			_pressFeedbackUntil = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._unpressedModulate)
		{
			_unpressedModulate = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._unpressedScale)
		{
			_unpressedScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._unpressedButtonScale)
		{
			_unpressedButtonScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._unpressedButtonPosition)
		{
			_unpressedButtonPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._feedbackWindow)
		{
			_feedbackWindow = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.textureDisplaySize)
		{
			value = VariantUtils.CreateFrom<Vector2>(textureDisplaySize);
			return true;
		}
		if (name == PropertyName.buttonNode)
		{
			value = VariantUtils.CreateFrom<Button>(buttonNode);
			return true;
		}
		if (name == PropertyName.graphics)
		{
			value = VariantUtils.CreateFrom(in graphics);
			return true;
		}
		if (name == PropertyName.sprite)
		{
			value = VariantUtils.CreateFrom(in sprite);
			return true;
		}
		if (name == PropertyName.button)
		{
			value = VariantUtils.CreateFrom(in button);
			return true;
		}
		if (name == PropertyName.index)
		{
			value = VariantUtils.CreateFrom(in index);
			return true;
		}
		if (name == PropertyName.@lock)
		{
			value = VariantUtils.CreateFrom(in @lock);
			return true;
		}
		if (name == PropertyName._pressFeedbackActive)
		{
			value = VariantUtils.CreateFrom(in _pressFeedbackActive);
			return true;
		}
		if (name == PropertyName._pressFeedbackReleased)
		{
			value = VariantUtils.CreateFrom(in _pressFeedbackReleased);
			return true;
		}
		if (name == PropertyName._pressFeedbackUntil)
		{
			value = VariantUtils.CreateFrom(in _pressFeedbackUntil);
			return true;
		}
		if (name == PropertyName._unpressedModulate)
		{
			value = VariantUtils.CreateFrom(in _unpressedModulate);
			return true;
		}
		if (name == PropertyName._unpressedScale)
		{
			value = VariantUtils.CreateFrom(in _unpressedScale);
			return true;
		}
		if (name == PropertyName._unpressedButtonScale)
		{
			value = VariantUtils.CreateFrom(in _unpressedButtonScale);
			return true;
		}
		if (name == PropertyName._unpressedButtonPosition)
		{
			value = VariantUtils.CreateFrom(in _unpressedButtonPosition);
			return true;
		}
		if (name == PropertyName._feedbackWindow)
		{
			value = VariantUtils.CreateFrom(in _feedbackWindow);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.graphics, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.sprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.button, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName.textureDisplaySize, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.@lock, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._pressFeedbackActive, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._pressFeedbackReleased, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._pressFeedbackUntil, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Color, PropertyName._unpressedModulate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._unpressedScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._unpressedButtonScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._unpressedButtonPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._feedbackWindow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.buttonNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.textureDisplaySize, Variant.From<Vector2>(textureDisplaySize));
		info.AddProperty(PropertyName.graphics, Variant.From(in graphics));
		info.AddProperty(PropertyName.sprite, Variant.From(in sprite));
		info.AddProperty(PropertyName.button, Variant.From(in button));
		info.AddProperty(PropertyName.index, Variant.From(in index));
		info.AddProperty(PropertyName.@lock, Variant.From(in @lock));
		info.AddProperty(PropertyName._pressFeedbackActive, Variant.From(in _pressFeedbackActive));
		info.AddProperty(PropertyName._pressFeedbackReleased, Variant.From(in _pressFeedbackReleased));
		info.AddProperty(PropertyName._pressFeedbackUntil, Variant.From(in _pressFeedbackUntil));
		info.AddProperty(PropertyName._unpressedModulate, Variant.From(in _unpressedModulate));
		info.AddProperty(PropertyName._unpressedScale, Variant.From(in _unpressedScale));
		info.AddProperty(PropertyName._unpressedButtonScale, Variant.From(in _unpressedButtonScale));
		info.AddProperty(PropertyName._unpressedButtonPosition, Variant.From(in _unpressedButtonPosition));
		info.AddProperty(PropertyName._feedbackWindow, Variant.From(in _feedbackWindow));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.textureDisplaySize, out var value))
		{
			textureDisplaySize = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.graphics, out var value2))
		{
			graphics = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.sprite, out var value3))
		{
			sprite = value3.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName.button, out var value4))
		{
			button = value4.As<Button>();
		}
		if (info.TryGetProperty(PropertyName.index, out var value5))
		{
			index = value5.As<int>();
		}
		if (info.TryGetProperty(PropertyName.@lock, out var value6))
		{
			@lock = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pressFeedbackActive, out var value7))
		{
			_pressFeedbackActive = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pressFeedbackReleased, out var value8))
		{
			_pressFeedbackReleased = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pressFeedbackUntil, out var value9))
		{
			_pressFeedbackUntil = value9.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._unpressedModulate, out var value10))
		{
			_unpressedModulate = value10.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._unpressedScale, out var value11))
		{
			_unpressedScale = value11.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._unpressedButtonScale, out var value12))
		{
			_unpressedButtonScale = value12.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._unpressedButtonPosition, out var value13))
		{
			_unpressedButtonPosition = value13.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._feedbackWindow, out var value14))
		{
			_feedbackWindow = value14.As<Window>();
		}
	}
}
