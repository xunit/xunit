using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test case from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is shared between reflection-based and code generation-based tests.
/// </remarks>
public interface ICoreTestCase : ITestCase
{
	/// <summary>
	/// Gets the arity (number of generic types) on the method itself.
	/// </summary>
	new int TestMethodArity { get; }

	/// <summary>
	/// Gets the exception types that, when thrown, will cause the test to be skipped rather than failed.
	/// </summary>
	Type[]? SkipExceptions { get; }

	/// <summary>
	/// Gets the display text for the reason a test that might being skipped.
	/// </summary>
	/// <remarks>
	/// This differs from the contract of <see cref="ITestCaseMetadata.SkipReason"/> by virtue
	/// of the fact that when this value is non-<see langword="null"/>, it may indicate that a test is
	/// statically skipped (if both <c>SkipUnless</c> and <c>SkipWhen</c> are
	/// <see langword="null"/>) or dynamically skipped (if one is non-<see langword="null"/>).
	/// </remarks>
	new string? SkipReason { get; }

	/// <summary>
	/// Gets the test class that this test case belongs to.
	/// </summary>
	new ICoreTestClass TestClass { get; }

	/// <summary>
	/// Gets the full name of the class where the test is defined (i.e. <see cref="Type.FullName"/>).
	/// </summary>
	new string TestClassName { get; }

	/// <summary>
	/// Gets the simple name of the class where the test is defined (i.e. <see cref="MemberInfo.Name"/>).
	/// </summary>
	new string TestClassSimpleName { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to.
	/// </summary>
	new ICoreTestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to.
	/// </summary>
	new ICoreTestMethod TestMethod { get; }

	/// <summary>
	/// Gets the method name where the test is defined.
	/// </summary>
	new string TestMethodName { get; }

	/// <summary>
	/// Gets the timeout of the test, in seconds; if zero or negative, means the test case has no timeout.
	/// </summary>
	int Timeout { get; }

	/// <summary>
	/// Allows the test case to run some code just after the test case is finished running.
	/// </summary>
	void PostInvoke();

	/// <summary>
	/// Allows the test case to run some code just before the test case is run.
	/// </summary>
	void PreInvoke();
}
