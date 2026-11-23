using System;
using System.Collections.Generic;

public sealed class StateMachineGraphChange
{
	public StateMachineGraphChangeKind Kind { get; set; }

	public IReadOnlyList<string> AffectedStableIds { get; set; } = Array.Empty<string>();
}
