namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModExtensionDiagnostic(string PointId, string OwnerId, long InvocationCount, long FaultCount, long RejectedCount, long SampleCount, double SampledMilliseconds, string LastDiagnostic);
