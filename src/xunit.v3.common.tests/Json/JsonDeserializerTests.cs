using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;

public class JsonDeserializerTests
{
	[Fact]
	public void GuardClause()
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
	public void IllegalJson_ReturnsFalse(string json)
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
		{ "\"\\\"\\\\\\b\\f\\n\\r\\t\"", "\"\\\b\f\n\r\t" },
	};

	[Theory]
	[MemberData(nameof(IntrinsicValueData))]
	public void CanDeserializeIntrinsicValues(
		string json,
		object? expected)
	{
		var success = JsonDeserializer.TryDeserialize(json, out var result);

		Assert.True(success);
		Assert.Equal(expected, result);
	}

	[Theory]
	[MemberData(nameof(IntrinsicValueData))]
	public void CanDeserializeArrays(
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
	[MemberData(nameof(IntrinsicValueData))]
	public void CanDeserializeObjectsAsDictionaries(
		string itemJson,
		object? expectedItem)
	{
		var json = $"{{ \"nullValue\" : null, \"item\" : {itemJson}, \"array\" : [42, 21.12] }}";

		var success = JsonDeserializer.TryDeserialize(json, out var result);

		Assert.True(success);
		var obj = Assert.IsAssignableFrom<IDictionary<string, object?>>(result);
		Assert.Equal(["array", "item", "nullValue"], obj.Keys.OrderBy(x => x));
		Assert.Null(obj["nullValue"]);
		Assert.Equal(expectedItem, obj["item"]);
		Assert.Equal(new decimal[] { 42m, 21.12m }, obj["array"]);
	}
}
