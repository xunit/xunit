namespace System.Threading;

internal static class ManualResetEventExtensions
{
	public static void SafeDispose(this ManualResetEvent @event)
	{
		try
		{
			@event.Dispose();
		}
		catch (ObjectDisposedException) { }
	}

	public static void SafeSet(this ManualResetEvent @event)
	{
		try
		{
			@event.Set();
		}
		catch (ObjectDisposedException) { }
	}
}
