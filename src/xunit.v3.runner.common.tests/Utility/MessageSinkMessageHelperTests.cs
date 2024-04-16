using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class MessageSinkMessageHelperTests
{
#if false
	class TestMessageWithEnum : _MessageSinkMessage
	{
		public FailureCause Cause { get; set; }
	}
#endif

	[Fact]
	public void DeserializesEnumsAsStrings()
	{
		var msg =
@"{
	""Type"":                   ""test-failed"",
	""Cause"":                  ""Assertion"",
	""ExceptionParentIndices"": [-1],
	""ExceptionTypes"":         [""exception-type""],
	""Messages"":               [""exception-message""],
	""StackTraces"":            [""stack-trace""],
	""ExecutionTime"":          123.45,
	""Output"":                 """",
	""TestUniqueID"":           ""test-id"",
	""TestCaseUniqueID"":       ""test-case-id"",
	""TestCollectionUniqueID"": ""test-collection-id"",
	""AssemblyUniqueID"":       ""asm-id""
}";

		var result = MessageSinkMessageHelper.Deserialize(Encoding.UTF8.GetBytes(msg));

		var testFailed = Assert.IsType<_TestFailed>(result);
		Assert.Equal(FailureCause.Assertion, testFailed.Cause);
	}

	[Fact]
	public void CanRoundTrip()
	{
		var serialized = new _DiagnosticMessage { Message = "Hello, world!" }.ToJson();
		Assert.NotNull(serialized);

		var deserialized = MessageSinkMessageHelper.Deserialize(serialized);
		var diagnosticMessage = Assert.IsType<_DiagnosticMessage>(deserialized);
		Assert.Equal("Hello, world!", diagnosticMessage.Message);
	}

	[Fact]
	public void CanRoundTripTraits()
	{
		var msg = new _TestCaseDiscovered
		{
			AssemblyUniqueID = "asm-id",
			Serialization = "serialized-value",
			TestCaseDisplayName = "test-case-display-name",
			TestCaseUniqueID = "test-case-id",
			TestCollectionUniqueID = "test-collection-id",
			Traits = new Dictionary<string, IReadOnlyList<string>>
			{
				{ "foo", new List<string> { "bar", "baz" } },
				{ "abc", new List<string> { "123" } },
				{ "empty", new List<string>() },
			},
		};

		// Validate serialization

		var serialized = msg.ToJson();
		Assert.NotNull(serialized);

		var expected =
@"{
	""Type"":                   ""test-case-discovered"",
	""AssemblyUniqueID"":       ""asm-id"",
	""TestCollectionUniqueID"": ""test-collection-id"",
	""TestCaseUniqueID"":       ""test-case-id"",
	""TestCaseDisplayName"":    ""test-case-display-name"",
	""Traits"":
	{
		""foo"":   [""bar"", ""baz""],
		""abc"":   [""123""],
		""empty"": []
	},
	""Serialization"":          ""serialized-value""
}".Replace("\n", "");
		Assert.Equal(expected, Encoding.UTF8.GetString(serialized), ignoreAllWhiteSpace: true);

		// Validate deserialization

		var deserialized = MessageSinkMessageHelper.Deserialize(serialized);

		var deserializedDiscovered = Assert.IsType<_TestCaseDiscovered>(deserialized);
		Assert.Collection(
			deserializedDiscovered.Traits.OrderBy(kvp => kvp.Key),
			trait =>
			{
				Assert.Equal("abc", trait.Key);
				Assert.Equal(new[] { "123" }, trait.Value);
			},
			trait =>
			{
				Assert.Equal("empty", trait.Key);
				Assert.Empty(trait.Value);
			},
			trait =>
			{
				Assert.Equal("foo", trait.Key);
				Assert.Equal(new[] { "bar", "baz" }, trait.Value);
			}
		);
	}
}
