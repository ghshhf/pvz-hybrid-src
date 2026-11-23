using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;

public static class CharacterComponentRuntimeTypeRegistry
{
	private sealed record Registration(string Owner, Type RuntimeType, long Order);

	private sealed class Lease : IDisposable
	{
		private string _owner;

		private bool _003Creservation_003EP;

		public Lease(string owner, bool reservation = false)
		{
			_003Creservation_003EP = reservation;
			_owner = owner;
		}

		public void Dispose()
		{
			lock (Gate)
			{
				if (_owner == null)
				{
					return;
				}
				int value;
				if (_003Creservation_003EP)
				{
					Unloading.Remove(_owner);
				}
				else if (Leases.TryGetValue(_owner, out value))
				{
					if (value <= 1)
					{
						Leases.Remove(_owner);
					}
					else
					{
						Leases[_owner] = value - 1;
					}
				}
				_owner = null;
			}
			GC.SuppressFinalize(this);
		}

		~Lease()
		{
			Dispose();
		}
	}

	private static readonly object Gate = new object();

	private static readonly Dictionary<string, List<Registration>> Registrations = new Dictionary<string, List<Registration>>(StringComparer.Ordinal);

	private static long _nextOrder;

	private static readonly Dictionary<string, int> Leases = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> Unloading = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static IDisposable AcquireCore(string owner)
	{
		if (Unloading.Contains(owner))
		{
			throw new InvalidOperationException("Mod '" + owner + "' 正在卸载。");
		}
		Leases[owner] = Leases.GetValueOrDefault(owner) + 1;
		return new Lease(owner);
	}

	internal static IDisposable Acquire(Type runtimeType)
	{
		lock (Gate)
		{
			if (TryGetOwner(runtimeType, out var owner))
			{
				return AcquireCore(owner);
			}
			if (runtimeType.Assembly != typeof(CharacterComponentRuntime).Assembly)
			{
				throw new InvalidOperationException("Mod 组件类型尚未注册或已卸载：" + runtimeType.FullName);
			}
			return null;
		}
	}

	public static StateMachineUnloadBlockers GetUnloadBlockers(string owner)
	{
		lock (Gate)
		{
			int valueOrDefault = Leases.GetValueOrDefault(owner);
			return new StateMachineUnloadBlockers(valueOrDefault, (valueOrDefault == 0) ? Array.Empty<StateMachineUnloadBlocker>() : new StateMachineUnloadBlocker[1]
			{
				new StateMachineUnloadBlocker(owner, $"CharacterComponentRuntime ({valueOrDefault})")
			});
		}
	}

	public static bool TryReserveUnload(string owner, out IDisposable reservation)
	{
		lock (Gate)
		{
			reservation = null;
			if (Leases.GetValueOrDefault(owner) != 0 || !Unloading.Add(owner))
			{
				return false;
			}
			reservation = new Lease(owner, reservation: true);
			return true;
		}
	}

