using System;
using System.Collections.Generic;
using System.Net;
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
            yield return new object[] { typeof(string), "Hello, world!" };
            yield return new object[] { typeof(string), "" };
            yield return new object[] { typeof(string), null };
            yield return new object[] { typeof(int), int.MaxValue };
            yield return new object[] { typeof(int?), int.MinValue };
            yield return new object[] { typeof(int?), null };
            yield return new object[] { typeof(long), long.MaxValue };
            yield return new object[] { typeof(long?), long.MinValue };
            yield return new object[] { typeof(long?), null };
            yield return new object[] { typeof(float), 1.1f };
            yield return new object[] { typeof(float?), -1.1f };
            yield return new object[] { typeof(float?), null };
            yield return new object[] { typeof(double), 2.2 };
            yield return new object[] { typeof(double?), -2.2 };
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
            yield return new object[] { typeof(object[]), new object[] { int.MinValue, long.MaxValue, null, "", 1.1f, -2.2, decimal.MaxValue, true, MyEnum.SomeValue, DateTime.Now, DateTimeOffset.UtcNow, typeof(decimal) } };
        }
    }

    enum MyEnum { SomeValue }

    public class Serialize
    {
        [Theory]
        [MemberData("SupportedIntrinsics", MemberType = typeof(XunitSerializationInfoTests))]
        public static void CanRoundTripIntrinsics(Type dataType, object data)
        {
            Assert.Equal(data, XunitSerializationInfo.Deserialize(dataType, XunitSerializationInfo.Serialize(data)));
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

        [Fact]
        public static void CannotSerializeEnumFromGAC()
        {
            var data = ConformanceLevel.Auto;

            var ex = Record.Exception(() => XunitSerializationInfo.Serialize(data));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We cannot serialize enum System.Xml.ConformanceLevel.Auto because it lives in the GAC", argEx.Message);
        }

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

        [Fact]
        public static void CannotSerializeTypeFromGAC()
        {
            var data = typeof(XmlDocument);

            var ex = Record.Exception(() => XunitSerializationInfo.Serialize(data));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("value", argEx.ParamName);
            Assert.StartsWith("We cannot serialize type System.Xml.XmlDocument because it lives in the GAC", argEx.Message);
        }
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
}
