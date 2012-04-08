using System;
using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

public class ExcelDataAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void ExcelDataTest()
    {
        if (IntPtr.Size == 8)  // Test always fails in 64-bit; no JET engine
            return;

        MethodResult[] results = RunClass(typeof(ExcelDataClass)).ToArray();

        Assert.Equal(3, results.Length);
        Assert.IsType<PassedResult>(results[0]);
        Assert.Equal(@"ExcelDataAcceptanceTests+ExcelDataClass.PassingTestData(foo: 1, bar: ""Foo"", baz: ""Bar"")", results[0].DisplayName);
        Assert.IsType<PassedResult>(results[1]);
        Assert.Equal(@"ExcelDataAcceptanceTests+ExcelDataClass.PassingTestData(foo: null, bar: null, baz: null)", results[1].DisplayName);
        Assert.IsType<PassedResult>(results[2]);
        Assert.Equal(@"ExcelDataAcceptanceTests+ExcelDataClass.PassingTestData(foo: 14, bar: ""Biff"", baz: ""Baz"")", results[2].DisplayName);
    }

    class ExcelDataClass
    {
        [Theory, ExcelData(@"DataTheories\AcceptanceTests\AcceptanceTestData.xls", "select * from Data")]
        public void PassingTestData(int? foo, string bar, string baz)
        {
        }
    }
}