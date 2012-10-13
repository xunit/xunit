using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

public class Executor2WrapperTests
{
    static readonly AssemblyName XunitAssemblyName = GetXunitAssemblyName();

    static AssemblyName GetXunitAssemblyName()
    {
#if true
        string path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "xunit2.dll");
        Assembly assm = Assembly.LoadFile(path);
        return assm.GetName();
#else
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            AssemblyName name = assembly.GetName();

            if (name.Name.ToLowerInvariant() == "xunit2")
                return name;
        }
#endif

        throw new Exception("Could not find xunit2.dll in currently loaded assembly list");
    }

    public class Construction
    {
        [Fact]
        public void SuccessfulConstructionCanReturnAssemblyFilename()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact2]
                    public void TestMethod()
                    {
                    }
                }
            ";

            using (var assembly = new MockAssembly(code))
            using (var wrapper = new Executor2Wrapper(assembly.FileName, @"C:\Foo\bar.config", false))
            {
                Assert.Equal(assembly.FileName, wrapper.AssemblyFileName);
                Assert.Equal(@"C:\Foo\bar.config", wrapper.ConfigFileName);
                Assert.Equal(XunitAssemblyName.Version.ToString(), wrapper.XunitVersion);
            }
        }

        [Fact]
        public void CannotConstructWithMissingXunitDll()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                string assemblyFilename = Path.Combine(tempPath, "test.dll");
                File.WriteAllText(assemblyFilename, "This is not an assembly");

                Exception ex = Record.Exception(() => new Executor2Wrapper(assemblyFilename, null, false));

                Assert.IsType<ArgumentException>(ex);
                Assert.Contains("xunit2.dll", ex.Message);
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void ConstructionWithNonAssemblyThrows()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                string assemblyFilename = Path.Combine(tempPath, "test.dll");
                File.WriteAllText(assemblyFilename, "This is not an assembly");

                string xunitFilename = Path.Combine(tempPath, "xunit2.dll");
                File.Copy(new Uri(XunitAssemblyName.CodeBase).LocalPath, xunitFilename);

                Assert.Throws<BadImageFormatException>(() => new Executor2Wrapper(assemblyFilename, null, false));
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    public class EnumerateTests
    {
        [Fact]
        public void NoTestMethods()
        {
            using (MockAssembly assm = new MockAssembly(code: ""))
            using (Executor2Wrapper wrapper = new Executor2Wrapper(assm.FileName, null, true))
                Assert.Empty(wrapper.EnumerateTests());
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

            using (MockAssembly assm = new MockAssembly(code))
            using (Executor2Wrapper wrapper = new Executor2Wrapper(assm.FileName, null, true))
            {
                ITestCase testCase = Assert.Single(wrapper.EnumerateTests());
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
                        [Trait(""Name!"", ""Value!"")]
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
            {
                string filename = assembly.FileName;

                using (var wrapper = new Executor2Wrapper(filename, null, false))
                {
                    ITestCase[] testCases = wrapper.EnumerateTests().ToArray();

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

            using (var assembly = new MockAssembly(code))
            using (var wrapper = new Executor2Wrapper(assembly.FileName, null, false))
            {
                string[] testCaseNames = wrapper.EnumerateTests().Select(tc => tc.DisplayName).ToArray();

                Assert.Equal(3, testCaseNames.Length);

                Assert.Contains("TestClass.TestMethod(x: ???)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42)", testCaseNames);
                Assert.Contains("TestClass.TestMethod(x: 42, ???: 21.12)", testCaseNames);
            }
        }
    }
}
