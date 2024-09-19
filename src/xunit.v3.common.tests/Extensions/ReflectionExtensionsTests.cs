using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

[assembly: ReflectionExtensionsTests.GetMatchingCustomAttributes.AttributeUnderTest]

#if !NETFRAMEWORK
[assembly: ReflectionExtensionsTests.GetMatchingCustomAttributes.AttributeUnderTest<int>]
#endif

public class ReflectionExtensionsTests
{
	[Fact]
	public void GetDefaultValue()
	{
		Assert.Null(typeof(object).GetDefaultValue());
		Assert.Equal(0, typeof(int).GetDefaultValue());
	}

	[Theory]
	// No parameter
	[InlineData(nameof(DisplayNameClass.Parameterless), null, null, "Parameterless")]                           // match (no args)
	[InlineData(nameof(DisplayNameClass.Parameterless), new object?[0], null, "Parameterless()")]               // match (empty args)
	[InlineData(nameof(DisplayNameClass.Parameterless), new object?[] { 42 }, null, "Parameterless(???: 42)")]  // extra arg

	// One parameter
	[InlineData(nameof(DisplayNameClass.OneParameter), new object?[] { 42 }, null, "OneParameter(x: 42)")]  // match
	[InlineData(nameof(DisplayNameClass.OneParameter), new object?[0], null, "OneParameter(x: ???)")]       // missing arg

	// One generic parameter
	[InlineData(nameof(DisplayNameClass.OneGeneric), new object?[] { 42 }, new[] { typeof(int) }, "OneGeneric<Int32>(x: 42)")]  // match

	// Optional parameter
	[InlineData(nameof(DisplayNameClass.Optional), new object?[] { 42 }, null, "Optional(x: 42)")]  // match
	[InlineData(nameof(DisplayNameClass.Optional), new object?[0], null, "Optional(x: 2112)")]      // default value
	public void GetDisplayNameWithArguments(
		string methodName,
		object?[]? arguments,
		Type[]? genericTypes,
		string expectedDisplayName)
	{
		var method = typeof(DisplayNameClass).GetMethod(methodName);
		Assert.NotNull(method);

		var result = method.GetDisplayNameWithArguments(methodName, arguments, genericTypes);

		Assert.Equal(expectedDisplayName, result);
	}

	class DisplayNameClass
	{
		public void Parameterless() { }
		public void OneParameter(int x) { }
		public void OneGeneric<T>(T x) { }
		public void Optional(int x = 2112) { }
	}

	public class GetMatchingCustomAttributes
	{
		public interface IAttributeUnderTest { }

