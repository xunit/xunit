using System;
using System.Linq;
using System.Threading;
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
	public async void FactWithoutParameters_ReturnsTestCaseThatRunsFact()
	{
		var discoverer = TestableFactDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassUnderTest>("FactWithNoParameters");

		var testCases = discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await testCase.RunAsync(SpyMessageSink.Create(), messageBus, new object[0], aggregator, cancellationTokenSource);
		Assert.Single(messageBus.Messages.OfType<_TestPassed>());
	}

	[Fact]
	public async void FactWithParameters_ReturnsTestCaseWhichThrows()
	{
		var discoverer = TestableFactDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassUnderTest>("FactWithParameters");

		var testCases = discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await testCase.RunAsync(SpyMessageSink.Create(), messageBus, new object[0], aggregator, cancellationTokenSource);
		var failed = Assert.Single(messageBus.Messages.OfType<_TestFailed>());
		Assert.Equal(typeof(InvalidOperationException).FullName, failed.ExceptionTypes.Single());
		Assert.Equal("[Fact] methods are not allowed to have parameters. Did you mean to use [Theory]?", failed.Messages.Single());
	}

	[Fact]
	public async void GenericFact_ReturnsTestCaseWhichThrows()
	{
		var discoverer = TestableFactDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassUnderTest>("GenericFact");

		var testCases = discoverer.Discover(options, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		await testCase.RunAsync(SpyMessageSink.Create(), messageBus, new object[0], aggregator, cancellationTokenSource);
		var failed = Assert.Single(messageBus.Messages.OfType<_TestFailed>());
		Assert.Equal(typeof(InvalidOperationException).FullName, failed.ExceptionTypes.Single());
		Assert.Equal("[Fact] methods are not allowed to be generic.", failed.Messages.Single());
	}

	class ClassUnderTest
	{
		[Fact]
		public void FactWithNoParameters() { }

		[Fact]
		public void FactWithParameters(int x) { }

		[Fact]
		public void GenericFact<T>() { }
	}

	class TestableFactDiscoverer : FactDiscoverer
	{
		public TestableFactDiscoverer(_IMessageSink diagnosticMessageSink)
			: base(diagnosticMessageSink)
		{ }

		public static TestableFactDiscoverer Create() =>
			new TestableFactDiscoverer(SpyMessageSink.Create());
	}
}
