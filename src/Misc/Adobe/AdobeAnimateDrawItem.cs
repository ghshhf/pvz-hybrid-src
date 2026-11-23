using System;
using Godot;

internal readonly struct AdobeAnimateDrawItem
{
	public Transform2D Transform { get; }

	public Vector2 Size { get; }

	public Rect2 UvRect { get; }

	public int AtlasLayer { get; }

	public Color Color { get; }

	public AdobeAnimateSortPath SortPath { get; }

	public bool ClipEnabled { get; }

	public float ClipUp { get; }

	public float ClipDown { get; }

	public AdobeAnimateSprite Owner { get; }

	public bool UseShaderPose { get; }

	public int PoseTexel { get; }

	public int PoseLayer { get; }

	public float PoseFrameT { get; }

	public Vector2 PoseOffset { get; }

	public bool UseVisualOverride { get; }

	public float ClipLeft { get; }

	public float ClipRight { get; }

	public AdobeAnimateDrawItem(Transform2D transform, Vector2 size, Rect2 uvRect, int atlasLayer, Color color, AdobeAnimateSortPath sortPath, bool clipEnabled = false, float clipUp = 0f, float clipDown = 0f, AdobeAnimateSprite owner = null, bool useShaderPose = false, int poseTexel = 0, int poseLayer = 0, float poseFrameT = 0f, Vector2 poseOffset = default(Vector2), bool useVisualOverride = false, float clipLeft = 0f, float clipRight = 0f)
	{
		Transform = transform;
		Size = size;
		UvRect = uvRect;
		AtlasLayer = Math.Max(0, atlasLayer);
		Color = color;
		SortPath = sortPath;
		ClipEnabled = clipEnabled;
		ClipUp = clipUp;
		ClipDown = clipDown;
		Owner = owner;
		UseShaderPose = useShaderPose;
		PoseTexel = Math.Max(0, poseTexel);
		PoseLayer = Math.Max(0, poseLayer);
		PoseFrameT = Mathf.Clamp(poseFrameT, 0f, 1f);
		PoseOffset = poseOffset;
		UseVisualOverride = useVisualOverride;
		ClipLeft = clipLeft;
		ClipRight = clipRight;
	}
}
