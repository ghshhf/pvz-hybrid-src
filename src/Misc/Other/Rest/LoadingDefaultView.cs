using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using PVZHE.ModEditor.ModSystem;

[GlobalClass]
[ScriptPath("res://Scene/Loading/LoadingDefaultView.cs")]
public class LoadingDefaultView : XWModLoadingView
{
	public new class MethodName : XWModLoadingView.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ActivatePresentation = "ActivatePresentation";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName BindBootstrapAtlasProfile = "BindBootstrapAtlasProfile";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateLoadingAnimationPlayback = "UpdateLoadingAnimationPlayback";

		public static readonly StringName ResumeLoadingAnimation = "ResumeLoadingAnimation";

		public static readonly StringName ApplyDeferredLoadingFont = "ApplyDeferredLoadingFont";

		public static readonly StringName ActivateLoadingAnimations = "ActivateLoadingAnimations";

		public static readonly StringName ActivateLoadingAnimation = "ActivateLoadingAnimation";

		public static readonly StringName DisconnectLoadingAnimationCompletion = "DisconnectLoadingAnimationCompletion";

		public static readonly StringName OnLoadingAnimationCompleted = "OnLoadingAnimationCompleted";

		public static readonly StringName EnsureSodRollCapTexture = "EnsureSodRollCapTexture";

