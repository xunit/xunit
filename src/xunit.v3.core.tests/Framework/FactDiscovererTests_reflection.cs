using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class FactDiscovererTests
{
	readonly ExceptionAggregator aggregator;
	readonly CancellationTokenSource cancellationTokenSource;
	readonly IFactAttribute factAttribute;
	readonly FixtureMappingManager fixtureMappings;
	readonly SpyMessageBus messageBus;
	readonly ITestFrameworkDiscoveryOptions options;

	public FactDiscovererTests()
	{
		aggregator = new();
		cancellationTokenSource = new();
		factAttribute = Mocks.FactAttribute();
		fixtureMappings = new("Mock");
		messageBus = new();
		options = TestData.TestFrameworkDiscoveryOptions();
	}

	[Fact]
	public async ValueTask FactWithoutParameters_ReturnsTestCaseThatRunsFact()
	{
		var discoverer = new FactDiscoverer();
		var testMethod = TestData.XunitTestMethod<ClassUnderTest>("FactWithNoParameters");

		var testCases = await discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await XunitRunnerHelper.RunXunitTestCase(testCase, messageBus, cancellationTokenSource, aggregator, ExplicitOption.Off, [], fixtureMappings);
		Assert.Single(messageBus.Messages.OfType<ITestPassed>());
	}

	[Fact]
	public async ValueTask FactWithParameters_ReturnsTestCaseWhichThrows()
	{
		var discoverer = new FactDiscoverer();
		var testMethod = TestData.XunitTestMethod<ClassUnderTest>("FactWithParameters");

		var testCases = await discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await XunitRunnerHelper.RunXunitTestCase(testCase, messageBus, cancellationTokenSource, aggregator, ExplicitOption.Off, [], fixtureMappings);
		var failed = Assert.Single(messageBus.Messages.OfType<ITestFailed>());
		Assert.Equal(typeof(TestPipelineException).FullName, failed.ExceptionTypes.Single());
		Assert.Equal("[Fact] methods are not allowed to have parameters. Did you mean to use [Theory]?", failed.Messages.Single());
	}

	[Fact]
	public async ValueTask GenericFact_ReturnsTestCaseWhichThrows()
	{
		var discoverer = new FactDiscoverer();
		var testMethod = TestData.XunitTestMethod<ClassUnderTest>("GenericFact");

		var testCases = await discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await XunitRunnerHelper.RunXunitTestCase(testCase, messageBus, cancellationTokenSource, aggregator, ExplicitOption.Off, [], fixtureMappings);
		var failed = Assert.Single(messageBus.Messages.OfType<ITestFailed>());
		Assert.Equal(typeof(TestPipelineException).FullName, failed.ExceptionTypes.Single());
		Assert.Equal("[Fact] methods are not allowed to be generic.", failed.Messages.Single());
	}

	class ClassUnderTest
	{
		[Fact]
		public void FactWithNoParameters() { }

#pragma warning disable xUnit1001 // Fact methods cannot have parameters

		[Fact]
		public void FactWithParameters(int _) { }

#pragma warning restore xUnit1001 // Fact methods cannot have parameters

#pragma warning disable xUnit1061 // Fact methods cannot be generic

		[Fact]
		public void GenericFact<T>() { }

#pragma warning restore xUnit1061 // Fact methods cannot be generic
	}
}
