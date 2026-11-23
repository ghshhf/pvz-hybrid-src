using System;
using Godot;

public readonly struct AdobeAnimateExternalTextureAtlasAllocation
{
	public Texture2D Texture { get; }

	public Rid TextureRid { get; }

	public TextureLayered TextureArray { get; }

	public Rid TextureArrayRid { get; }

	public Vector2 TextureArraySize { get; }

	public int TextureArrayLayerOffset { get; }

	public int AtlasPage { get; }

	public Rect2 Rect { get; }

	public bool UsesTextureArray { get; }

	public AdobeAnimateExternalTextureAtlasAllocation(Texture2D texture, Rid textureRid, int atlasPage, Rect2 rect, TextureLayered textureArray = null, Rid textureArrayRid = default(Rid), Vector2 textureArraySize = default(Vector2), int textureArrayLayerOffset = 0, bool usesTextureArray = false)
	{
		Texture = texture;
		TextureRid = textureRid;
		AtlasPage = atlasPage;
		Rect = rect;
		TextureArray = textureArray;
		TextureArrayRid = textureArrayRid;
		TextureArraySize = textureArraySize;
		TextureArrayLayerOffset = Math.Max(0, textureArrayLayerOffset);
		UsesTextureArray = usesTextureArray;
	}
}
