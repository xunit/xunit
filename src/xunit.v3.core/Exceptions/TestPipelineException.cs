using System;
using System.Runtime.Serialization;

// This lives in Xunit.Sdk instead of Xunit.v3 because our message filter will only simplify exception
// names in the "Xunit.Sdk" namespace. See ExceptionUtility.GetMessage for more information.
namespace Xunit.Sdk;

/// <summary>
/// Represents an exception that happened during the processing of the test pipeline. This typically
/// means there were problems identifying the correct test class constructor, problems creating the
/// fixture data, etc.
/// </summary>
[Serializable]
public class TestPipelineException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestPipelineException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public TestPipelineException(string message)
		: base(message)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TestPipelineException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception that is being reported.</param>
	public TestPipelineException(
		string message,
		Exception innerException)
			: base(message, innerException)
	{ }

	/// <inheritdoc/>
	protected TestPipelineException(
		SerializationInfo info,
		StreamingContext context)
			: base(info, context)
	{ }
}
