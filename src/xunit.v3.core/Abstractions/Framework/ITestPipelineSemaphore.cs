namespace Xunit.Sdk;

/// <summary>
/// Semaphore used to limit the number of concurrent running tests and excessive task creation in test assemblies with
/// a high degree of parallelism enabled.
/// </summary>
public interface ITestPipelineSemaphore : IDisposable
{
	/// <summary>
	/// Gets the current count of the <see cref="ITestPipelineSemaphore"/>.
	/// </summary>
	/// <value>The current count of the <see cref="ITestPipelineSemaphore"/>.</value>
	int CurrentCount { get; }

	/// <summary>
	/// Exits the <see cref="ITestPipelineSemaphore"/> once.
	/// </summary>
	/// <returns>The previous count of the <see cref="ITestPipelineSemaphore"/>.</returns>
	/// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
	int Release();

	/// <summary>
	/// Asynchronously waits to enter the <see cref="ITestPipelineSemaphore"/>, while observing a
	/// <see cref="CancellationToken"/>.
	/// </summary>
	/// <returns>
	/// A task that will complete when the semaphore has been entered.
	/// </returns>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> token to observe.</param>
	/// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
	Task WaitAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Asynchronously waits to enter the <see cref="ITestPipelineSemaphore"/>, while observing a
	/// <see cref="CancellationToken"/>.
	/// </summary>
	/// <returns>
	/// A task that will complete when the semaphore has been entered and return a value which releases the semaphore
	/// when disposed.
	/// </returns>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> token to observe.</param>
	/// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
	Task<IDisposable> LockAsync(CancellationToken cancellationToken);
}
