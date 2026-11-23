using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[GlobalClass]
[ScriptPath("res://Asset/Anime/Character/Item/ItemPoisonMist/Scene/TowerDefenseItemPoisonMist.cs")]
public class TowerDefenseItemPoisonMist : TowerDefenseCharacter
{
	public new class MethodName : TowerDefenseCharacter.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName IdleProcessing = "IdleProcessing";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName Spawn = "Spawn";

		public new static readonly StringName ExportVariantSave = "ExportVariantSave";

		public new static readonly StringName ImportVariantSave = "ImportVariantSave";
	}

	public new class PropertyName : TowerDefenseCharacter.PropertyName
	{
		public static readonly StringName _ticks = "_ticks";
	}

	public new class SignalName : TowerDefenseCharacter.SignalName
	{
	}

	private const double TickInterval = 1.0;

	private const int MaxTickCount = 10;

	private CharacterTimerComponent _timerComponent;

	private int _ticks;

	private readonly List<TowerDefenseCharacter> _targets = new List<TowerDefenseCharacter>();

	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint() && !editorPreviewMode)
		{
			instance.invincible = true;
			shadowSprite.Visible = false;
			base.sprite.SetAnimation("Fire");
			_timerComponent = componentManager.GetRuntime<CharacterTimerComponent>("character.timer");
			CharacterTimerComponent timerComponent = _timerComponent;
			if (timerComponent != null && !timerComponent.IsReleased)
			{
				_timerComponent.OnTimeout += OnTimerTimeout;
			}
		}
	}

	public override void IdleProcessing(double delta)
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.timeScale = timeScale;
		}
		base.IdleProcessing(delta);
		if (inGame && !isDestroy && TowerDefenseManager.HasGameplayAuthority)
		{
			CharacterTimerComponent timerComponent2 = _timerComponent;
			if (timerComponent2 != null && !timerComponent2.IsReleased && !_timerComponent.IsRunning("Tick"))
			{
				_timerComponent.Run("Tick", 1.0);
			}
		}
	}

	private void OnTimerTimeout(string timerName)
	{
		if (timerName != "Tick" || !inGame || isDestroy)
		{
			return;
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		_targets.Clear();
		towerDefenseManager.characterRegistry.FillCharactersIntersectingRectList(new Rect2(towerDefenseManager.GetMapCellPos(base.gridPos), towerDefenseManager.GetMapGridSize()), _targets);
		foreach (TowerDefenseCharacter target in _targets)
		{
			TowerDefenseCharacterEventPoisonMist.Run(GetLogicalGlobalPosition(), target, 30.0, base.gridPos, base.camp);
		}
		_ticks++;
		if (_ticks >= 10)
		{
			Destroy();
		}
	}

	public override void _ExitTree()
	{
		CharacterTimerComponent timerComponent = _timerComponent;
		if (timerComponent != null && !timerComponent.IsReleased)
		{
			_timerComponent.OnTimeout -= OnTimerTimeout;
		}
		base._ExitTree();
	}

	public static TowerDefenseCharacter Spawn(string key, Vector2I grid, double height = 0.0, bool hypnoses = false)
	{
		if (!TowerDefenseManager.HasGameplayAuthority)
		{
			return null;
		}
		TowerDefenseManager towerDefenseManager = TowerDefenseManager.Instance;
		if (!towerDefenseManager.CheckMapGridPosIn(grid))
		{
			return null;
		}
		TowerDefenseCharacter towerDefenseCharacter = TowerDefenseManager.GetPacketConfig(key).Create(towerDefenseManager.GetMapCellPosCenter(grid), grid, height);
		TowerDefenseManager.GetCharacterNode().AddChild(towerDefenseCharacter, forceReadableName: false, InternalMode.Disabled);
		if (hypnoses && GodotObject.IsInstanceValid(towerDefenseCharacter))
		{
			towerDefenseCharacter.Hypnoses();
		}
		TowerDefenseManager.PublishSpawnedCharacter(key, towerDefenseCharacter, useCreate: true, 0.0, walkAfterSpawn: false, "", towerDefenseCharacter.ExportNetworkSpawnState());
		return towerDefenseCharacter;
	}

	public override Dictionary ExportVariantSave()
	{
		Dictionary dictionary = base.ExportVariantSave();
		dictionary["poisonTickCount"] = _ticks;
		return dictionary;
	}

	public override void ImportVariantSave(Dictionary data)
	{
		base.ImportVariantSave(data);
		_ticks = data.GetValueOrDefault("poisonTickCount", 0).AsInt32();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(7)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.IdleProcessing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Spawn, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "key", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "grid", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "height", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "hypnoses", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExportVariantSave, new PropertyInfo(Variant.Type.Dictionary, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ImportVariantSave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.IdleProcessing && args.Count == 1)
		{
			IdleProcessing(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 1)
		{
			OnTimerTimeout(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Spawn && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(Spawn(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Vector2I>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
			return true;
		}
		if (method == MethodName.ExportVariantSave && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Dictionary>(ExportVariantSave());
			return true;
		}
		if (method == MethodName.ImportVariantSave && args.Count == 1)
		{
			ImportVariantSave(VariantUtils.ConvertTo<Dictionary>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Spawn && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<TowerDefenseCharacter>(Spawn(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Vector2I>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
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
		if (method == MethodName.IdleProcessing)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Spawn)
		{
			return true;
		}
		if (method == MethodName.ExportVariantSave)
		{
			return true;
		}
		if (method == MethodName.ImportVariantSave)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._ticks)
		{
			_ticks = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._ticks)
		{
			value = VariantUtils.CreateFrom(in _ticks);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, PropertyName._ticks, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._ticks, Variant.From(in _ticks));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._ticks, out var value))
		{
			_ticks = value.As<int>();
		}
	}
}
