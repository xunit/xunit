using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCase : ITestCase
{
    public TestCase()
    {
        Traits = new Dictionary<string, string>();
    }

    public TestCase(Type type, string methodName)
        : this()
    {
        Class = type;
        Method = type.GetMethod(methodName);
    }

    public IAssemblyInfo Assembly { get; set; }
    public Type Class { get; set; }
    public string ClassName { get; set; }
    public string DisplayName { get; set; }
    public MethodInfo Method { get; set; }
    public string MethodName { get; set; }
    public string SkipReason { get; set; }
    public int? SourceFileLine { get; set; }
    public string SourceFileName { get; set; }
    public ITestCollection TestCollection { get; set; }
    public IDictionary<string, string> Traits { get; set; }
}