		[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
		public sealed class AttributeUnderTest : Attribute, IAttributeUnderTest { }

#if !NETFRAMEWORK
		[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
		public sealed class AttributeUnderTest<T> : Attribute, IAttributeUnderTest { }
#endif

		[Fact]
		public void ForAssembly()
		{
			var attrs = typeof(ReflectionExtensionsTests).Assembly.GetMatchingCustomAttributes(typeof(IAttributeUnderTest));

			Assert.Contains(attrs, attr => attr is AttributeUnderTest);
#if !NETFRAMEWORK
			Assert.Contains(attrs, attr => attr is AttributeUnderTest<int>);
#endif
		}

		[Fact]
		public void ForAttribute()
		{
			var attrs = new AttributeWithAttribute().GetMatchingCustomAttributes(typeof(IAttributeUnderTest));

			Assert.Contains(attrs, attr => attr is AttributeUnderTest);
#if !NETFRAMEWORK
			Assert.Contains(attrs, attr => attr is AttributeUnderTest<int>);
#endif
		}

		[AttributeUnderTest]
#if !NETFRAMEWORK
		[AttributeUnderTest<int>]
#endif
		class AttributeWithAttribute : Attribute { }

		[Fact]
		public void ForMethod()
		{
			var attrs = typeof(GetMatchingCustomAttributes).GetMethod(nameof(MethodWithAttribute), BindingFlags.NonPublic | BindingFlags.Static)?.GetMatchingCustomAttributes(typeof(IAttributeUnderTest)) ?? [];

			Assert.Contains(attrs, attr => attr is AttributeUnderTest);
#if !NETFRAMEWORK
			Assert.Contains(attrs, attr => attr is AttributeUnderTest<int>);
#endif
		}

		[AttributeUnderTest]
#if !NETFRAMEWORK
		[AttributeUnderTest<int>]
#endif
		static void MethodWithAttribute() { }

		[Fact]
		public void ForParameter()
		{
			var method = typeof(GetMatchingCustomAttributes).GetMethod(nameof(MethodWithParameter), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);
			var attrs = method.GetParameters()[0].GetMatchingCustomAttributes(typeof(IAttributeUnderTest));

			Assert.Contains(attrs, attr => attr is AttributeUnderTest);
#if !NETFRAMEWORK
			Assert.Contains(attrs, attr => attr is AttributeUnderTest<int>);
#endif
		}

		static void MethodWithParameter(
			[AttributeUnderTest]
#if !NETFRAMEWORK
			[AttributeUnderTest<int>]
#endif
			int _
		)
		{ }

		[Fact]
		public void ForType()
		{
			var attrs = typeof(ClassWithAttribute).GetMatchingCustomAttributes(typeof(IAttributeUnderTest));

			Assert.Contains(attrs, attr => attr is AttributeUnderTest);
#if !NETFRAMEWORK
			Assert.Contains(attrs, attr => attr is AttributeUnderTest<int>);
#endif
		}

		[AttributeUnderTest]
#if !NETFRAMEWORK
		[AttributeUnderTest<int>]
#endif
		class ClassWithAttribute { }
	}

	[Fact]
	public void Implements()
	{
		Assert.True(typeof(string).Implements(typeof(IEnumerable<char>)));
		Assert.True(typeof(IEnumerable<>).Implements(typeof(IEnumerable)));
		Assert.True(typeof(IEnumerable<int>).Implements(typeof(IEnumerable)));

		Assert.False(typeof(object).Implements(typeof(IEnumerable)));
		Assert.False(typeof(IAsyncDisposable).Implements(typeof(IDisposable)));
	}

	[Fact]
	public void IsFromLocalAssembly()
	{
		Assert.True(typeof(MyEnum).IsFromLocalAssembly());
#if NETFRAMEWORK
		if (!EnvironmentHelper.IsMono)
			Assert.False(typeof(System.Xml.ConformanceLevel).IsFromLocalAssembly());
#endif
	}

	enum MyEnum { }

	[Fact]
	public void IsNullable()
	{
		Assert.True(typeof(object).IsNullable());
		Assert.True(typeof(string).IsNullable());
		Assert.True(typeof(IXunitSerializable).IsNullable());
		Assert.True(typeof(char?).IsNullable());
		Assert.False(typeof(char).IsNullable());
	}

	[Fact]
	public void IsNullableEnum()
	{
		Assert.True(typeof(MyEnum?).IsNullableEnum());
		Assert.False(typeof(MyEnum).IsNullableEnum());
		Assert.False(typeof(char?).IsNullableEnum());
	}

	public class ResolveGenericTypes
	{
		public static IEnumerable<TheoryDataRow<string, object?[], Type[]>> MethodTestData()
		{
			// Method()
			yield return new(nameof(GenericMethodContainer.NoGenericParameters_NoParameters), [], []);

			// Method(int)
			yield return new(nameof(GenericMethodContainer.NoGenericParameters_OneParameter), [1], []);

			// Method<T>()
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_NotUsed_NoParameters), [], [typeof(object)]);

			// Method<T>(T) non-null
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_Used_OneParameter), [1], [typeof(int)]);

