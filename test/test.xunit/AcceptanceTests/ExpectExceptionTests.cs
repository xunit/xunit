using System.Xml;
using TestUtility;
using Xunit;

public class ExpectExceptionTests : AcceptanceTest
{
    [Fact]
    public void ThrowsExpectedException()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void ExpectTest()
                    {
                        Assert.Throws<InvalidOperationException>(delegate { throw new InvalidOperationException(); });
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "MockTestClass.ExpectTest");
    }

    [Fact]
    public void ThrowsWrongException()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void ExpectTestFails()
                    {
                        Assert.Throws<ArgumentException>(delegate { throw new InvalidOperationException(); });
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Fail", "MockTestClass.ExpectTestFails");
    }
}