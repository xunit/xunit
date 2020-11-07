using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Common;

public class TestDiscoverySinkTests
{
	[Fact]
	public void CollectsTestCases()
	{
		var visitor = new TestDiscoverySink();
		var testCase1 = Substitute.For<ITestCase>();
		var testCase2 = Substitute.For<ITestCase>();
		var testCase3 = Substitute.For<ITestCase>();

		visitor.OnMessage(new DiscoveryMessage(testCase1));
		visitor.OnMessage(new DiscoveryMessage(testCase2));
		visitor.OnMessage(new DiscoveryMessage(testCase3));
		visitor.OnMessage(Substitute.For<IMessageSinkMessage>()); // Ignored

		Assert.Collection(
			visitor.TestCases,
			msg => Assert.Same(testCase1, msg),
			msg => Assert.Same(testCase2, msg),
			msg => Assert.Same(testCase3, msg)
		);
	}

	class DiscoveryMessage : ITestCaseDiscoveryMessage
	{
		public DiscoveryMessage(ITestCase testCase)
		{
			TestCase = testCase;
		}

		public ITestAssembly TestAssembly => TestCase.TestMethod.TestClass.TestCollection.TestAssembly;

		public ITestCase TestCase { get; private set; }

		public IEnumerable<ITestCase> TestCases => new[] { TestCase };

		public ITestClass TestClass => TestCase.TestMethod.TestClass;

		public ITestCollection TestCollection => TestCase.TestMethod.TestClass.TestCollection;

		public ITestMethod TestMethod => TestCase.TestMethod;
	}
}
