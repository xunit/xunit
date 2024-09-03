using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that is starting.
/// </summary>
public class TestStartingInfo(
	string typeName,
	string methodName,
	Dictionary<string, HashSet<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName) :
		TestInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
{ }
