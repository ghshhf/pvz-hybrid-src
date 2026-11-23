using Godot;

internal readonly struct AdobeAnimateGpuAttachmentSettings
{
	public static AdobeAnimateGpuAttachmentSettings Root => new AdobeAnimateGpuAttachmentSettings(AdobeAnimateGpuAttachmentKind.Root, -1, -1, usePos: true, useRotate: true, useScale: true, useSkew: true, Vector2.Zero, Vector2.Zero, 0f, useFollowVisible: false, Transform2D.Identity);

	public AdobeAnimateGpuAttachmentKind Kind { get; }

	public int Key { get; }

	public int PoseLookupBase { get; }

	public bool UsePos { get; }

	public bool UseRotate { get; }

	public bool UseScale { get; }

	public bool UseSkew { get; }

	public Vector2 ParentOffset { get; }

	public Vector2 SlotOffset { get; }

	public float OffsetRotate { get; }

	public bool UseFollowVisible { get; }

	public Transform2D LocalTransform { get; }

	public AdobeAnimateGpuAttachmentSettings(AdobeAnimateGpuAttachmentKind kind, int key, int poseLookupBase, bool usePos, bool useRotate, bool useScale, bool useSkew, Vector2 parentOffset, Vector2 slotOffset, float offsetRotate, bool useFollowVisible, Transform2D localTransform)
	{
		Kind = kind;
		Key = key;
		PoseLookupBase = poseLookupBase;
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

	public AdobeAnimateGpuAttachmentSettings WithPoseLookupBase(int poseLookupBase)
	{
		return new AdobeAnimateGpuAttachmentSettings(Kind, Key, poseLookupBase, UsePos, UseRotate, UseScale, UseSkew, ParentOffset, SlotOffset, OffsetRotate, UseFollowVisible, LocalTransform);
	}
}
