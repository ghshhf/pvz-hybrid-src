using System;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModDataKey<T>
{
	internal XWModDataService Service;

	internal XWModCodec<T> Codec;

	internal Func<int, ReadOnlyMemory<byte>, byte[]> Migrate;

	public string Name { get; internal init; }

	public int Version { get; internal init; }
}
