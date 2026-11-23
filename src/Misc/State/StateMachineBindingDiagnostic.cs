public readonly struct StateMachineBindingDiagnostic
{
	public string Code { get; }

	public string Message { get; }

	public string CallbackKey { get; }

	public string StableId { get; }

	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(Code))
			{
				return string.IsNullOrEmpty(Message);
			}
			return false;
		}
	}

	public StateMachineBindingDiagnostic(string code, string message, string callbackKey = "", string stableId = "")
	{
		Code = code ?? string.Empty;
		Message = message ?? string.Empty;
		CallbackKey = callbackKey ?? string.Empty;
		StableId = stableId ?? string.Empty;
	}
}
