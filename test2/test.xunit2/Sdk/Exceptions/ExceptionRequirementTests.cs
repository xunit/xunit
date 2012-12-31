using System;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;

public class ExceptionRequirementTests
{
    [Theory(Skip = "We don't have generalized DataAttribute support yet...")]
    //[ExceptionTypes]
    public void ExceptionMustBeSerializable(Type type)
    {
        Assert.Single(type.GetCustomAttributes(typeof(SerializableAttribute), false));
        Assert.NotNull(type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null));
    }

    //class ExceptionTypes : DataAttribute
    //{
    //    public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
    //    {
    //        return typeof(Assert).Assembly
    //                             .GetExportedTypes()
    //                             .Where(t => typeof(Exception).IsAssignableFrom(t))
    //                             .Select(t => new object[] { t });
    //    }
    //}
}