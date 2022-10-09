using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestCaseSerializerTests
{
	class ClassUnderTest
	{
		[Fact(Skip = "Skip me", DisplayName = "Hi there", Timeout = 2112)]
		[Trait("name", "value")]
		public void FactMethod()
		{
			Assert.True(false);
		}
	}

	public class WithXunitTestCase
	{
		[Fact]
		public static void CanSerializeFactBasedTestCase()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>("FactMethod");

			SerializationHelper.Serialize(testCase);  // Should not throw
		}

		[Fact]
		public static void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>("FactMethod");
			var serialized = SerializationHelper.Serialize(testCase);

			var result = SerializationHelper.Deserialize<IXunitTestCase>(serialized);

			Assert.NotNull(result);
			Assert.Equal(testCase.TestCollection.TestAssembly.Assembly.AssemblyPath, result.TestCollection.TestAssembly.Assembly.AssemblyPath);
			Assert.Equal(testCase.TestCollection.TestAssembly.ConfigFileName, result.TestCollection.TestAssembly.ConfigFileName);
			Assert.Null(result.TestCollection.CollectionDefinition);
			Assert.Equal(testCase.TestCollection.DisplayName, result.TestCollection.DisplayName);
			Assert.Equal(testCase.TestCollection.UniqueID, result.TestCollection.UniqueID);
			Assert.Equal(testCase.TestMethod.TestClass.Class.Name, result.TestClass?.Class.Name);
			Assert.Equal(testCase.TestMethod.Method.Name, result.TestMethod?.Method.Name);
			Assert.Equal(testCase.TestCaseDisplayName, result.TestCaseDisplayName);
			Assert.Equal(testCase.SkipReason, result.SkipReason);
			Assert.Equal(testCase.Timeout, result.Timeout);
			Assert.Empty(result.TestMethodArguments);
			Assert.Collection(
				result.Traits.Keys,
				key =>
				{
					Assert.Equal("Assembly", key);
					Assert.Equal("Trait", Assert.Single(result.Traits[key]));
				},
				key =>
				{
					Assert.Equal("name", key);
					Assert.Equal("value", Assert.Single(result.Traits[key]));
				}
			);
			Assert.Equal(testCase.UniqueID, result.UniqueID);
		}

		[Fact]
		public static void DeserializedTestWithSerializableArgumentsPreservesArguments()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>("FactMethod", testMethodArguments: new object[] { 42, 21.12, "Hello world" });
			var serialized = SerializationHelper.Serialize(testCase);

			var result = SerializationHelper.Deserialize<IXunitTestCase>(serialized);

			Assert.NotNull(result);
			Assert.NotNull(result.TestMethodArguments);
			Assert.Collection(
				result.TestMethodArguments,
				arg => Assert.Equal(42, arg),
				arg => Assert.Equal(21.12, arg),
				arg => Assert.Equal("Hello world", arg)
			);
		}
	}

	public class WithXunitDelayEnumeratedTheoryTestCase
	{
		[Fact]
		public static void CanSerializeFactBasedTestCase()
		{
			var testCase = TestData.XunitDelayEnumeratedTheoryTestCase<ClassUnderTest>("FactMethod");

			SerializationHelper.Serialize(testCase);  // Should not throw
		}

		[Fact]
		public static void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
		{
			var testCase = TestData.XunitDelayEnumeratedTheoryTestCase<ClassUnderTest>("FactMethod");
			var serialized = SerializationHelper.Serialize(testCase);

			var result = SerializationHelper.Deserialize<IXunitTestCase>(serialized);

			Assert.NotNull(result);
			Assert.Equal(testCase.TestCollection.TestAssembly.Assembly.AssemblyPath, result.TestCollection.TestAssembly.Assembly.AssemblyPath);
			Assert.Equal(testCase.TestCollection.TestAssembly.ConfigFileName, result.TestCollection.TestAssembly.ConfigFileName);
			Assert.Null(result.TestCollection.CollectionDefinition);
			Assert.Equal(testCase.TestCollection.DisplayName, result.TestCollection.DisplayName);
			Assert.Equal(testCase.TestCollection.UniqueID, result.TestCollection.UniqueID);
			Assert.Equal(testCase.TestMethod.TestClass.Class.Name, result.TestClass?.Class.Name);
			Assert.Equal(testCase.TestMethod.Method.Name, result.TestMethod?.Method.Name);
			Assert.Equal(testCase.TestCaseDisplayName, result.TestCaseDisplayName);
			Assert.Equal(testCase.SkipReason, result.SkipReason);
			Assert.Equal(testCase.Timeout, result.Timeout);
			Assert.Empty(result.TestMethodArguments);
			Assert.Collection(
				result.Traits.Keys,
				key =>
				{
					Assert.Equal("Assembly", key);
					Assert.Equal("Trait", Assert.Single(result.Traits[key]));
				},
				key =>
				{
					Assert.Equal("name", key);
					Assert.Equal("value", Assert.Single(result.Traits[key]));
				}
			);
			Assert.Equal(testCase.UniqueID, result.UniqueID);
		}
	}
}
