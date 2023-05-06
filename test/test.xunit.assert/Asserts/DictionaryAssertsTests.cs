using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using Xunit.Sdk;

#if XUNIT_IMMUTABLE_COLLECTIONS
using System.Collections.Immutable;
#endif

public class DictionaryAssertsTests
{
	public class Contains
	{
		[Fact]
		public static void KeyInDictionary()
		{
			var dictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
			{
				["forty-two"] = 42
			};

			Assert.Equal(42, Assert.Contains("FORTY-two", dictionary));
			Assert.Equal(42, Assert.Contains("FORTY-two", new ReadOnlyDictionary<string, int>(dictionary)));
			Assert.Equal(42, Assert.Contains("FORTY-two", (IDictionary<string, int>)dictionary));
			Assert.Equal(42, Assert.Contains("FORTY-two", (IReadOnlyDictionary<string, int>)dictionary));
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.Equal(42, Assert.Contains("FORTY-two", dictionary.ToImmutableDictionary(StringComparer.InvariantCultureIgnoreCase)));
			Assert.Equal(42, Assert.Contains("FORTY-two", (IImmutableDictionary<string, int>)dictionary.ToImmutableDictionary(StringComparer.InvariantCultureIgnoreCase)));
#endif
		}

		[Fact]
		public static void KeyNotInDictionary()
		{
			var dictionary = new Dictionary<string, int>()
			{
				["eleventeen"] = 110
			};

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Key not found in dictionary" + Environment.NewLine +
					"Keys:      [\"eleventeen\"]" + Environment.NewLine +
					"Not found: \"FORTY-two\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.Contains("FORTY-two", dictionary));
			assertFailure(() => Assert.Contains("FORTY-two", new ReadOnlyDictionary<string, int>(dictionary)));
			assertFailure(() => Assert.Contains("FORTY-two", (IDictionary<string, int>)dictionary));
			assertFailure(() => Assert.Contains("FORTY-two", (IReadOnlyDictionary<string, int>)dictionary));
#if XUNIT_IMMUTABLE_COLLECTIONS
			assertFailure(() => Assert.Contains("FORTY-two", dictionary.ToImmutableDictionary()));
			assertFailure(() => Assert.Contains("FORTY-two", (IImmutableDictionary<string, int>)dictionary.ToImmutableDictionary()));
#endif
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public static void KeyNotInDictionary()
		{
			var dictionary = new Dictionary<string, int>()
			{
				["eleventeen"] = 110
			};

			Assert.DoesNotContain("FORTY-two", dictionary);
			Assert.DoesNotContain("FORTY-two", new ReadOnlyDictionary<string, int>(dictionary));
			Assert.DoesNotContain("FORTY-two", (IDictionary<string, int>)dictionary);
			Assert.DoesNotContain("FORTY-two", (IReadOnlyDictionary<string, int>)dictionary);
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.DoesNotContain("FORTY-two", dictionary.ToImmutableDictionary());
			Assert.DoesNotContain("FORTY-two", (IImmutableDictionary<string, int>)dictionary.ToImmutableDictionary());
#endif
		}

		[Fact]
		public static void KeyInDictionary()
		{
			var dictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
			{
				["forty-two"] = 42
			};

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Key found in dictionary" + Environment.NewLine +
					"Keys:  [\"forty-two\"]" + Environment.NewLine +
					"Found: \"FORTY-two\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.DoesNotContain("FORTY-two", dictionary));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", new ReadOnlyDictionary<string, int>(dictionary)));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", (IDictionary<string, int>)dictionary));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", (IReadOnlyDictionary<string, int>)dictionary));
#if XUNIT_IMMUTABLE_COLLECTIONS
			assertFailure(() => Assert.DoesNotContain("FORTY-two", dictionary.ToImmutableDictionary(StringComparer.InvariantCultureIgnoreCase)));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", (IImmutableDictionary<string, int>)dictionary.ToImmutableDictionary(StringComparer.InvariantCultureIgnoreCase)));
#endif
		}
	}
}
