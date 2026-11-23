using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModHooks
{
	private readonly XWModHookDispatcher _dispatcher;

	private readonly string _owner;

	private readonly int _order;

	public IDisposable Modify<T>(XWModModifierPoint<T> point, string id, Func<XWModEventContext, T, T> callback)
	{
		return _dispatcher.Register(_owner, _order, point.Id, id, XWModHookKind.Modify, callback);
	}

	public IDisposable Subscribe<T>(XWModEventPoint<T> point, string id, Action<XWModEventContext, T> callback)
	{
		return _dispatcher.Register(_owner, _order, point.Id, id, XWModHookKind.Subscribe, callback);
	}

	internal XWModHooks(XWModHookDispatcher dispatcher, string owner, int order)
	{
		_dispatcher = dispatcher;
		_owner = owner;
		_order = order;
	}

	public IDisposable Before<TRequest, TResult>(XWModHookPoint<TRequest, TResult> point, string id, Action<TRequest> callback)
	{
		return _dispatcher.Register(_owner, _order, point.Id, id, XWModHookKind.Before, callback);
	}

	public IDisposable After<TRequest, TResult>(XWModHookPoint<TRequest, TResult> point, string id, Func<TRequest, TResult, TResult> callback)
	{
		return _dispatcher.Register(_owner, _order, point.Id, id, XWModHookKind.After, callback);
	}

	public IDisposable Replace<TRequest, TResult>(XWModHookPoint<TRequest, TResult> point, string id, Func<TRequest, Func<TResult>, TResult> callback)
	{
		return _dispatcher.Register(_owner, _order, point.Id, id, XWModHookKind.Replace, callback);
	}

	public IDisposable Subscribe<T>(XWModNotificationPoint<T> point, string id, Action<T> callback)
	{
		return _dispatcher.Register(_owner, _order, point.Id, id, XWModHookKind.Subscribe, callback);
	}
}
