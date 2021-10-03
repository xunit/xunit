using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using Xunit.Sdk;

public class EqualityAssertsTests
{
	public class Equal
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(42, 42);
		}

		[Fact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(42, 2112));
			Assert.Equal("42", ex.Expected);
			Assert.Equal("2112", ex.Actual);
		}

		[Fact]
		public void Comparable()
		{
			var obj1 = new SpyComparable();
			var obj2 = new SpyComparable();

			Assert.Equal(obj1, obj2);
			Assert.True(obj1.CompareCalled);
		}

		[Fact]
		public void Comparable_NonGeneric_SameType_Equal()
		{
			var expected = new MultiComparable(1);
			var actual = new MultiComparable(1);

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (IComparable)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void Comparable_NonGeneric_SameType_NotEqual()
		{
			var expected = new MultiComparable(1);
			var actual = new MultiComparable(2);

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (IComparable)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void Comparable_NonGeneric_DifferentType_Equal()
		{
			var expected = new MultiComparable(1);
			var actual = 1;

			Assert.Equal(expected, (IComparable)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void Comparable_NonGeneric_DifferentType_NotEqual()
		{
			var expected = new MultiComparable(1);
			var actual = 2;

			Assert.NotEqual(expected, (IComparable)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		class MultiComparable : IComparable
		{
			private int Value { get; }

			public MultiComparable(int value)
			{
				Value = value;
			}

			public int CompareTo(object? obj)
			{
				if (obj is int intObj)
					return Value.CompareTo(intObj);
				else if (obj is MultiComparable multiObj)
					return Value.CompareTo(multiObj.Value);

				throw new InvalidOperationException();
			}
		}

		[Fact]
		public void Comparable_Generic()
		{
			var obj1 = new SpyComparable_Generic();
			var obj2 = new SpyComparable_Generic();

			Assert.Equal(obj1, obj2);
			Assert.True(obj1.CompareCalled);
		}

		[Fact]
		public void Comparable_SubClass_SubClass_Equal()
		{
			var expected = new ComparableSubClassA(1);
			var actual = new ComparableSubClassB(1);

			Assert.Equal<ComparableBaseClass>(expected, actual);
		}

		[Fact]
		public void Comparable_SubClass_SubClass_NotEqual()
		{
			var expected = new ComparableSubClassA(1);
			var actual = new ComparableSubClassB(2);

			Assert.NotEqual<ComparableBaseClass>(expected, actual);
		}

		[Fact]
		public void Comparable_BaseClass_SubClass_Equal()
		{
			var expected = new ComparableBaseClass(1);
			var actual = new ComparableSubClassA(1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Comparable_SubClass_BaseClass_Equal()
		{
			var expected = new ComparableSubClassA(1);
			var actual = new ComparableBaseClass(1);

			Assert.Equal(expected, actual);
		}

		class ComparableBaseClass : IComparable<ComparableBaseClass>
		{
			private int Value { get; }

			public ComparableBaseClass(int value)
			{
				Value = value;
			}

			public int CompareTo(ComparableBaseClass? other) => Value.CompareTo(other!.Value);
		}

		class ComparableSubClassA : ComparableBaseClass
		{
			public ComparableSubClassA(int value) : base(value)
			{ }
		}

		class ComparableSubClassB : ComparableBaseClass
		{
			public ComparableSubClassB(int value) : base(value)
			{ }
		}

		[Fact]
		public void Comparable_Generic_ThrowsException_Equal()
		{
			var expected = new ComparableThrower(1);
			var actual = new ComparableThrower(1);

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (IComparable<ComparableThrower>)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void Comparable_Generic_ThrowsException_NotEqual()
		{
			var expected = new ComparableThrower(1);
			var actual = new ComparableThrower(2);

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (IComparable<ComparableThrower>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		class ComparableThrower : IComparable<ComparableThrower>
		{
			public int Value { get; }

			public ComparableThrower(int value)
			{
				Value = value;
			}

			public int CompareTo(ComparableThrower? other)
			{
				throw new InvalidOperationException();
			}

			public override bool Equals(object? obj) => Value == ((ComparableThrower?)obj)!.Value;

			public override int GetHashCode() => Value;
		}

		[Fact]
		public void Equatable()
		{
			var obj1 = new SpyEquatable();
			var obj2 = new SpyEquatable();

			Assert.Equal(obj1, obj2);

			Assert.True(obj1.Equals__Called);
			Assert.Same(obj2, obj1.Equals_Other);
		}

		[Fact]
		public void Equatable_SubClass_SubClass_Equal()
		{
			var expected = new EquatableSubClassA(1);
			var actual = new EquatableSubClassB(1);

			Assert.Equal<EquatableBaseClass>(expected, actual);
		}

		[Fact]
		public void Equatable_SubClass_SubClass_NotEqual()
		{
			var expected = new EquatableSubClassA(1);
			var actual = new EquatableSubClassB(2);

			Assert.NotEqual<EquatableBaseClass>(expected, actual);
		}

		[Fact]
		public void Equatable_BaseClass_SubClass_Equal()
		{
			var expected = new EquatableBaseClass(1);
			var actual = new EquatableSubClassA(1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Equatable_SubClass_BaseClass_Equal()
		{
			var expected = new EquatableSubClassA(1);
			var actual = new EquatableBaseClass(1);

			Assert.Equal(expected, actual);
		}

		class EquatableBaseClass : IEquatable<EquatableBaseClass>
		{
			private int Value { get; }

			public EquatableBaseClass(int value)
			{
				Value = value;
			}

			public bool Equals(EquatableBaseClass? other) => Value == other!.Value;
		}

		class EquatableSubClassA : EquatableBaseClass
		{
			public EquatableSubClassA(int value) : base(value) { }
		}

		class EquatableSubClassB : EquatableBaseClass
		{
			public EquatableSubClassB(int value) : base(value) { }
		}

		[Fact]
		public void NonComparable()
		{
			var nco1 = new NonComparableObject();
			var nco2 = new NonComparableObject();

			Assert.Equal(nco1, nco2);
		}

		[Fact]
		public void IStructuralEquatable_Equal()
		{
			var expected = new Tuple<StringWrapper>(new StringWrapper("a"));
			var actual = new Tuple<StringWrapper>(new StringWrapper("a"));

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (IStructuralEquatable)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IStructuralEquatable_NotEqual()
		{
			var expected = new Tuple<StringWrapper>(new StringWrapper("a"));
			var actual = new Tuple<StringWrapper>(new StringWrapper("b"));

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (IStructuralEquatable)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void IStructuralEquatable_ExpectedNull_ActualNull()
		{
			var expected = new Tuple<StringWrapper?>(null);
			var actual = new Tuple<StringWrapper?>(null);

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (IStructuralEquatable)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IStructuralEquatable_ExpectedNull_ActualNonNull()
		{
			var expected = new Tuple<StringWrapper?>(null);
			var actual = new Tuple<StringWrapper?>(new StringWrapper("a"));

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (IStructuralEquatable)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void IStructuralEquatable_ExpectedNonNull_ActualNull()
		{
			var expected = new Tuple<StringWrapper?>(new StringWrapper("a"));
			var actual = new Tuple<StringWrapper?>(null);

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (IStructuralEquatable)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		class StringWrapper : IEquatable<StringWrapper>
		{
			public string Value { get; }

			public StringWrapper(string value)
			{
				Value = value;
			}

			bool IEquatable<StringWrapper>.Equals(StringWrapper? other) => Value == other!.Value;
		}

		[Fact]
		public void DepthExample()
		{
			var x = new List<object> { new List<object> { new List<object> { new List<object>() } } };
			var y = new List<object> { new List<object> { new List<object> { new List<object>() } } };

			Assert.Equal(x, y);
		}

		[Fact]
		public void IReadOnlyCollection_IEnumerable()
		{
			var expected = new string[] { "foo", "bar" };
			var actual = (IReadOnlyCollection<string>)new ReadOnlyCollection<string>(expected);

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (object)actual);
			Assert.Equal(actual, expected);
			Assert.Equal(actual, (object)expected);
		}

		[Fact]
		public void StringArray_ObjectArray()
		{
			var expected = new string[] { "foo", "bar" };
			var actual = new object[] { "foo", "bar" };

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (object)actual);
			Assert.Equal(actual, expected);
			Assert.Equal(actual, (object)expected);
		}

		[Fact]
		public void IDictionary_SameTypes()
		{
			var expected = new Dictionary<string, string> { ["foo"] = "bar" };
			var actual = new Dictionary<string, string> { ["foo"] = "bar" };

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (IDictionary)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IDictionary_DifferentTypes()
		{
			var expected = new Dictionary<string, string> { ["foo"] = "bar" };
			var actual = new ConcurrentDictionary<string, string>(expected);

			Assert.Equal(expected, (IDictionary)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void ISet_SameTypes()
		{
			var expected = new HashSet<string> { "foo", "bar" };
			var actual = new HashSet<string> { "foo", "bar" };

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (ISet<string>)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void ISet_DifferentTypes()
		{
			var expected = new HashSet<string> { "bar", "foo" };
			var actual = new SortedSet<string> { "foo", "bar" };

			Assert.Equal(expected, (ISet<string>)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void ISet_NonGenericSubClass_Equal()
		{
			var expected = new NonGenericSet { "bar", "foo" };
			var actual = new NonGenericSet { "bar", "foo" };

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (ISet<string>)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void ISet_NonGenericSubClass_NotEqual()
		{
			var expected = new NonGenericSet { "bar", "foo" };
			var actual = new NonGenericSet { "bar", "baz" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (ISet<string>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void ISet_NonGenericSubClass_DifferentCounts()
		{
			var expected = new NonGenericSet { "bar" };
			var actual = new NonGenericSet { "bar", "foo" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (ISet<string>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void ISet_NonGenericSubClass_DifferentTypesEqual()
		{
			var expected = new NonGenericSet { "bar" };
			var actual = new HashSet<string> { "bar" };

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (ISet<string>)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void ISet_NonGenericSubClass_DifferentTypesNotEqual()
		{
			var expected = new NonGenericSet { "bar" };
			var actual = new HashSet<int> { 1 };

			Assert.NotEqual(expected, (object)actual);
		}

		class NonGenericSet : HashSet<string> { }

		[Fact]
		public void ISet_TwoGenericSubClass_Equal()
		{
			var expected = new TwoGenericSet<string, int> { "foo", "bar" };
			var actual = new TwoGenericSet<string, int> { "foo", "bar" };

			Assert.Equal(expected, actual);
			Assert.Equal(expected, (ISet<string>)actual);
			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void ISet_TwoGenericSubClass_NotEqual()
		{
			var expected = new TwoGenericSet<string, int> { "foo", "bar" };
			var actual = new TwoGenericSet<string, int> { "foo", "baz" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (ISet<string>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void ISet_TwoGenericSubClass_DifferentCounts()
		{
			var expected = new TwoGenericSet<string, int> { "bar" };
			var actual = new TwoGenericSet<string, int> { "foo", "bar" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (ISet<string>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		class TwoGenericSet<T, U> : HashSet<T> { }

		[Fact]
		public void IEquatableActual_Implicit_Equal()
		{
			var expected = new ImplicitIEquatableExpected(1);
			var actual = new IntWrapper(1);

			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IEquatableActual_Implicit_NotEqual()
		{
			var expected = new ImplicitIEquatableExpected(1);
			var actual = new IntWrapper(2);

			Assert.NotEqual(expected, (object)actual);
		}

		class ImplicitIEquatableExpected : IEquatable<IntWrapper>
		{
			public int Value { get; }

			public ImplicitIEquatableExpected(int value)
			{
				Value = value;
			}

			public bool Equals(IntWrapper? other) => Value == other!.Value;
		}

		[Fact]
		public void IEquatableActual_Explicit_Equal()
		{
			var expected = new ExplicitIEquatableExpected(1);
			var actual = new IntWrapper(1);

			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IEquatableActual_Explicit_NotEqual()
		{
			var expected = new ExplicitIEquatableExpected(1);
			var actual = new IntWrapper(2);

			Assert.NotEqual(expected, (object)actual);
		}

		class ExplicitIEquatableExpected : IEquatable<IntWrapper>
		{
			public int Value { get; }

			public ExplicitIEquatableExpected(int value)
			{
				Value = value;
			}

			bool IEquatable<IntWrapper>.Equals(IntWrapper? other) => Value == other!.Value;
		}

		[Fact]
		public void IComparableActual_Implicit_Equal()
		{
			var expected = new ImplicitIComparableExpected(1);
			var actual = new IntWrapper(1);

			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IComparableActual_Implicit_NotEqual()
		{
			var expected = new ImplicitIComparableExpected(1);
			var actual = new IntWrapper(2);

			Assert.NotEqual(expected, (object)actual);
		}

		class ImplicitIComparableExpected : IComparable<IntWrapper>
		{
			public int Value { get; }

			public ImplicitIComparableExpected(int value)
			{
				Value = value;
			}

			public int CompareTo(IntWrapper? other) => Value.CompareTo(other!.Value);
		}

		[Fact]
		public void IComparableActual_Explicit_Equal()
		{
			var expected = new ExplicitIComparableActual(1);
			var actual = new IntWrapper(1);

			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IComparableActual_Explicit_NotEqual()
		{
			var expected = new ExplicitIComparableActual(1);
			var actual = new IntWrapper(2);

			Assert.NotEqual(expected, (object)actual);
		}

		class ExplicitIComparableActual : IComparable<IntWrapper>
		{
			public int Value { get; }

			public ExplicitIComparableActual(int value)
			{
				Value = value;
			}

			int IComparable<IntWrapper>.CompareTo(IntWrapper? other) => Value.CompareTo(other!.Value);
		}

		[Fact]
		public void IComparableActual_ThrowsException_Equal()
		{
			var expected = new IComparableActualThrower(1);
			var actual = new IntWrapper(1);

			Assert.Equal(expected, (object)actual);
		}

		[Fact]
		public void IComparableActual_ThrowsException_NotEqual()
		{
			var expected = new IComparableActualThrower(1);
			var actual = new IntWrapper(2);

			Assert.NotEqual(expected, (object)actual);
		}

		class IComparableActualThrower : IComparable<IntWrapper>
		{
			public int Value { get; }

			public IComparableActualThrower(int value)
			{
				Value = value;
			}

			public int CompareTo(IntWrapper? other)
			{
				throw new NotSupportedException();
			}

			public override bool Equals(object? obj) => Value == ((IntWrapper?)obj)!.Value;

			public override int GetHashCode() => Value;
		}

		class IntWrapper
		{
			public int Value { get; }

			public IntWrapper(int value)
			{
				Value = value;
			}
		}

		class SpyComparable : IComparable
		{
			public bool CompareCalled;

			public int CompareTo(object? obj)
			{
				CompareCalled = true;
				return 0;
			}
		}

		class SpyComparable_Generic : IComparable<SpyComparable_Generic>
		{
			public bool CompareCalled;

			public int CompareTo(SpyComparable_Generic? other)
			{
				CompareCalled = true;
				return 0;
			}
		}

		public class SpyEquatable : IEquatable<SpyEquatable>
		{
			public bool Equals__Called;
			public SpyEquatable? Equals_Other;

			public bool Equals(SpyEquatable? other)
			{
				Equals__Called = true;
				Equals_Other = other;

				return true;
			}
		}

		class NonComparableObject
		{
			public override bool Equals(object? obj) => true;

			public override int GetHashCode() => 42;
		}
	}

	public class Equal_WithComparer
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(42, 21, new Comparer<int>(true));
		}

		[Fact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(42, 42, new Comparer<int>(false)));
			Assert.Equal("42", ex.Expected);
			Assert.Equal("42", ex.Actual);
		}

		class Comparer<T> : IEqualityComparer<T>
		{
			readonly bool result;

			public Comparer(bool result)
			{
				this.result = result;
			}

			public bool Equals(T? x, T? y) => result;

			public int GetHashCode(T obj) => throw new NotImplementedException();
		}
	}

	public class Equal_Decimal
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(0.11111M, 0.11444M, 2);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11111M, 0.11444M, 3));
			Assert.Equal($"{0.111M} (rounded from {0.11111M})", ex.Expected);
			Assert.Equal($"{0.114M} (rounded from {0.11444M})", ex.Actual);
		}
	}

	public class Equal_Double
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(0.11111, 0.11444, 2);
		}

		[Fact]
		public void Success_Zero()
		{
			Assert.Equal(0.0, 0.0);
			Assert.Equal(0.0, (object)0.0);
		}

		[Fact]
		public void Success_PositiveZero_NegativeZero()
		{
			Assert.Equal(0.0, -0.0);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11111, 0.11444, 3));
			Assert.Equal($"{0.111M} (rounded from {0.11111})", ex.Expected);
			Assert.Equal($"{0.114M} (rounded from {0.11444})", ex.Actual);
		}
	}

	public class Equal_Double_MidPointRounding
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(10.566, 10.565, 2, MidpointRounding.AwayFromZero);
		}

		[Fact]
		public void Success_Zero()
		{
			Assert.Equal(0.00, 0.05, 1, MidpointRounding.ToEven);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11113, 0.11115, 4, MidpointRounding.ToEven));
			Assert.Equal($"{0.1111} (rounded from {0.11113})", ex.Expected);
			Assert.Equal($"{0.1112} (rounded from {0.11115})", ex.Actual);
		}
	}

	public class Equal_Double_Tolerance
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(10.566, 10.565, 0.01);
		}

		[Fact]
		public void Success_Zero()
		{
			Assert.Equal(0.00, 0.05, 0.1);
		}

		[Fact]
		public void Success_NaN()
		{
			Assert.Equal(double.NaN, double.NaN, 1000.0);
		}

		[Fact]
		public void Success_Infinite()
		{
			Assert.Equal(double.MinValue, double.MaxValue, double.PositiveInfinity);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11113, 0.11115, 0.00001));
			Assert.Equal($"{0.11113:G17}", ex.Expected);
			Assert.Equal($"{0.11115:G17}", ex.Actual);
		}

		[CulturedFact]
		public void Failure_NaN()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(20210102.2208, double.NaN, 20000000.0));
			Assert.Equal($"{20210102.2208:G17}", ex.Expected);
			Assert.Equal($"NaN", ex.Actual);
		}

		[CulturedFact]
		public void Failure_PositiveInfinity()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(double.PositiveInfinity, 77.7, 1.0));
			Assert.Equal(double.PositiveInfinity.ToString(), ex.Expected);
			Assert.Equal($"{77.7:G17}", ex.Actual);
		}

		[CulturedFact]
		public void Failure_NegativeInfinity()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.0, double.NegativeInfinity, 1.0));
			Assert.Equal($"{0.0:G17}", ex.Expected);
			Assert.Equal(double.NegativeInfinity.ToString(), ex.Actual);
		}

		[CulturedFact]
		public void Failure_InvalidTolerance()
		{
			var ex = Assert.Throws<ArgumentException>(() => Assert.Equal(0.0, 1.0, double.NegativeInfinity));
			Assert.StartsWith($"Tolerance must be greater than or equal to zero", ex.Message);
			Assert.Equal("tolerance", ex.ParamName);
		}
	}

	public class Equal_Float
	{
		[Fact]
		public void Success()
		{
			Assert.Equal(10.566f, 10.565f, 0.01f);
		}

		[Fact]
		public void Success_Zero()
		{
			Assert.Equal(0.00f, 0.05f, 0.1f);
		}

		[Fact]
		public void Success_NaN()
		{
			Assert.Equal(float.NaN, float.NaN, 1000.0f);
		}

		[Fact]
		public void Success_Infinite()
		{
			Assert.Equal(float.MinValue, float.MaxValue, float.PositiveInfinity);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11113f, 0.11115f, 0.00001f));
			Assert.Equal($"{0.11113f:G9}", ex.Expected);
			Assert.Equal($"{0.11115f:G9}", ex.Actual);
		}

		[CulturedFact]
		public void Failure_NaN()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(20210102.2208f, float.NaN, 20000000.0f));
			Assert.Equal($"{20210102.2208f:G9}", ex.Expected);
			Assert.Equal($"NaN", ex.Actual);
		}

		[CulturedFact]
		public void Failure_PositiveInfinity()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(float.PositiveInfinity, 77.7f, 1.0f));
			Assert.Equal(float.PositiveInfinity.ToString(), ex.Expected);
			Assert.Equal($"{77.7f:G9}", ex.Actual);
		}

		[CulturedFact]
		public void Failure_NegativeInfinity()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.0f, float.NegativeInfinity, 1.0f));
			Assert.Equal($"{0.0f:G9}", ex.Expected);
			Assert.Equal(float.NegativeInfinity.ToString(), ex.Actual);
		}

		[CulturedFact]
		public void Failure_InvalidTolerance()
		{
			var ex = Assert.Throws<ArgumentException>(() => Assert.Equal(0.0f, 1.0f, float.NegativeInfinity));
			Assert.StartsWith($"Tolerance must be greater than or equal to zero", ex.Message);
			Assert.Equal("tolerance", ex.ParamName);
		}
	}

	public class StrictEqual
	{
		[Fact]
		public static void Success()
		{
			Assert.StrictEqual(42, 42);
		}

		[Fact]
		public static void Equals()
		{
			Assert.StrictEqual(new DerivedClass(), new BaseClass());
		}

		[Fact]
		public static void Failure()
		{
			var ex = Assert.Throws<EqualException>(() => Assert.StrictEqual(42, 2112));
			Assert.Equal("42", ex.Expected);
			Assert.Equal("2112", ex.Actual);
		}

		[Fact]
		public static void Collection_Failure()
		{
			var expected = new EnumerableClass("ploeh");
			var actual = new EnumerableClass("fnaah");

			var ex = Assert.Throws<EqualException>(() => Assert.StrictEqual(expected, actual));
			Assert.Equal("EnumerableClass []", ex.Expected);
			Assert.Equal("EnumerableClass []", ex.Actual);
		}
	}

	public class NotEqual
	{
		[Fact]
		public void Success()
		{
			Assert.NotEqual("bob", "jim");
		}

		[Fact]
		public void String_Double_Failure()
		{
			Assert.NotEqual("0", (object)0.0);
			Assert.NotEqual((object)0.0, "0");
		}

		[Fact]
		public void IReadOnlyCollection_IEnumerable_Success()
		{
			var expected = new string[] { "foo", "bar" };
			IReadOnlyCollection<string> actual = new ReadOnlyCollection<string>(new string[] { "foo", "baz" });

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void IReadOnlyCollection_IEnumerable_Failure()
		{
			var expected = new string[] { "foo", "bar" };
			IReadOnlyCollection<string> actual = new ReadOnlyCollection<string>(expected);

			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, (object)actual));
		}

		[Fact]
		public void StringArray_ObjectArray_Success()
		{
			var expected = new string[] { "foo", "bar" };
			var actual = new object[] { "foo", "baz" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void StringArray_ObjectArray_Failure()
		{
			var expected = new string[] { "foo", "bar" };
			var actual = new object[] { "foo", "bar" };

			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, (object)actual));
		}

		[Fact]
		public void MultidimensionalArrays()
		{
			var expected = new string[] { "foo", "bar" };
			var actual = new string[,] { { "foo" }, { "baz" } };

			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void IDictionary_SameTypes()
		{
			var expected = new Dictionary<string, string> { ["foo"] = "bar" };
			var actual = new Dictionary<string, string> { ["foo"] = "baz" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (IDictionary)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void IDictionary_DifferentTypes()
		{
			var expected = new Dictionary<string, string> { ["foo"] = "bar" };
			var actual = new ConcurrentDictionary<string, string> { ["foo"] = "baz" };

			Assert.NotEqual(expected, (IDictionary)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void ISet_SameTypes()
		{
			var expected = new HashSet<string> { "foo", "bar" };
			var actual = new HashSet<string> { "foo", "baz" };

			Assert.NotEqual(expected, actual);
			Assert.NotEqual(expected, (ISet<string>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void ISet_DifferentTypes()
		{
			var expected = new HashSet<string> { "bar", "foo" };
			var actual = new SortedSet<string> { "foo", "baz" };

			Assert.NotEqual(expected, (ISet<string>)actual);
			Assert.NotEqual(expected, (object)actual);
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.NotEqual("actual", "actual"));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				@"Assert.NotEqual() Failure" + Environment.NewLine +
				@"Expected: Not ""actual""" + Environment.NewLine +
				@"Actual:   ""actual""",
				ex.Message
			);
		}
	}

	public class NotEqual_WithComparer
	{
		[Fact]
		public void Success()
		{
			Assert.NotEqual("TestString", "testString", StringComparer.InvariantCulture);
		}

		[Fact]
		public void NotEqualWithCustomComparer()
		{
			var ex = Record.Exception(
				() => Assert.NotEqual("TestString", "testString", StringComparer.InvariantCultureIgnoreCase));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				@"Assert.NotEqual() Failure" + Environment.NewLine +
				@"Expected: Not ""TestString""" + Environment.NewLine +
				@"Actual:   ""testString""",
				ex.Message
			);
		}
	}

	public class NotEqual_Decimal
	{
		[Fact]
		public void Success()
		{
			Assert.NotEqual(0.11111M, 0.11444M, 3);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<NotEqualException>(() => Assert.NotEqual(0.11111M, 0.11444M, 2));

			Assert.Equal(
				"Assert.NotEqual() Failure" + Environment.NewLine +
				$"Expected: Not {0.11M} (rounded from {0.11111})" + Environment.NewLine +
				$"Actual:   {0.11M} (rounded from {0.11444})",
				ex.Message
			);
		}
	}

	public class NotEqual_Double
	{
		[Fact]
		public void Success()
		{
			Assert.NotEqual(0.11111, 0.11444, 3);
		}

		[CulturedFact]
		public void Failure()
		{
			var ex = Assert.Throws<NotEqualException>(() => Assert.NotEqual(0.11111, 0.11444, 2));

			Assert.Equal(
				"Assert.NotEqual() Failure" + Environment.NewLine +
				$"Expected: Not {0.11M} (rounded from {0.11111})" + Environment.NewLine +
				$"Actual:   {0.11M} (rounded from {0.11444})",
				ex.Message
			);
		}
	}

	public class NotStrictEqual
	{
		[Fact]
		public static void Success()
		{
			Assert.NotStrictEqual("bob", "jim");
		}

		[Fact]
		public static void Equals()
		{
			Assert.NotStrictEqual(new EnumerableClass("ploeh"), new EnumerableClass("fnaah"));
		}

		[Fact]
		public static void Failure()
		{
			var ex = Record.Exception(() => Assert.NotStrictEqual("actual", "actual"));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				@"Assert.NotEqual() Failure" + Environment.NewLine +
				@"Expected: Not ""actual""" + Environment.NewLine +
				@"Actual:   ""actual""",
				ex.Message
			);
		}

		[Fact]
		public static void Collection()
		{
			var ex = Assert.Throws<NotEqualException>(() => Assert.NotStrictEqual(new DerivedClass(), new BaseClass()));
			Assert.Equal(
				@"Assert.NotEqual() Failure" + Environment.NewLine +
				@"Expected: Not DerivedClass { }" + Environment.NewLine +
				@"Actual:   BaseClass { }",
				ex.Message
			);
		}
	}

	private class BaseClass { }

	private class DerivedClass : BaseClass
	{
		public override bool Equals(object? obj) =>
			obj is BaseClass || base.Equals(obj);

		public override int GetHashCode() => 0;
	}

	private class EnumerableClass : IEnumerable<BaseClass>
	{
		private readonly IEnumerable<BaseClass> bars;

		public EnumerableClass(string _, params BaseClass[] bars)
		{
			this.bars = bars;
		}

		public IEnumerator<BaseClass> GetEnumerator() => bars.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
