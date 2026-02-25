// Since we want to seal FactAttribute and friends (as deriving from it is no longer an extensibility
// point), we create this "public" base class (with an internal constructor, so it can't be derived from
// outside of this assembly) with the properties from FactAttribute, and hide it away here in Xunit.Internal.

using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class FactAttributeBase : Attribute
{
	internal FactAttributeBase()
	{ }

	/// <summary>
	/// Gets the name of the test to be used when the test is skipped. When <see langword="null"/>
	/// is returned, will cause a default display name to be used.
	/// </summary>
	public string? DisplayName { get; set; }

	/// <summary>
	/// Gets a flag which indicates whether the test should only be run explicitly.
	/// An explicit test is skipped by default unless explicit tests are requested
	/// to be run.
	/// </summary>
	public bool Explicit { get; set; }

	/// <summary>
	/// Gets the skip reason for the test. When <see langword="null"/> is returned, the test is
	/// not skipped.
	/// </summary>
	/// <remarks>
	/// Skipping is conditional based on whether <see cref="SkipWhen"/> or <see cref="SkipUnless"/>
	/// is set.
	/// </remarks>
	public string? Skip { get; set; }

	/// <summary>
	/// Gets exceptions that, when thrown, will cause the test to be skipped rather than failed.
	/// </summary>
	/// <remarks>
	/// The skip reason will be the exception's message.
	/// </remarks>
	public Type[]? SkipExceptions { get; set; }

	/// <summary>
	/// Gets the type to retrieve <see cref="SkipUnless"/> or <see cref="SkipWhen"/> from. If not set,
	/// then the property will be retrieved from the unit test class.
	/// </summary>
	public Type? SkipType { get; set; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <see cref="bool"/>
	/// to indicate whether the test should be skipped (<see langword="false"/>) or not (<see langword="true"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipWhen"/> is set. Setting both will
	/// result in a failed test.<br />
	/// <br />
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator,
	/// e.g., <c>SkipUnless = nameof(IsConditionMet)</c>.
	/// </remarks>
	public string? SkipUnless { get; set; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <see cref="bool"/>
	/// to indicate whether the test should be skipped (<see langword="true"/>) or not (<see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipUnless"/> is set. Setting both will
	/// result in a failed test.<br />
	/// <br />
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator,
	/// e.g., <c>SkipWhen = nameof(IsConditionMet)</c>.
	/// </remarks>
	public string? SkipWhen { get; set; }

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
	public int Timeout { get; set; }
}
