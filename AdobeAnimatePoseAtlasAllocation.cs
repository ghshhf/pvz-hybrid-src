using Godot;

public readonly struct AdobeAnimatePoseAtlasAllocation
{
	public Texture2D Texture { get; }

	public Rid TextureRid { get; }

	public TextureLayered TextureArray { get; }

	public Rid TextureArrayRid { get; }

	public int AtlasPage { get; }

	public int TexelStart { get; }

	public int TexelCount { get; }

	public Vector2I TextureSize { get; }

	public ulong Signature { get; }

	public bool UsesTextureArray { get; }

	public AdobeAnimatePoseAtlasAllocation(Texture2D texture, Rid textureRid, int atlasPage, int texelStart, int texelCount, Vector2I textureSize, ulong signature, TextureLayered textureArray = null, Rid textureArrayRid = default(Rid), bool usesTextureArray = false)
	{
		Texture = texture;
		TextureRid = textureRid;
		AtlasPage = atlasPage;
		TexelStart = texelStart;
		TexelCount = texelCount;
		TextureSize = textureSize;
		Signature = signature;
		TextureArray = textureArray;
		TextureArrayRid = textureArrayRid;
		UsesTextureArray = usesTextureArray;
	}
}
