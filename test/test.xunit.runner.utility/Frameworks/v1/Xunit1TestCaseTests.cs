using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

public class Xunit1TestCaseTests
{
    public class Serialization
    {
        [Fact]
        public void CanRoundTrip_PublicClass_PublicTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PublicTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        void CanRoundTrip_PublicClass_PrivateTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PrivateTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public void CannotRoundTrip_PrivateClass()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(PrivateClass), "TestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public void RoundTrippedTraitsAreCaseInsensitive()
        {
            var serializer = new BinaryFormatter();
            var traits = new Dictionary<string, List<string>> { { "foo", new List<string> { "bar" } } };
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PrivateTestMethod", traits);
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            var deserializedTestCase = (Xunit1TestCase)serializer.Deserialize(memoryStream);

            Assert.True(deserializedTestCase.Traits.Contains("fOo", "bAr", StringComparer.OrdinalIgnoreCase));
        }

        class PrivateClass
        {
            [Fact]
            public void TestMethod()
            {
                Assert.True(false);
            }
        }
    }

    public class UniqueID
    {
        [Fact]
        public void UniqueIDIsStable()
        {
            var typeUnderTest = typeof(ClassUnderTest);
            var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();

            var result = Create(typeUnderTest, "TestMethod").UniqueID;

            Assert.Equal(String.Format("Xunit1TestCaseTests+UniqueID+ClassUnderTest.TestMethod ({0})", assemblyFileName), result);
        }

        class ClassUnderTest
        {
            [Fact]
            public void TestMethod() { }
        }
    }

    static Xunit1TestCase Create(Type typeUnderTest, string methodName, Dictionary<string, List<string>> traits = null)
    {
        var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();

        return new Xunit1TestCase(assemblyFileName, typeUnderTest.FullName, methodName, null, traits);
    }
}