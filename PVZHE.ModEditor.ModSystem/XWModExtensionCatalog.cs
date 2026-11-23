using System;
using System.Collections.Generic;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

public static class XWModExtensionCatalog
{
	private static readonly Dictionary<string, XWModExtensionDescriptor> Points = new Dictionary<string, XWModExtensionDescriptor>(StringComparer.Ordinal);

	private static readonly HashSet<string> Legacy = new HashSet<string>(StringComparer.Ordinal) { "ui.mount.v1", "damage.calculate.v1", "plant.placement.v1", "plant.cost.v1" };

	public static IReadOnlyList<XWModExtensionDescriptor> Installed => Points.Values.OrderBy<XWModExtensionDescriptor, string>((XWModExtensionDescriptor x) => x.Id, StringComparer.Ordinal).ToArray();

	public static IReadOnlyList<string> InstalledLegacy => Legacy.OrderBy<string, string>((string x) => x, StringComparer.Ordinal).ToArray();

	internal static void RegisterLegacyPoint(string id)
	{
		if (!XWModHookPoints.AllIds.Contains(id))
		{
			throw new ArgumentException("Unknown legacy point.", "id");
		}
		Legacy.Add(id);
	}

	internal static XWModEventPoint<T> Notification<T>(string id, XWModAuthority authority, bool localPresentation = false)
	{
		return new XWModEventPoint<T>(Register(new XWModExtensionDescriptor(id, authority, Modifiable: false, localPresentation, typeof(T))));
	}

	internal static XWModModifierPoint<T> Modifier<T>(string id, XWModAuthority authority, Func<T, bool> validator)
	{
		if (validator == null)
		{
			throw new ArgumentNullException("validator");
		}
		return Modifier(id, authority, (T _, T candidate) => validator(candidate));
	}

	internal static XWModModifierPoint<T> Modifier<T>(string id, XWModAuthority authority, Func<T, T, bool> validator, Func<T, T, bool> validateTransition = null, Func<T, T, T> normalizeCandidate = null)
	{
		return new XWModModifierPoint<T>(Register(new XWModExtensionDescriptor(id, authority, Modifiable: true, LocalPresentation: false, typeof(T))), validator ?? throw new ArgumentNullException("validator"), validateTransition)
		{
			NormalizeCandidate = normalizeCandidate
		};
	}

	private static XWModExtensionDescriptor Register(XWModExtensionDescriptor descriptor)
	{
		if (string.IsNullOrWhiteSpace(descriptor.Id))
		{
			throw new ArgumentException("Missing extension ID.");
		}
		if (descriptor.LocalPresentation && descriptor.Authority != XWModAuthority.Local)
		{
			throw new ArgumentException("Presentation points must be local.");
		}
		if (Points.TryGetValue(descriptor.Id, out var value))
		{
			if (value != descriptor)
			{
				throw new InvalidOperationException("Conflicting extension contract: " + descriptor.Id);
			}
			return value;
		}
		Points.Add(descriptor.Id, descriptor);
		return descriptor;
	}
}
