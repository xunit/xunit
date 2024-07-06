using Xunit;
using Xunit.v3;

public class CollectionPerAssemblyTestCollectionFactoryTests
{
	[Fact]
	public void DefaultCollectionIsAssemblyCollection()
	{
		var testAssembly = Mocks.XunitTestAssembly(assemblyName: "my-test-assembly");
		var factory = new CollectionPerAssemblyTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(NoAttributes));

		Assert.Empty(testCollection.BeforeAfterTestAttributes);
		Assert.Empty(testCollection.ClassFixtureTypes);
		Assert.Null(testCollection.CollectionDefinition);
		Assert.Empty(testCollection.CollectionFixtureTypes);
		Assert.False(testCollection.DisableParallelization);
		Assert.Same(testAssembly, testCollection.TestAssembly);
		Assert.Null(testCollection.TestCaseOrderer);
		Assert.Null(testCollection.TestCollectionClassName);
		Assert.Equal("Test collection for my-test-assembly", testCollection.TestCollectionDisplayName);
		Assert.Equal("bc8d5a81006b98388f52ab91adad7269422924b2c94a3fbfc8f76efe83574de1", testCollection.UniqueID);
	}

	class NoAttributes { }
}
