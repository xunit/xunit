using System;
using System.Collections.Generic;
using Xunit.Abstractions;

public class TestCase : IMethodTestCase
{
    public TestCase()
    {
        Traits = new Dictionary<string, string>();
    }

    public TestCase(Type type, string methodName)
        : this()
    {
        Class = Reflector.Wrap(type);
        Method = Reflector.Wrap(type.GetMethod(methodName));
    }

    public IAssemblyInfo Assembly { get; set; }
    public ITypeInfo Class { get; set; }
    public string DisplayName { get; set; }
    public IMethodInfo Method { get; set; }
    public string SkipReason { get; set; }
    public int? SourceFileLine { get; set; }
    public string SourceFileName { get; set; }
    public ITestCollection TestCollection { get; set; }
    public IDictionary<string, string> Traits { get; set; }
}