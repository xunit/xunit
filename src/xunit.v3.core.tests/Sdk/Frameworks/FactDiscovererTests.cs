using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class FactDiscovererTests
{
	readonly ExceptionAggregator aggregator;
	readonly CancellationTokenSource cancellationTokenSource;
	readonly _IReflectionAttributeInfo factAttribute;
	readonly SpyMessageBus messageBus;
	readonly _ITestFrameworkDiscoveryOptions options;

	public FactDiscovererTests()
	{
		aggregator = new ExceptionAggregator();
		cancellationTokenSource = new CancellationTokenSource();
		factAttribute = Mocks.FactAttribute();
		messageBus = new SpyMessageBus();
		options = _TestFrameworkOptions.ForDiscovery();
	}

	[Fact]
	public async ValueTask FactWithoutParameters_ReturnsTestCaseThatRunsFact()
	{
		var discoverer = new FactDiscoverer();
		var testMethod = Mocks.TestMethod<ClassUnderTest>("FactWithNoParameters");

		var testCases = await discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await testCase.RunAsync(ExplicitOption.Off, messageBus, new object[0], aggregator, cancellationTokenSource);
		Assert.Single(messageBus.Messages.OfType<_TestPassed>());
	}

	[Fact]
	public async ValueTask FactWithParameters_ReturnsTestCaseWhichThrows()
	{
		var discoverer = new FactDiscoverer();
		var testMethod = Mocks.TestMethod<ClassUnderTest>("FactWithParameters");

		var testCases = await discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await testCase.RunAsync(ExplicitOption.Off, messageBus, new object[0], aggregator, cancellationTokenSource);
		var failed = Assert.Single(messageBus.Messages.OfType<_TestFailed>());
		Assert.Equal(typeof(InvalidOperationException).FullName, failed.ExceptionTypes.Single());
		Assert.Equal("[Fact] methods are not allowed to have parameters. Did you mean to use [Theory]?", failed.Messages.Single());
	}

	[Fact]
	public async ValueTask GenericFact_ReturnsTestCaseWhichThrows()
	{
		var discoverer = new FactDiscoverer();
		var testMethod = Mocks.TestMethod<ClassUnderTest>("GenericFact");

		var testCases = await discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await testCase.RunAsync(ExplicitOption.Off, messageBus, new object[0], aggregator, cancellationTokenSource);
		var failed = Assert.Single(messageBus.Messages.OfType<_TestFailed>());
		Assert.Equal(typeof(InvalidOperationException).FullName, failed.ExceptionTypes.Single());
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

		[Fact]
		public void GenericFact<T>() { }
	}
}
