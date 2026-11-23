using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModModifierPoint<T>
{
	public string Id => Descriptor.Id;

	public XWModExtensionDescriptor Descriptor { get; }

	internal Func<T, T, bool> Validate { get; }

	internal Func<T, T, bool> ValidateTransition { get; }

	internal Func<T, T, T> NormalizeCandidate { get; set; }

	internal XWModModifierPoint(XWModExtensionDescriptor descriptor, Func<T, T, bool> validate, Func<T, T, bool> validateTransition = null)
	{
		Descriptor = descriptor;
		Validate = validate;
		ValidateTransition = validateTransition;
	}
}
