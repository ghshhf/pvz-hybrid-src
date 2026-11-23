public struct CardCostContext
{
	public TowerDefensePacketConfig Packet;

	public readonly int BaseCost;

	public int Cost;

	public bool CanIncrease;

	public bool StopCurrentStage;

	public CardCostContext(TowerDefensePacketConfig packet, int baseCost)
	{
		Packet = packet;
		BaseCost = baseCost;
		Cost = baseCost;
		CanIncrease = packet?.canChangeCost ?? true;
		StopCurrentStage = false;
	}
}
