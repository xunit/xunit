using System.Xml;
using TestUtility;
using Xunit;
using Xunit.Sdk;

public class FailureTests : AcceptanceTest
{
    [Fact]
    public void AssertAreEqualTwoNumbersNotEqualShouldThrowException()
    {
        string code = @"
            using System;
            using Xunit;

            public class MockTestClass
            {
                [Fact]
                public void TwoNumbersAreNotEqual()
                {
                    Assert.Equal(2, 3);
                }
            }
        ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Fail", "MockTestClass.TwoNumbersAreNotEqual");
        XmlNode failureNode = testNode.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(EqualException).FullName);
    }

    [Fact]
    public void TimingForFailedTestShouldReflectActualRunTime()
    {
        string code = @"
            using System;
            using Xunit;

            public class MockTestClass
            {
                [Fact]
                public void TwoNumbersAreNotEqual()
                {
                    System.Threading.Thread.Sleep(100);
                    Assert.Equal(2, 3);
                }
            }
        ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Fail", "MockTestClass.TwoNumbersAreNotEqual");
        Assert.NotEqual("0.000", testNode.Attributes["time"].Value);
    }
}