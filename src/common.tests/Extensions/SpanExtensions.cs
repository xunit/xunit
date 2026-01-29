internal static class SpanExtensions
{
	public static Span<T> Spanify<T>(this T[]? values) =>
		new(values);

	public static Span<char> Spanify(this string? value) =>
		new((value ?? string.Empty).ToCharArray());
}
