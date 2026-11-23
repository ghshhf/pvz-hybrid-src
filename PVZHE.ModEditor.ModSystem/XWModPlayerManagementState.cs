using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using PVZHE.ModEditor.ModSystem.Validation;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModPlayerManagementState
{
	internal enum Filter
	{
		All,
		Enabled,
		Running,
		Pending,
		Problems
	}

	internal sealed record Item(XWModManager.ModEntry Entry, string SearchText, IReadOnlyList<string> PendingReasons, IReadOnlyList<XWValidationIssue> Issues, bool OrderPending)
	{
		internal bool Pending
		{
			get
			{
				if (PendingReasons.Count <= 0)
				{
					return OrderPending;
				}
				return true;
			}
		}

		internal bool HasProblems => Issues.Any((XWValidationIssue issue) => issue.Level != XWValidationIssue.Severity.Info);

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Entry = ");
			builder.Append(Entry);
			builder.Append(", SearchText = ");
			builder.Append((object?)SearchText);
			builder.Append(", PendingReasons = ");
			builder.Append(PendingReasons);
			builder.Append(", Issues = ");
			builder.Append(Issues);
			builder.Append(", OrderPending = ");
			builder.Append(OrderPending.ToString());
			return true;
		}
	}

	internal IReadOnlyList<Item> Items { get; }

	internal bool OrderChanged { get; }

	internal int PendingCount => (from item in Items
		where item.PendingReasons.Count > 0
		select item.Entry.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count() + (OrderChanged ? 1 : 0);

	internal XWModPlayerManagementState(IEnumerable<XWModManager.ModEntry> entries, IEnumerable<XWValidationIssue> issues, bool orderChanged = false, Func<XWModManager.ModEntry, XWModSettingsService.PendingState> readSettings = null)
	{
		OrderChanged = orderChanged;
		XWValidationIssue[] allIssues = issues.ToArray();
		Items = Array.AsReadOnly(entries.Select(delegate(XWModManager.ModEntry entry)
		{
			List<XWValidationIssue> problems = allIssues.Where((XWValidationIssue issue) => (issue.AffectedModIds ?? new List<string>()).Contains<string>(entry.Id, StringComparer.OrdinalIgnoreCase)).ToList();
			Problem(entry.Diagnostic);
			if (!entry.HasManifest && string.IsNullOrWhiteSpace(entry.Diagnostic))
			{
				Problem("安装包缺失或清单无效，请重新导入。");
			}
			if (entry.Enabled)
			{
				Problem(entry.LastApplyFailure);
			}
			string effectiveState = entry.EffectiveState;
			bool flag = ((effectiveState == "加载失败" || effectiveState == "回滚受阻") ? true : false);
			if (flag && string.IsNullOrWhiteSpace(entry.LastApplyFailure))
			{
				Problem(entry.EffectiveState);
			}
			List<string> list = new List<string>();
			if (entry.PendingDelete)
			{
				list.Add("重启后删除安装包");
			}
			else if (entry.Enabled != entry.Loaded)
			{
				list.Add(entry.Enabled ? "下次启动请求启用" : "重启后停用");
			}
			if (entry.EffectiveState == "已加载旧版本")
			{
				list.Add("重启后使用已安装的新包");
			}
			if (entry.HasManifest && entry.Manifest != null)
			{
				XWModSettingsService.PendingState pendingState = readSettings?.Invoke(entry) ?? XWModSettingsService.ReadPendingState(entry.Manifest, entry.PackageSha256);
				Problem(pendingState.Diagnostic, XWValidationIssue.Severity.Warning);
				if (pendingState.PackageChanged && entry.EffectiveState != "已加载旧版本")
				{
					list.Add("重启后使用已安装的新包");
				}
				if (pendingState.ApplyFailed)
				{
					list.Add("配置应用失败，需要重启");
					Problem("配置已保存，但 Mod 应用配置时出错，请重启游戏。");
				}
				foreach (string key in pendingState.ChangedKeys)
				{
					XWModSettingDefinition xWModSettingDefinition = entry.Manifest.Settings.Find((XWModSettingDefinition item) => item.Key == key);
					string text = (string.IsNullOrWhiteSpace(xWModSettingDefinition?.Label) ? key : xWModSettingDefinition.Label);
					list.Add("配置待生效：" + text);
				}
			}
			XWModManager.ModEntry entry2 = entry;
			global::_003C_003Ey__InlineArray3<string> buffer = default(global::_003C_003Ey__InlineArray3<string>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<string>, string>(ref buffer, 0) = entry.DisplayName;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<string>, string>(ref buffer, 1) = entry.Id;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<string>, string>(ref buffer, 2) = entry.Manifest?.Author;
			return new Item(entry2, string.Join(" ", global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray3<string>, string>(in buffer, 3)), list.AsReadOnly(), (from issue in problems.DistinctBy((XWValidationIssue issue) => (Code: issue.Code, Level: issue.Level, Message: issue.Message))
				orderby issue.Level descending
				select issue).ToArray(), orderChanged && entry.Enabled && entry.Loaded);
			void Problem(string message, XWValidationIssue.Severity severity = XWValidationIssue.Severity.Error)
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					problems.Add(new XWValidationIssue
					{
						Code = XWValidationIssue.IssueCode.RuntimeLoadFailed,
						Level = severity,
						Message = message,
						AffectedModIds = new List<string> { entry.Id }
					});
				}
			}
		}).ToArray());
	}

	internal IReadOnlyList<Item> Match(string query, Filter filter, string exactId = null)
	{
		string[] terms = (query ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		return Items.Where(delegate(Item item)
		{
			bool flag = (exactId == null || string.Equals(exactId, item.Entry.Id, StringComparison.OrdinalIgnoreCase)) && terms.All((string term) => item.SearchText.Contains(term, StringComparison.OrdinalIgnoreCase));
			if (flag)
			{
				flag = filter switch
				{
					Filter.Enabled => item.Entry.Enabled, 
					Filter.Running => item.Entry.Loaded, 
					Filter.Pending => item.Pending, 
					Filter.Problems => item.HasProblems, 
					_ => true, 
				};
			}
			return flag;
		}).ToArray();
	}

	internal Item Find(XWModManager.ModEntry entry)
	{
		if (entry != null)
		{
			return Items.FirstOrDefault((Item item) => Key(item.Entry).Equals(Key(entry), StringComparison.OrdinalIgnoreCase));
		}
		return null;
	}

	internal static string Key(XWModManager.ModEntry entry)
	{
		if (entry != null)
		{
			return entry.Id + "\n" + entry.PackagePath;
		}
		return "";
	}

	internal static string Evidence(IEnumerable<XWModManager.ModEntry> entries)
	{
		return string.Join("\n", from entry in entries.OrderBy<XWModManager.ModEntry, string>(Key, StringComparer.OrdinalIgnoreCase)
			select $"{Key(entry)}\0{entry.PackageSha256}\0{entry.Enabled}\0{entry.PendingDelete}\0{entry.HasManifest}\0{entry.Diagnostic}");
	}
}
