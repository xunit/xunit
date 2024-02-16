#if XUNIT_SPAN

using System;

internal static class MemoryExtensions
{
	public static Memory<T> Memoryify<T>(this T[]? values)
	{
		return new Memory<T>(values);
	}

	public static Memory<char> Memoryify(this string? value)
	{
		return new Memory<char>((value ?? string.Empty).ToCharArray());
	}
}

#endif
