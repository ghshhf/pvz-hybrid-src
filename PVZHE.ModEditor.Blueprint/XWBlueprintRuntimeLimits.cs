namespace PVZHE.ModEditor.Blueprint;

public sealed class XWBlueprintRuntimeLimits
{
	public int MaxSteps { get; set; } = 2048;

	public int MaxLoopIterations { get; set; } = 256;

	public int MaxCollectionItems { get; set; } = 1024;

	public int MaxStringLength { get; set; } = 16384;

	public int MaxOutputMessages { get; set; } = 128;

	public int MaxNodeVisitsPerNode { get; set; } = 512;

	public int MaxExecutionMilliseconds { get; set; } = 2000;

	public int MaxValueDepth { get; set; } = 16;

	public int MaxClonedValues { get; set; } = 8192;

	public int MaxCallArguments { get; set; } = 32;

	public int MaxCallDepth { get; set; } = 16;
}
