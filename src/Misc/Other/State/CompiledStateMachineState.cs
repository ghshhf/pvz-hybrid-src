using System;
using Godot;

public readonly struct CompiledStateMachineState
{
	public readonly string StableId;

	public readonly StringName DisplayName;

	public readonly StateMachineStateKind Kind;

	public readonly int ParentIndex;

	public readonly int InitialChildIndex;

	public readonly int Depth;

	public readonly StateMachineProcessFlags ProcessFlags;

	public readonly StringName CallbackKey;

	public readonly StringName EnterCallbackKey;

	public readonly StringName ExitCallbackKey;

	public readonly StringName ProcessCallbackKey;

	public readonly StringName PhysicsProcessCallbackKey;

	public readonly StateMachineExtensionProperty[] ExtensionProperties;

	public CompiledStateMachineState(string stableId, StringName displayName, StateMachineStateKind kind, int parentIndex, int initialChildIndex, int depth, StateMachineProcessFlags processFlags, StringName callbackKey, StringName enterCallbackKey, StringName exitCallbackKey, StringName processCallbackKey, StringName physicsProcessCallbackKey, StateMachineExtensionProperty[] extensionProperties)
	{
		StableId = stableId;
		DisplayName = displayName;
		Kind = kind;
		ParentIndex = parentIndex;
		InitialChildIndex = initialChildIndex;
		Depth = depth;
		ProcessFlags = processFlags;
		CallbackKey = callbackKey;
		EnterCallbackKey = enterCallbackKey;
		ExitCallbackKey = exitCallbackKey;
		ProcessCallbackKey = processCallbackKey;
		PhysicsProcessCallbackKey = physicsProcessCallbackKey;
		ExtensionProperties = extensionProperties ?? Array.Empty<StateMachineExtensionProperty>();
	}

	public StringName GetLifecycleCallbackKey(StateMachineCallbackPhase phase)
	{
		StringName stringName = phase switch
		{
			StateMachineCallbackPhase.Enter => EnterCallbackKey, 
			StateMachineCallbackPhase.Exit => ExitCallbackKey, 
			StateMachineCallbackPhase.Process => ProcessCallbackKey, 
			StateMachineCallbackPhase.PhysicsProcess => PhysicsProcessCallbackKey, 
			_ => new StringName(), 
		};
		if (!string.IsNullOrWhiteSpace(stringName.ToString()))
		{
			return stringName;
		}
		return CallbackKey;
	}

	public bool TryGetExtensionProperty(StringName propertyName, out Variant value)
	{
		return StateMachineExtensionProperties.TryGet(ExtensionProperties, propertyName, out value);
	}
}
