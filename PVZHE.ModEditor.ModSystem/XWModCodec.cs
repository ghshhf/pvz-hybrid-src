using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModCodec<T>
{
	public Func<T, byte[]> Encode { get; }

	public Func<ReadOnlyMemory<byte>, T> Decode { get; }

	public Func<T, bool> Validate { get; }

	public XWModCodec(Func<T, byte[]> encode, Func<ReadOnlyMemory<byte>, T> decode, Func<T, bool> validate)
	{
		Encode = encode ?? throw new ArgumentNullException("encode");
		Decode = decode ?? throw new ArgumentNullException("decode");
		Validate = validate ?? throw new ArgumentNullException("validate");
	}

	public static XWModCodec<T> Json(JsonTypeInfo<T> metadata, Func<T, bool> validate)
	{
		return new XWModCodec<T>((T value) => JsonSerializer.SerializeToUtf8Bytes(value, metadata), (ReadOnlyMemory<byte> bytes) => JsonSerializer.Deserialize(bytes.Span, metadata), validate);
	}
}
