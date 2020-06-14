using System;
using Xunit;
using Xunit.Sdk;

public class SerializationHelperTests
{
    class GenericType<T> { }

    public class GetTypeNameForSerialization
    {
        [Theory]
        // Types in mscorlib show up as simple names
        [InlineData(typeof(object), "System.Object")]
        // Open generic types can be round-tripped
        [InlineData(typeof(GenericType<>), "SerializationHelperTests+GenericType`1, xunit.v2.tests")]
        // Types outside of mscorlib include their assembly name
        [InlineData(typeof(FactAttribute), "Xunit.FactAttribute, xunit.core")]
        // Array types
        [InlineData(typeof(FactAttribute[]), "Xunit.FactAttribute[], xunit.core")]
        // Array of arrays with multi-dimensions
        [InlineData(typeof(FactAttribute[][,]), "Xunit.FactAttribute[,][], xunit.core")]
        // Single-nested generic type (both in mscorlib)
        [InlineData(typeof(Action<object>), "System.Action`1[[System.Object]]")]
        // Single-nested generic type (non-mscorlib)
        [InlineData(typeof(GenericType<GetTypeNameForSerialization>), "SerializationHelperTests+GenericType`1[[SerializationHelperTests+GetTypeNameForSerialization, xunit.v2.tests]], xunit.v2.tests")]
        // Multiply-nested generic types
        [InlineData(typeof(Action<Tuple<object, FactAttribute>, GetTypeNameForSerialization>), "System.Action`2[[System.Tuple`2[[System.Object],[Xunit.FactAttribute, xunit.core]]],[SerializationHelperTests+GetTypeNameForSerialization, xunit.v2.tests]]")]
        // Generics and arrays, living together, like cats and dogs
        [InlineData(typeof(Action<GetTypeNameForSerialization[,][]>[][,]), "System.Action`1[[SerializationHelperTests+GetTypeNameForSerialization[][,], xunit.v2.tests]][,][]")]
        public static void CanRoundTripSerializedTypeNames(Type type, string expectedName)
        {
            var name = SerializationHelper.GetTypeNameForSerialization(type);

            Assert.Equal<object>(expectedName, name);

            var deserializedType = SerializationHelper.GetType(name);

            Assert.Same(type, deserializedType);
        }
    }

    public class IsSerializable
    {
        public void TypeSerialization()
        {
            Assert.True(SerializationHelper.IsSerializable(typeof(string)));               // Can serialization types from mscorlib
            Assert.True(SerializationHelper.IsSerializable(typeof(SerializationHelper)));  // Can serialize types from local libraries
        }
    }
}
