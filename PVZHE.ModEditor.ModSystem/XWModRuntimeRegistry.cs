using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModRuntimeRegistry
{
	public sealed class Registration
	{
		public string OwnerMod { get; set; } = "";

		public XWModContentId ContentId { get; internal set; }

		public bool IsCompatibilityAlias { get; internal set; }

		public bool IsLegacyContent { get; internal set; }

		public string Category { get; set; } = "";

		public string Key { get; set; } = "";

		public Variant OriginalValue { get; set; }

		public Variant ModValue { get; set; }

		public bool OverridesBuiltIn { get; set; }

		public bool IsOverride { get; set; }

		public long LoadOrder { get; set; }

		public bool IsEffective { get; internal set; }
	}

	private sealed class RegistrationSlot
	{
		public string Category { get; set; } = "";

		public string Key { get; set; } = "";

		public Variant OriginalValue { get; set; }

		public bool HasOriginalValue { get; set; }

		public bool HasOriginalProjectileData { get; set; }

		public TowerDefenseProjectileData OriginalProjectileData { get; set; }

		public bool HasOriginalProjectileResource { get; set; }

		public Resource OriginalProjectileResource { get; set; }

		public Action RestoreOriginalRegistration { get; set; }

		public List<Registration> Layers { get; } = new List<Registration>();
	}

	private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, RegistrationSlot>> Slots = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, RegistrationSlot>>(StringComparer.OrdinalIgnoreCase);

	private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Registration>> Registrations = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Registration>>(StringComparer.OrdinalIgnoreCase);

	private static readonly System.Collections.Generic.Dictionary<string, Texture2D> RuntimeTextures = new System.Collections.Generic.Dictionary<string, Texture2D>(StringComparer.Ordinal);

	private static readonly System.Collections.Generic.Dictionary<string, AdobeAnimateAtlasProfile> RuntimeAnimationAtlasProfiles = new System.Collections.Generic.Dictionary<string, AdobeAnimateAtlasProfile>(StringComparer.Ordinal);

	private static long _nextLoadOrder;

	private static readonly System.Collections.Generic.Dictionary<string, int> PlanOrder = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private static readonly System.Collections.Generic.Dictionary<(string Category, string Key), List<XWModContentId>> Aliases = new System.Collections.Generic.Dictionary<(string, string), List<XWModContentId>>();

	private static bool _structureFrozen;

	public static bool IsStructureFrozen => _structureFrozen;

	public static void ConfigureOrder(IReadOnlyDictionary<string, int> order)
	{
		if (_structureFrozen)
		{
			throw new InvalidOperationException("Mod content structure is frozen until restart.");
		}
		PlanOrder.Clear();
		if (order != null)
		{
			foreach (KeyValuePair<string, int> item in order)
			{
				PlanOrder[item.Key] = item.Value;
			}
		}
		foreach (RegistrationSlot item2 in Slots.Values.SelectMany((System.Collections.Generic.Dictionary<string, RegistrationSlot> category) => category.Values))
		{
			foreach (Registration layer in item2.Layers)
			{
				if (PlanOrder.TryGetValue(layer.OwnerMod, out var value))
				{
					layer.LoadOrder = value;
				}
			}
			SortLayers(item2);
			RestoreEffectiveRuntimeValue(item2);
			RefreshEffectiveRegistration(item2);
		}
		RefreshAliases();
	}

	public static void BeginRegistration()
	{
		_structureFrozen = false;
	}

	public static void FreezeStructure()
	{
		_structureFrozen = true;
	}

	public static bool Register(string ownerMod, string category, string key, Variant value, bool allowOverride = false)
	{
		string diagnostic;
		return Register(ownerMod, category, key, value, allowOverride, out diagnostic);
	}

	public static bool Register(string ownerMod, string category, string key, Variant value, bool allowOverride, out string diagnostic)
	{
		category = NormalizeCategory(category?.Trim() ?? "");
		key = key?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(ownerMod) || category.Length == 0 || key.Length == 0)
		{
			diagnostic = "owner, category and key must all be non-empty";
			return false;
		}
		Registration registration;
		bool flag = TryGetEffectiveRegistration(category, key, out registration);
		Variant value2;
		bool flag2 = !flag && TryGetRuntimeValue(category, key, out value2);
		if (allowOverride)
		{
			if (!flag && !flag2)
			{
				diagnostic = "overrides target is missing: " + category + "/" + key;
				return false;
			}
			return RegisterCore(ownerMod, flag ? registration.ContentId : new XWModContentId("", category, key), value, allowOverride: true, out diagnostic);
		}
		if (flag2 || (flag && registration.ContentId.IsBuiltIn))
		{
			bool num = RegisterCore(ownerMod, new XWModContentId("", category, key), value, allowOverride: true, out diagnostic);
			if (num)
			{
				diagnostic = "compatibility warning: provides implicitly overrides built-in " + category + "/" + key;
			}
			return num;
		}
		XWModContentId xWModContentId = new XWModContentId(ownerMod, category, key);
		if (!RegisterCore(ownerMod, xWModContentId, value, allowOverride: false, out diagnostic))
		{
			return false;
		}
		foreach (Registration item in GetRegistrationStack(category, xWModContentId.RuntimeKey))
		{
			item.IsLegacyContent = true;
		}
		(string, string) key2 = (category, key);
		if (!Aliases.TryGetValue(key2, out var value3))
		{
			value3 = (Aliases[key2] = new List<XWModContentId>());
		}
		if (!value3.Contains(xWModContentId))
		{
			value3.Add(xWModContentId);
		}
		RefreshAliases();
		if (value3.Count > 1)
		{
			diagnostic = $"compatibility warning: alias {category}/{key} has {value3.Count} providers; later plan order wins";
		}
		return true;
	}

	public static bool Provide(string ownerMod, string category, string key, Variant value, out string diagnostic)
	{
		if (!XWModContentId.TryCreate(ownerMod, category, key, out var identity))
		{
			diagnostic = "invalid content identity";
			return false;
		}
		return RegisterCore(ownerMod, identity, value, allowOverride: false, out diagnostic, validateIdentity: true);
	}

	public static bool Override(string ownerMod, XWModContentId target, Variant value, out string diagnostic)
	{
		if (target == null)
		{
			diagnostic = "missing content target";
			return false;
		}
		return RegisterCore(ownerMod, target, value, allowOverride: true, out diagnostic, validateIdentity: true);
	}

	internal static string GetResourceKey(XWModContentId identity)
	{
		if (!Slots.TryGetValue(identity.Category, out var value) || !value.TryGetValue(identity.RuntimeKey, out var value2) || !value2.Layers.Any((Registration layer) => layer.IsLegacyContent))
		{
			return identity.RuntimeKey;
		}
		return identity.Key;
	}

	private static bool RegisterCore(string ownerMod, XWModContentId identity, Variant value, bool allowOverride, out string diagnostic, bool validateIdentity = false)
	{
		diagnostic = "";
		if (_structureFrozen)
		{
			diagnostic = "Mod content structure is frozen; restart to change registrations";
			return false;
		}
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			diagnostic = "Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。";
			return false;
		}
		if (string.IsNullOrWhiteSpace(ownerMod) || identity == null || !identity.IsValid)
		{
			diagnostic = "owner and content identity must be valid";
			return false;
		}
		ownerMod = ownerMod.Trim().ToLowerInvariant();
		string category = identity.Category;
		string runtimeKey = identity.RuntimeKey;
		if (identity.IsBuiltIn && Aliases.ContainsKey((category, runtimeKey)))
		{
			diagnostic = $"built-in target does not exist; '{category}/{runtimeKey}' is a Mod compatibility alias";
			return false;
		}
		if (validateIdentity && value.VariantType == Variant.Type.Object && value.AsGodotObject() is Resource resource && !ModLoader.ValidateContentResourceIdentity(category, GetResourceKey(identity), resource, null, out diagnostic))
		{
			return false;
		}
		if (!CanSetRuntimeValue(category, value))
		{
			diagnostic = "runtime value has the wrong type or unsupported category: " + category + "/" + runtimeKey;
			return false;
		}
		if (!Slots.TryGetValue(category, out var value2))
		{
			value2 = (Slots[category] = new System.Collections.Generic.Dictionary<string, RegistrationSlot>(StringComparer.Ordinal));
		}
		if (!value2.TryGetValue(runtimeKey, out var value3))
		{
			Variant value4;
			bool flag = TryGetRuntimeValue(category, runtimeKey, out value4);
			if (allowOverride && (!identity.IsBuiltIn || !flag))
			{
				RemoveEmptySlot(value2, category, runtimeKey);
				diagnostic = $"overrides target is missing: {identity}";
				return false;
			}
			if (!allowOverride && flag)
			{
				RemoveEmptySlot(value2, category, runtimeKey);
				diagnostic = $"content identity already exists: {identity}";
				return false;
			}
			TowerDefenseProjectileData value5 = null;
			Resource value6 = null;
			bool hasOriginalProjectileData = category == "Projectile" && TowerDefenseProjectileRegistry.ProjectileDictionary.TryGetValue(new StringName(runtimeKey), out value5);
			bool hasOriginalProjectileResource = category == "Projectile" && (ResourceManager.Instance?.PROJECTILE_CONFIG.TryGetValue(runtimeKey, out value6) ?? false);
			RegistrationSlot registrationSlot = new RegistrationSlot();
			registrationSlot.Category = category;
			registrationSlot.Key = runtimeKey;
			registrationSlot.OriginalValue = value4;
			registrationSlot.HasOriginalValue = flag;
			registrationSlot.HasOriginalProjectileData = hasOriginalProjectileData;
			registrationSlot.OriginalProjectileData = value5;
			registrationSlot.HasOriginalProjectileResource = hasOriginalProjectileResource;
			registrationSlot.OriginalProjectileResource = value6;
			RegistrationSlot registrationSlot2 = registrationSlot;
			registrationSlot2.RestoreOriginalRegistration = category switch
			{
				"Feature" => TowerDefenseBattleRegistry.CaptureFeatureRegistration(runtimeKey), 
				"Process" => TowerDefenseBattleRegistry.CaptureProcessRegistration(runtimeKey), 
				"Projectile" => CaptureProjectileRegistration(runtimeKey), 
				_ => null, 
			};
			value3 = (value2[runtimeKey] = registrationSlot);
		}
		Registration registration = value3.Layers.FirstOrDefault((Registration layer) => StringComparer.OrdinalIgnoreCase.Equals(layer.OwnerMod, ownerMod));
		Variant modValue = registration?.ModValue ?? default(Variant);
		int value7;
		Registration registration2 = registration ?? new Registration
		{
			OwnerMod = ownerMod,
			Category = category,
			Key = runtimeKey,
			ContentId = identity,
			OriginalValue = value3.OriginalValue,
			OverridesBuiltIn = identity.IsBuiltIn,
			LoadOrder = (PlanOrder.TryGetValue(ownerMod, out value7) ? value7 : (++_nextLoadOrder))
		};
		registration2.ModValue = value;
		registration2.IsOverride = allowOverride;
		registration2.IsLegacyContent = value3.Layers.Any((Registration layer) => layer.IsLegacyContent);
		if (registration == null)
		{
			value3.Layers.Add(registration2);
		}
		SortLayers(value3);
		if (category == "Projectile")
		{
			value3.RestoreOriginalRegistration?.Invoke();
		}
		List<Registration> layers = value3.Layers;
		if (!TrySetRuntimeValue(category, runtimeKey, layers[layers.Count - 1].ModValue, allowOverride: true))
		{
			if (registration == null)
			{
				value3.Layers.Remove(registration2);
			}
			else
			{
				registration.ModValue = modValue;
			}
			RestoreEffectiveRuntimeValue(value3);
			if (value3.Layers.Count == 0)
			{
				RemoveEmptySlot(value2, category, runtimeKey);
			}
			diagnostic = $"runtime registry could not write: {identity}";
			return false;
		}
		RefreshEffectiveRegistration(value3);
		RefreshAliases();
		XWModEnvironmentService.Invalidate("RuntimeRegistrationChanged");
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 3);
		defaultInterpolatedStringHandler.AppendFormatted(allowOverride ? "override" : "provide");
		defaultInterpolatedStringHandler.AppendLiteral(" ");
		defaultInterpolatedStringHandler.AppendFormatted(identity);
		defaultInterpolatedStringHandler.AppendLiteral("; effective owner '");
		List<Registration> layers2 = value3.Layers;
		defaultInterpolatedStringHandler.AppendFormatted(layers2[layers2.Count - 1].OwnerMod);
		defaultInterpolatedStringHandler.AppendLiteral("'");
		diagnostic = defaultInterpolatedStringHandler.ToStringAndClear();
		return true;
	}

	private static void SortLayers(RegistrationSlot slot)
	{
		slot.Layers.Sort(delegate(Registration a, Registration b)
		{
			int num = PlanOrder.GetValueOrDefault(a.OwnerMod).CompareTo(PlanOrder.GetValueOrDefault(b.OwnerMod));
			if (num != 0)
			{
				return num;
			}
			return (PlanOrder.Count == 0) ? a.LoadOrder.CompareTo(b.LoadOrder) : StringComparer.Ordinal.Compare(a.OwnerMod, b.OwnerMod);
		});
	}

	private static void RefreshAliases()
	{
		KeyValuePair<(string, string), List<XWModContentId>>[] array = Aliases.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<(string, string), List<XWModContentId>> keyValuePair = array[i];
			keyValuePair.Value.RemoveAll((XWModContentId id) => !Slots.TryGetValue(id.Category, out var value3) || !value3.TryGetValue(id.RuntimeKey, out var value4) || value4.Layers.Count == 0);
			Registration[] array2 = (from layer in keyValuePair.Value.Select(delegate(XWModContentId id)
				{
					if (!Slots.TryGetValue(id.Category, out var value3) || !value3.TryGetValue(id.RuntimeKey, out var value4) || value4.Layers.Count <= 0)
					{
						return (Registration)null;
					}
					List<Registration> layers = value4.Layers;
					return layers[layers.Count - 1];
				})
				where layer != null
				orderby PlanOrder.GetValueOrDefault(layer.OwnerMod), (PlanOrder.Count != 0) ? 0 : layer.LoadOrder
				select layer).ThenBy<Registration, string>((Registration layer) => layer.OwnerMod, StringComparer.Ordinal).ToArray();
			if (array2.Length == 0)
			{
				RemoveRuntimeValue(keyValuePair.Key.Item1, keyValuePair.Key.Item2);
				if (Registrations.TryGetValue(keyValuePair.Key.Item1, out var value))
				{
					value.Remove(keyValuePair.Key.Item2);
				}
				Aliases.Remove(keyValuePair.Key);
				continue;
			}
			Registration registration = array2[^1];
			TrySetRuntimeValue(keyValuePair.Key.Item1, keyValuePair.Key.Item2, registration.ModValue, allowOverride: true);
			if (!Registrations.TryGetValue(keyValuePair.Key.Item1, out var value2))
			{
				value2 = (Registrations[keyValuePair.Key.Item1] = new System.Collections.Generic.Dictionary<string, Registration>(StringComparer.Ordinal));
			}
			value2[keyValuePair.Key.Item2] = new Registration
			{
				OwnerMod = registration.OwnerMod,
				Category = registration.Category,
				Key = keyValuePair.Key.Item2,
				ContentId = registration.ContentId,
				ModValue = registration.ModValue,
				LoadOrder = registration.LoadOrder,
				IsOverride = registration.IsOverride,
				IsEffective = true,
				IsCompatibilityAlias = true,
				IsLegacyContent = true
			};
		}
	}

	public static int UnregisterOwner(string ownerMod)
	{
		using XWModEnvironmentService.EnvironmentMutation environmentMutation = XWModEnvironmentService.TryBeginEnvironmentMutation();
		if (environmentMutation == null)
		{
			GD.PushWarning("Mod 环境正在确认或用于联机战斗，请取消确认或退出战斗后重试。");
			return 0;
		}
		if (string.IsNullOrWhiteSpace(ownerMod))
		{
			return 0;
		}
		int num = 0;
		KeyValuePair<string, System.Collections.Generic.Dictionary<string, RegistrationSlot>>[] array = Slots.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, System.Collections.Generic.Dictionary<string, RegistrationSlot>> keyValuePair = array[i];
			KeyValuePair<string, RegistrationSlot>[] array2 = keyValuePair.Value.ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				KeyValuePair<string, RegistrationSlot> keyValuePair2 = array2[j];
				RegistrationSlot value = keyValuePair2.Value;
				int num2 = value.Layers.RemoveAll((Registration layer) => StringComparer.OrdinalIgnoreCase.Equals(layer.OwnerMod, ownerMod) || (!layer.ContentId.IsBuiltIn && StringComparer.OrdinalIgnoreCase.Equals(layer.ContentId.ProviderModId, ownerMod)));
				if (num2 == 0)
				{
					continue;
				}
				num += num2;
				RestoreEffectiveRuntimeValue(value);
				if (value.Layers.Count == 0)
				{
					keyValuePair.Value.Remove(keyValuePair2.Key);
					if (Registrations.TryGetValue(keyValuePair.Key, out var value2))
					{
						value2.Remove(keyValuePair2.Key);
					}
				}
				else
				{
					RefreshEffectiveRegistration(value);
				}
			}
			if (keyValuePair.Value.Count == 0)
			{
				Slots.Remove(keyValuePair.Key);
			}
		}
		RefreshAliases();
		string[] array3 = (from pair in Registrations
			where pair.Value.Count == 0
			select pair.Key).ToArray();
		foreach (string key in array3)
		{
			Registrations.Remove(key);
		}
		if (num > 0)
		{
			XWModEnvironmentService.Invalidate("RuntimeRegistrationRemoved");
		}
		return num;
	}

	public static IReadOnlyDictionary<string, System.Collections.Generic.Dictionary<string, Registration>> GetRegistrations()
	{
		return Registrations;
	}

	public static IReadOnlyList<Registration> GetRegistrationStack(string category, string key)
	{
		category = NormalizeCategory(category?.Trim() ?? "");
		key = key?.Trim() ?? "";
		if (Slots.TryGetValue(category, out var value) && value.TryGetValue(key, out var value2))
		{
			return value2.Layers.ToArray();
		}
		System.Collections.Generic.Dictionary<string, RegistrationSlot> value4;
		RegistrationSlot value5;
		if (Aliases.TryGetValue((category, key), out var value3))
		{
			return (from layer in value3.SelectMany((XWModContentId id) => (!Slots.TryGetValue(category, out value4) || !value4.TryGetValue(id.RuntimeKey, out value5)) ? Enumerable.Empty<Registration>() : value5.Layers)
				orderby PlanOrder.GetValueOrDefault(layer.OwnerMod), (PlanOrder.Count != 0) ? 0 : layer.LoadOrder
				select layer).ThenBy<Registration, string>((Registration layer) => layer.OwnerMod, StringComparer.Ordinal).ToArray();
		}
		return System.Array.Empty<Registration>();
	}

	public static bool TryGetEffectiveRegistration(string category, string key, out Registration registration)
	{
		category = NormalizeCategory(category?.Trim() ?? "");
		key = key?.Trim() ?? "";
		registration = null;
		if (Registrations.TryGetValue(category, out var value))
		{
			return value.TryGetValue(key, out registration);
		}
		return false;
	}

	public static IReadOnlyDictionary<string, Texture2D> GetRuntimeTextures()
	{
		return RuntimeTextures;
	}

	public static bool TryGetRuntimeTexture(string key, out Texture2D texture)
	{
		texture = null;
		if (!string.IsNullOrWhiteSpace(key) && RuntimeTextures.TryGetValue(key.Trim(), out texture))
		{
			return GodotObject.IsInstanceValid(texture);
		}
		return false;
	}

	public static IReadOnlyDictionary<string, AdobeAnimateAtlasProfile> GetRuntimeAnimationAtlasProfiles()
	{
		return RuntimeAnimationAtlasProfiles;
	}

	public static bool TryGetRuntimeAnimationAtlasProfile(string key, out AdobeAnimateAtlasProfile profile)
	{
		profile = null;
		if (!string.IsNullOrWhiteSpace(key) && RuntimeAnimationAtlasProfiles.TryGetValue(key.Trim(), out profile))
		{
			return GodotObject.IsInstanceValid(profile);
		}
		return false;
	}

	private static void RestoreEffectiveRuntimeValue(RegistrationSlot slot)
	{
		if (slot.Category == "Projectile")
		{
			slot.RestoreOriginalRegistration?.Invoke();
			if (slot.Layers.Count > 0)
			{
				string category = slot.Category;
				string key = slot.Key;
				List<Registration> layers = slot.Layers;
				SetRuntimeValue(category, key, layers[layers.Count - 1].ModValue, allowOverride: true);
			}
		}
		else if (slot.Layers.Count > 0)
		{
			string category2 = slot.Category;
			string key2 = slot.Key;
			List<Registration> layers2 = slot.Layers;
			SetRuntimeValue(category2, key2, layers2[layers2.Count - 1].ModValue, allowOverride: true);
		}
		else if (slot.RestoreOriginalRegistration != null)
		{
			slot.RestoreOriginalRegistration();
		}
		else if (slot.HasOriginalValue)
		{
			SetRuntimeValue(slot.Category, slot.Key, slot.OriginalValue, allowOverride: true);
		}
		else
		{
			RemoveRuntimeValue(slot.Category, slot.Key);
		}
	}

	private static void RefreshEffectiveRegistration(RegistrationSlot slot)
	{
		foreach (Registration layer in slot.Layers)
		{
			layer.IsEffective = false;
		}
		if (slot.Layers.Count != 0)
		{
			List<Registration> layers = slot.Layers;
			Registration registration = layers[layers.Count - 1];
			registration.IsEffective = true;
			if (!Registrations.TryGetValue(slot.Category, out var value))
			{
				value = new System.Collections.Generic.Dictionary<string, Registration>(StringComparer.Ordinal);
				Registrations[slot.Category] = value;
			}
			value[slot.Key] = registration;
		}
	}

	private static void RemoveEmptySlot(System.Collections.Generic.Dictionary<string, RegistrationSlot> categorySlots, string category, string key)
	{
		categorySlots.Remove(key);
		if (categorySlots.Count == 0)
		{
			Slots.Remove(category);
		}
	}

	private static string DescribeLowerLayer(RegistrationSlot slot)
	{
		if (slot.Layers.Count > 1)
		{
			List<Registration> layers = slot.Layers;
			return "Mod owner '" + layers[layers.Count - 2].OwnerMod + "'";
		}
		if (!slot.HasOriginalValue)
		{
			return "a missing lower value";
		}
		return "the built-in runtime value";
	}

	private static string DeclarationName(bool allowOverride)
	{
		if (!allowOverride)
		{
			return "provide";
		}
		return "override";
	}

	private static Action CaptureProjectileRegistration(string key)
	{
		StringName projectileName = new StringName(key);
		TowerDefenseProjectileRegistry.ProjectileRegistrationSnapshot snapshot = TowerDefenseProjectileRegistry.CaptureProjectileRegistration(projectileName);
		System.Collections.Generic.Dictionary<string, Resource> dictionary = ResourceManager.Instance?.PROJECTILE_CONFIG;
		Resource original = null;
		bool hasResource = dictionary?.TryGetValue(key, out original) ?? false;
		return delegate
		{
			TowerDefenseProjectileRegistry.RestoreProjectileRegistration(projectileName, snapshot);
			System.Collections.Generic.Dictionary<string, Resource> dictionary2 = ResourceManager.Instance?.PROJECTILE_CONFIG;
			if (dictionary2 != null)
			{
				if (hasResource)
				{
					dictionary2[key] = original;
				}
				else
				{
					dictionary2.Remove(key);
				}
			}
		};
	}

	private static void SetRuntimeValue(string category, string key, Variant value, bool allowOverride)
	{
		TrySetRuntimeValue(category, key, value, allowOverride);
	}

	private static void RemoveRuntimeValue(string category, string key)
	{
		TryRemoveRuntimeValue(category, key);
	}

	private static bool TryGetOriginalRuntimeValue(string category, string key, Variant candidate, out Variant value)
	{
		if (category == "Projectile" && candidate.VariantType == Variant.Type.Object && candidate.AsGodotObject() is TowerDefenseProjectileConfig)
		{
			System.Collections.Generic.Dictionary<string, Resource> dictionary = ResourceManager.Instance?.PROJECTILE_CONFIG;
			if (dictionary != null)
			{
				return TryGetObjectDictionaryValue(dictionary, key, out value);
			}
			value = default(Variant);
			return false;
		}
		return TryGetRuntimeValue(category, key, out value);
	}

	private static bool TryGetRuntimeValue(string category, string key, out Variant value)
	{
		if (CategoryMatches(category, "Texture", "Textures", "Image", "Images"))
		{
			return TryGetObjectDictionaryValue(RuntimeTextures, key, out value);
		}
		if (CategoryMatches(category, "AnimationAtlas", "AnimationAtlases", "AtlasProfile", "AtlasProfiles"))
		{
			return TryGetObjectDictionaryValue(RuntimeAnimationAtlasProfiles, key, out value);
		}
		if (CategoryMatches(category, "Projectile", "Projectiles") && TowerDefenseProjectileRegistry.ProjectileDictionary.TryGetValue(new StringName(key), out var value2))
		{
			value = Variant.From(in value2);
			return true;
		}
		ResourceManager instance = ResourceManager.Instance;
		if (instance != null)
		{
			if (CategoryMatches(category, "Level", "Levels"))
			{
				return TryGetGodotDictionaryValue(instance.LEVELS, key, out value);
			}
			if (CategoryMatches(category, "Audio", "Audios"))
			{
				return TryGetObjectDictionaryValue(instance.AUDIOS, key, out value);
			}
			if (CategoryMatches(category, "BGM", "Bgms"))
			{
				instance.EnsureAllBgmsLoaded();
				return TryGetObjectDictionaryValue(instance.BGMS, key, out value);
			}
			if (CategoryMatches(category, "Map", "Maps"))
			{
				return TryGetObjectDictionaryValue(instance.MAPS, key, out value);
			}
			if (CategoryMatches(category, "Projectile", "Projectiles"))
			{
				return TryGetObjectDictionaryValue(instance.PROJECTILE_CONFIG, key, out value);
			}
			if (CategoryMatches(category, "CharacterSprite"))
			{
				return TryGetObjectDictionaryValue(instance.CHARCTAER_SPRITE, key, out value);
			}
			if (CategoryMatches(category, "Character", "Characters"))
			{
				return TryGetObjectDictionaryValue(instance.TOWERDEFENSE_CHARCATERS, key, out value);
			}
			if (CategoryMatches(category, "Packet", "Packets", "Card", "Cards"))
			{
				return TryGetObjectDictionaryValue(instance.TOWERDEFENSE_PACKETS, key, out value);
			}
			if (CategoryMatches(category, "PacketBank", "PacketBanks"))
			{
				return TryGetObjectDictionaryValue(instance.TOWERDEFENSE_PACKETBANKS, key, out value);
			}
			if (CategoryMatches(category, "Tutorial", "Tutorials"))
			{
				return TryGetObjectDictionaryValue(instance.TUTORIALS, key, out value);
			}
			if (CategoryMatches(category, "NpcTalk", "NpcTalks", "Talk", "Talks"))
			{
				return TryGetObjectDictionaryValue(instance.TALKS, key, out value);
			}
			if (CategoryMatches(category, "Collectable", "Collectables"))
			{
				return TryGetObjectDictionaryValue(instance.COLLECTABLES, key, out value);
			}
			if (CategoryMatches(category, "Shovel", "Shovels"))
			{
				return TryGetObjectDictionaryValue(instance.SHOVELS, key, out value);
			}
			if (CategoryMatches(category, "Mower", "Mowers"))
			{
				return TryGetObjectDictionaryValue(instance.MOWERS, key, out value);
			}
			if (CategoryMatches(category, "Shop", "Shops"))
			{
				return TryGetObjectDictionaryValue(instance.SHOPS, key, out value);
			}
			if (CategoryMatches(category, "Survival", "Survivals"))
			{
				return TryGetObjectDictionaryValue(instance.SURVIVALS, key, out value);
			}
		}
		if (CategoryMatches(category, "Feature", "Features"))
		{
			return TryGetStringNameObjectDictionaryValue(TowerDefenseBattleRegistry.BattleFeatureDictionary, key, out value);
		}
		if (CategoryMatches(category, "Process", "Processes"))
		{
			return TryGetStringNameObjectDictionaryValue(TowerDefenseBattleRegistry.BattleProcessDictionary, key, out value);
		}
		if (CategoryMatches(category, "ProjectileChange", "ProjectileChanges"))
		{
			return TryGetStringNameObjectDictionaryValue(TowerDefenseProjectileRegistry.ProjectileChangeDataDictionary, key, out value);
		}
		value = default(Variant);
		return false;
	}

	private static bool TrySetRuntimeValue(string category, string key, Variant value, bool allowOverride)
	{
		if (CategoryMatches(category, "Texture", "Textures", "Image", "Images"))
		{
			return TrySetObjectDictionaryValue(RuntimeTextures, key, value, allowOverride);
		}
		if (CategoryMatches(category, "AnimationAtlas", "AnimationAtlases", "AtlasProfile", "AtlasProfiles"))
		{
			RuntimeAnimationAtlasProfiles.TryGetValue(key, out var value2);
			bool flag = TrySetObjectDictionaryValue(RuntimeAnimationAtlasProfiles, key, value, allowOverride);
			if (!flag || !(value.AsGodotObject() is AdobeAnimateAtlasProfile adobeAnimateAtlasProfile))
			{
				return flag;
			}
			if (GodotObject.IsInstanceValid(value2) && value2 != adobeAnimateAtlasProfile)
			{
				AdobeAnimateDefinitionCache.ReleaseProfile(value2);
			}
			if (adobeAnimateAtlasProfile.StartupOnly)
			{
				AdobeAnimateGlobalAtlasCache.PreloadVisualTextureArray(adobeAnimateAtlasProfile);
				AdobeAnimateGlobalAtlasCache.PreloadGpuPoseTextureArray(adobeAnimateAtlasProfile);
			}
			return true;
		}
		ResourceManager instance = ResourceManager.Instance;
		if (instance != null)
		{
			if (CategoryMatches(category, "Level", "Levels"))
			{
				return TrySetGodotDictionaryValue(instance.LEVELS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Audio", "Audios"))
			{
				return TrySetObjectDictionaryValue(instance.AUDIOS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "BGM", "Bgms"))
			{
				instance.EnsureAllBgmsLoaded();
				return TrySetObjectDictionaryValue(instance.BGMS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Map", "Maps"))
			{
				return TrySetObjectDictionaryValue(instance.MAPS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Projectile", "Projectiles"))
			{
				if (value.VariantType != Variant.Type.Object || !(value.AsGodotObject() is Resource resource) || (!(resource is TowerDefenseProjectileData) && !(resource is TowerDefenseProjectileConfig)) || (!allowOverride && (instance.PROJECTILE_CONFIG.ContainsKey(key) || TowerDefenseProjectileRegistry.ProjectileDictionary.ContainsKey(new StringName(key)))))
				{
					return false;
				}
				if (resource is TowerDefenseProjectileData towerDefenseProjectileData)
				{
					TowerDefenseProjectileRegistry.RegisterProjectileOverlay(new StringName(key), towerDefenseProjectileData);
					if (!instance.PROJECTILE_CONFIG.TryGetValue(key, out var value3) || !(value3 is TowerDefenseProjectileConfig))
					{
						instance.PROJECTILE_CONFIG[key] = towerDefenseProjectileData;
					}
				}
				else
				{
					instance.PROJECTILE_CONFIG[key] = resource;
					TowerDefenseProjectileRegistry.NotifyProjectileConfigChanged();
				}
				return true;
			}
			if (CategoryMatches(category, "CharacterSprite"))
			{
				return TrySetObjectDictionaryValue(instance.CHARCTAER_SPRITE, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Character", "Characters"))
			{
				return TrySetObjectDictionaryValue(instance.TOWERDEFENSE_CHARCATERS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Packet", "Packets", "Card", "Cards"))
			{
				return TrySetObjectDictionaryValue(instance.TOWERDEFENSE_PACKETS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "PacketBank", "PacketBanks"))
			{
				return TrySetObjectDictionaryValue(instance.TOWERDEFENSE_PACKETBANKS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Tutorial", "Tutorials"))
			{
				return TrySetObjectDictionaryValue(instance.TUTORIALS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "NpcTalk", "NpcTalks", "Talk", "Talks"))
			{
				return TrySetObjectDictionaryValue(instance.TALKS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Collectable", "Collectables"))
			{
				return TrySetObjectDictionaryValue(instance.COLLECTABLES, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Shovel", "Shovels"))
			{
				return TrySetObjectDictionaryValue(instance.SHOVELS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Mower", "Mowers"))
			{
				return TrySetObjectDictionaryValue(instance.MOWERS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Shop", "Shops"))
			{
				return TrySetObjectDictionaryValue(instance.SHOPS, key, value, allowOverride);
			}
			if (CategoryMatches(category, "Survival", "Survivals"))
			{
				return TrySetObjectDictionaryValue(instance.SURVIVALS, key, value, allowOverride);
			}
		}
		if (CategoryMatches(category, "Feature", "Features"))
		{
			if (!(value.AsGodotObject() is TowerDefenseBattleFeature prototype) || (!allowOverride && TowerDefenseBattleRegistry.BattleFeatureDictionary.ContainsKey(key)))
			{
				return false;
			}
			TowerDefenseBattleRegistry.SetFeaturePrototype(key, prototype);
			return true;
		}
		if (CategoryMatches(category, "Process", "Processes"))
		{
			if (!(value.AsGodotObject() is TowerDefenseBattleProcess prototype2) || (!allowOverride && TowerDefenseBattleRegistry.BattleProcessDictionary.ContainsKey(key)))
			{
				return false;
			}
			TowerDefenseBattleRegistry.SetProcessPrototype(key, prototype2);
			return true;
		}
		if (CategoryMatches(category, "ProjectileChange", "ProjectileChanges"))
		{
			return TrySetStringNameObjectDictionaryValue(TowerDefenseProjectileRegistry.ProjectileChangeDataDictionary, key, value, allowOverride);
		}
		return false;
	}

	private static bool CanSetRuntimeValue(string category, Variant value)
	{
		if (CategoryMatches(category, "Texture", "Textures", "Image", "Images"))
		{
			return CanSetObjectDictionaryValue(RuntimeTextures, value);
		}
		if (CategoryMatches(category, "AnimationAtlas", "AnimationAtlases", "AtlasProfile", "AtlasProfiles"))
		{
			return CanSetObjectDictionaryValue(RuntimeAnimationAtlasProfiles, value);
		}
		ResourceManager instance = ResourceManager.Instance;
		if (instance != null)
		{
			if (CategoryMatches(category, "Level", "Levels"))
			{
				return value.VariantType == Variant.Type.Dictionary;
			}
			if (CategoryMatches(category, "Audio", "Audios"))
			{
				return CanSetObjectDictionaryValue(instance.AUDIOS, value);
			}
			if (CategoryMatches(category, "BGM", "Bgms"))
			{
				return CanSetObjectDictionaryValue(instance.BGMS, value);
			}
			if (CategoryMatches(category, "Map", "Maps"))
			{
				return CanSetObjectDictionaryValue(instance.MAPS, value);
			}
			if (CategoryMatches(category, "Projectile", "Projectiles"))
			{
				bool flag = instance.PROJECTILE_CONFIG != null && value.VariantType == Variant.Type.Object;
				if (flag)
				{
					GodotObject godotObject = value.AsGodotObject();
					bool flag2 = ((godotObject is TowerDefenseProjectileData || godotObject is TowerDefenseProjectileConfig) ? true : false);
					flag = flag2;
				}
				return flag;
			}
			if (CategoryMatches(category, "CharacterSprite"))
			{
				return CanSetObjectDictionaryValue(instance.CHARCTAER_SPRITE, value);
			}
			if (CategoryMatches(category, "Character", "Characters"))
			{
				return CanSetObjectDictionaryValue(instance.TOWERDEFENSE_CHARCATERS, value);
			}
			if (CategoryMatches(category, "Packet", "Packets", "Card", "Cards"))
			{
				return CanSetObjectDictionaryValue(instance.TOWERDEFENSE_PACKETS, value);
			}
			if (CategoryMatches(category, "PacketBank", "PacketBanks"))
			{
				return CanSetObjectDictionaryValue(instance.TOWERDEFENSE_PACKETBANKS, value);
			}
			if (CategoryMatches(category, "Tutorial", "Tutorials"))
			{
				return CanSetObjectDictionaryValue(instance.TUTORIALS, value);
			}
			if (CategoryMatches(category, "NpcTalk", "NpcTalks", "Talk", "Talks"))
			{
				return CanSetObjectDictionaryValue(instance.TALKS, value);
			}
			if (CategoryMatches(category, "Collectable", "Collectables"))
			{
				return CanSetObjectDictionaryValue(instance.COLLECTABLES, value);
			}
			if (CategoryMatches(category, "Shovel", "Shovels"))
			{
				return CanSetObjectDictionaryValue(instance.SHOVELS, value);
			}
			if (CategoryMatches(category, "Mower", "Mowers"))
			{
				return CanSetObjectDictionaryValue(instance.MOWERS, value);
			}
			if (CategoryMatches(category, "Shop", "Shops"))
			{
				return CanSetObjectDictionaryValue(instance.SHOPS, value);
			}
			if (CategoryMatches(category, "Survival", "Survivals"))
			{
				return CanSetObjectDictionaryValue(instance.SURVIVALS, value);
			}
		}
		if (CategoryMatches(category, "Feature", "Features"))
		{
			return CanSetObjectDictionaryValue(TowerDefenseBattleRegistry.BattleFeatureDictionary, value);
		}
		if (CategoryMatches(category, "Process", "Processes"))
		{
			return CanSetObjectDictionaryValue(TowerDefenseBattleRegistry.BattleProcessDictionary, value);
		}
		if (CategoryMatches(category, "ProjectileChange", "ProjectileChanges"))
		{
			return CanSetObjectDictionaryValue(TowerDefenseProjectileRegistry.ProjectileChangeDataDictionary, value);
		}
		return false;
	}

	private static bool TryRemoveRuntimeValue(string category, string key)
	{
		if (CategoryMatches(category, "Texture", "Textures", "Image", "Images"))
		{
			return RuntimeTextures.Remove(key);
		}
		if (CategoryMatches(category, "AnimationAtlas", "AnimationAtlases", "AtlasProfile", "AtlasProfiles"))
		{
			if (!RuntimeAnimationAtlasProfiles.Remove(key, out var value))
			{
				return false;
			}
			AdobeAnimateDefinitionCache.ReleaseProfile(value);
			return true;
		}
		ResourceManager instance = ResourceManager.Instance;
		if (instance != null)
		{
			if (CategoryMatches(category, "Level", "Levels"))
			{
				return instance.LEVELS.Remove(key);
			}
			if (CategoryMatches(category, "Audio", "Audios"))
			{
				return instance.AUDIOS.Remove(key);
			}
			if (CategoryMatches(category, "BGM", "Bgms"))
			{
				return instance.BGMS.Remove(key);
			}
			if (CategoryMatches(category, "Map", "Maps"))
			{
				return instance.MAPS.Remove(key);
			}
			if (CategoryMatches(category, "Projectile", "Projectiles"))
			{
				bool flag = TowerDefenseProjectileRegistry.RemoveProjectileRegistration(new StringName(key));
				bool num = instance.PROJECTILE_CONFIG.Remove(key);
				if (num && !flag)
				{
					TowerDefenseProjectileRegistry.NotifyProjectileConfigChanged();
				}
				return num || flag;
			}
			if (CategoryMatches(category, "CharacterSprite"))
			{
				return instance.CHARCTAER_SPRITE.Remove(key);
			}
			if (CategoryMatches(category, "Character", "Characters"))
			{
				return instance.TOWERDEFENSE_CHARCATERS.Remove(key);
			}
			if (CategoryMatches(category, "Packet", "Packets", "Card", "Cards"))
			{
				return instance.TOWERDEFENSE_PACKETS.Remove(key);
			}
			if (CategoryMatches(category, "PacketBank", "PacketBanks"))
			{
				return instance.TOWERDEFENSE_PACKETBANKS.Remove(key);
			}
			if (CategoryMatches(category, "Tutorial", "Tutorials"))
			{
				return instance.TUTORIALS.Remove(key);
			}
			if (CategoryMatches(category, "NpcTalk", "NpcTalks", "Talk", "Talks"))
			{
				return instance.TALKS.Remove(key);
			}
			if (CategoryMatches(category, "Collectable", "Collectables"))
			{
				return instance.COLLECTABLES.Remove(key);
			}
			if (CategoryMatches(category, "Shovel", "Shovels"))
			{
				return instance.SHOVELS.Remove(key);
			}
			if (CategoryMatches(category, "Mower", "Mowers"))
			{
				return instance.MOWERS.Remove(key);
			}
			if (CategoryMatches(category, "Shop", "Shops"))
			{
				return instance.SHOPS.Remove(key);
			}
			if (CategoryMatches(category, "Survival", "Survivals"))
			{
				return instance.SURVIVALS.Remove(key);
			}
		}
		if (CategoryMatches(category, "Feature", "Features"))
		{
			return TowerDefenseBattleRegistry.RemoveFeatureRegistration(key);
		}
		if (CategoryMatches(category, "Process", "Processes"))
		{
			return TowerDefenseBattleRegistry.RemoveProcessRegistration(key);
		}
		if (CategoryMatches(category, "ProjectileChange", "ProjectileChanges"))
		{
			return TowerDefenseProjectileRegistry.ProjectileChangeDataDictionary.Remove(new StringName(key));
		}
		return false;
	}

	private static bool TryGetObjectDictionaryValue<T>(System.Collections.Generic.Dictionary<string, T> dict, string key, out Variant value) where T : GodotObject
	{
		if (dict != null && dict.TryGetValue(key, out var value2))
		{
			value = Variant.From(in value2);
			return true;
		}
		value = default(Variant);
		return false;
	}

	private static bool TrySetObjectDictionaryValue<T>(System.Collections.Generic.Dictionary<string, T> dict, string key, Variant value, bool allowOverride) where T : GodotObject
	{
		if (dict == null)
		{
			return false;
		}
		if (!allowOverride && dict.ContainsKey(key))
		{
			return false;
		}
		if (!(value.AsGodotObject() is T value2))
		{
			return false;
		}
		dict[key] = value2;
		return true;
	}

	private static bool CanSetObjectDictionaryValue<TKey, T>(System.Collections.Generic.Dictionary<TKey, T> dict, Variant value) where TKey : notnull where T : GodotObject
	{
		if (dict != null && value.VariantType == Variant.Type.Object)
		{
			return value.AsGodotObject() is T;
		}
		return false;
	}

	private static bool TryGetStringNameObjectDictionaryValue<T>(System.Collections.Generic.Dictionary<StringName, T> dict, string key, out Variant value) where T : GodotObject
	{
		if (dict != null && dict.TryGetValue(new StringName(key), out var value2))
		{
			value = Variant.From(in value2);
			return true;
		}
		value = default(Variant);
		return false;
	}

	private static bool TrySetStringNameObjectDictionaryValue<T>(System.Collections.Generic.Dictionary<StringName, T> dict, string key, Variant value, bool allowOverride) where T : GodotObject
	{
		if (dict == null)
		{
			return false;
		}
		StringName key2 = new StringName(key);
		if (!allowOverride && dict.ContainsKey(key2))
		{
			return false;
		}
		if (!(value.AsGodotObject() is T value2))
		{
			return false;
		}
		dict[key2] = value2;
		return true;
	}

	private static bool TryGetGodotDictionaryValue(System.Collections.Generic.Dictionary<string, Dictionary> dict, string key, out Variant value)
	{
		if (dict != null && dict.TryGetValue(key, out var value2))
		{
			value = Variant.From(in value2);
			return true;
		}
		value = default(Variant);
		return false;
	}

	private static bool TrySetGodotDictionaryValue(System.Collections.Generic.Dictionary<string, Dictionary> dict, string key, Variant value, bool allowOverride)
	{
		if (dict == null)
		{
			return false;
		}
		if (!allowOverride && dict.ContainsKey(key))
		{
			return false;
		}
		if (value.VariantType != Variant.Type.Dictionary)
		{
			return false;
		}
		dict[key] = value.AsGodotDictionary();
		return true;
	}

	private static bool CategoryMatches(string category, params string[] aliases)
	{
		foreach (string b in aliases)
		{
			if (string.Equals(category, b, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static string NormalizeCategory(string category)
	{
		if (CategoryMatches(category, "Level", "Levels"))
		{
			return "Level";
		}
		if (CategoryMatches(category, "Texture", "Textures", "Image", "Images"))
		{
			return "Texture";
		}
		if (CategoryMatches(category, "Audio", "Audios"))
		{
			return "Audio";
		}
		if (CategoryMatches(category, "AnimationAtlas", "AnimationAtlases", "AtlasProfile", "AtlasProfiles"))
		{
			return "AnimationAtlas";
		}
		if (CategoryMatches(category, "BGM", "Bgms"))
		{
			return "BGM";
		}
		if (CategoryMatches(category, "Map", "Maps"))
		{
			return "Map";
		}
		if (CategoryMatches(category, "Projectile", "Projectiles"))
		{
			return "Projectile";
		}
		if (CategoryMatches(category, "CharacterSprite"))
		{
			return "CharacterSprite";
		}
		if (CategoryMatches(category, "Character", "Characters"))
		{
			return "Character";
		}
		if (CategoryMatches(category, "Packet", "Packets", "Card", "Cards"))
		{
			return "Packet";
		}
		if (CategoryMatches(category, "PacketBank", "PacketBanks"))
		{
			return "PacketBank";
		}
		if (CategoryMatches(category, "Tutorial", "Tutorials"))
		{
			return "Tutorial";
		}
		if (CategoryMatches(category, "NpcTalk", "NpcTalks", "Talk", "Talks"))
		{
			return "NpcTalk";
		}
		if (CategoryMatches(category, "Collectable", "Collectables"))
		{
			return "Collectable";
		}
		if (CategoryMatches(category, "Shovel", "Shovels"))
		{
			return "Shovel";
		}
		if (CategoryMatches(category, "Mower", "Mowers"))
		{
			return "Mower";
		}
		if (CategoryMatches(category, "Shop", "Shops"))
		{
			return "Shop";
		}
		if (CategoryMatches(category, "Survival", "Survivals"))
		{
			return "Survival";
		}
		if (CategoryMatches(category, "Feature", "Features"))
		{
			return "Feature";
		}
		if (CategoryMatches(category, "Process", "Processes"))
		{
			return "Process";
		}
		if (CategoryMatches(category, "ProjectileChange", "ProjectileChanges"))
		{
			return "ProjectileChange";
		}
		return category;
	}
}
