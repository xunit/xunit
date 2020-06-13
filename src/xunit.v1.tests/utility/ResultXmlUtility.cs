using System;
using System.Xml;
using Xunit;

namespace TestUtility
{
    public class ResultXmlUtility
    {
        public static void AssertAttribute(XmlNode testNode, string attributeName, string attributeValue)
        {
            XmlAttribute attrib = testNode.Attributes[attributeName];
            if (attrib == null)
                throw new ArgumentException("Could not find attribute named " + attributeName + " in XML:\r\n" + testNode.OuterXml);
            Assert.Equal(attributeValue, attrib.Value);
        }

        public static XmlNode GetResult(XmlNode assemblyOrClassNode)
        {
            return GetResult(assemblyOrClassNode, 0);
        }

        public static XmlNode GetResult(XmlNode assemblyOrClassNode, int testIndex)
        {
            if (assemblyOrClassNode.Name == "assembly")
                return GetResult(assemblyOrClassNode, 0, testIndex);
            return GetResultFromClass(assemblyOrClassNode, testIndex);
        }

        public static XmlNode GetResult(XmlNode assemblyNode, int classIndex, int testIndex)
        {
            XmlNodeList classNodes = assemblyNode.SelectNodes("class");
            if (classNodes.Count <= classIndex)
                throw new ArgumentException("Could not find class item with index " + classIndex + " in XML:\r\n" + assemblyNode.OuterXml);
            return GetResultFromClass(classNodes[classIndex], testIndex);
        }

        public static XmlNode GetResultFromClass(XmlNode classNode, int testIndex)
        {
            XmlNodeList testNodes = classNode.SelectNodes("test");
            if (testNodes.Count <= testIndex)
                throw new ArgumentException("Could not find test item with index " + testIndex + " in XML:\r\n" + classNode.OuterXml);
            return testNodes[testIndex];
        }

        public static XmlNode AssertResult(XmlNode assemblyNode, string result, string name)
        {
            XmlNode node = GetResult(assemblyNode);
            AssertAttribute(node, "result", result);
            AssertAttribute(node, "name", name);
            return node;
        }
    }
}
