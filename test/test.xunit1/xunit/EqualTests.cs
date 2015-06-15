using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class EqualTests
    {
        public class ArrayTests
        {
            [Fact]
            public void Array()
            {
                string[] expected = { "@", "a", "ab", "b" };
                string[] actual = { "@", "a", "ab", "b" };

                Assert.Equal(expected, actual);
                Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
            }

            [Fact]
            public void ArrayInsideArray()
            {
                string[][] expected = { new[] { "@", "a" }, new[] { "ab", "b" } };
                string[][] actual = { new[] { "@", "a" }, new[] { "ab", "b" } };

                Assert.Equal(expected, actual);
                Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
            }

            [Fact]
            public void ArraysOfDifferentLengthsAreNotEqual()
            {
                string[] expected = { "@", "a", "ab", "b", "c" };
                string[] actual = { "@", "a", "ab", "b" };

                Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
                Assert.NotEqual(expected, actual);
            }

            [Fact]
            public void ArrayValuesAreDifferentNotEqual()
            {
                string[] expected = { "@", "d", "v", "d" };
                string[] actual = { "@", "a", "ab", "b" };

                Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
                Assert.NotEqual(expected, actual);
            }

            [Fact]
            public void EnumerableEquivalence()
            {
                int[] expected = new[] { 1, 2, 3, 4, 5 };
                List<int> actual = new List<int>(expected);

                Assert.Equal(expected, actual);
            }

            [Fact]
            public void EnumerableInequivalence()
            {
                int[] expected = new[] { 1, 2, 3, 4, 5 };
                List<int> actual = new List<int>(new[] { 1, 2, 3, 4, 6 });

                EqualException ex = Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));

                Assert.Contains("First difference is at position 4" + Environment.NewLine, ex.Message);
            }

            [Fact]
            public void EnumerableEquivalenceWithSuccessfulComparer()
            {
                int[] expected = new[] { 1, 2, 3, 4, 5 };
                List<int> actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

                Assert.Equal(expected, actual, new IntComparer(true));
            }

            [Fact]
            public void EnumerableEquivalenceWithFailedComparer()
            {
                int[] expected = new[] { 1, 2, 3, 4, 5 };
                List<int> actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

                EqualException ex = Assert.Throws<EqualException>(() => Assert.Equal(expected, actual, new IntComparer(false)));

                // TODO: When we fix up the assert exception messages, we should allow the enumerator who
                // did the comparisons to tell us exactly where the error was, rather than determining the
                // inequivalence after the fact.

                // Assert.Contains("First difference is at position 0\r\n", ex.Message);
            }

            [Fact]
            public void DepthExample()
            {
                var x = new List<object> { new List<object> { new List<object> { new List<object>() } } };
                var y = new List<object> { new List<object> { new List<object> { new List<object>() } } };

                Assert.Equal(x, y);
            }

            class IntComparer : IEqualityComparer<int>
            {
                bool answer;

                public IntComparer(bool answer)
                {
                    this.answer = answer;
                }

                public bool Equals(int x, int y)
                {
                    return answer;
                }

                public int GetHashCode(int obj)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class ComparableObject : IComparable
        {
            public bool CompareCalled;

            public int CompareTo(object obj)
            {
                CompareCalled = true;
                return 0;
            }
        }

        public class ComparableTests
        {
            [Fact]
            public void ObjectWithComparable()
            {
                ComparableObject obj1 = new ComparableObject();
                ComparableObject obj2 = new ComparableObject();

                Assert.Equal(obj1, obj2);
                Assert.True(obj1.CompareCalled);
            }

            [Fact]
            public void ObjectWithGenericComparable()
            {
                GenericComparableObject obj1 = new GenericComparableObject();
                GenericComparableObject obj2 = new GenericComparableObject();

                Assert.Equal(obj1, obj2);
                Assert.True(obj1.CompareCalled);
            }

            [Fact]
            public void ObjectWithoutIComparable()
            {
                NonComparableObject nco1 = new NonComparableObject();
                NonComparableObject nco2 = new NonComparableObject();

                Assert.Equal(nco1, nco2);
            }
        }

        public class DoubleInfinityTests
        {
            [Fact]
            public void DoubleNegativeInfinityEqualsNegativeInfinity()
            {
                Assert.Equal(double.NegativeInfinity, double.NegativeInfinity);
            }

            [Fact]
            public void DoubleNegativeInfinityNotEquals()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(1.23, double.NegativeInfinity));
            }

            [Fact]
            public void DoublePositiveInfinityEqualsPositiveInfinity()
            {
                Assert.Equal(double.PositiveInfinity, double.PositiveInfinity);
            }

            [Fact]
            public void DoublePositiveInfinityNotEquals()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(1.23, double.PositiveInfinity));
            }

            [Fact]
            public void DoublePositiveInfinityNotEqualsNegativeInfinity()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(double.NegativeInfinity, double.PositiveInfinity));
            }
        }

        public class EnumerableTests
        {
            [Fact]
            public void Select_should_equal_Select()
            {
                IEnumerable<int> items = IntGenerator.Range(1, 12);
                IEnumerable<int> others = IntGenerator.Range(1, 12);

                Assert.Equal(items, others);
            }

            class IntGenerator
            {
                public static IEnumerable<int> Range(int start, int end)
                {
                    for (int i = start; i <= end; i++)
                        yield return i;
                }
            }
        }

        public class EquatableObject : IEquatable<EquatableObject>
        {
            public bool Equals__Called;
            public EquatableObject Equals_Other;

            public bool Equals(EquatableObject other)
            {
                Equals__Called = true;
                Equals_Other = other;

                return true;
            }
        }

        public class EquatableObjectTests
        {
            [Fact]
            public void CallsIEquatable()
            {
                EquatableObject obj1 = new EquatableObject();
                EquatableObject obj2 = new EquatableObject();

                Assert.Equal(obj1, obj2);

                Assert.True(obj1.Equals__Called);
                Assert.Same(obj2, obj1.Equals_Other);
            }
        }

        public class GenericComparableObject : IComparable<GenericComparableObject>
        {
            public bool CompareCalled;

            public int CompareTo(GenericComparableObject other)
            {
                CompareCalled = true;
                return 0;
            }
        }

        public class NaNTests
        {
            [Fact]
            public void EqualsNaNFails()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(double.NaN, 1.234));
            }

            [Fact]
            public void NanEqualsFails()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(1.234, double.NaN));
            }

            [Fact]
            public void NanEqualsNaNSucceeds()
            {
                Assert.Equal(double.NaN, double.NaN);
            }
        }

        public class NonComparableObject
        {
            public override bool Equals(object obj)
            {
                return true;
            }

            public override int GetHashCode()
            {
                return 42;
            }
        }

        public class NullTests
        {
            [Fact]
            public void EqualsNull()
            {
                Assert.Equal<object>(null, null);
            }

            [Fact]
            public void FailsWhenActualIsNullExpectedIsNot()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(new object(), null));
            }

            [Fact]
            public void FailsWhenExpectedIsNullActualIsNot()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(null, new object()));
            }
        }

        public class NumericTests
        {
            [Fact]
            public void DecimalEqualsFails()
            {
                decimal expected = 25;
                decimal actual = 42;

                Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            }

            [Fact]
            public void DoubleEqualsFails()
            {
                double expected = 25.3;
                double actual = 42.0;

                Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            }

            [Fact]
            public void EqualsByte()
            {
                byte valueType = 35;
                Byte referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<byte>(valueType, 35);
                Assert.Equal<byte>(referenceValue, 35);
            }

            [Fact]
            public void EqualsDecimal()
            {
                decimal valueType = 35;
                Decimal referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<decimal>(valueType, 35);
                Assert.Equal(valueType, 35M);
                Assert.Equal<decimal>(referenceValue, 35);
            }

            [Fact]
            public void EqualsInt16()
            {
                short valueType = 35;
                Int16 referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<short>(valueType, 35);
                Assert.Equal<short>(referenceValue, 35);
            }

            [Fact]
            public void EqualsInt32()
            {
                int valueType = 35;
                Int32 referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal(valueType, 35);
                Assert.Equal(referenceValue, 35);
            }

            [Fact]
            public void EqualsInt64()
            {
                long valueType = 35;
                Int64 referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<long>(valueType, 35);
                Assert.Equal<long>(referenceValue, 35);
            }

            [Fact]
            public void EqualsSByte()
            {
                sbyte valueType = 35;
                SByte referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<sbyte>(valueType, 35);
                Assert.Equal<sbyte>(referenceValue, 35);
            }

            [Fact]
            public void EqualsUInt16()
            {
                ushort valueType = 35;
                UInt16 referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<ushort>(valueType, 35);
                Assert.Equal<ushort>(referenceValue, 35);
            }

            [Fact]
            public void EqualsUInt32()
            {
                uint valueType = 35;
                UInt32 referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<uint>(valueType, 35);
                Assert.Equal<uint>(referenceValue, 35);
            }

            [Fact]
            public void EqualsUInt64()
            {
                ulong valueType = 35;
                UInt64 referenceValue = 35;

                Assert.True(valueType == referenceValue);
                Assert.Equal(referenceValue, valueType);
                Assert.Equal<ulong>(valueType, 35);
                Assert.Equal<ulong>(referenceValue, 35);
            }

            [Fact]
            public void Int32Int64Comparison()
            {
                long l64 = 0;
                int i32 = 0;
                Assert.Equal<long>(l64, i32);
            }

            [Fact]
            public void IntegerLongComparison()
            {
                Assert.Equal<long>(1L, 1);
                Assert.Equal<long>(1, 1L);
            }

            [Fact]
            public void LongEquals()
            {
                Assert.Equal(2L, 2L);
            }

            [Fact]
            public void LongEqualsFails()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(3L, 2L));
            }

            [Fact]
            public void UInt64EqualsFails()
            {
                UInt64 expected = 25;
                UInt64 actual = 42;

                Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            }
        }

        public class SingleInfinityTests
        {
            [Fact]
            public void SingleNegativeInfinityEqualsNegativeInfinity()
            {
                Assert.Equal(float.NegativeInfinity, float.NegativeInfinity);
            }

            [Fact]
            public void SingleNumberNotEqualNegativeInfinity()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(1.23f, float.NegativeInfinity));
            }

            [Fact]
            public void SingleNumberNotEqualPositiiveInfinity()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(1.23f, float.PositiveInfinity));
            }

            [Fact]
            public void SinglePositiveInfinityEqualsPositiveInfinity()
            {
                Assert.Equal(float.PositiveInfinity, float.PositiveInfinity);
            }

            [Fact]
            public void SinglePositiveInfinityNotEqualNegativeInfinity()
            {
                Assert.Throws<EqualException>(() => Assert.Equal(float.NegativeInfinity, float.PositiveInfinity));
            }
        }

        public class StringTests
        {
            [Fact]
            public void EqualsFail()
            {
                Assert.Throws<EqualException>(() => Assert.Equal("expected", "actual"));
            }

            [Fact]
            public void EqualsString()
            {
                string testString = "Test String";
                string expected = testString;
                string actual = testString;

                Assert.True(actual == expected);
                Assert.Equal(expected, actual);
            }

            [Fact]
            public void EqualStringWithTrailingNull()
            {
                Exception ex = Record.Exception(() => Assert.Equal("foo", "foo\0"));

                Assert.IsType<EqualException>(ex);
            }

            [Fact]
            public void EqualsStringIgnoreCase()
            {
                string expected = "TestString";
                string actual = "testString";

                Assert.False(actual == expected);
                Assert.NotEqual(expected, actual);
                Assert.Equal(expected, actual, StringComparer.CurrentCultureIgnoreCase);
            }

            [Fact]
            public void String()
            {
                string s1 = "test";
                string s2 = new StringBuilder(s1).ToString();

                Assert.True(s1.Equals(s2));
                Assert.Equal(s2, s1);
            }
        }

        public class NullableValueTypesTests
        {
            [Fact]
            public void NullableValueTypesCanBeNull()
            {
                DateTime? dt1 = null;
                DateTime? dt2 = null;

                Assert.Equal(dt1, dt2);
            }
        }

        public class PrecisionTests
        {
            [Fact]
            public void AssertEqualWithDoubleWithPrecision()
            {
                Assert.Equal(0.11111, 0.11444, 2);
            }

            [Fact]
            public void AssertEqualWithDoubleWithPrecisionFailure()
            {
                var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11111, 0.11444, 3));
                Assert.Equal(string.Format("{0} (rounded from {1})", 0.111, 0.11111), ex.Expected);
                Assert.Equal(string.Format("{0} (rounded from {1})", 0.114, 0.11444), ex.Actual);
            }

            [Fact]
            public void AssertEqualWithDecimalWithPrecision()
            {
                Assert.Equal(0.11111M, 0.11444M, 2);
            }

            [Fact]
            public void AssertEqualWithDecimalWithPrecisionFailure()
            {
                var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11111M, 0.11444M, 3));
                Assert.Equal(string.Format("{0} (rounded from {1})", 0.111M, 0.11111M), ex.Expected);
                Assert.Equal(string.Format("{0} (rounded from {1})", 0.114M, 0.11444M), ex.Actual);
            }
        }
    }
}
