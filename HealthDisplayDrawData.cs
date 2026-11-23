using Godot;

internal readonly struct HealthDisplayDrawData
{
	public readonly Vector2 Center;

	public readonly string ShieldValueText;

	public readonly string ShieldText;

	public readonly string HelmetText;

	public readonly string BodyText;

	public readonly Color ShieldValueColor;

	public readonly Color ShieldColor;

	public readonly Color HelmetColor;

	public readonly Color BodyColor;

	public HealthDisplayDrawData(Vector2 center, string shieldValueText, string shieldText, string helmetText, string bodyText, Color shieldValueColor, Color shieldColor, Color helmetColor, Color bodyColor)
	{
		Center = center;
		ShieldValueText = shieldValueText;
		ShieldText = shieldText;
		HelmetText = helmetText;
		BodyText = bodyText;
		ShieldValueColor = shieldValueColor;
		ShieldColor = shieldColor;
		HelmetColor = helmetColor;
		BodyColor = bodyColor;
	}
}
