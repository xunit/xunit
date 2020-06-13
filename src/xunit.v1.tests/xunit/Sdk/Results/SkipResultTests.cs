using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class SkipResultTests
    {
        [Fact]
        public void ToXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("StubMethod");
            SkipResult skipResult = new SkipResult(Reflector.Wrap(method), null, "the reason");

            XmlNode resultNode = skipResult.ToXml(parentNode);

            Assert.Equal("Skip", resultNode.Attributes["result"].Value);
            Assert.Null(resultNode.Attributes["success"]);
            Assert.Null(resultNode.Attributes["time"]);
            Assert.Null(resultNode.SelectSingleNode("failure"));
            Assert.Equal("the reason", resultNode.SelectSingleNode("reason/message").InnerText);
        }

        internal class StubClass
        {
            public void StubMethod() { }
        }
    }
}
