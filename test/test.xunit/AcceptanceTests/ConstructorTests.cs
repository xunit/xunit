using System.Xml;
using TestUtility;
using Xunit;

public class ConstructorTests : AcceptanceTest
{
    [Fact]
    public void VerifiesConstructorIsCalled()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    int counter; 

                    public MockTestClass()
                    {
                        counter++;
                    }

                    [Fact]
                    public void CounterShouldBeIncrementedInConstructor()
                    {
                        Assert.Equal(counter, 1);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "MockTestClass.CounterShouldBeIncrementedInConstructor");
    }
}