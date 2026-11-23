using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ILSpy 把 C# 12 集合表达式还原成了编译器内部辅助方法，但没有生成承载类型。
// 这里补上等价实现：InlineArray 布局下，缓冲区首元素地址即 TElement 的地址。
internal static class _003CPrivateImplementationDetails_003E
{
	public static ref TElement InlineArrayElementRef<TBuffer, TElement>(ref TBuffer buffer, int index) where TBuffer : struct
	{
		return ref Unsafe.Add(ref Unsafe.As<TBuffer, TElement>(ref buffer), index);
	}

	public static ReadOnlySpan<TElement> InlineArrayAsReadOnlySpan<TBuffer, TElement>(in TBuffer buffer, int length) where TBuffer : struct
	{
		return MemoryMarshal.CreateReadOnlySpan(
			ref Unsafe.As<TBuffer, TElement>(ref Unsafe.AsRef(in buffer)), length);
	}
}
