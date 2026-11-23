using System;
using Godot;
using Godot.Collections;

internal readonly struct AdobeAnimateRenderSnapshot
{
	public AdobeAnimateSprite Sprite { get; }

	public AdobeAnimateData Data { get; }

	public AdobeAnimateRuntimeDefinition Definition { get; }

	public string Clip { get; }

	public Vector2I ClipRange { get; }

	public float FrameFloat { get; }

	public Transform2D GlobalTransform { get; }

	public Vector2 Offset { get; }

	public Color Modulate { get; }

	public Array<bool> LayerVisible { get; }

	public bool AllLayersVisible { get; }

	public bool CanUseLayerMask { get; }

	public ulong LayerMask { get; }

	public int LayerVisibleCount { get; }

	public bool[] LayerVisibleValues { get; }

	public Texture2D MediaReplaceAtlas { get; }

	public Array<Rect2> MediaReplaceRect { get; }

	public Array<bool> MediaReplaceUse { get; }

	public bool HasMediaReplace { get; }

	public int MediaReplaceLimit { get; }

	public ulong MediaReplaceUseMask { get; }

	public bool MediaReplaceUseMaskOverflow { get; }

	public Array<int> MediaReplaceAtlasPages { get; }

	public int MediaReplaceAtlasPageCount { get; }

	public bool[] MediaReplaceUseValues { get; }

	public Rect2[] MediaReplaceRects { get; }

	public int[] MediaReplaceAtlasPageValues { get; }

	public TextureLayered MediaReplaceAtlasArray { get; }

	public Vector2 MediaReplaceAtlasArraySize { get; }

	public bool MediaReplaceAtlasUsesTextureArray { get; }

	public VerticalClipState VerticalClip { get; }

	public Node RenderMountParent { get; }

	public int CanvasLayer { get; }

	public int EffectiveZIndex { get; }

	public int[] TreeOrderPath { get; }

	public bool NeedsDrawItemSort { get; }

	public AdobeAnimateClipBlendState ClipBlend { get; }

	public AdobeAnimateRenderSnapshot(AdobeAnimateSprite sprite, AdobeAnimateData data, AdobeAnimateRuntimeDefinition definition, string clip, Vector2I clipRange, float frameFloat, Transform2D globalTransform, Vector2 offset, Color modulate, Array<bool> layerVisible, bool allLayersVisible, bool canUseLayerMask, ulong layerMask, int layerVisibleCount, bool[] layerVisibleValues, Texture2D mediaReplaceAtlas, Array<Rect2> mediaReplaceRect, Array<bool> mediaReplaceUse, bool hasMediaReplace, int mediaReplaceLimit, ulong mediaReplaceUseMask, bool mediaReplaceUseMaskOverflow, Array<int> mediaReplaceAtlasPages, int mediaReplaceAtlasPageCount, bool[] mediaReplaceUseValues, Rect2[] mediaReplaceRects, int[] mediaReplaceAtlasPageValues, TextureLayered mediaReplaceAtlasArray, Vector2 mediaReplaceAtlasArraySize, bool mediaReplaceAtlasUsesTextureArray, VerticalClipState verticalClip, Node renderMountParent, int canvasLayer, int effectiveZIndex, int[] treeOrderPath, bool needsDrawItemSort, AdobeAnimateClipBlendState clipBlend = default(AdobeAnimateClipBlendState))
	{
		Sprite = sprite;
		Data = data;
		Definition = definition;
		Clip = clip;
		ClipRange = clipRange;
		FrameFloat = frameFloat;
		GlobalTransform = globalTransform;
		Offset = offset;
		Modulate = modulate;
		LayerVisible = layerVisible;
		AllLayersVisible = allLayersVisible;
		CanUseLayerMask = canUseLayerMask;
		LayerMask = layerMask;
		LayerVisibleCount = Math.Max(0, layerVisibleCount);
		LayerVisibleValues = layerVisibleValues ?? System.Array.Empty<bool>();
		MediaReplaceAtlas = mediaReplaceAtlas;
		MediaReplaceRect = mediaReplaceRect;
		MediaReplaceUse = mediaReplaceUse;
		HasMediaReplace = hasMediaReplace;
		MediaReplaceLimit = Math.Max(0, mediaReplaceLimit);
		MediaReplaceUseMask = mediaReplaceUseMask;
		MediaReplaceUseMaskOverflow = mediaReplaceUseMaskOverflow;
		MediaReplaceAtlasPages = mediaReplaceAtlasPages;
		MediaReplaceAtlasPageCount = Math.Max(0, mediaReplaceAtlasPageCount);
		MediaReplaceUseValues = mediaReplaceUseValues ?? System.Array.Empty<bool>();
		MediaReplaceRects = mediaReplaceRects ?? System.Array.Empty<Rect2>();
		MediaReplaceAtlasPageValues = mediaReplaceAtlasPageValues ?? System.Array.Empty<int>();
		MediaReplaceAtlasArray = mediaReplaceAtlasArray;
		MediaReplaceAtlasArraySize = mediaReplaceAtlasArraySize;
		MediaReplaceAtlasUsesTextureArray = mediaReplaceAtlasUsesTextureArray;
		VerticalClip = verticalClip;
		RenderMountParent = renderMountParent;
		CanvasLayer = canvasLayer;
		EffectiveZIndex = effectiveZIndex;
		TreeOrderPath = ((treeOrderPath == null || treeOrderPath.Length == 0) ? System.Array.Empty<int>() : treeOrderPath);
		NeedsDrawItemSort = needsDrawItemSort;
		ClipBlend = clipBlend;
	}
}
