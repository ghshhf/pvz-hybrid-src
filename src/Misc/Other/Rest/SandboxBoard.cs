using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[ScriptPath("res://Scene/Sandbox/SandboxBoard.cs")]
public class SandboxBoard : TextureRect
{
	public new class MethodName : TextureRect.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ImageRect = "ImageRect";

		public static readonly StringName WorldToBoard = "WorldToBoard";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName IsMarkerVisible = "IsMarkerVisible";

		public static readonly StringName GetTeamColor = "GetTeamColor";

		public new static readonly StringName _Draw = "_Draw";
	}

	public new class PropertyName : TextureRect.PropertyName
	{
		public static readonly StringName Session = "Session";

		public static readonly StringName BattleViewport = "BattleViewport";

		public static readonly StringName Selected = "Selected";

		public static readonly StringName _badgeStyles = "_badgeStyles";

		public static readonly StringName _badgeFont = "_badgeFont";

		public static readonly StringName _teamRings = "_teamRings";
	}

	public new class SignalName : TextureRect.SignalName
	{
	}

	private static readonly Vector2 BadgeSize = new Vector2(44f, 16f);

	private const int BadgeFontSize = 9;

	private readonly StyleBoxFlat[] _badgeStyles = new StyleBoxFlat[6];

	private Font _badgeFont;

	private SandboxTeamRings _teamRings;

	public SandboxSession Session { get; set; }

	public SubViewport BattleViewport { get; set; }

	public TowerDefenseCharacter Selected { get; set; }

	public override void _Ready()
	{
		_badgeFont = GD.Load<FontFile>("res://Asset/Font/fzjz.ttf");
		string[] array = new string[3] { "992c22", "1f5aa2", "445536" };
		for (int i = 0; i < _badgeStyles.Length; i++)
		{
			bool flag = i % 2 == 1;
			StyleBoxFlat styleBoxFlat = new StyleBoxFlat
			{
				BgColor = new Color(array[i / 2]),
				BorderColor = new Color(flag ? "ffe165" : "0f1917"),
				CornerRadiusTopLeft = 4,
				CornerRadiusTopRight = 4,
				CornerRadiusBottomLeft = 4,
				CornerRadiusBottomRight = 4
			};
			styleBoxFlat.SetBorderWidthAll((!flag) ? 1 : 2);
			_badgeStyles[i] = styleBoxFlat;
		}
	}

	public Rect2 ImageRect()
	{
		Vector2 vector = BattleViewport.Size;
		float num = Mathf.Min(base.Size.X / vector.X, base.Size.Y / vector.Y);
		Vector2 vector2 = vector * num;
		return new Rect2((base.Size - vector2) * 0.5f, vector2);
	}

	public bool TryGetWorldPosition(Vector2 point, out Vector2 world)
	{
		Rect2 rect = ImageRect();
		world = Vector2.Zero;
		if (!rect.HasPoint(point) || rect.Size.X <= 0f)
		{
			return false;
		}
		Vector2 vector = (point - rect.Position) / rect.Size * BattleViewport.Size;
		world = BattleViewport.CanvasTransform.AffineInverse() * vector;
		return true;
	}

	public Vector2 WorldToBoard(Vector2 world)
	{
		Rect2 rect = ImageRect();
		return rect.Position + BattleViewport.CanvasTransform * world / BattleViewport.Size * rect.Size;
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
		SandboxSession session = Session;
		if (session == null || session.Process?.IsReady != true)
		{
			return;
		}
		if (GetTree().Paused)
		{
			foreach (TowerDefenseCharacter character in Session.Characters)
			{
				if (SandboxSession.IsAlive(character))
				{
					character.shadowComponent?.BatchUpdate();
				}
			}
			TowerDefenseShadowMultiMeshRenderer nodeOrNull = TowerDefenseManager.Instance.currentControl.characterNode.GetNodeOrNull<TowerDefenseShadowMultiMeshRenderer>("TowerDefenseShadowMultiMeshRenderer");
			if (GodotObject.IsInstanceValid(nodeOrNull))
			{
				nodeOrNull.ProcessMode = ProcessModeEnum.Always;
			}
		}
		if (!GodotObject.IsInstanceValid(_teamRings))
		{
			_teamRings = new SandboxTeamRings
			{
				Name = "SandboxTeamRings",
				Board = this,
				TopLevel = true,
				ZAsRelative = false,
				ZIndex = -4095,
				ProcessMode = ProcessModeEnum.Always
			};
			TowerDefenseManager.Instance.currentControl.characterNode.AddChild(_teamRings, forceReadableName: false, InternalMode.Disabled);
			_teamRings.GlobalTransform = Transform2D.Identity;
		}
		_teamRings.QueueRedraw();
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(_teamRings))
		{
			_teamRings.QueueFree();
		}
	}

	internal static bool IsMarkerVisible(TowerDefenseCharacter character)
	{
		if (SandboxSession.IsAlive(character) && !character.invisible && character.IsVisibleInTree() && GodotObject.IsInstanceValid(character.sprite) && character.sprite.IsVisibleInTree())
		{
			return character.transformPoint.IsVisibleInTree();
		}
		return false;
	}

	internal static Color GetTeamColor(TowerDefenseCharacter character)
	{
		if (character.camp != TowerDefenseEnum.CHARACTER_CAMP.PLANT)
		{
			if (character.camp != TowerDefenseEnum.CHARACTER_CAMP.ZOMBIE)
			{
				return new Color("a3c18b");
			}
			return new Color("67b7ff");
		}
		return new Color("f56b50");
	}

	public override void _Draw()
	{
		SandboxSession session = Session;
		if (session == null || session.Process?.IsReady != true)
		{
			return;
		}
		Rect2 rect = ImageRect();
		foreach (TowerDefenseCharacter character in Session.Characters)
		{
			if (IsMarkerVisible(character))
			{
				Vector2 vector = WorldToBoard(character.GetLogicalGlobalPosition(character.transformPoint));
				if (rect.HasPoint(vector))
				{
					int num = ((character.camp != TowerDefenseEnum.CHARACTER_CAMP.PLANT) ? ((character.camp == TowerDefenseEnum.CHARACTER_CAMP.ZOMBIE) ? 1 : 2) : 0);
					string text = num switch
					{
						1 => "蓝队", 
						0 => "红队", 
						_ => "中立", 
					};
					text = ((SandboxSession.GetFacing(character) < 0) ? ("← " + text) : (text + " →"));
					Rect2 rect2 = new Rect2(vector - BadgeSize * 0.5f, BadgeSize);
					DrawStyleBox(_badgeStyles[num * 2 + ((character == Selected) ? 1 : 0)], rect2);
					Vector2 pos = new Vector2(rect2.Position.X, vector.Y + (_badgeFont.GetAscent(9) - _badgeFont.GetDescent(9)) * 0.5f);
					DrawStringOutline(_badgeFont, pos, text, HorizontalAlignment.Center, BadgeSize.X, 9, 1, new Color(0f, 0f, 0f, 0.8f), TextServer.JustificationFlag.Kashida | TextServer.JustificationFlag.WordBound, TextServer.Direction.Auto, TextServer.Orientation.Horizontal, 0f);
					DrawString(_badgeFont, pos, text, HorizontalAlignment.Center, BadgeSize.X, 9, new Color("fff9e6"), TextServer.JustificationFlag.Kashida | TextServer.JustificationFlag.WordBound, TextServer.Direction.Auto, TextServer.Orientation.Horizontal, 0f);
				}
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(8)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImageRect, new PropertyInfo(Variant.Type.Rect2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.WorldToBoard, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "world", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IsMarkerVisible, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.GetTeamColor, new PropertyInfo(Variant.Type.Color, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName._Draw, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.ImageRect && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Rect2>(ImageRect());
			return true;
		}
		if (method == MethodName.WorldToBoard && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(WorldToBoard(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsMarkerVisible && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsMarkerVisible(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.GetTeamColor && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Color>(GetTeamColor(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName._Draw && args.Count == 0)
		{
			_Draw();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IsMarkerVisible && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsMarkerVisible(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
			return true;
		}
		if (method == MethodName.GetTeamColor && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Color>(GetTeamColor(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0])));
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
		if (method == MethodName.ImageRect)
		{
			return true;
		}
		if (method == MethodName.WorldToBoard)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.IsMarkerVisible)
		{
			return true;
		}
		if (method == MethodName.GetTeamColor)
		{
			return true;
		}
		if (method == MethodName._Draw)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Session)
		{
			Session = VariantUtils.ConvertTo<SandboxSession>(in value);
			return true;
		}
		if (name == PropertyName.BattleViewport)
		{
			BattleViewport = VariantUtils.ConvertTo<SubViewport>(in value);
			return true;
		}
		if (name == PropertyName.Selected)
		{
			Selected = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		if (name == PropertyName._badgeFont)
		{
			_badgeFont = VariantUtils.ConvertTo<Font>(in value);
			return true;
		}
		if (name == PropertyName._teamRings)
		{
			_teamRings = VariantUtils.ConvertTo<SandboxTeamRings>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Session)
		{
			value = VariantUtils.CreateFrom<SandboxSession>(Session);
			return true;
		}
		if (name == PropertyName.BattleViewport)
		{
			value = VariantUtils.CreateFrom<SubViewport>(BattleViewport);
			return true;
		}
		if (name == PropertyName.Selected)
		{
			value = VariantUtils.CreateFrom<TowerDefenseCharacter>(Selected);
			return true;
		}
		if (name == PropertyName._badgeStyles)
		{
			GodotObject[] badgeStyles = _badgeStyles;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(badgeStyles);
			return true;
		}
		if (name == PropertyName._badgeFont)
		{
			value = VariantUtils.CreateFrom(in _badgeFont);
			return true;
		}
		if (name == PropertyName._teamRings)
		{
			value = VariantUtils.CreateFrom(in _teamRings);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.Session, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.BattleViewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.Selected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Array, PropertyName._badgeStyles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._badgeFont, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._teamRings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Session, Variant.From<SandboxSession>(Session));
		info.AddProperty(PropertyName.BattleViewport, Variant.From<SubViewport>(BattleViewport));
		info.AddProperty(PropertyName.Selected, Variant.From<TowerDefenseCharacter>(Selected));
		info.AddProperty(PropertyName._badgeFont, Variant.From(in _badgeFont));
		info.AddProperty(PropertyName._teamRings, Variant.From(in _teamRings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Session, out var value))
		{
			Session = value.As<SandboxSession>();
		}
		if (info.TryGetProperty(PropertyName.BattleViewport, out var value2))
		{
			BattleViewport = value2.As<SubViewport>();
		}
		if (info.TryGetProperty(PropertyName.Selected, out var value3))
		{
			Selected = value3.As<TowerDefenseCharacter>();
		}
		if (info.TryGetProperty(PropertyName._badgeFont, out var value4))
		{
			_badgeFont = value4.As<Font>();
		}
		if (info.TryGetProperty(PropertyName._teamRings, out var value5))
		{
			_teamRings = value5.As<SandboxTeamRings>();
		}
	}
}
