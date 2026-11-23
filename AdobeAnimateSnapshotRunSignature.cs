using System;
using System.Runtime.CompilerServices;
using Godot;

internal readonly struct AdobeAnimateSnapshotRunSignature : IEquatable<AdobeAnimateSnapshotRunSignature>
{
	public TextureLayered VisualAtlas { get; }

	public Vector2 VisualAtlasSize { get; }

	public TextureLayered PoseTexture { get; }

	public Vector2I PoseTextureSize { get; }

	public Texture2DArray GpuGraphTexture { get; }

	public Vector2I GpuGraphTextureSize { get; }

	public bool UsesGpuGraph => GpuGraphTexture != null;

	public AdobeAnimateSnapshotRunSignature(TextureLayered visualAtlas, Vector2 visualAtlasSize, TextureLayered poseTexture, Vector2I poseTextureSize, Texture2DArray gpuGraphTexture = null, Vector2I gpuGraphTextureSize = default(Vector2I))
	{
		VisualAtlas = visualAtlas;
		VisualAtlasSize = visualAtlasSize;
		PoseTexture = poseTexture;
		PoseTextureSize = poseTextureSize;
		GpuGraphTexture = gpuGraphTexture;
		GpuGraphTextureSize = gpuGraphTextureSize;
	}

	public bool Equals(AdobeAnimateSnapshotRunSignature other)
	{
		if (VisualAtlas == other.VisualAtlas && VisualAtlasSize == other.VisualAtlasSize && PoseTexture == other.PoseTexture && PoseTextureSize == other.PoseTextureSize && GpuGraphTexture == other.GpuGraphTexture)
		{
			return GpuGraphTextureSize == other.GpuGraphTextureSize;
		}
		return false;
	}

	public bool HasSameBaseResources(AdobeAnimateSnapshotRunSignature other)
	{
		if (VisualAtlas == other.VisualAtlas && VisualAtlasSize == other.VisualAtlasSize && PoseTexture == other.PoseTexture)
		{
			return PoseTextureSize == other.PoseTextureSize;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AdobeAnimateSnapshotRunSignature other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(IdentityHash(VisualAtlas), VisualAtlasSize, IdentityHash(PoseTexture), PoseTextureSize, IdentityHash(GpuGraphTexture), GpuGraphTextureSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int IdentityHash(object value)
	{
		if (value != null)
		{
			return RuntimeHelpers.GetHashCode(value);
		}
		return 0;
	}

	public static bool operator ==(AdobeAnimateSnapshotRunSignature left, AdobeAnimateSnapshotRunSignature right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AdobeAnimateSnapshotRunSignature left, AdobeAnimateSnapshotRunSignature right)
	{
		return !left.Equals(right);
	}
}
