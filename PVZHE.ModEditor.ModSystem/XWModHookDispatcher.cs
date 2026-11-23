using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModHookDispatcher : IDisposable
{
	private sealed class Registration : IDisposable
	{
		public string Owner;

		public string Id;

		public string Point;

		public int Order;

		public XWModHookKind Kind;

		public Delegate Callback;

		public bool Active = true;

		public XWModHookDispatcher Dispatcher;

		public void Dispose()
		{
			if (Active)
			{
				Active = false;
				Callback = null;
				Dispatcher.RegistrationRemoved(Owner);
			}
		}
	}

	private sealed class HostFailure : Exception
	{
		public ExceptionDispatchInfo Original { get; }

		public HostFailure(Exception ex)
		{
			Original = ExceptionDispatchInfo.Capture(ex);
		}
	}

	private readonly Dictionary<string, List<Registration>> _points = new Dictionary<string, List<Registration>>(StringComparer.Ordinal);

	private readonly Dictionary<string, int> _owners = new Dictionary<string, int>(StringComparer.Ordinal);

	private readonly HashSet<string> _failedOwners = new HashSet<string>(StringComparer.Ordinal);

	private readonly Action<string, Exception> _invalidate;

	private readonly Func<string, IDisposable> _enterOwner;

	private bool _frozen;

	private bool _disposing;

	private bool _removingOwner;

	public long Revision { get; private set; }

	internal T Modify<T>(XWModModifierPoint<T> point, T value, XWModEventContext context)
	{
		if (context.Authority != point.Descriptor.Authority)
		{
			throw new InvalidOperationException("Extension authority mismatch.");
		}
		if (point.Descriptor.Authority == XWModAuthority.Host && Global.IsMultiplayerMode && !MultiPlayerManager.IsHost)
		{
			return value;
		}
		XWModEventOrigin origin = context.Origin;
		if ((uint)(origin - 2) <= 1u)
		{
			return value;
		}
		if (!_points.TryGetValue(point.Id, out var value2))
		{
			return value;
		}
		using (XWModExtensions.EnterDispatch())
		{
			T arg = value;
			Registration[] array = value2.ToArray();
			foreach (Registration registration in array)
			{
				if (!registration.Active)
				{
					continue;
				}
				long start = XWModExtensions.BeginMeasurement(point.Id, registration.Owner);
				Exception error = null;
				try
				{
					using (_enterOwner?.Invoke(registration.Owner))
					{
						using (XWModExtensions.EnterCallback(registration.Owner, modifier: true, context))
						{
							T val = ((Func<XWModEventContext, T, T>)registration.Callback)(context, value);
							if (point.NormalizeCandidate != null)
							{
								val = point.NormalizeCandidate(arg, val);
							}
							if (!point.Validate(arg, val))
							{
								goto IL_0131;
							}
							Func<T, T, bool> validateTransition = point.ValidateTransition;
							if (validateTransition != null && !validateTransition(value, val))
							{
								goto IL_0131;
							}
							value = val;
							goto end_IL_00da;
							IL_0131:
							throw new InvalidOperationException("Modifier returned a value outside the extension contract.");
							end_IL_00da:;
						}
					}
				}
				catch (XWModHookCallbackException)
				{
					throw;
				}
				catch (Exception ex2)
				{
					error = ex2;
					Failed(registration, ex2);
				}
				finally
				{
					XWModExtensions.EndMeasurement(point.Id, registration.Owner, start, error);
				}
			}
			return value;
		}
	}

	internal void NotifyForOwners<T>(XWModEventPoint<T> point, Func<string, T> payloadFactory, XWModEventContext context)
	{
		if (context.Authority != point.Descriptor.Authority)
		{
			throw new InvalidOperationException("Extension authority mismatch.");
		}
		if ((point.Descriptor.Authority == XWModAuthority.Host && Global.IsMultiplayerMode && !MultiPlayerManager.IsHost) || !_points.TryGetValue(point.Id, out var value))
		{
			return;
		}
		using (XWModExtensions.EnterDispatch())
		{
			Registration[] array = value.ToArray();
			foreach (Registration entry in array)
			{
				if (!entry.Active)
				{
					continue;
				}
				long start = XWModExtensions.BeginMeasurement(point.Id, entry.Owner);
				Exception error = null;
				try
				{
					using (_enterOwner?.Invoke(entry.Owner))
					{
						using (XWModExtensions.EnterCallback(entry.Owner, modifier: false, context))
						{
							((Action<XWModEventContext, T>)entry.Callback)(context, payloadFactory(entry.Owner));
						}
					}
				}
				catch (XWModHookCallbackException)
				{
				}
				catch (Exception ex2)
				{
					error = ex2;
					if (point.Descriptor.LocalPresentation)
					{
						foreach (XWModExtensionDescriptor item in XWModExtensionCatalog.Installed.Where((XWModExtensionDescriptor x) => x.LocalPresentation))
						{
							if (!_points.TryGetValue(item.Id, out var value2))
							{
								continue;
							}
							foreach (Registration item2 in value2.Where((Registration x) => x.Owner == entry.Owner))
							{
								item2.Active = false;
								item2.Callback = null;
							}
						}
						Revision++;
						XWModExtensions.PresentationFailed(entry.Owner);
						GD.PushWarning("[Mod:" + entry.Owner + "] Local presentation stopped: " + ex2.GetBaseException().Message);
					}
					else
					{
						try
						{
							Failed(entry, ex2);
						}
						catch (XWModHookCallbackException)
						{
						}
					}
				}
				finally
				{
					XWModExtensions.EndMeasurement(point.Id, entry.Owner, start, error);
				}
			}
		}
	}

	public XWModHookDispatcher(Action<string, Exception> ownerFault = null, Func<string, IDisposable> enterOwner = null)
	{
		_invalidate = ownerFault;
		_enterOwner = enterOwner;
	}

	public XWModHooks ForOwner(string ownerId, int order)
	{
		if (_frozen)
		{
			throw new InvalidOperationException("Mod 扩展点已冻结。");
		}
		if (string.IsNullOrWhiteSpace(ownerId))
		{
			throw new ArgumentException("缺少 Mod ID。", "ownerId");
		}
		if (_owners.TryGetValue(ownerId, out var value) && value != order)
		{
			throw new InvalidOperationException("同一 Mod 的实际加载顺序不能改变。");
		}
		_owners[ownerId] = order;
		return new XWModHooks(this, ownerId, order);
	}

	internal IDisposable Register(string owner, int order, string point, string id, XWModHookKind kind, Delegate callback)
	{
		if (_frozen)
		{
			throw new InvalidOperationException("Mod 扩展点已冻结；结构变更需要重启。");
		}
		if (_failedOwners.Contains(owner))
		{
			throw new InvalidOperationException("故障 Mod 不能继续登记扩展点。");
		}
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException("缺少稳定注册 ID。", "id");
		}
		if ((object)callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (_points.Values.SelectMany((List<Registration> x) => x).Any((Registration x) => x.Owner == owner && x.Id == id))
		{
			throw new InvalidOperationException($"Mod '{owner}' 的注册 ID '{id}' 重复。");
		}
		if (!_points.TryGetValue(point, out var value))
		{
			value = (_points[point] = new List<Registration>());
		}
		Registration registration = new Registration
		{
			Owner = owner,
			Order = order,
			Point = point,
			Id = id,
			Kind = kind,
			Callback = callback,
			Dispatcher = this
		};
		value.Add(registration);
		Revision++;
		value.Sort(delegate(Registration a, Registration b)
		{
			int num = a.Order.CompareTo(b.Order);
			if (num == 0)
			{
				num = StringComparer.Ordinal.Compare(a.Owner, b.Owner);
			}
			return (num == 0) ? StringComparer.Ordinal.Compare(a.Id, b.Id) : num;
		});
		return registration;
	}

	public void Freeze()
	{
		_frozen = true;
	}

	public void Dispose()
	{
		_frozen = true;
		_disposing = true;
		foreach (List<Registration> value in _points.Values)
		{
			foreach (Registration item in value)
			{
				item.Dispose();
			}
		}
		_points.Clear();
		_owners.Clear();
	}

	public bool HasHandlers(string pointId)
	{
		if (_points.TryGetValue(pointId, out var value))
		{
			return value.Any((Registration x) => x.Active);
		}
		return false;
	}

	private void RegistrationRemoved(string owner)
	{
		Revision++;
		if (_frozen && !_disposing && !_removingOwner)
		{
			_invalidate?.Invoke(owner, new InvalidOperationException("冻结后的扩展点登记已释放，需要重启。"));
		}
	}

	public void RemoveOwner(string ownerId)
	{
		RemoveOwnerCore(ownerId, notify: true);
	}

	private void RemoveOwnerCore(string ownerId, bool notify)
	{
		bool flag = _failedOwners.Add(ownerId);
		long revision = Revision;
		_removingOwner = true;
		try
		{
			foreach (List<Registration> value in _points.Values)
			{
				foreach (Registration item in value)
				{
					if (item.Owner == ownerId)
					{
						item.Dispose();
					}
				}
			}
		}
		finally
		{
			_removingOwner = false;
		}
		if (flag && revision == Revision)
		{
			Revision++;
		}
		if (notify && _frozen && !_disposing && revision != Revision)
		{
			_invalidate?.Invoke(ownerId, new InvalidOperationException("冻结后的 Mod 扩展点已撤销，需要重启。"));
		}
	}

	public IReadOnlyList<XWModHookSnapshot> GetSnapshot()
	{
		return (from x in (from x in _points.Values.SelectMany((List<Registration> x) => x)
				orderby x.Order
				select x).ThenBy<Registration, string>((Registration x) => x.Owner, StringComparer.Ordinal).ThenBy<Registration, string>((Registration x) => x.Id, StringComparer.Ordinal)
			select new XWModHookSnapshot(x.Owner, x.Id, x.Point, x.Kind, x.Active, x.Order)).ToArray();
	}

	private void Failed(Registration registration, Exception ex)
	{
		RemoveOwnerCore(registration.Owner, notify: false);
		XWModHookCallbackException ex2 = new XWModHookCallbackException(registration.Owner, registration.Point, ex);
		try
		{
			_invalidate?.Invoke(registration.Owner, ex2);
		}
		catch (Exception ex3)
		{
			throw new AggregateException(ex2, ex3);
		}
		throw ex2;
	}

	public void Publish<T>(XWModNotificationPoint<T> point, T payload)
	{
		PublishForOwners(point, (string _) => payload);
	}

	public void PublishForOwners<T>(XWModNotificationPoint<T> point, Func<string, T> payloadFactory)
	{
		if (!_points.TryGetValue(point.Id, out var value))
		{
			return;
		}
		using (XWModExtensions.EnterDispatch())
		{
			XWModEventContext context = XWModExtensions.CreateInheritedContext(XWModAuthority.Host, point.Id);
			Registration[] array = value.ToArray();
			foreach (Registration registration in array)
			{
				if (!registration.Active)
				{
					continue;
				}
				try
				{
					using (_enterOwner?.Invoke(registration.Owner))
					{
						using (XWModExtensions.EnterCallback(registration.Owner, modifier: false, context))
						{
							T obj = payloadFactory(registration.Owner);
							((Action<T>)registration.Callback)(obj);
						}
					}
				}
				catch (XWModHookCallbackException)
				{
					throw;
				}
				catch (Exception ex2)
				{
					Failed(registration, ex2);
				}
			}
		}
	}

	public TResult Invoke<TRequest, TResult>(XWModHookPoint<TRequest, TResult> point, TRequest request, Func<TResult> original)
	{
		if (original == null)
		{
			throw new ArgumentNullException("original");
		}
		if (!_points.TryGetValue(point.Id, out var value) || !value.Any((Registration x) => x.Active))
		{
			return original();
		}
		bool called;
		InvalidOperationException originalCallViolation;
		HostFailure hostFailure;
		using (XWModExtensions.EnterDispatch())
		{
			XWModEventContext context = XWModExtensions.CreateInheritedContext(XWModAuthority.Host, point.Id);
			Registration[] array = value.ToArray();
			Registration[] array2 = array;
			foreach (Registration registration in array2)
			{
				if (!registration.Active || registration.Kind != XWModHookKind.Before)
				{
					continue;
				}
				try
				{
					using (_enterOwner?.Invoke(registration.Owner))
					{
						using (XWModExtensions.EnterCallback(registration.Owner, modifier: true, context))
						{
							((Action<TRequest>)registration.Callback)(request);
						}
					}
				}
				catch (XWModHookCallbackException)
				{
					throw;
				}
				catch (Exception ex2)
				{
					Failed(registration, ex2);
				}
			}
			Registration registration2 = array.LastOrDefault((Registration x) => x.Active && x.Kind == XWModHookKind.Replace);
			TResult val;
			if (registration2 == null)
			{
				val = original();
			}
			else
			{
				called = false;
				originalCallViolation = null;
				hostFailure = null;
				try
				{
					using (_enterOwner?.Invoke(registration2.Owner))
					{
						using (XWModExtensions.EnterCallback(registration2.Owner, modifier: true, context))
						{
							val = ((Func<TRequest, Func<TResult>, TResult>)registration2.Callback)(request, CallOriginal);
							if (hostFailure != null)
							{
								hostFailure.Original.Throw();
							}
							if (originalCallViolation != null)
							{
								throw originalCallViolation;
							}
						}
					}
				}
				catch (HostFailure hostFailure2)
				{
					hostFailure2.Original.Throw();
					throw;
				}
				catch (XWModHookCallbackException)
				{
					throw;
				}
				catch (Exception ex4)
				{
					if (hostFailure != null)
					{
						hostFailure.Original.Throw();
					}
					Failed(registration2, ex4);
					throw;
				}
			}
			array2 = array;
			foreach (Registration registration3 in array2)
			{
				if (!registration3.Active || registration3.Kind != XWModHookKind.After)
				{
					continue;
				}
				try
				{
					using (_enterOwner?.Invoke(registration3.Owner))
					{
						using (XWModExtensions.EnterCallback(registration3.Owner, modifier: true, context))
						{
							val = ((Func<TRequest, TResult, TResult>)registration3.Callback)(request, val);
						}
					}
				}
				catch (XWModHookCallbackException)
				{
					throw;
				}
				catch (Exception ex6)
				{
					Failed(registration3, ex6);
				}
			}
			return val;
		}
		TResult CallOriginal()
		{
			if (called)
			{
				originalCallViolation = new InvalidOperationException("原操作最多调用一次。");
				throw originalCallViolation;
			}
			called = true;
			try
			{
				return original();
			}
			catch (Exception ex7)
			{
				hostFailure = new HostFailure(ex7);
				throw hostFailure;
			}
		}
	}
}
