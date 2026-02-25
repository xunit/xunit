#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable xUnit1007 // Should be able to remove this when https://github.com/xunit/xunit/issues/3507 is resolved
#pragma warning disable xUnit1019 // Should be able to remove this when https://github.com/xunit/xunit/issues/3508 is resolved
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows

using System.Collections;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Sdk;

public partial class Xunit3TheoryAcceptanceTests
{
	public partial class ClassDataTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassDataSource
		{
			public static readonly object[] Data =
			[
				new object?[] { "Hello from class source", 2600 },
				Tuple.Create("Hello from Tuple", 42),
				("Class source will fail", 2112),
				new TheoryDataRow("Class source would fail if I ran", 96) { Skip = "Do not run" },
				new TheoryDataRow<string, int>("I only run explicitly", 9600) { Explicit = true },
			];
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ClassDisposable
		{
			[Theory]
			[ClassData(typeof(DataSource_ClassDisposable))]
			public void TestMethod(string _1, int _2) { }
		}

		// This is IEnumerable instead of strongly typed because it returns all the various forms of
		// data that are valid in a data source.
		public sealed class DataSource_ClassDisposable : IEnumerable, IDisposable
		{
			void IDisposable.Dispose() =>
				TestContext.Current.SendDiagnosticMessage(typeof(DataSource_ClassDisposable).SafeName() + ".Dispose");

			IEnumerator IEnumerable.GetEnumerator() =>
				ClassDataSource.Data.GetEnumerator();
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ClassAsyncDisposable
		{
			[Theory]
			[ClassData(typeof(DataSource_ClassAsyncDisposable))]
			public void TestMethod(string _1, int _2) { }
		}

		// This is IEnumerable instead of strongly typed because it returns all the various forms of
		// data that are valid in a data source.
		public sealed class DataSource_ClassAsyncDisposable : IEnumerable, IAsyncLifetime
		{
			async ValueTask IAsyncDisposable.DisposeAsync() =>
				TestContext.Current.SendDiagnosticMessage(typeof(DataSource_ClassAsyncDisposable).SafeName() + ".DisposeAsync");

			async ValueTask IAsyncLifetime.InitializeAsync() =>
				TestContext.Current.SendDiagnosticMessage(typeof(DataSource_ClassAsyncDisposable).SafeName() + ".InitializeAsync");

			IEnumerator IEnumerable.GetEnumerator() =>
				ClassDataSource.Data.GetEnumerator();
		}
	}

	public partial class ClassDataTests_Generic
	{
#if XUNIT_AOT
		public
#endif
		class ClassDataSource
		{
			public static readonly object[] Data =
			[
				new object?[] { "Hello from class source", 2600 },
				Tuple.Create("Hello from Tuple", 42),
				("Class source will fail", 2112),
				new TheoryDataRow("Class source would fail if I ran", 96) { Skip = "Do not run" },
				new TheoryDataRow<string, int>("I only run explicitly", 9600) { Explicit = true },
			];
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ClassDisposable
		{
			[Theory]
			[ClassData<DataSource_ClassDisposable>]
			public void TestMethod(string _1, int _2) { }
		}

		// This is IEnumerable instead of strongly typed because it returns all the various forms of
		// data that are valid in a data source.
		public sealed class DataSource_ClassDisposable : IEnumerable, IDisposable
		{
			void IDisposable.Dispose() =>
				TestContext.Current.SendDiagnosticMessage(typeof(DataSource_ClassDisposable).SafeName() + ".Dispose");

			IEnumerator IEnumerable.GetEnumerator() =>
				ClassDataSource.Data.GetEnumerator();
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ClassAsyncDisposable
		{
			[Theory]
			[ClassData<DataSource_ClassAsyncDisposable>]
			public void TestMethod(string _1, int _2) { }
		}

		// This is IEnumerable instead of strongly typed because it returns all the various forms of
		// data that are valid in a data source.
		public sealed class DataSource_ClassAsyncDisposable : IEnumerable, IAsyncLifetime
		{
			async ValueTask IAsyncDisposable.DisposeAsync() =>
				TestContext.Current.SendDiagnosticMessage(typeof(DataSource_ClassAsyncDisposable).SafeName() + ".DisposeAsync");

			async ValueTask IAsyncLifetime.InitializeAsync() =>
				TestContext.Current.SendDiagnosticMessage(typeof(DataSource_ClassAsyncDisposable).SafeName() + ".InitializeAsync");