		public static readonly StringName CanAnimateSodRollCap = "CanAnimateSodRollCap";
	}

	public new class PropertyName : XWModLoadingView.PropertyName
	{
		public new static readonly StringName PresentationComplete = "PresentationComplete";

		public static readonly StringName _loadLabel = "_loadLabel";

		public static readonly StringName _progressBar = "_progressBar";

		public static readonly StringName _startButton = "_startButton";

		public static readonly StringName _sodRollCap = "_sodRollCap";

		public static readonly StringName _loadBarSprout = "_loadBarSprout";

		public static readonly StringName _loadBarSprout2 = "_loadBarSprout2";

		public static readonly StringName _loadBarSprout3 = "_loadBarSprout3";

		public static readonly StringName _loadBarSprout4 = "_loadBarSprout4";

		public static readonly StringName _loadBarZombieHead = "_loadBarZombieHead";

		public static readonly StringName _percentage = "_percentage";

		public static readonly StringName _currentValue = "_currentValue";

		public static readonly StringName _completedLoadingAnimationCount = "_completedLoadingAnimationCount";
	}

	public new class SignalName : XWModLoadingView.SignalName
	{
	}

	private const string SodRollCapTexturePath = "res://Asset/Texture/GUI/General/Loading/SodRollCap.png";

	private const string LoadingFontPath = "res://Asset/Font/fzkt.ttf";

	private const string BootstrapAtlasProfilePath = "res://addons/AdobeAnimateEditor/GeneratedAtlas/Bootstrap/AdobeAnimateBootstrapAtlasProfile.tres";

	private const string LoadingAnimationClip = "Idle";

	private const int LoadingAnimationCount = 5;

	private const float LoadingPresentationCompleteProgress = 0.95f;

	private const float MaxPresentationDeltaSeconds = 1f / 30f;

	private Label _loadLabel;

	private TextureProgressBar _progressBar;

	private Button _startButton;

	private Sprite2D _sodRollCap;

	private AdobeAnimateSpriteBase _loadBarSprout;

	private AdobeAnimateSpriteBase _loadBarSprout2;

	private AdobeAnimateSpriteBase _loadBarSprout3;

	private AdobeAnimateSpriteBase _loadBarSprout4;

	private AdobeAnimateSpriteBase _loadBarZombieHead;

	private float _percentage;

	private float _currentValue;

	private int _completedLoadingAnimationCount;

	public override bool PresentationComplete
	{
		get
		{
			if (_currentValue >= 0.95f)
			{
				return _completedLoadingAnimationCount >= 5;
			}
			return false;
		}
	}

	public override void _Ready()
	{
		_loadLabel = GetNode<Label>("%LoadLabel");
		_progressBar = GetNode<TextureProgressBar>("%ProgressBar");
		_startButton = GetNode<Button>("%StartButton");
		_sodRollCap = GetNode<Sprite2D>("%SodRollCap");
		EnsureSodRollCapTexture();
		_loadBarSprout = GetNode<AdobeAnimateSpriteBase>("%LoadBarSprout");
		_loadBarSprout2 = GetNode<AdobeAnimateSpriteBase>("%LoadBarSprout2");
		_loadBarSprout3 = GetNode<AdobeAnimateSpriteBase>("%LoadBarSprout3");
		_loadBarSprout4 = GetNode<AdobeAnimateSpriteBase>("%LoadBarSprout4");
		_loadBarZombieHead = GetNode<AdobeAnimateSpriteBase>("%LoadBarZombieHead");
		BindBootstrapAtlasProfile();
		ApplyDeferredLoadingFont();
		ApplyState(base.State);
	}

	public override void ActivatePresentation()
	{
		ActivateLoadingAnimations();
	}

	public override void ApplyState(XWLoadingState state)
	{
		base.ApplyState(state);
		if (state.Progress.HasValue)
		{
			_percentage = (float)state.Progress.Value;
		}
		if (GodotObject.IsInstanceValid(_progressBar))
		{
			_progressBar.Value = _currentValue;
		}
		if (GodotObject.IsInstanceValid(_loadLabel))
		{
			_loadLabel.Visible = !state.CanStart;
		}
		if (state.CanStart && GodotObject.IsInstanceValid(_sodRollCap))
		{
			_sodRollCap.Visible = false;
		}
	}

	public override void _ExitTree()
	{
		DisconnectLoadingAnimationCompletion(_loadBarSprout);
		DisconnectLoadingAnimationCompletion(_loadBarSprout2);
		DisconnectLoadingAnimationCompletion(_loadBarSprout3);
		DisconnectLoadingAnimationCompletion(_loadBarSprout4);
		DisconnectLoadingAnimationCompletion(_loadBarZombieHead);
		base._ExitTree();
	}

	private void BindBootstrapAtlasProfile()
	{
		AdobeAnimateAtlasProfile adobeAnimateAtlasProfile = ResourceLoader.Load<AdobeAnimateAtlasProfile>("res://addons/AdobeAnimateEditor/GeneratedAtlas/Bootstrap/AdobeAnimateBootstrapAtlasProfile.tres", null, ResourceLoader.CacheMode.Reuse);
		if (!GodotObject.IsInstanceValid(adobeAnimateAtlasProfile))
		{
			throw new InvalidOperationException("Bootstrap Adobe Animate atlas profile is unavailable: res://addons/AdobeAnimateEditor/GeneratedAtlas/Bootstrap/AdobeAnimateBootstrapAtlasProfile.tres");
		}
		_loadBarSprout.atlasProfileOverride = adobeAnimateAtlasProfile;
		_loadBarSprout2.atlasProfileOverride = adobeAnimateAtlasProfile;
		_loadBarSprout3.atlasProfileOverride = adobeAnimateAtlasProfile;
		_loadBarSprout4.atlasProfileOverride = adobeAnimateAtlasProfile;
		_loadBarZombieHead.atlasProfileOverride = adobeAnimateAtlasProfile;
	}

	public override void _Process(double delta)
	{
		if (_currentValue < _percentage)
		{
			float num = Mathf.Min((float)delta, 1f / 30f);
			_currentValue = Mathf.Lerp(_currentValue, _percentage, Mathf.Min((_currentValue * 2f + 0.5f) * num, 1f));
			_progressBar.Value = _currentValue;
			UpdateLoadingAnimationPlayback(_currentValue);
			if (CanAnimateSodRollCap())
			{
				_sodRollCap.Position = new Vector2(_currentValue * 315f, -26f + _currentValue * 30f);
				_sodRollCap.Rotation = (float)Math.PI * 4f * _currentValue;
				_sodRollCap.Scale = Vector2.One * (1f - _currentValue * 0.75f);
			}
		}
		if (_currentValue >= 0.95f && GodotObject.IsInstanceValid(_sodRollCap))
		{
			_sodRollCap.Visible = false;
		}
	}

	private void UpdateLoadingAnimationPlayback(float progress, bool playAudio = true)
	{
		ResumeLoadingAnimation(_loadBarSprout, progress, 0.15f, playAudio);
		ResumeLoadingAnimation(_loadBarSprout2, progress, 0.3f, playAudio);
		ResumeLoadingAnimation(_loadBarSprout3, progress, 0.4f, playAudio);
		ResumeLoadingAnimation(_loadBarSprout4, progress, 0.6f, playAudio);
		if (GodotObject.IsInstanceValid(_loadBarZombieHead) && _loadBarZombieHead.pause && !(progress < 0.8f))
		{
			if (playAudio)
			{
				AudioManager.Instance.AudioPlay("LoadingBarFlower");
				AudioManager.Instance.AudioPlay("LoadingBarZombie");
			}
			_loadBarZombieHead.SetAnimation("Idle", loop: false);
			_loadBarZombieHead.pause = false;
		}
	}

	private static void ResumeLoadingAnimation(AdobeAnimateSpriteBase animation, float progress, float threshold, bool playAudio)
	{
		if (GodotObject.IsInstanceValid(animation) && animation.pause && !(progress < threshold))
		{
			if (playAudio)
			{
				AudioManager.Instance.AudioPlay("LoadingBarFlower");
			}
			animation.SetAnimation("Idle", loop: false);
			animation.pause = false;
		}
	}

	private void ApplyDeferredLoadingFont()
	{
		if (GodotObject.IsInstanceValid(_startButton))
		{
			FontFile fontFile = GD.Load<FontFile>("res://Asset/Font/fzkt.ttf");
			if (GodotObject.IsInstanceValid(fontFile))
			{
				_startButton.AddThemeFontOverride("font", fontFile);
			}
		}
	}

	private void ActivateLoadingAnimations()
	{
		ActivateLoadingAnimation(_loadBarSprout);
		ActivateLoadingAnimation(_loadBarSprout2);
		ActivateLoadingAnimation(_loadBarSprout3);
		ActivateLoadingAnimation(_loadBarSprout4);
		ActivateLoadingAnimation(_loadBarZombieHead);
	}

	private void ActivateLoadingAnimation(AdobeAnimateSpriteBase animation)
	{
		if (GodotObject.IsInstanceValid(animation))
		{
			animation.Visible = true;
			animation.pause = true;
			animation.OnAnimeCompleted -= OnLoadingAnimationCompleted;
			animation.OnAnimeCompleted += OnLoadingAnimationCompleted;
			animation.SetAnimation("Idle", loop: false);
		}
	}

	private void DisconnectLoadingAnimationCompletion(AdobeAnimateSpriteBase animation)
	{
		if (GodotObject.IsInstanceValid(animation))
		{
			animation.OnAnimeCompleted -= OnLoadingAnimationCompleted;
		}
	}

	private void OnLoadingAnimationCompleted(string clipName)
	{
		if (!(clipName != "Idle") && _completedLoadingAnimationCount < 5)
		{
			_completedLoadingAnimationCount++;
		}
	}

	private void EnsureSodRollCapTexture()
	{
		if (GodotObject.IsInstanceValid(_sodRollCap) && !GodotObject.IsInstanceValid(_sodRollCap.Texture))
		{
			_sodRollCap.Texture = GD.Load<Texture2D>("res://Asset/Texture/GUI/General/Loading/SodRollCap.png");
			if (!GodotObject.IsInstanceValid(_sodRollCap.Texture))
			{
				_sodRollCap.Visible = false;
			}
		}
	}

	private bool CanAnimateSodRollCap()
	{
		if (GodotObject.IsInstanceValid(_sodRollCap))
		{
			return GodotObject.IsInstanceValid(_sodRollCap.Texture);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(14)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ActivatePresentation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.BindBootstrapAtlasProfile, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.UpdateLoadingAnimationPlayback, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "progress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "playAudio", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResumeLoadingAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "animation", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Float, "progress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "threshold", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "playAudio", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyDeferredLoadingFont, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ActivateLoadingAnimations, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ActivateLoadingAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "animation", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.DisconnectLoadingAnimationCompletion, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "animation", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.OnLoadingAnimationCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "clipName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.EnsureSodRollCapTexture, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanAnimateSodRollCap, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
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
		if (method == MethodName.ActivatePresentation && args.Count == 0)
		{
			ActivatePresentation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BindBootstrapAtlasProfile && args.Count == 0)
		{
			BindBootstrapAtlasProfile();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateLoadingAnimationPlayback && args.Count == 2)
		{
			UpdateLoadingAnimationPlayback(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResumeLoadingAnimation && args.Count == 4)
		{
			ResumeLoadingAnimation(VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDeferredLoadingFont && args.Count == 0)
		{
			ApplyDeferredLoadingFont();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ActivateLoadingAnimations && args.Count == 0)
		{
			ActivateLoadingAnimations();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ActivateLoadingAnimation && args.Count == 1)
		{
			ActivateLoadingAnimation(VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectLoadingAnimationCompletion && args.Count == 1)
		{
			DisconnectLoadingAnimationCompletion(VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLoadingAnimationCompleted && args.Count == 1)
		{
			OnLoadingAnimationCompleted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnsureSodRollCapTexture && args.Count == 0)
		{
			EnsureSodRollCapTexture();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanAnimateSodRollCap && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanAnimateSodRollCap());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ResumeLoadingAnimation && args.Count == 4)
		{
			ResumeLoadingAnimation(VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3]));
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
		if (method == MethodName.ActivatePresentation)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.BindBootstrapAtlasProfile)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateLoadingAnimationPlayback)
		{
			return true;
		}
		if (method == MethodName.ResumeLoadingAnimation)
		{
			return true;
		}
		if (method == MethodName.ApplyDeferredLoadingFont)
		{
			return true;
		}
		if (method == MethodName.ActivateLoadingAnimations)
		{
			return true;
		}
		if (method == MethodName.ActivateLoadingAnimation)
		{
			return true;
		}
		if (method == MethodName.DisconnectLoadingAnimationCompletion)
		{
			return true;
		}
		if (method == MethodName.OnLoadingAnimationCompleted)
		{
			return true;
		}
		if (method == MethodName.EnsureSodRollCapTexture)
		{
			return true;
		}
		if (method == MethodName.CanAnimateSodRollCap)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._loadLabel)
		{
			_loadLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._progressBar)
		{
			_progressBar = VariantUtils.ConvertTo<TextureProgressBar>(in value);
			return true;
		}
		if (name == PropertyName._startButton)
		{
			_startButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._sodRollCap)
		{
			_sodRollCap = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._loadBarSprout)
		{
			_loadBarSprout = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._loadBarSprout2)
		{
			_loadBarSprout2 = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._loadBarSprout3)
		{
			_loadBarSprout3 = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._loadBarSprout4)
		{
			_loadBarSprout4 = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._loadBarZombieHead)
		{
			_loadBarZombieHead = VariantUtils.ConvertTo<AdobeAnimateSpriteBase>(in value);
			return true;
		}
		if (name == PropertyName._percentage)
		{
			_percentage = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._currentValue)
		{
			_currentValue = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._completedLoadingAnimationCount)
		{
			_completedLoadingAnimationCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.PresentationComplete)
		{
			value = VariantUtils.CreateFrom<bool>(PresentationComplete);
			return true;
		}
		if (name == PropertyName._loadLabel)
		{
			value = VariantUtils.CreateFrom(in _loadLabel);
			return true;
		}
		if (name == PropertyName._progressBar)
		{
			value = VariantUtils.CreateFrom(in _progressBar);
			return true;
		}
		if (name == PropertyName._startButton)
		{
			value = VariantUtils.CreateFrom(in _startButton);
			return true;
		}
		if (name == PropertyName._sodRollCap)
		{
			value = VariantUtils.CreateFrom(in _sodRollCap);
			return true;
		}
		if (name == PropertyName._loadBarSprout)
		{
			value = VariantUtils.CreateFrom(in _loadBarSprout);
			return true;
		}
		if (name == PropertyName._loadBarSprout2)
		{
			value = VariantUtils.CreateFrom(in _loadBarSprout2);
			return true;
		}
		if (name == PropertyName._loadBarSprout3)
		{
			value = VariantUtils.CreateFrom(in _loadBarSprout3);
			return true;
		}
		if (name == PropertyName._loadBarSprout4)
		{
			value = VariantUtils.CreateFrom(in _loadBarSprout4);
			return true;
		}
		if (name == PropertyName._loadBarZombieHead)
		{
			value = VariantUtils.CreateFrom(in _loadBarZombieHead);
			return true;
		}
		if (name == PropertyName._percentage)
		{
			value = VariantUtils.CreateFrom(in _percentage);
			return true;
		}
		if (name == PropertyName._currentValue)
		{
			value = VariantUtils.CreateFrom(in _currentValue);
			return true;
		}
		if (name == PropertyName._completedLoadingAnimationCount)
		{
			value = VariantUtils.CreateFrom(in _completedLoadingAnimationCount);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._loadLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._progressBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._startButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._sodRollCap, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._loadBarSprout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._loadBarSprout2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._loadBarSprout3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._loadBarSprout4, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName._loadBarZombieHead, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._percentage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName._currentValue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._completedLoadingAnimationCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.PresentationComplete, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._loadLabel, Variant.From(in _loadLabel));
		info.AddProperty(PropertyName._progressBar, Variant.From(in _progressBar));
		info.AddProperty(PropertyName._startButton, Variant.From(in _startButton));
		info.AddProperty(PropertyName._sodRollCap, Variant.From(in _sodRollCap));
		info.AddProperty(PropertyName._loadBarSprout, Variant.From(in _loadBarSprout));
		info.AddProperty(PropertyName._loadBarSprout2, Variant.From(in _loadBarSprout2));
		info.AddProperty(PropertyName._loadBarSprout3, Variant.From(in _loadBarSprout3));
		info.AddProperty(PropertyName._loadBarSprout4, Variant.From(in _loadBarSprout4));
		info.AddProperty(PropertyName._loadBarZombieHead, Variant.From(in _loadBarZombieHead));
		info.AddProperty(PropertyName._percentage, Variant.From(in _percentage));
		info.AddProperty(PropertyName._currentValue, Variant.From(in _currentValue));
		info.AddProperty(PropertyName._completedLoadingAnimationCount, Variant.From(in _completedLoadingAnimationCount));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._loadLabel, out var value))
		{
			_loadLabel = value.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._progressBar, out var value2))
		{
			_progressBar = value2.As<TextureProgressBar>();
		}
		if (info.TryGetProperty(PropertyName._startButton, out var value3))
		{
			_startButton = value3.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._sodRollCap, out var value4))
		{
			_sodRollCap = value4.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._loadBarSprout, out var value5))
		{
			_loadBarSprout = value5.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._loadBarSprout2, out var value6))
		{
			_loadBarSprout2 = value6.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._loadBarSprout3, out var value7))
		{
			_loadBarSprout3 = value7.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._loadBarSprout4, out var value8))
		{
			_loadBarSprout4 = value8.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._loadBarZombieHead, out var value9))
		{
			_loadBarZombieHead = value9.As<AdobeAnimateSpriteBase>();
		}
		if (info.TryGetProperty(PropertyName._percentage, out var value10))
		{
			_percentage = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._currentValue, out var value11))
		{
			_currentValue = value11.As<float>();
		}
		if (info.TryGetProperty(PropertyName._completedLoadingAnimationCount, out var value12))
		{
			_completedLoadingAnimationCount = value12.As<int>();
		}
	}
}
