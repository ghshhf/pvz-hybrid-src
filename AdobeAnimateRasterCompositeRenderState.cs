using System;
using Godot;

internal readonly struct AdobeAnimateRasterCompositeRenderState
{
	public AdobeAnimateRuntimeDefinition Definition { get; }

	public Node RenderMountParent { get; }

	public Transform2D GlobalTransform { get; }

	public Color Modulate { get; }

	public VerticalClipState VerticalClip { get; }

	public int EffectiveZIndex { get; }

	public AdobeAnimateRasterCompositeData RasterCompositeData { get; }

	public int RasterCompositeTileIndex { get; }

	public AdobeAnimateRasterCompositeRenderState(AdobeAnimateRuntimeDefinition definition, Node renderMountParent, Transform2D globalTransform, Color modulate, VerticalClipState verticalClip, int effectiveZIndex, AdobeAnimateRasterCompositeData rasterCompositeData, int rasterCompositeTileIndex)
	{
		Definition = definition;
		RenderMountParent = renderMountParent;
		GlobalTransform = globalTransform;
		Modulate = modulate;
		VerticalClip = verticalClip;
		EffectiveZIndex = effectiveZIndex;
		RasterCompositeData = rasterCompositeData;
		RasterCompositeTileIndex = Math.Max(0, rasterCompositeTileIndex);
	}

	public AdobeAnimateRasterCompositeRenderState WithDynamicState(in Transform2D globalTransform, in Color modulate, int effectiveZIndex, int rasterCompositeTileIndex)
	{
		return new AdobeAnimateRasterCompositeRenderState(Definition, RenderMountParent, globalTransform, modulate, VerticalClip, effectiveZIndex, RasterCompositeData, rasterCompositeTileIndex);
	}
}
