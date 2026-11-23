using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Zombie/Chapter2/Polestriker/Sprite/FireSnowPeaG/ZombiePolestrikerFireSnowPeaGSprite.cs")]
public class ZombiePolestrikerFireSnowPeaGSprite : AdobeAnimateSpriteBase
{
	public new class MethodName : AdobeAnimateSpriteBase.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetHeadsReversed = "SetHeadsReversed";

		public static readonly StringName SetHeadsVisible = "SetHeadsVisible";
	}

	public new class PropertyName : AdobeAnimateSpriteBase.PropertyName
	{
		public static readonly StringName _headRight = "_headRight";

		public static readonly StringName _headLeft = "_headLeft";

		public static readonly StringName _rightMuzzle = "_rightMuzzle";

		public static readonly StringName _leftMuzzle = "_leftMuzzle";

		public static readonly StringName _rightOffset = "_rightOffset";

		public static readonly StringName _leftOffset = "_leftOffset";

		public static readonly StringName _rightMuzzlePosition = "_rightMuzzlePosition";

		public static readonly StringName _leftMuzzlePosition = "_leftMuzzlePosition";

		public static readonly StringName _rightScale = "_rightScale";

		public static readonly StringName _leftScale = "_leftScale";

		public static readonly StringName _headsReversed = "_headsReversed";
	}

	public new class SignalName : AdobeAnimateSpriteBase.SignalName
	{
	}

	private AdobeAnimateSpriteBase _headRight;

	private AdobeAnimateSpriteBase _headLeft;

	private Marker2D _rightMuzzle;

	private Marker2D _leftMuzzle;

	private Vector2 _rightOffset;

	private Vector2 _leftOffset;

	private Vector2 _rightMuzzlePosition;

	private Vector2 _leftMuzzlePosition;

	private Vector2 _rightScale;

	private Vector2 _leftScale;

	private bool _headsReversed = true;

	public override void _Ready()
	{
		base._Ready();
		if (!AdobeAnimateSprite.IsEditorContext)
		{
			_headRight = GetNode<AdobeAnimateSpriteBase>("%HeadRight");
			_headLeft = GetNode<AdobeAnimateSpriteBase>("%HeadLeft");
			_rightMuzzle = _headRight.GetNodeOrNull<Marker2D>("Marker2D");
			_leftMuzzle = _headLeft.GetNodeOrNull<Marker2D>("Marker2D");
			_rightOffset = _headRight.offset;
			_leftOffset = _headLeft.offset;
			_rightMuzzlePosition = _rightMuzzle?.Position ?? Vector2.Zero;
			_leftMuzzlePosition = _leftMuzzle?.Position ?? Vector2.Zero;
			_rightScale = _headRight.Scale.Abs();
			_leftScale = _headLeft.Scale.Abs();
		}
	}

	public void SetHeadsReversed(bool reversed)
	{
		if (_headsReversed != reversed && GodotObject.IsInstanceValid(_headRight) && GodotObject.IsInstanceValid(_headLeft))
		{
			_headsReversed = reversed;
			Vector2 vector = (reversed ? Vector2.Zero : new Vector2(0f - (_rightMuzzlePosition.X + _leftMuzzlePosition.X), 0f));
			float num = (reversed ? 1f : (-1f));
			_headRight.Scale = new Vector2(num * _rightScale.X, _rightScale.Y);
			_headLeft.Scale = new Vector2(num * _leftScale.X, _leftScale.Y);
			_headRight.offset = _rightOffset + vector;
			_headLeft.offset = _leftOffset + vector;
			if (GodotObject.IsInstanceValid(_rightMuzzle))
			{
				_rightMuzzle.Position = _rightMuzzlePosition + vector;
			}
			if (GodotObject.IsInstanceValid(_leftMuzzle))
			{
				_leftMuzzle.Position = _leftMuzzlePosition + vector;
			}
			AdobeAnimateRenderManager.InvalidateGpuRenderGraph(GetRenderSortRootForRender(), AdobeAnimateGpuGraphInvalidationReason.OwnerDefinition);
			QueueRedraw();
		}
	}

	public void SetHeadsVisible(bool visible)
	{
		if (GodotObject.IsInstanceValid(_headRight))
		{
			_headRight.Visible = visible;
		}
		if (GodotObject.IsInstanceValid(_headLeft))
		{
			_headLeft.Visible = visible;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(3)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.SetHeadsReversed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "reversed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.SetHeadsVisible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Bool, "visible", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetHeadsReversed && args.Count == 1)
		{
			SetHeadsReversed(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetHeadsVisible && args.Count == 1)
		{
			SetHeadsVisible(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.SetHeadsReversed)
		{
			return true;
		}
		if (method == MethodName.SetHeadsVisible)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._headRight)
		{
			_headRight = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._headLeft)
		{
			_headLeft = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._rightMuzzle)
		{
			_rightMuzzle = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName._leftMuzzle)
		{
			_leftMuzzle = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName._rightOffset)
		{
			_rightOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._leftOffset)
		{
			_leftOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._rightMuzzlePosition)
		{
			_rightMuzzlePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._leftMuzzlePosition)
		{
			_leftMuzzlePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._rightScale)
		{
			_rightScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._leftScale)
		{
			_leftScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._headsReversed)
		{
			_headsReversed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._headRight)
		{
			value = VariantUtils.CreateFrom(in _headRight);
			return true;
		}
		if (name == PropertyName._headLeft)
		{
			value = VariantUtils.CreateFrom(in _headLeft);
			return true;
		}
		if (name == PropertyName._rightMuzzle)
		{
			value = VariantUtils.CreateFrom(in _rightMuzzle);
			return true;
		}
		if (name == PropertyName._leftMuzzle)
		{
			value = VariantUtils.CreateFrom(in _leftMuzzle);
			return true;
		}
		if (name == PropertyName._rightOffset)
		{
			value = VariantUtils.CreateFrom(in _rightOffset);
			return true;
		}
		if (name == PropertyName._leftOffset)
		{
			value = VariantUtils.CreateFrom(in _leftOffset);
			return true;
		}
		if (name == PropertyName._rightMuzzlePosition)
		{
			value = VariantUtils.CreateFrom(in _rightMuzzlePosition);
			return true;
		}
		if (name == PropertyName._leftMuzzlePosition)
		{
			value = VariantUtils.CreateFrom(in _leftMuzzlePosition);
			return true;
		}
		if (name == PropertyName._rightScale)
		{
			value = VariantUtils.CreateFrom(in _rightScale);
			return true;
		}
		if (name == PropertyName._leftScale)
		{
			value = VariantUtils.CreateFrom(in _leftScale);
			return true;
		}
		if (name == PropertyName._headsReversed)
		{
			value = VariantUtils.CreateFrom(in _headsReversed);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._headRight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._headLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._rightMuzzle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._leftMuzzle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._rightOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._leftOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._rightMuzzlePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._leftMuzzlePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._rightScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Vector2, PropertyName._leftScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._headsReversed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._headRight, Variant.From(in _headRight));
		info.AddProperty(PropertyName._headLeft, Variant.From(in _headLeft));
		info.AddProperty(PropertyName._rightMuzzle, Variant.From(in _rightMuzzle));
		info.AddProperty(PropertyName._leftMuzzle, Variant.From(in _leftMuzzle));
		info.AddProperty(PropertyName._rightOffset, Variant.From(in _rightOffset));
		info.AddProperty(PropertyName._leftOffset, Variant.From(in _leftOffset));
		info.AddProperty(PropertyName._rightMuzzlePosition, Variant.From(in _rightMuzzlePosition));
		info.AddProperty(PropertyName._leftMuzzlePosition, Variant.From(in _leftMuzzlePosition));
		info.AddProperty(PropertyName._rightScale, Variant.From(in _rightScale));
		info.AddProperty(PropertyName._leftScale, Variant.From(in _leftScale));
		info.AddProperty(PropertyName._headsReversed, Variant.From(in _headsReversed));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._headRight, out var value))
		{
			_headRight = value.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._headLeft, out var value2))
		{
			_headLeft = value2.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._rightMuzzle, out var value3))
		{
			_rightMuzzle = value3.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName._leftMuzzle, out var value4))
		{
			_leftMuzzle = value4.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName._rightOffset, out var value5))
		{
			_rightOffset = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._leftOffset, out var value6))
		{
			_leftOffset = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._rightMuzzlePosition, out var value7))
		{
			_rightMuzzlePosition = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._leftMuzzlePosition, out var value8))
		{
			_leftMuzzlePosition = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._rightScale, out var value9))
		{
			_rightScale = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._leftScale, out var value10))
		{
			_leftScale = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._headsReversed, out var value11))
		{
			_headsReversed = value11.As<bool>();
		}
	}
}
