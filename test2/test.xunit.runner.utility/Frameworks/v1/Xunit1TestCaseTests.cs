using System;
using System.IO;
using System.Linq;
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
            var assemblyFileName = new Uri(typeUnderTest.Assembly.CodeBase).LocalPath;

            var result = Create(typeUnderTest, "TestMethod").UniqueID;

            Assert.Equal<object>(String.Format("Xunit1TestCaseTests+UniqueID+ClassUnderTest.TestMethod ({0})", assemblyFileName), result);
        }

        class ClassUnderTest
        {
            [Fact]
            public void TestMethod() { }
        }
    }

    static Xunit1TestCase Create(Type typeUnderTest, string methodName)
    {
        var assemblyFileName = new Uri(typeUnderTest.Assembly.CodeBase).LocalPath;
        return new Xunit1TestCase(assemblyFileName, typeUnderTest.FullName, methodName, null);
    }
}