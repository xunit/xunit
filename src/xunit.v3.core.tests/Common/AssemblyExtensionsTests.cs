using System;
using Xunit;

public class AssemblyExtensionsTests
{
    public class GetLocalCodeBase
    {
        [Fact]
        public void NullAssembly_ReturnsNull()
        {
            var result = AssemblyExtensions.GetLocalCodeBase(null);

            Assert.Null(result);
        }

        [Fact]
        public void NullCodeBase_ReturnsNull()
        {
            var result = AssemblyExtensions.GetLocalCodeBase(null, '/');

            Assert.Null(result);
        }

        [Fact]
        public void UnsupportedCodeBaseFormat_Throws()
        {
            var ex = Record.Exception(() => AssemblyExtensions.GetLocalCodeBase("http://host/path", '/'));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("codeBase", argEx.ParamName);
            Assert.StartsWith("Codebase 'http://host/path' is unsupported; must start with 'file://'.", argEx.Message);
        }

        [Fact]
        public void UnsupportedDirectorySeparator_Throws()
        {
            var ex = Record.Exception(() => AssemblyExtensions.GetLocalCodeBase("file:///path", 'a'));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("directorySeparator", argEx.ParamName);
            Assert.StartsWith("Unknown directory separator 'a'; must be one of '/' or '\\'.", argEx.Message);
        }

        [Fact]
        public void PosixSystem_LocalPath()
        {
            var result = AssemblyExtensions.GetLocalCodeBase("file:///path/to/file.dll", '/');

            Assert.Equal("/path/to/file.dll", result);
        }

        [Fact]
        public void PosixSystem_UNCPath_Throws()
        {
            var ex = Record.Exception(() => AssemblyExtensions.GetLocalCodeBase("file://server/path", '/'));

            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("codeBase", argEx.ParamName);
            Assert.StartsWith("UNC-style codebase 'file://server/path' is not supported on POSIX-style file systems.", argEx.Message);
        }

        [Fact]
        public void WindowsSystem_LocalPath()
        {
            var result = AssemblyExtensions.GetLocalCodeBase("file:///C:/path/to/file.dll", '\\');

            Assert.Equal(@"C:\path\to\file.dll", result);
        }

        [Fact]
        public void WindowsSystem_UNCPath()
        {
            var result = AssemblyExtensions.GetLocalCodeBase("file://server/path/to/file.dll", '\\');

            Assert.Equal(@"\\server\path\to\file.dll", result);
        }
    }
}
