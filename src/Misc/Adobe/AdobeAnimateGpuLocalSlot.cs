internal readonly struct AdobeAnimateGpuLocalSlot
{
	public int SliceKey { get; }

	public int Occurrence { get; }

	public int LayerId { get; }

	public int DrawOrder { get; }

	public AdobeAnimateGpuLocalSlot(int sliceKey, int occurrence, int layerId, int drawOrder)
	{
		SliceKey = sliceKey;
		Occurrence = occurrence;
		LayerId = layerId;
		DrawOrder = drawOrder;
	}
}
