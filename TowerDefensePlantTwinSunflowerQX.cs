using System.ComponentModel;
using Godot;
using Godot.Bridge;

[Tool]
[ScriptPath("res://Asset/Anime/Character/Plant/Cover/TwinSunflowerQX/Scene/TowerDefensePlantTwinSunflowerQX.cs")]
public class TowerDefensePlantTwinSunflowerQX : TowerDefensePlantSunFlowerQX
{
	public new class MethodName : TowerDefensePlantSunFlowerQX.MethodName
	{
	}

	public new class PropertyName : TowerDefensePlantSunFlowerQX.PropertyName
	{
	}

	public new class SignalName : TowerDefensePlantSunFlowerQX.SignalName
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
