using Godot;

internal readonly struct AdobeAnimateRootMotionState
{
	public Transform2D PreviousRelativeTransform { get; }

	public bool Enabled { get; }

	public AdobeAnimateRootMotionState(Transform2D previousRelativeTransform, bool enabled)
	{
		PreviousRelativeTransform = previousRelativeTransform;
		Enabled = enabled;
	}
}
