using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class _MessageSinkMessageTests
{
	[Fact]
	public void DoesNotSerializeMessagesWithoutJsonTypeID()
	{
		var msg = new _MessageSinkMessage();

		var result = msg.ToJson();

		Assert.Null(result);
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

		var json = msg.ToJson();

		Assert.NotNull(json);
		var expected =
@"{
	""$type"":                    ""test-assembly-starting"",
	""AssemblyUniqueID"":         ""asm-id"",
	""AssemblyName"":             ""asm-name"",
	""StartTime"":                ""2020-09-26T13:55:27.2120000-07:00"",
	""TestEnvironment"":          ""test-env"",
	""TestFrameworkDisplayName"": ""test-framework""
}".Replace("\n", "");
		Assert.Equal(expected, json, ignoreAllWhiteSpace: true);
	}

	[Fact]
	public void SerializesEnumsAsStrings()
	{
		var msg = new _TestFailed
		{
			AssemblyUniqueID = "asm-id",
			Cause = FailureCause.Assertion,
			ExceptionParentIndices = [-1],
			ExceptionTypes = ["exception-type"],
			ExecutionTime = 123.45m,
			Messages = ["exception-message"],
			Output = "",
			StackTraces = [null],
			TestCaseUniqueID = "test-case-id",
			TestCollectionUniqueID = "test-collection-id",
			TestUniqueID = "test-id",
		};

		var json = msg.ToJson();

		Assert.NotNull(json);
		var expected =
@"{
	""$type"":                  ""test-failed"",
	""AssemblyUniqueID"":       ""asm-id"",
	""TestCollectionUniqueID"": ""test-collection-id"",
	""TestCaseUniqueID"":       ""test-case-id"",
	""TestUniqueID"":           ""test-id"",
	""ExecutionTime"":          123.45,
	""Output"":                 """",
	""ExceptionParentIndices"": [-1],
	""ExceptionTypes"":         [""exception-type""],
	""Messages"":               [""exception-message""],
	""StackTraces"":            [null],
	""Cause"":                  ""Assertion""
}".Replace("\n", "");
		Assert.Equal(expected, json, ignoreAllWhiteSpace: true);
	}

	[Fact]
	public void ValidatesAllDerivedTypesAreSupported()
	{
		var derivedTypes =
			typeof(_MessageSinkMessage)
				.Assembly
				.GetTypes()
				.Where(t => !t.IsAbstract && t != typeof(_MessageSinkMessage) && typeof(_MessageSinkMessage).IsAssignableFrom(t))
				.ToList();
		var missingTypes =
			derivedTypes
				.Where(t => t.GetCustomAttribute<JsonTypeIDAttribute>() is null)
				.ToList();

		if (missingTypes.Count > 0)
			throw new XunitException($"The following message classes are missing [JsonTypeID]:{Environment.NewLine}{string.Join(Environment.NewLine, missingTypes.Select(t => $"  - {t.SafeName()}").OrderBy(t => t))}");
	}
}
