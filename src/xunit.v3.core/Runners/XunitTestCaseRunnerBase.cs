namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public abstract class XunitTestCaseRunnerBase<TContext, TTestCase, TTest> :
	TestCaseRunner<XunitTestCaseRunnerContext, IXunitTestCase, IXunitTest>
		where TContext : XunitTestCaseRunnerBaseContext<TTestCase, TTest>
		where TTestCase : class, IXunitTestCase
		where TTest : class, IXunitTest
{ }
