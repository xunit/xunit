using System;
using System.Xml;
using TestUtility;
using Xunit;

public class TheoryAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void TheoryViaDataAcceptanceTest()
    {
        string code = @"
                using Xunit.Extensions;

                public class Stub
                {
                    [Theory]
                    [InlineData(1, ""hello"", 2.3)]
                    [InlineData(42, ""world"", 21.12)]
                    public void PassingTestData(int foo, string bar, double baz)
                    {
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode1 = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode1, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode1, "name", "Stub." + @"PassingTestData(foo: 1, bar: ""hello"", baz: 2.3)");

        XmlNode testNode2 = ResultXmlUtility.GetResult(assemblyNode, 0, 1);
        ResultXmlUtility.AssertAttribute(testNode2, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode2, "name", "Stub." + @"PassingTestData(foo: 42, bar: ""world"", baz: 21.12)");
    }

    [Fact]
    public void TheoryWithInlineDataOfSingleNullPassesNullToTestMethod()
    {
        string code = @"
                using Xunit;
                using Xunit.Extensions;

                public class Stub
                {
                    [Theory]
                    [InlineData(null)]
                    public void PassingTestData(string foo)
                    {
                        Assert.Null(foo);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode, "name", "Stub.PassingTestData(foo: null)");
    }

    [Fact]
    public void TheoryViaPropertyAcceptanceTest()
    {
        string code = @"
                using System.Collections.Generic;
                using Xunit.Extensions;

                public class Stub
                {
                    public static IEnumerable<object[]> MyTestData
                    {
                        get { yield return new object[] { 1, ""hello world"", 2.3 }; }
                    }

                    [Theory, PropertyData(""MyTestData"")]
                    public void PassingTestData(int foo, string bar, double baz)
                    {
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "Stub." + @"PassingTestData(foo: 1, bar: ""hello world"", baz: 2.3)");
    }

    [Fact]
    public void TheoryViaClassAcceptanceTest()
    {
        string code = @"
                using System.Collections;
                using System.Collections.Generic;
                using Xunit.Extensions;

                public class StubData : IEnumerable<object[]>
                {
                    public IEnumerator<object[]> GetEnumerator()
                    {
                        yield return new object[] { 1, ""hello world"", 2.3 };
                    }

                    IEnumerator IEnumerable.GetEnumerator()
                    {
                        return GetEnumerator();
                    }
                }

                public class Stub
                {
                    [Theory, ClassData(typeof(StubData))]
                    public void PassingTestData(int foo, string bar, double baz)
                    {
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "Stub." + @"PassingTestData(foo: 1, bar: ""hello world"", baz: 2.3)");
    }

    [Fact]
    public void TheoryViaXlsAcceptanceTest()
    {
        if (IntPtr.Size == 8)  // Test always fails in 64-bit; no JET engine
            return;

        string code = @"
                using System.Data;
                using Xunit.Extensions;

                public class Stub
                {
                    [Theory, ExcelData(@""DataTheories\AcceptanceTestData.xls"", ""select * from Data"")]
                    public void PassingTestData(int? foo, string bar, string baz)
                    {
                    }
                }
            ";


        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode1 = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode1, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode1, "name", "Stub." + @"PassingTestData(foo: 1, bar: ""Foo"", baz: ""Bar"")");

        XmlNode testNode2 = ResultXmlUtility.GetResult(assemblyNode, 0, 1);
        ResultXmlUtility.AssertAttribute(testNode2, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode2, "name", "Stub." + @"PassingTestData(foo: null, bar: null, baz: null)");

        XmlNode testNode3 = ResultXmlUtility.GetResult(assemblyNode, 0, 2);
        ResultXmlUtility.AssertAttribute(testNode3, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode3, "name", "Stub." + @"PassingTestData(foo: 14, bar: ""Biff"", baz: ""Baz"")");
    }

    [Fact]
    public void IncorrectParameterCountThrows()
    {
        string code = @"
                using Xunit.Extensions;

                public class Stub
                {
                    [Theory]
                    [InlineData(1)]
                    [InlineData(2, 3)]
                    [InlineData(4)]
                    public void PassingTestData(int x)
                    {
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode1 = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode1, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode1, "name", "Stub." + @"PassingTestData(x: 1)");

        XmlNode testNode2 = ResultXmlUtility.GetResult(assemblyNode, 0, 1);
        ResultXmlUtility.AssertAttribute(testNode2, "result", "Fail");
        ResultXmlUtility.AssertAttribute(testNode2, "name", "Stub." + @"PassingTestData(x: 2, ???: 3)");
        XmlNode failureNode = testNode2.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(InvalidOperationException).FullName);

        XmlNode testNode3 = ResultXmlUtility.GetResult(assemblyNode, 0, 2);
        ResultXmlUtility.AssertAttribute(testNode3, "result", "Pass");
        ResultXmlUtility.AssertAttribute(testNode3, "name", "Stub." + @"PassingTestData(x: 4)");
    }

    [Fact]
    public void TheoryWithNoDataAttributes()
    {
        string code = @"
                using Xunit.Extensions;

                public class Stub
                {
                    [Theory]
                    public void TheoryMethod(int x)
                    {
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode, "result", "Fail");
        ResultXmlUtility.AssertAttribute(testNode, "name", "Stub.TheoryMethod");
        XmlNode failureNode = testNode.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(InvalidOperationException).FullName);
        XmlNode messageNode = failureNode.SelectSingleNode("message");
        Assert.Equal("System.InvalidOperationException : No data found for Stub.TheoryMethod", messageNode.InnerText);
    }

    [Fact]
    public void TheoryWithDataAttributesWithNoData()
    {
        string code = @"
            using System;
            using System.Collections.Generic;
            using System.Reflection;
            using Xunit.Extensions;

            public class EmptyTheoryData : DataAttribute
            {
                public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
                {
                    return new object[0][];
                }
            }

            public class Stub
            {
                [Theory]
                [EmptyTheoryData]
                public void TheoryMethod(int x)
                {
                }
            }
        ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode, "result", "Fail");
        ResultXmlUtility.AssertAttribute(testNode, "name", "Stub.TheoryMethod");
        XmlNode failureNode = testNode.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(InvalidOperationException).FullName);
        XmlNode messageNode = failureNode.SelectSingleNode("message");
        Assert.Equal("System.InvalidOperationException : No data found for Stub.TheoryMethod", messageNode.InnerText);
    }

    [Fact]
    public void ThrowingDataAttributeAcceptanceTest()
    {
        string code = @"
            using System;
            using System.Collections.Generic;
            using System.Reflection;
            using Xunit;
            using Xunit.Extensions;

            public class MisbehavingTestClass
            {
                [Theory, MisbehavedData]
                public void TheoryWithMisbehavingData(string a)
                {
                    Assert.True(true);
                }
            }

            public class MisbehavedDataAttribute : DataAttribute
            {
                public override IEnumerable<object[]> GetData(MethodInfo method, Type[] paramTypes)
                {
                    throw new Exception();
                }
            }
        ";

        XmlNode assemblyNode = Execute(code, null, "xunit.extensions.dll");

        XmlNode testNode = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
        ResultXmlUtility.AssertAttribute(testNode, "result", "Fail");
        ResultXmlUtility.AssertAttribute(testNode, "name", "MisbehavingTestClass.TheoryWithMisbehavingData");
        XmlNode failureNode = testNode.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(InvalidOperationException).FullName);
        XmlNode messageNode = failureNode.SelectSingleNode("message");
        Assert.Contains("System.InvalidOperationException : An exception was thrown while getting data for theory MisbehavingTestClass.TheoryWithMisbehavingData:", messageNode.InnerText);
    }
}