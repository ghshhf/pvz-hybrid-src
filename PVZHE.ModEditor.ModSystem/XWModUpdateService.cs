using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModUpdateService : IDisposable
{
	private readonly object _gate = new object();

	private readonly HashSet<XWModUpdateSubmission> _pending = new HashSet<XWModUpdateSubmission>();

	private readonly XWModRuntimeSession _session;

	private readonly XWModUpdateInbox _inbox;

	private bool _ended;

	internal string OwnerId { get; }

	internal string DisplayName { get; }

	internal string CurrentVersion { get; }

	internal int LoadOrder { get; }

	internal bool OwnerIsActive
	{
		get
		{
			if (_session == XWModRuntimeSession.Current)
			{
				return _session.IsOwnerActive(OwnerId);
			}
			return false;
		}
	}

	internal XWModUpdateService(XWModRuntimeSession session, string owner, string name, string version, int loadOrder, XWModUpdateInbox inbox)
	{
		XWModExtensions.EnsureHostThread();
		if (session == null || session != XWModRuntimeSession.Current || !session.IsOwnerActive(owner))
		{
			throw new InvalidOperationException("更新通知仅在正式加载的 Mod 运行会话中可用。");
		}
		_session = session;
		_inbox = inbox;
		OwnerId = owner;
		DisplayName = (string.IsNullOrWhiteSpace(name) ? owner : name);
		CurrentVersion = version;
		LoadOrder = loadOrder;
		_inbox.Register(this);
		XWModExtensions.OwnerEnding += OwnerEnding;
	}

	public Task<XWModCommandResult<bool>> ReportAvailableAsync(XWModUpdateNotice notice)
	{
		lock (_gate)
		{
			if (_ended)
			{
				return Task.FromResult(Cancelled());
			}
			if (!XWModUpdateInbox.TryNormalize(notice, out var normalized, out var diagnostic))
			{
				return Task.FromResult(new XWModCommandResult<bool>(XWModCommandStatus.Rejected, Value: false, diagnostic));
			}
			XWModUpdateSubmission xWModUpdateSubmission = new XWModUpdateSubmission(this, normalized);
			_pending.Add(xWModUpdateSubmission);
			_inbox.Enqueue(xWModUpdateSubmission);
			return xWModUpdateSubmission.Completion.Task;
		}
	}

	public Task<XWModCommandResult<bool>> ReportUpToDateAsync()
	{
		lock (_gate)
		{
			if (_ended)
			{
				return Task.FromResult(Cancelled());
			}
			XWModUpdateSubmission xWModUpdateSubmission = new XWModUpdateSubmission(this, null);
			_pending.Add(xWModUpdateSubmission);
			_inbox.Enqueue(xWModUpdateSubmission);
			return xWModUpdateSubmission.Completion.Task;
		}
	}

	internal void Accept(XWModUpdateSubmission submission)
	{
		bool flag = false;
		lock (_gate)
		{
			if (!_pending.Remove(submission))
			{
				return;
			}
			if (_ended || _session != XWModRuntimeSession.Current || !_session.IsOwnerActive(OwnerId))
			{
				submission.Completion.TrySetResult(Cancelled());
			}
			else
			{
				flag = _inbox.Store(this, submission.Notice);
				submission.Completion.TrySetResult(new XWModCommandResult<bool>(XWModCommandStatus.Completed, Value: true, ""));
			}
		}
		if (flag)
		{
			_inbox.NotifyChanged();
		}
	}

	private static XWModCommandResult<bool> Cancelled()
	{
		return new XWModCommandResult<bool>(XWModCommandStatus.Cancelled, Value: false, "Mod 运行会话已结束，更新通知未接收。");
	}

	private void OwnerEnding(string owner)
	{
		if (string.Equals(owner, OwnerId, StringComparison.OrdinalIgnoreCase))
		{
			Dispose();
		}
	}

	public void Dispose()
	{
		lock (_gate)
		{
			if (_ended)
			{
				return;
			}
			_ended = true;
			foreach (XWModUpdateSubmission item in _pending)
			{
				item.Completion.TrySetResult(Cancelled());
			}
			_pending.Clear();
		}
		XWModExtensions.OwnerEnding -= OwnerEnding;
		_inbox.EnqueueRemoval(this);
	}
}
