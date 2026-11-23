using Godot;

internal readonly struct AdobeAnimateCrowdResourceRequirements
{
	public TextureLayered VisualAtlas { get; }

	public Vector2 VisualAtlasSize { get; }

	public bool RequiresPoseTexture { get; }

	public TextureLayered PoseTexture { get; }

	public Vector2I PoseTextureSize { get; }

	public bool RequiresGpuGraphTexture { get; }

	public Texture2DArray GpuGraphTexture { get; }

	public Vector2I GpuGraphTextureSize { get; }

	public bool RequiresDynamicOverride { get; }

	public AdobeAnimateGpuDynamicOverrideAtlas DynamicOverrideProvider { get; }

	public AdobeAnimateRasterCompositeData RasterCompositeData { get; }

	public AdobeAnimateCrowdResourceRequirements(TextureLayered visualAtlas, Vector2 visualAtlasSize, bool requiresPoseTexture, TextureLayered poseTexture, Vector2I poseTextureSize, bool requiresGpuGraphTexture, Texture2DArray gpuGraphTexture, Vector2I gpuGraphTextureSize, bool requiresDynamicOverride, AdobeAnimateGpuDynamicOverrideAtlas dynamicOverrideProvider, AdobeAnimateRasterCompositeData rasterCompositeData)
	{
		VisualAtlas = visualAtlas;
		VisualAtlasSize = visualAtlasSize;
		RequiresPoseTexture = requiresPoseTexture;
		PoseTexture = poseTexture;
		PoseTextureSize = poseTextureSize;
		RequiresGpuGraphTexture = requiresGpuGraphTexture;
		GpuGraphTexture = gpuGraphTexture;
		GpuGraphTextureSize = gpuGraphTextureSize;
		RequiresDynamicOverride = requiresDynamicOverride;
		DynamicOverrideProvider = dynamicOverrideProvider;
		RasterCompositeData = rasterCompositeData;
	}

	public bool IsExactMatch(in AdobeAnimateCrowdResourceRequirements other)
	{
		if (VisualAtlas == other.VisualAtlas && VisualAtlasSize == other.VisualAtlasSize && RequiresPoseTexture == other.RequiresPoseTexture && PoseTexture == other.PoseTexture && PoseTextureSize == other.PoseTextureSize && RequiresGpuGraphTexture == other.RequiresGpuGraphTexture && GpuGraphTexture == other.GpuGraphTexture && GpuGraphTextureSize == other.GpuGraphTextureSize && RequiresDynamicOverride == other.RequiresDynamicOverride && DynamicOverrideProvider == other.DynamicOverrideProvider)
		{
			return RasterCompositeData == other.RasterCompositeData;
		}
		return false;
	}
}
