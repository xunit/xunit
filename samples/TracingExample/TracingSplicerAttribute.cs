using System;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TracingSplicerAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        Console.WriteLine("Before : {0}.{1}", methodUnderTest.DeclaringType.FullName, methodUnderTest.Name);
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Console.WriteLine("After : {0}.{1}", methodUnderTest.DeclaringType.FullName, methodUnderTest.Name);
    }
}