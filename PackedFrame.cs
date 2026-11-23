public readonly struct PackedFrame
{
	public int Offset { get; }

	public int Count { get; }

	public PackedFrame(int offset, int count)
	{
		Offset = offset;
		Count = count;
	}
}
