using System;
using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModNotificationPoint<TPayload>
{
	public string Id { get; }

	public Type PayloadType => typeof(TPayload);

	public IReadOnlyList<XWModHookKind> AllowedKinds { get; } = Array.AsReadOnly(new XWModHookKind[1] { XWModHookKind.Subscribe });

	internal XWModNotificationPoint(string id)
	{
		Id = id;
	}
}
