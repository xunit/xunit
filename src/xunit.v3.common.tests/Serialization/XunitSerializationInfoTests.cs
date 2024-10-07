using System;
using System.Text;
using Xunit;
using Xunit.Sdk;

public class XunitSerializationInfoTests
{
	public class Ctor
	{
		[Fact]
		public void GuardClause()
		{
			var ex = Record.Exception(() => new XunitSerializationInfo(SerializationHelper.Instance, default(IXunitSerializable)!));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("object", argnEx.ParamName);
		}

		[Theory]
		[InlineData(default(string))]
		[InlineData("")]
		public void EmptySerializedValueResultsInEmptySerialization(string? serialization)
		{
			var info = new XunitSerializationInfo(SerializationHelper.Instance, serialization!);

			Assert.Empty(info.ToSerializedString());
		}

		[Fact]
		public void InitializedWithSerializedValue()
		{
			var info = new XunitSerializationInfo(SerializationHelper.Instance, $"ch:1:97\nst:0:{ToBase64("Hello world")}\ndec:12:21.12");

			Assert.Equal('a', info.GetValue<char>("ch"));
			Assert.Equal("Hello world", info.GetValue<string>("st"));
			Assert.Equal(21.12m, info.GetValue<decimal>("dec"));
		}

		[Fact]
		public void EntryMissingDataThrowsDuringConstructor()
		{
			var serialization = "ch:1:97\nst\ndec:12:21.12";

			var ex = Record.Exception(() => new XunitSerializationInfo(SerializationHelper.Instance, serialization));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("serializedValue", argEx.ParamName);
			Assert.StartsWith($"Serialized piece 'st' is malformed. Full serialization:{Environment.NewLine}{serialization}", argEx.Message);
		}
	}

	public class AddValue
	{
		[Fact]
		public void GuardClauseForNonSerializableData()
		{
			var info = new XunitSerializationInfo(SerializationHelper.Instance);

			var ex = Record.Exception(() => info.AddValue("v", new MyClass()));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("value", argEx.ParamName);
			Assert.StartsWith("Cannot serialize a value of type 'XunitSerializationInfoTests+MyClass': unsupported type for serialization", argEx.Message);
		}
	}

	public class ToSerializedString
	{
		[Fact]
		public void EmptyInfo()
		{
			var info = new XunitSerializationInfo(SerializationHelper.Instance);

			var result = info.ToSerializedString();

			Assert.Empty(result);
		}

		// Indices in this list match the supported index list in SerializationHelper
		public static TheoryData<object?, Type> SuccessData = new()
		{
			// Char value
			{ 'a', typeof(char) },
			{ 'a', typeof(char?) },
			{ null, typeof(char?) },

			// String value
			{ "Hello", typeof(string) },
			{ null, typeof(string) },

			// Byte values
			{ (byte)1, typeof(byte) },
			{ (byte)2, typeof(byte?) },
			{ null, typeof(byte?) },

			// Signed byte values
			{ (sbyte)1, typeof(sbyte) },
			{ (sbyte)2, typeof(sbyte?) },
			{ null, typeof(sbyte?) },

			// Short values
			{ (short)1, typeof(short) },
			{ (short)2, typeof(short?) },
			{ null, typeof(short?) },

			// Unsigned short values
			{ (ushort)1, typeof(ushort) },
			{ (ushort)2, typeof(ushort?) },
			{ null, typeof(ushort?) },

			// Int values
			{ 1, typeof(int) },
			{ 2, typeof(int?) },
			{ null, typeof(int?) },

			// Unsigned int values
			{ 1U, typeof(uint) },
			{ 2U, typeof(uint?) },
			{ null, typeof(uint?) },

			// Long values
			{ 1L, typeof(long) },
			{ 2L, typeof(long?) },
			{ null, typeof(long?) },

			// Unsigned long values
			{ 1UL, typeof(ulong) },
			{ 2UL, typeof(ulong?) },
			{ null, typeof(ulong?) },

			// Float values
			{ 21.12f, typeof(float) },
			{ 21.12f, typeof(float?) },
			{ null, typeof(float?) },

			// Double values
			{ 21.12d, typeof(double) },
			{ 21.12d, typeof(double?) },
			{ null, typeof(double?) },

			// Decimal values
			{ 21.12m, typeof(decimal) },
			{ 21.12m, typeof(decimal?) },
			{ null, typeof(decimal?) },

			// Boolean values
			{ true, typeof(bool) },
			{ false, typeof(bool?) },
			{ null, typeof(bool?) },

			// DateTime values
			{ new DateTime(2022, 4, 21, 23, 18, 19, 20, DateTimeKind.Utc), typeof(DateTime) },
			{ new DateTime(2022, 4, 21, 23, 18, 19, 20, DateTimeKind.Utc), typeof(DateTime?) },
			{ null, typeof(DateTime?) },

			// DateTimeOffset values
			{ new DateTimeOffset(2022, 4, 21, 23, 19, 20, 21, TimeSpan.Zero), typeof(DateTimeOffset) },
			{ new DateTimeOffset(2022, 4, 21, 23, 19, 20, 21, TimeSpan.Zero), typeof(DateTimeOffset?) },
			{ null, typeof(DateTimeOffset?) },

			// Enum values
			{ MyEnum.MyValue, typeof(MyEnum) },
			{ MyEnum.MyValue, typeof(MyEnum?) },
			{ null, typeof(MyEnum?) },

			// IXunitSerializable (special handling as '-2' with encoded type)
			{ new MySerializable(1, "2", 3.4m), typeof(MySerializable) },
			{ null, typeof(MySerializable) },
		};

		[Theory]
		[MemberData(nameof(SuccessData), DisableDiscoveryEnumeration = true)]
		public void SuccessCases(
			object? value,
			Type valueType)
		{
			var info = new XunitSerializationInfo(SerializationHelper.Instance);
			info.AddValue("v", value, valueType);

			var result = info.ToSerializedString();

			var serialization = SerializationHelper.Instance.Serialize(value, valueType);
			Assert.Equal($"v:{serialization}", result);
		}

		[Theory]
		[MemberData(nameof(SuccessData), DisableDiscoveryEnumeration = true)]
		public void SuccessCasesAsArrays(
			object? value,
			Type valueType)
		{
			var info = new XunitSerializationInfo(SerializationHelper.Instance);
			var arrayType = valueType.MakeArrayType();
			var array = Array.CreateInstance(valueType, 1);
			array.SetValue(value, 0);
			info.AddValue("v", array, arrayType);

			var result = info.ToSerializedString();

			var serialization = SerializationHelper.Instance.Serialize(array, arrayType);
			Assert.Equal($"v:{serialization}", result);
		}
	}

	static string FromBase64(string value)
	{
		var bytes = Convert.FromBase64String(value);
		return Encoding.UTF8.GetString(bytes);
	}

	static string ToBase64(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		return Convert.ToBase64String(bytes);
	}

	class MyClass { }

	enum MyEnum
	{
		MyValue = 123
	}

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
