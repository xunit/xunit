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

				void assertFailure(Action action)
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

				void assertFailure(Action action)
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

				void assertFailure(Action action)
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

				void assertFailure(Action action)
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

				void assertFailure(Action action)
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

				void assertFailure(Action action)
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
				var expected = new string[] { "foo", "bar" };
				var actual = new ReadOnlyCollection<string>(new[] { "bar", "foo" });

				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"                                      ↓ (pos 0)" + Environment.NewLine +
						"Expected: string[]                   [\"foo\", \"bar\"]" + Environment.NewLine +
						"Actual:   ReadOnlyCollection<string> [\"bar\", \"foo\"]" + Environment.NewLine +
						"                                      ↑ (pos 0)",
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

				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"                           ↓ (pos 1)" + Environment.NewLine +
						"Expected: string[] [\"foo\", \"bar\"]" + Environment.NewLine +
						"Actual:   object[] [\"foo\", \"baz\"]" + Environment.NewLine +
						"                           ↑ (pos 1)",
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
				var expected = Array.CreateInstance(typeof(int), new[] { 1 }, new[] { 1 });
				expected.SetValue(42, 1);
				var actual = Array.CreateInstance(typeof(int), new[] { 1 }, new[] { 1 });
				actual.SetValue(42, 1);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void NonZeroBoundedArrays_NotEqual()
			{
				var expected = Array.CreateInstance(typeof(int), new[] { 1 }, new[] { 1 });
				expected.SetValue(42, 1);
				var actual = Array.CreateInstance(typeof(int), new[] { 1 }, new[] { 0 });
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

				void assertFailure(Action action)
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

				void assertFailure(Action action)
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

				Assert.Equal(expected, (ISet<string>)actual);
				Assert.Equal(expected, (object)actual);

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
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void InOrder_NotEqual()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "bar", "baz" };

				void assertFailure(Action action)
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
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void OutOfOrder_Equal()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "foo", "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void OutOfOrder_NotEqual()
			{
				var expected = new NonGenericSet { "bar", "foo" };
				var actual = new NonGenericSet { "foo", "baz" };

				void assertFailure(Action action)
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
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void DifferentContents()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new NonGenericSet { "bar", "foo" };

				void assertFailure(Action action)
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
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void DifferentTypes_Equal()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void DifferentTypes_NotEqual()
			{
				var expected = new NonGenericSet { "bar" };
				var actual = new HashSet<string> { "baz" };

				void assertFailure(Action action)
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
				assertFailure(() => Assert.Equal(expected, (object)actual));
			}

			[Fact]
			public void TwoGenericSubClass_Equal()
			{
				var expected = new TwoGenericSet<string, int> { "foo", "bar" };
				var actual = new TwoGenericSet<string, int> { "foo", "bar" };

				Assert.Equal(expected, actual);
				Assert.Equal(expected, (ISet<string>)actual);
				Assert.Equal(expected, (object)actual);
			}

			[Fact]
			public void TwoGenericSubClass_NotEqual()
			{
				var expected = new TwoGenericSet<string, int> { "foo", "bar" };
				var actual = new TwoGenericSet<string, int> { "foo", "baz" };

				void assertFailure(Action action)
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
				assertFailure(() => Assert.Equal(expected, (object)actual));
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
					$"Expected: {ArgumentFormatter2.Format(expected)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(actual)}",
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
					$"Expected: {ArgumentFormatter2.Format(date1)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(date2)} (difference {difference} is larger than {precision})",
					ex.Message
				);

				// expected later than actual
				var ex2 = Record.Exception(() => Assert.Equal(date2, date1, precision));

				Assert.IsType<EqualException>(ex2);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter2.Format(date2)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(date1)} (difference {difference} is larger than {precision})",
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
					$"Expected: {ArgumentFormatter2.Format(expected)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(actual)}",
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
					$"Expected: {ArgumentFormatter2.Format(expected)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(actual)}",
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
					$"Expected: {ArgumentFormatter2.Format(date1)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(date2)} (difference {difference} is larger than {precision})",
					ex.Message
				);

				// expected later than actual
				var ex2 = Record.Exception(() => Assert.Equal(date2, date1, precision));

				Assert.IsType<EqualException>(ex2);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter2.Format(date2)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(date1)} (difference {difference} is larger than {precision})",
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
					$"Expected: {ArgumentFormatter2.Format(date1)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(date2)} (difference {difference} is larger than {precision})",
					ex.Message
				);

				// expected later than actual
				var ex2 = Record.Exception(() => Assert.Equal(date2, date1, precision));

				Assert.IsType<EqualException>(ex2);
				Assert.Equal(
					$"Assert.Equal() Failure: Values differ" + Environment.NewLine +
					$"Expected: {ArgumentFormatter2.Format(date2)}" + Environment.NewLine +
					$"Actual:   {ArgumentFormatter2.Format(date1)} (difference {difference} is larger than {precision})",
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

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure" + Environment.NewLine +
					"Expected: Not [\"foo\", \"bar\"]" + Environment.NewLine +
					"Actual:   [\"foo\", \"bar\"]",
					ex.Message
				);
			}

			assertFailure(() => Assert.NotEqual(expected, actual));
			assertFailure(() => Assert.NotEqual(expected, (object)actual));
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

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure" + Environment.NewLine +
					"Expected: Not [\"foo\", \"bar\"]" + Environment.NewLine +
					"Actual:   [\"foo\", \"bar\"]",
					ex.Message
				);
			}

			assertFailure(() => Assert.NotEqual(expected, actual));
			assertFailure(() => Assert.NotEqual(expected, (object)actual));
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

	public class NotEqual_Comparer
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
			var ex = Record.Exception(() => Assert.NotEqual(0.11111M, 0.11444M, 2));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				$"Assert.NotEqual() Failure" + Environment.NewLine +
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
			var ex = Record.Exception(() => Assert.NotEqual(0.11111, 0.11444, 2));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				$"Assert.NotEqual() Failure" + Environment.NewLine +
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
			var ex = Record.Exception(() => Assert.NotStrictEqual(new DerivedClass(), new BaseClass()));

			Assert.IsType<NotEqualException>(ex);
			Assert.Equal(
				@"Assert.NotEqual() Failure" + Environment.NewLine +
				@"Expected: Not DerivedClass { }" + Environment.NewLine +
				@"Actual:   BaseClass { }",
				ex.Message
			);
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
			var ex = Record.Exception(() => Assert.StrictEqual(42, 2112));

			Assert.IsType<EqualException>(ex);
			Assert.Equal(
				"Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}

		[Fact]
		public static void Collection_Failure()
		{
			var expected = new EnumerableClass("ploeh");
			var actual = new EnumerableClass("fnaah");

			var ex = Record.Exception(() => Assert.StrictEqual(expected, actual));

			Assert.IsType<EqualException>(ex);
			Assert.Equal(
				"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
				"Expected: []" + Environment.NewLine +
				"Actual:   []",
				ex.Message
			);
		}
	}

	class BaseClass { }

	class DerivedClass : BaseClass
	{
		public override bool Equals(object? obj) =>
			obj is BaseClass || base.Equals(obj);

		public override int GetHashCode() => 0;
	}

	class EnumerableClass : IEnumerable<BaseClass>
	{
		private readonly IEnumerable<BaseClass> bars;

		public EnumerableClass(string _, params BaseClass[] bars)
		{
			this.bars = bars;
		}

		public IEnumerator<BaseClass> GetEnumerator() => bars.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class MultiComparable : IComparable
	{
		public int Value { get; }

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

	class ComparableBaseClass : IComparable<ComparableBaseClass>
	{
		public int Value { get; }

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

	class EquatableBaseClass : IEquatable<EquatableBaseClass>
	{
		public int Value { get; }

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

	class StringWrapper : IEquatable<StringWrapper>
	{
		public string Value { get; }

		public StringWrapper(string value)
		{
			Value = value;
		}

		bool IEquatable<StringWrapper>.Equals(StringWrapper? other) => Value == other!.Value;
	}

	class NonGenericSet : HashSet<string> { }

	class TwoGenericSet<T, U> : HashSet<T> { }

	class ImplicitIEquatableExpected : IEquatable<IntWrapper>
	{
		public int Value { get; }

		public ImplicitIEquatableExpected(int value)
		{
			Value = value;
		}

		public bool Equals(IntWrapper? other) => Value == other!.Value;
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

	class ImplicitIComparableExpected : IComparable<IntWrapper>
	{
		public int Value { get; }

		public ImplicitIComparableExpected(int value)
		{
			Value = value;
		}

		public int CompareTo(IntWrapper? other) => Value.CompareTo(other!.Value);
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
		readonly int result;

		public bool CompareCalled;

		public SpyComparable(int result)
		{
			this.result = result;
		}

		public int CompareTo(object? obj)
		{
			CompareCalled = true;
			return result;
		}
	}

	class SpyComparable_Generic : IComparable<SpyComparable_Generic>
	{
		int result;

		public bool CompareCalled;

		public SpyComparable_Generic(int result = 0)
		{
			this.result = result;
		}

		public int CompareTo(SpyComparable_Generic? other)
		{
			CompareCalled = true;
			return result;
		}
	}

	class SpyEquatable : IEquatable<SpyEquatable>
	{
		bool result;
		public bool Equals__Called;
		public SpyEquatable? Equals_Other;

		public SpyEquatable(bool result = true)
		{
			this.result = result;
		}

		public bool Equals(SpyEquatable? other)
		{
			Equals__Called = true;
			Equals_Other = other;

			return result;
		}
	}

	class NonComparableObject
	{
		bool result;

		public NonComparableObject(bool result = true)
		{
			this.result = result;
		}

		public override bool Equals(object? obj) => result;

		public override int GetHashCode() => 42;
	}
}
