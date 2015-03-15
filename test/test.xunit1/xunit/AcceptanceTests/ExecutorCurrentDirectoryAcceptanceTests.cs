using System.IO;
using System.Reflection;
using System.Xml;
using TestUtility;
using Xunit;

namespace Xunit1
{
    public class ExecutorCurrentDirectoryAcceptanceTests : AcceptanceTestInNewAppDomain
    {
        [Fact]
        public void CurrentDirectoryWhenRunningTestsIsTestAssemblyPath()
        {
            string code = @"
            using System.IO;
            using Xunit;

            public class ChangeDirectoryTests
            {
                [Fact]
                public void ChangeDirectory()
                {
                    string tempPath = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
                    string currentPath = Path.GetFullPath(Directory.GetCurrentDirectory()).TrimEnd(Path.DirectorySeparatorChar);
                    Assert.Equal(tempPath, currentPath);
                }
            }
        ";

            string assemblyName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            XmlNode assemblyNode = ExecuteWithCustomAssemblyName(code, assemblyName);

            ResultXmlUtility.AssertResult(assemblyNode, "Pass", "ChangeDirectoryTests.ChangeDirectory");
        }

        [Fact]
        [PreserveWorkingDirectory]
        public void CurrentDirectoryIsRestoredAfterExecution()
        {
            string code = @"
            using System.IO;
            using Xunit;

            public class ChangeDirectoryTests
            {
                [Fact]
                public void ChangeDirectory()
                {
                    Directory.SetCurrentDirectory(Path.GetTempPath());
                }
            }
        ";

            string directory = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            Directory.SetCurrentDirectory(directory);
            string newDirectory = Directory.GetCurrentDirectory();

            Execute(code);

            Assert.Equal(newDirectory, Directory.GetCurrentDirectory());
        }

        class PreserveWorkingDirectoryAttribute : BeforeAfterTestAttribute
        {
            string workingDirectory;

            public override void Before(MethodInfo methodUnderTest)
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            public override void After(MethodInfo methodUnderTest)
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }
        }
    }
}
