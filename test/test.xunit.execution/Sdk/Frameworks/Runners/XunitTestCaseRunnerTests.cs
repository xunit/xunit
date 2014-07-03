using System.Threading;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

public class XunitTestCaseRunnerTests
{
    [Fact]
    public static void BeforeAfterTestAttributesComeFromBothTestClassAndTestMethod()
    {
        var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing");
        var messageBus = Substitute.For<IMessageBus>();
        var aggregator = new ExceptionAggregator();
        var tokenSource = new CancellationTokenSource();

        var runner = new XunitTestCaseRunner(testCase, "Display Name", "Skip Reason", new object[0], new object[0], messageBus, aggregator, tokenSource);

        Assert.Collection(runner.BeforeAfterAttributes,
            attr => Assert.IsType<BeforeAfterOnClass>(attr),
            attr => Assert.IsType<BeforeAfterOnMethod>(attr)
        );
    }

    [BeforeAfterOnClass]
    class ClassUnderTest
    {
        [Fact]
        [BeforeAfterOnMethod]
        public void Passing() { }
    }

    class BeforeAfterOnClass : BeforeAfterTestAttribute { }
    class BeforeAfterOnMethod : BeforeAfterTestAttribute { }
}
