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
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTest));

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
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTest));

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

        [Fact]
        public void SingleNullValuesWork()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTestForNullValues));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForNullValues.TestMethod(value: null)", passing.TestDisplayName);
                return true;
            });
        }

        class ClassUnderTestForNullValues
        {
            [Theory]
            [InlineData(null)]
            public void TestMethod(string value) { }
        }

        [Fact]
        public void ArraysWork()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTestForArrays));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Contains("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForArrays.TestMethod", passing.TestDisplayName);
                return true;
            });
        }

        class ClassUnderTestForArrays
        {
            [Theory]
            [InlineData(new[] { 42, 2112 }, new[] { "SELF", "PARENT1", "PARENT2", "PARENT3" }, null, 10.5, "Hello, world!")]
            public void TestMethod(int[] v1, string[] v2, float[] v3, double v4, string v5) { }
        }
    }

    public class PropertyDataTests : AcceptanceTest
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfPropertyData));

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
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithImportedPropertyData));

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
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithMissingPropertyData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithMissingPropertyData.TestViaPropertyData", failed.TestDisplayName);
                Assert.Equal("System.ArgumentException : Could not find public static property Foo on Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithMissingPropertyData", failed.Message);
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
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithNonStaticPropertyData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData.TestViaPropertyData", failed.TestDisplayName);
                Assert.Equal("System.ArgumentException : Could not find public static property DataSource on Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData", failed.Message);
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
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithBaseClassData));

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

    public class ErrorAggregation : AcceptanceTest
    {
        [Fact]
        public void EachTheoryHasIndividualExceptionMessage()
        {
            var testMessages = Run<ITestFailed>(typeof(ClassUnderTest));

            var equalFailure = Assert.Single(testMessages, msg => msg.TestDisplayName == "Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 42, y: 21.12, z: Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest)");
            Assert.Contains("Assert.Equal() Failure", equalFailure.Message);

            var notNullFailure = Assert.Single(testMessages, msg => msg.TestDisplayName == "Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)");
            Assert.Contains("Assert.NotNull() Failure", notNullFailure.Message);
        }

        class ClassUnderTest
        {
            public static IEnumerable<object[]> Data
            {
                get
                {
                    yield return new object[] { 42, 21.12, new ClassUnderTest() };
                    yield return new object[] { 0, 0.0, null };
                }
            }

            [Theory]
            [PropertyData("Data")]
            public void TestViaInlineData(int x, double y, object z)
            {
                Assert.Equal(0, x); // Fails the first data item
                Assert.NotNull(z);  // Fails the second data item
            }
        }
    }
}