using System.Collections.Generic;

public sealed record CharacterComponentAttachmentResult(CharacterComponentAttachmentStatus Status, string Message, IReadOnlyList<string> RetainedIdentities, IReadOnlyList<string> UnclearedComponents)
{
	public bool Success => Status == CharacterComponentAttachmentStatus.Committed;

	public bool FullyRolledBack
	{
		get
		{
			CharacterComponentAttachmentStatus status = Status;
			if ((uint)(status - 2) <= 1u)
			{
				return true;
			}
			return false;
		}
	}
}
