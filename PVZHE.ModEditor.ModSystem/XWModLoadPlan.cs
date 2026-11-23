using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PVZHE.ModEditor.ModSystem.Validation;
using PVZHE.ModEditor.ScriptEditor;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModLoadPlan
{
	private readonly XWModManager.ModEntry[] _loadOrder;

	private readonly XWValidationIssue[] _issues;

	private readonly Dictionary<string, string> _packagePaths;

	private readonly XWModManager.ModEntry[] _sourceEntries;

	private readonly string[] _enabledIds;

	private readonly IReadOnlySet<string> _capabilities;

	private readonly XWValidationIssue[] _hashFailures;

	private readonly XWValidationIssue[] _assemblyIssues;

	private readonly Dictionary<string, string> _manifestHashes = new Dictionary<string, string>(StringComparer.Ordinal);

	public bool IsReorderPreview { get; }

	public IReadOnlyList<XWModManager.ModEntry> LoadOrder => Array.AsReadOnly(_loadOrder.Select(CloneEntry).ToArray());

	public IReadOnlyList<XWValidationIssue> Issues => Array.AsReadOnly(_issues.Select(CloneIssue).ToArray());

	public IReadOnlyDictionary<string, string> PackageHashes { get; }

	public IReadOnlyDictionary<string, IReadOnlyList<XWModAssemblyMetadata>> AssemblyMetadata { get; }

	public string PlanHash { get; }

	public bool HasErrors => _issues.Any((XWValidationIssue i) => i.Level == XWValidationIssue.Severity.Error);

	private XWModLoadPlan(IEnumerable<XWModManager.ModEntry> order, IEnumerable<XWValidationIssue> issues, Dictionary<string, string> hashes, Dictionary<string, string> paths, IEnumerable<string> enabled, IReadOnlyDictionary<string, IReadOnlyList<XWModAssemblyMetadata>> metadata, XWModManager.ModEntry[] sourceEntries, IReadOnlySet<string> capabilities, IEnumerable<XWValidationIssue> hashFailures, IEnumerable<XWValidationIssue> assemblyIssues, bool isPreview)
	{
		_loadOrder = order.Select(CloneEntry).ToArray();
		_issues = issues.Select(CloneIssue).ToArray();
		PackageHashes = new ReadOnlyDictionary<string, string>(hashes);
		_packagePaths = paths;
		AssemblyMetadata = metadata;
		_sourceEntries = sourceEntries;
		_enabledIds = enabled.ToArray();
		_capabilities = capabilities;
		_hashFailures = hashFailures.Select(CloneIssue).ToArray();
		_assemblyIssues = assemblyIssues.Select(CloneIssue).ToArray();
		IsReorderPreview = isPreview;
		foreach (IGrouping<string, XWModManager.ModEntry> item in from entry in sourceEntries
			where entry.Manifest != null && _enabledIds.Contains<string>(CanonicalId(entry.Id), StringComparer.Ordinal)
			group entry by CanonicalId(entry.Id))
		{
			if (item.Count() == 1 && TryManifestHash(item.Single().Manifest, out var digest))
			{
				_manifestHashes[item.Key] = digest;
			}
		}
		StringBuilder material = new StringBuilder("XWModLoadPlan:1;");
		foreach (string item2 in enabled.OrderBy<string, string>((string x) => x, StringComparer.Ordinal))
		{
			Add(item2);
			Add(hashes.GetValueOrDefault(item2));
			Add(_manifestHashes.GetValueOrDefault(item2));
		}
		XWModManager.ModEntry[] loadOrder = _loadOrder;
		foreach (XWModManager.ModEntry obj in loadOrder)
		{
			Add(CanonicalId(obj.Id));
			Add(obj.Manifest.Version);
		}
		XWValidationIssue[] issues2 = _issues;
		foreach (XWValidationIssue obj2 in issues2)
		{
			Add(obj2.Level.ToString());
			Add(obj2.Code.ToString());
			Add(obj2.ResourceKey);
		}
		PlanHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material.ToString())));
		void Add(string value)
		{
			if (value == null)
			{
				value = "";
			}
			material.Append(value.Length).Append(':').Append(value);
		}
	}

	public bool VerifyPackageHashes(out IReadOnlyList<XWValidationIssue> issues)
	{
		List<XWValidationIssue> list = new List<XWValidationIssue>();
		if (IsReorderPreview)
		{
			list.Add(Issue(XWValidationIssue.IssueCode.PackageChanged, "", "内存排序预览不能直接执行，请重新读取安装包并生成正式加载计划。"));
			issues = list.AsReadOnly();
			return false;
		}
		foreach (KeyValuePair<string, string> packageHash in PackageHashes)
		{
			try
			{
				using FileStream source = File.OpenRead(_packagePaths[packageHash.Key]);
				if (!string.Equals(Convert.ToHexString(SHA256.HashData(source)), packageHash.Value, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(Issue(XWValidationIssue.IssueCode.PackageChanged, packageHash.Key, "安装包在预检后发生变化，必须重新生成加载计划。"));
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException) ? 1 : 0) != 0)
			{
				list.Add(Issue(XWValidationIssue.IssueCode.PackageChanged, packageHash.Key, "无法复核安装包：" + ex.Message));
			}
		}
		issues = list.AsReadOnly();
		return list.Count == 0;
	}

	public static XWModLoadPlan Build(IEnumerable<XWModManager.ModEntry> entries, IEnumerable<string> enabledIds, IEnumerable<string> preferredOrder = null, IReadOnlyDictionary<string, string> packageHashes = null, IReadOnlySet<string> availableCapabilities = null)
	{
		return BuildCore(entries, enabledIds, preferredOrder, packageHashes, availableCapabilities, null);
	}

	public XWModLoadPlan Reorder(IEnumerable<string> preferredOrder)
	{
		return BuildCore(_sourceEntries, _enabledIds, preferredOrder, PackageHashes, _capabilities, this);
	}

	private static XWModLoadPlan BuildCore(IEnumerable<XWModManager.ModEntry> entries, IEnumerable<string> enabledIds, IEnumerable<string> preferredOrder, IReadOnlyDictionary<string, string> packageHashes, IReadOnlySet<string> availableCapabilities, XWModLoadPlan cached)
	{
		XWModManager.ModEntry[] array = cached?._sourceEntries ?? (entries ?? Array.Empty<XWModManager.ModEntry>()).Where((XWModManager.ModEntry e) => e != null).Select(CloneEntry).ToArray();
		availableCapabilities = cached?._capabilities ?? ((availableCapabilities == null) ? null : new HashSet<string>(availableCapabilities, StringComparer.Ordinal));
		List<XWValidationIssue> list = new List<XWValidationIssue>();
		HashSet<string> enabled = (from x in (enabledIds ?? Array.Empty<string>()).Select(CanonicalId)
			where x.Length > 0
			select x).ToHashSet<string>(StringComparer.Ordinal);
		List<XWValidationIssue> issues = new List<XWValidationIssue>();
		HashSet<string> invalid = new HashSet<string>(StringComparer.Ordinal);
		Dictionary<string, XWModManager.ModEntry> catalog = new Dictionary<string, XWModManager.ModEntry>(StringComparer.Ordinal);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (IGrouping<string, XWModManager.ModEntry> item2 in (from e in array
			group e by CanonicalId(e.Id)).OrderBy<IGrouping<string, XWModManager.ModEntry>, string>((IGrouping<string, XWModManager.ModEntry> g) => g.Key, StringComparer.Ordinal))
		{
			if (!enabled.Contains(item2.Key))
			{
				continue;
			}
			XWModManager.ModEntry[] array2 = XWModManager.PreferValidCandidates(item2);
			if (array2.Length > 1 && array2[0].Manifest != null)
			{
				Reject(item2.Key, XWValidationIssue.IssueCode.ManifestError, "存在重复的已安装 Mod ID。");
				continue;
			}
			catalog[item2.Key] = CloneEntry(array2[0]);
			if (array2[0].Manifest == null)
			{
				catalog[item2.Key].Diagnostic = string.Join("；", (from entry in array2
					select entry.Diagnostic into message
					where !string.IsNullOrWhiteSpace(message)
					select message).Distinct().Take(6));
			}
		}
		foreach (string id in enabled.OrderBy<string, string>((string x) => x, StringComparer.Ordinal))
		{
			if (!catalog.TryGetValue(id, out var value) || value.Manifest == null)
			{
				Reject(id, XWValidationIssue.IssueCode.ManifestError, string.IsNullOrWhiteSpace(value?.Diagnostic) ? "已启用 Mod 缺失或清单无效。" : value.Diagnostic);
				continue;
			}
			if (CanonicalId(value.Manifest.Id) != id || value.PendingDelete)
			{
				Reject(id, XWValidationIssue.IssueCode.ManifestError, "清单 ID 不一致或 Mod 正等待删除。");
				continue;
			}
			if (!XWModRuntimeCompatibility.ValidatePackage(value.Manifest, out var diagnostic))
			{
				Reject(id, XWValidationIssue.IssueCode.ManifestError, diagnostic);
			}
			dictionary2[id] = value.PackagePath;
			if (cached != null)
			{
				if (cached.PackageHashes.TryGetValue(id, out var value2))
				{
					dictionary[id] = value2;
					value.PackageSha256 = value2;
				}
				foreach (XWValidationIssue item3 in cached._hashFailures.Where((XWValidationIssue issue) => issue.ResourceKey == id))
				{
					invalid.Add(id);
					issues.Add(CloneIssue(item3));
					list.Add(CloneIssue(item3));
				}
			}
			else
			{
				try
				{
					string text = packageHashes?.FirstOrDefault((KeyValuePair<string, string> p) => CanonicalId(p.Key) == id).Value;
					if (text != null)
					{
						dictionary[id] = text.ToUpperInvariant();
					}
					else
					{
						using FileStream source = File.OpenRead(value.PackagePath);
						dictionary[id] = Convert.ToHexString(SHA256.HashData(source));
					}
					value.PackageSha256 = dictionary[id];
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException) ? 1 : 0) != 0)
				{
					XWValidationIssue item = Issue(XWValidationIssue.IssueCode.PackageChanged, id, "无法读取安装包：" + ex.Message);
					invalid.Add(id);
					issues.Add(item);
					list.Add(item);
				}
			}
			foreach (string item4 in value.Manifest.RequiredCapabilities ?? new List<string>())
			{
				if (string.IsNullOrWhiteSpace(item4) || availableCapabilities == null || !availableCapabilities.Contains(item4))
				{
					Reject(id, XWValidationIssue.IssueCode.MissingCapability, "宿主未提供所需能力：" + item4 + "。");
				}
			}
			foreach (XWModDependency item5 in (value.Manifest.Dependencies ?? new List<XWModDependency>()).Concat(value.Manifest.OptionalDependencies ?? new List<XWModDependency>()).Concat(value.Manifest.Conflicts ?? new List<XWModDependency>()))
			{
				if (item5 == null || CanonicalId(item5.Id).Length == 0 || !XWModLoadPlanVersion.IsValidRequirement(item5))
				{
					Reject(id, XWValidationIssue.IssueCode.ManifestError, "依赖或冲突声明 ID / 版本范围无效。");
				}
			}
		}
		HashSet<(string From, string To)> hard = new HashSet<(string, string)>();
		HashSet<(string, string)> hashSet = new HashSet<(string, string)>();
		foreach (KeyValuePair<string, XWModManager.ModEntry> item6 in catalog.OrderBy<KeyValuePair<string, XWModManager.ModEntry>, string>((KeyValuePair<string, XWModManager.ModEntry> p) => p.Key, StringComparer.Ordinal))
		{
			string key = item6.Key;
			XWModManifest manifest = item6.Value.Manifest;
			if (manifest == null)
			{
				continue;
			}
			foreach (XWModDependency d in manifest.Dependencies ?? new List<XWModDependency>())
			{
				if (!Find(d, out var target))
				{
					XWModManager.ModEntry[] array3 = XWModManager.PreferValidCandidates(array.Where((XWModManager.ModEntry e) => CanonicalId(e.Id) == CanonicalId(d?.Id)));
					string value3 = ((array3.Length == 0) ? "尚未安装，请导入对应安装包" : (array3.Any((XWModManager.ModEntry e) => e.PendingDelete) ? "正在等待删除" : ((array3.Length > 1) ? "存在重复 ID，请检查安装包" : ((!array3[0].HasManifest || array3[0].Manifest == null) ? "安装包异常，请重新导入" : "已安装但未启用，请先启用此依赖"))));
					Reject(key, XWValidationIssue.IssueCode.DependencyConflict, $"必需依赖 {d?.Id}（要求 {DependencyVersion(d)}）：{value3}。", new string[1] { d?.Id });
				}
				else
				{
					if (!XWModLoadPlanVersion.Matches(d, target.Manifest.Version))
					{
						Reject(key, XWValidationIssue.IssueCode.VersionMismatch, $"依赖 {d.Id} 要求 {DependencyVersion(d)}，实际版本为 {target.Manifest.Version}。请安装符合要求的版本。", new string[1] { d.Id });
					}
					hard.Add((CanonicalId(d.Id), key));
				}
			}
			foreach (XWModContentEntry item7 in manifest.ContentEntries ?? new List<XWModContentEntry>())
			{
				if (item7?.Target != null)
				{
					string provider = CanonicalId(item7.Target.ProviderModId);
					if (provider.Length != 0 && !(provider == key) && !(manifest.Dependencies ?? new List<XWModDependency>()).Any((XWModDependency xWModDependency) => xWModDependency != null && CanonicalId(xWModDependency.Id) == provider))
					{
						Reject(key, XWValidationIssue.IssueCode.DependencyConflict, $"覆盖其他 Mod 的内容 {provider}/{item7.Target.Category}/{item7.Target.Key} 必须在 dependencies 中显式声明提供者 {provider}。", new string[1] { provider });
					}
				}
			}
			foreach (XWModDependency item8 in manifest.OptionalDependencies ?? new List<XWModDependency>())
			{
				if (Find(item8, out var target2) && XWModLoadPlanVersion.Matches(item8, target2.Manifest.Version))
				{
					hashSet.Add((CanonicalId(item8.Id), key));
				}
			}
			foreach (string item9 in manifest.LoadBefore ?? new List<string>())
			{
				if (enabled.Contains(CanonicalId(item9)))
				{
					hashSet.Add((key, CanonicalId(item9)));
				}
			}
			foreach (string item10 in manifest.LoadAfter ?? new List<string>())
			{
				if (enabled.Contains(CanonicalId(item10)))
				{
					hashSet.Add((CanonicalId(item10), key));
				}
			}
			foreach (XWModDependency item11 in manifest.Conflicts ?? new List<XWModDependency>())
			{
				if (Find(item11, out var target3) && XWModLoadPlanVersion.Matches(item11, target3.Manifest.Version))
				{
					Reject(key, XWValidationIssue.IssueCode.DependencyConflict, "与 " + item11.Id + " 显式不兼容，请选择其中一个启用。", new string[1] { item11.Id });
					Reject(CanonicalId(item11.Id), XWValidationIssue.IssueCode.DependencyConflict, "与 " + key + " 显式不兼容，请选择其中一个启用。", new string[1] { key });
				}
			}
		}
		HashSet<string> candidates = enabled.Where((string text2) => catalog.ContainsKey(text2) && !invalid.Contains(text2)).ToHashSet<string>(StringComparer.Ordinal);
		foreach (List<string> item12 in Components(candidates, hard))
		{
			if (item12.Count <= 1 && !hard.Contains((item12[0], item12[0])))
			{
				continue;
			}
			foreach (string item13 in item12)
			{
				Reject(item13, XWValidationIssue.IssueCode.DependencyCycle, "必需依赖形成环：" + string.Join(", ", item12) + "。请联系作者修正依赖声明。", item12);
			}
		}
		bool flag;
		do
		{
			flag = false;
			foreach (var item14 in hard.OrderBy<(string, string), string>(((string From, string To) e) => e.To, StringComparer.Ordinal).ThenBy<(string, string), string>(((string From, string To) e) => e.From, StringComparer.Ordinal))
			{
				if (invalid.Contains(item14.Item1) && !invalid.Contains(item14.Item2))
				{
					Reject(item14.Item2, XWValidationIssue.IssueCode.DependencyConflict, "必需依赖 " + item14.Item1 + " 无法加载，请先处理该依赖的问题。", new string[1] { item14.Item1 });
					flag = true;
				}
			}
		}
		while (flag);
		int count = issues.Count;
		IReadOnlyDictionary<string, IReadOnlyList<XWModAssemblyMetadata>> metadata = cached?.AssemblyMetadata;
		if (cached == null)
		{
			metadata = PreflightAssemblies(catalog, invalid, packageHashes, issues);
		}
		else
		{
			XWValidationIssue[] assemblyIssues = cached._assemblyIssues;
			foreach (XWValidationIssue xWValidationIssue in assemblyIssues)
			{
				issues.Add(CloneIssue(xWValidationIssue));
				if (xWValidationIssue.Level == XWValidationIssue.Severity.Error)
				{
					invalid.Add(xWValidationIssue.ResourceKey);
				}
			}
		}
		XWValidationIssue[] assemblyIssues2 = issues.Skip(count).Select(CloneIssue).ToArray();
		do
		{
			flag = false;
			foreach (var item15 in hard.OrderBy<(string, string), string>(((string From, string To) e) => e.To, StringComparer.Ordinal).ThenBy<(string, string), string>(((string From, string To) e) => e.From, StringComparer.Ordinal))
			{
				if (invalid.Contains(item15.Item1) && !invalid.Contains(item15.Item2))
				{
					Reject(item15.Item2, XWValidationIssue.IssueCode.DependencyConflict, "必需依赖 " + item15.Item1 + " 无法加载，请先处理该依赖的问题。", new string[1] { item15.Item1 });
					flag = true;
				}
			}
		}
		while (flag);
		candidates.ExceptWith(invalid);
		hard.RemoveWhere(((string From, string To) e) => !candidates.Contains(e.From) || !candidates.Contains(e.To));
		hashSet.RemoveWhere(((string From, string To) e) => !candidates.Contains(e.From) || !candidates.Contains(e.To) || hard.Contains(e));
		foreach (List<string> item16 in Components(candidates, hard.Concat<(string, string)>(hashSet)))
		{
			HashSet<string> members = item16.ToHashSet<string>(StringComparer.Ordinal);
			if ((item16.Count >= 2 || hashSet.Contains((item16[0], item16[0]))) && hashSet.RemoveWhere(((string From, string To) e) => members.Contains(e.From) && members.Contains(e.To)) > 0)
			{
				issues.Add(Issue(XWValidationIssue.IssueCode.SoftDependencyCycle, string.Join(",", item16), "排序提示形成环，已移除环内软排序约束并保留必需依赖。", XWValidationIssue.Severity.Warning, item16, item16));
			}
		}
		Dictionary<string, int> ranks = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (string item17 in (preferredOrder ?? Array.Empty<string>()).Select(CanonicalId))
		{
			if (!ranks.ContainsKey(item17))
			{
				ranks[item17] = ranks.Count;
			}
		}
		HashSet<(string From, string To)> edges = hard.Concat<(string, string)>(hashSet).ToHashSet();
		Dictionary<string, int> indegrees = candidates.ToDictionary<string, string, int>((string result) => result, (string text2) => edges.Count(((string From, string To) e) => e.To == text2), StringComparer.Ordinal);
		Comparer<string> comparer = Comparer<string>.Create(delegate(string a, string b)
		{
			int num2 = ranks.GetValueOrDefault(a, 2147483647).CompareTo(ranks.GetValueOrDefault(b, 2147483647));
			return (num2 == 0) ? StringComparer.Ordinal.Compare(a, b) : num2;
		});
		SortedSet<string> sortedSet = new SortedSet<string>(candidates.Where((string key3) => indegrees[key3] == 0), comparer);
		List<XWModManager.ModEntry> list2 = new List<XWModManager.ModEntry>();
		while (sortedSet.Count > 0)
		{
			string id2 = sortedSet.Min;
			sortedSet.Remove(id2);
			list2.Add(catalog[id2]);
			foreach (var item18 in edges.Where(((string From, string To) e) => e.From == id2))
			{
				if (--indegrees[item18.To] == 0)
				{
					sortedSet.Add(item18.To);
				}
			}
		}
		Dictionary<string, List<string>> overrides = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		foreach (XWModManager.ModEntry item19 in list2)
		{
			foreach (KeyValuePair<string, List<string>> item20 in item19.Manifest.Overrides ?? new Dictionary<string, List<string>>())
			{
				foreach (string item21 in item20.Value ?? new List<string>())
				{
					AddOverride("legacy/" + item20.Key.ToLowerInvariant() + "/" + item21, item19.Id);
				}
			}
			foreach (XWModContentEntry item22 in item19.Manifest.ContentEntries ?? new List<XWModContentEntry>())
			{
				if (item22?.Target != null)
				{
					AddOverride($"{CanonicalId(item22.Target.ProviderModId)}/{item22.Target.Category?.ToLowerInvariant()}/{item22.Target.Key}", item19.Id);
				}
			}
		}
		foreach (KeyValuePair<string, List<string>> item23 in overrides.Where((KeyValuePair<string, List<string>> p) => p.Value.Count > 1).OrderBy<KeyValuePair<string, List<string>>, string>((KeyValuePair<string, List<string>> p) => p.Key, StringComparer.Ordinal))
		{
			List<XWValidationIssue> list3 = issues;
			string key2 = item23.Key;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 2);
			defaultInterpolatedStringHandler.AppendLiteral("覆盖顺序：");
			defaultInterpolatedStringHandler.AppendFormatted(string.Join(" → ", item23.Value));
			defaultInterpolatedStringHandler.AppendLiteral("；生效者：");
			List<string> value4 = item23.Value;
			defaultInterpolatedStringHandler.AppendFormatted(value4[value4.Count - 1]);
			defaultInterpolatedStringHandler.AppendLiteral("。");
			list3.Add(Issue(XWValidationIssue.IssueCode.OverrideConflict, key2, defaultInterpolatedStringHandler.ToStringAndClear(), XWValidationIssue.Severity.Warning, item23.Value, item23.Value));
		}
		return new XWModLoadPlan(list2, issues, dictionary, dictionary2, enabled, metadata, array, availableCapabilities, list, assemblyIssues2, cached != null);
		void AddOverride(string key3, string owner)
		{
			if (!overrides.TryGetValue(key3, out var value5))
			{
				value5 = (overrides[key3] = new List<string>());
			}
			if (!value5.Contains<string>(owner, StringComparer.OrdinalIgnoreCase))
			{
				value5.Add(owner);
			}
		}
		bool Find(XWModDependency xWModDependency, out XWModManager.ModEntry reference)
		{
			reference = null;
			if (xWModDependency != null && enabled.Contains(CanonicalId(xWModDependency.Id)) && catalog.TryGetValue(CanonicalId(xWModDependency.Id), out reference))
			{
				return reference.Manifest != null;
			}
			return false;
		}
		void Reject(string text2, XWValidationIssue.IssueCode code, string message, IEnumerable<string> related = null)
		{
			invalid.Add(text2);
			issues.Add(Issue(code, text2, message, XWValidationIssue.Severity.Error, null, related));
		}
	}

	public static string CanonicalId(string value)
	{
		return (value ?? "").Trim().ToLowerInvariant();
	}

	private static string DependencyVersion(XWModDependency dependency)
	{
		if (string.IsNullOrWhiteSpace(dependency?.VersionRange))
		{
			if (string.IsNullOrWhiteSpace(dependency?.Version))
			{
				return "任意版本";
			}
			return dependency.Version;
		}
		return dependency.VersionRange;
	}

	private static XWValidationIssue Issue(XWValidationIssue.IssueCode code, string key, string message, XWValidationIssue.Severity severity = XWValidationIssue.Severity.Error, IEnumerable<string> affected = null, IEnumerable<string> related = null)
	{
		XWValidationIssue xWValidationIssue = new XWValidationIssue();
		xWValidationIssue.Code = code;
		xWValidationIssue.ResourceKey = key;
		xWValidationIssue.Message = message;
		xWValidationIssue.Level = severity;
		xWValidationIssue.AffectedModIds = (from id in (affected ?? new string[1] { key }).Select(CanonicalId)
			where id.Length > 0
			select id).Distinct().ToList();
		xWValidationIssue.RelatedModIds = (from id in (related ?? Array.Empty<string>()).Select(CanonicalId)
			where id.Length > 0
			select id).Distinct().ToList();
		return xWValidationIssue;
	}

	private static XWValidationIssue CloneIssue(XWValidationIssue i)
	{
		return new XWValidationIssue
		{
			Code = i.Code,
			Level = i.Level,
			Message = i.Message,
			ResourceKey = i.ResourceKey,
			FilePath = i.FilePath,
			Line = i.Line,
			Column = i.Column,
			JumpTarget = i.JumpTarget,
			AffectedModIds = new List<string>(i.AffectedModIds ?? new List<string>()),
			RelatedModIds = new List<string>(i.RelatedModIds ?? new List<string>())
		};
	}

	private static List<List<string>> Components(HashSet<string> nodes, IEnumerable<(string From, string To)> source)
	{
		Dictionary<string, List<string>> adjacency = nodes.ToDictionary<string, string, List<string>>((string n) => n, (string _) => new List<string>(), StringComparer.Ordinal);
		foreach (var item in source)
		{
			if (nodes.Contains(item.From) && nodes.Contains(item.To))
			{
				adjacency[item.From].Add(item.To);
			}
		}
		int next = 0;
		Dictionary<string, int> indexes = new Dictionary<string, int>(StringComparer.Ordinal);
		Dictionary<string, int> low = new Dictionary<string, int>(StringComparer.Ordinal);
		Stack<string> stack = new Stack<string>();
		HashSet<string> active = new HashSet<string>(StringComparer.Ordinal);
		List<List<string>> result = new List<List<string>>();
		foreach (string item2 in nodes.OrderBy<string, string>((string x) => x, StringComparer.Ordinal))
		{
			if (!indexes.ContainsKey(item2))
			{
				Visit(item2);
			}
		}
		return result;
		void Visit(string node)
		{
			Dictionary<string, int> dictionary = indexes;
			int value = (low[node] = next++);
			dictionary[node] = value;
			stack.Push(node);
			active.Add(node);
			foreach (string item3 in adjacency[node].OrderBy<string, string>((string x) => x, StringComparer.Ordinal))
			{
				if (!indexes.ContainsKey(item3))
				{
					Visit(item3);
					low[node] = Math.Min(low[node], low[item3]);
				}
				else if (active.Contains(item3))
				{
					low[node] = Math.Min(low[node], indexes[item3]);
				}
			}
			if (low[node] == indexes[node])
			{
				List<string> list = new List<string>();
				string text;
				do
				{
					text = stack.Pop();
					active.Remove(text);
					list.Add(text);
				}
				while (text != node);
				list.Sort(StringComparer.Ordinal);
				result.Add(list);
			}
		}
	}

	private static XWModManager.ModEntry CloneEntry(XWModManager.ModEntry e)
	{
		return new XWModManager.ModEntry
		{
			Id = e.Id,
			DisplayName = e.DisplayName,
			Version = e.Version,
			PackagePath = e.PackagePath,
			Enabled = e.Enabled,
			HasManifest = e.HasManifest,
			Manifest = CloneManifest(e.Manifest),
			Diagnostic = e.Diagnostic,
			Loaded = e.Loaded,
			PendingDelete = e.PendingDelete,
			PackageSha256 = e.PackageSha256,
			EffectiveState = e.EffectiveState,
			InstalledVersion = e.InstalledVersion,
			RunningVersion = e.RunningVersion,
			RunningSha256 = e.RunningSha256,
			EffectiveMode = e.EffectiveMode,
			LastApplyFailure = e.LastApplyFailure
		};
	}

	private static XWModManifest CloneManifest(XWModManifest m)
	{
		if (m == null)
		{
			return null;
		}
		return new XWModManifest
		{
			SchemaVersion = m.SchemaVersion,
			Id = m.Id,
			Name = m.Name,
			Version = m.Version,
			Author = m.Author,
			Description = m.Description,
			PlayInstructions = m.PlayInstructions,
			Dependencies = Dependencies(m.Dependencies),
			OptionalDependencies = Dependencies(m.OptionalDependencies),
			Conflicts = Dependencies(m.Conflicts),
			LoadBefore = Copy<string>(m.LoadBefore),
			LoadAfter = Copy<string>(m.LoadAfter),
			RequiredCapabilities = Copy<string>(m.RequiredCapabilities),
			SharedAssemblies = Copy<string>(m.SharedAssemblies),
			Provides = Map(m.Provides),
			Overrides = Map(m.Overrides),
			Scripts = Copy<string>(m.Scripts),
			RuntimeAssembly = m.RuntimeAssembly,
			RuntimeEntryType = m.RuntimeEntryType,
			RuntimeApiVersion = m.RuntimeApiVersion,
			RuntimeAssemblyPolicy = m.RuntimeAssemblyPolicy,
			Blueprints = Copy<string>(m.Blueprints),
			Translations = Copy<string>(m.Translations),
			Resources = Copy<string>(m.Resources),
			PrivateResources = Copy<string>(m.PrivateResources),
			LoadingPresentation = m.LoadingPresentation?.Clone(),
			ContentEntries = (m.ContentEntries ?? new List<XWModContentEntry>()).Select((XWModContentEntry c) => (c != null) ? new XWModContentEntry
			{
				Category = c.Category,
				Key = c.Key,
				Path = c.Path,
				Target = ((c.Target == null) ? null : new XWModContentTarget
				{
					ProviderModId = c.Target.ProviderModId,
					Category = c.Target.Category,
					Key = c.Target.Key
				})
			} : null).ToList(),
			Settings = (m.Settings ?? new List<XWModSettingDefinition>()).Select((XWModSettingDefinition s) => (s != null) ? new XWModSettingDefinition
			{
				Key = s.Key,
				Type = s.Type,
				Label = s.Label,
				Description = s.Description,
				Content = s.Content,
				Group = s.Group,
				Default = ((s.Default.ValueKind == JsonValueKind.Undefined) ? default(JsonElement) : s.Default.Clone()),
				Min = s.Min,
				Max = s.Max,
				Step = s.Step,
				MinSelected = s.MinSelected,
				MaxSelected = s.MaxSelected,
				ApplyMode = s.ApplyMode,
				AffectsGameplay = s.AffectsGameplay,
				Options = (s.Options ?? new List<XWModSettingOption>()).Select((XWModSettingOption o) => (o != null) ? new XWModSettingOption
				{
					Value = o.Value,
					Label = o.Label
				} : null).ToList()
			} : null).ToList()
		};
		static List<T> Copy<T>(List<T> values)
		{
			if (values != null)
			{
				return new List<T>(values);
			}
			return new List<T>();
		}
		static List<XWModDependency> Dependencies(List<XWModDependency> values)
		{
			return (values ?? new List<XWModDependency>()).Select((XWModDependency d) => (d != null) ? new XWModDependency
			{
				Id = d.Id,
				Version = d.Version,
				VersionRange = d.VersionRange
			} : null).ToList();
		}
		static Dictionary<string, List<string>> Map(Dictionary<string, List<string>> values)
		{
			return (values ?? new Dictionary<string, List<string>>()).ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> p) => p.Key, (KeyValuePair<string, List<string>> p) => Copy<string>(p.Value), StringComparer.Ordinal);
		}
	}

	public bool MatchesManifest(string ownerId, XWModManifest actual)
	{
		if (actual != null && CanonicalId(actual.Id) == CanonicalId(ownerId) && _manifestHashes.TryGetValue(CanonicalId(ownerId), out var value) && TryManifestHash(actual, out var digest))
		{
			return string.Equals(value, digest, StringComparison.Ordinal);
		}
		return false;
	}

	private static bool TryManifestHash(XWModManifest manifest, out string digest)
	{
		digest = "";
		if (manifest == null)
		{
			return false;
		}
		try
		{
			XWModManifest xWModManifest = CloneManifest(manifest);
			xWModManifest.Id = CanonicalId(xWModManifest.Id);
			using JsonDocument jsonDocument = JsonDocument.Parse("null");
			foreach (XWModSettingDefinition setting in xWModManifest.Settings)
			{
				if (setting != null && setting.Default.ValueKind == JsonValueKind.Undefined)
				{
					setting.Default = jsonDocument.RootElement.Clone();
				}
			}
			JsonElement value = JsonSerializer.SerializeToElement(xWModManifest, XWModJsonContext.Default.XWModManifest);
			using MemoryStream memoryStream = new MemoryStream();
			using (Utf8JsonWriter writer = new Utf8JsonWriter(memoryStream))
			{
				WriteCanonicalManifestValue(writer, value);
			}
			digest = Convert.ToHexString(SHA256.HashData(memoryStream.ToArray()));
			return true;
		}
		catch (Exception ex) when (((ex is JsonException || ex is InvalidOperationException || ex is ArgumentException || ex is NotSupportedException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	private static void WriteCanonicalManifestValue(Utf8JsonWriter writer, JsonElement value)
	{
		if (value.ValueKind == JsonValueKind.Object)
		{
			writer.WriteStartObject();
			foreach (JsonProperty item in value.EnumerateObject().OrderBy<JsonProperty, string>((JsonProperty property) => property.Name, StringComparer.Ordinal))
			{
				writer.WritePropertyName(item.Name);
				WriteCanonicalManifestValue(writer, item.Value);
			}
			writer.WriteEndObject();
		}
		else if (value.ValueKind == JsonValueKind.Array)
		{
			writer.WriteStartArray();
			foreach (JsonElement item2 in value.EnumerateArray())
			{
				WriteCanonicalManifestValue(writer, item2);
			}
			writer.WriteEndArray();
		}
		else
		{
			value.WriteTo(writer);
		}
	}

	private static IReadOnlyDictionary<string, IReadOnlyList<XWModAssemblyMetadata>> PreflightAssemblies(Dictionary<string, XWModManager.ModEntry> catalog, HashSet<string> invalid, IReadOnlyDictionary<string, string> suppliedHashes, List<XWValidationIssue> issues)
	{
		Dictionary<string, IReadOnlyList<XWModAssemblyMetadata>> dictionary = new Dictionary<string, IReadOnlyList<XWModAssemblyMetadata>>(StringComparer.Ordinal);
		Dictionary<string, List<(string, XWModAssemblyMetadata)>> dictionary2 = new Dictionary<string, List<(string, XWModAssemblyMetadata)>>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, XWModManager.ModEntry> item in catalog.OrderBy<KeyValuePair<string, XWModManager.ModEntry>, string>((KeyValuePair<string, XWModManager.ModEntry> p) => p.Key, StringComparer.Ordinal))
		{
			string owner = item.Key;
			XWModManager.ModEntry value = item.Value;
			if (value.Manifest == null || invalid.Contains(owner) || !XWModRuntimeCompatibility.HasManagedCode(value.Manifest) || (!File.Exists(value.PackagePath) && suppliedHashes != null && suppliedHashes.Keys.Any((string id) => CanonicalId(id) == owner)))
			{
				continue;
			}
			try
			{
				IReadOnlyDictionary<string, XWModAssemblyMetadata> readOnlyDictionary = XWModAssemblyMetadata.ReadPackage(value.PackagePath);
				dictionary[owner] = Array.AsReadOnly(readOnlyDictionary.Values.OrderBy<XWModAssemblyMetadata, string>((XWModAssemblyMetadata m) => m.SimpleName, StringComparer.Ordinal).ToArray());
				string mainPath = (value.Manifest.RuntimeAssembly ?? "").Replace('\\', '/');
				XWModAssemblyMetadata xWModAssemblyMetadata = readOnlyDictionary.Values.FirstOrDefault((XWModAssemblyMetadata m) => string.Equals(m.Path, mainPath, StringComparison.Ordinal));
				if (xWModAssemblyMetadata == null)
				{
					throw new InvalidDataException("声明的主程序集不存在：" + mainPath);
				}
				if (IsHostAssemblyName(xWModAssemblyMetadata.SimpleName))
				{
					throw new InvalidDataException("主程序集不得使用宿主程序集名称：" + xWModAssemblyMetadata.SimpleName);
				}
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string item2 in value.Manifest.SharedAssemblies ?? new List<string>())
				{
					if (string.IsNullOrWhiteSpace(item2) || item2 != item2.Trim() || item2.Contains('/') || item2.Contains('\\') || item2.Contains(','))
					{
						throw new InvalidDataException("共享程序集必须声明简单名称：" + item2);
					}
					if (IsHostAssemblyName(item2) || string.Equals(item2, xWModAssemblyMetadata.SimpleName, StringComparison.OrdinalIgnoreCase))
					{
						throw new InvalidDataException("主程序集和宿主程序集不能声明为共享依赖：" + item2);
					}
					if (!hashSet.Add(item2))
					{
						throw new InvalidDataException("重复声明共享程序集：" + item2);
					}
					if (!readOnlyDictionary.ContainsKey(item2))
					{
						throw new InvalidDataException("声明的共享程序集未打包：" + item2);
					}
				}
				Queue<(XWModAssemblyMetadata, bool)> queue = new Queue<(XWModAssemblyMetadata, bool)>();
				queue.Enqueue((xWModAssemblyMetadata, false));
				foreach (string item3 in hashSet.OrderBy<string, string>((string n) => n, StringComparer.Ordinal))
				{
					queue.Enqueue((readOnlyDictionary[item3], true));
				}
				HashSet<(string, bool)> hashSet2 = new HashSet<(string, bool)>();
				while (queue.Count > 0)
				{
					(XWModAssemblyMetadata, bool) tuple = queue.Dequeue();
					if (!hashSet2.Add((tuple.Item1.SimpleName.ToUpperInvariant(), tuple.Item2)))
					{
						continue;
					}
					foreach (string reference in tuple.Item1.References)
					{
						AssemblyName assemblyName = new AssemblyName(reference);
						if (!IsHostAssemblyName(assemblyName.Name))
						{
							if (!readOnlyDictionary.TryGetValue(assemblyName.Name ?? "", out var value2))
							{
								throw new InvalidDataException(tuple.Item1.SimpleName + " 缺少私有依赖：" + reference);
							}
							if (!XWModAssemblyMetadata.SameIdentity(assemblyName, new AssemblyName(value2.Identity)))
							{
								throw new InvalidDataException($"{tuple.Item1.SimpleName} 引用 {reference}，包内实际为 {value2.Identity}");
							}
							if (tuple.Item2 && !hashSet.Contains(value2.SimpleName))
							{
								throw new InvalidDataException($"共享程序集 {tuple.Item1.SimpleName} 的依赖 {value2.SimpleName} 也必须显式声明为共享。");
							}
							queue.Enqueue((value2, tuple.Item2 || hashSet.Contains(value2.SimpleName)));
						}
					}
				}
				foreach (string item4 in hashSet.OrderBy<string, string>((string n) => n, StringComparer.Ordinal))
				{
					if (!dictionary2.TryGetValue(item4, out var value3))
					{
						value3 = (dictionary2[item4] = new List<(string, XWModAssemblyMetadata)>());
					}
					value3.Add((owner, readOnlyDictionary[item4]));
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is BadImageFormatException) ? 1 : 0) != 0)
			{
				Fail(owner, ex.Message);
			}
		}
		foreach (KeyValuePair<string, List<(string, XWModAssemblyMetadata)>> item5 in dictionary2.OrderBy<KeyValuePair<string, List<(string, XWModAssemblyMetadata)>>, string>((KeyValuePair<string, List<(string Owner, XWModAssemblyMetadata Metadata)>> p) => p.Key, StringComparer.Ordinal))
		{
			XWModAssemblyMetadata baseline = item5.Value[0].Item2;
			if (item5.Value.All<(string, XWModAssemblyMetadata)>(((string Owner, XWModAssemblyMetadata Metadata) p) => XWModAssemblyMetadata.SameIdentity(new AssemblyName(baseline.Identity), new AssemblyName(p.Metadata.Identity)) && string.Equals(baseline.Sha256, p.Metadata.Sha256, StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}
			string value4 = string.Join(", ", item5.Value.Select<(string, XWModAssemblyMetadata), string>(((string Owner, XWModAssemblyMetadata Metadata) p) => p.Owner));
			foreach (var item6 in item5.Value)
			{
				Fail(item6.Item1, $"共享程序集 {item5.Key} 在 Mod [{value4}] 中的身份或字节不一致", item5.Value.Select<(string, XWModAssemblyMetadata), string>(((string Owner, XWModAssemblyMetadata Metadata) p) => p.Owner));
			}
		}
		return new ReadOnlyDictionary<string, IReadOnlyList<XWModAssemblyMetadata>>(dictionary);
		void Fail(string text, string message, IEnumerable<string> related = null)
		{
			bool flag = catalog[text].Manifest.IsRuntimeAssemblyRequired();
			if (flag)
			{
				invalid.Add(text);
			}
			issues.Add(Issue(XWValidationIssue.IssueCode.RuntimeLoadFailed, text, "程序集预检失败：" + message + (flag ? "" : "；运行时为可选，保留资源回退。"), (!flag) ? XWValidationIssue.Severity.Warning : XWValidationIssue.Severity.Error, null, related));
		}
	}

	private static bool IsHostAssemblyName(string name)
	{
		if (name != null)
		{
			if (!name.Equals("PlantsVsZombies", StringComparison.OrdinalIgnoreCase) && !name.Equals("GodotSharp", StringComparison.OrdinalIgnoreCase) && !name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) && !name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) && !name.Equals("System", StringComparison.OrdinalIgnoreCase) && !name.Equals("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
			{
				return name.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}
}
