using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 theories (which could not be pre-enumerated;
/// pre-enumerated test cases use <see cref="XunitTestCaseRunner"/>).
/// </summary>
public class XunitDelayEnumeratedTheoryTestCaseRunner : XunitTestCaseRunnerBase<XunitDelayEnumeratedTheoryTestCaseRunnerContext>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitDelayEnumeratedTheoryTestCaseRunner"/> class.
	/// </summary>
	protected XunitDelayEnumeratedTheoryTestCaseRunner()
	{ }
	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitDelayEnumeratedTheoryTestCaseRunner"/> class.
	/// </summary>
	public static XunitDelayEnumeratedTheoryTestCaseRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override async ValueTask AfterTestCaseStartingAsync(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		await base.AfterTestCaseStartingAsync(ctxt);

		if (ctxt.TestCase.TestMethod == null)
			return;

		try
		{
			var dataAttributes = ctxt.TestCase.TestMethod.Method.GetCustomAttributes(typeof(DataAttribute));

			var testIndex = 0;

			foreach (var dataAttribute in dataAttributes)
			{
				var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).FirstOrDefault();
				if (discovererAttribute == null)
				{
					if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
						ctxt.Aggregator.Add(new InvalidOperationException($"Data attribute {reflectionAttribute.Attribute.GetType().FullName} on {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name} does not have a discoverer attribute attached."));
					else
						ctxt.Aggregator.Add(new InvalidOperationException($"A data attribute specified on {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name} does not have a discoverer attribute attached."));

					continue;
				}

				IDataDiscoverer? discoverer;
				try
				{
					discoverer = ExtensibilityPointFactory.GetDataDiscoverer(discovererAttribute);
					if (discoverer == null)
					{
						if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
							ctxt.Aggregator.Add(new InvalidOperationException($"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name} does not exist or could not be constructed."));
						else
							ctxt.Aggregator.Add(new InvalidOperationException($"A data discoverer specified on {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name} does not exist or could not be constructed."));

						continue;
					}
				}
				catch (InvalidCastException)
				{
					if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
						ctxt.Aggregator.Add(new InvalidOperationException($"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name} does not implement IDataDiscoverer."));
					else
						ctxt.Aggregator.Add(new InvalidOperationException($"A data discoverer specified on {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name} does not implement IDataDiscoverer."));

					continue;
				}

				var data = await discoverer.GetData(dataAttribute, ctxt.TestCase.TestMethod.Method);
				if (data == null)
				{
					ctxt.Aggregator.Add(new InvalidOperationException($"Test data returned null for {ctxt.TestCase.TestMethod.TestClass.Class.Name}.{ctxt.TestCase.TestMethod.Method.Name}. Make sure it is statically initialized before this test method is called."));
					continue;
				}

				foreach (var dataRow in data)
				{
					var dataRowData = dataRow.GetData();
					foreach (var dataRowItem in dataRowData)
						ctxt.DisposalTracker.Add(dataRowItem);

					_ITypeInfo[]? resolvedTypes = null;
					var methodToRun = ctxt.TestMethod;
					var convertedDataRow = methodToRun.ResolveMethodArguments(dataRowData);

					if (methodToRun.IsGenericMethodDefinition)
					{
						resolvedTypes = ctxt.TestCase.TestMethod.Method.ResolveGenericTypes(convertedDataRow);
						methodToRun = methodToRun.MakeGenericMethod(resolvedTypes.Select(t => ((_IReflectionTypeInfo)t).Type).ToArray());
					}

					var parameterTypes = methodToRun.GetParameters().Select(p => p.ParameterType).ToArray();
					convertedDataRow = Reflector.ConvertArguments(convertedDataRow, parameterTypes);

					var theoryDisplayName = ctxt.TestCase.TestMethod.Method.GetDisplayNameWithArguments(ctxt.DisplayName, convertedDataRow, resolvedTypes);
					var test = CreateTest(ctxt, theoryDisplayName, testIndex++);
					var skipReason = ctxt.SkipReason ?? dataAttribute.GetNamedArgument<string>("Skip") ?? dataRow.Skip;

					ctxt.DiscoveredTests.Add((test, methodToRun, convertedDataRow, skipReason));
				}
			}
		}
		catch (Exception ex)
		{
			// Stash the exception so we can surface it during RunTestAsync
			ctxt.DiscoveryException = ex;
		}
	}

	/// <inheritdoc/>
	protected override ValueTask BeforeTestCaseFinishedAsync(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		ctxt.Aggregator.Aggregate(ctxt.CleanupAggregator);

		return base.BeforeTestCaseFinishedAsync(ctxt);
	}

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public ValueTask<RunSummary> RunAsync(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		object?[] constructorArguments,
		object?[]? testMethodArguments)
	{
		Guard.ArgumentNotNull(testCase);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(constructorArguments);

		var (testClass, testMethod, beforeAfterTestAttributes) = Initialize(testCase, ref testMethodArguments);

		return RunAsync(new(testCase, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterTestAttributes));
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestsAsync(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		if (ctxt.DiscoveryException != null)
			return RunTest_DataDiscoveryException(ctxt);

		var runSummary = new RunSummary();

		foreach (var discoveredTest in ctxt.DiscoveredTests)
			runSummary.Aggregate(
				await XunitTestRunner.Instance.RunAsync(
					discoveredTest.Test,
					ctxt.MessageBus,
					ctxt.TestClass,
					ctxt.ConstructorArguments,
					discoveredTest.TestMethod,
					discoveredTest.TestMethodArguments,
					discoveredTest.SkipReason,
					ctxt.Aggregator,
					ctxt.CancellationTokenSource,
					ctxt.BeforeAfterTestAttributes
				)
			);

		// Run the cleanup here so we can include cleanup time in the run summary,
		// but save any exceptions so we can surface them during the cleanup phase,
		// so they get properly reported as test case cleanup failures.
		TimeSpan elapsedTime = TimeSpan.Zero;

		foreach (var trackedObject in ctxt.DisposalTracker.TrackedObjects)
		{
			if (trackedObject is IAsyncDisposable asyncDisposable)
				elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.CleanupAggregator.RunAsync(asyncDisposable.DisposeAsync));
			if (trackedObject is IDisposable disposable)
				elapsedTime += ExecutionTimer.Measure(() => ctxt.CleanupAggregator.Run(disposable.Dispose));
		}

		ctxt.DisposalTracker.Clear();

		runSummary.Time += (decimal)elapsedTime.TotalSeconds;
		return runSummary;
	}

	RunSummary RunTest_DataDiscoveryException(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		// Use -1 for the index here so we don't collide with any legitimate test IDs that might've been used
		var test = new XunitTest(ctxt.TestCase, ctxt.DisplayName, testIndex: -1);

		var testAssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID;
		var testClassUniqueID = ctxt.TestCase.TestMethod?.TestClass.UniqueID;
		var testMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID;
		var testCaseUniqueID = ctxt.TestCase.UniqueID;

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

		if (!ctxt.MessageBus.QueueMessage(testStarting))
			ctxt.CancellationTokenSource.Cancel();
		else
		{
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ctxt.DiscoveryException!.Unwrap());
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

			if (!ctxt.MessageBus.QueueMessage(testFailed))
				ctxt.CancellationTokenSource.Cancel();
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

		if (!ctxt.MessageBus.QueueMessage(testFinished))
			ctxt.CancellationTokenSource.Cancel();

		return new RunSummary { Total = 1, Failed = 1 };
	}
}
