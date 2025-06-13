using System;
using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents information about a test that was executed.
/// </summary>
[Obsolete("Please use the TestFinishedInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public abstract class TestExecutedInfo(
	string typeName,
	string methodName,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName,
	decimal executionTime,
	string? output) :
		TestInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
{
	/// <summary>
	/// The number of seconds the test spent executing.
	/// </summary>
	public decimal ExecutionTime { get; } = executionTime;

	/// <summary>
	/// The output from the test.
	/// </summary>
	public string Output { get; } = output ?? string.Empty;
}