			IEnumerator IEnumerable.GetEnumerator() =>
				ClassDataSource.Data.GetEnumerator();
		}
	}

	public partial class DataAttributeTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ExplicitAcceptanceTests
		{
			public static List<TheoryDataRow<int, string>> MemberDataSource =
			[
				new(43, "Member inherited"),
				new(0, "Member forced true") { Explicit = true },
				new(2113, "Member forced false") { Explicit = false },
			];

			[Theory]
			[InlineData(42, "Inline inherited")]
			[InlineData(0, "Inline forced true", Explicit = true)]
			[InlineData(2112, "Inline forced false", Explicit = false)]
			[MemberData(nameof(MemberDataSource))]
			public void TestWithTheoryExplicitFalse(
				int x,
				string _)
			{
				Assert.NotEqual(0, x);
			}

			[Theory(Explicit = true)]
			[InlineData(42, "Inline inherited")]
			[InlineData(0, "Inline forced true", Explicit = true)]
			[InlineData(2112, "Inline forced false", Explicit = false)]
			[MemberData(nameof(MemberDataSource))]
			public void TestWithTheoryExplicitTrue(
				int x,
				string _)
			{
				Assert.NotEqual(0, x);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_LabelAcceptanceTests
		{
			public static TheoryDataRow<int, string>[] MemberDataSource =
			[
				new(42, "Member unset"),
				new(0, "Member empty") { Label = "" },
				new(2112, "Member set") { Label = "Custom member" },
			];

			[Theory]
			[InlineData(42, "Inline unset")]
			[InlineData(0, "Inline empty", Label = "")]
			[InlineData(2112, "Inline set", Label = "Custom inline")]
			public void TestMethod_Inline(int x, string _)
			{
				Assert.NotEqual(0, x);
			}

			[Theory]
			[MemberData(nameof(MemberDataSource))]
			public void TestMethod_Member(int x, string _)
			{
				Assert.NotEqual(0, x);
			}

			[Theory]
			[MemberData(nameof(MemberDataSource), Label = "Base label")]
			public void TestMethod_MemberWithBaseLabel(int x, string _)
			{
				Assert.NotEqual(0, x);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_SkipTests
		{
			public static List<TheoryDataRow<int>> DataSource =
			[
				new(43),
				new(2113) { Skip = "Skip from theory data row" }
			];

			[Theory]
			[InlineData(42)]
			[InlineData(2112, Skip = "Skip from InlineData")]
			[MemberData(nameof(DataSource), Skip = "Skip from MemberData")]
			public void TestWithNoSkipOnTheory(int _)
			{ }

			[Theory(Skip = "Skip from theory")]
			[InlineData(42)]
			[InlineData(2112, Skip = "Skip from InlineData")]
			[MemberData(nameof(DataSource), Skip = "Skip from MemberData")]
			public void TestWithSkipOnTheory(int _)
			{ }

			public static bool AlwaysTrue => true;

			[Theory(Skip = "Dynamically skipped", SkipUnless = nameof(AlwaysTrue))]
			[InlineData(1)]
			[InlineData(2)]
			[InlineData(3, Skip = "Always skipped")]
			[InlineData(4, Skip = "Skip dynamically flipped", SkipWhen = nameof(AlwaysTrue))]
			public void TestWithDynamicSkipOnTheory(int _)
			{ }
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_TestDisplayNameTests
		{
			public static List<TheoryDataRow<int>> DefaultMemberDataSource =
			[
				new(43),
				new(1) { TestDisplayName = "One Test Default (Member)" },
			];

			[Theory]
			[InlineData(42)]
			[InlineData(0, TestDisplayName = "Zero Test Default (Inline)")]
			[MemberData(nameof(DefaultMemberDataSource), TestDisplayName = "Default Member Test")]
			public void TestWithDefaultName(int _)
			{ }

			public static List<TheoryDataRow<int>> OverrideMemberDataSource =
			[
				new(45),
				new(3) { TestDisplayName = "Three Test Override (Member)" },
			];

			[Theory(DisplayName = "Theory Display Name")]
			[InlineData(44)]
			[InlineData(2, TestDisplayName = "Two Test Override (Inline)")]
			[MemberData(nameof(OverrideMemberDataSource), TestDisplayName = "Override Member Test")]
			public void TestWithOverriddenName(int _)
			{ }
		}

		[Trait("Location", "Class")]
#if XUNIT_AOT
		public
#endif
		class ClassUnderTests_TraitsTests
		{
			public static List<TheoryDataRow<int>> MemberDataSource =
			[
				new TheoryDataRow<int>(2112),
				new TheoryDataRow<int>(42).WithTrait("Location", "TheoryDataRow"),
			];

			[Theory]
			[Trait("Location", "Method")]
			[InlineData(0, Traits = new[] { "Location", "InlineData", "Discarded" })]
			[MemberData(nameof(MemberDataSource), Traits = new[] { "Location", "MemberData", "Discarded" })]
			public void TestMethod(int _)
			{ }
		}
	}

	public partial class DataAttributeTimeoutTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest
		{
			public static List<TheoryDataRow<int>> MemberDataSource =
			[
				new TheoryDataRow<int>(11000),
				new TheoryDataRow<int>(100) { Timeout = 10000 },
			];

			[Theory(Timeout = 42)]
			[InlineData(10000)]
			[InlineData(10, Timeout = 10000)]
			[MemberData(nameof(MemberDataSource), Timeout = 10)]
			public Task LongRunningTask(int delay) =>
				Task.Delay(delay, TestContext.Current.CancellationToken);
		}
	}

	public partial class DataConversionTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithIncompatibleData
		{
			[Theory]
#pragma warning disable xUnit1010 // The value is not convertible to the method parameter type
			[InlineData("Foo")]
#pragma warning restore xUnit1010 // The value is not convertible to the method parameter type
			public void TestViaIncompatibleData(int _) { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithImplicitlyConvertibleData
		{
			[Theory]
			[InlineData(42)]
			public void TestViaImplicitData(int? _) { }
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

#if XUNIT_AOT
		public
#endif
		class ClassWithIConvertibleData
		{
			public static IEnumerable<ITheoryDataRow> Data = new TheoryData<MyConvertible>(new MyConvertible());

			[Theory]
			[MemberData(nameof(Data))]
			public void TestViaIConvertible(int _) { }
		}
	}

	public partial class ErrorAggregation
	{
#if XUNIT_AOT
		public
#endif
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
			public void TestMethod(int x, double _, object? z)
			{
				Assert.Equal(0, x); // Fails the first data item
				Assert.NotNull(z);  // Fails the second data item
			}
		}
	}

	public partial class InlineDataTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest
		{
			[Theory]
			[InlineData(42, 21.12, "Hello, world!")]
			[InlineData(0, 0.0, null)]
			public void TestMethod(int _1, double _2, string? z)
			{
				Assert.NotNull(z);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTestForNullValues
		{
			[Theory]
			[InlineData(null)]
			public void TestMethod(string? _) { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTestForArrays
		{
			[Theory]
			[InlineData(new[] { 42, 2112 }, new[] { "SELF", "PARENT1", "PARENT2", "PARENT3" }, null, 10.5, "Hello, world!")]
			public void TestMethod(int[] _1, string[] _2, float[]? _3, double _4, string _5) { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTestForValueArraysWithObjectParameter
		{
			[Theory]
			[InlineData(new[] { 42, 2112 }, typeof(int[]))]
			public void TestMethod(object value, Type expected)
			{
				Assert.IsType(expected, value);
			}
		}

#if XUNIT_AOT
		public
#endif
		enum SomeEnum
		{ A, B }

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncTaskMethod
		{
			[Theory]
			[InlineData(SomeEnum.A)]
			[InlineData(SomeEnum.B)]
			public async Task TestMethod(SomeEnum _)
			{
				await Task.Run(() => "Any statement, to prevent a C# compiler error");
			}
		}
	}

	public partial class MemberDataTests
	{

#if XUNIT_AOT
		public
#endif
		class DataBase
		{
			static readonly object?[] baseData =
			[
				new object?[] { "Hello from base" },
				Tuple.Create("Base will fail"),
				new TheoryDataRow<string>("Base would fail if I ran") { Skip = "Do not run" },
			];
			static readonly IAsyncEnumerable<object?> baseDataAsync = baseData.ToAsyncEnumerable();

			protected static readonly object?[] data =
			[
				new object?[] { "Hello, world!" },
				Tuple.Create("I will fail"),
				new TheoryDataRow<string>("I would fail if I ran") { Skip = "Do not run" },
			];
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

#if XUNIT_AOT
		public
#endif
		class OtherDataSource
		{
			static readonly object[] data =
			[
				new object?[] { "Hello from other source" },
				Tuple.Create("Other source will fail"),
				new TheoryDataRow<string>("Other source would fail if I ran") { Skip = "Do not run" },
			];
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

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_IAsyncEnumerable : DataBase
		{
			// These are IAsyncEnumerable<object?> instead of strongly typed because they return all the various forms of
			// data that are valid in a data source.
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

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_IEnumerable : DataBase
		{
			// These are IEnumerable instead of strongly typed because they return all the various forms of
			// data that are valid in a data source.
			public static IEnumerable FieldDataSource = data;
			public static IEnumerable MethodDataSource() => data;
			public static IEnumerable PropertyDataSource => data;

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(Enumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.Enumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(Enumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.Enumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(Enumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.Enumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_TaskOfIAsyncEnumerable : DataBase
		{
			// These are Task<IAsyncEnumerable<object?>> instead of strongly typed because they return all the various forms of
			// data that are valid in a data source.
			public static Task<IAsyncEnumerable<object?>> FieldDataSource = Task.FromResult(dataAsync);
			public static Task<IAsyncEnumerable<object?>> MethodDataSource() => Task.FromResult(dataAsync);
			public static Task<IAsyncEnumerable<object?>> PropertyDataSource => Task.FromResult(dataAsync);

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(TaskOfAsyncEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfAsyncEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(TaskOfAsyncEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfAsyncEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(TaskOfAsyncEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfAsyncEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_TaskOfIEnumerable : DataBase
		{
			// These are Task<IEnumerable> instead of strongly typed because they return all the various forms of
			// data that are valid in a data source.
			public static Task<IEnumerable> FieldDataSource = Task.FromResult<IEnumerable>(data);
			public static Task<IEnumerable> MethodDataSource() => Task.FromResult<IEnumerable>(data);
			public static Task<IEnumerable> PropertyDataSource => Task.FromResult<IEnumerable>(data);

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(TaskOfEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(TaskOfEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(TaskOfEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.TaskOfEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ValueTaskOfIAsyncEnumerable : DataBase
		{
			// These are ValueTask<IAsyncEnumerable<object?>> instead of strongly typed because they return all the various forms of
			// data that are valid in a data source.
			public static ValueTask<IAsyncEnumerable<object?>> FieldDataSource = new(dataAsync);
			public static ValueTask<IAsyncEnumerable<object?>> MethodDataSource() => new(dataAsync);
			public static ValueTask<IAsyncEnumerable<object?>> PropertyDataSource => new(dataAsync);

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(ValueTaskOfAsyncEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfAsyncEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(ValueTaskOfAsyncEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfAsyncEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(ValueTaskOfAsyncEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfAsyncEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_ValueTaskOfIEnumerable : DataBase
		{
			// These are ValueTask<IEnumerable> instead of strongly typed because they return all the various forms of
			// data that are valid in a data source.
			public static ValueTask<IEnumerable> FieldDataSource = new(data);
			public static ValueTask<IEnumerable> MethodDataSource() => new(data);
			public static ValueTask<IEnumerable> PropertyDataSource => new(data);

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(FieldDataSource))]
			[MemberData(nameof(ValueTaskOfEnumerable_FieldBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfEnumerable_FieldOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void FieldTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(MethodDataSource))]
			[MemberData(nameof(ValueTaskOfEnumerable_MethodBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfEnumerable_MethodOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void MethodTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}

			[Theory]
#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type
			[MemberData(nameof(PropertyDataSource))]
			[MemberData(nameof(ValueTaskOfEnumerable_PropertyBaseDataSource))]
			[MemberData(nameof(OtherDataSource.ValueTaskOfEnumerable_PropertyOtherDataSource), MemberType = typeof(OtherDataSource))]
#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type
			public void PropertyTestMethod(string z)
			{
				Assert.DoesNotContain("fail", z);
			}
		}
	}

	public partial class MethodDataTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithMismatchedMethodData
		{
			public static IEnumerable<object?[]> DataSource(int x) => null!;

			[Theory]
#pragma warning disable xUnit1035 // The value is not convertible to the method parameter type
			[MemberData(nameof(DataSource), "Hello world")]
#pragma warning restore xUnit1035 // The value is not convertible to the method parameter type
			public void TestViaMethodData(int _1, double _2, string _3) { }
		}

#if XUNIT_AOT
		public
#endif
		abstract class BaseClassWithTestAndData
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

#if XUNIT_AOT
		public
#endif
		class SubClassWithNoTests : BaseClassWithTestAndData
		{ }

#if XUNIT_AOT
		public
#endif
		class ClassWithParameterizedMethodData
		{
			public static IEnumerable<object?[]> DataSource(int x) =>
			[
				[x / 2, 21.12, "Hello, world!"],
				[0, 0.0, null]
			];

			[Theory]
			[MemberData(nameof(DataSource), 84)]
			public void TestViaMethodData(int _1, double _2, string z)
			{
				Assert.NotNull(z);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithDowncastedMethodData
		{
			public static IEnumerable<object?[]> DataSource(object x, string? y) { yield return new object?[] { 42, 21.12, "Hello world" }; }

			[Theory]
			[MemberData(nameof(DataSource), 42, "Hello world")]
			[MemberData(nameof(DataSource), 21.12, null)]
			public void TestViaMethodData(int _1, double _2, string _3) { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithDataMethodsWithOptionalParameters
		{
			public static IEnumerable<object[]> DataMethodWithOptionalParameters(string name, int scenarios = 444)
			{
				for (int i = 1; i <= scenarios; i++)
					yield return new object[] { name, i };
			}

			[Theory]
			[MemberData(nameof(DataMethodWithOptionalParameters), "MyFirst")]
			[MemberData(nameof(DataMethodWithOptionalParameters), "MySecond")]
			public void TestMethod(string name, int scenario)
			{
				Assert.True(name.Length > 0);
				Assert.True(scenario > 0);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncDataSources
		{
			public static async Task<IEnumerable<object?[]>> TaskData()
			{
				await Task.Yield();
				return [[42, 21.12m, "Hello world"]];
			}

			public static async ValueTask<IEnumerable<TheoryDataRow<int, decimal, string?>>> ValueTaskData()
			{
				await Task.Yield();
				return [
					new TheoryDataRow<int, decimal, string?>(0, 0m, null),
					new TheoryDataRow<int, decimal, string?>(1, 2.3m, "No") { Skip = "This row is skipped" },
				];
			}

			[Theory]
			[MemberData(nameof(TaskData))]
			[MemberData(nameof(ValueTaskData))]
			public void TestMethod(int _1, decimal _2, string? _3)
			{ }
		}
	}

	public partial class TheoryTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithOptionalParameters
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
			[InlineData(null)]
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

#if XUNIT_AOT
		public
#endif
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
				[[new Explicit { Value = "abc" }]];

			// Explicit conversion defined on the argument's type
			[Theory]
			[MemberData(nameof(ExplicitArgument))]
			public void ArgumentDeclaredExplicitConversion(string value)
			{
				Assert.Equal("abc", value);
			}

			public static IEnumerable<object?[]> ImplicitArgument =
				[[new Implicit { Value = "abc" }]];

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

#if XUNIT_AOT
		public
#endif
		class ClassWithSkips
		{
			[Theory(Skip = "Don't run this!")]
			[InlineData(42, "Hello, world!")]
			[InlineData(0, null)]
			public void SkippedTheory(int _, string? y)
			{
				Assert.NotNull(y);
			}

			[Theory]
			[InlineData(42, "Hello, world!")]
			[InlineData(0, null, Skip = "Don't run this!")]
			public void SkippedInlineData(int _, string? y)
			{
				Assert.NotNull(y);
			}

			[Theory]
			[InlineData(42, "Hello, world!")]
			[MemberData(nameof(MemberDataSource), Skip = "Don't run this!")]
			public void SkippedMemberData(int _, string? y)
			{
				Assert.NotNull(y);
			}

			public static IEnumerable<object?[]> MemberDataSource()
			{
				yield return new object?[] { 0, null };
			}

			[Theory]
			[MemberData(nameof(DataRowSource))]
			public void SkippedDataRow(int _, string? y)
			{
				Assert.NotNull(y);
			}

			public static IEnumerable<ITheoryDataRow> DataRowSource()
			{
				yield return new TheoryDataRow<int, string?>(42, "Hello, world!");
				yield return new TheoryDataRow<int, string?>(0, null) { Skip = "Don't run this!" };
			}
		}
	}
}
