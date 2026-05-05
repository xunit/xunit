using System.Net;
using Xunit;
using Xunit.Sdk;

public static partial class TypeHelperTests
{
	public static class ConversionAcceptanceTests
	{
		[Theory]
		[InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
		public static void GuidAcceptanceTest(Guid actual)
		{
			var expected = Guid.Parse("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}");

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("2017-11-3 16:48")]
		public static void DateTimeAcceptanceTest(DateTime actual)
		{
			var expected = DateTime.Parse("2017-11-3 16:48", CultureInfo.InvariantCulture);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("2017-11-3 16:48")]
		public static void DateTimeOffsetAcceptanceTest(DateTimeOffset actual)
		{
			var expected = DateTimeOffset.Parse("2017-11-3 16:48", CultureInfo.InvariantCulture);

			Assert.Equal(expected, actual);
		}
	}

	public static class TryConvert
	{
		[Theory]
		[InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
		[InlineData("4EBCD32C-A2B8-4600-9E72-3873347E285C")]
		[InlineData("39A3B4C85FEF43A988EB4BB4AC4D4103")]
		[InlineData("{5b21e154-15eb-4b1e-bc30-127e8a41eca1}")]
		[InlineData("4ebcd32c-a2b8-4600-9e72-3873347e285c")]
		[InlineData("39a3b4c85fef43a988eb4bb4ac4d4103")]
		public static void ConvertsStringToGuid(string text)
		{
			var guid = Guid.Parse(text);

			var success = TypeHelper.TryConvert<Guid>(text, out var result);

			Assert.True(success);
			Assert.Equal(guid, result);
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public static void ConvertsStringToDateTime(string text)
		{
			var dateTime = DateTime.Parse(text, CultureInfo.InvariantCulture);

			var success = TypeHelper.TryConvert<DateTime>(text, out var result);

			Assert.True(success);
			Assert.Equal(dateTime, result);
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public static void ConvertsStringToDateTimeOffset(string text)
		{
			var dateTimeOffset = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);

			var success = TypeHelper.TryConvert<DateTimeOffset>(text, out var result);

			Assert.True(success);
			Assert.Equal(dateTimeOffset, result);
		}
	}

	public static class TryConvertNullable
	{
		[Fact]
		public static void SameClass()
		{
			var success = TypeHelper.TryConvertNullable<string>("Hello", out var result);

			Assert.True(success);
			Assert.Equal("Hello", result);
		}

		[Fact]
		public static void ConvertsStringToIntegralTypes()
		{
			void validate<T>(
				string value,
				T expected)
					where T : struct
			{
				var success = TypeHelper.TryConvertNullable<T>(value, out var result);

				Assert.True(success);
				Assert.Equal(expected, result);
			}

			validate("1", 1);
			validate("1", 1L);
			validate("1", 1U);
			validate("1", 1UL);
		}

		[Theory]
		[InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
		[InlineData("4EBCD32C-A2B8-4600-9E72-3873347E285C")]
		[InlineData("39A3B4C85FEF43A988EB4BB4AC4D4103")]
		[InlineData("{5b21e154-15eb-4b1e-bc30-127e8a41eca1}")]
		[InlineData("4ebcd32c-a2b8-4600-9e72-3873347e285c")]
		[InlineData("39a3b4c85fef43a988eb4bb4ac4d4103")]
		public static void ConvertsStringToGuid(string text)
		{
			var guid = Guid.Parse(text);

			var success = TypeHelper.TryConvertNullable<Guid>(text, out var result);

			Assert.True(success);
			Assert.Equal(guid, result);
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public static void ConvertsStringToDateTime(string text)
		{
			var dateTime = DateTime.Parse(text, CultureInfo.InvariantCulture);

			var success = TypeHelper.TryConvertNullable<DateTime>(text, out var result);

			Assert.True(success);
			Assert.Equal(dateTime, result);
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public static void ConvertsStringToDateTimeOffset(string text)
		{
			var dateTimeOffset = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);

			var success = TypeHelper.TryConvertNullable<DateTimeOffset>(text, out var result);

			Assert.True(success);
			Assert.Equal(dateTimeOffset, result);
		}

		[Theory]
		[InlineData(404, HttpStatusCode.NotFound)]
		[InlineData(null, null)]
		public static void ConvertsIntToEnum(
			int? value,
			HttpStatusCode? expected)
		{
			var success = TypeHelper.TryConvertNullable<HttpStatusCode>(value, out var result);

			Assert.True(success);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(42)]
		[InlineData(null)]
		public static void NullableValueType(int? value)
		{
			var success = TypeHelper.TryConvertNullable<int>(value, out var result);

			Assert.True(success);
			Assert.Equal(value, result);
		}
	}
}
