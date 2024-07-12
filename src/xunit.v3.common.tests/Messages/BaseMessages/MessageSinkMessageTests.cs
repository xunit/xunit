using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

public class MessageSinkMessageTests
{
	[Fact]
	public void DoesNotSerializeMessagesWithoutJsonTypeID()
	{
		var msg = new MessageSinkMessage();

		var result = msg.ToJson();

		Assert.Null(result);
	}

	[Fact]
	public void SerializationExcludesNullValues()
	{
		var startTime = new DateTimeOffset(2020, 09, 26, 13, 55, 27, 212, TimeSpan.FromHours(-7));
		var msg = new TestAssemblyStarting
		{
			AssemblyUniqueID = "asm-id",
			AssemblyName = "asm-name",
			AssemblyPath = "asm-path",
			ConfigFilePath = null,
			Seed = null,
			StartTime = startTime,
			TargetFramework = null,
			TestEnvironment = "test-env",
			TestFrameworkDisplayName = "test-framework",
			Traits = TestData.EmptyTraits,
		};

		var json = msg.ToJson();

		Assert.NotNull(json);
		var expected =
@"{
	""$type"":                    ""test-assembly-starting"",
	""AssemblyUniqueID"":         ""asm-id"",
	""AssemblyName"":             ""asm-name"",
	""AssemblyPath"":             ""asm-path"",
	""Traits"":                   {},
	""StartTime"":                ""2020-09-26T13:55:27.2120000-07:00"",
	""TestEnvironment"":          ""test-env"",
	""TestFrameworkDisplayName"": ""test-framework""
}".Replace("\n", "");
		Assert.Equal(expected, json, ignoreAllWhiteSpace: true);
	}

	[Fact]
	public void SerializesEnumsAsStrings()
	{
		var msg = new TestFailed
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
			TestClassUniqueID = null,
			TestCollectionUniqueID = "test-collection-id",
			TestMethodUniqueID = null,
			TestUniqueID = "test-id",
			Warnings = null,
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
	""ExceptionParentIndices"": [-1],
	""ExceptionTypes"":         [""exception-type""],
	""Messages"":               [""exception-message""],
	""StackTraces"":            [null],
	""ExecutionTime"":          123.45,
	""Output"":                 """",
	""Cause"":                  ""Assertion""
}".Replace("\n", "");
		Assert.Equal(expected, json, ignoreAllWhiteSpace: true);
	}

	[Fact]
	public void ValidatesAllDerivedTypesAreSupported()
	{
		var derivedTypes =
			typeof(MessageSinkMessage)
				.Assembly
				.GetTypes()
				.Where(t => !t.IsAbstract && t != typeof(MessageSinkMessage) && typeof(MessageSinkMessage).IsAssignableFrom(t))
				.ToList();
		var missingTypes =
			derivedTypes
				.Where(t => t.GetCustomAttribute<JsonTypeIDAttribute>() is null)
				.ToList();

		if (missingTypes.Count > 0)
			throw new XunitException($"The following message classes are missing [JsonTypeID]:{Environment.NewLine}{string.Join(Environment.NewLine, missingTypes.Select(t => $"  - {t.SafeName()}").OrderBy(t => t))}");
	}
}
