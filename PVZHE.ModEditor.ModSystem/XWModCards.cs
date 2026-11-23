using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModCards
{
	private sealed class Identity
	{
		public long Id;
	}

	private static ConditionalWeakTable<TowerDefenseInGamePacketShow, Identity> _ids = new ConditionalWeakTable<TowerDefenseInGamePacketShow, Identity>();

	private static readonly Dictionary<long, WeakReference<TowerDefenseInGamePacketShow>> Cards = new Dictionary<long, WeakReference<TowerDefenseInGamePacketShow>>();

	private static Guid _session;

	private static long _nextId;

	private static void CheckSession()
	{
		Guid sessionId = XWModExtensions.SessionId;
		if (!(_session == sessionId))
		{
			_session = sessionId;
			Cards.Clear();
			_ids = new ConditionalWeakTable<TowerDefenseInGamePacketShow, Identity>();
		}
	}

	internal static XWModCardHandle GetHandle(TowerDefenseInGamePacketShow card)
	{
		CheckSession();
		if (!_ids.TryGetValue(card, out var value))
		{
			value = new Identity
			{
				Id = ++_nextId
			};
			_ids.Add(card, value);
			Cards.Add(value.Id, new WeakReference<TowerDefenseInGamePacketShow>(card));
		}
		if (!Cards.ContainsKey(value.Id))
		{
			Cards.Add(value.Id, new WeakReference<TowerDefenseInGamePacketShow>(card));
		}
		return new XWModCardHandle(_session, value.Id);
	}

	internal static void Unbind(TowerDefenseInGamePacketShow card)
	{
		XWModPresentation.UnmountCard(card);
		if (_ids.TryGetValue(card, out var value))
		{
			Cards.Remove(value.Id);
		}
		_ids.Remove(card);
	}

	internal static bool TryResolve(XWModCardHandle handle, out TowerDefenseInGamePacketShow card)
	{
		CheckSession();
		card = null;
		if (_session != Guid.Empty && handle.SessionId == _session && Cards.TryGetValue(handle.Id, out var value) && value.TryGetTarget(out card) && GodotObject.IsInstanceValid(card) && !card.IsQueuedForDeletion() && card.IsInsideTree() && !card.onlyDraw)
		{
			if (card.HasSunAccount)
			{
				return card.SunAccountId.IsLocal;
			}
			return true;
		}
		return false;
	}

	internal static IReadOnlyList<XWModCardState> GetCards()
	{
		CheckSession();
		List<XWModCardState> list = new List<XWModCardState>();
		foreach (long item in new List<long>(Cards.Keys))
		{
			if (TryResolve(new XWModCardHandle(_session, item), out var card))
			{
				list.Add(card.GetModCardState());
			}
			else
			{
				Cards.Remove(item);
			}
		}
		return list.AsReadOnly();
	}

	internal static void Notify(TowerDefenseInGamePacketShow card, string phase, XWModEventOrigin origin = XWModEventOrigin.Gameplay)
	{
		CheckSession();
		if (card.onlyDraw || (card.HasSunAccount && !card.SunAccountId.IsLocal))
		{
			return;
		}
		XWModRuntimeSession current = XWModRuntimeSession.Current;
		if (current != null && current.HasRuntimeOwners)
		{
			XWModCardHandle handle = GetHandle(card);
			if (XWModExtensions.HasHandlers(XWModCardPoints.StateChanged.Id))
			{
				XWModExtensions.Notify(XWModCardPoints.StateChanged, new XWModCardEvent(handle, card.config?.saveKey ?? "", phase, card.coldDownTimer), XWModExtensions.CreateEventContext(origin, XWModAuthority.Owner, phase));
			}
		}
	}
}
