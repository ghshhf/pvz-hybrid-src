public readonly struct StateMachineCallbackRegistrationResult
{
	public bool Success { get; }

	public string Error { get; }

	public int CallbackCount { get; }

	public StateMachineCallbackRegistrationResult(bool success, string error = "", int callbackCount = 0)
	{
		Success = success;
		Error = error ?? string.Empty;
		CallbackCount = callbackCount;
	}
}
