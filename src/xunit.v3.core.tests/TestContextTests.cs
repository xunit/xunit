using Xunit;

public class TestContextTests
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
}
