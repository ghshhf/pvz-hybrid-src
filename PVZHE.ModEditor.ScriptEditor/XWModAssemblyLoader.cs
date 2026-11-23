using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Godot;
using PVZHE.ModEditor.ModSystem;

namespace PVZHE.ModEditor.ScriptEditor;

public sealed class XWModAssemblyLoader
{
	public sealed class LoadedModAssembly
	{
		private int _callbackCount;

		internal bool CallbacksRegistered;

		internal bool ComponentTypesRegistered;

		internal bool RegistrationsDetached;

		internal bool IsStaged;

		internal bool UnloadRequestInProgress;

		public string ModId { get; set; } = "";

		public string AssemblyPath { get; set; } = "";

		public Assembly Assembly { get; set; }

		public AssemblyLoadContext LoadContext { get; set; }

		public int StateMachineCallbackCount
		{
			get
			{
				return _callbackCount;
			}
			init
			{
				_callbackCount = value;
			}
		}

		public string LastUnloadDiagnostic { get; internal set; } = "";

		public int CharacterComponentRuntimeCount { get; internal set; }

		public bool SupportsCollectibleUnload { get; set; }

		public bool HasSharedContracts { get; set; }

		internal void SetCallbackCount(int count)
		{
			_callbackCount = count;
		}
	}

	internal sealed class ModLoadContext : AssemblyLoadContext
	{
		private readonly IReadOnlyDictionary<string, XWModAssemblyMetadata> _assemblies;

		private readonly HashSet<string> _shared;

		private readonly Dictionary<string, Assembly> _host;

		private readonly object _loadGate = new object();

		private readonly string _owner;

