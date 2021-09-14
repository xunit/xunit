using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SerializationTests
{
    [Serializable]
    class SerializableObject { }

    [Fact]
    public static void CanSerializeAndDeserializeObjectsInATest()
    {
        var bf = new BinaryFormatter();

        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, (object)new SerializableObject());
            ms.Position = 0;
            object o = bf.Deserialize(ms);

            Assert.IsType(typeof(SerializableObject), o);
            var o2 = (SerializableObject)o;  // Should not throw
        }
    }

    [Fact]
    public static void SerializedTestsInSameCollectionRemainInSameCollection()
    {
        var sourceProvider = new NullSourceInformationProvider();
        var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
        var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, SpyMessageSink.Create());
        var sink = new TestDiscoverySink();

        discoverer.Find(typeof(ClassWithFacts).FullName, false, sink, TestFrameworkOptions.ForDiscovery());
        sink.Finished.WaitOne();

        var first = sink.TestCases[0];
        var second = sink.TestCases[1];
        Assert.NotEqual(first.UniqueID, second.UniqueID);

        Assert.True(TestCollectionComparer.Instance.Equals(first.TestMethod.TestClass.TestCollection, second.TestMethod.TestClass.TestCollection));

        var serializedFirst = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(first));
        var serializedSecond = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(second));

        Assert.NotSame(serializedFirst.TestMethod.TestClass.TestCollection, serializedSecond.TestMethod.TestClass.TestCollection);
        Assert.True(TestCollectionComparer.Instance.Equals(serializedFirst.TestMethod.TestClass.TestCollection, serializedSecond.TestMethod.TestClass.TestCollection));
    }

    class ClassWithFacts
    {
        [Fact]
        public void Test1() { }

        [Fact]
        public void Test2() { }
    }

    [Fact]
    public static void TheoriesWithSerializableData_ReturnAsIndividualTestCases()
    {
        var sourceProvider = new NullSourceInformationProvider();
        var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
        var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, SpyMessageSink.Create());
        var sink = new TestDiscoverySink();

        discoverer.Find(typeof(ClassWithTheory).FullName, false, sink, TestFrameworkOptions.ForDiscovery());
        sink.Finished.WaitOne();

        var first = sink.TestCases[0];
        var second = sink.TestCases[1];
        Assert.NotEqual(first.UniqueID, second.UniqueID);

        Assert.True(TestCollectionComparer.Instance.Equals(first.TestMethod.TestClass.TestCollection, second.TestMethod.TestClass.TestCollection));

        var serializedFirst = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(first));
        var serializedSecond = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(second));

        Assert.NotSame(serializedFirst.TestMethod.TestClass.TestCollection, serializedSecond.TestMethod.TestClass.TestCollection);
        Assert.True(TestCollectionComparer.Instance.Equals(serializedFirst.TestMethod.TestClass.TestCollection, serializedSecond.TestMethod.TestClass.TestCollection));
    }

    class ClassWithTheory
    {
        [Theory]
        [InlineData(1)]
        [InlineData("hello")]
        public void Test(object x) { }
    }

    [Fact]
    public static void TheoryWithNonSerializableData_ReturnsAsASingleTestCase()
    {
        var sourceProvider = new NullSourceInformationProvider();
        var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
        var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, SpyMessageSink.Create());
        var sink = new TestDiscoverySink();

        discoverer.Find(typeof(ClassWithNonSerializableTheoryData).FullName, false, sink, TestFrameworkOptions.ForDiscovery());
        sink.Finished.WaitOne();

        var testCase = Assert.Single(sink.TestCases);
        Assert.IsType<XunitTheoryTestCase>(testCase);

        var deserialized = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(testCase));
        Assert.IsType<XunitTheoryTestCase>(deserialized);
    }

    class ClassWithNonSerializableTheoryData
    {
        public static IEnumerable<object[]> Data = new[] { new[] { new object() }, new[] { new object() } };

        [Theory]
        [MemberData("Data")]
        public void Test(object x) { }
    }
}
