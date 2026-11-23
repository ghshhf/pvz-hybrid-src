using System;
using Godot;

public readonly struct CompiledStateMachineGuard
{
	public readonly bool IsDefined;

	public readonly StateMachineGuardKind Kind;

	public readonly StringName PropertyName;

	public readonly StateMachineComparisonOperator Operator;

	public readonly Variant ExpectedValue;

	public readonly bool Negate;

	public readonly StringName CallbackKey;

	public readonly int CallbackIndex;

	public readonly CompiledStateMachineGuard[] Children;

	public CompiledStateMachineGuard(bool isDefined, StateMachineGuardKind kind, StringName propertyName, StateMachineComparisonOperator comparisonOperator, Variant expectedValue, bool negate, StringName callbackKey, int callbackIndex, CompiledStateMachineGuard[] children = null)
	{
		IsDefined = isDefined;
		Kind = kind;
		PropertyName = propertyName;
		Operator = comparisonOperator;
		ExpectedValue = expectedValue;
		Negate = negate;
		CallbackKey = callbackKey;
		CallbackIndex = callbackIndex;
		Children = children ?? Array.Empty<CompiledStateMachineGuard>();
	}
}
