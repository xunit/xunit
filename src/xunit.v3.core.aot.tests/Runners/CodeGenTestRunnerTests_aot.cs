using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CodeGenTestRunnerTests
{
	[Collection("Shared state in FixtureWithEvents")]
	public static class Fixtures
	{
		[Fact]
		public static async ValueTask FixtureEvents()
		{
			FixtureWithEvents.Events.Clear();

			var factories = new Dictionary<Type, FixtureFactory> { [typeof(FixtureWithEvents)] = (_, _) => new(new FixtureWithEvents()) };
			var testClass = Mocks.CodeGenTestClass(classFixtureFactories: factories);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var test = Mocks.CodeGenTest(testCase: testCase);
			var runner = new TestableCodeGenTestRunner(test);

			await runner.RunAsync();

			Assert.Collection(
				FixtureWithEvents.Events,
				e => Assert.Equal("OnTestStarting", e),
				e => Assert.Equal("OnTestStartingAsync", e),
				e => Assert.Equal("OnTestFinishedAsync", e),
				e => Assert.Equal("OnTestFinished", e)
			);
		}
	}

	class TestableCodeGenTestRunner(ICodeGenTest test) :
		CodeGenTestRunner
	{
		public async ValueTask<RunSummary> RunAsync()
		{
			var fixtureMappings = new FixtureMappingManager("Mock", test.TestCase.TestClass.ClassFixtureFactories);
			await fixtureMappings.InitializeAsync(false);

			return await Run(
				test,
				new SpyMessageBus(),
				ExplicitOption.Off,
				new(),
				new(),
				fixtureMappings
			);
		}
	}
}
