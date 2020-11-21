using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.v2;

public class XunitSerializationInfoTests
{
	public static IEnumerable<object?[]> SupportedIntrinsics
	{
		get
		{
			var nonZeroLowerBoundSingleDimensionArray = Array.CreateInstance(typeof(int), new int[] { 1 }, new int[] { 2 });
			var nonZeroLowerBoundMultiDimensionArray = Array.CreateInstance(typeof(int), new int[] { 1, 2 }, new int[] { 3, 4 });

			return new TheoryData<Type, object?>
			{
				{ typeof(char), char.MaxValue },
				{ typeof(char?), char.MinValue },
				{ typeof(char?), null },
				{ typeof(string), "Hello, world!" },
				{ typeof(string), "" },
				{ typeof(string), null },
				{ typeof(byte), byte.MaxValue },
				{ typeof(byte?), byte.MinValue },
				{ typeof(byte?), null },
				{ typeof(sbyte), sbyte.MaxValue },
				{ typeof(sbyte?), sbyte.MinValue },
				{ typeof(sbyte?), null },
				{ typeof(short), short.MaxValue },
				{ typeof(short?), short.MinValue },
				{ typeof(short?), null },
				{ typeof(ushort), ushort.MaxValue },
				{ typeof(ushort?), ushort.MinValue },
				{ typeof(ushort?), null },
				{ typeof(int), int.MaxValue },
				{ typeof(int?), int.MinValue },
				{ typeof(int?), null },
				{ typeof(uint), uint.MaxValue },
				{ typeof(uint?), uint.MinValue },
				{ typeof(uint?), null },
				{ typeof(long), long.MaxValue },
				{ typeof(long?), long.MinValue },
				{ typeof(long?), null },
				{ typeof(ulong), ulong.MaxValue },
				{ typeof(ulong?), ulong.MinValue },
				{ typeof(ulong?), null },
				{ typeof(float), float.MaxValue },
				{ typeof(float?), float.MinValue },
				{ typeof(float?), null },
				{ typeof(double), double.MaxValue },
				{ typeof(double), double.PositiveInfinity },
				{ typeof(double), double.NegativeInfinity },
				{ typeof(double), 0.0 },
				{ typeof(double), -0.0 },
				{ typeof(double), double.NaN },
				{ typeof(double?), double.MinValue },
				{ typeof(double?), null },
				{ typeof(decimal), decimal.MaxValue },
				{ typeof(decimal?), decimal.MinValue },
				{ typeof(decimal?), null },
				{ typeof(bool), true },
				{ typeof(bool?), false },
				{ typeof(bool?), null },
				{ typeof(MyEnum), MyEnum.SomeValue },
				{ typeof(MyEnum?), MyEnum.SomeValue },
				{ typeof(MyEnum?), null },
				{ typeof(DateTime), DateTime.Now },
				{ typeof(DateTime?), DateTime.UtcNow },
				{ typeof(DateTime?), null },
				{ typeof(DateTimeOffset), DateTimeOffset.Now },
				{ typeof(DateTimeOffset?), DateTimeOffset.UtcNow },
				{ typeof(DateTimeOffset?), null },
				{ typeof(Type), typeof(object) },
				{ typeof(Type), null },
				{ typeof(MyEnum[]), new MyEnum[] { MyEnum.SomeValue, MyEnum.OtherValue } },
				{ typeof(object?[]), new object?[] { char.MaxValue, byte.MaxValue, short.MinValue, ushort.MaxValue, int.MinValue, uint.MaxValue, long.MinValue, ulong.MaxValue, null, "", 1.1f, -2.2, decimal.MaxValue, true, MyEnum.SomeValue, DateTime.Now, DateTimeOffset.UtcNow, typeof(decimal) } },
				{ typeof(int[,]), new int[,] { { 1, 2 }, { 3, 4 } } },
				{ typeof(int[,]), new int[,] { { 1, 2, 3 }, { 4, 5, 6 } } },
				{ typeof(int[,,]), new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 4, 5 }, { 6, 7 } } } },
				{ nonZeroLowerBoundSingleDimensionArray.GetType(), nonZeroLowerBoundSingleDimensionArray },
				{ nonZeroLowerBoundMultiDimensionArray.GetType(), nonZeroLowerBoundMultiDimensionArray },
			};
		}
	}

	enum MyEnum { SomeValue, OtherValue }

	public class Serialize
	{
		[Theory]
		[MemberData("SupportedIntrinsics", MemberType = typeof(XunitSerializationInfoTests))]
		public static void CanRoundTripIntrinsics(Type dataType, object? data)
		{
			Assert.Equal(data, XunitSerializationInfo.Deserialize(dataType, XunitSerializationInfo.Serialize(data)));
		}

		[Theory]
		[InlineData(0.0, false)]
		[InlineData(-0.0, true)]
		public void SerializesNegativeZeroFloatCorrectly(
			double value,
			bool isNegativeZero)
		{
			var expected = isNegativeZero ? -0.0 : 0.0;
			var expectedBits = BitConverter.DoubleToInt64Bits(expected);
			var actualBits = BitConverter.DoubleToInt64Bits(value);

			Assert.Equal(expectedBits, actualBits);
		}

		[Theory]
		[InlineData(0.0f, false)]
		[InlineData(-0.0f, true)]
		public void SerializesNegativeZeroDoubleCorrectly(
			float value,
			bool isNegativeZero)
		{
			var expected = isNegativeZero ? -0.0f : 0.0f;
			var expectedBytes = BitConverter.GetBytes(expected);
			var actualBytes = BitConverter.GetBytes(value);

			Assert.Equal(expectedBytes, actualBytes);
		}

		[Fact]
		public static void CanRoundTypeIXunitSerializable()
		{
			var data = new MySerializable { IntValue = 42, StringValue = "Hello\r\nworld!" };

			var deserialized = (MySerializable?)XunitSerializationInfo.Deserialize(typeof(MySerializable), XunitSerializationInfo.Serialize(data));

			Assert.NotNull(deserialized);
			Assert.Equal(42, deserialized.IntValue);
			Assert.Equal("Hello\r\nworld!", deserialized.StringValue);
		}

		[Fact]
		public static void CanRoundTripIXunitSerializableWithNoSerializedData()
		{
			var data = new MySerializableWithNoData();

			var serialized = XunitSerializationInfo.Serialize(data);
			var deserialized = (MySerializableWithNoData?)XunitSerializationInfo.Deserialize(typeof(MySerializableWithNoData), serialized);

			Assert.NotNull(deserialized);
		}

		[Fact]
		public static void CanRoundTripEmbeddedIXunitSerializableWithNoSerializedData()
		{
			var data = new MySerializableWithEmbeddedEmptySerializable { NoData = new MySerializableWithNoData() };

			var serialized = XunitSerializationInfo.Serialize(data);
			var deserialized = (MySerializableWithEmbeddedEmptySerializable?)XunitSerializationInfo.Deserialize(typeof(MySerializableWithEmbeddedEmptySerializable), serialized);

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
			Assert.SkipUnless(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "The GAC is only available on Windows");

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

			Assert.Equal("Xunit.Runner.v2.XunitSerializationInfo, xunit.v3.common", result);
		}

#if NETFRAMEWORK
		[Fact]
		public static void CannotSerializeTypeFromGAC()
		{
			Assert.SkipUnless(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "The GAC is only available on Windows");

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

			var deserializedInner = (MySerializable?)deserialized.Value;
			Assert.NotNull(deserializedInner);
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
		public string? StringValue;

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
		public MySerializableWithNoData? NoData { get; set; }

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
