using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class to be used for running delay-enumerated theory data test cases.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TTestCase"></typeparam>
public abstract class XunitDelayEnumeratedTestCaseRunnerBase<TContext, TTestCase> :
	XunitTestCaseRunnerBase<TContext, TTestCase>
		where TContext : XunitDelayEnumeratedTestCaseRunnerContext<TTestCase>
		where TTestCase : class, IXunitDelayEnumeratedTestCase
{
	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCaseFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		var elapsedTime = TimeSpan.Zero;

		foreach (var trackedObject in ctxt.DisposalTracker.TrackedObjects)
		{
			if (trackedObject is IAsyncDisposable asyncDisposable)
				elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(asyncDisposable.DisposeAsync));
			else if (trackedObject is IDisposable disposable)
				elapsedTime += ExecutionTimer.Measure(() => ctxt.Aggregator.Run(disposable.Dispose));
		}

		ctxt.DisposalTracker.Clear();

		summary.Time += (decimal)elapsedTime.TotalSeconds;
		return await base.OnTestCaseFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCaseStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestCaseStarting(ctxt);

		if (ctxt.TestCase.TestMethod is not null)
			try
			{
				var testIndex = 0;

				foreach (var dataAttribute in ctxt.TestCase.TestMethod.DataAttributes)
				{
					var data = await dataAttribute.GetData(ctxt.TestCase.TestMethod.Method, ctxt.DisposalTracker);
					if (data is null)
					{
						ctxt.Aggregator.Add(
							new InvalidOperationException(
								string.Format(
									CultureInfo.CurrentCulture,
									"Test data returned null for {0}.{1}. Make sure it is statically initialized before this test method is called.",
									ctxt.TestCase.TestMethod.TestClass.TestClassName,
									ctxt.TestCase.TestMethod.MethodName
								)
							)
						);

						continue;
					}


					foreach (var dataRow in data)
					{
						var dataRowData = dataRow.GetData();
						ctxt.DisposalTracker.AddRange(dataRowData);

						var testMethod = ctxt.TestCase.TestMethod;
						var resolvedTypes = testMethod.ResolveGenericTypes(dataRowData);
						if (resolvedTypes is not null)
							testMethod = new XunitTestMethod(testMethod.TestClass, testMethod.MakeGenericMethod(resolvedTypes), dataRowData);

						var convertedDataRow = testMethod.ResolveMethodArguments(dataRowData);

						var parameterTypes = testMethod.Parameters.Select(p => p.ParameterType).ToArray();
						convertedDataRow = TypeHelper.ConvertArguments(convertedDataRow, parameterTypes);

						var baseDisplayName = dataRow.TestDisplayName ?? dataAttribute.TestDisplayName ?? ctxt.DisplayName;
						var theoryDisplayName = testMethod.GetDisplayName(baseDisplayName, convertedDataRow, resolvedTypes);
						var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);
						var timeout = dataRow.Timeout ?? dataAttribute.Timeout ?? ctxt.TestCase.Timeout;
						var test = CreateTest(ctxt, testMethod, dataRow.Explicit, theoryDisplayName, testIndex++, traits.ToReadOnly(), timeout, convertedDataRow);
						var skipReason = dataRow.Skip ?? dataAttribute.Skip ?? ctxt.SkipReason;

						ctxt.DiscoveredTests.Add((test, skipReason));
					}

					if (ctxt.DiscoveredTests.Count == 0)
					{
						var testMethod = ctxt.TestCase.TestMethod;
						var message = string.Format(CultureInfo.CurrentCulture, "No data found for {0}.{1}", testMethod.TestClass.TestClassName, testMethod.MethodName);

						throw new TestPipelineException(
							ctxt.TestCase.SkipTestWithoutData
								? DynamicSkipToken.Value + message
								: message
						);
					}
				}
			}
			catch (Exception ex)
			{
				// Stash the exception so we can surface it during RunTestAsync
				ctxt.DiscoveryException = ex.Unwrap();
			}

		return result;
	}

	/// <summary>
	/// Runs a single test. Override to change the default behavior for running an individual test.
	/// </summary>
	/// <remarks>
	/// By default, this uses <see cref="XunitTestRunner.RunAsync"/> to run the test.
	/// </remarks>
	/// <param name="ctxt"></param>
	/// <param name="test"></param>
	/// <param name="skipReason"></param>
	/// <returns></returns>
	protected virtual ValueTask<RunSummary> RunTestAsync(
		TContext ctxt,
		IXunitTest test,
		string? skipReason)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(test);

		return XunitTestRunner.Instance.RunAsync(
			test,
			ctxt.MessageBus,
			ctxt.ConstructorArguments,
			skipReason,
			ctxt.ExplicitOption,
			ctxt.Aggregator.Clone(),
			ctxt.CancellationTokenSource,
			ctxt.BeforeAfterTestAttributes
		);
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestsAsync(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var startupException = (exception, ctxt.DiscoveryException) switch
		{
			(null, null) => null,
			(null, _) => ctxt.DiscoveryException,
			(_, null) => exception,
			_ => new AggregateException(exception, ctxt.DiscoveryException),
		};

		if (startupException is not null)
		{
			var skipReason =
				startupException.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal)
					? startupException.Message.Substring(DynamicSkipToken.Value.Length)
					: null;

			// Create a new test case so it reflects the potentially overridden display name and skip reason
			await using var testCase = new XunitTestCase(
				ctxt.TestCase.TestMethod,
				ctxt.DisplayName,
				ctxt.TestCase.UniqueID,
				ctxt.TestCase.Explicit,
				skipReason ?? ctxt.SkipReason,
				ctxt.TestCase.SkipType,
				ctxt.TestCase.SkipUnless,
				ctxt.TestCase.SkipWhen,
				ctxt.TestCase.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
				ctxt.TestMethodArguments,
				ctxt.TestCase.SourceFilePath,
				ctxt.TestCase.SourceLineNumber,
				ctxt.TestCase.Timeout
			);

			return
				skipReason is null
					? XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [testCase], startupException, sendTestCaseMessages: false)
					: XunitRunnerHelper.SkipTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [testCase], skipReason, sendTestCaseMessages: false);
		}

		var summary = new RunSummary();

		foreach (var (test, skipReason) in ctxt.DiscoveredTests)
			summary.Aggregate(await RunTestAsync(ctxt, test, skipReason));

		return summary;
	}
}
