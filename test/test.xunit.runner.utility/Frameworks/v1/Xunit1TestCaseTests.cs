using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class Xunit1TestCaseTests
{
    public class Serialization
    {
        [Fact]
        public static void CanRoundTrip_PublicClass_PublicTestMethod()
        {
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PublicTestMethod");

            var serialized = SerializationHelper.Serialize(testCase);
            var deserialized = SerializationHelper.Deserialize<Xunit1TestCase>(serialized);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public static void CanRoundTrip_PublicClass_PrivateTestMethod()
        {
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PrivateTestMethod");

            var serialized = SerializationHelper.Serialize(testCase);
            var deserialized = SerializationHelper.Deserialize<Xunit1TestCase>(serialized);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public static void CanRoundTrip_PrivateClass()
        {
            var testCase = Create(typeof(PrivateClass), "TestMethod");

            var serialized = SerializationHelper.Serialize(testCase);
            var deserialized = SerializationHelper.Deserialize<Xunit1TestCase>(serialized);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public static void RoundTrippedTraitsAreCaseInsensitive()
        {
            var traits = new Dictionary<string, List<string>> { { "foo", new List<string> { "bar" } } };
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PrivateTestMethod", traits);

            var serialized = SerializationHelper.Serialize(testCase);
            var deserialized = SerializationHelper.Deserialize<Xunit1TestCase>(serialized);

            Assert.True(deserialized.Traits.Contains("fOo", "bAr", StringComparer.OrdinalIgnoreCase));
        }

        class PrivateClass
        {
            [Fact]
            public static void TestMethod()
            {
                Assert.True(false);
            }
        }
    }

    public class UniqueID
    {
        [Fact]
        public static void UniqueIDIsStable()
        {
            var typeUnderTest = typeof(ClassUnderTest);
            var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();

            var result = Create(typeUnderTest, "TestMethod").UniqueID;

            Assert.Equal($"Xunit1TestCaseTests+UniqueID+ClassUnderTest.TestMethod ({assemblyFileName})", result);
        }

        class ClassUnderTest
        {
            [Fact]
            public static void TestMethod() { }
        }
    }

    static Xunit1TestCase Create(Type typeUnderTest, string methodName, Dictionary<string, List<string>> traits = null)
    {
        var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();

        return new Xunit1TestCase(assemblyFileName, null, typeUnderTest.FullName, methodName, null, traits);
    }
}