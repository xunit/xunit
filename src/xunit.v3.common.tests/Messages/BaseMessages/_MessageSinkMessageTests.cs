#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class _MessageSinkMessageTests
{
	[Fact]
	public void BaseSerializationIncludesTypeName()
	{
		var msg = new _MessageSinkMessage();

		var result = Encoding.UTF8.GetString(msg.ToJson());

		var expected =
@"{
	""$type"": ""_MessageSinkMessage""
}".Replace("\n", "");
		Assert.Equal(expected, result, ignoreAllWhiteSpace: true);
	}

	[Fact]
	public void SerializationExcludesNullValues()
	{
		var startTime = new DateTimeOffset(2020, 09, 26, 13, 55, 27, 212, TimeSpan.FromHours(-7));
		var msg = new _TestAssemblyStarting
		{
			AssemblyUniqueID = "asm-id",
			AssemblyName = "asm-name",
			AssemblyPath = null,
			ConfigFilePath = null,
			StartTime = startTime,
			TestEnvironment = "test-env",
			TestFrameworkDisplayName = "test-framework"
		};

		var result = Encoding.UTF8.GetString(msg.ToJson());

		var expected =
@"{
	""$type"":                    ""_TestAssemblyStarting"",
	""AssemblyName"":             ""asm-name"",
	""StartTime"":                ""2020-09-26T13:55:27.212-07:00"",
	""TestEnvironment"":          ""test-env"",
	""TestFrameworkDisplayName"": ""test-framework"",
	""AssemblyUniqueID"":         ""asm-id""
}".Replace("\n", "");
		Assert.Equal(expected, result, ignoreAllWhiteSpace: true);
	}

	[Fact]
	public void SerializesEnumsAsStrings()
	{
		var msg = new _TestFailed
		{
			AssemblyUniqueID = "asm-id",
			Cause = FailureCause.Assertion,
			ExceptionParentIndices = new[] { -1 },
			ExceptionTypes = new[] { "exception-type" },
			ExecutionTime = 123.45m,
			Messages = new[] { "exception-message" },
			Output = "",
			StackTraces = new[] { "stack-trace" },
			TestCaseUniqueID = "test-case-id",
			TestCollectionUniqueID = "test-collection-id",
			TestUniqueID = "test-id",
		};

		var result = Encoding.UTF8.GetString(msg.ToJson());

		var expected =
@"{
	""$type"":                  ""_TestFailed"",
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
}".Replace("\n", "");
		Assert.Equal(expected, result, ignoreAllWhiteSpace: true);
	}

	class TestMessageWithEnum : _MessageSinkMessage
	{
		public FailureCause Cause { get; set; }
	}

	[Fact]
	public void DeserializesEnumsAsStrings()
	{
		var msg =
@"{
	""$type"":                  ""_TestFailed"",
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

		var result = _MessageSinkMessage.ParseJson(Encoding.UTF8.GetBytes(msg));

		var testFailed = Assert.IsType<_TestFailed>(result);
		Assert.Equal(FailureCause.Assertion, testFailed.Cause);
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

		var expected =
@"{
	""$type"":               ""_TestCaseDiscovered"",
	""Serialization"":       ""serialized-value"",
	""TestCaseDisplayName"": ""test-case-display-name"",
	""Traits"":
	{
		""foo"":   [""bar"", ""baz""],
		""abc"":   [""123""],
		""empty"": []
	},
	""TestCaseUniqueID"":       ""test-case-id"",
	""TestCollectionUniqueID"": ""test-collection-id"",
	""AssemblyUniqueID"":       ""asm-id""
}".Replace("\n", "");
		Assert.Equal(expected, Encoding.UTF8.GetString(serialized), ignoreAllWhiteSpace: true);

		// Validate deserialization

		var deserialized = _MessageSinkMessage.ParseJson(serialized);

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

	[Fact]
	public void RoundTrip()
	{
		var msg = new _TestAssemblyMessage { AssemblyUniqueID = "asm-id" };

		var serialized = msg.ToJson();
		var deserialized = _MessageSinkMessage.ParseJson(serialized);

		var deserializedTAM = Assert.IsType<_TestAssemblyMessage>(deserialized);
		Assert.Equal("asm-id", deserializedTAM.AssemblyUniqueID);
	}

	[Fact]
	public void ValidatesAllDerivedTypesAreSupported()
	{
		var messageSinkMessageType = typeof(_MessageSinkMessage);
		var missingTypes = new List<Type>();
		var decoratedTypes =
			messageSinkMessageType
				.GetCustomAttributes(typeof(JsonDerivedTypeAttribute))
				.Cast<JsonDerivedTypeAttribute>()
				.Select(a => a.DerivedType)
				.ToHashSet();

		Type[] publicTypes;
		try
		{
			publicTypes = messageSinkMessageType.Assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			publicTypes = ex.Types.WhereNotNull().ToArray();
		}

		foreach (var type in publicTypes.Where(t => !t.IsAbstract && messageSinkMessageType.IsAssignableFrom(t)))
			if (!decoratedTypes.Contains(type))
				missingTypes.Add(type);

		if (missingTypes.Count > 0)
			throw new XunitException($"The following attributes are missing from {nameof(_MessageSinkMessage)}:{Environment.NewLine}{string.Join(Environment.NewLine, missingTypes.OrderBy(t => t.Name).Select(t => $"  [JsonDerivedType(typeof({t.Name}), nameof({t.Name}))]"))}");
	}
}
#endif
