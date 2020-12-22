using System.Linq;
using System.Threading;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: XunitTestCaseRunnerTests.BeforeAfterOnAssembly]

public class XunitTestCaseRunnerTests
{
	[Fact]
	public static void BeforeAfterTestAttributesComeFromTestCollectionAndTestClassAndTestMethod()
	{
		var collection = Mocks.TestCollection(definition: Reflector.Wrap(typeof(BeforeAfterCollection)));
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing", collection);
		var messageBus = Substitute.For<IMessageBus>();
		var aggregator = new ExceptionAggregator();
		var tokenSource = new CancellationTokenSource();

		var runner = new XunitTestCaseRunner(testCase, "Display Name", "Skip Reason", new object[0], new object[0], messageBus, aggregator, tokenSource);

		Assert.Collection(
			runner.BeforeAfterAttributes.OrderBy(a => a.GetType().Name),
			attr => Assert.IsType<BeforeAfterOnAssembly>(attr),
			attr => Assert.IsType<BeforeAfterOnClass>(attr),
			attr => Assert.IsType<BeforeAfterOnCollection>(attr),
			attr => Assert.IsType<BeforeAfterOnMethod>(attr)
		);
	}

	[BeforeAfterOnCollection]
	class BeforeAfterCollection { }

	[BeforeAfterOnClass]
	class ClassUnderTest
	{
		[Fact]
		[BeforeAfterOnMethod]
		public void Passing() { }
	}

	class BeforeAfterOnCollection : BeforeAfterTestAttribute { }
	class BeforeAfterOnClass : BeforeAfterTestAttribute { }
	class BeforeAfterOnMethod : BeforeAfterTestAttribute { }
	public class BeforeAfterOnAssembly : BeforeAfterTestAttribute { }
}
