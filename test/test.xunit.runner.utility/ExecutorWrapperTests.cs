using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using TestUtility;
using Xunit;

public class ExecutorWrapperTests
{
    static AssemblyName XunitAssemblyName
    {
        get
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssemblyName name = assembly.GetName();

                if (name.Name.ToLowerInvariant() == "xunit")
                    return name;
            }

            throw new Exception("Could not find xunit.dll in currently loaded assembly list");
        }
    }

    public class AppDomainBehavior : AcceptanceTestInNewAppDomain
    {
        [Fact]
        public void ShouldNotBeExcutingInTheSameAppDomain()
        {
            string codeTemplate = @"
                using System;
                using System.Diagnostics;
                using Xunit;

                public class AppDomainTest
                {{
                    [Fact]
                    public void TestDomainName()
                    {{
                        Assert.False({0} == AppDomain.CurrentDomain.Id);
                    }}
                }}
            ";

            string code = String.Format(codeTemplate, AppDomain.CurrentDomain.Id);

            XmlNode assemblyNode = Execute(code);

            ResultXmlUtility.AssertResult(assemblyNode, "Pass", "AppDomainTest.TestDomainName");
        }
    }

    public class Cancellation
    {
        [Fact]
        public void CanCancelBetweenTestMethodRuns()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void TestMethod1()
                    {
                    }

                    [Fact]
                    public void TestMethod2()
                    {
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    wrapper.RunClass("TestClass", node => { lastNode = node; return false; });

                Assert.Equal(0, lastNode.ChildNodes.Count);   // Cancels from the start of the first test
            }
        }
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
                    [Fact]
                    public void TestMethod()
                    {
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, @"C:\Foo\bar.config", false))
                {
                    Assert.Equal(assembly.FileName, wrapper.AssemblyFilename);
                    Assert.Equal(@"C:\Foo\bar.config", wrapper.ConfigFilename);
                    Assert.Equal(XunitAssemblyName.Version.ToString(), wrapper.XunitVersion);
                }
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

                Exception ex = Record.Exception(() => new ExecutorWrapper(assemblyFilename, null, false));

                Assert.IsType<ArgumentException>(ex);
                Assert.Contains("xunit.dll", ex.Message);
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

                string xunitFilename = Path.Combine(tempPath, "xunit.dll");
                File.Copy(new Uri(XunitAssemblyName.CodeBase).LocalPath, xunitFilename);

                Assert.Throws<BadImageFormatException>(() => new ExecutorWrapper(assemblyFilename, null, false));
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    public class DefaultConfigFileBehavior : AcceptanceTestInNewAppDomain, IDisposable
    {
        static readonly string assemblyFileName = Path.GetRandomFileName();
        static readonly string configFile = assemblyFileName + ".dll.config";

        public DefaultConfigFileBehavior()
        {
            string config =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <appSettings>
        <add key=""ConfigurationValue"" value=""42"" />
    </appSettings>
</configuration>";

            File.WriteAllText(configFile, config);
        }

        public void Dispose()
        {
            if (File.Exists(configFile))
                File.Delete(configFile);
        }

        [Fact]
        public void ValueFromUserSpecifiedConfigFile()
        {
            string code = @"
                using System;
                using System.Configuration;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void CheckConfigurationFileEntry()
                    {
                        Assert.Equal(ConfigurationSettings.AppSettings[""ConfigurationValue""], ""42"");
                    }
                }
            ";

            XmlNode assemblyNode;

            using (MockAssembly mockAssembly = new MockAssembly(assemblyFileName))
            {
                mockAssembly.Compile(code, null);
                assemblyNode = mockAssembly.Run(configFile);
            }

            ResultXmlUtility.AssertResult(assemblyNode, "Pass", "MockTestClass.CheckConfigurationFileEntry");
            Assert.Equal(Path.GetFullPath(configFile), assemblyNode.Attributes["configFile"].Value);
        }
    }

    public class InvalidConfigurationFileBehavior : AcceptanceTestInNewAppDomain, IDisposable
    {
        static readonly string assemblyFileName = Path.GetRandomFileName();
        static readonly string configFile = assemblyFileName + ".dll.config";

        public InvalidConfigurationFileBehavior()
        {
            string config = @"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration!>
</configuration!>";

            File.WriteAllText(configFile, config);
        }

        public void Dispose()
        {
            if (File.Exists(configFile))
                File.Delete(configFile);
        }

        [Fact]
        public void ConfigurationExceptionShouldBeThrown()
        {
            string code = @"
                using System;
                using System.Configuration;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void TestMethod()
                    {
                    }
                }
            ";

            XmlNode assemblyNode;

            using (MockAssembly mockAssembly = new MockAssembly(assemblyFileName))
            {
                mockAssembly.Compile(code, null);
                assemblyNode = mockAssembly.Run(configFile);
            }

            var resultNode = ResultXmlUtility.GetResult(assemblyNode);
            var failureNode = resultNode.SelectSingleNode("failure");
            Assert.NotNull(failureNode);
            Assert.Equal("System.Configuration.ConfigurationErrorsException", failureNode.Attributes["exception-type"].Value);
        }
    }

    public class Lifetime
    {
        [Fact]
        public void CallbackHandlerImplementsForeverLifetimePolicy()
        {
            ExecutorWrapper.IntCallbackHandler handler = new ExecutorWrapper.IntCallbackHandlerWithIMessageSink();

            object result = handler.InitializeLifetimeService();

            Assert.Null(result);
        }

        [Fact]
        public void XmlNodeCallbackHandlerImplementsForeverLifetimePolicy()
        {
            ExecutorWrapper.XmlNodeCallbackHandler handler = new ExecutorWrapper.XmlNodeCallbackHandlerWithIMessageSink(null, null);

            object result = handler.InitializeLifetimeService();

            Assert.Null(result);
        }

        [Fact]
        public void OutgoingMessageImplementsForeverLifetimePolicy()
        {
            ExecutorWrapper.OutgoingMessage message = new ExecutorWrapper.OutgoingMessage(null);

            object result = message.InitializeLifetimeService();

            Assert.Null(result);
        }
    }

    public class EnumerateTests
    {
        [Fact]
        public void AcceptanceTest()
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
                        public void Passing()
                        {
                            Assert.Equal(2, 2);
                        }

                        [Fact]
                        public void Failing()
                        {
                            Assert.Equal(2, 3);
                        }

                        [Fact(Skip=""Skipping"")]
                        public void Skipped() {}

                        [Fact(Name=""Custom Test Name"")]
                        public void CustomName() {}
                    }
                }

                namespace Namespace2
                {
                    public class OuterClass
                    {
                        public class Class2
                        {
                            [Fact]
                            public void Passing()
                            {
                                Assert.Equal(2, 2);
                            }
                        }
                    }
                }
            ";

            XmlNode assemblyNode = null;
            string filename = null;

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);
                filename = assembly.FileName;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    assemblyNode = wrapper.EnumerateTests();
            }

            Assert.Equal(filename, assemblyNode.Attributes["name"].Value);

            XmlNodeList classNodes = assemblyNode.SelectNodes("class");
            Assert.Equal(classNodes.Count, 2);

            XmlNode class1Node = classNodes[0];
            Assert.Equal("Namespace1.Class1", class1Node.Attributes["name"].Value);

            XmlNodeList class1MethodNodes = class1Node.SelectNodes("method");
            Assert.Equal(class1MethodNodes.Count, 4);
            XmlNode passingNode = class1Node.SelectSingleNode(@"//method[@method=""Passing""]");
            Assert.NotNull(passingNode);
            Assert.Equal("Namespace1.Class1.Passing", passingNode.Attributes["name"].Value);
            XmlNodeList traitsNodes = passingNode.SelectNodes("traits/trait");
            XmlNode traitNode = (XmlNode)Assert.Single(traitsNodes);
            Assert.Equal("Name!", traitNode.Attributes["name"].Value);
            Assert.Equal("Value!", traitNode.Attributes["value"].Value);
            Assert.NotNull(class1Node.SelectSingleNode(@"//method[@method=""Failing""]"));
            XmlNode skipNode = class1Node.SelectSingleNode(@"//method[@method=""Skipped""]");
            Assert.NotNull(skipNode);
            Assert.Equal("Skipping", skipNode.Attributes["skip"].Value);
            XmlNode customNameNode = class1Node.SelectSingleNode(@"//method[@method=""CustomName""]");
            Assert.NotNull(customNameNode);
            Assert.Equal("Custom Test Name", customNameNode.Attributes["name"].Value);

            XmlNode class2Node = classNodes[1];
            Assert.Equal("Namespace2.OuterClass+Class2", class2Node.Attributes["name"].Value);

            XmlNodeList class2MethodNodes = class2Node.SelectNodes("method");
            Assert.Equal(class2MethodNodes.Count, 1);
            Assert.Equal("Namespace2.OuterClass+Class2", class2MethodNodes[0].Attributes["type"].Value);
            Assert.Equal("Passing", class2MethodNodes[0].Attributes["method"].Value);
        }
    }

    public class GetAssemblyTestCount
    {
        [Fact]
        public void AssemblyWithNoTests()
        {
            string code = @"
                public class JustAPlainOldClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Equal(0, wrapper.GetAssemblyTestCount());
            }
        }

        [Fact]
        public void AssemblyWithMultipleTestsAndMultipleClasses()
        {
            string code = @"
                using Xunit;

                public class JustAPlainOldClass
                {
                    public class Class1
                    {
                        [Fact] public void Test1() {}
                        [Fact] public void Test2() {}
                    }

                    public class Class2
                    {
                        [Fact] public void Test3() {}
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Equal(3, wrapper.GetAssemblyTestCount());
            }
        }
    }

    public class RunAssembly
    {
        [Fact]
        public void AcceptanceTest()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void TestMethod()
                    {
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;
                XmlNode returnValue = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    returnValue = wrapper.RunAssembly(node => { lastNode = node; return true; });

                XmlNode resultNode = ResultXmlUtility.GetResult(lastNode);
                Assert.Equal("Pass", resultNode.Attributes["result"].Value);
                Assert.Equal(returnValue, lastNode);
            }
        }

        [Fact]
        public void AssemblyWithNoTests()
        {
            string code = @"
                using Xunit;

                public class PlainOldDotNetClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    wrapper.RunAssembly(node => { lastNode = node; return true; });

                Assert.NotNull(lastNode);   // Always get an <assembly> node, even if there are no tests
                Assert.Equal(0, lastNode.ChildNodes.Count);
            }
        }
    }

    public class RunClass
    {
        [Fact]
        public void AcceptanceTest()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void TestMethod()
                    {
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;
                XmlNode returnValue = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    returnValue = wrapper.RunClass("TestClass", node => { lastNode = node; return true; });

                XmlNode resultNode = ResultXmlUtility.GetResult(lastNode);
                Assert.Equal("Pass", resultNode.Attributes["result"].Value);
                Assert.Equal(returnValue, lastNode);
            }
        }

        [Fact]
        public void ClassWhichHasNoTests()
        {
            string code = @"
                using Xunit;

                public class PlainOldDotNetClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    wrapper.RunClass("PlainOldDotNetClass", node => { lastNode = node; return true; });

                Assert.Equal("class", lastNode.Name);
                Assert.Equal(0, lastNode.ChildNodes.Count);   // Empty class node
            }
        }

        [Fact]
        public void InvalidClassName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(() => wrapper.RunClass("TestClassIsNotMe", null));
            }
        }
    }

    public class RunTest
    {
        [Fact]
        public void AcceptanceTest()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void TestMethod()
                    {
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;
                XmlNode returnValue = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    returnValue = wrapper.RunTest("TestClass", "TestMethod", node => { lastNode = node; return true; });

                XmlNode resultNode = ResultXmlUtility.GetResult(lastNode);
                Assert.Equal("Pass", resultNode.Attributes["result"].Value);
                Assert.Equal(returnValue, lastNode);
            }
        }

        [Fact]
        public void NonTestMethodInClassWithTestMethod()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    public void NonTestMethod()
                    {
                    }

                    [Fact]
                    public void TestMethod()
                    {
                    }
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    wrapper.RunTest("TestClass", "NonTestMethod", node => { lastNode = node; return true; });

                Assert.Equal("class", lastNode.Name);
                Assert.Equal(0, lastNode.ChildNodes.Count);   // Empty class node
            }
        }

        [Fact]
        public void InvalidClassName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(() => wrapper.RunTest("TestClassIsNotMe", "DummyMethod", null));
            }
        }

        [Fact]
        public void InvalidMethodName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(() => wrapper.RunTest("TestClass", "DummyMethod", null));
            }
        }

        [Fact]
        public void AmbiguousMethodName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    public void DummyMethod() {}
                    public void DummyMethod(string s) {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(() => wrapper.RunTest("TestClass", "DummyMethod", null));
            }
        }
    }

    public class RunTests
    {
        [Fact]
        public void AcceptanceTest()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact] public void TestMethod1() {}
                    [Fact] public void TestMethod2() {}
                    [Fact] public void TestMethod3() {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;
                XmlNode returnValue = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    returnValue = wrapper.RunTests("TestClass",
                                                   new List<string> { "TestMethod1", "TestMethod2" },
                                                   node => { lastNode = node; return true; });

                Assert.Equal(returnValue, lastNode);
                Assert.Equal(2, lastNode.ChildNodes.Count); // Two test results
                XmlNode result0 = ResultXmlUtility.GetResult(lastNode, 0);
                Assert.Equal("Pass", result0.Attributes["result"].Value);
                XmlNode result1 = ResultXmlUtility.GetResult(lastNode, 1);
                Assert.Equal("Pass", result1.Attributes["result"].Value);
            }
        }

        [Fact]
        public void CallbackIncludesStartMessages()
        {
            const string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact] public void TestMethod1() {}
                    [Fact] public void TestMethod2() {}
                    [Fact] public void TestMethod3() {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                List<XmlNode> nodes = new List<XmlNode>();

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    wrapper.RunTests("TestClass",
                                     new List<string> { "TestMethod1" },
                                     node => { nodes.Add(node); return true; });

                Assert.Equal(3, nodes.Count);
                Assert.Equal("start", nodes[0].Name);  // <start>
                ResultXmlUtility.AssertAttribute(nodes[0], "name", "TestClass.TestMethod1");
                ResultXmlUtility.AssertAttribute(nodes[0], "type", "TestClass");
                ResultXmlUtility.AssertAttribute(nodes[0], "method", "TestMethod1");
                Assert.Equal("test", nodes[1].Name);
                Assert.Equal("class", nodes[2].Name);
            }
        }

        [Fact]
        public void TestMethodWithNonTestMethod()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact] public void TestMethod1() {}
                    [Fact] public void TestMethod2() {}
                    [Fact] public void TestMethod3() {}
                    public void NonTestMethod() {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode lastNode = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    wrapper.RunTests("TestClass",
                                     new List<string> { "TestMethod1", "NonTestMethod" },
                                     node => { lastNode = node; return true; });

                Assert.Single(lastNode.ChildNodes); // Only the test method
                XmlNode result = ResultXmlUtility.GetResult(lastNode, 0);
                Assert.Equal("Pass", result.Attributes["result"].Value);
            }
        }

        [Fact]
        public void InvalidClassName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(
                        () => wrapper.RunTests("TestClassIsNotMe",
                                               new List<string> { "DummyMethod" },
                                               null));
            }
        }

        [Fact]
        public void InvalidMethodName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    public void DummyMethod() {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(
                        () => wrapper.RunTests("TestClass",
                                               new List<string> { "DummyMethod", "DummyMethod2" },
                                               null));
            }
        }

        [Fact]
        public void AmbiguousMethodName()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    public void DummyMethod() {}
                    public void DummyMethod(string s) {}
                    public void DummyMethod2() {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    Assert.Throws<ArgumentException>(
                        () => wrapper.RunTests("TestClass",
                                               new List<string> { "DummyMethod", "DummyMethod2" },
                                               null));
            }
        }

        [Fact]
        public void NonPublicTestMethod()
        {
            string code = @"
                using Xunit;

                public class TestClass
                {
                    [Fact] void NonPublicTestMethod() {}
                }
            ";

            using (MockAssembly assembly = new MockAssembly())
            {
                assembly.Compile(code);

                XmlNode returnValue = null;

                using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.FileName, null, false))
                    returnValue = wrapper.RunTests("TestClass",
                                                   new List<string> { "NonPublicTestMethod" },
                                                   node => { return true; });

                Assert.Single(returnValue.ChildNodes);
                XmlNode result = ResultXmlUtility.GetResult(returnValue, 0);
                Assert.Equal("Pass", result.Attributes["result"].Value);
            }
        }
    }

    public class SpecfiedConfigFileBehavior : AcceptanceTestInNewAppDomain, IDisposable
    {
        readonly string configFile;

        public SpecfiedConfigFileBehavior()
        {
            configFile = Path.Combine(MockAssembly.BasePath, Path.GetRandomFileName());

            string config =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <appSettings>
        <add key=""ConfigurationValue"" value=""42"" />
    </appSettings>
</configuration>
";

            File.WriteAllText(configFile, config);
        }

        public void Dispose()
        {
            if (File.Exists(configFile))
                File.Delete(configFile);
        }

        [Fact]
        public void ValueFromUserSpecifiedConfigFile()
        {
            string code = @"
                using System;
                using System.Configuration;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void CheckConfigurationFileEntry()
                    {
                        Assert.Equal(ConfigurationSettings.AppSettings[""ConfigurationValue""], ""42"");
                    }
                }
            ";

            XmlNode assemblyNode = Execute(code, configFile);

            ResultXmlUtility.AssertResult(assemblyNode, "Pass", "MockTestClass.CheckConfigurationFileEntry");
        }
    }
}