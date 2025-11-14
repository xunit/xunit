using System;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

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

	[Fact]
	public static void CannotAccessCancellationTokenOfDisposedContext()
	{
		TestContextInternal.Current.Dispose();

		var ex = Record.Exception(() => TestContext.Current.CancellationToken);

		var odex = Assert.IsType<ObjectDisposedException>(ex);
		Assert.Equal(nameof(TestContext), odex.ObjectName);
	}

	[Fact]
	public static void CannotCancelTestWithDisposedContext()
	{
		TestContextInternal.Current.Dispose();

		var ex = Record.Exception(() => TestContext.Current.CancelCurrentTest());

		var odex = Assert.IsType<ObjectDisposedException>(ex);
		Assert.Equal(nameof(TestContext), odex.ObjectName);
	}

	[Fact]
	[ClearAttachments]
	public static void StringAttachmentCannotBeReplacedByDefault()
	{
		TestContext.Current.AddAttachment("foo", "bar");

		var ex = Record.Exception(() => TestContext.Current.AddAttachment("foo", "baz"));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("name", argEx.ParamName);
		Assert.StartsWith("Attempted to add an attachment with an existing name: 'foo'", argEx.Message);
	}

	[Fact]
	[ClearAttachments]
	public static void StringAttachmentCanBeReplaced()
	{
		TestContext.Current.AddAttachment("foo", "bar");

		TestContext.Current.AddAttachment("foo", "baz", replaceExistingValue: true);

		var value = default(TestAttachment);
		TestContext.Current.Attachments?.TryGetValue("foo", out value);
		Assert.NotNull(value);
		Assert.Equal("baz", value.AsString());
	}

	[Fact]
	[ClearAttachments]
	public static void BinaryAttachmentCannotBeReplacedByDefault()
	{
		TestContext.Current.AddAttachment("foo", [1, 2, 3]);

		var ex = Record.Exception(() => TestContext.Current.AddAttachment("foo", [4, 5, 6]));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("name", argEx.ParamName);
		Assert.StartsWith("Attempted to add an attachment with an existing name: 'foo'", argEx.Message);
	}

	[Fact]
	[ClearAttachments]
	public static void BinaryAttachmentCanBeReplaced()
	{
		TestContext.Current.AddAttachment("foo", [1, 2, 3]);

		TestContext.Current.AddAttachment("foo", [4, 5, 6], replaceExistingValue: true);

		var value = default(TestAttachment);
		TestContext.Current.Attachments?.TryGetValue("foo", out value);
		Assert.NotNull(value);
		Assert.Equal([4, 5, 6], value.AsByteArray().ByteArray);
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

	class ClearAttachmentsAttribute : BeforeAfterTestAttribute
	{
		public override void After(
			MethodInfo methodUnderTest,
			IXunitTest test) =>
				TestContextInternal.Current.ClearAttachments();
	}
}
