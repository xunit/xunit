using System.Collections.Generic;
using Xunit;
using Xunit.Runner.v3;

public class _MessageSinkMessageTests
{
	[Fact]
	public void BaseSerializationIncludesTypeName()
	{
		var msg = new _MessageSinkMessage();

		var result = msg.Serialize();

		Assert.Equal(@"{""$type"":""_MessageSinkMessage""}", result);
	}

	[Fact]
	public void SerializationExcludesNullValues()
	{
		var msg = new _TestAssemblyMessage
		{
			AssemblyName = "asm-name",
			AssemblyPath = null,
			ConfigFilePath = null
		};

		var result = msg.Serialize();

		Assert.Equal(@"{""$type"":""_TestAssemblyMessage"",""AssemblyName"":""asm-name""}", result);
	}

	[Fact]
	public void ComplexSerialization()
	{
		var msg = new _TestResultMessage
		{
			AssemblyName = "asm-name",
			ExecutionTime = 1.23m,
			TestCaseDisplayName = "test-case-display-name",
			TestCaseId = "test-case-id",
			TestCollectionDisplayName = "test-collection-display-name",
			TestCollectionId = "test-collection-id",
			TestDisplayName = "test-display-name",
			Traits = new Dictionary<string, string[]>
			{
				{ "foo", new[] { "bar", "baz" } },
				{ "abc", new[] { "123" } },
				{ "empty", new string[0] },
			},
		};

		var result = msg.Serialize();

		Assert.Equal(
			@"{" +
				@"""$type"":""_TestResultMessage""," +
				@"""AssemblyName"":""asm-name""," +
				@"""ExecutionTime"":1.23," +
				@"""TestCaseDisplayName"":""test-case-display-name""," +
				@"""TestCaseId"":""test-case-id""," +
				@"""TestCollectionDisplayName"":""test-collection-display-name""," +
				@"""TestCollectionId"":""test-collection-id""," +
				@"""TestDisplayName"":""test-display-name""," +
				@"""Traits"":" +
				@"{" +
					@"""foo"":[""bar"",""baz""]," +
					@"""abc"":[""123""]," +
					@"""empty"":[]" +
				@"}" +
			@"}",
			result
		);
	}

	[Fact]
	public void RoundTrip()
	{
		var msg = new _TestAssemblyMessage
		{
			AssemblyName = "asm-name",
			AssemblyPath = "asm-path"
		};

		var serialized = msg.Serialize();
		var deserialized = _MessageSinkMessage.Deserialize(serialized);

		var deserializedTAM = Assert.IsType<_TestAssemblyMessage>(deserialized);
		Assert.Equal("asm-name", deserializedTAM.AssemblyName);
		Assert.Equal("asm-path", deserializedTAM.AssemblyPath);
		Assert.Null(deserializedTAM.ConfigFilePath);
	}
}
