using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit;

public class TestAssemblyBuilderTests
{
    private const string TestListXml = @"
<assembly name='C:\Foo\Bar.dll'>
    <class name='ThisIsTheType'>
        <method type='ThisIsTheType' method='NonSkipMethod' />
        <method type='ThisIsTheType' method='SkipMethod' skip='Skip Reason' />
        <method type='ThisIsTheType' method='TestMethodWithTraits' name='This is a custom display name'>
            <traits>
                <trait name='Category' value='Foo' />
                <trait name='Author' value='Dumb Attribute' />
            </traits>
        </method>
    </class>
    <class name='Monkey'>
        <method type='Monkey' method='DummyMethod' />
    </class>
</assembly>";

    public class Build
    {
        [Fact]
        public void CreatesAssemblyObjectWithAssemblyInformation()
        {
            StubExecutorWrapper wrapper = CreateStubExecutor(@"C:\Foo\Bar.dll", @"C:\Biff\Baz.config");

            TestAssembly assembly = TestAssemblyBuilder.Build(wrapper);

            Assert.Equal(@"C:\Foo\Bar.dll", assembly.AssemblyFilename);
            Assert.Equal(@"C:\Biff\Baz.config", assembly.ConfigFilename);
            Assert.Equal(wrapper.XunitVersion, assembly.XunitVersion);
            Assert.Same(wrapper, assembly.ExecutorWrapper);
        }

        [Fact]
        public void AssemblyObjectHasClassList()
        {
            StubExecutorWrapper wrapper = CreateStubExecutor();

            TestAssembly assembly = TestAssemblyBuilder.Build(wrapper);

            var testClasses = assembly.EnumerateClasses();
            Assert.Equal(2, testClasses.Count());
            TestClass testClass = testClasses.First();
            Assert.Equal("ThisIsTheType", testClass.TypeName);
        }

        [Fact]
        public void ClassObjectHasTestList()
        {
            StubExecutorWrapper wrapper = CreateStubExecutor();

            TestAssembly assembly = TestAssemblyBuilder.Build(wrapper);

            TestClass testClass = assembly.EnumerateClasses().First();
            IEnumerable<TestMethod> testMethods = testClass.EnumerateTestMethods();
            Assert.True(testMethods.Any(tm => tm.TestClass == testClass &&
                                        tm.MethodName == "NonSkipMethod" &&
                                        tm.DisplayName == "ThisIsTheType.NonSkipMethod" &&
                                        tm.RunResults.Count == 0 &&
                                        tm.Traits.Count == 0));
            Assert.True(testMethods.Any(tm => tm.TestClass == testClass &&
                                        tm.MethodName == "TestMethodWithTraits" &&
                                        tm.DisplayName == "This is a custom display name" &&
                                        tm.RunResults.Count == 0 &&
                                        tm.Traits["Category"].FirstOrDefault() == "Foo" &&
                                        tm.Traits["Author"].FirstOrDefault() == "Dumb Attribute"));
            Assert.True(testMethods.Any(tm => tm.TestClass == testClass &&
                                        tm.MethodName == "SkipMethod" &&
                                        tm.DisplayName == "ThisIsTheType.SkipMethod" &&
                                        tm.RunResults.Count == 1 &&
                                        ((TestSkippedResult)tm.RunResults[0]).Reason == "Skip Reason" &&
                                        tm.Traits.Count == 0));
        }
    }

    static StubExecutorWrapper CreateStubExecutor()
    {
        return CreateStubExecutor(@"C:\Foo\Bar.dll", null);
    }

    static StubExecutorWrapper CreateStubExecutor(string assemblyFilename, string configFilename)
    {
        return new StubExecutorWrapper
        {
            AssemblyFilename = assemblyFilename,
            ConfigFilename = configFilename,
            XunitVersion = "1.2.3.4",
            EnumerateTests__Result = LoadXml(TestListXml),
        };
    }

    static XmlNode LoadXml(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        return doc.ChildNodes[0];
    }
}
