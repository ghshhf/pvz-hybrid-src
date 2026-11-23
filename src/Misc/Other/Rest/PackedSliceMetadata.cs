public readonly struct PackedSliceMetadata
{
	public int SliceKey { get; }

	public ushort MediaId { get; }

	public ushort LayerId { get; }

	public int DrawOrder { get; }

	public int Flags { get; }

	public PackedSliceMetadata(int sliceKey, ushort mediaId, ushort layerId, int drawOrder, int flags)
	{
		SliceKey = sliceKey;
		MediaId = mediaId;
		LayerId = layerId;
		DrawOrder = drawOrder;
		Flags = flags;
	}
}
