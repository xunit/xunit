using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.v3;

public class TestCollectionFactoryBaseTests
{
	[Theory]
	[InlineData(typeof(TestClassForByName), null, null, "foo", "ec41e871ca6761e15a5b062a4a39cf6a2f2c6e6e6cb5939681b917eca76a151f")]
	[InlineData(
		typeof(TestClassForByType),
		typeof(TestCollectionFactoryBaseTests),
		"TestCollectionFactoryBaseTests",
#if BUILD_X86
		"Test collection for TestCollectionFactoryBaseTests (id: d6ad35df498d002650211247bf3ad438c14b529c58abcae9e19de92da2a3dd5e)",
		"bf79509be2bd4938d1a29a27ff642508130cfd8d91e0f78831f45d35375ea4ab"
#else
		"Test collection for TestCollectionFactoryBaseTests (id: 477518a275c9dd2651ca8e44c34048e56a23ba41b493cb8f0fc781e836a58950)",
		"744f0f118ef547151afb4bb11e4b0da6206e452610a070fe47772a10e5084607"
#endif
	)]
	public void Defaults(
		Type testClass,
		Type? collectionDefinition,
		string? testCollectionClassName,
		string testCollectionDisplayName,
		string uniqueID)
	{
		var testAssembly = Mocks.XunitTestAssembly();
		var factory = new TestableTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(testClass);

		Assert.Empty(testCollection.BeforeAfterTestAttributes);
		Assert.Empty(testCollection.ClassFixtureTypes);
		Assert.Equal(collectionDefinition, testCollection.CollectionDefinition);
		Assert.Empty(testCollection.CollectionFixtureTypes);
		Assert.False(testCollection.DisableParallelization);
		Assert.Same(testAssembly, testCollection.TestAssembly);
		Assert.Null(testCollection.TestCaseOrderer);
		Assert.Equal(testCollectionClassName, testCollection.TestCollectionClassName);
		Assert.Equal(testCollectionDisplayName, testCollection.TestCollectionDisplayName);
		Assert.Equal(uniqueID, testCollection.UniqueID);
	}

	[Collection("foo")]
	class TestClassForByName { }

	[Collection(typeof(TestCollectionFactoryBaseTests))]
	class TestClassForByType { }

	[Fact]
	public void AcquiresBeforeAfterTestAttributesFromCollectionDefinition_AndMergesThemWithTheAssemblyAttributes()
	{
		var testAssembly = Mocks.XunitTestAssembly(beforeAfterTestAttributes: [new BeforeAfterTestAttribute1()]);
		var factory = new TestableTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(TestClassForBeforeAfterAttribute));

		Assert.Collection(
			testCollection.BeforeAfterTestAttributes.OrderBy(a => a.GetType().Name),
			a => Assert.Equal("BeforeAfterTestAttribute1", a.GetType().Name),
			a => Assert.Equal("BeforeAfterTestAttribute2", a.GetType().Name)
		);
	}

	[BeforeAfterTestAttribute2]
	class TestCollectionWithBeforeAfterAttribute { }

	[Collection(typeof(TestCollectionWithBeforeAfterAttribute))]
	class TestClassForBeforeAfterAttribute { }

	public class BeforeAfterTestAttribute1 : BeforeAfterTestAttribute { }
	public class BeforeAfterTestAttribute2 : BeforeAfterTestAttribute { }

	[Fact]
	public void AcquiresFixtureTypesFromCollectionDefinition()
	{
		var testAssembly = Mocks.XunitTestAssembly();
		var factory = new TestableTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(TestClassForFixtures));

		var collectionFixtureType = Assert.Single(testCollection.CollectionFixtureTypes);
		Assert.Equal(typeof(object), collectionFixtureType);
		var classFixtureType = Assert.Single(testCollection.ClassFixtureTypes);
		Assert.Equal(typeof(string), classFixtureType);
	}

	class TestCollectionWithFixtures : ICollectionFixture<object>, IClassFixture<string> { }

	[Collection(typeof(TestCollectionWithFixtures))]
	class TestClassForFixtures { }

	[Fact]
	public void ReadsCollectionDefinitionAttributeForParallelization()
	{
		// Decorated definitions are read and cached by the test assembly
		var definitions = new Dictionary<string, (Type, CollectionDefinitionAttribute)>
		{
			["foo"] = (typeof(TestCollectionWithoutParallelization), new CollectionDefinitionAttribute { DisableParallelization = true })
		};
		var testAssembly = Mocks.XunitTestAssembly(collectionDefinitions: definitions);
		var factory = new TestableTestCollectionFactory(testAssembly);

		var testCollection = factory.Get(typeof(TestClassForParallelization));

		Assert.True(testCollection.DisableParallelization);
	}

	class TestCollectionWithoutParallelization { }

	[Collection("foo")]
	class TestClassForParallelization { }

	class TestableTestCollectionFactory(IXunitTestAssembly testAssembly) :
		TestCollectionFactoryBase(testAssembly)
	{
		public override string DisplayName => throw new NotImplementedException();

		protected override IXunitTestCollection GetDefaultTestCollection(Type testClass) => throw new NotImplementedException();
	}
}
