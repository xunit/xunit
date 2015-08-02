using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class Xunit2TheoryAcceptanceTests
{
    public class TheoryTests : AcceptanceTestV2
    {
        [Fact]
        public void Skipped()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTest));

            var skipped = Assert.Single(testMessages.Cast<ITestSkipped>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+TheoryTests+ClassUnderTest.TestViaInlineData", skipped.Test.DisplayName);
            Assert.Equal("Don't run this!", skipped.Reason);
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
            var results = Run<ITestResultMessage>(typeof(GenericWithSerializableData));

            Assert.Collection(results.Cast<ITestPassed>().OrderBy(r => r.Test.DisplayName),
                result => Assert.Equal(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<Int32, Object>(value1: 42, value2: null)", result.Test.DisplayName),
                result => Assert.Equal(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<Int32[], List<String>>(value1: [1, 2, 3], value2: [""a"", ""b"", ""c""])", result.Test.DisplayName),
                result => Assert.Equal($@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<String, Double>(value1: ""Hello, world!"", value2: {21.12})", result.Test.DisplayName)
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
            var results = Run<ITestResultMessage>(typeof(GenericWithNonSerializableData));

            Assert.Collection(results.Cast<ITestPassed>(),
                result => Assert.Equal(@"Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData.GenericTest<Xunit2TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData>(value: GenericWithNonSerializableData { })", result.Test.DisplayName)
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

    public class InlineDataTests : AcceptanceTestV2
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTest));

            var passing = Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Equal($"Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 42, y: {21.12}, z: \"Hello, world!\")", passing.Test.DisplayName);
            var failed = Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)", failed.Test.DisplayName);
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            var passing = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForNullValues.TestMethod(value: null)", passing.Test.DisplayName);
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

            var passing = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Contains("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForArrays.TestMethod", passing.Test.DisplayName);
        }

        class ClassUnderTestForArrays
        {
            [Theory]
            [InlineData(new[] { 42, 2112 }, new[] { "SELF", "PARENT1", "PARENT2", "PARENT3" }, null, 10.5, "Hello, world!")]
            public void TestMethod(int[] v1, string[] v2, float[] v3, double v4, string v5) { }
        }

        [Fact]
        public void ValueArraysWithObjectParameterInjectCorrectType()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTestForValueArraysWithObjectParameter));

            var passing = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Contains("Xunit2TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForValueArraysWithObjectParameter.TestMethod", passing.Test.DisplayName);
        }

        class ClassUnderTestForValueArraysWithObjectParameter
        {
            [Theory]
            [InlineData(new[] { 42, 2112 }, typeof(int[]))]
            public void TestMethod(object value, Type expected)
            {
                Assert.IsType(expected, value);
            }
        }
    }

    public class ClassDataTests : AcceptanceTestV2
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassUnderTest));

            var passing = Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Equal($"Xunit2TheoryAcceptanceTests+ClassDataTests+ClassUnderTest.TestViaInlineData(x: 42, y: {21.12}, z: \"Hello, world!\")", passing.Test.DisplayName);
            var failed = Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+ClassDataTests+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)", failed.Test.DisplayName);
            Assert.Empty(testMessages.OfType<ITestSkipped>());
        }

        class ClassUnderTest
        {
            [Theory]
            [ClassData(typeof(ClassDataSource))]
            public void TestViaInlineData(int x, double y, string z)
            {
                Assert.NotNull(z);
            }
        }

        class ClassDataSource : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { 42, 21.12, "Hello, world!" };
                yield return new object[] { 0, 0.0, null };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    public class MissingDataTests : AcceptanceTestV2
    {
        [Fact]
        public void MissingDataThrows()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithMissingData));

            var failed = Assert.Single(testMessages.Cast<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData.TestViaMissingData", failed.Test.DisplayName);
            Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
            Assert.Equal("Could not find public static member (property, field, or method) named 'Foo' on Xunit2TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData", failed.Messages.Single());
        }

        class ClassWithMissingData
        {
            [Theory]
            [MemberData("Foo")]
            public void TestViaMissingData(int x, double y, string z) { }
        }
    }

    public class DataConversionTests : AcceptanceTestV2
    {
        [Fact]
        public void IncompatibleDataThrows()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithIncompatibleData));

            var failed = Assert.Single(testMessages.Cast<ITestFailed>());
            Assert.Equal(@"Xunit2TheoryAcceptanceTests+DataConversionTests+ClassWithIncompatibleData.TestViaIncompatibleData(x: ""Foo"")", failed.Test.DisplayName);
            Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
            Assert.Equal("Object of type 'System.String' cannot be converted to type 'System.Int32'.", failed.Messages.Single());
        }

        class ClassWithIncompatibleData
        {
            [Theory]
            [InlineData("Foo")]
            public void TestViaIncompatibleData(int x) { }
        }

        [Fact]
        public void ImplicitlyConvertibleDataPasses()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithImplicitlyConvertibleData));

            var passed = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Equal(@"Xunit2TheoryAcceptanceTests+DataConversionTests+ClassWithImplicitlyConvertibleData.TestViaImplicitData(x: 42)", passed.Test.DisplayName);
        }

        class ClassWithImplicitlyConvertibleData
        {
            [Theory]
            [InlineData(42)]
            public void TestViaImplicitData(int? x) { }
        }

        [Fact]
        public void IConvertibleDataPasses()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithIConvertibleData));

            var passed = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Equal(@"Xunit2TheoryAcceptanceTests+DataConversionTests+ClassWithIConvertibleData.TestViaIConvertible(x: 42)", passed.Test.DisplayName);
        }

        class MyConvertible : IConvertible
        {
            public TypeCode GetTypeCode()
            {
                return TypeCode.Int32;
            }

            public int ToInt32(IFormatProvider provider)
            {
                return 42;
            }

            #region Noise

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public string ToString(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            #endregion
        }

        class ClassWithIConvertibleData
        {
            public static IEnumerable<object[]> Data = new TheoryData<MyConvertible> { new MyConvertible() };

            [Theory]
            [MemberData("Data")]
            public void TestViaIConvertible(int x) { }
        }
    }

    public class FieldDataTests : AcceptanceTestV2
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfFieldData));

            var passing = Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Equal($"Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithSelfFieldData.TestViaFieldData(x: 42, y: {21.12}, z: \"Hello, world!\")", passing.Test.DisplayName);
            var failed = Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithSelfFieldData.TestViaFieldData(x: 0, y: 0, z: null)", failed.Test.DisplayName);
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            var failed = Assert.Single(testMessages.Cast<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithNonStaticFieldData.TestViaFieldData", failed.Test.DisplayName);
            Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
            Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithNonStaticFieldData", failed.Messages.Single());
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

            var passed = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+FieldDataTests+ClassWithBaseClassData.TestViaFieldData(x: 42)", passed.Test.DisplayName);
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

    public class MethodDataTests : AcceptanceTestV2
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfMethodData));

            var passing = Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Equal($"Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithSelfMethodData.TestViaMethodData(x: 42, y: {21.12}, z: \"Hello, world!\")", passing.Test.DisplayName);
            var failed = Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithSelfMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failed.Test.DisplayName);
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            var failed = Assert.Single(testMessages.Cast<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithNonStaticMethodData.TestViaMethodData", failed.Test.DisplayName);
            Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
            Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithNonStaticMethodData", failed.Messages.Single());
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

            var passed = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithBaseClassData.TestViaMethodData(x: 42)", passed.Test.DisplayName);
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

            var passing = Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Equal($"Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 42, y: {21.12}, z: \"Hello, world!\")", passing.Test.DisplayName);
            var failed = Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failed.Test.DisplayName);
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

    public class PropertyDataTests : AcceptanceTestV2
    {
        [Fact]
        public void RunsForEachDataElement()
        {
            var testMessages = Run<ITestResultMessage>(typeof(ClassWithSelfPropertyData));

            var passing = Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Equal($"Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 42, y: {21.12}, z: \"Hello, world!\")", passing.Test.DisplayName);
            var failed = Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 0, y: 0, z: null)", failed.Test.DisplayName);
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            Assert.Single(testMessages.OfType<ITestPassed>());
            Assert.Single(testMessages.OfType<ITestFailed>());
            Assert.Empty(testMessages.OfType<ITestSkipped>());
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

            var failed = Assert.Single(testMessages.Cast<ITestFailed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData.TestViaPropertyData", failed.Test.DisplayName);
            Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
            Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData", failed.Messages.Single());
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

            var passed = Assert.Single(testMessages.Cast<ITestPassed>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+PropertyDataTests+ClassWithBaseClassData.TestViaPropertyData(x: 42)", passed.Test.DisplayName);
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

    public class CustomDataTests : AcceptanceTestV2
    {
        [Fact]
        public void TestDataWithInternalConstructor_ReturnsSingleFailingTheory()
        {
            var testMessages = Run<IMessageSinkMessage>(typeof(ClassWithCustomDataWithInternalDataCtor));

            var types = testMessages.Select(t => t.GetType()).ToList();

            Assert.Collection(testMessages.OfType<ITestFailed>().OrderBy(t => t.TestCase.DisplayName),
                failed => Assert.Equal("Constructor on type 'Xunit2TheoryAcceptanceTests+CustomDataTests+MyCustomData' not found.", failed.Messages[0])
            );
            Assert.Empty(testMessages.OfType<ITestPassed>());
            Assert.Empty(testMessages.OfType<ITestSkipped>());
        }

        internal class MyCustomData : DataAttribute
        {
            internal MyCustomData() { }

            public override IEnumerable<object[]> GetData(MethodInfo testMethod)
                => new[] { new object[] { 42 }, new object[] { 2112 } };
        }

        class ClassWithCustomDataWithInternalDataCtor
        {
            [Theory]
            [MyCustomData]
            public void Passing(int unused) { }
        }
    }

    public class ErrorAggregation : AcceptanceTestV2
    {
        [Fact]
        public void EachTheoryHasIndividualExceptionMessage()
        {
            var testMessages = Run<ITestFailed>(typeof(ClassUnderTest));

            var equalFailure = Assert.Single(testMessages, msg => msg.Test.DisplayName == $"Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 42, y: {21.12}, z: ClassUnderTest {{ }})");
            Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

            var notNullFailure = Assert.Single(testMessages, msg => msg.Test.DisplayName == "Xunit2TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)");
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

    public class OverloadedMethodTests : AcceptanceTestV2
    {
        [Fact]
        public void TestMethodMessagesOnlySentOnce()
        {
            var testMessages = Run<IMessageSinkMessage>(typeof(ClassUnderTest));

            var methodStarting = Assert.Single(testMessages.OfType<ITestMethodStarting>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+OverloadedMethodTests+ClassUnderTest", methodStarting.TestClass.Class.Name);
            Assert.Equal("Theory", methodStarting.TestMethod.Method.Name);
            var methodFinished = Assert.Single(testMessages.OfType<ITestMethodFinished>());
            Assert.Equal("Xunit2TheoryAcceptanceTests+OverloadedMethodTests+ClassUnderTest", methodFinished.TestClass.Class.Name);
            Assert.Equal("Theory", methodFinished.TestMethod.Method.Name);
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