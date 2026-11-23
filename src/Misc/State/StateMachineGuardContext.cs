using System;
using Godot;

public readonly struct StateMachineGuardContext
{
	public object Host { get; }

	public StateMachineRuntime Runtime { get; }

	public string SourceStateId { get; }

	public string TargetStateId { get; }

	public StringName EventName { get; }

	public StateMachineGuardContext(object host, StateMachineRuntime runtime, string sourceStateId, string targetStateId, StringName eventName)
	{
		Host = host;
		Runtime = runtime;
		SourceStateId = sourceStateId ?? string.Empty;
		TargetStateId = targetStateId ?? string.Empty;
		EventName = eventName;
	}

	public Variant GetProperty(StringName name, Variant fallback = default(Variant))
	{
		return Runtime?.GetExpressionProperty(name, fallback) ?? fallback;
	}

	public T RequireHost<T>() where T : class
	{
		return (Host as T) ?? throw new InvalidOperationException($"State-machine guard requires host '{typeof(T).FullName}', but received '{Host?.GetType().FullName ?? "<null>"}'.");
	}
}
