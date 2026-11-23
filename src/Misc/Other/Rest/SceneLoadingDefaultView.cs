using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;

[GlobalClass]
[ScriptPath("res://Core/SceneManager/SceneLoadeing/SceneLoadingDefaultView.cs")]
public class SceneLoadingDefaultView : XWModLoadingView
{
	public new class MethodName : XWModLoadingView.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName AnimeCompleted = "AnimeCompleted";

		public static readonly StringName Timeout = "Timeout";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : XWModLoadingView.PropertyName
	{
		public static readonly StringName background = "background";

		public static readonly StringName stinky = "stinky";

		public static readonly StringName label = "label";

		public static readonly StringName _pointNum = "_pointNum";

		public static readonly StringName _timer = "_timer";
	}

	public new class SignalName : XWModLoadingView.SignalName
	{
	}

	public Control background;

	public AdobeAnimateSpriteBase stinky;

	public Label label;

	private int _pointNum;

	private Timer _timer;

	public override void _Ready()
	{
		background = GetNode<Control>("%Background");
		stinky = GetNode<AdobeAnimateSpriteBase>("%Stinky");
		stinky.forceLocalRender = true;
		stinky.SetRenderClipControl(GetNode<Control>("%StinkyRenderMount"));
		stinky.OnAnimeCompleted += AnimeCompleted;
		label = GetNode<Label>("%Label");
		stinky.SetAnimation("Out", loop: false);
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += Timeout;
	}

	public override async Task<bool> PlayMotionAsync(bool enter)
	{
		Tween tween = CreateTween().SetParallel().SetIgnoreTimeScale();
		tween.SetEase((Tween.EaseType)(enter ? 1 : 0));
		tween.SetTrans(Tween.TransitionType.Back);
		if (enter)
		{
			tween.TweenProperty(stinky, "scale", Vector2.One, 0.5).From(new Vector2(2f, 0f));
			tween.TweenProperty(background, "modulate:a", 1.0, 0.5).From(0.0);
		}
		else
		{
			tween.TweenProperty(stinky, "scale", Vector2.Zero, 1.0).From(Vector2.One);
			tween.TweenProperty(background, "modulate:a", 0.0, 0.75).From(1.0);
		}
		SceneTree tree = GetTree();
		ulong deadline = Time.GetTicksMsec() + 3000;
		while (GodotObject.IsInstanceValid(this) && IsInsideTree() && tween.IsRunning())
		{
			if (Time.GetTicksMsec() >= deadline)
			{
				tween.Kill();
				return false;
			}
			await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		}
		return GodotObject.IsInstanceValid(this) && IsInsideTree();
	}

	private void AnimeCompleted(string clip)
	{
		AdobeAnimateSpriteBase adobeAnimateSpriteBase = stinky;
		string clipName = ((clip == "In") ? "Out" : ((!(clip == "Out")) ? "In" : "Crawl"));
		adobeAnimateSpriteBase.SetAnimation(clipName, loop: false);
	}

	private void Timeout()
	{
		_pointNum = (_pointNum + 1) % 7;
		label.Text = string.Concat("加载中", string.Concat(Enumerable.Repeat(" .", _pointNum)));
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(stinky))
		{
			stinky.OnAnimeCompleted -= AnimeCompleted;
		}
		if (GodotObject.IsInstanceValid(_timer))
		{
			_timer.Timeout -= Timeout;
		}
		base._ExitTree();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(4)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AnimeCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clip", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Timeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
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
		if (method == MethodName.AnimeCompleted && args.Count == 1)
		{
			AnimeCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Timeout && args.Count == 0)
		{
			Timeout();
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
		if (method == MethodName.AnimeCompleted)
		{
			return true;
		}
		if (method == MethodName.Timeout)
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
		if (name == PropertyName.background)
		{
			background = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.stinky)
		{
			stinky = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName.label)
		{
			label = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._pointNum)
		{
			_pointNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._timer)
		{
			_timer = VariantUtils.ConvertTo<Timer>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.background)
		{
			value = VariantUtils.CreateFrom(in background);
			return true;
		}
		if (name == PropertyName.stinky)
		{
			value = VariantUtils.CreateFrom(in stinky);
			return true;
		}
		if (name == PropertyName.label)
		{
			value = VariantUtils.CreateFrom(in label);
			return true;
		}
		if (name == PropertyName._pointNum)
		{
			value = VariantUtils.CreateFrom(in _pointNum);
			return true;
		}
		if (name == PropertyName._timer)
		{
			value = VariantUtils.CreateFrom(in _timer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName.background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.stinky, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._pointNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.background, Variant.From(in background));
		info.AddProperty(PropertyName.stinky, Variant.From(in stinky));
		info.AddProperty(PropertyName.label, Variant.From(in label));
		info.AddProperty(PropertyName._pointNum, Variant.From(in _pointNum));
		info.AddProperty(PropertyName._timer, Variant.From(in _timer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.background, out var value))
		{
			background = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.stinky, out var value2))
		{
			stinky = value2.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName.label, out var value3))
		{
			label = value3.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._pointNum, out var value4))
		{
			_pointNum = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._timer, out var value5))
		{
			_timer = value5.As<Timer>();
		}
	}
}
