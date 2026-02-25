namespace Xunit.v3;

/// <summary>
/// Test method runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunnerBase<TContext, TTestMethod, TTestCase> :
	CoreTestMethodRunner<TContext, TTestMethod, TTestCase>
		where TContext : XunitTestMethodRunnerBaseContext<TTestMethod, TTestCase>
		where TTestMethod : class, IXunitTestMethod
		where TTestCase : class, IXunitTestCase
{ }
