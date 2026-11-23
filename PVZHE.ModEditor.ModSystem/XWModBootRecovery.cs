using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModBootRecovery
{
	private XWModBootJournal _current;

	private bool _ownsBoot;

	public string JournalPath { get; }

	public string LastGoodPath { get; }

	public XWModBootRecovery(string directory)
	{
		directory = Path.GetFullPath(directory);
		JournalPath = Path.Combine(directory, "mod_boot_journal.json");
		LastGoodPath = Path.Combine(directory, "mod_last_good.json");
	}

	public bool Inspect(out XWModBootJournal journal, out string error)
	{
		if (!ReadJournal(out journal, out error))
		{
			return true;
		}
		return journal.RecoveryRequired;
	}

	public bool Begin(string planHash, XWModSelectionState desired, IEnumerable<XWModBootPackage> packages, out string error)
	{
		if (_ownsBoot)
		{
			error = "已有加载事务正在进行。";
			return false;
		}
		if (Inspect(out var _, out error))
		{
			if (error.Length == 0)
			{
				error = "需要玩家明确重试或恢复后才能加载 Mod。";
			}
			return false;
		}
		if (string.IsNullOrWhiteSpace(planHash) || !ValidateCombination(desired, packages, out var _, out error))
		{
			if (string.IsNullOrWhiteSpace(planHash))
			{
				error = "加载计划哈希不能为空。";
			}
			return false;
		}
		XWModBootJournal xWModBootJournal = new XWModBootJournal
		{
			PlanHash = planHash,
			Phase = "preflight",
			InProgress = true
		};
		if (!SaveJournal(xWModBootJournal, out error))
		{
			return false;
		}
		_current = xWModBootJournal;
		_ownsBoot = true;
		return true;
	}

	public bool RecordPhase(string owner, string phase, out string error)
	{
		if (!_ownsBoot)
		{
			error = "没有当前加载事务。";
			return false;
		}
		if (string.IsNullOrWhiteSpace(phase))
		{
			error = "加载阶段不能为空。";
			return false;
		}
		XWModBootJournal xWModBootJournal = CopyCurrent();
		xWModBootJournal.Owner = XWModSelectionState.Owner(owner);
		xWModBootJournal.Phase = phase;
		if (!SaveJournal(xWModBootJournal, out error))
		{
			return false;
		}
		_current = xWModBootJournal;
		return true;
	}

	public bool FinishReady(XWModSelectionState actualSelection, IEnumerable<XWModBootPackage> packages, bool fullApplySucceeded, bool mainMenuReady, out string error)
	{
		if (!_ownsBoot || !fullApplySucceeded || !mainMenuReady)
		{
			error = "只有完整应用成功且主菜单就绪的加载事务才能记录成功组合。";
			return false;
		}
		if (!ValidateCombination(actualSelection, packages, out var normalized, out error))
		{
			return false;
		}
		try
		{
			XWModLastGoodState value = new XWModLastGoodState
			{
				Selection = actualSelection.Normalize(),
				Packages = normalized
			};
			XWModStateFile.Write(LastGoodPath, JsonSerializer.SerializeToUtf8Bytes(value, XWModBootJsonContext.Default.XWModLastGoodState));
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = "保存成功组合失败：" + ex.Message;
			return false;
		}
		return Finish("ready", recoveryRequired: false, out error);
	}

	public bool FinishDegraded(bool rollbackBlocked, out string error)
	{
		return Finish(rollbackBlocked ? "rollback-blocked" : "degraded", rollbackBlocked, out error);
	}

	public bool RequireRecovery(string phase, out string error)
	{
		if (_ownsBoot)
		{
			return FinishDegraded(rollbackBlocked: true, out error);
		}
		if (!ReadJournal(out var journal, out error))
		{
			return false;
		}
		journal.RecoveryRequired = true;
		journal.Phase = phase;
		return SaveJournal(journal, out error);
	}

	private bool Finish(string phase, bool recoveryRequired, out string error)
	{
		if (!_ownsBoot)
		{
			error = "没有当前加载事务。";
			return false;
		}
		XWModBootJournal xWModBootJournal = CopyCurrent();
		xWModBootJournal.Phase = phase;
		xWModBootJournal.InProgress = false;
		xWModBootJournal.RecoveryRequired = recoveryRequired;
		if (!SaveJournal(xWModBootJournal, out error))
		{
			return false;
		}
		_current = xWModBootJournal;
		_ownsBoot = false;
		return true;
	}

	public bool RequestRetry(out string error)
	{
		if (_ownsBoot)
		{
			error = "加载尚未结束，不能重置恢复状态。";
			return false;
		}
		try
		{
			if (File.Exists(JournalPath))
			{
				XWModStateFile.Write(JournalPath + "." + Guid.NewGuid().ToString("N") + ".recovery.bak", File.ReadAllBytes(JournalPath), overwrite: false);
			}
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = "备份恢复日志失败：" + ex.Message;
			return false;
		}
		if (!SaveJournal(new XWModBootJournal
		{
			Phase = "retry-requested"
		}, out error))
		{
			return false;
		}
		_current = null;
		return true;
	}

	public bool TryRestoreLastGood(IEnumerable<XWModBootPackage> availablePackages, out XWModSelectionState selection, out string error)
	{
		selection = null;
		error = "";
		try
		{
			if (!File.Exists(LastGoodPath))
			{
				throw new InvalidDataException("尚无成功加载记录。");
			}
			byte[] array = File.ReadAllBytes(LastGoodPath);
			using JsonDocument jsonDocument = JsonDocument.Parse(array);
			Require(jsonDocument.RootElement, "schemaVersion", "selection", "packages");
			Require(jsonDocument.RootElement.GetProperty("selection"), "schemaVersion", "enabledIds", "preferredOrder");
			JsonElement property = jsonDocument.RootElement.GetProperty("packages");
			if (property.ValueKind != JsonValueKind.Array)
			{
				throw new InvalidDataException("成功组合包列表无效。");
			}
			foreach (JsonElement item in property.EnumerateArray())
			{
				Require(item, "id", "version", "sha256");
			}
			XWModLastGoodState xWModLastGoodState = JsonSerializer.Deserialize(array, XWModBootJsonContext.Default.XWModLastGoodState);
			if (xWModLastGoodState == null || xWModLastGoodState.SchemaVersion != 1 || !ValidateCombination(xWModLastGoodState.Selection, xWModLastGoodState.Packages, out var normalized, out error))
			{
				throw new InvalidDataException((error.Length > 0) ? error : "成功组合版本无效。");
			}
			List<XWModBootPackage> source = NormalizePackages(availablePackages);
			foreach (XWModBootPackage package in normalized)
			{
				if (!source.Any((XWModBootPackage candidate) => candidate.Id == package.Id && candidate.Version == package.Version && candidate.Sha256 == package.Sha256))
				{
					throw new InvalidDataException("成功组合中的 Mod '" + package.Id + "' 缺失或版本/哈希不匹配。");
				}
			}
			selection = xWModLastGoodState.Selection.Normalize();
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = "不能恢复成功组合：" + ex.Message;
			return false;
		}
	}

	private bool ReadJournal(out XWModBootJournal journal, out string error)
	{
		journal = new XWModBootJournal();
		error = "";
		try
		{
			if (!File.Exists(JournalPath))
			{
				return true;
			}
			byte[] array = File.ReadAllBytes(JournalPath);
			using JsonDocument jsonDocument = JsonDocument.Parse(array);
			Require(jsonDocument.RootElement, "schemaVersion", "planHash", "owner", "phase", "inProgress", "recoveryRequired");
			journal = JsonSerializer.Deserialize(array, XWModBootJsonContext.Default.XWModBootJournal);
			XWModBootJournal obj = journal;
			if (obj == null || obj.SchemaVersion != 1 || journal.PlanHash == null || journal.Owner == null || string.IsNullOrWhiteSpace(journal.Phase))
			{
				throw new InvalidDataException("启动日志格式或版本无效。");
			}
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			journal = new XWModBootJournal
			{
				RecoveryRequired = true,
				Phase = "invalid-journal"
			};
			error = "启动日志不可读，将进入零 Mod 安全模式：" + ex.Message;
			return false;
		}
	}

	private bool SaveJournal(XWModBootJournal journal, out string error)
	{
		try
		{
			XWModStateFile.Write(JournalPath, JsonSerializer.SerializeToUtf8Bytes(journal, XWModBootJsonContext.Default.XWModBootJournal));
			error = "";
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = "写入启动日志失败：" + ex.Message;
			return false;
		}
	}

	private XWModBootJournal CopyCurrent()
	{
		return new XWModBootJournal
		{
			PlanHash = _current.PlanHash,
			Owner = _current.Owner,
			Phase = _current.Phase,
			InProgress = _current.InProgress,
			RecoveryRequired = _current.RecoveryRequired
		};
	}

	private static void Require(JsonElement value, params string[] names)
	{
		if (value.ValueKind != JsonValueKind.Object || names.Any((string name) => !value.TryGetProperty(name, out var _)))
		{
			throw new InvalidDataException("恢复文件缺少必要字段。");
		}
	}

	private static bool ValidateCombination(XWModSelectionState state, IEnumerable<XWModBootPackage> packages, out List<XWModBootPackage> normalized, out string error)
	{
		normalized = null;
		error = "";
		try
		{
			if (state == null || state.SchemaVersion != 2 || state.EnabledIds == null || state.PreferredOrder == null || state.EnabledIds.Any((string id) => id == null) || state.PreferredOrder.Any((string id) => id == null))
			{
				throw new InvalidDataException("组合中的启用配置无效。");
			}
			normalized = NormalizePackages(packages);
			if (normalized.Select((XWModBootPackage package) => package.Id).Distinct<string>(StringComparer.Ordinal).Count() != normalized.Count || !state.Normalize().EnabledIds.SequenceEqual(normalized.Select((XWModBootPackage package) => package.Id)))
			{
				throw new InvalidDataException("组合必须为每个启用 Mod 记录且仅记录一个包。");
			}
			return true;
		}
		catch (Exception ex) when (XWModStateFile.IsStorageError(ex))
		{
			error = ex.Message;
			return false;
		}
	}

	private static List<XWModBootPackage> NormalizePackages(IEnumerable<XWModBootPackage> packages)
	{
		if (packages == null)
		{
			throw new InvalidDataException("包列表为空。");
		}
		return packages.Select(delegate(XWModBootPackage package)
		{
			if (package == null || string.IsNullOrWhiteSpace(package.Id) || package.Version == null || package.Sha256 == null || package.Sha256.Length != 64 || !package.Sha256.All(Uri.IsHexDigit))
			{
				throw new InvalidDataException("包 ID、版本或 SHA256 无效。");
			}
			return new XWModBootPackage
			{
				Id = XWModSelectionState.Owner(package.Id),
				Version = package.Version,
				Sha256 = package.Sha256.ToLowerInvariant()
			};
		}).OrderBy<XWModBootPackage, string>((XWModBootPackage package) => package.Id, StringComparer.Ordinal).ToList();
	}
}
