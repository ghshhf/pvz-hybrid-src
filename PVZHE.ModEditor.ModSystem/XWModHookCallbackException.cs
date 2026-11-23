using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModHookCallbackException : Exception
{
	public string OwnerId { get; }

	public string PointId { get; }

	internal XWModHookCallbackException(string owner, string point, Exception inner)
		: base($"Mod '{owner}' 扩展点 '{point}' 执行失败。", inner)
	{
		OwnerId = owner;
		PointId = point;
	}
}
