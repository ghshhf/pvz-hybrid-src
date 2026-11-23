using Godot;
using Godot.Collections;

public readonly struct PacketSpawnState
{
	public int SyncId { get; }

	public string PacketName { get; }

	public Vector2 Position { get; }

	public double AliveTime { get; }

	public bool IsFall { get; }

	public bool UseCost { get; }

	public Vector2 Velocity { get; }

	public int ZIndex { get; }

	public double FallHeight { get; }

	public double Height { get; }

	public Dictionary RuntimeState { get; }

	public EconomyAccountId SunAccountId { get; }

	public double Gravity { get; }

	public PacketSpawnState(int syncId, string packetName, Vector2 position, double aliveTime, bool isFall, bool useCost, Vector2 velocity, int zIndex, double fallHeight, double height = 1.0, Dictionary runtimeState = null, EconomyAccountId sunAccountId = default(EconomyAccountId), double gravity = 980.0)
	{
		SyncId = syncId;
		PacketName = packetName ?? "";
		Position = position;
		AliveTime = aliveTime;
		IsFall = isFall;
		UseCost = useCost;
		Velocity = velocity;
		ZIndex = zIndex;
		FallHeight = fallHeight;
		Height = height;
		RuntimeState = runtimeState;
		SunAccountId = sunAccountId;
		Gravity = gravity;
	}
}
