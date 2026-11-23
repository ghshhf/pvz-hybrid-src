namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModLifeChanged(XWModEntityHandle Target, double Before, double After, string Cause);
