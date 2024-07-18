using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Sdk;

public class JsonObjectSerializerTests
{
	public static IEnumerable<TheoryDataRow<Action<JsonObjectSerializer>, string>> ValueFuncs()
	{
		// Empty
		yield return new(s => { }, @"{}");

		// bool?
		yield return new(s => s.Serialize("value", true), @"{""value"":true}");
		yield return new(s => s.Serialize("value", default(bool?), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(bool?), includeNullValues: false), "{}");

		// DateTimeOffset?
		yield return new(s => s.Serialize("value", DateTimeOffset.MinValue), @"{""value"":""0001-01-01T00:00:00.0000000+00:00""}");
		yield return new(s => s.Serialize("value", default(DateTimeOffset?), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(DateTimeOffset?), includeNullValues: false), "{}");

		// decimal?
		yield return new(s => s.Serialize("value", 21.12m), @"{""value"":21.12}");
		yield return new(s => s.Serialize("value", default(decimal?), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(decimal?), includeNullValues: false), "{}");

		// Enum?
		yield return new(s => s.Serialize("value", TestMethodDisplay.Method), @"{""value"":""Method""}");
		yield return new(s => s.Serialize("value", default(TestMethodDisplay?), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(TestMethodDisplay?), includeNullValues: false), "{}");

		// int?
		yield return new(s => s.Serialize("value", int.MaxValue), @"{""value"":2147483647}");
		yield return new(s => s.Serialize("value", default(int?), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(int?), includeNullValues: false), "{}");

		// long?
		yield return new(s => s.Serialize("value", long.MaxValue), @"{""value"":9223372036854775807}");
		yield return new(s => s.Serialize("value", default(long?), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(long?), includeNullValues: false), "{}");

		// string?
		yield return new(s => s.Serialize("value", "Hello"), @"{""value"":""Hello""}");
		yield return new(s => s.Serialize("value", default(string), includeNullValues: true), @"{""value"":null}");
		yield return new(s => s.Serialize("value", default(string), includeNullValues: false), "{}");

		// int array, via JsonSerializerExtensions
		yield return new(s => s.SerializeIntArray("value", [1, 2, 3]), @"{""value"":[1,2,3]}");
		yield return new(s => s.SerializeIntArray("value", null, includeNullArray: true), @"{""value"":null}");
		yield return new(s => s.SerializeIntArray("value", null, includeNullArray: false), @"{}");

		// string array, via JsonSerializerExtensions
		yield return new(s => s.SerializeStringArray("value", ["Hello", null, "World"]), @"{""value"":[""Hello"",null,""World""]}");
		yield return new(s => s.SerializeStringArray("value", null, includeNullArray: true), @"{""value"":null}");
		yield return new(s => s.SerializeStringArray("value", null, includeNullArray: false), @"{}");

		// Traits dictionary (IReadOnlyDictionary<string, IReadOnlyCollection<string>>), via JsonSerializerExtensions
		var traits = new Dictionary<string, IReadOnlyCollection<string>>()
		{
			["foo"] = ["bar", "baz"],
			["biff"] = ["bop"]
		};
		yield return new(s => s.SerializeTraits("value", traits), @"{""value"":{""biff"":[""bop""],""foo"":[""bar"",""baz""]}}");

		// Complex result
		yield return new(s =>
		{
			s.Serialize("valid", true);
			using (var a = s.SerializeArray("input"))
			{
				a.Serialize(42);
				a.Serialize("Hello");
			}
			using (var o = s.SerializeObject("name"))
			{
				o.Serialize("first", "Brad");
				o.Serialize("last", "Wilson");
			}
		}, @"{""valid"":true,""input"":[42,""Hello""],""name"":{""first"":""Brad"",""last"":""Wilson""}}");
	}

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(ValueFuncs))]
	public void SerializeValues(
		Action<JsonObjectSerializer> action,
		string expectedResult)
	{
		var buffer = new StringBuilder();

		using (var serializer = new JsonObjectSerializer(buffer))
			action(serializer);

		Assert.Equal(expectedResult, buffer.ToString());
	}
}
