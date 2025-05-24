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
#if BUILD_X86 && NETFRAMEWORK
		"Test collection for TestCollectionFactoryBaseTests (id: 59fd624246292e3a2de338c200932449020495a4dacab77ee986ce56f8c2d01c)",
		"a723fcc8303e835c13c2cd5724d4b1dc31bee77e4bd7d727947992a0d4cffe1a"
#elif BUILD_X86
		"Test collection for TestCollectionFactoryBaseTests (id: 16fe7e3476c5fc10b852512c4fc23ecba9a1b14225fa4b53f7dce49bce3ee0cc)",
		"33eb7b31a5aa1851a9a542bfd17c19db99f445d7a5d43b3178676f73ff63894c"
#elif NETFRAMEWORK
		"Test collection for TestCollectionFactoryBaseTests (id: 31a1f441047127513b6e4ff093f4332fc4c0d0d0ee5d4957c81fb490aa083729)",
		"0644374cbb9d1f1071ad468d280c4f6d86421896341a61b3a62b2878b45f8d58"
#else
		"Test collection for TestCollectionFactoryBaseTests (id: 2d86aaa8c37c91ffd7aa8351081dc9cc822d8930d4b9888058a71b9876e25183)",
		"79e637cfea518b2fa38f43d3988bd7a1630308112c312f1ad357bcc0229c76bd"
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
