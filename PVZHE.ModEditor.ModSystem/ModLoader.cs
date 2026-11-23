using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Godot;
using Godot.Collections;

namespace PVZHE.ModEditor.ModSystem;

public static class ModLoader
{
	public enum ApplicationState
	{
		Extracted,
		Applying,
		Applied,
		Failed,
		PendingRollback,
		Unloaded
	}

	public enum EffectiveRuntimeMode
	{
		ResourceOnly,
		Managed,
		OptionalResourceFallback
	}

	public class LoadedMod
	{
		public ModExporter.ModInfo Info;

		public XWModManifest Manifest;

		public string FilePath = "";

		public string ExtractRoot = "";

		public List<string> ExtractedFiles = new List<string>();

		public List<string> RelativeFiles = new List<string>();

		public List<string> Diagnostics = new List<string>();

		public System.Collections.Generic.Dictionary<string, Resource> RuntimeResourceCache = new System.Collections.Generic.Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);

		public XWModCharacterCompanionRuntime CharacterCompanionRuntime;

		public string RuntimeAssemblyPath = "";

		private XWModSettings _settings;

		public int RejectedFileCount;

		public int AppliedResourceCount;

		public int CompanionBoundResourceCount;

		private WeakReference _unloadedContext;

		internal long LoadingSourceBytes;

		internal long LoadingTexturePixels;

		public long CachedRuntimeSourceBytes { get; private set; }

		public long CachedRuntimeTexturePixels { get; private set; }

		public long CachedRuntimeDecodedAudioBytes { get; private set; }

		public bool RuntimeEntryInitialized => CharacterCompanionRuntime?.RuntimeEntryInitialized ?? false;

		public int StateMachineCallbackCount => CharacterCompanionRuntime?.StateMachineCallbackCount ?? 0;

		public int CharacterComponentRuntimeCount => CharacterCompanionRuntime?.CharacterComponentRuntimeCount ?? 0;

		public ApplicationState State { get; internal set; }

		public EffectiveRuntimeMode EffectiveMode { get; internal set; }

		public string PackageSha256 { get; private set; } = "";

		public string LoadedId { get; private set; } = "";

		public string LoadedVersion { get; private set; } = "";

		public int LoadedRuntimeApiVersion { get; private set; }

		public string LoadedPackagePath { get; private set; } = "";

		public string RunningPackagePath { get; internal set; } = "";

		public bool RuntimeAssemblyUnloadRequested { get; private set; }

		public bool RequiresRestart { get; private set; }

		public bool? UnloadedContextCollected
		{
			get
			{
				if (_unloadedContext != null)
				{
					return !_unloadedContext.IsAlive;
				}
				return null;
			}
		}

		public bool Applied => State == ApplicationState.Applied;

		internal bool HasAppliedContent
		{
			get
			{
				if (AppliedResourceCount <= 0 && LoadingScenes.Count <= 0 && !RuntimeEntryInitialized && StateMachineCallbackCount <= 0)
				{
					return CharacterComponentRuntimeCount > 0;
				}
				return true;
			}
		}

		internal HashSet<string> LoadingNativeAssets { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		internal HashSet<string> LoadingAccountedFiles { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		internal System.Collections.Generic.Dictionary<(string Path, XWLoadingKind Kind), PackedScene> LoadingScenes { get; } = new System.Collections.Generic.Dictionary<(string, XWLoadingKind), PackedScene>();

		internal void ActivateSettings()
		{
			_settings = XWModSettingsService.Activate(Manifest, PackageSha256);
		}

		internal void RecordIdentity(string hash)
		{
			PackageSha256 = hash;
			LoadedId = Manifest.Id.Trim().ToLowerInvariant();
			LoadedVersion = Manifest.Version ?? "";
			LoadedRuntimeApiVersion = Manifest.RuntimeApiVersion;
			LoadedPackagePath = Path.GetFullPath(FilePath);
		}

		public void ClearRuntimeResourceCache()
		{
			RuntimeResourceCache.Clear();
			CachedRuntimeSourceBytes = 0L;
			CachedRuntimeTexturePixels = 0L;
			CachedRuntimeDecodedAudioBytes = 0L;
		}

		public bool UnloadRuntimeAssembly()
		{
			StateMachineUnloadBlockers blockers;
			return UnloadRuntimeAssembly(out blockers);
		}

		public bool CanUnloadRuntimeAssembly(out StateMachineUnloadBlockers blockers)
		{
			blockers = new StateMachineUnloadBlockers(0, System.Array.Empty<StateMachineUnloadBlocker>());
			if (CharacterCompanionRuntime != null)
			{
				return CharacterCompanionRuntime.CanUnload(out blockers);
			}
			return true;
		}

		public bool UnloadRuntimeAssembly(out StateMachineUnloadBlockers blockers)
		{
			blockers = new StateMachineUnloadBlockers(0, System.Array.Empty<StateMachineUnloadBlocker>());
			if (CharacterCompanionRuntime != null && !CharacterCompanionRuntime.TryUnload(out blockers))
			{
				return false;
			}
			if (CharacterCompanionRuntime != null)
			{
				RuntimeAssemblyUnloadRequested = CharacterCompanionRuntime.UnloadRequested;
				RequiresRestart = CharacterCompanionRuntime.RequiresRestart;
				_unloadedContext = CharacterCompanionRuntime.LastUnloadContext;
			}
			CharacterCompanionRuntime = null;
			if (_settings != null)
			{
				XWModSettingsService.Deactivate(_settings);
				_settings = null;
			}
			CompanionBoundResourceCount = 0;
			return true;
		}

		public bool NotifyAllModsLoaded()
		{
			if (CharacterCompanionRuntime == null)
			{
				return true;
			}
			if (CharacterCompanionRuntime.NotifyAllModsLoaded(out var diagnostic))
			{
				return true;
			}
			AddDiagnostic(this, "OnAllModsLoaded 失败：" + diagnostic);
			State = ApplicationState.Failed;
			XWModEnvironmentService.Invalidate("LifecycleFailed");
			return false;
		}

		internal void PrepareLifecycleRollback()
		{
			CharacterCompanionRuntime?.PrepareLifecycleRollback();
		}

		internal void AccountCachedRuntimeResource(long sourceBytes, long texturePixels, long decodedAudioBytes)
		{
			CachedRuntimeSourceBytes += sourceBytes;
			CachedRuntimeTexturePixels += texturePixels;
			CachedRuntimeDecodedAudioBytes += decodedAudioBytes;
		}
	}

	private sealed class RuntimeCandidate
	{
		public string RelativePath = "";

		public string ExtractedPath = "";

		public string Category = "";

		public string InferredKey = "";
	}

	private sealed class RuntimeBinding
	{
		public RuntimeCandidate Candidate;

		public string Key = "";

		public bool AllowOverride;

		public bool Required;

		public XWModContentEntry ContentEntry;
	}

	private const long MaxEntryBytes = 67108864L;

	private const long MaxArchiveBytes = 536870912L;

	private const int MaxArchiveEntries = 4096;

	private const int MaxRuntimeCacheEntriesPerMod = 512;

	private const int MaxRuntimeCacheEntriesTotal = 4096;

	private const long MaxRuntimeSourceBytesPerMod = 268435456L;

	private const long MaxRuntimeSourceBytesTotal = 536870912L;

	private const long MaxRuntimeTexturePixelsPerMod = 67108864L;

	private const long MaxRuntimeTexturePixelsTotal = 134217728L;

	private const long MaxRuntimeDecodedAudioBytesPerMod = 134217728L;

	private const long MaxRuntimeDecodedAudioBytesTotal = 268435456L;

	private static readonly List<LoadedMod> LoadedMods = new List<LoadedMod>();

	private static bool _coldCachePrepared;

	private static bool _runtimeCacheTouched;

	private static System.Threading.Mutex _cacheLifetimeMutex;

	private static bool _ownsCacheLifetimeMutex;

	private static FileStream _cacheProcessLease;

	private const string CacheSessionMarker = ".cache-session";

	private static readonly HashSet<string> CompanionOwners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly List<WeakReference<LoadedMod>> LoadingAssetOwners = new List<WeakReference<LoadedMod>>();

	public static int LoadAll()
	{
		return new XWModManager().LoadEnabledModsWithResult(recoverStorage: true);
	}

	public static XWModStartupReport LoadAllDetailed()
	{
		return new XWModManager().LoadEnabledModsDetailed(recoverStorage: true);
	}

	public static bool TryReadPackageMetadata(string pmodPath, out XWModManifest manifest, out ModExporter.ModInfo info, out string diagnostic)
	{
		return ValidatePackageArchive(pmodPath, out manifest, out info, out diagnostic);
	}

	public static bool ValidatePackageArchive(string pmodPath, out XWModManifest manifest, out ModExporter.ModInfo info, out string diagnostic)
	{
		XWModPackageMetadataPreview preview;
		return TryReadPackageMetadata(pmodPath, out manifest, out info, out diagnostic, out preview);
	}

	internal static bool TryReadPackageMetadata(string pmodPath, out XWModManifest manifest, out ModExporter.ModInfo info, out string diagnostic, out XWModPackageMetadataPreview preview)
	{
		manifest = null;
		info = null;
		diagnostic = "";
		preview = null;
		try
		{
			if (string.IsNullOrWhiteSpace(pmodPath) || !File.Exists(pmodPath))
			{
				throw new FileNotFoundException("Mod package was not found.", pmodPath);
			}
			using FileStream stream = File.OpenRead(pmodPath);
			using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
			if (zipArchive.Entries.Count > 4096)
			{
				throw new InvalidDataException($"archive entry limit exceeded: {zipArchive.Entries.Count} > {4096}");
			}
			string extractRoot = Path.Combine(Path.GetTempPath(), "PVZHE-PmodValidation");
			long num = 0L;
			List<string> relativeEntries = new List<string>(zipArchive.Entries.Count);
			List<string> list = new List<string>(zipArchive.Entries.Count);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (!ValidateArchiveEntryPath(extractRoot, entry.FullName, out var relativePath, out var _))
				{
					throw new InvalidDataException("unsafe archive entry: " + entry.FullName);
				}
				relativeEntries.Add(relativePath);
				if (!hashSet.Add(relativePath))
				{
					throw new InvalidDataException("duplicate archive entry: " + relativePath);
				}
				if (!string.IsNullOrEmpty(entry.Name))
				{
					list.Add(relativePath);
					if (entry.Length > 67108864 || num + entry.Length > 536870912)
					{
						throw new InvalidDataException("archive size limit exceeded by: " + relativePath);
					}
					num += entry.Length;
				}
			}
			ZipArchiveEntry[] array = zipArchive.Entries.Where((ZipArchiveEntry entry, int index) => string.Equals(relativeEntries[index], "mod.json", StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array.Length != 1)
			{
				throw new InvalidDataException("package must contain exactly one root mod.json");
			}
			if (array[0].Length > 1048576)
			{
				throw new InvalidDataException("mod.json exceeds the 1 MiB limit");
			}
			using Stream stream2 = array[0].Open();
			using StreamReader streamReader = new StreamReader(stream2);
			ParseManifest(streamReader.ReadToEnd(), out manifest, out info, out preview);
			NormalizeManifest(manifest, info, pmodPath);
			if (manifest == null)
			{
				manifest = CreateLegacyManifest(info, pmodPath);
			}
			ValidateDeclaredPackageExecutables(manifest, relativeEntries);
			ValidateContentEntries(manifest, relativeEntries);
			ValidatePrivateResources(manifest, list);
			ValidateLoadingPresentation(manifest, list);
			return manifest != null;
		}
		catch (Exception ex)
		{
			diagnostic = ex.GetBaseException().Message;
			manifest = null;
			info = null;
			return false;
		}
	}

	private static void ValidateDeclaredPackageExecutables(XWModManifest manifest, IReadOnlyList<string> relativeEntries)
	{
		string text = (manifest?.RuntimeAssembly ?? "").Replace('\\', '/').Trim('/');
		if (!string.IsNullOrWhiteSpace(text) && !text.Equals("Runtime/ModAssembly.dll", StringComparison.Ordinal))
		{
			throw new InvalidDataException("invalid declared runtime assembly path: " + text);
		}
		bool flag = false;
		foreach (string relativeEntry in relativeEntries)
		{
			if (!string.IsNullOrWhiteSpace(relativeEntry))
			{
				if (string.Equals(relativeEntry, text, StringComparison.Ordinal))
				{
					flag = true;
				}
				if (IsExecutablePackageFile(relativeEntry) && !string.Equals(relativeEntry, text, StringComparison.Ordinal) && !IsRuntimeDependencyFile(relativeEntry))
				{
					throw new InvalidDataException("undeclared executable package file: " + relativeEntry);
				}
			}
		}
		if (!string.IsNullOrWhiteSpace(text) && manifest.IsRuntimeAssemblyRequired() && !flag)
		{
			throw new InvalidDataException("declared runtime assembly is missing: " + text);
		}
	}

	private static XWModContentId ContentTarget(XWModContentTarget target)
	{
		return new XWModContentId(target.ProviderModId, target.Category, target.Key);
	}

	private static string BindingRuntimeKey(string owner, RuntimeBinding binding)
	{
		if (binding.ContentEntry != null)
		{
			if (binding.ContentEntry.Target != null)
			{
				return ContentTarget(binding.ContentEntry.Target).RuntimeKey;
			}
			return new XWModContentId(owner, binding.Candidate.Category, binding.Key).RuntimeKey;
		}
		return binding.Key;
	}

	private static string BindingResourceKey(string owner, RuntimeBinding binding)
	{
		if (binding.ContentEntry?.Target != null)
		{
			return XWModRuntimeRegistry.GetResourceKey(ContentTarget(binding.ContentEntry.Target));
		}
		return BindingRuntimeKey(owner, binding);
	}

	private static void ValidateContentEntries(XWModManifest manifest, IReadOnlyList<string> paths)
	{
		if (manifest == null)
		{
			return;
		}
		if (manifest.ContentEntries == null)
		{
			List<XWModContentEntry> list = (manifest.ContentEntries = new List<XWModContentEntry>());
		}
		if (manifest.ContentEntries.Count > 0 && manifest.SchemaVersion < 3)
		{
			throw new InvalidDataException("contentEntries requires schemaVersion 3");
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.Ordinal);
		foreach (XWModContentEntry contentEntry in manifest.ContentEntries)
		{
			if (contentEntry == null)
			{
				throw new InvalidDataException("contentEntries cannot contain null");
			}
			contentEntry.Category = NormalizeCategory(contentEntry.Category);
			contentEntry.Key = (contentEntry.Key ?? "").Trim();
			contentEntry.Path = (contentEntry.Path ?? "").Replace('\\', '/').Trim();
			if (!XWModContentId.TryCreate(manifest.Id, contentEntry.Category, contentEntry.Key, out var _) || !hashSet.Add(contentEntry.Category + "\n" + contentEntry.Key) || ManifestContains(manifest.Provides, contentEntry.Category, contentEntry.Key) || ManifestContains(manifest.Overrides, contentEntry.Category, contentEntry.Key))
			{
				throw new InvalidDataException("invalid or duplicate content identity: " + contentEntry.Category + "/" + contentEntry.Key);
			}
			if (contentEntry.Path.Length > 0 && (!ValidateArchiveEntryPath(Path.GetTempPath(), contentEntry.Path, out var relativePath, out var _) || relativePath != contentEntry.Path || !paths.Contains<string>(contentEntry.Path, StringComparer.Ordinal) || IsExecutablePackageFile(contentEntry.Path) || !hashSet2.Add(contentEntry.Path)))
			{
				throw new InvalidDataException("invalid, missing or multiply bound content path: " + contentEntry.Path);
			}
			if (contentEntry.Target != null)
			{
				contentEntry.Target.Category = NormalizeCategory(contentEntry.Target.Category);
				contentEntry.Target.Key = (contentEntry.Target.Key ?? "").Trim();
				contentEntry.Target.ProviderModId = (contentEntry.Target.ProviderModId ?? "").Trim().ToLowerInvariant();
				if (!ContentTarget(contentEntry.Target).IsValid || contentEntry.Target.Category != contentEntry.Category)
				{
					throw new InvalidDataException("invalid content override target: " + contentEntry.Category + "/" + contentEntry.Key);
				}
			}
		}
	}

	private static HashSet<string> ValidatePrivateResources(XWModManifest manifest, IReadOnlyList<string> paths)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (manifest == null)
		{
			return hashSet;
		}
		if (manifest.PrivateResources == null)
		{
			List<string> list = (manifest.PrivateResources = new List<string>());
		}
		if (manifest.PrivateResources.Count == 0)
		{
			return hashSet;
		}
		if (manifest.SchemaVersion < 3)
		{
			throw new InvalidDataException("privateResources requires schemaVersion 3");
		}
		HashSet<string> hashSet2 = new HashSet<string>(paths, StringComparer.Ordinal);
		HashSet<string> hashSet3 = new HashSet<string>(from entry in manifest.ContentEntries ?? new List<XWModContentEntry>()
			where entry != null
			select (entry.Path ?? "").Replace('\\', '/').Trim(), StringComparer.OrdinalIgnoreCase);
		for (int num = 0; num < manifest.PrivateResources.Count; num++)
		{
			string text = (manifest.PrivateResources[num] ?? "").Replace('\\', '/').Trim();
			if (!ValidateArchiveEntryPath(Path.GetTempPath(), text, out var relativePath, out var _) || relativePath != text)
			{
				throw new InvalidDataException("invalid private resource path: " + text);
			}
			if (!hashSet2.Contains(text))
			{
				throw new InvalidDataException("missing or case-mismatched private resource: " + text);
			}
			if (!hashSet.Add(text))
			{
				throw new InvalidDataException("duplicate private resource path: " + text);
			}
			if (hashSet3.Contains(text))
			{
				throw new InvalidDataException("private resource conflicts with contentEntries: " + text);
			}
			if (IsExecutablePackageFile(text) || text.StartsWith("Runtime/", StringComparison.OrdinalIgnoreCase) || IsCharacterPackageDependency(text) || !IsSupportedPrivateResource(text))
			{
				throw new InvalidDataException("unsupported private resource path: " + text);
			}
			manifest.PrivateResources[num] = text;
		}
		return hashSet;
	}

