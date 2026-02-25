namespace Xunit.v3;

/// <summary>
/// Represents a test case from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for code generation-based tests.
/// </remarks>
public interface ICodeGenTestCase : ICoreTestCase
{
	/// <summary>
	/// If this is set and it returns <see langword="false" />, the test will be skipped.
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<see langword="null"/> value.
	/// </remarks>
	Func<bool>? SkipUnless { get; }

	/// <summary>
	/// If this is set and it returns <see langword="true" />, the test will be skipped.
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<see langword="null"/> value.
	/// </remarks>
	Func<bool>? SkipWhen { get; }

	/// <summary>
	/// Gets the test class that this test case belongs to.
	/// </summary>
	new ICodeGenTestClass TestClass { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to.
	/// </summary>
	new ICodeGenTestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to.
	/// </summary>
	new ICodeGenTestMethod TestMethod { get; }

	/// <summary>
	/// Creates the tests that are emitted from this test case. Exceptions thrown here
	/// will be caught and converted into a test case failure.
	/// </summary>
	ValueTask<IReadOnlyCollection<ICodeGenTest>> CreateTests();
}
