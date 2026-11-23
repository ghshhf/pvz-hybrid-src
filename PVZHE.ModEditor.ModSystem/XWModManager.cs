using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using PVZHE.ModEditor.ModSystem.Validation;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModManager
{
	public sealed class ModEntry
	{
		public string Id { get; set; } = "";

		public string DisplayName { get; set; } = "";

		public string Version { get; set; } = "";

		public string PackagePath { get; set; } = "";

		public string DirectoryPath
		{
			get
			{
				return PackagePath;
			}
			set
			{
				PackagePath = value ?? "";
			}
		}

		public bool Enabled { get; set; }

		public bool HasManifest { get; set; }

		public XWModManifest Manifest { get; set; }

		public string Diagnostic { get; set; } = "";

		public bool Loaded { get; set; }

		public bool PendingDelete { get; set; }

		public string PackageSha256 { get; set; } = "";

		public string EffectiveState { get; set; } = "未应用";

		public string InstalledVersion { get; set; } = "";

		public string RunningVersion { get; set; } = "";

		public string RunningSha256 { get; set; } = "";

		public string EffectiveMode { get; set; } = "";

		public string LastApplyFailure { get; set; } = "";
	}

	public sealed class DeleteResult
	{
		public bool Success { get; set; }

		public bool PendingRestart { get; set; }

		public bool Blocked { get; set; }

		public string Message { get; set; } = "";

		public List<XWValidationIssue> Issues { get; set; } = new List<XWValidationIssue>();
	}

	public sealed class ApplyFailureRecord
	{
		public string ModId { get; internal set; } = "";

		public string Message { get; internal set; } = "";

		public string Version { get; internal set; } = "";

		public string PackageSha256 { get; internal set; } = "";
	}

	public sealed class SetEnabledResult
	{
		public bool Success { get; set; }

		public bool Changed { get; set; }

		public bool Blocked { get; set; }

		public List<XWValidationIssue> Issues { get; set; } = new List<XWValidationIssue>();
	}

	public sealed class ResolveEnabledResult
	{
		public XWModLoadPlan Plan { get; set; }

		public List<ModEntry> LoadOrder { get; set; } = new List<ModEntry>();

		public List<XWValidationIssue> Issues { get; set; } = new List<XWValidationIssue>();
	}

	internal sealed record SelectionChange(string Id, string Name, bool Enabled);

	internal sealed class SelectionPlan
	{
		internal string ModId { get; set; }

		internal bool Enabled { get; set; }

		internal IReadOnlyList<string> PreviousIds { get; set; } = Array.Empty<string>();

		internal IReadOnlyList<string> DesiredIds { get; set; } = Array.Empty<string>();

		internal IReadOnlyList<SelectionChange> Changes { get; set; } = Array.Empty<SelectionChange>();

		internal IReadOnlyList<XWValidationIssue> Issues { get; set; } = Array.Empty<XWValidationIssue>();

		internal string Evidence { get; set; } = "";

		internal bool CanApply { get; set; }

		internal bool HasRelatedChanges => Changes.Any((SelectionChange change) => !change.Id.Equals(ModId, StringComparison.OrdinalIgnoreCase));
	}

	private XWModBootRecovery _bootRecovery;

	private static XWModManager _startupManager;

	private bool _safeMode;

	private string _recoveryDiagnostic = "";

	private bool _bootAwaitingMenu;

	private static bool _applyingEnabledPlan;

	private XWModSelectionState _appliedSelection;

	private XWModBootPackage[] _appliedPackages;

	private bool _startupStoragePrepared;

	private XWModStartupContext _preparedStartup;

	public const string ModsDirectoryName = "Mods";

	public const string EnabledStateFileName = "enabled_mods.json";

	private readonly Dictionary<string, XWModManifest> _loadedManifests = new Dictionary<string, XWModManifest>(StringComparer.OrdinalIgnoreCase);

	private static readonly object ApplyFailureGate = new object();

	private static readonly Dictionary<string, Dictionary<string, string>> RecentApplyFailures = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

	private readonly string _modsDirectory;

	private XWModSelectionStore SelectionStore => new XWModSelectionStore(EnabledStatePath);

	private XWModBootRecovery BootRecovery => _bootRecovery ?? (_bootRecovery = new XWModBootRecovery(_modsDirectory));

	private XWModManager StartupForDirectory
	{
		get
		{
			if (_startupManager == null || !string.Equals(_startupManager.FailureDomain, FailureDomain, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			return _startupManager;
		}
	}

	public bool IsSafeMode
	{
		get
		{
			XWModBootJournal journal;
			string error;
			return StartupForDirectory?._safeMode ?? BootRecovery.Inspect(out journal, out error);
		}
	}

	public string RecoveryDiagnostic
	{
		get
		{
			XWModManager startupForDirectory = StartupForDirectory;
			if (startupForDirectory != null)
			{
				return startupForDirectory._recoveryDiagnostic;
			}
			XWModBootJournal journal;
			string error;
			bool flag = BootRecovery.Inspect(out journal, out error);
			if (error.Length <= 0)
			{
				if (!flag)
				{
					return "";
				}
				return $"上次 Mod 启动已记录需要恢复的故障（阶段 {journal.Phase}，最后执行项 {journal.Owner}，仅供排查）。";
			}
			return error;
		}
	}

	public string ModsDirectory => _modsDirectory;

	public string EnabledStatePath => Path.Combine(_modsDirectory, "enabled_mods.json");

	public IReadOnlyDictionary<string, XWModManifest> LoadedManifests => _loadedManifests;

	private string FailureDomain => Path.GetFullPath(_modsDirectory);

	public bool TryGetSelection(out XWModSelectionState state, out string diagnostic)
	{
		return SelectionStore.TryRead(out state, out diagnostic);
	}

	public XWModLoadPlan PreviewLoadPlan(IEnumerable<string> preferredOrder = null)
	{
		if (!TryGetSelection(out var state, out var diagnostic))
		{
			throw new InvalidDataException(diagnostic);
		}
		if (!XWModInstallTransaction.TryLoadPendingDeletes(_modsDirectory, out var result, out diagnostic))
		{
			throw new InvalidDataException(diagnostic);
		}
		List<ModEntry> list = ScanMods();
		foreach (ModEntry entry in list)
		{
			entry.PendingDelete |= result.Any((XWModInstallTransaction.PendingDelete item) => string.Equals(item.ModId, entry.Id, StringComparison.OrdinalIgnoreCase));
		}
		return XWModLoadPlan.Build(list, state.EnabledIds, preferredOrder ?? state.PreferredOrder, null, XWModRuntimeSession.AvailableCapabilities);
	}

	public bool TrySetPreferredOrder(IEnumerable<string> order, out string diagnostic)
	{
		using (XWModInstallTransaction.Enter())
		{
			if (!TryGetSelection(out var state, out diagnostic))
			{
				return false;
			}
			state.PreferredOrder = (order ?? Array.Empty<string>()).ToList();
			return SelectionStore.TryWrite(state, out diagnostic);
		}
	}

	public bool TryRequestStartupRetry(out string diagnostic)
	{
		using (XWModInstallTransaction.Enter())
		{
			if (!TryGetSelection(out var _, out diagnostic))
			{
				return false;
			}
			if (!(StartupForDirectory?.BootRecovery ?? BootRecovery).RequestRetry(out diagnostic))
			{
				return false;
			}
			XWModManager startupForDirectory = StartupForDirectory;
			if (startupForDirectory != null)
			{
				startupForDirectory._recoveryDiagnostic = "已安排重试；当前仍为安全模式，请完整重启游戏。";
			}
			return true;
		}
	}

	public bool TryRestoreLastGood(out string diagnostic)
	{
		using (XWModInstallTransaction.Enter())
		{
			XWModBootRecovery xWModBootRecovery = StartupForDirectory?.BootRecovery ?? BootRecovery;
			ModEntry[] source = (from entry in ScanMods(includeHashes: true)
				where entry.HasManifest && !entry.PendingDelete
				select entry).ToArray();
			if ((from entry in source
				group entry by XWModLoadPlan.CanonicalId(entry.Id)).Any((IGrouping<string, ModEntry> group) => group.Count() > 1))
			{
				diagnostic = "存在重复 Mod ID，不能恢复成功组合。";
				return false;
			}
			IEnumerable<XWModBootPackage> availablePackages = source.Select((ModEntry entry) => new XWModBootPackage
			{
				Id = entry.Id,
				Version = entry.Manifest.Version,
				Sha256 = entry.PackageSha256
			});
			if (!xWModBootRecovery.TryRestoreLastGood(availablePackages, out var selection, out diagnostic))
			{
				return false;
			}
			if (!SelectionStore.TryRestoreVerified(selection, out diagnostic))
			{
				return false;
			}
			if (!xWModBootRecovery.RequestRetry(out diagnostic))
			{
				return false;
			}
			XWModManager startupForDirectory = StartupForDirectory;
			if (startupForDirectory != null)
			{
				startupForDirectory._recoveryDiagnostic = "已恢复上次成功组合，请完整重启游戏。";
			}
			return true;
		}
	}

	public bool TryResetSelectionForRecovery(out string diagnostic)
	{
		using (XWModInstallTransaction.Enter())
		{
			XWModBootRecovery xWModBootRecovery = StartupForDirectory?.BootRecovery ?? BootRecovery;
			if (!SelectionStore.TryRestoreVerified(new XWModSelectionState(), out diagnostic))
			{
				return false;
			}
			if (!xWModBootRecovery.RequestRetry(out diagnostic))
			{
				return false;
			}
			XWModManager startupForDirectory = StartupForDirectory;
			if (startupForDirectory != null)
			{
				startupForDirectory._recoveryDiagnostic = "已清空待启用列表，损坏配置原文已备份；请完整重启游戏。Mod 安装包、配置、进度和成功组合均已保留。";
			}
			return true;
		}
	}

	public static async Task NotifyMainMenuReadyAsync()
	{
		XWModManager manager = _startupManager;
		if (manager == null || !manager._bootAwaitingMenu)
		{
			return;
		}
		manager._bootAwaitingMenu = false;
		try
		{
			int num;
			if ((await XWModEnvironmentService.EnsureReadyAsync()).Ready)
			{
				XWModRuntimeSession current = XWModRuntimeSession.Current;
				num = ((current != null && current.Frozen && !current.HasFailures) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			if (!((num != 0) ? manager.BootRecovery.FinishReady(manager._appliedSelection, manager._appliedPackages, fullApplySucceeded: true, mainMenuReady: true, out var error) : manager.BootRecovery.FinishDegraded(XWModRuntimeSession.Current?.HasFailures ?? false, out error)))
			{
				manager._recoveryDiagnostic = error;
				GD.PushWarning("[ModManager] " + error);
			}
		}
		catch (Exception ex)
		{
			manager._recoveryDiagnostic = ex.GetBaseException().Message;
			manager.BootRecovery.FinishDegraded(rollbackBlocked: true, out var _);
			GD.PushWarning("[ModManager] 启动完成记录失败：" + manager._recoveryDiagnostic);
		}
	}

	private void CancelPreparedStartup()
	{
		XWModStartupContext preparedStartup = _preparedStartup;
		if (preparedStartup != null && !preparedStartup.Applied && preparedStartup.JournalActive && !BootRecovery.FinishDegraded(rollbackBlocked: false, out var error))
		{
			_recoveryDiagnostic = error;
			GD.PushWarning("[ModManager] " + error);
		}
		_preparedStartup?.Revoke();
		_preparedStartup = null;
	}

	public void PrepareStartupStorage()
	{
		if (_applyingEnabledPlan)
		{
			throw new InvalidOperationException("Mod 正式应用期间不能重新准备启动事务。");
		}
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			throw new InvalidOperationException("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
		}
		if (!_startupStoragePrepared)
		{
			if (ModLoader.GetLoadedMods().Count != 0)
			{
				throw new InvalidOperationException("当前已有运行中的 Mod，准备启动计划需要重启游戏。");
			}
			try
			{
				ModLoader.PrepareColdStartCache();
			}
			catch (Exception ex)
			{
				GD.PushWarning("[ModManager] 旧缓存清理未完成，保留缓存继续启动：" + ex.GetBaseException().Message);
			}
			_startupManager = this;
			_safeMode = BootRecovery.Inspect(out var journal, out var error);
			_recoveryDiagnostic = ((error.Length > 0) ? error : (_safeMode ? $"上次 Mod 启动已记录需要恢复的故障（阶段 {journal.Phase}，最后执行项 {journal.Owner}，仅供排查）。当前以零 Mod 安全模式运行。" : ""));
			if (!_safeMode && journal.InProgress)
			{
				GD.Print($"[ModManager] 上次启动未完成（阶段 {journal.Phase}，最后执行项 {journal.Owner}，不代表故障归因）；本次将按已保存的启用配置继续加载。");
			}
			if (!RecoverPendingStorage(out error))
			{
				_safeMode = true;
				_recoveryDiagnostic = "安装事务恢复受阻：" + error;
				BootRecovery.RequireRecovery("storage-recovery-blocked", out var _);
			}
			_startupStoragePrepared = true;
		}
	}

	public XWModStartupContext PrepareStartup()
	{
		if (_applyingEnabledPlan)
		{
			throw new InvalidOperationException("Mod 正式应用期间不能重新准备启动事务。");
		}
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			throw new InvalidOperationException("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
		}
		return PrepareStartupCore(recoverStorage: true) ?? throw new InvalidOperationException((_recoveryDiagnostic.Length > 0) ? _recoveryDiagnostic : "无法冻结 Mod 启动计划。");
	}

	internal void RecordStartupPhase(XWModStartupContext context, string owner, string phase)
	{
		if (context.Manager != this)
		{
			throw new InvalidOperationException("启动上下文不属于当前 Manager。");
		}
		if (context.JournalActive && !BootRecovery.RecordPhase(owner, phase, out var error))
		{
			throw new IOException(error);
		}
	}

	private XWModStartupContext PrepareStartupCore(bool recoverStorage)
	{
		if (recoverStorage && _preparedStartup != null)
		{
			return _preparedStartup;
		}
		if (ModLoader.GetLoadedMods().Count != 0)
		{
			GD.PushWarning("[ModManager] 当前已有运行中的 Mod，重新应用需要重启游戏。");
			return null;
		}
		_loadedManifests.Clear();
		if (recoverStorage)
		{
			PrepareStartupStorage();
		}
		if (!TryGetSelection(out var state, out var diagnostic))
		{
			if (!recoverStorage)
			{
				XWModEnvironmentService.Invalidate("InvalidEnabledState");
				return null;
			}
			_safeMode = true;
			_recoveryDiagnostic = diagnostic;
			BootRecovery.RequireRecovery("invalid-selection", out var _);
			state = new XWModSelectionState();
		}
		List<string> enabledIds = state.EnabledIds;
		IReadOnlyList<string> readOnlyList2;
		if (!_safeMode)
		{
			IReadOnlyList<string> readOnlyList = enabledIds;
			readOnlyList2 = readOnlyList;
		}
		else
		{
			IReadOnlyList<string> readOnlyList = Array.Empty<string>();
			readOnlyList2 = readOnlyList;
		}
		IReadOnlyList<string> readOnlyList3 = readOnlyList2;
		if (_safeMode)
		{
			GD.PushWarning("[ModManager] " + _recoveryDiagnostic);
		}
		XWModEnvironmentService.Invalidate("ApplyingEnabledSet");
		ResolveEnabledResult resolveEnabledResult = ResolveEnabledMods(readOnlyList3);
		if (_safeMode)
		{
			resolveEnabledResult = new ResolveEnabledResult
			{
				Plan = XWModLoadPlan.Build(Array.Empty<ModEntry>(), Array.Empty<string>(), null, null, XWModRuntimeSession.AvailableCapabilities)
			};
		}
		XWModLoadPlan plan = resolveEnabledResult.Plan;
		foreach (XWValidationIssue issue in resolveEnabledResult.Issues)
		{
			if (issue.Level == XWValidationIssue.Severity.Error && !string.IsNullOrWhiteSpace(issue.ResourceKey))
			{
				RecordApplyFailure(issue.ResourceKey, issue.Message);
			}
			GD.PushWarning("[ModManager] " + issue.Message);
		}
		if (plan == null || !plan.VerifyPackageHashes(out var _))
		{
			XWModEnvironmentService.Invalidate("LoadPlanChanged");
			return null;
		}
		IReadOnlyList<ModEntry> loadOrder = plan.LoadOrder;
		bool journalActive = false;
		if (recoverStorage && !_safeMode)
		{
			XWModSelectionState xWModSelectionState = state.Clone();
			xWModSelectionState.EnabledIds = loadOrder.Select((ModEntry entry) => entry.Id).ToList();
			if (!BootRecovery.Begin(plan.PlanHash, xWModSelectionState, PackagesFor(loadOrder), out var error2))
			{
				_recoveryDiagnostic = error2;
				XWModEnvironmentService.Invalidate("BootJournalBlocked");
				return null;
			}
			journalActive = true;
		}
		XWModStartupContext xWModStartupContext = new XWModStartupContext(this, state, readOnlyList3, plan, journalActive, recoverStorage);
		if (recoverStorage)
		{
			_preparedStartup = xWModStartupContext;
		}
		return xWModStartupContext;
	}

	private int ApplyEnabledPlan(bool recoverStorage, XWModStartupContext prepared = null)
	{
		if (_applyingEnabledPlan)
		{
			return -1;
		}
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return -1;
		}
		if (prepared == null)
		{
			prepared = PrepareStartupCore(recoverStorage);
		}
		if (prepared == null)
		{
			return -1;
		}
		_applyingEnabledPlan = true;
		try
		{
			return ApplyEnabledPlanCore(prepared);
		}
		finally
		{
			_applyingEnabledPlan = false;
		}
	}

	private int ApplyEnabledPlanCore(XWModStartupContext prepared)
	{
		if (prepared.Manager != this || ModLoader.GetLoadedMods().Count != 0)
		{
			GD.PushWarning("[ModManager] 当前环境不能应用此启动计划，需要重启游戏。");
			return -1;
		}
		XWModLoadPlan plan = prepared.Plan;
		XWModSelectionState selection = prepared.Selection;
		IReadOnlyList<string> actualIds = prepared.ActualIds;
		IReadOnlyList<ModEntry> order = plan.LoadOrder;
		bool journalActive = prepared.JournalActive;
		XWModSelectionState state;
		string diagnostic;
		bool flag = _safeMode || (TryGetSelection(out state, out diagnostic) && state.Normalize().EnabledIds.SequenceEqual(selection.Normalize().EnabledIds) && state.Normalize().PreferredOrder.SequenceEqual(selection.Normalize().PreferredOrder));
		if (!plan.VerifyPackageHashes(out var _) || !flag)
		{
			_recoveryDiagnostic = "Mod 包或启用配置在启动准备后发生变化，已拒绝本次应用，请重启游戏。";
			XWModEnvironmentService.Invalidate("LoadPlanChanged");
			if (journalActive)
			{
				BootRecovery.FinishDegraded(rollbackBlocked: false, out diagnostic);
			}
			GD.PushWarning("[ModManager] " + _recoveryDiagnostic);
			return -1;
		}
		XWModRuntimeSession session = XWModRuntimeSession.Begin(order);
		Dictionary<string, ModLoader.LoadedMod> loadedById = new Dictionary<string, ModLoader.LoadedMod>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> failed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		bool rollbackBlocked = false;
		try
		{
			foreach (ModEntry item in order)
			{
				if (item.Manifest.Dependencies.Any((XWModDependency dependency) => failed.Contains(dependency.Id)))
				{
					Fail(item.Id, "依赖 Mod 应用失败，当前包未加载。");
					continue;
				}
				Phase(item.Id, "initialize");
				ModLoader.LoadedMod value;
				ModLoader.LoadedMod loadedMod = (prepared.PreparedPackages.TryGetValue(item.Id, out value) ? value : ModLoader.LoadMod(item.PackagePath));
				if (loadedMod != null && (!string.Equals(loadedMod.PackageSha256, item.PackageSha256, StringComparison.OrdinalIgnoreCase) || !plan.MatchesManifest(item.Id, loadedMod.Manifest)))
				{
					Fail(item.Id, "安装包字节或清单与加载计划不一致，已拒绝执行；请重启重新生成计划。");
				}
				else if (loadedMod == null || !ModLoader.ApplyMod(loadedMod))
				{
					Fail(item.Id, (loadedMod == null) ? "Mod 包加载失败。" : string.Join("；", loadedMod.Diagnostics.DefaultIfEmpty("Mod 应用失败。")));
					if (loadedMod != null && ModLoader.IsTracked(loadedMod))
					{
						rollbackBlocked = true;
						loadedById[item.Id] = loadedMod;
						_loadedManifests[item.Id] = item.Manifest;
					}
					else
					{
						session.RemoveOwner(item.Id);
					}
				}
				else
				{
					loadedById[item.Id] = loadedMod;
					_loadedManifests[item.Id] = item.Manifest;
					ClearApplyFailure(item.Id);
				}
			}
			foreach (ModEntry item2 in order)
			{
				if (failed.Contains(item2.Id) || !loadedById.TryGetValue(item2.Id, out var value2))
				{
					continue;
				}
				Phase(item2.Id, "all-mods-loaded");
				if (value2.NotifyAllModsLoaded())
				{
					continue;
				}
				Fail(item2.Id, "Mod 运行入口第二阶段失败，已清理其依赖闭包。");
				bool flag2;
				do
				{
					flag2 = false;
					foreach (ModEntry item3 in order)
					{
						if (!failed.Contains(item3.Id) && item3.Manifest.Dependencies.Any((XWModDependency dependency) => failed.Contains(dependency.Id)))
						{
							Fail(item3.Id, "必需依赖初始化失败，已回滚。");
							flag2 = true;
						}
					}
				}
				while (flag2);
				Phase(item2.Id, "rollback-dependent-closure");
				Rollback(failed);
			}
			session.Freeze();
			rollbackBlocked |= session.HasFailures;
			string error;
			if (!rollbackBlocked && failed.Count == 0 && order.Count == actualIds.Count && !plan.HasErrors)
			{
				XWModEnvironmentService.CompleteFormalApply(this, actualIds, order.Select((ModEntry entry) => loadedById[entry.Id]).ToArray());
				XWModEnvironmentService.CaptureAsync();
				if (journalActive)
				{
					Phase("", "awaiting-main-menu");
					_appliedSelection = selection.Clone();
					_appliedPackages = PackagesFor(order);
					_bootAwaitingMenu = true;
				}
			}
			else if (journalActive && !BootRecovery.FinishDegraded(rollbackBlocked, out error))
			{
				_recoveryDiagnostic = error;
			}
			GD.Print($"[ModManager] enabled runtime apply complete: {_loadedManifests.Count}/{order.Count}; rollbackBlocked={rollbackBlocked}; safeMode={_safeMode}");
			return rollbackBlocked ? (-1) : _loadedManifests.Count;
		}
		catch (Exception ex)
		{
			string[] array = loadedById.Keys.ToArray();
			foreach (string id in array)
			{
				Fail(id, ex.GetBaseException().Message);
			}
			try
			{
				Rollback(loadedById.Keys.ToArray());
			}
			catch
			{
				rollbackBlocked = true;
			}
			session.Freeze();
			XWModEnvironmentService.Invalidate("FormalApplyFailed");
			if (journalActive)
			{
				BootRecovery.FinishDegraded(rollbackBlocked: true, out diagnostic);
			}
			_recoveryDiagnostic = ex.GetBaseException().Message;
			GD.PushWarning("[ModManager] " + _recoveryDiagnostic);
			return -1;
		}
		void Fail(string text, string message)
		{
			failed.Add(text);
			RecordApplyFailure(text, message);
		}
		void Phase(string owner, string phase)
		{
			if (journalActive && !BootRecovery.RecordPhase(owner, phase, out var error2))
			{
				throw new IOException(error2);
			}
		}
		void Rollback(IEnumerable<string> ids)
		{
			HashSet<string> hashSet = ids.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ModEntry entry in order.Reverse())
			{
				if (hashSet.Contains(entry.Id))
				{
					if (ModLoader.GetLoadedMods().Any((ModLoader.LoadedMod mod) => string.Equals(mod.LoadedId, entry.Id, StringComparison.OrdinalIgnoreCase)) && !ModLoader.RollbackMod(entry.Id, out var _))
					{
						rollbackBlocked = true;
						RecordApplyFailure(entry.Id, "Mod 回滚仍受运行时引用阻止，需要重启；保留尚未释放的运行对象。");
					}
					else
					{
						session.RemoveOwner(entry.Id);
						loadedById.Remove(entry.Id);
						_loadedManifests.Remove(entry.Id);
					}
				}
			}
		}
	}

	private static XWModBootPackage[] PackagesFor(IEnumerable<ModEntry> order)
	{
		return order.Select((ModEntry entry) => new XWModBootPackage
		{
			Id = entry.Id,
			Version = entry.Manifest.Version,
			Sha256 = entry.PackageSha256
		}).ToArray();
	}

	public XWModManager(string modsDirectory = "")
	{
		_modsDirectory = (string.IsNullOrWhiteSpace(modsDirectory) ? GetDefaultModsDirectory() : Path.GetFullPath(modsDirectory));
	}

	internal static ModEntry[] PreferValidCandidates(IEnumerable<ModEntry> entries)
	{
		ModEntry[] array = entries.ToArray();
		ModEntry[] array2 = array.Where((ModEntry entry) => entry.Manifest != null).ToArray();
		if (array2.Length == 0)
		{
			return array;
		}
		return array2;
	}

	public IReadOnlyList<ApplyFailureRecord> GetRecentApplyFailures()
	{
		lock (ApplyFailureGate)
		{
			if (!RecentApplyFailures.TryGetValue(FailureDomain, out var value))
			{
				return Array.Empty<ApplyFailureRecord>();
			}
			return value.Select((KeyValuePair<string, string> pair) => new ApplyFailureRecord
			{
				ModId = pair.Key,
				Message = pair.Value
			}).ToArray();
		}
	}

	private void RecordApplyFailure(string modId, string message)
	{
		if (string.IsNullOrWhiteSpace(modId))
		{
			return;
		}
		lock (ApplyFailureGate)
		{
			if (!RecentApplyFailures.TryGetValue(FailureDomain, out var value))
			{
				value = (RecentApplyFailures[FailureDomain] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
			}
			value[modId] = message ?? "Mod 应用失败。";
		}
	}

	private void ClearApplyFailure(string modId)
	{
		lock (ApplyFailureGate)
		{
			if (RecentApplyFailures.TryGetValue(FailureDomain, out var value))
			{
				value.Remove(modId);
			}
		}
	}

	public static string GetDefaultModsDirectory()
	{
		return Path.GetFullPath(ProjectSettings.GlobalizePath("user://Mods/"));
	}

	public static string GetGameDirectory()
	{
		if (Engine.IsEditorHint())
		{
			return ProjectSettings.GlobalizePath("res://");
		}
		string directoryName = Path.GetDirectoryName(OS.GetExecutablePath());
		if (!string.IsNullOrWhiteSpace(directoryName))
		{
			return directoryName;
		}
		return Directory.GetCurrentDirectory();
	}

	public void EnsureModsDirectory()
	{
		Directory.CreateDirectory(_modsDirectory);
	}

	public bool RecoverPendingStorage(out string diagnostic)
	{
		diagnostic = "";
		using (XWModInstallTransaction.Enter())
		{
			using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
			if (environmentMutation == null)
			{
				diagnostic = "Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。";
				return false;
			}
			if (ModLoader.GetLoadedMods().Any((ModLoader.LoadedMod mod) => string.Equals(Path.GetDirectoryName(mod.LoadedPackagePath), _modsDirectory, StringComparison.OrdinalIgnoreCase)))
			{
				diagnostic = "当前安装目录仍有运行中的 Mod，不能执行冷启动恢复。";
				return false;
			}
			EnsureModsDirectory();
			if (!XWModInstallTransaction.TryRecover(this, out diagnostic))
			{
				return false;
			}
			if (!XWModInstallTransaction.TryLoadPendingDeletes(_modsDirectory, out var result, out var diagnostic2))
			{
				diagnostic = diagnostic2;
				return false;
			}
			XWModInstallTransaction.PendingDelete[] array = result.ToArray();
			foreach (XWModInstallTransaction.PendingDelete pending in array)
			{
				if (ModLoader.GetLoadedMods().Any((ModLoader.LoadedMod mod) => string.Equals(mod.LoadedId, pending.ModId, StringComparison.OrdinalIgnoreCase)))
				{
					diagnostic = "待删除 Mod 仍在运行，延后处理：" + pending.ModId + "。";
					continue;
				}
				try
				{
					RestoreEnabledState(pending.ModId, enabled: false);
				}
				catch (Exception ex)
				{
					diagnostic = "无法更新待删除 Mod 启用状态：" + ex.GetBaseException().Message;
					continue;
				}
				if (!File.Exists(pending.PackagePath))
				{
					XWModInstallTransaction.RemovePendingDelete(_modsDirectory, pending.ModId);
					continue;
				}
				if (!XWModInstallTransaction.TryHash(pending.PackagePath, out var sha) || !string.Equals(sha, pending.PackageSha256, StringComparison.OrdinalIgnoreCase))
				{
					diagnostic = "待删除 Mod 文件已被替换，拒绝删除：" + pending.ModId + "。";
					continue;
				}
				if (!ModLoader.TryReadPackageMetadata(pending.PackagePath, out var manifest, out var _, out var _) || !string.Equals(manifest?.Id, pending.ModId, StringComparison.OrdinalIgnoreCase))
				{
					diagnostic = "待删除文件的 Mod ID 不匹配，保留安装包：" + pending.ModId + "。";
					continue;
				}
				try
				{
					File.Delete(pending.PackagePath);
					XWModInstallTransaction.RemovePendingDelete(_modsDirectory, pending.ModId);
				}
				catch (Exception ex2)
				{
					diagnostic = ex2.GetBaseException().Message;
				}
			}
			return string.IsNullOrWhiteSpace(diagnostic);
		}
	}

	internal void RestoreEnabledState(string modId, bool enabled)
	{
		if (!TryLoadEnabledIds(out var ids, out var diagnostic))
		{
			throw new InvalidDataException("启用列表损坏，拒绝覆盖：" + diagnostic);
		}
		List<string> list = ids.ToList();
		list.RemoveAll((string id) => string.Equals(id, modId, StringComparison.OrdinalIgnoreCase));
		if (enabled)
		{
			list.Add(modId);
		}
		SaveEnabledIds(list);
	}

	public List<ModEntry> ScanMods(bool includeHashes = false)
	{
		EnsureModsDirectory();
		HashSet<string> hashSet = new HashSet<string>(LoadEnabledIds(), StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = new HashSet<string>(from item in XWModInstallTransaction.LoadPendingDeletes(_modsDirectory)
			select item.ModId, StringComparer.OrdinalIgnoreCase);
		List<ModEntry> list = new List<ModEntry>();
		foreach (string packagePath in Directory.EnumerateFiles(_modsDirectory, "*.pmod", SearchOption.TopDirectoryOnly).OrderBy<string, string>((string path) => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
		{
			XWModManifest manifest;
			ModExporter.ModInfo info;
			string diagnostic;
			XWModPackageMetadataPreview preview;
			bool hasManifest = ModLoader.TryReadPackageMetadata(packagePath, out manifest, out info, out diagnostic, out preview);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(packagePath);
			string text = (hasManifest ? manifest.Id : preview?.Id);
			string id = (string.IsNullOrWhiteSpace(text) ? fileNameWithoutExtension : text.Trim().ToLowerInvariant());
			list.Add(new ModEntry
			{
				Id = id,
				DisplayName = (string.IsNullOrWhiteSpace(manifest?.Name ?? preview?.Name) ? id : (manifest?.Name ?? preview.Name)),
				Version = (manifest?.Version ?? ""),
				PackagePath = packagePath,
				Enabled = hashSet.Contains(id),
				HasManifest = hasManifest,
				Manifest = manifest,
				Diagnostic = diagnostic,
				PendingDelete = hashSet2.Contains(id),
				Loaded = ModLoader.GetLoadedMods().Any((ModLoader.LoadedMod mod) => string.Equals(mod.LoadedId, id, StringComparison.OrdinalIgnoreCase) && (hasManifest || string.Equals(mod.LoadedPackagePath, packagePath, StringComparison.OrdinalIgnoreCase)))
			});
		}
		foreach (ModEntry entry in list)
		{
			if (!entry.HasManifest && string.IsNullOrWhiteSpace(entry.PackagePath))
			{
				entry.EffectiveState = "缺少安装包";
				continue;
			}
			if (includeHashes && XWModInstallTransaction.TryHash(entry.PackagePath, out var sha))
			{
				entry.PackageSha256 = sha;
			}
			ModLoader.LoadedMod loadedMod = ModLoader.GetLoadedMods().FirstOrDefault((ModLoader.LoadedMod mod) => string.Equals(mod.LoadedId, entry.Id, StringComparison.OrdinalIgnoreCase) && SameDirectory(mod.LoadedPackagePath, _modsDirectory) && (entry.HasManifest || string.Equals(mod.LoadedPackagePath, entry.PackagePath, StringComparison.OrdinalIgnoreCase)));
			entry.Loaded = loadedMod?.Applied ?? false;
			if (entry.PendingDelete)
			{
				entry.EffectiveState = "待删除";
			}
			else if (loadedMod != null && loadedMod.State == ModLoader.ApplicationState.PendingRollback)
			{
				entry.EffectiveState = "回滚受阻";
			}
			else if (loadedMod != null && loadedMod.State == ModLoader.ApplicationState.Failed)
			{
				entry.EffectiveState = "加载失败";
			}
			else if (loadedMod != null && loadedMod.Applied)
			{
				entry.EffectiveState = ((!entry.Enabled) ? "待停用" : ((string.IsNullOrWhiteSpace(entry.PackageSha256) || string.Equals(entry.PackageSha256, loadedMod.PackageSha256, StringComparison.OrdinalIgnoreCase)) ? "已加载" : "已加载旧版本"));
			}
			else
			{
				entry.EffectiveState = (entry.Enabled ? "待重启" : "未启用");
			}
			entry.InstalledVersion = entry.Version;
			if (loadedMod != null)
			{
				entry.RunningVersion = loadedMod.LoadedVersion;
				entry.RunningSha256 = loadedMod.PackageSha256;
				ModEntry modEntry = entry;
				modEntry.EffectiveMode = loadedMod.EffectiveMode.ToString() switch
				{
					"ResourceOnly" => "仅资源", 
					"Managed" => "托管代码与资源", 
					"OptionalResourceFallback" => "可选代码失败，已回退为资源模式", 
					_ => loadedMod.EffectiveMode.ToString(), 
				};
			}
			lock (ApplyFailureGate)
			{
				if (RecentApplyFailures.TryGetValue(FailureDomain, out var value) && value.TryGetValue(entry.Id, out var value2))
				{
					entry.LastApplyFailure = value2;
				}
			}
			if (entry.Enabled && !string.IsNullOrWhiteSpace(entry.LastApplyFailure) && !entry.PendingDelete && entry.EffectiveState != "回滚受阻")
			{
				entry.EffectiveState = "加载失败";
			}
			if (IsSafeMode && entry.Enabled && loadedMod == null && !entry.PendingDelete)
			{
				entry.EffectiveState = "安全模式未加载";
			}
		}
		return list.OrderBy<ModEntry, string>((ModEntry modEntry2) => modEntry2.Id, StringComparer.OrdinalIgnoreCase).ThenBy<ModEntry, string>((ModEntry modEntry2) => modEntry2.PackagePath, StringComparer.OrdinalIgnoreCase).ToList();
	}

	public List<ModEntry> GetManagementSnapshot(bool includeHashes = true)
	{
		List<ModEntry> list = ScanMods(includeHashes);
		HashSet<string> present = list.Select((ModEntry entry) => entry.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in from id in LoadEnabledIds()
			where !present.Contains(id)
			select id)
		{
			list.Add(new ModEntry
			{
				Id = item,
				DisplayName = item,
				Enabled = true,
				HasManifest = false,
				Diagnostic = "启用列表中缺少安装包。",
				EffectiveState = "缺少安装包"
			});
		}
		return list.OrderBy<ModEntry, string>((ModEntry entry) => entry.Id, StringComparer.OrdinalIgnoreCase).ToList();
	}

	public IReadOnlyList<string> LoadEnabledIds()
	{
		if (TryLoadEnabledIds(out var ids, out var diagnostic))
		{
			return ids;
		}
		GD.PushWarning("[ModManager] enabled state ignored: " + diagnostic);
		return Array.Empty<string>();
	}

	public bool TryLoadEnabledIds(out IReadOnlyList<string> ids, out string diagnostic)
	{
		XWModSelectionState state;
		bool result = TryGetSelection(out state, out diagnostic);
		ids = state.EnabledIds.AsReadOnly();
		return result;
	}

	public void SaveEnabledIds(IEnumerable<string> modIds)
	{
		using (XWModInstallTransaction.Enter())
		{
			if (!TryGetSelection(out var state, out var diagnostic))
			{
				throw new InvalidDataException(diagnostic);
			}
			state.EnabledIds = (modIds ?? Array.Empty<string>()).ToList();
			if (!SelectionStore.TryWrite(state, out diagnostic))
			{
				throw new IOException(diagnostic);
			}
		}
	}

	public bool SetEnabled(string modId, bool enabled)
	{
		return TrySetEnabled(modId, enabled).Changed;
	}

	public SetEnabledResult TrySetEnabled(string modId, bool enabled)
	{
		using (XWModInstallTransaction.Enter())
		{
			if (File.Exists(XWModInstallTransaction.JournalPath(_modsDirectory)))
			{
				return new SetEnabledResult
				{
					Blocked = true,
					Issues = new List<XWValidationIssue> { SelectionIssue(modId, "上次安装尚未完成恢复，请先重启游戏。") }
				};
			}
			if (string.IsNullOrWhiteSpace(modId))
			{
				return new SetEnabledResult
				{
					Success = false,
					Blocked = true
				};
			}
			if (enabled)
			{
				if (!XWModInstallTransaction.TryLoadPendingDeletes(_modsDirectory, out var result, out var diagnostic))
				{
					return new SetEnabledResult
					{
						Success = false,
						Blocked = true,
						Issues = new List<XWValidationIssue>
						{
							new XWValidationIssue
							{
								Code = XWValidationIssue.IssueCode.RuntimeLoadFailed,
								Message = "待删除 Mod 记录损坏，不能启用：" + diagnostic,
								ResourceKey = modId,
								AffectedModIds = new List<string> { modId }
							}
						}
					};
				}
				if (result.Any((XWModInstallTransaction.PendingDelete item) => string.Equals(item.ModId, modId, StringComparison.OrdinalIgnoreCase)))
				{
					return new SetEnabledResult
					{
						Success = false,
						Blocked = true,
						Issues = new List<XWValidationIssue>
						{
							new XWValidationIssue
							{
								Code = XWValidationIssue.IssueCode.RuntimeLoadFailed,
								Message = "Mod 正在等待删除，不能重新启用：" + modId + "。",
								ResourceKey = modId,
								AffectedModIds = new List<string> { modId }
							}
						}
					};
				}
			}
			if (!TryLoadEnabledIds(out var ids, out var diagnostic2))
			{
				return new SetEnabledResult
				{
					Success = false,
					Blocked = true,
					Issues = new List<XWValidationIssue>
					{
						new XWValidationIssue
						{
							Code = XWValidationIssue.IssueCode.RuntimeLoadFailed,
							Message = "启用列表损坏，拒绝覆盖：" + diagnostic2,
							ResourceKey = modId,
							AffectedModIds = new List<string> { modId }
						}
					}
				};
			}
			List<string> list = ids.ToList();
			bool flag;
			if (enabled)
			{
				flag = !list.Contains<string>(modId, StringComparer.OrdinalIgnoreCase);
				if (flag)
				{
					list.Add(modId.Trim());
				}
			}
			else
			{
				flag = list.RemoveAll((string id) => string.Equals(id, modId, StringComparison.OrdinalIgnoreCase)) > 0;
			}
			List<string> list2 = list.Distinct<string>(StringComparer.OrdinalIgnoreCase).OrderBy<string, string>((string id) => id, StringComparer.OrdinalIgnoreCase).ToList();
			ResolveEnabledResult candidate = ResolveEnabledMods(list2);
			List<XWValidationIssue> issues = candidate.Issues;
			if (!enabled && flag && HasEnabledDependents(modId, list2))
			{
				return new SetEnabledResult
				{
					Success = false,
					Blocked = true,
					Issues = issues
				};
			}
			if (enabled && (!candidate.LoadOrder.Any((ModEntry entry) => string.Equals(entry.Id, modId, StringComparison.OrdinalIgnoreCase)) || ResolveEnabledMods(ids).LoadOrder.Any((ModEntry previous) => !candidate.LoadOrder.Any((ModEntry next) => string.Equals(previous.Id, next.Id, StringComparison.OrdinalIgnoreCase)))))
			{
				return new SetEnabledResult
				{
					Success = false,
					Blocked = true,
					Issues = issues
				};
			}
			if (flag)
			{
				SaveEnabledIds(list2);
			}
			return new SetEnabledResult
			{
				Success = true,
				Changed = flag,
				Issues = issues
			};
		}
	}

	public bool Enable(string modId)
	{
		return SetEnabled(modId, enabled: true);
	}

	public bool Disable(string modId)
	{
		return SetEnabled(modId, enabled: false);
	}

	public DeleteResult TryDeleteMod(string modId, string expectedSha256 = "")
	{
		using (XWModInstallTransaction.Enter())
		{
			using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
			if (environmentMutation == null)
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。"
				};
			}
			if (File.Exists(XWModInstallTransaction.JournalPath(_modsDirectory)))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "已有未完成的 Mod 事务，请先重启游戏完成恢复。"
				};
			}
			if (string.IsNullOrWhiteSpace(modId))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "未指定 Mod。"
				};
			}
			ModEntry[] array = (from item in ScanMods()
				where string.Equals(item.Id, modId, StringComparison.OrdinalIgnoreCase)
				select item).ToArray();
			string sha2;
			ModEntry[] array2 = (string.IsNullOrWhiteSpace(expectedSha256) ? PreferValidCandidates(array) : array.Where((ModEntry item) => XWModInstallTransaction.TryHash(item.PackagePath, out sha2) && string.Equals(sha2, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray());
			if (array2.Length > 1)
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "安装包身份存在歧义，拒绝删除。"
				};
			}
			ModEntry modEntry = array2.FirstOrDefault();
			if (modEntry == null)
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "未找到 Mod：" + modId + "。"
				};
			}
			if (modEntry.Manifest == null && array.Any((ModEntry item) => item.Manifest != null))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "此包身份未通过校验，且与合法安装包重名，已拒绝删除以保留合法包。"
				};
			}
			if (!XWModInstallTransaction.TryHash(modEntry.PackagePath, out var sha))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "无法读取 Mod 文件。"
				};
			}
			if (!string.IsNullOrWhiteSpace(expectedSha256) && !string.Equals(sha, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "Mod 文件已变化，拒绝删除。"
				};
			}
			if (!TryLoadEnabledIds(out var ids, out var diagnostic))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "启用列表损坏，拒绝删除：" + diagnostic
				};
			}
			if (!XWModInstallTransaction.TryLoadPendingDeletes(_modsDirectory, out var _, out var diagnostic2))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "待删除 Mod 记录损坏，拒绝删除：" + diagnostic2
				};
			}
			if (HasEnabledDependents(modId, ids))
			{
				return new DeleteResult
				{
					Blocked = true,
					Message = "删除会破坏其他 Mod 的依赖关系。"
				};
			}
			bool num = ModLoader.GetLoadedMods().Any((ModLoader.LoadedMod mod) => string.Equals(mod.LoadedId, modId, StringComparison.OrdinalIgnoreCase));
			bool wasEnabled = ids.Contains<string>(modId, StringComparer.OrdinalIgnoreCase);
			if (num)
			{
				XWModInstallTransaction.AddPendingDelete(_modsDirectory, new XWModInstallTransaction.PendingDelete
				{
					ModId = modEntry.Id,
					PackagePath = modEntry.PackagePath,
					PackageSha256 = sha
				});
				RestoreEnabledState(modId, enabled: false);
				return new DeleteResult
				{
					Success = true,
					PendingRestart = true,
					Message = modEntry.DisplayName + " 已标记为删除，将在下次启动前移除。"
				};
			}
			XWModInstallTransaction.Journal journal = new XWModInstallTransaction.Journal
			{
				ModId = modEntry.Id,
				TargetPath = modEntry.PackagePath,
				PreviousSha256 = sha,
				WasEnabled = wasEnabled,
				BackupPath = modEntry.PackagePath + $".delete-backup-{Guid.NewGuid():N}"
			};
			bool flag = false;
			try
			{
				XWModInstallTransaction.BeginDelete(_modsDirectory, journal);
				File.Move(modEntry.PackagePath, journal.BackupPath);
				XWModInstallTransaction.SetPhase(_modsDirectory, journal, "package-moved");
				RestoreEnabledState(modId, enabled: false);
				XWModInstallTransaction.SetPhase(_modsDirectory, journal, "committed");
				flag = true;
				XWModEnvironmentService.PackageInstalled(_modsDirectory, modId);
				File.Delete(journal.BackupPath);
				XWModInstallTransaction.Complete(_modsDirectory);
				return new DeleteResult
				{
					Success = true,
					Message = "已删除 Mod：" + modEntry.DisplayName + "。Mod 进度已保留。"
				};
			}
			catch (Exception ex)
			{
				if (flag)
				{
					return new DeleteResult
					{
						Success = true,
						Message = "安装包已移除，旧备份将在下次启动时继续清理。Mod 进度已保留。"
					};
				}
				if (!XWModInstallTransaction.TryRecover(this, out var diagnostic3))
				{
					return new DeleteResult
					{
						Blocked = true,
						Message = "删除失败，恢复仍受阻：" + diagnostic3
					};
				}
				return new DeleteResult
				{
					Blocked = true,
					Message = "删除失败：" + ex.GetBaseException().Message
				};
			}
		}
	}

	public bool LoadMod(string modId)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return false;
		}
		ModEntry modEntry = PreferValidCandidates(from item in ScanMods()
			where string.Equals(item.Id, modId, StringComparison.OrdinalIgnoreCase)
			select item).FirstOrDefault();
		ModLoader.LoadedMod loadedMod = ((modEntry?.Manifest == null) ? null : ModLoader.LoadMod(modEntry.PackagePath));
		if (loadedMod == null)
		{
			return false;
		}
		if (!ModLoader.ApplyMod(loadedMod))
		{
			if (ModLoader.IsTracked(loadedMod))
			{
				_loadedManifests[modEntry.Id] = modEntry.Manifest;
			}
			return false;
		}
		if (!loadedMod.NotifyAllModsLoaded())
		{
			if (!ModLoader.RollbackMod(modEntry.Id, out var blockers))
			{
				_loadedManifests[modEntry.Id] = modEntry.Manifest;
				GD.PushWarning("[ModManager] lifecycle rollback remains tracked: " + XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(modEntry.Id, blockers));
			}
			return false;
		}
		_loadedManifests[modEntry.Id] = modEntry.Manifest;
		return true;
	}

	public void LoadEnabledMods()
	{
		LoadEnabledModsWithResult();
	}

	public int LoadEnabledModsWithResult(bool recoverStorage = false)
	{
		return LoadEnabledModsDetailed(recoverStorage).LegacyCount;
	}

	public XWModStartupReport LoadEnabledModsDetailed(bool recoverStorage = false)
	{
		return CreateStartupReport(LoadEnabledModsCore(recoverStorage));
	}

	internal XWModStartupReport ApplyPreparedStartup(XWModStartupContext context)
	{
		return CreateStartupReport(ApplyEnabledPlan(recoverStorage: false, context));
	}

	private XWModStartupReport CreateStartupReport(int count)
	{
		IReadOnlyList<string> configured;
		string diagnostic;
		bool flag = TryLoadEnabledIds(out configured, out diagnostic);
		Dictionary<string, string> failures = (from item in GetRecentApplyFailures()
			where configured.Contains<string>(item.ModId, StringComparer.OrdinalIgnoreCase)
			select item).ToDictionary<ApplyFailureRecord, string, string>((ApplyFailureRecord item) => item.ModId, (ApplyFailureRecord item) => item.Message, StringComparer.OrdinalIgnoreCase);
		string[] array = (from mod in ModLoader.GetLoadedMods()
			where mod.Applied && SameDirectory(mod.LoadedPackagePath, _modsDirectory) && !failures.ContainsKey(mod.LoadedId)
			select mod.LoadedId).ToArray();
		foreach (string item in configured)
		{
			if (!array.Contains<string>(item, StringComparer.OrdinalIgnoreCase) && !failures.ContainsKey(item))
			{
				failures[item] = (_safeMode ? "安全模式下未加载；原启用配置已保留。" : "安装包或依赖不可用，请查看诊断。");
			}
		}
		XWModEnvironmentStatus status = XWModEnvironmentService.GetStatus();
		return new XWModStartupReport
		{
			LegacyCount = count,
			ConfiguredIds = configured,
			LoadedIds = array,
			FailedById = failures,
			Environment = status,
			BlockingReason = (_safeMode ? RecoveryDiagnostic : (flag ? status.Reason : diagnostic))
		};
	}

	private int LoadEnabledModsCore(bool recoverStorage)
	{
		return ApplyEnabledPlan(recoverStorage);
	}

	public void UnloadMod(string modId)
	{
		TryUnloadMod(modId);
	}

	public bool TryUnloadMod(string modId)
	{
		if (_applyingEnabledPlan || string.IsNullOrWhiteSpace(modId))
		{
			return false;
		}
		if (!ModLoader.UnloadMod(modId))
		{
			return false;
		}
		_loadedManifests.Remove(modId);
		return true;
	}

	public void UnloadAll()
	{
		TryUnloadAll();
	}

	public bool TryUnloadAll()
	{
		if (_applyingEnabledPlan)
		{
			return false;
		}
		if (!ModLoader.TryUnloadAll())
		{
			return false;
		}
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		current?.Dispose();
		XWModManager startupForDirectory = StartupForDirectory;
		if (startupForDirectory != null && startupForDirectory._bootAwaitingMenu)
		{
			startupForDirectory._bootAwaitingMenu = false;
			if (!startupForDirectory.BootRecovery.FinishDegraded(current?.HasFailures ?? false, out var error))
			{
				startupForDirectory._recoveryDiagnostic = error;
				GD.PushWarning("[ModManager] " + error);
			}
		}
		XWModRuntimeRegistry.BeginRegistration();
		_loadedManifests.Clear();
		XWModManager startupForDirectory2 = StartupForDirectory;
		startupForDirectory2?.CancelPreparedStartup();
		if (startupForDirectory2 != this)
		{
			CancelPreparedStartup();
		}
		XWModStartupContext.InvalidateForManager(this);
		return true;
	}

	public List<XWValidationIssue> ValidateEnableSet(IEnumerable<string> enabledIds)
	{
		return ResolveEnabledMods(enabledIds).Issues;
	}

	internal bool HasEnabledDependents(string modId, IEnumerable<string> enabledIds)
	{
		HashSet<string> enabled = new HashSet<string>(enabledIds, StringComparer.OrdinalIgnoreCase);
		return ScanMods().Any((ModEntry entry) => enabled.Contains(entry.Id) && !string.Equals(entry.Id, modId, StringComparison.OrdinalIgnoreCase) && (entry.Manifest?.Dependencies.Any((XWModDependency dependency) => string.Equals(dependency?.Id, modId, StringComparison.OrdinalIgnoreCase)) ?? false));
	}

	internal List<XWValidationIssue> ValidateReplacement(IEnumerable<string> enabledIds, XWModManifest replacement, string candidatePath = null)
	{
		List<ModEntry> list = ScanMods();
		string text = list.FirstOrDefault((ModEntry entry) => string.Equals(entry.Id, replacement.Id, StringComparison.OrdinalIgnoreCase))?.PackagePath;
		list.RemoveAll((ModEntry entry) => string.Equals(entry.Id, replacement.Id, StringComparison.OrdinalIgnoreCase));
		list.Add(new ModEntry
		{
			Id = replacement.Id,
			Manifest = replacement,
			HasManifest = true,
			PackagePath = (candidatePath ?? text ?? "")
		});
		return ResolveEnabledMods(enabledIds, list).Issues;
	}

	public ResolveEnabledResult ResolveEnabledMods(IEnumerable<string> enabledIds)
	{
		return ResolveEnabledMods(enabledIds, ScanMods());
	}

	private ResolveEnabledResult ResolveEnabledMods(IEnumerable<string> enabledIds, List<ModEntry> entries)
	{
		ResolveEnabledResult resolveEnabledResult = new ResolveEnabledResult();
		if (!TryGetSelection(out var state, out var diagnostic))
		{
			AddIssue(resolveEnabledResult, XWValidationIssue.IssueCode.ManifestError, diagnostic, "");
			return resolveEnabledResult;
		}
		if (!XWModInstallTransaction.TryLoadPendingDeletes(_modsDirectory, out var result, out diagnostic))
		{
			AddIssue(resolveEnabledResult, XWValidationIssue.IssueCode.RuntimeLoadFailed, diagnostic, "");
			return resolveEnabledResult;
		}
		foreach (ModEntry entry in entries)
		{
			entry.PendingDelete |= result.Any((XWModInstallTransaction.PendingDelete item) => string.Equals(item.ModId, entry.Id, StringComparison.OrdinalIgnoreCase));
		}
		XWModLoadPlan xWModLoadPlan = (resolveEnabledResult.Plan = XWModLoadPlan.Build(entries, enabledIds, state.PreferredOrder, null, XWModRuntimeSession.AvailableCapabilities));
		resolveEnabledResult.LoadOrder = xWModLoadPlan.LoadOrder.ToList();
		resolveEnabledResult.Issues = xWModLoadPlan.Issues.ToList();
		return resolveEnabledResult;
	}

	private static bool SameDirectory(string path, string directory)
	{
		if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(directory))
		{
			return false;
		}
		return string.Equals((Path.GetDirectoryName(Path.GetFullPath(path)) ?? "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
	}

	private static void AddIssue(ResolveEnabledResult result, XWValidationIssue.IssueCode code, string message, string resourceKey)
	{
		if (!result.Issues.Any((XWValidationIssue issue) => issue.Code == code && string.Equals(issue.Message, message, StringComparison.Ordinal)))
		{
			result.Issues.Add(new XWValidationIssue
			{
				Code = code,
				Message = message,
				ResourceKey = resourceKey,
				AffectedModIds = (string.IsNullOrWhiteSpace(resourceKey) ? new List<string>() : new List<string> { resourceKey })
			});
		}
	}

	internal SelectionPlan PreviewSelection(string modId, bool enabled, XWModManifest replacement = null, string candidatePath = null)
	{
		modId = modId?.Trim() ?? "";
		if (!TryLoadEnabledIds(out var ids, out var diagnostic))
		{
			return SelectionError(modId, enabled, "无法读取启用列表：" + diagnostic);
		}
		if (!XWModInstallTransaction.TryLoadPendingDeletes(_modsDirectory, out var _, out diagnostic))
		{
			return SelectionError(modId, enabled, "无法读取待删除记录：" + diagnostic);
		}
		List<ModEntry> entries = ScanMods();
		ResolveEnabledResult resolveEnabledResult = ResolveEnabledMods(ids, entries);
		if (replacement != null)
		{
			if (PreferValidCandidates(entries.Where((ModEntry entry) => entry.Id.Equals(modId, StringComparison.OrdinalIgnoreCase))).Length > 1)
			{
				return SelectionError(modId, enabled, "存在多个同名身份的安装包，请先在详情中查看重复项。");
			}
			entries.RemoveAll((ModEntry entry) => entry.Id.Equals(modId, StringComparison.OrdinalIgnoreCase));
			entries.Add(new ModEntry
			{
				Id = modId,
				DisplayName = replacement.Name,
				Manifest = replacement,
				HasManifest = true,
				PackagePath = (candidatePath ?? "")
			});
		}
		HashSet<string> desired = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Queue<string> queue = new Queue<string>();
		queue.Enqueue(modId);
		while (true)
		{
			if (!queue.TryDequeue(out var id))
			{
				break;
			}
			if (!hashSet.Add(id))
			{
				continue;
			}
			if (enabled)
			{
				desired.Add(id);
				ModEntry[] array = PreferValidCandidates(entries.Where((ModEntry entry) => entry.Id.Equals(id, StringComparison.OrdinalIgnoreCase)));
				if (array.Length != 1)
				{
					continue;
				}
				XWModManifest manifest = array[0].Manifest;
				if (manifest == null)
				{
					continue;
				}
				foreach (XWModDependency item in manifest.Dependencies ?? new List<XWModDependency>())
				{
					if (!string.IsNullOrWhiteSpace(item?.Id))
					{
						queue.Enqueue(item.Id);
					}
				}
				continue;
			}
			desired.Remove(id);
			foreach (ModEntry item2 in entries.Where((ModEntry entry) => desired.Contains(entry.Id)))
			{
				XWModManifest manifest2 = item2.Manifest;
				if (manifest2 != null && manifest2.Dependencies?.Any((XWModDependency dependency) => string.Equals(dependency?.Id, id, StringComparison.OrdinalIgnoreCase)) == true)
				{
					queue.Enqueue(item2.Id);
				}
			}
		}
		string[] array2 = desired.OrderBy<string, string>((string result2) => result2, StringComparer.OrdinalIgnoreCase).ToArray();
		ResolveEnabledResult resolveEnabledResult2 = ResolveEnabledMods(array2, entries);
		HashSet<string> loadable = resolveEnabledResult2.LoadOrder.Select((ModEntry entry) => entry.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> blocked = (from entry in resolveEnabledResult.LoadOrder
			where desired.Contains(entry.Id) && !loadable.Contains(entry.Id)
			select entry.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (enabled && !loadable.Contains(modId))
		{
			blocked.Add(modId);
		}
		XWValidationIssue[] array3 = resolveEnabledResult2.Issues.Where((XWValidationIssue issue) => issue.Level == XWValidationIssue.Severity.Error && (issue.AffectedModIds.Count == 0 || issue.AffectedModIds.Any(blocked.Contains))).ToArray();
		if (blocked.Count > 0 && array3.Length == 0)
		{
			array3 = new XWValidationIssue[1] { SelectionIssue(modId, "这次更改无法加载目标 Mod 或它需要的 Mod，请查看详情。") };
		}
		HashSet<string> previousSet = ids.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		SelectionChange[] changes = (from text in (from item in previousSet.Union<string>(desired, StringComparer.OrdinalIgnoreCase)
				where previousSet.Contains(item) != desired.Contains(item)
				select item).OrderBy<string, string>((string result2) => result2, StringComparer.OrdinalIgnoreCase)
			select new SelectionChange(text, entries.FirstOrDefault((ModEntry entry) => entry.Id.Equals(text, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? text, desired.Contains(text))).ToArray();
		SelectionPlan selectionPlan = new SelectionPlan();
		selectionPlan.ModId = modId;
		selectionPlan.Enabled = enabled;
		selectionPlan.PreviousIds = ids.ToArray();
		selectionPlan.DesiredIds = array2;
		selectionPlan.Changes = changes;
		selectionPlan.Issues = array3;
		selectionPlan.CanApply = blocked.Count == 0;
		selectionPlan.Evidence = string.Join(";", ids.OrderBy<string, string>((string result2) => result2, StringComparer.OrdinalIgnoreCase)) + "|" + string.Join(";", array2) + "|" + Hashes(resolveEnabledResult) + "|" + Hashes(resolveEnabledResult2);
		return selectionPlan;
		static string Hashes(ResolveEnabledResult resolveEnabledResult3)
		{
			if (resolveEnabledResult3.Plan != null)
			{
				return string.Join(";", from pair in resolveEnabledResult3.Plan.PackageHashes.OrderBy<KeyValuePair<string, string>, string>((KeyValuePair<string, string> pair) => pair.Key, StringComparer.OrdinalIgnoreCase)
					select pair.Key.ToLowerInvariant() + ":" + pair.Value.ToUpperInvariant());
			}
			return "";
		}
	}

	internal SetEnabledResult ApplySelection(SelectionPlan plan)
	{
		using (XWModInstallTransaction.Enter())
		{
			if (File.Exists(XWModInstallTransaction.JournalPath(_modsDirectory)))
			{
				return new SetEnabledResult
				{
					Blocked = true,
					Issues = new List<XWValidationIssue> { SelectionIssue(plan?.ModId ?? "", "上次安装尚未完成恢复，请先重启游戏。") }
				};
			}
			if (plan == null || !plan.CanApply)
			{
				return new SetEnabledResult
				{
					Blocked = true,
					Issues = (plan?.Issues.ToList() ?? new List<XWValidationIssue>())
				};
			}
			SelectionPlan selectionPlan = PreviewSelection(plan.ModId, plan.Enabled);
			if (!selectionPlan.CanApply || selectionPlan.Evidence != plan.Evidence)
			{
				return new SetEnabledResult
				{
					Blocked = true,
					Issues = new List<XWValidationIssue> { SelectionIssue(plan.ModId, "Mod 状态已变化，请重新查看并应用这次更改。") }
				};
			}
			if (selectionPlan.Changes.Count > 0)
			{
				SaveEnabledIds(selectionPlan.DesiredIds);
			}
			return new SetEnabledResult
			{
				Success = true,
				Changed = (selectionPlan.Changes.Count > 0)
			};
		}
	}

	private static SelectionPlan SelectionError(string id, bool enabled, string message)
	{
		SelectionPlan selectionPlan = new SelectionPlan();
		selectionPlan.ModId = id;
		selectionPlan.Enabled = enabled;
		selectionPlan.Issues = new XWValidationIssue[1] { SelectionIssue(id, message) };
		return selectionPlan;
	}

	private static XWValidationIssue SelectionIssue(string id, string message)
	{
		return new XWValidationIssue
		{
			Code = XWValidationIssue.IssueCode.RuntimeLoadFailed,
			Message = message,
			ResourceKey = id,
			AffectedModIds = new List<string> { id }
		};
	}
}
