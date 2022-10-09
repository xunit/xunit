using System;
using System.Runtime.Serialization;

namespace Xunit.v3;

/// <summary>
/// Represents an exception that happened during the processing of a test class. This typically
/// means there were problems identifying the correct test class constructor, or problems
/// creating the fixture data for the test class.
/// </summary>
[Serializable]
public class TestClassException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestClassException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public TestClassException(string message)
		: base(message)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TestClassException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception that is being reported.</param>
	public TestClassException(
		string message,
		Exception innerException)
			: base(message, innerException)
	{ }

	/// <inheritdoc/>
	protected TestClassException(
		SerializationInfo info,
		StreamingContext context)
			: base(info, context)
	{ }
}
