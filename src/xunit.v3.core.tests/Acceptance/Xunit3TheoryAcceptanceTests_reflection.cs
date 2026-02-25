#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows

using System.Collections;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class Xunit3TheoryAcceptanceTests
{
	partial class ClassDataTests
	{
		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask IncompatibleDataReturnType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_IncompatibleReturnType));

			var result = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest_IncompatibleReturnType.TestMethod", result.Test.TestDisplayName);
			Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
			Assert.Equal(
				"'Xunit3TheoryAcceptanceTests+ClassDataTests+ClassWithIncompatibleReturnType' must implement one of the following interfaces to be used as ClassData:" + Environment.NewLine +
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
#pragma warning disable xUnit1007 // ClassData must point at a valid class
			[ClassData(typeof(ClassWithIncompatibleReturnType))]
#pragma warning restore xUnit1007 // ClassData must point at a valid class
			public void TestMethod(int _) { }
		}

		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask IncompatibleDataValueType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_IncomptableValueData));

			var result = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests+ClassUnderTest_IncomptableValueData.TestMethod", result.Test.TestDisplayName);
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
#pragma warning disable xUnit1007 // ClassData must point at a valid class
			[ClassData(typeof(ClassWithIncompatibleValueData))]
#pragma warning restore xUnit1007 // ClassData must point at a valid class
			public void TestMethod(int _) { }
		}
	}

#if !NETFRAMEWORK

	partial class ClassDataTests_Generic
	{
		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask IncompatibleDataReturnType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_IncompatibleReturnType));

			var result = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests_Generic+ClassUnderTest_IncompatibleReturnType.TestMethod", result.Test.TestDisplayName);
			Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
			Assert.Equal(
				"'Xunit3TheoryAcceptanceTests+ClassDataTests_Generic+ClassWithIncompatibleReturnType' must implement one of the following interfaces to be used as ClassData:" + Environment.NewLine +
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
#pragma warning disable xUnit1007 // ClassData must point at a valid class
			[ClassData<ClassWithIncompatibleReturnType>]
#pragma warning restore xUnit1007 // ClassData must point at a valid class
			public void TestMethod(int _) { }
		}

		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask IncompatibleDataValueType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_IncomptableValueData));

			var result = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+ClassDataTests_Generic+ClassUnderTest_IncomptableValueData.TestMethod", result.Test.TestDisplayName);
			Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
			Assert.StartsWith("Class 'Xunit3TheoryAcceptanceTests+ClassDataTests_Generic+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
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
#pragma warning disable xUnit1007 // ClassData must point at a valid class
			[ClassData<ClassWithIncompatibleValueData>]
#pragma warning restore xUnit1007 // ClassData must point at a valid class
			public void TestMethod(int _) { }
		}
	}

#endif  // !NETFRAMEWORK

	// Custom data sources require custom source generators in AOT
	public class CustomDataTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestDataWithInternalConstructor_ReturnsTwoPassingTheories()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithCustomDataWithInternalDataCtor));

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithCustomDataWithInternalDataCtor.Passing(_: 2112)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithCustomDataWithInternalDataCtor.Passing(_: 42)", displayName)
			);
			Assert.Empty(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
		}

		class MyCustomData : DataAttribute
		{
			internal MyCustomData() { }

			public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
				MethodInfo testMethod,
				DisposalTracker disposalTracker) =>
					new([new TheoryDataRow<int>(42), new TheoryDataRow<int>(2112)]);

			public override bool SupportsDiscoveryEnumeration() => true;
		}

		class ClassWithCustomDataWithInternalDataCtor
		{
			[Theory]
			[MyCustomData]
			public void Passing(int _) { }
		}

		[Fact]
		public async ValueTask CanSupportConstructorOverloadingWithDataAttribute()  // https://github.com/xunit/xunit/issues/1711
		{
			var testMessages = await RunAsync(typeof(DataConstructorOverloadExample));

			Assert.Single(testMessages.OfType<ITestPassed>());
			Assert.Empty(testMessages.OfType<ITestFailed>());
			Assert.Empty(testMessages.OfType<ITestSkipped>());
		}

		class DataConstructorOverloadExample
		{
			[Theory]
			[MyData((object?)null)]
			public void TestMethod(object? _)
			{ }
		}

		class MyDataAttribute : DataAttribute
		{
			public MyDataAttribute(object? value)
			{ }

			public MyDataAttribute(string parameter2Name)
			{
				Assert.False(true);
			}

			public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
				MethodInfo testMethod,
				DisposalTracker disposalTracker) =>
					new([new TheoryDataRow(new object())]);

			public override bool SupportsDiscoveryEnumeration() => true;
		}

		[Fact]
		public async ValueTask MemberDataAttributeBaseSubclass_Success()
		{
			var results = await RunForResultsAsync(typeof(ClassWithMemberDataAttributeBase));

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(_: ""3"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(_: ""4"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(_: 1)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+CustomDataTests+ClassWithMemberDataAttributeBase.Passing(_: 2)", displayName)
			);
		}

		private class SingleMemberDataAttribute(
			string memberName,
			params object?[] parameters) :
				MemberDataAttributeBase(memberName, parameters)
		{
			protected override ITheoryDataRow ConvertDataRow(object dataRow) =>
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
			public void Passing(object _) { }
		}
	}

	partial class MemberDataTests
	{
		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask NonStaticData_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithNonStaticData));

			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData.FieldTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Could not find public static member (property, field, or method) named 'FieldDataSource' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData'", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData.MethodTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Could not find public static member (property, field, or method) named 'MethodDataSource' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData'", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData.PropertyTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Could not find public static member (property, field, or method) named 'PropertyDataSource' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithNonStaticData'", result.Messages.Single());
				}
			);
		}

