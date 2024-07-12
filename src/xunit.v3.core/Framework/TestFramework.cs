using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A default implementation of <see cref="ITestFramework"/> that tracks objects to be
/// disposed when the framework is disposed. The discoverer and executor are automatically
/// tracked for disposal, since those interfaces mandate an implementation
/// of <see cref="IDisposable"/>.
/// </summary>
public abstract class TestFramework : ITestFramework, IAsyncDisposable
{
	bool disposed;

	/// <summary>
	/// Gets the disposal tracker for the test framework.
	/// </summary>
	protected DisposalTracker DisposalTracker { get; } = new();

	/// <inheritdoc/>
	public abstract string TestFrameworkDisplayName { get; }

	/// <summary>
	/// Gets the value that was set for the test pipeline startup, if one was present.
	/// </summary>
	protected ITestPipelineStartup? TestPipelineStartup { get; private set; }

	/// <inheritdoc/>
	public virtual async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		await DisposalTracker.DisposeAsync();
	}

	/// <summary>
	/// Override this method to provide the implementation of <see cref="ITestFrameworkDiscoverer"/>.
	/// </summary>
	/// <param name="assembly">The assembly that is being discovered.</param>
	/// <returns>Returns the test framework discoverer.</returns>
	protected abstract ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly);

	/// <summary>
	/// Override this method to provide the implementation of <see cref="ITestFrameworkExecutor"/>.
	/// </summary>
	/// <param name="assembly">The assembly that is being executed.</param>
	/// <returns>Returns the test framework executor.</returns>
	protected abstract ITestFrameworkExecutor CreateExecutor(Assembly assembly);

	/// <inheritdoc/>
	public ITestFrameworkDiscoverer GetDiscoverer(Assembly assembly)
	{
		Guard.ArgumentNotNull(assembly);

		var discoverer = CreateDiscoverer(assembly);
		DisposalTracker.Add(discoverer);
		return discoverer;
	}

	/// <inheritdoc/>
	public ITestFrameworkExecutor GetExecutor(Assembly assembly)
	{
		Guard.ArgumentNotNull(assembly);

		var executor = CreateExecutor(assembly);
		DisposalTracker.Add(executor);
		return executor;
	}

	/// <inheritdoc/>
	public void SetTestPipelineStartup(ITestPipelineStartup pipelineStartup) =>
		TestPipelineStartup = Guard.ArgumentNotNull(pipelineStartup);
}
