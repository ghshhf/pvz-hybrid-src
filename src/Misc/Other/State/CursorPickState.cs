public readonly struct CursorPickState
{
	public string PickType { get; }

	public string PickName { get; }

	public CursorPickState(string pickType, string pickName)
	{
		PickType = pickType ?? "";
		PickName = pickName ?? "";
	}
}
