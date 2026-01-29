using System.Text;
using Xunit;
using Xunit.Sdk;

public class JsonArraySerializerTests
{
	public static IEnumerable<TheoryDataRow<Action<JsonArraySerializer>, string>> ValueFuncs()
	{
		// Empty
		yield return new(s => { }, /* lang=json */"""[]""");

		// int?
		yield return new(s => s.Serialize(42), /* lang=json */"""[42]""");
		yield return new(s => s.Serialize(default(int?)), /* lang=json */"""[null]""");

		// string?
		yield return new(s => s.Serialize("Hello"), /* lang=json */"""["Hello"]""");
		yield return new(s => s.Serialize(default(string?)), /* lang=json */"""[null]""");

		// Multi-value
		yield return new(s =>
		{
			s.Serialize(42);
			s.Serialize("Hello");
			s.Serialize(2112);
		}, /* lang=json */"""[42,"Hello",2112]""");

		// Complex result
		yield return new(s =>
		{
			s.Serialize(42);
			using (var a = s.SerializeArray())
			{
				a.Serialize(2112);
				a.Serialize("Hello");
			}
			using var o = s.SerializeObject();
			o.Serialize("first", "Brad");
			o.Serialize("last", "Wilson");
		}, /* lang=json */"""[42,[2112,"Hello"],{"first":"Brad","last":"Wilson"}]""");
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
