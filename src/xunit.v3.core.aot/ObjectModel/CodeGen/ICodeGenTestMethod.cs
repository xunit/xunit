namespace Xunit.v3;

/// <summary>
/// Represents a test method from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for code generation-based tests.
/// </remarks>
public interface ICodeGenTestMethod : ICoreTestMethod
{
	/// <summary>
	/// Gets the <see cref="BeforeAfterTestAttribute"/>s attached to the test method (and the test class,
	/// test collection, and test assembly).
	/// </summary>
	IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the declared type index for the test method. Will return <see langword="null"/> if the test
	/// method resides on the same type as the test class.
	/// </summary>
	string? DeclaredTypeIndex { get; }

	/// <summary>
	/// Gets a flag which indicates if the test method is static.
	/// </summary>
	/// <remarks>
	/// When this returns <see langword="true"/>, creation of the test class will be skipped.
	/// </remarks>
	bool IsStatic { get; }

	/// <summary>
	/// Gets the source file where the method lives, if known.
	/// </summary>
	string? SourceFilePath { get; }

	/// <summary>
	/// Gets the line number in <see cref="SourceFilePath"/> where the method lives, if known.
	/// </summary>
	int? SourceLineNumber { get; }

	/// <summary>
	/// Gets the test class that this test method belongs to.
	/// </summary>
	new ICodeGenTestClass TestClass { get; }
}
