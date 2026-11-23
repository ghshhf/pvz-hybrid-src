using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModLevelPreparation
{
	private sealed record PreparationSource(TowerDefenseLevelBaseConfig Source);

	private static readonly ConditionalWeakTable<TowerDefenseLevelBaseConfig, PreparationSource> Prepared = new ConditionalWeakTable<TowerDefenseLevelBaseConfig, PreparationSource>();

	internal static TowerDefenseLevelBaseConfig CopySelectedSource(TowerDefenseLevelBaseConfig source)
	{
		if (!GodotObject.IsInstanceValid(source))
		{
			throw new ArgumentException("Missing level config.");
		}
		PreparationSource value;
		return (TowerDefenseLevelBaseConfig)(Prepared.TryGetValue(source, out value) ? value.Source : source).Duplicate(deep: true);
	}

	internal static TowerDefenseLevelBaseConfig CopyAuthoritative(TowerDefenseLevelBaseConfig source)
	{
		TowerDefenseLevelBaseConfig obj = (TowerDefenseLevelBaseConfig)source.Duplicate(deep: true);
		AssociateAuthoritativeCopy(obj, source);
		return obj;
	}

	internal static void AssociateAuthoritativeCopy(TowerDefenseLevelBaseConfig received, TowerDefenseLevelBaseConfig prepared)
	{
		if (GodotObject.IsInstanceValid(received) && GodotObject.IsInstanceValid(prepared) && received != prepared && Prepared.TryGetValue(prepared, out var value))
		{
			Prepared.Remove(received);
			Prepared.Add(received, value);
		}
	}

	internal static TowerDefenseLevelBaseConfig PrepareForLaunch(TowerDefenseLevelBaseConfig source, bool restoring)
	{
		if (!GodotObject.IsInstanceValid(source))
		{
			throw new ArgumentException("Missing level config.");
		}
		if (Prepared.TryGetValue(source, out var _))
		{
			return source;
		}
		TowerDefenseLevelBaseConfig towerDefenseLevelBaseConfig = (TowerDefenseLevelBaseConfig)source.Duplicate(deep: true);
		towerDefenseLevelBaseConfig.Init();
		Prepared.Add(towerDefenseLevelBaseConfig, new PreparationSource(source));
		if (restoring || (Global.IsMultiplayerMode && !MultiPlayerManager.IsHost) || !XWModExtensions.HasHandlers(XWModLevelPoints.Prepare.Id))
		{
			return towerDefenseLevelBaseConfig;
		}
		Dictionary dictionary;
		if (!(towerDefenseLevelBaseConfig is TowerDefenseLevelConfig towerDefenseLevelConfig))
		{
			if (!(towerDefenseLevelBaseConfig is TowerDefenseLevelNewConfig towerDefenseLevelNewConfig))
			{
				throw new InvalidOperationException("Unsupported level configuration type.");
			}
			dictionary = towerDefenseLevelNewConfig.Export();
		}
		else
		{
			dictionary = towerDefenseLevelConfig.Export();
		}
		Dictionary dictionary2 = dictionary;
		Json json = ((towerDefenseLevelBaseConfig is TowerDefenseLevelConfig towerDefenseLevelConfig2) ? towerDefenseLevelConfig2.data : ((!(towerDefenseLevelBaseConfig is TowerDefenseLevelNewConfig towerDefenseLevelNewConfig2)) ? null : towerDefenseLevelNewConfig2.data));
		Json json2 = json;
		Dictionary dictionary3 = ((GodotObject.IsInstanceValid(json2) && json2.Data.VariantType == Variant.Type.Dictionary) ? json2.Data.AsGodotDictionary().Duplicate(deep: true) : new Dictionary());
		foreach (KeyValuePair<Variant, Variant> item in dictionary2)
		{
			dictionary3[item.Key] = item.Value;
		}
		dictionary3 = dictionary3.Duplicate(deep: true);
		Godot.Collections.Array features = dictionary3["Feature"].AsGodotArray();
		Dictionary dictionary4 = Find("Map");
		Dictionary dictionary5 = Find("Sun");
		Dictionary dictionary6 = Find("Wave");
		string processName = dictionary3.GetValueOrDefault("Process", new Dictionary()).AsGodotDictionary().GetValueOrDefault("Name", "")
			.AsString();
		XWModLevelPreparationValues value2 = new XWModLevelPreparationValues(towerDefenseLevelBaseConfig.name, processName, dictionary4?.GetValueOrDefault("MapName", "").AsString(), dictionary5?.GetValueOrDefault("Begin", 300L).AsInt64(), dictionary6?.GetValueOrDefault("BeginCol", 20.0).AsDouble());
		XWModLevelPreparationValues xWModLevelPreparationValues;
		try
		{
			xWModLevelPreparationValues = XWModExtensions.Modify(XWModLevelPoints.Prepare, value2, XWModExtensions.CreateEventContext(XWModEventOrigin.InitialBinding, XWModAuthority.Host, "level-prepare"));
		}
		catch (XWModHookCallbackException ex)
		{
			towerDefenseLevelBaseConfig.Dispose();
			throw new InvalidOperationException("Mod level preparation failed: " + ex.Message, ex);
		}
		if (dictionary4 != null)
		{
			dictionary4["MapName"] = xWModLevelPreparationValues.MapKey;
		}
		if (dictionary5 != null)
		{
			dictionary5["Begin"] = xWModLevelPreparationValues.InitialSun.Value;
		}
		if (dictionary6 != null)
		{
			dictionary6["BeginCol"] = xWModLevelPreparationValues.FirstWaveDelay.Value;
		}
		Json data = new Json
		{
			Data = dictionary3
		};
		if (towerDefenseLevelBaseConfig is TowerDefenseLevelConfig towerDefenseLevelConfig3)
		{
			towerDefenseLevelConfig3.data = data;
		}
		else
		{
			((TowerDefenseLevelNewConfig)towerDefenseLevelBaseConfig).data = data;
		}
		return towerDefenseLevelBaseConfig;
		Dictionary Find(string id)
		{
			foreach (Variant item2 in features)
			{
				Dictionary dictionary7 = item2.AsGodotDictionary();
				if (dictionary7.GetValueOrDefault("Name", "").AsString() == id)
				{
					return dictionary7["Data"].AsGodotDictionary();
				}
			}
			return null;
		}
	}

	internal static bool Validate(XWModLevelPreparationValues original, XWModLevelPreparationValues value)
	{
		if (value == null || value.LevelKey != original.LevelKey || value.ProcessName != original.ProcessName || value.InitialSun.HasValue != original.InitialSun.HasValue || value.FirstWaveDelay.HasValue != original.FirstWaveDelay.HasValue || value.MapKey == null != (original.MapKey == null))
		{
			return false;
		}
		if (value.FirstWaveDelay != original.FirstWaveDelay && (!value.FirstWaveDelay.HasValue || !double.IsFinite(value.FirstWaveDelay.Value) || value.FirstWaveDelay < 0.0 || value.FirstWaveDelay > 86400.0))
		{
			return false;
		}
		if (value.MapKey == original.MapKey)
		{
			return true;
		}
		TowerDefenseManager instance = TowerDefenseManager.Instance;
		if (!GodotObject.IsInstanceValid(instance) || string.IsNullOrWhiteSpace(value.MapKey))
		{
			return false;
		}
		return MapsCompatible(instance.GetMapConfig(original.MapKey), instance.GetMapConfig(value.MapKey));
	}

	internal static bool MapsCompatible(TowerDefenseMapConfig before, TowerDefenseMapConfig after)
	{
		if (!GodotObject.IsInstanceValid(before) || !GodotObject.IsInstanceValid(after))
		{
			return false;
		}
		if (before.gridNum != after.gridNum || before.gridBeginPos != after.gridBeginPos || before.gridSize != after.gridSize || before.mapOffset != after.mapOffset || before.mapSize != after.mapSize || before.plantOffset != after.plantOffset || before.edge != after.edge || !before.lineUse.SequenceEqual(after.lineUse) || before.cellConfig.Count != after.cellConfig.Count)
		{
			return false;
		}
		for (int i = 0; i < before.cellConfig.Count; i++)
		{
			TowerDefenseCellConfig towerDefenseCellConfig = before.cellConfig[i];
			TowerDefenseCellConfig towerDefenseCellConfig2 = after.cellConfig[i];
			if (towerDefenseCellConfig != towerDefenseCellConfig2)
			{
				if (!GodotObject.IsInstanceValid(towerDefenseCellConfig) || !GodotObject.IsInstanceValid(towerDefenseCellConfig2) || towerDefenseCellConfig.pos != towerDefenseCellConfig2.pos || towerDefenseCellConfig.ElementFlags != towerDefenseCellConfig2.ElementFlags || !towerDefenseCellConfig.gridType.SequenceEqual(towerDefenseCellConfig2.gridType))
				{
					return false;
				}
				if (towerDefenseCellConfig.groundHeightCurve != towerDefenseCellConfig2.groundHeightCurve && (!GodotObject.IsInstanceValid(towerDefenseCellConfig.groundHeightCurve) || !GodotObject.IsInstanceValid(towerDefenseCellConfig2.groundHeightCurve) || string.IsNullOrEmpty(towerDefenseCellConfig.groundHeightCurve.ResourcePath) || towerDefenseCellConfig.groundHeightCurve.ResourcePath != towerDefenseCellConfig2.groundHeightCurve.ResourcePath))
				{
					return false;
				}
			}
		}
		return true;
	}
}
