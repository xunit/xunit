using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents live test output.
/// </summary>
public class TestOutputInfo(
	string typeName,
	string methodName,
	Dictionary<string, HashSet<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName,
	string? output) :
		TestInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
{
	/// <summary>
	/// The output from the test.
	/// </summary>
	public string? Output { get; } = output;
}
