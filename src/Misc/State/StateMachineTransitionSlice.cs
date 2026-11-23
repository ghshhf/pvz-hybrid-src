public readonly struct StateMachineTransitionSlice
{
	public readonly int Offset;

	public readonly int Count;

	public StateMachineTransitionSlice(int offset, int count)
	{
		Offset = offset;
		Count = count;
	}
}
