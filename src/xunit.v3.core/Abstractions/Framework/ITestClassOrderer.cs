using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A class implements this interface to participate in ordering tests for the test runner.
/// Test class orderers are applied using an implementation of <see cref="ITestClassOrdererAttribute"/>
/// (most commonly <see cref="TestClassOrdererAttribute"/>), which can be applied at the assembly and
/// test collection level.
/// </summary>
public interface ITestClassOrderer
{
	/// <summary>
	/// Orders test classes for execution.
	/// </summary>
	/// <param name="testClasses">The test classes to be ordered.</param>
	/// <returns>The test classes in the order to be run.</returns>
	IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
		where TTestClass : notnull, ITestClass;
}
