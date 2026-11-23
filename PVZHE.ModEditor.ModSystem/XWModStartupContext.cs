using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModStartupContext
{
	private readonly Dictionary<XWLoadingKind, (string Owner, PackedScene Scene)> _views = new Dictionary<XWLoadingKind, (string, PackedScene)>();

	private readonly List<string> _diagnostics = new List<string>();

	private readonly SemaphoreSlim _presentationGate = new SemaphoreSlim(1, 1);

	private bool _presentationPrepared;

	public static XWModStartupContext Current { get; private set; }

	internal XWModManager Manager { get; }

	internal XWModSelectionState Selection { get; }

	internal IReadOnlyList<string> ActualIds { get; }

	internal bool JournalActive { get; }

	internal Dictionary<string, ModLoader.LoadedMod> PreparedPackages { get; } = new Dictionary<string, ModLoader.LoadedMod>(StringComparer.OrdinalIgnoreCase);

	public XWModLoadPlan Plan { get; }

	public bool Applied { get; private set; }

	public bool Revoked { get; private set; }

	public IReadOnlyList<string> PresentationDiagnostics => _diagnostics.AsReadOnly();

	public event Action PresentationChanged;

	internal XWModStartupContext(XWModManager manager, XWModSelectionState selection, IReadOnlyList<string> actualIds, XWModLoadPlan plan, bool journalActive, bool formal)
	{
		Manager = manager;
		Selection = selection.Clone();
		ActualIds = actualIds.ToArray();
		Plan = plan;
		JournalActive = journalActive;
		if (formal)
		{
			Current = this;
		}
	}

	public async Task PreparePresentationAsync(Node host)
	{
		await _presentationGate.WaitAsync();
		try
		{
			if (!Revoked && !_presentationPrepared && GodotObject.IsInstanceValid(host) && host.IsInsideTree())
			{
				await PreparePresentationCoreAsync(host);
			}
		}
		finally
		{
			_presentationGate.Release();
		}
	}

	private async Task PreparePresentationCoreAsync(Node host)
	{
		XWLoadingKind[] array = new XWLoadingKind[2]
		{
			XWLoadingKind.Startup,
			XWLoadingKind.Transition
		};
		foreach (XWLoadingKind kind in array)
		{
			foreach (XWModManager.ModEntry entry in Plan.LoadOrder.Reverse())
			{
				XWModLoadingPresentation loadingPresentation = entry.Manifest.LoadingPresentation;
				string path = ((kind != XWLoadingKind.Startup) ? loadingPresentation?.TransitionScene : loadingPresentation?.StartupScene);
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}
				SceneTree tree = host.GetTree();
				await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
				if (Revoked || !GodotObject.IsInstanceValid(host) || !host.IsInsideTree())
				{
					return;
				}
				try
				{
					Manager.RecordStartupPhase(this, entry.Id, "loading-presentation");
					if (!PreparedPackages.TryGetValue(entry.Id, out var value))
					{
						value = ModLoader.LoadMod(entry.PackagePath) ?? throw new InvalidOperationException("界面包提取失败。");
						if (!string.Equals(value.PackageSha256, entry.PackageSha256, StringComparison.OrdinalIgnoreCase) || !Plan.MatchesManifest(entry.Id, value.Manifest))
						{
							throw new InvalidOperationException("界面包与冻结的加载计划不一致，请重启。");
						}
						PreparedPackages.Add(entry.Id, value);
					}
					_views[kind] = (entry.Id, ModLoader.PrepareLoadingScene(value, path, kind));
					Diagnose($"{kind} 界面采用 {entry.Id} 的声明（按加载顺序取最后一个有效声明）。", warning: false);
				}
				catch (Exception ex)
				{
					Diagnose($"{kind} 界面（{entry.Id}）声明无效，继续查找：{ex.GetBaseException().Message}");
					continue;
				}
				break;
			}
		}
		Manager.RecordStartupPhase(this, "", "host-resources");
		_presentationPrepared = true;
	}

	public bool TryCreateView(XWLoadingKind kind, out XWModLoadingView view, out string owner)
	{
		view = null;
		owner = "";
		if (Revoked)
		{
			return false;
		}
		if (!_views.TryGetValue(kind, out var provider))
		{
			return false;
		}
		if (Applied && !ModLoader.GetLoadedMods().Any((ModLoader.LoadedMod mod) => mod.Applied && string.Equals(mod.LoadedId, provider.Owner, StringComparison.OrdinalIgnoreCase)))
		{
			DisableView(kind, "提供者已卸载。");
			return false;
		}
		Node node = null;
		try
		{
			node = provider.Scene.Instantiate(PackedScene.GenEditState.Disabled);
			if (!(node is XWModLoadingView xWModLoadingView))
			{
				throw new InvalidOperationException("场景根必须使用宿主 XWModLoadingView 脚本。");
			}
			if (!xWModLoadingView.TryBind(kind, out var error))
			{
				throw new InvalidOperationException(error);
			}
			xWModLoadingView.ProcessMode = Node.ProcessModeEnum.Always;
			view = xWModLoadingView;
			owner = provider.Owner;
			return true;
		}
		catch (Exception ex)
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.Free();
			}
			DisableView(kind, ex.GetBaseException().Message);
			return false;
		}
	}

	public string GetProvider(XWLoadingKind kind)
	{
		if (!_views.TryGetValue(kind, out (string, PackedScene) value))
		{
			return "";
		}
		return value.Item1;
	}

	internal static void InvalidateForManager(XWModManager manager)
	{
		XWModStartupContext current = Current;
		if (current != null && string.Equals(current.Manager.ModsDirectory, manager.ModsDirectory, StringComparison.OrdinalIgnoreCase))
		{
			Current = null;
			current.Revoke();
		}
	}

	internal void Revoke()
	{
		Revoked = true;
		_views.Clear();
		PreparedPackages.Clear();
		this.PresentationChanged?.Invoke();
	}

	public void DisableView(XWLoadingKind kind, string diagnostic)
	{
		string provider = GetProvider(kind);
		_views.Remove(kind);
		Diagnose($"{kind} 界面{((provider.Length > 0) ? ("（" + provider + "）") : "")}不可用，使用宿主默认界面：{diagnostic}");
		this.PresentationChanged?.Invoke();
	}

	public XWModStartupReport Apply()
	{
		if (Revoked)
		{
			throw new InvalidOperationException("启动计划已撤销，需要重新启动游戏。");
		}
		if (Applied)
		{
			throw new InvalidOperationException("启动计划已经应用，重新加载需要重启游戏。");
		}
		Applied = true;
		XWModStartupReport xWModStartupReport;
		XWLoadingKind[] array;
		try
		{
			xWModStartupReport = Manager.ApplyPreparedStartup(this);
		}
		catch
		{
			array = _views.Keys.ToArray();
			foreach (XWLoadingKind kind in array)
			{
				DisableView(kind, "正式 Mod 加载未完成。");
			}
			throw;
		}
		array = _views.Keys.ToArray();
		foreach (XWLoadingKind xWLoadingKind in array)
		{
			if (!xWModStartupReport.LoadedIds.Contains<string>(_views[xWLoadingKind].Owner, StringComparer.OrdinalIgnoreCase))
			{
				DisableView(xWLoadingKind, "提供者未完成正式 Mod 加载或已回滚。");
			}
		}
		return xWModStartupReport;
	}

	private void Diagnose(string message, bool warning = true)
	{
		_diagnostics.Add(message);
		if (warning)
		{
			GD.PushWarning("[ModLoading] " + message);
		}
		else
		{
			GD.Print("[ModLoading] " + message);
		}
	}
}
