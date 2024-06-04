using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestAssemblyRunner"/>.
/// </summary>
public class XunitTestAssemblyRunnerContext : TestAssemblyRunnerContext<IXunitTestCase>
{
	_IAttributeInfo? collectionBehaviorAttribute;
	SemaphoreSlim? parallelSemaphore;
	MaxConcurrencySyncContext? syncContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestAssemblyRunnerContext"/> class.
	/// </summary>
	public XunitTestAssemblyRunnerContext(
		_ITestAssembly testAssembly,
		IReadOnlyCollection<IXunitTestCase> testCases,
		_IMessageSink executionMessageSink,
		_ITestFrameworkExecutionOptions executionOptions) :
			base(testAssembly, testCases, executionMessageSink, executionOptions)
	{ }

	/// <summary>
	/// Gets the mapping manager for assembly-level fixtures.
	/// </summary>
	public FixtureMappingManager AssemblyFixtureMappings { get; } = new("Assembly");

	/// <summary>
	/// Gets the assembly-level test case orderer, if one is present.
	/// </summary>
	public ITestCaseOrderer? AssemblyTestCaseOrderer { get; private set; }

	/// <summary>
	/// Gets the assembly-level test collection orderer, if one is present.
	/// </summary>
	public ITestCollectionOrderer? AssemblyTestCollectionOrderer { get; private set; }

	/// <summary>
	/// Gets a flag which indicates whether the user has requested that parallelization be disabled.
	/// </summary>
	public bool DisableParallelization { get; private set; }

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If this returns a
	/// positive integer, that is the maximum number of threads; if it returns -1, that indicates that
	/// unlimited threads should be allowed.
	/// </summary>
	public int MaxParallelThreads { get; private set; }

	/// <summary>
	/// Gets the algorithm used for parallelism.
	/// </summary>
	public ParallelAlgorithm ParallelAlgorithm { get; private set; }

	/// <inheritdoc/>
	public override string TestFrameworkDisplayName =>
		XunitTestFramework.DisplayName;

	/// <inheritdoc/>
	public override string TestFrameworkEnvironment
	{
		get
		{
			var testCollectionFactory =
				ExtensibilityPointFactory.GetXunitTestCollectionFactory(collectionBehaviorAttribute, TestAssembly)
				?? new CollectionPerClassTestCollectionFactory(TestAssembly);

			var threadCountText = MaxParallelThreads < 0 ? "unlimited" : MaxParallelThreads.ToString(CultureInfo.CurrentCulture);
			threadCountText += " thread";
			if (MaxParallelThreads != 1)
				threadCountText += 's';
			if (MaxParallelThreads > 0 && ParallelAlgorithm == ParallelAlgorithm.Aggressive)
				threadCountText += "/aggressive";

			return string.Format(
				CultureInfo.CurrentCulture,
				"{0} [{1}, {2}]",
				base.TestFrameworkEnvironment,
				testCollectionFactory.DisplayName,
				DisableParallelization
					? "non-parallel"
					: string.Format(CultureInfo.CurrentCulture, "parallel ({0})", threadCountText)
			);
		}
	}

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (syncContext is IAsyncDisposable asyncDisposable)
			await asyncDisposable.DisposeAsync();
		else if (syncContext is IDisposable disposable)
			disposable.Dispose();

		parallelSemaphore?.Dispose();

