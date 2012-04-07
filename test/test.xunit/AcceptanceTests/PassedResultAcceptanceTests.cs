using System.Xml;
using TestUtility;
using Xunit;

public class PassedResultAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void AssertAreEqualTwoNumbersEqualShouldBePassedResult()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void SuccessTest()
                    {
                        Assert.Equal(2, 2);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "MockTestClass.SuccessTest");
    }
}