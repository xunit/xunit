using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

public class Xunit2AcceptanceTests
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
            using (var assm = new MockAssembly(code: ""))
            using (var controller = new XunitFrontController(assm.FileName, null, true))
                Assert.Empty(controller.EnumerateTests());
        }

        [Fact]
        public void SingleTestMethod()
        {
            string code = @"
                using Xunit;
        
                public class Foo
                {
                    [Fact2]
                    public void Bar() { }
                }
            ";

            using (var assm = new MockAssembly(code))
            using (var controller = new XunitFrontController(assm.FileName, null, true))
            {
                ITestCase testCase = Assert.Single(controller.EnumerateTests());
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
                        [Fact2]
                        [Trait2(""Name!"", ""Value!"")]
                        public void Trait() { }
        
                        [Fact2(Skip=""Skipping"")]
                        public void Skipped() { }
        
                        [Fact2(DisplayName=""Custom Test Name"")]
                        public void CustomName() { }
                    }
                }
        
                namespace Namespace2
                {
                    public class OuterClass
                    {
                        public class Class2
                        {
                            [Fact2]
                            public void TestMethod() { }
                        }
                    }
                }
            ";

            using (var assembly = new MockAssembly(code))
            using (var controller = new XunitFrontController(assembly.FileName, null, true))
            {
                ITestCase[] testCases = controller.EnumerateTests().ToArray();

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
                    [Theory2]
                    [InlineData2]
                    [InlineData2(42)]
                    [InlineData2(42, 21.12)]
                    public void TestMethod(int x) { }
                }
            ";

            using (var assembly = new MockAssembly(code))
            using (var controller = new XunitFrontController(assembly.FileName, null, true))
            {
                string[] testCaseNames = controller.EnumerateTests().Select(tc => tc.DisplayName).ToArray();

                Assert.Equal(3, testCaseNames.Length);

                Assert.Contains("TestClass.TestMethod(x: ???)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42, ???: 21.12)", testCaseNames);
            }
        }
    }
}
