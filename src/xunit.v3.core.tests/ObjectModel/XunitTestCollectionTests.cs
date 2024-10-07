using System;
using System.Collections.Generic;
using System.Linq;
using SomeNamespace;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCollectionTests
{
	readonly XunitTestCollection testCollection;

	public XunitTestCollectionTests()
	{
		var collectionDefinitions = new Dictionary<string, (Type, CollectionDefinitionAttribute)> { ["foo"] = (typeof(BeforeAfterCollection), new CollectionDefinitionAttribute()) };
		var testAssembly = Mocks.XunitTestAssembly(beforeAfterTestAttributes: [new BeforeAfterOnAssembly()], collectionDefinitions: collectionDefinitions);
		testCollection = new XunitTestCollection(testAssembly, typeof(MyCollection), true, "display name");
	}

	[Fact]
	public void Metadata()
	{
		Assert.Equal(typeof(MyCollection), testCollection.CollectionDefinition);
		Assert.Equal("XunitTestCollectionTests+MyCollection", testCollection.TestCollectionClassName);
		Assert.Equal("display name", testCollection.TestCollectionDisplayName);
	}

	[Fact]
	public void BeforeAfterTestAttributes()
	{
		var result = testCollection.BeforeAfterTestAttributes;

		Assert.Collection(
			result.OrderBy(a => a.GetType().Name),
			attr => Assert.IsType<BeforeAfterOnAssembly>(attr),
			attr => Assert.IsType<BeforeAfterOnCollection>(attr)
		);
	}

	[Fact]
	public void ClassFixtureTypes()
	{
		var fixtures = testCollection.ClassFixtureTypes;

		var fixture = Assert.Single(fixtures);
		Assert.Equal(typeof(MyClassFixture), fixture);
	}

	[Fact]
	public void CollectionFixtureTypes()
	{
		var fixtures = testCollection.CollectionFixtureTypes;

		var fixture = Assert.Single(fixtures);
		Assert.Equal(typeof(MyCollectionFixture), fixture);
	}

	[Fact]
	public void DisableParallelization()
	{
		var disableParallelization = testCollection.DisableParallelization;

		Assert.True(disableParallelization);
	}

	[Fact]
	public void TestCaseOrderer()
	{
		var orderer = testCollection.TestCaseOrderer;

		Assert.IsType<MyTestCaseOrderer>(orderer);
	}

	[Fact]
	public void Serialization()
	{
		// We can't use the XunitTestCollection backed by mocks because they don't serialize, so we'll create
		// one here that's backed by an actual XunitTestAssembly object.
		var testAssembly = TestData.XunitTestAssembly<ClassUnderTest>();
		var testCollection = new XunitTestCollection(testAssembly, typeof(MyCollection), true, "display name");

		var serialized = SerializationHelper.Instance.Serialize(testCollection);
		var deserialized = SerializationHelper.Instance.Deserialize(serialized);

		Assert.IsType<XunitTestCollection>(deserialized);
		Assert.Equivalent(testCollection, deserialized);
	}

	class BeforeAfterOnAssembly : BeforeAfterTestAttribute { }
	class BeforeAfterOnCollection : BeforeAfterTestAttribute { }

	class MyClassFixture { }

	class MyCollectionFixture { }

	class MyTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : notnull, ITestCase =>
				throw new System.NotImplementedException();
	}

	[BeforeAfterOnCollection]
	[TestCaseOrderer(typeof(MyTestCaseOrderer))]
	class MyCollection : ICollectionFixture<MyCollectionFixture>, IClassFixture<MyClassFixture> { }
}
