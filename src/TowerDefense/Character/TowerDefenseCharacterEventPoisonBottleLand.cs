using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Resource/TowerDefense/Character/Event/General/TowerDefenseCharacterEventPoisonBottleLand.cs")]
public class TowerDefenseCharacterEventPoisonBottleLand : TowerDefenseCharacterEventBase
{
	public new class MethodName : TowerDefenseCharacterEventBase.MethodName
	{
		public new static readonly StringName Execute = "Execute";

		public new static readonly StringName ExecuteDps = "ExecuteDps";

		public new static readonly StringName ExecuteProject = "ExecuteProject";

		public new static readonly StringName ExecuteGroundProject = "ExecuteGroundProject";

		public static readonly StringName Run = "Run";
	}

	public new class PropertyName : TowerDefenseCharacterEventBase.PropertyName
	{
	}

	public new class SignalName : TowerDefenseCharacterEventBase.SignalName
	{
	}

	public override void Execute(Vector2 pos, TowerDefenseCharacter target)
	{
		Run(pos);
	}

	public override void ExecuteDps(Vector2 pos, TowerDefenseCharacter target, double delta)
	{
		Run(pos);
	}

	public override void ExecuteProject(TowerDefenseProjectile projectile, TowerDefenseCharacter target)
	{
		Run(projectile.GlobalPosition);
	}

	public override void ExecuteGroundProject(TowerDefenseProjectile projectile, Vector2 pos, Vector2I gridPos)
	{
		Run(pos);
	}

	public static void Run(Vector2 pos, TowerDefenseEnum.CHARACTER_CAMP sourceCamp = TowerDefenseEnum.CHARACTER_CAMP.ZOMBIE)
	{
		if (!TowerDefenseManager.HasGameplayAuthority)
		{
			return;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		Vector2I mapGridPos = instance.GetMapGridPos(pos);
		if (!instance.CheckMapGridPosIn(mapGridPos))
		{
			return;
		}
		List<TowerDefenseCharacter> list = new List<TowerDefenseCharacter>();
		instance.characterRegistry.FillCharactersIntersectingRectList(new Rect2(instance.GetMapCellPos(mapGridPos), instance.GetMapGridSize()), list);
		bool flag = false;
		foreach (TowerDefenseCharacter item in list)
		{
			if (item is TowerDefensePlant && item.camp != sourceCamp && item.gridPos == mapGridPos && !item.die && !item.isDestroy && (item.instance.maskFlags & 1) != 0)
			{
				flag = true;
				break;
			}
		}
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(mapGridPos);
		TowerDefenseItemPoisonMist.Spawn(flag ? "ItemPoisonMist" : "PoisonBottle", mapGridPos, GodotObject.IsInstanceValid(mapCell) ? mapCell.GetGroundHeight() : 0.0, sourceCamp == TowerDefenseEnum.CHARACTER_CAMP.PLANT);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(5)
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
			new MethodInfo(MethodName.ExecuteGroundProject, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "projectile", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Vector2I, "gridPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Run, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
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
		if (method == MethodName.ExecuteGroundProject && args.Count == 3)
		{
			ExecuteGroundProject(VariantUtils.ConvertTo<TowerDefenseProjectile>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Vector2I>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Run && args.Count == 2)
		{
			Run(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Run && args.Count == 2)
		{
			Run(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<TowerDefenseEnum.CHARACTER_CAMP>(in args[1]));
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
		if (method == MethodName.ExecuteGroundProject)
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
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
