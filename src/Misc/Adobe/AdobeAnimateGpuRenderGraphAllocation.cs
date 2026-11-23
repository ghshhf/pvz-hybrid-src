internal readonly struct AdobeAnimateGpuRenderGraphAllocation
{
	public ulong Signature { get; }

	public int Page { get; }

	public int BaseTexel { get; }

	public int TexelCount { get; }

	public int RenderSlotCount { get; }

	public int OwnerCount { get; }

	public AdobeAnimateGpuRenderGraphAllocation(ulong signature, int page, int baseTexel, int texelCount, int renderSlotCount, int ownerCount)
	{
		Signature = signature;
		Page = page;
		BaseTexel = baseTexel;
		TexelCount = texelCount;
		RenderSlotCount = renderSlotCount;
		OwnerCount = ownerCount;
	}
}
