using System;

internal readonly struct StateMachineScannedCallback
{
	public string FullKey { get; }

	public string OwnerId { get; }

	public StateMachineCallbackPhase Phase { get; }

	public Delegate Callback { get; }

	public string Source { get; }

	public StateMachineCallbackSourceKind SourceKind { get; }

	public string DisplayName { get; }

	public string SourcePath { get; }

	public StateMachineScannedCallback(string fullKey, string ownerId, StateMachineCallbackPhase phase, Delegate callback, string source, StateMachineCallbackSourceKind sourceKind, string displayName, string sourcePath)
	{
		FullKey = fullKey;
		OwnerId = ownerId;
		Phase = phase;
		Callback = callback;
		Source = source;
		SourceKind = sourceKind;
		DisplayName = displayName ?? string.Empty;
		SourcePath = sourcePath ?? string.Empty;
	}
}
