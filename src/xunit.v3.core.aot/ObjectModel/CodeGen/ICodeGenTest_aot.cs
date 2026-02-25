namespace Xunit.v3;

/// <summary>
/// Represents a test from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for code generation-based tests.
/// </remarks>
public interface ICodeGenTest : ICoreTest
{
	/// <summary>
	/// Gets the invoker which will call the test method.
	/// </summary>
	Func<object?, ValueTask> MethodInvoker { get; }

	/// <summary>
	/// Gets a skip reason for this test.
	/// </summary>
	/// <remarks>
	/// This value may not line up the with <see cref="ICoreTestCase.SkipReason"/>, as you can skip
	/// individual data rows during delay enumeration.
	/// </remarks>
	string? SkipReason { get; }

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
	/// Gets the test case this test belongs to.
	/// </summary>
	new ICodeGenTestCase TestCase { get; }
}
