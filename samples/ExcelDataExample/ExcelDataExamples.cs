using System;
using Xunit;

public class ExcelDataExamples
{
    static Type[] ParameterTypes = new Type[] { typeof(int), typeof(string), typeof(string) }; 

    [Theory]
    [MemberData("DataSource", 
                "UnitTestData.xls", 
                "select * from [Sheet1$A1:C5]",
                new Type[] { typeof(int), typeof(string), typeof(string) },
                MemberType = typeof(ExcelDataAdapter))]
    public void ExcelXlsTests(int x, string y, string z)
    {
        Assert.NotEqual("Baz", z);
    }
}

