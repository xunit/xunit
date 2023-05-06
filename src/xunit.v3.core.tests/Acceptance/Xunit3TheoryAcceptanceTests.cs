using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit3TheoryAcceptanceTests
{
	public class TheoryTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask OptionalParameters_Valid()
		{
			var results = await RunForResultsAsync(typeof(ClassWithOptionalParameters));

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_NonePassed(s: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_OneNonNullPassed(s: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_OneNullPassed(s: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneParameter_NonePassed(s: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneParameter_OnePassed(s: ""def"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_TwoParameters_OnePassed(s: ""abc"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_TwoParameters_TwoPassed(s: ""abc"", i: 6)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptionalAttributes_NonePassed(x: null, y: 0)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_FirstOnePassed(s: ""def"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_NonePassed(s: ""abc"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_TwoPassedInOrder(s: ""def"", i: 6)", displayName)
			);
		}

		public class ClassWithOptionalParameters
		{
			[Theory]
			[InlineData]
			public void OneOptional_OneParameter_NonePassed(string s = "abc")
			{
				Assert.Equal("abc", s);
			}

			[Theory]
			[InlineData("def")]
			public void OneOptional_OneParameter_OnePassed(string s = "abc")
			{
				Assert.Equal("def", s);
			}

			[Theory]
			[InlineData]
			public void OneOptional_OneNullParameter_NonePassed(string? s = null)
			{
				Assert.Null(s);
			}

			[Theory]
			[InlineData("abc")]
			public void OneOptional_OneNullParameter_OneNonNullPassed(string? s = null)
			{
				Assert.Equal("abc", s);
			}

			[Theory]
			[InlineData(null!)]
			public void OneOptional_OneNullParameter_OneNullPassed(string? s = null)
			{
				Assert.Null(s);
			}

			[Theory]
			[InlineData("abc")]
			public void OneOptional_TwoParameters_OnePassed(string s, int i = 5)
			{
				Assert.Equal("abc", s);
				Assert.Equal(5, i);
			}

			[Theory]
			[InlineData("abc", 6)]
			public void OneOptional_TwoParameters_TwoPassed(string s, int i = 5)
			{
				Assert.Equal("abc", s);
				Assert.Equal(6, i);
			}

			[Theory]
			[InlineData]
			public void TwoOptional_TwoParameters_NonePassed(string s = "abc", int i = 5)
			{
				Assert.Equal("abc", s);
				Assert.Equal(5, i);
			}

			[Theory]
			[InlineData("def")]
			public void TwoOptional_TwoParameters_FirstOnePassed(string s = "abc", int i = 5)
			{
				Assert.Equal("def", s);
				Assert.Equal(5, i);
			}

			[Theory]
			[InlineData("def", 6)]
			public void TwoOptional_TwoParameters_TwoPassedInOrder(string s = "abc", int i = 5)
			{
				Assert.Equal("def", s);
				Assert.Equal(6, i);
			}

			[Theory]
			[InlineData()]
			public void TwoOptionalAttributes_NonePassed([Optional] object x, [Optional] int y)
			{
				Assert.Null(x);
				Assert.Equal(0, y);
			}
		}

		[Fact]
		public async ValueTask ParamsParameters_Valid()
		{
			var results = await RunForResultsAsync(typeof(ClassWithParamsParameters));

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase),
				displayName => Assert.Equal(@$"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_ManyPassed(array: [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_NonePassed(array: [])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_OnePassed_MatchingArray(array: [1])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_OnePassed_NonArray(array: [1])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_OnePassed_NonMatchingArray(array: [[1]])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_OnePassed_Null(array: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OptionalParameters_ManyPassed(s: ""def"", i: 2, array: [3, 4, 5])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OptionalParameters_NonePassed(s: ""abc"", i: 1, array: [])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.TwoParameters_ManyPassed(i: 1, array: [2, 3, 4, 5, 6])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.TwoParameters_NullPassed(i: 1, array: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.TwoParameters_OnePassed(i: 1, array: [])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.TwoParameters_OnePassed_MatchingArray(i: 1, array: [2])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.TwoParameters_OnePassed_NonArray(i: 1, array: [2])", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.TwoParameters_OnePassed_NonMatchingArray(i: 1, array: [[2]])", displayName)
			);
		}

		public class ClassWithParamsParameters
		{
			[Theory]
			[InlineData]
			public void OneParameter_NonePassed(params object?[] array)
			{
				Assert.Empty(array);
			}

			[Theory]
			[InlineData(null!)]
			public void OneParameter_OnePassed_Null(params object?[] array)
			{
				Assert.Null(array);
			}

			[Theory]
			[InlineData(1)]
			public void OneParameter_OnePassed_NonArray(params object?[] array)
			{
				Assert.Equal(new object?[] { 1 }, array);
			}

			[Theory]
			[InlineData(new object?[] { new object?[] { 1 } })]
			public void OneParameter_OnePassed_MatchingArray(params object?[] array)
			{
				Assert.Equal(new object?[] { 1 }, array);
			}

			[Theory]
			[InlineData(new int[] { 1 })]
			public void OneParameter_OnePassed_NonMatchingArray(params object?[] array)
			{
				Assert.Equal(new object?[] { new int[] { 1 } }, array);
			}

			[Theory]
			[InlineData(1, 2, 3, 4, 5, 6)]
			public void OneParameter_ManyPassed(params object?[] array)
			{
				Assert.Equal(new object?[] { 1, 2, 3, 4, 5, 6 }, array);
			}

			[Theory]
			[InlineData(1)]
			public void TwoParameters_OnePassed(int i, params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Empty(array);
			}

			[Theory]
			[InlineData(1, null)]
			public void TwoParameters_NullPassed(int i, params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Null(array);
			}

			[Theory]
			[InlineData(1, 2)]
			public void TwoParameters_OnePassed_NonArray(int i, params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal(new object?[] { 2 }, array);
			}

			[Theory]
			[InlineData(1, new object?[] { 2 })]
			public void TwoParameters_OnePassed_MatchingArray(int i, params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal(new object?[] { 2 }, array);
			}

			[Theory]
			[InlineData(1, new int[] { 2 })]
			public void TwoParameters_OnePassed_NonMatchingArray(int i, params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal(new object?[] { new int[] { 2 } }, array);
			}

			[Theory]
			[InlineData(1, 2, 3, 4, 5, 6)]
			public void TwoParameters_ManyPassed(int i, params object?[] array)
			{
				Assert.Equal(i, 1);
				Assert.Equal(new object?[] { 2, 3, 4, 5, 6 }, array);
			}

			[Theory]
			[InlineData]
			public void OptionalParameters_NonePassed(string s = "abc", int i = 1, params object?[] array)
			{
				Assert.Equal("abc", s);
				Assert.Equal(1, i);
				Assert.Empty(array);
			}

			[Theory]
			[InlineData("def", 2, 3, 4, 5)]
			public void OptionalParameters_ManyPassed(string s = "abc", int i = 1, params object?[] array)
			{
				Assert.Equal("def", s);
				Assert.Equal(2, i);
				Assert.Equal(new object?[] { 3, 4, 5 }, array);
			}
		}

		[Fact]
		public async ValueTask ImplicitExplicitConversions()
		{
			var results = await RunForResultsAsync(typeof(ClassWithOperatorConversions));

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredExplicitConversion(value: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredImplicitConversion(value: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.DecimalToInt(value: 43)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.IntToDecimal(value: 43)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.IntToLong(i: 1)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ParameterDeclaredExplicitConversion(e: Explicit { Value = ""abc"" })", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ParameterDeclaredImplicitConversion(i: Implicit { Value = ""abc"" })", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.UIntToULong(i: 1)", displayName)
			);
			Assert.Empty(results.OfType<_TestFailed>());
			Assert.Empty(results.OfType<_TestSkipped>());
		}

		class ClassWithOperatorConversions
		{
			// Explicit conversion defined on the parameter's type
			[Theory]
			[InlineData("abc")]
			public void ParameterDeclaredExplicitConversion(Explicit e)
			{
				Assert.Equal("abc", e.Value);
			}

			// Implicit conversion defined on the parameter's type
			[Theory]
			[InlineData("abc")]
			public void ParameterDeclaredImplicitConversion(Implicit i)
			{
				Assert.Equal("abc", i.Value);
			}

			public static IEnumerable<object?[]> ExplicitArgument =
				new[] { new[] { new Explicit { Value = "abc" } } };

			// Explicit conversion defined on the argument's type
			[Theory]
			[MemberData(nameof(ExplicitArgument))]
			public void ArgumentDeclaredExplicitConversion(string value)
			{
				Assert.Equal("abc", value);
			}

			public static IEnumerable<object?[]> ImplicitArgument =
				new[] { new[] { new Implicit { Value = "abc" } } };

			// Implicit conversion defined on the argument's type
			[Theory]
			[MemberData(nameof(ImplicitArgument))]
			public void ArgumentDeclaredImplicitConversion(string value)
			{
				Assert.Equal("abc", value);
			}

			[Theory]
			[InlineData(1)]
			public void IntToLong(long i)
			{
				Assert.Equal(1L, i);
			}

			[Theory]
			[InlineData((uint)1)]
			public void UIntToULong(ulong i)
			{
				Assert.Equal(1UL, i);
			}

			public static IEnumerable<object[]> DecimalArgument()
			{
				yield return new object[] { 43M };
			}

			// Decimal type offers multiple explicit conversions
			[Theory]
			[MemberData(nameof(DecimalArgument))]
			public void DecimalToInt(int value)
			{
				Assert.Equal(43, value);
			}

			// Decimal type offers multiple implicit conversions
			[Theory]
			[InlineData(43)]
			public void IntToDecimal(decimal value)
			{
				Assert.Equal(43M, value);
			}

			public class Explicit
			{
				public string? Value { get; set; }

				public static explicit operator Explicit(string value)
				{
					return new Explicit() { Value = value };
				}

				public static explicit operator string?(Explicit e)
				{
					return e.Value;
				}
			}

			public class Implicit
			{
				public string? Value { get; set; }

				public static implicit operator Implicit(string value)
				{
					return new Implicit() { Value = value };
				}

				public static implicit operator string?(Implicit i)
				{
					return i.Value;
				}
			}
		}

		[Fact]
		public async ValueTask GenericParameter_Func_Valid()
		{
			var results = await RunForResultsAsync(typeof(ClassWithFuncMethod));

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Double>(source: [4, 5, 6, 7], ", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Int32>(source: [4, 5, 6, 7]", displayName),
				displayName => Assert.StartsWith(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithFuncMethod.TestMethod<Single>(source: [4, 5, 6, 7]", displayName)
			);
		}

		internal class ClassWithFuncMethod
		{
			public static IEnumerable<object?[]> TestData()
			{
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, float>(i => i + 0.5f) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, double>(i => i + 0.5d) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i * 2) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i + 1) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i) };
				yield return new object?[] { new[] { 4, 5, 6, 7 }, new Func<int, int>(i => i) };
			}

			[Theory]
			[MemberData(nameof(TestData))]
			public void TestMethod<TResult>(IEnumerable<int> source, Func<int, TResult> selector)
			{ }
		}

		[Fact]
		public async ValueTask Skipped()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithSkips));

			Assert.Collection(
				testMessages.OfType<TestSkippedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedDataRow(x: 0, y: null)", skipped.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedInlineData(x: 0, y: null)", skipped.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedMemberData(x: 0, y: null)", skipped.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedTheory", skipped.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				}
			);
			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedDataRow(x: 42, y: \"Hello, world!\")", passed.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedInlineData(x: 42, y: \"Hello, world!\")", passed.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedMemberData(x: 42, y: \"Hello, world!\")", passed.TestDisplayName)
			);
		}

		class ClassWithSkips
		{
			[Theory(Skip = "Don't run this!")]
			[InlineData(42, "Hello, world!")]
			[InlineData(0, null)]
			public void SkippedTheory(int x, string y)
			{
				Assert.NotNull(y);
			}

			[Theory]
			[InlineData(42, "Hello, world!")]
			[InlineData(0, null, Skip = "Don't run this!")]
			public void SkippedInlineData(int x, string y)
			{
				Assert.NotNull(y);
			}

			[Theory]
			[InlineData(42, "Hello, world!")]
			[MemberData(nameof(MemberDataSource), Skip = "Don't run this!")]
			public void SkippedMemberData(int x, string y)
			{
				Assert.NotNull(y);
			}

			public static IEnumerable<object?[]> MemberDataSource()
			{
				yield return new object?[] { 0, null };
			}

			[Theory]
			[MemberData(nameof(DataRowSource))]
			public void SkippedDataRow(int x, string y)
			{
				Assert.NotNull(y);
			}

			public static IEnumerable<ITheoryDataRow> DataRowSource()
			{
				yield return new TheoryDataRow(42, "Hello, world!");
				yield return new TheoryDataRow(0, null) { Skip = "Don't run this!" };
			}
		}

		[Fact]
		public async ValueTask GenericTheoryWithSerializableData()
		{
			var results = await RunForResultsAsync(typeof(GenericWithSerializableData));

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().Select(p => p.TestDisplayName).OrderBy(x => x),
				// Embedded (T1, Empty<T2>)
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Embedded<Int32, Int32>(value: 1, value2: Empty<Int32>)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Embedded<Object, Int32>(value: null, value2: Empty<Int32>)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Embedded<String, Int32>(value: ""1"", value2: Empty<Int32>)", displayName),
				// Simple (T1, T2)
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<Int32, Object>(value1: 42, value2: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<Int32[], List<String>>(value1: [1, 2, 3], value2: [""a"", ""b"", ""c""])", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<Object, Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData+Empty<Int32>>(value1: null, value2: Empty<Int32>)", displayName),
				displayName => Assert.Equal($@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<String, Double>(value1: ""Hello, world!"", value2: {21.12:G17})", displayName)
			);
		}

		class GenericWithSerializableData
		{
			public static IEnumerable<object?[]> GenericData_Embedded()
			{
				yield return new object?[] { 1, default(Empty<int>) };
				yield return new object?[] { "1", default(Empty<int>) };
				yield return new object?[] { null, default(Empty<int>) };
			}

			[Theory, MemberData(nameof(GenericData_Embedded))]
			public void GenericTest_Embedded<T1, T2>(T1 value, Empty<T2> value2) { }

			public struct Empty<T>
			{
				public override string ToString() => $"Empty<{typeof(T).Name}>";
			}

			public static IEnumerable<object?[]> GenericData_Simple()
			{
				yield return new object?[] { 42, null };
				yield return new object?[] { "Hello, world!", 21.12 };
				yield return new object?[] { new int[] { 1, 2, 3 }, new List<string> { "a", "b", "c" } };
				yield return new object?[] { null, default(Empty<int>) };
			}

			[Theory, MemberData(nameof(GenericData_Simple))]
			public void GenericTest_Simple<T1, T2>(T1 value1, T2 value2) { }
		}

		[Fact]
		public async ValueTask GenericTheoryWithNonSerializableData()
		{
			var results = await RunForResultsAsync(typeof(GenericWithNonSerializableData));

			var displayName = Assert.Single(results.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName));
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData.GenericTest<Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData>(value: GenericWithNonSerializableData { })", displayName);
		}

		class GenericWithNonSerializableData
		{
			public static IEnumerable<object?[]> GenericData
			{
				get
				{
					yield return new object?[] { new GenericWithNonSerializableData() };
				}
			}

			[Theory, MemberData(nameof(GenericData))]
			public void GenericTest<T>(T value) { }
		}
	}

	public class DataAttributeTests : AcceptanceTestV3
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask ExplicitAcceptanceTest_ExplicitOff(bool preEnumerateTheories)
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_ExplicitAcceptanceTests), preEnumerateTheories, ExplicitOption.Off);

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 2112, y: \"Inline forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 2113, y: \"Member forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 42, y: \"Inline inherited\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 43, y: \"Member inherited\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 2112, y: \"Inline forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 2113, y: \"Member forced false\")", passed.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Empty(testMessages.OfType<TestSkippedWithDisplayName>());
			Assert.Collection(
				testMessages.OfType<TestNotRunWithDisplayName>().OrderBy(x => x.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 0, y: \"Inline forced true\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 0, y: \"Member forced true\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 0, y: \"Inline forced true\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 0, y: \"Member forced true\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 42, y: \"Inline inherited\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 43, y: \"Member inherited\")", notRun.TestDisplayName)
			);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask ExplicitAcceptanceTest_ExplicitOn(bool preEnumerateTheories)
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_ExplicitAcceptanceTests), preEnumerateTheories, ExplicitOption.On);

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 2112, y: \"Inline forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 2113, y: \"Member forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 42, y: \"Inline inherited\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 43, y: \"Member inherited\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 2112, y: \"Inline forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 2113, y: \"Member forced false\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 42, y: \"Inline inherited\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 43, y: \"Member inherited\")", passed.TestDisplayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 0, y: \"Inline forced true\")", failed.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 0, y: \"Member forced true\")", failed.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 0, y: \"Inline forced true\")", failed.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 0, y: \"Member forced true\")", failed.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestSkippedWithDisplayName>());
			Assert.Empty(testMessages.OfType<TestNotRunWithDisplayName>());
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask ExplicitAcceptanceTest_ExplicitOnly(bool preEnumerateTheories)
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_ExplicitAcceptanceTests), preEnumerateTheories, ExplicitOption.Only);

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 42, y: \"Inline inherited\")", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 43, y: \"Member inherited\")", passed.TestDisplayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 0, y: \"Inline forced true\")", failed.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 0, y: \"Member forced true\")", failed.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 0, y: \"Inline forced true\")", failed.TestDisplayName),
				failed => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 0, y: \"Member forced true\")", failed.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestSkippedWithDisplayName>());
			Assert.Collection(
				testMessages.OfType<TestNotRunWithDisplayName>().OrderBy(x => x.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 2112, y: \"Inline forced false\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 2113, y: \"Member forced false\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 42, y: \"Inline inherited\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse)}(x: 43, y: \"Member inherited\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 2112, y: \"Inline forced false\")", notRun.TestDisplayName),
				notRun => Assert.Equal($"{typeof(ClassUnderTest_ExplicitAcceptanceTests).FullName}.{nameof(ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue)}(x: 2113, y: \"Member forced false\")", notRun.TestDisplayName)
			);
		}

		class ClassUnderTest_ExplicitAcceptanceTests
		{
			public static List<TheoryDataRow> MemberDataSource = new()
			{
				new(43, "Member inherited"),
				new(0, "Member forced true") { Explicit = true },
				new(2113, "Member forced false") { Explicit = false },
			};

			[Theory]
			[InlineData(42, "Inline inherited")]
			[InlineData(0, "Inline forced true", Explicit = true)]
			[InlineData(2112, "Inline forced false", Explicit = false)]
			[MemberData(nameof(MemberDataSource))]
			public void TestWithTheoryExplicitFalse(int x, string y)
			{
				Assert.NotEqual(0, x);
			}

			[Theory(Explicit = true)]
			[InlineData(42, "Inline inherited")]
			[InlineData(0, "Inline forced true", Explicit = true)]
			[InlineData(2112, "Inline forced false", Explicit = false)]
			[MemberData(nameof(MemberDataSource))]
			public void TestWithTheoryExplicitTrue(int x, string y)
			{
				Assert.NotEqual(0, x);
			}
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask SkipAcceptanceTest(bool preEnumerateTheories)
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_SkipTests), preEnumerateTheories);

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal($"{typeof(ClassUnderTest_SkipTests).FullName}.{nameof(ClassUnderTest_SkipTests.TestWithNoSkipOnTheory)}(x: 42)", passed.TestDisplayName);
			Assert.Collection(
				testMessages.OfType<TestSkippedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				// Skip per data row
				skipped =>
				{
					Assert.Equal($"{typeof(ClassUnderTest_SkipTests).FullName}.{nameof(ClassUnderTest_SkipTests.TestWithNoSkipOnTheory)}(x: 2112)", skipped.TestDisplayName);
					Assert.Equal("Skip from InlineData", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal($"{typeof(ClassUnderTest_SkipTests).FullName}.{nameof(ClassUnderTest_SkipTests.TestWithNoSkipOnTheory)}(x: 2113)", skipped.TestDisplayName);
					Assert.Equal("Skip from theory data row", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal($"{typeof(ClassUnderTest_SkipTests).FullName}.{nameof(ClassUnderTest_SkipTests.TestWithNoSkipOnTheory)}(x: 43)", skipped.TestDisplayName);
					Assert.Equal("Skip from MemberData", skipped.Reason);
				},
				// Single skipped theory, not one per data row
				skipped =>
				{
					Assert.Equal($"{typeof(ClassUnderTest_SkipTests).FullName}.{nameof(ClassUnderTest_SkipTests.TestWithSkipOnTheory)}", skipped.TestDisplayName);
					Assert.Equal("Skip from theory", skipped.Reason);
				}
			);
		}

		class ClassUnderTest_SkipTests
		{
			public static List<TheoryDataRow> DataSource = new()
			{
				new(43),
				new(2113) { Skip = "Skip from theory data row" }
			};

			[Theory]
			[InlineData(42)]
			[InlineData(2112, Skip = "Skip from InlineData")]
			[MemberData(nameof(DataSource), Skip = "Skip from MemberData")]
			public void TestWithNoSkipOnTheory(int x)
			{ }

			[Theory(Skip = "Skip from theory")]
			[InlineData(42)]
			[InlineData(2112, Skip = "Skip from InlineData")]
			[MemberData(nameof(DataSource), Skip = "Skip from MemberData")]
			public void TestWithSkipOnTheory(int x)
			{ }
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask TestDisplayNameAcceptanceTest(bool preEnumerateTheories)
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_TestDisplayNameTests), preEnumerateTheories);

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				passed => Assert.Equal("Default Member Test(x: 43)", passed.TestDisplayName),
				passed => Assert.Equal("One Test Default (Member)(x: 1)", passed.TestDisplayName),
				passed => Assert.Equal("Override Member Test(x: 45)", passed.TestDisplayName),
				passed => Assert.Equal("Theory Display Name(x: 44)", passed.TestDisplayName),
				passed => Assert.Equal("Three Test Override (Member)(x: 3)", passed.TestDisplayName),
				passed => Assert.Equal("Two Test Override (Inline)(x: 2)", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest_TestDisplayNameTests).FullName}.{nameof(ClassUnderTest_TestDisplayNameTests.TestWithDefaultName)}(x: 42)", passed.TestDisplayName),
				passed => Assert.Equal("Zero Test Default (Inline)(x: 0)", passed.TestDisplayName)
			);
		}

		class ClassUnderTest_TestDisplayNameTests
		{
			public static List<TheoryDataRow> DefaultMemberDataSource = new()
			{
				new(43),
				new(1) { TestDisplayName = "One Test Default (Member)" },
			};

			[Theory]
			[InlineData(42)]
			[InlineData(0, TestDisplayName = "Zero Test Default (Inline)")]
			[MemberData(nameof(DefaultMemberDataSource), TestDisplayName = "Default Member Test")]
			public void TestWithDefaultName(int x)
			{ }

			public static List<TheoryDataRow> OverrideMemberDataSource = new()
			{
				new(45),
				new(3) { TestDisplayName = "Three Test Override (Member)" },
			};

			[Theory(DisplayName = "Theory Display Name")]
			[InlineData(44)]
			[InlineData(2, TestDisplayName = "Two Test Override (Inline)")]
			[MemberData(nameof(OverrideMemberDataSource), TestDisplayName = "Override Member Test")]
			public void TestWithOverriddenName(int x)
			{ }
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask TraitsAcceptanceTest(bool preEnumerateTheories)
		{
			var testMessages = await RunAsync(typeof(ClassUnderTests_TraitsTests), preEnumerateTheories);

			Assert.Collection(
				testMessages.OfType<_TestStarting>().OrderBy(x => x.TestDisplayName),
				starting =>
				{
					Assert.Equal($"{typeof(ClassUnderTests_TraitsTests).FullName}.{nameof(ClassUnderTests_TraitsTests.TestMethod)}(x: 0)", starting.TestDisplayName);
					Assert.Collection(
						starting.Traits["Location"].OrderBy(x => x),
						trait => Assert.Equal("Class", trait),
						trait => Assert.Equal("InlineData", trait),
						trait => Assert.Equal("Method", trait)
					);
					Assert.False(starting.Traits.ContainsKey("Discarded"));
				},
				starting =>
				{
					Assert.Equal($"{typeof(ClassUnderTests_TraitsTests).FullName}.{nameof(ClassUnderTests_TraitsTests.TestMethod)}(x: 2112)", starting.TestDisplayName);
					Assert.Collection(
						starting.Traits["Location"].OrderBy(x => x),
						trait => Assert.Equal("Class", trait),
						trait => Assert.Equal("MemberData", trait),
						trait => Assert.Equal("Method", trait)
					);
					Assert.False(starting.Traits.ContainsKey("Discarded"));
				},
				starting =>
				{
					Assert.Equal($"{typeof(ClassUnderTests_TraitsTests).FullName}.{nameof(ClassUnderTests_TraitsTests.TestMethod)}(x: 42)", starting.TestDisplayName);
					Assert.Collection(
						starting.Traits["Location"].OrderBy(x => x),
						trait => Assert.Equal("Class", trait),
						trait => Assert.Equal("MemberData", trait),
						trait => Assert.Equal("Method", trait),
						trait => Assert.Equal("TheoryDataRow", trait)
					);
					Assert.False(starting.Traits.ContainsKey("Discarded"));
				}
			);
		}

		[Trait("Location", "Class")]
		class ClassUnderTests_TraitsTests
		{
			public static List<TheoryDataRow> MemberDataSource = new()
			{
				new TheoryDataRow(2112),
				new TheoryDataRow(42).WithTrait("Location", "TheoryDataRow"),
			};

			[Theory]
			[Trait("Location", "Method")]
			[InlineData(0, Traits = new[] { "Location", "InlineData", "Discarded" })]
			[MemberData(nameof(MemberDataSource), Traits = new[] { "Location", "MemberData", "Discarded" })]
			public void TestMethod(int x)
			{ }
		}
	}

	[Collection("Timeout Tests")]
	public class DataAttributeTimeoutTests : AcceptanceTestV3
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask TimeoutAcceptanceTest(bool preEnumerateTheories)
		{
			var stopwatch = Stopwatch.StartNew();
			var results = await RunForResultsAsync(typeof(ClassUnderTest), preEnumerateTheories);
			stopwatch.Stop();

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.LongRunningTask)}(delay: 10)", passed.TestDisplayName),
				passed => Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.LongRunningTask)}(delay: 100)", passed.TestDisplayName)
			);
			Assert.Collection(
				results.OfType<TestFailedWithDisplayName>().OrderBy(f => f.TestDisplayName),
				failed =>
				{
					Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.LongRunningTask)}(delay: 10000)", failed.TestDisplayName);
					Assert.Equal("Test execution timed out after 42 milliseconds", failed.Messages.Single());
				},
				failed =>
				{
					Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.LongRunningTask)}(delay: 11000)", failed.TestDisplayName);
					Assert.Equal("Test execution timed out after 10 milliseconds", failed.Messages.Single());
				}
			);

			Assert.True(stopwatch.ElapsedMilliseconds < 10000, "Elapsed time should be less than 10 seconds");
		}

		class ClassUnderTest
		{
			public static List<TheoryDataRow> MemberDataSource = new()
			{
				new TheoryDataRow(11000),
				new TheoryDataRow(100) { Timeout = 10000 },
			};

			[Theory(Timeout = 42)]
			[InlineData(10000)]
			[InlineData(10, Timeout = 10000)]
			[MemberData(nameof(MemberDataSource), Timeout = 10)]
			public Task LongRunningTask(int delay) => Task.Delay(delay);
		}
	}

	public class InlineDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask RunsForEachDataElement()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal($"Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestMethod(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passed.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestMethod(x: 0, y: 0, z: null)", failed.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassUnderTest
		{
			[Theory]
			[InlineData(42, 21.12, "Hello, world!")]
			[InlineData(0, 0.0, null)]
			public void TestMethod(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async ValueTask SingleNullValuesWork()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTestForNullValues));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForNullValues.TestMethod(value: null)", passed.TestDisplayName);
		}

		class ClassUnderTestForNullValues
		{
			[Theory]
			[InlineData(null!)]
			public void TestMethod(string value) { }
		}

		[Fact]
		public async ValueTask ArraysWork()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTestForArrays));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Contains("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForArrays.TestMethod", passed.TestDisplayName);
		}

		class ClassUnderTestForArrays
		{
			[Theory]
			[InlineData(new[] { 42, 2112 }, new[] { "SELF", "PARENT1", "PARENT2", "PARENT3" }, null, 10.5, "Hello, world!")]
			public void TestMethod(int[] v1, string[] v2, float[] v3, double v4, string v5) { }
		}

		[Fact]
		public async ValueTask ValueArraysWithObjectParameterInjectCorrectType()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTestForValueArraysWithObjectParameter));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Contains("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForValueArraysWithObjectParameter.TestMethod", passed.TestDisplayName);
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

		[Fact]
		public async ValueTask AsyncTaskMethod_MultipleInlineDataAttributes()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithAsyncTaskMethod));

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithAsyncTaskMethod.TestMethod(x: A)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithAsyncTaskMethod.TestMethod(x: B)", displayName)
			);
		}

		enum SomeEnum
		{
			A, B
		}

		class ClassWithAsyncTaskMethod
		{
			[Theory]
			[InlineData(SomeEnum.A)]
			[InlineData(SomeEnum.B)]
			async Task TestMethod(SomeEnum x)
			{
				await Task.Run(() => "Any statement, to prevent a C# compiler error");
			}
		}
	}

	public class ClassDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask IncompatibleDataReturnType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_IncompatibleReturnType));

			var result = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest_IncompatibleReturnType.TestMethod", result.TestDisplayName);
			Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
			Assert.Equal(
				"'Xunit3TheoryAcceptanceTests+ClassDataTests+ClassWithIncompatibleReturnType' must implement one of the following interfaces to be used as ClassData for the test method named 'TestMethod' on 'Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest_IncompatibleReturnType':" + Environment.NewLine +
				"- IEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- IEnumerable<object[]>" + Environment.NewLine +
				"- IAsyncEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- IAsyncEnumerable<object[]>",
				result.Messages.Single()
			);
		}

		class ClassWithIncompatibleReturnType { }

		class ClassUnderTest_IncompatibleReturnType
		{
			[Theory]
			[ClassData(typeof(ClassWithIncompatibleReturnType))]
			public void TestMethod(int z) { }
		}

		[Fact]
		public async ValueTask IncompatibleDataValueType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_IncomptableValueData));

			var result = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest_IncomptableValueData.TestMethod", result.TestDisplayName);
			Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
			Assert.StartsWith("Class 'Xunit3TheoryAcceptanceTests+ClassDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
		}

		class ClassWithIncompatibleValueData : IEnumerable
		{
			public IEnumerator GetEnumerator()
			{
				yield return 42;
			}
		}

		class ClassUnderTest_IncomptableValueData
		{
			[Theory]
			[ClassData(typeof(ClassWithIncompatibleValueData))]
			public void TestMethod(int z) { }
		}

		class ClassDataSource
		{
			public static readonly object[] Data =
				new object[]
				{
					new object?[] { "Hello from class source", 2600 },
					Tuple.Create("Hello from Tuple", 42),
					("Class source will fail", 2112),
					new TheoryDataRow("Class source would fail if I ran", 96) { Skip = "Do not run" },
					new TheoryDataRow("I only run explicitly", 9600) { Explicit = true },
				};
		}

		class DataSource_Enumerable : IEnumerable
		{
			public IEnumerator GetEnumerator() =>
				ClassDataSource.Data.GetEnumerator();
		}

		class DataSource_AsyncEnumerable : IAsyncEnumerable<object?>
		{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
			public async IAsyncEnumerator<object?> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			{
				foreach (var dataValue in ClassDataSource.Data)
					yield return dataValue;
			}
#pragma warning restore CS1998
		}

		class ClassUnderTest_IAsyncEnumerable
		{
			[Theory]
			[ClassData(typeof(DataSource_AsyncEnumerable))]
			public void TestMethod(string z, int _)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

		class ClassUnderTest_IEnumerable
		{
			[Theory]
			[ClassData(typeof(DataSource_Enumerable))]
			public void TestMethod(string z, int _)
			{
				Assert.DoesNotContain("fail", z);
			}
		}
	}

	public class MissingDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask MissingDataThrows()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithMissingData));

			var failed = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData.TestViaMissingData", failed.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'Foo' on Xunit3TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData", failed.Messages.Single());
		}

		class ClassWithMissingData
		{
			[Theory]
			[MemberData("Foo")]
			public void TestViaMissingData(int x, double y, string z) { }
		}
	}

	public class DataConversionTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask IncompatibleDataThrows()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithIncompatibleData));

			var failed = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIncompatibleData.TestViaIncompatibleData(x: ""Foo"")", failed.TestDisplayName);
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
		public async ValueTask ImplicitlyConvertibleDataPasses()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithImplicitlyConvertibleData));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithImplicitlyConvertibleData.TestViaImplicitData(x: 42)", passed.TestDisplayName);
		}

		class ClassWithImplicitlyConvertibleData
		{
			[Theory]
			[InlineData(42)]
			public void TestViaImplicitData(int? x) { }
		}

		[Fact]
		public async ValueTask IConvertibleDataPasses()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithIConvertibleData));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIConvertibleData.TestViaIConvertible(x: 42)", passed.TestDisplayName);
		}

		class MyConvertible : IConvertible
		{
			public TypeCode GetTypeCode()
			{
				return TypeCode.Int32;
			}

			public int ToInt32(IFormatProvider? provider)
			{
				return 42;
			}

			#region Noise

			public bool ToBoolean(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public byte ToByte(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public char ToChar(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public DateTime ToDateTime(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public decimal ToDecimal(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public double ToDouble(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public short ToInt16(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public long ToInt64(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public sbyte ToSByte(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public float ToSingle(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public string ToString(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public object ToType(Type conversionType, IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public ushort ToUInt16(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public uint ToUInt32(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			public ulong ToUInt64(IFormatProvider? provider)
			{
				throw new InvalidCastException();
			}

			#endregion
		}

		class ClassWithIConvertibleData
		{
			public static IEnumerable<object?[]> Data = new TheoryData<MyConvertible> { new MyConvertible() };

			[Theory]
			[MemberData(nameof(Data))]
			public void TestViaIConvertible(int x) { }
		}
	}

	public class MemberDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask NonStaticData_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithNonStaticData));

			Assert.Collection(
				testMessages.OfType<TestFailedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData.FieldTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Could not find public static member (property, field, or method) named 'FieldDataSource' on Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData.MethodTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Could not find public static member (property, field, or method) named 'MethodDataSource' on Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData.PropertyTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Could not find public static member (property, field, or method) named 'PropertyDataSource' on Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData", result.Messages.Single());
				}
			);
		}

		class ClassWithNonStaticData
		{
			public IEnumerable<object?[]>? FieldDataSource = null;

			public IEnumerable<object?[]>? MethodDataSource() => null;

			public IEnumerable<object?[]>? PropertyDataSource => null;

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			public void FieldTestMethod(int x, double y, string z) { }

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			public void MethodTestMethod(int x, double y, string z) { }

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			public void PropertyTestMethod(int x, double y, string z) { }
		}

		[Fact]
		public async ValueTask IncompatibleDataReturnType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithIncompatibleReturnType));
			var exceptionEpilogue =
				" must return data in one of the following formats:" + Environment.NewLine +
				"- IEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- Task<IEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- ValueTask<IEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- IEnumerable<object[]>" + Environment.NewLine +
				"- Task<IEnumerable<object[]>>" + Environment.NewLine +
				"- ValueTask<IEnumerable<object[]>>" + Environment.NewLine +
				"- IAsyncEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- IAsyncEnumerable<object[]>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<object[]>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<object[]>>";

			Assert.Collection(
				testMessages.OfType<TestFailedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType.FieldTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Member 'IncompatibleField' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType'" + exceptionEpilogue, result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType.MethodTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Member 'IncompatibleMethod' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType'" + exceptionEpilogue, result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType.PropertyTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Member 'IncompatibleProperty' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType'" + exceptionEpilogue, result.Messages.Single());
				}
			);
		}

		class ClassWithIncompatibleReturnType
		{
			public static int IncompatibleField = 42;

			public static int IncompatibleMethod() => 42;

			public static int IncompatibleProperty => 42;

			[Theory]
			[MemberData(nameof(IncompatibleField))]
			public void FieldTestMethod(int x) { }

			[Theory]
			[MemberData(nameof(IncompatibleMethod))]
			public void MethodTestMethod(int x) { }

			[Theory]
			[MemberData(nameof(IncompatibleProperty))]
			public void PropertyTestMethod(int x) { }
		}

		[Fact]
		public async ValueTask IncompatibleDataValueType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithIncompatibleValueData));

			Assert.Collection(
				testMessages.OfType<TestFailedWithDisplayName>().OrderBy(x => x.TestDisplayName),
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData.FieldTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.StartsWith("Member 'IncompatibleFieldData' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData.MethodTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.StartsWith("Member 'IncompatibleMethodData' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData.PropertyTestMethod", result.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.StartsWith("Member 'IncompatiblePropertyData' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
				}
			);
		}

		class ClassWithIncompatibleValueData
		{
			public static IEnumerable<int> IncompatibleFieldData = new[] { 42 };

			public static IEnumerable<int> IncompatibleMethodData() => new[] { 42 };

			public static IEnumerable<int> IncompatiblePropertyData => new[] { 42 };

			[Theory]
			[MemberData(nameof(IncompatibleFieldData))]
			public void FieldTestMethod(int x) { }

			[Theory]
			[MemberData(nameof(IncompatibleMethodData))]
			public void MethodTestMethod(int x) { }

			[Theory]
			[MemberData(nameof(IncompatiblePropertyData))]
			public void PropertyTestMethod(int x) { }
		}

		[Theory]
		[InlineData(typeof(ClassUnderTest_IAsyncEnumerable))]
		[InlineData(typeof(ClassUnderTest_IEnumerable))]
		[InlineData(typeof(ClassUnderTest_TaskOfIAsyncEnumerable))]
		[InlineData(typeof(ClassUnderTest_TaskOfIEnumerable))]
		[InlineData(typeof(ClassUnderTest_ValueTaskOfIAsyncEnumerable))]
		[InlineData(typeof(ClassUnderTest_ValueTaskOfIEnumerable))]
		public async ValueTask AcceptanceTest(Type classUnderTest)
		{
			var testMessages = await RunForResultsAsync(classUnderTest);

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Hello from base\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Hello from other source\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Hello, world!\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Hello from base\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Hello from other source\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Hello, world!\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Hello from base\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Hello from other source\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Hello, world!\")", displayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithDisplayName>().Select(failed => failed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Base will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"I will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Other source will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Base will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"I will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Other source will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Base will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"I will fail\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Other source will fail\")", displayName)
			);
			Assert.Collection(
				testMessages.OfType<TestSkippedWithDisplayName>().Select(skipped => skipped.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Base would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"I would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.FieldTestMethod(z: \"Other source would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Base would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"I would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.MethodTestMethod(z: \"Other source would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Base would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"I would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{classUnderTest.FullName}.PropertyTestMethod(z: \"Other source would fail if I ran\")", displayName)
			);
		}

		class DataBase
		{
			static readonly object?[] baseData =
				new object[]
				{
					new object?[] { "Hello from base" },
					Tuple.Create("Base will fail"),
					new TheoryDataRow("Base would fail if I ran") { Skip = "Do not run" },
				};
			static readonly IAsyncEnumerable<object?> baseDataAsync = baseData.ToAsyncEnumerable();

			protected static readonly object?[] data =
				new object[]
				{
					new object?[] { "Hello, world!" },
					Tuple.Create("I will fail"),
					new TheoryDataRow("I would fail if I ran") { Skip = "Do not run" },
				};
			protected static readonly IAsyncEnumerable<object?> dataAsync = data.ToAsyncEnumerable();

			public static IAsyncEnumerable<object?> AsyncEnumerable_FieldBaseDataSource = baseDataAsync;
			public static IAsyncEnumerable<object?> AsyncEnumerable_MethodBaseDataSource() => baseDataAsync;
			public static IAsyncEnumerable<object?> AsyncEnumerable_PropertyBaseDataSource => baseDataAsync;

			public static IEnumerable Enumerable_FieldBaseDataSource = baseData;
			public static IEnumerable Enumerable_MethodBaseDataSource() => baseData;
			public static IEnumerable Enumerable_PropertyBaseDataSource => baseData;

			public static Task<IAsyncEnumerable<object?>> TaskOfAsyncEnumerable_FieldBaseDataSource = Task.FromResult(baseDataAsync);
			public static Task<IAsyncEnumerable<object?>> TaskOfAsyncEnumerable_MethodBaseDataSource() => Task.FromResult(baseDataAsync);
			public static Task<IAsyncEnumerable<object?>> TaskOfAsyncEnumerable_PropertyBaseDataSource => Task.FromResult(baseDataAsync);

			public static Task<IEnumerable> TaskOfEnumerable_FieldBaseDataSource = Task.FromResult<IEnumerable>(baseData);
			public static Task<IEnumerable> TaskOfEnumerable_MethodBaseDataSource() => Task.FromResult<IEnumerable>(baseData);
			public static Task<IEnumerable> TaskOfEnumerable_PropertyBaseDataSource => Task.FromResult<IEnumerable>(baseData);

			public static ValueTask<IAsyncEnumerable<object?>> ValueTaskOfAsyncEnumerable_FieldBaseDataSource = new(baseDataAsync);
			public static ValueTask<IAsyncEnumerable<object?>> ValueTaskOfAsyncEnumerable_MethodBaseDataSource() => new(baseDataAsync);
			public static ValueTask<IAsyncEnumerable<object?>> ValueTaskOfAsyncEnumerable_PropertyBaseDataSource => new(baseDataAsync);

			public static ValueTask<IEnumerable> ValueTaskOfEnumerable_FieldBaseDataSource = new(baseData);
			public static ValueTask<IEnumerable> ValueTaskOfEnumerable_MethodBaseDataSource() => new(baseData);
			public static ValueTask<IEnumerable> ValueTaskOfEnumerable_PropertyBaseDataSource => new(baseData);
		}

		class OtherDataSource
		{
			static readonly object[] data =
				new object[]
				{
					new object?[] { "Hello from other source" },
					Tuple.Create("Other source will fail"),
					new TheoryDataRow("Other source would fail if I ran") { Skip = "Do not run" },
				};
			static readonly IAsyncEnumerable<object> dataAsync = data.ToAsyncEnumerable();

			public static IAsyncEnumerable<object> AsyncEnumerable_FieldOtherDataSource = dataAsync;
			public static IAsyncEnumerable<object> AsyncEnumerable_MethodOtherDataSource() => dataAsync;
			public static IAsyncEnumerable<object> AsyncEnumerable_PropertyOtherDataSource => dataAsync;

			public static IEnumerable Enumerable_FieldOtherDataSource = data;
			public static IEnumerable Enumerable_MethodOtherDataSource() => data;
			public static IEnumerable Enumerable_PropertyOtherDataSource => data;

			public static Task<IAsyncEnumerable<object>> TaskOfAsyncEnumerable_FieldOtherDataSource = Task.FromResult(dataAsync);
			public static Task<IAsyncEnumerable<object>> TaskOfAsyncEnumerable_MethodOtherDataSource() => Task.FromResult(dataAsync);
			public static Task<IAsyncEnumerable<object>> TaskOfAsyncEnumerable_PropertyOtherDataSource => Task.FromResult(dataAsync);

			public static Task<IEnumerable> TaskOfEnumerable_FieldOtherDataSource = Task.FromResult<IEnumerable>(data);
			public static Task<IEnumerable> TaskOfEnumerable_MethodOtherDataSource() => Task.FromResult<IEnumerable>(data);
			public static Task<IEnumerable> TaskOfEnumerable_PropertyOtherDataSource => Task.FromResult<IEnumerable>(data);

			public static ValueTask<IAsyncEnumerable<object>> ValueTaskOfAsyncEnumerable_FieldOtherDataSource = new(dataAsync);
			public static ValueTask<IAsyncEnumerable<object>> ValueTaskOfAsyncEnumerable_MethodOtherDataSource() => new(dataAsync);
			public static ValueTask<IAsyncEnumerable<object>> ValueTaskOfAsyncEnumerable_PropertyOtherDataSource => new(dataAsync);

			public static ValueTask<IEnumerable> ValueTaskOfEnumerable_FieldOtherDataSource = new(data);
			public static ValueTask<IEnumerable> ValueTaskOfEnumerable_MethodOtherDataSource() => new(data);
			public static ValueTask<IEnumerable> ValueTaskOfEnumerable_PropertyOtherDataSource => new(data);
		}

		class ClassUnderTest_IAsyncEnumerable : DataBase
		{
			public static IAsyncEnumerable<object?> FieldDataSource = dataAsync;
			public static IAsyncEnumerable<object?> MethodDataSource() => dataAsync;
			public static IAsyncEnumerable<object?> PropertyDataSource => dataAsync;

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(AsyncEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.AsyncEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(AsyncEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.AsyncEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(AsyncEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.AsyncEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

		class ClassUnderTest_IEnumerable : DataBase
		{
			public static IEnumerable FieldDataSource = data;
			public static IEnumerable MethodDataSource() => data;
			public static IEnumerable PropertyDataSource => data;

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(Enumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.Enumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(Enumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.Enumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(Enumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.Enumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

		class ClassUnderTest_TaskOfIAsyncEnumerable : DataBase
		{
			public static Task<IAsyncEnumerable<object?>> FieldDataSource = Task.FromResult(dataAsync);
			public static Task<IAsyncEnumerable<object?>> MethodDataSource() => Task.FromResult(dataAsync);
			public static Task<IAsyncEnumerable<object?>> PropertyDataSource => Task.FromResult(dataAsync);

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(TaskOfAsyncEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfAsyncEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(TaskOfAsyncEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfAsyncEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(TaskOfAsyncEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfAsyncEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

		class ClassUnderTest_TaskOfIEnumerable : DataBase
		{
			public static Task<IEnumerable> FieldDataSource = Task.FromResult<IEnumerable>(data);
			public static Task<IEnumerable> MethodDataSource() => Task.FromResult<IEnumerable>(data);
			public static Task<IEnumerable> PropertyDataSource => Task.FromResult<IEnumerable>(data);

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(TaskOfEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(TaskOfEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(TaskOfEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

		class ClassUnderTest_ValueTaskOfIAsyncEnumerable : DataBase
		{
			public static ValueTask<IAsyncEnumerable<object?>> FieldDataSource = new(dataAsync);
			public static ValueTask<IAsyncEnumerable<object?>> MethodDataSource() => new(dataAsync);
			public static ValueTask<IAsyncEnumerable<object?>> PropertyDataSource => new(dataAsync);

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(ValueTaskOfAsyncEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfAsyncEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(ValueTaskOfAsyncEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfAsyncEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(ValueTaskOfAsyncEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfAsyncEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

		class ClassUnderTest_ValueTaskOfIEnumerable : DataBase
		{
			public static ValueTask<IEnumerable> FieldDataSource = new(data);
			public static ValueTask<IEnumerable> MethodDataSource() => new(data);
			public static ValueTask<IEnumerable> PropertyDataSource => new(data);

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(ValueTaskOfEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(ValueTaskOfEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(ValueTaskOfEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}
	}

	public class MethodDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask NonMatchingMethodInputDataThrows()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithMismatchedMethodData));

			var failed = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData.TestViaMethodData", failed.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData with parameter types: System.Double", failed.Messages.Single());
		}

		class ClassWithMismatchedMethodData
		{
			public static IEnumerable<object?[]>? DataSource(int x) => null;

			[Theory]
			[MemberData(nameof(DataSource), 21.12)]
			public void TestViaMethodData(int x, double y, string z) { }
		}

		[Fact]
		public async ValueTask SubTypeInheritsTestsFromBaseType()
		{
			var testMessages = await RunForResultsAsync(typeof(SubClassWithNoTests));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithNoTests.Test(x: 42)", passed.TestDisplayName);
		}

		public abstract class BaseClassWithTestAndData
		{
			public static IEnumerable<object?[]> TestData()
			{
				yield return new object?[] { 42 };
			}

			[Theory]
			[MemberData(nameof(TestData))]
			public void Test(int x)
			{
				Assert.Equal(42, x);
			}
		}

		public class SubClassWithNoTests : BaseClassWithTestAndData { }

		[Fact]
		public async ValueTask CanPassParametersToDataMethod()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithParameterizedMethodData));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal($"Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passed.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failed.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithParameterizedMethodData
		{
			public static IEnumerable<object?[]> DataSource(int x)
			{
				return new[] {
					new object?[] { x / 2, 21.12, "Hello, world!" },
					new object?[] { 0, 0.0, null }
				};
			}

			[Theory]
			[MemberData(nameof(DataSource), 84)]
			public void TestViaMethodData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async ValueTask CanDowncastMethodData()
		{
			var testMessages = await RunAsync(typeof(ClassWithDowncastedMethodData));

			Assert.Equal(2, testMessages.OfType<_TestPassed>().Count());
			Assert.Empty(testMessages.OfType<_TestFailed>());
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithDowncastedMethodData
		{
			public static IEnumerable<object?[]> DataSource(object x, string y) { yield return new object?[] { 42, 21.12, "Hello world" }; }

			[Theory]
			[MemberData(nameof(DataSource), 42, "Hello world")]
			[MemberData(nameof(DataSource), 21.12, null)]
			public void TestViaMethodData(int x, double y, string z) { }
		}

		[Fact]
		public async ValueTask CanUseMethodDataInSubTypeFromTestInBaseType()
		{
			var testMessages = await RunForResultsAsync(typeof(SubClassWithTestData));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithDisplayName>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithTestData.Test(x: 42)", passed.TestDisplayName);
		}

		public abstract class BaseClassWithTestWithoutData
		{
			[Theory]
			[MemberData(nameof(TestData))]
			public void Test(int x)
			{
				Assert.Equal(42, x);
			}
		}

		public class SubClassWithTestData : BaseClassWithTestWithoutData
		{
			public static IEnumerable<object?[]> TestData()
			{
				yield return new object?[] { 42 };
			}
		}
	}

	public class CustomDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestDataWithInternalConstructor_ReturnsTwoPassingTheories()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithCustomDataWithInternalDataCtor));

			Assert.Collection(
				testMessages.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithCustomDataWithInternalDataCtor.Passing(unused: 2112)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithCustomDataWithInternalDataCtor.Passing(unused: 42)", displayName)
			);
			Assert.Empty(testMessages.OfType<TestFailedWithDisplayName>());
			Assert.Empty(testMessages.OfType<TestSkippedWithDisplayName>());
		}

		internal class MyCustomData : DataAttribute
		{
			internal MyCustomData() { }

			public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod) =>
				new(
					new[]
					{
						new TheoryDataRow(42),
						new TheoryDataRow(2112)
					}
				);
		}

		class ClassWithCustomDataWithInternalDataCtor
		{
			[Theory]
			[MyCustomData]
			public void Passing(int unused) { }
		}

		[Fact]
		public async ValueTask CanSupportConstructorOverloadingWithDataAttribute()  // https://github.com/xunit/xunit/issues/1711
		{
			var testMessages = await RunAsync(typeof(DataConstructorOverloadExample));

			Assert.Single(testMessages.OfType<_TestPassed>());
			Assert.Empty(testMessages.OfType<_TestFailed>());
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class DataConstructorOverloadExample
		{
			[Theory]
			[MyData((object?)null)]
			public void TestMethod(object? data)
			{ }
		}

		internal class MyDataAttribute : DataAttribute
		{
			public MyDataAttribute(object? value)
			{ }

			public MyDataAttribute(string parameter2Name)
			{
				Assert.False(true);
			}

			public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod) =>
				new(new[] { new TheoryDataRow(new object()) });
		}

		[Fact]
		public async ValueTask MemberDataAttributeBaseSubclass_Success()
		{
			var results = await RunForResultsAsync(typeof(ClassWithMemberDataAttributeBase));

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().Select(passed => passed.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: ""3"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: ""4"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: 1)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: 2)", displayName)
			);
		}

		private class SingleMemberDataAttribute : MemberDataAttributeBase
		{
			public SingleMemberDataAttribute(
				string memberName,
				params object?[] parameters) :
					base(memberName, parameters)
			{ }

			protected override ITheoryDataRow ConvertDataRow(
				MethodInfo testMethod,
				object dataRow) =>
					new TheoryDataRow(dataRow);
		}

		private class ClassWithMemberDataAttributeBase
		{
			private static IEnumerable IEnumerableMethod()
			{
				yield return 1;
			}

			private static IEnumerable<int> IEnumerableIntMethod()
			{
				yield return 2;
			}

			private static IEnumerable<string> IEnumerableStringMethod()
			{
				yield return "3";
			}

			private static IEnumerable<object> IEnumerableObjectMethod()
			{
				yield return "4";
			}

			[Theory]
			[SingleMemberData(nameof(IEnumerableMethod))]
			[SingleMemberData(nameof(IEnumerableIntMethod))]
			[SingleMemberData(nameof(IEnumerableStringMethod))]
			[SingleMemberData(nameof(IEnumerableObjectMethod))]
			public void Passing(object unused) { }
		}
	}

	public class ErrorAggregation : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask EachTheoryHasIndividualExceptionMessage()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest));

			var equalFailure = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>(), msg => msg.TestDisplayName == $"Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestMethod(x: 42, y: {21.12:G17}, z: ClassUnderTest {{ }})");
			Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

			var notNullFailure = Assert.Single(testMessages.OfType<TestFailedWithDisplayName>(), msg => msg.TestDisplayName == "Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestMethod(x: 0, y: 0, z: null)");
			Assert.Contains("Assert.NotNull() Failure", notNullFailure.Messages.Single());
		}

		class ClassUnderTest
		{
			public static IEnumerable<object?[]> Data
			{
				get
				{
					yield return new object?[] { 42, 21.12, new ClassUnderTest() };
					yield return new object?[] { 0, 0.0, null };
				}
			}

			[Theory]
			[MemberData(nameof(Data))]
			public void TestMethod(int x, double y, object z)
			{
				Assert.Equal(0, x); // Fails the first data item
				Assert.NotNull(z);  // Fails the second data item
			}
		}
	}

	public class OverloadedMethodTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestMethodMessagesOnlySentOnce()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var methodStarting = Assert.Single(testMessages.OfType<_TestMethodStarting>());
			Assert.Equal("Theory", methodStarting.TestMethod);
			var methodFinished = Assert.Single(testMessages.OfType<_TestMethodFinished>());
			Assert.Equal(methodStarting.TestMethodUniqueID, methodFinished.TestMethodUniqueID);
		}

		class ClassUnderTest
		{
			[Theory]
			[InlineData(42)]
			public void Theory(int value)
			{ }

			[Theory]
			[InlineData("42")]
			public void Theory(string value)
			{ }
		}
	}
}
