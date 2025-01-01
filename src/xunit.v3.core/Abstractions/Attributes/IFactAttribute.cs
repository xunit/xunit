using System;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a test method that should
/// be run by the default test runner. Implementations must be decorated by
/// <see cref="XunitTestCaseDiscovererAttribute"/> to indicate which class is responsible
/// for converting the test method into one or more tests.
/// </summary>
/// <remarks>The attribute can only be applied to methods, and only one attribute is allowed.</remarks>
public interface IFactAttribute
{
	/// <summary>
	/// Gets the name of the test to be used when the test is skipped. When <c>null</c>
	/// is returned, will cause a default display name to be used.
	/// </summary>
	string? DisplayName { get; }

	/// <summary>
	/// Gets a flag which indicates whether the test should only be run explicitly.
	/// An explicit test is skipped by default unless explicit tests are requested
	/// to be run.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the skip reason for the test. When <c>null</c> is returned, the test is
	/// not skipped.
	/// </summary>
	/// <remarks>
	/// Skipping is conditional based on whether <see cref="SkipWhen"/> or <see cref="SkipUnless"/>
	/// is set.
	/// </remarks>
	string? Skip { get; }

	/// <summary>
	/// Gets exceptions that, when thrown, will cause the test to be skipped rather than failed.
	/// </summary>
	/// <remarks>
	/// The skip reason will be the exception's mesage.
	/// </remarks>
	Type[]? SkipExceptions { get; }

	/// <summary>
	/// Gets the type to retrieve <see cref="SkipUnless"/> or <see cref="SkipWhen"/> from. If not set,
	/// then the property will be retrieved from the unit test class.
	/// </summary>
	Type? SkipType { get; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <c>bool</c>
	/// to indicate whether the test should be skipped (<c>false</c>) or not (<c>true</c>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipWhen"/> is set. Setting both will
	/// result in a failed test.
	/// To ensure compile-time safety and easier refactoring, use the <c>nameof</c> operator,
	/// e.g., <c>SkipUnless = nameof(IsConditionMet)</c>.
	/// </remarks>
	string? SkipUnless { get; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <c>bool</c>
	/// to indicate whether the test should be skipped (<c>true</c>) or not (<c>false</c>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipUnless"/> is set. Setting both will
	/// result in a failed test.
	/// To avoid issues during refactoring, it is recommended to use the <c>nameof</c> operator
	/// to reference the property, e.g., <c>SkipWhen = nameof(IsTestSkipped)</c>.
	/// </remarks>
	string? SkipWhen { get; }

	/// <summary>
	/// Gets the timeout for test (in milliseconds). When <c>0</c> is returned, the test
	/// will not have a timeout.
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with <see cref="ParallelAlgorithm.Aggressive"/> will result
	/// in undefined behavior. Test timing and timeouts are only reliable when using
	/// <see cref="ParallelAlgorithm.Conservative"/> (or when parallelization is disabled
	/// completely).
	/// </remarks>
	int Timeout { get; }
}
