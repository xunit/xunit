using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A simple implementation of <see cref="IXunitTestCase"/> that can be used to report an error
/// rather than running a test.
/// </summary>
public class ExecutionErrorTestCase : XunitTestCase
{
	string? errorMessage;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public ExecutionErrorTestCase()
	{ }

	/// <summary>
	/// Please use <see cref="ExecutionErrorTestCase(IXunitTestMethod, string, string, string?, int?, string)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	[Obsolete("Please use the constructor which accepts sourceFilePath and sourceLineNumber. This overload will be removed in the next major version.")]
	public ExecutionErrorTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		string errorMessage) :
			this(testMethod, testCaseDisplayName, uniqueID, null, null, errorMessage)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionErrorTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The unique ID for the test case.</param>
	/// <param name="sourceFilePath">The source filename, if known</param>
	/// <param name="sourceLineNumber">The source line number, if known</param>
	/// <param name="errorMessage">The error message to report for the test.</param>
	public ExecutionErrorTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		string? sourceFilePath,
		int? sourceLineNumber,
		string errorMessage) :
			base(testMethod, testCaseDisplayName, uniqueID, @explicit: false, sourceFilePath: sourceFilePath, sourceLineNumber: sourceLineNumber) =>
				this.errorMessage = Guard.ArgumentNotNull(errorMessage);

	/// <summary>
	/// Gets the error message that will be displayed when the test is run.
	/// </summary>
	public string ErrorMessage =>
		this.ValidateNullablePropertyValue(errorMessage, nameof(ErrorMessage));

	/// <summary>
	/// Throws the expected error mesage rather than creating tests.
	/// </summary>
	public override ValueTask<IReadOnlyCollection<IXunitTest>> CreateTests() =>
		throw new TestPipelineException(ErrorMessage);

	/// <inheritdoc/>
	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		errorMessage = Guard.NotNull("Could not retrieve ErrorMessage from serialization", info.GetValue<string>("em"));
	}

	/// <inheritdoc/>
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("em", ErrorMessage);
	}
}
