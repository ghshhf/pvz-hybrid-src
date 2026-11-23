public readonly struct StateMachineUnloadBlocker
{
	public string DefinitionId { get; }

	public string HostType { get; }

	public StateMachineUnloadBlocker(string definitionId, string hostType)
	{
		DefinitionId = definitionId ?? string.Empty;
		HostType = hostType ?? string.Empty;
	}
}
