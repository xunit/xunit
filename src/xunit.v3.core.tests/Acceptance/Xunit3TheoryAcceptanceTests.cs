using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit3TheoryAcceptanceTests
{
	public class TheoryTests : AcceptanceTestV3
	{
		[Fact(Skip = "Flaky")]
		public async void OptionalParameters_Valid()
		{
			var results = await RunAsync(typeof(ClassWithOptionalParameters));

			Assert.Collection(
				results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_NonePassed(s: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_OneNonNullPassed(s: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_OneNullPassed(s: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneParameter_NonePassed(s: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneParameter_OnePassed(s: ""def"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_TwoParameters_OnePassed(s: ""abc"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_TwoParameters_TwoPassed(s: ""abc"", i: 6)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_FirstOnePassed(s: ""def"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_NonePassed(s: ""abc"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_TwoPassedInOrder(s: ""def"", i: 6)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptionalAttributes_NonePassed(x: null, y: 0)", displayName)
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
		public async void ParamsParameters_Valid()
		{
			var results = await RunAsync(typeof(ClassWithParamsParameters));
			var orderedResults = results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

			Assert.Collection(
				orderedResults,
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithParamsParameters.OneParameter_ManyPassed(array: [1, 2, 3, 4, 5, ...])", displayName),
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
		public async void ImplicitExplicitConversions()
		{
			var results = await RunAsync(typeof(ClassWithOperatorConversions));

			Assert.Collection(
				results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredExplicitConversion(value: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredImplicitConversion(value: ""abc"")", displayName),
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
		public async void GenericParameter_Func_Valid()
		{
			var results = await RunAsync(typeof(ClassWithFuncMethod));

			Assert.Collection(
				results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
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
		public async void Skipped()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var skipped = Assert.Single(testMessages.OfType<_TestSkipped>());
			var skippedStarting = Assert.Single(testMessages.OfType<_TestStarting>().Where(s => s.TestUniqueID == skipped.TestUniqueID));
			Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassUnderTest.TestViaInlineData", skippedStarting.TestDisplayName);
			Assert.Equal("Don't run this!", skipped.Reason);
		}

		class ClassUnderTest
		{
			[Theory(Skip = "Don't run this!")]
			[InlineData(42, 21.12, "Hello, world!")]
			[InlineData(0, 0.0, null)]
			public void TestViaInlineData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void GenericTheoryWithSerializableData()
		{
			var results = await RunAsync(typeof(GenericWithSerializableData));

			Assert.Collection(
				results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<Int32, Object>(value1: 42, value2: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<Int32[], List<String>>(value1: [1, 2, 3], value2: [""a"", ""b"", ""c""])", displayName),
				displayName => Assert.Equal($@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest<String, Double>(value1: ""Hello, world!"", value2: {21.12:G17})", displayName)
			);
		}

		class GenericWithSerializableData
		{
			public static IEnumerable<object?[]> GenericData
			{
				get
				{
					yield return new object?[] { 42, null };
					yield return new object?[] { "Hello, world!", 21.12 };
					yield return new object?[] { new int[] { 1, 2, 3 }, new List<string> { "a", "b", "c" } };
				}
			}

			[Theory, MemberData("GenericData")]
			public void GenericTest<T1, T2>(T1 value1, T2 value2) { }
		}

		[Fact]
		public async void GenericTheoryWithNonSerializableData()
		{
			var results = await RunAsync(typeof(GenericWithNonSerializableData));

			var displayName = Assert.Single(results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x));
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

			[Theory, MemberData("GenericData")]
			public void GenericTest<T>(T value) { }
		}
	}

	public class InlineDataTests : AcceptanceTestV3
	{
		[Fact]
		public async void RunsForEachDataElement()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == passing.TestUniqueID);
			Assert.Equal($"Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passingStarting.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)", failedStarting.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassUnderTest
		{
			[Theory]
			[InlineData(42, 21.12, "Hello, world!")]
			[InlineData(0, 0.0, null)]
			public void TestViaInlineData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void SingleNullValuesWork()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTestForNullValues));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passing.TestUniqueID).Single();
			Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForNullValues.TestMethod(value: null)", passingStarting.TestDisplayName);
		}

		class ClassUnderTestForNullValues
		{
			[Theory]
			[InlineData(null!)]
			public void TestMethod(string value) { }
		}

		[Fact]
		public async void ArraysWork()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTestForArrays));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passing.TestUniqueID).Single();
			Assert.Contains("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForArrays.TestMethod", passingStarting.TestDisplayName);
		}

		class ClassUnderTestForArrays
		{
			[Theory]
			[InlineData(new[] { 42, 2112 }, new[] { "SELF", "PARENT1", "PARENT2", "PARENT3" }, null, 10.5, "Hello, world!")]
			public void TestMethod(int[] v1, string[] v2, float[] v3, double v4, string v5) { }
		}

		[Fact]
		public async void ValueArraysWithObjectParameterInjectCorrectType()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTestForValueArraysWithObjectParameter));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passing.TestUniqueID).Single();
			Assert.Contains("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForValueArraysWithObjectParameter.TestMethod", passingStarting.TestDisplayName);
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
		public async void AsyncTaskMethod_MultipleInlineDataAttributes()
		{
			var testMessages = await RunAsync(typeof(ClassWithAsyncTaskMethod));

			Assert.Collection(
				testMessages.OfType<_TestPassed>().Select(passed => testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
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
		public async void RunsForEachDataElement()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == passing.TestUniqueID);
			Assert.Equal($"Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest.TestViaClassData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passingStarting.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest.TestViaClassData(x: 0, y: 0, z: null)", failedStarting.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassUnderTest
		{
			[Theory]
			[ClassData(typeof(ClassDataSource))]
			public void TestViaClassData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		class ClassDataSource : IEnumerable<object?[]>
		{
			public IEnumerator<object?[]> GetEnumerator()
			{
				yield return new object?[] { 42, 21.12, "Hello, world!" };
				yield return new object?[] { 0, 0.0, null };
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[Fact]
		public async void NoDefaultConstructor_Fails()
		{
			var testMessages = await RunAsync(typeof(ClassNotImplementingIEnumerable));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassNotImplementingIEnumerable.TestMethod", failedStarting.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassNotImplementingIEnumerable must implement IEnumerable<object?[]> to be used as ClassData for the test method named 'TestMethod' on Xunit3TheoryAcceptanceTests+ClassDataTests+ClassNotImplementingIEnumerable", failed.Messages.Single());
		}

		class ClassNotImplementingIEnumerable
		{
			[Theory]
			[ClassData(typeof(ClassNotImplementingIEnumerable))]
			public void TestMethod() { }
		}
	}

	public class MissingDataTests : AcceptanceTestV3
	{
		[Fact]
		public async void MissingDataThrows()
		{
			var testMessages = await RunAsync(typeof(ClassWithMissingData));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData.TestViaMissingData", failedStarting.TestDisplayName);
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
		public async void IncompatibleDataThrows()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			var testMessages = await RunAsync(typeof(ClassWithIncompatibleData));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIncompatibleData.TestViaIncompatibleData(x: ""Foo"")", failedStarting.TestDisplayName);
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
		public async void ImplicitlyConvertibleDataPasses()
		{
			var testMessages = await RunAsync(typeof(ClassWithImplicitlyConvertibleData));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithImplicitlyConvertibleData.TestViaImplicitData(x: 42)", passedStarting.TestDisplayName);
		}

		class ClassWithImplicitlyConvertibleData
		{
			[Theory]
			[InlineData(42)]
			public void TestViaImplicitData(int? x) { }
		}

		[Fact]
		public async void IConvertibleDataPasses()
		{
			var testMessages = await RunAsync(typeof(ClassWithIConvertibleData));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIConvertibleData.TestViaIConvertible(x: 42)", passedStarting.TestDisplayName);
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
			[MemberData("Data")]
			public void TestViaIConvertible(int x) { }
		}
	}

	public class FieldDataTests : AcceptanceTestV3
	{
		[Fact]
		public async void RunsForEachDataElement()
		{
			var testMessages = await RunAsync(typeof(ClassWithSelfFieldData));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == passing.TestUniqueID);
			Assert.Equal($"Xunit3TheoryAcceptanceTests+FieldDataTests+ClassWithSelfFieldData.TestViaFieldData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passingStarting.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+FieldDataTests+ClassWithSelfFieldData.TestViaFieldData(x: 0, y: 0, z: null)", failedStarting.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithSelfFieldData
		{
			public static IEnumerable<object?[]> DataSource = new[] {
				new object?[] { 42, 21.12, "Hello, world!" },
				new object?[] { 0, 0.0, null }
			};

			[Theory]
			[MemberData("DataSource")]
			public void TestViaFieldData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void CanUseFieldDataFromOtherClass()
		{
			var testMessages = await RunAsync(typeof(ClassWithImportedFieldData));

			Assert.Single(testMessages.OfType<_TestPassed>());
			Assert.Single(testMessages.OfType<_TestFailed>());
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithImportedFieldData
		{
			[Theory]
			[MemberData("DataSource", MemberType = typeof(ClassWithSelfFieldData))]
			public void TestViaFieldData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void NonStaticFieldDataThrows()
		{
			var testMessages = await RunAsync(typeof(ClassWithNonStaticFieldData));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+FieldDataTests+ClassWithNonStaticFieldData.TestViaFieldData", failedStarting.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit3TheoryAcceptanceTests+FieldDataTests+ClassWithNonStaticFieldData", failed.Messages.Single());
		}

		class ClassWithNonStaticFieldData
		{
			public IEnumerable<object?[]>? DataSource = null;

			[Theory]
			[MemberData("DataSource")]
			public void TestViaFieldData(int x, double y, string z) { }
		}

		[Fact]
		public async void CanUseFieldDataFromBaseType()
		{
			var testMessages = await RunAsync(typeof(ClassWithBaseClassData));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal("Xunit3TheoryAcceptanceTests+FieldDataTests+ClassWithBaseClassData.TestViaFieldData(x: 42)", passedStarting.TestDisplayName);
		}

		class BaseClass
		{
			public static IEnumerable<object?[]> DataSource = new[] { new object?[] { 42 } };
		}

		class ClassWithBaseClassData : BaseClass
		{
			[Theory]
			[MemberData("DataSource")]
			public void TestViaFieldData(int x) { }
		}
	}

	public class MethodDataTests : AcceptanceTestV3
	{
		[Fact]
		public async void RunsForEachDataElement()
		{
			var testMessages = await RunAsync(typeof(ClassWithSelfMethodData));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == passing.TestUniqueID);
			Assert.Equal($"Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithSelfMethodData.TestViaMethodData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passingStarting.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithSelfMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failedStarting.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithSelfMethodData
		{
			public static IEnumerable<object?[]> DataSource()
			{
				return new[] {
					new object?[] { 42, 21.12, "Hello, world!" },
					new object?[] { 0, 0.0, null }
				};
			}

			[Theory]
			[MemberData("DataSource")]
			public void TestViaMethodData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void CanUseMethodDataFromOtherClass()
		{
			var testMessages = await RunAsync(typeof(ClassWithImportedMethodData));

			Assert.Single(testMessages.OfType<_TestPassed>());
			Assert.Single(testMessages.OfType<_TestFailed>());
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithImportedMethodData
		{
			[Theory]
			[MemberData("DataSource", MemberType = typeof(ClassWithSelfMethodData))]
			public void TestViaMethodData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void NonStaticMethodDataThrows()
		{
			var testMessages = await RunAsync(typeof(ClassWithNonStaticMethodData));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithNonStaticMethodData.TestViaMethodData", failedStarting.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithNonStaticMethodData", failed.Messages.Single());
		}

		class ClassWithNonStaticMethodData
		{
			public IEnumerable<object?[]>? DataSource() => null;

			[Theory]
			[MemberData("DataSource")]
			public void TestViaMethodData(int x, double y, string z) { }
		}

		[Fact]
		public async void NonMatchingMethodInputDataThrows()
		{
			var testMessages = await RunAsync(typeof(ClassWithMismatchedMethodData));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData.TestViaMethodData", failedStarting.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData with parameter types: System.Double", failed.Messages.Single());
		}

		class ClassWithMismatchedMethodData
		{
			public static IEnumerable<object?[]>? DataSource(int x) => null;

			[Theory]
			[MemberData("DataSource", 21.12)]
			public void TestViaMethodData(int x, double y, string z) { }
		}

		[Fact]
		public async void CanDowncastMethodData()
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
			[MemberData("DataSource", 42, "Hello world")]
			[MemberData("DataSource", 21.12, null)]
			public void TestViaMethodData(int x, double y, string z) { }
		}

		[Fact]
		public async void CanUseMethodDataFromBaseType()
		{
			var testMessages = await RunAsync(typeof(ClassWithBaseClassData));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithBaseClassData.TestViaMethodData(x: 42)", passedStarting.TestDisplayName);
		}

		class BaseClass
		{
			public static IEnumerable<object?[]> DataSource()
			{
				return new[] { new object?[] { 42 } };
			}
		}

		class ClassWithBaseClassData : BaseClass
		{
			[Theory]
			[MemberData("DataSource")]
			public void TestViaMethodData(int x) { }
		}

		[Fact]
		public async void CanUseMethodDataInSubTypeFromTestInBaseType()
		{
			var testMessages = await RunAsync(typeof(SubClassWithTestData));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithTestData.Test(x: 42)", passedStarting.TestDisplayName);
		}

		public abstract class BaseClassWithTestWithoutData
		{
			[Theory]
			[MemberData("TestData")]
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

		[Fact]
		public async void SubTypeInheritsTestsFromBaseType()
		{
			var testMessages = await RunAsync(typeof(SubClassWithNoTests));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithNoTests.Test(x: 42)", passedStarting.TestDisplayName);
		}

		public abstract class BaseClassWithTestAndData
		{
			public static IEnumerable<object?[]> TestData()
			{
				yield return new object?[] { 42 };
			}

			[Theory]
			[MemberData("TestData")]
			public void Test(int x)
			{
				Assert.Equal(42, x);
			}
		}

		public class SubClassWithNoTests : BaseClassWithTestAndData { }

		[Fact]
		public async void CanPassParametersToDataMethod()
		{
			var testMessages = await RunAsync(typeof(ClassWithParameterizedMethodData));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == passing.TestUniqueID);
			Assert.Equal($"Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passingStarting.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(x: 0, y: 0, z: null)", failedStarting.TestDisplayName);
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
			[MemberData("DataSource", 84)]
			public void TestViaMethodData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}
	}

	public class PropertyDataTests : AcceptanceTestV3
	{
		[Fact]
		public async void RunsForEachDataElement()
		{
			var testMessages = await RunAsync(typeof(ClassWithSelfPropertyData));

			var passing = Assert.Single(testMessages.OfType<_TestPassed>());
			var passingStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == passing.TestUniqueID);
			Assert.Equal($"Xunit3TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 42, y: {21.12:G17}, z: \"Hello, world!\")", passingStarting.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+PropertyDataTests+ClassWithSelfPropertyData.TestViaPropertyData(x: 0, y: 0, z: null)", failedStarting.TestDisplayName);
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithSelfPropertyData
		{
			public static IEnumerable<object?[]> DataSource
			{
				get
				{
					yield return new object?[] { 42, 21.12, "Hello, world!" };
					yield return new object?[] { 0, 0.0, null };
				}
			}

			[Theory]
			[MemberData("DataSource")]
			public void TestViaPropertyData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void CanUsePropertyDataFromOtherClass()
		{
			var testMessages = await RunAsync(typeof(ClassWithImportedPropertyData));

			Assert.Single(testMessages.OfType<_TestPassed>());
			Assert.Single(testMessages.OfType<_TestFailed>());
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		class ClassWithImportedPropertyData
		{
			[Theory]
			[MemberData("DataSource", MemberType = typeof(ClassWithSelfPropertyData))]
			public void TestViaPropertyData(int x, double y, string z)
			{
				Assert.NotNull(z);
			}
		}

		[Fact]
		public async void NonStaticPropertyDataThrows()
		{
			var testMessages = await RunAsync(typeof(ClassWithNonStaticPropertyData));

			var failed = Assert.Single(testMessages.OfType<_TestFailed>());
			var failedStarting = testMessages.OfType<_TestStarting>().Single(s => s.TestUniqueID == failed.TestUniqueID);
			Assert.Equal("Xunit3TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData.TestViaPropertyData", failedStarting.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on Xunit3TheoryAcceptanceTests+PropertyDataTests+ClassWithNonStaticPropertyData", failed.Messages.Single());
		}

		class ClassWithNonStaticPropertyData
		{
			public IEnumerable<object?[]>? DataSource => null;

			[Theory]
			[MemberData("DataSource")]
			public void TestViaPropertyData(int x, double y, string z) { }
		}

		[Fact]
		public async void CanUsePropertyDataFromBaseType()
		{
			var testMessages = await RunAsync(typeof(ClassWithBaseClassData));

			var passed = Assert.Single(testMessages.OfType<_TestPassed>());
			var passedStarting = testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single();
			Assert.Equal("Xunit3TheoryAcceptanceTests+PropertyDataTests+ClassWithBaseClassData.TestViaPropertyData(x: 42)", passedStarting.TestDisplayName);
		}

		class BaseClass
		{
			public static IEnumerable<object?[]> DataSource { get { yield return new object?[] { 42 }; } }
		}

		class ClassWithBaseClassData : BaseClass
		{
			[Theory]
			[MemberData("DataSource")]
			public void TestViaPropertyData(int x) { }
		}
	}

	public class CustomDataTests : AcceptanceTestV3
	{
		[Fact]
		public async void TestDataWithInternalConstructor_ReturnsTwoPassingTheories()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			var testMessages = await RunAsync(typeof(ClassWithCustomDataWithInternalDataCtor));

			Assert.Collection(
				testMessages.OfType<_TestPassed>().Select(passed => testMessages.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithCustomDataWithInternalDataCtor.Passing(unused: 2112)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithCustomDataWithInternalDataCtor.Passing(unused: 42)", displayName)
			);
			Assert.Empty(testMessages.OfType<_TestFailed>());
			Assert.Empty(testMessages.OfType<_TestSkipped>());
		}

		internal class MyCustomData : DataAttribute
		{
			internal MyCustomData() { }

			public override IReadOnlyCollection<object?[]> GetData(MethodInfo testMethod) =>
				new[]
				{
					new object?[] { 42 },
					new object?[] { 2112 }
				};
		}

		class ClassWithCustomDataWithInternalDataCtor
		{
			[Theory]
			[MyCustomData]
			public void Passing(int unused) { }
		}

		[Fact]
		public async void CanSupportConstructorOverloadingWithDataAttribute()  // https://github.com/xunit/xunit/issues/1711
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

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

			public override IReadOnlyCollection<object[]> GetData(MethodInfo testMethod) =>
				new[] { new[] { new object() } };
		}

		[Fact]
		public async void MemberDataAttributeBaseSubclass_Success()
		{
			var results = await RunAsync(typeof(ClassWithMemberDataAttributeBase));

			Assert.Collection(
				results.OfType<_TestPassed>().Select(passed => results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == passed.TestUniqueID).Single().TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: ""3"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: ""4"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: 1)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(unused: 2)", displayName)
			);
		}

		private class SingleMemberDataAttribute : MemberDataAttributeBase
		{
			public SingleMemberDataAttribute(string memberName, params object?[] parameters) : base(memberName, parameters) { }

			protected override object?[] ConvertDataItem(MethodInfo testMethod, object? item)
			{
				return new object?[] { item };
			}
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
		public async void EachTheoryHasIndividualExceptionMessage()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var equalStarting = Assert.Single(testMessages.OfType<_TestStarting>(), msg => msg.TestDisplayName == $"Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 42, y: {21.12:G17}, z: ClassUnderTest {{ }})");
			var equalFailure = Assert.Single(testMessages.OfType<_TestFailed>(), msg => msg.TestUniqueID == equalStarting.TestUniqueID);
			Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

			var notNullStarting = Assert.Single(testMessages.OfType<_TestStarting>(), msg => msg.TestDisplayName == "Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestViaInlineData(x: 0, y: 0, z: null)");
			var notNullFailure = Assert.Single(testMessages.OfType<_TestFailed>(), msg => msg.TestUniqueID == notNullStarting.TestUniqueID);
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
			[MemberData("Data")]
			public void TestViaInlineData(int x, double y, object z)
			{
				Assert.Equal(0, x); // Fails the first data item
				Assert.NotNull(z);  // Fails the second data item
			}
		}
	}

	public class OverloadedMethodTests : AcceptanceTestV3
	{
		[Fact]
		public async void TestMethodMessagesOnlySentOnce()
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
