using Godot;

public readonly struct PackedCpuPoseSample
{
	public int MediaId { get; }

	public Transform2D Transform { get; }

	public bool Valid { get; }

	public PackedCpuPoseSample(int mediaId, Transform2D transform, bool valid)
	{
		MediaId = mediaId;
		Transform = transform;
		Valid = valid;
	}
}
