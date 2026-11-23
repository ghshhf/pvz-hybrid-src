using System;

internal readonly struct AdobeAnimateGpuDynamicOverrideAllocation
{
	public ulong Signature { get; }

	public int BaseTexel { get; }

	public int MediaCount { get; }

	public AdobeAnimateGpuDynamicOverrideAllocation(ulong signature, int baseTexel, int mediaCount)
	{
		Signature = signature;
		BaseTexel = baseTexel;
		MediaCount = Math.Max(0, mediaCount);
	}
}
