using System;

public sealed class CharacterComponentAttachmentTransaction : IDisposable
{
	private ComponentManager _manager;

	internal bool Executing { get; private set; }

	internal string CancellationReason { get; set; }

	public CharacterComponentAttachmentResult Result { get; private set; } = new CharacterComponentAttachmentResult(CharacterComponentAttachmentStatus.Open, "", Array.Empty<string>(), Array.Empty<string>());

	public bool IsOpen => Result.Status == CharacterComponentAttachmentStatus.Open;

	internal CharacterComponentAttachmentTransaction(ComponentManager manager, string rejection = null)
	{
		_manager = manager;
		if (rejection != null)
		{
			Complete(new CharacterComponentAttachmentResult(CharacterComponentAttachmentStatus.Rejected, rejection, Array.Empty<string>(), Array.Empty<string>()));
		}
	}

	public CharacterComponentAttachResult Attach(CharacterComponentDefinition definition)
	{
		if (!IsOpen)
		{
			return new CharacterComponentAttachResult(null, "", Result.Message);
		}
		RequireNotExecuting();
		Executing = true;
		CharacterComponentAttachResult characterComponentAttachResult;
		try
		{
			characterComponentAttachResult = _manager.AttachInTransaction(this, definition);
		}
		finally
		{
			Executing = false;
		}
		if (!characterComponentAttachResult.Success)
		{
			Rollback(characterComponentAttachResult.Message);
		}
		return characterComponentAttachResult;
	}

	public CharacterComponentAttachmentResult Commit()
	{
		if (!IsOpen)
		{
			return Result;
		}
		RequireNotExecuting();
		return _manager.CommitAttachmentTransaction(this);
	}

	public CharacterComponentAttachmentResult Rollback(string reason = "Attachment transaction cancelled.")
	{
		if (!IsOpen)
		{
			return Result;
		}
		RequireNotExecuting();
		Executing = true;
		try
		{
			return _manager.RollbackAttachmentTransaction(this, reason);
		}
		finally
		{
			Executing = false;
		}
	}

	public void Dispose()
	{
		if (IsOpen)
		{
			Rollback();
		}
	}

	internal void Complete(CharacterComponentAttachmentResult result)
	{
		Result = result;
		_manager = null;
	}

	private void RequireNotExecuting()
	{
		if (Executing)
		{
			throw new InvalidOperationException("An attachment callback cannot re-enter its transaction.");
		}
	}
}
