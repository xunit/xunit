using Xunit;
using Xunit.Sdk;

partial class TypeHelperTests
{
	public class ConvertArgument
	{
		[Theory]
		[InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
		[InlineData("4EBCD32C-A2B8-4600-9E72-3873347E285C")]
		[InlineData("39A3B4C85FEF43A988EB4BB4AC4D4103")]
		[InlineData("{5b21e154-15eb-4b1e-bc30-127e8a41eca1}")]
		[InlineData("4ebcd32c-a2b8-4600-9e72-3873347e285c")]
		[InlineData("39a3b4c85fef43a988eb4bb4ac4d4103")]
		public void ConvertsStringToGuid(string text)
		{
			var guid = Guid.Parse(text);

			var arg = TypeHelper.ConvertArgument(text, typeof(Guid));

			Assert.Equal(guid, Assert.IsType<Guid>(arg));
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public void ConvertsStringToDateTime(string text)
		{
			var dateTime = DateTime.Parse(text, CultureInfo.InvariantCulture);

			var arg = TypeHelper.ConvertArgument(text, typeof(DateTime));

			Assert.Equal(dateTime, Assert.IsType<DateTime>(arg));
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public void ConvertsStringToDateTimeOffset(string text)
		{
			var dateTimeOffset = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);

			var arg = TypeHelper.ConvertArgument(text, typeof(DateTimeOffset));

			Assert.Equal(dateTimeOffset, Assert.IsType<DateTimeOffset>(arg));
		}
	}

	public class ConvertArguments
	{
		[Theory]
		[InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
		[InlineData("4EBCD32C-A2B8-4600-9E72-3873347E285C")]
		[InlineData("39A3B4C85FEF43A988EB4BB4AC4D4103")]
		[InlineData("{5b21e154-15eb-4b1e-bc30-127e8a41eca1}")]
		[InlineData("4ebcd32c-a2b8-4600-9e72-3873347e285c")]
		[InlineData("39a3b4c85fef43a988eb4bb4ac4d4103")]
		public void ConvertsStringToGuid(string text)
		{
			var guid = Guid.Parse(text);

			var args = TypeHelper.ConvertArguments([text], [typeof(Guid)]);

			Assert.Equal(guid, Assert.IsType<Guid>(args[0]));
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public void ConvertsStringToDateTime(string text)
		{
			var dateTime = DateTime.Parse(text, CultureInfo.InvariantCulture);

			var args = TypeHelper.ConvertArguments([text], [typeof(DateTime)]);

			Assert.Equal(dateTime, Assert.IsType<DateTime>(args[0]));
		}

		[Theory]
		[InlineData("2017-11-3")]
		[InlineData("2017-11-3 16:48")]
		[InlineData("16:48")]
		public void ConvertsStringToDateTimeOffset(string text)
		{
			var dateTimeOffset = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);

			var args = TypeHelper.ConvertArguments([text], [typeof(DateTimeOffset)]);

			Assert.Equal(dateTimeOffset, Assert.IsType<DateTimeOffset>(args[0]));
		}
	}
}
