using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A class implements this interface to participate in ordering tests for the test runner.
/// Test method orderers are applied using an implementation of <see cref="ITestMethodOrdererAttribute"/>
/// (most commonly <see cref="TestMethodOrdererAttribute"/>), which can be applied at the assembly,
/// test collection, and test class level.
/// </summary>
public interface ITestMethodOrderer
{
	/// <summary>
	/// Orders test methods for execution.
	/// </summary>
	/// <param name="testMethods">The test methods to be ordered.</param>
	/// <returns>The test methods in the order to be run.</returns>
	IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
		where TTestMethod : notnull, ITestMethod;
}
