using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class Xunit2Tests
{
    public class EnumerateTests
    {
        [Fact]
        public void NoTestMethods()
        {
            using (var assm = AcceptanceTestV2Assembly.Create(code: ""))
            using (var controller = new TestableXunit2(assm.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: TestFrameworkOptions.ForDiscovery());

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

            using (var assm = AcceptanceTestV2Assembly.Create(code))
            using (var controller = new TestableXunit2(assm.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: TestFrameworkOptions.ForDiscovery());

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

            using (var assembly = AcceptanceTestV2Assembly.Create(code))
            using (var controller = new TestableXunit2(assembly.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: TestFrameworkOptions.ForDiscovery());

                sink.Finished.WaitOne();
                ITestCase[] testCases = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(tcdm => tcdm.TestCase).ToArray();

                Assert.Equal(4, testCases.Length);

                ITestCase traitTest = Assert.Single(testCases, tc => tc.DisplayName == "Namespace1.Class1.Trait");
                string key = Assert.Single(traitTest.Traits.Keys);
                Assert.Equal("Name!", key);
                string value = Assert.Single(traitTest.Traits[key]);
                Assert.Equal("Value!", value);

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

            using (var assembly = AcceptanceTestV2Assembly.Create(code))
            using (var controller = new TestableXunit2(assembly.FileName, null, true))
            {
                var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();

                controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: TestFrameworkOptions.ForDiscovery());

                sink.Finished.WaitOne();
                string[] testCaseNames = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(tcdm => tcdm.TestCase.DisplayName).ToArray();

                Assert.Equal(3, testCaseNames.Length);

                Assert.Contains("TestClass.TestMethod(x: ???)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42)", testCaseNames);
                Assert.Contains(string.Format("TestClass.TestMethod(x: 42, ???: {0})", 21.12), testCaseNames);
            }
        }
    }

    class TestableXunit2 : Xunit2
    {
        public TestableXunit2(string assemblyFileName, string configFileName = null, bool shadowCopy = true, bool useAppDomain = true)
            : base(useAppDomain, new NullSourceInformationProvider(), assemblyFileName, configFileName, shadowCopy)
        {
        }
    }
}