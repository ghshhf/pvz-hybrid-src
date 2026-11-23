using Godot;

public readonly struct PackedReplaceSlot
{
	public StringName MediaName { get; }

	public int MediaId { get; }

	public Vector2 Size { get; }

	public PackedReplaceSlot(StringName mediaName, int mediaId, Vector2 size)
	{
		MediaName = mediaName;
		MediaId = mediaId;
		Size = size;
	}
}
