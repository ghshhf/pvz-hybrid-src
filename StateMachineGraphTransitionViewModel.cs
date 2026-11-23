public sealed class StateMachineGraphTransitionViewModel
{
	public StateMachineTransitionDefinition Transition { get; set; }

	public string StableId => Transition?.StableId ?? string.Empty;

	public bool IsInherited { get; set; }

	public bool IsLocalOverride { get; set; }

	public bool IsReadOnly { get; set; }
}
