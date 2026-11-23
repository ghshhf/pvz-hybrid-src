using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModRuntimeSession : IDisposable
{
	private readonly Dictionary<string, int> _order;

	private readonly Dictionary<string, AssemblyLoadContext> _contexts = new Dictionary<string, AssemblyLoadContext>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, XWModLifetime> _lifetimes = new Dictionary<string, XWModLifetime>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, XWModLifetime> _scenes = new Dictionary<string, XWModLifetime>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _endingOwners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string> _faults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public static XWModRuntimeSession Current { get; private set; }

	public XWModHookDispatcher Hooks { get; }

	public bool HasFailures => _faults.Count != 0;

	public IReadOnlyDictionary<string, string> Faults => _faults;

	public bool Frozen { get; private set; }

	internal bool HasRuntimeOwners => _lifetimes.Count != 0;

	public static XWModCapabilities HostCapabilities => XWModCapabilities.Create(isolatedAssembliesSupported: true, isolatedAssembliesVerified: false, XWModExtensionCatalog.InstalledLegacy);

	public static IReadOnlySet<string> AvailableCapabilities => (from item in HostCapabilities.Items.Values
		where item.Supported
		select item.Id).ToHashSet<string>(StringComparer.Ordinal);

	internal IDisposable EnterOwnerReflection(string owner)
	{
		AssemblyLoadContext value;
		return (!_contexts.TryGetValue(owner, out value)) ? ((AssemblyLoadContext.ContextualReflectionScope?)null) : value?.EnterContextualReflection();
	}

	internal bool IsOwnerActive(string owner)
	{
		if (owner != null && _lifetimes.ContainsKey(owner) && !_faults.ContainsKey(owner))
		{
			return !_endingOwners.Contains(owner);
		}
		return false;
	}

	internal void ReportExtensionFault(string owner, Exception error)
	{
		Hooks.RemoveOwner(owner);
		Fault(owner, error);
	}

	private XWModRuntimeSession(IReadOnlyList<XWModManager.ModEntry> order)
	{
		_order = order.Select((XWModManager.ModEntry entry, int index) => (Id: entry.Id, index: index)).ToDictionary<(string, int), string, int>(((string Id, int index) item) => item.Id, ((string Id, int index) item) => item.index, StringComparer.OrdinalIgnoreCase);
		Hooks = new XWModHookDispatcher(Fault);
	}

	internal static XWModRuntimeSession Begin(IReadOnlyList<XWModManager.ModEntry> order)
	{
		Current?.Dispose();
		XWModExtensions.BindHostThread();
		XWModExtensions.ResetDiagnostics();
		XWModRuntimeSession xWModRuntimeSession = new XWModRuntimeSession(order);
		XWModRuntimeRegistry.BeginRegistration();
		XWModRuntimeRegistry.ConfigureOrder(xWModRuntimeSession._order);
		Current = xWModRuntimeSession;
		return xWModRuntimeSession;
	}

	internal XWModRuntimeContext CreateContext(XWModManifest manifest, string root, AssemblyLoadContext context)
	{
		if (Frozen || !_order.TryGetValue(manifest.Id, out var value))
		{
			throw new InvalidOperationException("Mod '" + manifest.Id + "' 不在当前可登记的加载计划中。");
		}
		if (_lifetimes.ContainsKey(manifest.Id))
		{
			throw new InvalidOperationException("Mod 运行入口重复初始化。");
		}
		XWModLifetime xWModLifetime = new XWModLifetime(manifest.Id);
		_lifetimes.Add(manifest.Id, xWModLifetime);
		_contexts.Add(manifest.Id, context);
		_scenes.Add(manifest.Id, xWModLifetime.CreateChildScope());
		XWModRuntimeContext xWModRuntimeContext = new XWModRuntimeContext(manifest, root, Hooks.ForOwner(manifest.Id, value), xWModLifetime, HostCapabilities);
		xWModRuntimeContext.AttachServiceSession(this);
		xWModRuntimeContext.UpdateLoadOrder = value;
		return xWModRuntimeContext;
	}

	internal static XWModRuntimeContext CreateStandaloneContext(XWModManifest manifest, string root)
	{
		XWModHookDispatcher xWModHookDispatcher = new XWModHookDispatcher(delegate(string owner, Exception error)
		{
			XWModEnvironmentService.Invalidate("ModHookFault:" + owner);
		});
		XWModLifetime xWModLifetime = new XWModLifetime(manifest.Id);
		xWModLifetime.Track(xWModHookDispatcher);
		return new XWModRuntimeContext(manifest, root, xWModHookDispatcher.ForOwner(manifest.Id, 0), xWModLifetime, HostCapabilities);
	}

	public void Freeze()
	{
		XWModNetworkService.FreezeAll();
		XWModDataService.FreezeAll();
		Hooks.Freeze();
		foreach (XWModLifetime value in _lifetimes.Values)
		{
			value.Freeze();
		}
		XWModRuntimeRegistry.FreezeStructure();
		Frozen = true;
	}

	internal void RemoveOwner(string owner)
	{
		if (_endingOwners.Add(owner))
		{
			XWModExtensions.NotifyOwnerEnding(owner);
		}
		Hooks.RemoveOwner(owner);
		if (_lifetimes.Remove(owner, out var value))
		{
			foreach (Exception item in value.Cleanup())
			{
				Fault(owner, item);
			}
		}
		_contexts.Remove(owner);
		_scenes.Remove(owner);
	}

	private void Fault(string owner, Exception error)
	{
		if (_endingOwners.Add(owner))
		{
			XWModExtensions.NotifyOwnerEnding(owner);
		}
		_faults[owner] = error.GetBaseException().Message;
		XWModEnvironmentService.Invalidate("ModHookFault:" + owner);
		GD.PushWarning("[Mod:" + owner + "] 扩展点或清理失败，需要重启：" + error.GetBaseException().Message);
	}

	public static void MountUi(Control root)
	{
		XWModRuntimeSession session = Current;
		if (session == null || !GodotObject.IsInstanceValid(root) || !session.Hooks.HasHandlers(XWModHookPoints.UiMount.Id))
		{
			return;
		}
		try
		{
			session.Hooks.PublishForOwners(XWModHookPoints.UiMount, delegate(string owner)
			{
				XWModLifetime scope = session._scenes[owner].CreateChildScope();
				Control control = new Control
				{
					Name = "ModUi_" + owner.Replace('.', '_'),
					MouseFilter = Control.MouseFilterEnum.Ignore
				};
				control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize);
				root.AddChild(control, forceReadableName: false, Node.InternalMode.Disabled);
				scope.TrackNode(control);
				root.TreeExited += Exit;
				scope.Track(delegate
				{
					if (GodotObject.IsInstanceValid(root))
					{
						root.TreeExited -= Exit;
					}
				});
				return new XWModUiMountEvent
				{
					Root = control,
					SceneLifetime = scope
				};
				void Exit()
				{
					root.TreeExited -= Exit;
					foreach (Exception item in scope.Cleanup())
					{
						session.Fault(owner, item);
					}
				}
			});
		}
		catch (XWModHookCallbackException)
		{
		}
	}

	internal static string ResolutionHash(string owner)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Dictionary<string, XWModRuntimeRegistry.Registration>> item in XWModRuntimeRegistry.GetRegistrations().OrderBy<KeyValuePair<string, Dictionary<string, XWModRuntimeRegistry.Registration>>, string>((KeyValuePair<string, Dictionary<string, XWModRuntimeRegistry.Registration>> pair) => pair.Key, StringComparer.Ordinal))
		{
			foreach (KeyValuePair<string, XWModRuntimeRegistry.Registration> item2 in item.Value.OrderBy<KeyValuePair<string, XWModRuntimeRegistry.Registration>, string>((KeyValuePair<string, XWModRuntimeRegistry.Registration> pair) => pair.Key, StringComparer.Ordinal))
			{
				XWModRuntimeRegistry.Registration value = item2.Value;
				if (string.Equals(value.OwnerMod, owner, StringComparison.OrdinalIgnoreCase))
				{
					list.Add($"content\0{item.Key}\0{item2.Key}\0{value.ContentId}\0{value.IsEffective}\0{value.IsOverride}");
				}
			}
		}
		if (Current != null)
		{
			foreach (XWModHookSnapshot hook in from xWModHookSnapshot in Current.Hooks.GetSnapshot()
				where string.Equals(xWModHookSnapshot.OwnerId, owner, StringComparison.OrdinalIgnoreCase)
				select xWModHookSnapshot)
			{
				string value2 = (XWModExtensionCatalog.Installed.Any((XWModExtensionDescriptor point) => point.Id == hook.PointId && point.LocalPresentation) ? "local" : hook.Active.ToString());
				list.Add($"hook\0{hook.PointId}\0{hook.RegistrationId}\0{hook.Kind}\0{value2}\0{hook.Order}");
			}
		}
		list.AddRange(XWModNetworkService.GetSchemaSignatures(owner));
		list.AddRange(XWModDataService.GetSchemaSignatures(owner));
		foreach (XWModExtensionDescriptor item3 in XWModExtensionCatalog.Installed)
		{
			list.Add($"contract\0{item3.Id}\0{item3.Authority}\0{item3.Modifiable}\0{item3.LocalPresentation}\0{item3.PayloadType.FullName}");
		}
		list.Sort(StringComparer.Ordinal);
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', list)))).ToLowerInvariant();
	}

	public void Dispose()
	{
		Hooks.Dispose();
		string[] array = _lifetimes.Keys.OrderByDescending((string id) => _order.GetValueOrDefault(id)).ToArray();
		foreach (string owner in array)
		{
			RemoveOwner(owner);
		}
		if (Current == this)
		{
			XWModExtensions.EndBattle();
			Current = null;
		}
	}

	internal int GetUpdateLoadOrder(string owner, int fallback)
	{
		return _order.GetValueOrDefault(owner, fallback);
	}
}