#pragma warning disable xUnit1017 // MemberData must reference a static member
#pragma warning disable CA1822 // Mark members as static

		class ClassWithNonStaticData
		{
			public IEnumerable<object?[]>? FieldDataSource = null;

			public IEnumerable<object?[]>? MethodDataSource() => null;

			public IEnumerable<object?[]>? PropertyDataSource => null;

			[Theory]
			[MemberData(nameof(FieldDataSource))]
			public void FieldTestMethod(int _1, double _2, string _3) { }

			[Theory]
			[MemberData(nameof(MethodDataSource))]
			public void MethodTestMethod(int _1, double _2, string _3) { }

			[Theory]
			[MemberData(nameof(PropertyDataSource))]
			public void PropertyTestMethod(int _1, double _2, string _3) { }
		}

#pragma warning restore CA1822 // Mark members as static
#pragma warning restore xUnit1017 // MemberData must reference a static member

		// Native AOT reports these in the generator
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

				"- IEnumerable<Tuple<...>>" + Environment.NewLine +
				"- Task<IEnumerable<Tuple<...>>>" + Environment.NewLine +
				"- ValueTask<IEnumerable<Tuple<...>>>" + Environment.NewLine +

				"- IAsyncEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<ITheoryDataRow>>" + Environment.NewLine +

				"- IAsyncEnumerable<object[]>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<object[]>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<object[]>>" + Environment.NewLine +

				"- IAsyncEnumerable<Tuple<...>>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<Tuple<...>>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<Tuple<...>>>";

			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType.FieldTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Member 'IncompatibleField' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType'" + exceptionEpilogue, result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType.MethodTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Member 'IncompatibleMethod' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType'" + exceptionEpilogue, result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType.PropertyTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.Equal("Member 'IncompatibleProperty' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleReturnType'" + exceptionEpilogue, result.Messages.Single());
				}
			);
		}

#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type

		class ClassWithIncompatibleReturnType
		{
			public static int IncompatibleField = 42;

			public static int IncompatibleMethod() => 42;

			public static int IncompatibleProperty => 42;

			[Theory]
			[MemberData(nameof(IncompatibleField))]
			public void FieldTestMethod(int _) { }

			[Theory]
			[MemberData(nameof(IncompatibleMethod))]
			public void MethodTestMethod(int _) { }

			[Theory]
			[MemberData(nameof(IncompatibleProperty))]
			public void PropertyTestMethod(int _) { }
		}

#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type

		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask IncompatibleDataValueType_Throws()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithIncompatibleValueData));

			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData.FieldTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.StartsWith("Member 'IncompatibleFieldData' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData.MethodTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.StartsWith("Member 'IncompatibleMethodData' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
				},
				result =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData.PropertyTestMethod", result.Test.TestDisplayName);
					Assert.Equal("System.ArgumentException", result.ExceptionTypes.Single());
					Assert.StartsWith("Member 'IncompatiblePropertyData' on 'Xunit3TheoryAcceptanceTests+MemberDataTests+ClassWithIncompatibleValueData' yielded an item of type 'System.Int32' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'", result.Messages.Single());
				}
			);
		}