	private static bool IsSupportedPrivateResource(string relativePath)
	{
		string text = Path.GetExtension(relativePath).ToLowerInvariant();
		if (text != null)
		{
			int length = text.Length;
			if (length != 4)
			{
				if (length == 5)
				{
					char c = text[2];
					if (c != 'l')
					{
						if (c != 'r')
						{
							if (c == 's' && text == ".tscn")
							{
								goto IL_00f6;
							}
						}
						else if (text == ".tres")
						{
							goto IL_00f6;
						}
					}
					else if (text == ".flac")
					{
						goto IL_00f6;
					}
				}
			}
			else
			{
				char c = text[1];
				if ((uint)c <= 111u)
				{
					if (c != 'm')
					{
						if (c == 'o' && (text == ".otf" || text == ".ogg"))
						{
							goto IL_00f6;
						}
					}
					else if (text == ".mp3")
					{
						goto IL_00f6;
					}
				}
				else if (c != 'r')
				{
					if (c != 't')
					{
						if (c == 'w' && text == ".wav")
						{
							goto IL_00f6;
						}
					}
					else if (text == ".ttf")
					{
						goto IL_00f6;
					}
				}
				else if (text == ".res")
				{
					goto IL_00f6;
				}
			}
		}
		bool flag = false;
		goto IL_00fc;
		IL_00fc:
		if (!flag)
		{
			return XWModExternalMediaLoader.IsSupportedTextureExtension(text);
		}
		return true;
		IL_00f6:
		flag = true;
		goto IL_00fc;
	}

	private static void PreparePackageResourceReferences(LoadedMod mod)
	{
		for (int i = 0; i < mod.RelativeFiles.Count; i++)
		{
			string text = Path.GetExtension(mod.RelativeFiles[i]).ToLowerInvariant();
			if ((!(text == ".tscn") && !(text == ".tres")) || 1 == 0)
			{
				continue;
			}
			string text2 = mod.ExtractedFiles[i];
			string[] array = File.ReadAllLines(text2);
			bool flag = false;
			for (int j = 0; j < array.Length; j++)
			{
				string text3 = array[j];
				string text4 = text3.TrimStart();
				if (text4.StartsWith("[gd_resource", StringComparison.Ordinal) || text4.StartsWith("[gd_scene", StringComparison.Ordinal))
				{
					string text5 = Regex.Replace(text3, "\\s+uid=\"[^\"]*\"", "");
					flag |= text5 != text3;
					array[j] = text5;
				}
				else
				{
					if (!text4.StartsWith("[ext_resource", StringComparison.Ordinal))
					{
						continue;
					}
					string text6 = ExtractAttribute(text3, "path");
					string text7 = ExtractAttribute(text3, "type");
					if (text6.Length == 0)
					{
						throw new InvalidDataException($"UID-only resource reference has no portable path: {mod.RelativeFiles[i]}:{j + 1}");
					}
					string extractedResource;
					bool num = text7 != "Script" && TryResolvePackagedResource(mod, text2, text6, out extractedResource);
					string resourcePath = text6;
					if (num)
					{
						TryResolvePackagedResource(mod, text2, text6, out var extractedResource2);
						if (!TryGetGodotResourcePath(extractedResource2, out resourcePath))
						{
							throw new InvalidDataException("cannot localize packaged resource: " + text6);
						}
					}
					else if (!text6.StartsWith("res://", StringComparison.Ordinal) || !ResourceLoader.Exists(text6))
					{
						if (!(text7 == "Script") || !(text == ".tscn") || !IsCharacterPackageDependency(mod.RelativeFiles[i]))
						{
							throw new InvalidDataException("missing or nonportable resource reference: " + mod.RelativeFiles[i] + ": " + text6);
						}
						continue;
					}
					string input = text3.Replace("path=\"" + text6 + "\"", "path=\"" + resourcePath + "\"", StringComparison.Ordinal);
					input = Regex.Replace(input, "\\s+uid=\"[^\"]*\"", "");
					flag |= input != text3;
					array[j] = input;
				}
			}
			if (flag)
			{
				File.WriteAllLines(text2, array);
			}
		}
	}

