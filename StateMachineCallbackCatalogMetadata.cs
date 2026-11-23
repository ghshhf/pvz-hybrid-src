internal readonly struct StateMachineCallbackCatalogMetadata
{
	public StateMachineCallbackSourceKind SourceKind { get; }

	public string DisplayName { get; }

	public string SourcePath { get; }

	public StateMachineCallbackCatalogMetadata(StateMachineCallbackSourceKind sourceKind, string displayName, string sourcePath)
	{
		SourceKind = sourceKind;
		DisplayName = displayName ?? string.Empty;
		SourcePath = sourcePath ?? string.Empty;
	}
}
