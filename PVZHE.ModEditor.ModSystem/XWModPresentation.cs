using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModPresentation
{
	private sealed class CardMount
	{
		public string Key;

		public XWModCardHandle Handle;

		public readonly List<XWModLifetime> Scopes = new List<XWModLifetime>();
	}

	private static readonly ConditionalWeakTable<XWModLifetime, object> CleanedScopes = new ConditionalWeakTable<XWModLifetime, object>();

	private static readonly Dictionary<string, int> CleanupWarnings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private static Control _battleRoot;

	private static readonly ConditionalWeakTable<TowerDefenseCharacter, List<XWModLifetime>> CharacterMounts = new ConditionalWeakTable<TowerDefenseCharacter, List<XWModLifetime>>();

	private static readonly ConditionalWeakTable<Control, CardMount> CardMounts = new ConditionalWeakTable<Control, CardMount>();

	private static readonly Dictionary<string, List<XWModLifetime>> Scopes = new Dictionary<string, List<XWModLifetime>>(StringComparer.OrdinalIgnoreCase);

	internal static void CleanupLocal(XWModLifetime scope)
	{
		if (CleanedScopes.TryGetValue(scope, out var _))
		{
			return;
		}
		CleanedScopes.Add(scope, new object());
		long start = XWModExtensions.BeginMeasurement("ui.cleanup.v3", scope.OwnerId);
		IReadOnlyList<Exception> readOnlyList = scope.Cleanup();
		Exception error = ((readOnlyList.Count == 0) ? null : new AggregateException($"{readOnlyList.Count} local cleanup error(s).", readOnlyList));
		XWModExtensions.EndMeasurement("ui.cleanup.v3", scope.OwnerId, start, error);
		if (readOnlyList.Count == 0 || (!CleanupWarnings.TryGetValue(scope.OwnerId, out var value2) && CleanupWarnings.Count >= 1024))
		{
			return;
		}
		foreach (Exception item in readOnlyList)
		{
			if (value2 >= 3)
			{
				break;
			}
			string text = item.GetBaseException().Message;
			if (text.Length > 512)
			{
				text = text.Substring(0, 512);
			}
			GD.PushWarning("[Mod:" + scope.OwnerId + "] local cleanup failed: " + text);
			value2++;
		}
		CleanupWarnings[scope.OwnerId] = value2;
	}

	[ModuleInitializer]
	internal static void Install()
	{
		XWModExtensions.LocalPresentationFault += CloseOwner;
		XWModExtensions.OwnerEnding += CloseOwner;
	}

	internal static XWModUiSurface Surface(string owner, Control parent, Node scene, string kind, XWModEntityHandle entity = default(XWModEntityHandle), string key = "")
	{
		XWModLifetime scope = new XWModLifetime(owner);
		if (!Scopes.TryGetValue(owner, out var entries))
		{
			Scopes[owner] = (entries = new List<XWModLifetime>());
		}
		entries.Add(scope);
		Control control = new Control
		{
			Name = "Mod_" + owner.Replace('.', '_'),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		parent.AddChild(control, forceReadableName: false, Node.InternalMode.Disabled);
		control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize);
		scope.TrackNode(control);
		scene.TreeExited += Exit;
		scope.Track(delegate
		{
			if (GodotObject.IsInstanceValid(scene))
			{
				scene.TreeExited -= Exit;
			}
			entries.Remove(scope);
		});
		return new XWModUiSurface(control, scope, kind, entity, key, (parent is TowerDefenseInGamePacketShow card) ? XWModCards.GetHandle(card) : default(XWModCardHandle));
		void Exit()
		{
			CleanupLocal(scope);
		}
	}

	internal static void MountMainMenu(Control root)
	{
		XWModExtensions.NotifyForOwners(XWModPresentationPoints.MainMenu, (string owner) => Surface(owner, root, root, "main-menu"), XWModExtensions.CreateEventContext(XWModEventOrigin.InitialBinding, XWModAuthority.Local));
	}

	internal static void MountBattle(Node battle)
	{
		if (!XWModExtensions.HasHandlers(XWModPresentationPoints.BattleHud.Id) && !XWModExtensions.HasHandlers(XWModPresentationPoints.CharacterOverlay.Id))
		{
			_battleRoot = null;
			return;
		}
		CanvasLayer canvasLayer = new CanvasLayer
		{
			Name = "ModHudLayer",
			Layer = 5
		};
		battle.AddChild(canvasLayer, forceReadableName: false, Node.InternalMode.Disabled);
		_battleRoot = new Control
		{
			Name = "ModHud",
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		canvasLayer.AddChild(_battleRoot, forceReadableName: false, Node.InternalMode.Disabled);
		_battleRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize);
		Control root = _battleRoot;
		XWModExtensions.NotifyForOwners(XWModPresentationPoints.BattleHud, (string owner) => Surface(owner, root, battle, "battle-hud"), XWModExtensions.CreateEventContext(XWModEventOrigin.InitialBinding, XWModAuthority.Local));
	}

	internal static void MountCharacter(TowerDefenseCharacter character)
	{
		if (!XWModExtensions.HasHandlers(XWModPresentationPoints.CharacterOverlay.Id) || !GodotObject.IsInstanceValid(_battleRoot) || CharacterMounts.TryGetValue(character, out var _))
		{
			return;
		}
		List<XWModLifetime> scopes = new List<XWModLifetime>();
		CharacterMounts.Add(character, scopes);
		XWModExtensions.NotifyForOwners(XWModPresentationPoints.CharacterOverlay, delegate(string owner)
		{
			XWModUiSurface surface = Surface(owner, _battleRoot, character, "character-overlay", XWModCharacters.GetHandle(character));
			scopes.Add(surface.Lifetime);
			surface.Lifetime.Track(delegate
			{
				scopes.Remove(surface.Lifetime);
			});
			surface.Root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft, Control.LayoutPresetMode.Minsize);
			surface.Root.Size = Vector2.Zero;
			XWModOverlayAnchor node = new XWModOverlayAnchor
			{
				Target = character,
				Surface = surface.Root
			};
			surface.Root.AddChild(node, forceReadableName: false, Node.InternalMode.Disabled);
			return surface;
		}, XWModExtensions.CreateEventContext(XWModEventOrigin.InitialBinding, XWModAuthority.Local));
	}

	internal static void MountCard(Control card, string contentKey)
	{
		if (!XWModExtensions.HasHandlers(XWModPresentationPoints.CardAugment.Id))
		{
			return;
		}
		CardMount state = CardMounts.GetValue(card, (Control _) => new CardMount());
		XWModCardHandle xWModCardHandle = ((card is TowerDefenseInGamePacketShow card2) ? XWModCards.GetHandle(card2) : default(XWModCardHandle));
		if (state.Key == contentKey && state.Handle == xWModCardHandle)
		{
			return;
		}
		XWModLifetime[] array = state.Scopes.ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			CleanupLocal(array[num]);
		}
		state.Scopes.Clear();
		state.Key = contentKey;
		state.Handle = xWModCardHandle;
		XWModExtensions.NotifyForOwners(XWModPresentationPoints.CardAugment, delegate(string owner)
		{
			Control parent = card;
			Control scene = card;
			string key = contentKey;
			XWModUiSurface surface = Surface(owner, parent, scene, "card", default(XWModEntityHandle), key);
			state.Scopes.Add(surface.Lifetime);
			surface.Lifetime.Track(delegate
			{
				state.Scopes.Remove(surface.Lifetime);
			});
			return surface;
		}, XWModExtensions.CreateEventContext(XWModEventOrigin.InitialBinding, XWModAuthority.Local));
	}

	internal static void UnmountCard(Control card)
	{
		if (CardMounts.TryGetValue(card, out var value))
		{
			XWModLifetime[] array = value.Scopes.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				CleanupLocal(array[i]);
			}
			CardMounts.Remove(card);
		}
	}

	internal static void UnmountCharacter(TowerDefenseCharacter character)
	{
		if (CharacterMounts.TryGetValue(character, out var value))
		{
			XWModLifetime[] array = value.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				CleanupLocal(array[i]);
			}
			CharacterMounts.Remove(character);
		}
	}

	internal static void CloseBattle()
	{
		_battleRoot = null;
	}

	private static void CloseOwner(string owner)
	{
		if (Scopes.Remove(owner, out var value))
		{
			XWModLifetime[] array = value.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				CleanupLocal(array[i]);
			}
		}
	}
}
