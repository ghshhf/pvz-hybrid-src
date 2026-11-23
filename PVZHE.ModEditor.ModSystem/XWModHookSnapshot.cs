namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModHookSnapshot(string OwnerId, string RegistrationId, string PointId, XWModHookKind Kind, bool Active, int Order);
