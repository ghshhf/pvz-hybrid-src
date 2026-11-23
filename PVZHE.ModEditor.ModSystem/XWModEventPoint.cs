namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModEventPoint<T>
{
	public string Id => Descriptor.Id;

	public XWModExtensionDescriptor Descriptor { get; }

	internal XWModEventPoint(XWModExtensionDescriptor descriptor)
	{
		Descriptor = descriptor;
	}
}
