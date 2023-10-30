using System;
using System.Globalization;
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
		Guard.ArgumentNotNull(ctxt);

		await base.AfterTestCaseStartingAsync(ctxt);

		if (ctxt.TestCase.TestMethod is null)
			return;

		try
		{
			var dataAttributes = ctxt.TestCase.TestMethod.Method.GetCustomAttributes(typeof(DataAttribute));

			var testIndex = 0;

			foreach (var dataAttribute in dataAttributes)
			{
				var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).FirstOrDefault();
				if (discovererAttribute is null)
				{
					ctxt.Aggregator.Add(
						new InvalidOperationException(
							string.Format(
								CultureInfo.CurrentCulture,
								"Data attribute {0} on {1}.{2} does not have a discoverer attribute attached.",
								dataAttribute is _IReflectionAttributeInfo reflectionAttribute ? reflectionAttribute.Attribute.GetType().SafeName() : dataAttribute.AttributeType.Name,
								ctxt.TestCase.TestMethod.TestClass.Class.Name,
								ctxt.TestCase.TestMethod.Method.Name
							)
						)
					);

					continue;
				}

				IDataDiscoverer? discoverer;
				try
				{
					discoverer = ExtensibilityPointFactory.GetDataDiscoverer(discovererAttribute);
					if (discoverer is null)
					{
						ctxt.Aggregator.Add(
							new InvalidOperationException(
								string.Format(
									CultureInfo.CurrentCulture,
									"Data discoverer specified for {0} on {1}.{2} does not exist or could not be constructed.",
									dataAttribute is _IReflectionAttributeInfo reflectionAttribute ? reflectionAttribute.Attribute.GetType().SafeName() : dataAttribute.AttributeType.Name,
									ctxt.TestCase.TestMethod.TestClass.Class.Name,
									ctxt.TestCase.TestMethod.Method.Name
								)
							)
						);

						continue;
					}
				}
				catch (InvalidCastException)
				{
					ctxt.Aggregator.Add(
						new InvalidOperationException(
							string.Format(
								CultureInfo.CurrentCulture,
								"Data discoverer specified for {0} on {1}.{2} does not implement IDataDiscoverer.",
								dataAttribute is _IReflectionAttributeInfo reflectionAttribute ? reflectionAttribute.Attribute.GetType().SafeName() : dataAttribute.AttributeType.Name,
								ctxt.TestCase.TestMethod.TestClass.Class.Name,
								ctxt.TestCase.TestMethod.Method.Name
							)
						)
					);

					continue;
				}

				var data = await discoverer.GetData(dataAttribute, ctxt.TestCase.TestMethod.Method, ctxt.DisposalTracker);
				if (data is null)
				{
					ctxt.Aggregator.Add(
						new InvalidOperationException(
							string.Format(
								CultureInfo.CurrentCulture,
								"Test data returned null for {0}.{1}. Make sure it is statically initialized before this test method is called.",
								ctxt.TestCase.TestMethod.TestClass.Class.Name,
								ctxt.TestCase.TestMethod.Method.Name
							)
						)
					);

					continue;
				}

				foreach (var dataRow in data)
				{
					var dataRowData = dataRow.GetData();
					ctxt.DisposalTracker.AddRange(dataRowData);

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

					var baseDisplayName = dataRow.TestDisplayName ?? dataAttribute.GetNamedArgument<string>(nameof(DataAttribute.TestDisplayName)) ?? ctxt.DisplayName;
					var theoryDisplayName = ctxt.TestCase.TestMethod.Method.GetDisplayNameWithArguments(baseDisplayName, convertedDataRow, resolvedTypes);
					var traits = TestIntrospectionHelper.GetTraits(ctxt.TestCase.TestMethod, dataRow);
					var timeout = dataRow.Timeout ?? dataAttribute.GetNamedArgument<int?>(nameof(DataAttribute.Timeout)) ?? ctxt.TestCase.Timeout;
					var test = CreateTest(ctxt, dataRow.Explicit, theoryDisplayName, testIndex++, traits.ToReadOnly(), timeout);
					var skipReason = dataRow.Skip ?? dataAttribute.GetNamedArgument<string>(nameof(DataAttribute.Skip)) ?? ctxt.SkipReason;

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
		Guard.ArgumentNotNull(ctxt);

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
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> RunAsync(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		object?[] constructorArguments,
		object?[]? testMethodArguments)
	{
		Guard.ArgumentNotNull(testCase);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(constructorArguments);

		var (testClass, testMethod, beforeAfterTestAttributes) = Initialize(testCase, ref testMethodArguments);

		await using var ctxt = new XunitDelayEnumeratedTheoryTestCaseRunnerContext(testCase, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, explicitOption, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterTestAttributes);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestsAsync(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.DiscoveryException is not null)
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
					ctxt.ExplicitOption,
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
			else if (trackedObject is IDisposable disposable)
				elapsedTime += ExecutionTimer.Measure(() => ctxt.CleanupAggregator.Run(disposable.Dispose));
		}

		ctxt.DisposalTracker.Clear();

		runSummary.Time += (decimal)elapsedTime.TotalSeconds;
		return runSummary;
	}

	static RunSummary RunTest_DataDiscoveryException(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		// Use -1 for the index here so we don't collide with any legitimate test IDs that might've been used
		var test = new XunitTest(ctxt.TestCase, @explicit: null, ctxt.DisplayName, testIndex: -1, ctxt.TestCase.Traits, timeout: 0);

		var testAssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID;
		var testClassUniqueID = ctxt.TestCase.TestClass?.UniqueID;
		var testMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID;
		var testCaseUniqueID = ctxt.TestCase.UniqueID;

		var testStarting = new _TestStarting
		{
			AssemblyUniqueID = testAssemblyUniqueID,
			Explicit = false,
			TestCaseUniqueID = testCaseUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestDisplayName = test.TestDisplayName,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = test.UniqueID,
			Timeout = 0,
			Traits = test.Traits,
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
