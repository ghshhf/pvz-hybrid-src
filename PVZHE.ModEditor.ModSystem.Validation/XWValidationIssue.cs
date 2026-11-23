using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem.Validation;

public sealed class XWValidationIssue
{
	public enum Severity
	{
		Info,
		Warning,
		Error
	}

	public enum IssueCode
	{
		MissingResource,
		InvalidUid,
		DuplicateKey,
		DependencyConflict,
		DotNetToolchainMissing,
		ScriptCompileFailed,
		BlueprintGenerationFailed,
		OverrideConflict,
		ManifestError,
		VersionMismatch,
		DependencyCycle,
		SoftDependencyCycle,
		MissingCapability,
		PackageChanged,
		RuntimeLoadFailed
	}

	public Severity Level { get; set; } = Severity.Error;

	public IssueCode Code { get; set; }

	public string Message { get; set; } = "";

	public string ResourceKey { get; set; } = "";

	public List<string> AffectedModIds { get; set; } = new List<string>();

	public List<string> RelatedModIds { get; set; } = new List<string>();

	public string FilePath { get; set; } = "";

	public int Line { get; set; } = -1;

	public int Column { get; set; } = -1;

	public string JumpTarget { get; set; } = "";
}
