internal readonly struct AdobeAnimateCrowdLayout
{
	public int StateTexelCount { get; }

	public int QuadCount { get; }

	public AdobeAnimateCrowdLayout(int stateTexelCount, int quadCount)
	{
		StateTexelCount = stateTexelCount;
		QuadCount = quadCount;
	}
}
