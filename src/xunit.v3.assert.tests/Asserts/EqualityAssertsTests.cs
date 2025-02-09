using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Sdk;

public class EqualityAssertsTests
{
	public class Equal
	{
		public class ReferenceEquality
		{
			// https://github.com/xunit/xunit/issues/2271
			[Fact]
			public void TwoIdenticalReferencesShouldBeEqual()
			{
				Field x = new Field();

				Assert.Equal(x, x);
			}

			sealed class Field : IReadOnlyList<Field>
			{
				Field IReadOnlyList<Field>.this[int index]
				{
					get
					{
						if (index != 0)
							throw new ArgumentOutOfRangeException(nameof(index));

						return this;
					}
				}

				int IReadOnlyCollection<Field>.Count => 1;

				IEnumerator<Field> IEnumerable<Field>.GetEnumerator()
				{
					yield return this;
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					yield return this;
				}
			}
		}

		public class Intrinsics
		{
			[Fact]
			public void Equal()
			{
				Assert.Equal(42, 42);
			}

			[Fact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(42, 2112));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: 42" + Environment.NewLine +
					"Actual:   2112",
					ex.Message
				);
			}

			[Fact]
			public void StringsPassViaObjectEqualAreNotFormattedOrTruncated()
			{
				var ex = Record.Exception(
					() => Assert.Equal<object>(
						$"This is a long{Environment.NewLine}string with{Environment.NewLine}new lines",
						$"This is a long{Environment.NewLine}string with embedded{Environment.NewLine}new lines"
					)
				);

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: This is a long" + Environment.NewLine +
					"          string with" + Environment.NewLine +
					"          new lines" + Environment.NewLine +
					"Actual:   This is a long" + Environment.NewLine +
					"          string with embedded" + Environment.NewLine +
					"          new lines",
					ex.Message
				);
			}
		}

		public class WithComparer
		{
			[Fact]
			public void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("comparer", () => Assert.Equal(1, 2, default(IEqualityComparer<int>)!));
			}

			[Fact]
			public void Equal()
			{
				Assert.Equal(42, 21, new Comparer<int>(true));
			}

			[Fact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(42, 42, new Comparer<int>(false)));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: 42" + Environment.NewLine +
					"Actual:   42",
					ex.Message
				);
			}

			class Comparer<T>(bool result) :
				IEqualityComparer<T>
			{
				readonly bool result = result;

				public bool Equals(T? x, T? y) => result;

				public int GetHashCode(T obj) => throw new NotImplementedException();
			}

			[Fact]
			public void NonEnumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.Equal(42, 2112, new ThrowingIntComparer()));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: 42" + Environment.NewLine +
					"Actual:   2112",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			public class ThrowingIntComparer : IEqualityComparer<int>
			{
				public bool Equals(int x, int y) =>
					throw new DivideByZeroException();
				public int GetHashCode(int obj) =>
					throw new NotImplementedException();
			}

			[Fact]
			public void Enumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.Equal([1, 2], [1, 3], new ThrowingEnumerableComparer()));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: [1, 2]" + Environment.NewLine +
					"Actual:   [1, 3]",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			public class ThrowingEnumerableComparer : IEqualityComparer<IEnumerable<int>>
			{
				public bool Equals(IEnumerable<int>? x, IEnumerable<int>? y) =>
					throw new DivideByZeroException();
				public int GetHashCode(IEnumerable<int> obj) =>
					throw new NotImplementedException();
			}
		}

		public class WithFunc
		{
			[Fact]
			public void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("comparer", () => Assert.Equal(1, 2, default(Func<int, int, bool>)!));
			}

			[Fact]
			public void Equal()
			{
				Assert.Equal(42, 21, (x, y) => true);
			}

			[Fact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(42, 42, (x, y) => false));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: 42" + Environment.NewLine +
					"Actual:   42",
					ex.Message
				);
			}

			[Fact]
			public void NonEnumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.Equal(42, 2112, (e, a) => throw new DivideByZeroException()));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: 42" + Environment.NewLine +
					"Actual:   2112",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			[Fact]
			public void Enumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(
					() => Assert.Equal(
						[1, 2],
						[1, 3],
						(IEnumerable<int> e, IEnumerable<int> a) => throw new DivideByZeroException()
					)
				);

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: [1, 2]" + Environment.NewLine +
					"Actual:   [1, 3]",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}
		}

		public class Comparable
		{
			[Fact]
			public void Equal()
			{
				var obj1 = new SpyComparable(0);
				var obj2 = new SpyComparable(0);

				Assert.Equal(obj1, obj2);
				Assert.True(obj1.CompareCalled);
			}

			[Fact]
			public void NotEqual()
			{
				var obj1 = new SpyComparable(-1);
				var obj2 = new SpyComparable(0);

				var ex = Record.Exception(() => Assert.Equal(obj1, obj2));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: SpyComparable { CompareCalled = True }" + Environment.NewLine +
					"Actual:   SpyComparable { CompareCalled = False }",
					ex.Message
				);
			}

			[Fact]
			public void NonGeneric_SameType_Equal()
			{
				var expected = new MultiComparable(1);
				var actual = new MultiComparable(1);

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (IComparable)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void NonGeneric_SameType_NotEqual()
			{
				var expected = new MultiComparable(1);
				var actual = new MultiComparable(2);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Values differ" + Environment.NewLine +
						"Expected: MultiComparable { Value = 1 }" + Environment.NewLine +
						"Actual:   MultiComparable { Value = 2 }",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (IComparable)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void NonGeneric_DifferentType_Equal()
			{
				var expected = new MultiComparable(1);
				var actual = 1;

				Assert.Equal(expected, (IComparable)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void NonGeneric_DifferentType_NotEqual()
			{
				var expected = new MultiComparable(1);
				var actual = 2;

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Values differ" + Environment.NewLine +
						"Expected: MultiComparable { Value = 1 }" + Environment.NewLine +
						"Actual:   2",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, (IComparable)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void Generic_Equal()
			{
				var obj1 = new SpyComparable_Generic();
				var obj2 = new SpyComparable_Generic();

				Assert.Equal(obj1, obj2);
				Assert.True(obj1.CompareCalled);
			}

			[Fact]
			public void Generic_NotEqual()
			{
				var obj1 = new SpyComparable_Generic(-1);
				var obj2 = new SpyComparable_Generic();

				var ex = Record.Exception(() => Assert.Equal(obj1, obj2));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: SpyComparable_Generic { CompareCalled = True }" + Environment.NewLine +
					"Actual:   SpyComparable_Generic { CompareCalled = False }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_SubClass_Equal()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableSubClassB(1);

				Assert.Equal<ComparableBaseClass>(expected, actual);
			}

			[Fact]
			public void SubClass_SubClass_NotEqual()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableSubClassB(2);

				var ex = Record.Exception(() => Assert.Equal<ComparableBaseClass>(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ComparableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:   ComparableSubClassB { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void BaseClass_SubClass_Equal()
			{
				var expected = new ComparableBaseClass(1);
				var actual = new ComparableSubClassA(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void BaseClass_SubClass_NotEqual()
			{
				var expected = new ComparableBaseClass(1);
				var actual = new ComparableSubClassA(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ComparableBaseClass { Value = 1 }" + Environment.NewLine +
					"Actual:   ComparableSubClassA { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_BaseClass_Equal()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableBaseClass(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void SubClass_BaseClass_NotEqual()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableBaseClass(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ComparableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:   ComparableBaseClass { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void Generic_ThrowsException_Equal()
			{
				var expected = new ComparableThrower(1);
				var actual = new ComparableThrower(1);

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (IComparable<ComparableThrower>)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void Generic_ThrowsException_NotEqual()
			{
				var expected = new ComparableThrower(1);
				var actual = new ComparableThrower(2);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Values differ" + Environment.NewLine +
						"Expected: ComparableThrower { Value = 1 }" + Environment.NewLine +
						"Actual:   ComparableThrower { Value = 2 }",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (IComparable<ComparableThrower>)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_Equal()
			{
				object expected = new ImplicitIComparableExpected(1);
				object actual = new IntWrapper(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_NotEqual()
			{
				object expected = new ImplicitIComparableExpected(1);
				object actual = new IntWrapper(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ImplicitIComparableExpected { Value = 1 }" + Environment.NewLine +
					"Actual:   IntWrapper { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_Equal()
			{
				object expected = new ExplicitIComparableActual(1);
				object actual = new IntWrapper(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_NotEqual()
			{
				object expected = new ExplicitIComparableActual(1);
				object actual = new IntWrapper(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ExplicitIComparableActual { Value = 1 }" + Environment.NewLine +
					"Actual:   IntWrapper { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_Throws_Equal()
			{
				object expected = new IComparableActualThrower(1);
				object actual = new IntWrapper(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void DifferentTypes_Throws_NotEqual()
			{
				object expected = new IComparableActualThrower(1);
				object actual = new IntWrapper(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: IComparableActualThrower { Value = 1 }" + Environment.NewLine +
					"Actual:   IntWrapper { Value = 2 }",
					ex.Message
				);
			}
		}

		public class NotComparable
		{
			[Fact]
			public void Equal()
			{
				var nco1 = new NonComparableObject();
				var nco2 = new NonComparableObject();

				Assert.Equal(nco1, nco2);
			}

			[Fact]
			public void NotEqual()
			{
				var nco1 = new NonComparableObject(false);
				var nco2 = new NonComparableObject();

				var ex = Record.Exception(() => Assert.Equal(nco1, nco2));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: NonComparableObject { }" + Environment.NewLine +
					"Actual:   NonComparableObject { }",
					ex.Message
				);
			}
		}

		public class Equatable
		{
			[Fact]
			public void Equal()
			{
				var obj1 = new SpyEquatable();
				var obj2 = new SpyEquatable();

				Assert.Equal(obj1, obj2);

				Assert.True(obj1.Equals__Called);
				Assert.Same(obj2, obj1.Equals_Other);
			}

			[Fact]
			public void NotEqual()
			{
				var obj1 = new SpyEquatable(false);
				var obj2 = new SpyEquatable();

				var ex = Record.Exception(() => Assert.Equal(obj1, obj2));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: SpyEquatable { Equals__Called = True, Equals_Other = SpyEquatable { Equals__Called = False, Equals_Other = null } }" + Environment.NewLine +
					"Actual:   SpyEquatable { Equals__Called = False, Equals_Other = null }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_SubClass_Equal()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableSubClassB(1);

				Assert.Equal<EquatableBaseClass>(expected, actual);
			}

			[Fact]
			public void SubClass_SubClass_NotEqual()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableSubClassB(2);

				var ex = Record.Exception(() => Assert.Equal<EquatableBaseClass>(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: EquatableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:   EquatableSubClassB { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void BaseClass_SubClass_Equal()
			{
				var expected = new EquatableBaseClass(1);
				var actual = new EquatableSubClassA(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void BaseClass_SubClass_NotEqual()
			{
				var expected = new EquatableBaseClass(1);
				var actual = new EquatableSubClassA(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: EquatableBaseClass { Value = 1 }" + Environment.NewLine +
					"Actual:   EquatableSubClassA { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_BaseClass_Equal()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableBaseClass(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void SubClass_BaseClass_NotEqual()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableBaseClass(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: EquatableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:   EquatableBaseClass { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_Equal()
			{
				object expected = new ImplicitIEquatableExpected(1);
				object actual = new IntWrapper(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_NotEqual()
			{
				object expected = new ImplicitIEquatableExpected(1);
				object actual = new IntWrapper(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ImplicitIEquatableExpected { Value = 1 }" + Environment.NewLine +
					"Actual:   IntWrapper { Value = 2 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_Equal()
			{
				object expected = new ExplicitIEquatableExpected(1);
				object actual = new IntWrapper(1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_NotEqual()
			{
				object expected = new ExplicitIEquatableExpected(1);
				object actual = new IntWrapper(2);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: ExplicitIEquatableExpected { Value = 1 }" + Environment.NewLine +
					"Actual:   IntWrapper { Value = 2 }",
					ex.Message
				);
			}
		}

		public class StructuralEquatable
		{
			[Fact]
			public void Equal()
			{
				var expected = new Tuple<StringWrapper>(new StringWrapper("a"));
				var actual = new Tuple<StringWrapper>(new StringWrapper("a"));

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (IStructuralEquatable)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void NotEqual()
			{
				var expected = new Tuple<StringWrapper>(new StringWrapper("a"));
				var actual = new Tuple<StringWrapper>(new StringWrapper("b"));

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Values differ" + Environment.NewLine +
						"Expected: Tuple (StringWrapper { Value = \"a\" })" + Environment.NewLine +
						"Actual:   Tuple (StringWrapper { Value = \"b\" })",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (IStructuralEquatable)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void ExpectedNull_ActualNull()
			{
				var expected = new Tuple<StringWrapper?>(null);
				var actual = new Tuple<StringWrapper?>(null);

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (IStructuralEquatable)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void ExpectedNull_ActualNonNull()
			{
				var expected = new Tuple<StringWrapper?>(null);
				var actual = new Tuple<StringWrapper?>(new StringWrapper("a"));

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Values differ" + Environment.NewLine +
						"Expected: Tuple (null)" + Environment.NewLine +
						"Actual:   Tuple (StringWrapper { Value = \"a\" })",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (IStructuralEquatable)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void _ExpectedNonNull_ActualNull()
			{
				var expected = new Tuple<StringWrapper?>(new StringWrapper("a"));
				var actual = new Tuple<StringWrapper?>(null);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Values differ" + Environment.NewLine +
						"Expected: Tuple (StringWrapper { Value = \"a\" })" + Environment.NewLine +
						"Actual:   Tuple (null)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (IStructuralEquatable)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}
		}

		public class Collections
		{
			[Fact]
			public void IReadOnlyCollection_IEnumerable_Equal()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new ReadOnlyCollection<string>(expected);

				Assert.Equal(expected, (IReadOnlyCollection<string>)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void IReadOnlyCollection_IEnumerable_NotEqual()
			{
				var expected = new string[] { @"C:\Program Files (x86)\Common Files\Extremely Long Path Name\VST2" };
				var actual = new ReadOnlyCollection<string>([@"C:\Program Files (x86)\Common Files\Extremely Long Path Name\VST3"]);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ at index 0" + Environment.NewLine +
						"                                                                  ↓ (pos 64)" + Environment.NewLine +
						"Expected: ···\"s (x86)\\\\Common Files\\\\Extremely Long Path Name\\\\VST2\"" + Environment.NewLine +
						"Actual:   ···\"s (x86)\\\\Common Files\\\\Extremely Long Path Name\\\\VST3\"" + Environment.NewLine +
						"                                                                  ↑ (pos 64)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, (IReadOnlyCollection<string>)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void CollectionDepth_Equal()
			{
				var x = new List<object> { new List<object> { new List<object> { 1 } } };
				var y = new List<object> { new List<object> { new List<object> { 1 } } };

				Assert.Equal(x, y);
			}

			[Fact]
			public void CollectionDepth_NotEqual()
			{
				var x = new List<object> { new List<object> { new List<object> { 1 } } };
				var y = new List<object> { new List<object> { new List<object> { 2 } } };

				var ex = Record.Exception(() => Assert.Equal(x, y));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"           ↓ (pos 0)" + Environment.NewLine +
					"Expected: [[[1]]]" + Environment.NewLine +
					"Actual:   [[[2]]]" + Environment.NewLine +
					"           ↑ (pos 0)",
					ex.Message
				);
			}

			[Fact]
			public void StringArray_ObjectArray_Equal()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new object[] { "foo", "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void StringArray_ObjectArray_NotEqual()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new object[] { "foo", "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ at index 1" + Environment.NewLine +
						"             ↓ (pos 2)" + Environment.NewLine +
						"Expected: \"bar\"" + Environment.NewLine +
						"Actual:   \"baz\"" + Environment.NewLine +
						"             ↑ (pos 2)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void MultidimensionalArrays_Equal()
			{
				var expected = new int[,] { { 1 }, { 2 } };
				var actual = new int[,] { { 1 }, { 2 } };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void MultidimensionalArrays_NotEqual()
			{
				var expected = new int[,] { { 1, 2 } };
				var actual = new int[,] { { 1 }, { 2 } };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				// TODO: Would be better to have formatting that preserves the ranks instead of
				// flattening, which happens because multi-dimensional arrays enumerate flatly
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"Expected: [1, 2]" + Environment.NewLine +
					"Actual:   [1, 2]",
					ex.Message
				);
			}

			[Fact]
			public void NonZeroBoundedArrays_Equal()
			{
				var expected = Array.CreateInstance(typeof(int), [1], [1]);
				expected.SetValue(42, 1);
				var actual = Array.CreateInstance(typeof(int), [1], [1]);
				actual.SetValue(42, 1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void NonZeroBoundedArrays_NotEqual()
			{
				var expected = Array.CreateInstance(typeof(int), [1], [1]);
				expected.SetValue(42, 1);
				var actual = Array.CreateInstance(typeof(int), [1], [0]);
				actual.SetValue(42, 0);

				var ex = Record.Exception(() => Assert.Equal(expected, (object)actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"Expected: int[*] [42]" + Environment.NewLine +
					"Actual:   int[]  [42]",
					ex.Message
				);
			}

			[Fact]
			public void PrintPointersWithCompatibleComparers()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new[] { 1, 2, 0, 4, 5 };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"                 ↓ (pos 2)" + Environment.NewLine +
						"Expected: [1, 2, 3, 4, 5]" + Environment.NewLine +
						"Actual:   [1, 2, 0, 4, 5]" + Environment.NewLine +
						"                 ↑ (pos 2)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, actual, EqualityComparer<IEnumerable<int>>.Default));
			}

			[Fact]
			public void CustomComparerWithSafeEnumerable()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new[] { 1, 2, 0, 4, 5 };

				var ex = Record.Exception(() => Assert.Equal(expected, actual, new MyComparer()));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"Expected: [1, 2, 3, 4, 5]" + Environment.NewLine +
					"Actual:   [1, 2, 0, 4, 5]",
					ex.Message
				);
			}

			[Fact]
			public void CustomComparerWithUnsafeEnumerable()
			{
				var ex = Record.Exception(() => Assert.Equal(new UnsafeEnumerable(), new[] { 1, 2, 3 }, new MyComparer()));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					$"Expected: UnsafeEnumerable [{ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					"Actual:   int[]            [1, 2, 3]",
					ex.Message
				);
			}

			class UnsafeEnumerable : IEnumerable
			{
				public IEnumerator GetEnumerator()
				{
					while (true)
						yield return 1;
				}
			}

			class MyComparer : IEqualityComparer<IEnumerable>
			{
				public bool Equals(IEnumerable? x, IEnumerable? y)
					=> false;

				public int GetHashCode([DisallowNull] IEnumerable obj) =>
					throw new NotImplementedException();
			}

			[Fact]
			public void CollectionWithIEquatable_Equal()
			{
				var expected = new EnumerableEquatable<int> { 42, 2112 };
				var actual = new EnumerableEquatable<int> { 2112, 42 };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void CollectionWithIEquatable_NotEqual()
			{
				var expected = new EnumerableEquatable<int> { 42, 2112 };
				var actual = new EnumerableEquatable<int> { 2112, 2600 };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				// No pointers because it's relying on IEquatable<>
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"Expected: [42, 2112]" + Environment.NewLine +
					"Actual:   [2112, 2600]",
					ex.Message
				);
			}

			public sealed class EnumerableEquatable<T> :
				IEnumerable<T>, IEquatable<EnumerableEquatable<T>>
			{
				readonly List<T> values = [];

				public void Add(T value) => values.Add(value);

				public bool Equals(EnumerableEquatable<T>? other)
				{
					if (other == null)
						return false;

					return !values.Except(other.values).Any() && !other.values.Except(values).Any();
				}

				public IEnumerator<T> GetEnumerator() => values.GetEnumerator();

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}
		}

		public class Dictionaries
		{
			[Fact]
			public void SameTypes_Equal()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new Dictionary<string, string> { ["foo"] = "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (IDictionary)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void SameTypes_NotEqual()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new Dictionary<string, string> { ["foo"] = "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Dictionaries differ" + Environment.NewLine +
						"Expected: [[\"foo\"] = \"bar\"]" + Environment.NewLine +
						"Actual:   [[\"foo\"] = \"baz\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (IDictionary)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new ConcurrentDictionary<string, string>(expected);

				Assert.Equal(expected, (IDictionary)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new ConcurrentDictionary<string, string> { ["foo"] = "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"Expected: Dictionary<string, string>           [[\"foo\"] = \"bar\"]" + Environment.NewLine +
						"Actual:   ConcurrentDictionary<string, string> [[\"foo\"] = \"baz\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, (IDictionary)actual));
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void NullValue_Equal()
			{
				var expected = new Dictionary<string, int?> { { "two", null } };
				var actual = new Dictionary<string, int?> { { "two", null } };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void NullValue_NotEqual()
			{
				var expected = new Dictionary<string, int?> { { "two", null } };
				var actual = new Dictionary<string, int?> { { "two", 1 } };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Dictionaries differ" + Environment.NewLine +
					"Expected: [[\"two\"] = null]" + Environment.NewLine +
					"Actual:   [[\"two\"] = 1]",
					ex.Message
				);
			}
		}

		public class HashSets
		{
			[Fact]
			public static void InOrder_Equal()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 1, 2, 3 };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void InOrder_NotEqual()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 1, 2, 4 };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: HashSets differ" + Environment.NewLine +
					"Expected: [1, 2, 3]" + Environment.NewLine +
					"Actual:   [1, 2, 4]",
					ex.Message
				);
			}

			[Fact]
			public static void OutOfOrder_Equal()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 2, 3, 1 };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void OutOfOrder_NotEqual()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 2, 4, 1 };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: HashSets differ" + Environment.NewLine +
					"Expected: [1, 2, 3]" + Environment.NewLine +
					"Actual:   [2, 4, 1]",
					ex.Message
				);
			}

			[Fact]
			public static void ExpectedLarger()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 1, 2 };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: HashSets differ" + Environment.NewLine +
					"Expected: [1, 2, 3]" + Environment.NewLine +
					"Actual:   [1, 2]",
					ex.Message
				);
			}

			[Fact]
			public static void ActualLarger()
			{
				var expected = new HashSet<int> { 1, 2 };
				var actual = new HashSet<int> { 1, 2, 3 };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: HashSets differ" + Environment.NewLine +
					"Expected: [1, 2]" + Environment.NewLine +
					"Actual:   [1, 2, 3]",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new HashSet<string> { "bar", "foo" };
				var actual = new SortedSet<string> { "foo", "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.Equal(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				object expected = new HashSet<int> { 42 };
				object actual = new HashSet<long> { 42L };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: HashSets differ" + Environment.NewLine +
					"Expected: HashSet<int>  [42]" + Environment.NewLine +
					"Actual:   HashSet<long> [42]",
					ex.Message
				);
			}

			[Fact]
			public void ComparerFunc_Throws()
			{
				var expected = new HashSet<string> { "bar" };
				var actual = new HashSet<string> { "baz" };

#pragma warning disable xUnit2026 // Comparison of sets must be done with IEqualityComparer
				var ex = Record.Exception(() => Assert.Equal(expected, actual, (string l, string r) => true));
#pragma warning restore xUnit2026 // Comparison of sets must be done with IEqualityComparer

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: During comparison of two collections, GetHashCode was called, but only a comparison function was provided. This typically indicates trying to compare two sets with an item comparison function, which is not supported. For more information, see https://xunit.net/docs/hash-sets-vs-linear-containers",
					ex.Message
				);
			}
		}

		public class Sets
		{
			[Fact]
			public void InOrder_Equal()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "bar", "foo" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.Equal(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void InOrder_NotEqual()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "bar", "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Sets differ" + Environment.NewLine +
						"Expected: [\"bar\", \"foo\"]" + Environment.NewLine +
						"Actual:   [\"bar\", \"baz\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.Equal(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void OutOfOrder_Equal()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "foo", "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.Equal(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void OutOfOrder_NotEqual()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "foo", "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Sets differ" + Environment.NewLine +
						"Expected: [\"bar\", \"foo\"]" + Environment.NewLine +
						"Actual:   [\"foo\", \"baz\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.Equal(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentContents()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new NonGenericSet { "bar", "foo" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Sets differ" + Environment.NewLine +
						"Expected: [\"bar\"]" + Environment.NewLine +
						"Actual:   [\"bar\", \"foo\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.Equal(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.Equal(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Sets differ" + Environment.NewLine +
						"Expected: NonGenericSet   [\"bar\"]" + Environment.NewLine +
						"Actual:   HashSet<string> [\"baz\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.Equal(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void TwoGenericSubClass_Equal()
			{
				var expected = new TwoGenericSet<string, int> { "foo", "bar" };
				var actual = new TwoGenericSet<string, int> { "foo", "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.Equal(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void TwoGenericSubClass_NotEqual()
			{
				var expected = new TwoGenericSet<string, int> { "foo", "bar" };
				var actual = new TwoGenericSet<string, int> { "foo", "baz" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Sets differ" + Environment.NewLine +
						"Expected: [\"foo\", \"bar\"]" + Environment.NewLine +
						"Actual:   [\"foo\", \"baz\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected, actual));
				assertFailure(() => Assert.Equal(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.Equal(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void ComparerFunc_Throws()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "baz" };

#pragma warning disable xUnit2026 // Comparison of sets must be done with IEqualityComparer
				var ex = Record.Exception(() => Assert.Equal(expected, actual, (string l, string r) => true));
#pragma warning restore xUnit2026 // Comparison of sets must be done with IEqualityComparer

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: During comparison of two collections, GetHashCode was called, but only a comparison function was provided. This typically indicates trying to compare two sets with an item comparison function, which is not supported. For more information, see https://xunit.net/docs/hash-sets-vs-linear-containers",
					ex.Message
				);
			}
		}

		// https://github.com/xunit/xunit/issues/3137
		public class ImmutableArrays
		{
			[Fact]
			public void Equal()
			{
				var expected = new[] { 1, 2, 3 }.ToImmutableArray();
				var actual = new[] { 1, 2, 3 }.ToImmutableArray();

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void NotEqual()
			{
				var expected = new[] { 1, 2, 3 }.ToImmutableArray();
				var actual = new[] { 1, 2, 4 }.ToImmutableArray();

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"                 ↓ (pos 2)" + Environment.NewLine +
					"Expected: [1, 2, 3]" + Environment.NewLine +
					"Actual:   [1, 2, 4]" + Environment.NewLine +
					"                 ↑ (pos 2)",
					ex.Message
				);
			}
		}

		public class KeyValuePair
		{
			[Fact]
			public void CollectionKeys_Equal()
			{
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				// Different concrete collection types in the key slot, per https://github.com/xunit/xunit/issues/2850
				var expected = new KeyValuePair<IEnumerable<string>, int>(new List<string> { "Key1", "Key2" }, 42);
				var actual = new KeyValuePair<IEnumerable<string>, int>(new string[] { "Key1", "Key2" }, 42);
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void CollectionKeys_NotEqual()
			{
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				// Different concrete collection types in the key slot, per https://github.com/xunit/xunit/issues/2850
				var expected = new KeyValuePair<IEnumerable<string>, int>(new List<string> { "Key1", "Key2" }, 42);
				var actual = new KeyValuePair<IEnumerable<string>, int>(new string[] { "Key1", "Key3" }, 42);
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: [[\"Key1\", \"Key2\"]] = 42" + Environment.NewLine +
					"Actual:   [[\"Key1\", \"Key3\"]] = 42",
					ex.Message
				);
			}

			[Fact]
			public void CollectionValues_Equal()
			{
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				// Different concrete collection types in the value slot, per https://github.com/xunit/xunit/issues/2850
				var expected = new KeyValuePair<string, IEnumerable<string>>("Key1", new List<string> { "Value1a", "Value1b" });
				var actual = new KeyValuePair<string, IEnumerable<string>>("Key1", new string[] { "Value1a", "Value1b" });
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void CollectionValues_NotEqual()
			{
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				// Different concrete collection types in the value slot, per https://github.com/xunit/xunit/issues/2850
				var expected = new KeyValuePair<string, IEnumerable<string>>("Key1", new List<string> { "Value1a", "Value1b" });
				var actual = new KeyValuePair<string, IEnumerable<string>>("Key1", new string[] { "Value1a", "Value2a" });
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: [\"Key1\"] = [\"Value1a\", \"Value1b\"]" + Environment.NewLine +
					"Actual:   [\"Key1\"] = [\"Value1a\", \"Value2a\"]",
					ex.Message
				);
			}

			[Fact]
			public void EquatableKeys_Equal()
			{
				var expected = new KeyValuePair<EquatableObject, int>(new() { Char = 'a' }, 42);
				var actual = new KeyValuePair<EquatableObject, int>(new() { Char = 'a' }, 42);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void EquatableKeys_NotEqual()
			{
				var expected = new KeyValuePair<EquatableObject, int>(new() { Char = 'a' }, 42);
				var actual = new KeyValuePair<EquatableObject, int>(new() { Char = 'b' }, 42);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: [EquatableObject { Char = 'a' }] = 42" + Environment.NewLine +
					"Actual:   [EquatableObject { Char = 'b' }] = 42",
					ex.Message
				);
			}

			[Fact]
			public void EquatableValues_Equal()
			{
				var expected = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'a' });
				var actual = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'a' });

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void EquatableValues_NotEqual()
			{
				var expected = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'a' });
				var actual = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'b' });

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					"Expected: [\"Key1\"] = EquatableObject { Char = 'a' }" + Environment.NewLine +
					"Actual:   [\"Key1\"] = EquatableObject { Char = 'b' }",
					ex.Message
				);
			}

			public class EquatableObject : IEquatable<EquatableObject>
			{
				public char Char { get; set; }

				public bool Equals(EquatableObject? other) =>
					other != null && other.Char == Char;
			}
		}

		public class DoubleEnumerationPrevention
		{
			[Fact]
			public static void EnumeratesOnlyOnce_Equal()
			{
				var expected = new RunOnceEnumerable<int>([1, 2, 3, 4, 5]);
				var actual = new RunOnceEnumerable<int>([1, 2, 3, 4, 5]);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void EnumeratesOnlyOnce_NotEqual()
			{
				var expected = new RunOnceEnumerable<int>([1, 2, 3, 4, 5]);
				var actual = new RunOnceEnumerable<int>([1, 2, 3, 4, 5, 6]);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					$"Expected: [{ArgumentFormatter.Ellipsis}, 2, 3, 4, 5]" + Environment.NewLine +
					$"Actual:   [{ArgumentFormatter.Ellipsis}, 2, 3, 4, 5, 6]" + Environment.NewLine +
					"                            ↑ (pos 5)",
					ex.Message
				);
			}
		}
	}

	public class Equal_DateTime
	{
		public class WithoutPrecision
		{
			[Fact]
			public void Equal()
			{
				var expected = new DateTime(2023, 2, 11, 15, 4, 0);
				var actual = new DateTime(2023, 2, 11, 15, 4, 0);

				Assert.Equal(expected, actual);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var expected = new DateTime(2023, 2, 11, 15, 4, 0);
				var actual = new DateTime(2023, 2, 11, 15, 5, 0);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(expected)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(actual)}",
					ex.Message
				);
			}
		}

		public class WithPrecision
		{
			[Fact]
			public void InRange()
			{
				var date1 = new DateTime(2023, 2, 11, 15, 4, 0);
				var date2 = new DateTime(2023, 2, 11, 15, 5, 0);
				var precision = TimeSpan.FromMinutes(1);

				Assert.Equal(date1, date2, precision);  // expected earlier than actual
				Assert.Equal(date2, date1, precision);  // expected later than actual
			}

			[CulturedFact]
			public void OutOfRange()
			{
				var date1 = new DateTime(2023, 2, 11, 15, 4, 0);
				var date2 = new DateTime(2023, 2, 11, 15, 6, 0);
				var precision = TimeSpan.FromMinutes(1);
				var difference = TimeSpan.FromMinutes(2);

				// expected earlier than actual
				var ex = Record.Exception(() => Assert.Equal(date1, date2, precision));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(date1)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(date2)} (difference {difference} is larger than {precision})",
					ex.Message
				);

				// expected later than actual
				var ex2 = Record.Exception(() => Assert.Equal(date2, date1, precision));

				Assert.IsType<EqualException>(ex2);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(date2)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(date1)} (difference {difference} is larger than {precision})",
					ex2.Message
				);
			}
		}
	}

	public class Equal_DateTimeOffset
	{
		public class WithoutPrecision_SameTimeZone
		{
			[Fact]
			public void Equal()
			{
				var expected = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var actual = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);

				Assert.Equal(expected, actual);
				Assert.Equal(expected, actual);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var expected = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var actual = new DateTimeOffset(2023, 2, 11, 15, 5, 0, TimeSpan.Zero);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(expected)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(actual)}",
					ex.Message
				);
			}
		}

		public class WithoutPrecision_DifferentTimeZone
		{
			[Fact]
			public void Equal()
			{
				var expected = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var actual = new DateTimeOffset(2023, 2, 11, 16, 4, 0, TimeSpan.FromHours(1));

				Assert.Equal(expected, actual);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var expected = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var actual = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.FromHours(1));

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(expected)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(actual)}",
					ex.Message
				);
			}
		}

		public class WithPrecision_SameTimeZone
		{
			[Fact]
			public void InRange()
			{
				var date1 = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var date2 = new DateTimeOffset(2023, 2, 11, 15, 5, 0, TimeSpan.Zero);
				var precision = TimeSpan.FromMinutes(1);

				Assert.Equal(date1, date2, precision);  // expected earlier than actual
				Assert.Equal(date2, date1, precision);  // expected later than actual
			}

			[CulturedFact]
			public void OutOfRange()
			{
				var date1 = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var date2 = new DateTimeOffset(2023, 2, 11, 15, 6, 0, TimeSpan.Zero);
				var precision = TimeSpan.FromMinutes(1);
				var difference = TimeSpan.FromMinutes(2);

				// expected earlier than actual
				var ex = Record.Exception(() => Assert.Equal(date1, date2, precision));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(date1)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(date2)} (difference {difference} is larger than {precision})",
					ex.Message
				);

				// expected later than actual
				var ex2 = Record.Exception(() => Assert.Equal(date2, date1, precision));

				Assert.IsType<EqualException>(ex2);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(date2)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(date1)} (difference {difference} is larger than {precision})",
					ex2.Message
				);
			}
		}

		public class WithPrecision_DifferentTimeZone
		{
			[Fact]
			public void InRange()
			{
				var date1 = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var date2 = new DateTimeOffset(2023, 2, 11, 16, 5, 0, TimeSpan.FromHours(1));
				var precision = TimeSpan.FromMinutes(1);

				Assert.Equal(date1, date2, precision);  // expected earlier than actual
				Assert.Equal(date2, date1, precision);  // expected later than actual
			}

			[CulturedFact]
			public void OutOfRange()
			{
				var date1 = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.Zero);
				var date2 = new DateTimeOffset(2023, 2, 11, 15, 4, 0, TimeSpan.FromHours(1));
				var precision = TimeSpan.FromMinutes(1);
				var difference = TimeSpan.FromHours(1);

				// expected earlier than actual
				var ex = Record.Exception(() => Assert.Equal(date1, date2, precision));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(date1)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(date2)} (difference {difference} is larger than {precision})",
					ex.Message
				);

				// expected later than actual
				var ex2 = Record.Exception(() => Assert.Equal(date2, date1, precision));

				Assert.IsType<EqualException>(ex2);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter.Format(date2)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter.Format(date1)} (difference {difference} is larger than {precision})",
					ex2.Message
				);
			}
		}
	}

	public class Equal_Decimal
	{
		[Fact]
		public void Equal()
		{
			Assert.Equal(0.11111M, 0.11444M, 2);
		}

		[CulturedFact]
		public void NotEqual()
		{
			var ex = Record.Exception(() => Assert.Equal(0.11111M, 0.11444M, 3));

			Assert.IsType<EqualException>(ex);
			Assert.Equal(
				"Assert.Equal() Failure: Values differ" + Environment.NewLine +
				$"Expected: {0.111M} (rounded from {0.11111M})" + Environment.NewLine +
				$"Actual:   {0.114M} (rounded from {0.11444M})",
				ex.Message
			);
		}
	}

	public class Equal_Double
	{
		public class WithPrecision
		{
			[Fact]
			public void Equal()
			{
				Assert.Equal(0.11111, 0.11444, 2);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.11111, 0.11444, 3));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values are not within 3 decimal places" + Environment.NewLine +
					$"Expected: {0.111:G17} (rounded from {0.11111:G17})" + Environment.NewLine +
					$"Actual:   {0.114:G17} (rounded from {0.11444:G17})",
					ex.Message
				);
			}
		}

		public class WithMidPointRounding
		{
			[Fact]
			public void Equal()
			{
				Assert.Equal(10.565, 10.566, 2, MidpointRounding.AwayFromZero);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.11113, 0.11115, 4, MidpointRounding.ToEven));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within 4 decimal places" + Environment.NewLine +
					$"Expected: {0.1111:G17} (rounded from {0.11113:G17})" + Environment.NewLine +
					$"Actual:   {0.1112:G17} (rounded from {0.11115:G17})",
					ex.Message
				);
			}
		}

		public class WithTolerance
		{
			[Fact]
			public void GuardClause()
			{
				var ex = Record.Exception(() => Assert.Equal(0.0, 1.0, double.NegativeInfinity));

				var argEx = Assert.IsType<ArgumentException>(ex);
				Assert.StartsWith("Tolerance must be greater than or equal to zero", ex.Message);
				Assert.Equal("tolerance", argEx.ParamName);
			}

			[Fact]
			public void Equal()
			{
				Assert.Equal(10.566, 10.565, 0.01);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.11113, 0.11115, 0.00001));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {0.00001:G17}" + Environment.NewLine +
					$"Expected: {0.11113:G17}" + Environment.NewLine +
					$"Actual:   {0.11115:G17}",
					ex.Message
				);
			}

			[Fact]
			public void NaN_Equal()
			{
				Assert.Equal(double.NaN, double.NaN, 1000.0);
			}

			[CulturedFact]
			public void NaN_NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(20210102.2208, double.NaN, 20000000.0));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {20000000.0:G17}" + Environment.NewLine +
					$"Expected: {20210102.2208:G17}" + Environment.NewLine +
					$"Actual:   NaN",
					ex.Message
				);
			}

			[Fact]
			public void InfiniteTolerance_Equal()
			{
				Assert.Equal(double.MinValue, double.MaxValue, double.PositiveInfinity);
			}

			[CulturedFact]
			public void PositiveInfinity_NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(double.PositiveInfinity, 77.7, 1.0));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {1.0:G17}" + Environment.NewLine +
					$"Expected: {double.PositiveInfinity}" + Environment.NewLine +
					$"Actual:   {77.7:G17}",
					ex.Message
				);
			}

			[CulturedFact]
			public void NegativeInfinity_NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.0, double.NegativeInfinity, 1.0));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {1.0:G17}" + Environment.NewLine +
					$"Expected: {0.0:G17}" + Environment.NewLine +
					$"Actual:   {double.NegativeInfinity}",
					ex.Message
				);
			}
		}
	}

	public class Equal_Float
	{
		public class WithPrecision
		{
			[Fact]
			public void Equal()
			{
				Assert.Equal(0.11111f, 0.11444f, 2);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.11111f, 0.11444f, 3));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values are not within 3 decimal places" + Environment.NewLine +
					$"Expected: {0.111:G9} (rounded from {0.11111f:G9})" + Environment.NewLine +
					$"Actual:   {0.114:G9} (rounded from {0.11444f:G9})",
					ex.Message
				);
			}
		}

		public class WithMidPointRounding
		{
			[Fact]
			public void Equal()
			{
				Assert.Equal(10.5655f, 10.5666f, 2, MidpointRounding.AwayFromZero);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.111133f, 0.111155f, 4, MidpointRounding.ToEven));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Values are not within 4 decimal places" + Environment.NewLine +
					$"Expected: {0.1111:G9} (rounded from {0.111133f:G9})" + Environment.NewLine +
					$"Actual:   {0.1112:G9} (rounded from {0.111155f:G9})",
					ex.Message
				);
			}
		}

		public class WithTolerance
		{
			[Fact]
			public void GuardClause()
			{
				var ex = Record.Exception(() => Assert.Equal(0.0f, 1.0f, float.NegativeInfinity));

				var argEx = Assert.IsType<ArgumentException>(ex);
				Assert.StartsWith("Tolerance must be greater than or equal to zero", ex.Message);
				Assert.Equal("tolerance", argEx.ParamName);
			}

			[Fact]
			public void Equal()
			{
				Assert.Equal(10.569f, 10.562f, 0.01f);
			}

			[CulturedFact]
			public void NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.11113f, 0.11115f, 0.00001f));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {0.00001f:G9}" + Environment.NewLine +
					$"Expected: {0.11113f:G9}" + Environment.NewLine +
					$"Actual:   {0.11115f:G9}",
					ex.Message
				);
			}

			[Fact]
			public void NaN_Equal()
			{
				Assert.Equal(float.NaN, float.NaN, 1000.0f);
			}

			[CulturedFact]
			public void NaN_NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(20210102.2208f, float.NaN, 20000000.0f));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {20000000.0f:G9}" + Environment.NewLine +
					$"Expected: {20210102.2208f:G9}" + Environment.NewLine +
					$"Actual:   NaN",
					ex.Message
				);
			}

			[Fact]
			public void InfiniteTolerance_Equal()
			{
				Assert.Equal(float.MinValue, float.MaxValue, float.PositiveInfinity);
			}

			[CulturedFact]
			public void PositiveInfinity_NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(float.PositiveInfinity, 77.7f, 1.0f));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {1.0f:G9}" + Environment.NewLine +
					$"Expected: {float.PositiveInfinity}" + Environment.NewLine +
					$"Actual:   {77.7f:G9}",
					ex.Message
				);
			}

			[CulturedFact]
			public void NegativeInfinity_NotEqual()
			{
				var ex = Record.Exception(() => Assert.Equal(0.0f, float.NegativeInfinity, 1.0f));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					$"Assert.Equal() Failure: Values are not within tolerance {1.0f:G9}" + Environment.NewLine +
					$"Expected: {0.0f:G9}" + Environment.NewLine +
					$"Actual:   {float.NegativeInfinity}",
					ex.Message
				);
			}
		}
	}

	public class NotEqual
	{
		public class Intrinsics
		{
			[Fact]
			public void EqualValues()
			{
				var ex = Record.Exception(() => Assert.NotEqual(42, 42));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not 42" + Environment.NewLine +
					"Actual:       42",
					ex.Message
				);
			}

			[Fact]
			public void UnequalValues()
			{
				Assert.NotEqual(42, 2112);
			}
		}

		public class WithComparer
		{
			[Fact]
			public void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("comparer", () => Assert.NotEqual(1, 2, default(IEqualityComparer<int>)!));
			}

			[Fact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(42, 21, new Comparer<int>(true)));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not 42" + Environment.NewLine +
					"Actual:       21",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(42, 42, new Comparer<int>(false));
			}

			class Comparer<T>(bool result) :
				IEqualityComparer<T>
			{
				readonly bool result = result;

				public bool Equals(T? x, T? y) => result;

				public int GetHashCode(T obj) => throw new NotImplementedException();
			}

			[Fact]
			public void NonEnumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.NotEqual(42, 42, new ThrowingIntComparer()));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: Not 42" + Environment.NewLine +
					"Actual:       42",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			public class ThrowingIntComparer : IEqualityComparer<int>
			{
				public bool Equals(int x, int y) =>
					throw new DivideByZeroException();
				public int GetHashCode(int obj) =>
					throw new NotImplementedException();
			}

			[Fact]
			public void Enumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.NotEqual([1, 2], [1, 2], new ThrowingEnumerableComparer()));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: Not [1, 2]" + Environment.NewLine +
					"Actual:       [1, 2]",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			public class ThrowingEnumerableComparer : IEqualityComparer<IEnumerable<int>>
			{
				public bool Equals(IEnumerable<int>? x, IEnumerable<int>? y) =>
					throw new DivideByZeroException();
				public int GetHashCode(IEnumerable<int> obj) =>
					throw new NotImplementedException();
			}

			[Fact]
			public void Strings_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.NotEqual("42", "42", new ThrowingStringComparer()));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: Not \"42\"" + Environment.NewLine +
					"Actual:       \"42\"",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			public class ThrowingStringComparer : IEqualityComparer<string>
			{
				public bool Equals(string? x, string? y) =>
					throw new DivideByZeroException();
				public int GetHashCode(string obj) =>
					throw new NotImplementedException();
			}
		}

		public class WithFunc
		{
			[Fact]
			public void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("comparer", () => Assert.NotEqual(1, 2, default(Func<int, int, bool>)!));
			}

			[Fact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(42, 21, (x, y) => true));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not 42" + Environment.NewLine +
					"Actual:       21",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(42, 42, (x, y) => false);
			}

			[Fact]
			public void NonEnumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.NotEqual(42, 42, (e, a) => throw new DivideByZeroException()));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: Not 42" + Environment.NewLine +
					"Actual:       42",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			[Fact]
			public void Enumerable_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(
					() => Assert.NotEqual(
						[1, 2],
						[1, 2],
						(IEnumerable<int> e, IEnumerable<int> a) => throw new DivideByZeroException()
					)
				);

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: Not [1, 2]" + Environment.NewLine +
					"Actual:       [1, 2]",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}

			[Fact]
			public void Strings_WithThrow_RecordsInnerException()
			{
				var ex = Record.Exception(() => Assert.NotEqual("42", "42", (e, a) => throw new DivideByZeroException()));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
					"Expected: Not \"42\"" + Environment.NewLine +
					"Actual:       \"42\"",
					ex.Message
				);
				Assert.IsType<DivideByZeroException>(ex.InnerException);
			}
		}

		public class Comparable
		{
			[Fact]
			public void Equal()
			{
				var obj1 = new SpyComparable(0);
				var obj2 = new SpyComparable(0);

				var ex = Record.Exception(() => Assert.NotEqual(obj1, obj2));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not SpyComparable { CompareCalled = True }" + Environment.NewLine +
					"Actual:       SpyComparable { CompareCalled = False }",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				var obj1 = new SpyComparable(-1);
				var obj2 = new SpyComparable(0);

				Assert.NotEqual(obj1, obj2);
				Assert.True(obj1.CompareCalled);
			}

			[Fact]
			public void NonGeneric_SameType_Equal()
			{
				var expected = new MultiComparable(1);
				var actual = new MultiComparable(1);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
						"Expected: Not MultiComparable { Value = 1 }" + Environment.NewLine +
						"Actual:       MultiComparable { Value = 1 }",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (IComparable)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void NonGeneric_SameType_NotEqual()
			{
				var expected = new MultiComparable(1);
				var actual = new MultiComparable(2);

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (IComparable)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void NonGeneric_DifferentType_Equal()
			{
				var expected = new MultiComparable(1);
				var actual = 1;

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
						"Expected: Not MultiComparable { Value = 1 }" + Environment.NewLine +
						"Actual:       1",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, (IComparable)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void NonGeneric_DifferentType_NotEqual()
			{
				var expected = new MultiComparable(1);
				var actual = 2;

				Assert.NotEqual(expected, (IComparable)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void Generic_Equal()
			{
				var obj1 = new SpyComparable_Generic();
				var obj2 = new SpyComparable_Generic();

				var ex = Record.Exception(() => Assert.NotEqual(obj1, obj2));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not SpyComparable_Generic { CompareCalled = True }" + Environment.NewLine +
					"Actual:       SpyComparable_Generic { CompareCalled = False }",
					ex.Message
				);
			}

			[Fact]
			public void Generic_NotEqual()
			{
				var obj1 = new SpyComparable_Generic(-1);
				var obj2 = new SpyComparable_Generic();

				Assert.NotEqual(obj1, obj2);
				Assert.True(obj1.CompareCalled);
			}

			[Fact]
			public void SubClass_SubClass_Equal()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableSubClassB(1);

				var ex = Record.Exception(() => Assert.NotEqual<ComparableBaseClass>(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ComparableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:       ComparableSubClassB { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_SubClass_NotEqual()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableSubClassB(2);

				Assert.NotEqual<ComparableBaseClass>(expected, actual);
			}

			[Fact]
			public void BaseClass_SubClass_Equal()
			{
				var expected = new ComparableBaseClass(1);
				var actual = new ComparableSubClassA(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ComparableBaseClass { Value = 1 }" + Environment.NewLine +
					"Actual:       ComparableSubClassA { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void BaseClass_SubClass_NotEqual()
			{
				var expected = new ComparableBaseClass(1);
				var actual = new ComparableSubClassA(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void SubClass_BaseClass_Equal()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableBaseClass(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ComparableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:       ComparableBaseClass { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_BaseClass_NotEqual()
			{
				var expected = new ComparableSubClassA(1);
				var actual = new ComparableBaseClass(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void Generic_ThrowsException_Equal()
			{
				var expected = new ComparableThrower(1);
				var actual = new ComparableThrower(1);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
						"Expected: Not ComparableThrower { Value = 1 }" + Environment.NewLine +
						"Actual:       ComparableThrower { Value = 1 }",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (IComparable<ComparableThrower>)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void Generic_ThrowsException_NotEqual()
			{
				var expected = new ComparableThrower(1);
				var actual = new ComparableThrower(2);

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (IComparable<ComparableThrower>)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_Equal()
			{
				object expected = new ImplicitIComparableExpected(1);
				object actual = new IntWrapper(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ImplicitIComparableExpected { Value = 1 }" + Environment.NewLine +
					"Actual:       IntWrapper { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_NotEqual()
			{
				object expected = new ImplicitIComparableExpected(1);
				object actual = new IntWrapper(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_Equal()
			{
				object expected = new ExplicitIComparableActual(1);
				object actual = new IntWrapper(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ExplicitIComparableActual { Value = 1 }" + Environment.NewLine +
					"Actual:       IntWrapper { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_NotEqual()
			{
				object expected = new ExplicitIComparableActual(1);
				object actual = new IntWrapper(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void DifferentTypes_Throws_Equal()
			{
				object expected = new IComparableActualThrower(1);
				object actual = new IntWrapper(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not IComparableActualThrower { Value = 1 }" + Environment.NewLine +
					"Actual:       IntWrapper { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_Throws_NotEqual()
			{
				object expected = new IComparableActualThrower(1);
				object actual = new IntWrapper(2);

				Assert.NotEqual(expected, actual);
			}
		}

		public class NotComparable
		{
			[Fact]
			public void Equal()
			{
				var nco1 = new NonComparableObject();
				var nco2 = new NonComparableObject();

				var ex = Record.Exception(() => Assert.NotEqual(nco1, nco2));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not NonComparableObject { }" + Environment.NewLine +
					"Actual:       NonComparableObject { }",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				var nco1 = new NonComparableObject(false);
				var nco2 = new NonComparableObject();

				Assert.NotEqual(nco1, nco2);
			}
		}

		public class Equatable
		{
			[Fact]
			public void Equal()
			{
				var obj1 = new SpyEquatable();
				var obj2 = new SpyEquatable();

				var ex = Record.Exception(() => Assert.NotEqual(obj1, obj2));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not SpyEquatable { Equals__Called = True, Equals_Other = SpyEquatable { Equals__Called = False, Equals_Other = null } }" + Environment.NewLine +
					"Actual:       SpyEquatable { Equals__Called = False, Equals_Other = null }",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				var obj1 = new SpyEquatable(false);
				var obj2 = new SpyEquatable();

				Assert.NotEqual(obj1, obj2);

				Assert.True(obj1.Equals__Called);
				Assert.Same(obj2, obj1.Equals_Other);
			}

			[Fact]
			public void SubClass_SubClass_Equal()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableSubClassB(1);

				var ex = Record.Exception(() => Assert.NotEqual<EquatableBaseClass>(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not EquatableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:       EquatableSubClassB { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_SubClass_NotEqual()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableSubClassB(2);

				Assert.NotEqual<EquatableBaseClass>(expected, actual);
			}

			[Fact]
			public void BaseClass_SubClass_Equal()
			{
				var expected = new EquatableBaseClass(1);
				var actual = new EquatableSubClassA(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not EquatableBaseClass { Value = 1 }" + Environment.NewLine +
					"Actual:       EquatableSubClassA { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void BaseClass_SubClass_NotEqual()
			{
				var expected = new EquatableBaseClass(1);
				var actual = new EquatableSubClassA(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void SubClass_BaseClass_Equal()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableBaseClass(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not EquatableSubClassA { Value = 1 }" + Environment.NewLine +
					"Actual:       EquatableBaseClass { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void SubClass_BaseClass_NotEqual()
			{
				var expected = new EquatableSubClassA(1);
				var actual = new EquatableBaseClass(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_Equal()
			{
				object expected = new ImplicitIEquatableExpected(1);
				object actual = new IntWrapper(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ImplicitIEquatableExpected { Value = 1 }" + Environment.NewLine +
					"Actual:       IntWrapper { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ImplicitImplementation_NotEqual()
			{
				object expected = new ImplicitIEquatableExpected(1);
				object actual = new IntWrapper(2);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_Equal()
			{
				object expected = new ExplicitIEquatableExpected(1);
				object actual = new IntWrapper(1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not ExplicitIEquatableExpected { Value = 1 }" + Environment.NewLine +
					"Actual:       IntWrapper { Value = 1 }",
					ex.Message
				);
			}

			[Fact]
			public void DifferentTypes_ExplicitImplementation_NotEqual()
			{
				object expected = new ExplicitIEquatableExpected(1);
				object actual = new IntWrapper(2);

				Assert.NotEqual(expected, actual);
			}
		}

		public class StructuralEquatable
		{
			[Fact]
			public void Equal()
			{
				var expected = new Tuple<StringWrapper>(new StringWrapper("a"));
				var actual = new Tuple<StringWrapper>(new StringWrapper("a"));

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
						"Expected: Not Tuple (StringWrapper { Value = \"a\" })" + Environment.NewLine +
						"Actual:       Tuple (StringWrapper { Value = \"a\" })",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (IStructuralEquatable)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void NotEqual()
			{
				var expected = new Tuple<StringWrapper>(new StringWrapper("a"));
				var actual = new Tuple<StringWrapper>(new StringWrapper("b"));

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (IStructuralEquatable)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void ExpectedNull_ActualNull()
			{
				var expected = new Tuple<StringWrapper?>(null);
				var actual = new Tuple<StringWrapper?>(null);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
						"Expected: Not Tuple (null)" + Environment.NewLine +
						"Actual:       Tuple (null)",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (IStructuralEquatable)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void ExpectedNull_ActualNonNull()
			{
				var expected = new Tuple<StringWrapper?>(null);
				var actual = new Tuple<StringWrapper?>(new StringWrapper("a"));

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (IStructuralEquatable)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void ExpectedNonNull_ActualNull()
			{
				var expected = new Tuple<StringWrapper?>(new StringWrapper("a"));
				var actual = new Tuple<StringWrapper?>(null);

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (IStructuralEquatable)actual);
				Assert.NotEqual(expected, (object)actual);
			}
		}

		public class Collections
		{
			[Fact]
			public void IReadOnlyCollection_IEnumerable_Equal()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new ReadOnlyCollection<string>(expected);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						"Expected: Not string[]                   [\"foo\", \"bar\"]" + Environment.NewLine +
						"Actual:       ReadOnlyCollection<string> [\"foo\", \"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, (IReadOnlyCollection<string>)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void IReadOnlyCollection_IEnumerable_NotEqual()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new ReadOnlyCollection<string>(["bar", "foo"]);

				Assert.NotEqual(expected, (IReadOnlyCollection<string>)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void CollectionDepth_Equal()
			{
				var x = new List<object> { new List<object> { new List<object> { 1 } } };
				var y = new List<object> { new List<object> { new List<object> { 1 } } };

				var ex = Record.Exception(() => Assert.NotEqual(x, y));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [[[1]]]" + Environment.NewLine +
					"Actual:       [[[1]]]",
					ex.Message
				);
			}

			[Fact]
			public void CollectionDepth_NotEqual()
			{
				var x = new List<object> { new List<object> { new List<object> { 1 } } };
				var y = new List<object> { new List<object> { new List<object> { 2 } } };

				Assert.NotEqual(x, y);
			}

			[Fact]
			public void StringArray_ObjectArray_Equal()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new object[] { "foo", "bar" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						"Expected: Not string[] [\"foo\", \"bar\"]" + Environment.NewLine +
						"Actual:       object[] [\"foo\", \"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void StringArray_ObjectArray_NotEqual()
			{
				var expected = new string[] { "foo", "bar" };
				var actual = new object[] { "foo", "baz" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void MultidimensionalArrays_Equal()
			{
				var expected = new int[,] { { 1 }, { 2 } };
				var actual = new int[,] { { 1 }, { 2 } };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				// TODO: Would be better to have formatting that preserves the ranks instead of
				// flattening, which happens because multi-dimensional arrays enumerate flatly
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [1, 2]" + Environment.NewLine +
					"Actual:       [1, 2]",
					ex.Message
				);
			}

			[Fact]
			public void MultidimensionalArrays_NotEqual()
			{
				var expected = new int[,] { { 1, 2 } };
				var actual = new int[,] { { 1 }, { 2 } };

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void NonZeroBoundedArrays_Equal()
			{
				var expected = Array.CreateInstance(typeof(int), [1], [1]);
				expected.SetValue(42, 1);
				var actual = Array.CreateInstance(typeof(int), [1], [1]);
				actual.SetValue(42, 1);

				var ex = Record.Exception(() => Assert.NotEqual(expected, (object)actual));

				Assert.IsType<NotEqualException>(ex);
				// TODO: Would be better to have formatting that shows the non-zero bounds
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [42]" + Environment.NewLine +
					"Actual:       [42]",
					ex.Message
				);
			}

			[Fact]
			public void NonZeroBoundedArrays_NotEqual()
			{
				var expected = Array.CreateInstance(typeof(int), [1], [1]);
				expected.SetValue(42, 1);
				var actual = Array.CreateInstance(typeof(int), [1], [0]);
				actual.SetValue(42, 0);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void CollectionWithIEquatable_Equal()
			{
				var expected = new EnumerableEquatable<int> { 42, 2112 };
				var actual = new EnumerableEquatable<int> { 2112, 42 };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [42, 2112]" + Environment.NewLine +
					"Actual:       [2112, 42]",
					ex.Message
				);
			}

			[Fact]
			public void CollectionWithIEquatable_NotEqual()
			{
				var expected = new EnumerableEquatable<int> { 42, 2112 };
				var actual = new EnumerableEquatable<int> { 2112, 2600 };

				Assert.NotEqual(expected, actual);
			}

			public sealed class EnumerableEquatable<T> :
				IEnumerable<T>, IEquatable<EnumerableEquatable<T>>
			{
				readonly List<T> values = [];

				public void Add(T value) => values.Add(value);

				public bool Equals(EnumerableEquatable<T>? other)
				{
					if (other == null)
						return false;

					return !values.Except(other.values).Any() && !other.values.Except(values).Any();
				}

				public IEnumerator<T> GetEnumerator() => values.GetEnumerator();

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}
		}

		public class Dictionaries
		{
			[Fact]
			public void SameTypes_Equal()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new Dictionary<string, string> { ["foo"] = "bar" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Dictionaries are equal" + Environment.NewLine +
						"Expected: Not [[\"foo\"] = \"bar\"]" + Environment.NewLine +
						"Actual:       [[\"foo\"] = \"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (IDictionary)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void SameTypes_NotEqual()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new Dictionary<string, string> { ["foo"] = "baz" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (IDictionary)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new ConcurrentDictionary<string, string>(expected);

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						"Expected: Not Dictionary<string, string>           [[\"foo\"] = \"bar\"]" + Environment.NewLine +
						"Actual:       ConcurrentDictionary<string, string> [[\"foo\"] = \"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, (IDictionary)actual));
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				var expected = new Dictionary<string, string> { ["foo"] = "bar" };
				var actual = new ConcurrentDictionary<string, string> { ["foo"] = "baz" };

				Assert.NotEqual(expected, (IDictionary)actual);
				Assert.NotEqual(expected, (object)actual);
			}

			[Fact]
			public void NullValue_Equal()
			{
				var expected = new Dictionary<string, int?> { { "two", null } };
				var actual = new Dictionary<string, int?> { { "two", null } };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Dictionaries are equal" + Environment.NewLine +
					"Expected: Not [[\"two\"] = null]" + Environment.NewLine +
					"Actual:       [[\"two\"] = null]",
					ex.Message
				);
			}

			[Fact]
			public void NullValue_NotEqual()
			{
				var expected = new Dictionary<string, int?> { { "two", null } };
				var actual = new Dictionary<string, int?> { { "two", 1 } };

				Assert.NotEqual(expected, actual);
			}
		}

		public class HashSets
		{
			[Fact]
			public static void InOrder_Equal()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 1, 2, 3 };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: HashSets are equal" + Environment.NewLine +
					"Expected: Not [1, 2, 3]" + Environment.NewLine +
					"Actual:       [1, 2, 3]",
					ex.Message
				);
			}

			[Fact]
			public static void InOrder_NotEqual()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 1, 2, 4 };

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public static void OutOfOrder_Equal()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 2, 3, 1 };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: HashSets are equal" + Environment.NewLine +
					"Expected: Not [1, 2, 3]" + Environment.NewLine +
					"Actual:       [2, 3, 1]",
					ex.Message
				);
			}

			[Fact]
			public static void OutOfOrder_NotEqual()
			{
				var expected = new HashSet<int> { 1, 2, 3 };
				var actual = new HashSet<int> { 2, 4, 1 };

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new HashSet<string> { "bar", "foo" };
				var actual = new SortedSet<string> { "foo", "bar" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Sets are equal" + Environment.NewLine +
						"Expected: Not HashSet<string>   [\"bar\", \"foo\"]" + Environment.NewLine +
						"Actual:       SortedSet<string> [\"bar\", \"foo\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				object expected = new HashSet<int> { 42 };
				object actual = new HashSet<long> { 42L };

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void ComparerFunc_Throws()
			{
				var expected = new HashSet<string> { "bar" };
				var actual = new HashSet<string> { "baz" };

#pragma warning disable xUnit2026 // Comparison of sets must be done with IEqualityComparer
				var ex = Record.Exception(() => Assert.NotEqual(expected, actual, (string l, string r) => false));
#pragma warning restore xUnit2026 // Comparison of sets must be done with IEqualityComparer

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: During comparison of two collections, GetHashCode was called, but only a comparison function was provided. This typically indicates trying to compare two sets with an item comparison function, which is not supported. For more information, see https://xunit.net/docs/hash-sets-vs-linear-containers",
					ex.Message
				);
			}
		}

		public class Sets
		{
			[Fact]
			public void InOrder_Equal()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "bar", "foo" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Sets are equal" + Environment.NewLine +
						"Expected: Not [\"bar\", \"foo\"]" + Environment.NewLine +
						"Actual:       [\"bar\", \"foo\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void InOrder_NotEqual()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "bar", "baz" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.NotEqual(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void OutOfOrder_Equal()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "foo", "bar" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Sets are equal" + Environment.NewLine +
						"Expected: Not [\"bar\", \"foo\"]" + Environment.NewLine +
						"Actual:       [\"foo\", \"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void OutOfOrder_NotEqual()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "foo", "baz" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.NotEqual(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentContents()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new NonGenericSet { "bar", "foo" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.NotEqual(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "bar" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Sets are equal" + Environment.NewLine +
						"Expected: Not NonGenericSet   [\"bar\"]" + Environment.NewLine +
						"Actual:       HashSet<string> [\"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "baz" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.NotEqual(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void TwoGenericSubClass_Equal()
			{
				var expected = new TwoGenericSet<string, int> { "foo", "bar" };
				var actual = new TwoGenericSet<string, int> { "foo", "bar" };

				static void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Sets are equal" + Environment.NewLine +
						"Expected: Not [\"foo\", \"bar\"]" + Environment.NewLine +
						"Actual:       [\"foo\", \"bar\"]",
						ex.Message
					);
				}

				assertFailure(() => Assert.NotEqual(expected, actual));
				assertFailure(() => Assert.NotEqual(expected, (ISet<string>)actual));
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				assertFailure(() => Assert.NotEqual(expected, (object)actual));
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void TwoGenericSubClass_NotEqual()
			{
				var expected = new TwoGenericSet<string, int> { "foo", "bar" };
				var actual = new TwoGenericSet<string, int> { "foo", "baz" };

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected, (ISet<string>)actual);
#pragma warning disable xUnit2027 // Comparison of sets to linear containers have undefined results
				Assert.NotEqual(expected, (object)actual);
#pragma warning restore xUnit2027 // Comparison of sets to linear containers have undefined results
			}

			[Fact]
			public void ComparerFunc_Throws()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "baz" };

#pragma warning disable xUnit2026 // Comparison of sets must be done with IEqualityComparer
				var ex = Record.Exception(() => Assert.NotEqual(expected, actual, (string l, string r) => false));
#pragma warning restore xUnit2026 // Comparison of sets must be done with IEqualityComparer

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: During comparison of two collections, GetHashCode was called, but only a comparison function was provided. This typically indicates trying to compare two sets with an item comparison function, which is not supported. For more information, see https://xunit.net/docs/hash-sets-vs-linear-containers",
					ex.Message
				);
			}
		}

		// https://github.com/xunit/xunit/issues/3137
		public class ImmutableArrays
		{
			[Fact]
			public void Equal()
			{
				var expected = new[] { 1, 2, 3 }.ToImmutableArray();
				var actual = new[] { 1, 2, 3 }.ToImmutableArray();

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [1, 2, 3]" + Environment.NewLine +
					"Actual:       [1, 2, 3]",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				var expected = new[] { 1, 2, 3 }.ToImmutableArray();
				var actual = new[] { 1, 2, 4 }.ToImmutableArray();

				Assert.NotEqual(expected, actual);
			}
		}

		public class Strings
		{
			[Fact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual("actual", "actual"));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					@"Assert.NotEqual() Failure: Strings are equal" + Environment.NewLine +
					@"Expected: Not ""actual""" + Environment.NewLine +
					@"Actual:       ""actual""",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual("foo", "bar");
			}

			[Fact]
			public void Truncation()
			{
				var ex = Record.Exception(
					() => Assert.NotEqual(
						"This is a long string so that we can test truncation behavior",
						"This is a long string so that we can test truncation behavior"
					)
				);

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Strings are equal" + Environment.NewLine +
					@"Expected: Not ""This is a long string so that we can test truncati""···" + Environment.NewLine +
					@"Actual:       ""This is a long string so that we can test truncati""···",
					ex.Message
				);
			}
		}

		public class KeyValuePair
		{
			[Fact]
			public void CollectionKeys_Equal()
			{
				// Different concrete collection types in the key slot, per https://github.com/xunit/xunit/issues/2850
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				var expected = new KeyValuePair<IEnumerable<string>, int>(new List<string> { "Key1", "Key2" }, 42);
				var actual = new KeyValuePair<IEnumerable<string>, int>(new string[] { "Key1", "Key2" }, 42);
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not [[\"Key1\", \"Key2\"]] = 42" + Environment.NewLine +
					"Actual:       [[\"Key1\", \"Key2\"]] = 42",
					ex.Message
				);
			}

			[Fact]
			public void CollectionKeys_NotEqual()
			{
				// Different concrete collection types in the key slot, per https://github.com/xunit/xunit/issues/2850
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				var expected = new KeyValuePair<IEnumerable<string>, int>(new List<string> { "Key1", "Key2" }, 42);
				var actual = new KeyValuePair<IEnumerable<string>, int>(new string[] { "Key1", "Key3" }, 42);
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void CollectionValues_Equal()
			{
				// Different concrete collection types in the key slot, per https://github.com/xunit/xunit/issues/2850
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				var expected = new KeyValuePair<string, IEnumerable<string>>("Key1", new List<string> { "Value1a", "Value1b" });
				var actual = new KeyValuePair<string, IEnumerable<string>>("Key1", new string[] { "Value1a", "Value1b" });
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not [\"Key1\"] = [\"Value1a\", \"Value1b\"]" + Environment.NewLine +
					"Actual:       [\"Key1\"] = [\"Value1a\", \"Value1b\"]",
					ex.Message
				);
			}

			[Fact]
			public void CollectionValues_NotEqual()
			{
				// Different concrete collection types in the key slot, per https://github.com/xunit/xunit/issues/2850
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
				var expected = new KeyValuePair<string, IEnumerable<string>>("Key1", new List<string> { "Value1a", "Value1b" });
				var actual = new KeyValuePair<string, IEnumerable<string>>("Key1", new string[] { "Value1a", "Value2a" });
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0028 // Simplify collection initialization

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void EquatableKeys_Equal()
			{
				var expected = new KeyValuePair<EquatableObject, int>(new() { Char = 'a' }, 42);
				var actual = new KeyValuePair<EquatableObject, int>(new() { Char = 'a' }, 42);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not [EquatableObject { Char = 'a' }] = 42" + Environment.NewLine +
					"Actual:       [EquatableObject { Char = 'a' }] = 42",
					ex.Message
				);
			}

			[Fact]
			public void EquatableKeys_NotEqual()
			{
				var expected = new KeyValuePair<EquatableObject, int>(new() { Char = 'a' }, 42);
				var actual = new KeyValuePair<EquatableObject, int>(new() { Char = 'b' }, 42);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public void EquatableValues_Equal()
			{
				var expected = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'a' });
				var actual = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'a' });

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not [\"Key1\"] = EquatableObject { Char = 'a' }" + Environment.NewLine +
					"Actual:       [\"Key1\"] = EquatableObject { Char = 'a' }",
					ex.Message
				);
			}

			[Fact]
			public void EquatableValues_NotEqual()
			{
				var expected = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'a' });
				var actual = new KeyValuePair<string, EquatableObject>("Key1", new() { Char = 'b' });

				Assert.NotEqual(expected, actual);
			}

			public class EquatableObject : IEquatable<EquatableObject>
			{
				public char Char { get; set; }

				public bool Equals(EquatableObject? other) =>
					other != null && other.Char == Char;
			}
		}

		public class DoubleEnumerationPrevention
		{
			[Fact]
			public static void EnumeratesOnlyOnce_Equal()
			{
				var expected = new RunOnceEnumerable<int>([1, 2, 3, 4, 5]);
				var actual = new RunOnceEnumerable<int>([1, 2, 3, 4, 5]);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [1, 2, 3, 4, 5]" + Environment.NewLine +
					"Actual:       [1, 2, 3, 4, 5]",
					ex.Message
				);
			}

			[Fact]
			public static void EnumeratesOnlyOnce_NotEqual()
			{
				var expected = new RunOnceEnumerable<int>([1, 2, 3, 4, 5]);
				var actual = new RunOnceEnumerable<int>([1, 2, 3, 4, 5, 6]);

				Assert.NotEqual(expected, actual);
			}
		}
	}

	public class NotEqual_Decimal
	{
		[CulturedFact]
		public void Equal()
		{
			var ex = Record.Exception(() => Assert.NotEqual(0.11111M, 0.11444M, 2));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
				$"Expected: Not {0.11M} (rounded from {0.11111})" + Environment.NewLine +
				$"Actual:       {0.11M} (rounded from {0.11444})",
				ex.Message
			);
		}

		[Fact]
		public void NotEqual()
		{
			Assert.NotEqual(0.11111M, 0.11444M, 3);
		}
	}

	public class NotEqual_Double
	{
		public class WithPrecision
		{
			[CulturedFact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(0.11111, 0.11444, 2));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are within 2 decimal places" + Environment.NewLine +
					$"Expected: Not {0.11:G17} (rounded from {0.11111:G17})" + Environment.NewLine +
					$"Actual:       {0.11:G17} (rounded from {0.11444:G17})",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(0.11111, 0.11444, 3);
			}
		}

		public class WithMidPointRounding
		{
			[CulturedFact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(10.565, 10.566, 2, MidpointRounding.AwayFromZero));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within 2 decimal places" + Environment.NewLine +
					$"Expected: Not {10.57:G17} (rounded from {10.565:G17})" + Environment.NewLine +
					$"Actual:       {10.57:G17} (rounded from {10.566:G17})",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(0.11113, 0.11115, 4, MidpointRounding.ToEven);
			}
		}

		public class WithTolerance
		{
			[Fact]
			public void GuardClause()
			{
				var ex = Record.Exception(() => Assert.NotEqual(0.0, 1.0, double.NegativeInfinity));

				var argEx = Assert.IsType<ArgumentException>(ex);
				Assert.StartsWith("Tolerance must be greater than or equal to zero", ex.Message);
				Assert.Equal("tolerance", argEx.ParamName);
			}

			[CulturedFact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(10.566, 10.565, 0.01));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within tolerance {0.01:G17}" + Environment.NewLine +
					$"Expected: Not {10.566:G17}" + Environment.NewLine +
					$"Actual:       {10.565:G17}",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(0.11113, 0.11115, 0.00001);
			}

			[CulturedFact]
			public void NaN_Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(double.NaN, double.NaN, 1000.0));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within tolerance {1000.0:G17}" + Environment.NewLine +
					$"Expected: Not {double.NaN}" + Environment.NewLine +
					$"Actual:       {double.NaN}",
					ex.Message
				);
			}

			[Fact]
			public void NaN_NotEqual()
			{
				Assert.NotEqual(20210102.2208, double.NaN, 20000000.0);
			}

			[CulturedFact]
			public void InfiniteTolerance_Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(double.MinValue, double.MaxValue, double.PositiveInfinity));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within tolerance {double.PositiveInfinity}" + Environment.NewLine +
					$"Expected: Not {double.MinValue:G17}" + Environment.NewLine +
					$"Actual:       {double.MaxValue:G17}",
					ex.Message
				);
			}

			[Fact]
			public void PositiveInfinity_NotEqual()
			{
				Assert.NotEqual(double.PositiveInfinity, 77.7, 1.0);
			}

			[Fact]
			public void NegativeInfinity_NotEqual()
			{
				Assert.NotEqual(0.0, double.NegativeInfinity, 1.0);
			}
		}
	}

	public class NotEqual_Float
	{
		public class WithPrecision
		{
			[CulturedFact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(0.11111f, 0.11444f, 2));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are within 2 decimal places" + Environment.NewLine +
					$"Expected: Not {0.11:G9} (rounded from {0.11111f:G9})" + Environment.NewLine +
					$"Actual:       {0.11:G9} (rounded from {0.11444f:G9})",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(0.11111f, 0.11444f, 3);
			}
		}

		public class WithMidPointRounding
		{
			[CulturedFact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(10.5655f, 10.5666f, 2, MidpointRounding.AwayFromZero));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are within 2 decimal places" + Environment.NewLine +
					$"Expected: Not {10.57:G9} (rounded from {10.5655f:G9})" + Environment.NewLine +
					$"Actual:       {10.57:G9} (rounded from {10.5666f:G9})",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(0.111133f, 0.111155f, 4, MidpointRounding.ToEven);
			}
		}

		public class WithTolerance
		{
			[Fact]
			public void GuardClause()
			{
				var ex = Record.Exception(() => Assert.NotEqual(0.0f, 1.0f, float.NegativeInfinity));

				var argEx = Assert.IsType<ArgumentException>(ex);
				Assert.StartsWith("Tolerance must be greater than or equal to zero", ex.Message);
				Assert.Equal("tolerance", argEx.ParamName);
			}

			[CulturedFact]
			public void Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(10.569f, 10.562f, 0.01f));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within tolerance {0.01f:G9}" + Environment.NewLine +
					$"Expected: Not {10.569f:G9}" + Environment.NewLine +
					$"Actual:       {10.562f:G9}",
					ex.Message
				);
			}

			[Fact]
			public void NotEqual()
			{
				Assert.NotEqual(0.11113f, 0.11115f, 0.00001f);
			}

			[CulturedFact]
			public void NaN_Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(float.NaN, float.NaN, 1000.0f));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within tolerance {1000.0f:G9}" + Environment.NewLine +
					"Expected: Not NaN" + Environment.NewLine +
					"Actual:       NaN",
					ex.Message
				);
			}

			[Fact]
			public void NaN_NotEqual()
			{
				Assert.NotEqual(20210102.2208f, float.NaN, 20000000.0f);
			}

			[CulturedFact]
			public void InfiniteTolerance_Equal()
			{
				var ex = Record.Exception(() => Assert.NotEqual(float.MinValue, float.MaxValue, float.PositiveInfinity));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					$"Assert.NotEqual() Failure: Values are within tolerance {float.PositiveInfinity}" + Environment.NewLine +
					$"Expected: Not {float.MinValue:G9}" + Environment.NewLine +
					$"Actual:       {float.MaxValue:G9}",
					ex.Message
				);
			}

			[Fact]
			public void PositiveInfinity_NotEqual()
			{
				Assert.NotEqual(float.PositiveInfinity, 77.7f, 1.0f);
			}

			[Fact]
			public void NegativeInfinity_NotEqual()
			{
				Assert.NotEqual(0.0f, float.NegativeInfinity, 1.0f);
			}
		}
	}

	public class NotStrictEqual
	{
		[Fact]
		public static void Equal()
		{
			var ex = Record.Exception(() => Assert.NotStrictEqual("actual", "actual"));

			Assert.IsType<NotStrictEqualException>(ex);
			Assert.Equal(
				"Assert.NotStrictEqual() Failure: Values are equal" + Environment.NewLine +
				@"Expected: Not ""actual""" + Environment.NewLine +
				@"Actual:       ""actual""",
				ex.Message
			);
		}

		[Fact]
		public static void NotEqual_Strings()
		{
			Assert.NotStrictEqual("bob", "jim");
		}

		[Fact]
		public static void NotEqual_Classes()
		{
			Assert.NotStrictEqual(new EnumerableClass("ploeh"), new EnumerableClass("fnaah"));
		}

		[Fact]
		public static void DifferentTypes_Equal()
		{
			var ex = Record.Exception(() => Assert.NotStrictEqual(new DerivedClass(), new BaseClass()));

			Assert.IsType<NotStrictEqualException>(ex);
			Assert.Equal(
				"Assert.NotStrictEqual() Failure: Values are equal" + Environment.NewLine +
				"Expected: Not DerivedClass { }" + Environment.NewLine +
				"Actual:       BaseClass { }",
				ex.Message
			);
		}
	}

	public class StrictEqual
	{
		[Fact]
		public static void Equal()
		{
#pragma warning disable xUnit2006 // Do not use invalid string equality check
			Assert.StrictEqual("actual", "actual");
#pragma warning restore xUnit2006 // Do not use invalid string equality check
		}

		[Fact]
		public static void NotEqual_Strings()
		{
#pragma warning disable xUnit2006 // Do not use invalid string equality check
			var ex = Record.Exception(() => Assert.StrictEqual("bob", "jim"));
#pragma warning restore xUnit2006 // Do not use invalid string equality check

			Assert.IsType<StrictEqualException>(ex);
			Assert.Equal(
				"Assert.StrictEqual() Failure: Values differ" + Environment.NewLine +
				@"Expected: ""bob""" + Environment.NewLine +
				@"Actual:   ""jim""",
				ex.Message
			);
		}

		[Fact]
		public static void NotEqual_Classes()
		{
			var ex = Record.Exception(() => Assert.StrictEqual(new EnumerableClass("ploeh"), new EnumerableClass("fnaah")));

			Assert.IsType<StrictEqualException>(ex);
			Assert.Equal(
				"Assert.StrictEqual() Failure: Values differ" + Environment.NewLine +
				$"Expected: [{ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
				$"Actual:   [{ArgumentFormatter.Ellipsis}]",
				ex.Message
			);
		}

		[Fact]
		public static void DifferentTypes_Equal()
		{
			Assert.StrictEqual(new DerivedClass(), new BaseClass());
		}
	}

	class BaseClass { }

	class DerivedClass : BaseClass
	{
		public override bool Equals(object? obj) =>
			obj is BaseClass || base.Equals(obj);

		public override int GetHashCode() => 0;
	}

	class EnumerableClass(string _, params BaseClass[] bars) :
		IEnumerable<BaseClass>
	{
		readonly string _ = _;
		readonly IEnumerable<BaseClass> bars = bars;

		public IEnumerator<BaseClass> GetEnumerator() => bars.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class MultiComparable(int value) :
		IComparable
	{
		public int Value { get; } = value;

		public int CompareTo(object? obj)
		{
			if (obj is int intObj)
				return Value.CompareTo(intObj);
			else if (obj is MultiComparable multiObj)
				return Value.CompareTo(multiObj.Value);

			throw new InvalidOperationException();
		}
	}

	class ComparableBaseClass(int value) :
		IComparable<ComparableBaseClass>
	{
		public int Value { get; } = value;

		public int CompareTo(ComparableBaseClass? other) => Value.CompareTo(other!.Value);
	}

	class ComparableSubClassA(int value) :
		ComparableBaseClass(value)
	{ }

	class ComparableSubClassB(int value) :
		ComparableBaseClass(value)
	{ }

	class ComparableThrower(int value) :
		IComparable<ComparableThrower>
	{
		public int Value { get; } = value;

		public int CompareTo(ComparableThrower? other) =>
			throw new InvalidOperationException();

		public override bool Equals(object? obj) => Value == ((ComparableThrower?)obj)!.Value;

		public override int GetHashCode() => Value;
	}

	class EquatableBaseClass(int value) :
		IEquatable<EquatableBaseClass>
	{
		public int Value { get; } = value;

		public bool Equals(EquatableBaseClass? other) => Value == other!.Value;
	}

	class EquatableSubClassA(int value) :
		EquatableBaseClass(value)
	{ }

	class EquatableSubClassB(int value) :
		EquatableBaseClass(value)
	{ }

	class StringWrapper(string value) :
		IEquatable<StringWrapper>
	{
		public string Value { get; } = value;

		bool IEquatable<StringWrapper>.Equals(StringWrapper? other) => Value == other!.Value;
	}

	class NonGenericSet : HashSet<string> { }

	class TwoGenericSet<T, U> : HashSet<T> { }

	class ImplicitIEquatableExpected(int value) :
		IEquatable<IntWrapper>
	{
		public int Value { get; } = value;

		public bool Equals(IntWrapper? other) => Value == other!.Value;
	}

	class ExplicitIEquatableExpected(int value) :
		IEquatable<IntWrapper>
	{
		public int Value { get; } = value;

		bool IEquatable<IntWrapper>.Equals(IntWrapper? other) => Value == other!.Value;
	}

	class ImplicitIComparableExpected(int value) :
		IComparable<IntWrapper>
	{
		public int Value { get; } = value;

		public int CompareTo(IntWrapper? other) => Value.CompareTo(other!.Value);
	}

	class ExplicitIComparableActual(int value) :
		IComparable<IntWrapper>
	{
		public int Value { get; } = value;

		int IComparable<IntWrapper>.CompareTo(IntWrapper? other) => Value.CompareTo(other!.Value);
	}

	class IComparableActualThrower(int value) :
		IComparable<IntWrapper>
	{
		public int Value { get; } = value;

		public int CompareTo(IntWrapper? other) =>
			throw new NotSupportedException();

		public override bool Equals(object? obj) => Value == ((IntWrapper?)obj)!.Value;

		public override int GetHashCode() => Value;
	}

	class IntWrapper(int value)
	{
		public int Value { get; } = value;
	}

	class SpyComparable(int result) :
		IComparable
	{
		public bool CompareCalled;

		public int CompareTo(object? obj)
		{
			CompareCalled = true;
			return result;
		}
	}

	class SpyComparable_Generic(int result = 0) :
		IComparable<SpyComparable_Generic>
	{
		public bool CompareCalled;

		public int CompareTo(SpyComparable_Generic? other)
		{
			CompareCalled = true;
			return result;
		}
	}

	class SpyEquatable(bool result = true) :
		IEquatable<SpyEquatable>
	{
		public bool Equals__Called;
		public SpyEquatable? Equals_Other;

		public bool Equals(SpyEquatable? other)
		{
			Equals__Called = true;
			Equals_Other = other;

			return result;
		}
	}

	class NonComparableObject(bool result = true)
	{
		public override bool Equals(object? obj) => result;

		public override int GetHashCode() => 42;
	}

	sealed class RunOnceEnumerable<T>(IEnumerable<T> source) :
		IEnumerable<T>
	{
		private bool _called;

		public IEnumerable<T> Source { get; } = source;

		public IEnumerator<T> GetEnumerator()
		{
			Assert.False(_called, "GetEnumerator() was called more than once");
			_called = true;
			return Source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
