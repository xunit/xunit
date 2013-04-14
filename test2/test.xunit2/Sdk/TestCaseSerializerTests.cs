using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;
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
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));

            Assert.DoesNotThrow(() => TestCaseSerializer.Serialize(testCase));
        }

        [Fact]
        public void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
        {
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));
            var serialized = TestCaseSerializer.Serialize(testCase);

            var result = (XunitTestCase)TestCaseSerializer.Deserialize(serialized);

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
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact), new object[] { 42, 21.12, "Hello world" });
            var serialized = TestCaseSerializer.Serialize(testCase);

            var result = (XunitTestCase)TestCaseSerializer.Deserialize(serialized);

            Assert.Collection(result.Arguments,
                arg => Assert.Equal(42, arg),
                arg => Assert.Equal(21.12, arg),
                arg => Assert.Equal("Hello world", arg));
        }

        [Fact]
        public void DeserializedTestWithNonSerializableArgumentsThrows()
        {
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTestCase(Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact), new object[] { new ClassUnderTest() });

            var ex = Record.Exception(() => TestCaseSerializer.Serialize(testCase));

            Assert.IsType<SerializationException>(ex);
        }
    }

    public class WithXunitTheoryTestCase
    {
        [Fact]
        public void CanSerializeFactBasedTestCase()
        {
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTheoryTestCase(Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));

            Assert.DoesNotThrow(() => TestCaseSerializer.Serialize(testCase));
        }

        [Fact]
        public void DeserializedTestCaseContainsSameDataAsOriginalTestCase()
        {
            var type = typeof(ClassUnderTest);
            var method = type.GetMethod("FactMethod");
            var fact = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(FactAttribute));
            var testCase = new XunitTheoryTestCase(Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(fact));
            var serialized = TestCaseSerializer.Serialize(testCase);

            var result = (XunitTheoryTestCase)TestCaseSerializer.Deserialize(serialized);

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