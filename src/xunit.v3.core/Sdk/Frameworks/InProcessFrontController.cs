using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// This class is a "philosophical" implementation of <see cref="T:Xunit.IFrontController"/> (which isn't a type
/// that's available here), intended to be used by in-process runners, wrapped around an implementation
/// of <see cref="_ITestFramework"/>. The signatures of the methods are slightly different, as they permit
/// and require direct access to <see cref="_ITestCase"/> instances rather than forcing the test cases through
/// a round of serialization and deserialization. It will also manufacture the <see cref="_DiscoveryStarting"/>
/// and <see cref="_DiscoveryComplete"/> messages that the test framework is not responsible for. When connected
/// to remote meta-runners, the in-process runner is can convert <see cref="_ITestCase"/> instances into
/// <see cref="_TestCaseDiscovered"/> by using <see cref="TestCaseExtensions.ToTestCaseDiscovered"/> (which should
/// be called from a callback passed to <see cref="Find"/>).
/// </summary>
public class InProcessFrontController
{
	readonly string? configFilePath;
	readonly Lazy<_ITestFrameworkDiscoverer> discoverer;
	readonly Lazy<_ITestFrameworkExecutor> executor;
	readonly _IReflectionAssemblyInfo testAssembly;
	readonly string testAssemblyUniqueID;

	/// <summary>
	/// Initializes a new instance of the <see cref="InProcessFrontController"/> class.
	/// </summary>
	/// <param name="testFramework">The test framework to be wrapped.</param>
	/// <param name="testAssembly">The assembly under test.</param>
	/// <param name="configFilePath">The optional configuration file path.</param>
	public InProcessFrontController(
		_ITestFramework testFramework,
		_IReflectionAssemblyInfo testAssembly,
		string? configFilePath)
	{
		Guard.ArgumentNotNull(testFramework);

		this.testAssembly = Guard.ArgumentNotNull(testAssembly);
		this.configFilePath = configFilePath;

		testAssemblyUniqueID = UniqueIDGenerator.ForAssembly(testAssembly.Name, testAssembly.AssemblyPath, configFilePath);

		discoverer = new(() => testFramework.GetDiscoverer(testAssembly));
		executor = new(() => testFramework.GetExecutor(testAssembly));
	}

	/// <summary>
	/// Gets the target framework that the test assembly is linked against.
	/// </summary>
	public string TargetFramework =>
		discoverer.Value.TargetFramework;

	/// <summary>
	/// Gets the unique ID for the test assembly provided to the discoverer.
	/// </summary>
	public string TestAssemblyUniqueID =>
		testAssemblyUniqueID;

	/// <summary>
	/// Returns the display name of the test framework that this discoverer is running tests for.
	/// </summary>
	public string TestFrameworkDisplayName =>
		discoverer.Value.TestFrameworkDisplayName;

	/// <summary>
	/// Starts the process of finding tests in an assembly. Typically only used by
	/// runners which discover tests and present them into a UI for the user to interactively
	/// choose for selective run (via <see cref="Run"/>). For runners which simply wish to
	/// discover and immediately run tests, they should instead use <see cref="FindAndRun"/>,
	/// which permits the same filtering logic as this method.
	/// </summary>
	/// <param name="messageSink">The message sink to report messages to.</param>
	/// <param name="options">The options to be used for discovery.</param>
	/// <param name="filter">The filter function for filtering test cases.</param>
	/// <param name="types">When passed a non-<c>null</c> collection, only returns tests found
	/// from one of the provided types; when passed a <c>null</c> collection, discovers all
	/// tests in the assembly.</param>
	/// <param name="discoveryCallback">An optional callback to be called for each discovered test case.
	/// It provides both the test case and a flag which indicates if it passed the provided filter.</param>
	public async ValueTask Find(
		_IMessageSink messageSink,
		_ITestFrameworkDiscoveryOptions options,
		Func<_ITestCaseMetadata, bool> filter,
		Type[]? types = null,
		Func<_ITestCase, bool, ValueTask<bool>>? discoveryCallback = null)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(options);
		Guard.ArgumentNotNull(filter);

		int testCasesToRun = 0;

		messageSink.OnMessage(new _DiscoveryStarting
		{
			AssemblyName = testAssembly.Name,
			AssemblyPath = testAssembly.AssemblyPath,
			AssemblyUniqueID = testAssemblyUniqueID,
			ConfigFilePath = configFilePath,
		});

		try
		{
			using var cts = new CancellationTokenSource();

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
							cts.Cancel();
					}

					return result;
				},
				options,
				types,
				cancellationToken: cts.Token
			);
		}
		finally
		{
			messageSink.OnMessage(new _DiscoveryComplete
			{
				AssemblyUniqueID = testAssemblyUniqueID,
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
	/// <param name="types">When passed a non-<c>null</c> collection, discovery/filtering/execution
	/// only looks for tests from one of the provided types; when passed a <c>null</c> collection,
	/// discovery/filtering/execution looks at all types in the assembly.</param>
	public async ValueTask FindAndRun(
		_IMessageSink messageSink,
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestFrameworkExecutionOptions executionOptions,
		Func<_ITestCaseMetadata, bool> filter,
		Type[]? types = null)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(executionOptions);
		Guard.ArgumentNotNull(filter);

		List<_ITestCase> testCasesToRun = new();

		await Find(
			messageSink,
			discoveryOptions,
			filter,
			types,
			(testCase, passedFilter) =>
			{
				if (passedFilter)
					testCasesToRun.Add(testCase);

				return new(true);
			}
		);

		await Run(messageSink, executionOptions, testCasesToRun);
	}

	/// <summary>
	/// Starts the process of running selected tests in the assembly. The test cases to run come from
	/// calling <see cref="Find"/> and collecting the test cases that were returned via the callback.
	/// </summary>
	public ValueTask Run(
		_IMessageSink messageSink,
		_ITestFrameworkExecutionOptions executionOptions,
		IReadOnlyCollection<_ITestCase> testCases)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(executionOptions);
		Guard.ArgumentNotNull(testCases);

		return executor.Value.RunTestCases(testCases, messageSink, executionOptions);
	}
}
