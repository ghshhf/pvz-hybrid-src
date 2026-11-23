using System;
using System.Collections.Generic;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModRuntimeContext
{
	private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

	private XWModLifetime _serviceLifetime;

	private XWModRuntimeSession _serviceSession;

	private bool _servicesEnded;

	private readonly XWModSettings _settings;

	private readonly XWModHooks _hooks;

	private readonly XWModLifetime _lifetime;

	public XWModDataService Data => GetOrCreateService(() => new XWModDataService(ModId));

	internal XWModCardService Cards => GetOrCreateService(() => new XWModCardService(ModId));

	internal XWModCharacterService Characters => GetOrCreateService(() => new XWModCharacterService(ModId));

	internal XWModCombatService Combat => GetOrCreateService(() => new XWModCombatService(ModId));

	internal XWModEconomyService Economy => GetOrCreateService(() => new XWModEconomyService(ModId));

	public XWModModeService Modes => GetOrCreateService(() => new XWModModeService(ModId));

	public XWModGameServices Game => GetOrCreateService(() => new XWModGameServices(this));

	public XWModNetworkService Network => GetOrCreateService(() => new XWModNetworkService(ModId));

	public XWModPlantPlacementService PlantPlacement => GetOrCreateService(() => new XWModPlantPlacementService(this));

	public XWModPresentationService Presentation => GetOrCreateService(() => new XWModPresentationService(this));

	public XWModHooks Hooks => _hooks ?? throw new InvalidOperationException("扩展点仅在正式加载的运行入口中可用。");

	public XWModLifetime Lifetime => _lifetime ?? throw new InvalidOperationException("生命周期仅在正式加载的运行入口中可用。");

	public XWModCapabilities Capabilities { get; }

	public XWModContentApi Content { get; }

	public XWModSettings Settings => _settings ?? throw new InvalidOperationException("Mod 配置仅在正式加载的运行入口中可用。");

	public string ModId { get; }

	public string ModVersion { get; }

	public string PackageRoot { get; }

	public int ApiVersion { get; }

	public bool IsAndroid => OperatingSystem.IsAndroid();

	internal string UpdateDisplayName { get; private set; }

	internal int UpdateLoadOrder { get; set; }

	public XWModUpdateService Updates => GetOrCreateService(() => new XWModUpdateService(_serviceSession, ModId, UpdateDisplayName, ModVersion, UpdateLoadOrder, XWModUpdateInbox.Shared));

	internal void AttachServiceSession(XWModRuntimeSession session)
	{
		_serviceSession = session;
	}

	internal void PrepareServiceLifetime()
	{
		if (_serviceLifetime == null && _lifetime != null)
		{
			_serviceLifetime = _lifetime.CreateChildScope();
			_lifetime.Track(delegate
			{
				_servicesEnded = true;
			});
		}
	}

	private void RequireServiceLifetime()
	{
		if (_serviceLifetime == null)
		{
			throw new InvalidOperationException("Services require a formal runtime context.");
		}
		if (_servicesEnded || (_serviceSession != null && (XWModRuntimeSession.Current != _serviceSession || !_serviceSession.IsOwnerActive(ModId))))
		{
			throw new ObjectDisposedException("XWModRuntimeContext", "This runtime context no longer owns an active Mod session.");
		}
	}

	internal T GetOrCreateService<T>(Func<T> factory) where T : class
	{
		RequireServiceLifetime();
		if (_services.TryGetValue(typeof(T), out var value))
		{
			return (T)value;
		}
		T val = factory() ?? throw new InvalidOperationException("Service factory returned null.");
		try
		{
			RequireServiceLifetime();
			if (val is IDisposable value2)
			{
				_serviceLifetime.Track(value2);
			}
			_services.Add(typeof(T), val);
			return val;
		}
		catch
		{
			if (val is IDisposable disposable)
			{
				disposable.Dispose();
			}
			throw;
		}
	}

	internal XWModRuntimeContext(XWModManifest manifest, string packageRoot)
		: this(manifest, packageRoot, null, null, null)
	{
	}

	internal XWModRuntimeContext(XWModManifest manifest, string packageRoot, XWModHooks hooks, XWModLifetime lifetime, XWModCapabilities capabilities)
	{
		if (manifest == null)
		{
			throw new ArgumentNullException("manifest");
		}
		ModId = manifest.Id ?? "";
		ModVersion = manifest.Version ?? "";
		UpdateDisplayName = manifest.Name ?? "";
		PackageRoot = packageRoot ?? "";
		ApiVersion = (XWModRuntimeApi.IsSupported(manifest.RuntimeApiVersion) ? manifest.RuntimeApiVersion : 3);
		_settings = XWModSettingsService.GetActive(ModId);
		_hooks = hooks;
		_lifetime = lifetime;
		Capabilities = capabilities ?? XWModCapabilities.Create(isolatedAssembliesSupported: false);
		Content = new XWModContentApi(ModId);
		PrepareServiceLifetime();
	}

	public bool TryRegister(string category, string key, Variant value, bool allowOverride, out string diagnostic)
	{
		return XWModRuntimeRegistry.Register(ModId, category, key, value, allowOverride, out diagnostic);
	}

	public void Log(string message)
	{
		GD.Print("[Mod:" + ModId + "] " + message);
	}

	public void Warn(string message)
	{
		GD.PushWarning("[Mod:" + ModId + "] " + message);
	}
}
