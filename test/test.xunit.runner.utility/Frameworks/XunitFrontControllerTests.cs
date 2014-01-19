using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitFrontControllerTests
{
    public class Run
    {
        [Fact]
        public void NullTestListCausesDiscoveryBeforeRun()
        {
            var sink = Substitute.For<IMessageSink>();
            var testCase = Mocks.TestCase<Run>("NullTestListCausesDiscoveryBeforeRun");
            var controller = new TestableXunitFrontController();
            controller.InnerController
                      .When(x => x.Find(false, Arg.Any<IMessageSink>()))
                      .Do(callInfo =>
                      {
                          var discoverySink = callInfo.Arg<IMessageSink>();
                          discoverySink.OnMessage(new TestCaseDiscoveryMessage(testCase));
                          discoverySink.OnMessage(new DiscoveryCompleteMessage(new string[0]));
                      });

            controller.Run(null, sink);

            var args = controller.InnerController.Captured(x => x.Run(null, null));
            Assert.Same(sink, args.Arg<IMessageSink>());
            Assert.Collection(args.Arg<IEnumerable<ITestCase>>(),
                tc => Assert.Same(testCase, tc)
            );
        }
    }

    class TestableXunitFrontController : XunitFrontController
    {
        public readonly IFrontController InnerController = Substitute.For<IFrontController>();

        protected override IFrontController CreateInnerController()
        {
            return InnerController;
        }
    }
}
