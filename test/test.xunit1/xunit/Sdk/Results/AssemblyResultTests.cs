using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class AssemblyResultTests : IDisposable
    {
        static readonly string assembly = "StubAssembly";
        static readonly string fullPathName = @"C:\Foo\Bar";

        protected AssemblyBuilder builder;
        static readonly string filename = "StubAssembly.dll";
        protected ModuleBuilder moduleBuilder;

        static AssemblyResultTests()
        {
            // Let the system compute it for non-Windows systems
            fullPathName = Path.GetFullPath(fullPathName);
        }

        public AssemblyResultTests()
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = assembly;

            AppDomain appDomain = Thread.GetDomain();
            builder = appDomain.DefineDynamicAssembly(assemblyName,
                                                      AssemblyBuilderAccess.RunAndSave);
        }

        public void Dispose()
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }

        [Fact]
        public void AssemblyResultCodeBase()
        {
            AssemblyResult assemblyResult = new AssemblyResult(filename);

            Assert.Equal(Path.GetDirectoryName(Path.GetFullPath(filename)), assemblyResult.Directory);
        }

        [Fact]
        public void AssemblyResultName()
        {
            AssemblyResult assemblyResult = new AssemblyResult(filename);

            Assert.Equal(Path.GetFullPath(filename), assemblyResult.Filename);
        }

        [Fact]
        public void AssemblyResultConfigFilename()
        {
            AssemblyResult assemblyResult = new AssemblyResult(filename, fullPathName);

            Assert.Equal(fullPathName, assemblyResult.ConfigFilename);
        }

        [Fact]
        public void ToXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            AssemblyResult assemblyResult = new AssemblyResult(filename, fullPathName);

            XmlNode resultNode = assemblyResult.ToXml(parentNode);

            Assert.Equal("assembly", resultNode.Name);
            Assert.Equal(Path.GetFullPath(filename), resultNode.Attributes["name"].Value);
            Assert.Equal(fullPathName, resultNode.Attributes["configFile"].Value);
            Assert.NotNull(resultNode.Attributes["run-date"]);
            Assert.NotNull(resultNode.Attributes["run-time"]);
            Assert.Equal("0.000", resultNode.Attributes["time"].Value);
            Assert.Equal("0", resultNode.Attributes["total"].Value);
            Assert.Equal("0", resultNode.Attributes["passed"].Value);
            Assert.Equal("0", resultNode.Attributes["failed"].Value);
            Assert.Equal("0", resultNode.Attributes["skipped"].Value);
            Assert.Contains("xUnit.net", resultNode.Attributes["test-framework"].Value);
            string expectedEnvironment = string.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version);
            Assert.Equal(expectedEnvironment, resultNode.Attributes["environment"].Value);
        }

        [Fact]
        public void ToXml_WithChildren()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            PassedResult passedResult = new PassedResult("foo", "bar", null, null);
            passedResult.ExecutionTime = 1.1;
            FailedResult failedResult = new FailedResult("foo", "bar", null, null, "extype", "message", "stack");
            failedResult.ExecutionTime = 2.2;
            SkipResult skipResult = new SkipResult("foo", "bar", null, null, "reason");
            ClassResult classResult = new ClassResult(typeof(object));
            classResult.Add(passedResult);
            classResult.Add(failedResult);
            classResult.Add(skipResult);
            AssemblyResult assemblyResult = new AssemblyResult(filename);
            assemblyResult.Add(classResult);

            XmlNode resultNode = assemblyResult.ToXml(parentNode);

            Assert.Equal("3.300", resultNode.Attributes["time"].Value);
            Assert.Equal("3", resultNode.Attributes["total"].Value);
            Assert.Equal("1", resultNode.Attributes["passed"].Value);
            Assert.Equal("1", resultNode.Attributes["failed"].Value);
            Assert.Equal("1", resultNode.Attributes["skipped"].Value);
            Assert.Single(resultNode.SelectNodes("class"));
        }
    }
}
