using Xunit;
using Xunit.v3;

public class CollectionPerClassTestCollectionFactoryTests
{
	[Fact]
	public void DefaultCollectionIsClassCollection()
	{
		var testAssembly = Mocks.CodeGenTestAssembly();
		var factory = new CollectionPerClassTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(NoAttributes));

		Assert.Empty(testCollection.BeforeAfterTestAttributes);
		Assert.Empty(testCollection.ClassFixtureFactories);
		Assert.Empty(testCollection.CollectionFixtureFactories);
		Assert.False(testCollection.DisableParallelization);
		Assert.Same(testAssembly, testCollection.TestAssembly);
		Assert.Null(testCollection.TestCaseOrderer);
		Assert.Null(testCollection.TestClassOrderer);
		Assert.Null(testCollection.TestCollectionClassName);
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: 2c85dafcd105124ca9a080cfa3a60cb795f23a1ab552499d776a422223f2281e)", testCollection.TestCollectionDisplayName);
		Assert.Null(testCollection.TestMethodOrderer);
		Assert.Equal(TestData.DefaultTraits, testCollection.Traits);
		Assert.Equal("4005eee7875a1ee25c7c48347b7dabf0a0bb2e043fb194f90039010c721e6832", testCollection.UniqueID);
	}

	class NoAttributes { }
}