	public static void PrepareColdStartCache()
	{
		if (_coldCachePrepared)
		{
			return;
		}
		_coldCachePrepared = true;
		if (_runtimeCacheTouched || LoadedMods.Count > 0 || XWModRuntimeRegistry.GetRegistrations().Count > 0)
		{
			GD.PushWarning("[ModLoader] cache cleanup skipped: runtime references already exist");
			return;
		}
		string fullPath = Path.GetFullPath(ProjectSettings.GlobalizePath("user://ModsCache/"));
		try
		{
			string text = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(OperatingSystem.IsWindows() ? fullPath.ToUpperInvariant() : fullPath)));
			_cacheLifetimeMutex = new System.Threading.Mutex(initiallyOwned: false, "PVZHE-ModCache-" + text);
			try
			{
				_ownsCacheLifetimeMutex = _cacheLifetimeMutex.WaitOne(0);
			}
			catch (AbandonedMutexException)
			{
				_ownsCacheLifetimeMutex = true;
			}
			if (!_ownsCacheLifetimeMutex)
			{
				GD.PushWarning("[ModLoader] cache cleanup skipped: another process owns this cache");
				return;
			}
			Directory.CreateDirectory(fullPath);
			if (HasReparsePoint(fullPath))
			{
				return;
			}
			EnsureCacheProcessLease(fullPath);
			Guid result;
			foreach (string item in Directory.EnumerateFiles(fullPath, ".active-*.lock"))
			{
				if (item == _cacheProcessLease?.Name)
				{
					continue;
				}
				string fileName = Path.GetFileName(item);
				if (HasReparsePoint(item))
				{
					continue;
				}
				string text2 = fileName;
				if (!Guid.TryParseExact(text2.Substring(8, text2.Length - 5 - 8), "N", out result))
				{
					continue;
				}
				try
				{
					using (new FileStream(item, FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.None))
					{
					}
				}
				catch (IOException)
				{
					GD.PushWarning("[ModLoader] cache cleanup skipped: another process has runtime cache references");
					return;
				}
				File.Delete(item);
			}
			foreach (string item2 in Directory.EnumerateDirectories(fullPath))
			{
				if (HasReparsePoint(item2))
				{
					continue;
				}
				List<DirectoryInfo> list = new List<DirectoryInfo>();
				foreach (string item3 in Directory.EnumerateDirectories(item2))
				{
					if (HasReparsePoint(item3) || !Regex.IsMatch(Path.GetFileName(item3), "^[a-f0-9]{64}$"))
					{
						continue;
					}
					foreach (string item4 in Directory.EnumerateDirectories(item3))
					{
						if (!HasReparsePoint(item4) && Guid.TryParseExact(Path.GetFileName(item4), "N", out result) && IsRecognizedCacheSession(item4, item2, Path.GetFileName(item3)))
						{
							list.Add(new DirectoryInfo(item4));
						}
					}
				}
				foreach (DirectoryInfo item5 in list.OrderByDescending((DirectoryInfo directory) => directory.CreationTimeUtc).ThenByDescending<DirectoryInfo, string>((DirectoryInfo directory) => directory.Name, StringComparer.Ordinal).Skip(2))
				{
					try
					{
						string fullPath2 = Path.GetFullPath(item5.FullName);
						if (fullPath2.StartsWith(fullPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && IsRecognizedCacheSession(fullPath2, item2, Path.GetFileName(item5.Parent.FullName)))
						{
							Directory.Delete(fullPath2, recursive: true);
							if (!Directory.EnumerateFileSystemEntries(item5.Parent.FullName).Any())
							{
								Directory.Delete(item5.Parent.FullName);
							}
						}
					}
					catch (Exception ex3)
					{
						GD.PushWarning("[ModLoader] old cache retained: " + item5.Name + ": " + ex3.Message);
					}
				}
			}
		}
		catch (Exception ex4)
		{
			GD.PushWarning("[ModLoader] cache cleanup skipped: " + ex4.Message);
		}
		finally
		{
			if (_ownsCacheLifetimeMutex)
			{
				_cacheLifetimeMutex.ReleaseMutex();
				_ownsCacheLifetimeMutex = false;
			}
			_cacheLifetimeMutex?.Dispose();
			_cacheLifetimeMutex = null;
		}
	}

	private static void EnsureCacheProcessLease(string root)
	{
		if (_cacheProcessLease == null)
		{
			Directory.CreateDirectory(root);
			_cacheProcessLease = new FileStream(Path.Combine(root, $".active-{Guid.NewGuid():N}.lock"), FileMode.CreateNew, System.IO.FileAccess.ReadWrite, FileShare.None);
		}
	}

	private static bool HasReparsePoint(string path)
	{
		return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
	}

	private static bool IsRecognizedCacheSession(string session, string ownerDirectory, string hash)
	{
		try
		{
			string text = Path.Combine(session, ".cache-session");
			string path = Path.Combine(session, "source.pmod");
			if (!File.Exists(text) || !File.Exists(path) || HasReparsePoint(text) || HasReparsePoint(path) || new FileInfo(text).Length > 4096)
			{
				return false;
			}
			string[] array = File.ReadAllLines(text);
			if (array.Length != 3 || array[0] != "pvzhe-mod-session-v1" || array[2] != hash || SanitizeCacheName(array[1]) != Path.GetFileName(ownerDirectory))
			{
				return false;
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cache-session", "source.pmod" };
			HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "content" };
			using FileStream stream = File.OpenRead(path);
			using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
			if (zipArchive.Entries.Count > 4096)
			{
				return false;
			}
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (!ValidateArchiveEntryPath(Path.Combine(session, "content"), entry.FullName, out var relativePath, out var _))
				{
					return false;
				}
				string text2 = "content/" + relativePath;
				if (entry.Name.Length > 0)
				{
					hashSet.Add(text2);
				}
				else
				{
					hashSet2.Add(text2);
				}
				for (int num = text2.LastIndexOf('/'); num > 0; num = text2.LastIndexOf('/', num - 1))
				{
					hashSet2.Add(text2.Substring(0, num));
				}
			}
			Stack<string> stack = new Stack<string>();
			stack.Push(session);
			while (stack.Count > 0)
			{
				foreach (string item2 in Directory.EnumerateFileSystemEntries(stack.Pop()))
				{
					if (HasReparsePoint(item2))
					{
						return false;
					}
					string item = Path.GetRelativePath(session, item2).Replace('\\', '/');
					if (Directory.Exists(item2))
					{
						if (!hashSet2.Contains(item))
						{
							return false;
						}
						stack.Push(item2);
					}
					else if (!hashSet.Contains(item))
					{
						return false;
					}
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static LoadedMod LoadMod(string pmodPath)
	{
		_runtimeCacheTouched = true;
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return null;
		}
		if (!ValidatePackageArchive(pmodPath, out var manifest, out var _, out var diagnostic))
		{
			GD.PushWarning("[ModLoader] package rejected: " + pmodPath + ": " + diagnostic);
			return null;
		}
		if (string.IsNullOrWhiteSpace(pmodPath) || !File.Exists(pmodPath))
		{
			GD.PrintErr("[ModLoader] package does not exist: " + pmodPath);
			return null;
		}
		string fullPath = Path.GetFullPath(ProjectSettings.GlobalizePath("user://ModsCache/"));
		string text = "";
		LoadedMod loadedMod = new LoadedMod
		{
			FilePath = pmodPath
		};
		try
		{
			EnsureCacheProcessLease(fullPath);
			using FileStream fileStream = File.OpenRead(pmodPath);
			string text2 = Convert.ToHexString(SHA256.HashData(fileStream)).ToLowerInvariant();
			fileStream.Position = 0L;
			string path = SanitizeCacheName(manifest.Id.Trim().ToLowerInvariant());
			text = Path.GetFullPath(Path.Combine(fullPath, path, text2, Guid.NewGuid().ToString("N")));
			string value = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			if (!text.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("unsafe cache destination");
			}
			Directory.CreateDirectory(text);
			string text3 = Path.Combine(text, "source.pmod");
			using (FileStream destination = new FileStream(text3, FileMode.CreateNew, System.IO.FileAccess.Write, FileShare.None))
			{
				fileStream.CopyTo(destination);
			}
			File.WriteAllLines(Path.Combine(text, ".cache-session"), new string[3]
			{
				"pvzhe-mod-session-v1",
				manifest.Id.Trim().ToLowerInvariant(),
				text2
			});
			loadedMod.RunningPackagePath = text3;
			loadedMod.ExtractRoot = Path.Combine(text, "content");
			string extractRoot = loadedMod.ExtractRoot;
			Directory.CreateDirectory(extractRoot);
			using FileStream fileStream2 = File.OpenRead(text3);
			if (!Convert.ToHexString(SHA256.HashData(fileStream2)).Equals(text2, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("package changed while creating runtime snapshot");
			}
			fileStream2.Position = 0L;
			long num = 0L;
			using ZipArchive zipArchive = new ZipArchive(fileStream2, ZipArchiveMode.Read);
			if (zipArchive.Entries.Count > 4096)
			{
				throw new InvalidDataException($"archive entry limit exceeded: {zipArchive.Entries.Count} > {4096}");
			}
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (!ValidateArchiveEntryPath(extractRoot, entry.FullName, out var relativePath, out var outputPath))
				{
					throw new InvalidDataException("unsafe archive entry: " + entry.FullName);
				}
				if (string.IsNullOrEmpty(entry.Name))
				{
					Directory.CreateDirectory(outputPath);
					continue;
				}
				if (entry.Length > 67108864 || num + entry.Length > 536870912)
				{
					throw new InvalidDataException("archive size limit exceeded by: " + relativePath);
				}
				num += entry.Length;
				Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? extractRoot);
				if (relativePath.Equals("mod.json", StringComparison.OrdinalIgnoreCase))
				{
					using Stream stream = entry.Open();
					using StreamReader streamReader = new StreamReader(stream);
					string text4 = streamReader.ReadToEnd();
					ParseManifest(text4, loadedMod);
					File.WriteAllText(outputPath, text4);
				}
				else
				{
					entry.ExtractToFile(outputPath, overwrite: false);
					loadedMod.RelativeFiles.Add(relativePath);
					loadedMod.ExtractedFiles.Add(outputPath);
				}
			}
			if (loadedMod.Manifest == null && loadedMod.Info == null)
			{
				AddDiagnostic(loadedMod, "missing or invalid root mod.json");
				GD.PushWarning("[ModLoader] package rejected: " + pmodPath + ": missing or invalid root mod.json");
				return null;
			}
			NormalizeManifest(loadedMod.Manifest, loadedMod.Info, pmodPath);
			LoadedMod loadedMod2 = loadedMod;
			if (loadedMod2.Manifest == null)
			{
				loadedMod2.Manifest = CreateLegacyManifest(loadedMod.Info, pmodPath);
			}
			if (!string.Equals(loadedMod.Manifest.Id, manifest.Id, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("package identity changed after preflight");
			}
			ValidateDeclaredPackageExecutables(loadedMod.Manifest, loadedMod.RelativeFiles);
			ValidateContentEntries(loadedMod.Manifest, loadedMod.RelativeFiles);
			ValidatePrivateResources(loadedMod.Manifest, loadedMod.RelativeFiles);
			ValidateLoadingPresentation(loadedMod.Manifest, loadedMod.RelativeFiles);
			PreparePackageResourceReferences(loadedMod);
			if (!ResolveDeclaredRuntimeAssembly(loadedMod))
			{
				return null;
			}
			GD.Print($"[ModLoader] package extracted safely: {GetOwnerId(loadedMod)} ({loadedMod.RelativeFiles.Count} files)");
			loadedMod.RecordIdentity(text2);
			XWModEnvironmentService.Invalidate("PackageExtracted");
			return loadedMod;
		}
		catch (Exception ex)
		{
			loadedMod.RejectedFileCount++;
			AddDiagnostic(loadedMod, ex.Message);
			GD.PushWarning("[ModLoader] package rejected: " + pmodPath + ": " + ex.Message);
			return null;
		}
	}

	public static bool ApplyMod(LoadedMod mod)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return false;
		}
		if (mod == null || (mod.Manifest == null && mod.Info == null))
		{
			return false;
		}
		string ownerId = GetOwnerId(mod);
		if (!XWModRuntimeCompatibility.ValidatePackage(mod.Manifest, out var diagnostic))
		{
			AddDiagnostic(mod, diagnostic);
			return false;
		}
		HashSet<string> hashSet;
		try
		{
			hashSet = ValidatePrivateResources(mod.Manifest, mod.RelativeFiles);
			foreach (string relativePath in hashSet)
			{
				int num = mod.RelativeFiles.FindIndex((string path) => string.Equals(path, relativePath, StringComparison.Ordinal));
				if (num < 0 || num >= mod.ExtractedFiles.Count || !File.Exists(mod.ExtractedFiles[num]))
				{
					throw new InvalidDataException("missing extracted private resource: " + relativePath);
				}
			}
		}
		catch (Exception ex)
		{
			AddDiagnostic(mod, ex.GetBaseException().Message);
			return false;
		}
		if (!TryRemoveTrackedOwner(ownerId, out var _, out var blockers, mod))
		{
			AddDiagnostic(mod, XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(ownerId, blockers));
			return false;
		}
		XWModRuntimeRegistry.UnregisterOwner(ownerId);
		XWModEnvironmentService.Invalidate("Applying");
		mod.State = ApplicationState.Failed;
		mod.AppliedResourceCount = 0;
		mod.CompanionBoundResourceCount = 0;
		if (!mod.UnloadRuntimeAssembly(out var blockers2))
		{
			AddDiagnostic(mod, XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(ownerId, blockers2));
			return false;
		}
		if (!string.IsNullOrWhiteSpace(mod.RuntimeAssemblyPath))
		{
			try
			{
				mod.CharacterCompanionRuntime = new XWModCharacterCompanionRuntime();
				mod.CharacterCompanionRuntime.Load(ownerId, mod.RuntimeAssemblyPath, mod.Manifest.SharedAssemblies);
			}
			catch (Exception ex2)
			{
				AddDiagnostic(mod, "声明的运行程序集加载失败：" + ex2.GetBaseException().Message);
				mod.UnloadRuntimeAssembly();
				if (mod.Manifest.IsRuntimeAssemblyRequired())
				{
					return false;
				}
			}
		}
		try
		{
			mod.ActivateSettings();
		}
		catch (Exception ex3)
		{
			AddDiagnostic(mod, "配置初始化失败：" + ex3.GetBaseException().Message);
			TrackFailedApplyUntilRollback(mod, ownerId);
			return false;
		}
		bool flag = HasExplicitRuntimeEntries(mod.Manifest);
		StringComparison declarationComparison = ManifestKeyComparison(mod.Manifest);
		StringComparer comparer = ManifestKeyComparer(mod.Manifest);
		List<RuntimeCandidate> list = new List<RuntimeCandidate>();
		for (int num2 = 0; num2 < mod.RelativeFiles.Count && num2 < mod.ExtractedFiles.Count; num2++)
		{
			string relativePath2 = mod.RelativeFiles[num2];
			string extractedPath = mod.ExtractedFiles[num2];
			if (IsExecutablePackageFile(relativePath2))
			{
				if (!IsDeclaredRuntimeAssembly(mod, relativePath2) && !IsRuntimeDependencyFile(relativePath2))
				{
					AddDiagnostic(mod, "blocked executable package file: " + relativePath2);
				}
			}
			else
			{
				if (IsDeclaredRuntimeSymbols(mod, relativePath2) || hashSet.Contains(relativePath2))
				{
					continue;
				}
				XWModContentEntry xWModContentEntry = mod.Manifest.ContentEntries.FirstOrDefault((XWModContentEntry entry) => string.Equals(entry.Path, relativePath2, StringComparison.Ordinal));
				string category;
				string key;
				if (xWModContentEntry != null)
				{
					category = xWModContentEntry.Category;
					key = xWModContentEntry.Key;
				}
				else if (!InferRuntimeEntry(relativePath2, out category, out key))
				{
					if (!IsCharacterPackageDependency(relativePath2) && !IsAnimationAtlasPackageDependency(relativePath2) && !IsLevelPackageDependency(relativePath2))
					{
						AddDiagnostic(mod, "unsupported package file: " + relativePath2);
					}
					continue;
				}
				list.Add(new RuntimeCandidate
				{
					RelativePath = relativePath2,
					ExtractedPath = extractedPath,
					Category = category,
					InferredKey = key
				});
			}
		}
		List<RuntimeBinding> list2 = new List<RuntimeBinding>();
		bool flag2 = false;
		foreach (RuntimeCandidate candidate in list)
		{
			XWModContentEntry xWModContentEntry2 = mod.Manifest.ContentEntries.FirstOrDefault((XWModContentEntry entry) => string.Equals(entry.Path, candidate.RelativePath, StringComparison.Ordinal));
			if (xWModContentEntry2 != null)
			{
				list2.Add(new RuntimeBinding
				{
					Candidate = candidate,
					Key = xWModContentEntry2.Key,
					Required = true,
					AllowOverride = (xWModContentEntry2.Target != null),
					ContentEntry = xWModContentEntry2
				});
				continue;
			}
			if (list.Count((RuntimeCandidate runtimeCandidate) => NormalizeCategory(runtimeCandidate.Category).Equals(NormalizeCategory(candidate.Category), StringComparison.OrdinalIgnoreCase) && runtimeCandidate.InferredKey.Equals(candidate.InferredKey, declarationComparison)) != 1)
			{
				AddDiagnostic(mod, $"ambiguous automatic runtime key: {candidate.Category}/{candidate.InferredKey}: {candidate.RelativePath}");
				List<(string, bool)> manifestDeclarations = GetManifestDeclarations(mod.Manifest, candidate.Category);
				flag2 |= manifestDeclarations.Any<(string, bool)>(((string Key, bool Override) tuple) => tuple.Key.Equals(candidate.InferredKey, declarationComparison));
				continue;
			}
			string key2 = candidate.InferredKey;
			bool flag3 = ManifestContains(mod.Manifest?.Provides, candidate.Category, key2, declarationComparison);
			bool flag4 = ManifestContains(mod.Manifest?.Overrides, candidate.Category, key2, declarationComparison);
			if (flag && !flag3 && !flag4)
			{
				List<(string, bool)> manifestDeclarations2 = GetManifestDeclarations(mod.Manifest, candidate.Category);
				if (list.Count((RuntimeCandidate runtimeCandidate) => NormalizeCategory(runtimeCandidate.Category).Equals(NormalizeCategory(candidate.Category), StringComparison.OrdinalIgnoreCase)) != 1 || manifestDeclarations2.Count != 1)
				{
					AddDiagnostic(mod, "resource is not unambiguously declared by manifest: " + candidate.Category + "/" + candidate.InferredKey);
					continue;
				}
				key2 = manifestDeclarations2[0].Item1;
				flag4 = manifestDeclarations2[0].Item2;
				flag3 = !flag4;
			}
			list2.Add(new RuntimeBinding
			{
				Candidate = candidate,
				Key = key2,
				AllowOverride = flag4,
				Required = (flag3 || flag4)
			});
		}
		HashSet<string> hashSet2 = (from @group in list2.GroupBy<RuntimeBinding, string>((RuntimeBinding binding) => NormalizeCategory(binding.Candidate.Category) + "\n" + BindingRuntimeKey(ownerId, binding), comparer)
			where @group.Count() > 1
			select @group.Key).ToHashSet<string>(comparer);
		if (flag2)
		{
			TrackFailedApplyUntilRollback(mod, ownerId);
			return false;
		}
		foreach (RuntimeBinding item2 in list2)
		{
			RuntimeCandidate candidate2 = item2.Candidate;
			string item = NormalizeCategory(candidate2.Category) + "\n" + BindingRuntimeKey(ownerId, item2);
			if (hashSet2.Contains(item))
			{
				AddDiagnostic(mod, $"ambiguous manifest runtime key: {candidate2.Category}/{item2.Key}: {candidate2.RelativePath}");
				if (item2.Required)
				{
					TrackFailedApplyUntilRollback(mod, ownerId);
					return false;
				}
				continue;
			}
			if (!TryLoadRuntimeResource(mod, candidate2, out var resource))
			{
				if (item2.Required)
				{
					AddDiagnostic(mod, $"required runtime resource failed: {ownerId}: {candidate2.Category}/{item2.Key}: {candidate2.RelativePath}");
					TrackFailedApplyUntilRollback(mod, ownerId);
					return false;
				}
				continue;
			}
			if (item2.ContentEntry != null && !ValidateContentResourceIdentity(candidate2.Category, BindingResourceKey(ownerId, item2), resource, item2.Key, out var diagnostic2))
			{
				AddDiagnostic(mod, "content identity rejected: " + candidate2.RelativePath + ": " + diagnostic2);
				TrackFailedApplyUntilRollback(mod, ownerId);
				return false;
			}
			try
			{
				PrepareOwnedResourceGraph(mod, resource);
			}
			catch (Exception ex4)
			{
				AddDiagnostic(mod, "owned resource preparation failed: " + candidate2.RelativePath + ": " + ex4.Message);
				TrackFailedApplyUntilRollback(mod, ownerId);
				return false;
			}
			if (candidate2.Category.Equals("Character", StringComparison.OrdinalIgnoreCase) && resource is PackedScene packedScene && CharacterRequiresCompanion(packedScene))
			{
				string diagnostic3 = "伴随脚本运行时不可用";
				Node runtimeRoot = null;
				if (mod.CharacterCompanionRuntime == null || !mod.CharacterCompanionRuntime.TryCreateInstance(packedScene, out runtimeRoot, out diagnostic3))
				{
					AddDiagnostic(mod, "角色伴随脚本绑定被拒绝：" + candidate2.RelativePath + "：" + diagnostic3);
					GD.PushWarning(TrackFailedApplyUntilRollback(mod, ownerId) ? ("[ModLoader] package rolled back after companion binding failure: " + ownerId) : ("[ModLoader] companion binding failure remains tracked pending rollback: " + ownerId));
					return false;
				}
				runtimeRoot.Free();
				mod.CompanionBoundResourceCount++;
			}
			string text = BindingRuntimeKey(ownerId, item2);
			if (item2.ContentEntry != null && resource is TowerDefensePacketConfig towerDefensePacketConfig)
			{
				towerDefensePacketConfig.saveKey = BindingResourceKey(ownerId, item2);
			}
			Variant value = Variant.From(in resource);
			if (resource is LevelCatalogConfig levelCatalogConfig)
			{
				value = Variant.From<Dictionary>(XWModContentCatalog.FromResource(levelCatalogConfig));
				if (!flag && !string.IsNullOrWhiteSpace(levelCatalogConfig.catalogKey))
				{
					text = levelCatalogConfig.catalogKey.Trim();
				}
			}
			if (!((item2.ContentEntry == null) ? XWModRuntimeRegistry.Register(ownerId, candidate2.Category, text, value, item2.AllowOverride, out var diagnostic4) : ((item2.ContentEntry.Target == null) ? XWModRuntimeRegistry.Provide(ownerId, candidate2.Category, item2.Key, value, out diagnostic4) : XWModRuntimeRegistry.Override(ownerId, ContentTarget(item2.ContentEntry.Target), value, out diagnostic4))))
			{
				AddDiagnostic(mod, $"runtime registry rejected resource: {candidate2.Category}/{text}: {diagnostic4}: {ownerId}: {candidate2.RelativePath}");
				if (item2.Required)
				{
					TrackFailedApplyUntilRollback(mod, ownerId);
					return false;
				}
			}
			else
			{
				if (diagnostic4.StartsWith("compatibility warning:", StringComparison.Ordinal))
				{
					AddDiagnostic(mod, diagnostic4);
				}
				mod.AppliedResourceCount++;
			}
		}
		if (mod.CharacterCompanionRuntime != null && !mod.CharacterCompanionRuntime.TryInitializeRuntimeEntry(mod.Manifest, mod.ExtractRoot, out var diagnostic5))
		{
			AddDiagnostic(mod, "运行入口初始化失败：" + diagnostic5);
			GD.PushWarning(TrackFailedApplyUntilRollback(mod, ownerId) ? ("[ModLoader] package rolled back after runtime entry failure: " + ownerId) : ("[ModLoader] runtime entry failure remains tracked pending rollback: " + ownerId));
			return false;
		}
		if (flag && !ValidateManifestRegistrations(mod, ownerId))
		{
			TrackFailedApplyUntilRollback(mod, ownerId);
			return false;
		}
		if (!XWModContentValidation.Validate(mod.Manifest, out var error))
		{
			AddDiagnostic(mod, "Mod 内容关联无效：" + error);
			TrackFailedApplyUntilRollback(mod, ownerId);
			return false;
		}
		PrepareDeclaredLoadingScenes(mod);
		if (!mod.HasAppliedContent)
		{
			TrackFailedApplyUntilRollback(mod, ownerId);
			GD.PushWarning("[ModLoader] package not applied: " + ownerId + "; no supported resources, runtime entry, or state-machine callbacks were registered");
			return false;
		}
		mod.State = ApplicationState.Applied;
		XWModCharacterCompanionRuntime characterCompanionRuntime = mod.CharacterCompanionRuntime;
		mod.EffectiveMode = ((characterCompanionRuntime != null && characterCompanionRuntime.IsLoaded) ? EffectiveRuntimeMode.Managed : (XWModRuntimeCompatibility.HasManagedCode(mod.Manifest) ? EffectiveRuntimeMode.OptionalResourceFallback : EffectiveRuntimeMode.ResourceOnly));
		LoadedMods.Add(mod);
		XWModEnvironmentService.Invalidate("RuntimeApplied");
		if (mod.CharacterCompanionRuntime != null)
		{
			CompanionOwners.Add(ownerId);
		}
		GD.Print($"[ModLoader] package applied: {ownerId}; resources={mod.AppliedResourceCount}; runtimeEntry={mod.RuntimeEntryInitialized}; callbacks={mod.StateMachineCallbackCount}; diagnostics={mod.Diagnostics.Count}");
		return true;
	}

	public static bool UnloadMod(string ownerId)
	{
		StateMachineUnloadBlockers blockers;
		return UnloadMod(ownerId, out blockers);
	}

	internal static bool RollbackMod(string ownerId, out StateMachineUnloadBlockers blockers)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			blockers = new StateMachineUnloadBlockers(0, System.Array.Empty<StateMachineUnloadBlocker>());
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return false;
		}
		LoadedMod loadedMod = LoadedMods.LastOrDefault((LoadedMod item) => string.Equals(GetOwnerId(item), ownerId, StringComparison.OrdinalIgnoreCase));
		loadedMod?.PrepareLifecycleRollback();
		bool num = UnloadMod(ownerId, out blockers);
		if (!num && loadedMod != null)
		{
			loadedMod.State = ApplicationState.PendingRollback;
		}
		XWModEnvironmentService.Invalidate("LifecycleRollback");
		return num;
	}

	internal static bool IsTracked(LoadedMod mod)
	{
		if (mod != null)
		{
			return LoadedMods.Contains(mod);
		}
		return false;
	}

	public static bool UnloadMod(string ownerId, out StateMachineUnloadBlockers blockers)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		blockers = new StateMachineUnloadBlockers(0, System.Array.Empty<StateMachineUnloadBlocker>());
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return false;
		}
		if (string.IsNullOrWhiteSpace(ownerId))
		{
			return false;
		}
		if (!TryRemoveTrackedOwner(ownerId, out var removedCount, out blockers))
		{
			GD.PushWarning("[ModLoader] " + XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(ownerId, blockers));
			return false;
		}
		int num = XWModRuntimeRegistry.UnregisterOwner(ownerId);
		return removedCount > 0 || num > 0;
	}

	private static bool TrackFailedApplyUntilRollback(LoadedMod mod, string ownerId)
	{
		mod.State = ApplicationState.Failed;
		XWModEnvironmentService.Invalidate("ApplyFailed");
		XWModRuntimeRegistry.UnregisterOwner(ownerId);
		mod.ClearRuntimeResourceCache();
		mod.AppliedResourceCount = 0;
		mod.CompanionBoundResourceCount = 0;
		mod.PrepareLifecycleRollback();
		if (mod.UnloadRuntimeAssembly(out var blockers))
		{
			return true;
		}
		mod.State = ApplicationState.PendingRollback;
		AddDiagnostic(mod, XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(ownerId, blockers));
		if (!LoadedMods.Contains(mod))
		{
			LoadedMods.Add(mod);
		}
		if (mod.CharacterCompanionRuntime != null)
		{
			CompanionOwners.Add(ownerId);
		}
		GD.PushWarning("[ModLoader] failed apply remains tracked: " + XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(ownerId, blockers));
		return false;
	}

	public static void UnloadAll()
	{
		TryUnloadAll();
	}

	public static bool TryUnloadAll()
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return false;
		}
		foreach (LoadedMod loadedMod in LoadedMods)
		{
			if (!loadedMod.CanUnloadRuntimeAssembly(out var blockers))
			{
				GD.PushWarning("[ModLoader] " + XWModCharacterCompanionRuntime.FormatUnloadBlockedDiagnostic(GetOwnerId(loadedMod), blockers));
				return false;
			}
		}
		string[] array = (from value in LoadedMods.Select(GetOwnerId)
			where !string.IsNullOrWhiteSpace(value)
			select value).Distinct<string>(StringComparer.OrdinalIgnoreCase).Reverse().ToArray();
		bool flag = true;
		string[] array2 = array;
		foreach (string ownerId in array2)
		{
			flag &= UnloadMod(ownerId, out var _);
		}
		return flag;
	}

	public static IReadOnlyList<LoadedMod> GetLoadedMods()
	{
		return LoadedMods;
	}

	public static bool TryInstantiateCharacter(string ownerId, string characterKey, out Node runtimeRoot, out string diagnostic)
	{
		return TryInstantiateCharacter(ownerId, characterKey, allowNativeCharacter: false, out runtimeRoot, out diagnostic);
	}

	private static bool TryInstantiateCharacter(string ownerId, string characterKey, bool allowNativeCharacter, out Node runtimeRoot, out string diagnostic)
	{
		runtimeRoot = null;
		diagnostic = "";
		LoadedMod loadedMod = LoadedMods.LastOrDefault((LoadedMod item) => string.Equals(GetOwnerId(item), ownerId, StringComparison.OrdinalIgnoreCase));
		if (loadedMod?.CharacterCompanionRuntime == null)
		{
			diagnostic = "该 Mod 没有已加载的角色伴随脚本运行时。";
			return false;
		}
		if (!XWModRuntimeRegistry.TryGetEffectiveRegistration("Character", characterKey, out var registration) || !string.Equals(registration.OwnerMod, ownerId, StringComparison.OrdinalIgnoreCase) || !(registration.ModValue.AsGodotObject() is PackedScene authoredScene))
		{
			diagnostic = "Mod 角色注册不可用：" + characterKey + "。";
			return false;
		}
		if (!allowNativeCharacter)
		{
			return loadedMod.CharacterCompanionRuntime.TryCreateInstance(authoredScene, out runtimeRoot, out diagnostic);
		}
		return loadedMod.CharacterCompanionRuntime.TryCreateEffectiveCharacterInstance(authoredScene, out runtimeRoot, out diagnostic);
	}

	public static bool TryInstantiateEffectiveCharacter(string characterKey, out TowerDefenseCharacter character, out string diagnostic)
	{
		character = null;
		diagnostic = "";
		if (CompanionOwners.Count == 0 || string.IsNullOrWhiteSpace(characterKey))
		{
			return false;
		}
		if (!XWModRuntimeRegistry.TryGetEffectiveRegistration("Character", characterKey, out var registration) || !CompanionOwners.Contains(registration.OwnerMod))
		{
			return false;
		}
		if (!TryInstantiateCharacter(registration.OwnerMod, characterKey, allowNativeCharacter: true, out var runtimeRoot, out diagnostic))
		{
			return false;
		}
		if (runtimeRoot is TowerDefenseCharacter towerDefenseCharacter)
		{
			character = towerDefenseCharacter;
			return true;
		}
		if (GodotObject.IsInstanceValid(runtimeRoot))
		{
			runtimeRoot.Free();
		}
		diagnostic = "当前生效的 Mod 角色根节点类型错误：" + characterKey + "。";
		return false;
	}

	private static bool TryRemoveTrackedOwner(string ownerId, out int removedCount, out StateMachineUnloadBlockers blockers, LoadedMod preservedRuntimeCache = null)
	{
		removedCount = 0;
		blockers = new StateMachineUnloadBlockers(0, System.Array.Empty<StateMachineUnloadBlocker>());
		LoadedMod[] removed = LoadedMods.Where((LoadedMod mod) => string.Equals(GetOwnerId(mod), ownerId, StringComparison.OrdinalIgnoreCase)).ToArray();
		LoadedMod[] array = removed;
		for (int num = 0; num < array.Length; num++)
		{
			if (!array[num].UnloadRuntimeAssembly(out blockers))
			{
				return false;
			}
		}
		array = removed;
		foreach (LoadedMod loadedMod in array)
		{
			loadedMod.State = ApplicationState.Unloaded;
			if (loadedMod != preservedRuntimeCache)
			{
				loadedMod.ClearRuntimeResourceCache();
			}
		}
		CompanionOwners.Remove(ownerId);
		removedCount = LoadedMods.RemoveAll((LoadedMod mod) => removed.Contains(mod));
		if (removedCount > 0)
		{
			XWModEnvironmentService.Invalidate("RuntimeUnloaded");
		}
		return true;
	}

	private static void AddDiagnostic(LoadedMod mod, string diagnostic)
	{
		if (mod != null && !string.IsNullOrWhiteSpace(diagnostic) && !mod.Diagnostics.Contains<string>(diagnostic, StringComparer.Ordinal))
		{
			mod.Diagnostics.Add(diagnostic);
		}
	}

	private static bool ResolveDeclaredRuntimeAssembly(LoadedMod mod)
	{
		string declared = (mod?.Manifest?.RuntimeAssembly ?? "").Replace('\\', '/').Trim('/');
		if (string.IsNullOrWhiteSpace(declared))
		{
			return true;
		}
		if (!declared.Equals("Runtime/ModAssembly.dll", StringComparison.Ordinal) || Path.IsPathRooted(declared) || declared.Contains("..", StringComparison.Ordinal) || declared.Contains(':'))
		{
			AddDiagnostic(mod, "声明的运行程序集路径无效：" + declared);
			GD.PushWarning("[ModLoader] package rejected: invalid declared runtime assembly path: " + declared);
			return false;
		}
		int num = mod.RelativeFiles.FindIndex((string relative) => relative.Equals(declared, StringComparison.Ordinal));
		if (num < 0 || num >= mod.ExtractedFiles.Count || !File.Exists(mod.ExtractedFiles[num]))
		{
			AddDiagnostic(mod, "声明的运行程序集不存在：" + declared);
			if (mod.Manifest.IsRuntimeAssemblyRequired())
			{
				GD.PushWarning("[ModLoader] package rejected: declared runtime assembly is missing: " + declared);
				return false;
			}
			return true;
		}
		mod.RuntimeAssemblyPath = mod.ExtractedFiles[num];
		return true;
	}

	private static bool IsDeclaredRuntimeAssembly(LoadedMod mod, string relativePath)
	{
		string text = (mod?.Manifest?.RuntimeAssembly ?? "").Replace('\\', '/').Trim('/');
		if (!string.IsNullOrWhiteSpace(text))
		{
			return string.Equals((relativePath ?? "").Replace('\\', '/').Trim('/'), text, StringComparison.Ordinal);
		}
		return false;
	}

	private static bool IsDeclaredRuntimeSymbols(LoadedMod mod, string relativePath)
	{
		string text = (mod?.Manifest?.RuntimeAssembly ?? "").Replace('\\', '/').Trim('/');
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		return string.Equals((relativePath ?? "").Replace('\\', '/').Trim('/'), Path.ChangeExtension(text, ".pdb").Replace('\\', '/'), StringComparison.Ordinal);
	}

	private static bool IsRuntimeDependencyFile(string relativePath)
	{
		string text = (relativePath ?? "").Replace('\\', '/').Trim('/');
		if (!text.StartsWith("Runtime/Dependencies/", StringComparison.Ordinal))
		{
			return false;
		}
		string text2 = text.Substring("Runtime/Dependencies/".Length);
		if (string.IsNullOrWhiteSpace(text2) || text2.Contains('/'))
		{
			return false;
		}
		string extension = Path.GetExtension(text2);
		if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".pdb", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool CharacterRequiresCompanion(PackedScene scene)
	{
		Node node = null;
		try
		{
			node = scene?.Instantiate(PackedScene.GenEditState.Disabled);
			return GodotObject.IsInstanceValid(node) && node.HasMeta("mod_character_script_binding") && string.Equals(node.GetMeta("mod_character_script_binding").AsString(), "CompanionOnly", StringComparison.Ordinal);
		}
		catch
		{
			return false;
		}
		finally
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.Free();
			}
		}
	}

	private static bool IsAnimationAtlasPackageDependency(string relativePath)
	{
		string text = (relativePath ?? "").Replace('\\', '/').Trim('/');
		string extension = Path.GetExtension(text);
		if (text.StartsWith("Resources/AnimationAtlasManifests/", StringComparison.OrdinalIgnoreCase))
		{
			return new string[10] { ".tres", ".res", ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tga", ".exr", ".hdr" }.Contains<string>(extension, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool ValidateArchiveEntryPath(string extractRoot, string entryName, out string relativePath, out string outputPath)
	{
		relativePath = (entryName ?? "").Replace('\\', '/').TrimStart('/');
		outputPath = "";
		if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(entryName ?? "") || (entryName ?? "").StartsWith("/", StringComparison.Ordinal) || relativePath.Contains(':'))
		{
			return false;
		}
		string[] array = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0 || array.Any((string segment) => (segment == "." || segment == "..") ? true : false))
		{
			return false;
		}
		relativePath = string.Join('/', array);
		string text = Path.GetFullPath(extractRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		outputPath = Path.GetFullPath(Path.Combine(text, relativePath.Replace('/', Path.DirectorySeparatorChar)));
		string value = text + Path.DirectorySeparatorChar;
		if (!outputPath.StartsWith(value, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return string.Equals(Path.GetRelativePath(text, outputPath).Replace('\\', '/'), relativePath, StringComparison.Ordinal);
	}

	public static bool InferRuntimeEntry(string relativePath, out string category, out string key)
	{
		category = "";
		key = "";
		string text = (relativePath ?? "").Replace('\\', '/').Trim('/');
		string extension = Path.GetExtension(text);
		if (text.StartsWith("Assets/Audio/", StringComparison.OrdinalIgnoreCase) && (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase) || extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) || extension.Equals(".flac", StringComparison.OrdinalIgnoreCase)))
		{
			category = "Audio";
			key = Path.GetFileNameWithoutExtension(text);
			return !string.IsNullOrWhiteSpace(key);
		}
		if ((text.StartsWith("Assets/Textures/", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Assets/Texture/", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Assets/Images/", StringComparison.OrdinalIgnoreCase)) && XWModExternalMediaLoader.IsSupportedTextureExtension(extension))
		{
			category = "Texture";
			key = Path.GetFileNameWithoutExtension(text);
			return !string.IsNullOrWhiteSpace(key);
		}
		if (TryInferCharacterScene(text, out key))
		{
			category = "Character";
			return true;
		}
		if (TryInferCharacterScene(text, out key, "Sprite"))
		{
			category = "CharacterSprite";
			return true;
		}
		if (!extension.Equals(".tres", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".res", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (text.StartsWith("Resources/Tutorials/Conditions/", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Resources/Tutorials/Steps/", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		(string, string)[] array = new(string, string)[17]
		{
			("Battle/Features/", "Feature"),
			("Battle/Processes/", "Process"),
			("Resources/LevelCatalogs/", "Level"),
			("Resources/Maps/", "Map"),
			("Resources/Cards/", "Packet"),
			("Resources/PacketBank/", "PacketBank"),
			("Resources/Projectiles/", "Projectile"),
			("Resources/ProjectileChanges/", "ProjectileChange"),
			("Resources/Collectables/", "Collectable"),
			("Resources/Mowers/", "Mower"),
			("Resources/Shovels/", "Shovel"),
			("Resources/Survivals/", "Survival"),
			("Resources/Tutorials/", "Tutorial"),
			("Resources/NpcTalks/", "NpcTalk"),
			("Resources/Shops/", "Shop"),
			("Resources/AnimationAtlasProfiles/", "AnimationAtlas"),
			("Resources/BGMConfigs/", "BGM")
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (value, text2) = array[i];
			if (text.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				category = text2;
				key = Path.GetFileNameWithoutExtension(text);
				return !string.IsNullOrWhiteSpace(key);
			}
		}
		return false;
	}

	internal static bool ValidateContentResourceIdentity(string category, string runtimeKey, Resource resource, string authoredKey, out string diagnostic)
	{
		diagnostic = "";
		string text = null;
		Node node = null;
		try
		{
			switch (NormalizeCategory(category))
			{
			case "Packet":
				if (resource is TowerDefensePacketConfig towerDefensePacketConfig)
				{
					text = towerDefensePacketConfig.saveKey;
					break;
				}
				goto default;
			case "Projectile":
				if (resource is TowerDefenseProjectileConfig towerDefenseProjectileConfig)
				{
					text = towerDefenseProjectileConfig.name;
					break;
				}
				if (resource is TowerDefenseProjectileData towerDefenseProjectileData)
				{
					text = towerDefenseProjectileData.name;
					break;
				}
				goto default;
			case "Collectable":
				if (resource is CollectableConfig collectableConfig)
				{
					text = collectableConfig.saveKey;
					break;
				}
				goto default;
			case "Shovel":
				if (resource is ShovelConfig shovelConfig)
				{
					text = shovelConfig.saveKey;
					break;
				}
				goto default;
			case "Mower":
				if (resource is MowerConfig mowerConfig)
				{
					text = mowerConfig.saveKey;
					break;
				}
				goto default;
			case "Character":
				if (resource is PackedScene packedScene)
				{
					node = packedScene.Instantiate(PackedScene.GenEditState.Disabled);
					if (!(node is TowerDefenseCharacter towerDefenseCharacter) || !GodotObject.IsInstanceValid(towerDefenseCharacter.config))
					{
						diagnostic = "Character content requires a TowerDefenseCharacter root with a character config";
						return false;
					}
					text = towerDefenseCharacter.config.name;
					break;
				}
				goto default;
			default:
				return true;
			}
			if (string.Equals(text, runtimeKey, StringComparison.Ordinal) || (!string.IsNullOrEmpty(authoredKey) && string.Equals(text, authoredKey, StringComparison.Ordinal)))
			{
				return true;
			}
			diagnostic = $"{NormalizeCategory(category)}/{runtimeKey} declares a different internal identity '{text ?? ""}'" + (string.IsNullOrEmpty(authoredKey) ? "" : $"; expected '{authoredKey}' or '{runtimeKey}'");
			return false;
		}
		catch (Exception ex)
		{
			diagnostic = ex.GetBaseException().Message;
			return false;
		}
		finally
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.Free();
			}
		}
	}

	private static void PrepareOwnedResourceGraph(LoadedMod mod, Resource root)
	{
		HashSet<ulong> visited = new HashSet<ulong>();
		int traversed = 0;
		string owner = GetOwnerId(mod);
		string rootPrefix = Path.GetFullPath(mod.ExtractRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		System.Collections.Generic.Dictionary<(string Category, string Key), XWModContentId> identities = mod.Manifest.ContentEntries.ToDictionary((XWModContentEntry entry) => (Category: entry.Category, Key: entry.Key), (XWModContentEntry entry) => (entry.Target != null) ? ContentTarget(entry.Target) : new XWModContentId(owner, entry.Category, entry.Key));
		Visit(root, 0);
		string Qualify(string category, string key)
		{
			if (!identities.TryGetValue((category, key ?? ""), out var value))
			{
				return key;
			}
			return XWModRuntimeRegistry.GetResourceKey(value);
		}
		void QualifyStrings(Godot.Collections.Array values, string category)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (values[i].VariantType == Variant.Type.String)
				{
					values[i] = Qualify(category, values[i].AsString());
				}
				else if (values[i].VariantType == Variant.Type.StringName)
				{
					values[i] = new StringName(Qualify(category, values[i].AsString()));
				}
			}
		}
		void Visit(Resource resource, int depth)
		{
			if (GodotObject.IsInstanceValid(resource) && !(resource is Script) && visited.Add(resource.GetInstanceId()))
			{
				string text = resource.ResourcePath ?? "";
				if (text.Length <= 0 || Path.GetFullPath(ProjectSettings.GlobalizePath(text.Split("::")[0])).StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
				{
					ModCharacterComponentDefinition.AttachOwner(resource, owner);
					if (identities.Count > 0)
					{
						if (!(resource is TowerDefenseCharacterConfig towerDefenseCharacterConfig))
						{
							if (!(resource is TowerDefensePacketConfig towerDefensePacketConfig))
							{
								if (!(resource is TowerDefensePacketBankData towerDefensePacketBankData))
								{
									if (!(resource is TowerDefenseLevelPreSpawnConfig towerDefenseLevelPreSpawnConfig))
									{
										if (!(resource is TowerDefenseLevelPacketConfig towerDefenseLevelPacketConfig))
										{
											if (!(resource is TowerDefenseLevelSpawnConfig towerDefenseLevelSpawnConfig))
											{
												if (!(resource is TowerDefenseLevelSpawnDynamicConfig towerDefenseLevelSpawnDynamicConfig))
												{
													if (!(resource is TowerDefenseProjectileConfig towerDefenseProjectileConfig))
													{
														if (!(resource is TowerDefenseProjectileData towerDefenseProjectileData))
														{
															if (!(resource is CollectableConfig collectableConfig))
															{
																if (!(resource is ShovelConfig shovelConfig))
																{
																	if (resource is MowerConfig mowerConfig)
																	{
																		mowerConfig.saveKey = Qualify("Mower", mowerConfig.saveKey);
																	}
																}
																else
																{
																	shovelConfig.saveKey = Qualify("Shovel", shovelConfig.saveKey);
																}
															}
															else
															{
																collectableConfig.saveKey = Qualify("Collectable", collectableConfig.saveKey);
															}
														}
														else
														{
															towerDefenseProjectileData.name = Qualify("Projectile", towerDefenseProjectileData.name);
															towerDefenseProjectileData.splatAudio = Qualify("Audio", towerDefenseProjectileData.splatAudio);
														}
													}
													else
													{
														towerDefenseProjectileConfig.name = Qualify("Projectile", towerDefenseProjectileConfig.name);
														towerDefenseProjectileConfig.splatAudio = Qualify("Audio", towerDefenseProjectileConfig.splatAudio);
													}
												}
												else
												{
													for (int i = 0; i < towerDefenseLevelSpawnDynamicConfig.zombiePool.Count; i++)
													{
														towerDefenseLevelSpawnDynamicConfig.zombiePool[i] = Qualify("Character", towerDefenseLevelSpawnDynamicConfig.zombiePool[i]);
													}
												}
											}
											else
											{
												towerDefenseLevelSpawnConfig.zombie = Qualify("Character", towerDefenseLevelSpawnConfig.zombie);
											}
										}
										else
										{
											towerDefenseLevelPacketConfig.packetName = Qualify("Packet", towerDefenseLevelPacketConfig.packetName);
										}
									}
									else
									{
										towerDefenseLevelPreSpawnConfig.packetName = Qualify("Packet", towerDefenseLevelPreSpawnConfig.packetName);
									}
								}
								else
								{
									foreach (Variant value2 in towerDefensePacketBankData.category.Values)
									{
										if (value2.VariantType == Variant.Type.Array)
										{
											QualifyStrings(value2.AsGodotArray(), "Packet");
										}
									}
								}
							}
							else
							{
								towerDefensePacketConfig.saveKey = Qualify("Packet", towerDefensePacketConfig.saveKey);
							}
						}
						else
						{
							towerDefenseCharacterConfig.name = Qualify("Character", towerDefenseCharacterConfig.name);
							for (int j = 0; j < towerDefenseCharacterConfig.plantCover.Count; j++)
							{
								towerDefenseCharacterConfig.plantCover[j] = Qualify("Character", towerDefenseCharacterConfig.plantCover[j]);
							}
						}
						if (resource is TowerDefenseLevelConfig || resource is TowerDefenseLevelNewConfig)
						{
							resource.Set("processName", new StringName(Qualify("Process", resource.Get("processName").AsString())));
							Dictionary dictionary = resource.Get("featureData").AsGodotDictionary();
							Variant[] array = dictionary.Keys.ToArray();
							for (int k = 0; k < array.Length; k++)
							{
								Variant key = array[k];
								string text2 = Qualify("Feature", key.AsString());
								if (!(text2 == key.AsString()))
								{
									Variant value = dictionary[key];
									dictionary.Remove(key);
									dictionary[new StringName(text2)] = value;
								}
							}
						}
						if (resource is TowerDefenseLevelConfig towerDefenseLevelConfig)
						{
							towerDefenseLevelConfig.map = Qualify("Map", towerDefenseLevelConfig.map);
							towerDefenseLevelConfig.backgroundMusic = Qualify("BGM", towerDefenseLevelConfig.backgroundMusic);
							towerDefenseLevelConfig.packetBank = Qualify("PacketBank", towerDefenseLevelConfig.packetBank);
							QualifyStrings(towerDefenseLevelConfig.packetBankList, "Packet");
							if (towerDefenseLevelConfig.tutorial.VariantType == Variant.Type.String)
							{
								towerDefenseLevelConfig.tutorial = Qualify("Tutorial", towerDefenseLevelConfig.tutorial.AsString());
							}
							if (towerDefenseLevelConfig.talk.VariantType == Variant.Type.String)
							{
								towerDefenseLevelConfig.talk = Qualify("NpcTalk", towerDefenseLevelConfig.talk.AsString());
							}
						}
					}
					if (resource is PackedScene packedScene)
					{
						SceneState state = packedScene.GetState();
						for (int l = 0; l < state.GetNodeCount(); l++)
						{
							for (int m = 0; m < state.GetNodePropertyCount(l); m++)
							{
								VisitValue(state.GetNodePropertyValue(l, m), depth + 1);
							}
						}
					}
					foreach (Dictionary property in resource.GetPropertyList())
					{
						if ((property["usage"].AsInt64() & 2) != 0L)
						{
							string text3 = property["name"].AsString();
							if ((!(text3 == "script") && !(text3 == "_bundled")) || 1 == 0)
							{
								VisitValue(resource.Get(text3), depth + 1);
							}
						}
					}
				}
			}
		}
		void VisitValue(Variant value, int depth)
		{
			if (++traversed > 100000 || depth > 128)
			{
				throw new InvalidDataException("owned resource graph exceeds traversal limit");
			}
			if (value.VariantType == Variant.Type.Object && value.AsGodotObject() is Resource resource)
			{
				Visit(resource, depth + 1);
			}
			else
			{
				if (value.VariantType == Variant.Type.Array)
				{
					foreach (Variant item in value.AsGodotArray())
					{
						VisitValue(item, depth + 1);
					}
					return;
				}
				if (value.VariantType == Variant.Type.Dictionary)
				{
					foreach (Variant value3 in value.AsGodotDictionary().Values)
					{
						VisitValue(value3, depth + 1);
					}
				}
			}
		}
	}

	private static bool TryLoadRuntimeResource(LoadedMod mod, RuntimeCandidate candidate, out Resource resource)
	{
		resource = null;
		string key = NormalizeCategory(candidate.Category) + "\n" + Path.GetFullPath(candidate.ExtractedPath);
		if (mod.RuntimeResourceCache.TryGetValue(key, out var value) && GodotObject.IsInstanceValid(value))
		{
			resource = value;
			return true;
		}
		if (!CanAdmitRuntimeCacheEntry(mod, candidate, out var sourceBytes, out var diagnostic))
		{
			AddDiagnostic(mod, "runtime media cache rejected: " + candidate.RelativePath + ": " + diagnostic);
			return false;
		}
		if (!TryGetGodotResourcePath(candidate.ExtractedPath, out var resourcePath))
		{
			AddDiagnostic(mod, "resource path is outside Godot resource roots: " + candidate.RelativePath);
			return false;
		}
		try
		{
			if (candidate.Category.Equals("Audio", StringComparison.OrdinalIgnoreCase))
			{
				if (!XWModExternalMediaLoader.TryLoadAudio(resourcePath, Path.GetExtension(candidate.RelativePath), out var audio, out var diagnostic2))
				{
					AddDiagnostic(mod, "audio runtime load rejected: " + candidate.RelativePath + ": " + diagnostic2);
					return false;
				}
				resource = audio;
			}
			else if (candidate.Category.Equals("Texture", StringComparison.OrdinalIgnoreCase))
			{
				if (!XWModExternalMediaLoader.TryLoadTexture(resourcePath, Path.GetExtension(candidate.RelativePath), out var texture, out var diagnostic3))
				{
					AddDiagnostic(mod, "texture runtime load rejected: " + candidate.RelativePath + ": " + diagnostic3);
					return false;
				}
				resource = texture;
			}
			else if (candidate.Category.Equals("Character", StringComparison.OrdinalIgnoreCase) || candidate.Category.Equals("CharacterSprite", StringComparison.OrdinalIgnoreCase))
			{
				if (!PrepareSafeCharacterPackage(mod, candidate))
				{
					return false;
				}
				resource = ResourceLoader.Load<PackedScene>(resourcePath, "", ResourceLoader.CacheMode.Ignore);
			}
			else
			{
				resource = ResourceLoader.Load<Resource>(resourcePath, "", ResourceLoader.CacheMode.Ignore);
			}
			if (candidate.Category.Equals("AnimationAtlas", StringComparison.OrdinalIgnoreCase))
			{
				if (!(resource is AdobeAnimateAtlasProfile profile))
				{
					AddDiagnostic(mod, "animation atlas profile has the wrong resource type: " + candidate.RelativePath);
					return false;
				}
				if (!TryPreparePackagedAnimationAtlasProfile(mod, candidate, profile, out var diagnostic4))
				{
					AddDiagnostic(mod, "animation atlas profile rejected: " + candidate.RelativePath + ": " + diagnostic4);
					return false;
				}
			}
		}
		catch (Exception ex)
		{
			AddDiagnostic(mod, "resource load exception: " + candidate.RelativePath + ": " + ex.Message);
			return false;
		}
		if (!GodotObject.IsInstanceValid(resource))
		{
			AddDiagnostic(mod, "resource load failed: " + candidate.RelativePath);
			return false;
		}
		if (candidate.Category.Equals("Audio", StringComparison.OrdinalIgnoreCase) && !(resource is AudioStream))
		{
			AddDiagnostic(mod, "audio file did not produce an AudioStream: " + candidate.RelativePath);
			return false;
		}
		if (candidate.Category.Equals("Texture", StringComparison.OrdinalIgnoreCase) && !(resource is Texture2D))
		{
			AddDiagnostic(mod, "texture file did not produce a Texture2D: " + candidate.RelativePath);
			return false;
		}
		if (candidate.Category.Equals("Character", StringComparison.OrdinalIgnoreCase) && !(resource is PackedScene))
		{
			AddDiagnostic(mod, "character scene did not produce a PackedScene: " + candidate.RelativePath);
			return false;
		}
		long texturePixels = ((resource is Texture2D texture2D) ? ((long)texture2D.GetWidth() * (long)texture2D.GetHeight()) : 0);
		long decodedAudioBytes = ((resource is AudioStreamWav audioStreamWav) ? audioStreamWav.Data.LongLength : 0);
		if (!CanAdmitDecodedTexture(mod, texturePixels, out diagnostic))
		{
			AddDiagnostic(mod, "runtime media cache rejected: " + candidate.RelativePath + ": " + diagnostic);
			resource.Dispose();
			resource = null;
			return false;
		}
		if (!CanAdmitDecodedAudio(mod, decodedAudioBytes, out diagnostic))
		{
			AddDiagnostic(mod, "runtime media cache rejected: " + candidate.RelativePath + ": " + diagnostic);
			resource.Dispose();
			resource = null;
			return false;
		}
		mod.RuntimeResourceCache[key] = resource;
		mod.AccountCachedRuntimeResource(sourceBytes, texturePixels, decodedAudioBytes);
		return true;
	}

	private static bool CanAdmitRuntimeCacheEntry(LoadedMod mod, RuntimeCandidate candidate, out long sourceBytes, out string diagnostic)
	{
		sourceBytes = 0L;
		diagnostic = "";
		try
		{
			sourceBytes = new FileInfo(candidate.ExtractedPath).Length;
		}
		catch (Exception ex)
		{
			diagnostic = "source size unavailable: " + ex.Message;
			return false;
		}
		int num = LoadedMods.Sum((LoadedMod item) => item.RuntimeResourceCache.Count) + mod.RuntimeResourceCache.Count;
		long num2 = LoadingBudgetOwners(mod).Sum((LoadedMod item) => item.CachedRuntimeSourceBytes + item.LoadingSourceBytes);
		if (mod.RuntimeResourceCache.Count >= 512 || num >= 4096)
		{
			diagnostic = "runtime resource cache entry limit exceeded";
			return false;
		}
		if (sourceBytes < 0 || mod.CachedRuntimeSourceBytes + mod.LoadingSourceBytes + sourceBytes > 268435456 || num2 + sourceBytes > 536870912)
		{
			diagnostic = "runtime resource cache byte limit exceeded";
			return false;
		}
		return true;
	}

	private static bool CanAdmitDecodedTexture(LoadedMod mod, long texturePixels, out string diagnostic)
	{
		diagnostic = "";
		if (texturePixels <= 0)
		{
			return true;
		}
		long num = LoadingBudgetOwners(mod).Sum((LoadedMod item) => item.CachedRuntimeTexturePixels + item.LoadingTexturePixels);
		if (mod.CachedRuntimeTexturePixels + mod.LoadingTexturePixels + texturePixels > 67108864 || num + texturePixels > 134217728)
		{
			diagnostic = "runtime decoded texture pixel budget exceeded";
			return false;
		}
		return true;
	}

	private static bool CanAdmitDecodedAudio(LoadedMod mod, long decodedAudioBytes, out string diagnostic)
	{
		diagnostic = "";
		if (decodedAudioBytes <= 0)
		{
			return true;
		}
		long num = LoadedMods.Sum((LoadedMod item) => item.CachedRuntimeDecodedAudioBytes) + mod.CachedRuntimeDecodedAudioBytes;
		if (mod.CachedRuntimeDecodedAudioBytes + decodedAudioBytes > 134217728 || num + decodedAudioBytes > 268435456)
		{
			diagnostic = "runtime decoded audio PCM byte budget exceeded";
			return false;
		}
		return true;
	}

	private static bool TryInferCharacterScene(string normalizedPath, out string key, string folder = "Scene")
	{
		key = "";
		string[] array = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 6 || !array[0].Equals("Resources", StringComparison.OrdinalIgnoreCase) || !array[1].Equals("Characters", StringComparison.OrdinalIgnoreCase) || !IsKnownCharacterCategory(array[2]) || !array[4].Equals(folder, StringComparison.OrdinalIgnoreCase) || !Path.GetExtension(array[5]).Equals(".tscn", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(array[5]);
		if (string.IsNullOrWhiteSpace(array[3]) || !fileNameWithoutExtension.Equals(array[3], StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		key = array[3];
		return true;
	}

	private static bool IsKnownCharacterCategory(string category)
	{
		if (!category.Equals("Plants", StringComparison.OrdinalIgnoreCase) && !category.Equals("Zombies", StringComparison.OrdinalIgnoreCase) && !category.Equals("Props", StringComparison.OrdinalIgnoreCase) && !category.Equals("Vases", StringComparison.OrdinalIgnoreCase) && !category.Equals("Mowers", StringComparison.OrdinalIgnoreCase) && !category.Equals("Items", StringComparison.OrdinalIgnoreCase) && !category.Equals("Graves", StringComparison.OrdinalIgnoreCase))
		{
			return category.Equals("Craters", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsCharacterPackageDependency(string relativePath)
	{
		string[] array = (relativePath ?? "").Replace('\\', '/').Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length >= 5 && array[0].Equals("Resources", StringComparison.OrdinalIgnoreCase) && array[1].Equals("Characters", StringComparison.OrdinalIgnoreCase))
		{
			return IsKnownCharacterCategory(array[2]);
		}
		return false;
	}

	private static bool IsExecutablePackageFile(string relativePath)
	{
		string text = Path.GetExtension(relativePath ?? "").ToLowerInvariant();
		if (text != null)
		{
			int length = text.Length;
			if (length != 3)
			{
				if (length == 4)
				{
					switch (text[1])
					{
					case 'd':
						break;
					case 'e':
						goto IL_009b;
					case 'b':
						goto IL_00aa;
					case 'c':
						goto IL_00b9;
					case 'p':
						goto IL_00c8;
					default:
						goto IL_00d9;
					}
					if (text == ".dll")
					{
						goto IL_00d5;
					}
				}
			}
			else
			{
				char c = text[1];
				if (c != 'c')
				{
					if (c == 'g' && text == ".gd")
					{
						goto IL_00d5;
					}
				}
				else if (text == ".cs")
				{
					goto IL_00d5;
				}
			}
		}
		goto IL_00d9;
		IL_00aa:
		if (text == ".bat")
		{
			goto IL_00d5;
		}
		goto IL_00d9;
		IL_00d9:
		return false;
		IL_009b:
		if (text == ".exe")
		{
			goto IL_00d5;
		}
		goto IL_00d9;
		IL_00b9:
		if (text == ".cmd")
		{
			goto IL_00d5;
		}
		goto IL_00d9;
		IL_00d5:
		return true;
		IL_00c8:
		if (text == ".ps1")
		{
			goto IL_00d5;
		}
		goto IL_00d9;
	}

	private static bool PrepareSafeCharacterPackage(LoadedMod mod, RuntimeCandidate candidate)
	{
		string[] array = candidate.RelativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 6)
		{
			return false;
		}
		string value = string.Join('/', array.Take(4)) + "/";
		for (int i = 0; i < mod.RelativeFiles.Count && i < mod.ExtractedFiles.Count; i++)
		{
			string text = mod.RelativeFiles[i].Replace('\\', '/');
			if (text.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				string text2 = Path.GetExtension(text).ToLowerInvariant();
				if ((text2 == ".scn" || text2 == ".res") ? true : false)
				{
					AddDiagnostic(mod, "character package rejected unsafe binary dependency: " + text);
					return false;
				}
				bool flag = ((text2 == ".tscn" || text2 == ".tres") ? true : false);
				if (flag && !SanitizeCharacterTextResource(mod, text, mod.ExtractedFiles[i], text2 == ".tscn"))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool SanitizeCharacterTextResource(LoadedMod mod, string relativePath, string absolutePath, bool canStripScripts)
	{
		string text = File.ReadAllText(absolutePath);
		if (text.Contains("type=\"GDScript\"", StringComparison.OrdinalIgnoreCase) || text.Contains("type=\"CSharpScript\"", StringComparison.OrdinalIgnoreCase))
		{
			AddDiagnostic(mod, "character package rejected embedded script: " + relativePath);
			return false;
		}
		List<string> blockedIds = new List<string>();
		List<string> list = new List<string>();
		bool flag = false;
		string[] array = text.Replace("\r\n", "\n").Split('\n');
		foreach (string text2 in array)
		{
			if (text2.Contains("[ext_resource", StringComparison.OrdinalIgnoreCase) && text2.Contains("type=\"Script\"", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = ExtractAttribute(text2, "path");
				if (!text3.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
				{
					if (!canStripScripts)
					{
						AddDiagnostic(mod, "character package rejected untrusted script reference: " + relativePath + ": " + text3);
						return false;
					}
					string text4 = ExtractAttribute(text2, "id");
					if (!string.IsNullOrWhiteSpace(text4))
					{
						blockedIds.Add(text4);
					}
					AddDiagnostic(mod, "blocked untrusted character script reference: " + relativePath + ": " + text3);
					flag = true;
					continue;
				}
			}
			list.Add(text2);
		}
		if (blockedIds.Count > 0)
		{
			list.RemoveAll((string line) => blockedIds.Any((string id) => line.Contains("ExtResource(\"" + id + "\")", StringComparison.Ordinal)));
		}
		if (flag)
		{
			File.WriteAllText(absolutePath, string.Join('\n', list));
		}
		return true;
	}

	private static string ExtractAttribute(string line, string attribute)
	{
		Match match = Regex.Match(line ?? "", "(?:^|\\s)" + Regex.Escape(attribute) + "=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (!match.Success)
		{
			return "";
		}
		return match.Groups[1].Value;
	}

	private static List<(string Key, bool Override)> GetManifestDeclarations(XWModManifest manifest, string category)
	{
		List<(string, bool)> list = new List<(string, bool)>();
		AddManifestDeclarations(list, manifest?.Provides, category, allowOverride: false);
		AddManifestDeclarations(list, manifest?.Overrides, category, allowOverride: true);
		return (from @group in list.GroupBy<(string, bool), string>(((string Key, bool Override) item) => item.Key, ManifestKeyComparer(manifest))
			select (Key: @group.Key, @group.Any(((string Key, bool Override) item) => item.Override))).ToList();
	}

	private static void AddManifestDeclarations(List<(string Key, bool Override)> result, System.Collections.Generic.Dictionary<string, List<string>> entries, string category, bool allowOverride)
	{
		if (entries == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<string>> entry in entries)
		{
			if (!NormalizeCategory(entry.Key).Equals(NormalizeCategory(category), StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			foreach (string item in entry.Value ?? new List<string>())
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					result.Add((item.Trim(), allowOverride));
				}
			}
		}
	}

	private static bool IsLevelPackageDependency(string relativePath)
	{
		string text = (relativePath ?? "").Replace('\\', '/').Trim('/');
		bool flag = text.StartsWith("Resources/Levels/", StringComparison.OrdinalIgnoreCase);
		if (flag)
		{
			string text2 = Path.GetExtension(text).ToLowerInvariant();
			bool flag2 = ((text2 == ".tres" || text2 == ".res") ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private static bool ValidateManifestRegistrations(LoadedMod mod, string ownerId)
	{
		bool result = ValidateManifestRegistrations(mod, ownerId, mod.Manifest?.Provides, isOverride: false) & ValidateManifestRegistrations(mod, ownerId, mod.Manifest?.Overrides, isOverride: true);
		foreach (XWModContentEntry content in mod.Manifest.ContentEntries)
		{
			XWModContentId identity = ((content.Target == null) ? new XWModContentId(ownerId, content.Category, content.Key) : ContentTarget(content.Target));
			if (!XWModRuntimeRegistry.GetRegistrationStack(identity.Category, identity.RuntimeKey).Any((XWModRuntimeRegistry.Registration registration) => string.Equals(registration.OwnerMod, ownerId, StringComparison.OrdinalIgnoreCase) && registration.ContentId == identity && registration.IsOverride == (content.Target != null)))
			{
				result = false;
				AddDiagnostic(mod, $"required content entry is not registered: {ownerId}: {identity}");
			}
		}
		return result;
	}

	private static bool ValidateManifestRegistrations(LoadedMod mod, string ownerId, System.Collections.Generic.Dictionary<string, List<string>> entries, bool isOverride)
	{
		if (entries == null)
		{
			return true;
		}
		bool result = true;
		foreach (KeyValuePair<string, List<string>> pair in entries)
		{
			foreach (string key in pair.Value ?? new List<string>())
			{
				if (!string.IsNullOrWhiteSpace(key) && !(from registration in XWModRuntimeRegistry.GetRegistrations().Values.SelectMany((System.Collections.Generic.Dictionary<string, XWModRuntimeRegistry.Registration> registrations) => registrations.Values)
					where NormalizeCategory(registration.Category).Equals(NormalizeCategory(pair.Key), StringComparison.OrdinalIgnoreCase) && registration.Key.Equals(key.Trim(), ManifestKeyComparison(mod.Manifest))
					select registration).SelectMany((XWModRuntimeRegistry.Registration registration) => XWModRuntimeRegistry.GetRegistrationStack(registration.Category, registration.Key)).Any((XWModRuntimeRegistry.Registration registration) => registration.OwnerMod.Equals(ownerId, StringComparison.OrdinalIgnoreCase) && (registration.IsOverride == isOverride || (!isOverride && registration.OverridesBuiltIn))))
				{
					result = false;
					AddDiagnostic(mod, $"required manifest runtime entry is not registered: {ownerId}: {(isOverride ? "overrides" : "provides")}: {pair.Key}/{key.Trim()}");
				}
			}
		}
		return result;
	}

	private static void ParseManifest(string json, LoadedMod mod)
	{
		ParseManifest(json, out var manifest, out var info);
		mod.Manifest = manifest;
		mod.Info = info;
	}

	private static void ParseManifest(string json, out XWModManifest manifest, out ModExporter.ModInfo info)
	{
		ParseManifest(json, out manifest, out info, out var _);
	}

	private static void ParseManifest(string json, out XWModManifest manifest, out ModExporter.ModInfo info, out XWModPackageMetadataPreview preview)
	{
		manifest = null;
		info = null;
		preview = null;
		using JsonDocument jsonDocument = JsonDocument.Parse(json);
		JsonElement rootElement = jsonDocument.RootElement;
		JsonElement value;
		bool flag = XWModMetadataValidation.TryGetJsonField(rootElement, "schemaVersion", out value);
		bool flag2 = flag || XWModMetadataValidation.TryGetJsonField(rootElement, "id", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "dependencies", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "conflicts", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "provides", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "overrides", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "scripts", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "settings", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "runtimeAssembly", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "runtimeEntryType", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "runtimeApiVersion", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "runtimeAssemblyPolicy", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "blueprints", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "translations", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "resources", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "contentEntries", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "privateResources", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "sharedAssemblies", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "requiredCapabilities", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "optionalDependencies", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "loadBefore", out value) || XWModMetadataValidation.TryGetJsonField(rootElement, "loadAfter", out value);
		string text = XWModMetadataValidation.ReadJsonString(rootElement, flag2 ? "id" : "name");
		string name = XWModMetadataValidation.ReadJsonString(rootElement, "name");
		preview = new XWModPackageMetadataPreview(text, name);
		XWModMetadataValidation.RequireValid(XWModMetadataValidation.ReadJsonString(rootElement, "author"), text, XWModMetadataValidation.ReadJsonString(rootElement, "version"));
		if (flag2)
		{
			manifest = JsonSerializer.Deserialize(json, XWModJsonContext.Default.XWModManifest);
			if (manifest != null && !flag)
			{
				manifest.SchemaVersion = 1;
			}
		}
		else
		{
			info = JsonSerializer.Deserialize(json, XWModJsonContext.Default.ModInfo);
		}
	}

	private static void NormalizeManifest(XWModManifest manifest, ModExporter.ModInfo info, string pmodPath)
	{
		if (manifest != null)
		{
			if (!XWModRuntimeCompatibility.ValidatePackage(manifest, out var diagnostic))
			{
				throw new InvalidDataException(diagnostic);
			}
			if (manifest.SchemaVersion <= 0)
			{
				manifest.SchemaVersion = 1;
			}
			if (manifest.SchemaVersion > 3)
			{
				throw new InvalidDataException($"unsupported mod schema version: {manifest.SchemaVersion}");
			}
			XWModManifestSyncService.NormalizeManifestCollections(manifest);
			XWModManifest xWModManifest = manifest;
			if (xWModManifest.ContentEntries == null)
			{
				List<XWModContentEntry> list = (xWModManifest.ContentEntries = new List<XWModContentEntry>());
			}
			xWModManifest = manifest;
			if (xWModManifest.PrivateResources == null)
			{
				List<string> list3 = (xWModManifest.PrivateResources = new List<string>());
			}
			xWModManifest = manifest;
			if (xWModManifest.SharedAssemblies == null)
			{
				List<string> list3 = (xWModManifest.SharedAssemblies = new List<string>());
			}
			xWModManifest = manifest;
			if (xWModManifest.OptionalDependencies == null)
			{
				List<XWModDependency> list6 = (xWModManifest.OptionalDependencies = new List<XWModDependency>());
			}
			xWModManifest = manifest;
			if (xWModManifest.LoadBefore == null)
			{
				List<string> list3 = (xWModManifest.LoadBefore = new List<string>());
			}
			xWModManifest = manifest;
			if (xWModManifest.LoadAfter == null)
			{
				List<string> list3 = (xWModManifest.LoadAfter = new List<string>());
			}
			xWModManifest = manifest;
			if (xWModManifest.RequiredCapabilities == null)
			{
				List<string> list3 = (xWModManifest.RequiredCapabilities = new List<string>());
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pmodPath ?? "mod");
			if (string.IsNullOrWhiteSpace(manifest.Id))
			{
				manifest.Id = fileNameWithoutExtension;
			}
			if (string.IsNullOrWhiteSpace(manifest.Name))
			{
				manifest.Name = (string.IsNullOrWhiteSpace(info?.Name) ? manifest.Id : info.Name);
			}
			if (string.IsNullOrWhiteSpace(manifest.Version))
			{
				manifest.Version = (string.IsNullOrWhiteSpace(info?.Version) ? "1.0.0" : info.Version);
			}
			string text = (manifest.RuntimeAssemblyPolicy ?? "").Trim();
			if (!string.IsNullOrWhiteSpace(text) && !text.Equals("required", StringComparison.OrdinalIgnoreCase) && !text.Equals("optional", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("invalid runtimeAssemblyPolicy: " + manifest.RuntimeAssemblyPolicy);
			}
			if (!string.IsNullOrWhiteSpace(manifest.RuntimeEntryType) && string.IsNullOrWhiteSpace(manifest.RuntimeAssembly))
			{
				throw new InvalidDataException("runtimeEntryType requires runtimeAssembly");
			}
		}
	}

	private static XWModManifest CreateLegacyManifest(ModExporter.ModInfo info, string pmodPath)
	{
		if (info == null)
		{
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pmodPath ?? "mod");
		return new XWModManifest
		{
			SchemaVersion = 1,
			Id = (string.IsNullOrWhiteSpace(info.Name) ? fileNameWithoutExtension : info.Name),
			Name = (string.IsNullOrWhiteSpace(info.Name) ? fileNameWithoutExtension : info.Name),
			Version = (string.IsNullOrWhiteSpace(info.Version) ? "1.0.0" : info.Version),
			Author = (info.Author ?? ""),
			Description = (info.Description ?? "")
		};
	}

	private static bool TryGetGodotResourcePath(string absolutePath, out string resourcePath)
	{
		resourcePath = ProjectSettings.LocalizePath(absolutePath).Replace('\\', '/');
		if (resourcePath.StartsWith("user://", StringComparison.OrdinalIgnoreCase) || resourcePath.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		string fullPath = Path.GetFullPath(absolutePath);
		(string, string)[] array = new(string, string)[2]
		{
			("user://", ProjectSettings.GlobalizePath("user://")),
			("res://", ProjectSettings.GlobalizePath("res://"))
		};
		for (int i = 0; i < array.Length; i++)
		{
			(string, string) tuple = array[i];
			string item = tuple.Item1;
			string text = Path.GetFullPath(tuple.Item2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string value = text + Path.DirectorySeparatorChar;
			if (fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				string text2 = Path.GetRelativePath(text, fullPath).Replace('\\', '/');
				if (!text2.StartsWith("../", StringComparison.Ordinal) && !(text2 == ".."))
				{
					resourcePath = item + text2;
					return true;
				}
			}
		}
		resourcePath = "";
		return false;
	}

	private static bool TryPreparePackagedAnimationAtlasProfile(LoadedMod mod, RuntimeCandidate candidate, AdobeAnimateAtlasProfile profile, out string diagnostic)
	{
		diagnostic = "";
		string text = profile?.ManifestPath?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(text))
		{
			diagnostic = "manifest path is empty";
			return false;
		}
		string resourcePath = "";
		string extractedResource;
		bool flag = TryResolvePackagedResource(mod, candidate.ExtractedPath, text, out extractedResource) && TryGetGodotResourcePath(extractedResource, out resourcePath);
		if (flag)
		{
			profile.ManifestPath = resourcePath;
		}
		else if (!IsBuiltInAtlasManifestPath(text))
		{
			diagnostic = "manifest is not a portable package resource: " + text;
			return false;
		}
		AdobeAnimateGlobalAtlasManifest adobeAnimateGlobalAtlasManifest = (flag ? ResourceLoader.Load<AdobeAnimateGlobalAtlasManifest>(profile.ManifestPath, "", ResourceLoader.CacheMode.Replace) : profile.LoadManifest());
		if (!GodotObject.IsInstanceValid(adobeAnimateGlobalAtlasManifest))
		{
			diagnostic = "manifest could not be loaded after relocation: " + profile.ManifestPath;
			return false;
		}
		if (flag && !TryRelocatePackagedAtlasManifestPaths(mod, extractedResource, adobeAnimateGlobalAtlasManifest, out diagnostic))
		{
			return false;
		}
		return true;
	}

	private static bool TryRelocatePackagedAtlasManifestPaths(LoadedMod mod, string extractedManifest, AdobeAnimateGlobalAtlasManifest manifest, out string diagnostic)
	{
		diagnostic = "";
		if (!TryRelocatePackagedPath(mod, extractedManifest, manifest.AtlasTextureArrayPath, out var relocatedPath, out diagnostic) || !TryRelocatePackagedPath(mod, extractedManifest, manifest.PoseTextureArrayPath, out var relocatedPath2, out diagnostic))
		{
			return false;
		}
		manifest.AtlasTextureArrayPath = relocatedPath;
		manifest.PoseTextureArrayPath = relocatedPath2;
		for (int i = 0; i < manifest.PoseAtlasPagePaths.Count; i++)
		{
			if (!TryRelocatePackagedPath(mod, extractedManifest, manifest.PoseAtlasPagePaths[i], out var relocatedPath3, out diagnostic))
			{
				return false;
			}
			manifest.PoseAtlasPagePaths[i] = relocatedPath3;
		}
		for (int j = 0; j < manifest.ExternalTexturePaths.Count; j++)
		{
			if (!TryRelocatePackagedPath(mod, extractedManifest, manifest.ExternalTexturePaths[j], out var relocatedPath4, out diagnostic))
			{
				return false;
			}
			manifest.ExternalTexturePaths[j] = relocatedPath4;
		}
		for (int k = 0; k < manifest.SourceKeys.Count; k++)
		{
			string text = manifest.SourceKeys[k];
			if (LooksLikeResourcePath(text))
			{
				if (!TryRelocatePackagedPath(mod, extractedManifest, text, out var relocatedPath5, out diagnostic))
				{
					return false;
				}
				manifest.SourceKeys[k] = relocatedPath5;
			}
		}
		return true;
	}

	private static bool TryRelocatePackagedPath(LoadedMod mod, string ownerExtractedPath, string declaredPath, out string relocatedPath, out string diagnostic)
	{
		relocatedPath = declaredPath ?? "";
		diagnostic = "";
		if (string.IsNullOrWhiteSpace(declaredPath))
		{
			return true;
		}
		if (TryResolvePackagedResource(mod, ownerExtractedPath, declaredPath, out var extractedResource) && TryGetGodotResourcePath(extractedResource, out relocatedPath))
		{
			return true;
		}
		if (declaredPath.StartsWith("res://", StringComparison.OrdinalIgnoreCase) && ResourceLoader.Exists(declaredPath))
		{
			relocatedPath = declaredPath;
			return true;
		}
		diagnostic = "manifest dependency is not portable or missing: " + declaredPath;
		return false;
	}

	private static bool TryResolvePackagedResource(LoadedMod mod, string ownerExtractedPath, string declaredPath, out string extractedResource)
	{
		extractedResource = "";
		if (mod == null || string.IsNullOrWhiteSpace(ownerExtractedPath) || string.IsNullOrWhiteSpace(declaredPath))
		{
			return false;
		}
		string text = declaredPath.Replace('\\', '/').Trim();
		List<string> list = new List<string>();
		if (text.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
		{
			list.Add(text.Substring(text.IndexOf("://", StringComparison.Ordinal) + 3));
		}
		else if (!Path.IsPathRooted(declaredPath))
		{
			try
			{
				string fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ownerExtractedPath) ?? mod.ExtractRoot, text.Replace('/', Path.DirectorySeparatorChar)));
				string item = Path.GetRelativePath(mod.ExtractRoot, fullPath).Replace('\\', '/');
				list.Add(item);
			}
			catch
			{
			}
			string text2 = text.TrimStart('/');
			if (text2.Split('/', StringSplitOptions.RemoveEmptyEntries).All((string segment) => !(segment == ".") && !(segment == "..")))
			{
				list.Add(text2);
			}
		}
		else
		{
			string[] array = new string[3] { "/Resources/", "/Assets/", "/Battle/" };
			foreach (string value in array)
			{
				int num2 = text.IndexOf(value, StringComparison.OrdinalIgnoreCase);
				if (num2 >= 0)
				{
					list.Add(text.Substring(num2 + 1));
				}
			}
		}
		foreach (string item2 in list.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (ValidateArchiveEntryPath(mod.ExtractRoot, item2, out var safeRelative, out var outputPath) && mod.RelativeFiles.Any((string path) => string.Equals(path.Replace('\\', '/'), safeRelative, StringComparison.OrdinalIgnoreCase)) && File.Exists(outputPath))
			{
				extractedResource = outputPath;
				return true;
			}
		}
		return false;
	}

	private static bool LooksLikeResourcePath(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		string text = value.Replace('\\', '/');
		if (!text.Contains("://", StringComparison.Ordinal) && !text.StartsWith(".", StringComparison.Ordinal) && !text.StartsWith("/", StringComparison.Ordinal))
		{
			return !string.IsNullOrWhiteSpace(Path.GetExtension(text));
		}
		return true;
	}

	private static bool IsBuiltInAtlasManifestPath(string path)
	{
		return (path ?? "").Replace('\\', '/').StartsWith("res://addons/AdobeAnimateEditor/GeneratedAtlas/", StringComparison.OrdinalIgnoreCase);
	}

	private static bool HasExplicitRuntimeEntries(XWModManifest manifest)
	{
		if (manifest != null)
		{
			List<XWModContentEntry> contentEntries = manifest.ContentEntries;
			if (contentEntries == null || contentEntries.Count <= 0)
			{
				System.Collections.Generic.Dictionary<string, List<string>> provides = manifest.Provides;
				if (provides == null || !provides.Any(delegate(KeyValuePair<string, List<string>> pair)
				{
					List<string> value = pair.Value;
					return value != null && value.Count > 0;
				}))
				{
					return manifest.Overrides?.Any(delegate(KeyValuePair<string, List<string>> pair)
					{
						List<string> value = pair.Value;
						return value != null && value.Count > 0;
					}) ?? false;
				}
			}
			return true;
		}
		return false;
	}

	private static StringComparison ManifestKeyComparison(XWModManifest manifest)
	{
		if ((manifest?.SchemaVersion ?? 1) > 2)
		{
			return StringComparison.Ordinal;
		}
		return StringComparison.OrdinalIgnoreCase;
	}

	private static StringComparer ManifestKeyComparer(XWModManifest manifest)
	{
		if ((manifest?.SchemaVersion ?? 1) > 2)
		{
			return StringComparer.Ordinal;
		}
		return StringComparer.OrdinalIgnoreCase;
	}

	private static bool ManifestContains(System.Collections.Generic.Dictionary<string, List<string>> entries, string category, string key, StringComparison keyComparison = StringComparison.Ordinal)
	{
		if (entries == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, List<string>> entry in entries)
		{
			if (NormalizeCategory(entry.Key).Equals(NormalizeCategory(category), StringComparison.OrdinalIgnoreCase))
			{
				return entry.Value?.Any((string item) => string.Equals(item, key, keyComparison)) ?? false;
			}
		}
		return false;
	}

	private static string NormalizeCategory(string category)
	{
		string text = (category ?? "").Trim();
		switch (text.ToLowerInvariant())
		{
		case "level":
		case "levels":
			return "Level";
		case "image":
		case "images":
		case "texture":
		case "textures":
			return "Texture";
		case "audio":
		case "audios":
			return "Audio";
		case "animationatlases":
		case "atlasprofile":
		case "animationatlas":
		case "atlasprofiles":
			return "AnimationAtlas";
		case "bgm":
		case "bgms":
			return "BGM";
		case "map":
		case "maps":
			return "Map";
		case "projectile":
		case "projectiles":
			return "Projectile";
		case "charactersprite":
			return "CharacterSprite";
		case "characters":
		case "character":
			return "Character";
		case "cards":
		case "packet":
		case "packets":
		case "card":
			return "Packet";
		case "packetbank":
		case "packetbanks":
			return "PacketBank";
		case "tutorial":
		case "tutorials":
			return "Tutorial";
		case "talks":
		case "npctalk":
		case "npctalks":
		case "talk":
			return "NpcTalk";
		case "collectables":
		case "collectable":
			return "Collectable";
		case "shovel":
		case "shovels":
			return "Shovel";
		case "mower":
		case "mowers":
			return "Mower";
		case "shops":
		case "shop":
			return "Shop";
		case "survival":
		case "survivals":
			return "Survival";
		case "feature":
		case "features":
			return "Feature";
		case "process":
		case "processes":
			return "Process";
		case "projectilechange":
		case "projectilechanges":
			return "ProjectileChange";
		default:
			return text;
		}
	}

	private static string GetOwnerId(LoadedMod mod)
	{
		string text = mod?.Manifest?.Id;
		if (string.IsNullOrWhiteSpace(text))
		{
			text = mod?.Info?.Name;
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			text = Path.GetFileNameWithoutExtension(mod?.FilePath ?? "mod");
		}
		return text.Trim();
	}

	private static string SanitizeCacheName(string name)
	{
		char[] invalid = Path.GetInvalidFileNameChars();
		string text = new string((name ?? "mod").Select((char character) => (!invalid.Contains(character)) ? character : '_').ToArray());
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return "mod";
	}

	private static IReadOnlyList<LoadedMod> LoadingBudgetOwners(LoadedMod mod)
	{
		LoadingAssetOwners.RemoveAll((WeakReference<LoadedMod> reference) => !reference.TryGetTarget(out var target));
		LoadedMod[] result = LoadingAssetOwners
			.Select((WeakReference<LoadedMod> reference) => reference.TryGetTarget(out var owner) ? owner : null)
			.Where((LoadedMod owner) => owner != null)
			.Concat(LoadedMods).Append(mod).Distinct()
			.ToArray();
		if (!LoadingAssetOwners.Any((WeakReference<LoadedMod> reference) => reference.TryGetTarget(out var target) && target == mod))
		{
			LoadingAssetOwners.Add(new WeakReference<LoadedMod>(mod));
		}
		return result;
	}

	internal static void ValidateLoadingPresentation(XWModManifest manifest, IReadOnlyList<string> files)
	{
		XWModLoadingPresentation xWModLoadingPresentation = manifest?.LoadingPresentation;
		if (xWModLoadingPresentation == null)
		{
			return;
		}
		if (manifest.SchemaVersion < 3)
		{
			throw new InvalidDataException("loadingPresentation requires schemaVersion 3");
		}
		string[] array = new string[2] { xWModLoadingPresentation.StartupScene, xWModLoadingPresentation.TransitionScene };
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (!manifest.RequiredCapabilities.Contains<string>("loading-presentation-v1", StringComparer.Ordinal))
				{
					throw new InvalidDataException("loadingPresentation requires loading-presentation-v1");
				}
				if (!ValidateArchiveEntryPath(Path.GetTempPath(), text, out var relativePath, out var _) || relativePath != text || Path.GetExtension(text) != ".tscn" || !files.Contains<string>(text, StringComparer.Ordinal) || !manifest.PrivateResources.Contains<string>(text, StringComparer.Ordinal))
				{
					throw new InvalidDataException("loading scene must be a packaged private .tscn: " + text);
				}
			}
		}
	}

	internal static bool CollectLoadingPresentationFiles(string projectRoot, XWModManifest manifest)
	{
		if (manifest.LoadingPresentation == null)
		{
			return false;
		}
		LoadedMod mod = new LoadedMod
		{
			ExtractRoot = Path.GetFullPath(projectRoot)
		};
		mod.RelativeFiles = (from path in Directory.EnumerateFiles(projectRoot, "*", SearchOption.AllDirectories)
			select Path.GetRelativePath(projectRoot, path).Replace('\\', '/')).ToList();
		HashSet<string> collected = new HashSet<string>(StringComparer.Ordinal);
		Visit(manifest.LoadingPresentation.StartupScene);
		Visit(manifest.LoadingPresentation.TransitionScene);
		bool result = false;
		XWModManifest xWModManifest = manifest;
		if (xWModManifest.PrivateResources == null)
		{
			List<string> list = (xWModManifest.PrivateResources = new List<string>());
		}
		xWModManifest = manifest;
		if (xWModManifest.RequiredCapabilities == null)
		{
			List<string> list = (xWModManifest.RequiredCapabilities = new List<string>());
		}
		foreach (string item in collected.OrderBy<string, string>((string path) => path, StringComparer.Ordinal))
		{
			if (!manifest.PrivateResources.Contains<string>(item, StringComparer.Ordinal))
			{
				manifest.PrivateResources.Add(item);
				result = true;
			}
		}
		if (collected.Count > 0 && !manifest.RequiredCapabilities.Contains<string>("loading-presentation-v1", StringComparer.Ordinal))
		{
			manifest.RequiredCapabilities.Add("loading-presentation-v1");
			result = true;
		}
		return result;
		void Visit(string relative)
		{
			if (!string.IsNullOrEmpty(relative) && collected.Add(relative))
			{
				if (!ValidateArchiveEntryPath(projectRoot, relative, out var relativePath, out var outputPath) || !File.Exists(outputPath))
				{
					throw new InvalidDataException("missing loading asset: " + relative);
				}
				relativePath = Path.GetExtension(outputPath);
				if ((relativePath == ".tscn" || relativePath == ".tres") ? true : false)
				{
					XWModProjectLayout.MakeSavedTextResourceReferencesPortable(outputPath, projectRoot);
					foreach (string item2 in from line in File.ReadLines(outputPath)
						where line.TrimStart().StartsWith("[ext_resource", StringComparison.Ordinal)
						select line)
					{
						string declaredPath = ExtractAttribute(item2, "path");
						if (ExtractAttribute(item2, "type") != "Script" && TryResolvePackagedResource(mod, outputPath, declaredPath, out var extractedResource))
						{
							Visit(Path.GetRelativePath(projectRoot, extractedResource).Replace('\\', '/'));
						}
					}
				}
			}
		}
	}

	internal static PackedScene PrepareLoadingScene(LoadedMod mod, string relative, XWLoadingKind kind)
	{
		if (mod.LoadingScenes.TryGetValue((relative, kind), out var value))
		{
			return value;
		}
		ValidateLoadingPresentation(mod.Manifest, mod.RelativeFiles);
		IReadOnlyList<LoadedMod> source = LoadingBudgetOwners(mod);
		HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string root = Path.GetFullPath(mod.ExtractRoot);
		HashSet<string> privatePaths = new HashSet<string>(mod.Manifest.PrivateResources, StringComparer.Ordinal);
		System.Collections.Generic.Dictionary<string, string[]> texts = new System.Collections.Generic.Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
		if (!ValidateArchiveEntryPath(root, relative, out var _, out var outputPath))
		{
			throw new InvalidDataException("invalid loading scene path");
		}
		Validate(outputPath);
		System.Collections.Generic.Dictionary<string, string> dictionary = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item2 in visited)
		{
			string text = Path.GetExtension(item2).ToLowerInvariant();
			Resource resource = null;
			long length = new FileInfo(item2).Length;
			if (!mod.LoadingAccountedFiles.Contains(item2))
			{
				if (mod.LoadingSourceBytes + mod.CachedRuntimeSourceBytes + length > 268435456 || source.Sum((LoadedMod owner) => owner.LoadingSourceBytes + owner.CachedRuntimeSourceBytes) + length > 536870912)
				{
					throw new InvalidDataException("loading assets exceed source budget");
				}
				mod.LoadingSourceBytes += length;
				mod.LoadingAccountedFiles.Add(item2);
			}
			try
			{
				if (XWModExternalMediaLoader.IsSupportedTextureExtension(text))
				{
					if (!XWModExternalMediaLoader.TryLoadTexture(item2, text, out var texture, out var diagnostic))
					{
						throw new InvalidDataException(diagnostic);
					}
					resource = texture;
					long num = (long)texture.GetWidth() * (long)texture.GetHeight();
					if (mod.LoadingTexturePixels + mod.CachedRuntimeTexturePixels + num > 67108864 || source.Sum((LoadedMod owner) => owner.LoadingTexturePixels + owner.CachedRuntimeTexturePixels) + num > 134217728)
					{
						throw new InvalidDataException("loading textures exceed pixel budget");
					}
					mod.LoadingTexturePixels += num;
					goto IL_02af;
				}
				if ((text == ".ttf" || text == ".otf") ? true : false)
				{
					if (((FontFile)(resource = new FontFile())).LoadDynamicFont(item2) != Error.Ok)
					{
						throw new InvalidDataException("invalid loading font: " + Path.GetFileName(item2));
					}
					goto IL_02af;
				}
				goto end_IL_01ad;
				IL_02af:
				string text2 = Path.Combine(root, ".loading-native", Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(item2))) + ".res");
				Directory.CreateDirectory(Path.GetDirectoryName(text2));
				if (!TryGetGodotResourcePath(text2, out var resourcePath) || ResourceSaver.Save(resource, resourcePath, ResourceSaver.SaverFlags.None) != Error.Ok)
				{
					throw new InvalidDataException("cannot save native loading asset");
				}
				dictionary[item2] = resourcePath;
				mod.LoadingNativeAssets.Add(text2);
				end_IL_01ad:;
			}
			finally
			{
				resource?.Dispose();
			}
		}
		foreach (KeyValuePair<string, string[]> item3 in texts)
		{
			bool flag = false;
			for (int num2 = 0; num2 < item3.Value.Length; num2++)
			{
				string text3 = item3.Value[num2];
				if (text3.TrimStart().StartsWith("[ext_resource", StringComparison.Ordinal) && !(ExtractAttribute(text3, "type") == "Script"))
				{
					string text4 = ExtractAttribute(text3, "path");
					if (text4.StartsWith("user://", StringComparison.Ordinal) && dictionary.TryGetValue(Path.GetFullPath(ProjectSettings.GlobalizePath(text4)), out var value2))
					{
						item3.Value[num2] = text3.Replace("path=\"" + text4 + "\"", "path=\"" + value2 + "\"", StringComparison.Ordinal);
						flag = true;
					}
				}
			}
			if (flag)
			{
				File.WriteAllLines(item3.Key, item3.Value);
			}
		}
		if (!TryGetGodotResourcePath(outputPath, out var resourcePath2))
		{
			throw new InvalidDataException("cannot localize loading scene");
		}
		PackedScene packedScene = ResourceLoader.Load<PackedScene>(resourcePath2, null, ResourceLoader.CacheMode.Reuse) ?? throw new InvalidDataException("loading scene cannot be loaded");
		Node node = null;
		try
		{
			node = packedScene.Instantiate(PackedScene.GenEditState.Disabled);
			if (!((node as XWModLoadingView) ?? throw new InvalidDataException("loading scene root must use XWModLoadingView")).TryBind(kind, out var error))
			{
				throw new InvalidDataException(error);
			}
			mod.LoadingScenes.Add((relative, kind), packedScene);
			return packedScene;
		}
		finally
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.Free();
			}
		}
		string Resolve(string reference)
		{
			string fullPath = Path.GetFullPath(ProjectSettings.GlobalizePath(reference));
			string item = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
			if (mod.LoadingNativeAssets.Contains(fullPath))
			{
				return fullPath;
			}
			if (!privatePaths.Contains(item) || !File.Exists(fullPath))
			{
				throw new InvalidDataException("loading dependency must be private to this package: " + reference);
			}
			return fullPath;
		}
		void Validate(string absolute)
		{
			if (!mod.LoadingNativeAssets.Contains(absolute) && visited.Add(absolute))
			{
				string text5 = Path.GetExtension(absolute).ToLowerInvariant();
				if ((!(text5 == ".tscn") && !(text5 == ".tres")) || 1 == 0)
				{
					bool flag2 = !XWModExternalMediaLoader.IsSupportedTextureExtension(text5);
					if (flag2)
					{
						bool flag3 = ((text5 == ".ttf" || text5 == ".otf") ? true : false);
						flag2 = !flag3;
					}
					if (flag2)
					{
						throw new InvalidDataException("unsupported early loading asset: " + text5);
					}
				}
				else
				{
					string[] array = File.ReadAllLines(absolute);
					texts.Add(absolute, array);
					string[] array2 = array;
					foreach (string text6 in array2)
					{
						string text7 = text6.TrimStart();
						if (text7.StartsWith("[connection", StringComparison.Ordinal) || text7.StartsWith("[sub_resource type=\"GDScript\"", StringComparison.Ordinal) || text7.StartsWith("[sub_resource type=\"CSharpScript\"", StringComparison.Ordinal) || Regex.IsMatch(text7, "^tracks/[0-9]+/type\\s*=\\s*\"method\""))
						{
							throw new InvalidDataException("loading views cannot contain scripts, signal callbacks or method animation tracks");
						}
						if (text7.StartsWith("[ext_resource", StringComparison.Ordinal))
						{
							string text8 = ExtractAttribute(text6, "path");
							string text9 = ExtractAttribute(text6, "type");
							if (text9 == "Script")
							{
								if (text8 != "res://addons/ModEditor/ModSystem/XWModLoadingView.cs")
								{
									throw new InvalidDataException("loading views may only use the host XWModLoadingView script");
								}
							}
							else if (text8.StartsWith("user://", StringComparison.Ordinal))
							{
								Validate(Resolve(text8));
							}
							else
							{
								bool flag2 = !text8.StartsWith("res://", StringComparison.Ordinal);
								if (!flag2)
								{
									bool flag3;
									switch (text9)
									{
									case "Texture2D":
									case "FontFile":
									case "Shader":
									case "AudioStream":
										flag3 = true;
										break;
									default:
										flag3 = false;
										break;
									}
									flag2 = !flag3;
								}
								if (flag2)
								{
									throw new InvalidDataException("unsupported host loading dependency: " + text8);
								}
							}
						}
					}
				}
			}
		}
	}

	private static void PrepareDeclaredLoadingScenes(LoadedMod mod)
	{
		XWModLoadingPresentation loadingPresentation = mod.Manifest.LoadingPresentation;
		if (loadingPresentation == null)
		{
			return;
		}
		XWLoadingKind[] array = new XWLoadingKind[2]
		{
			XWLoadingKind.Startup,
			XWLoadingKind.Transition
		};
		foreach (XWLoadingKind xWLoadingKind in array)
		{
			string text = ((xWLoadingKind == XWLoadingKind.Startup) ? loadingPresentation.StartupScene : loadingPresentation.TransitionScene);
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					PrepareLoadingScene(mod, text, xWLoadingKind);
				}
				catch (Exception ex)
				{
					AddDiagnostic(mod, "加载界面声明无效：" + ex.GetBaseException().Message);
				}
			}
		}
	}
}
