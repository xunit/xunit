using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;
using Xunit.Sdk;

#if NETFRAMEWORK
using System.Runtime.InteropServices;
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
		// Floats and doubles are converted to byte[] and then serialized
		{ 21.12f, $"10:{ToBase64("t:-4:System.Byte\nr:6:1\ntl:6:4\nl0:6:4\nlb0:6:0\ni0:2:195\ni1:2:245\ni2:2:168\ni3:2:65")}" },
		{ 21.12d, $"11:{ToBase64("t:-4:System.Byte\nr:6:1\ntl:6:8\nl0:6:8\nlb0:6:0\ni0:2:31\ni1:2:133\ni2:2:235\ni3:2:81\ni4:2:184\ni5:2:30\ni6:2:53\ni7:2:64")}" },
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
		{ new Version(1, 2, 3, 4), "20:1.2.3.4" },
#if NET6_0_OR_GREATER
		{ new Index(42, fromEnd: true), "21:^42" },
		{ new Range(10, new Index(10, fromEnd: true)), "22:10..^10" },
#endif
		{ new Guid("cbe55b7a-51ad-4e97-a3d9-e41e1db75364"), "23:cbe55b7a51ad4e97a3d9e41e1db75364" },
		{ new Uri("https://xunit.net/"), $"24:{ToBase64("https://xunit.net/")}" },  // Absolute
		{ new Uri("a/b#c", UriKind.Relative), $"24:{ToBase64("a/b#c")}" },          // Relative

		// Types are serialized as their type name
		{ typeof(string), "-4:System.String" },

		// Enums, IXunitSerializable types, and types supported by IXunitSerializer contain embedded type information in addition to a type index
		{ new MySerializable(1, "2", 3.4m), $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MySerializable)))}:{ToBase64($"p1:6:1\np2:0:{ToBase64("2")}\np3:12:3.4")}" },
		{ MyEnum.MyValue, $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyEnum)))}:{ToBase64("123")}" },
		{ (MyEnum)int.MinValue, $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyEnum)))}:{ToBase64("-2147483648")}" },
		{ (MyEnum)int.MaxValue, $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyEnum)))}:{ToBase64("2147483647")}" },
		{ (MyUnsignedEnum)ulong.MinValue, $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyUnsignedEnum)))}:{ToBase64("0")}" },
		{ (MyUnsignedEnum)ulong.MaxValue, $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyUnsignedEnum)))}:{ToBase64("18446744073709551615")}" },
		{ new MyCustomType { Age = 42, Name = "Someone" }, $"-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyCustomType)))}:{ToBase64("42:Someone")}" },

		// Trait dictionaries are serialized as a keys list and values arrays
		{
			new Dictionary<string, HashSet<string>>
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

		// Arrays use array notation for embedded types, plus this serialization format:
		//   t = array type, r = ranks, tl = total length, l[n] = length of rank n, lb[n] = lower bound of rank n, i[n] = item[n]
		{ new[] { 1, 2, 3 }, $"[]:{ToBase64("t:-4:System.Int32\nr:6:1\ntl:6:3\nl0:6:3\nlb0:6:0\ni0:6:1\ni1:6:2\ni2:6:3")}" },
		{ new int?[] { 1, null, 3 }, $"[]:{ToBase64("t:-4:System.Nullable`1[[System.Int32]]\nr:6:1\ntl:6:3\nl0:6:3\nlb0:6:0\ni0:6:1\ni1:6\ni2:6:3")}" },
		{ new[] { MyEnum.MyValue }, $"[]:{ToBase64($"t:-4:{SerializationHelper.TypeToSerializedTypeName(typeof(MyEnum))}\nr:6:1\ntl:6:1\nl0:6:1\nlb0:6:0\ni0:-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyEnum)))}:{ToBase64("123")}")}" },
		{ new[] { new MyCustomType { Age = 42, Name = "Someone" } }, $"[]:{ToBase64($"t:-4:{SerializationHelper.TypeToSerializedTypeName(typeof(MyCustomType))}\nr:6:1\ntl:6:1\nl0:6:1\nlb0:6:0\ni0:-3:{ToBase64(SerializationHelper.TypeToSerializedTypeName(typeof(MyCustomType)))}:{ToBase64("42:Someone")}")}" },

		// Object arrays are allowed to hold any serializable data
		{ new object?[] { 1, "2", 3.4m, null }, $"[]:{ToBase64($"t:-4:System.Object\nr:6:1\ntl:6:4\nl0:6:4\nlb0:6:0\ni0:6:1\ni1:0:{ToBase64("2")}\ni2:12:3.4\ni3:-1")}" },
	};

	public static TheoryData<Type, string> NullSuccessData = new()
	{
		{ typeof(Type), "-4" },
		{ typeof(MyCustomType), "-3" },
		{ typeof(MySerializable), "-3" },
		{ typeof(MyEnum?), "-3" },
		{ typeof(Dictionary<string, HashSet<string>>), "-2" },
		{ typeof(object), "-1" },
		{ typeof(string), "0" },
		{ typeof(char?), "1" },
		{ typeof(byte?), "2" },
		{ typeof(sbyte?), "3" },
		{ typeof(short?), "4" },
		{ typeof(ushort?), "5" },
		{ typeof(int?), "6" },
		{ typeof(uint?), "7" },
		{ typeof(long?), "8" },
		{ typeof(ulong?), "9" },
		{ typeof(float?), "10" },
		{ typeof(double?), "11" },
		{ typeof(decimal?), "12" },
		{ typeof(bool?), "13" },
		{ typeof(DateTime?), "14" },
		{ typeof(DateTimeOffset?), "15" },
		{ typeof(TimeSpan?), "16" },
		{ typeof(BigInteger?), "17" },
#if NET6_0_OR_GREATER
		{ typeof(DateOnly?), "18" },
		{ typeof(TimeOnly?), "19" },
#endif
		{ typeof(Version), "20" },
#if NET6_0_OR_GREATER
		{ typeof(Index?), "21" },
		{ typeof(Range?), "22" },
#endif
		{ typeof(Guid?), "23" },
		{ typeof(Uri), "24" },
	};

	public class Deserialize
	{
		[Fact]
		public void GuardClauseForNullSerializedValue()
		{
			var ex = Record.Exception(() => TestableSerializationHelper.Instance.Deserialize(null!));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("serializedValue", argnEx.ParamName);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("abc:123")]
		public void GuardClauseForUnknownTypeIndex(string value)
		{
			var ex = Record.Exception(() => TestableSerializationHelper.Instance.Deserialize(value));

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
			var result = TestableSerializationHelper.Instance.Deserialize(serialization);

			Assert.Null(result);
		}

		[Fact]
		public void ArraysCanBeNull()
		{
			var result = TestableSerializationHelper.Instance.Deserialize("[]");

			Assert.Null(result);
		}

		[CulturedTheory("en-US", "fo-FO", DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(NonNullSuccessData), MemberType = typeof(SerializationHelperTests))]
		public void NonNullSuccessCases<T>(
			T? expectedValue,
			string serialization)
		{
			var result = TestableSerializationHelper.Instance.Deserialize<T>(serialization);

			Assert.Equivalent(expectedValue, result);
		}

#if NETFRAMEWORK
		[Theory]
		[InlineData("18:738526", "DateOnly")]
		[InlineData("18", "DateOnly")]
		[InlineData("19:326550000000", "TimeOnly")]
		[InlineData("19", "TimeOnly")]
		public void UnsupportedPlatform(
			string value,
			string typeName)
		{
			var ex = Record.Exception(() => TestableSerializationHelper.Instance.Deserialize(value));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("serializedValue", argEx.ParamName);
			Assert.StartsWith($"Cannot deserialize value of '{typeName}': unsupported platform", argEx.Message);
		}

		public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		// Index and Range are available on Mono, but not on .NET Framework on Windows
		[Theory(Skip = "This test is only supported on Windows", SkipUnless = nameof(IsWindows))]
		[InlineData("21:123", "Index")]
		[InlineData("21", "Index")]
		[InlineData("22:1..2", "Range")]
		[InlineData("22", "Range")]
		public void UnsupportedPlatform_Windows(
			string value,
			string typeName)
		{
			var ex = Record.Exception(() => TestableSerializationHelper.Instance.Deserialize(value));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("serializedValue", argEx.ParamName);
			Assert.StartsWith($"Cannot deserialize value of '{typeName}': unsupported platform", argEx.Message);
		}
#endif
	}

	public class IsSerializable
	{
		public static TheoryData<Type> SupportedTypes =
		[
			typeof(Type),
			typeof(IXunitSerializable),
			typeof(Dictionary<string, HashSet<string>>),
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
			typeof(Version),
#if NET6_0_OR_GREATER
			typeof(Index),
			typeof(Range),
#endif
			typeof(Guid),
			typeof(Uri),
			// Registered into TestableSerializationHelper by default
			typeof(MyEnum),
			typeof(MyCustomType),
		];

		[CulturedTheory("en-US", "fo-FO")]
		[MemberData(nameof(SupportedTypes), DisableDiscoveryEnumeration = true)]
		public void SuccessCases(Type type)
		{
			Assert.True(TestableSerializationHelper.Instance.IsSerializable(null, type));

			if (type.IsValueType)
			{
				var nullableType = typeof(Nullable<>).MakeGenericType(type);

				Assert.True(TestableSerializationHelper.Instance.IsSerializable(null, nullableType));
			}
		}

		[Fact]
		public void CanSerializeRuntimeType()
		{
			// Type is abstract; RuntimeType is what you get at runtime and since it's
			// internal, we can't just call typeof() to get one
			var type = 42.GetType().GetType();

			Assert.True(TestableSerializationHelper.Instance.IsSerializable(null, type));
		}

		[Fact]
		public void CannotSerializeGenericArgumentType()
		{
			var value = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.GenericMethod))!.GetGenericArguments()[0];
			var type = value.GetType();

			Assert.False(TestableSerializationHelper.Instance.IsSerializable(value, type));
		}

		class ClassWithGenericMethod
		{
			public void GenericMethod<U>() { }
		}

		[Fact]
		public void CannotSerializeGenericArgumentTypeInsideArray()
		{
			Type[] value =
			[
				// Okay
				typeof(Type),
				// Not okay
				typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.GenericMethod))!.GetGenericArguments()[0],
			];
			var type = value.GetType();

			Assert.False(TestableSerializationHelper.Instance.IsSerializable(value, type));
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
			var result = TestableSerializationHelper.Instance.Serialize(null, nullableType);

			Assert.Equal(expectedSerialization, result);
		}

		[Theory]
		[InlineData(typeof(object[]))]
		[InlineData(typeof(int?[]))]
		public void ArraysCanBeNull(Type arrayType)
		{
			var result = TestableSerializationHelper.Instance.Serialize(null, arrayType);

			Assert.Equal("[]", result);
		}

		[CulturedTheory("en-US", "fo-FO", DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(NonNullSuccessData), MemberType = typeof(SerializationHelperTests))]
		public void NonNullSuccessCases<T>(
			T? value,
			string? expectedSerialization)
		{
			var result = TestableSerializationHelper.Instance.Serialize(value);

			Assert.Equal(expectedSerialization, result);
		}

#pragma warning disable xUnit1047

		public static IEnumerable<TheoryDataRow<object?, Type, string>> FailureData()
		{
#if NETFRAMEWORK
			// GAC'd enums can't be serialized (Mono doesn't have a GAC, so skip it there)
			if (!EnvironmentHelper.IsMono)
				yield return new(ConformanceLevel.Auto, typeof(ConformanceLevel), "Cannot serialize type 'System.Xml.ConformanceLevel' because it lives in the GAC");
#endif

			// Unsupported built-in types can't be serialized
			yield return new(new Exception(), typeof(Exception), "Cannot serialize a value of type 'System.Exception': unsupported type for serialization");

			// Custom types which aren't IXunitSerializable can't be serialized
			yield return new(new SerializationHelperTests(), typeof(SerializationHelperTests), "Cannot serialize a value of type 'SerializationHelperTests': unsupported type for serialization");

			// Non-null value, incompatible type)
			yield return new(new object(), typeof(MyEnum), "Cannot serialize a value of type 'System.Object' as type 'SerializationHelperTests+MyEnum' because it's type-incompatible");

			// Null value, non-nullable type
			yield return new(null, typeof(int), "Cannot serialize a null value as type 'System.Int32' because it's type-incompatible");

			// Object is a special case: can only be serialized as null
			yield return new(new object(), typeof(object), "Cannot serialize a non-null value of type 'System.Object'");

			// Cannot serialize generic argument types
			yield return new(typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.GenericMethod))!.GetGenericArguments()[0], typeof(Type), "Cannot serialize typeof(U) because it has no full name");
		}

#pragma warning restore xUnit1047

		class ClassWithGenericMethod
		{
			public void GenericMethod<U>() { }
		}

		[CulturedTheory("en-US", "fo-FO", DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(FailureData))]
		public void FailureCases(
			object? value,
			Type valueType,
			string expectedExceptionMessage)
		{
			var ex = Record.Exception(() => TestableSerializationHelper.Instance.Serialize(value, valueType));

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

	sealed class MyCustomType
	{
		public required int Age { get; set; }
		public required string Name { get; set; }
	}

	class MyCustomTypeSerializer : IXunitSerializer
	{
		public object Deserialize(
			Type type,
			string serializedValue)
		{
			var pieces = serializedValue?.Split([':'], 2);
			if (pieces is null || pieces.Length != 2)
				throw new ArgumentException($"Improperly formatted serialized value '{serializedValue ?? "null"}'");

			return new MyCustomType { Age = int.Parse(pieces[0]), Name = pieces[1] };
		}

		public bool IsSerializable(
			Type type,
			object? value) =>
				type == typeof(MyCustomType);

		public string Serialize(object value)
		{
			if (value is not MyCustomType myCustomType)
				throw new ArgumentException($"Tried to serialize a value of type '{value?.GetType().FullName ?? "(null)"}'");

			return $"{myCustomType.Age}:{myCustomType.Name}";
		}
	}

	class TestableSerializationHelper : SerializationHelper
	{
		public TestableSerializationHelper(params IRegisterXunitSerializerAttribute[] serializers) =>
			AddSerializers(serializers, Warnings);

		public List<string> Warnings { get; } = [];

		public new static TestableSerializationHelper Instance { get; } = new(new RegisterXunitSerializerAttribute(typeof(MyCustomTypeSerializer), typeof(MyCustomType)));
	}
}
