using System;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTest : ITest
{
	/// <summary>
	/// Gets a flag indicating whether this test was marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets a skip reason for this test.
	/// </summary>
	/// <remarks>
	/// This value may not line up the with <see cref="IXunitTestCase.SkipReason"/>, as you can skip
	/// individual data rows during delay enumeration.
	/// </remarks>
	string? SkipReason { get; }

	/// <summary>
	/// When set, indicates the type to use when resolving <see cref="SkipUnless"/> or
	/// <see cref="SkipWhen"/>. If not set, uses the test class type.
	/// </summary>
	/// <remarks>
	/// This value may not line up the with <see cref="IXunitTestCase.SkipType"/>, as you can skip
	/// individual data rows during delay enumeration.
	/// </remarks>
	Type? SkipType { get; }

	/// <summary>
	/// When set, indicates a public static property that is used at runtime to determine
	/// whether the test is skipped or not (<see langword="true"/> to run, <see langword="false"/> to skip).
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<see langword="null"/> value.<br />
	/// <br />
	/// This value may not line up the with <see cref="IXunitTestCase.SkipUnless"/>, as you can skip
	/// individual data rows during delay enumeration.
	/// </remarks>
	string? SkipUnless { get; }

	/// <summary>
	/// When set, indicates a public static property that is used at runtime to determine
	/// whether the test is skipped or not (<see langword="false"/> to run, <see langword="true"/> to skip).
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<see langword="null"/> value.
	/// <br />
	/// This value may not line up the with <see cref="IXunitTestCase.SkipWhen"/>, as you can skip
	/// individual data rows during delay enumeration.
	/// </remarks>
	string? SkipWhen { get; }

	/// <summary>
	/// Gets the test case this test belongs to.
	/// </summary>
	new IXunitTestCase TestCase { get; }

	/// <summary>
	/// Gets the test method to run. May different from the test method embedded in the test case.
	/// </summary>
	IXunitTestMethod TestMethod { get; }

	/// <summary>
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[] TestMethodArguments { get; }

	/// <summary>
	/// Gets the timeout for the test, in milliseconds; if <c>0</c>, there is no timeout.
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with <see cref="ParallelAlgorithm.Aggressive"/> will result
	/// in undefined behavior. Timeout is only supported by <see cref="ParallelAlgorithm.Conservative"/>
	/// (or when parallelization is disabled completely).
	/// </remarks>
	int Timeout { get; }
}
