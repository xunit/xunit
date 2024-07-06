using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class EnvironmentRestorer(params string[] variables) :
    BeforeAfterTestAttribute
{
    readonly Dictionary<string, string> restoreValues = new();
    readonly string[] variables = variables;

    public override void After(MethodInfo methodUnderTest)
    {
        foreach (var kvp in restoreValues)
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
    }

    public override void Before(MethodInfo methodUnderTest)
    {
        foreach (var variable in variables)
            restoreValues[variable] = Environment.GetEnvironmentVariable(variable);
    }
}
