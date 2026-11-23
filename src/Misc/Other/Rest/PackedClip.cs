using Godot;

public readonly struct PackedClip
{
	public StringName Name { get; }

	public Vector2I Range { get; }

	public PackedClip(StringName name, Vector2I range)
	{
		Name = name;
		Range = range;
	}
}
