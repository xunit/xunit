using Xunit.v3;

internal class SpyTestProcessBase : ITestProcessBase
{
	public SpyTestProcessBase() =>
		OnWaitForExit = _ => HasExited;

	public bool HasExited { get; set; }

	public Action<bool>? OnCancel { get; set; }

	public Action? OnDispose { get; set; }

	public Func<int, bool> OnWaitForExit { get; set; }

	public void Cancel(bool forceCancellation) =>
		OnCancel?.Invoke(forceCancellation);

	public void Dispose() =>
		OnDispose?.Invoke();

	public bool WaitForExit(int milliseconds) =>
		OnWaitForExit(milliseconds);
}
