internal readonly struct AdobeAnimateGpuFrameSlotEntry
{
	public int PoseTexel { get; }

	public int MediaId { get; }

	public int LayerId { get; }

	public bool Visible { get; }

	public AdobeAnimateGpuFrameSlotEntry(int poseTexel, int mediaId, int layerId, bool visible)
	{
		PoseTexel = poseTexel;
		MediaId = mediaId;
		LayerId = layerId;
		Visible = visible;
	}
}
