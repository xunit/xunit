using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

public class Xunit2Tests
{
    static readonly AssemblyName XunitAssemblyName = GetXunitAssemblyName();

    static AssemblyName GetXunitAssemblyName()
    {
        string path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "xunit2.dll");
        return Assembly.LoadFile(path).GetName();
    }

    public class EnumerateTests
    {
        [Fact]
        public void NoTestMethods()
        {
            using (var assm = new AcceptanceTestAssembly(code: ""))
            using (var controller = new TestableXunit2(assm.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink);

                sink.Finished.WaitOne();

                Assert.False(sink.Messages.Any(msg => msg is ITestCaseDiscoveryMessage));
            }
        }

        [Fact]
        public void SingleTestMethod()
        {
            string code = @"
                using Xunit;
        
                public class Foo
                {
                    [Fact]
                    public void Bar() { }
                }
            ";

            using (var assm = new AcceptanceTestAssembly(code))
            using (var controller = new TestableXunit2(assm.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink);

                sink.Finished.WaitOne();

                ITestCase testCase = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Single().TestCase;
                Assert.Equal("Foo.Bar", testCase.DisplayName);
            }
        }

        [Fact]
        public void FactAcceptanceTest()
        {
            string code = @"
                using System;
                using Xunit;
                
                namespace Namespace1
                {
                    public class Class1
                    {
                        [Fact]
                        [Trait(""Name!"", ""Value!"")]
                        public void Trait() { }
                
                        [Fact(Skip=""Skipping"")]
                        public void Skipped() { }
                
                        [Fact(DisplayName=""Custom Test Name"")]
                        public void CustomName() { }
                    }
                }
                
                namespace Namespace2
                {
                    public class OuterClass
                    {
                        public class Class2
                        {
                            [Fact]
                            public void TestMethod() { }
                        }
                    }
                }
            ";

            using (var assembly = new AcceptanceTestAssembly(code))
            using (var controller = new TestableXunit2(assembly.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink);

                sink.Finished.WaitOne();
                ITestCase[] testCases = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(tcdm => tcdm.TestCase).ToArray();

                Assert.Equal(4, testCases.Length);

                ITestCase traitTest = Assert.Single(testCases, tc => tc.DisplayName == "Namespace1.Class1.Trait");
                KeyValuePair<string, string> kvp = Assert.Single(traitTest.Traits);
                Assert.Equal("Name!", kvp.Key);
                Assert.Equal("Value!", kvp.Value);

                ITestCase skipped = Assert.Single(testCases, tc => tc.DisplayName == "Namespace1.Class1.Skipped");
                Assert.Equal("Skipping", skipped.SkipReason);

                Assert.Single(testCases, tc => tc.DisplayName == "Custom Test Name");
                Assert.Single(testCases, tc => tc.DisplayName == "Namespace2.OuterClass+Class2.TestMethod");
            }
        }

        [Fact]
        public void TheoryWithInlineData()
        {
            string code = @"
                using System;
                using Xunit;
                
                public class TestClass
                {
                    [Theory]
                    [InlineData]
                    [InlineData(42)]
                    [InlineData(42, 21.12)]
                    public void TestMethod(int x) { }
                }
            ";

            using (var assembly = new AcceptanceTestAssembly(code))
            using (var controller = new TestableXunit2(assembly.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink);

                sink.Finished.WaitOne();
                string[] testCaseNames = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(tcdm => tcdm.TestCase.DisplayName).ToArray();

                Assert.Equal(3, testCaseNames.Length);

                Assert.Contains("TestClass.TestMethod(x: ???)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42, ???: 21.12)", testCaseNames);
            }
        }
    }

    class TestableXunit2 : Xunit2
    {
        public TestableXunit2(string assemblyFileName, string configFileName = null, bool shadowCopy = true)
            : base(new NullSourceInformationProvider(), assemblyFileName, configFileName, shadowCopy)
        {
        }
    }
}