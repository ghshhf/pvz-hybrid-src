using Godot;

internal readonly struct AdobeAnimateGpuManagedAttachmentPose
{
	public Vector2 Origin { get; }

	public float Rotation { get; }

	public Vector2 Scale { get; }

	public float Skew { get; }

	public bool Valid { get; }

	public AdobeAnimateGpuManagedAttachmentPose(Transform2D transform, bool valid)
	{
		Origin = transform.Origin;
		Rotation = transform.Rotation;
		Scale = transform.Scale;
		Skew = transform.Skew;
		Valid = valid;
	}
}
