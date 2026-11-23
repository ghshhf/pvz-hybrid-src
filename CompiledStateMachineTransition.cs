using System;
using Godot;

public readonly struct CompiledStateMachineTransition
{
	public readonly int SourceIndex;

	public readonly int TargetIndex;

	public readonly StateMachineTriggerKind TriggerKind;

	public readonly StringName EventName;

	public readonly double DelaySeconds;

	public readonly int Priority;

	public readonly int DeclarationOrder;

	public readonly string StableId;

	public readonly CompiledStateMachineGuard Guard;

	public readonly StateMachineExtensionProperty[] ExtensionProperties;

	public CompiledStateMachineTransition(int sourceIndex, int targetIndex, StateMachineTriggerKind triggerKind, StringName eventName, double delaySeconds, int priority, int declarationOrder, string stableId, CompiledStateMachineGuard guard, StateMachineExtensionProperty[] extensionProperties)
	{
		SourceIndex = sourceIndex;
		TargetIndex = targetIndex;
		TriggerKind = triggerKind;
		EventName = eventName;
		DelaySeconds = delaySeconds;
		Priority = priority;
		DeclarationOrder = declarationOrder;
		StableId = stableId;
		Guard = guard;
		ExtensionProperties = extensionProperties ?? Array.Empty<StateMachineExtensionProperty>();
	}

	public bool TryGetExtensionProperty(StringName propertyName, out Variant value)
	{
		return StateMachineExtensionProperties.TryGet(ExtensionProperties, propertyName, out value);
	}
}
