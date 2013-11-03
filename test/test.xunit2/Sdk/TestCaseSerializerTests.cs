using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCaseSerializerTests
{
    class ClassUnderTest
    {
        [Fact(Skip = "Skip me", DisplayName = "Hi there")]
        [Trait("name", "value")]
        public void FactMethod()
        {
            Assert.True(false);
        }
    }

    public class WithXunitTestCase
    {
        [Fact]
        public void CanSerializeFactBasedTestCase()
        {
            var testCollection = new XunitTestCollection();
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));

            Assert.DoesNotThrow(() => SerializationHelper.Serialize(testCase));
        }

        [Fact]
        public void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
        {
            var testCollection = new XunitTestCollection();
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));
            var serialized = SerializationHelper.Serialize(testCase);

            var result = SerializationHelper.Deserialize<XunitTestCase>(serialized);

            Assert.Equal(testCase.Assembly.AssemblyPath, result.Assembly.AssemblyPath);
            Assert.Equal(testCase.Assembly.Name, result.Assembly.Name);
            Assert.Equal(testCase.Class.Name, result.Class.Name);
            Assert.Equal(testCase.Method.Name, result.Method.Name);
            Assert.Equal(testCase.DisplayName, result.DisplayName);
            Assert.Null(result.Arguments);
            Assert.Equal(testCase.SkipReason, result.SkipReason);
            Assert.Collection(result.Traits,
                trait =>
                {
                    Assert.Equal("name", trait.Key);
                    Assert.Equal("value", trait.Value);
                });
        }

        [Fact]
        public void DeserializedTestWithSerializableArgumentsPreservesArguments()
        {
            var testCollection = new XunitTestCollection();
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact), new object[] { 42, 21.12, "Hello world" });
            var serialized = SerializationHelper.Serialize(testCase);

            var result = SerializationHelper.Deserialize<XunitTestCase>(serialized);

            Assert.Collection(result.Arguments,
                arg => Assert.Equal(42, arg),
                arg => Assert.Equal(21.12, arg),
                arg => Assert.Equal("Hello world", arg));
        }

        [Fact]
        public void DeserializedTestWithNonSerializableArgumentsThrows()
        {
            var testCollection = new XunitTestCollection();
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact), new object[] { new ClassUnderTest() });

            var ex = Record.Exception(() => SerializationHelper.Serialize(testCase));

            Assert.IsType<SerializationException>(ex);
        }
    }

    public class WithXunitTheoryTestCase
    {
        [Fact]
        public void CanSerializeFactBasedTestCase()
        {
            var testCollection = new XunitTestCollection();
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTheoryTestCase(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));

            Assert.DoesNotThrow(() => SerializationHelper.Serialize(testCase));
        }

        [Fact]
        public void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
        {
            var testCollection = new XunitTestCollection();
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTheoryTestCase(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));
            var serialized = SerializationHelper.Serialize(testCase);

            var result = SerializationHelper.Deserialize<XunitTheoryTestCase>(serialized);

            Assert.Equal(testCase.Assembly.AssemblyPath, result.Assembly.AssemblyPath);
            Assert.Equal(testCase.Assembly.Name, result.Assembly.Name);
            Assert.Equal(testCase.Class.Name, result.Class.Name);
            Assert.Equal(testCase.Method.Name, result.Method.Name);
            Assert.Equal(testCase.DisplayName, result.DisplayName);
            Assert.Null(result.Arguments);
            Assert.Equal(testCase.SkipReason, result.SkipReason);
            Assert.Collection(result.Traits,
                trait =>
                {
                    Assert.Equal("name", trait.Key);
                    Assert.Equal("value", trait.Value);
                });
        }
    }
}