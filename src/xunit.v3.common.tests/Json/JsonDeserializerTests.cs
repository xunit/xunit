using Xunit;
using Xunit.Sdk;

public static class JsonDeserializerTests
{
	[Fact]
	public static void GuardClause()
	{
		Assert.Throws<ArgumentNullException>("json", () => JsonDeserializer.TryDeserialize(null!, out var _));
	}

	[Theory]
	[InlineData("")]
	[InlineData("foo")]
	[InlineData("undefined")]
	[InlineData("nul")]
	[InlineData("null :")]                  // Trailing garbage
	[InlineData("\"Hello")]                 // Unterminated string
	[InlineData("\"\\")]                    // Unterminated escape sequence
	[InlineData("\"Hello\r\nWorld\"")]      // Inline CRLF not allowed
	[InlineData("1a2")]                     // Invalid numeric value
	[InlineData("[1")]                      // No closing bracket
	[InlineData("[1,]")]                    // Illegal trailing comma
	[InlineData("{\"Hello\":\"World\"")]    // No closing brace
	[InlineData("{\"Hello\":\"World\",}")]  // Illegal trailing comma
	[InlineData("{\"Hello\"}")]             // Key without a value (without a colon)
	[InlineData("{\"Hello\":}")]            // Key without a value (with a colon)
	[InlineData("{12 : \"Hello\"}")]        // Non-string key
	public static void IllegalJson_ReturnsFalse(string json)
	{
		var success = JsonDeserializer.TryDeserialize(json, out var result);

		Assert.False(success);
		Assert.Null(result);
	}

	public static TheoryData<string, object?> IntrinsicValueData = new()
	{
		// null
		{ "null", null },

		// boolean
		{ " true", true },
		{ "false ", false },

		// numbers
		{ "42", 42m },
		{ " 21.12", 21.12m },
		{ "-2600 ", -2600m },
		{ "1.5E+3", 1500m },
		{ "1.5E-3", 0.0015m },

		// strings
		{ " \"Hello\"", "Hello" },
		{ "\"World\" ", "World" },
		{
			"\"\\u0000\\u0001\\u0002\\u0003\\u0004\\u0005\\u0006\\u0007\\b\\t\\n\\u000b\\f\\r\\u000e\\u000f\\u0010\\u0011\\u0012\\u0013\\u0014\\u0015\\u0016\\u0017\\u0018\\u0019\\u001a\\u001b\\u001c\\u001d\\u001e\\u001f\\\"\\\\\"",
			"\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F\"\\"
		},
	};

	[Theory]
	[MemberData(nameof(IntrinsicValueData), DisableDiscoveryEnumeration = true)]
	public static void CanDeserializeIntrinsicValues(
		string json,
		object? expected)
	{
		var success = JsonDeserializer.TryDeserialize(json, out var result);

		Assert.True(success);
		Assert.Equal(expected, result);
	}

	[Theory]
	[MemberData(nameof(IntrinsicValueData), DisableDiscoveryEnumeration = true)]
	public static void CanDeserializeArrays(
		string itemJson,
		object? expectedItem)
	{
		var json = $@"[
  null  ,
{itemJson}	, 21.12
]";

		var success = JsonDeserializer.TryDeserialize(json, out var result);

		Assert.True(success);
		var items = Assert.IsType<object?[]>(result);
		Assert.Equal([null, expectedItem, 21.12m], items);
	}

	[Theory]
	[MemberData(nameof(IntrinsicValueData), DisableDiscoveryEnumeration = true)]
	public static void CanDeserializeObjectsAsDictionaries(
		string itemJson,
		object? expectedItem)
	{
		var json = $"{{ \"nullValue\" : null, \"item\" : {itemJson}, \"array\" : [42, 21.12] }}";

		var success = JsonDeserializer.TryDeserialize(json, out var result);

		Assert.True(success);
		var obj = Assert.IsType<IDictionary<string, object?>>(result, exactMatch: false);
		Assert.Equal(["array", "item", "nullValue"], obj.Keys.OrderBy(x => x));
		Assert.Null(obj["nullValue"]);
		Assert.Equal(expectedItem, obj["item"]);
		Assert.Equal(new decimal[] { 42m, 21.12m }, obj["array"]);
	}
}
