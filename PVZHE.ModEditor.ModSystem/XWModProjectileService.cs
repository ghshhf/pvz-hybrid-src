using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModProjectileService
{
	private readonly string _owner;

	private readonly XWModRuntimeSession _runtimeSession;

	internal XWModProjectileService(string owner)
	{
		_owner = XWModSelectionState.Owner(owner);
		_runtimeSession = XWModRuntimeSession.Current;
	}

	public XWModProjectileSource CreateSource(string instanceKey)
	{
		if (_runtimeSession == XWModRuntimeSession.Current)
		{
			XWModRuntimeSession runtimeSession = _runtimeSession;
			if (runtimeSession != null && runtimeSession.IsOwnerActive(_owner))
			{
				if (XWModExtensions.SessionId == Guid.Empty)
				{
					throw new InvalidOperationException("Projectile sources require an active battle.");
				}
				if (string.IsNullOrWhiteSpace(instanceKey) || instanceKey.Length > 256 || instanceKey.Contains('\0'))
				{
					throw new ArgumentException("A weapon instance key of at most 256 characters is required.", "instanceKey");
				}
				return new XWModProjectileSource(_owner, XWModExtensions.SessionId, instanceKey, Guid.NewGuid());
			}
		}
		throw new ObjectDisposedException("XWModProjectileService");
	}
}
