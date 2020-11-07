using System;
using System.Collections.Generic;
using Xunit;
using Xunit.v3;

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

		var result = msg.Serialize();

		Assert.Equal(
			@"{" +
				@"""$type"":""_TestAssemblyStarting""," +
				@"""AssemblyName"":""asm-name""," +
				@"""AssemblyUniqueID"":""asm-id""," +
				@"""StartTime"":""2020-09-26T13:55:27.212-07:00""," +
				@"""TestEnvironment"":""test-env""," +
				@"""TestFrameworkDisplayName"":""test-framework""" +
			@"}",
			result
		);
	}

	[Fact]
	public void CanSerializeTraits()
	{
		var msg = new _TestCaseDiscovered
		{
			TestCaseDisplayName = "test-case-display-name",
			TestCaseUniqueID = "test-case-id",
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
				@"""$type"":""_TestCaseDiscovered""," +
				@"""TestCaseDisplayName"":""test-case-display-name""," +
				@"""TestCaseUniqueID"":""test-case-id""," +
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
		var msg = new _TestAssemblyMessage { AssemblyUniqueID = "asm-id" };

		var serialized = msg.Serialize();
		var deserialized = _MessageSinkMessage.Deserialize(serialized);

		var deserializedTAM = Assert.IsType<_TestAssemblyMessage>(deserialized);
		Assert.Equal("asm-id", deserializedTAM.AssemblyUniqueID);
	}
}
