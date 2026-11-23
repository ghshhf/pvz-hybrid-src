namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModBootJournal
{
	public int SchemaVersion { get; set; } = 1;

	public string PlanHash { get; set; } = "";

	public string Owner { get; set; } = "";

	public string Phase { get; set; } = "idle";

	public bool InProgress { get; set; }

	public bool RecoveryRequired { get; set; }
}