			// Method<T>(T) null
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_Used_OneParameter), [null], [typeof(object)]);

			// Method<T>(T) array
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_Used_OneParameter), [new int[5]], [typeof(int[])]);

			// Method<T>(T, T) matching
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_UsedTwice_TwoParameters), [1, 2], [typeof(int)]);

			// Method<T>(T, T) non matching
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_UsedTwice_TwoParameters), [1, "2"], [typeof(int)]);

			// Method<T>(T, int)
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_UsedOnceFirst_TwoParameters), ["1", 2], [typeof(string)]);

			// Method<T>(int, T)
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_UsedOnceSecond_TwoParameters), [1, "2"], [typeof(string)]);

			// Method<T>(int)
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_NotUsed_OneParameter), [1], [typeof(object)]);

			// Method<T, U>()
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_NoneUsed_NoParameters), [], [typeof(object), typeof(object)]);

			// Method<T, U>(int)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_NoneUsed_OneParameter), [1], [typeof(object), typeof(object)]);

			// Method<T, U>(int, long)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_NoneUsed_TwoParameters), [1, 2L], [typeof(object), typeof(object)]);

			// Method<T, U>(T)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_OnlyFirstUsed_OneParameter), [1], [typeof(int), typeof(object)]);

			// Method<T, U>(U)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_OnlySecondUsed_OneParameter), [1], [typeof(object), typeof(int)]);

			// Method<T, U>(T, T) matching
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_OnlyFirstUsed_TwoParameters), [1, 2], [typeof(int), typeof(object)]);

			// Method<T, U>(T, T) unmatching
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_OnlyFirstUsed_TwoParameters), [1, "2"], [typeof(int), typeof(object)]);

			// Method<T, U>(U, U) matching
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_OnlySecondUsed_TwoParameters), [1, 2], [typeof(object), typeof(int)]);

			// Method<T, U>(U, U) unmatching
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_OnlySecondUsed_TwoParameters), [1, "2"], [typeof(object), typeof(int)]);

			// Method<T, U>(T, U) with normal inputs
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_TwoUsed_TwoParameters), [5, null], [typeof(int), typeof(object)]);

			// Method<T, U>(T, U) with array inputs
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_TwoUsed_TwoParameters), [new int[1], new string[1]], [typeof(int[]), typeof(string[])]);

			// Method<T, U>(T, int)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_FirstUsedFirst_TwoParameters), ["5", 5], [typeof(string), typeof(object)]);

			// Method<T, U>(U, int)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_SecondUsedFirst_TwoParameters), ["5", 5], [typeof(object), typeof(string)]);

			// Method<T, U>(int, U)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_FirstUsedSecond_TwoParameters), [5, "5"], [typeof(string), typeof(object)]);

			// Method<T, U>(int, U)
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_SecondUsedSecond_TwoParameters), [5, "5"], [typeof(object), typeof(string)]);

			// Method<T>(T[]>)
			yield return new(nameof(GenericMethodContainer.GenericArrayTest), [new int[5]], [typeof(int)]);

			// Method<T>(ref T)
			yield return new(nameof(GenericMethodContainer.GenericRefTest), ["abc"], [typeof(string)]);

			// Method<T>(Generic<T>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_OneGenericParameter_Used), [new GenericClass<string>()], [typeof(string)]);

			// Method<T>(Generic<string>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_OneGenericParameter_Unused), [new GenericClass<string>()], [typeof(object)]);

			// Method(Generic<string>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_NoGenericParameters), [new GenericClass<string>()], []);

			// Method<T>(Generic<T[]>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_OneGenericParameter_Array), [new GenericClass<string[]>()], [typeof(string)]);

			// Method<T>(Generic<T?>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_OneGenericParameter_Nullable), [new GenericClass<int?>()], [typeof(int)]);

			// Method<T>(Generic<T>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_OneGenericParameter_Used), [new GenericClass<GenericClass<string>>()], [typeof(GenericClass<string>)]);

			// Method<T>(Generic<Generic<T>>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric1_OneGenericParameter_Recursive), [new GenericClass<GenericClass<string>>()], [typeof(string)]);

			// Method<T>(Generic<T>[])
			yield return new(nameof(GenericMethodContainer.GenericArrayOfEmbeddedGeneric1_OneGenericParameter), [new GenericClass<int>[1]], [typeof(int)]);

			// Method<T>(T?[])
			yield return new(nameof(GenericMethodContainer.GenericArrayOfGenericNullable1_OneGenericParameter), [new int?[1]], [typeof(int)]);

			// Method<T, U>(Generic2<T, U>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_TwoGenericParameters_SameType), [new GenericClass2<string, int>()], [typeof(string), typeof(int)]);

			// Method<T>(Generic2<T, int>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGenericGeneric2_OneGenericParameter_First), [new GenericClass2<string, int>()], [typeof(string)]);

			// Method<T>(Generic2<string, T>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGenericGeneric2_OneGenericParameter_Second), [new GenericClass2<string, int>()], [typeof(int)]);

			// Method<T>(Generic2<string, int>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_OneGeneric_Unused), [new GenericClass2<string, int>()], [typeof(object)]);

			// Method(Generic2<string, int>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_NotGeneric), [new GenericClass2<string, int>()], []);

			// Method<T, U>(Generic2<T, int>, Generic2<ulong, T>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest1), [new GenericClass2<string, int>(), new GenericClass2<ulong, long>()], [typeof(string), typeof(long)]);

			// Method<T, U>(Generic2<T, int>, Generic2<T, long>)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest2), [new GenericClass2<string, int>(), new GenericClass2<ulong, long>()], [typeof(string), typeof(ulong)]);

			// Method<T, U>(Generic2<string, T>, Generic2<T, long)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest3), [new GenericClass2<string, int>(), new GenericClass2<ulong, long>()], [typeof(int), typeof(ulong)]);

			// Method<T, U>(Generic2<string, T>, Generic2<ulong, T)
			yield return new(nameof(GenericMethodContainer.EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest4), [new GenericClass2<string, int>(), new GenericClass2<ulong, long>()], [typeof(int), typeof(long)]);

			// Stress test
			yield return new(nameof(GenericMethodContainer.CrazyGenericMethod), [new GenericClass3<GenericClass<bool>, GenericClass2<GenericClass3<ulong, long, int>, string>, uint>()], [typeof(bool), typeof(ulong), typeof(long), typeof(object), typeof(uint)]);

			// Func test
			yield return new(nameof(GenericMethodContainer.FuncTestMethod), [new int[] { 4, 5, 6, 7 }, 0, 0, new Func<int, float>(i => i + 0.5f)], [typeof(float)]);
			yield return new(nameof(GenericMethodContainer.FuncTestMethod), [new int[] { 4, 5, 6, 7 }, 0, 1, new Func<int, double>(i => i + 0.5d)], [typeof(double)]);
			yield return new(nameof(GenericMethodContainer.FuncTestMethod), [new int[] { 4, 5, 6, 7 }, 0, 2, new Func<int, int>(i => i)], [typeof(int)]);
		}

		public static IEnumerable<TheoryDataRow<string, object?[], Type[]>> MismatchedGenericTypeArguments_TestData()
		{
			// SubClass: GenericBaseClass<int> -> GenericBaseClass<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericBaseClass), [new ImplementsGeneric1BaseClass()], [typeof(int)]);

			// SubClass: BaseClass<int, string> -> BaseClass<T, U>
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_GenericBaseClass), [new ImplementsGeneric2BaseClass()], [typeof(int), typeof(uint)]);

			// SubClass<T>: BaseClass<T, string> -> BaseClass<T, U>
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_GenericBaseClass), [new GenericImplements2BaseClass<int>()], [typeof(int), typeof(string)]);

			// SubClass<T, U>: BaseClass<U> -> BaseClass<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericBaseClass), [new GenericImplements1BaseClass<int, string>()], [typeof(string)]);

			// SubClass<T, U>: (SubClass<T, U>: BaseClass<U>) -> BaseClass<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericBaseClass), [new GenericImplements2SubClassOf1BaseClass<int, string>()], [typeof(string)]);

			// Class: Interface<int> -> Interface<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericInterface), [new ImplementsGeneric1Interface()], [typeof(int)]);

			// Class: Interface<int, string> -> Interface<T, U>
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_GenericInterface), [new ImplementsGeneric2Interface()], [typeof(int), typeof(uint)]);

			// Class<T>: Interface<T, string> -> Interface<T, U>
			yield return new(nameof(GenericMethodContainer.TwoGenericParameters_GenericInterface), [new GenericImplements2Interface<int>()], [typeof(int), typeof(string)]);

			// SubClass<T, U>: Interface<U> -> Interface<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericInterface), [new GenericImplements1Interface<int, string>()], [typeof(string)]);

			// SubClass<T, U>: (SubClass<T, U>: Interface<U>) -> Interface<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericInterface), [new GenericImplements2SubClassOf1Interface<int, string>()], [typeof(string)]);

			// SubClass<T, U>: (Interface<T, U>: Interface<U>) -> Interface<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericInterface), [new GenericImplements2InterfaceOf1Interface<int, string>()], [typeof(string)]);

			// SubClass<T, U>: OtherInterface<T>, Interface<U> -> Interface<T>
			yield return new(nameof(GenericMethodContainer.OneGenericParameter_GenericInterface), [new GenericImplementsTwo1Interfaces<int, string>()], [typeof(string)]);
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(MethodTestData))]
		[MemberData(nameof(MismatchedGenericTypeArguments_TestData))]
		public static void ResolveGenericType(
			string methodName,
			object?[] parameters,
			Type[] expected)
		{
			var methodInfo = typeof(GenericMethodContainer).GetMethod(methodName);
			Assert.NotNull(methodInfo);

			var actual = methodInfo.ResolveGenericTypes(parameters);

			Assert.Equal(expected, actual);
		}

		public class GenericMethodContainer
		{
			public static void NoGenericParameters_NoParameters() { }
			public static void NoGenericParameters_OneParameter(int i) { }
			public static void OneGenericParameter_NotUsed_NoParameters<T>() { }
			public static void OneGenericParameter_NotUsed_OneParameter<T>(int i) { }
			public static void TwoGenericParameters_NoneUsed_NoParameters<T, U>() { }
			public static void TwoGenericParameters_NoneUsed_OneParameter<T, U>(int i) { }
			public static void TwoGenericParameters_NoneUsed_TwoParameters<T, U>(int i, long l) { }

			public static void OneGenericParameter_Used_OneParameter<T>(T t) { }
			public static void OneGenericParameter_UsedTwice_TwoParameters<T>(T t1, T t2) { }
			public static void OneGenericParameter_UsedOnceFirst_TwoParameters<T>(T t1, int i) { }
			public static void OneGenericParameter_UsedOnceSecond_TwoParameters<T>(int i, T t1) { }

			public static void TwoGenericParameters_OnlyFirstUsed_TwoParameters<T, U>(T t1, T t2) { }
			public static void TwoGenericParameters_OnlySecondUsed_TwoParameters<T, U>(U u1, U u) { }
			public static void TwoGenericParameters_OnlyFirstUsed_OneParameter<T, U>(T t) { }
			public static void TwoGenericParameters_OnlySecondUsed_OneParameter<T, U>(U u) { }
			public static void TwoGenericParameters_TwoUsed_TwoParameters<T, U>(T t, U u) { }
			public static void TwoGenericParameters_FirstUsedFirst_TwoParameters<T, U>(T t, int i) { }
			public static void TwoGenericParameters_SecondUsedFirst_TwoParameters<T, U>(U u, int i) { }
			public static void TwoGenericParameters_FirstUsedSecond_TwoParameters<T, U>(int i, T t) { }
			public static void TwoGenericParameters_SecondUsedSecond_TwoParameters<T, U>(int i, U u) { }

			public static void GenericArrayTest<T>(T[] value) { }

			public static void GenericRefTest<T>(ref T value) { }

			public static void EmbeddedGeneric1_OneGenericParameter_Used<T>(GenericClass<T> value) { }
			public static void EmbeddedGeneric1_OneGenericParameter_Unused<T>(GenericClass<string> value) { }
			public static void EmbeddedGeneric1_NoGenericParameters(GenericClass<string> value) { }

			public static void EmbeddedGeneric1_OneGenericParameter_Array<T>(GenericClass<T[]> generic) { }
			public static void EmbeddedGeneric1_OneGenericParameter_Nullable<T>(GenericClass<T?> generic) where T : struct { }

			public static void EmbeddedGeneric1_OneGenericParameter_Recursive<T>(GenericClass<GenericClass<T>> generic) { }

			public static void GenericArrayOfEmbeddedGeneric1_OneGenericParameter<T>(GenericClass<T>[] generic) { }
			public static void GenericArrayOfGenericNullable1_OneGenericParameter<T>(T?[] generic) where T : struct { }

			public static void EmbeddedGeneric2_TwoGenericParameters_SameType<T, U>(GenericClass2<T, U> t) { }
			public static void EmbeddedGenericGeneric2_OneGenericParameter_First<T>(GenericClass2<T, int> t) { }
			public static void EmbeddedGenericGeneric2_OneGenericParameter_Second<T>(GenericClass2<string, T> t) { }

			public static void EmbeddedGeneric2_OneGeneric_Unused<T>(GenericClass2<string, int> t) { }
			public static void EmbeddedGeneric2_NotGeneric(GenericClass2<string, int> t) { }

			public static void EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest1<T, U>(GenericClass2<T, int> t1, GenericClass2<ulong, U> t2) { }
			public static void EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest2<T, U>(GenericClass2<T, int> t1, GenericClass2<U, long> t2) { }
			public static void EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest3<T, U>(GenericClass2<string, T> t1, GenericClass2<U, long> t2) { }
			public static void EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest4<T, U>(GenericClass2<string, T> t1, GenericClass2<ulong, U> t2) { }

			public static void CrazyGenericMethod<T, U, V, W, X>(GenericClass3<GenericClass<T>, GenericClass2<GenericClass3<U, V, int>, string>, X> gen) { }

			public static void FuncTestMethod<TResult>(IEnumerable<int> source, int start, int length, Func<int, TResult> selector) { }

			public static void OneGenericParameter_GenericBaseClass<T>(GenericClass<T> x) { }
			public static void TwoGenericParameters_GenericBaseClass<T, U>(GenericClass2<T, U> x) { }

			public static void OneGenericParameter_GenericInterface<T>(Generic1Interface<T> x) { }
			public static void TwoGenericParameters_GenericInterface<T, U>(Generic2Interface<T, U> x) { }
		}

		public class GenericClass<T> { }
		public class GenericClass2<T, U> { }
		public class GenericClass3<T, U, V> { }

		public interface Generic1Interface<T> { }
		public interface OtherGeneric1Interface<T> { }
		public interface Generic2Interface<T, U> { }

		public class ImplementsGeneric1BaseClass : GenericClass<int> { }
		public class ImplementsGeneric2BaseClass : GenericClass2<int, uint> { }

		public class ImplementsGeneric1Interface : Generic1Interface<int> { }
		public class ImplementsGeneric2Interface : Generic2Interface<int, uint> { }

		public class GenericImplements2BaseClass<T> : GenericClass2<T, string> { }
		public class GenericImplements2Interface<T> : Generic2Interface<T, string> { }

		public class GenericImplements1Interface<T, U> : Generic1Interface<U> { }
		public class GenericImplementsTwo1Interfaces<T, U> : OtherGeneric1Interface<T>, Generic1Interface<U> { }
		public interface GenericExtends1Interface<T, U> : Generic1Interface<U> { }
		public class GenericImplements1BaseClass<T, U> : GenericClass<U> { }

		public class GenericImplements2SubClassOf1Interface<T, U> : GenericImplements1Interface<T, U> { }
		public class GenericImplements2InterfaceOf1Interface<T, U> : GenericExtends1Interface<T, U> { }
		public class GenericImplements2SubClassOf1BaseClass<T, U> : GenericImplements1BaseClass<T, U> { }
	}

	static void GenericMethod<T>(T _) { }

	public class ResolveMethodArguments
	{
		[Fact]
		public void NoArguments()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithNoArgs), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var result = method.ResolveMethodArguments([]);

			Assert.Empty(result);
		}

		static void MethodWithNoArgs() { }

		[Fact]
		public void TooFewArguments()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithTwoArgs), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);
			var args = new[] { new object() };

			var result = method.ResolveMethodArguments(args);

			Assert.Same(args, result);
		}

		static void MethodWithTwoArgs(object _1, int _2) { }

		[Fact]
		public void MethodWithDefaultValue_UsesDefaultValue()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithDefaultValue), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var results = method.ResolveMethodArguments([]);

			var result = Assert.Single(results);
			Assert.Equal(42, result);
		}

		static void MethodWithDefaultValue(int _ = 42) { }

		[Fact]
		public void MethodWithParams_NoValuesProvided()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithParams), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var results = method.ResolveMethodArguments([]);

			var result = Assert.Single(results);
			var resultArray = Assert.IsType<int[]>(result);
			Assert.Empty(resultArray);
		}

		[Fact]
		public void MethodWithParams_NullProvided()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithParams), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var results = method.ResolveMethodArguments([null]);

			var result = Assert.Single(results);
			Assert.Null(result);
		}

		[Fact]
		public void MethodWithParams_DirectArrayProvided()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithParams), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);
			var valueArray = new[] { 42, 2112 };

			var results = method.ResolveMethodArguments([valueArray]);

			var result = Assert.Single(results);
			Assert.Same(valueArray, result);
		}

		[Fact]
		public void MethodWithParams_MultipleValuesProvided()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithParams), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var results = method.ResolveMethodArguments([42, 2112]);

			var result = Assert.Single(results);
			var resultArray = Assert.IsType<int[]>(result);
			Assert.Equal(new[] { 42, 2112 }, resultArray);
		}

		[Fact]
		public void MethodWithParams_WrongType()
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithParams), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var ex = Record.Exception(() => method.ResolveMethodArguments([42, "Hello"]));

			Assert.IsType<InvalidOperationException>(ex);
			Assert.Equal("The arguments for this test method did not match the parameters: [42, \"Hello\"]", ex.Message);
		}

		static void MethodWithParams(params int[]? _) { }

		[Theory]
		[InlineData("Hello", "string: Hello")]
		[InlineData(42, "int: 42")]
		public void CanConvert(
			object input,
			string expectedConversion)
		{
			var method = typeof(ResolveMethodArguments).GetMethod(nameof(MethodWithConvertibleValue), BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(method);

			var results = method.ResolveMethodArguments([input]);

			var result = Assert.Single(results);
			var value1 = Assert.IsType<Value1>(result);
			Assert.Equal(expectedConversion, value1.Input);
		}

		static void MethodWithConvertibleValue(Value1 _) { }

		class Value1
		{
			Value1(string input) => Input = $"string: {input}";
			Value1(int input) => Input = $"int: {input}";

			public string Input { get; }

			public static implicit operator Value1(string input) => new(input);
			public static explicit operator Value1(int input) => new(input);
		}
	}

	[Fact]
	public void SafeName()
	{
		Assert.Equal("System.Object", typeof(object).SafeName());

		var genericMethod = typeof(ReflectionExtensionsTests).GetMethod(nameof(GenericMethod), BindingFlags.NonPublic | BindingFlags.Static);
		Assert.NotNull(genericMethod);
		var genericArgumentType = genericMethod.GetGenericArguments()[0];

		Assert.Equal("T", genericArgumentType.SafeName());
	}

	[Fact]
	public void ToCommaSeparatedList()
	{
		Assert.Equal("'System.Object', 'System.Int32'", new[] { typeof(object), typeof(int) }.ToCommaSeparatedList());
	}

	[Fact]
	public void ToDisplayName()
	{
		Assert.Equal("Object", typeof(object).ToDisplayName());
		Assert.Equal("Dictionary<String, List<Int32>>", typeof(Dictionary<string, List<int>>).ToDisplayName());
	}

	[Fact]
	public void ToSimpleName()
	{
		// Without namespace
		Assert.Equal("ReflectionExtensionsTests", typeof(ReflectionExtensionsTests).ToSimpleName());
		Assert.Equal("ReflectionExtensionsTests+ResolveMethodArguments", typeof(ResolveMethodArguments).ToSimpleName());

		// With namespace
		Assert.Equal("ParentClass", typeof(NS1.ParentClass).ToSimpleName());
		Assert.Equal("ParentClass+ChildClass", typeof(NS1.ParentClass.ChildClass).ToSimpleName());
	}

	[Fact]
	public void UnwrapNullable()
	{
		Assert.Equal(typeof(object), typeof(object).UnwrapNullable());
		Assert.Equal(typeof(int), typeof(int).UnwrapNullable());
		Assert.Equal(typeof(int), typeof(int?).UnwrapNullable());
	}
}

namespace NS1
{
	public class ParentClass
	{
		public class ChildClass { }
	}
}
