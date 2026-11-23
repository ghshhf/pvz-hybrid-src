using Godot;

internal readonly struct AdobeAnimateClipBlendState
{
	public bool Enabled { get; }

	public float FromFrameFloat { get; }

	public float Weight { get; }

	public AdobeAnimateClipBlendState(bool enabled, float fromFrameFloat, float weight)
	{
		Enabled = enabled;
		FromFrameFloat = fromFrameFloat;
		Weight = Mathf.Clamp(weight, 0f, 1f);
	}
}
