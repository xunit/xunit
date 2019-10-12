using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.ConsoleClient;

public class CommandLineOptionsFileReaderTests
{
    public class Filename
    {
        [Fact]
        public static void OptionsFileNameNotPresentThrows()
        {
            var exception = Record.Exception(() => TestableCommandLineOptionsFile.Read(null));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("missing options file name", exception.Message);
        }

        //    [Fact]
        //    public static void OptionsFileNamePresentDoesNotThrow()
        //    {
        //        var fileName = "someFileName";

        //        TestableCommandLineOptionsFile.Read(fileName);  // Should not throw
        //    }

        //[Fact]
        //public static void OptionsFileDoesNotExist()
        //{
        //    var arguments = "notExistingFile";

        //    var exception = Record.Exception(() => TestableCommandLineOptionsFile.Read(arguments));

        //    Assert.IsType<ArgumentException>(exception);
        //    Assert.Equal($"file not found: {arguments}", exception.Message);
        //}
    }

    public class FileContents
    {
        [Fact]
        public static void OptionsFileIsParsedCorrectly()
        {
            var fileContents = @"test\test.xunit.console\bin\Release\net472\test.xunit.console.dll " + (char)10 + @"test\test.xunit.console\bin\Release\net472\test.xunit.console.dll " + (char)13 + @"-xml artifacts/test/v2.xml -html " + (char)34 + @"artifa  cts/test/v 2.html" + (char)34 + @" -appdomains denied     -serialize -parallel all -maxthreads 16";

            var expectedOtptions = new Stack<string>();
            expectedOtptions.Push(@"test\test.xunit.console\bin\Release\net472\test.xunit.console.dll");
            expectedOtptions.Push(@"test\test.xunit.console\bin\Release\net472\test.xunit.console.dll");
            expectedOtptions.Push(@"-xml");
            expectedOtptions.Push(@"artifacts/test/v2.xml");
            expectedOtptions.Push(@"-html");
            expectedOtptions.Push(@"artifa  cts/test/v 2.html");
            expectedOtptions.Push(@"-appdomains");
            expectedOtptions.Push(@"denied");
            expectedOtptions.Push(@"-serialize");
            expectedOtptions.Push(@"-parallel");
            expectedOtptions.Push(@"all");
            expectedOtptions.Push(@"-maxthreads");
            expectedOtptions.Push(@"16");

            int expectedPops = expectedOtptions.Count;

            var commandLineOptions = TestableCommandLineOptionsFile.Read("fileName", fileContents).Options;

            Assert.Equal(expectedOtptions.Count, commandLineOptions.Count);

            int actualPops = 0;
            while (expectedOtptions.Count > 0)
            {
                Assert.Equal(expectedOtptions.Pop(), commandLineOptions.Pop());
                actualPops++;
            }

            Assert.Equal(expectedPops, actualPops);
        }
    }

    class TestableCommandLineOptionsFile : CommandLineOptionsFile
    {
        private TestableCommandLineOptionsFile(string optionsFileName, StreamReader streamReader)
            : base(optionsFileName, streamReader, filename => !filename.StartsWith("notExistingFile")) { }

        public static new TestableCommandLineOptionsFile Read(string optionsFile)
            => new TestableCommandLineOptionsFile(optionsFile, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty))));

        public static new TestableCommandLineOptionsFile Read(string optionsFile, string fileContents)
            => new TestableCommandLineOptionsFile(optionsFile, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContents ?? string.Empty))));
    }
}
