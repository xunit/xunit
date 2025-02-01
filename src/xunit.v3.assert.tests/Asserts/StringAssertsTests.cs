using System;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Sdk;

public class StringAssertsTests
{
	public class Contains
	{
		[Fact]
		public void CanSearchForSubstrings()
		{
			Assert.Contains("wor", "Hello, world!");
			Assert.Contains("wor".Memoryify(), "Hello, world!".Memoryify());
			Assert.Contains("wor".AsMemory(), "Hello, world!".Memoryify());
			Assert.Contains("wor".Memoryify(), "Hello, world!".AsMemory());
			Assert.Contains("wor".AsMemory(), "Hello, world!".AsMemory());
			Assert.Contains("wor".Spanify(), "Hello, world!".Spanify());
			Assert.Contains("wor".AsSpan(), "Hello, world!".Spanify());
			Assert.Contains("wor".Spanify(), "Hello, world!".AsSpan());
			Assert.Contains("wor".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SubstringContainsIsCaseSensitiveByDefault()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"Hello, world!\"" + Environment.NewLine +
					"Not found: \"WORLD\"",
					ex.Message
				);
			}

			verify(() => Assert.Contains("WORLD", "Hello, world!"));
			verify(() => Assert.Contains("WORLD".Memoryify(), "Hello, world!".Memoryify()));
			verify(() => Assert.Contains("WORLD".AsMemory(), "Hello, world!".Memoryify()));
			verify(() => Assert.Contains("WORLD".Memoryify(), "Hello, world!".AsMemory()));
			verify(() => Assert.Contains("WORLD".AsMemory(), "Hello, world!".AsMemory()));
			verify(() => Assert.Contains("WORLD".Spanify(), "Hello, world!".Spanify()));
			verify(() => Assert.Contains("WORLD".AsSpan(), "Hello, world!".Spanify()));
			verify(() => Assert.Contains("WORLD".Spanify(), "Hello, world!".AsSpan()));
			verify(() => Assert.Contains("WORLD".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void SubstringNotFound()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"Hello, world!\"" + Environment.NewLine +
					"Not found: \"hey\"",
					ex.Message
				);
			}

			verify(() => Assert.Contains("hey", "Hello, world!"));
			verify(() => Assert.Contains("hey".Memoryify(), "Hello, world!".Memoryify()));
			verify(() => Assert.Contains("hey".AsMemory(), "Hello, world!".Memoryify()));
			verify(() => Assert.Contains("hey".Memoryify(), "Hello, world!".AsMemory()));
			verify(() => Assert.Contains("hey".AsMemory(), "Hello, world!".AsMemory()));
			verify(() => Assert.Contains("hey".Spanify(), "Hello, world!".Spanify()));
			verify(() => Assert.Contains("hey".AsSpan(), "Hello, world!".Spanify()));
			verify(() => Assert.Contains("hey".Spanify(), "Hello, world!".AsSpan()));
			verify(() => Assert.Contains("hey".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void NullActualStringThrows()
		{
			var ex = Record.Exception(() => Assert.Contains("foo", default(string)));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
				"String:    null" + Environment.NewLine +
				"Not found: \"foo\"",
				ex.Message
			);
		}

		[Fact]
		public void VeryLongStrings()
		{
			var expected = "We are looking for something that is actually very long as well";
			var actual = "This is a relatively long string so that we can see the truncation in action";

			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					$"String:    \"This is a relatively long string so that we can se\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
					$"Not found: \"We are looking for something that is actually very\"{ArgumentFormatter.Ellipsis}",
					ex.Message
				);
			}

