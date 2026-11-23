namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModCardState(XWModCardHandle Handle, string ContentId, long Cost, double Remaining, double Duration, bool CoolingDown, bool Available, bool Selected);
