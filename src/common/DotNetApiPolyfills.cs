#pragma warning disable IDE0060 // API signatures must be respected here, since they're polyfills

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
static class DotNetApiPolyfills
{
#if !NET8_0_OR_GREATER
	public static Task CancelAsync(this CancellationTokenSource cancellationTokenSource)
	{
		cancellationTokenSource.Cancel();
		return Task.CompletedTask;
	}
#endif

	public static int GetHashCodeOrdinal(this string value) =>
#if NETCOREAPP
		value.GetHashCode(StringComparison.Ordinal);
#else
		value.GetHashCode();
#endif

	public static int IndexOfOrdinal(
		this string str,
		char value) =>
#if NETCOREAPP
			str.IndexOf(value, StringComparison.Ordinal);
#else
			str.IndexOf(value);
#endif

	public static string ReplaceOrdinal(
		this string str,
		string oldValue,
		string? newValue) =>
#if NETCOREAPP
			str.Replace(oldValue, newValue, StringComparison.Ordinal);
#else
			str.Replace(oldValue, newValue);
#endif

#if !NETCOREAPP2_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
	public static bool StartsWith(
		this string str,
		char value) =>
			str.Length != 0 && str[0] == value;
#endif

#if !NET8_0_OR_GREATER
	extension(Enum)
	{
		public static TEnum Parse<TEnum>(string value)
			where TEnum : struct =>
				(TEnum)Enum.Parse(typeof(TEnum), value);
	}

	extension(ObjectDisposedException)
	{
		public static void ThrowIf(
			[DoesNotReturnIf(true)] bool condition,
			object instance)
		{
			if (condition)
				throw new ObjectDisposedException(instance?.GetType().FullName);
		}
	}
#endif
}
