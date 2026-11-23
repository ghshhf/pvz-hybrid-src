using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModExtensions
{
	private sealed class Counter
	{
		public long Calls;

		public long Faults;

		public long Rejected;

		public long Samples;

		public long Ticks;

		public string Last = "";
	}

	private sealed record Frame(string Owner, bool Modifier, XWModEventContext Context);

	private sealed record Pending(string Owner, string Operation, Action Action, Func<bool> Authorize, Guid Session, string[] Chain, XWModEventContext Context, Action<XWModCommandResult> Completion);

	private sealed class Scope(Action close) : IDisposable
	{
		private Action _close = close;

		public void Dispose()
		{
			Interlocked.Exchange(ref close, null)?.Invoke();
		}
	}

	private static readonly Dictionary<(string Point, string Owner), Counter> Diagnostics = new Dictionary<(string, string), Counter>();

	private const int MaximumDiagnostics = 1024;

	[ThreadStatic]
	private static Frame _frame;

	[ThreadStatic]
	private static Guid _operationId;

	[ThreadStatic]
	private static string[] _chain;

	[ThreadStatic]
	private static int _dispatchDepth;

	[ThreadStatic]
	private static bool _draining;

	[ThreadStatic]
	private static Queue<Pending> _pending;

	private static int _hostThread;

	private static Guid _lifecycleSession;

	private static bool _readyPublished;

	private static bool _resultPublished;

	private static bool _settlementPublished;

	private static string _result = "";

	private static string _levelKey = "";

	public static Guid SessionId { get; private set; }

	internal static Func<string> NetworkSessionProvider { get; set; }

	public static event Action<string> LocalPresentationFault;

	internal static event Action<string> OwnerEnding;

	public static IReadOnlyList<XWModExtensionDiagnostic> GetDiagnostics()
	{
		lock (Diagnostics)
		{
			return (from x in Diagnostics.OrderBy<KeyValuePair<(string Point, string Owner), Counter>, string>((KeyValuePair<(string Point, string Owner), Counter> x) => x.Key.Point, StringComparer.Ordinal).ThenBy<KeyValuePair<(string Point, string Owner), Counter>, string>((KeyValuePair<(string Point, string Owner), Counter> x) => x.Key.Owner, StringComparer.Ordinal)
				select new XWModExtensionDiagnostic(x.Key.Point, x.Key.Owner, x.Value.Calls, x.Value.Faults, x.Value.Rejected, x.Value.Samples, (double)x.Value.Ticks * 1000.0 / (double)Stopwatch.Frequency, x.Value.Last)).ToArray();
		}
	}

	internal static long BeginMeasurement(string point, string owner)
	{
		lock (Diagnostics)
		{
			(string, string) key = (point, owner);
			if (!Diagnostics.TryGetValue(key, out var value))
			{
				if (Diagnostics.Count >= 1024)
				{
					return 0L;
				}
				Diagnostics.Add(key, value = new Counter());
			}
			value.Calls++;
			return ((value.Calls & 0x3F) == 1) ? Stopwatch.GetTimestamp() : 0;
		}
	}

	internal static void EndMeasurement(string point, string owner, long start, Exception error = null, string rejection = null)
	{
		lock (Diagnostics)
		{
			if (Diagnostics.TryGetValue((point, owner), out var value))
			{
				if (start != 0L)
				{
					value.Samples++;
					value.Ticks += Stopwatch.GetTimestamp() - start;
				}
				if (error != null)
				{
					value.Faults++;
				}
				if (rejection != null)
				{
					value.Rejected++;
				}
				string text = error?.GetBaseException().Message ?? rejection;
				if (text != null)
				{
					value.Last = ((text.Length > 512) ? text.Substring(0, 512) : text);
				}
			}
		}
	}

	internal static void ResetDiagnostics()
	{
		lock (Diagnostics)
		{
			Diagnostics.Clear();
		}
	}

	internal static void NotifyOwnerEnding(string owner)
	{
		if (XWModExtensions.OwnerEnding == null)
		{
			return;
		}
		Delegate[] invocationList = XWModExtensions.OwnerEnding.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action<string> action = (Action<string>)invocationList[i];
			try
			{
				action(owner);
			}
			catch (Exception ex)
			{
				GD.PushWarning(ex.Message);
			}
		}
	}

	internal static IDisposable DeferCommands()
	{
		return EnterDispatch();
	}

	internal static IDisposable EnterOperation(Guid operationId = default(Guid))
	{
		EnsureHostThread();
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		if (current == null || !current.HasRuntimeOwners)
		{
			return null;
		}
		return EnterOwnedOperation(operationId);
	}

	private static IDisposable EnterOwnedOperation(Guid operationId)
	{
		Guid previous = _operationId;
		_operationId = ((operationId != Guid.Empty) ? operationId : (_frame?.Context.OperationId ?? ((_operationId != Guid.Empty) ? _operationId : Guid.NewGuid())));
		IDisposable dispatch = EnterDispatch();
		return new Scope(delegate
		{
			try
			{
				dispatch.Dispose();
			}
			finally
			{
				_operationId = previous;
			}
		});
	}

	internal static void BindHostThread()
	{
		_hostThread = System.Environment.CurrentManagedThreadId;
	}

	internal static void BeginBattle()
	{
		EnsureHostThread();
		SessionId = Guid.NewGuid();
		CancelPending();
	}

	internal static void EndBattle()
	{
		EnsureHostThread();
		SessionId = Guid.Empty;
		CancelPending();
	}

	public static XWModEventContext CreateEventContext(XWModEventOrigin origin = XWModEventOrigin.Gameplay, XWModAuthority authority = XWModAuthority.Host, string reason = "")
	{
		return new XWModEventContext(SessionId, _frame?.Context.OperationId ?? ((_operationId != Guid.Empty) ? _operationId : Guid.NewGuid()), _frame?.Owner ?? "", origin, authority, reason ?? "")
		{
			NetworkSessionId = (NetworkSessionProvider?.Invoke() ?? "")
		};
	}

	internal static XWModEventContext CreateInheritedContext(XWModAuthority authority = XWModAuthority.Host, string reason = "")
	{
		return CreateEventContext(_frame?.Context.Origin ?? XWModEventOrigin.Gameplay, authority, reason);
	}

	public static bool HasHandlers(string id)
	{
		return XWModRuntimeSession.Current?.Hooks.HasHandlers(id) ?? false;
	}

	internal static T Modify<T>(XWModModifierPoint<T> point, T value, XWModEventContext context = null)
	{
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		if (current != null)
		{
			return current.Hooks.Modify(point, value, context ?? CreateEventContext(XWModEventOrigin.Gameplay, point.Descriptor.Authority));
		}
		return value;
	}

	internal static void Notify<T>(XWModEventPoint<T> point, T payload, XWModEventContext context = null)
	{
		NotifyForOwners(point, (string _) => payload, context);
	}

	internal static void NotifyForOwners<T>(XWModEventPoint<T> point, Func<string, T> payloadFactory, XWModEventContext context = null)
	{
		XWModRuntimeSession.Current?.Hooks.NotifyForOwners(point, payloadFactory, context ?? CreateEventContext(XWModEventOrigin.Gameplay, point.Descriptor.Authority));
	}

	internal static void EnsureHostThread()
	{
		if (_hostThread != 0 && System.Environment.CurrentManagedThreadId != _hostThread)
		{
			throw new InvalidOperationException("Mod game operations require the host game thread.");
		}
	}

	internal static IDisposable EnterCallback(string owner, bool modifier, XWModEventContext context)
	{
		EnsureHostThread();
		Frame previous = _frame;
		IDisposable reflection = XWModRuntimeSession.Current?.EnterOwnerReflection(owner);
		_frame = new Frame(owner, modifier || (previous?.Modifier ?? false), context);
		return new Scope(delegate
		{
			try
			{
				reflection?.Dispose();
			}
			finally
			{
				_frame = previous;
			}
		});
	}

	internal static IDisposable EnterDispatch()
	{
		EnsureHostThread();
		_dispatchDepth++;
		return new Scope(delegate
		{
			if (--_dispatchDepth == 0)
			{
				Drain();
			}
		});
	}

	internal static void PresentationFailed(string owner)
	{
		if (XWModExtensions.LocalPresentationFault == null)
		{
			return;
		}
		Delegate[] invocationList = XWModExtensions.LocalPresentationFault.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action<string> action = (Action<string>)invocationList[i];
			try
			{
				action(owner);
			}
			catch (Exception ex)
			{
				GD.PushWarning(ex.Message);
			}
		}
	}

	internal static bool Command(string owner, string operation, Action action, Func<bool> authorization = null, string entityKey = "")
	{
		return ExecuteCommand(owner, operation, action, authorization, entityKey).Accepted;
	}

	internal static XWModCommandResult ExecuteCommand(string owner, string operation, Action action, Func<bool> authorization = null, string entityKey = "")
	{
		return Submit(owner, operation, action, authorization, entityKey, null);
	}

	internal static Task<XWModCommandResult<T>> ExecuteCommand<T>(string owner, string operation, Func<T> action, Func<bool> authorization = null, string entityKey = "")
	{
		TaskCompletionSource<XWModCommandResult<T>> completion = new TaskCompletionSource<XWModCommandResult<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
		T value = default(T);
		XWModCommandResult xWModCommandResult = Submit(owner, operation, delegate
		{
			value = action();
		}, authorization, entityKey, delegate(XWModCommandResult outcome)
		{
			completion.TrySetResult(new XWModCommandResult<T>(outcome.Status, value, outcome.Diagnostic));
		});
		if (xWModCommandResult.Status != XWModCommandStatus.Queued)
		{
			completion.TrySetResult(new XWModCommandResult<T>(xWModCommandResult.Status, value, xWModCommandResult.Diagnostic));
		}
		return completion.Task;
	}

	internal static bool TryMutableCallback(string owner, out string diagnostic)
	{
		EnsureHostThread();
		Frame frame = _frame;
		string text;
		if ((object)frame != null && frame.Modifier)
		{
			text = "Modifiers cannot perform mutable operations.";
			goto IL_009f;
		}
		string text2;
		if (_frame != null && !string.Equals(_frame.Owner, owner, StringComparison.OrdinalIgnoreCase))
		{
			text2 = "Operation owner does not match the current callback.";
			goto IL_009d;
		}
		XWModEventOrigin? xWModEventOrigin = _frame?.Context.Origin;
		bool flag;
		if (xWModEventOrigin.HasValue)
		{
			XWModEventOrigin valueOrDefault = xWModEventOrigin.GetValueOrDefault();
			if ((uint)(valueOrDefault - 1) <= 2u)
			{
				flag = true;
				goto IL_008c;
			}
		}
		flag = false;
		goto IL_008c;
		IL_009d:
		text = text2;
		goto IL_009f;
		IL_008c:
		text2 = (flag ? "Recovery notifications cannot perform mutable operations." : "");
		goto IL_009d;
		IL_009f:
		diagnostic = text;
		return diagnostic.Length == 0;
	}

	internal static void DemandMutableCallback(string owner)
	{
		if (!TryMutableCallback(owner, out var diagnostic))
		{
			throw new InvalidOperationException(diagnostic);
		}
	}

	private static XWModCommandResult Submit(string owner, string operation, Action action, Func<bool> authorization, string entityKey, Action<XWModCommandResult> completion)
	{
		EnsureHostThread();
		if (action == null || string.IsNullOrWhiteSpace(operation))
		{
			return Reject("Missing operation.");
		}
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		if (current == null || !current.IsOwnerActive(owner))
		{
			return Reject("Owner is not active.");
		}
		if (!TryMutableCallback(owner, out var diagnostic))
		{
			return Reject(diagnostic);
		}
		string text = owner + ":" + operation + ":" + entityKey;
		string[] array = _chain ?? Array.Empty<string>();
		if (array.Length >= 8 || array.Contains<string>(text, StringComparer.Ordinal))
		{
			return Reject("Operation cycle or depth limit.");
		}
		string[] chain = array.Append(text).ToArray();
		Pending pending = new Pending(owner, operation, action, authorization, SessionId, chain, CreateEventContext(), completion);
		if (_dispatchDepth > 0)
		{
			if ((_pending?.Count ?? 0) >= 1024)
			{
				return Reject("Pending operation limit reached.");
			}
			(_pending ?? (_pending = new Queue<Pending>())).Enqueue(pending);
			return new XWModCommandResult(XWModCommandStatus.Queued, "Queued until notification dispatch completes.");
		}
		return Run(pending);
		XWModCommandResult Reject(string text2)
		{
			string point = "command:" + operation;
			EndMeasurement(start: BeginMeasurement(point, owner), point: point, owner: owner, error: null, rejection: text2);
			return new XWModCommandResult(XWModCommandStatus.Rejected, text2);
		}
	}

	private static XWModCommandResult Run(Pending command)
	{
		string text = "command:" + command.Operation;
		long num = BeginMeasurement(text, command.Owner);
		XWModCommandResult xWModCommandResult = RunCore(command);
		string point = text;
		string owner = command.Owner;
		long start = num;
		Exception error = ((xWModCommandResult.Status == XWModCommandStatus.Failed) ? new InvalidOperationException(xWModCommandResult.Diagnostic) : null);
		XWModCommandStatus status = xWModCommandResult.Status;
		bool flag = ((status == XWModCommandStatus.Rejected || status == XWModCommandStatus.Cancelled) ? true : false);
		EndMeasurement(point, owner, start, error, flag ? xWModCommandResult.Diagnostic : null);
		return xWModCommandResult;
	}

	private static XWModCommandResult RunCore(Pending command)
	{
		if (!(command.Session != SessionId))
		{
			XWModRuntimeSession current = XWModRuntimeSession.Current;
			if (current != null && current.IsOwnerActive(command.Owner))
			{
				string[] chain = _chain;
				_chain = command.Chain;
				try
				{
					if (command.Authorize != null)
					{
						try
						{
							if (!command.Authorize())
							{
								return new XWModCommandResult(XWModCommandStatus.Rejected, "Operation permission or state rejected.");
							}
						}
						catch (XWModHookCallbackException ex)
						{
							return new XWModCommandResult(XWModCommandStatus.Failed, ex.Message);
						}
						catch (Exception ex2)
						{
							return new XWModCommandResult(XWModCommandStatus.Rejected, "Operation validation failed: " + ex2.GetBaseException().Message);
						}
					}
					using (EnterCallback(command.Owner, modifier: false, command.Context))
					{
						command.Action();
						return new XWModCommandResult(XWModCommandStatus.Completed, "");
					}
				}
				catch (XWModHookCallbackException ex3)
				{
					return new XWModCommandResult(XWModCommandStatus.Failed, ex3.Message);
				}
				catch (Exception ex4)
				{
					XWModEnvironmentService.Invalidate("ModHostOperationFailed:" + command.Owner + ":" + command.Operation);
					return new XWModCommandResult(XWModCommandStatus.Failed, ex4.GetBaseException().Message);
				}
				finally
				{
					_chain = chain;
				}
			}
		}
		return new XWModCommandResult(XWModCommandStatus.Cancelled, "Operation session or owner expired.");
	}

	private static void Drain()
	{
		if (_draining || _pending == null)
		{
			return;
		}
		_draining = true;
		try
		{
			Pending result;
			while (_pending.TryDequeue(out result))
			{
				XWModCommandResult obj = Run(result);
				result.Completion?.Invoke(obj);
			}
		}
		finally
		{
			_draining = false;
		}
	}

	private static void CancelPending()
	{
		if (_pending != null)
		{
			Pending result;
			while (_pending.TryDequeue(out result))
			{
				result.Completion?.Invoke(new XWModCommandResult(XWModCommandStatus.Cancelled, "Battle session ended."));
			}
		}
	}

	internal static void SunChanged(EconomyAccountId account, long before, long balance, SunTransactionReason reason)
	{
		if (account.IsLocal && HasHandlers(XWModEconomyPoints.BalanceChanged.Id))
		{
			XWModEventOrigin origin = reason switch
			{
				SunTransactionReason.Initialization => XWModEventOrigin.InitialBinding, 
				SunTransactionReason.Load => XWModEventOrigin.SaveRestore, 
				_ => XWModEventOrigin.Gameplay, 
			};
			Notify(XWModEconomyPoints.BalanceChanged, new XWModSunChange(account.Value, before, balance, reason.ToString()), CreateEventContext(origin, XWModAuthority.Owner, reason.ToString()));
		}
	}

	internal static void SunSpend(EconomyAccountId account, long amount, long delta, long id, string phase)
	{
		if (account.IsLocal && HasHandlers(XWModEconomyPoints.SpendChanged.Id))
		{
			Notify(XWModEconomyPoints.SpendChanged, new XWModSunSpend(account.Value, amount, delta, id, phase), CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Owner, phase));
		}
	}

	private static void EnsureLifecycleSession()
	{
		if (!(_lifecycleSession == SessionId))
		{
			_lifecycleSession = SessionId;
			_readyPublished = (_resultPublished = (_settlementPublished = false));
			_result = "";
			_levelKey = TowerDefenseManager.CurrentControl?.levelConfig?.name ?? "";
		}
	}

	internal static bool TryBattleReady(bool restored)
	{
		EnsureLifecycleSession();
		if (_readyPublished || SessionId == Guid.Empty)
		{
			return false;
		}
		_readyPublished = true;
		if (restored)
		{
			BattleRestored(XWModEventOrigin.SaveRestore);
		}
		else
		{
			Notify(XWModLifecyclePoints.Ready, new XWModBattleState(_levelKey, "ready"));
		}
		return !restored;
	}

	internal static void BattleRestored(XWModEventOrigin origin)
	{
		if ((uint)(origin - 2) > 1u)
		{
			throw new ArgumentOutOfRangeException("origin", "Restore notifications require a restore origin.");
		}
		EnsureLifecycleSession();
		if (!(SessionId == Guid.Empty))
		{
			Notify(XWModLifecyclePoints.Restored, new XWModBattleState(_levelKey, "restored"), CreateEventContext(origin, XWModAuthority.Local, "restored"));
		}
	}

	internal static bool BattleResult(string result)
	{
		EnsureLifecycleSession();
		if (_resultPublished || SessionId == Guid.Empty)
		{
			return false;
		}
		_resultPublished = true;
		_result = result;
		Notify(XWModLifecyclePoints.Result, new XWModBattleState(_levelKey, "result", result));
		return true;
	}

	internal static bool TryCompleteBattleSettlement(TowerDefenseBattleProcess process, Guid expectedSessionId)
	{
		EnsureHostThread();
		if (_frame != null || _dispatchDepth > 0)
		{
			return false;
		}
		TowerDefenseControlNew control = TowerDefenseManager.CurrentControl;
		if (!IsCurrentOwner())
		{
			return false;
		}
		EnsureLifecycleSession();
		if (!_resultPublished)
		{
			return false;
		}
		string result = _result;
		bool flag3;
		try
		{
			bool flag = ControllerTerminalMatches();
			if (flag)
			{
				string text = result;
				bool flag2 = ((text == "victory") ? process.CheckFinal() : (text == "failed" && process.CheckFail()));
				flag = flag2;
			}
			flag3 = flag;
		}
		catch (Exception ex)
		{
			GD.PushWarning("[BattleSettlement] Process terminal check failed: " + ex.Message);
			return false;
		}
		if (!flag3 || !IsCurrentOwner() || _lifecycleSession != expectedSessionId || !_resultPublished || _result != result || !ControllerTerminalMatches())
		{
			return false;
		}
		BattleSettlementCompleted();
		return true;
		bool ControllerTerminalMatches()
		{
			string text2 = result;
			if (text2 == "victory")
			{
				return control.HasBattleAwarded && !control.isGameFail;
			}
			if (text2 == "failed")
			{
				return control.isGameFail;
			}
			return false;
		}
		bool HasActiveRegistration()
		{
			if (string.IsNullOrEmpty(process.BoundProcessKey))
			{
				return false;
			}
			XWModRuntimeRegistry.TryGetEffectiveRegistration("Process", process.BoundProcessKey, out var registration);
			if (registration == process.BoundProcessRegistration)
			{
				if (registration != null)
				{
					if (string.Equals(registration.OwnerMod, process.BoundProcessOwner, StringComparison.OrdinalIgnoreCase))
					{
						return XWModRuntimeSession.Current?.IsOwnerActive(process.BoundProcessOwner) ?? false;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		bool IsCurrentOwner()
		{
			if (expectedSessionId != Guid.Empty && expectedSessionId == SessionId && GodotObject.IsInstanceValid(process) && process.IsLifetimeActive && HasActiveRegistration() && GodotObject.IsInstanceValid(control) && control.IsInsideTree() && !control.IsQueuedForDeletion() && control == TowerDefenseManager.CurrentControl && process.control == control && control.process == process && (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost))
			{
				return !control.HasPendingBattleOperations;
			}
			return false;
		}
	}

	internal static void BattleSettlementCompleted()
	{
		EnsureLifecycleSession();
		if (!_settlementPublished && _resultPublished && !(SessionId == Guid.Empty))
		{
			_settlementPublished = true;
			Notify(XWModLifecyclePoints.SettlementCompleted, new XWModBattleState(_levelKey, "settled", _result), CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Local));
		}
	}

	internal static void BattleExited()
	{
		EnsureLifecycleSession();
		if (!(SessionId == Guid.Empty))
		{
			Notify(XWModLifecyclePoints.Exited, new XWModBattleState(_levelKey, "exited", _result), CreateEventContext(XWModEventOrigin.Gameplay, XWModAuthority.Local));
		}
	}

	internal static IReadOnlyList<XWModReinforcement> GetWaveReinforcements(int wave, int lanes, Func<int, bool> laneEnabled)
	{
		if (!HasHandlers(XWModLifecyclePoints.Reinforcements.Id))
		{
			return Array.Empty<XWModReinforcement>();
		}
		return Modify(XWModLifecyclePoints.Reinforcements, new XWModWaveReinforcements(wave, Array.Empty<XWModReinforcement>())).Entries.Where((XWModReinforcement x) => x.Lane <= lanes && laneEnabled(x.Lane) && GodotObject.IsInstanceValid(ResourceManager.Instance) && GodotObject.IsInstanceValid(TowerDefenseManager.GetPacketConfigReadOnly(x.ContentKey))).ToArray();
	}
}
