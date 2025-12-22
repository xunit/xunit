#if NETCOREAPP
using System;
#endif

#if !NET8_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

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

	public static string ReplaceOrdinal(this string str, string oldValue, string? newValue) =>
#if NETCOREAPP
		str.Replace(oldValue, newValue, StringComparison.Ordinal);
#else
		str.Replace(oldValue, newValue);
#endif


#if !NETCOREAPP2_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
	public static bool StartsWith(this string str, char value) =>
		str.Length != 0 && str[0] == value;
#endif
}
