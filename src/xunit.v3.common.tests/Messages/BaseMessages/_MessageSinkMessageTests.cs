using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.v3;

public class _MessageSinkMessageTests
{
	[Fact]
	public void BaseSerializationIncludesTypeName()
	{
		var msg = new _MessageSinkMessage();

		var result = Encoding.UTF8.GetString(msg.ToJson());

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

		var result = Encoding.UTF8.GetString(msg.ToJson());

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
	public void CanRoundTripTraits()
	{
		var msg = new _TestCaseDiscovered
		{
			AssemblyUniqueID = "asm-id",
			Serialization = "serialized-value",
			TestCaseDisplayName = "test-case-display-name",
			TestCaseUniqueID = "test-case-id",
			TestCollectionUniqueID = "test-collection-id",
			Traits = new Dictionary<string, List<string>>
			{
				{ "foo", new List<string> { "bar", "baz" } },
				{ "abc", new List<string> { "123" } },
				{ "empty", new List<string>() },
			},
		};

		// Validate serialization

		var serialized = msg.ToJson();

		Assert.Equal(
			@"{" +
				@"""$type"":""_TestCaseDiscovered""," +
				@"""AssemblyUniqueID"":""asm-id""," +
				@"""Serialization"":""serialized-value""," +
				@"""TestCaseDisplayName"":""test-case-display-name""," +
				@"""TestCaseUniqueID"":""test-case-id""," +
				@"""TestCollectionUniqueID"":""test-collection-id""," +
				@"""Traits"":" +
				@"{" +
					@"""foo"":[""bar"",""baz""]," +
					@"""abc"":[""123""]," +
					@"""empty"":[]" +
				@"}" +
			@"}",
			Encoding.UTF8.GetString(serialized)
		);

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
}
