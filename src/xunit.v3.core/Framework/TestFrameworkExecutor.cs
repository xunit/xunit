using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A reusable implementation of <see cref="ITestFrameworkExecutor"/> which contains the basic behavior
/// for running tests.
/// </summary>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
/// <param name="testAssembly">The test assembly.</param>
public abstract class TestFrameworkExecutor<TTestCase>(ITestAssembly testAssembly) :
	ITestFrameworkExecutor, IAsyncDisposable
		where TTestCase : ITestCase
{
	bool disposed;

	/// <summary>
	/// Gets the disposal tracker for the test framework discoverer.
	/// </summary>
	protected DisposalTracker DisposalTracker { get; } = new();

	/// <summary>
	/// Gets the test assembly for execution.
	/// </summary>
	protected ITestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

	/// <summary>
	/// Override to create a test framework discoverer that can be used to discover
	/// tests when the user asks to run all test.
	/// </summary>
	/// <returns>The test framework discoverer</returns>
	protected abstract ITestFrameworkDiscoverer CreateDiscoverer();

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		if (disposed)
			return default;

		disposed = true;

		GC.SuppressFinalize(this);

		return DisposalTracker.DisposeAsync();
	}

	/// <summary>
	/// Runs selected test cases in the assembly.
	/// </summary>
	/// <param name="testCases">The test cases to run.</param>
	/// <param name="executionMessageSink">The message sink to report results back to.</param>
	/// <param name="executionOptions">The options to be used during test execution.</param>
	/// <param name="cancellationToken">The cancellation token which can be used to cancel the test
	/// execution process.</param>
	public abstract ValueTask RunTestCases(
		IReadOnlyCollection<TTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken
	);

	ValueTask ITestFrameworkExecutor.RunTestCases(
		IReadOnlyCollection<ITestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken? cancellationToken)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(executionMessageSink);
		Guard.ArgumentNotNull(executionOptions);

		var seed = executionOptions.Seed() ?? TestAssembly.ModuleVersionID.GetHashCode();
		Randomizer.Seed = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);

		var tcs = new TaskCompletionSource<object?>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				using (new PreserveWorkingFolder(TestAssembly))
				using (new CultureOverride(executionOptions.Culture()))
					await RunTestCases(testCases.Cast<TTestCase>().CastOrToReadOnlyCollection(), executionMessageSink, executionOptions, cancellationToken.GetValueOrDefault());

				tcs.SetResult(null);
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		});

		return new(tcs.Task);
	}
}
