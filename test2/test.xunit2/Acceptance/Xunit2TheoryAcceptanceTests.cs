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
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run(typeof(ClassWithSelfPropertyData));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 42, y: 21.12, z: \"Hello, world!\")", passing.TestDisplayName);
                return true;
            });
            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 0, y: 0, z: null)", failed.TestDisplayName);
                return true;
            });
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithSelfPropertyData
        {
            public static IEnumerable<object[]> DataSource
            {
                get
                {
                    yield return new object[] { 42, 21.12, "Hello, world!" };
                    yield return new object[] { 0, 0.0, null };
                }
            }

            [Theory]
            [PropertyData("DataSource")]
            public void TestViaPropertyData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        [Fact]
        public void CanUsePropertyDataFromOtherClass()
        {
            var testMessages = Run(typeof(ClassWithImportedPropertyData));

            Assert.Single(testMessages, msg => msg is ITestPassed);
            Assert.Single(testMessages, msg => msg is ITestFailed);
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithImportedPropertyData
        {
            [Theory]
            [PropertyData("DataSource", PropertyType = typeof(ClassWithSelfPropertyData))]
            public void TestViaPropertyData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        [Fact]
        public void MissingPropertyDataThrows()
        {
            var testMessages = Run(typeof(ClassWithMissingPropertyData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithMissingPropertyData.TestViaPropertyData", failed.TestDisplayName);
                Assert.Contains("An exception was thrown while getting data for theory Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithMissingPropertyData.TestViaPropertyData: System.ArgumentException: Could not find public static property Foo on Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithMissingPropertyData", failed.Message);
                return true;
            });
        }

        class ClassWithMissingPropertyData
        {
            [Theory]
            [PropertyData("Foo")]
            public void TestViaPropertyData(int x, double y, string z) { }
        }

        [Fact]
        public void NonStaticPropertyDataThrows()
        {
            var testMessages = Run(typeof(ClassWithNonStaticPropertyData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData.TestViaPropertyData", failed.TestDisplayName);
                Assert.Contains("An exception was thrown while getting data for theory Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData.TestViaPropertyData: System.ArgumentException: Could not find public static property DataSource on Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData", failed.Message);
                return true;
            });
        }

        class ClassWithNonStaticPropertyData
        {
            public IEnumerable<object[]> DataSource { get { return null; } }

            [Theory]
            [PropertyData("DataSource")]
            public void TestViaPropertyData(int x, double y, string z) { }
        }

        [Fact]
        public void CanUsePropertyDataFromBaseType()
        {
            var testMessages = Run(typeof(ClassWithBaseClassData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestPassed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithBaseClassData.TestViaPropertyData(x: 42)", failed.TestDisplayName);
                return true;
            });
        }

        class BaseClass
        {
            public static IEnumerable<object[]> DataSource { get { yield return new object[] { 42 }; } }
        }

        class ClassWithBaseClassData : BaseClass
        {
            [Theory]
            [PropertyData("DataSource")]
            public void TestViaPropertyData(int x) { }
        }
    }
}