using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;
using Xunit.Sdk;

#if NETFRAMEWORK
using System.Xml;
#endif

public class SerializationHelperTests
{
	public static TheoryData<object?, string> NonNullSuccessData = new()
	{
		// Supported built-in types just contain a type index and the value
		{ "String value", $"0:{ToBase64("String value")}" },
		{ 'a', "1:97" },
		{ (byte)1, "2:1" },
		{ (sbyte)2, "3:2" },
		{ (short)3, "4:3" },
		{ (ushort)4, "5:4" },
		{ 5, "6:5" },
		{ 6U, "7:6" },
		{ 7L, "8:7" },
		{ 8UL, "9:8" },
		{ 21.12f, $"10:{SerializationHelper.Serialize(BitConverter.GetBytes(21.12f))}" },
		{ 21.12d, $"11:{SerializationHelper.Serialize(BitConverter.GetBytes(21.12d))}" },
		{ 21.12m, "12:21.12" },
		{ true, "13:True" },
		{ new DateTime(2022, 4, 21, 23, 18, 19, 20, DateTimeKind.Utc), "14:2022-04-21T23:18:19.0200000Z" },
		{ new DateTimeOffset(2022, 4, 21, 23, 19, 20, 21, TimeSpan.Zero), "15:2022-04-21T23:19:20.0210000+00:00" },
		{ new TimeSpan(1, 2, 3, 4, 5), "16:1.02:03:04.0050000" },
		{ BigInteger.Parse("123456789009876543210123456789"), "17:123456789009876543210123456789" },
#if NET6_0_OR_GREATER
		{ new DateOnly(2023, 1, 7), "18:738526" },
		{ new TimeOnly(9, 4, 15), "19:326550000000" },
#endif

		// Arrays use array notation for embedded types, plus this serialization format:
		//   r = ranks, tl = total length, l[n] = length of rank n, lb[n] = lower bound of rank n, i[n] = item[n]
		{ new[] { 1, 2, 3 }, $"6[]:{ToBase64("r:6:1\ntl:6:3\nl0:6:3\nlb0:6:0\ni0:6:1\ni1:6:2\ni2:6:3")}" },
		{ new int?[] { 1, null, 3 }, $"6?[]:{ToBase64("r:6:1\ntl:6:3\nl0:6:3\nlb0:6:0\ni0:6:1\ni1:6?\ni2:6:3")}" },

		// Types are serialized as their type name
		{ typeof(string), "-5:System.String" },

		// IXunitSerializable and enums contain embedded type information in addition to a type index
#if BUILD_X86
		{ new MySerializable(1, "2", 3.4m), $"-4:{ToBase64("SerializationHelperTests+MySerializable,xunit.v3.common.tests.x86")}:{ToBase64($"p1:6:1\np2:0:{ToBase64("2")}\np3:12:3.4")}" },
		{ MyEnum.MyValue, $"-3:{ToBase64("SerializationHelperTests+MyEnum,xunit.v3.common.tests.x86")}:123" },
		{ (MyEnum)int.MinValue, $"-3:{ToBase64("SerializationHelperTests+MyEnum,xunit.v3.common.tests.x86")}:-2147483648" },
		{ (MyEnum)int.MaxValue, $"-3:{ToBase64("SerializationHelperTests+MyEnum,xunit.v3.common.tests.x86")}:2147483647" },
		{ (MyUnsignedEnum)ulong.MinValue, $"-3:{ToBase64("SerializationHelperTests+MyUnsignedEnum,xunit.v3.common.tests.x86")}:0" },
		{ (MyUnsignedEnum)ulong.MaxValue, $"-3:{ToBase64("SerializationHelperTests+MyUnsignedEnum,xunit.v3.common.tests.x86")}:18446744073709551615" },
#else
		{ new MySerializable(1, "2", 3.4m), $"-4:{ToBase64("SerializationHelperTests+MySerializable,xunit.v3.common.tests")}:{ToBase64($"p1:6:1\np2:0:{ToBase64("2")}\np3:12:3.4")}" },
		{ MyEnum.MyValue, $"-3:{ToBase64("SerializationHelperTests+MyEnum,xunit.v3.common.tests")}:123" },
		{ (MyEnum)int.MinValue, $"-3:{ToBase64("SerializationHelperTests+MyEnum,xunit.v3.common.tests")}:-2147483648" },
		{ (MyEnum)int.MaxValue, $"-3:{ToBase64("SerializationHelperTests+MyEnum,xunit.v3.common.tests")}:2147483647" },
		{ (MyUnsignedEnum)ulong.MinValue, $"-3:{ToBase64("SerializationHelperTests+MyUnsignedEnum,xunit.v3.common.tests")}:0" },
		{ (MyUnsignedEnum)ulong.MaxValue, $"-3:{ToBase64("SerializationHelperTests+MyUnsignedEnum,xunit.v3.common.tests")}:18446744073709551615" },
#endif

		// Trait dictionaries are serialized as a keys list and values arrays
		{
			new Dictionary<string, List<string>>
			{
				{ "foo", new() { "bar", string.Empty } },
				{ "biff", new() { "hello" } }
			},
			$"-2:" +
			ToBase64(
				$"{ToBase64($"{ToBase64("foo")}\n{ToBase64("biff")}")}\n" +
				$"{ToBase64($"{ToBase64("bar")}\n")}\n" +
				$"{ToBase64($"{ToBase64("hello")}")}"
			)
		},

		// Object arrays are allowed to hold any serializable data
		{ new object?[] { 1, "2", 3.4m, null }, $"-1[]:{ToBase64($"r:6:1\ntl:6:4\nl0:6:4\nlb0:6:0\ni0:6:1\ni1:0:{ToBase64("2")}\ni2:12:3.4\ni3:-1")}" },
	};

