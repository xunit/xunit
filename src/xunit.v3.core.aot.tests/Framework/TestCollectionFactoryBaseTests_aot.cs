using Xunit;
using Xunit.v3;

public class TestCollectionFactoryBaseTests
{
	[Theory]
	[InlineData(typeof(TestClassForByName), null, "foo", "ec41e871ca6761e15a5b062a4a39cf6a2f2c6e6e6cb5939681b917eca76a151f")]
	[InlineData(
		typeof(TestClassForByType),
		"TestCollectionFactoryBaseTests",
		"Test collection for TestCollectionFactoryBaseTests (id: 451ac6e6b5be1c1d0ec121bc23343cbf659c669ac4039ef6a593cab00fd8f6bb)",
		"d0a104e6f9218ad42e7c6302fa178e7a34fd9d7cec13733a752a303d18058485"
	)]
	public static void Defaults(
		Type testClass,
		string? testCollectionClassName,
		string testCollectionDisplayName,
		string uniqueID)
	{
		var collectionDefinitions = new Dictionary<string, CodeGenTestCollectionRegistration>()
		{
			[CollectionAttribute.GetCollectionNameForType(typeof(TestCollectionFactoryBaseTests))] = new() { Type = typeof(TestCollectionFactoryBaseTests) }
		};
		var testAssembly = Mocks.CodeGenTestAssembly(collectionDefinitions: collectionDefinitions);
		var factory = new TestableTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(testClass);

		Assert.Empty(testCollection.BeforeAfterTestAttributes);
		Assert.Empty(testCollection.ClassFixtureFactories);
		Assert.Empty(testCollection.CollectionFixtureFactories);
		Assert.False(testCollection.DisableParallelization);
		Assert.Same(testAssembly, testCollection.TestAssembly);
		Assert.Null(testCollection.TestCaseOrderer);
		Assert.Equal(testCollectionClassName, testCollection.TestCollectionClassName);
		Assert.Equal(testCollectionDisplayName, testCollection.TestCollectionDisplayName);
		Assert.Equal(uniqueID, testCollection.UniqueID);
	}

	[Collection("foo")]
	internal class TestClassForByName { }

	[Collection(typeof(TestCollectionFactoryBaseTests))]
	internal class TestClassForByType { }

	[Fact]
	public void ReadsCollectionDefinitionAttributeForParallelization()
	{
		var collectionDefinitions = new Dictionary<string, CodeGenTestCollectionRegistration>()
		{
			["foo"] = new() { DisableParallelization = true }
		};
		var testAssembly = Mocks.CodeGenTestAssembly(collectionDefinitions: collectionDefinitions);
		var factory = new TestableTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(TestClassForParallelization));

		Assert.True(testCollection.DisableParallelization);
	}

	[Collection("foo")]
	class TestClassForParallelization { }

	class TestableTestCollectionFactory(ICodeGenTestAssembly testAssembly) :
		TestCollectionFactoryBase(testAssembly)
	{
		public override string DisplayName => throw new NotImplementedException();

		protected override ICodeGenTestCollection GetDefaultTestCollection(Type testClass) => throw new NotImplementedException();
	}
}
