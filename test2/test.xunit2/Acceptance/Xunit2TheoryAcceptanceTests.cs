using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

public class Xunit2TheoryAcceptanceTests
{
    public class TheoryTests : AcceptanceTest
    {
        [Fact]
        public void Skipped()
        {
            var testMessages = Run(typeof(ClassUnderTest));

            Assert.None(testMessages, msg => msg is ITestPassed);
            Assert.None(testMessages, msg => msg is ITestFailed);
            Assert.Single(testMessages, msg =>
            {
                var skipped = msg as ITestSkipped;
                if (skipped == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+TheoryTests+ClassUnderTest.TestViaInlineData", skipped.TestDisplayName);
                Assert.Equal("Don't run this!", skipped.Reason);
                return true;
            });
        }

        class ClassUnderTest
        {
            [Theory(Skip = "Don't run this!")]
            [InlineData(42, 21.12, "Hello, world!")]
            [InlineData(0, 0.0, null)]
            public void TestViaInlineData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }
    }

    public class InlineDataTests : AcceptanceTest
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run(typeof(ClassUnderTest));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 42, y: 21.12, z: \"Hello, world!\")", passing.TestDisplayName);
                return true;
            });
            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)", failed.TestDisplayName);
                return true;
            });
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassUnderTest
        {
            [Theory]
            [InlineData(42, 21.12, "Hello, world!")]
            [InlineData(0, 0.0, null)]
            public void TestViaInlineData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }
    }

    public class PropertyDataTests : AcceptanceTest
    {
        [Fact(Skip = "Working towards this...")]
        public void RunsForEachDataElement()
        {
            var testMessages = Run(typeof(ClassUnderTest));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassUnderTest.TestViaPropertyData(x: 42, y: 21.12, z: \"Hello, world!\")", passing.TestDisplayName);
                return true;
            });
            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassUnderTest.TestViaPropertyData(x: 0, y: 0, z: null)", failed.TestDisplayName);
                return true;
            });
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        public static IEnumerable<object[]> DataSource
        {
            get
            {
                yield return new object[] { 42, 21.12, "Hello, world!" };
                yield return new object[] { 0, 0.0, null };
            }
        }

        class ClassUnderTest
        {
            [Theory]
            //[PropertyData("DataSource")]
            public void TestViaPropertyData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }
    }

    //internal class TestMethodCommandClassBaseClass
    //{
    //    public static IEnumerable<object[]> BasePropertyData
    //    {
    //        get { yield return new object[] { 4 }; }
    //    }
    //}

    //internal class TestMethodCommandClass : TestMethodCommandClassBaseClass
    //{
    //    public static IEnumerable<object[]> GenericData
    //    {
    //        get
    //        {
    //            yield return new object[] { 42 };
    //            yield return new object[] { "Hello, world!" };
    //            yield return new object[] { new int[] { 1, 2, 3 } };
    //            yield return new object[] { new List<string> { "a", "b", "c" } };
    //        }
    //    }

    //    public static IEnumerable<object[]> TheoryDataProperty
    //    {
    //        get { yield return new object[] { 2 }; }
    //    }

    //    //[Theory, OleDbData(
    //    //    @"Provider=Microsoft.Jet.OleDb.4.0; Data Source=DataTheories\UnitTestData.xls; Extended Properties=Excel 8.0",
    //    //    "SELECT x, y, z FROM Data")]
    //    //public void TestViaOleDb(double x, string y, string z) { }

    //    //[Theory, PropertyData("TheoryDataProperty")]
    //    //public void TestViaProperty(int x) { }

    //    //[Theory, PropertyData("BasePropertyData")]
    //    //public void TestViaPropertyOnBaseClass(int x) { }

    //    //[Theory, PropertyData("TheoryDataProperty", PropertyType = typeof(ExternalData))]
    //    //public void TestViaOtherTypeProperty(int x) { }

    //    //[Theory, ExcelData(@"DataTheories\UnitTestData.xls", "SELECT x, y, z FROM Data")]
    //    //public void TestViaXls(double x, string y, string z) { }

    //    //[Theory, PropertyData("GenericData")]
    //    //public void GenericTest<T>(T value) { }

    //    class ExternalData
    //    {
    //        public static IEnumerable<object[]> TheoryDataProperty
    //        {
    //            get { yield return new object[] { 3 }; }
    //        }
    //    }
    //}
}