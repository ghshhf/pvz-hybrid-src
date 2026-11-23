using Godot.Collections;

public readonly struct PackedEventFrame
{
	public int Frame { get; }

	public Array Events { get; }

	public PackedEventFrame(int frame, Array events)
	{
		Frame = frame;
		Events = events;
	}
}
