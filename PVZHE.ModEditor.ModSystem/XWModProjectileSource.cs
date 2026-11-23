using System;

namespace PVZHE.ModEditor.ModSystem;

public readonly record struct XWModProjectileSource
{
	public string ModId { get; }

	public Guid SessionId { get; }

	public string InstanceKey { get; }

	public Guid InstanceId { get; }

	public bool IsEmpty => InstanceId == Guid.Empty;

	public bool IsCurrentBattle
	{
		get
		{
			if (!IsEmpty)
			{
				return SessionId == XWModExtensions.SessionId;
			}
			return false;
		}
	}

	internal XWModProjectileSource(string modId, Guid sessionId, string instanceKey, Guid instanceId)
	{
		ModId = modId;
		SessionId = sessionId;
		InstanceKey = instanceKey;
		InstanceId = instanceId;
	}
}
