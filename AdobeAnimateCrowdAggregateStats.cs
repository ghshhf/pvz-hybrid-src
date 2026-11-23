internal struct AdobeAnimateCrowdAggregateStats
{
	public int ActiveMounts { get; set; }

	public int ActiveZBuckets { get; set; }

	public int CrowdRoots { get; set; }

	public int CrowdStateTexels { get; set; }

	public int CrowdStateCapacityTexels { get; set; }

	public int ResourceSignatures { get; set; }

	public int StateTextureUploads { get; set; }

	public int StateTextureUpdatedLayers { get; set; }

	public long StateTextureUploadedBytes { get; set; }

	public int MultiMeshUploads { get; set; }

	public long RdSubmittedBatches { get; set; }

	public long RdAppliedBatches { get; set; }

	public long RdQueuedUploads { get; set; }

	public long RdAppliedUploads { get; set; }

	public long RdUploadedBytes { get; set; }

	public long RdBufferRidRefreshes { get; set; }

	public long RdFailedBatches { get; set; }

	public long RdBufferUpdateFailures { get; set; }

	public long RdInvalidTargets { get; set; }

	public long RdStaleDrops { get; set; }

	public long RdQueueDepth { get; set; }

	public long RdMaximumQueueDepth { get; set; }

	public long RdLastQueuedFrameVersion { get; set; }

	public long RdLastAppliedFrameVersion { get; set; }

	public int SignatureConflictBuckets { get; set; }

	public int FallbackRuns { get; set; }

	public int FallbackRoots { get; set; }

	public int ArenaGrowthCount { get; set; }

	public int CrowdMeshQuadCapacity { get; set; }

	public int CrowdMeshVersion { get; set; }

	public int MeshRebinds { get; set; }

	public int CpuRoots { get; set; }

	public int CpuFallbackRoots { get; set; }

	public int CpuValidationFailures { get; set; }

	public int CpuMeshRebuilds { get; set; }

	public int CpuVertexUploads { get; set; }

	public int CpuUploadedVertices { get; set; }

	public int CpuCapacityGrowths { get; set; }

	public int CpuSkippedUploads { get; set; }

	public int CpuSurfaceCount { get; set; }

	public int CpuSharedMaterialRoots { get; set; }

	public int CpuSharedMaterialInstances { get; set; }

	public int CpuRetiredRoots { get; set; }

	public int CpuRetiredSurfaceCount { get; set; }
}
