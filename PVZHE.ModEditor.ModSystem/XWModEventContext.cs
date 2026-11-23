using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed record XWModEventContext(Guid SessionId, Guid OperationId, string SourceMod, XWModEventOrigin Origin, XWModAuthority Authority, string Reason)
{
	public string NetworkSessionId { get; set; } = "";
}
