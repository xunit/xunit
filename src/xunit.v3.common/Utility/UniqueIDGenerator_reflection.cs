namespace Xunit.Sdk;

partial class UniqueIDGenerator
{
	/// <summary>
	/// Computes a unique ID for a test case.
	/// </summary>
	/// <param name="parentUniqueID">The unique ID of the parent in the hierarchy; typically the test method
	/// unique ID, but may also be the test class or test collection unique ID, when test method (and
	/// possibly test class) don't exist.</param>
	/// <param name="testMethodGenericTypes">The test method's generic types</param>
	/// <param name="testMethodArguments">The test method's arguments</param>
	/// <returns>The computed unique ID for the test case</returns>
	public static string ForTestCase(
		string parentUniqueID,
		Type[]? testMethodGenericTypes,
		object?[]? testMethodArguments)
	{
		Guard.ArgumentNotNull(parentUniqueID);

		using var generator = new UniqueIDGenerator();

		generator.Add(parentUniqueID);

		if (testMethodArguments is not null)
			generator.Add(SerializationHelper.Instance.Serialize(testMethodArguments));

		if (testMethodGenericTypes is not null)
			for (var idx = 0; idx < testMethodGenericTypes.Length; idx++)
				generator.Add(testMethodGenericTypes[idx].SafeName());

		return generator.Compute();
	}
}
