//using System.Collections.Generic;
//using System.Linq;
//using Moq;
//using Xunit;
//using Xunit.Abstractions;

//public class MockXunitController : Mock<IXunitController>
//{
//    public MockXunitController()
//    {
//        this.Setup(c => c.Find(It.IsAny<bool>(), It.IsAny<IMessageSink>()))
//                  .Callback<bool, IMessageSink>((includeSrc, sink) =>
//                  {
//                      Operations.Add("Discovery: assembly (includeSourceInformation = " + includeSrc + ")");
//                      DoFind(sink);
//                  });

//        this.Setup(c => c.Find(It.IsAny<ITypeInfo>(), It.IsAny<bool>(), It.IsAny<IMessageSink>()))
//                  .Callback<ITypeInfo, bool, IMessageSink>((type, includeSrc, sink) =>
//                  {
//                      Operations.Add("Discovery: type " + type.Name + " (includeSourceInformation = " + includeSrc + ")");
//                      DoFind(sink);
//                  });

//        this.Setup(c => c.Run(It.IsAny<IEnumerable<ITestCase>>(), It.IsAny<IMessageSink>()))
//            .Callback<IEnumerable<ITestCase>, IMessageSink>((cases, sink) =>
//            {
//                Operations.Add("Run: " + cases.Count() + " test case(s)");

//                TestCasesRan.AddRange(cases);

//                sink.OnMessage(new Mock<ITestAssemblyStarting>().Object);
//                sink.OnMessage(new Mock<ITestAssemblyFinished>().Object);
//            });

//        TestCasesToRun = new List<IMethodTestCase> { new Mock<IMethodTestCase>().Object };

//        TestCasesRan = new List<ITestCase>();

//        Operations = new List<string>();
//    }

//    public List<string> Operations;

//    public List<ITestCase> TestCasesRan;

//    public List<IMethodTestCase> TestCasesToRun;

//    private void DoFind(IMessageSink sink)
//    {
//        foreach (var testCase in TestCasesToRun)
//        {
//            var discovery = new Mock<ITestCaseDiscoveryMessage>();
//            discovery.SetupGet(m => m.TestCase).Returns(testCase);
//            sink.OnMessage(discovery.Object);
//        }

//        sink.OnMessage(new Mock<IDiscoveryCompleteMessage>().Object);
//    }
//}