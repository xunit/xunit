using System;

internal static class MemoryExtensions
{
	public static Memory<T> Memoryify<T>(this T[]? values) =>
		new(values);

	public static Memory<char> Memoryify(this string? value) =>
		new((value ?? string.Empty).ToCharArray());
}
