namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModAttackEvent(XWModEntityHandle Source, XWModEntityHandle Target, string Kind, string Phase);
