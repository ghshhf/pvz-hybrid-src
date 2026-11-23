using System;
using System.Text.Json.Serialization;
using Godot;

public sealed class SandboxPlacement
{
	[JsonRequired]
	public string Packet { get; set; } = "";

	[JsonRequired]
	public int Team { get; set; }

	[JsonRequired]
	public int Column { get; set; }

	[JsonRequired]
	public int Row { get; set; }

	[JsonRequired]
	public int Facing { get; set; } = 1;

	public double HealthScale { get; set; } = 1.0;

	public double ActionSpeed { get; set; } = 1.0;

	public bool AutoAttack { get; set; } = true;

	public float OffsetX { get; set; }

	public SandboxPlacement Copy()
	{
		return (SandboxPlacement)MemberwiseClone();
	}

	public bool Validate(TowerDefenseMapConfig map, out string error)
	{
		error = "角色、阵营、位置或倍率无效。";
		if (Team < 0 || Team > 1 || (Facing != -1 && Facing != 1) || Column < 1 || Row < 1 || Column > map.gridNum.X || Row > map.gridNum.Y || !double.IsFinite(HealthScale) || HealthScale < 0.1 || HealthScale > 100.0 || !double.IsFinite(ActionSpeed) || ActionSpeed < 0.1 || ActionSpeed > 10.0 || !float.IsFinite(OffsetX) || Math.Abs(OffsetX) > map.gridSize.X)
		{
			return false;
		}
		Resource value;
		TowerDefensePacketConfig towerDefensePacketConfig = ((!string.IsNullOrWhiteSpace(Packet) && ResourceManager.Instance.TOWERDEFENSE_PACKETS.TryGetValue(Packet, out value)) ? (value as TowerDefensePacketConfig) : null);
		if (!IsSupportedPacket(towerDefensePacketConfig))
		{
			error = "角色不存在或需要专用目标：" + Packet;
			return false;
		}
		if (towerDefensePacketConfig.characterConfig is TowerDefensePlantConfig towerDefensePlantConfig)
		{
			foreach (Vector2I item in towerDefensePlantConfig.extendGrid)
			{
				if (Column + item.X < 1 || Column + item.X > map.gridNum.X || Row + item.Y < 1 || Row + item.Y > map.gridNum.Y)
				{
					error = "多格角色超出场地边界。";
					return false;
				}
			}
		}
		error = "";
		return true;
	}

	public static bool IsSupportedPacket(TowerDefensePacketConfig packet)
	{
		if (GodotObject.IsInstanceValid(packet) && !packet.requiresPlantTarget)
		{
			if (!(packet.characterConfig is TowerDefensePlantConfig))
			{
				return packet.characterConfig is TowerDefenseZombieConfig;
			}
			return true;
		}
		return false;
	}
}
