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
#if BUILD_X86 && NETFRAMEWORK
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: cad8434edaa657e7f71258f4c968d14dc874d68ef9531d04f265430426d95d48)", testCollection.TestCollectionDisplayName);
		Assert.Equal("00f615915ef801a49a09c8a5ad40a7e4172c284b992aa0f77247749a73e27968", testCollection.UniqueID);
#elif BUILD_X86
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: 7cfee15dbd8d60bf8c21dd09bd1ab74d56b948a12096c340642b9f39bef012f1)", testCollection.TestCollectionDisplayName);
		Assert.Equal("44fd295dc6b0e5599b41a18d42adcfadcfed3941648f517724ca23b9377c0bb4", testCollection.UniqueID);
#elif NETFRAMEWORK
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: d15a5b77bdf919b59e06808ca61eb642e984118362c80ede328966fcf64e7b1f)", testCollection.TestCollectionDisplayName);
		Assert.Equal("3bcc9cdbeda7675f5211ecacb601cf2b6bf787cc67d9196e7af9c1fd9c6ac5b2", testCollection.UniqueID);
#else
		Assert.Equal("Test collection for CollectionPerClassTestCollectionFactoryTests+NoAttributes (id: 69ffd2d539ea9ff61b47eee653580191b60aabac6c339de33d72f950f00f243f)", testCollection.TestCollectionDisplayName);
		Assert.Equal("18ea3da232dfcf67f1ea4135bf65f869f64ae245c62d9f71b073c657c4d41685", testCollection.UniqueID);
#endif
	}

	class NoAttributes { }
}
