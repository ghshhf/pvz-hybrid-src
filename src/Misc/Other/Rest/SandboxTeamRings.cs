using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[ScriptPath("res://Scene/Sandbox/SandboxTeamRings.cs")]
public class SandboxTeamRings : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Draw = "_Draw";

		public static readonly StringName GetVisibleRegion = "GetVisibleRegion";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName Board = "Board";

		public static readonly StringName _teamPoints = "_teamPoints";

		public static readonly StringName _selectionPoints = "_selectionPoints";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const int Segments = 96;

	private const float StrokeRatio = 0.035f;

	private const float SelectionScale = 0.86f;

	private readonly Dictionary<Texture2D, Rect2> _visibleRegions = new Dictionary<Texture2D, Rect2>();

	private readonly Vector2[] _teamPoints = new Vector2[97];

	private readonly Vector2[] _selectionPoints = new Vector2[97];

	public SandboxBoard Board { get; set; }

	public override void _Draw()
	{
		if (!GodotObject.IsInstanceValid(Board))
		{
			return;
		}
		SandboxSession session = Board.Session;
		if (session == null || session.Process?.IsReady != true)
		{
			return;
		}
		foreach (TowerDefenseCharacter character in Board.Session.Characters)
		{
			if (!SandboxBoard.IsMarkerVisible(character) || character.shadowComponent == null || !character.shadowComponent.TryGetDrawQuad(out var texture, out var transform))
			{
				continue;
			}
			Rect2 visibleRegion = GetVisibleRegion(texture);
			Vector2 vector = transform * visibleRegion.GetCenter();
			Vector2 vector2 = transform.X * visibleRegion.Size.X * 0.5f;
			Vector2 vector3 = transform.Y * visibleRegion.Size.Y * 0.5f;
			float num = Mathf.Min(vector2.Length(), vector3.Length()) * 2f * 0.035f;
			if (!(num <= 0f))
			{
				for (int i = 0; i < 96; i++)
				{
					float s = (float)Math.PI * 2f * (float)i / 96f;
					Vector2 vector4 = vector2 * Mathf.Cos(s) + vector3 * Mathf.Sin(s);
					_teamPoints[i] = vector + vector4;
					_selectionPoints[i] = vector + vector4 * 0.86f;
				}
				_teamPoints[96] = _teamPoints[0];
				_selectionPoints[96] = _selectionPoints[0];
				DrawPolyline(_teamPoints, SandboxBoard.GetTeamColor(character), num, antialiased: true);
				if (character == Board.Selected)
				{
					DrawPolyline(_selectionPoints, new Color("ffe165"), num * 0.86f, antialiased: true);
				}
			}
		}
	}

	private Rect2 GetVisibleRegion(Texture2D texture)
	{
		if (_visibleRegions.TryGetValue(texture, out var value))
		{
			return value;
		}
		using Image image = texture.GetImage();
		if (image.IsCompressed())
		{
			image.Decompress();
		}
		Rect2I usedRect = image.GetUsedRect();
		Vector2 size = texture.GetSize();
		value = new Rect2(usedRect.Position / size - Vector2.One * 0.5f, usedRect.Size / size);
		_visibleRegions.Add(texture, value);
		return value;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(2)
		{
			new MethodInfo(MethodName._Draw, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetVisibleRegion, new PropertyInfo(Variant.Type.Rect2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "texture", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Draw && args.Count == 0)
		{
			_Draw();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetVisibleRegion && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Rect2>(GetVisibleRegion(VariantUtils.ConvertTo<Texture2D>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Draw)
		{
			return true;
		}
		if (method == MethodName.GetVisibleRegion)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Board)
		{
			Board = VariantUtils.ConvertTo<SandboxBoard>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Board)
		{
			value = VariantUtils.CreateFrom<SandboxBoard>(Board);
			return true;
		}
		if (name == PropertyName._teamPoints)
		{
			value = VariantUtils.CreateFrom(in _teamPoints);
			return true;
		}
		if (name == PropertyName._selectionPoints)
		{
			value = VariantUtils.CreateFrom(in _selectionPoints);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.Board, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.PackedVector2Array, PropertyName._teamPoints, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.PackedVector2Array, PropertyName._selectionPoints, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Board, Variant.From<SandboxBoard>(Board));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Board, out var value))
		{
			Board = value.As<SandboxBoard>();
		}
	}
}
