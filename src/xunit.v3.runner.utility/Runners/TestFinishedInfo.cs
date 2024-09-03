using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that finished, regardless of the result.
/// </summary>
public class TestFinishedInfo(
	string typeName,
	string methodName,
	Dictionary<string, HashSet<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName,
	decimal executionTime,
	string? output) :
		TestExecutedInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName, executionTime, output)
{ }
