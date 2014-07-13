using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SerializationTests
{
    [Fact]
    public static void SerializedTestsInSameCollectionRemainInSameCollection()
    {
        var sourceProvider = new NullSourceInformationProvider();
        var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
        var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider);
        var visitor = new TestDiscoveryVisitor();

        discoverer.Find(typeof(ClassWithFacts).FullName, false, visitor, new XunitDiscoveryOptions());
        visitor.Finished.WaitOne();

        var first = visitor.TestCases[0];
        var second = visitor.TestCases[1];
        Assert.NotEqual(first.UniqueID, second.UniqueID);

        Assert.True(TestCollectionComparer.Instance.Equals(first.TestMethod.TestClass.TestCollection, second.TestMethod.TestClass.TestCollection));

        var deserializedFirst = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(first));
        var deserializedSecond = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(second));

        Assert.NotSame(deserializedFirst.TestMethod.TestClass.TestCollection, deserializedSecond.TestMethod.TestClass.TestCollection);
        Assert.True(TestCollectionComparer.Instance.Equals(deserializedFirst.TestMethod.TestClass.TestCollection, deserializedSecond.TestMethod.TestClass.TestCollection));
    }

    class ClassWithFacts
    {
        [Fact]
        public void Test1() { }

        [Fact]
        public void Test2() { }
    }

    [Fact]
    public static void TheoriesAlwaysComeBackAsSingleXunitTheoryTestCase()
    {
        var sourceProvider = new NullSourceInformationProvider();
        var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
        var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider);
        var visitor = new TestDiscoveryVisitor();

        discoverer.Find(typeof(ClassWithTheory).FullName, false, visitor, new XunitDiscoveryOptions());
        visitor.Finished.WaitOne();

        var testCase = Assert.Single(visitor.TestCases);
        Assert.IsType<XunitTheoryTestCase>(testCase);

        var deserialized = SerializationHelper.Deserialize<ITestCase>(SerializationHelper.Serialize(testCase));
        Assert.IsType<XunitTheoryTestCase>(deserialized);
    }

    class ClassWithTheory
    {
        [Theory]
        [InlineData(1)]
        [InlineData("hello")]
        public void Test(object x) { }
    }
}
