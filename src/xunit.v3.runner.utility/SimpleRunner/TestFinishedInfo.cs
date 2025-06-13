using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.SimpleRunner;

/// <summary>
/// Represents information about a test that was finished.
/// </summary>
public abstract class TestFinishedInfo : TestStartingInfo
{
	/// <summary>
	/// Gets any attachments that were added to the test result.
	/// </summary>
	/// <remarks>
	/// Test attachments are only supported by v3 test projects.
	/// </remarks>
	public required IReadOnlyDictionary<string, TestAttachment> Attachments { get; set; }

	/// <summary>
	/// Gets the number of seconds the test spent executing.
	/// </summary>
	public required decimal ExecutionTime { get; set; }

	/// <summary>
	/// Gets the date and time when the test execution finished.
	/// </summary>
	public required DateTimeOffset FinishTime { get; set; }

	/// <summary>
	/// Gets the output from the test.
	/// </summary>
	public required string Output { get; set; }

	/// <summary>
	/// Gets the warnings that occurred during execution.
	/// </summary>
	public required string[] Warnings { get; set; }
}
