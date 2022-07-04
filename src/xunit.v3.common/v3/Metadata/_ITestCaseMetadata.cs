using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.v3;

/// <summary>
/// Represents metadata about a test case.
/// </summary>
public interface _ITestCaseMetadata
{
	/// <summary>
	/// Gets the display text for the reason a test is being skipped; if the test
	/// is not skipped, returns <c>null</c>.
	/// </summary>
	string? SkipReason { get; }

	/// <summary>
	/// Gets the source file name. A <c>null</c> value indicates that the
	/// source file name is not known.
	/// </summary>
	string? SourceFilePath { get; }

	/// <summary>
	/// Gets the source file line number. A <c>null</c> value indicates that the
	/// source file line number is not known.
	/// </summary>
	int? SourceLineNumber { get; }

	/// <summary>
	/// Gets the display name of the test case.
	/// </summary>
	string TestCaseDisplayName { get; }

	/// <summary>
	/// Gets the name of the class where the test is defined. If the test did not originiate
	/// in a class, will return <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethodName))]
	public string? TestClassName { get; }

	/// <summary>
	/// Gets the namespace of the class where the test is defined. If the test did not
	/// originate in a class, or the class it originated in does not reside in a namespace,
	/// will return <c>null</c>.
	/// </summary>
	public string? TestClassNamespace { get; }

	/// <summary>
	/// Gets the fully qualified type name (without assembly) of the class where the test is defined.
	/// If the test did not originiate in a class, will return <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestClassName))]
	public string? TestClassNameWithNamespace { get; }

	/// <summary>
	/// Gets the method name where the test is defined, in the <see cref="TestClassName"/> class.
	/// If the test did not originiate in a method, will return <c>null</c>.
	/// </summary>
	public string? TestMethodName { get; }

	/// <summary>
	/// Gets the trait values associated with this test case. If
	/// there are none, or the framework does not support traits,
	/// this should return an empty dictionary (not <c>null</c>).
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }
}
