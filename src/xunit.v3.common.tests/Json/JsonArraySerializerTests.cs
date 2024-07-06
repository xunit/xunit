using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Sdk;

public class JsonArraySerializerTests
{
	public static IEnumerable<TheoryDataRow<Action<JsonArraySerializer>, string>> ValueFuncs()
	{
		// Empty
		yield return new(s => { }, @"[]");

		// int?
		yield return new(s => s.Serialize(42), @"[42]");
		yield return new(s => s.Serialize(default(int?)), @"[null]");

		// string?
		yield return new(s => s.Serialize("Hello"), @"[""Hello""]");
		yield return new(s => s.Serialize(default(string?)), @"[null]");

		// Multi-value
		yield return new(s =>
		{
			s.Serialize(42);
			s.Serialize("Hello");
			s.Serialize(2112);
		}, @"[42,""Hello"",2112]");

		// Complex result
		yield return new(s =>
		{
			s.Serialize(42);
			using (var a = s.SerializeArray())
			{
				a.Serialize(2112);
				a.Serialize("Hello");
			}
			using (var o = s.SerializeObject())
			{
				o.Serialize("first", "Brad");
				o.Serialize("last", "Wilson");
			}
		}, @"[42,[2112,""Hello""],{""first"":""Brad"",""last"":""Wilson""}]");
	}

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(ValueFuncs))]
	public void SerializeValues(
		Action<JsonArraySerializer> action,
		string expectedResult)
	{
		var buffer = new StringBuilder();

		using (var serializer = new JsonArraySerializer(buffer))
			action(serializer);

		Assert.Equal(expectedResult, buffer.ToString());
	}
}
