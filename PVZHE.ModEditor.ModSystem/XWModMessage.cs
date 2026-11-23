namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModMessage<T>
{
	internal XWModNetworkService Service;

	internal XWModCodec<T> Codec;

	public string Name { get; internal init; }

	public int Version { get; internal init; }

	public XWModMessageKind Kind { get; internal init; }
}
