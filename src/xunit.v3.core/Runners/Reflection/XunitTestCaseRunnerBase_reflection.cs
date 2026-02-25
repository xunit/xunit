namespace Xunit.v3;

/// <summary>
/// Test case runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public abstract class XunitTestCaseRunnerBase<TContext, TTestCase, TTest> :
	CoreTestCaseRunner<TContext, TTestCase, TTest>
		where TContext : XunitTestCaseRunnerBaseContext<TTestCase, TTest>
		where TTestCase : class, IXunitTestCase
		where TTest : class, IXunitTest
{ }
