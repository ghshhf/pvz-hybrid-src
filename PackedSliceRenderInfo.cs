using Godot;

public readonly struct PackedSliceRenderInfo
{
	public Vector2 SourceSize { get; }

	public Rect2 UvRect { get; }

	public int AtlasLayer { get; }

	public bool IsValid
	{
		get
		{
			if (SourceSize.X > 0f)
			{
				return SourceSize.Y > 0f;
			}
			return false;
		}
	}

	public PackedSliceRenderInfo(Vector2 sourceSize, Rect2 uvRect, int atlasLayer)
	{
		SourceSize = sourceSize;
		UvRect = uvRect;
		AtlasLayer = atlasLayer;
	}
}
