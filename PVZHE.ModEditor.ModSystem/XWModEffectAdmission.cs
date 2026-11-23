namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModEffectAdmission(XWModEntityHandle Target, string Key)
{
	public bool Blocked { get; set; }
}
