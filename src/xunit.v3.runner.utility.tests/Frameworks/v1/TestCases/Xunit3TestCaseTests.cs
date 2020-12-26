#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.v1;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit3TestCaseTests
{
	public class Ctor
	{
		[Fact]
		public static void DefaultBehavior()
		{
			var typeUnderTest = typeof(ClassUnderTest);
			var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();
			var testCase = Create(typeUnderTest, "TestMethod");

			Assert.Equal("Xunit3TestCaseTests+Ctor+ClassUnderTest.TestMethod", testCase.DisplayName);
			Assert.Null(testCase.SkipReason);
			Assert.Null(testCase.SourceInformation);
			Assert.NotNull(testCase.TestMethod);
			Assert.Null(testCase.TestMethodArguments);
			Assert.Empty(testCase.Traits);
			Assert.Equal($"Xunit3TestCaseTests+Ctor+ClassUnderTest.TestMethod ({assemblyFileName})", testCase.UniqueID);
		}

		[Fact]
		public static void Overrides()
		{
			var typeUnderTest = typeof(ClassUnderTest);
			var traits = new Dictionary<string, List<string>> { { "FOO", new List<string> { "BAR" } } };

			var testCase = Create(typeUnderTest, "TestMethod", "Custom test display name", traits, "Skip me!");

			Assert.Equal("Custom test display name", testCase.DisplayName);
			Assert.Equal("Skip me!", testCase.SkipReason);
			Assert.Collection(
				testCase.Traits,
				kvp =>
				{
					Assert.Equal("FOO", kvp.Key);
					Assert.Equal("BAR", Assert.Single(kvp.Value));
				}
			);
		}

		class ClassUnderTest
		{
			[Fact]
			public static void TestMethod()
			{ }
		}
	}

	public class Serialization
	{
		[Fact]
		public static void CanRoundTrip_PublicClass_PublicTestMethod()
		{
			var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PublicTestMethod");

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<Xunit3TestCase>(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PublicClass_PrivateTestMethod()
		{
			var testCase = Create(typeof(Serialization), "PrivateTestMethod");

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<Xunit3TestCase>(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PrivateClass()
		{
			var testCase = Create(typeof(PrivateClass), "TestMethod");

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<Xunit3TestCase>(deserialized);
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
		[Fact]
		public void TraitNamesAreCaseInsensitive_AddedAfter()
		{
			var testCase = Create(typeof(Traits), "TraitNamesAreCaseInsensitive_AddedAfter");
			testCase.Traits.Add("FOO", new List<string> { "BAR" });

			var fooTraitValues = testCase.Traits["foo"];

			var fooTraitValue = Assert.Single(fooTraitValues);
			Assert.Equal("BAR", fooTraitValue);
		}

		[Fact]
		public void TraitNamesAreCaseInsensitive_PreSeeded()
		{
			var traits = new Dictionary<string, List<string>> { { "FOO", new List<string> { "BAR" } } };
			var testCase = Create(typeof(Traits), "TraitNamesAreCaseInsensitive_PreSeeded", traits: traits);

			var fooTraitValues = testCase.Traits["foo"];

			var fooTraitValue = Assert.Single(fooTraitValues);
			Assert.Equal("BAR", fooTraitValue);
		}
	}

	static Xunit3TestCase Create(
		Type typeUnderTest,
		string methodName,
		string? displayName = null,
		Dictionary<string, List<string>>? traits = null,
		string? skipReason = null)
	{
		var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();
		var assembly = new Xunit3TestAssembly(assemblyFileName);
		var collection = new Xunit3TestCollection(assembly);
		var @class = new Xunit3TestClass(collection, typeUnderTest.FullName!);
		var method = new Xunit3TestMethod(@class, methodName);

		return new Xunit3TestCase(method, displayName, traits, skipReason);
	}
}

#endif
