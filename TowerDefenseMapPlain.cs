using System.ComponentModel;
using Godot;
using Godot.Bridge;

[GlobalClass]
[ScriptPath("res://Registry/Battle/Feature/Map/Base/TowerDefenseMapPlain.cs")]
public class TowerDefenseMapPlain : TowerDefenseMap
{
	public new class MethodName : TowerDefenseMap.MethodName
	{
	}

	public new class PropertyName : TowerDefenseMap.PropertyName
	{
	}

	public new class SignalName : TowerDefenseMap.SignalName
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