		internal ModLoadContext(string owner, string assemblyPath, IEnumerable<string> probingRoots, bool isCollectible, IReadOnlyCollection<string> sharedAssemblies = null)
			: base("mod:" + owner, isCollectible)
		{
			_owner = owner;
			_host = HostAssemblies();
			_shared = new HashSet<string>((IEnumerable<string>)(((object)sharedAssemblies) ?? ((object)Array.Empty<string>())), StringComparer.OrdinalIgnoreCase);
			IEnumerable<string> roots = new string[1] { Path.GetDirectoryName(assemblyPath) ?? "" }.Concat(probingRoots ?? Array.Empty<string>());
			_assemblies = XWModAssemblyMetadata.BuildIndex(assemblyPath, roots);
			string simpleName = XWModAssemblyMetadata.Read(assemblyPath).SimpleName;
			if (_host.ContainsKey(simpleName) || IsFrameworkAssembly(simpleName) || _shared.Contains(simpleName))
			{
				throw new InvalidDataException("Mod '" + owner + "' 主程序集不能使用宿主、框架或共享契约名称：" + simpleName);
			}
			foreach (string item in _shared)
			{
				if (!_assemblies.TryGetValue(item, out var value))
				{
					throw new FileNotFoundException("Mod '" + owner + "' 声明的共享契约不存在：" + item);
				}
				foreach (string reference in value.References)
				{
					string name = new AssemblyName(reference).Name;
					if (!_host.ContainsKey(name) && !IsFrameworkAssembly(name) && !_shared.Contains(name))
					{
						throw new InvalidDataException($"共享契约 '{item}' 不能引用 Mod 私有程序集 '{name}'。");
					}
				}
			}
			Queue<XWModAssemblyMetadata> queue = new Queue<XWModAssemblyMetadata>();
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			queue.Enqueue(_assemblies[simpleName]);
			foreach (string item2 in _shared)
			{
				queue.Enqueue(_assemblies[item2]);
			}
			while (queue.Count > 0)
			{
				XWModAssemblyMetadata xWModAssemblyMetadata = queue.Dequeue();
				if (!hashSet.Add(xWModAssemblyMetadata.SimpleName))
				{
					continue;
				}
				if (_host.TryGetValue(xWModAssemblyMetadata.SimpleName, out var value2))
				{
					if (!XWModAssemblyMetadata.SameIdentity(new AssemblyName(xWModAssemblyMetadata.Identity), value2.GetName()))
					{
						throw new InvalidDataException("Mod '" + owner + "' 包含不兼容的宿主程序集副本：" + xWModAssemblyMetadata.Identity);
					}
				}
				else
				{
					if (IsFrameworkAssembly(xWModAssemblyMetadata.SimpleName))
					{
						continue;
					}
					foreach (string reference2 in xWModAssemblyMetadata.References)
					{
						AssemblyName assemblyName = new AssemblyName(reference2);
						if (!_host.ContainsKey(assemblyName.Name) && !IsFrameworkAssembly(assemblyName.Name))
						{
							if (!_assemblies.TryGetValue(assemblyName.Name, out var value3) || !XWModAssemblyMetadata.SameIdentity(assemblyName, new AssemblyName(value3.Identity)))
							{
								throw new FileNotFoundException($"Mod '{owner}' 私有依赖缺失或身份不匹配：{reference2}（引用者 {xWModAssemblyMetadata.SimpleName}）");
							}
							queue.Enqueue(value3);
						}
					}
				}
			}
			SharedContracts.Register(_shared.Select((string key) => _assemblies[key]), _host);
		}

		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Explicit managed Mod payloads are loaded at runtime.")]
		public Assembly LoadAssemblyWithoutLock(string path)
		{
			lock (_loadGate)
			{
				XWModAssemblyMetadata xWModAssemblyMetadata = XWModAssemblyMetadata.Read(path);
				if (!_assemblies.TryGetValue(xWModAssemblyMetadata.SimpleName, out var value) || !string.Equals(value.Sha256, xWModAssemblyMetadata.Sha256, StringComparison.Ordinal))
				{
					throw new IOException("Mod '" + _owner + "' 程序集在预检后发生变化：" + path);
				}
				using FileStream fileStream = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read);
				if (!string.Equals(Convert.ToHexString(SHA256.HashData(fileStream)).ToLowerInvariant(), value.Sha256, StringComparison.Ordinal))
				{
					throw new IOException("Mod '" + _owner + "' 程序集在加载前发生变化：" + path);
				}
				fileStream.Position = 0L;
				return LoadFromStream(fileStream);
			}
		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			if (_host.TryGetValue(assemblyName.Name, out var value))
			{
				return value;
			}
			if (IsFrameworkAssembly(assemblyName.Name))
			{
				return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
			}
			if (_shared.Contains(assemblyName.Name))
			{
				return SharedContracts.Resolve(assemblyName);
			}
			if (_assemblies.TryGetValue(assemblyName.Name, out var value2) && XWModAssemblyMetadata.SameIdentity(assemblyName, new AssemblyName(value2.Identity)))
			{
				return LoadAssemblyWithoutLock(value2.Path);
			}
			throw new FileNotFoundException("Mod '" + _owner + "' 未声明或缺失私有依赖：" + assemblyName.FullName);
		}
	}

	private sealed class SharedContracts : AssemblyLoadContext
	{
		private static readonly object Gate = new object();

		private static readonly Dictionary<string, XWModAssemblyMetadata> Files = new Dictionary<string, XWModAssemblyMetadata>(StringComparer.OrdinalIgnoreCase);

		private static readonly SharedContracts Context = new SharedContracts();

		private static Dictionary<string, Assembly> _host;

		private SharedContracts()
			: base("mod-shared-contracts")
		{
		}

		internal static void Register(IEnumerable<XWModAssemblyMetadata> contracts, Dictionary<string, Assembly> host)
		{
			lock (Gate)
			{
				XWModAssemblyMetadata[] array = contracts.ToArray();
				XWModAssemblyMetadata[] array2 = array;
				foreach (XWModAssemblyMetadata xWModAssemblyMetadata in array2)
				{
					if (Files.TryGetValue(xWModAssemblyMetadata.SimpleName, out var value) && (!string.Equals(value.Identity, xWModAssemblyMetadata.Identity, StringComparison.Ordinal) || !string.Equals(value.Sha256, xWModAssemblyMetadata.Sha256, StringComparison.Ordinal)))
					{
						throw new InvalidDataException("共享契约程序集不同：" + xWModAssemblyMetadata.SimpleName + "；需要所有声明者使用相同版本和字节，并重启游戏。");
					}
				}
				if (_host == null)
				{
					_host = host;
				}
				array2 = array;
				foreach (XWModAssemblyMetadata xWModAssemblyMetadata2 in array2)
				{
					Files.TryAdd(xWModAssemblyMetadata2.SimpleName, xWModAssemblyMetadata2);
				}
			}
		}

		internal static Assembly Resolve(AssemblyName name)
		{
			lock (Gate)
			{
				return Context.LoadFromAssemblyName(name);
			}
		}

		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Explicit shared Mod contracts are supplied at runtime and verified before loading.")]
		protected override Assembly Load(AssemblyName name)
		{
			Dictionary<string, Assembly> host = _host;
			if (host != null && host.TryGetValue(name.Name, out var value))
			{
				return value;
			}
			if (IsFrameworkAssembly(name.Name))
			{
				return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
			}
			lock (Gate)
			{
				if (!Files.TryGetValue(name.Name, out var value2) || !XWModAssemblyMetadata.SameIdentity(name, new AssemblyName(value2.Identity)))
				{
					throw new FileNotFoundException($"共享契约依赖未声明或身份不匹配：{name}");
				}
				using FileStream fileStream = new FileStream(value2.Path, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read);
				if (!string.Equals(Convert.ToHexString(SHA256.HashData(fileStream)).ToLowerInvariant(), value2.Sha256, StringComparison.Ordinal))
				{
					throw new IOException("共享契约在预检后发生变化：" + value2.Path);
				}
				fileStream.Position = 0L;
				return LoadFromStream(fileStream);
			}
		}
	}

	private readonly Dictionary<string, LoadedModAssembly> _loaded = new Dictionary<string, LoadedModAssembly>(StringComparer.Ordinal);

	private static readonly HashSet<string> RestartRequired = new HashSet<string>(StringComparer.Ordinal);

	public IReadOnlyDictionary<string, LoadedModAssembly> LoadedAssemblies => _loaded;

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Mod assemblies are loaded from player-created build output at runtime by design.")]
	public LoadedModAssembly LoadModAssembly(string modId, string assemblyPath, IEnumerable<string> probingRoots = null, bool registerCharacterComponentRuntimes = true)
	{
		return LoadModAssemblyCore(modId, assemblyPath, probingRoots, registerCharacterComponentRuntimes, staged: false);
	}

	public LoadedModAssembly LoadModAssembly(string modId, string assemblyPath, IEnumerable<string> probingRoots, bool registerCharacterComponentRuntimes, IReadOnlyCollection<string> sharedAssemblies)
	{
		return LoadModAssemblyCore(modId, assemblyPath, probingRoots, registerCharacterComponentRuntimes, staged: false, sharedAssemblies);
	}

	internal LoadedModAssembly LoadStagedModAssembly(string modId, string assemblyPath, IEnumerable<string> probingRoots = null)
	{
		return LoadModAssemblyCore(modId, assemblyPath, probingRoots, registerCharacterComponentRuntimes: false, staged: true);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Trusted Mod assemblies are loaded and discovered at runtime.")]
	private LoadedModAssembly LoadModAssemblyCore(string modId, string assemblyPath, IEnumerable<string> probingRoots, bool registerCharacterComponentRuntimes, bool staged, IReadOnlyCollection<string> sharedAssemblies = null)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			throw new InvalidOperationException("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
		}
		string text = NormalizeExternalModIdOrThrow(modId);
		if (string.IsNullOrWhiteSpace(assemblyPath) || !File.Exists(assemblyPath))
		{
			throw new FileNotFoundException("Mod assembly was not found.", assemblyPath);
		}
		if (_loaded.ContainsKey(text) && (staged || !UnloadMod(text)))
		{
			throw new InvalidOperationException("Mod '" + text + "' cannot reload while a state-machine callback runtime is active.");
		}
		if (RestartRequired.Contains(text))
		{
			throw new InvalidOperationException("Mod '" + text + "' was unloaded from a non-collectible runtime and can only reload after restart.");
		}
		string fullPath = Path.GetFullPath(assemblyPath);
		bool flag = !OperatingSystem.IsAndroid();
		ModLoadContext modLoadContext = new ModLoadContext(text, fullPath, probingRoots, flag, sharedAssemblies);
		Assembly assembly;
		try
		{
			assembly = modLoadContext.LoadAssemblyWithoutLock(fullPath);
		}
		catch
		{
			ReleaseFailedContext(text, modLoadContext, flag);
			throw;
		}
		StateMachineCallbackRegistrationResult stateMachineCallbackRegistrationResult = (staged ? new StateMachineCallbackRegistrationResult(success: true) : StateMachineCallbackRegistry.Shared.RegisterAssembly(text, assembly));
		if (!stateMachineCallbackRegistrationResult.Success)
		{
			ReleaseFailedContext(text, modLoadContext, flag);
			throw new InvalidOperationException("Mod '" + text + "' state-machine callbacks are invalid: " + stateMachineCallbackRegistrationResult.Error);
		}
		LoadedModAssembly loadedModAssembly = new LoadedModAssembly
		{
			ModId = text,
			AssemblyPath = fullPath,
			Assembly = assembly,
			LoadContext = modLoadContext,
			StateMachineCallbackCount = stateMachineCallbackRegistrationResult.CallbackCount,
			CallbacksRegistered = !staged,
			IsStaged = staged,
			SupportsCollectibleUnload = flag,
			HasSharedContracts = (sharedAssemblies != null && sharedAssemblies.Count > 0)
		};
		_loaded[text] = loadedModAssembly;
		try
		{
			if (registerCharacterComponentRuntimes)
			{
				loadedModAssembly.ComponentTypesRegistered = true;
				loadedModAssembly.CharacterComponentRuntimeCount = CharacterComponentRuntimeTypeRegistry.RegisterAssembly(text, loadedModAssembly.Assembly);
			}
		}
		catch
		{
			CharacterComponentRuntimeTypeRegistry.UnregisterOwner(text);
			_loaded.Remove(text);
			StateMachineCallbackRegistry.Shared.TryUnloadOwner(text, out var _);
			ReleaseFailedContext(text, modLoadContext, flag);
			throw;
		}
		XWModEnvironmentService.AssemblyChanged(text, loaded: true);
		return loadedModAssembly;
	}

	public int RegisterCharacterComponentRuntimeTypes(string modId)
	{
		if (!TryNormalizeExternalModId(modId, out var normalizedModId) || !_loaded.TryGetValue(normalizedModId, out var value) || value.RegistrationsDetached)
		{
			return 0;
		}
		value.ComponentTypesRegistered = true;
		return value.CharacterComponentRuntimeCount = CharacterComponentRuntimeTypeRegistry.RegisterAssembly(normalizedModId, value.Assembly);
	}

	internal StateMachineCallbackRegistrationResult RegisterStateMachineCallbacks(string modId)
	{
		LoadedModAssembly loadedModAssembly = _loaded[modId];
		if (loadedModAssembly.RegistrationsDetached)
		{
			return new StateMachineCallbackRegistrationResult(success: false, "程序集已注销，不能重新发布回调。");
		}
		StateMachineCallbackRegistrationResult result = StateMachineCallbackRegistry.Shared.RegisterAssembly(modId, loadedModAssembly.Assembly);
		if (result.Success)
		{
			loadedModAssembly.CallbacksRegistered = true;
			loadedModAssembly.SetCallbackCount(result.CallbackCount);
		}
		return result;
	}

	public bool UnloadMod(string modId)
	{
		StateMachineUnloadBlockers blockers;
		return UnloadMod(modId, out blockers);
	}

	public bool CanUnloadMod(string modId, out StateMachineUnloadBlockers blockers)
	{
		blockers = new StateMachineUnloadBlockers(0, Array.Empty<StateMachineUnloadBlocker>());
		if (!TryNormalizeExternalModId(modId, out var normalizedModId) || !_loaded.ContainsKey(normalizedModId))
		{
			return true;
		}
		StateMachineCallbackRegistry.Shared.CanUnloadOwner(normalizedModId, out blockers);
		StateMachineUnloadBlockers unloadBlockers = CharacterComponentRuntimeTypeRegistry.GetUnloadBlockers(normalizedModId);
		blockers = new StateMachineUnloadBlockers(blockers.LeaseCount + unloadBlockers.LeaseCount, blockers.Entries.Concat(unloadBlockers.Entries).ToArray());
		return blockers.LeaseCount == 0;
	}

	public bool RequiresRestart(string modId)
	{
		if (TryNormalizeExternalModId(modId, out var normalizedModId))
		{
			return RestartRequired.Contains(normalizedModId);
		}
		return false;
	}

	public bool UnloadMod(string modId, out StateMachineUnloadBlockers blockers)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			blockers = new StateMachineUnloadBlockers(0, Array.Empty<StateMachineUnloadBlocker>());
			return false;
		}
		LoadedModAssembly detached;
		string diagnostic;
		return TryDetachMod(modId, out detached, out blockers) && TryRequestUnload(detached, out diagnostic);
	}

	internal bool TryDetachMod(string modId, out LoadedModAssembly detached, out StateMachineUnloadBlockers blockers)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		detached = null;
		blockers = new StateMachineUnloadBlockers(0, Array.Empty<StateMachineUnloadBlocker>());
		if (environmentMutation == null)
		{
			return false;
		}
		if (!TryNormalizeExternalModId(modId, out var normalizedModId) || !_loaded.TryGetValue(normalizedModId, out var value))
		{
			return false;
		}
		if (value.RegistrationsDetached)
		{
			detached = value;
			return true;
		}
		if (!CharacterComponentRuntimeTypeRegistry.TryReserveUnload(normalizedModId, out var reservation))
		{
			CanUnloadMod(normalizedModId, out blockers);
			return false;
		}
		using (reservation)
		{
			if (value.CallbacksRegistered && !StateMachineCallbackRegistry.Shared.TryUnloadOwner(normalizedModId, out blockers))
			{
				StateMachineUnloadBlockers obj = blockers;
				if (obj != null && obj.LeaseCount > 0)
				{
					return false;
				}
			}
			if (!value.IsStaged)
			{
				XWModRuntimeRegistry.UnregisterOwner(normalizedModId);
			}
			if (value.ComponentTypesRegistered)
			{
				CharacterComponentRuntimeTypeRegistry.UnregisterOwner(normalizedModId);
			}
			value.RegistrationsDetached = true;
			detached = value;
			XWModEnvironmentService.AssemblyChanged(normalizedModId, loaded: false);
			return true;
		}
	}

	internal bool TryRequestUnload(LoadedModAssembly detached, out string diagnostic)
	{
		diagnostic = "";
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			diagnostic = "Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。";
			return false;
		}
		if (detached == null || !detached.RegistrationsDetached || detached.UnloadRequestInProgress)
		{
			diagnostic = "程序集尚未注销或正在卸载，不能重复请求。";
			return false;
		}
		try
		{
			detached.UnloadRequestInProgress = true;
			if (detached.SupportsCollectibleUnload)
			{
				detached.LoadContext.Unload();
			}
			if (!detached.SupportsCollectibleUnload || detached.HasSharedContracts)
			{
				RestartRequired.Add(detached.ModId);
			}
			_loaded.Remove(detached.ModId);
			detached.LastUnloadDiagnostic = "";
			return true;
		}
		catch (Exception ex)
		{
			string text = (detached.LastUnloadDiagnostic = ex.GetBaseException().Message);
			diagnostic = text;
			return false;
		}
		finally
		{
			detached.UnloadRequestInProgress = false;
		}
	}

	public void UnloadAll()
	{
		foreach (string item in new List<string>(_loaded.Keys))
		{
			UnloadMod(item);
		}
	}

	private static void ReleaseFailedContext(string modId, AssemblyLoadContext context, bool supportsCollectibleUnload)
	{
		if (supportsCollectibleUnload)
		{
			context?.Unload();
		}
		else
		{
			RestartRequired.Add(modId);
		}
	}

	public List<Type> FindTypesAssignableTo(string modId, Type baseType)
	{
		List<Type> list = new List<Type>();
		if (baseType == null || !TryNormalizeExternalModId(modId, out var normalizedModId) || !_loaded.TryGetValue(normalizedModId, out var value))
		{
			return list;
		}
		foreach (Type item in GetTypesSafe(value.Assembly))
		{
			if (!(item == null) && !item.IsAbstract && baseType.IsAssignableFrom(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static string NormalizeExternalModIdOrThrow(string modId)
	{
		if (!StateMachineCallbackKey.TryNormalizeOwnerId(modId, out var normalized))
		{
			throw new ArgumentException("Mod id is invalid. Use a canonical owner id without path separators or control characters.", "modId");
		}
		if (string.Equals(normalized, "builtin", StringComparison.Ordinal))
		{
			throw new ArgumentException("Mod id 'builtin' is reserved for game-owned callbacks.", "modId");
		}
		return normalized;
	}

	private static bool TryNormalizeExternalModId(string modId, out string normalizedModId)
	{
		if (StateMachineCallbackKey.TryNormalizeOwnerId(modId, out normalizedModId))
		{
			return !string.Equals(normalizedModId, "builtin", StringComparison.Ordinal);
		}
		return false;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Mod type discovery must inspect runtime-loaded assemblies.")]
	private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			List<Type> list = new List<Type>();
			Type[] types = ex.Types;
			foreach (Type type in types)
			{
				if (type != null)
				{
					list.Add(type);
				}
			}
			return list;
		}
	}

	private static Dictionary<string, Assembly> HostAssemblies()
	{
		return new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase)
		{
			[typeof(XWModRuntimeContext).Assembly.GetName().Name] = typeof(XWModRuntimeContext).Assembly,
			[typeof(Node).Assembly.GetName().Name] = typeof(Node).Assembly,
			[typeof(object).Assembly.GetName().Name] = typeof(object).Assembly
		};
	}

	internal static bool IsFrameworkAssembly(string name)
	{
		if (!string.Equals(name, "mscorlib", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "netstandard", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "System", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "Microsoft.CSharp", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
		{
			return name.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
