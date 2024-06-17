using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using SerializationHelper = Xunit.Sdk.SerializationHelper;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;

#if NET6_0
using System.Numerics;
#endif

public class SerializationHelperTests
{
    public class GetTypeNameForSerialization
    {
        [Theory]
        // Types in mscorlib show up as simple names
        [InlineData(typeof(object), "System.Object")]
        // Open generic types can be round-tripped
        [InlineData(typeof(TestCaseRunner<>), "Xunit.Sdk.TestCaseRunner`1, xunit.execution.{Platform}")]
        // Types outside of mscorlib include their assembly name
        [InlineData(typeof(FactAttribute), "Xunit.FactAttribute, xunit.core")]
        // Types in platform-specific libraries show up with substitution tokens
        [InlineData(typeof(XunitTestFramework), "Xunit.Sdk.XunitTestFramework, xunit.execution.{Platform}")]
        // Array types
        [InlineData(typeof(FactAttribute[]), "Xunit.FactAttribute[], xunit.core")]
        // Array of arrays with multi-dimensions
        [InlineData(typeof(FactAttribute[][,]), "Xunit.FactAttribute[,][], xunit.core")]
        // Single-nested generic type (both in mscorlib)
        [InlineData(typeof(Action<object>), "System.Action`1[[System.Object]]")]
        // Single-nested generic type (non-mscorlib)
        [InlineData(typeof(TestMethodRunner<XunitTestCase>), "Xunit.Sdk.TestMethodRunner`1[[Xunit.Sdk.XunitTestCase, xunit.execution.{Platform}]], xunit.execution.{Platform}")]
        // Multiply-nested generic types
        [InlineData(typeof(Action<Tuple<object, FactAttribute>, XunitTestFramework>), "System.Action`2[[System.Tuple`2[[System.Object],[Xunit.FactAttribute, xunit.core]]],[Xunit.Sdk.XunitTestFramework, xunit.execution.{Platform}]]")]
        // Generics and arrays, living together, like cats and dogs
        [InlineData(typeof(Action<XunitTestCase[,][]>[][,]), "System.Action`1[[Xunit.Sdk.XunitTestCase[][,], xunit.execution.{Platform}]][,][]")]
        public static void CanRoundTripSerializedTypeNames(Type type, string expectedName)
        {
            var name = SerializationHelper.GetTypeNameForSerialization(type);

            Assert.Equal(expectedName, name);

            var deserializedType = SerializationHelper.GetType(name);

            Assert.Same(type, deserializedType);
        }

#if NETFRAMEWORK
        [Fact]
        public static void CannotRoundTripTypesFromTheGAC()
        {
            var ex = Assert.Throws<ArgumentException>("type", () => SerializationHelper.GetTypeNameForSerialization(typeof(Uri)));

            Assert.StartsWith("We cannot serialize type System.Uri because it lives in the GAC", ex.Message);
        }
#endif
    }

