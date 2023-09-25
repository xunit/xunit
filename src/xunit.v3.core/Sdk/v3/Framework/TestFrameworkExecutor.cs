using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A reusable implementation of <see cref="_ITestFrameworkExecutor"/> which contains the basic behavior
/// for running tests.
/// </summary>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="_ITestCase"/>.</typeparam>
public abstract class TestFrameworkExecutor<TTestCase> : _ITestFrameworkExecutor, IAsyncDisposable
	where TTestCase : _ITestCase
{
	_IReflectionAssemblyInfo assemblyInfo;
	bool disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestFrameworkExecutor{TTestCase}"/> class.
	/// </summary>
	/// <param name="assemblyInfo">The test assembly.</param>
	protected TestFrameworkExecutor(_IReflectionAssemblyInfo assemblyInfo)
	{
		this.assemblyInfo = Guard.ArgumentNotNull(assemblyInfo);
	}

	/// <summary>
	/// Gets the assembly information of the assembly under test.
	/// </summary>
	protected _IReflectionAssemblyInfo AssemblyInfo
	{
		get => assemblyInfo;
		set => assemblyInfo = Guard.ArgumentNotNull(value, nameof(AssemblyInfo));
	}

	/// <summary>
	/// Gets the disposal tracker for the test framework discoverer.
	/// </summary>
	protected DisposalTracker DisposalTracker { get; } = new();

	/// <summary>
	/// Override to create a test framework discoverer that can be used to discover
	/// tests when the user asks to run all test.
	/// </summary>
	/// <returns>The test framework discoverer</returns>
	protected abstract _ITestFrameworkDiscoverer CreateDiscoverer();

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		if (disposed)
			return default;

		disposed = true;

		GC.SuppressFinalize(this);

		return DisposalTracker.DisposeAsync();
	}

	/// <inheritdoc/>
	public abstract ValueTask RunTestCases(
		IReadOnlyCollection<TTestCase> testCases,
		_IMessageSink executionMessageSink,
		_ITestFrameworkExecutionOptions executionOptions
	);

	ValueTask _ITestFrameworkExecutor.RunTestCases(
		IReadOnlyCollection<_ITestCase> testCases,
		_IMessageSink executionMessageSink,
		_ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(executionMessageSink);
		Guard.ArgumentNotNull(executionOptions);

		var seed = executionOptions.Seed() ?? AssemblyInfo.Assembly.Modules.First().ModuleVersionId.GetHashCode();
		Randomizer.Seed = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);

		var tcs = new TaskCompletionSource<object?>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				using (new PreserveWorkingFolder(AssemblyInfo))
				using (new CultureOverride(executionOptions.Culture()))
					await RunTestCases(testCases.Cast<TTestCase>().CastOrToReadOnlyCollection(), executionMessageSink, executionOptions);

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
