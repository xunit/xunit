using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CollectionPerAssemblyTestCollectionFactoryTests
{
	[Fact]
	public static void DefaultCollectionIsAssemblyCollection()
	{
		var testAssembly = Mocks.CodeGenTestAssembly(assemblyName: "my-test-assembly");
		var factory = new CollectionPerAssemblyTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(NoAttributes));

		Assert.Empty(testCollection.BeforeAfterTestAttributes);
		Assert.Empty(testCollection.ClassFixtureFactories);
		Assert.Empty(testCollection.CollectionFixtureFactories);
		Assert.Equal(ParallelismOptionsAliases.Default, testCollection.ParallelismOptions);
		Assert.Same(testAssembly, testCollection.TestAssembly);
		Assert.Null(testCollection.TestCaseOrderer);
		Assert.Null(testCollection.TestClassOrderer);
		Assert.Null(testCollection.TestCollectionClassName);
		Assert.Equal("Test collection for my-test-assembly", testCollection.TestCollectionDisplayName);
		Assert.Null(testCollection.TestMethodOrderer);
		Assert.Equal(TestData.DefaultTraits, testCollection.Traits);
		Assert.Equal("bc8d5a81006b98388f52ab91adad7269422924b2c94a3fbfc8f76efe83574de1", testCollection.UniqueID);
	}

	class NoAttributes { }
}
