using System;
using System.Runtime.Serialization;

namespace Xunit.v3;

/// <summary>
/// Represents an exception that happened during cleanup of a test fixture.
/// </summary>
[Serializable]
public class TestFixtureCleanupException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestFixtureCleanupException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception that is being reported.</param>
	public TestFixtureCleanupException(
		string message,
		Exception innerException)
			: base(message, innerException)
	{ }

	/// <inheritdoc/>
	protected TestFixtureCleanupException(
		SerializationInfo info,
		StreamingContext context)
			: base(info, context)
	{ }
}
