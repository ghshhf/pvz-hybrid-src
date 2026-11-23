public readonly struct AdobeAnimateExternalVisualDescriptor
{
	public AdobeAnimateExternalVisualAttachmentMode AttachmentMode { get; }

	public AdobeAnimateExternalVisualDrawBand DrawBand { get; }

	public AdobeAnimateSlot Slot { get; }

	public int RelativeZ { get; }

	public AdobeAnimateExternalVisualDescriptor(AdobeAnimateExternalVisualAttachmentMode attachmentMode, AdobeAnimateExternalVisualDrawBand drawBand, AdobeAnimateSlot slot = null, int relativeZ = 0)
	{
		AttachmentMode = attachmentMode;
		DrawBand = drawBand;
		Slot = slot;
		RelativeZ = relativeZ;
	}
}
