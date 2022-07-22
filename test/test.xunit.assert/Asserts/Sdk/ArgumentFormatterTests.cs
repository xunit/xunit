using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class ArgumentFormatterTests
{
	public class SimpleValues
	{
		[CulturedFact]
		public static void NullValue()
		{
			Assert.Equal("null", ArgumentFormatter.Format(null!));
		}

		// NOTE: It's important that this stays as MemberData
		// instead of InlineData. String constants in attributes
		// are stored as UTF-8 as IL, so since "\uD800" cannot
		// be converted to valid UTF-8, it ends up getting corrupted
		// and being stored as two characters instead. Thus, this test
		// will fail:

		// [Theory]
		// [InlineData("\uD800", 1)]
		// public void HasLength(string s, int length)
		// {
		//     Assert.Equal(length, s.Length);
		// }

		// While this one will pass:

		// public static IEnumerable<object[]> HasLength_TestData()
		// {
		//     yield return new object[] { "\uD800", 1 };
		// }

		// [Theory]
		// [MemberData(nameof(HasLength_TestData))]
		// public void HasLength(string s, int length)
		// {
		//     Assert.Equal(length, s.Length);
		// }

		// For more information, see the following links:
		// - http://stackoverflow.com/q/36104766/4077294
		// - http://codeblog.jonskeet.uk/2014/11/07/when-is-a-string-not-a-string/
		public static IEnumerable<object[]> StringValue_TestData()
		{
			yield return new object[] { "\uD800", @"""\xd800""" };
			yield return new object[] { "\uDC00", @"""\xdc00""" };
			yield return new object[] { "\uDC00\uD800", @"""\xdc00\xd800""" };
			yield return new object[] { "\uFFFD", "\"\uFFFD\"" };
		}

		[Theory]
		[InlineData("Hello, world!", "\"Hello, world!\"")]
		[InlineData(@"""", @"""\""""")] // quotes should be escaped
		[InlineData("\uD800\uDFFF", "\"\uD800\uDFFF\"")] // valid surrogates should print normally
		[InlineData("\uFFFE", @"""\xfffe""")] // same for U+FFFE...
		[InlineData("\uFFFF", @"""\xffff""")] // and U+FFFF, which are non-characters
		[InlineData("\u001F", @"""\x1f""")] // non-escaped C0 controls should be 2 digits
											// Other escape sequences
		[InlineData("\0", @"""\0""")] // null
		[InlineData("\r", @"""\r""")] // carriage return
		[InlineData("\n", @"""\n""")] // line feed
		[InlineData("\a", @"""\a""")] // alert
		[InlineData("\b", @"""\b""")] // backspace
		[InlineData("\\", @"""\\""")] // backslash
		[InlineData("\v", @"""\v""")] // vertical tab
		[InlineData("\t", @"""\t""")] // tab
		[InlineData("\f", @"""\f""")] // formfeed
		[InlineData("----|----1----|----2----|----3----|----4----|----5-", "\"----|----1----|----2----|----3----|----4----|----5\"...")] // truncation
		[MemberData(nameof(StringValue_TestData), DisableDiscoveryEnumeration = true)]
		public static void StringValue(string value, string expected)
		{
			Assert.Equal(expected, ArgumentFormatter.Format(value));
		}

		public static IEnumerable<object[]> CharValue_TestData()
		{
			yield return new object[] { '\uD800', "0xd800" };
			yield return new object[] { '\uDC00', "0xdc00" };
			yield return new object[] { '\uFFFD', "'\uFFFD'" };
			yield return new object[] { '\uFFFE', "0xfffe" };
		}

		[Theory]

		// Printable
		[InlineData(' ', "' '")]
		[InlineData('a', "'a'")]
		[InlineData('1', "'1'")]
		[InlineData('!', "'!'")]

		// Escape sequences
		[InlineData('\t', @"'\t'")] // tab
		[InlineData('\n', @"'\n'")] // newline
		[InlineData('\'', @"'\''")] // single quote
		[InlineData('\v', @"'\v'")] // vertical tab
		[InlineData('\a', @"'\a'")] // alert
		[InlineData('\\', @"'\\'")] // backslash
		[InlineData('\b', @"'\b'")] // backspace
		[InlineData('\r', @"'\r'")] // carriage return
		[InlineData('\f', @"'\f'")] // formfeed

		// Non-ASCII
		[InlineData('©', "'©'")]
		[InlineData('╬', "'╬'")]
		[InlineData('ئ', "'ئ'")]

		// Unprintable
		[InlineData(char.MinValue, @"'\0'")]
		[InlineData(char.MaxValue, "0xffff")]
		[MemberData(nameof(CharValue_TestData), DisableDiscoveryEnumeration = true)]
		public static void CharacterValue(char value, string expected)
		{
			Assert.Equal(expected, ArgumentFormatter.Format(value));
		}

		[CulturedFact]
		public static void FloatValue()
		{
			var floatPI = (float)Math.PI;

			Assert.Equal(floatPI.ToString("G9"), ArgumentFormatter.Format(floatPI));
		}

		[CulturedFact]
		public static void DoubleValue()
		{
			Assert.Equal(Math.PI.ToString("G17"), ArgumentFormatter.Format(Math.PI));
		}

		[CulturedFact]
		public static void DecimalValue()
		{
			Assert.Equal(123.45M.ToString(), ArgumentFormatter.Format(123.45M));
		}

		[CulturedFact]
		public static void DateTimeValue()
		{
			var now = DateTime.UtcNow;

			Assert.Equal(now.ToString("o"), ArgumentFormatter.Format(now));
		}

		[CulturedFact]
		public static void DateTimeOffsetValue()
		{
			var now = DateTimeOffset.UtcNow;

			Assert.Equal(now.ToString("o"), ArgumentFormatter.Format(now));
		}

		[Fact]
		public static async Task TaskValue()
		{
			var task = Task.Run(() => { });
			await task;

			Assert.Equal("Task { Status = RanToCompletion }", ArgumentFormatter.Format(task));
		}

		[Fact]
		public static void TaskGenericValue()
		{
			var taskCompletionSource = new TaskCompletionSource<int>();
			taskCompletionSource.SetException(new DivideByZeroException());

			Assert.Equal("Task<int> { Status = Faulted }", ArgumentFormatter.Format(taskCompletionSource.Task));
		}

		public static TheoryData<Type, string> TypeValueData = new()
		{
			{ typeof(string), "typeof(string)" },
			{ typeof(int[]), "typeof(int[])" },
			{ typeof(DateTime[,]), "typeof(System.DateTime[,])" },
			{ typeof(decimal[][,]), "typeof(decimal[][,])" },
			{ typeof(IEnumerable<>), "typeof(System.Collections.Generic.IEnumerable<>)" },
			{ typeof(IEnumerable<int>), "typeof(System.Collections.Generic.IEnumerable<int>)" },
			{ typeof(IDictionary<,>), "typeof(System.Collections.Generic.IDictionary<,>)" },
			{ typeof(IDictionary<string, DateTime>), "typeof(System.Collections.Generic.IDictionary<string, System.DateTime>)" },
			{ typeof(IDictionary<string[,], DateTime[,][]>), "typeof(System.Collections.Generic.IDictionary<string[,], System.DateTime[,][]>)" },
			{ typeof(bool?), "typeof(bool?)" },
			{ typeof(bool?[]), "typeof(bool?[])" }
		};

		[Theory]
		[MemberData(nameof(TypeValueData), DisableDiscoveryEnumeration = true)]
		public static void TypeValue(Type type, string expected)
		{
			Assert.Equal(expected, ArgumentFormatter.Format(type));
		}
	}

	public class Enums
	{
		public enum NonFlagsEnum
		{
			Value0 = 0,
			Value1 = 1
		}

		[CulturedTheory]
		[InlineData(0, "Value0")]
		[InlineData(1, "Value1")]
		[InlineData(42, "42")]
		public static void NonFlags(NonFlagsEnum enumValue, string expected)
		{
			var actual = ArgumentFormatter.Format(enumValue);

			Assert.Equal(expected, actual);
		}

		[Flags]
		public enum FlagsEnum
		{
			Nothing = 0,
			Value1 = 1,
			Value2 = 2,
		}

		[CulturedTheory]
		[InlineData(0, "Nothing")]
		[InlineData(1, "Value1")]
		[InlineData(3, "Value1 | Value2")]
		// This is expected, not "Value1 | Value2 | 4"
		[InlineData(7, "7")]
		public static void Flags(FlagsEnum enumValue, string expected)
		{
			var actual = ArgumentFormatter.Format(enumValue);

			Assert.Equal(expected, actual);
		}
	}

	public class KeyValuePair
	{
		[CulturedFact]
		public static void KeyValuePairValue()
		{
			var kvp = new KeyValuePair<object, List<object>>(42, new() { 21.12M, "2600" });
			var expected = $"[42] = [{21.12M}, \"2600\"]";

			Assert.Equal(expected, ArgumentFormatter.Format(kvp));
		}
	}

	public class Enumerables
	{
		[CulturedFact]
		public static void EnumerableValue()
		{
			var expected = $"[1, {2.3M}, \"Hello, world!\"]";

			Assert.Equal(expected, ArgumentFormatter.Format(new object[] { 1, 2.3M, "Hello, world!" }));
		}

		[CulturedFact]
		public static void DictionaryValue()
		{
			var value = new Dictionary<object, List<object>>
			{
				{ 42, new() { 21.12M, "2600" } },
				{ "123", new() { } },
			};
			var expected = $"[[42] = [{21.12M}, \"2600\"], [\"123\"] = []]";

			Assert.Equal(expected, ArgumentFormatter.Format(value));
		}

		[CulturedFact]
		public static void OnlyFirstFewValuesOfEnumerableAreRendered()
		{
			Assert.Equal("[0, 1, 2, 3, 4, ...]", ArgumentFormatter.Format(Enumerable.Range(0, int.MaxValue)));
		}

		[CulturedFact]
		public static void EnumerablesAreRenderedWithMaximumDepthToPreventInfiniteRecursion()
		{
			var looping = new object[2];
			looping[0] = 42;
			looping[1] = looping;

			Assert.Equal("[42, [42, [...]]]", ArgumentFormatter.Format(looping));
		}
	}

	public class ComplexTypes
	{
		[CulturedFact]
		public static void ReturnsValuesInAlphabeticalOrder()
		{
			var expected = $"MyComplexType {{ MyPublicField = 42, MyPublicProperty = {21.12M} }}";

			Assert.Equal(expected, ArgumentFormatter.Format(new MyComplexType()));
		}

		public class MyComplexType
		{
#pragma warning disable 414
			private string MyPrivateField = "Hello, world";
#pragma warning restore 414

			public static int MyPublicStaticField = 2112;

			public decimal MyPublicProperty { get; private set; }

			public int MyPublicField = 42;

			public MyComplexType()
			{
				MyPublicProperty = 21.12M;
			}
		}

		[CulturedFact]
		public static void ComplexTypeInsideComplexType()
		{
			var expected = $"MyComplexTypeWrapper {{ c = 'A', s = \"Hello, world!\", t = MyComplexType {{ MyPublicField = 42, MyPublicProperty = {21.12M} }} }}";

			Assert.Equal(expected, ArgumentFormatter.Format(new MyComplexTypeWrapper()));
		}

		public class MyComplexTypeWrapper
		{
			public MyComplexType t = new();
			public char c = 'A';
			public string s = "Hello, world!";
		}

		[CulturedFact]
		public static void Empty()
		{
			Assert.Equal("Object { }", ArgumentFormatter.Format(new object()));
		}

		[CulturedFact]
		public static void WithThrowingPropertyGetter()
		{
			Assert.Equal("ThrowingGetter { MyThrowingProperty = (throws NotImplementedException) }", ArgumentFormatter.Format(new ThrowingGetter()));
		}

		public class ThrowingGetter
		{
			public string MyThrowingProperty { get { throw new NotImplementedException(); } }
		}

		[CulturedFact]
		public static void LimitsOutputToFirstFewValues()
		{
			var expected = $@"Big {{ MyField1 = 42, MyField2 = ""Hello, world!"", MyProp1 = {21.12}, MyProp2 = typeof(ArgumentFormatterTests+ComplexTypes+Big), MyProp3 = 2014-04-17T07:45:23.0000000+00:00, ... }}";

			Assert.Equal(expected, ArgumentFormatter.Format(new Big()));
		}

		public class Big
		{
			public string MyField2 = "Hello, world!";

			public decimal MyProp1 { get; set; }

			public object MyProp4 { get; set; }

			public object MyProp3 { get; set; }

			public int MyField1 = 42;

			public Type MyProp2 { get; set; }

			public Big()
			{
				MyProp1 = 21.12M;
				MyProp2 = typeof(Big);
				MyProp3 = new DateTimeOffset(2014, 04, 17, 07, 45, 23, TimeSpan.Zero);
				MyProp4 = "Should not be shown";
			}
		}

		[CulturedFact]
		public static void TypesAreRenderedWithMaximumDepthToPreventInfiniteRecursion()
		{
			Assert.Equal("Looping { Me = Looping { Me = Looping { ... } } }", ArgumentFormatter.Format(new Looping()));
		}

		public class Looping
		{
			public Looping Me;

			public Looping() => Me = this;
		}

		[Fact]
		public static void WhenCustomTypeImplementsToString_UsesToString()
		{
			Assert.Equal("This is what you should show", ArgumentFormatter.Format(new TypeWithToString()));
		}

		public class TypeWithToString
		{
			public override string ToString() => "This is what you should show";
		}
	}

	public class TypeNames
	{
		public static TheoryData<Type, string> ArgumentFormatterFormatTypeNamesData = new()
		{
			{ typeof(int), "typeof(int)" },
			{ typeof(long), "typeof(long)" },
			{ typeof(string), "typeof(string)" },
			{ typeof(List<int>), "typeof(System.Collections.Generic.List<int>)" },
			{ typeof(Dictionary<int, string>), "typeof(System.Collections.Generic.Dictionary<int, string>)" },
			{ typeof(List<>), "typeof(System.Collections.Generic.List<>)" },
			{ typeof(Dictionary<,>), "typeof(System.Collections.Generic.Dictionary<,>)" }
		};

		[Theory]
		[MemberData(nameof(ArgumentFormatterFormatTypeNamesData), DisableDiscoveryEnumeration = true)]
		public void ArgumentFormatterFormatTypeNames(Type type, string expectedResult)
		{
			Assert.Equal(expectedResult, ArgumentFormatter.Format(type));
		}

		[Fact]
		public void ArgumentFormatterFormatTypeNameGenericTypeParameter()
		{
			var genericTypeParameters = typeof(List<>).GetGenericArguments();
			var parameterType = genericTypeParameters.First();

			Assert.Equal("typeof(T)", ArgumentFormatter.Format(parameterType));
		}

		[Fact]
		public void ArgumentFormatterFormatTypeNameGenericTypeParameters()
		{
			var genericTypeParameters = typeof(Dictionary<,>).GetGenericArguments();
			var parameterTKey = genericTypeParameters.First();

			Assert.Equal("typeof(TKey)", ArgumentFormatter.Format(parameterTKey));

			var parameterTValue = genericTypeParameters.Last();
			Assert.Equal("typeof(TValue)", ArgumentFormatter.Format(parameterTValue));
		}
	}
}
