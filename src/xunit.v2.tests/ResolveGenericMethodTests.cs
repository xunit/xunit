using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ResolveGenericMethodTests
{
    public static IEnumerable<object[]> ResolveGenericType_TestData()
    {
        // Method()
        yield return new object[]
        {
            nameof(NoGenericParameters_NoParameters),
            new object[0],
            new Type[0]
        };

        // Method(int)
        yield return new object[]
        {
            nameof(NoGenericParameters_OneParameter),
            new object[] { 1 },
            new Type[0]
        };

        // Method<T>()
        yield return new object[]
        {
            nameof(OneGenericParameter_NotUsed_NoParameters),
            new object[0],
            new Type[] { typeof(object) }
        };

        // Method<T>(T) non-null
        yield return new object[]
        {
            nameof(OneGenericParameter_Used_OneParameter),
            new object[] { 1 },
            new Type[] { typeof(int) }
        };

        // Method<T>(T) null
        yield return new object[]
        {
            nameof(OneGenericParameter_Used_OneParameter),
            new object[] { null },
            new Type[] { typeof(object) }
        };

        // Method<T>(T) array
        yield return new object[]
        {
            nameof(OneGenericParameter_Used_OneParameter),
            new object[] { new int[5] },
            new Type[] { typeof(int[]) }
        };

        // Method<T>(T, T) matching
        yield return new object[]
        {
            nameof(OneGenericParameter_UsedTwice_TwoParameters),
            new object[] { 1, 2 },
            new Type[] { typeof(int) }
        };

        // Method<T>(T, T) non matching
        yield return new object[]
        {
            nameof(OneGenericParameter_UsedTwice_TwoParameters),
            new object[] { 1, "2" },
            new Type[] { typeof(int) }
        };

        // Method<T>(T, int)
        yield return new object[]
        {
            nameof(OneGenericParameter_UsedOnceFirst_TwoParameters),
            new object[] { "1", 2 },
            new Type[] { typeof(string) }
        };

        // Method<T>(int, T)
        yield return new object[]
        {
            nameof(OneGenericParameter_UsedOnceSecond_TwoParameters),
            new object[] { 1, "2" },
            new Type[] { typeof(string) }
        };

        // Method<T>(int)
        yield return new object[]
        {
            nameof(OneGenericParameter_NotUsed_OneParameter),
            new object[] { 1 },
            new Type[] { typeof(object) }
        };

        // Method<T, U>()
        yield return new object[]
        {
            nameof(TwoGenericParameters_NoneUsed_NoParameters),
            new object[0],
            new Type[] { typeof(object), typeof(object) }
        };

        // Method<T, U>(int)
        yield return new object[]
        {
            nameof(TwoGenericParameters_NoneUsed_OneParameter),
            new object[] { 1 },
            new Type[] { typeof(object), typeof(object) }
        };

        // Method<T, U>(int, long)
        yield return new object[]
        {
            nameof(TwoGenericParameters_NoneUsed_TwoParameters),
            new object[] { 1, 2L },
            new Type[] { typeof(object), typeof(object) }
        };

        // Method<T, U>(T)
        yield return new object[]
        {
            nameof(TwoGenericParameters_OnlyFirstUsed_OneParameter),
            new object[] { 1 },
            new Type[] { typeof(int), typeof(object) }
        };

        // Method<T, U>(U)
        yield return new object[]
        {
            nameof(TwoGenericParameters_OnlySecondUsed_OneParameter),
            new object[] { 1 },
            new Type[] { typeof(object), typeof(int) }
        };

        // Method<T, U>(T, T) matching
        yield return new object[]
        {
            nameof(TwoGenericParameters_OnlyFirstUsed_TwoParameters),
            new object[] { 1, 2 },
            new Type[] { typeof(int), typeof(object) }
        };

        // Method<T, U>(T, T) unmatching
        yield return new object[]
        {
            nameof(TwoGenericParameters_OnlyFirstUsed_TwoParameters),
            new object[] { 1, "2" },
            new Type[] { typeof(int), typeof(object) }
        };

        // Method<T, U>(U, U) matching
        yield return new object[]
        {
            nameof(TwoGenericParameters_OnlySecondUsed_TwoParameters),
            new object[] { 1, 2 },
            new Type[] { typeof(object), typeof(int) }
        };

        // Method<T, U>(U, U) unmatching
        yield return new object[]
        {
            nameof(TwoGenericParameters_OnlySecondUsed_TwoParameters),
            new object[] { 1, "2" },
            new Type[] { typeof(object), typeof(int) }
        };

        // Method<T, U>(T, U) with normal inputs
        yield return new object[]
        {
            nameof(TwoGenericParameters_TwoUsed_TwoParameters),
            new object[] { 5, null },
            new Type[] { typeof(int), typeof(object) }
        };

        // Method<T, U>(T, U) with array inputs
        yield return new object[]
        {
            nameof(TwoGenericParameters_TwoUsed_TwoParameters),
            new object[] { new int[1], new string[1] },
            new Type[] { typeof(int[]), typeof(string[]) }
        };

        // Method<T, U>(T, int)
        yield return new object[]
        {
            nameof(TwoGenericParameters_FirstUsedFirst_TwoParameters),
            new object[] { "5", 5 },
            new Type[] { typeof(string), typeof(object) }
        };

        // Method<T, U>(U, int)
        yield return new object[]
        {
            nameof(TwoGenericParameters_SecondUsedFirst_TwoParameters),
            new object[] { "5", 5 },
            new Type[] { typeof(object), typeof(string) }
        };

        // Method<T, U>(int, U)
        yield return new object[]
        {
            nameof(TwoGenericParameters_FirstUsedSecond_TwoParameters),
            new object[] { 5, "5" },
            new Type[] { typeof(string), typeof(object) }
        };

        // Method<T, U>(int, U)
        yield return new object[]
        {
            nameof(TwoGenericParameters_SecondUsedSecond_TwoParameters),
            new object[] { 5, "5" },
            new Type[] { typeof(object), typeof(string) }
        };

        // Method<T>(T[]>)
        yield return new object[]
        {
            nameof(GenericArrayTest),
            new object[] { new int[5] },
            new Type[] { typeof(int) }
        };

        // Method<T>(ref T>)
        yield return new object[]
        {
            nameof(GenericRefTest),
            new object[] { "abc" },
            new Type[] { typeof(string) }
        };

        // Method<T>(Generic<T>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_OneGenericParameter_Used),
            new object[] { new GenericClass<string>() },
            new Type[] { typeof(string) }
        };

        // Method<T>(Generic<string>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_OneGenericParameter_Unused),
            new object[] { new GenericClass<string>() },
            new Type[] { typeof(object) }
        };

        // Method(Generic<string>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_NoGenericParameters),
            new object[] { new GenericClass<string>() },
            new Type[0]
        };

        // Method<T>(Generic<T[]>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_OneGenericParameter_Array),
            new object[] { new GenericClass<string[]>() },
            new Type[] { typeof(string) }
        };

        // Method<T>(Generic<T?>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_OneGenericParameter_Nullable),
            new object[] { new GenericClass<int?>() },
            new Type[] { typeof(int) }
        };

        // Method<T>(Generic<T>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_OneGenericParameter_Used),
            new object[] { new GenericClass<GenericClass<string>>() },
            new Type[] { typeof(GenericClass<string>) }
        };

        // Method<T>(Generic<Generic<T>>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric1_OneGenericParameter_Recursive),
            new object[] { new GenericClass<GenericClass<string>>() },
            new Type[] { typeof(string) }
        };

        // Method<T>(Generic<T>[])
        yield return new object[]
        {
            nameof(GenericArrayOfEmbeddedGeneric1_OneGenericParameter),
            new object[] { new GenericClass<int>[1] },
            new Type[] { typeof(int) }
        };

        // Method<T>(T?[])
        yield return new object[]
        {
            nameof(GenericArrayOfGenericNullable1_OneGenericParameter),
            new object[] { new int?[1] },
            new Type[] { typeof(int) }
        };

        // Method<T, U>(Generic2<T, U>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_TwoGenericParameters_SameType),
            new object[] { new GenericClass2<string, int>() },
            new Type[] { typeof(string), typeof(int) }
        };

        // Method<T>(Generic2<T, int>)
        yield return new object[]
        {
            nameof(EmbeddedGenericGeneric2_OneGenericParameter_First),
            new object[] { new GenericClass2<string, int>() },
            new Type[] { typeof(string) }
        };

        // Method<T>(Generic2<string, T>)
        yield return new object[]
        {
            nameof(EmbeddedGenericGeneric2_OneGenericParameter_Second),
            new object[] { new GenericClass2<string, int>() },
            new Type[] { typeof(int) }
        };

        // Method<T>(Generic2<string, int>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_OneGeneric_Unused),
            new object[] { new GenericClass2<string, int>() },
            new Type[] { typeof(object) }
        };

        // Method(Generic2<string, int>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_NotGeneric),
            new object[] { new GenericClass2<string, int>() },
            new Type[0]
        };

        // Method<T, U>(Generic2<T, int>, Generic2<ulong, T>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest1),
            new object[] { new GenericClass2<string, int>(), new GenericClass2<ulong, long>() },
            new Type[] { typeof(string), typeof(long) }
        };

        // Method<T, U>(Generic2<T, int>, Generic2<T, long>)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest2),
            new object[] { new GenericClass2<string, int>(), new GenericClass2<ulong, long>() },
            new Type[] { typeof(string), typeof(ulong) }
        };

        // Method<T, U>(Generic2<string, T>, Generic2<T, long)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest3),
            new object[] { new GenericClass2<string, int>(), new GenericClass2<ulong, long>() },
            new Type[] { typeof(int), typeof(ulong) }
        };

        // Method<T, U>(Generic2<string, T>, Generic2<ulong, T)
        yield return new object[]
        {
            nameof(EmbeddedGeneric2_TwoGenericParameters_DifferentTypeTest4),
            new object[] { new GenericClass2<string, int>(), new GenericClass2<ulong, long>() },
            new Type[] { typeof(int), typeof(long) }
        };

        // Stress test
        yield return new object[]
        {
            nameof(CrazyGenericMethod),
            new object[] { new GenericClass3<GenericClass<bool>, GenericClass2<GenericClass3<ulong, long, int>, string>, uint>() },
            new Type[] { typeof(bool), typeof(ulong), typeof(long), typeof(object), typeof(uint) }
        };

        // Func test
        yield return new object[]
        {
            nameof(FuncTestMethod),
            new object[] { new int[] { 4, 5, 6, 7 }, 0, 0, new Func<int, float>(i => i + 0.5f) },
            new Type[] { typeof(float) }
        };

        yield return new object[]
        {
            nameof(FuncTestMethod),
            new object[] { new int[] { 4, 5, 6, 7 }, 0, 1, new Func<int, double>(i => i + 0.5d) },
            new Type[] { typeof(double) }
        };

        yield return new object[]
        {
            nameof(FuncTestMethod),
            new object[] { new int[] { 4, 5, 6, 7 }, 0, 2, new Func<int, int>(i => i) },
            new Type[] { typeof(int) }
        };
    }

    public static IEnumerable<object[]> ResolveGenericType_MismatchedGenericTypeArguments_TestData()
    {
        // SubClass: GenericBaseClass<int> -> GenericBaseClass<T>
        yield return new object[]
        {
            nameof(OneGenericParameter_GenericBaseClass),
            new object[] { new ImplementsGeneric1BaseClass() },
            new Type[] { typeof(int) }
        };

        // SubClass: BaseClass<int, string> -> BaseClass<T, U>
        yield return new object[]
        {
            nameof(TwoGenericParameters_GenericBaseClass),
            new object[] { new ImplementsGeneric2BaseClass() },
            new Type[] { typeof(int), typeof(uint) }
        };

        // SubClass<T>: BaseClass<T, string> -> BaseClass<T, U>
        yield return new object[]
        {
            nameof(TwoGenericParameters_GenericBaseClass),
            new object[] { new GenericImplements2BaseClass<int>() },
            new Type[] { typeof(int), typeof(string) }
        };

        // Class: Interface<int> -> Interface<T>
        yield return new object[]
        {
            nameof(OneGenericParameter_GenericInterface),
            new object[] { new ImplementsGeneric1Interface() },
            new Type[] { typeof(int) }
        };

        // Class: Interface<int, string> -> Interface<T, U>
        yield return new object[]
        {
            nameof(TwoGenericParameters_GenericInterface),
            new object[] { new ImplementsGeneric2Interface() },
            new Type[] { typeof(int), typeof(uint) }
        };

        // Class<T>: Interface<T, string> -> Interface<T, U>
        yield return new object[]
        {
            nameof(TwoGenericParameters_GenericInterface),
            new object[] { new GenericImplements2Interface<int>() },
            new Type[] { typeof(int), typeof(string) }
        };
    }

    [Theory]
    [MemberData(nameof(ResolveGenericType_TestData))]
    [MemberData(nameof(ResolveGenericType_MismatchedGenericTypeArguments_TestData))]
    public static void ResolveGenericType(string methodName, object[] parameters, Type[] expected)
    {
        IMethodInfo method = Reflector.Wrap(typeof(ResolveGenericMethodTests).GetMethod(methodName));
        Type[] actual = method.ResolveGenericTypes(parameters).Select(t => ((ReflectionTypeInfo)t).Type).ToArray();
        Assert.Equal(expected, actual);
    }

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

    public class GenericClass<T> { }
    public class GenericClass2<T, U> { }
    public class GenericClass3<T, U, V> { }

    public interface Generic1Interface<T> { }
    public interface Generic2Interface<T, U> { }

    public class ImplementsGeneric1BaseClass : GenericClass<int> { }
    public class ImplementsGeneric2BaseClass : GenericClass2<int, uint> { }

    public class ImplementsGeneric1Interface : Generic1Interface<int> { }
    public class ImplementsGeneric2Interface : Generic2Interface<int, uint> { }

    public class GenericImplements2BaseClass<T> : GenericClass2<T, string> { }
    public class GenericImplements2Interface<T> : Generic2Interface<T, string> { }
}
