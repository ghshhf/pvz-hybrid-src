namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModEffectChanged(XWModEntityHandle Target, string Key, XWModEffectChange Change, double? Duration, double? Strength);
