using System;
using System.Collections.Generic;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModHookPoint<TRequest, TResult>
{
	public string Id { get; }

	public Type RequestType => typeof(TRequest);

	public Type ResultType => typeof(TResult);

	public IReadOnlyList<XWModHookKind> AllowedKinds { get; } = Array.AsReadOnly(new XWModHookKind[3]
	{
		XWModHookKind.Before,
		XWModHookKind.After,
		XWModHookKind.Replace
	});

	internal XWModHookPoint(string id)
	{
		Id = id;
	}
}
