using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Moq;
using Xunit;

public class MultiAssemblyTestEnvironmentTests
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

    public class EnumerateTestAssemblies
    {
        [Fact]
        public void LoadedAssemblyIsPartOfEnumeratedAssemblyList()
        {
            var wrapper = CreateStubExecutor();
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly = mate.Load(wrapper);

            var assemblies = mate.EnumerateTestAssemblies();

            TestAssembly testAssembly = Assert.Single(assemblies);
            Assert.Same(assembly, testAssembly);
        }
    }

    public class EnumerateTestMethods
    {
        [Fact]
        public void UnfilteredReturnsAllTestsFromAllClasses()
        {
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly1 = mate.Load(CreateStubExecutor());
            var assembly2 = mate.Load(CreateStubExecutor("assembly", "config"));

            var tests = mate.EnumerateTestMethods();

            Assert.Equal(8, tests.Count());
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "ThisIsTheType.NonSkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "ThisIsTheType.SkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "This is a custom display name"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "Monkey.DummyMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "ThisIsTheType.NonSkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "ThisIsTheType.SkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "This is a custom display name"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "Monkey.DummyMethod"));
        }

        [Fact]
        public void NullFilterThrows()
        {
            var mate = new MultiAssemblyTestEnvironment();

            Exception ex = Record.Exception(() => mate.EnumerateTestMethods(null).ToList());

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void FilterWithTruePredicate()
        {
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly1 = mate.Load(CreateStubExecutor());
            var assembly2 = mate.Load(CreateStubExecutor("assembly", "config"));

            var tests = mate.EnumerateTestMethods(testMethod => true);

            Assert.Equal(8, tests.Count());
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "ThisIsTheType.NonSkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "ThisIsTheType.SkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "This is a custom display name"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly1 &&
                                        tm.DisplayName == "Monkey.DummyMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "ThisIsTheType.NonSkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "ThisIsTheType.SkipMethod"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "This is a custom display name"));
            Assert.True(tests.Any(tm => tm.TestClass.TestAssembly == assembly2 &&
                                        tm.DisplayName == "Monkey.DummyMethod"));
        }

        [Fact]
        public void FilterWithFalsePredicate()
        {
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly1 = mate.Load(CreateStubExecutor());
            var assembly2 = mate.Load(CreateStubExecutor("assembly", "config"));

            var tests = mate.EnumerateTestMethods(testMethod => false);

            Assert.Equal(0, tests.Count());
        }
    }

    public class Dispose
    {
        [Fact]
        public void DisposesTestAssembliesWhichDisposesWrapper()
        {
            var wrapper = CreateStubExecutor();
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly = mate.Load(wrapper);

            mate.Dispose();

            Assert.True(wrapper.Dispose__Called);
        }

        [Fact]
        public void RemovesDisposedTestAssemblies()
        {
            var wrapper = CreateStubExecutor();
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly = mate.Load(wrapper);

            mate.Dispose();

            Assert.False(mate.EnumerateTestAssemblies().Any());
        }
    }

    public class Load
    {
        [Fact]
        public void NullAssemblyNameThrows()
        {
            var mate = new TestableMultiAssemblyTestEnvironment();

            Assert.Throws<ArgumentNullException>(() => mate.Load((string)null));
            Assert.Throws<ArgumentNullException>(() => mate.Load(null, "configFile"));
        }

        [Fact]
        public void NullExecutorWrapperThrows()
        {
            var mate = new TestableMultiAssemblyTestEnvironment();

            Assert.Throws<ArgumentNullException>(() => mate.Load((IExecutorWrapper)null));
        }

        [Fact]
        public void ReturnsLoadedAssembly()
        {
            var wrapper = CreateStubExecutor();
            var mate = new TestableMultiAssemblyTestEnvironment();

            var testAssembly = mate.Load(wrapper);

            Assert.Same(wrapper, testAssembly.ExecutorWrapper);
        }
    }

    public class Run
    {
        [Fact]
        public void NullTestMethodsThrows()
        {
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();

            Assert.Throws<ArgumentNullException>(() => mate.Run(null, callback.Object));
        }

        [Fact]
        public void EmptyTestMethodsThrows()
        {
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();

            Assert.Throws<ArgumentException>(() => mate.Run(new TestMethod[0], callback.Object));
        }

        [Fact]
        public void NullCallbackThrows()
        {
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var testMethod = mate.Assembly1.Object.EnumerateTestMethods().First();

            Assert.Throws<ArgumentNullException>(() => mate.Run(new[] { testMethod }, null));
        }

        [Fact]
        public void TestMethodNotForThisEnvironmentThrows()
        {
            var wrapper = new Mock<IExecutorWrapper>();
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var testMethod = new TestMethod(null, null, null);
            var testClass = new TestClass(null, new[] { testMethod });
            var testAssembly = new TestAssembly(wrapper.Object, new[] { testClass });
            var callback = new Mock<ITestMethodRunnerCallback>();

            Assert.Throws<ArgumentException>(() => mate.Run(new[] { testMethod }, callback.Object));
        }

        [Fact]
        public void RunSortsClassesByAssemblyAndCallsRunOnAssemblies()
        {
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();
            TestMethod assembly1Method1 = mate.Assembly1.Object.EnumerateTestMethods().First();
            TestMethod assembly2Method2 = mate.Assembly2.Object.EnumerateTestMethods().First();
            mate.Assembly1.Setup(a => a.Run(new[] { assembly1Method1 }, callback.Object))
                          .Returns("<assembly1/>")
                          .Verifiable();
            mate.Assembly2.Setup(c => c.Run(new[] { assembly2Method2 }, callback.Object))
                          .Returns("<assembly2/>")
                          .Verifiable();

            mate.Run(mate.EnumerateTestMethods(), callback.Object);

            mate.Assembly1.Verify();
            mate.Assembly2.Verify();
        }

        [Fact]
        public void RunOnlyCallsRunOnAssembliesWithMethodsToBeRun()
        {
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();
            TestMethod assembly1Method1 = mate.Assembly1.Object.EnumerateTestMethods().First();
            TestMethod assembly2Method2 = mate.Assembly2.Object.EnumerateTestMethods().First();
            mate.Assembly1.Setup(a => a.Run(new[] { assembly1Method1 }, callback.Object))
                          .Returns("<assembly1/>")
                          .Verifiable();

            mate.Run(new[] { assembly1Method1 }, callback.Object);

            mate.Assembly1.Verify();
            mate.Assembly2.Verify(c => c.Run(It.IsAny<IEnumerable<TestMethod>>(), callback.Object), Times.Never());
        }

        [Fact]
        public void RunReturnsAssembliesXml()
        {
            var mate = TestableMultiAssemblyTestEnvironment.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();
            TestMethod assembly1Method1 = mate.Assembly1.Object.EnumerateTestMethods().First();
            TestMethod assembly2Method2 = mate.Assembly2.Object.EnumerateTestMethods().First();
            mate.Assembly1.Setup(a => a.Run(new[] { assembly1Method1 }, callback.Object))
                          .Returns("<assembly1/>")
                          .Verifiable();
            mate.Assembly2.Setup(c => c.Run(new[] { assembly2Method2 }, callback.Object))
                          .Returns("<assembly2/>")
                          .Verifiable();

            var result = mate.Run(mate.EnumerateTestMethods(), callback.Object);

            Assert.Equal("<assemblies><assembly1/><assembly2/></assemblies>", result);
        }
    }

    public class Unload
    {
        [Fact]
        public void NullAssemblyThrows()
        {
            Assert.Throws<ArgumentNullException>(
                () => new TestableMultiAssemblyTestEnvironment().Unload(null));
        }

        [Fact]
        public void AssemblyNotInAssemblyList()
        {
            var wrapper = CreateStubExecutor();
            var assembly = new TestAssembly(wrapper, new TestClass[0]);

            Assert.Throws<ArgumentException>(
                () => new TestableMultiAssemblyTestEnvironment().Unload(assembly));
        }

        [Fact]
        public void LoadedAssemblyIsPartOfEnumeratedAssemblyList()
        {
            var wrapper = CreateStubExecutor();
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly = mate.Load(wrapper);

            mate.Unload(assembly);

            Assert.Equal(0, mate.EnumerateTestAssemblies().Count());
        }

        [Fact]
        public void ExecutorShouldBeDisposedOnUnload()
        {
            var wrapper = CreateStubExecutor();
            var mate = new TestableMultiAssemblyTestEnvironment();
            var assembly = mate.Load(wrapper);

            mate.Unload(assembly);

            Assert.True(wrapper.Dispose__Called);
        }
    }

    class TestableMultiAssemblyTestEnvironment : MultiAssemblyTestEnvironment
    {
        public Mock<TestAssembly> Assembly1 { get; set; }

        public Mock<TestAssembly> Assembly2 { get; set; }

        public static TestableMultiAssemblyTestEnvironment Create()
        {
            var wrapper1 = new Mock<IExecutorWrapper>();
            var wrapper2 = new Mock<IExecutorWrapper>();
            var class1Method1 = new TestMethod("method1", null, null);
            var class2Method2 = new TestMethod("method2", null, null);
            var class1 = new TestClass("foo", new[] { class1Method1 });
            var class2 = new TestClass("bar", new[] { class2Method2 });
            var assembly1 = new Mock<TestAssembly>(wrapper1.Object, new[] { class1 });
            var assembly2 = new Mock<TestAssembly>(wrapper2.Object, new[] { class2 });
            var result = new TestableMultiAssemblyTestEnvironment();
            result.Assembly1 = assembly1;
            result.Assembly2 = assembly2;
            result.TestAssemblies.Add(assembly1.Object);
            result.TestAssemblies.Add(assembly2.Object);

            return result;
        }

        public new TestAssembly Load(IExecutorWrapper executorWrapper)
        {
            return base.Load(executorWrapper);
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