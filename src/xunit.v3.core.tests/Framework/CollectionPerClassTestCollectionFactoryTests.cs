using Xunit;
using Xunit.v3;

public class CollectionPerClassTestCollectionFactoryTests
{
	[Fact]
	public void DefaultCollectionIsClassCollection()
	{
		var testAssembly = Mocks.XunitTestAssembly();
		var factory = new CollectionPerClassTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(NoAttributes));

		Assert.Empty(testCollection.BeforeAfterTestAttributes);
		Assert.Empty(testCollection.ClassFixtureTypes);
		Assert.Null(testCollection.CollectionDefinition);
		Assert.Empty(testCollection.CollectionFixtureTypes);
		Assert.False(testCollection.DisableParallelization);
		Assert.Same(testAssembly, testCollection.TestAssembly);
		Assert.Null(testCollection.TestCaseOrderer);
		Assert.Null(testCollection.TestCollectionClassName);
#if BUILD_X86
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: 9ac2955700bba5c2c1f3339038d7b852f51df7caf9f0120872a5c8790269d0fd)", testCollection.TestCollectionDisplayName);
		Assert.Equal("148a06e0297542c9f312f0283d430d1bba2b0a507c1d8a34d5922005ce91f096", testCollection.UniqueID);
#else
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: 15ba074ef867ff586a13be3833053c74a0cb38e99106e598cc5d99dcdfa8ecc7)", testCollection.TestCollectionDisplayName);
		Assert.Equal("9bdf912a195340f6895be236979b92cfba48f86658a699c70304ba007c111163", testCollection.UniqueID);
#endif
	}

	class NoAttributes { }
}
