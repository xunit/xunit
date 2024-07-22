using System;

namespace Xunit.Sdk;

/// <summary>
/// Base interface for all individual test results (e.g., tests which pass, fail, skipped, or aren't run).
/// </summary>
public interface ITestResultMessage : IMessageSinkMessage, ITestMessage, IExecutionMetadata
{
	/// <summary>
	/// Gets the date and time when the test execution finished.
	/// </summary>
	DateTimeOffset FinishTime { get; }
}
