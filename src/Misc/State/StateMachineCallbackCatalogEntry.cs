public readonly struct StateMachineCallbackCatalogEntry
{
	public string Key { get; }

	public string OwnerId { get; }

	public StateMachineCallbackPhaseFlags Phases { get; }

	public StateMachineCallbackSourceKind SourceKind { get; }

	public string DisplayName { get; }

	public string SourcePath { get; }

	public StateMachineCallbackCatalogEntry(string key, string ownerId, StateMachineCallbackPhaseFlags phases, StateMachineCallbackSourceKind sourceKind = StateMachineCallbackSourceKind.CSharp, string displayName = "", string sourcePath = "")
	{
		Key = key ?? string.Empty;
		OwnerId = ownerId ?? string.Empty;
		Phases = phases;
		SourceKind = sourceKind;
		DisplayName = displayName ?? string.Empty;
		SourcePath = sourcePath ?? string.Empty;
	}
}
