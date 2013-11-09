using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

[UseCulture("pl-PL")]
public class TestClassCallbackHandlerTests
{
    [Fact]
    public static void WithClassNode_ParsesNumbersWithInvariantCulture()
    {
        var handler = new TestClassCallbackHandler(new Xunit1TestCase[0], Substitute.For<IMessageSink>());
        var xml = new XmlDocument();
        xml.LoadXml("<class time='1.234' total='4' failed='3' skipped='2' />");

        handler.OnXmlNode(xml.FirstChild);

        Assert.Equal(1.234M, handler.TestClassResults.Time);
        Assert.Equal(4, handler.TestClassResults.Total);
        Assert.Equal(3, handler.TestClassResults.Failed);
        Assert.Equal(2, handler.TestClassResults.Skipped);
    }

    [Fact]
    public static void WithTestNode_ParsesNumberWithInvariantCulture()
    {
        var sink = Substitute.For<IMessageSink>();
        var testCollection = new Xunit1TestCollection("assembly");
        var testCase = new Xunit1TestCase("assembly", "foo", "bar", "foo.bar") { TestCollection = testCollection };
        var handler = new TestClassCallbackHandler(new [] { testCase }, sink);
        var xml = new XmlDocument();
        xml.LoadXml("<test type='foo' method='bar' name='foo.bar' time='1.234' result='Pass' />");

        handler.OnXmlNode(xml.FirstChild);

        var args = sink.Captured(1, x => x.OnMessage(null));
        var message = args.Arg<ITestFinished>();
        Assert.Equal(1.234M, message.ExecutionTime);
    }
}
