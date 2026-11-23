namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModArmorChanged(XWModEntityHandle Target, string ArmorKey, double Before, double After, bool Removed, string Cause);
