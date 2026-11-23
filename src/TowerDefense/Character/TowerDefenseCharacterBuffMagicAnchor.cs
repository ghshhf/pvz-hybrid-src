using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Buff/TowerDefenseCharacterBuffMagicAnchor.cs")]
public class TowerDefenseCharacterBuffMagicAnchor : TowerDefenseCharacterBuffConfig
{
	public new class MethodName : TowerDefenseCharacterBuffConfig.MethodName
	{
		public new static readonly StringName _Init = "_Init";

		public new static readonly StringName CreateRuntimeInstance = "CreateRuntimeInstance";

		public new static readonly StringName Enter = "Enter";

		public new static readonly StringName EnterReadOnlyClient = "EnterReadOnlyClient";

		public static readonly StringName ShowBubble = "ShowBubble";

		public new static readonly StringName Step = "Step";

		public new static readonly StringName StepReadOnlyClient = "StepReadOnlyClient";

		public new static readonly StringName Exit = "Exit";

		public new static readonly StringName ExitReadOnlyClient = "ExitReadOnlyClient";

		public static readonly StringName HideBubble = "HideBubble";

		public new static readonly StringName Refresh = "Refresh";
	}

	public new class PropertyName : TowerDefenseCharacterBuffConfig.PropertyName
	{
		public static readonly StringName _bubbleSprite = "_bubbleSprite";

		public static readonly StringName time = "time";

		public static readonly StringName currentTime = "currentTime";
	}

	public new class SignalName : TowerDefenseCharacterBuffConfig.SignalName
	{
	}

	public const string BubbleKey = "MagicBubble";

	public const string SurfKey = "SurfWaveHold";

	private static PackedScene _bubbleScene;

	private AdobeAnimateSprite _bubbleSprite;

	[Export(PropertyHint.None, "")]
	public double time = 3.0;

	[Export(PropertyHint.None, "")]
	public double currentTime;

	public override void _Init()
	{
		key = "MagicBubble";
	}

	public override TowerDefenseCharacterBuffConfig CreateRuntimeInstance()
	{
		return new TowerDefenseCharacterBuffMagicAnchor
		{
			key = key,
			time = time,
			currentTime = currentTime,
			refresh = refresh,
			canFliter = canFliter
		};
	}

	public override void Enter()
	{
		if (!(character is TowerDefenseZombie) || character.instance.zombiePhysique == TowerDefenseEnum.ZOMBIE_PHYSIQUE.BOSS)
		{
			character.BuffDelete(key);
		}
		else if (character.instance.maskFlags == 0)
		{
			character.BuffDelete(key);
		}
		else
		{
			ShowBubble();
		}
	}

	public override void EnterReadOnlyClient()
	{
		ShowBubble();
	}

	private void ShowBubble()
	{
		if (!(key != "MagicBubble") && !GodotObject.IsInstanceValid(_bubbleSprite))
		{
			if (_bubbleScene == null)
			{
				_bubbleScene = GD.Load<PackedScene>("uid://dr7re5khwxxs1");
			}
			_bubbleSprite = _bubbleScene.Instantiate<AdobeAnimateSprite>(PackedScene.GenEditState.Disabled);
			Transform2D logicalGlobalTransform = character.GetLogicalGlobalTransform(character.spriteGroup);
			logicalGlobalTransform.Origin = character.GetLogicalGlobalPosition(character.sprite);
			character.AttachAnimatedStatusVisual(_bubbleSprite, character.headSlot, logicalGlobalTransform);
		}
	}

	public override bool Step(double delta)
	{
		currentTime += delta;
		if (character.die || character.nearDie || currentTime >= time)
		{
			return true;
		}
		character.timeScale *= 0.0;
		return false;
	}

	public override void StepReadOnlyClient(double delta)
	{
		Step(delta);
	}

	public override void Exit()
	{
		HideBubble();
	}

	public override void ExitReadOnlyClient()
	{
		HideBubble();
	}

	private void HideBubble()
	{
		if (GodotObject.IsInstanceValid(_bubbleSprite))
		{
			character.DetachAnimatedStatusVisual(_bubbleSprite);
			_bubbleSprite.QueueFree();
			_bubbleSprite = null;
		}
	}

	public override void Refresh(TowerDefenseCharacterBuffConfig config)
	{
		TowerDefenseCharacterBuffMagicAnchor towerDefenseCharacterBuffMagicAnchor = (TowerDefenseCharacterBuffMagicAnchor)config;
		time = Mathf.Max(time - currentTime, towerDefenseCharacterBuffMagicAnchor.time);
		currentTime = 0.0;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(11)
		{
			new MethodInfo(MethodName._Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CreateRuntimeInstance, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Enter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.EnterReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ShowBubble, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Step, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StepReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Exit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ExitReadOnlyClient, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.HideBubble, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Refresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "config", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Resource"), exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Init && args.Count == 0)
		{
			_Init();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateRuntimeInstance && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacterBuffConfig>(CreateRuntimeInstance());
			return true;
		}
		if (method == MethodName.Enter && args.Count == 0)
		{
			Enter();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnterReadOnlyClient && args.Count == 0)
		{
			EnterReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowBubble && args.Count == 0)
		{
			ShowBubble();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Step && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(Step(VariantUtils.ConvertTo<double>(in args[0])));
			return true;
		}
		if (method == MethodName.StepReadOnlyClient && args.Count == 1)
		{
			StepReadOnlyClient(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Exit && args.Count == 0)
		{
			Exit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExitReadOnlyClient && args.Count == 0)
		{
			ExitReadOnlyClient();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideBubble && args.Count == 0)
		{
			HideBubble();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Refresh && args.Count == 1)
		{
			Refresh(VariantUtils.ConvertTo<TowerDefenseCharacterBuffConfig>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Init)
		{
			return true;
		}
		if (method == MethodName.CreateRuntimeInstance)
		{
			return true;
		}
		if (method == MethodName.Enter)
		{
			return true;
		}
		if (method == MethodName.EnterReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.ShowBubble)
		{
			return true;
		}
		if (method == MethodName.Step)
		{
			return true;
		}
		if (method == MethodName.StepReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.Exit)
		{
			return true;
		}
		if (method == MethodName.ExitReadOnlyClient)
		{
			return true;
		}
		if (method == MethodName.HideBubble)
		{
			return true;
		}
		if (method == MethodName.Refresh)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._bubbleSprite)
		{
			_bubbleSprite = VariantUtils.ConvertTo<AdobeAnimateSprite>(in value);
			return true;
		}
		if (name == PropertyName.time)
		{
			time = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.currentTime)
		{
			currentTime = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._bubbleSprite)
		{
			value = VariantUtils.CreateFrom(in _bubbleSprite);
			return true;
		}
		if (name == PropertyName.time)
		{
			value = VariantUtils.CreateFrom(in time);
			return true;
		}
		if (name == PropertyName.currentTime)
		{
			value = VariantUtils.CreateFrom(in currentTime);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._bubbleSprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.time, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Float, PropertyName.currentTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bubbleSprite, Variant.From(in _bubbleSprite));
		info.AddProperty(PropertyName.time, Variant.From(in time));
		info.AddProperty(PropertyName.currentTime, Variant.From(in currentTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bubbleSprite, out var value))
		{
			_bubbleSprite = value.As<AdobeAnimateSprite>();
		}
		if (info.TryGetProperty(PropertyName.time, out var value2))
		{
			time = value2.As<double>();
		}
		if (info.TryGetProperty(PropertyName.currentTime, out var value3))
		{
			currentTime = value3.As<double>();
		}
	}
}
