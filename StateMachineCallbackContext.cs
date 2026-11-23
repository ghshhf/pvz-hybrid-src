using System;
using Godot;

public readonly struct StateMachineCallbackContext
{
	public object Host { get; }

	public StateMachineRuntime Runtime { get; }

	public StateHandle State { get; }

	public StateMachineCallbackContext(object host, StateMachineRuntime runtime, StateHandle state)
	{
		Host = host;
		Runtime = runtime;
		State = state;
	}

	public bool SendEvent(StringName eventName)
	{
		return Runtime?.SendEvent(eventName) ?? false;
	}

	public Variant GetProperty(StringName name, Variant fallback = default(Variant))
	{
		return Runtime?.GetExpressionProperty(name, fallback) ?? fallback;
	}

	public void SetProperty(StringName name, Variant value)
	{
		Runtime?.SetExpressionProperty(name, value);
	}

	public T RequireHost<T>() where T : class
	{
		return (Host as T) ?? throw new InvalidOperationException($"State-machine callback requires host '{typeof(T).FullName}', but received '{Host?.GetType().FullName ?? "<null>"}'.");
	}
}