			verify(() => Assert.Contains(expected, actual));
			verify(() => Assert.Contains(expected.Memoryify(), actual.Memoryify()));
			verify(() => Assert.Contains(expected.AsMemory(), actual.Memoryify()));
			verify(() => Assert.Contains(expected.Memoryify(), actual.AsMemory()));
			verify(() => Assert.Contains(expected.AsMemory(), actual.AsMemory()));
			verify(() => Assert.Contains(expected.Spanify(), actual.Spanify()));
			verify(() => Assert.Contains(expected.AsSpan(), actual.Spanify()));
			verify(() => Assert.Contains(expected.Spanify(), actual.AsSpan()));
			verify(() => Assert.Contains(expected.AsSpan(), actual.AsSpan()));
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitive()
		{
			Assert.Contains("WORLD", "Hello, world!", StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".AsMemory(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".Memoryify(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".AsSpan(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".Spanify(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.Contains("WORLD".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public void CanSearchForSubstrings()
		{
			Assert.DoesNotContain("hey", "Hello, world!");
			Assert.DoesNotContain("hey".Memoryify(), "Hello, world!".Memoryify());
			Assert.DoesNotContain("hey".AsMemory(), "Hello, world!".Memoryify());
			Assert.DoesNotContain("hey".Memoryify(), "Hello, world!".AsMemory());
			Assert.DoesNotContain("hey".AsMemory(), "Hello, world!".AsMemory());
			Assert.DoesNotContain("hey".Spanify(), "Hello, world!".Spanify());
			Assert.DoesNotContain("hey".AsSpan(), "Hello, world!".Spanify());
			Assert.DoesNotContain("hey".Spanify(), "Hello, world!".AsSpan());
			Assert.DoesNotContain("hey".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SubstringDoesNotContainIsCaseSensitiveByDefault()
		{
			Assert.DoesNotContain("WORLD", "Hello, world!");
			Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".Memoryify());
			Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".Memoryify());
			Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".AsMemory());
			Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".AsMemory());
			Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".Spanify());
			Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".Spanify());
			Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".AsSpan());
			Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SubstringFound()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                ↓ (pos 7)" + Environment.NewLine +
					"String: \"Hello, world!\"" + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}

			verify(() => Assert.DoesNotContain("world", "Hello, world!"));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world!".Memoryify()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "Hello, world!".Memoryify()));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world!".AsMemory()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "Hello, world!".AsMemory()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "Hello, world!".Spanify()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world!".Spanify()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "Hello, world!".AsSpan()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void NullActualStringDoesNotThrow()
		{
			Assert.DoesNotContain("foo", (string?)null);
		}

		[Fact]
		public void VeryLongString_FoundAtFront()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                ↓ (pos 7)" + Environment.NewLine +
					$"String: \"Hello, world from a very long string that will end\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}

			verify(() => Assert.DoesNotContain("world", "Hello, world from a very long string that will end up being truncated"));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world from a very long string that will end up being truncated".Memoryify()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "Hello, world from a very long string that will end up being truncated".Memoryify()));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world from a very long string that will end up being truncated".AsMemory()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "Hello, world from a very long string that will end up being truncated".AsMemory()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "Hello, world from a very long string that will end up being truncated".Spanify()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world from a very long string that will end up being truncated".Spanify()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "Hello, world from a very long string that will end up being truncated".AsSpan()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world from a very long string that will end up being truncated".AsSpan()));
		}

		[Fact]
		public void VeryLongString_FoundInMiddle()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                                     ↓ (pos 50)" + Environment.NewLine +
					$"String: {ArgumentFormatter.Ellipsis}\" string that has 'Hello, world' placed in the midd\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}

			verify(() => Assert.DoesNotContain("world", "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction"));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".Memoryify()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".Memoryify()));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".AsMemory()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".AsMemory()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".Spanify()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".Spanify()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".AsSpan()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".AsSpan()));
		}

