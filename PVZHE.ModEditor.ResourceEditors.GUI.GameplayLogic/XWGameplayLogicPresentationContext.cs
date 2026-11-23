using Godot;
using PVZHE.ModEditor.Core;

namespace PVZHE.ModEditor.ResourceEditors.GUI.GameplayLogic;

public sealed class XWGameplayLogicPresentationContext
{
	public Resource Resource { get; set; }

	public XWResourceEditContext EditContext { get; set; }

	public XWVisualPropertyBinding PropertyBinding { get; set; }

	public XWEmbeddedResourceEditService EmbeddedResourceEditService { get; set; }

	public XWGameplayResourcePickerWindow ResourcePicker { get; set; }

	public XWGameplayLogicPreviewSafety PreviewSafety { get; set; }

	public SubViewport StageViewport { get; set; }

	public Node2D StageRoot { get; set; }

	public Control OverlayRoot { get; set; }

	public Control HudRoot { get; set; }

	public Control TimelineRoot { get; set; }

	public Control ShelfRoot { get; set; }

	public Control AdvancedInspectorHost { get; set; }

	public Control ErrorBar { get; set; }
}
