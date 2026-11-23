using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/HybridPark/CoffeePot/Scene/TowerDefensePlantCoffeePot.cs")]
public class TowerDefensePlantCoffeePot : TowerDefensePlant
{
	public new class MethodName : TowerDefensePlant.MethodName
	{
		public new static readonly StringName DestroySet = "DestroySet";
	}

	public new class PropertyName : TowerDefensePlant.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlant.SignalName
	{
	}

	public const string HasteBuffKey = "CoffeePotHaste";

	public const double HasteTimeScaleValue = 2.0;

	public const double HasteDuration = 30.0;

	public override void DestroySet()
	{
		base.DestroySet();
		if (!TowerDefenseManager.HasGameplayAuthority || isShovel || suppressDeathrattles)
		{
			return;
		}
		TowerDefenseCellInstance mapCell = TowerDefenseManager.GetMapCell(base.gridPos);
		if (!GodotObject.IsInstanceValid(mapCell))
		{
			return;
		}
		Array<TowerDefenseCharacterBuffConfig> buffList = new Array<TowerDefenseCharacterBuffConfig>
		{
			new TowerDefenseCharacterBuffOverallHaste
			{
				key = "CoffeePotHaste",
				timeScaleValue = 2.0,
				time = 30.0
			}
		};
		foreach (TowerDefenseCharacter character in mapCell.GetCharacterList())
		{
			if (GodotObject.IsInstanceValid(character) && character != this && character is TowerDefensePlant && character.camp == base.camp && !character.die && !character.isDestroy)
			{
				BuffComponent buffComponent = character.buff;
				if (buffComponent != null && !buffComponent.IsReleased)
				{
					TowerDefenseCharacterEventAddBuff.Run(character, buffList);
				}
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(1)
		{
			new MethodInfo(MethodName.DestroySet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.DestroySet && args.Count == 0)
		{
			DestroySet();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.DestroySet)
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
