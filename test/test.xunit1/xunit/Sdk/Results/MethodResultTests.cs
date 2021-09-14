using System;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class MethodResultTests
    {
        [Fact]
        public void ConstructWithMethodInfo()
        {
            Type stubType = typeof(StubClass);
            MethodInfo method = stubType.GetMethod("Method");

            MethodResult result = new TestableMethodResult(Reflector.Wrap(method));

            Assert.Equal("Method", result.MethodName);
            Assert.Equal(typeof(StubClass).FullName, result.TypeName);
            Assert.Equal(0.0, result.ExecutionTime);
            Assert.Equal(0, result.Traits.Count);
        }

        [Fact]
        public void ConstructWithMethodInfoWithProperties()
        {
            Type stubType = typeof(StubClass);
            MethodInfo method = stubType.GetMethod("MethodWithTraits");

            MethodResult result = new TestableMethodResult(Reflector.Wrap(method));

            Assert.Equal(2, result.Traits.Count);
        }

        static XmlNode FindTrait(XmlNodeList traitNodes, string name)
        {
            foreach (XmlNode xmlNode in traitNodes)
                if (xmlNode.Attributes["name"].Value.Equals(name))
                    return xmlNode;

            return null;
        }

        [Fact]
        public void ToXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("Method");
            TestableMethodResult methodResult = new TestableMethodResult(Reflector.Wrap(method));

            XmlNode resultNode = methodResult.ToXml(parentNode);

            Assert.Equal("test", resultNode.Name);
            Assert.Equal(methodResult.TypeName, resultNode.Attributes["type"].Value);
            Assert.Equal(methodResult.MethodName, resultNode.Attributes["method"].Value);
            Assert.Equal(methodResult.TypeName + "." + methodResult.MethodName, resultNode.Attributes["name"].Value);
            Assert.Null(resultNode.SelectSingleNode("traits"));
            Assert.Null(resultNode.SelectSingleNode("output"));
        }

        [Fact]
        public void ToXmlWithDisplayName()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("Method");
            TestableMethodResult methodResult = new TestableMethodResult(Reflector.Wrap(method), "Display Name");

            XmlNode resultNode = methodResult.ToXml(parentNode);

            Assert.Equal("Display Name", resultNode.Attributes["name"].Value);
        }

        [Fact]
        public void ToXmlWithTraits()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("MethodWithTraits");
            TestableMethodResult methodResult = new TestableMethodResult(Reflector.Wrap(method));

            XmlNode resultNode = methodResult.ToXml(parentNode);

            XmlNode traitsNode = resultNode.SelectSingleNode("traits");
            Assert.NotNull(traitsNode);
            XmlNodeList traitNodes = traitsNode.SelectNodes("trait");
            Assert.Equal("larry", FindTrait(traitNodes, "author").Attributes["value"].Value);
            Assert.Equal("PassedResults", FindTrait(traitNodes, "Class").Attributes["value"].Value);
        }

        [Fact]
        public void ToXmlWithOutput()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("Method");
            TestableMethodResult methodResult = new TestableMethodResult(Reflector.Wrap(method), "Display Name");
            methodResult.Output = "This is my output!";

            XmlNode resultNode = methodResult.ToXml(parentNode);

            Assert.Equal("This is my output!", resultNode.SelectSingleNode("output").InnerText);
        }

        internal class StubClass
        {
            [Fact]
            public void Method() { }

            [Fact]
            public void MethodWithParams(int x, string y) { }

            [Fact]
            [Trait("author", "larry")]
            [Trait("Class", "PassedResults")]
            public void MethodWithTraits() { }
        }

        internal class StubMethodResult : MethodResult
        {
            public StubMethodResult(IMethodInfo method)
                : base(method, null) { }
        }

        class TestableMethodResult : MethodResult
        {
            public TestableMethodResult(IMethodInfo method)
                : base(method, null) { }

            public TestableMethodResult(IMethodInfo method, string displayName)
                : base(method, displayName) { }

            public TestableMethodResult(string methodName, string typeName, Xunit.Sdk.MultiValueDictionary<string, string> traits)
                : base(methodName, typeName, null, traits) { }
        }
    }
}
