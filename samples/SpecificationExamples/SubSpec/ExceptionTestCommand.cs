using System;
using System.Xml;
using Xunit.Sdk;

namespace SubSpec
{
    public class ExceptionTestCommand : ITestCommand
    {
        readonly Exception exception;
        readonly IMethodInfo method;

        public ExceptionTestCommand(IMethodInfo method, Exception exception)
        {
            this.method = method;
            this.exception = exception;
        }

        public string Name
        {
            get { return null; }
        }

        public bool ShouldCreateInstance
        {
            get { return false; }
        }

        public MethodResult Execute(object testClass)
        {
            return new FailedResult(method, exception, Name);
        }

        public string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public int Timeout
        {
            get { return 0; }
        }

        public virtual XmlNode ToStartXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<dummy/>");
            XmlNode testNode = XmlUtility.AddElement(doc.ChildNodes[0], "start");

            string typeName = method.TypeName;
            string methodName = method.Name;

            XmlUtility.AddAttribute(testNode, "name", typeName + "." + methodName);
            XmlUtility.AddAttribute(testNode, "type", typeName);
            XmlUtility.AddAttribute(testNode, "method", methodName);

            return testNode;
        }
    }
}