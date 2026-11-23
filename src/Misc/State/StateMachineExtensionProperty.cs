using Godot;

public readonly struct StateMachineExtensionProperty
{
	public readonly StringName Name;

	public readonly Variant.Type DeclaredType;

	public readonly Variant Value;

	public StateMachineExtensionProperty(StringName name, Variant.Type declaredType, Variant value)
	{
		Name = name;
		DeclaredType = declaredType;
		Value = value;
	}
}
