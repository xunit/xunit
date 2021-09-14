using System;
using System.Collections.Generic;
using Xunit;

public class Xunit1TestCaseTests
{
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
