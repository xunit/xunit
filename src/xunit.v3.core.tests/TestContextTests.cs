using System;
using Xunit;

[CollectionDefinition]
public class TestContextTestsCollection : ICollectionFixture<TestContextTests.MyCollectionFixture> { }

[Collection(typeof(TestContextTestsCollection))]
public sealed class TestContextTests : IClassFixture<TestContextTests.MyClassFixture>
{
	[Fact]
	public static void AmbientTestContextIsAvailableInTest()
	{
		var context = TestContext.Current;

		Assert.NotNull(context);
		// Test
		Assert.Equal(TestEngineStatus.Running, context.TestStatus);
		Assert.Equal(TestPipelineStage.TestExecution, context.PipelineStage);
		var test = context.Test;
		Assert.NotNull(test);
		Assert.Equal($"{nameof(TestContextTests)}.{nameof(AmbientTestContextIsAvailableInTest)}", test.TestDisplayName);
		Assert.Null(context.TestState);
		// Test case
		Assert.Equal(TestEngineStatus.Running, context.TestCaseStatus);
		Assert.Same(test.TestCase, context.TestCase);
		// Test method
		Assert.Equal(TestEngineStatus.Running, context.TestMethodStatus);
		Assert.Same(test.TestCase.TestMethod, context.TestMethod);
		// Test class
		Assert.Equal(TestEngineStatus.Running, context.TestClassStatus);
		Assert.Same(test.TestCase.TestMethod!.TestClass, context.TestClass);
		// Test collection
		Assert.Equal(TestEngineStatus.Running, context.TestCollectionStatus);
		Assert.Same(test.TestCase.TestCollection, context.TestCollection);
		// Test assembly
		Assert.Equal(TestEngineStatus.Running, context.TestAssemblyStatus);
		Assert.Same(test.TestCase.TestCollection.TestAssembly, context.TestAssembly);
	}

	[Fact]
	public static void KeyValueStorageIsAvailableThroughoutPipeline()
	{
		Assert.Equal(42, TestContext.Current.KeyValueStorage["collectionValue"]);
		Assert.Equal(2112, TestContext.Current.KeyValueStorage["classValue"]);

		TestContext.Current.KeyValueStorage["testValue"] = 2600;
	}

	public sealed class MyClassFixture : IDisposable
	{
		public MyClassFixture()
		{
			Assert.Equal(42, TestContext.Current.KeyValueStorage["collectionValue"]);

			TestContext.Current.KeyValueStorage["classValue"] = 2112;
		}

		public void Dispose()
		{
			Assert.Equal(42, TestContext.Current.KeyValueStorage["collectionValue"]);
			Assert.Equal(2112, TestContext.Current.KeyValueStorage["classValue"]);
			Assert.Equal(2600, TestContext.Current.KeyValueStorage["testValue"]);
		}
	}

	public sealed class MyCollectionFixture : IDisposable
	{
		public MyCollectionFixture() =>
			TestContext.Current.KeyValueStorage["collectionValue"] = 42;

		public void Dispose()
		{
			Assert.Equal(42, TestContext.Current.KeyValueStorage["collectionValue"]);
			Assert.Equal(2112, TestContext.Current.KeyValueStorage["classValue"]);
			Assert.Equal(2600, TestContext.Current.KeyValueStorage["testValue"]);
		}
	}
}
