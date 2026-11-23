internal readonly struct AdobeAnimateGpuRenderSlot
{
	public int OwnerIndex { get; }

	public int OwnerTopologyOrder { get; }

	public int OwnerLocalSlot { get; }

	public AdobeAnimateGpuRenderSlotKind Kind { get; }

	public int StaticVisualIndex { get; }

	public AdobeAnimateGpuRenderSlot(int ownerIndex, int ownerTopologyOrder, int ownerLocalSlot, AdobeAnimateGpuRenderSlotKind kind, int staticVisualIndex)
	{
		OwnerIndex = ownerIndex;
		OwnerTopologyOrder = ownerTopologyOrder;
		OwnerLocalSlot = ownerLocalSlot;
		Kind = kind;
		StaticVisualIndex = staticVisualIndex;
	}
}
