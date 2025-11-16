using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The base test method runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public class XunitTestMethodRunnerBase<TContext, TTestMethod, TTestCase> :
	TestMethodRunner<TContext, TTestMethod, TTestCase>
		where TContext : XunitTestMethodRunnerBaseContext<TTestMethod, TTestCase>
		where TTestMethod : class, IXunitTestMethod
		where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Orders the test cases using the test case orderer from
	/// <see cref="XunitTestMethodRunnerBaseContext{TTestMethod, TTestCase}.TestCaseOrderer"/>.
	/// If the user hasn't provided an override, the default orderer is <see cref="DefaultTestCaseOrderer"/>,
	/// which orders them in an unpredictable but stable order based on their unique ID.
	/// </summary>
	/// <inheritdoc/>
	protected override IReadOnlyCollection<TTestCase> OrderTestCases(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		try
		{
			return ctxt.TestCaseOrderer.OrderTestCases(ctxt.TestCases);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			ctxt.MessageBus.QueueMessage(new ErrorMessage
			{
				ExceptionParentIndices = [-1],
				ExceptionTypes = [typeof(TestPipelineException).SafeName()],
				Messages = [
					string.Format(
						CultureInfo.CurrentCulture,
						"Test case orderer '{0}' threw '{1}' during ordering: {2}",
						ctxt.TestCaseOrderer.GetType().SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message
					)
				],
				StackTraces = [innerEx.StackTrace],
			});

			return [];
		}
	}

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		TTestCase testCase)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCase);

		if (testCase is ISelfExecutingXunitTestCase selfExecutingTestCase)
			return selfExecutingTestCase.Run(ctxt.ExplicitOption, ctxt.MessageBus, ctxt.ConstructorArguments, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);

		return XunitRunnerHelper.RunXunitTestCase(
			testCase,
			ctxt.MessageBus,
			ctxt.CancellationTokenSource,
			ctxt.Aggregator.Clone(),
			ctxt.ExplicitOption,
			ctxt.ConstructorArguments
		);
	}
}
