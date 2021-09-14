using System.Xml;
using Xunit.Sdk;

public class StubTestCommand : ITestCommand
{
    public bool Execute__Called;
    public MethodResult Execute__Result;
    public object Execute_TestClass;
    public string Name__Result;
    public bool ShouldCreateInstance__Result = true;
    public int Timeout__Result;
    public XmlNode ToStartXml__Result;

    public string DisplayName
    {
        get { return Name__Result; }
    }

    public bool ShouldCreateInstance
    {
        get { return ShouldCreateInstance__Result; }
    }

    public int Timeout
    {
        get { return Timeout__Result; }
    }

    public MethodResult Execute(object testClass)
    {
        Execute__Called = true;
        Execute_TestClass = testClass;

        return Execute__Result;
    }

    public XmlNode ToStartXml()
    {
        return ToStartXml__Result;
    }
}
