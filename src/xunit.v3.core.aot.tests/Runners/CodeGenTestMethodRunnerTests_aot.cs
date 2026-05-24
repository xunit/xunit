using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CodeGenTestMethodRunnerTests
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
			var runner = new TestableCodeGenTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				FixtureWithEvents.Events,
				e => Assert.Equal("OnTestMethodStarting", e),
				e => Assert.Equal("OnTestMethodStartingAsync", e),

				e => Assert.Equal("OnTestCaseStarting", e),
				e => Assert.Equal("OnTestCaseStartingAsync", e),

				e => Assert.Equal("OnTestStarting", e),
				e => Assert.Equal("OnTestStartingAsync", e),
				e => Assert.Equal("OnTestFinishedAsync", e),
				e => Assert.Equal("OnTestFinished", e),

				e => Assert.Equal("OnTestCaseFinishedAsync", e),
				e => Assert.Equal("OnTestCaseFinished", e),

				e => Assert.Equal("OnTestMethodFinishedAsync", e),
				e => Assert.Equal("OnTestMethodFinished", e)
			);
		}
	}

	class TestableCodeGenTestMethodRunner(ICodeGenTestCase testCase) :
		CodeGenTestMethodRunner
	{
		public async ValueTask<RunSummary> RunAsync()
		{
			var fixtureMappings = new FixtureMappingManager("Mock", testCase.TestClass.ClassFixtureFactories);
			await fixtureMappings.InitializeAsync(false);

			return await Run(
				testCase.TestMethod,
				[testCase],
				ExplicitOption.Off,
				new SpyMessageBus(),
				new(),
				ParallelismOptionsAliases.Default,
				parallelizationSemaphore: null,
				new(),
				fixtureMappings
			);
		}
	}
}
