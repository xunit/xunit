using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This class is a "philosophical" implementation of <see cref="T:Xunit.IFrontController"/> (which isn't a type
/// that's available here), intended to be used by in-process runners, wrapped around an implementation
/// of <see cref="ITestFramework"/>. The signatures of the methods are slightly different, as they permit
/// and require direct access to <see cref="ITestCase"/> instances rather than forcing the test cases through
/// a round of serialization and deserialization. It will also manufacture the <see cref="IDiscoveryStarting"/>
/// and <see cref="IDiscoveryComplete"/> messages that the test framework is not responsible for. When connected
/// to remote meta-runners, the in-process runner can convert <see cref="ITestCase"/> instances into
/// <see cref="T:Xunit.Runner.Common.TestCaseDiscovered"/> instances by using a converter like
/// <see cref="M:Xunit.Runner.Common.TestCaseExtensions.ToTestCaseDiscovered"/> (which should be called from a
/// callback passed to <see cref="Find"/>).
/// </summary>
public class InProcessFrontController
{
	readonly string? configFilePath;
	readonly Lazy<ITestFrameworkDiscoverer> discoverer;
	readonly Lazy<ITestFrameworkExecutor> executor;
	readonly Assembly testAssembly;

	/// <summary>
	/// Initializes a new instance of the <see cref="InProcessFrontController"/> class.
	/// </summary>
	/// <param name="testFramework">The test framework to be wrapped.</param>
	/// <param name="testAssembly">The assembly under test.</param>
	/// <param name="configFilePath">The optional configuration file path.</param>
	public InProcessFrontController(
		ITestFramework testFramework,
		Assembly testAssembly,
		string? configFilePath)
	{
		Guard.ArgumentNotNull(testFramework);

		this.testAssembly = Guard.ArgumentNotNull(testAssembly);
		this.configFilePath = configFilePath;

		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(testAssembly.Location, configFilePath);

		discoverer = new(() => testFramework.GetDiscoverer(testAssembly));
		executor = new(() => testFramework.GetExecutor(testAssembly));
		TestFrameworkDisplayName = testFramework.TestFrameworkDisplayName;
	}

	/// <summary>
	/// Gets the unique ID for the test assembly provided to the discoverer.
	/// </summary>
	public string TestAssemblyUniqueID { get; }

	/// <summary>
	/// Returns the display name of the test framework that this discoverer is running tests for.
	/// </summary>
	public string TestFrameworkDisplayName { get; }

	/// <summary>
	/// Starts the process of finding tests in an assembly. Typically only used by
	/// runners which discover tests and present them into a UI for the user to interactively
	/// choose for selective run (via <see cref="Run"/>). For runners which simply wish to
	/// discover and immediately run tests, they should instead use <see cref="FindAndRun"/>,
	/// which permits the same filtering logic as this method.
	/// </summary>
	/// <param name="messageSink">The message sink to report discovery starting and discovery complete messages to.
	/// Discovered tests are *not* reported to this message sink, since it's also used for <see cref="FindAndRun"/>,
	/// so it is assumed that callers to this method will use the <paramref name="discoveryCallback"/> to report
	/// discovered tests if needed.</param>
	/// <param name="options">The options to be used for discovery.</param>
	/// <param name="filter">The filter function for filtering test cases.</param>
	/// <param name="cancellationTokenSource">The cancellation token source used to cancel discovery.</param>
	/// <param name="types">When passed a non-<c>null</c> collection, only returns tests found
	/// from one of the provided types; when passed a <c>null</c> collection, discovers all
	/// tests in the assembly.</param>
	/// <param name="discoveryCallback">An optional callback to be called for each discovered test case.
	/// It provides both the test case and a flag which indicates if it passed the provided filter.</param>
	public async ValueTask Find(
		IMessageSink? messageSink,
		ITestFrameworkDiscoveryOptions options,
		Func<ITestCase, bool> filter,
		CancellationTokenSource cancellationTokenSource,
		Type[]? types = null,
		Func<ITestCase, bool, ValueTask<bool>>? discoveryCallback = null)
	{
		Guard.ArgumentNotNull(options);
		Guard.ArgumentNotNull(filter);
		Guard.ArgumentNotNull(cancellationTokenSource);

		var testCasesToRun = 0;

		messageSink?.OnMessage(new DiscoveryStarting
		{
			AssemblyName = Path.GetFileNameWithoutExtension(testAssembly.Location),
			AssemblyPath = testAssembly.Location,
			AssemblyUniqueID = TestAssemblyUniqueID,
			ConfigFilePath = configFilePath,
		});

		try
		{
			await discoverer.Value.Find(
				async testCase =>
				{
					var willRun = filter(testCase);
					if (willRun)
						Interlocked.Increment(ref testCasesToRun);

					var result = true;
					if (discoveryCallback is not null)
					{
						result = await discoveryCallback(testCase, willRun);

						if (!result)
							cancellationTokenSource.Cancel();
					}

					return result;
				},
				options,
				types,
				cancellationTokenSource.Token
			);
		}
		finally
		{
			messageSink?.OnMessage(new DiscoveryComplete
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				TestCasesToRun = testCasesToRun,
			});
		}
	}

	/// <summary>
	/// Starts the process of finding and running tests in an assembly. Typically only used
	/// by runner which do not present test discovery UIs to users that allow them to run
	/// selected tests (those should instead use <see cref="Find"/> and <see cref="Run"/>
	/// as separate operations).
	/// </summary>
	/// <param name="messageSink">The message sink to report messages to.</param>
	/// <param name="discoveryOptions">The options to be used for discovery.</param>
	/// <param name="executionOptions">The options to be used for execution.</param>
	/// <param name="filter">The filter function for filtering test cases.</param>
	/// <param name="cancellationTokenSource">The cancellation token sourced used to cancel discovery/execution.</param>
	/// <param name="types">When passed a non-<c>null</c> collection, discovery/filtering/execution
	/// only looks for tests from one of the provided types; when passed a <c>null</c> collection,
	/// discovery/filtering/execution looks at all types in the assembly.</param>
	public async ValueTask FindAndRun(
		IMessageSink messageSink,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ITestFrameworkExecutionOptions executionOptions,
		Func<ITestCase, bool> filter,
		CancellationTokenSource cancellationTokenSource,
		Type[]? types = null)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(executionOptions);
		Guard.ArgumentNotNull(filter);
		Guard.ArgumentNotNull(cancellationTokenSource);

		List<ITestCase> testCases = [];
		List<ITestCase> testCasesToRun = [];

		await Find(
			messageSink,
			discoveryOptions,
			filter,
			cancellationTokenSource,
			types,
			(testCase, passedFilter) =>
			{
				testCases.Add(testCase);
				if (passedFilter)
					testCasesToRun.Add(testCase);

				return new(true);
			}
		);

		await Run(messageSink, executionOptions, testCasesToRun, cancellationTokenSource);

		foreach (var testCase in testCases)
			if (testCase is IAsyncDisposable asyncDisposable)
				await asyncDisposable.DisposeAsync();
			else if (testCase is IDisposable disposable)
				disposable.Dispose();
	}

	/// <summary>
	/// Starts the process of running selected tests in the assembly. The test cases to run come from
	/// calling <see cref="Find"/> and collecting the test cases that were returned via the callback.
	/// </summary>
	/// <param name="messageSink">The message sink to report messages to.</param>
	/// <param name="executionOptions">The options to be used for execution.</param>
	/// <param name="testCases">The test cases to execute.</param>
	/// <param name="cancellationTokenSource">The cancellation token source used to cancel discovery.</param>
	public ValueTask Run(
		IMessageSink messageSink,
		ITestFrameworkExecutionOptions executionOptions,
		IReadOnlyCollection<ITestCase> testCases,
		CancellationTokenSource cancellationTokenSource)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(executionOptions);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(cancellationTokenSource);

		return executor.Value.RunTestCases(testCases, messageSink, executionOptions, cancellationTokenSource.Token);
	}
}
