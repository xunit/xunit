using System.Threading.Tasks;

namespace System;

internal static class DisposableExtensions
{
	public static void SafeDispose(this IDisposable? disposable)
	{
		try
		{
			disposable?.Dispose();
		}
		catch { }
	}

	public static async ValueTask SafeDisposeAsync(this IAsyncDisposable? asyncDisposable)
	{
		if (asyncDisposable is not null)
			try
			{
				await asyncDisposable.DisposeAsync();
			}
			catch { }
	}
}
