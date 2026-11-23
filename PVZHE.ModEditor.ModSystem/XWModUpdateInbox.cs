using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModUpdateInbox
{
	private readonly ConcurrentQueue<XWModUpdateSubmission> _submissions = new ConcurrentQueue<XWModUpdateSubmission>();

	private readonly ConcurrentQueue<XWModUpdateService> _removals = new ConcurrentQueue<XWModUpdateService>();

	private readonly HashSet<XWModUpdateService> _services = new HashSet<XWModUpdateService>();

	private readonly Dictionary<string, (XWModUpdateService Service, XWHostUpdateNotice Notice)> _mods = new Dictionary<string, (XWModUpdateService, XWHostUpdateNotice)>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, (XWOfficialModDescriptor Owner, XWHostUpdateNotice Notice)> _officialMods = new Dictionary<string, (XWOfficialModDescriptor, XWHostUpdateNotice)>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, XWModUpdateService> _selfChecks = new Dictionary<string, XWModUpdateService>(StringComparer.OrdinalIgnoreCase);

	private readonly Func<IReadOnlyList<ModLoader.LoadedMod>> _loadedMods;

	private XWHostUpdateNotice _host;

	internal static XWModUpdateInbox Shared { get; } = new XWModUpdateInbox();

	internal XWModUpdateStartupPolicy Startup { get; }

	internal int Count => _mods.Count + _officialMods.Count + ((!(_host == null)) ? 1 : 0);

	internal event Action Changed;

	internal XWModUpdateInbox(Func<long> clock = null, Func<IReadOnlyList<ModLoader.LoadedMod>> loadedMods = null)
	{
		Startup = new XWModUpdateStartupPolicy(clock);
		_loadedMods = loadedMods ?? new Func<IReadOnlyList<ModLoader.LoadedMod>>(ModLoader.GetLoadedMods);
	}

	internal void Register(XWModUpdateService service)
	{
		_services.Add(service);
	}

	internal void Enqueue(XWModUpdateSubmission submission)
	{
		_submissions.Enqueue(submission);
	}

	internal void EnqueueRemoval(XWModUpdateService service)
	{
		_removals.Enqueue(service);
	}

	internal void Pump()
	{
		if (_selfChecks.Count != 0)
		{
			string[] array = (from pair in _selfChecks
				where !pair.Value.OwnerIsActive
				select pair.Key).ToArray();
			foreach (string key in array)
			{
				_selfChecks.Remove(key);
			}
		}
		if (_officialMods.Count != 0)
		{
			IReadOnlyList<ModLoader.LoadedMod> loaded = _loadedMods();
			string[] array = (from pair in _officialMods
				where !pair.Value.Owner.IsCurrent(loaded)
				select pair.Key).ToArray();
			foreach (string key2 in array)
			{
				Startup.Advance(Count);
				_officialMods.Remove(key2);
				NotifyChanged();
			}
		}
		XWModUpdateService result;
		while (_removals.TryDequeue(out result))
		{
			_services.Remove(result);
			if (_mods.TryGetValue(result.OwnerId, out (XWModUpdateService, XWHostUpdateNotice) value) && value.Item1 == result)
			{
				Startup.Advance(Count);
				_mods.Remove(result.OwnerId);
				NotifyChanged();
			}
		}
		int num2 = Math.Min(_submissions.Count, 256);
		for (int num3 = 0; num3 < num2; num3++)
		{
			if (!_submissions.TryDequeue(out var result2))
			{
				break;
			}
			result2.Service.Accept(result2);
		}
		Startup.Advance(Count);
	}

	internal bool Store(XWModUpdateService service, XWModUpdateNotice notice)
	{
		Startup.Advance(Count);
		_selfChecks[service.OwnerId] = service;
		bool flag = _officialMods.Remove(service.OwnerId);
		if (notice == null)
		{
			return _mods.Remove(service.OwnerId) || flag;
		}
		XWHostUpdateNotice xWHostUpdateNotice = new XWHostUpdateNotice(service.OwnerId, service.DisplayName, service.CurrentVersion, notice.LatestVersion, notice.Notes, notice.UpdateUrl, IsHost: false, service.LoadOrder);
		if (_mods.TryGetValue(service.OwnerId, out (XWModUpdateService, XWHostUpdateNotice) value) && value.Item1 == service && value.Item2 == xWHostUpdateNotice)
		{
			return flag;
		}
		_mods[service.OwnerId] = (service, xWHostUpdateNotice);
		return true;
	}

	internal void ReportOfficial(XWOfficialModDescriptor owner, XWModUpdateNotice notice)
	{
		if (!owner.IsCurrent(_loadedMods()) || (_selfChecks.TryGetValue(owner.ModId, out var value) && value.OwnerIsActive))
		{
			return;
		}
		Startup.Advance(Count);
		if (notice == null)
		{
			if (_officialMods.Remove(owner.ModId))
			{
				NotifyChanged();
			}
			return;
		}
		XWHostUpdateNotice xWHostUpdateNotice = new XWHostUpdateNotice(owner.ModId, owner.DisplayName, owner.CurrentVersion, notice.LatestVersion, notice.Notes, notice.UpdateUrl, IsHost: false, owner.LoadOrder);
		if (!_officialMods.TryGetValue(owner.ModId, out (XWOfficialModDescriptor, XWHostUpdateNotice) value2) || !(value2.Item1 == owner) || !(value2.Item2 == xWHostUpdateNotice))
		{
			_officialMods[owner.ModId] = (owner, xWHostUpdateNotice);
			NotifyChanged();
		}
	}

	internal void ReportHost(string currentVersion, string latestVersion, string notes, string url, bool available)
	{
		Startup.Advance(Count);
		XWHostUpdateNotice xWHostUpdateNotice = (available ? new XWHostUpdateNotice("", "游戏本体", currentVersion, latestVersion, notes ?? "", IsWebUrl(url) ? url : null, IsHost: true, -1) : null);
		if (!(_host == xWHostUpdateNotice))
		{
			_host = xWHostUpdateNotice;
			NotifyChanged();
		}
	}

	internal IReadOnlyList<XWHostUpdateNotice> Snapshot()
	{
		List<XWHostUpdateNotice> list = (from row in _mods.Values.Select(((XWModUpdateService Service, XWHostUpdateNotice Notice) row) => row.Notice).Concat(_officialMods.Values.Select(((XWOfficialModDescriptor Owner, XWHostUpdateNotice Notice) row) => row.Notice))
			orderby row.LoadOrder
			select row).ThenBy<XWHostUpdateNotice, string>((XWHostUpdateNotice row) => row.ModId, StringComparer.OrdinalIgnoreCase).ToList();
		if (_host != null)
		{
			list.Insert(0, _host);
		}
		return list.AsReadOnly();
	}

	internal void NotifyChanged()
	{
		if (this.Changed == null)
		{
			return;
		}
		Delegate[] invocationList = this.Changed.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action action = (Action)invocationList[i];
			try
			{
				action();
			}
			catch (Exception ex)
			{
				GD.PushWarning("更新列表刷新失败：" + ex.GetBaseException().Message);
			}
		}
	}

	internal void Shutdown()
	{
		XWModUpdateService[] array = _services.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
		Pump();
		_selfChecks.Clear();
		bool num = _officialMods.Count > 0;
		_officialMods.Clear();
		if (num)
		{
			NotifyChanged();
		}
	}

	internal static bool IsWebUrl(string url)
	{
		if (Uri.TryCreate(url, UriKind.Absolute, out Uri result))
		{
			if (!(result.Scheme == Uri.UriSchemeHttp))
			{
				return result.Scheme == Uri.UriSchemeHttps;
			}
			return true;
		}
		return false;
	}

	internal static bool TryNormalize(XWModUpdateNotice notice, out XWModUpdateNotice normalized, out string diagnostic)
	{
		normalized = null;
		diagnostic = "";
		if (notice == null || string.IsNullOrWhiteSpace(notice.LatestVersion))
		{
			diagnostic = "更新通知必须提供最新版本。";
		}
		else if (!string.IsNullOrWhiteSpace(notice.UpdateUrl) && !IsWebUrl(notice.UpdateUrl))
		{
			diagnostic = "更新页面必须是 HTTP 或 HTTPS 链接。";
		}
		if (diagnostic.Length != 0)
		{
			return false;
		}
		normalized = notice with
		{
			LatestVersion = notice.LatestVersion.Trim(),
			Notes = (notice.Notes ?? ""),
			UpdateUrl = (string.IsNullOrWhiteSpace(notice.UpdateUrl) ? null : notice.UpdateUrl)
		};
		return true;
	}
}
