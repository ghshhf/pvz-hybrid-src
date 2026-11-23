using Godot;

internal readonly struct AdobeAnimateCompositePoseRefreshState
{
	public AdobeAnimateRuntimeDefinition Definition { get; }

	public Transform2D GlobalTransform { get; }

	public Color Modulate { get; }

	public Vector2 Offset { get; }

	public VerticalClipState VerticalClip { get; }

	public int FrameOffset { get; }

	public int FrameCount { get; }

	public int PoseBaseTexel { get; }

	public int PoseLayer { get; }

	public float InterpolationT { get; }

	public ulong FrameLayoutSignature { get; }

	public ulong StaticSignature { get; }

	public AdobeAnimateCompositePoseRefreshState(AdobeAnimateRuntimeDefinition definition, Transform2D globalTransform, Color modulate, Vector2 offset, VerticalClipState verticalClip, int frameOffset, int frameCount, int poseBaseTexel, int poseLayer, float interpolationT, ulong frameLayoutSignature, ulong staticSignature)
	{
		Definition = definition;
		GlobalTransform = globalTransform;
		Modulate = modulate;
		Offset = offset;
		VerticalClip = verticalClip;
		FrameOffset = frameOffset;
		FrameCount = frameCount;
		PoseBaseTexel = poseBaseTexel;
		PoseLayer = poseLayer;
		InterpolationT = interpolationT;
		FrameLayoutSignature = frameLayoutSignature;
		StaticSignature = staticSignature;
	}
}
