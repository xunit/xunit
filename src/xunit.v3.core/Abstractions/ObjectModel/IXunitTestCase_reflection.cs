using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Represents a single test case from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestCase : ICoreTestCase
{
	/// <summary>
	/// When set, indicates the type to use when resolving <see cref="SkipUnless"/> or
	/// <see cref="SkipWhen"/>. If not set, uses the test class type.
	/// </summary>
	Type? SkipType { get; }

	/// <summary>
	/// When set, indicates a public static property that is used at runtime to determine
	/// whether the test is skipped or not (<see langword="true"/> to run, <see langword="false"/> to skip).
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<see langword="null"/> value.
	/// </remarks>
	string? SkipUnless { get; }

	/// <summary>
	/// When set, indicates a public static property that is used at runtime to determine
	/// whether the test is skipped or not (<see langword="false"/> to run, <see langword="true"/> to skip).
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<see langword="null"/> value.
	/// </remarks>
	string? SkipWhen { get; }

	/// <summary>
	/// Gets the test class that this test case belongs to.
	/// </summary>
	new IXunitTestClass TestClass { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to.
	/// </summary>
	new IXunitTestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to.
	/// </summary>
	new IXunitTestMethod TestMethod { get; }

	/// <summary>
	/// Gets the <see cref="MemberInfo.MetadataToken"/> for the test method.
	/// </summary>
	new int TestMethodMetadataToken { get; }

	/// <summary>
	/// Gets the types for the test method parameters.
	/// </summary>
	/// <remarks>
	/// The values here are formatted according to
	/// <see href="https://github.com/microsoft/vstest/blob/main/docs/RFCs/0017-Managed-TestCase-Properties.md">VSTest rules</see>
	/// in order to support Test Explorer. Note that this is not the same as <see cref="Type.FullName"/>.
	/// </remarks>
	new string[] TestMethodParameterTypesVSTest { get; }

	/// <summary>
	/// Gets the test method return type.
	/// </summary>
	/// <remarks>
	/// The value here is formatted according to
	/// <see href="https://github.com/microsoft/vstest/blob/main/docs/RFCs/0017-Managed-TestCase-Properties.md">VSTest rules</see>
	/// in order to support Test Explorer. Note that this is not the same as <see cref="Type.FullName"/>.
	/// </remarks>
	new string TestMethodReturnTypeVSTest { get; }

	/// <summary>
	/// Creates the tests that are emitted from this test case. Exceptions thrown here
	/// will be caught and converted into a test case failure.
	/// </summary>
	ValueTask<IReadOnlyCollection<IXunitTest>> CreateTests();
}
