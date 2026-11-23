namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModSunCollection(string Account, long Requested, long AppliedDelta, long Balance)
{
	public XWModSunSource Attribution { get; set; }
}
