using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class PassedResultTests
    {
        [Fact]
        public void ToXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("StubMethod");
            PassedResult passedResult = new PassedResult(Reflector.Wrap(method), null);
            passedResult.ExecutionTime = 1.2;

            XmlNode resultNode = passedResult.ToXml(parentNode);

            Assert.Equal("Pass", resultNode.Attributes["result"].Value);
            Assert.Equal("1.200", resultNode.Attributes["time"].Value);
            Assert.Null(resultNode.SelectSingleNode("failure"));
            Assert.Null(resultNode.SelectSingleNode("reason"));
        }

        internal class StubClass
        {
            public void StubMethod() { }
        }
    }
}
