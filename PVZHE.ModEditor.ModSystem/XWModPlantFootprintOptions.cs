using System;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModPlantFootprintOptions(int Width, bool ReplaceSupports, bool ProtectFromReplacement)
{
	public bool Surround { get; set; }

	public bool ReplaceSurrounds { get; set; }

	public bool StrictCenter { get; set; }

	public bool RefundAsSunDrops { get; set; }

	public float PreviewOffsetY { get; set; }

	public Func<TowerDefenseCharacter, Vector2I, double, double> SupportHeightProvider { get; set; }
}
