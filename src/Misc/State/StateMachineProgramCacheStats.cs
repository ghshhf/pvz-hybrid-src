public readonly struct StateMachineProgramCacheStats
{
	public readonly int EntryCount;

	public readonly int ActiveEntryCount;

	public readonly int PinnedEntryCount;

	public readonly int TotalReferences;

	public StateMachineProgramCacheStats(int entryCount, int activeEntryCount, int pinnedEntryCount, int totalReferences)
	{
		EntryCount = entryCount;
		ActiveEntryCount = activeEntryCount;
		PinnedEntryCount = pinnedEntryCount;
		TotalReferences = totalReferences;
	}
}
