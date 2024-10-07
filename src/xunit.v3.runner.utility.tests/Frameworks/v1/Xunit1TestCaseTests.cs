#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.v1;
using Xunit.Sdk;

public class Xunit1TestCaseTests
{
	public class Serialization
	{
		[Fact]
		public static void CanRoundTrip_PublicClass_PublicTestMethod()
		{
			var testCase = Create(typeof(Serialization), nameof(CanRoundTrip_PublicClass_PublicTestMethod));

			var serialized = SerializationHelper.Instance.Serialize(testCase);
			var deserialized = SerializationHelper.Instance.Deserialize<Xunit1TestCase>(serialized);

			Assert.NotNull(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PublicClass_PrivateTestMethod()
		{
			var testCase = Create(typeof(Serialization), nameof(PrivateTestMethod));

			var serialized = SerializationHelper.Instance.Serialize(testCase);
			var deserialized = SerializationHelper.Instance.Deserialize<Xunit1TestCase>(serialized);

			Assert.NotNull(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PrivateClass()
		{
			var testCase = Create(typeof(PrivateClass), nameof(PrivateClass.TestMethod));

			var serialized = SerializationHelper.Instance.Serialize(testCase);
			var deserialized = SerializationHelper.Instance.Deserialize<Xunit1TestCase>(serialized);

			Assert.NotNull(deserialized);
		}

		[Fact]
		public static void CanSerializeTraits_StayCaseInsensitive()
		{
			var traits = new Dictionary<string, IReadOnlyCollection<string>>
			{
				{ "foo", new List<string> { "bar", "baz" } },
				{ "biff", new List<string> { "Hello, world!" } }
			};
			var testCase = Create(typeof(Serialization), nameof(CanRoundTrip_PublicClass_PublicTestMethod), traits: traits);

			var serialized = SerializationHelper.Instance.Serialize(testCase);
			var deserialized = SerializationHelper.Instance.Deserialize<Xunit1TestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.NotNull(deserialized.Traits);
			Assert.Collection(
				deserialized.Traits["FOO"],
				bar => Assert.Equal("bar", bar),
				baz => Assert.Equal("baz", baz)
			);
			var helloWorld = Assert.Single(deserialized.Traits["biff"]);
			Assert.Equal("Hello, world!", helloWorld);
		}

		[Fact]
		void PrivateTestMethod() { }

		class PrivateClass
		{
			[Fact]
			public static void TestMethod()
			{
				Assert.True(false);
			}
		}
	}

	public class Traits
	{
		//[Fact]
		//public void TraitNamesAreCaseInsensitive_AddedAfter()
		//{
		//	var testCase = Create(typeof(Traits), "TraitNamesAreCaseInsensitive_AddedAfter");
		//	testCase.Traits.Add("FOO", new List<string> { "BAR" });

		//	var fooTraitValues = testCase.Traits["foo"];

		//	var fooTraitValue = Assert.Single(fooTraitValues);
		//	Assert.Equal("BAR", fooTraitValue);
		//}

		[Fact]
		public void TraitNamesAreCaseInsensitive_PreSeeded()
		{
			var traits = new Dictionary<string, IReadOnlyCollection<string>> { { "FOO", new List<string> { "BAR" } } };
			var testCase = Create(typeof(Traits), "TraitNamesAreCaseInsensitive_PreSeeded", traits: traits);

			var fooTraitValues = testCase.Traits["foo"];

			var fooTraitValue = Assert.Single(fooTraitValues);
			Assert.Equal("BAR", fooTraitValue);
		}
	}

	static Xunit1TestCase Create(
		Type typeUnderTest,
		string methodName,
		string? displayName = null,
		Dictionary<string, IReadOnlyCollection<string>>? traits = null,
		string? skipReason = null)
	{
		var typeName = typeUnderTest.FullName!;

		var result = new Xunit1TestCase
		{
			AssemblyUniqueID = $"asm-id",
			SkipReason = skipReason,
			TestCaseDisplayName = displayName ?? "display-name",
			TestCaseUniqueID = $"case-id: {typeName}:{methodName}",
			TestClass = typeName,
			TestClassUniqueID = $"class-id: {typeName}",
			TestCollectionUniqueID = $"collection-id",
			TestMethod = methodName,
			TestMethodUniqueID = $"method-id: {typeName}:{methodName}",
		};

		if (traits is not null)
			result.Traits = traits;

		return result;
	}
}

#endif