#pragma warning disable xUnit1019 // MemberData must reference a member providing a valid data type

		class ClassWithIncompatibleValueData
		{
			public static IEnumerable<int> IncompatibleFieldData = [42];

			public static IEnumerable<int> IncompatibleMethodData() => [42];

			public static IEnumerable<int> IncompatiblePropertyData => [42];

			[Theory]
			[MemberData(nameof(IncompatibleFieldData))]
			public void FieldTestMethod(int _) { }

			[Theory]
			[MemberData(nameof(IncompatibleMethodData))]
			public void MethodTestMethod(int _) { }

			[Theory]
			[MemberData(nameof(IncompatiblePropertyData))]
			public void PropertyTestMethod(int _) { }
		}

#pragma warning restore xUnit1019 // MemberData must reference a member providing a valid data type

#if NET8_0_OR_GREATER

		public abstract class Repro<TSelf> where TSelf : IInterfaceWithStaticVirtualMember
		{
			[Theory]
#pragma warning disable xUnit1015 // MemberData must reference an existing member
			[MemberData(nameof(TSelf.TestData))]
#pragma warning restore xUnit1015 // MemberData must reference an existing member
			public void Test(int value)
			{
				Assert.NotEqual(0, value);
			}
		}

		class ClassUnderTest_StaticInterfaceMethod : Repro<ClassUnderTest_StaticInterfaceMethod>, IInterfaceWithStaticVirtualMember
		{ }

		public interface IInterfaceWithStaticVirtualMember
		{
			static virtual TheoryData<int> TestData => [1, 2, 3];
		}

		// Native AOT does not support static interface members
		[Fact]
		public async ValueTask MemberData_ReferencingStaticInterfaceData_Succeeds()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_StaticInterfaceMethod));

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				result => Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_StaticInterfaceMethod.Test(value: 1)", result.Test.TestDisplayName),
				result => Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_StaticInterfaceMethod.Test(value: 2)", result.Test.TestDisplayName),
				result => Assert.Equal("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_StaticInterfaceMethod.Test(value: 3)", result.Test.TestDisplayName)
			);
		}

#endif  // NET8_0_OR_GREATER
	}

	partial class MethodDataTests
	{
		// TODO: Native AOT's generator triggers on the MemberData for the same reason that xUnit1015 currently triggers
		[Fact]
		public async ValueTask CanUseMethodDataInSubTypeFromTestInBaseType()
		{
			var testMessages = await RunForResultsAsync(typeof(SubClassWithTestData));

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithTestData.Test(x: 42)", passed.Test.TestDisplayName);
		}

		public abstract class BaseClassWithTestWithoutData
		{
			[Theory]
#pragma warning disable xUnit1015 // Remove this once https://github.com/xunit/xunit/issues/3501 is implemented
			[MemberData(nameof(SubClassWithTestData.TestData))]
#pragma warning restore xUnit1015
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

		// TODO: Non-ambiguous overloaded member data is not currently supported by Native AOT
		[Fact]
		public async Task MultipleMethodsButOnlyOneNonOptionalSupported()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithMultipleMethodsButOneWithoutOptionalParameters));

			Assert.Equal(2, testMessages.OfType<TestPassedWithMetadata>().Count());
			Assert.Empty(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
		}

		class ClassWithMultipleMethodsButOneWithoutOptionalParameters
		{
			public static IEnumerable<object[]> OverloadedDataMethod(string name, int scenarios = 444)
			{
				for (int i = 1; i <= scenarios; i++)
					yield return new object[] { name, i };
			}

			public static IEnumerable<object[]> OverloadedDataMethod(string name)
			{
				yield return new object[] { name, -1 };
			}

			[Theory]
			[MemberData(nameof(OverloadedDataMethod), "MyFirst")]
			[MemberData(nameof(OverloadedDataMethod), "MySecond")]
			public void TestMethod(string name, int scenario)
			{
				Assert.True(name.Length > 0);
				Assert.Equal(-1, scenario);
			}
		}

		// Native AOT reports these in the generator
		[Fact]
		public async Task MultipleMethodsWithAmbiguousOptionalParameters()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithMultipleMethodsAndAmbiguousOptionalParameters));

			Assert.Empty(testMessages.OfType<TestPassedWithMetadata>());
			var failed = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.StartsWith(
				"The call to method 'Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMultipleMethodsAndAmbiguousOptionalParameters.OverloadedDataMethod' is ambigous between 2 different options for the given arguments.",
				failed.Messages.Single()
			);
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
		}

		class ClassWithMultipleMethodsAndAmbiguousOptionalParameters
		{
			public static IEnumerable<object[]> OverloadedDataMethod(string name, int scenarios = 444)
			{
				for (int i = 1; i <= scenarios; i++)
					yield return new object[] { name, i };
			}

			public static IEnumerable<object[]> OverloadedDataMethod(string name, string scenariosAsString = "444")
			{
				yield return new object[] { name, int.Parse(scenariosAsString) };
			}

			[Theory]
			[MemberData(nameof(OverloadedDataMethod), "MyFirst")]
			[MemberData(nameof(OverloadedDataMethod), "MySecond")]
			public void TestMethod(string _1, int _2)
			{ }
		}

		// Native AOT does not support generic test methods
		[Fact]
		public async ValueTask GenericParameter_Func_Valid()
		{
			var results = await RunForResultsAsync(typeof(ClassWithFuncMethod));

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Double>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Int32>(_1: [4, 5, 6, 7], _2: ", displayName),
				displayName => Assert.StartsWith("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithFuncMethod.TestMethod<Single>(_1: [4, 5, 6, 7], _2: ", displayName)
			);
		}

		class ClassWithFuncMethod
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
			public void TestMethod<TResult>(IEnumerable<int> _1, Func<int, TResult> _2)
			{ }
		}
	}

	public class MissingDataTests : AcceptanceTestV3
	{
		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask MissingDataThrows()
		{
			var testMessages = await RunForResultsAsync(typeof(ClassWithMissingData));

			var failed = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData.TestViaMissingData", failed.Test.TestDisplayName);
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'Foo' on 'Xunit3TheoryAcceptanceTests+MissingDataTests+ClassWithMissingData'", failed.Messages.Single());
		}

		class ClassWithMissingData
		{
			[Theory]
#pragma warning disable xUnit1015 // MemberData must reference an existing member
			[MemberData("Foo")]
#pragma warning restore xUnit1015 // MemberData must reference an existing member
			public void TestViaMissingData(int _1, double _2, string _3) { }
		}
	}

	public class OverloadedMethodTests : AcceptanceTestV3
	{
		// Native AOT reports these in the generator
		[Fact]
		public async ValueTask TestMethodMessagesOnlySentOnce()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var methodStarting = Assert.Single(testMessages.OfType<ITestMethodStarting>());
			Assert.Equal("Theory", methodStarting.MethodName);
			var methodFinished = Assert.Single(testMessages.OfType<ITestMethodFinished>());
			Assert.Equal(methodStarting.TestMethodUniqueID, methodFinished.TestMethodUniqueID);
		}

