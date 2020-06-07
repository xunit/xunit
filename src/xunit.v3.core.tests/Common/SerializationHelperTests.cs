using System;
using Xunit;
using Xunit.Sdk;

public class SerializationHelperTests
{
    public class GetTypeNameForSerialization
    {
        [Theory]
        // Types in mscorlib show up as simple names
        [InlineData(typeof(object), "System.Object")]
        // Open generic types can be round-tripped
        [InlineData(typeof(TestCaseRunner<>), "Xunit.Sdk.TestCaseRunner`1, xunit.v3.core")]
        // Types outside of mscorlib include their assembly name
        [InlineData(typeof(FactAttribute), "Xunit.FactAttribute, xunit.v3.core")]
        // Types in platform-specific libraries show up with substitution tokens
        [InlineData(typeof(XunitTestFramework), "Xunit.Sdk.XunitTestFramework, xunit.v3.core")]
        // Array types
        [InlineData(typeof(FactAttribute[]), "Xunit.FactAttribute[], xunit.v3.core")]
        // Array of arrays with multi-dimensions
        [InlineData(typeof(FactAttribute[][,]), "Xunit.FactAttribute[,][], xunit.v3.core")]
        // Single-nested generic type (both in mscorlib)
        [InlineData(typeof(Action<object>), "System.Action`1[[System.Object]]")]
        // Single-nested generic type (non-mscorlib)
        [InlineData(typeof(TestMethodRunner<XunitTestCase>), "Xunit.Sdk.TestMethodRunner`1[[Xunit.Sdk.XunitTestCase, xunit.v3.core]], xunit.v3.core")]
        // Multiply-nested generic types
        [InlineData(typeof(Action<Tuple<object, FactAttribute>, XunitTestFramework>), "System.Action`2[[System.Tuple`2[[System.Object],[Xunit.FactAttribute, xunit.v3.core]]],[Xunit.Sdk.XunitTestFramework, xunit.v3.core]]")]
        // Generics and arrays, living together, like cats and dogs
        [InlineData(typeof(Action<XunitTestCase[,][]>[][,]), "System.Action`1[[Xunit.Sdk.XunitTestCase[][,], xunit.v3.core]][,][]")]
        public static void CanRoundTripSerializedTypeNames(Type type, string expectedName)
        {
            var name = SerializationHelper.GetTypeNameForSerialization(type);

            Assert.Equal(expectedName, name);

            var deserializedType = SerializationHelper.GetType(name);

            Assert.Same(type, deserializedType);
        }

#if NETFRAMEWORK
        [Fact(Skip = "Not working on Linux")]
        public static void CannotRoundTripTypesFromTheGAC()
        {
            var ex = Assert.Throws<ArgumentException>("type", () => SerializationHelper.GetTypeNameForSerialization(typeof(Uri)));

            Assert.StartsWith("We cannot serialize type System.Uri because it lives in the GAC", ex.Message);
        }
#endif
    }

    public class IsSerializable
    {
        [Fact(Skip = "Not working on Linux")]
        public void TypeSerialization()
        {
            Assert.True(SerializationHelper.IsSerializable(typeof(string)));               // Can serialization types from mscorlib
            Assert.True(SerializationHelper.IsSerializable(typeof(SerializationHelper)));  // Can serialize types from local libraries
#if NETFRAMEWORK
            Assert.False(SerializationHelper.IsSerializable(typeof(Uri)));                 // Can't serialize types from the GAC
#endif
        }
    }
}