		await base.DisposeAsync();
	}

	/// <inheritdoc/>
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();

		collectionBehaviorAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
		if (collectionBehaviorAttribute is not null)
		{
			DisableParallelization = collectionBehaviorAttribute.GetNamedArgument<bool>(nameof(CollectionBehaviorAttribute.DisableTestParallelization));
			MaxParallelThreads = collectionBehaviorAttribute.GetNamedArgument<int>(nameof(CollectionBehaviorAttribute.MaxParallelThreads));
		}

		ParallelAlgorithm = ExecutionOptions.ParallelAlgorithm() ?? ParallelAlgorithm;
		DisableParallelization = ExecutionOptions.DisableParallelization() ?? DisableParallelization;
		MaxParallelThreads = ExecutionOptions.MaxParallelThreads() ?? MaxParallelThreads;
		if (MaxParallelThreads == 0)
			MaxParallelThreads = Environment.ProcessorCount;

		var testCaseOrdererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
		if (testCaseOrdererAttribute is not null)
		{
			try
			{
				AssemblyTestCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(testCaseOrdererAttribute);
				if (AssemblyTestCaseOrderer is null)
				{
					var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCaseOrdererAttribute);

					TestContext.Current?.SendDiagnosticMessage("Could not find type '{0}' in {1} for assembly-level test case orderer", type, assembly);
				}
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();
				var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCaseOrdererAttribute);

				TestContext.Current?.SendDiagnosticMessage(
					"Assembly-level test case orderer '{0}' threw '{1}' during construction: {2}{3}{4}",
					type,
					innerEx.GetType().FullName,
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}
		}

		var testCollectionOrdererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCollectionOrdererAttribute)).SingleOrDefault();
		if (testCollectionOrdererAttribute is not null)
		{
			try
			{
				AssemblyTestCollectionOrderer = ExtensibilityPointFactory.GetTestCollectionOrderer(testCollectionOrdererAttribute);
				if (AssemblyTestCollectionOrderer is null)
				{
					var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCollectionOrdererAttribute);

					TestContext.Current?.SendDiagnosticMessage("Could not find type '{0}' in {1} for assembly-level test collection orderer", type, assembly);
				}
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();
				var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCollectionOrdererAttribute);

				TestContext.Current?.SendDiagnosticMessage(
					"Assembly-level test collection orderer '{0}' threw '{1}' during construction: {2}{3}{4}",
					type,
					innerEx.GetType().FullName,
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}
		}
	}

	/// <summary>
	/// Delegation of <see cref="XunitTestAssemblyRunner.RunTestCollectionAsync"/> that properly obeys the parallel
	/// algorithm requirements.
	/// </summary>
	public async ValueTask<RunSummary> RunTestCollectionAsync(
		_ITestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ITestCaseOrderer testCaseOrderer)
	{
		if (parallelSemaphore is not null)
			await parallelSemaphore.WaitAsync(CancellationTokenSource.Token);

		try
		{
			return await XunitTestCollectionRunner.Instance.RunAsync(
				testCollection,
				testCases,
				ExplicitOption,
				MessageBus,
				testCaseOrderer,
				Aggregator.Clone(),
				CancellationTokenSource,
				AssemblyFixtureMappings
			);
		}
		finally
		{
			parallelSemaphore?.Release();
		}
	}

	/// <summary>
	/// Sets up the mechanics for parallelism.
	/// </summary>
	public virtual void SetupParallelism()
	{
		// When unlimited, we just launch everything and let the .NET Thread Pool sort it out
		if (MaxParallelThreads < 0)
			return;

		// For aggressive, we launch everything and let our sync context limit what's allowed to run
		if (ParallelAlgorithm == ParallelAlgorithm.Aggressive && MaxConcurrencySyncContext.IsSupported)
		{
			syncContext = new MaxConcurrencySyncContext(MaxParallelThreads);
			SetupSyncContextInternal(syncContext);
		}
		// For conversative, we use a semaphore to limit the number of launched tests, and ensure
		// that the .NET Thread Pool has enough threads based on the user's requested maximum
		else
		{
			parallelSemaphore = new(initialCount: MaxParallelThreads);

			ThreadPool.GetMinThreads(out var minThreads, out var minIOPorts);
			if (minThreads < MaxParallelThreads)
				ThreadPool.SetMinThreads(MaxParallelThreads, minIOPorts);
		}
	}

	[SecuritySafeCritical]
	static void SetupSyncContextInternal(SynchronizationContext? context) =>
		SynchronizationContext.SetSynchronizationContext(context);
}
