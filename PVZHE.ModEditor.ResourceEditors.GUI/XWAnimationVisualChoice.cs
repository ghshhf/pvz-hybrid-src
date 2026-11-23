using Godot;

namespace PVZHE.ModEditor.ResourceEditors.GUI;

public readonly struct XWAnimationVisualChoice
{
	public int Id { get; }

	public string Title { get; }

	public string Subtitle { get; }

	public Texture2D Icon { get; }

	public Color Accent { get; }

	public XWAnimationVisualChoice(int id, string title, string subtitle, Texture2D icon, Color accent)
	{
		Id = id;
		Title = title ?? "";
		Subtitle = subtitle ?? "";
		Icon = icon;
		Accent = accent;
	}
}
