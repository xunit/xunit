using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base test method runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public class CoreTestMethodRunner<TContext, TTestMethod, TTestCase> : TestMethodRunner<TContext, TTestMethod, TTestCase>
	where TContext : CoreTestMethodRunnerContext<TTestMethod, TTestCase>
	where TTestMethod : class, ICoreTestMethod
	where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Orders the test cases using the first available orderer from:
	/// <list type="bullet">
	/// <item><see cref="ICoreTestMethod.TestCaseOrderer"/></item>
	/// <item><see cref="ICoreTestClass.TestCaseOrderer"/></item>
	/// <item><see cref="ICoreTestCollection.TestCaseOrderer"/></item>
	/// <item><see cref="ICoreTestAssembly.TestCaseOrderer"/></item>
	/// <item><see cref="DefaultTestClassOrderer"/></item>
	/// </list>
	/// </summary>
	/// <inheritdoc/>
	protected override IReadOnlyCollection<TTestCase> OrderTestCases(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCaseOrderer =
			ctxt.TestMethod.TestCaseOrderer
				?? ctxt.TestMethod.TestClass.TestCaseOrderer
				?? ctxt.TestMethod.TestClass.TestCollection.TestCaseOrderer
				?? ctxt.TestMethod.TestClass.TestCollection.TestAssembly.TestCaseOrderer
				?? DefaultTestCaseOrderer.Instance;

		try
		{
			return testCaseOrderer.OrderTestCases(ctxt.TestCases);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Test case orderer '{0}' threw during ordering",
					testCaseOrderer.GetType().SafeName()
				),
				innerEx
			);
		}
	}

	/// <summary>
	/// Runs the test case via the context.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		TTestCase testCase) =>
			Guard.ArgumentNotNull(ctxt).RunTestCase(testCase);
}
