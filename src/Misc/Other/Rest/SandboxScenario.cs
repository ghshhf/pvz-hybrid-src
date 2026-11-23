using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public sealed class SandboxScenario
{
	public const int MaximumCharacters = 500;

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	private static readonly SandboxJsonContext JsonContext = new SandboxJsonContext(JsonOptions);

	[JsonRequired]
	public string Format { get; set; } = "PVZHE-Sandbox";

	[JsonRequired]
	public int Version { get; set; } = 1;

	[JsonRequired]
	public string Map { get; set; } = "Frontlawn";

	public bool AllowStacking { get; set; }

	[JsonRequired]
	public List<SandboxPlacement> Characters { get; set; } = new List<SandboxPlacement>();

	public SandboxScenario Copy()
	{
		return JsonSerializer.Deserialize(ToJson(), JsonContext.SandboxScenario);
	}

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, JsonContext.SandboxScenario);
	}

	public static bool TryRead(string json, out SandboxScenario scenario, out string error)
	{
		scenario = null;
		error = "方案文件为空或过大。";
		if (string.IsNullOrWhiteSpace(json) || json.Length > 1048576)
		{
			return false;
		}
		try
		{
			SandboxScenario sandboxScenario = JsonSerializer.Deserialize(json, JsonContext.SandboxScenario);
			if (sandboxScenario == null || !sandboxScenario.Validate(out error))
			{
				return false;
			}
			scenario = sandboxScenario;
			return true;
		}
		catch (JsonException)
		{
			error = "方案 JSON 格式错误或包含不支持的字段。";
			return false;
		}
	}

	public bool Validate(out string error)
	{
		error = "请选择有效的沙盒方案。";
		if (Format != "PVZHE-Sandbox" || Version != 1 || Characters == null || Characters.Count > 500 || string.IsNullOrWhiteSpace(Map))
		{
			return false;
		}
		Resource value;
		TowerDefenseMapConfig towerDefenseMapConfig = (ResourceManager.Instance.MAPS.TryGetValue(Map, out value) ? (value as TowerDefenseMapConfig) : null);
		if (!GodotObject.IsInstanceValid(towerDefenseMapConfig))
		{
			error = "地图不存在：" + Map;
			return false;
		}
		Dictionary<Vector2I, List<SandboxPlacement>> dictionary = new Dictionary<Vector2I, List<SandboxPlacement>>();
		foreach (SandboxPlacement placement in Characters)
		{
			if (placement == null || !placement.Validate(towerDefenseMapConfig, out error))
			{
				return false;
			}
			TowerDefenseCharacterConfig config = TowerDefenseManager.GetPacketConfig(placement.Packet).characterConfig;
			foreach (Vector2I plantingCell in GetPlantingCells(config, new Vector2I(placement.Column, placement.Row)))
			{
				if (!dictionary.TryGetValue(plantingCell, out var value2))
				{
					value2 = new List<SandboxPlacement>();
					dictionary.Add(plantingCell, value2);
				}
				if (!AllowStacking && value2.Exists((SandboxPlacement other) => !CanSharePlantingCell(config, placement.Team, TowerDefenseManager.GetPacketConfig(other.Packet).characterConfig, other.Team)))
				{
					error = "该格已有同层角色；底座与植物可正常共存，额外重叠请开启允许叠放。";
					return false;
				}
				value2.Add(placement);
			}
		}
		error = "";
		return true;
	}

	internal static bool CanSharePlantingCell(TowerDefenseCharacterConfig leftConfig, int leftTeam, TowerDefenseCharacterConfig rightConfig, int rightTeam)
	{
		if (leftTeam != rightTeam || !(leftConfig is TowerDefensePlantConfig towerDefensePlantConfig) || !(rightConfig is TowerDefensePlantConfig towerDefensePlantConfig2))
		{
			return false;
		}
		if (towerDefensePlantConfig.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.SURROUND) != towerDefensePlantConfig2.plantGridType.Contains(TowerDefenseEnum.PLANTGRIDTYPE.SURROUND))
		{
			return true;
		}
		if (towerDefensePlantConfig.plantGridOverrideType == TowerDefenseEnum.PLANTGRIDTYPE.NOONE || towerDefensePlantConfig.plantGridOverrideType == towerDefensePlantConfig2.plantGridOverrideType || !towerDefensePlantConfig2.plantGridType.Contains(towerDefensePlantConfig.plantGridOverrideType))
		{
			if (towerDefensePlantConfig2.plantGridOverrideType != TowerDefenseEnum.PLANTGRIDTYPE.NOONE && towerDefensePlantConfig.plantGridOverrideType != towerDefensePlantConfig2.plantGridOverrideType)
			{
				return towerDefensePlantConfig.plantGridType.Contains(towerDefensePlantConfig2.plantGridOverrideType);
			}
			return false;
		}
		return true;
	}

	internal static HashSet<Vector2I> GetPlantingCells(TowerDefenseCharacterConfig config, Vector2I grid)
	{
		HashSet<Vector2I> hashSet = new HashSet<Vector2I> { grid };
		if (config is TowerDefensePlantConfig towerDefensePlantConfig)
		{
			foreach (Vector2I item in towerDefensePlantConfig.extendGrid)
			{
				hashSet.Add(grid + item);
			}
		}
		return hashSet;
	}
}
