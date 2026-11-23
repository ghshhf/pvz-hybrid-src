using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Event/General/TowerDefenseCharacterEventPoisonMist.cs")]
public class TowerDefenseCharacterEventPoisonMist : TowerDefenseCharacterEventBase
{
	public new class MethodName : TowerDefenseCharacterEventBase.MethodName
	{
		public new static readonly StringName Execute = "Execute";

		public new static readonly StringName ExecuteDps = "ExecuteDps";

		public new static readonly StringName ExecuteProject = "ExecuteProject";

		public static readonly StringName Run = "Run";
	}

	public new class PropertyName : TowerDefenseCharacterEventBase.PropertyName
	{
		public static readonly StringName num = "num";

		public static readonly StringName cellGrid = "cellGrid";

		public static readonly StringName sourceCamp = "sourceCamp";
	}

	public new class SignalName : TowerDefenseCharacterEventBase.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public double num = 30.0;

	[Export(PropertyHint.None, "")]
	public Vector2I cellGrid;

	[Export(PropertyHint.None, "")]
	public TowerDefenseEnum.CHARACTER_CAMP sourceCamp = TowerDefenseEnum.CHARACTER_CAMP.NOONE;

	public override void Execute(Vector2 pos, TowerDefenseCharacter target)
	{
		Run(pos, target, num, cellGrid, sourceCamp);
	}

	public override void ExecuteDps(Vector2 pos, TowerDefenseCharacter target, double delta)
	{
		Run(pos, target, num * delta, cellGrid, sourceCamp);
	}

	public override void ExecuteProject(TowerDefenseProjectile projectile, TowerDefenseCharacter target)
	{
		Run(projectile.GlobalPosition, target, num, cellGrid, sourceCamp);
	}

	public static void Run(Vector2 pos, TowerDefenseCharacter target, double num = 30.0, Vector2I cellGrid = default(Vector2I), TowerDefenseEnum.CHARACTER_CAMP sourceCamp = TowerDefenseEnum.CHARACTER_CAMP.NOONE)
	{
		if (TowerDefenseManager.HasGameplayAuthority && !(target.gridPos != cellGrid) && !target.die && !target.isDestroy && (sourceCamp == TowerDefenseEnum.CHARACTER_CAMP.NOONE || target.camp != sourceCamp))
		{
			TowerDefenseCharacterEventHurt.Run(pos, target, num, 67, playSplatAudio: false);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(4)
		{
			new MethodInfo(MethodName.Execute, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ExecuteDps, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ExecuteProject, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projectile", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.Run, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Float, "num", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "cellGrid", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "sourceCamp", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Execute && args.Count == 2)
		{
			Execute(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExecuteDps && args.Count == 3)
		{
			ExecuteDps(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExecuteProject && args.Count == 2)
		{
			ExecuteProject(VariantUtils.ConvertTo<TowerDefenseProjectile>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Run && args.Count == 5)
		{
			Run(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]), VariantUtils.ConvertTo<Vector2I>(in args[3]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[4]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Run && args.Count == 5)
		{
			Run(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]), VariantUtils.ConvertTo<Vector2I>(in args[3]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[4]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Execute)
		{
			return true;
		}
		if (method == MethodName.ExecuteDps)
		{
			return true;
		}
		if (method == MethodName.ExecuteProject)
		{
			return true;
		}
		if (method == MethodName.Run)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.num)
		{
			num = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName.cellGrid)
		{
			cellGrid = VariantUtils.ConvertTo<Vector2I>(in value);
			return true;
		}
		if (name == PropertyName.sourceCamp)
		{
			sourceCamp = VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.num)
		{
			value = VariantUtils.CreateFrom(in num);
			return true;
		}
		if (name == PropertyName.cellGrid)
		{
			value = VariantUtils.CreateFrom(in cellGrid);
			return true;
		}
		if (name == PropertyName.sourceCamp)
		{
			value = VariantUtils.CreateFrom(in sourceCamp);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, PropertyName.num, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Vector2I, PropertyName.cellGrid, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Int, PropertyName.sourceCamp, PropertyHint.Enum, "NOONE:-1,PLANT:0,ZOMBIE:1,ALL:2", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.num, Variant.From(in num));
		info.AddProperty(PropertyName.cellGrid, Variant.From(in cellGrid));
		info.AddProperty(PropertyName.sourceCamp, Variant.From(in sourceCamp));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.num, out var value))
		{
			num = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName.cellGrid, out var value2))
		{
			cellGrid = value2.As<Vector2I>();
		}
		if (info.TryGetProperty(PropertyName.sourceCamp, out var value3))
		{
			sourceCamp = value3.As<TowerDefenseEnum.CHARACTER_CAMP>();
		}
	}
}
