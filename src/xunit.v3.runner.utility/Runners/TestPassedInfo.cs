using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that passed.
/// </summary>
public class TestPassedInfo(
	string typeName,
	string methodName,
	Dictionary<string, HashSet<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName,
	decimal executionTime,
	string? output) :
		TestExecutedInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName, executionTime, output)
{ }