    public class Serialization
    {
        public static TheoryData<object, string> ValidValueData = new TheoryData<object, string>
        {
            { 'a', "System.Char:97" },
            { "a", "System.String:YQ==" },
            { (byte)42, "System.Byte:42" },
            { (sbyte)-42, "System.SByte:-42" },
            { (ushort)42, "System.UInt16:42" },
            { (short)-42, "System.Int16:-42" },
            { 42U, "System.UInt32:42" },
            { -42, "System.Int32:-42" },
            { 42UL, "System.UInt64:42" },
            { -42L, "System.Int64:-42" },
            { 42.2112F, "System.Single:RWxlbWVudFR5cGU6U3lzdGVtLlN0cmluZzpVM2x6ZEdWdExrSjVkR1U9ClJhbms6U3lzdGVtLkludDMyOjEKVG90YWxMZW5ndGg6U3lzdGVtLkludDMyOjQKTGVuZ3RoMDpTeXN0ZW0uSW50MzI6NApMb3dlckJvdW5kMDpTeXN0ZW0uSW50MzI6MApJdGVtMDpTeXN0ZW0uQnl0ZTo2OQpJdGVtMTpTeXN0ZW0uQnl0ZToyMTYKSXRlbTI6U3lzdGVtLkJ5dGU6NDAKSXRlbTM6U3lzdGVtLkJ5dGU6NjY=" },
            { 42.2112D, "System.Double:RWxlbWVudFR5cGU6U3lzdGVtLlN0cmluZzpVM2x6ZEdWdExrSjVkR1U9ClJhbms6U3lzdGVtLkludDMyOjEKVG90YWxMZW5ndGg6U3lzdGVtLkludDMyOjgKTGVuZ3RoMDpTeXN0ZW0uSW50MzI6OApMb3dlckJvdW5kMDpTeXN0ZW0uSW50MzI6MApJdGVtMDpTeXN0ZW0uQnl0ZTozNwpJdGVtMTpTeXN0ZW0uQnl0ZToxMTcKSXRlbTI6U3lzdGVtLkJ5dGU6MgpJdGVtMzpTeXN0ZW0uQnl0ZToxNTQKSXRlbTQ6U3lzdGVtLkJ5dGU6OApJdGVtNTpTeXN0ZW0uQnl0ZToyNwpJdGVtNjpTeXN0ZW0uQnl0ZTo2OQpJdGVtNzpTeXN0ZW0uQnl0ZTo2NA==" },
            { 42.2112M, "System.Decimal:42.2112" },
            { true, "System.Boolean:True" },
            { new DateTime(2112L), "System.DateTime:0001-01-01T00:00:00.0002112" },
            { new DateTimeOffset(2112L, TimeSpan.Zero), "System.DateTimeOffset:0001-01-01T00:00:00.0002112+00:00" },
            { new TimeSpan(3, 4, 5), "System.TimeSpan:03:04:05" },
#if NET6_0
            { new DateOnly(2023, 8, 9), "System.DateOnly:738740" },
            { new TimeOnly(21, 12, 42, 567), "System.TimeOnly:763625670000" },
            { new BigInteger(42), "System.Numerics.BigInteger, System.Runtime.Numerics:42" },
#endif
            { typeof(string), "System.Type:System.String" },
            { TestMethodDisplay.Method, "Xunit.Sdk.TestMethodDisplay, xunit.core:2" },
            { (TestMethodDisplay)int.MinValue, "Xunit.Sdk.TestMethodDisplay, xunit.core:-2147483648" },
            { (TestMethodDisplay)int.MaxValue, "Xunit.Sdk.TestMethodDisplay, xunit.core:2147483647" },
            { (MyUnsignedEnum)ulong.MinValue, "SerializationHelperTests+Serialization+MyUnsignedEnum, test.xunit.execution:0" },
            { (MyUnsignedEnum)ulong.MaxValue, "SerializationHelperTests+Serialization+MyUnsignedEnum, test.xunit.execution:18446744073709551615" },
            { new[] { 1, 2, 3 }, "System.Int32[]:RWxlbWVudFR5cGU6U3lzdGVtLlN0cmluZzpVM2x6ZEdWdExrbHVkRE15ClJhbms6U3lzdGVtLkludDMyOjEKVG90YWxMZW5ndGg6U3lzdGVtLkludDMyOjMKTGVuZ3RoMDpTeXN0ZW0uSW50MzI6MwpMb3dlckJvdW5kMDpTeXN0ZW0uSW50MzI6MApJdGVtMDpTeXN0ZW0uSW50MzI6MQpJdGVtMTpTeXN0ZW0uSW50MzI6MgpJdGVtMjpTeXN0ZW0uSW50MzI6Mw==" },
            { 42, "System.Int32:42" },
            { new MySerializable(42), "SerializationHelperTests+Serialization+MySerializable, test.xunit.execution:dmFsdWU6U3lzdGVtLkludDMyOjQy" },
        };

        [CulturedTheory("en-US", "fo-FO")]
        [MemberData(nameof(ValidValueData))]
        public void ValidValues<T>(T value, string serializedValue)
        {
            // Serialization
            var serialized = SerializationHelper.Serialize(value);

            Assert.Equal(serializedValue, serialized);

            // Deserialization
            var deserialized = SerializationHelper.Deserialize<T>(serializedValue);

            Assert.Equal(value, deserialized);
        }

        [Fact]
        public void InvalidValue()
        {
            var obj = new object();

            Assert.False(SerializationHelper.IsSerializable(obj));
            var ex = Record.Exception(() => SerializationHelper.Serialize(obj));
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We don't know how to serialize type System.Object", argEx.Message);
        }

        [Fact]
        public void CannotSerializeGenericArgumentType()
        {
            var type = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.GenericMethod)).GetGenericArguments()[0];

            Assert.False(SerializationHelper.IsSerializable(type));
            var ex = Record.Exception(() => SerializationHelper.Serialize(type));
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We don't know how to serialize value typeof(U) (no full name)", argEx.Message);
        }

        class ClassWithGenericMethod
        {
            public void GenericMethod<U>() { }
        }

        class MySerializable : IXunitSerializable, IEquatable<MySerializable>
        {
            int value;

            [Obsolete("For deserialization purposes only")]
            public MySerializable()
            { }

            public MySerializable(int value)
            {
                this.value = value;
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                value = info.GetValue<int>(nameof(value));
            }

            public bool Equals(MySerializable other)
            {
                return other != null && value == other.value;
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(value), value);
            }
        }

        enum MyUnsignedEnum : ulong
        { }
    }
}
