using System;
using Xunit;
using Xunit.Sdk;

public class SerializationHelperTests
{
    [Theory]
    // Types in mscorlib show up as simple names
    [InlineData(typeof(object), "System.Object")]
    // Types outside of mscorlib include their assembly name
    [InlineData(typeof(FactAttribute), "Xunit.FactAttribute, xunit.core")]
    // Types in device-specific execution library show up in xunit.execution
    [InlineData(typeof(XunitTestFramework), "Xunit.Sdk.XunitTestFramework, xunit.execution")]
    // Array types
    [InlineData(typeof(FactAttribute[]), "Xunit.FactAttribute[], xunit.core")]
    // Array of arrays with multi-dimensions
    [InlineData(typeof(FactAttribute[][,]), "Xunit.FactAttribute[,][], xunit.core")]
    // Single-nested generic type (both in mscorlib)
    [InlineData(typeof(Action<object>), "System.Action`1[[System.Object]]")]
    // Single-nested generic type (non-mscorlib)
    [InlineData(typeof(TestMethodRunner<XunitTestCase>), "Xunit.Sdk.TestMethodRunner`1[[Xunit.Sdk.XunitTestCase, xunit.execution]], xunit.execution")]
    // Multiply-nested generic types
    [InlineData(typeof(Action<Tuple<object, FactAttribute>, XunitTestFramework>), "System.Action`2[[System.Tuple`2[[System.Object],[Xunit.FactAttribute, xunit.core]]],[Xunit.Sdk.XunitTestFramework, xunit.execution]]")]
    // Generics and arrays, living together, like cats and dogs
    [InlineData(typeof(Action<XunitTestCase[,][]>[][,]), "System.Action`1[[Xunit.Sdk.XunitTestCase[][,], xunit.execution]][,][]")]
    public static void CanRoundTripSerializedTypeNames(Type type, string expectedName)
    {
        var name = SerializationHelper.GetTypeNameForSerialization(type);

        Assert.Equal(expectedName, name);

        var deserializedType = SerializationHelper.GetType(name);

        Assert.Same(type, deserializedType);
    }
}
