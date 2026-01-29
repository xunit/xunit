namespace Xunit.Runners;

/// <summary>
/// Represents a test that is starting.
/// </summary>
[Obsolete("Please use the TestStartingInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class TestStartingInfo(
	string typeName,
	string methodName,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName) :
		TestInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
{ }
