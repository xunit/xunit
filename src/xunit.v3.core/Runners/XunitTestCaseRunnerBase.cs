using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class for running test cases that implement <see cref="IXunitTestCase"/>. Gives an opportunity
/// for derived classes to define their own context class and custom test case class.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The test case type used by the runner</typeparam>
public class XunitTestCaseRunnerBase<TContext, TTestCase> : TestCaseRunner<TContext, TTestCase>
	where TContext : XunitTestCaseRunnerContext<TTestCase>
	where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Creates the <see cref="_ITest"/> instance for the given test case. By default, creates an instance
	/// of the <see cref="XunitTest"/> class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="testMethod">The test method to be run; may differ from the test method embedded into the test case</param>
	/// <param name="explicit">A flag to indicate the test was marked as explicit; if not set, will fall back to the test case</param>
	/// <param name="displayName">The display name for the test; if <c>null</c>is passed, defaults to
	/// the DisplayName value from <paramref name="ctxt"/>.</param>
	/// <param name="testIndex">The test index for the test. Multiple test per test case scenarios will need
	/// to use the test index to help construct the test unique ID.</param>
	/// <param name="traits">The traits for the test.</param>
	/// <param name="timeout">The timeout for the test.</param>
	protected virtual IXunitTest CreateTest(
		TContext ctxt,
		IXunitTestMethod testMethod,
		bool? @explicit,
		string? displayName,
		int testIndex,
		IReadOnlyDictionary<string, IReadOnlyList<string>> traits,
		int timeout)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(traits);

		return new XunitTest(ctxt.TestCase, testMethod, @explicit, displayName ?? ctxt.DisplayName, testIndex, traits, timeout);
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCaseCleanupFailure(
		TContext ctxt,
		Exception exception) =>
			new(ReportMessage(ctxt, new _TestCaseCleanupFailure(), exception: exception));

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCaseFinished(
		TContext ctxt,
		RunSummary summary) =>
			new(ReportMessage(ctxt, new _TestCaseFinished(), summary: summary));

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCaseStarting(TContext ctxt) =>
		new(ReportMessage(ctxt, new _TestCaseStarting
		{
			SkipReason = Guard.ArgumentNotNull(ctxt).TestCase.SkipReason,
			SourceFilePath = ctxt.TestCase.SourceFilePath,
			SourceLineNumber = ctxt.TestCase.SourceLineNumber,
			TestCaseDisplayName = ctxt.TestCase.TestCaseDisplayName,
			Traits = ctxt.TestCase.Traits,
		}));

	static bool ReportMessage(
		TContext ctxt,
		_TestCaseMessage message,
		RunSummary summary = default,
		Exception? exception = null)
	{
		Guard.ArgumentNotNull(ctxt);

		message.AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID;
		message.TestCaseUniqueID = ctxt.TestCase.UniqueID;
		message.TestClassUniqueID = ctxt.TestCase.TestClass.UniqueID;
		message.TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID;
		message.TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID;

		if (message is _IWritableExecutionSummaryMetadata summaryMessage)
		{
			summaryMessage.ExecutionTime = summary.Time;
			summaryMessage.TestsFailed = summary.Failed;
			summaryMessage.TestsNotRun = summary.NotRun;
			summaryMessage.TestsSkipped = summary.Skipped;
			summaryMessage.TestsTotal = summary.Total;
		}

		if (exception is not null && message is _IWritableErrorMetadata errorMessage)
		{
			var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

			errorMessage.ExceptionParentIndices = indices;
			errorMessage.ExceptionTypes = types;
			errorMessage.Messages = messages;
			errorMessage.StackTraces = stackTraces;
		}

		return ctxt.MessageBus.QueueMessage(message);
	}

	/// <summary>
	/// Computes values from the test case and resolves the test method arguments. To be called by the public RunAsync
	/// method that will end up being exposed by the derived class as the primary public API.
	/// </summary>
	/// <param name="testCase">The test case that is being run</param>
	/// <param name="testMethodArguments">The test method arguments to be converted</param>
	protected object?[] ResolveTestMethodArguments(
		IXunitTestCase testCase,
		object?[] testMethodArguments)
	{
		Guard.ArgumentNotNull(testCase);

		var parameters = testCase.TestMethod.Parameters.CastOrToArray();
		var parameterTypes = new Type[parameters.Length];
		for (var i = 0; i < parameters.Length; i++)
			parameterTypes[i] = parameters[i].ParameterType;

		return TypeHelper.ConvertArguments(testMethodArguments, parameterTypes);
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestsAsync(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [ctxt.TestCase], exception, sendTestCaseMessages: false);

		return await XunitTestRunner.Instance.RunAsync(
			CreateTest(ctxt, ctxt.TestCase.TestMethod, @explicit: null, displayName: null, testIndex: 0, ctxt.TestCase.Traits, ctxt.TestCase.Timeout),
			ctxt.MessageBus,
			ctxt.ConstructorArguments,
			ctxt.TestMethodArguments,
			ctxt.SkipReason,
			ctxt.ExplicitOption,
			ctxt.Aggregator,
			ctxt.CancellationTokenSource,
			ctxt.BeforeAfterTestAttributes
		);
	}
}
