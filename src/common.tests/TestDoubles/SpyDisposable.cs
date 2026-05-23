#pragma warning disable CS0649 // This is a shared file, and DisposeException is only assigned in some test projects

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
