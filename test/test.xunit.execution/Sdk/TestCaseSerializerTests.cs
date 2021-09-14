using System;
using Xunit;
using Xunit.Sdk;

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
            var testCase = Mocks.XunitTestCase<ClassUnderTest>("FactMethod");

            SerializationHelper.Serialize(testCase);  // Should not throw
        }

        [Fact]
        public static void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
        {
            var testCase = Mocks.XunitTestCase<ClassUnderTest>("FactMethod");
            var serialized = SerializationHelper.Serialize(testCase);

            var result = SerializationHelper.Deserialize<XunitTestCase>(serialized);

            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath, result.TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath);
            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.TestAssembly.ConfigFileName, result.TestMethod.TestClass.TestCollection.TestAssembly.ConfigFileName);
            Assert.Null(result.TestMethod.TestClass.TestCollection.CollectionDefinition);
            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.DisplayName, result.TestMethod.TestClass.TestCollection.DisplayName);
            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.UniqueID, result.TestMethod.TestClass.TestCollection.UniqueID);
            Assert.Equal(testCase.TestMethod.TestClass.Class.Name, result.TestMethod.TestClass.Class.Name);
            Assert.Equal(testCase.TestMethod.Method.Name, result.TestMethod.Method.Name);
            Assert.Equal(testCase.DisplayName, result.DisplayName);
            Assert.Equal(testCase.SkipReason, result.SkipReason);
            Assert.Equal(testCase.Timeout, result.Timeout);
            Assert.Null(result.TestMethodArguments);
            Assert.Collection(result.Traits.Keys,
                key =>
                {
                    Assert.Equal("Assembly", key);
                    Assert.Equal("Trait", Assert.Single(result.Traits[key]));
                },
                key =>
                {
                    Assert.Equal("name", key);
                    Assert.Equal("value", Assert.Single(result.Traits[key]));
                });
            Assert.Equal(testCase.UniqueID, result.UniqueID);
        }

        [Fact]
        public static void DeserializedTestWithSerializableArgumentsPreservesArguments()
        {
            var testCase = Mocks.XunitTestCase<ClassUnderTest>("FactMethod", testMethodArguments: new object[] { 42, 21.12, "Hello world" });
            var serialized = SerializationHelper.Serialize(testCase);

            var result = SerializationHelper.Deserialize<XunitTestCase>(serialized);

            Assert.Collection(result.TestMethodArguments,
                arg => Assert.Equal(42, arg),
                arg => Assert.Equal(21.12, arg),
                arg => Assert.Equal("Hello world", arg));
        }

        [Fact]
        public static void DeserializedTestWithNonSerializableArgumentsThrows()
        {
            var testCase = Mocks.XunitTestCase<ClassUnderTest>("FactMethod", testMethodArguments: new object[] { new ClassUnderTest() });

            var ex = Record.Exception(() => SerializationHelper.Serialize(testCase));

            Assert.IsType<ArgumentException>(ex);
            Assert.StartsWith("There is at least one object in this array that cannot be serialized", ex.Message);
        }
    }

    public class WithXunitTheoryTestCase
    {
        [Fact]
        public static void CanSerializeFactBasedTestCase()
        {
            var testCase = Mocks.XunitTheoryTestCase<ClassUnderTest>("FactMethod");

            SerializationHelper.Serialize(testCase);  // Should not throw
        }

        [Fact]
        public static void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
        {
            var testCase = Mocks.XunitTheoryTestCase<ClassUnderTest>("FactMethod");
            var serialized = SerializationHelper.Serialize(testCase);

            var result = SerializationHelper.Deserialize<XunitTheoryTestCase>(serialized);

            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath, result.TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath);
            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.TestAssembly.ConfigFileName, result.TestMethod.TestClass.TestCollection.TestAssembly.ConfigFileName);
            Assert.Null(result.TestMethod.TestClass.TestCollection.CollectionDefinition);
            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.DisplayName, result.TestMethod.TestClass.TestCollection.DisplayName);
            Assert.Equal(testCase.TestMethod.TestClass.TestCollection.UniqueID, result.TestMethod.TestClass.TestCollection.UniqueID);
            Assert.Equal(testCase.TestMethod.TestClass.Class.Name, result.TestMethod.TestClass.Class.Name);
            Assert.Equal(testCase.TestMethod.Method.Name, result.TestMethod.Method.Name);
            Assert.Equal(testCase.DisplayName, result.DisplayName);
            Assert.Equal(testCase.SkipReason, result.SkipReason);
            Assert.Equal(testCase.Timeout, result.Timeout);
            Assert.Null(result.TestMethodArguments);
            Assert.Collection(result.Traits.Keys,
                key =>
                {
                    Assert.Equal("Assembly", key);
                    Assert.Equal("Trait", Assert.Single(result.Traits[key]));
                },
                key =>
                {
                    Assert.Equal("name", key);
                    Assert.Equal("value", Assert.Single(result.Traits[key]));
                });
            Assert.Equal(testCase.UniqueID, result.UniqueID);
        }
    }
}
