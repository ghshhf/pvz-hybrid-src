namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModCommandResult(XWModCommandStatus Status, string Diagnostic)
{
	public bool Accepted
	{
		get
		{
			XWModCommandStatus status = Status;
			if ((uint)status <= 1u)
			{
				return true;
			}
			return false;
		}
	}
}
public sealed record XWModCommandResult<T>(XWModCommandStatus Status, T Value, string Diagnostic)
{
	public bool Succeeded => Status == XWModCommandStatus.Completed;
}
