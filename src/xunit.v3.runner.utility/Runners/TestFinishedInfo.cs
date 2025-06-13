using System;
using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that finished, regardless of the result.
/// </summary>
[Obsolete("Please use the TestFinishedInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class TestFinishedInfo(
	string typeName,
	string methodName,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName,
	decimal executionTime,
	string? output) :
		TestExecutedInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName, executionTime, output)
{ }
