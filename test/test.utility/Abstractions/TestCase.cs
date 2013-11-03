using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCase : ITestCase
{
    public TestCase()
    {
        Traits = new MultiValueDictionary<string, string>();
    }

    public TestCase(Type type, string methodName)
        : this()
    {
        Class = Reflector.Wrap(type);
        Method = Reflector.Wrap(type.GetMethod(methodName));
    }

    public IAssemblyInfo Assembly { get; set; }
    public ITypeInfo Class { get; set; }
    public string ClassName { get; set; }
    public string DisplayName { get; set; }
    public IMethodInfo Method { get; set; }
    public string MethodName { get; set; }
    public string SkipReason { get; set; }
    public SourceInformation SourceInformation { get; set; }
    public ITestCollection TestCollection { get; set; }
    public IMultiValueDictionary<string, string> Traits { get; set; }
    public string UniqueID { get; set; }
}