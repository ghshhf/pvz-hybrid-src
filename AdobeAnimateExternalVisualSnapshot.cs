using Godot;

internal readonly struct AdobeAnimateExternalVisualSnapshot
{
	public Sprite2D Sprite { get; }

	public AdobeAnimateExternalVisualDescriptor Descriptor { get; }

	public bool Visible { get; }

	public Texture2D Texture { get; }

	public Transform2D Transform { get; }

	public Transform2D RegisteredWorldTransform { get; }

	public Color Modulate { get; }

	public bool TopologyDirty { get; }

	public bool StateDirty { get; }

	public long LastSuccessfulPublishedFrame { get; }

	public AdobeAnimateExternalVisualSnapshot(Sprite2D sprite, in AdobeAnimateExternalVisualDescriptor descriptor, bool visible, Texture2D texture, in Transform2D transform, in Transform2D registeredWorldTransform, in Color modulate, bool topologyDirty, bool stateDirty, long lastSuccessfulPublishedFrame)
	{
		Sprite = sprite;
		Descriptor = descriptor;
		Visible = visible;
		Texture = texture;
		Transform = transform;
		RegisteredWorldTransform = registeredWorldTransform;
		Modulate = modulate;
		TopologyDirty = topologyDirty;
		StateDirty = stateDirty;
		LastSuccessfulPublishedFrame = lastSuccessfulPublishedFrame;
	}
}
