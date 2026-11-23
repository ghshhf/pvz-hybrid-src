using Godot;

internal readonly struct TowerDefenseDeathRecord
{
	public TowerDefensePacketConfig Packet { get; }

	public Vector2 Position { get; }

	public Vector2I GridPosition { get; }

	public TowerDefenseEnum.CHARACTER_CAMP Camp { get; }

	public double Scale { get; }

	public double HitpointScale { get; }

	public bool Invisible { get; }

	public bool CanAngelRevive { get; }

	public TowerDefenseDeathRecord(TowerDefensePacketConfig packet, Vector2 position, Vector2I gridPosition, TowerDefenseEnum.CHARACTER_CAMP camp, double scale, double hitpointScale, bool invisible, bool canAngelRevive)
	{
		Packet = packet;
		Position = position;
		GridPosition = gridPosition;
		Camp = camp;
		Scale = scale;
		HitpointScale = hitpointScale;
		Invisible = invisible;
		CanAngelRevive = canAngelRevive;
	}
}
