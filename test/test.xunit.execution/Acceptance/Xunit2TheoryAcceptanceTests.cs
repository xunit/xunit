using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void GenericTheoryWithSerializableData()
        {
            var results = Run<ITestPassed>(typeof(GenericWithSerializableData));

            Assert.Collection(results.OrderBy(r => r.TestDisplayName),
                result => Assert.Equal(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<Int32, Object>(value1: 42, value2: null)", result.TestDisplayName),
                result => Assert.Equal(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<Int32[], List<String>>(value1: [1, 2, 3], value2: [""a"", ""b"", ""c""])", result.TestDisplayName),
                result => Assert.Equal(String.Format(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<String, Double>(value1: ""Hello, world!"", value2: {0})", 21.12), result.TestDisplayName)
            );
        }

        public class GenericWithSerializableData
        {
            public static IEnumerable<object[]> GenericData
            {
                get
                {
                    yield return new object[] { 42, null };
                    yield return new object[] { "Hello, world!", 21.12 };
                    yield return new object[] { new int[] { 1, 2, 3 }, new List<string> { "a", "b", "c" } };
                }
            }

            [Theory, MemberData("GenericData")]
            public void GenericTest<T1, T2>(T1 value1, T2 value2) { }
        }

        [Fact]
        public void GenericTheoryWithNonSerializableData()
        {
            var results = Run<ITestPassed>(typeof(GenericWithNonSerializableData));

            Assert.Collection(results,
                result => Assert.Equal(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData.GenericTest<Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData>(value: GenericWithNonSerializableData { })", result.TestDisplayName)
            );
        }

        public class GenericWithNonSerializableData
        {
            public static IEnumerable<object[]> GenericData
            {
                get
                {
                    yield return new object[] { new GenericWithNonSerializableData() };
                }
            }

            [Theory, MemberData("GenericData")]
            public void GenericTest<T>(T value) { }
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

                Assert.Equal(String.Format("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 42, y: {0}, z: \"Hello, world!\")", 21.12), passing.TestDisplayName);
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

    public class MissingDataTests : AcceptanceTest
    {
        [Fact]
        public void MissingDataThrows()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithMissingData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData.TestViaMissingData", failed.TestDisplayName);
                Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
                Assert.Equal("Could not find public static member (property, field, or method) named 'Foo' on Xunit2TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData", failed.Messages.Single());
                return true;
            });
        }

        class ClassWithMissingData
        {
            [Theory]
            [MemberData("Foo")]
            public void TestViaMissingData(int x, double y, string z) { }
        }
    }

    public class FieldDataTests : AcceptanceTest
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfFieldData));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal(String.Format("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithSelfFieldData.TestViaFieldData(x: 42, y: {0}, z: \"Hello, world!\")", 21.12), passing.TestDisplayName);
                return true;
            });
            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithSelfFieldData.TestViaFieldData(x: 0, y: 0, z: null)", failed.TestDisplayName);
                return true;
            });
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithSelfFieldData
        {
            public static IEnumerable<object[]> DataSource = new[] {
                new object[] { 42, 21.12, "Hello, world!" },
                new object[] { 0, 0.0, null }
            };

            [Theory]
            [MemberData("DataSource")]
            public void TestViaFieldData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        [Fact]
        public void CanUseFieldDataFromOtherClass()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithImportedFieldData));

            Assert.Single(testMessages, msg => msg is ITestPassed);
            Assert.Single(testMessages, msg => msg is ITestFailed);
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithImportedFieldData
        {
            [Theory]
            [MemberData("DataSource", MemberType = typeof(ClassWithSelfFieldData))]
            public void TestViaFieldData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        [Fact]
        public void NonStaticFieldDataThrows()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithNonStaticFieldData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithNonStaticFieldData.TestViaFieldData", failed.TestDisplayName);
                Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
                Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithNonStaticFieldData", failed.Messages.Single());
                return true;
            });
        }

        class ClassWithNonStaticFieldData
        {
            public IEnumerable<object[]> DataSource = null;

            [Theory]
            [MemberData("DataSource")]
            public void TestViaFieldData(int x, double y, string z) { }
        }

        [Fact]
        public void CanUseFieldDataFromBaseType()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithBaseClassData));

            Assert.Single(testMessages, msg =>
            {
                var passed = msg as ITestPassed;
                if (passed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithBaseClassData.TestViaFieldData(x: 42)", passed.TestDisplayName);
                return true;
            });
        }

        class BaseClass
        {
            public static IEnumerable<object[]> DataSource = new[] { new object[] { 42 } };
        }

        class ClassWithBaseClassData : BaseClass
        {
            [Theory]
            [MemberData("DataSource")]
            public void TestViaFieldData(int x) { }
        }
    }

    public class MethodDataTests : AcceptanceTest
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfMethodData));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal(String.Format("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithSelfMethodData.TestViaMethodData(x: 42, y: {0}, z: \"Hello, world!\")", 21.12), passing.TestDisplayName);
                return true;
            });
            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithSelfMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failed.TestDisplayName);
                return true;
            });
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithSelfMethodData
        {
            public static IEnumerable<object[]> DataSource()
            {
                return new[] {
                    new object[] { 42, 21.12, "Hello, world!" },
                    new object[] { 0, 0.0, null }
                };
            }

            [Theory]
            [MemberData("DataSource")]
            public void TestViaMethodData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        [Fact]
        public void CanUseMethodDataFromOtherClass()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithImportedMethodData));

            Assert.Single(testMessages, msg => msg is ITestPassed);
            Assert.Single(testMessages, msg => msg is ITestFailed);
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithImportedMethodData
        {
            [Theory]
            [MemberData("DataSource", MemberType = typeof(ClassWithSelfMethodData))]
            public void TestViaMethodData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        [Fact]
        public void NonStaticMethodDataThrows()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithNonStaticMethodData));

            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithNonStaticMethodData.TestViaMethodData", failed.TestDisplayName);
                Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
                Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithNonStaticMethodData", failed.Messages.Single());
                return true;
            });
        }

        class ClassWithNonStaticMethodData
        {
            public IEnumerable<object[]> DataSource() { return null; }

            [Theory]
            [MemberData("DataSource")]
            public void TestViaMethodData(int x, double y, string z) { }
        }

        [Fact]
        public void CanUseMethodDataFromBaseType()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithBaseClassData));

            Assert.Single(testMessages, msg =>
            {
                var passed = msg as ITestPassed;
                if (passed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithBaseClassData.TestViaMethodData(x: 42)", passed.TestDisplayName);
                return true;
            });
        }

        class BaseClass
        {
            public static IEnumerable<object[]> DataSource()
            {
                return new[] { new object[] { 42 } };
            }
        }

        class ClassWithBaseClassData : BaseClass
        {
            [Theory]
            [MemberData("DataSource")]
            public void TestViaMethodData(int x) { }
        }

        [Fact]
        public void CanPassParametersToDataMethod()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithParameterizedMethodData));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal(String.Format("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 42, y: {0}, z: \"Hello, world!\")", 21.12), passing.TestDisplayName);
                return true;
            });
            Assert.Single(testMessages, msg =>
            {
                var failed = msg as ITestFailed;
                if (failed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failed.TestDisplayName);
                return true;
            });
            Assert.None(testMessages, msg => msg is ITestSkipped);
        }

        class ClassWithParameterizedMethodData
        {
            public static IEnumerable<object[]> DataSource(int x)
            {
                return new[] {
                    new object[] { x / 2, 21.12, "Hello, world!" },
                    new object[] { 0, 0.0, null }
                };
            }

            [Theory]
            [MemberData("DataSource", 84)]
            public void TestViaMethodData(int x, double y, string z)
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
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfPropertyData));

            Assert.Single(testMessages, msg =>
            {
                var passing = msg as ITestPassed;
                if (passing == null)
                    return false;

                Assert.Equal(String.Format("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 42, y: {0}, z: \"Hello, world!\")", 21.12), passing.TestDisplayName);
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
            [MemberData("DataSource")]
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
            [MemberData("DataSource", MemberType = typeof(ClassWithSelfPropertyData))]
            public void TestViaPropertyData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
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
                Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
                Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData", failed.Messages.Single());
                return true;
            });
        }

        class ClassWithNonStaticPropertyData
        {
            public IEnumerable<object[]> DataSource { get { return null; } }

            [Theory]
            [MemberData("DataSource")]
            public void TestViaPropertyData(int x, double y, string z) { }
        }

        [Fact]
        public void CanUsePropertyDataFromBaseType()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithBaseClassData));

            Assert.Single(testMessages, msg =>
            {
                var passed = msg as ITestPassed;
                if (passed == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithBaseClassData.TestViaPropertyData(x: 42)", passed.TestDisplayName);
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
            [MemberData("DataSource")]
            public void TestViaPropertyData(int x) { }
        }
    }

    public class ErrorAggregation : AcceptanceTest
    {
        [Fact]
        public void EachTheoryHasIndividualExceptionMessage()
        {
            var testMessages = Run<ITestFailed>(typeof(ClassUnderTest));

            var equalFailure = Assert.Single(testMessages, msg => msg.TestDisplayName == String.Format("Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 42, y: {0}, z: ClassUnderTest {{ }})", 21.12));
            Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

            var notNullFailure = Assert.Single(testMessages, msg => msg.TestDisplayName == "Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)");
            Assert.Contains("Assert.NotNull() Failure", notNullFailure.Messages.Single());
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
            [MemberData("Data")]
            public void TestViaInlineData(int x, double y, object z)
            {
                Assert.Equal(0, x); // Fails the first data item
                Assert.NotNull(z);  // Fails the second data item
            }
        }
    }

    public class OverloadedMethodTests : AcceptanceTest
    {
        [Fact]
        public void TestMethodMessagesOnlySentOnce()
        {
            var testMessages = Run<IMessageSinkMessage>(typeof(ClassUnderTest));

            Assert.Single(testMessages, msg =>
            {
                var methodStarting = msg as ITestMethodStarting;
                if (methodStarting == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+OverloadedMethodTests+ClassUnderTest", methodStarting.TestClass.Class.Name);
                Assert.Equal("Theory", methodStarting.TestMethod.Method.Name);
                return true;
            });

            Assert.Single(testMessages, msg =>
            {
                var methodFinished = msg as ITestMethodFinished;
                if (methodFinished == null)
                    return false;

                Assert.Equal("Xunit2TheoryAcceptanceTests+OverloadedMethodTests+ClassUnderTest", methodFinished.TestClass.Class.Name);
                Assert.Equal("Theory", methodFinished.TestMethod.Method.Name);
                return true;
            });
        }

        class ClassUnderTest
        {
            [Theory]
            [InlineData(42)]
            public void Theory(int value)
            {
            }

            [Theory]
            [InlineData("42")]
            public void Theory(string value)
            {
            }
        }
    }
}