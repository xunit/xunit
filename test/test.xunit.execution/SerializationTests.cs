using System;
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
            Assert.DoesNotThrow(delegate { SerializableObject o2 = (SerializableObject)o; });
        }
    }

    [Fact]
    public static void SerializedTestsInSameCollectionRemainInSameCollection()
    {
        var sourceProvider = new NullSourceInformationProvider();
        var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
        var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider);
        var visitor = new TestDiscoveryVisitor();

        discoverer.Find(typeof(ClassUnderTest).FullName, false, visitor, new XunitDiscoveryOptions());
        visitor.Finished.WaitOne();

        var first = visitor.TestCases[0];
        var second = visitor.TestCases[1];

        Assert.True(TestCollectionComparer.Instance.Equals(first.TestMethod.TestClass.TestCollection, second.TestMethod.TestClass.TestCollection));

        var serializedFirst = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(first));
        var serializedSecond = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(second));

        Assert.NotSame(serializedFirst.TestMethod.TestClass.TestCollection, serializedSecond.TestMethod.TestClass.TestCollection);
        Assert.True(TestCollectionComparer.Instance.Equals(serializedFirst.TestMethod.TestClass.TestCollection, serializedSecond.TestMethod.TestClass.TestCollection));
    }

    class ClassUnderTest
    {
        [Fact]
        public void Test1() { }

        [Fact]
        public void Test2() { }
    }
}