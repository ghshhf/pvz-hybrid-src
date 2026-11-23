using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Registry/Battle/Process/Sandbox/TowerDefenseBattleProcessSandbox.cs")]
public class TowerDefenseBattleProcessSandbox : TowerDefenseBattleProcess, ITowerDefenseProgressSaveGuard
{
	public new class MethodName : TowerDefenseBattleProcess.MethodName
	{
		public new static readonly StringName PhysicsProcess = "PhysicsProcess";

		public new static readonly StringName ZombieEnterHouse = "ZombieEnterHouse";

		public new static readonly StringName CanLoadProgress = "CanLoadProgress";

		public new static readonly StringName CanFinish = "CanFinish";

		public new static readonly StringName Destroy = "Destroy";
	}

	public new class PropertyName : TowerDefenseBattleProcess.PropertyName
	{
		public static readonly StringName IsReady = "IsReady";

		public static readonly StringName SimulationTicks = "SimulationTicks";

		public static readonly StringName ElapsedSeconds = "ElapsedSeconds";
	}

	public new class SignalName : TowerDefenseBattleProcess.SignalName
	{
	}

	public bool IsReady { get; private set; }

	public long SimulationTicks { get; private set; }

	public double ElapsedSeconds { get; private set; }

	public override Task GameStart()
	{
		control.waitPause = true;
		control.state.ProcessMode = Node.ProcessModeEnum.Pausable;
		control.buttonPause.Visible = false;
		control.optionButton.Visible = false;
		control.checkBox2X.Visible = false;
		IsReady = true;
		if (!Global.IsMultiplayerMode)
		{
			GetTree().Paused = true;
		}
		return Task.CompletedTask;
	}

	public override void PhysicsProcess(double delta)
	{
		if (IsReady && !Global.IsMultiplayerMode && !GetTree().Paused)
		{
			SimulationTicks++;
			ElapsedSeconds += delta;
		}
	}

	public override void ZombieEnterHouse(TowerDefenseCharacter character)
	{
		if (!Global.IsMultiplayerMode && GodotObject.IsInstanceValid(character))
		{
			character.skipDestroySet = true;
			character.suppressDeathrattles = true;
			character.Destroy();
		}
	}

	public override bool CanLoadProgress()
	{
		return false;
	}

	public bool CanSaveProgress(out string reason)
	{
		reason = "沙盒请使用保存方案。";
		return false;
	}

	public override bool CanFinish()
	{
		if (base.CanFinish())
		{
			return false;
		}
		return false;
	}

	public override void Destroy()
	{
		IsReady = false;
		base.Destroy();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(5)
		{
			new MethodInfo(MethodName.PhysicsProcess, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ZombieEnterHouse, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "character", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.CanLoadProgress, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CanFinish, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.Destroy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.PhysicsProcess && args.Count == 1)
		{
			PhysicsProcess(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ZombieEnterHouse && args.Count == 1)
		{
			ZombieEnterHouse(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanLoadProgress && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanLoadProgress());
			return true;
		}
		if (method == MethodName.CanFinish && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanFinish());
			return true;
		}
		if (method == MethodName.Destroy && args.Count == 0)
		{
			Destroy();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.PhysicsProcess)
		{
			return true;
		}
		if (method == MethodName.ZombieEnterHouse)
		{
			return true;
		}
		if (method == MethodName.CanLoadProgress)
		{
			return true;
		}
		if (method == MethodName.CanFinish)
		{
			return true;
		}
		if (method == MethodName.Destroy)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsReady)
		{
			IsReady = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.SimulationTicks)
		{
			SimulationTicks = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName.ElapsedSeconds)
		{
			ElapsedSeconds = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsReady)
		{
			value = VariantUtils.CreateFrom<bool>(IsReady);
			return true;
		}
		if (name == PropertyName.SimulationTicks)
		{
			value = VariantUtils.CreateFrom<long>(SimulationTicks);
			return true;
		}
		if (name == PropertyName.ElapsedSeconds)
		{
			value = VariantUtils.CreateFrom<double>(ElapsedSeconds);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsReady, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.SimulationTicks, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Float, PropertyName.ElapsedSeconds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsReady, Variant.From<bool>(IsReady));
		info.AddProperty(PropertyName.SimulationTicks, Variant.From<long>(SimulationTicks));
		info.AddProperty(PropertyName.ElapsedSeconds, Variant.From<double>(ElapsedSeconds));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsReady, out var value))
		{
			IsReady = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.SimulationTicks, out var value2))
		{
			SimulationTicks = value2.As<long>();
		}
		if (info.TryGetProperty(PropertyName.ElapsedSeconds, out var value3))
		{
			ElapsedSeconds = value3.As<double>();
		}
	}
}
