namespace PVZHE.ModEditor.ModSystem;

public sealed record XWLoadingState(XWLoadingKind Kind, string Status, double? Progress = null, string Error = "", bool CanStart = false, bool CanClearCache = false);
