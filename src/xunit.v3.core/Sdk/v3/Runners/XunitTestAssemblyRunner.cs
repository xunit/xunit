using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test assembly runner for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestAssemblyRunner : TestAssemblyRunner<IXunitTestCase>
	{
		_IAttributeInfo? collectionBehaviorAttribute;
		bool disableParallelization;
		bool initialized;
		int maxParallelThreads;
		SynchronizationContext? originalSyncContext;
		MaxConcurrencySyncContext? syncContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestAssemblyRunner"/> class.
		/// </summary>
		/// <param name="testAssembly">The assembly that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="executionMessageSink">The message sink to report run status to.</param>
		/// <param name="executionOptions">The user's requested execution options.</param>
		public XunitTestAssemblyRunner(
			_ITestAssembly testAssembly,
			IReadOnlyCollection<IXunitTestCase> testCases,
			_IMessageSink diagnosticMessageSink,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
				: base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
		{ }

		/// <inheritdoc/>
		public override async ValueTask DisposeAsync()
		{
			if (syncContext is IAsyncDisposable asyncDisposable)
				await asyncDisposable.DisposeAsync();
			if (syncContext is IDisposable disposable)
				disposable.Dispose();
		}

		/// <inheritdoc/>
		protected override string GetTestFrameworkDisplayName() =>
			XunitTestFrameworkDiscoverer.DisplayName;

		/// <inheritdoc/>
		protected override string GetTestFrameworkEnvironment()
		{
			Initialize();

			var testCollectionFactory =
				ExtensibilityPointFactory.GetXunitTestCollectionFactory(DiagnosticMessageSink, collectionBehaviorAttribute, TestAssembly)
				?? new CollectionPerClassTestCollectionFactory(TestAssembly, DiagnosticMessageSink);

			var threadCountText = maxParallelThreads < 0 ? "unlimited" : maxParallelThreads.ToString();

			return $"{base.GetTestFrameworkEnvironment()} [{testCollectionFactory?.DisplayName}, {(disableParallelization ? "non-parallel" : $"parallel ({threadCountText} threads)")}]";
		}

		/// <summary>
		/// Gets the synchronization context used when potentially running tests in parallel.
		/// If <paramref name="maxParallelThreads"/> is greater than 0, it creates
		/// and uses an instance of <see cref="T:Xunit.Sdk.MaxConcurrencySyncContext"/>.
		/// </summary>
		/// <param name="maxParallelThreads">The maximum number of parallel threads.</param>
		protected virtual void SetupSyncContext(int maxParallelThreads)
		{
			if (MaxConcurrencySyncContext.IsSupported && maxParallelThreads > 0)
			{
				syncContext = new MaxConcurrencySyncContext(maxParallelThreads);
				SetSynchronizationContext(syncContext);
			}
		}

		/// <summary>
		/// Ensures the assembly runner is initialized (sets up the collection behavior,
		/// parallelization options, and test orderers from their assembly attributes).
		/// </summary>
		protected void Initialize()
		{
			if (initialized)
				return;

			collectionBehaviorAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
			if (collectionBehaviorAttribute != null)
			{
				disableParallelization = collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");
				maxParallelThreads = collectionBehaviorAttribute.GetNamedArgument<int>("MaxParallelThreads");
			}

			disableParallelization = ExecutionOptions.DisableParallelization() ?? disableParallelization;
			maxParallelThreads = ExecutionOptions.MaxParallelThreads() ?? maxParallelThreads;
			if (maxParallelThreads == 0)
				maxParallelThreads = Environment.ProcessorCount;

			var testCaseOrdererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
			if (testCaseOrdererAttribute != null)
			{
				try
				{
					var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(DiagnosticMessageSink, testCaseOrdererAttribute);
					if (testCaseOrderer != null)
						TestCaseOrderer = testCaseOrderer;
					else
					{
						var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCaseOrdererAttribute);
						DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Could not find type '{type}' in {assembly} for assembly-level test case orderer" });
					}
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();
					var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCaseOrdererAttribute);
					DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Assembly-level test case orderer '{type}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}" });
				}
			}

			var testCollectionOrdererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCollectionOrdererAttribute)).SingleOrDefault();
			if (testCollectionOrdererAttribute != null)
			{
				try
				{
					var testCollectionOrderer = ExtensibilityPointFactory.GetTestCollectionOrderer(DiagnosticMessageSink, testCollectionOrdererAttribute);
					if (testCollectionOrderer != null)
						TestCollectionOrderer = testCollectionOrderer;
					else
					{
						var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCollectionOrdererAttribute);
						DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Could not find type '{type}' in {assembly} for assembly-level test collection orderer" });
					}
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();
					var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(testCollectionOrdererAttribute);
					DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Assembly-level test collection orderer '{type}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}" });
				}
			}

			initialized = true;
		}

		/// <inheritdoc/>
		protected override Task AfterTestAssemblyStartingAsync()
		{
			Initialize();
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override Task BeforeTestAssemblyFinishedAsync()
		{
			SetSynchronizationContext(originalSyncContext);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override async Task<RunSummary> RunTestCollectionsAsync(
			IMessageBus messageBus,
			CancellationTokenSource cancellationTokenSource)
		{
			originalSyncContext = SynchronizationContext.Current;

			if (disableParallelization)
				return await base.RunTestCollectionsAsync(messageBus, cancellationTokenSource);

			SetupSyncContext(maxParallelThreads);

			Func<Func<Task<RunSummary>>, Task<RunSummary>> taskRunner;
			if (SynchronizationContext.Current != null)
			{
				var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
				taskRunner = code => Task.Factory.StartNew(code, cancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap();
			}
			else
				taskRunner = code => Task.Run(code, cancellationTokenSource.Token);

			List<Task<RunSummary>>? parallel = null;
			List<Func<Task<RunSummary>>>? nonParallel = null;
			var summaries = new List<RunSummary>();

			foreach (var collection in OrderTestCollections())
			{
				Task<RunSummary> task() => RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource);

				var attr = collection.Item1.CollectionDefinition?.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).SingleOrDefault();
				if (attr?.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) == true)
					(nonParallel ??= new List<Func<Task<RunSummary>>>()).Add(task);
				else
					(parallel ??= new List<Task<RunSummary>>()).Add(taskRunner(task));
			}

			if (parallel?.Count > 0)
			{
				foreach (var task in parallel)
				{
					try
					{
						summaries.Add(await task);
					}
					catch (TaskCanceledException) { }
				}
			}

			if (nonParallel?.Count > 0)
			{
				foreach (var task in nonParallel)
				{
					try
					{
						summaries.Add(await taskRunner(task));
						if (cancellationTokenSource.IsCancellationRequested)
							break;
					}
					catch (TaskCanceledException) { }
				}
			}

			return new RunSummary()
			{
				Total = summaries.Sum(s => s.Total),
				Failed = summaries.Sum(s => s.Failed),
				Skipped = summaries.Sum(s => s.Skipped)
			};
		}

		/// <inheritdoc/>
		protected override Task<RunSummary> RunTestCollectionAsync(
			IMessageBus messageBus,
			_ITestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases,
			CancellationTokenSource cancellationTokenSource) =>
				new XunitTestCollectionRunner(
					testCollection,
					testCases,
					DiagnosticMessageSink,
					messageBus,
					TestCaseOrderer,
					new ExceptionAggregator(Aggregator),
					cancellationTokenSource
				).RunAsync();

		[SecuritySafeCritical]
		static void SetSynchronizationContext(SynchronizationContext? context) =>
			SynchronizationContext.SetSynchronizationContext(context);
	}
}
