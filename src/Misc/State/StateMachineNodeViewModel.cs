public sealed class StateMachineNodeViewModel
{
	public StateMachineStateDefinition State { get; set; }

	public string StableId => State?.StableId ?? string.Empty;

	public bool IsInherited { get; set; }

	public bool IsLocalOverride { get; set; }

	public bool IsReadOnly { get; set; }
}
