using System;
using System.Collections.Generic;
using System.Xml;
using Xunit;
using Xunit.Abstractions;
using Xunit.Serialization;

public class XunitSerializationInfoTests
{
    public static IEnumerable<object[]> SupportedIntrinsics
    {
        get
        {
            yield return new object[] { typeof(char), char.MaxValue };
            yield return new object[] { typeof(char?), char.MinValue };
            yield return new object[] { typeof(char?), null };
            yield return new object[] { typeof(string), "Hello, world!" };
            yield return new object[] { typeof(string), "" };
            yield return new object[] { typeof(string), null };
            yield return new object[] { typeof(byte), byte.MaxValue };
            yield return new object[] { typeof(byte?), byte.MinValue };
            yield return new object[] { typeof(byte?), null };
            yield return new object[] { typeof(sbyte), sbyte.MaxValue };
            yield return new object[] { typeof(sbyte?), sbyte.MinValue };
            yield return new object[] { typeof(sbyte?), null };
            yield return new object[] { typeof(short), short.MaxValue };
            yield return new object[] { typeof(short?), short.MinValue };
            yield return new object[] { typeof(short?), null };
            yield return new object[] { typeof(ushort), ushort.MaxValue };
            yield return new object[] { typeof(ushort?), ushort.MinValue };
            yield return new object[] { typeof(ushort?), null };
            yield return new object[] { typeof(int), int.MaxValue };
            yield return new object[] { typeof(int?), int.MinValue };
            yield return new object[] { typeof(int?), null };
            yield return new object[] { typeof(uint), uint.MaxValue };
            yield return new object[] { typeof(uint?), uint.MinValue };
            yield return new object[] { typeof(uint?), null };
            yield return new object[] { typeof(long), long.MaxValue };
            yield return new object[] { typeof(long?), long.MinValue };
            yield return new object[] { typeof(long?), null };
            yield return new object[] { typeof(ulong), ulong.MaxValue };
            yield return new object[] { typeof(ulong?), ulong.MinValue };
            yield return new object[] { typeof(ulong?), null };
            yield return new object[] { typeof(float), float.MaxValue };
            yield return new object[] { typeof(float?), float.MinValue };
            yield return new object[] { typeof(float?), null };
            yield return new object[] { typeof(double), double.MaxValue };
            yield return new object[] { typeof(double), double.PositiveInfinity };
            yield return new object[] { typeof(double), double.NegativeInfinity };
            yield return new object[] { typeof(double), 0.0 };
            yield return new object[] { typeof(double), -0.0 };
            yield return new object[] { typeof(double), double.NaN };
            yield return new object[] { typeof(double?), double.MinValue };
            yield return new object[] { typeof(double?), null };
            yield return new object[] { typeof(decimal), decimal.MaxValue };
            yield return new object[] { typeof(decimal?), decimal.MinValue };
            yield return new object[] { typeof(decimal?), null };
            yield return new object[] { typeof(bool), true };
            yield return new object[] { typeof(bool?), false };
            yield return new object[] { typeof(bool?), null };
            yield return new object[] { typeof(MyEnum), MyEnum.SomeValue };
            yield return new object[] { typeof(MyEnum?), MyEnum.SomeValue };
            yield return new object[] { typeof(MyEnum?), null };
            yield return new object[] { typeof(DateTime), DateTime.Now };
            yield return new object[] { typeof(DateTime?), DateTime.UtcNow };
            yield return new object[] { typeof(DateTime?), null };
            yield return new object[] { typeof(DateTimeOffset), DateTimeOffset.Now };
            yield return new object[] { typeof(DateTimeOffset?), DateTimeOffset.UtcNow };
            yield return new object[] { typeof(DateTimeOffset?), null };
            yield return new object[] { typeof(Type), typeof(object) };
            yield return new object[] { typeof(Type), null };
            yield return new object[] { typeof(MyEnum[]), new MyEnum[] { MyEnum.SomeValue, MyEnum.OtherValue } };
            yield return new object[] { typeof(object[]), new object[] { char.MaxValue, byte.MaxValue, short.MinValue, ushort.MaxValue, int.MinValue, uint.MaxValue, long.MinValue, ulong.MaxValue, null, "", 1.1f, -2.2, decimal.MaxValue, true, MyEnum.SomeValue, DateTime.Now, DateTimeOffset.UtcNow, typeof(decimal) } };
            yield return new object[] { typeof(int[,]), new int[,] { { 1, 2 }, { 3, 4 } } };
            yield return new object[] { typeof(int[,]), new int[,] { { 1, 2, 3 }, { 4, 5, 6 } } };
            yield return new object[] { typeof(int[,,]), new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 4, 5 }, { 6, 7 } } } };

            Array nonZeroLowerBoundSingleDimensionArray = Array.CreateInstance(typeof(int), new int[] { 1 }, new int[] { 2 });
            yield return new object[] { nonZeroLowerBoundSingleDimensionArray.GetType(), nonZeroLowerBoundSingleDimensionArray };

            Array nonZeroLowerBoundMultiDimensionArray = Array.CreateInstance(typeof(int), new int[] { 1, 2 }, new int[] { 3, 4 });
            yield return new object[] { nonZeroLowerBoundMultiDimensionArray.GetType(), nonZeroLowerBoundMultiDimensionArray };
        }
    }

    enum MyEnum { SomeValue, OtherValue }

    public class Serialize
    {
        [Theory]
        [MemberData("SupportedIntrinsics", MemberType = typeof(XunitSerializationInfoTests))]
        public static void CanRoundTripIntrinsics(Type dataType, object data)
        {
            Assert.Equal(data, XunitSerializationInfo.Deserialize(dataType, XunitSerializationInfo.Serialize(data)));
        }

        [Theory]
        [InlineData(0.0, false)]
        [InlineData(-0.0, true)]
        public void SerializesNegativeZeroFloatCorrectly(double value, bool isNegativeZero)
        {
            double expected = isNegativeZero ? -0.0 : 0.0;
            long expectedBits = BitConverter.DoubleToInt64Bits(expected);
            long actualBits = BitConverter.DoubleToInt64Bits(value);

            Assert.Equal(expectedBits, actualBits);
        }

        [Theory]
        [InlineData(0.0f, false)]
        [InlineData(-0.0f, true)]
        public void SerializesNegativeZeroDoubleCorrectly(float value, bool isNegativeZero)
        {
            float expected = isNegativeZero ? -0.0f : 0.0f;
            byte[] expectedBytes = BitConverter.GetBytes(expected);
            byte[] actualBytes = BitConverter.GetBytes(value);

            Assert.Equal(expectedBytes, actualBytes);
        }

        [Fact]
        public static void CanRoundTypeIXunitSerializable()
        {
            var data = new MySerializable { IntValue = 42, StringValue = "Hello\r\nworld!" };

            var deserialized = (MySerializable)XunitSerializationInfo.Deserialize(typeof(MySerializable), XunitSerializationInfo.Serialize(data));

            Assert.Equal(42, deserialized.IntValue);
            Assert.Equal("Hello\r\nworld!", deserialized.StringValue);
        }

        [Fact]
        public static void CanRoundTripIXunitSerializableWithNoSerializedData()
        {
            var data = new MySerializableWithNoData();

            var serialized = XunitSerializationInfo.Serialize(data);
            var deserialized = (MySerializableWithNoData)XunitSerializationInfo.Deserialize(typeof(MySerializableWithNoData), serialized);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public static void CanRoundTripEmbeddedIXunitSerializableWithNoSerializedData()
        {
            var data = new MySerializableWithEmbeddedEmptySerializable { NoData = new MySerializableWithNoData() };

            var serialized = XunitSerializationInfo.Serialize(data);
            var deserialized = (MySerializableWithEmbeddedEmptySerializable)XunitSerializationInfo.Deserialize(typeof(MySerializableWithEmbeddedEmptySerializable), serialized);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.NoData);
        }

        [Fact]
        public static void IXunitSerializableWithoutParameterlessConstructorThrows()
        {
            var data = new MySerializableWithoutParameterlessConstructor(42);

            var serialized = XunitSerializationInfo.Serialize(data);
            var ex = Record.Exception(() => XunitSerializationInfo.Deserialize(typeof(MySerializableWithoutParameterlessConstructor), serialized));

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("Could not de-serialize type 'XunitSerializationInfoTests+MySerializableWithoutParameterlessConstructor' because it lacks a parameterless constructor.", ex.Message);
        }

        [Fact]
        public static void UnsupportedTypeThrows()
        {
            var data = new object();

            var ex = Record.Exception(() => XunitSerializationInfo.Serialize(data));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We don't know how to serialize type System.Object", argEx.Message);
        }

        [Fact]
        public static void CanSerializeEnumFromMscorlib()
        {
            var data = Base64FormattingOptions.InsertLineBreaks;

            var result = XunitSerializationInfo.Serialize(data);

            Assert.Equal("InsertLineBreaks", result);
        }

        [Fact]
        public static void CanSerializeEnumFromLocalAssembly()
        {
            var data = MyEnum.SomeValue;

            var result = XunitSerializationInfo.Serialize(data);

            Assert.Equal("SomeValue", result);
        }

#if NETFRAMEWORK
        [Fact]
        public static void CannotSerializeEnumFromGAC()
        {
            var data = ConformanceLevel.Auto;

            var ex = Record.Exception(() => XunitSerializationInfo.Serialize(data));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We cannot serialize enum System.Xml.ConformanceLevel.Auto because it lives in the GAC", argEx.Message);
        }
#endif

        [Fact]
        public static void CanSerializeTypeFromMscorlib()
        {
            var data = typeof(string);

            var result = XunitSerializationInfo.Serialize(data);

            Assert.Equal("System.String", result);
        }

        [Fact]
        public static void CanSerializeTypeFromLocalAssembly()
        {
            var data = typeof(XunitSerializationInfo);

            var result = XunitSerializationInfo.Serialize(data);

            Assert.Equal("Xunit.Serialization.XunitSerializationInfo, test.xunit.execution", result);
        }

#if NETFRAMEWORK
        [Fact]
        public static void CannotSerializeTypeFromGAC()
        {
            var data = typeof(XmlDocument);

            var ex = Record.Exception(() => XunitSerializationInfo.Serialize(data));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("type", argEx.ParamName);
            Assert.StartsWith("We cannot serialize type System.Xml.XmlDocument because it lives in the GAC", argEx.Message);
        }
#endif
    }

    public class SerializeTriple
    {
        [Theory]
        [MemberData("SupportedIntrinsics", MemberType = typeof(XunitSerializationInfoTests))]
        public static void CanRoundTripIntrinsics(Type dataType, object data)
        {
            var triple = new XunitSerializationTriple(Guid.NewGuid().ToString(), data, dataType);
            var serialized = XunitSerializationInfo.SerializeTriple(triple);
            var deserialized = XunitSerializationInfo.DeserializeTriple(serialized);

            Assert.Equal(triple.Key, deserialized.Key);
            Assert.Equal(triple.Type, deserialized.Type);
            Assert.Equal(triple.Value, deserialized.Value);
        }

        [Fact]
        public static void CanRoundTypeIXunitSerializable()
        {
            var data = new MySerializable { IntValue = 42, StringValue = "Hello\r\nworld!" };
            var triple = new XunitSerializationTriple(Guid.NewGuid().ToString(), data, data.GetType());
            var serialized = XunitSerializationInfo.SerializeTriple(triple);
            var deserialized = XunitSerializationInfo.DeserializeTriple(serialized);

            Assert.Equal(triple.Key, deserialized.Key);
            Assert.Equal(triple.Type, deserialized.Type);
            var deserializedInner = (MySerializable)deserialized.Value;
            Assert.Equal(42, deserializedInner.IntValue);
            Assert.Equal("Hello\r\nworld!", deserializedInner.StringValue);
        }

        [Fact]
        public static void UnsupportedTypeThrows()
        {
            var triple = new XunitSerializationTriple(Guid.NewGuid().ToString(), new object(), typeof(object));

            var ex = Record.Exception(() => XunitSerializationInfo.SerializeTriple(triple));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We don't know how to serialize type System.Object", argEx.Message);
        }
    }

    class MySerializable : IXunitSerializable
    {
        public int IntValue;
        public string StringValue;

        public void Deserialize(IXunitSerializationInfo info)
        {
            IntValue = info.GetValue<int>("IntValue");
            StringValue = info.GetValue<string>("StringValue");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("IntValue", IntValue);
            info.AddValue("StringValue", StringValue);
        }
    }

    class MySerializableWithNoData : IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info) { }

        public void Serialize(IXunitSerializationInfo info) { }
    }

    class MySerializableWithEmbeddedEmptySerializable : IXunitSerializable
    {
        public MySerializableWithNoData NoData { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            NoData = info.GetValue<MySerializableWithNoData>("NoData");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("NoData", NoData);
        }
    }

    class MySerializableWithoutParameterlessConstructor : IXunitSerializable
    {
        public MySerializableWithoutParameterlessConstructor(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Value = info.GetValue<int>(nameof(Value));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value), Value);
        }
    }
}
