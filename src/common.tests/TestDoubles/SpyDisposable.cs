#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

internal class SpyDisposable : IDisposable
{
	public int CtorCalled;
	public int DisposeCalled;
	public Exception? DisposeException;

	public SpyDisposable() =>
		CtorCalled++;

	public void Dispose()
	{
		DisposeCalled++;

		if (DisposeException is not null)
			throw DisposeException;
	}
}
