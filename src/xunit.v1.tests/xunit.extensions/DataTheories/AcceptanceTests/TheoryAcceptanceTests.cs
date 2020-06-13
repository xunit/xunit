using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestUtility;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Xunit1.Extensions
{
    public class TheoryAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void IncorrectParameterCount()
        {
            MethodResult[] results = RunClass(typeof(IncorrectParameterCountClass)).ToArray();

            Assert.Equal(2, results.Length);
            PassedResult passedResult = results.OfType<PassedResult>().Single();
            Assert.Equal(@"Xunit1.Extensions.TheoryAcceptanceTests+IncorrectParameterCountClass.PassingTestData(x: 1)", passedResult.DisplayName);
            FailedResult failedResult = results.OfType<FailedResult>().Single();
            Assert.Equal(@"Xunit1.Extensions.TheoryAcceptanceTests+IncorrectParameterCountClass.PassingTestData(x: 2, ???: 3)", failedResult.DisplayName);
        }

        class IncorrectParameterCountClass
        {
            [Theory]
            [InlineData(1)]
            [InlineData(2, 3)]
            public void PassingTestData(int x)
            {
            }
        }

        [Fact]
        public void NoDataAttributes()
        {
            MethodResult result = RunClass(typeof(NoDataAttributesClass)).Single();

            FailedResult failedResult = Assert.IsType<FailedResult>(result);
            Assert.Equal(@"Xunit1.Extensions.TheoryAcceptanceTests+NoDataAttributesClass.TheoryMethod", failedResult.DisplayName);
            Assert.Equal("System.InvalidOperationException : No data found for Xunit1.Extensions.TheoryAcceptanceTests+NoDataAttributesClass.TheoryMethod", failedResult.Message);
        }

        class NoDataAttributesClass
        {
            [Theory]
            public void TheoryMethod(int x) { }
        }

        [Fact]
        public void EmptyTheoryData()
        {
            MethodResult result = RunClass(typeof(EmptyTheoryDataClass)).Single();

            FailedResult failedResult = Assert.IsType<FailedResult>(result);
            Assert.Equal(@"Xunit1.Extensions.TheoryAcceptanceTests+EmptyTheoryDataClass.TheoryMethod", failedResult.DisplayName);
            Assert.Equal("System.InvalidOperationException : No data found for Xunit1.Extensions.TheoryAcceptanceTests+EmptyTheoryDataClass.TheoryMethod", failedResult.Message);
        }

        class EmptyTheoryDataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                return new object[0][];
            }
        }

        class EmptyTheoryDataClass
        {
            [Theory, EmptyTheoryData]
            public void TheoryMethod(int x) { }
        }

        [Fact]
        public void ThrowingData()
        {
            MethodResult result = RunClass(typeof(ThrowingDataClass)).Single();

            FailedResult failedResult = Assert.IsType<FailedResult>(result);
            Assert.Equal(@"Xunit1.Extensions.TheoryAcceptanceTests+ThrowingDataClass.TheoryWithMisbehavingData", failedResult.DisplayName);
            Assert.Contains("System.InvalidOperationException : An exception was thrown while getting data for theory Xunit1.Extensions.TheoryAcceptanceTests+ThrowingDataClass.TheoryWithMisbehavingData", failedResult.Message);
        }

        public class ThrowingDataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo method, Type[] paramTypes)
            {
                throw new Exception();
            }
        }

        class ThrowingDataClass
        {
            [Theory, ThrowingData]
            public void TheoryWithMisbehavingData(string a) { }
        }
    }
}
