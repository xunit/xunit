using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
				$"Expected: {expected ?? "null"}" + Environment.NewLine +
				$"Actual:   {actual ?? "null"}",
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
		[InlineData(ConsoleKey.A, ConsoleKey.A)]
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
		public void SameValueFromDifferentIntrinsicTypes_Success()
		{
			Assert.Equivalent(12, 12L);
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

	public class ValueTypes_Identical_Deep
	{
		[Fact]
		public void Success()
		{
			var expected = new DeepStruct(new ShallowClass { Value1 = 42, Value2 = "Hello, world!" });
			var actual = new DeepStruct(new ShallowClass { Value1 = 42, Value2 = "Hello, world!" });

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void Failure()
		{
			var expected = new DeepStruct(new ShallowClass { Value1 = 42, Value2 = "Hello, world!" });
			var actual = new DeepStruct(new ShallowClass { Value1 = 13, Value2 = "Hello, world!" });

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Shallow.Value1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   13",
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
				"Expected: \"Hello, world\"" + Environment.NewLine +
				"Actual:   \"Hello, world!\"",
				ex.Message
			);
		}

		[Fact]
		public void NullIsNotEquivalentToEmptyString()
		{
			var ex = Record.Exception(() => Assert.Equivalent(null, string.Empty));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: null" + Environment.NewLine +
				"Actual:   \"\"",
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
		public void Success_IgnorePrivateValue()
		{
			var expected = new PrivateMembersClass(1, "help");
			var actual = new PrivateMembersClass(2, "me");

			Assert.Equivalent(expected, actual);
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
		public void Success_IgnoreStaticValue()
		{
			try
			{
				ShallowClass.StaticValue = 1;
				ShallowClass2.StaticValue = 2;

				var expected = new ShallowClass();
				var actual = new ShallowClass2();

				Assert.Equivalent(expected, actual);
			}
			finally
			{
				ShallowClass.StaticValue = default;
				ShallowClass2.StaticValue = default;
			}
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
				"Expected: \"Hello, world\"" + Environment.NewLine +
				"Actual:   \"Hello, world!\"",
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

	public class ImmutableArrayOfValueTypes_NotStrict
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new[] { 1, 4 }.ToImmutableArray(), new[] { 9, 4, 1 }.ToImmutableArray(), strict: false);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { 1, 4 }.ToImmutableArray() };
			var actual = new { x = new[] { 9, 4, 1 }.ToImmutableArray() };

			Assert.Equivalent(expected, actual, strict: false);
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new[] { 1, 6 }.ToImmutableArray(), new[] { 9, 4, 1 }.ToImmutableArray(), strict: false));

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
			var expected = new { x = new[] { 1, 6 }.ToImmutableArray() };
			var actual = new { x = new[] { 9, 4, 1 }.ToImmutableArray() };

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

	public class ImmutableArrayOfValueTypes_Strict
	{
		[Fact]
		public void Success()
		{
			Assert.Equivalent(new[] { 1, 9, 4 }.ToImmutableArray(), new[] { 9, 4, 1 }.ToImmutableArray(), strict: true);
		}

		[Fact]
		public void Success_EmbeddedArray()
		{
			var expected = new { x = new[] { 1, 9, 4 }.ToImmutableArray() };
			var actual = new { x = new[] { 9, 4, 1 }.ToImmutableArray() };

			Assert.Equivalent(expected, actual, strict: true);
		}

		[Fact]
		public void Failure_ValueNotFoundInActual()
		{
			var ex = Record.Exception(() => Assert.Equivalent(new[] { 1, 6 }.ToImmutableArray(), new[] { 9, 4, 1 }.ToImmutableArray(), strict: true));

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
			var ex = Record.Exception(() => Assert.Equivalent(new[] { 1, 9, 4 }.ToImmutableArray(), new[] { 6, 9, 4, 1 }.ToImmutableArray(), strict: true));

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
			var expected = new { x = new[] { 1, 6 }.ToImmutableArray() };
			var actual = new { x = new[] { 9, 4, 1 }.ToImmutableArray() };

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
			var expected = new { x = new[] { 1, 9, 4 }.ToImmutableArray() };
			var actual = new { x = new[] { 6, 9, 4, 1, 12 }.ToImmutableArray() };

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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: [{ Foo = \"Bar\" }]" + Environment.NewLine +
				"Actual:   [{ Foo = \"Baz\" }] left over from [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: [{ Foo = \"Bar\" }]" + Environment.NewLine +
				"Actual:   [{ Foo = \"Baz\" }] left over from [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: [{ Foo = \"Bar\" }]" + Environment.NewLine +
				"Actual:   [{ Foo = \"Baz\" }] left over from [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: { Foo = \"Biff\" }" + Environment.NewLine +
				"In:       [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
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
				"Expected: [{ Foo = \"Bar\" }]" + Environment.NewLine +
				"Actual:   [{ Foo = \"Baz\" }] left over from [{ Foo = \"Baz\" }, { Foo = \"Bar\" }]",
				ex.Message
			);
		}
	}

	public class EquivalentCollectionsInDifferentTypes
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

		[Fact]
		public void ArrayIsEquivalentToImmutableArray()
		{
			Assert.Equivalent(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }.ToImmutableArray());
		}

		[Fact]
		public void ImmutableArrayIsEquivalentToArray()
		{
			Assert.Equivalent(new[] { 1, 2, 3 }.ToImmutableArray(), new[] { 1, 2, 3 });
		}

		[Fact]
		public void ImmutableListIsEquivalentToImmutableSortedSet()
		{
			Assert.Equivalent(new[] { 1, 2, 3 }.ToImmutableList(), new[] { 1, 2, 3 }.ToImmutableSortedSet());
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

	public class SpecialCases
	{
		// DateTime

		[Fact]
		public void DateTime_Success()
		{
			var expected = new DateTime(2022, 12, 1, 1, 3, 1);
			var actual = new DateTime(2022, 12, 1, 1, 3, 1);

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void DateTime_Failure()
		{
			var expected = new DateTime(2022, 12, 1, 1, 3, 1);
			var actual = new DateTime(2011, 9, 13, 18, 22, 0);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: 2022-12-01T01:03:01.0000000" + Environment.NewLine +
				"Actual:   2011-09-13T18:22:00.0000000",
				ex.Message
			);
		}

		[Fact]
		public void DateTimeToString_Failure()
		{
			var expected = new DateTime(2022, 12, 1, 1, 3, 1);
			var actual = "2022-12-01T01:03:01.0000000";

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: 2022-12-01T01:03:01.0000000" + Environment.NewLine +
				"Actual:   \"2022-12-01T01:03:01.0000000\"",
				ex.Message
			);
			Assert.IsType<ArgumentException>(ex.InnerException);  // Thrown by DateTime.CompareTo
		}

		[Fact]
		public void StringToDateTime_Success()
		{
			var expected = "2022-12-01T01:03:01.0000000";
			var actual = new DateTime(2022, 12, 1, 1, 3, 1);

			Assert.Equivalent(expected, actual);
		}

		// DateTimeOffset

		[Fact]
		public void DateTimeOffset_Success()
		{
			var expected = new DateTimeOffset(2022, 12, 1, 1, 3, 1, TimeSpan.Zero);
			var actual = new DateTimeOffset(2022, 12, 1, 1, 3, 1, TimeSpan.Zero);

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void DateTimeOffset_Failure()
		{
			var expected = new DateTimeOffset(2022, 12, 1, 1, 3, 1, TimeSpan.Zero);
			var actual = new DateTimeOffset(2011, 9, 13, 18, 22, 0, TimeSpan.Zero);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure" + Environment.NewLine +
				"Expected: 2022-12-01T01:03:01.0000000+00:00" + Environment.NewLine +
				"Actual:   2011-09-13T18:22:00.0000000+00:00",
				ex.Message
			);
		}

		// FileSystemInfo-derived types

		[Fact]
		public void DirectoryInfo_Success()
		{
			var assemblyPath = Path.GetDirectoryName(typeof(SpecialCases).Assembly.Location);
			Assert.NotNull(assemblyPath);

			var expected = new DirectoryInfo(assemblyPath);
			var actual = new DirectoryInfo(assemblyPath);

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void DirectoryInfo_Failure()
		{
			var assemblyPath = Path.GetDirectoryName(typeof(SpecialCases).Assembly.Location);
			Assert.NotNull(assemblyPath);
			var assemblyParentPath = Path.GetDirectoryName(assemblyPath);
			Assert.NotNull(assemblyParentPath);
			Assert.NotEqual(assemblyPath, assemblyParentPath);

			var expected = new FileInfo(assemblyPath);
			var actual = new FileInfo(assemblyParentPath);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.StartsWith("Assert.Equivalent() Failure: Mismatched value on member 'FullName'" + Environment.NewLine, ex.Message);
		}

		[Fact]
		public void FileInfo_Success()
		{
			var assembly = typeof(SpecialCases).Assembly.Location;
			var expected = new FileInfo(assembly);
			var actual = new FileInfo(assembly);

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void FileInfo_Failure()
		{
			var expected = new FileInfo(typeof(SpecialCases).Assembly.Location);
			var actual = new FileInfo(typeof(Assert).Assembly.Location);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.StartsWith("Assert.Equivalent() Failure: Mismatched value on member 'FullName'" + Environment.NewLine, ex.Message);
		}

		[Fact]
		public void FileInfoToDirectoryInfo_Failure_TopLevel()
		{
			var location = typeof(SpecialCases).Assembly.Location;
			var expected = new FileInfo(location);
			var actual = new DirectoryInfo(location);

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Types did not match" + Environment.NewLine +
				"Expected type: System.IO.FileInfo" + Environment.NewLine +
				"Actual type:   System.IO.DirectoryInfo",
				ex.Message
			);
		}

		[Fact]
		public void FileInfoToDirectoryInfo_Failure_Embedded()
		{
			var location = typeof(SpecialCases).Assembly.Location;
			var expected = new { Info = new FileInfo(location) };
			var actual = new { Info = new DirectoryInfo(location) };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Types did not match in member 'Info'" + Environment.NewLine +
				"Expected type: System.IO.FileInfo" + Environment.NewLine +
				"Actual type:   System.IO.DirectoryInfo",
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

	public class DepthLimit
	{
		[Fact]
		public void PreventArbitrarilyLargeDepthObjectTree()
		{
			var expected = new InfiniteRecursionClass();
			var actual = new InfiniteRecursionClass();

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Exceeded the maximum depth 50 with 'Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent'; check for infinite recursion or circular references",
				ex.Message
			);
		}

		class InfiniteRecursionClass
		{
			public InfiniteRecursionClass Parent => new();
		}
	}

	public class Indexers
	{
		[Fact]
		public void Equivalent()
		{
			var expected = new ClassWithIndexer { Value = "Hello" };
			var actual = new ClassWithIndexer { Value = "Hello" };

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void NotEquivalent()
		{
			var expected = new ClassWithIndexer { Value = "Hello" };
			var actual = new ClassWithIndexer { Value = "There" };

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Value'" + Environment.NewLine +
				"Expected: \"Hello\"" + Environment.NewLine +
				"Actual:   \"There\"",
				ex.Message
			);
		}
	}

	public class Tuples
	{
		[Fact]
		public void Equivalent()
		{
			var expected = Tuple.Create(42, "Hello world");
			var actual = Tuple.Create(42, "Hello world");

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void NotEquivalent()
		{
			var expected = Tuple.Create(42, "Hello world");
			var actual = Tuple.Create(2112, "Hello world");

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Item1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}
	}

	public class ValueTuples
	{
		[Fact]
		public void Equivalent()
		{
			var expected = (answer: 42, greeting: "Hello world");
			var actual = (answer: 42, greeting: "Hello world");

			Assert.Equivalent(expected, actual);
		}

		[Fact]
		public void NotEquivalent()
		{
			var expected = (answer: 42, greeting: "Hello world");
			var actual = (answer: 2112, greeting: "Hello world");

			var ex = Record.Exception(() => Assert.Equivalent(expected, actual));

			Assert.IsType<EquivalentException>(ex);
			Assert.Equal(
				"Assert.Equivalent() Failure: Mismatched value on member 'Item1'" + Environment.NewLine +
				"Expected: 42" + Environment.NewLine +
				"Actual:   2112",
				ex.Message
			);
		}

		[Fact]
		public void ValueTupleInsideClass_Equivalent()
		{
			var expected = new Person { ID = 42, Relationships = (parent: new Person { ID = 2112 }, child: null) };
			var actual = new Person { ID = 42, Relationships = (parent: new Person { ID = 2112 }, child: null) };

			Assert.Equivalent(expected, actual);
		}

		class Person
		{
			public int ID { get; set; }

			public (Person? parent, Person? child) Relationships;
		}
	}

	class ShallowClass
	{
		public static int StaticValue { get; set; }
		public int Value1;
		public string? Value2 { get; set; }
	}

	class ShallowClass2
	{
		public static int StaticValue { get; set; }
		public int Value1 { get; set; }
		public string? Value2;
	}

	class PrivateMembersClass
	{
		public PrivateMembersClass(int value1, string value2)
		{
			Value1 = value1;
			Value2 = value2;
		}

		private readonly int Value1;
		private string Value2 { get; }
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

	struct DeepStruct
	{
		public DeepStruct(ShallowClass shallow)
		{
			Shallow = shallow;
		}

		public ShallowClass Shallow { get; }
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

	class ClassWithIndexer
	{
		public string? Value;

		public string this[int idx] => idx.ToString();
	}
}
