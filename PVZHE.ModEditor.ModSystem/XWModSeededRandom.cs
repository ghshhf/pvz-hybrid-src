using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModSeededRandom
{
	private readonly string _owner;

	internal XWModSeededRandom(string owner)
	{
		_owner = XWModSelectionState.Owner(owner);
	}

	public double Unit(ulong seed, string stream, ulong sampleIndex)
	{
		return (double)(Sample(seed, stream, sampleIndex, 0u) >> 11) * 1.1102230246251565E-16;
	}

	public int Integer(ulong seed, string stream, ulong sampleIndex, int minimum, int maximumExclusive)
	{
		if (minimum >= maximumExclusive)
		{
			throw new ArgumentOutOfRangeException("maximumExclusive");
		}
		ulong num = (ulong)maximumExclusive - (ulong)minimum;
		ulong num2 = (0 - num) % num;
		uint num3 = 0u;
		while (true)
		{
			ulong num4 = Sample(seed, stream, sampleIndex, num3);
			if (num4 >= num2)
			{
				return (int)((long)(num4 % num) + (long)minimum);
			}
			if (num3 == 4294967295u)
			{
				break;
			}
			num3++;
		}
		throw new InvalidOperationException("Random rejection budget exhausted.");
	}

	private ulong Sample(ulong seed, string stream, ulong sampleIndex, uint attempt)
	{
		if (string.IsNullOrWhiteSpace(stream) || stream.Length > 256 || stream.Contains('\0'))
		{
			throw new ArgumentException("A stable stream name of at most 256 characters is required.", "stream");
		}
		string s = $"mod-random-v1\0{_owner}\0{seed:x16}\0{stream}\0{sampleIndex:x16}\0{attempt:x8}";
		return BinaryPrimitives.ReadUInt64LittleEndian(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
	}
}