	internal static bool WithCreationGate(Func<bool> commit)
	{
		lock (Gate)
		{
			return commit();
		}
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Player Mod component runtimes are intentionally discovered from loaded assemblies.")]
	[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Registration filters runtime types to public parameterless constructors before storing them.")]
	public static int RegisterAssembly(string owner, Assembly assembly)
	{
		if (string.IsNullOrWhiteSpace(owner) || assembly == null)
		{
			return 0;
		}
		Type[] array;
		try
		{
			array = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			array = ex.Types.Where((Type type2) => type2 != null).ToArray();
		}
		lock (Gate)
		{
			if (Leases.GetValueOrDefault(owner) != 0 || Unloading.Contains(owner))
			{
				throw new InvalidOperationException("Mod '" + owner + "' 存在活动组件或正在卸载，不能替换类型注册。");
			}
			UnregisterOwnerCore(owner);
			int num = 0;
			Type[] array2 = array;
			foreach (Type type in array2)
			{
				if (type == null || type.IsAbstract || !typeof(CharacterComponentRuntime).IsAssignableFrom(type) || type.GetConstructor(Type.EmptyTypes) == null)
				{
					continue;
				}
				HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal) { type.Name };
				if (!string.IsNullOrWhiteSpace(type.FullName))
				{
					hashSet.Add(type.FullName);
				}
				Registration item = new Registration(owner, type, ++_nextOrder);
				foreach (string item2 in hashSet)
				{
					if (!Registrations.TryGetValue(item2, out var value))
					{
						value = new List<Registration>();
						Registrations[item2] = value;
					}
					value.Add(item);
				}
				num++;
			}
			return num;
		}
	}

	public static int UnregisterOwner(string owner)
	{
		if (string.IsNullOrWhiteSpace(owner))
		{
			return 0;
		}
		lock (Gate)
		{
			return (Leases.GetValueOrDefault(owner) == 0) ? UnregisterOwnerCore(owner) : 0;
		}
	}

	public static CharacterComponentRuntime Create(string runtimeTypeName)
	{
		return Create(runtimeTypeName, null);
	}

	public static CharacterComponentRuntime Create(string runtimeTypeName, string ownerModId)
	{
		if (TryCreate(runtimeTypeName, ownerModId, out var runtime, out var diagnostic))
		{
			return runtime;
		}
		if (diagnostic.StartsWith("Ambiguous", StringComparison.Ordinal))
		{
			GD.PushError(diagnostic);
		}
		return null;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Only explicitly registered Mod runtime types with public parameterless constructors are instantiated.")]
	[UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Dynamically loaded Mod types are outside host trimming; RegisterAssembly verifies their public parameterless constructors.")]
	public static bool TryCreate(string runtimeTypeName, string ownerModId, out CharacterComponentRuntime runtime, out string diagnostic)
	{
		runtime = null;
		diagnostic = "Mod character component runtime is not loaded: " + ownerModId + ":" + runtimeTypeName;
		if (string.IsNullOrWhiteSpace(runtimeTypeName))
		{
			return false;
		}
		Type runtimeType;
		IDisposable disposable;
		lock (Gate)
		{
			if (!Registrations.TryGetValue(runtimeTypeName.Trim(), out var value) || value.Count == 0)
			{
				return false;
			}
			Registration[] array = value.Where((Registration item) => string.IsNullOrWhiteSpace(ownerModId) || string.Equals(item.Owner, ownerModId, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array.Length == 0)
			{
				return false;
			}
			if (array.Length != 1)
			{
				diagnostic = "Ambiguous Mod component runtime '" + runtimeTypeName + "'; specify owner and full type name. Candidates: " + string.Join(", ", array.Select((Registration item) => item.Owner + ":" + item.RuntimeType.FullName).OrderBy<string, string>((string item) => item, StringComparer.Ordinal));
				return false;
			}
			runtimeType = array[0].RuntimeType;
			if (Unloading.Contains(array[0].Owner))
			{
				diagnostic = "Mod '" + array[0].Owner + "' is unloading.";
				return false;
			}
			disposable = AcquireCore(array[0].Owner);
		}
		try
		{
			using (AssemblyLoadContext.GetLoadContext(runtimeType.Assembly)?.EnterContextualReflection())
			{
				runtime = Activator.CreateInstance(runtimeType) as CharacterComponentRuntime;
				if (runtime == null)
				{
					return false;
				}
				runtime.AdoptModLease(disposable);
				disposable = null;
				diagnostic = "";
				return true;
			}
		}
		catch (Exception ex)
		{
			diagnostic = ex.GetBaseException().Message;
			return false;
		}
		finally
		{
			disposable?.Dispose();
		}
	}

	public static bool Contains(string runtimeTypeName)
	{
		if (string.IsNullOrWhiteSpace(runtimeTypeName))
		{
			return false;
		}
		lock (Gate)
		{
			List<Registration> value;
			return Registrations.TryGetValue(runtimeTypeName.Trim(), out value) && value.Count > 0;
		}
	}

	public static bool TryGetOwner(Type runtimeType, out string owner)
	{
		owner = string.Empty;
		if (runtimeType == null)
		{
			return false;
		}
		string key = runtimeType.FullName ?? runtimeType.Name;
		lock (Gate)
		{
			if (!Registrations.TryGetValue(key, out var value))
			{
				return false;
			}
			for (int num = value.Count - 1; num >= 0; num--)
			{
				Registration registration = value[num];
				if ((object)registration.RuntimeType == runtimeType)
				{
					owner = registration.Owner;
					return true;
				}
			}
		}
		return false;
	}

	private static int UnregisterOwnerCore(string owner)
	{
		int num = 0;
		string[] array = Registrations.Keys.ToArray();
		foreach (string key in array)
		{
			List<Registration> list = Registrations[key];
			num += list.RemoveAll((Registration item) => string.Equals(item.Owner, owner, StringComparison.OrdinalIgnoreCase));
			if (list.Count == 0)
			{
				Registrations.Remove(key);
			}
		}
		return num;
	}
}
