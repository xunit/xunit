using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test case runner for xUnit.net v3 theories (which could not be pre-enumerated;
	/// pre-enumerated test cases use <see cref="XunitTestCaseRunner"/>).
	/// </summary>
	public class XunitDelayEnumeratedTheoryTestCaseRunner : XunitTestCaseRunner
	{
		static readonly object?[] NoArguments = new object[0];

		readonly ExceptionAggregator cleanupAggregator = new();
		Exception? dataDiscoveryException;
		readonly DisposalTracker disposalTracker = new();
		readonly List<XunitTestRunner> testRunners = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitDelayEnumeratedTheoryTestCaseRunner"/> class.
		/// </summary>
		/// <param name="testCase">The test case to be run.</param>
		/// <param name="displayName">The display name of the test case.</param>
		/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		public XunitDelayEnumeratedTheoryTestCaseRunner(
			IXunitTestCase testCase,
			string displayName,
			string? skipReason,
			object?[] constructorArguments,
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(testCase, displayName, skipReason, constructorArguments, NoArguments, messageBus, aggregator, cancellationTokenSource)
		{
			DiagnosticMessageSink = Guard.ArgumentNotNull(diagnosticMessageSink);
		}

		/// <summary>
		/// Gets the message sink used to report <see cref="_DiagnosticMessage"/> messages.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink { get; }

		/// <inheritdoc/>
		protected override async ValueTask AfterTestCaseStartingAsync()
		{
			await base.AfterTestCaseStartingAsync();

			if (TestCase.TestMethod == null)
				return;

			try
			{
				var dataAttributes = TestCase.TestMethod.Method.GetCustomAttributes(typeof(DataAttribute));

				var testIndex = 0;

				foreach (var dataAttribute in dataAttributes)
				{
					var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).FirstOrDefault();
					if (discovererAttribute == null)
					{
						if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
							Aggregator.Add(new InvalidOperationException($"Data attribute {reflectionAttribute.Attribute.GetType().FullName} on {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name} does not have a discoverer attribute attached."));
						else
							Aggregator.Add(new InvalidOperationException($"A data attribute specified on {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name} does not have a discoverer attribute attached."));

						continue;
					}

					IDataDiscoverer? discoverer;
					try
					{
						discoverer = ExtensibilityPointFactory.GetDataDiscoverer(DiagnosticMessageSink, discovererAttribute);
						if (discoverer == null)
						{
							if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
								Aggregator.Add(new InvalidOperationException($"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name} does not exist or could not be constructed."));
							else
								Aggregator.Add(new InvalidOperationException($"A data discoverer specified on {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name} does not exist or could not be constructed."));

							continue;
						}
					}
					catch (InvalidCastException)
					{
						if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
							Aggregator.Add(new InvalidOperationException($"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name} does not implement IDataDiscoverer."));
						else
							Aggregator.Add(new InvalidOperationException($"A data discoverer specified on {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name} does not implement IDataDiscoverer."));

						continue;
					}

					var data = await discoverer.GetData(dataAttribute, TestCase.TestMethod.Method);
					if (data == null)
					{
						Aggregator.Add(new InvalidOperationException($"Test data returned null for {TestCase.TestMethod.TestClass.Class.Name}.{TestCase.TestMethod.Method.Name}. Make sure it is statically initialized before this test method is called."));
						continue;
					}

					foreach (var dataRow in data)
					{
						var dataRowData = dataRow.GetData();
						foreach (var dataRowItem in dataRowData)
							disposalTracker.Add(dataRowItem);

						_ITypeInfo[]? resolvedTypes = null;
						var methodToRun = TestMethod;
						var convertedDataRow = methodToRun.ResolveMethodArguments(dataRowData);

						if (methodToRun.IsGenericMethodDefinition)
						{
							resolvedTypes = TestCase.TestMethod.Method.ResolveGenericTypes(convertedDataRow);
							methodToRun = methodToRun.MakeGenericMethod(resolvedTypes.Select(t => ((_IReflectionTypeInfo)t).Type).ToArray());
						}

						var parameterTypes = methodToRun.GetParameters().Select(p => p.ParameterType).ToArray();
						convertedDataRow = Reflector.ConvertArguments(convertedDataRow, parameterTypes);

						var theoryDisplayName = TestCase.TestMethod.Method.GetDisplayNameWithArguments(DisplayName, convertedDataRow, resolvedTypes);
						var test = CreateTest(TestCase, theoryDisplayName, testIndex++);
						var skipReason = SkipReason ?? dataAttribute.GetNamedArgument<string>("Skip") ?? dataRow.Skip;
						testRunners.Add(CreateTestRunner(test, MessageBus, TestClass, ConstructorArguments, methodToRun, convertedDataRow, skipReason, BeforeAfterAttributes, Aggregator, CancellationTokenSource));
					}
				}
			}
			catch (Exception ex)
			{
				// Stash the exception so we can surface it during RunTestAsync
				dataDiscoveryException = ex;
			}
		}

		/// <inheritdoc/>
		protected override ValueTask BeforeTestCaseFinishedAsync()
		{
			Aggregator.Aggregate(cleanupAggregator);

			return base.BeforeTestCaseFinishedAsync();
		}

		/// <inheritdoc/>
		protected override async ValueTask<RunSummary> RunTestAsync()
		{
			if (dataDiscoveryException != null)
				return RunTest_DataDiscoveryException();

			var runSummary = new RunSummary();
			foreach (var testRunner in testRunners)
				runSummary.Aggregate(await testRunner.RunAsync());

			// Run the cleanup here so we can include cleanup time in the run summary,
			// but save any exceptions so we can surface them during the cleanup phase,
			// so they get properly reported as test case cleanup failures.
			TimeSpan elapsedTime = TimeSpan.Zero;

			foreach (var trackedObject in disposalTracker.TrackedObjects)
			{
				if (trackedObject is IAsyncDisposable asyncDisposable)
					elapsedTime += await ExecutionTimer.MeasureAsync(() => cleanupAggregator.RunAsync(asyncDisposable.DisposeAsync));
				if (trackedObject is IDisposable disposable)
					elapsedTime += ExecutionTimer.Measure(() => cleanupAggregator.Run(disposable.Dispose));
			}

			disposalTracker.Clear();

			runSummary.Time += (decimal)elapsedTime.TotalSeconds;
			return runSummary;
		}

		RunSummary RunTest_DataDiscoveryException()
		{
			// Use -1 for the index here so we don't collide with any legitimate test case IDs that might've been used
			var test = new XunitTest(TestCase, DisplayName, testIndex: -1);

			var testAssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = TestCase.TestCollection.UniqueID;
			var testClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = TestCase.UniqueID;

			var testStarting = new _TestStarting
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestDisplayName = test.DisplayName,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = test.UniqueID
			};

			if (!MessageBus.QueueMessage(testStarting))
				CancellationTokenSource.Cancel();
			else
			{
				var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(dataDiscoveryException!.Unwrap());
				var testFailed = new _TestFailed
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					ExceptionParentIndices = exceptionParentIndices,
					ExceptionTypes = exceptionTypes,
					ExecutionTime = 0m,
					Messages = messages,
					Output = "",
					StackTraces = stackTraces,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = test.UniqueID
				};
				if (!MessageBus.QueueMessage(testFailed))
					CancellationTokenSource.Cancel();
			}

			var testFinished = new _TestFinished
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				ExecutionTime = 0m,
				Output = "",
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = test.UniqueID
			};

			if (!MessageBus.QueueMessage(testFinished))
				CancellationTokenSource.Cancel();

			return new RunSummary { Total = 1, Failed = 1 };
		}
	}
}