	public static TheoryData<Type, string> NullSuccessData = new()
	{
		{ typeof(Type), "-5" },
		{ typeof(MySerializable), "-4" },
		{ typeof(MyEnum?), "-3?" },
		{ typeof(Dictionary<string, List<string>>), "-2" },
		{ typeof(object), "-1" },
		{ typeof(string), "0" },
		{ typeof(char?), "1?" },
		{ typeof(byte?), "2?" },
		{ typeof(sbyte?), "3?" },
		{ typeof(short?), "4?" },
		{ typeof(ushort?), "5?" },
		{ typeof(int?), "6?" },
		{ typeof(uint?), "7?" },
		{ typeof(long?), "8?" },
		{ typeof(ulong?), "9?" },
		{ typeof(float?), "10?" },
		{ typeof(double?), "11?" },
		{ typeof(decimal?), "12?" },
		{ typeof(bool?), "13?" },
		{ typeof(DateTime?), "14?" },
		{ typeof(DateTimeOffset?), "15?" },
		{ typeof(TimeSpan?), "16?" },
		{ typeof(BigInteger?), "17?" },
#if NET6_0_OR_GREATER
		{ typeof(DateOnly?), "18?" },
		{ typeof(TimeOnly?), "19?" },
#endif
	};

	public class Deserialize
	{
		[Fact]
		public void GuardClauseForNullSerializedValue()
		{
			var ex = Record.Exception(() => SerializationHelper.Deserialize(null!));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("serializedValue", argnEx.ParamName);
		}

		[Theory]
		[InlineData("abc?")]
		[InlineData("abc:123")]
		[InlineData("abc[]:def")]
		public void GuardClauseForUnknownTypeIndex(string value)
		{
			var ex = Record.Exception(() => SerializationHelper.Deserialize(value));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("serializedValue", argEx.ParamName);
			Assert.StartsWith("Tried to deserialize unknown type index 'abc'", argEx.Message);
		}

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(NullSuccessData), MemberType = typeof(SerializationHelperTests), DisableDiscoveryEnumeration = true)]
		public void NullSuccessCases(
			Type _,
			string serialization)
		{
			var result = SerializationHelper.Deserialize(serialization);

			Assert.Null(result);
		}

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(NullSuccessData), MemberType = typeof(SerializationHelperTests), DisableDiscoveryEnumeration = true)]
		public void NullSuccessCasesAsArrays(
			Type _,
			string serialization)
		{
			var result = SerializationHelper.Deserialize(serialization + "[]");

			Assert.Null(result);
		}

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(NonNullSuccessData), MemberType = typeof(SerializationHelperTests))]
		public void NonNullSuccessCases<T>(
			T? expectedValue,
			string serialization)
		{
			var result = SerializationHelper.Deserialize<T>(serialization);

			Assert.Equivalent(expectedValue, result);
		}

