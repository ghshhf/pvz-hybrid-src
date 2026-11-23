public sealed record CharacterComponentAttachResult(CharacterComponentRuntime Runtime, string WireKey, string Message)
{
	public bool Success => Runtime != null;
}