		[Fact]
		public void VeryLongString_FoundAtEnd()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                                                        ↓ (pos 89)" + Environment.NewLine +
					$"String: {ArgumentFormatter.Ellipsis}\"om the front truncated, just to say 'Hello, world'\"" + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}

			verify(() => Assert.DoesNotContain("world", "This is a relatively long string that will from the front truncated, just to say 'Hello, world'"));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".Memoryify()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".Memoryify()));
			verify(() => Assert.DoesNotContain("world".Memoryify(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".AsMemory()));
			verify(() => Assert.DoesNotContain("world".AsMemory(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".AsMemory()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".Spanify()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".Spanify()));
			verify(() => Assert.DoesNotContain("world".Spanify(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".AsSpan()));
			verify(() => Assert.DoesNotContain("world".AsSpan(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".AsSpan()));
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitive()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                ↓ (pos 7)" + Environment.NewLine +
					"String: \"Hello, world!\"" + Environment.NewLine +
					"Found:  \"WORLD\"",
					ex.Message
				);
			}

			verify(() => Assert.DoesNotContain("WORLD", "Hello, world!", StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase));
			verify(() => Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase));
		}
	}

	public class DoesNotMatch_Pattern
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegexPattern", () => Assert.DoesNotMatch((string?)null!, "Hello, world!"));
		}

		[Fact]
		public void Success()
		{
			Assert.DoesNotMatch(@"\d", "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.DoesNotMatch("ll", "Hello, world!"));

			Assert.IsType<DoesNotMatchException>(ex);
			Assert.Equal(
				"Assert.DoesNotMatch() Failure: Match found" + Environment.NewLine +
				"           ↓ (pos 2)" + Environment.NewLine +
				"String: \"Hello, world!\"" + Environment.NewLine +
				"RegEx:  \"ll\"",
				ex.Message
			);
		}
	}

	public class DoesNotMatch_Regex
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegex", () => Assert.DoesNotMatch((Regex?)null!, "Hello, world!"));
		}

		[Fact]
		public void Success()
		{
			Assert.DoesNotMatch(new Regex(@"\d"), "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.DoesNotMatch(new Regex(@"ll"), "Hello, world!"));

			Assert.IsType<DoesNotMatchException>(ex);
			Assert.Equal(
				"Assert.DoesNotMatch() Failure: Match found" + Environment.NewLine +
				"           ↓ (pos 2)" + Environment.NewLine +
				"String: \"Hello, world!\"" + Environment.NewLine +
				"RegEx:  \"ll\"",
				ex.Message
			);
		}
	}

	public class Empty
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("value", () => Assert.Empty(default(string)!));
		}

		[Fact]
		public static void EmptyString()
		{
			Assert.Empty("");
		}

		[Fact]
		public static void NonEmptyString()
		{
			var ex = Record.Exception(() => Assert.Empty("Foo"));

			Assert.IsType<EmptyException>(ex);
			Assert.Equal(
				"Assert.Empty() Failure: String was not empty" + Environment.NewLine +
				"String: \"Foo\"",
				ex.Message
			);
		}
	}

	public class EndsWith
	{
		[Fact]
		public void Success()
		{
			Assert.EndsWith("world!", "Hello, world!");
			Assert.EndsWith("world!".Memoryify(), "Hello, world!".Memoryify());
			Assert.EndsWith("world!".AsMemory(), "Hello, world!".Memoryify());
			Assert.EndsWith("world!".Memoryify(), "Hello, world!".AsMemory());
			Assert.EndsWith("world!".AsMemory(), "Hello, world!".AsMemory());
			Assert.EndsWith("world!".Spanify(), "Hello, world!".Spanify());
			Assert.EndsWith("world!".AsSpan(), "Hello, world!".Spanify());
			Assert.EndsWith("world!".Spanify(), "Hello, world!".AsSpan());
			Assert.EndsWith("world!".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void Failure()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       \"Hello, world!\"" + Environment.NewLine +
					"Expected end: \"hey\"",
					ex.Message
				);
			}

			verify(() => Assert.EndsWith("hey", "Hello, world!"));
			verify(() => Assert.EndsWith("hey".Memoryify(), "Hello, world!".Memoryify()));
			verify(() => Assert.EndsWith("hey".AsMemory(), "Hello, world!".Memoryify()));
			verify(() => Assert.EndsWith("hey".Memoryify(), "Hello, world!".AsMemory()));
			verify(() => Assert.EndsWith("hey".AsMemory(), "Hello, world!".AsMemory()));
			verify(() => Assert.EndsWith("hey".Spanify(), "Hello, world!".Spanify()));
			verify(() => Assert.EndsWith("hey".AsSpan(), "Hello, world!".Spanify()));
			verify(() => Assert.EndsWith("hey".Spanify(), "Hello, world!".AsSpan()));
			verify(() => Assert.EndsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void CaseSensitiveByDefault()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       \"world!\"" + Environment.NewLine +
					"Expected end: \"WORLD!\"",
					ex.Message
				);
			}

			verify(() => Assert.EndsWith("WORLD!", "world!"));
			verify(() => Assert.EndsWith("WORLD!".Memoryify(), "world!".Memoryify()));
			verify(() => Assert.EndsWith("WORLD!".AsMemory(), "world!".Memoryify()));
			verify(() => Assert.EndsWith("WORLD!".Memoryify(), "world!".AsMemory()));
			verify(() => Assert.EndsWith("WORLD!".AsMemory(), "world!".AsMemory()));
			verify(() => Assert.EndsWith("WORLD!".Spanify(), "world!".Spanify()));
			verify(() => Assert.EndsWith("WORLD!".AsSpan(), "world!".Spanify()));
			verify(() => Assert.EndsWith("WORLD!".Spanify(), "world!".AsSpan()));
			verify(() => Assert.EndsWith("WORLD!".AsSpan(), "world!".AsSpan()));
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!", "Hello, world!", StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".AsMemory(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Memoryify(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".AsSpan(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Spanify(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void NullString()
		{
			var ex = Record.Exception(() => Assert.EndsWith("foo", null));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       null" + Environment.NewLine +
				"Expected end: \"foo\"",
				ex.Message
			);
		}

		[Fact]
		public void Truncation()
		{
			var expected = "This is a long string that we're looking for at the end";
			var actual = "This is the long string that we expected to find this ending inside";

			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       " + ArgumentFormatter.Ellipsis + "\"string that we expected to find this ending inside\"" + Environment.NewLine +
					"Expected end: \"This is a long string that we're looking for at th\"" + ArgumentFormatter.Ellipsis,
					ex.Message
				);
			}

			verify(() => Assert.EndsWith(expected, actual));
			verify(() => Assert.EndsWith(expected.Memoryify(), actual.Memoryify()));
			verify(() => Assert.EndsWith(expected.AsMemory(), actual.Memoryify()));
			verify(() => Assert.EndsWith(expected.Memoryify(), actual.AsMemory()));
			verify(() => Assert.EndsWith(expected.AsMemory(), actual.AsMemory()));
			verify(() => Assert.EndsWith(expected.Spanify(), actual.Spanify()));
			verify(() => Assert.EndsWith(expected.AsSpan(), actual.Spanify()));
			verify(() => Assert.EndsWith(expected.Spanify(), actual.AsSpan()));
			verify(() => Assert.EndsWith(expected.AsSpan(), actual.AsSpan()));
		}
	}

	public class Equal
	{
		[Theory]
		// Null values
		[InlineData(null, null, false, false, false, false)]
		// Empty values
		[InlineData("", "", false, false, false, false)]
		// Identical values
		[InlineData("foo", "foo", false, false, false, false)]
		// Case differences
		[InlineData("foo", "FoO", true, false, false, false)]
		// Line ending differences
		[InlineData("foo \r\n bar", "foo \r bar", false, true, false, false)]
		[InlineData("foo \r\n bar", "foo \n bar", false, true, false, false)]
		[InlineData("foo \n bar", "foo \r bar", false, true, false, false)]
		// Whitespace differences
		[InlineData(" ", "\t", false, false, true, false)]
		[InlineData(" \t", "\t ", false, false, true, false)]
		[InlineData("    ", "\t", false, false, true, false)]
		[InlineData(" ", " \u180E", false, false, true, false)]
		[InlineData(" \u180E", "\u180E ", false, false, true, false)]
		[InlineData("    ", "\u180E", false, false, true, false)]
		[InlineData(" ", " \u200B", false, false, true, false)]
		[InlineData(" \u200B", "\u200B ", false, false, true, false)]
		[InlineData("    ", "\u200B", false, false, true, false)]
		[InlineData(" ", " \u200B\uFEFF", false, false, true, false)]
		[InlineData(" \u180E", "\u200B\u202F\u1680\u180E ", false, false, true, false)]
		[InlineData("\u2001\u2002\u2003\u2006\u2009    ", "\u200B", false, false, true, false)]
		[InlineData("\u00A0\u200A\u2009\u2006\u2009    ", "\u200B", false, false, true, false)]
		// The ogham space mark (\u1680) kind of looks like a faint dash, but Microsoft has put it
		// inside the SpaceSeparator unicode category, so we also treat it as a space
		[InlineData("\u2007\u2008\u1680\t\u0009\u3000   ", " ", false, false, true, false)]
		[InlineData("\u1680", "\t", false, false, true, false)]
		[InlineData("\u1680", "       ", false, false, true, false)]
		// All whitespace differences
		[InlineData("", "  ", false, false, false, true)]
		[InlineData("", "  ", false, false, true, true)]
		[InlineData("", "\t", false, false, true, true)]
		[InlineData("foobar", "foo bar", false, false, true, true)]
		public void Success(
			string? value1,
			string? value2,
			bool ignoreCase,
			bool ignoreLineEndingDifferences,
			bool ignoreWhiteSpaceDifferences,
			bool ignoreAllWhiteSpace)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1, value2, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2, value1, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.Memoryify(), value2.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.Memoryify(), value1.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.AsMemory(), value2.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.AsMemory(), value1.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.Memoryify(), value2.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.Memoryify(), value1.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.AsMemory(), value2.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.AsMemory(), value1.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.Spanify(), value2.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.Spanify(), value1.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.AsSpan(), value2.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.AsSpan(), value1.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.Spanify(), value2.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.Spanify(), value1.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value1.AsSpan(), value2.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.AsSpan(), value1.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
		}

		[Theory]
		// Non-identical values
		[InlineData("foo", "foo!", false, false, false, false, null, "   ↑ (pos 3)")]
		[InlineData("foo\0", "foo\0\0", false, false, false, false, null, "     ↑ (pos 4)")]
		// Nulls
		[InlineData("first test 1", null, false, false, false, false, null, null)]
		[InlineData(null, "first test 1", false, false, false, false, null, null)]
		// Overruns
		[InlineData("first test", "first test 1", false, false, false, false, null, "          ↑ (pos 10)")]
		[InlineData("first test 1", "first test", false, false, false, false, "          ↓ (pos 10)", null)]
		// Case differences
		[InlineData("Foobar", "foo bar", true, false, false, false, "   ↓ (pos 3)", "   ↑ (pos 3)")]
		// Line ending differences
		[InlineData("foo\nbar", "foo\rBar", false, true, false, false, "     ↓ (pos 4)", "     ↑ (pos 4)")]
		// Non-zero whitespace quantity differences
		[InlineData("foo bar", "foo  Bar", false, false, true, false, "    ↓ (pos 4)", "     ↑ (pos 5)")]
		// Ignore all white space differences
		[InlineData("foobar", "foo Bar", false, false, false, true, "   ↓ (pos 3)", "    ↑ (pos 4)")]
		public void Failure(
			string? expected,
			string? actual,
			bool ignoreCase,
			bool ignoreLineEndingDifferences,
			bool ignoreWhiteSpaceDifferences,
			bool ignoreAllWhiteSpace,
			string? expectedPointer,
			string? actualPointer)
		{
			void verify(Action action)
			{
				var message = "Assert.Equal() Failure: Strings differ";

				if (expectedPointer is not null)
					message += Environment.NewLine + "           " + expectedPointer;

				message +=
					Environment.NewLine + "Expected: " + ArgumentFormatter.Format(expected) +
					Environment.NewLine + "Actual:   " + ArgumentFormatter.Format(actual);

				if (actualPointer is not null)
					message += Environment.NewLine + "           " + actualPointer;

				var ex = Record.Exception(action);

				Assert.IsType<EqualException>(ex);
				Assert.Equal(message, ex.Message);
			}

			verify(() => Assert.Equal(expected, actual, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
			if (expected is not null && actual is not null)
			{
				verify(() => Assert.Equal(expected.Memoryify(), actual.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.AsMemory(), actual.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.Memoryify(), actual.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.AsMemory(), actual.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.Spanify(), actual.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.AsSpan(), actual.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.Spanify(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				verify(() => Assert.Equal(expected.AsSpan(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
			}
		}

		[Fact]
		public void Truncation()
		{
			var expected = "Why hello there world, you're a long string with some truncation!";
			var actual = "Why hello there world! You're a long string!";

			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Strings differ" + Environment.NewLine +
					"                                ↓ (pos 21)" + Environment.NewLine +
					$"Expected: \"Why hello there world, you're a long string with s\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
					"Actual:   \"Why hello there world! You're a long string!\"" + Environment.NewLine +
					"                                ↑ (pos 21)",
					ex.Message
				);
			}

			verify(() => Assert.Equal(expected, actual));
			verify(() => Assert.Equal(expected.Memoryify(), actual.Memoryify()));
			verify(() => Assert.Equal(expected.AsMemory(), actual.Memoryify()));
			verify(() => Assert.Equal(expected.Memoryify(), actual.AsMemory()));
			verify(() => Assert.Equal(expected.AsMemory(), actual.AsMemory()));
			verify(() => Assert.Equal(expected.Spanify(), actual.Spanify()));
			verify(() => Assert.Equal(expected.AsSpan(), actual.Spanify()));
			verify(() => Assert.Equal(expected.Spanify(), actual.AsSpan()));
			verify(() => Assert.Equal(expected.AsSpan(), actual.AsSpan()));
		}
	}

	public class Matches_Pattern
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegexPattern", () => Assert.Matches((string?)null!, "Hello, world!"));
		}

		[Fact]
		public void Success()
		{
			Assert.Matches(@"\w", "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Matches(@"\d+", "Hello, world!"));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				@"Value: ""Hello, world!""",
				ex.Message
			);
		}

		[Fact]
		public void Failure_NullActual()
		{
			var ex = Record.Exception(() => Assert.Matches(@"\d+", null));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				"Value: null",
				ex.Message
			);
		}
	}

	public class Matches_Regex
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegex", () => Assert.Matches((Regex?)null!, "Hello, world!"));
		}

		[Fact]
		public void Success()
		{
			Assert.Matches(new Regex(@"\w+"), "Hello");
		}

		[Fact]
		public void UsesRegexOptions()
		{
			Assert.Matches(new Regex(@"[a-z]+", RegexOptions.IgnoreCase), "HELLO");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Matches(new Regex(@"\d+"), "Hello, world!"));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				@"Value: ""Hello, world!""",
				ex.Message
			);
		}

		[Fact]
		public void Failure_NullActual()
		{
			var ex = Record.Exception(() => Assert.Matches(new Regex(@"\d+"), null));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				"Value: null",
				ex.Message
			);
		}
	}

	public class StartsWith
	{
		[Fact]
		public void Success()
		{
			Assert.StartsWith("Hello", "Hello, world!");
			Assert.StartsWith("Hello".Memoryify(), "Hello, world!".Memoryify());
			Assert.StartsWith("Hello".AsMemory(), "Hello, world!".Memoryify());
			Assert.StartsWith("Hello".Memoryify(), "Hello, world!".AsMemory());
			Assert.StartsWith("Hello".AsMemory(), "Hello, world!".AsMemory());
			Assert.StartsWith("Hello".Spanify(), "Hello, world!".Spanify());
			Assert.StartsWith("Hello".AsSpan(), "Hello, world!".Spanify());
			Assert.StartsWith("Hello".Spanify(), "Hello, world!".AsSpan());
			Assert.StartsWith("Hello".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void Failure()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"Hello, world!\"" + Environment.NewLine +
					"Expected start: \"hey\"",
					ex.Message
				);
			}

			verify(() => Assert.StartsWith("hey", "Hello, world!"));
			verify(() => Assert.StartsWith("hey".Memoryify(), "Hello, world!".Memoryify()));
			verify(() => Assert.StartsWith("hey".AsMemory(), "Hello, world!".Memoryify()));
			verify(() => Assert.StartsWith("hey".Memoryify(), "Hello, world!".AsMemory()));
			verify(() => Assert.StartsWith("hey".AsMemory(), "Hello, world!".AsMemory()));
			verify(() => Assert.StartsWith("hey".Spanify(), "Hello, world!".Spanify()));
			verify(() => Assert.StartsWith("hey".AsSpan(), "Hello, world!".Spanify()));
			verify(() => Assert.StartsWith("hey".Spanify(), "Hello, world!".AsSpan()));
			verify(() => Assert.StartsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void CaseSensitiveByDefault()
		{
			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"world!\"" + Environment.NewLine +
					"Expected start: \"WORLD!\"",
					ex.Message
				);
			}

			verify(() => Assert.StartsWith("WORLD!", "world!"));
			verify(() => Assert.StartsWith("WORLD!".Memoryify(), "world!".Memoryify()));
			verify(() => Assert.StartsWith("WORLD!".AsMemory(), "world!".Memoryify()));
			verify(() => Assert.StartsWith("WORLD!".Memoryify(), "world!".AsMemory()));
			verify(() => Assert.StartsWith("WORLD!".AsMemory(), "world!".AsMemory()));
			verify(() => Assert.StartsWith("WORLD!".Spanify(), "world!".Spanify()));
			verify(() => Assert.StartsWith("WORLD!".AsSpan(), "world!".Spanify()));
			verify(() => Assert.StartsWith("WORLD!".Spanify(), "world!".AsSpan()));
			verify(() => Assert.StartsWith("WORLD!".AsSpan(), "world!".AsSpan()));
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.StartsWith("HELLO", "Hello, world!", StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".AsMemory(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Memoryify(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".AsSpan(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Spanify(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void NullStrings()
		{
			var ex = Record.Exception(() => Assert.StartsWith(default(string), default(string)));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
				"String:         null" + Environment.NewLine +
				"Expected start: null",
				ex.Message
			);
		}

		[Fact]
		public void Truncation()
		{
			var expected = "This is a long string that we're looking for at the start";
			var actual = "This is the long string that we expected to find this starting inside";

			void verify(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"This is the long string that we expected to find t\"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
					"Expected start: \"This is a long string that we're looking for at th\"" + ArgumentFormatter.Ellipsis,
					ex.Message
				);
			}

			verify(() => Assert.StartsWith(expected, actual));
			verify(() => Assert.StartsWith(expected.Memoryify(), actual.Memoryify()));
			verify(() => Assert.StartsWith(expected.AsMemory(), actual.Memoryify()));
			verify(() => Assert.StartsWith(expected.Memoryify(), actual.AsMemory()));
			verify(() => Assert.StartsWith(expected.AsMemory(), actual.AsMemory()));
			verify(() => Assert.StartsWith(expected.Spanify(), actual.Spanify()));
			verify(() => Assert.StartsWith(expected.AsSpan(), actual.Spanify()));
			verify(() => Assert.StartsWith(expected.Spanify(), actual.AsSpan()));
			verify(() => Assert.StartsWith(expected.AsSpan(), actual.AsSpan()));
		}
	}
}