#if NETFRAMEWORK
		[Theory]
		[InlineData("18:738526", "DateOnly")]
		[InlineData("18?", "DateOnly")]
		[InlineData("19:326550000000", "TimeOnly")]
		[InlineData("19?", "TimeOnly")]
		public void UnsupportedPlatform(
			string value,
			string typeName)
		{
			var ex = Record.Exception(() => SerializationHelper.Deserialize(value));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("serializedValue", argEx.ParamName);
			Assert.StartsWith($"Cannot deserialize value of '{typeName}': unsupported platform", argEx.Message);
		}
#endif
	}

	public class IsSerializable
	{
		public static TheoryData<Type> SupportedTypes = new()
		{
			typeof(Enum),
			typeof(IXunitSerializable),
			typeof(Dictionary<string, List<string>>),
			typeof(object),
			typeof(string),
			typeof(char),
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(bool),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
			typeof(BigInteger),
#if NET6_0_OR_GREATER
			typeof(DateOnly),
			typeof(TimeOnly),
#endif
		};

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(SupportedTypes), DisableDiscoveryEnumeration = true)]
		public void SuccessCases(Type type)
		{
			Assert.True(SerializationHelper.IsSerializable(null, type));
			Assert.True(SerializationHelper.IsSerializable(null, Reflector.Wrap(type)));

			if (type.IsValueType)
			{
				var nullableType = typeof(Nullable<>).MakeGenericType(type);

				Assert.True(SerializationHelper.IsSerializable(null, nullableType));
				Assert.True(SerializationHelper.IsSerializable(null, Reflector.Wrap(nullableType)));
			}
		}

		[Fact]
		public void CanSerializeRuntimeType()
		{
			// Type is abstract; RuntimeType is what you get at runtime and since it's
			// internal, we can't just call typeof() to get one
			var type = 42.GetType().GetType();

			Assert.True(SerializationHelper.IsSerializable(null, type));
			Assert.True(SerializationHelper.IsSerializable(null, Reflector.Wrap(type)));
		}
	}

	public class Serialize
	{
		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(NullSuccessData), MemberType = typeof(SerializationHelperTests), DisableDiscoveryEnumeration = true)]
		public void NullSuccessCases(
			Type nullableType,
			string? expectedSerialization)
		{
			var result = SerializationHelper.Serialize(null, nullableType);

			Assert.Equal(expectedSerialization, result);
		}

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(NullSuccessData), MemberType = typeof(SerializationHelperTests), DisableDiscoveryEnumeration = true)]
		public void NullSuccessCasesAsArrays(
			Type nullableType,
			string? expectedSerialization)
		{
			var result = SerializationHelper.Serialize(null, nullableType.MakeArrayType());

			Assert.Equal(expectedSerialization + "[]", result);
		}

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(NonNullSuccessData), MemberType = typeof(SerializationHelperTests))]
		public void NonNullSuccessCases<T>(
			T? value,
			string? expectedSerialization)
		{
			var result = SerializationHelper.Serialize(value);

			Assert.Equal(expectedSerialization, result);
		}

		public static TheoryData<object?, Type, string> FailureData()
		{
			var result = new TheoryData<object?, Type, string>();

#if NETFRAMEWORK
			// GAC'd enums can't be serialized (Mono doesn't have a GAC, so skip it there)
			if (!EnvironmentHelper.IsMono)
				result.Add(ConformanceLevel.Auto, typeof(ConformanceLevel), "Cannot serialize enum 'System.Xml.ConformanceLevel.Auto' because it lives in the GAC");
#endif

			// Unsupported built-in types can't be serialized
			result.Add(new Exception(), typeof(Exception), "Cannot serialize a value of type 'System.Exception': unsupported type for serialization");

			// Custom types which aren't IXunitSerializable can't be serialized
			result.Add(new SerializationHelperTests(), typeof(SerializationHelperTests), "Cannot serialize a value of type 'SerializationHelperTests': unsupported type for serialization");

			// Non-null value, incompatible type)
			result.Add(new object(), typeof(MyEnum), "Cannot serialize a value of type 'System.Object' as type 'SerializationHelperTests+MyEnum' because it's type-incompatible");

			// Null value, non-nullable type
			result.Add(null, typeof(int), "Cannot serialize a null value as type 'System.Int32' because it's type-incompatible");

			// Object is a special case: can only be serialized as null
			result.Add(new object(), typeof(object), "Cannot serialize a non-null value of type 'System.Object'");

			return result;
		}

		[CulturedTheory("en-US", "fo-FO", DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(FailureData))]
		public void FailureCases(
			object? value,
			Type valueType,
			string expectedExceptionMessage)
		{
			var ex = Record.Exception(() => SerializationHelper.Serialize(value, valueType));

			Assert.NotNull(ex);
			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("value", argEx.ParamName);
			Assert.StartsWith(expectedExceptionMessage, ex.Message);
		}
	}

	public class TypeNameSerialization
	{
		public static TheoryData<Type, string> TypeSerializationData = new()
		{
			// Types in mscorlib show up as simple names
			{ typeof(object), "System.Object" },
			// Types outside of mscorlib include their assembly name
			{ typeof(FactAttribute), "Xunit.FactAttribute,xunit.v3.core" },
			// Array types
			{ typeof(FactAttribute[]), "Xunit.FactAttribute[],xunit.v3.core" },
			// Array of arrays with multi-dimensions
			{ typeof(FactAttribute[][,]), "Xunit.FactAttribute[,][],xunit.v3.core" },
			// Open generic type
			{ typeof(IEnumerable<>), "System.Collections.Generic.IEnumerable`1" },
			// Single-nested generic type (both in mscorlib)
			{ typeof(Action<object>), "System.Action`1[[System.Object]]" },
			// Single-nested generic type (non-mscorlib)
			{ typeof(TheoryData<FactAttribute>), "Xunit.TheoryData`1[[Xunit.FactAttribute,xunit.v3.core]],xunit.v3.core" },
			// Multiply-nested generic types
			{ typeof(Action<Tuple<object, FactAttribute>, string>), "System.Action`2[[System.Tuple`2[[System.Object],[Xunit.FactAttribute,xunit.v3.core]]],[System.String]]" },
			// Generics and arrays, living together, like cats and dogs
			{ typeof(Action<FactAttribute[,][]>[][,]), "System.Action`1[[Xunit.FactAttribute[][,],xunit.v3.core]][,][]" },
		};

		[Theory]
		[MemberData(nameof(TypeSerializationData))]
		public void TypeToSerializedName(
			Type type,
			string expectedName)
		{
			var name = SerializationHelper.TypeToSerializedTypeName(type);

			Assert.Equal(expectedName, name);
		}

		[Theory]
		[MemberData(nameof(TypeSerializationData))]
		public void SerializedTypeNameToType(
			Type expectedType,
			string name)
		{
			var deserializedType = SerializationHelper.SerializedTypeNameToType(name);

			Assert.Same(expectedType, deserializedType);
		}
	}

	static string ToBase64(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		return Convert.ToBase64String(bytes);
	}

	enum MyEnum
	{
		MyValue = 123
	}

	enum MyUnsignedEnum : ulong
	{ }

	class MySerializable : IXunitSerializable
	{
		public int P1;
		public string? P2;
		public decimal P3;

		public MySerializable()
		{ }

		public MySerializable(
			int p1,
			string p2,
			decimal p3)
		{
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			P1 = info.GetValue<int>("p1");
			P2 = info.GetValue<string>("p2");
			P3 = info.GetValue<decimal>("p3");
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue("p1", P1);
			info.AddValue("p2", P2);
			info.AddValue("p3", P3);
		}
	}
}
