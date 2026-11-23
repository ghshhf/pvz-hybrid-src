using Godot;

internal readonly struct AdobeAnimateGpuRenderOwner
{
	public AdobeAnimateRuntimeDefinition Definition { get; }

	public int ParentOwnerIndex { get; }

	public int TopologyOrder { get; }

	public int Depth { get; }

	public AdobeAnimateGpuAttachmentKind AttachmentKind { get; }

	public int AttachmentKey { get; }

	public int AttachmentPoseLookupBase { get; }

	public int RenderLayer { get; }

	public int LocalSlotCount { get; }

	public int FrameLookupBase { get; }

	public bool UsePos { get; }

	public bool UseRotate { get; }

	public bool UseScale { get; }

	public bool UseSkew { get; }

	public Vector2 ParentOffset { get; }

	public Vector2 SlotOffset { get; }

	public float OffsetRotate { get; }

	public bool UseFollowVisible { get; }

	public Transform2D LocalTransform { get; }

	public AdobeAnimateGpuRenderOwner(AdobeAnimateRuntimeDefinition definition, int parentOwnerIndex, int topologyOrder, int depth, AdobeAnimateGpuAttachmentKind attachmentKind, int attachmentKey, int attachmentPoseLookupBase, int renderLayer, int localSlotCount, int frameLookupBase, bool usePos, bool useRotate, bool useScale, bool useSkew, Vector2 parentOffset, Vector2 slotOffset, float offsetRotate, bool useFollowVisible, Transform2D localTransform)
	{
		Definition = definition;
		ParentOwnerIndex = parentOwnerIndex;
		TopologyOrder = topologyOrder;
		Depth = depth;
		AttachmentKind = attachmentKind;
		AttachmentKey = attachmentKey;
		AttachmentPoseLookupBase = attachmentPoseLookupBase;
		RenderLayer = renderLayer;
		LocalSlotCount = localSlotCount;
		FrameLookupBase = frameLookupBase;
		UsePos = usePos;
		UseRotate = useRotate;
		UseScale = useScale;
		UseSkew = useSkew;
		ParentOffset = parentOffset;
		SlotOffset = slotOffset;
		OffsetRotate = offsetRotate;
		UseFollowVisible = useFollowVisible;
		LocalTransform = localTransform;
	}
}
