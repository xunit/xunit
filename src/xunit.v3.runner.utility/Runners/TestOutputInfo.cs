namespace Xunit.Runners;

/// <summary>
/// Represents live test output.
/// </summary>
[Obsolete("Please use the TestOutputInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class TestOutputInfo(
	string typeName,
	string methodName,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
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
