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
	/// Creates the <see cref="ITest"/> instance for the given test case. By default, creates an instance
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
	/// <param name="testMethodArguments">The arguments to be passed to the test method</param>
	protected virtual IXunitTest CreateTest(
		TContext ctxt,
		IXunitTestMethod testMethod,
		bool? @explicit,
		string? displayName,
		int testIndex,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		int timeout,
		object?[] testMethodArguments)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(traits);

		return new XunitTest(ctxt.TestCase, testMethod, @explicit, displayName ?? ctxt.DisplayName, testIndex, traits, timeout, testMethodArguments);
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCaseCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestCaseCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestCaseUniqueID = ctxt.TestCase.UniqueID,
			TestClassUniqueID = ctxt.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCaseFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestCaseFinished
		{
			AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestCaseUniqueID = ctxt.TestCase.UniqueID,
			TestClassUniqueID = ctxt.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCaseStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestCaseStarting
		{
			AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID,
			Explicit = ctxt.TestCase.Explicit,
			SkipReason = Guard.ArgumentNotNull(ctxt).TestCase.SkipReason,
			SourceFilePath = ctxt.TestCase.SourceFilePath,
			SourceLineNumber = ctxt.TestCase.SourceLineNumber,
			TestCaseDisplayName = ctxt.TestCase.TestCaseDisplayName,
			TestCaseUniqueID = ctxt.TestCase.UniqueID,
			TestClassMetadataToken = ctxt.TestCase.TestClassMetadataToken,
			TestClassName = ctxt.TestCase.TestClassName,
			TestClassNamespace = ctxt.TestCase.TestClassNamespace,
			TestClassSimpleName = ctxt.TestCase.TestClassSimpleName,
			TestClassUniqueID = ctxt.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID,
			TestMethodMetadataToken = ctxt.TestCase.TestMethodMetadataToken,
			TestMethodName = ctxt.TestCase.TestMethodName,
			TestMethodParameterTypesVSTest = ctxt.TestCase.TestMethodParameterTypesVSTest,
			TestMethodReturnTypeVSTest = ctxt.TestCase.TestMethodReturnTypeVSTest,
			TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID,
			Traits = ctxt.TestCase.Traits,
		}));
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

		return
			exception is not null
				? XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [ctxt.TestCase], exception, sendTestCaseMessages: false)
				: await XunitTestRunner.Instance.RunAsync(
					CreateTest(ctxt, ctxt.TestCase.TestMethod, @explicit: null, displayName: null, testIndex: 0, ctxt.TestCase.Traits, ctxt.TestCase.Timeout, ctxt.TestMethodArguments),
					ctxt.MessageBus,
					ctxt.ConstructorArguments,
					ctxt.SkipReason,
					ctxt.ExplicitOption,
					ctxt.Aggregator.Clone(),
					ctxt.CancellationTokenSource,
					ctxt.BeforeAfterTestAttributes
				);
	}
}
