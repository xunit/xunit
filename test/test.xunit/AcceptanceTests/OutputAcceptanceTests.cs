using System;
using System.Xml;
using TestUtility;
using Xunit;

public class OutputAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void StdOutput()
    {
        string expected = "Line 1 to Standard Output" + Environment.NewLine +
                          "Line 2 to Standard Error" + Environment.NewLine +
                          "Line 3 to Standard Output" + Environment.NewLine;

        string code =
            @"
                using System;
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void SampleTest()
                    {
                        Console.WriteLine(""Line 1 to Standard Output"");
                        Console.Error.WriteLine(""Line 2 to Standard Error"");
                        Console.WriteLine(""Line 3 to Standard Output"");
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode node = ResultXmlUtility.GetResult(assemblyNode);
        Assert.Equal(expected, node.SelectSingleNode("output").InnerText);
    }

    [Fact]
    public void TraceOutput()
    {
        string expected = "Line 1 to Standard Output" + Environment.NewLine +
                          "Line 2 to Trace" + Environment.NewLine +
                          "Line 3 to Standard Output" + Environment.NewLine;

        string code =
            @"
                #define TRACE

                using System;
                using System.Diagnostics;
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void SampleTest()
                    {
                        Console.WriteLine(""Line 1 to Standard Output"");
                        Trace.WriteLine(""Line 2 to Trace"");
                        Console.WriteLine(""Line 3 to Standard Output"");
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode node = ResultXmlUtility.GetResult(assemblyNode);
        Assert.Equal(expected, node.SelectSingleNode("output").InnerText);
    }

    [Fact]
    public void OutputIsPresentEvenIfTestFails()
    {
        string expected = "Line 1 to Standard Output" + Environment.NewLine;

        string code =
            @"
                using System;
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void SampleTest()
                    {
                        Console.WriteLine(""Line 1 to Standard Output"");
                        Assert.False(true);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode node = ResultXmlUtility.GetResult(assemblyNode);
        Assert.Equal(expected, node.SelectSingleNode("output").InnerText);
    }

    [Fact]
    public void TraceAssertDoesNotCreateAnyOutput()
    {
        string code =
           @"
                #define TRACE

                using System;
                using System.Diagnostics;
                using Xunit;

                public class TestClass
                {
                    [Fact]
                    public void SampleTest()
                    {
                        Trace.Assert(false);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode node = ResultXmlUtility.AssertResult(assemblyNode, "Fail", "TestClass.SampleTest");
        Assert.Null(node.SelectSingleNode("output"));
    }
}