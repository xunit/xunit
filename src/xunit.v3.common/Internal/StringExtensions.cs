namespace Xunit.Internal
{
	static class StringExtensions
	{
		public static string Quoted(this string? value) =>
			value == null ? "null" : "\"" + value + "\"";
	}
}
