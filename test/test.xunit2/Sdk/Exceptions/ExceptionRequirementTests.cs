using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

public class ExceptionRequirementTests
{
    [Theory]
    [ExceptionTypes]
    public void ExceptionMustBeSerializable(Type type)
    {
        Assert.Single(type.GetCustomAttributes(typeof(SerializableAttribute), false));
        Assert.NotNull(type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null));
    }

    class ExceptionTypes : DataAttribute
    {
        public override System.Collections.Generic.IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return typeof(Assert).Assembly
                                 .GetExportedTypes()
                                 .Where(t => typeof(Exception).IsAssignableFrom(t))
                                 .Select(t => new object[] { t });
        }
    }
}