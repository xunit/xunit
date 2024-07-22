using System;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test is about to start executing.
/// </summary>
public interface ITestStarting : ITestMessage, ITestMetadata
{
	/// <summary>
	/// Gets a flag which indicates whether the test is marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the date and time when the test execution began.
	/// </summary>
	DateTimeOffset StartTime { get; }

	/// <summary>
	/// Gets the timeout for the test, in milliseconds; if <c>0</c>, there is no timeout.
	/// </summary>
	int Timeout { get; }
}
