using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test assembly runner for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestAssemblyRunner : TestAssemblyRunner<IXunitTestCase>
	{
		Dictionary<Type, object> assemblyFixtureMappings = new();
		_IAttributeInfo? collectionBehaviorAttribute;
		bool disableParallelization;
		bool initialized;
		int maxParallelThreads;
		SynchronizationContext? originalSyncContext;
		MaxConcurrencySyncContext? syncContext;
		ITestCaseOrderer? testCaseOrdererOverride;
		ITestCollectionOrderer? testCollectionOrdererOverride;

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

		/// <summary>
		/// Gets the fixture mappings that were created during <see cref="AfterTestAssemblyStartingAsync"/>.
		/// </summary>
		protected Dictionary<Type, object> AssemblyFixtureMappings
		{
			get => assemblyFixtureMappings;
			set => assemblyFixtureMappings = Guard.ArgumentNotNull(value, nameof(AssemblyFixtureMappings));
		}

		/// <inheritdoc/>
		protected override async ValueTask AfterTestAssemblyStartingAsync()
		{
			Initialize();

			await CreateAssemblyFixturesAsync();
			await base.AfterTestAssemblyStartingAsync();
		}

		/// <inheritdoc/>
		protected override async ValueTask BeforeTestAssemblyFinishedAsync()
		{
			var disposeAsyncTasks =
				AssemblyFixtureMappings
					.Values
					.OfType<IAsyncDisposable>()
					.Select(fixture => Aggregator.RunAsync(async () =>
					{
						try
						{
							await fixture.DisposeAsync();
						}
						catch (Exception ex)
						{
							throw new TestFixtureCleanupException($"Assembly fixture type '{fixture.GetType().FullName}' threw in DisposeAsync", ex.Unwrap());
						}
					}).AsTask())
					.ToList();

			await Task.WhenAll(disposeAsyncTasks);

			foreach (var fixture in AssemblyFixtureMappings.Values.OfType<IDisposable>())
				Aggregator.Run(() =>
				{
					try
					{
						fixture.Dispose();
					}
					catch (Exception ex)
					{
						throw new TestFixtureCleanupException($"Assembly fixture type '{fixture.GetType().FullName}' threw in Dispose", ex.Unwrap());
					}
				});

			SetupSyncContextInternal(originalSyncContext);
			await base.BeforeTestAssemblyFinishedAsync();
		}

		/// <summary>
		/// Creates the instance of a assembly fixture type to be used by the test assembly. If the fixture can be created,
		/// it should be placed into the <see cref="AssemblyFixtureMappings"/> dictionary; if it cannot, then the method
		/// should record the error by calling <code>Aggregator.Add</code>.
		/// </summary>
		/// <param name="fixtureType">The type of the fixture to be created</param>
		protected virtual void CreateAssemblyFixture(Type fixtureType)
		{
			var ctors =
				fixtureType
					.GetConstructors()
					.Where(ci => !ci.IsStatic && ci.IsPublic)
					.ToList();

			if (ctors.Count != 1)
			{
				Aggregator.Add(new TestClassException($"Assembly fixture type '{fixtureType.FullName}' may only define a single public constructor."));
				return;
			}

			var ctor = ctors[0];
			var missingParameters = new List<ParameterInfo>();
			var ctorArgs = ctor.GetParameters().Select(p =>
			{
				object? arg = null;
				if (p.ParameterType == typeof(_IMessageSink))
					arg = DiagnosticMessageSink;
				else if (p.ParameterType == typeof(ITestContextAccessor))
					arg = TestContextAccessor.Instance;
				else
					missingParameters.Add(p);
				return arg;
			}).ToArray();

			if (missingParameters.Count > 0)
				Aggregator.Add(new TestClassException(
					$"Assembly fixture type '{fixtureType.FullName}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}"
				));
			else
				Aggregator.Run(() =>
				{
					try
					{
						AssemblyFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs);
					}
					catch (Exception ex)
					{
						throw new TestClassException($"Assembly fixture type '{fixtureType.FullName}' threw in its constructor", ex.Unwrap());
					}
				});
		}

		ValueTask CreateAssemblyFixturesAsync()
		{
			foreach (var attributeInfo in TestAssembly.Assembly.GetCustomAttributes(typeof(AssemblyFixtureAttribute)))
			{
				var fixtureType = attributeInfo.GetConstructorArguments().Single() as Type;
				if (fixtureType != null)
					CreateAssemblyFixture(fixtureType);
			}

			var initializeAsyncTasks =
				AssemblyFixtureMappings
					.Values
					.OfType<IAsyncLifetime>()
					.Select(
						fixture => Aggregator.RunAsync(async () =>
						{
							try
							{
								await fixture.InitializeAsync();
							}
							catch (Exception ex)
							{
								throw new TestClassException($"Assembly fixture type '{fixture.GetType().FullName}' threw in InitializeAsync", ex.Unwrap());
							}
						}).AsTask()
					)
					.ToList();

			return new(Task.WhenAll(initializeAsyncTasks));
		}

		/// <inheritdoc/>
		public override async ValueTask DisposeAsync()
		{
			if (syncContext is IAsyncDisposable asyncDisposable)
				await asyncDisposable.DisposeAsync();
			else if (syncContext is IDisposable disposable)
				disposable.Dispose();
		}

		/// <inheritdoc/>
		protected override ITestCaseOrderer GetTestCaseOrderer()
		{
			Initialize();

			return testCaseOrdererOverride ?? base.GetTestCaseOrderer();
		}

		/// <inheritdoc/>
		protected override ITestCollectionOrderer GetTestCollectionOrderer()
		{
			Initialize();

			return testCollectionOrdererOverride ?? base.GetTestCollectionOrderer();
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
		/// Ensures the assembly runner is initialized (sets up the collection behavior, parallelization options, and test orderers
		/// from their assembly attributes). If this method is overridden, it must call this base version before doing any
		/// further initialization work; derived methods must also support multiple calls to this method gracefully.
		/// </summary>
		protected virtual void Initialize()
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
					testCaseOrdererOverride = ExtensibilityPointFactory.GetTestCaseOrderer(DiagnosticMessageSink, testCaseOrdererAttribute);
					if (testCaseOrdererOverride == null)
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
					testCollectionOrdererOverride = ExtensibilityPointFactory.GetTestCollectionOrderer(DiagnosticMessageSink, testCollectionOrdererAttribute);
					if (testCollectionOrdererOverride == null)
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
		protected override async ValueTask<RunSummary> RunTestCollectionsAsync(
			IMessageBus messageBus,
			CancellationTokenSource cancellationTokenSource)
		{
			originalSyncContext = SynchronizationContext.Current;

			if (disableParallelization)
				return await base.RunTestCollectionsAsync(messageBus, cancellationTokenSource);

			SetupSyncContext(maxParallelThreads);

			Func<Func<ValueTask<RunSummary>>, ValueTask<RunSummary>> taskRunner;
			if (SynchronizationContext.Current != null)
			{
				var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
				taskRunner = code => new(Task.Factory.StartNew(() => code().AsTask(), cancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap());
			}
			else
				taskRunner = code => new(Task.Run(() => code().AsTask(), cancellationTokenSource.Token));

			List<ValueTask<RunSummary>>? parallel = null;
			List<Func<ValueTask<RunSummary>>>? nonParallel = null;
			var summaries = new List<RunSummary>();

			foreach (var collection in OrderTestCollections())
			{
				ValueTask<RunSummary> task() => RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource);

				var attr = collection.Item1.CollectionDefinition?.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).SingleOrDefault();
				if (attr?.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) == true)
					(nonParallel ??= new List<Func<ValueTask<RunSummary>>>()).Add(task);
				else
					(parallel ??= new List<ValueTask<RunSummary>>()).Add(taskRunner(task));
			}

			if (parallel?.Count > 0)
				foreach (var task in parallel)
					try
					{
						summaries.Add(await task);
					}
					catch (TaskCanceledException) { }

			if (nonParallel?.Count > 0)
				foreach (var task in nonParallel)
					try
					{
						summaries.Add(await taskRunner(task));
						if (cancellationTokenSource.IsCancellationRequested)
							break;
					}
					catch (TaskCanceledException) { }

			return new RunSummary()
			{
				Total = summaries.Sum(s => s.Total),
				Failed = summaries.Sum(s => s.Failed),
				Skipped = summaries.Sum(s => s.Skipped)
			};
		}

		/// <inheritdoc/>
		protected override ValueTask<RunSummary> RunTestCollectionAsync(
			IMessageBus messageBus,
			_ITestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases,
			CancellationTokenSource cancellationTokenSource) =>
				new XunitTestCollectionRunner(
					testCollection,
					testCases,
					DiagnosticMessageSink,
					messageBus,
					GetTestCaseOrderer(),
					new ExceptionAggregator(Aggregator),
					cancellationTokenSource,
					AssemblyFixtureMappings
				).RunAsync();

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
				SetupSyncContextInternal(syncContext);
			}
		}

		[SecuritySafeCritical]
		static void SetupSyncContextInternal(SynchronizationContext? context) =>
			SynchronizationContext.SetSynchronizationContext(context);
	}
}
