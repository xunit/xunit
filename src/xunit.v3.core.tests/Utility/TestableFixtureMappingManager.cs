namespace Xunit.v3;

public class TestableFixtureMappingManager : FixtureMappingManager
{
	public TestableFixtureMappingManager(FixtureMappingManager parent) :
		base("Testable", parent)
	{ }

	public TestableFixtureMappingManager(params object[] cachedFixtureValues) :
		base("Testable", cachedFixtureValues)
	{ }
}