#pragma warning disable xUnit1024 // Test methods cannot have overloads

		class ClassUnderTest
		{
			[Theory]
			[InlineData(42)]
			public void Theory(int _)
			{ }

			[Theory]
			[InlineData("42")]
			public void Theory(string _)
			{ }
		}

#pragma warning restore xUnit1024 // Test methods cannot have overloads
	}

	partial class TheoryTests
	{
		// Native AOT does not support params parameters for theory methods
		[Fact]
		public async ValueTask ParamsParameters_Valid()
		{
			var results = await RunForResultsAsync(typeof(ClassWithParamsParameters));

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase),
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

		class ClassWithParamsParameters
		{
			[Theory]
			[InlineData]
			public void OneParameter_NonePassed(params object?[] array)
			{
				Assert.Empty(array);
			}

			[Theory]
			[InlineData(null)]
			public void OneParameter_OnePassed_Null(params object?[] array)
			{
				Assert.Null(array);
			}

			[Theory]
			[InlineData(1)]
			public void OneParameter_OnePassed_NonArray(params object?[] array)
			{
				Assert.Equal([1], array);
			}

			[Theory]
			[InlineData([new object?[] { 1 }])]
			public void OneParameter_OnePassed_MatchingArray(params object?[] array)
			{
				Assert.Equal([1], array);
			}

			[Theory]
			[InlineData(new int[] { 1 })]
			public void OneParameter_OnePassed_NonMatchingArray(params object?[] array)
			{
				Assert.Equal([new int[] { 1 }], array);
			}

			[Theory]
			[InlineData(1, 2, 3, 4, 5, 6)]
			public void OneParameter_ManyPassed(params object?[] array)
			{
				Assert.Equal([1, 2, 3, 4, 5, 6], array);
			}

			[Theory]
			[InlineData(1)]
			public void TwoParameters_OnePassed(
				int i,
				params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Empty(array);
			}

			[Theory]
			[InlineData(1, null)]
			public void TwoParameters_NullPassed(
				int i,
				params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Null(array);
			}

			[Theory]
			[InlineData(1, 2)]
			public void TwoParameters_OnePassed_NonArray(
				int i,
				params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal([2], array);
			}

			[Theory]
			[InlineData(1, new object?[] { 2 })]
			public void TwoParameters_OnePassed_MatchingArray(
				int i,
				params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal([2], array);
			}

			[Theory]
			[InlineData(1, new int[] { 2 })]
			public void TwoParameters_OnePassed_NonMatchingArray(
				int i,
				params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal([new int[] { 2 }], array);
			}

			[Theory]
			[InlineData(1, 2, 3, 4, 5, 6)]
			public void TwoParameters_ManyPassed(
				int i,
				params object?[] array)
			{
				Assert.Equal(1, i);
				Assert.Equal([2, 3, 4, 5, 6], array);
			}

			[Theory]
			[InlineData]
			public void OptionalParameters_NonePassed(
				string s = "abc",
				int i = 1,
				params object?[] array)
			{
				Assert.Equal("abc", s);
				Assert.Equal(1, i);
				Assert.Empty(array);
			}

			[Theory]
			[InlineData("def", 2, 3, 4, 5)]
			public void OptionalParameters_ManyPassed(
				string s = "abc",
				int i = 1,
				params object?[] array)
			{
				Assert.Equal("def", s);
				Assert.Equal(2, i);
				Assert.Equal([3, 4, 5], array);
			}
		}

		// Native AOT does not support generic test methods
		[Fact]
		public async ValueTask GenericTheoryWithSerializableData()
		{
			var results = await RunForResultsAsync(typeof(GenericWithSerializableData), preEnumerateTheories: false);

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(p => p.Test.TestDisplayName).OrderBy(x => x),
				// Embedded (T1, Empty<T2>)
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Embedded<Int32, Int32>(_1: 1, _2: Empty<Int32>)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Embedded<Object, Int32>(_1: null, _2: Empty<Int32>)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Embedded<String, Int32>(_1: ""1"", _2: Empty<Int32>)", displayName),
				// Simple (T1, T2)
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<Int32, Object>(_1: 42, _2: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<Int32[], List<String>>(_1: [1, 2, 3], _2: [""a"", ""b"", ""c""])", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<Object, Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData+Empty<Int32>>(_1: null, _2: Empty<Int32>)", displayName),
				displayName => Assert.Equal($@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithSerializableData.GenericTest_Simple<String, Double>(_1: ""Hello, world!"", _2: {21.12:G17})", displayName)
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
			public void GenericTest_Embedded<T1, T2>(T1 _1, Empty<T2> _2) { }

			public struct Empty<T>
			{
				public override readonly string ToString() => $"Empty<{typeof(T).Name}>";
			}

			public static IEnumerable<object?[]> GenericData_Simple()
			{
				yield return new object?[] { 42, null };
				yield return new object?[] { "Hello, world!", 21.12 };
				yield return new object?[] { new int[] { 1, 2, 3 }, new List<string> { "a", "b", "c" } };
				yield return new object?[] { null, default(Empty<int>) };
			}

			[Theory, MemberData(nameof(GenericData_Simple))]
			public void GenericTest_Simple<T1, T2>(T1 _1, T2 _2) { }
		}

		// Native AOT does not support generic test methods
		[Fact]
		public async ValueTask GenericTheoryWithNonSerializableData()
		{
			var results = await RunForResultsAsync(typeof(GenericWithNonSerializableData));

			var displayName = Assert.Single(results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName));
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData.GenericTest<Xunit3TheoryAcceptanceTests+TheoryTests+GenericWithNonSerializableData>(_: GenericWithNonSerializableData { })", displayName);
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
			public void GenericTest<T>(T _) { }
		}

		// These conversions are only supported via reflection
		[Fact]
		public async ValueTask ImplicitExplicitConversions()
		{
			var results = await RunForResultsAsync(typeof(ClassWithOperatorConversions));

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredExplicitConversion(value: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredImplicitConversion(value: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.DecimalToInt(value: 43)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.IntToDecimal(value: 43)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.IntToLong(i: 1)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ParameterDeclaredExplicitConversion(e: Explicit { Value = ""abc"" })", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ParameterDeclaredImplicitConversion(i: Implicit { Value = ""abc"" })", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.UIntToULong(i: 1)", displayName)
			);
			Assert.Empty(results.OfType<TestFailedWithMetadata>());
			Assert.Empty(results.OfType<TestSkippedWithMetadata>());
		}
	}
}
