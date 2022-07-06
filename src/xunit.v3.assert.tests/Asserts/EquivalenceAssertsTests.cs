using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

public class EquivalenceAssertsTests
{
	public class NullValues
	{
		[Fact]
		public void TwoNullsAreEquivalent()
		{
			Assert.Equivalent(null, null);
		}

		[Theory]
		[InlineData(null, 42)]
		[InlineData(42, null)]
		public void NullIsNotEquivalentToNonNull(
			object? expected,
			object? actual)
		{
			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				$"Expected: {expected ?? "(null)"}" + Environment.NewLine +
				$"Actual:   {actual ?? "(null)"}",
				ex.Message
			);
		}
	}

	public class ValueTypes
	{
		[Theory]
		[InlineData(42, 42)]
		[InlineData(2112L, 2112L)]
		[InlineData('a', 'a')]
		[InlineData(1.1f, 1.1f)]
		[InlineData(1.1, 1.1)]
		[InlineData(true, true)]
		public void SameType_Success(
			object? expected,
			object? actual)
		{
			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void SameType_Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(12, 13));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: 12" + Environment.NewLine +
				"Actual:   13",
				ex.Message
			);
		}

		[Fact]
		public void SameValueFromDifferentTypes_Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(12, 12L));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: 12 (System.Int32)" + Environment.NewLine +
				"Actual:   12 (System.Int64)",
				ex.Message
			);
		}
	}

	public class NullableValueTypes
	{
		[Fact]
		public void Success()
		{
			int? expected = 42;
			int? actual = 42;

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			int? expected = 42;
			int? actual = 2112;

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class Strings
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent("Hello", "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent("Hello, world", "Hello, world!"));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: Hello, world" + Environment.NewLine +
				"Actual:   Hello, world!",
				ex.Message
			);
		}
	}

	public class AnonymousTypes_Identical_Shallow
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new { x = 42 }, new { x = 42 });
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new { x = 42 }, new { x = 2112 }));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'x'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class AnonymousTypes_Identical_Deep
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new { x = new { y = 42 } }, new { x = new { y = 42 } });
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new { x = new { y = 42 } }, new { x = new { y = 2112 } }));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'x.y'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class AnonymousTypes_Compatible_Shallow
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new { x = 42, y = 2112 }, new { y = 2112, x = 42 });
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new { x = 42, y = 2600 }, new { y = 2600, x = 2112 }));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'x'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class AnonymousTypes_Compatible_Deep
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new { x = new { y = 2112 }, z = 42 }, new { z = 42, x = new { y = 2112 } });
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new { x = new { y = 2600 }, z = 42 }, new { z = 42, x = new { y = 2112 } }));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'x.y'" + Environment.NewLine +
				"Expected: 2600" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class ComplexTypes_Identical_Shallow_NotStructuralEquatable
	{
		[Fact]
		public void Success()
		{
			var expected = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" };
			var actual = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" };

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			var expected = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" };
			var actual = new ShallowClass { Value1 = 2112, Value2 = "Hello, world!" };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Value1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class ComplexTypes_Identical_Deep_NotStructuralEquatable
	{
		[Fact]
		public void Success()
		{
			var expected = new DeepClass { Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };
			var actual = new DeepClass { Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			var expected = new DeepClass { Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };
			var actual = new DeepClass { Shallow = new ShallowClass { Value1 = 2600, Value2 = "Hello, world!" }, Value3 = 21.12m };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Shallow.Value1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2600",
				ex.Message
			);
		}
	}

	public class ComplexTypes_Compatible_Shallow_NotStructuralEquatable
	{
		[Fact]
		public void Success()
		{
			var expected = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" };
			var actual = new ShallowClass2 { Value1 = 42, Value2 = "Hello, world!" };

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			var expected = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" };
			var actual = new ShallowClass2 { Value1 = 2112, Value2 = "Hello, world!" };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Value1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class ComplexTypes_Compatible_Deep_NotStructuralEquatable
	{
		[Fact]
		public void Success()
		{
			var expected = new DeepClass { Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };
			var actual = new DeepClass2 { Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			var expected = new DeepClass { Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };
			var actual = new DeepClass2 { Shallow = new ShallowClass { Value1 = 2600, Value2 = "Hello, world!" }, Value3 = 21.12m };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Shallow.Value1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2600",
				ex.Message
			);
		}
	}

	public class MixedComplexAndAnonymousTypes
	{
		[Fact]
		public void Success()
		{
			var expected = new { Shallow = new { Value1 = 42, Value2 = "Hello, world!" }, Value3 = 21.12m };
			var actual = new DeepClass { Value3 = 21.12m, Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" } };

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			var expected = new { Shallow = new { Value1 = 42, Value2 = "Hello, world" }, Value3 = 21.12m };
			var actual = new DeepClass { Value3 = 21.12m, Shallow = new ShallowClass { Value1 = 42, Value2 = "Hello, world!" } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Shallow.Value2'" + Environment.NewLine +
				"Expected: Hello, world" + Environment.NewLine +
				"Actual:   Hello, world!",
				ex.Message
			);
		}
	}

	public class MismatchedMembers_NotStrict
	{
		[Fact]
		public void Shallow_Success()
		{
			// Expected can be subset of Actual when strict is false
			Assert.Equivalent(
				new { x = 42 },
				new { x = 42, y = 2112 },
				strict: false
			);
		}

		[Fact]
		public void Deep_Success()
		{
			// Expected can be subset of Actual when strict is false
			Assert.Equivalent(
				new { w = 42, x = new { y = 2112 } },
				new { w = 42, x = new { y = 2112, z = 2600 } },
				strict: false
			);
		}

		[Fact]
		public void Shallow_Failure()
		{
			// Expected can never be superset of Actual
			var ex = Record.Exception(
				() => Assert.Equivalent(
					new { x = 42, y = 2112 },
					new { x = 42, z = 2112 },
					strict: false
				)
			);

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched member list" + Environment.NewLine +
				"Expected: [\"x\", \"y\"]" + Environment.NewLine +
				"Actual:   [\"x\", \"z\"]",
				ex.Message
			);
		}

		[Fact]
		public void Deep_Failure()
		{
			// Expected can never be superset of Actual
			var ex = Record.Exception(
				() => Assert.Equivalent(
					new { w = 42, x = new { y = 2112 } },
					new { w = 42, x = new { z = 2112 } },
					strict: false
				)
			);

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched member list" + Environment.NewLine +
				"Expected: [\"x.y\"]" + Environment.NewLine +
				"Actual:   [\"x.z\"]",
				ex.Message
			);
		}
	}

	public class MismatchedMembers_Strict
	{
		[Fact]
		public void Failure()
		{
			// Expected cannot be subset of Actual when strict is true
			var ex = Record.Exception(
				() => Assert.Equivalent(
					new { x = 42 },
					new { x = 42, y = 2112 },
					strict: true
				)
			);

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched member list" + Environment.NewLine +
				"Expected: [\"x\"]" + Environment.NewLine +
				"Actual:   [\"x\", \"y\"]",
				ex.Message
			);
		}
	}

	public class ArrayOfValueTypes_NotStrict
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new[] { 1, 4 }, new[] { 9, 4, 1 }, strict: false);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { 1, 4 } };
			var actual = new { x = new[] { 9, 4, 1 } };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new[] { 1, 6 }, new[] { 9, 4, 1 }, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: 6" + Environment.NewLine +
				"In:       [9, 4, 1]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray()
		{
			var expected = new { x = new[] { 1, 6 } };
			var actual = new { x = new[] { 9, 4, 1 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: 6" + Environment.NewLine +
				"In:       [9, 4, 1]",
				ex.Message
			);
		}
	}

	public class ArrayOfValueTypes_Strict
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new[] { 1, 9, 4 }, new[] { 9, 4, 1 }, strict: true);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { 1, 9, 4 } };
			var actual = new { x = new[] { 9, 4, 1 } };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Failure_ValueNotFoundInActual()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new[] { 1, 6 }, new[] { 9, 4, 1 }, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: 6" + Environment.NewLine +
				"In:       [9, 4, 1]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_ExtraValueInActual()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new[] { 1, 9, 4 }, new[] { 6, 9, 4, 1 }, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found" + Environment.NewLine +
				"Expected: [1, 9, 4]" + Environment.NewLine +
				"Actual:   [6] left over from [6, 9, 4, 1]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray_ValueNotFoundInActual()
		{
			var expected = new { x = new[] { 1, 6 } };
			var actual = new { x = new[] { 9, 4, 1 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: 6" + Environment.NewLine +
				"In:       [9, 4, 1]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray_ExtraValueInActual()
		{
			var expected = new { x = new[] { 1, 9, 4 } };
			var actual = new { x = new[] { 6, 9, 4, 1, 12 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found in member 'x'" + Environment.NewLine +
				"Expected: [1, 9, 4]" + Environment.NewLine +
				"Actual:   [6, 12] left over from [6, 9, 4, 1, 12]",
				ex.Message
			);
		}
	}

	public class ArrayOfObjects_NotStrict
	{
		[Fact]
		public void Success()
		{
			var expected = new[] { new { Foo = "Bar" } };
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { new { Foo = "Bar" } } };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } } };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Failure()
		{
			var expected = new[] { new { Foo = "Biff" } };
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray()
		{
			var expected = new { x = new[] { new { Foo = "Biff" } } };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}
	}

	public class ArrayOfObjects_Strict
	{
		[Fact]
		public void Success()
		{
			var expected = new[] { new { Foo = "Bar" }, new { Foo = "Baz" } };
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { new { Foo = "Bar" }, new { Foo = "Baz" } } };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } } };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Failure_ValueNotFoundInActual()
		{
			var expected = new[] { new { Foo = "Biff" } };
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_ExtraValueInActual()
		{
			var expected = new[] { new { Foo = "Bar" } };
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found" + Environment.NewLine +
				"Expected: [{ Foo = Bar }]" + Environment.NewLine +
				"Actual:   [{ Foo = Baz }] left over from [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray_ValueNotFoundInActual()
		{
			var expected = new { x = new[] { new { Foo = "Biff" } } };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray_ExtraValueInActual()
		{
			var expected = new { x = new[] { new { Foo = "Bar" } } };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found in member 'x'" + Environment.NewLine +
				"Expected: [{ Foo = Bar }]" + Environment.NewLine +
				"Actual:   [{ Foo = Baz }] left over from [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}
	}

	public class ListOfObjects_NotStrict
	{
		[Fact]
		public void Success()
		{
			var expected = new[] { new { Foo = "Bar" } }.ToList();
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList();

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { new { Foo = "Bar" } }.ToList() };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList() };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Failure()
		{
			var expected = new[] { new { Foo = "Biff" } }.ToList();
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList();

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray()
		{
			var expected = new { x = new[] { new { Foo = "Biff" } }.ToList() };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList() };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}
	}

	public class ListOfObjects_Strict
	{
		[Fact]
		public void Success()
		{
			var expected = new[] { new { Foo = "Bar" }, new { Foo = "Baz" } }.ToList();
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList();

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Success_EmbeddedList()
		{
			var expected = new { x = new[] { new { Foo = "Bar" }, new { Foo = "Baz" } }.ToList() };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList() };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Failure_ValueNotFoundInActual()
		{
			var expected = new[] { new { Foo = "Biff" } }.ToList();
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList();

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_ExtraValueInActual()
		{
			var expected = new[] { new { Foo = "Bar" } }.ToList();
			var actual = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList();

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found" + Environment.NewLine +
				"Expected: [{ Foo = Bar }]" + Environment.NewLine +
				"Actual:   [{ Foo = Baz }] left over from [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray_ValueNotFoundInActual()
		{
			var expected = new { x = new[] { new { Foo = "Biff" } }.ToList() };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList() };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: { Foo = Biff }" + Environment.NewLine +
				"In:       [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedArray_ExtraValueInActual()
		{
			var expected = new { x = new[] { new { Foo = "Bar" } }.ToList() };
			var actual = new { x = new[] { new { Foo = "Baz" }, new { Foo = "Bar" } }.ToList() };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found in member 'x'" + Environment.NewLine +
				"Expected: [{ Foo = Bar }]" + Environment.NewLine +
				"Actual:   [{ Foo = Baz }] left over from [{ Foo = Baz }, { Foo = Bar }]",
				ex.Message
			);
		}
	}

	public class ArraysAndListsAreEquivalent
	{
		[Fact]
		public void ArrayIsEquivalentToList()
		{
			Assert.Equivalent(new[] { 1, 2, 3 }, new List<int> { 1, 2, 3 });
		}

		[Fact]
		public void ListIsEquivalentToArray()
		{
			Assert.Equivalent(new List<int> { 1, 2, 3 }, new[] { 1, 2, 3 });
		}
	}

	public class Dictionaries_NotStrict
	{
		[Fact]
		public void Success()
		{
			var expected = new Dictionary<string, int> { ["Foo"] = 42 };
			var actual = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void SuccessWithArrayValues()
		{
			var expected = new Dictionary<string, int[]> { ["Foo"] = new[] { 42 } };
			var actual = new Dictionary<string, int[]> { ["Foo"] = new[] { 42 }, ["Bar"] = new[] { 2112 } };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void SuccessWithListValues()
		{
			var expected = new Dictionary<string, List<int>> { ["Foo"] = new List<int> { 42 } };
			var actual = new Dictionary<string, List<int>> { ["Foo"] = new List<int> { 42 }, ["Bar"] = new List<int> { 2112 } };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Success_EmbeddedDictionary()
		{
			var expected = new { x = new Dictionary<string, int> { ["Foo"] = 42 } };
			var actual = new { x = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 } };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Failure()
		{
			var expected = new Dictionary<string, int> { ["Foo"] = 16 };
			var actual = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: [\"Foo\"] = 16" + Environment.NewLine +
				"In:       [[\"Foo\"] = 42, [\"Bar\"] = 2112]",
				ex.Message
			);
		}

		[Fact]
		public void FailureWithArrayValues()
		{
			var expected = new Dictionary<string, int[]> { ["Foo"] = new[] { 16 } };
			var actual = new Dictionary<string, int[]> { ["Foo"] = new[] { 42 }, ["Bar"] = new[] { 2112 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: [\"Foo\"] = [16]" + Environment.NewLine +
				"In:       [[\"Foo\"] = [42], [\"Bar\"] = [2112]]",
				ex.Message
			);
		}

		[Fact]
		public void FailureWithListValues()
		{
			var expected = new Dictionary<string, List<int>> { ["Foo"] = new List<int> { 16 } };
			var actual = new Dictionary<string, List<int>> { ["Foo"] = new List<int> { 42 }, ["Bar"] = new List<int> { 2112 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: [\"Foo\"] = [16]" + Environment.NewLine +
				"In:       [[\"Foo\"] = [42], [\"Bar\"] = [2112]]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedDictionary()
		{
			var expected = new { x = new Dictionary<string, int> { ["Foo"] = 16 } };
			var actual = new { x = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: [\"Foo\"] = 16" + Environment.NewLine +
				"In:       [[\"Foo\"] = 42, [\"Bar\"] = 2112]",
				ex.Message
			);
		}
	}

	public class Dictionaries_Strict
	{
		[Fact]
		public void Success()
		{
			var expected = new Dictionary<string, int> { ["Bar"] = 2112, ["Foo"] = 42 };
			var actual = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Success_EmbeddedDictionary()
		{
			var expected = new { x = new Dictionary<string, int> { ["Bar"] = 2112, ["Foo"] = 42 } };
			var actual = new { x = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 } };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Failure_ValueNotFoundInActual()
		{
			var expected = new Dictionary<string, int> { ["Foo"] = 16 };
			var actual = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found" + Environment.NewLine +
				"Expected: [\"Foo\"] = 16" + Environment.NewLine +
				"In:       [[\"Foo\"] = 42, [\"Bar\"] = 2112]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_ExtraValueInActual()
		{
			var expected = new Dictionary<string, int> { ["Bar"] = 2112, ["Foo"] = 42 };
			var actual = new Dictionary<string, int> { ["Foo"] = 42, ["Biff"] = 2600, ["Bar"] = 2112 };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found" + Environment.NewLine +
				"Expected: [[\"Bar\"] = 2112, [\"Foo\"] = 42]" + Environment.NewLine +
				"Actual:   [[\"Biff\"] = 2600] left over from [[\"Foo\"] = 42, [\"Biff\"] = 2600, [\"Bar\"] = 2112]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedDictionary_ValueNotFoundInActual()
		{
			var expected = new { x = new Dictionary<string, int> { ["Foo"] = 16 } };
			var actual = new { x = new Dictionary<string, int> { ["Foo"] = 42, ["Bar"] = 2112 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'x'" + Environment.NewLine +
				"Expected: [\"Foo\"] = 16" + Environment.NewLine +
				"In:       [[\"Foo\"] = 42, [\"Bar\"] = 2112]",
				ex.Message
			);
		}

		[Fact]
		public void Failure_EmbeddedDictionary_ExtraValueInActual()
		{
			var expected = new { x = new Dictionary<string, int> { ["Bar"] = 2112, ["Foo"] = 42 } };
			var actual = new { x = new Dictionary<string, int> { ["Foo"] = 42, ["Biff"] = 2600, ["Bar"] = 2112 } };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Extra values found in member 'x'" + Environment.NewLine +
				"Expected: [[\"Bar\"] = 2112, [\"Foo\"] = 42]" + Environment.NewLine +
				"Actual:   [[\"Biff\"] = 2600] left over from [[\"Foo\"] = 42, [\"Biff\"] = 2600, [\"Bar\"] = 2112]",
				ex.Message
			);
		}
	}

	public class KeyValuePairs_NotStrict
	{
		[Fact]
		public void Success()
		{
			var expected = new KeyValuePair<int, int[]>(42, new[] { 1, 4 });
			var actual = new KeyValuePair<int, int[]>(42, new[] { 9, 4, 1 });

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Failure_Key()
		{
			var expected = new KeyValuePair<int, int[]>(42, new[] { 1, 4 });
			var actual = new KeyValuePair<int, int[]>(41, new[] { 9, 4, 1 });

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Key'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   41",
				ex.Message
			);
		}

		[Fact]
		public void Failure_Value()
		{
			var expected = new KeyValuePair<int, int[]>(42, new[] { 1, 6 });
			var actual = new KeyValuePair<int, int[]>(42, new[] { 9, 4, 1 });

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: false));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'Value'" + Environment.NewLine +
				"Expected: 6" + Environment.NewLine +
				"In:       [9, 4, 1]",
				ex.Message
			);
		}
	}

	public class KeyValuePairs_Strict
	{
		[Fact]
		public void Success()
		{
			var expected = new KeyValuePair<int, int[]>(42, new[] { 1, 4 });
			var actual = new KeyValuePair<int, int[]>(42, new[] { 4, 1 });

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Failure_Key()
		{
			var expected = new KeyValuePair<int, int[]>(42, new[] { 1, 4 });
			var actual = new KeyValuePair<int, int[]>(41, new[] { 4, 1 });

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Key'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   41",
				ex.Message
			);
		}

		[Fact]
		public void Failure_Value()
		{
			var expected = new KeyValuePair<int, int[]>(42, new[] { 1, 6 });
			var actual = new KeyValuePair<int, int[]>(42, new[] { 4, 1 });

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual, strict: true));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Collection value not found in member 'Value'" + Environment.NewLine +
				"Expected: 6" + Environment.NewLine +
				"In:       [4, 1]",
				ex.Message
			);
		}
	}

	public class CircularReferences
	{
		[Fact]
		public void Expected_Shallow()
		{
			var expected = new SelfReferential(circularReference: true);
			var actual = new SelfReferential(circularReference: false);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal("Assert.Equivalent() Failure: Circular reference found in 'expected.Other'", ex.Message);
		}

		[Fact]
		public void Actual_Shallow()
		{
			var expected = new SelfReferential(circularReference: false);
			var actual = new SelfReferential(circularReference: true);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal("Assert.Equivalent() Failure: Circular reference found in 'actual.Other'", ex.Message);
		}
	}

	class ShallowClass
	{
		public int Value1;
		public string? Value2 { get; set; }
	}

	class ShallowClass2
	{
		public int Value1 { get; set; }
		public string? Value2;
	}

	class DeepClass
	{
		public decimal Value3;

		public ShallowClass? Shallow { get; set; }
	}

	class DeepClass2
	{
		public decimal Value3 { get; set; }
		public ShallowClass? Shallow;
	}

	class SelfReferential
	{
		public SelfReferential(bool circularReference)
		{
			// When we don't want this object to be self-referential, we need to make *another*
			// object here instead, since a null value would end up short circuiting the
			// circular reference check before we saw it. We don't anticipate people having
			// to do strange things like this; it's just for testing. :)
			Other = circularReference ? this : new SelfReferential(true);
		}

		public SelfReferential Other { get; }
	}
}
