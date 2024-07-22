using System;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the execution process has been completed for
/// the requested assembly.
/// </summary>
public interface ITestAssemblyFinished : ITestAssemblyMessage, IExecutionSummaryMetadata
{
	/// <summary>
	/// Gets the date and time when the test assembly execution finished.
	/// </summary>
	DateTimeOffset FinishTime { get; }
}
