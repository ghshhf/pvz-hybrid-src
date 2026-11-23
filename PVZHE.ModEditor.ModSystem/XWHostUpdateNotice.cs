namespace PVZHE.ModEditor.ModSystem;

internal sealed record XWHostUpdateNotice(string ModId, string DisplayName, string CurrentVersion, string LatestVersion, string Notes, string UpdateUrl, bool IsHost, int LoadOrder);
