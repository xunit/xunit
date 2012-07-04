using System;
using System.Xml;
using TestUtility;
using Xunit;

public class TestTimeoutFixture : AcceptanceTest
{
    [Fact]
    public void TestHasTimeoutAndExceeds()
    {
        string code =
            @"
                using System;
                using System.Threading;
                using Xunit;

                public class Stub
                {
                    [Fact(Timeout = 50)]
                    public void TestShouldTimeout()
                    {
                        Thread.Sleep(120); 
                        Assert.Equal(2, 2);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Fail", "Stub.TestShouldTimeout");
        var time = Decimal.Parse(testNode.Attributes["time"].Value);
        Assert.Equal(50M, time);
        XmlNode messageNode = testNode.SelectSingleNode("failure/message");
        Assert.Equal("Test execution time exceeded: 50ms", messageNode.InnerText);
    }
}