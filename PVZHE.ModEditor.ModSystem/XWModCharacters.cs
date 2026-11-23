using System;
using System.Collections.Generic;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModCharacters
{
	private sealed class Entry
	{
		internal XWModEntityHandle Handle;

		internal WeakReference<TowerDefenseCharacter> Character;

		internal readonly HashSet<string> Phases = new HashSet<string>(StringComparer.Ordinal);

		internal bool Bound;

		internal XWModEventContext Context;

		internal Action Exited;
	}

	private static Guid _session;

	private static long _next;

	private static readonly Dictionary<ulong, Entry> _byInstance = new Dictionary<ulong, Entry>();

	private static readonly Dictionary<long, Entry> _byId = new Dictionary<long, Entry>();

	internal static int TrackedEntityCount
	{
		get
		{
			EnsureSession();
			return _byId.Count;
		}
	}

	private static void EnsureSession()
	{
		Guid sessionId = XWModExtensions.SessionId;
		if (sessionId == _session)
		{
			return;
		}
		foreach (Entry value in _byId.Values)
		{
			if (value.Exited != null && value.Character.TryGetTarget(out var target) && GodotObject.IsInstanceValid(target))
			{
				target.TreeExited -= value.Exited;
			}
		}
		_session = sessionId;
		_next = 0L;
		_byInstance.Clear();
		_byId.Clear();
	}

	internal static XWModEntityHandle GetHandle(TowerDefenseCharacter character)
	{
		EnsureSession();
		if (!(_session == Guid.Empty))
		{
			XWModRuntimeSession current = XWModRuntimeSession.Current;
			if (current != null && current.HasRuntimeOwners && GodotObject.IsInstanceValid(character) && character.inGame && !character.editorPreviewMode)
			{
				ulong instanceId = character.GetInstanceId();
				if (!_byInstance.TryGetValue(instanceId, out var value))
				{
					if (character.isDestroy || character.IsQueuedForDeletion())
					{
						return default(XWModEntityHandle);
					}
					value = new Entry
					{
						Handle = new XWModEntityHandle(_session, ++_next),
						Character = new WeakReference<TowerDefenseCharacter>(character),
						Context = XWModExtensions.CreateEventContext()
					};
					_byInstance.Add(instanceId, value);
					_byId.Add(value.Handle.Id, value);
					Action value2 = (value.Exited = delegate
					{
						Release(character);
					});
					character.TreeExited += value2;
				}
				return value.Handle;
			}
		}
		return default(XWModEntityHandle);
	}

	internal static bool TryGetHandle(TowerDefenseCharacter character, out XWModEntityHandle handle)
	{
		EnsureSession();
		handle = default(XWModEntityHandle);
		if (!(_session == Guid.Empty))
		{
			XWModRuntimeSession current = XWModRuntimeSession.Current;
			if (current != null && current.HasRuntimeOwners && GodotObject.IsInstanceValid(character) && character.inGame && !character.editorPreviewMode && character.IsInsideTree() && !character.isDestroy && !character.IsQueuedForDeletion())
			{
				if (!_byInstance.TryGetValue(character.GetInstanceId(), out var value) || value.Handle.SessionId != _session || !value.Character.TryGetTarget(out var target) || target != character)
				{
					return false;
				}
				handle = value.Handle;
				return true;
			}
		}
		return false;
	}

	internal static void Release(TowerDefenseCharacter character)
	{
		EnsureSession();
		if (!GodotObject.IsInstanceValid(character) || !_byInstance.TryGetValue(character.GetInstanceId(), out var value))
		{
			return;
		}
		try
		{
			Lifecycle(character, "visual-exit");
		}
		finally
		{
			if (value.Exited != null)
			{
				character.TreeExited -= value.Exited;
				value.Exited = null;
			}
			_byInstance.Remove(character.GetInstanceId());
			_byId.Remove(value.Handle.Id);
			XWModPresentation.UnmountCharacter(character);
		}
	}

	internal static bool TryResolve(XWModEntityHandle handle, out TowerDefenseCharacter character)
	{
		EnsureSession();
		character = null;
		if (handle.SessionId == _session && _session != Guid.Empty && _byId.TryGetValue(handle.Id, out var value) && value.Character.TryGetTarget(out character) && GodotObject.IsInstanceValid(character) && character.IsInsideTree())
		{
			return !character.IsQueuedForDeletion();
		}
		return false;
	}

	internal static XWModCharacterSnapshot Snapshot(TowerDefenseCharacter character)
	{
		return new XWModCharacterSnapshot(GetHandle(character), character.config?.name ?? "", character.gridPos.X, character.gridPos.Y, (int)character.camp, GodotObject.IsInstanceValid(character.instance) ? character.instance.hitpoints : 0.0, character.die, character.isDestroy);
	}

	internal static IReadOnlyList<XWModCharacterSnapshot> Query()
	{
		EnsureSession();
		List<XWModCharacterSnapshot> list = new List<XWModCharacterSnapshot>();
		foreach (Entry value in _byId.Values)
		{
			if (TryResolve(value.Handle, out var character))
			{
				list.Add(Snapshot(character));
			}
		}
		return list.AsReadOnly();
	}

	internal static void Ready(TowerDefenseCharacter character, bool restored = false)
	{
		using (XWModExtensions.EnterOperation())
		{
			XWModEntityHandle handle = GetHandle(character);
			if (handle == default(XWModEntityHandle) || !character.IsInsideTree() || !character.HasValidRuntimeConfiguration || character.IsProgressRestoreInFlight)
			{
				return;
			}
			Entry entry = _byId[handle.Id];
			if (entry.Bound && !restored)
			{
				return;
			}
			entry.Bound = true;
			int num;
			if (!character.IsRemoteNetworkReplica)
			{
				if (!restored)
				{
					TowerDefenseControlNew currentControl = TowerDefenseManager.CurrentControl;
					num = ((currentControl == null || !currentControl.isGameRunning) ? 1 : 0);
				}
				else
				{
					num = 2;
				}
			}
			else
			{
				num = 3;
			}
			XWModEventOrigin xWModEventOrigin = (XWModEventOrigin)num;
			Lifecycle(character, restored ? "restored" : "ready", xWModEventOrigin);
			if (xWModEventOrigin == XWModEventOrigin.Gameplay && (!Global.IsMultiplayerMode || MultiPlayerManager.IsHost))
			{
				BattleEventBus.Instance?.EmitCharacterSpawned(character);
			}
			XWModPresentation.MountCharacter(character);
		}
	}

	internal static void Lifecycle(TowerDefenseCharacter character, string phase, XWModEventOrigin? origin = null)
	{
		EnsureSession();
		if (GodotObject.IsInstanceValid(character) && _byInstance.TryGetValue(character.GetInstanceId(), out var value) && value.Phases.Add(phase) && XWModExtensions.HasHandlers(XWModCharacterPoints.Lifecycle.Id))
		{
			XWModEventOrigin origin2 = (XWModEventOrigin)(((int?)origin) ?? (character.IsProgressRestoreInFlight ? 2 : (character.IsRemoteNetworkReplica ? 3 : 0)));
			XWModExtensions.Notify(XWModCharacterPoints.Lifecycle, new XWModCharacterLifecycleEvent(Snapshot(character), phase), value.Context with
			{
				Origin = origin2,
				Authority = XWModAuthority.Local,
				Reason = phase
			});
		}
	}

	internal static void Revived(TowerDefenseCharacter character)
	{
		XWModEntityHandle handle = GetHandle(character);
		if (!(handle == default(XWModEntityHandle)))
		{
			HashSet<string> phases = _byId[handle.Id].Phases;
			if (phases.Remove("near-death") | phases.Remove("death-committed"))
			{
				phases.Remove("revived");
				Lifecycle(character, "revived");
			}
		}
	}

	internal static void Attack(TowerDefenseCharacter source, TowerDefenseCharacter target, string kind, string phase)
	{
		if (XWModExtensions.HasHandlers(XWModCharacterPoints.Attack.Id) && GodotObject.IsInstanceValid(source) && !source.IsProgressRestoreInFlight && !source.IsRemoteNetworkReplica)
		{
			XWModExtensions.Notify(XWModCharacterPoints.Attack, new XWModAttackEvent(GetHandle(source), GetHandle(target), kind, phase));
		}
	}

	internal static void ProjectileHit(TowerDefenseCharacter source, TowerDefenseCharacter target, string projectileKey, double lifeLost, XWModEventContext context = null, XWModProjectileSource attribution = default(XWModProjectileSource))
	{
		if (XWModExtensions.HasHandlers(XWModCharacterPoints.ProjectileHit.Id))
		{
			XWModExtensions.Notify(XWModCharacterPoints.ProjectileHit, new XWModProjectileHitEvent(GetHandle(source), GetHandle(target), projectileKey ?? "", lifeLost)
			{
				Attribution = attribution
			}, context);
		}
	}

	internal static void ModifyProjectile(TowerDefenseCharacter source, string key, ref double damage, ref double speed, ref Vector2 velocity, XWModProjectileSource attribution = default(XWModProjectileSource))
	{
		if (XWModExtensions.HasHandlers(XWModCharacterPoints.ProjectileLaunch.Id))
		{
			XWModProjectileLaunch value = new XWModProjectileLaunch(GetHandle(source), key ?? "", damage, speed, velocity)
			{
				Attribution = attribution
			};
			XWModProjectileLaunch xWModProjectileLaunch = XWModExtensions.Modify(XWModCharacterPoints.ProjectileLaunch, value);
			damage = xWModProjectileLaunch.Damage;
			speed = xWModProjectileLaunch.Speed;
			velocity = xWModProjectileLaunch.Velocity;
		}
	}
}
