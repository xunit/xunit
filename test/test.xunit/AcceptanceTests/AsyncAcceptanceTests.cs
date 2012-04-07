using System.Xml;
using TestUtility;
using Xunit;
using Xunit.Sdk;

public class AsyncAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void Async40AcceptanceTest()
    {
        string code = @"
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Xunit;

            public class TestClass
            {
                [Fact]
                public Task TestMethod()
                {
                    return Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1);
                    })
                    .ContinueWith(_ =>
                    {
                        Assert.True(false);
                    });
                }
            }
        ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Fail", "TestClass.TestMethod");
        XmlNode failureNode = testNode.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(TrueException).FullName);
    }
}
