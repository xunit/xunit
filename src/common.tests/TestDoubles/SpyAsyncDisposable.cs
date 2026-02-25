internal class SpyAsyncDisposable : IAsyncDisposable
{
	public int CtorCalled;
	public int DisposeAsyncCalled;

	public SpyAsyncDisposable() =>
		CtorCalled++;

	public ValueTask DisposeAsync()
	{
		DisposeAsyncCalled++;
		return default;
	}
}
