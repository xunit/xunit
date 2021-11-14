using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class TheoryDiscovererTests : AcceptanceTestV3
{
	readonly _ITestFrameworkDiscoveryOptions discoveryOptions = _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true);

	[Fact]
	public async void NoDataAttributes()
	{
		var failures = await RunAsync<_TestFailed>(typeof(NoDataAttributesClass));

		var failure = Assert.Single(failures);
		Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
		Assert.Equal($"No data found for {typeof(NoDataAttributesClass).FullName}.{nameof(NoDataAttributesClass.TheoryMethod)}", failure.Messages.Single());
	}

	class NoDataAttributesClass
	{
		[Theory]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public async void NullMemberData_ThrowsInvalidOperationException()
	{
		var results = await RunAsync<_TestFailed>(typeof(NullDataClass));

		var failure = Assert.Single(results);
		Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
		Assert.Equal($"Test data returned null for {typeof(NullDataClass).FullName}.{nameof(NullDataClass.NullMemberData)}. Make sure it is statically initialized before this test method is called.", failure.Messages.Single());
	}

	class NullDataClass
	{
		public static IEnumerable<object[]>? InitializedInConstructor;

		public NullDataClass()
		{
			InitializedInConstructor = new List<object[]>
			{
				new object[] { "1", "2" }
			};
		}

		[Theory]
		[MemberData(nameof(InitializedInConstructor))]
		public void NullMemberData(string str1, string str2) { }
	}

	[Fact]
	public async void EmptyTheoryData()
	{
		var failures = await RunAsync<_TestFailed>(typeof(EmptyTheoryDataClass));

		var failure = Assert.Single(failures);
		Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
		Assert.Equal($"No data found for {typeof(EmptyTheoryDataClass).FullName}.{nameof(EmptyTheoryDataClass.TheoryMethod)}", failure.Messages.Single());
	}

	class EmptyTheoryDataAttribute : DataAttribute
	{
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod) =>
			new(Array.Empty<ITheoryDataRow>());
	}

	class EmptyTheoryDataClass
	{
		[Theory, EmptyTheoryData]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public async void DiscoveryOptions_PreEnumerateTheoriesSetToTrue_YieldsTestCasePerDataRow()
	{
		discoveryOptions.SetPreEnumerateTheories(true);
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MultipleDataClass>(nameof(MultipleDataClass.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = (await discoverer.Discover(discoveryOptions, testMethod, factAttribute)).ToList();

		Assert.Collection(
			testCases.Select(tc => tc.TestCaseDisplayName).OrderBy(x => x),
			displayName => Assert.Equal($"{typeof(MultipleDataClass).FullName}.{nameof(MultipleDataClass.TheoryMethod)}(x: 2112)", displayName),
			displayName => Assert.Equal($"{typeof(MultipleDataClass).FullName}.{nameof(MultipleDataClass.TheoryMethod)}(x: 42)", displayName)
		);
	}

	[Fact]
	public async void DiscoveryOptions_PreEnumerateTheoriesSetToFalse_YieldsSingleTestCase()
	{
		discoveryOptions.SetPreEnumerateTheories(false);
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MultipleDataClass>(nameof(MultipleDataClass.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(MultipleDataClass).FullName}.{nameof(MultipleDataClass.TheoryMethod)}", testCase.TestCaseDisplayName);
	}

	class MultipleDataAttribute : DataAttribute
	{
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod) =>
			new(
				new[]
				{
					new TheoryDataRow(42),
					new TheoryDataRow(2112)
				}
			);
	}

	class MultipleDataClass
	{
		[Theory, MultipleDataAttribute]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public async void DiscoveryOptions_PreEnumerateTheoriesSetToTrueWithSkipOnData_YieldsSkippedTestCasePerDataRow()
	{
		discoveryOptions.SetPreEnumerateTheories(true);
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MultipleDataClassSkipped>(nameof(MultipleDataClassSkipped.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = (await discoverer.Discover(discoveryOptions, testMethod, factAttribute)).ToList();

		Assert.Collection(
			testCases.OrderBy(tc => tc.TestCaseDisplayName),
			testCase =>
			{
				Assert.Equal($"{typeof(MultipleDataClassSkipped).FullName}.{nameof(MultipleDataClassSkipped.TheoryMethod)}(x: 2112)", testCase.TestCaseDisplayName);
				Assert.Equal("Skip this attribute", testCase.SkipReason);
			},
			testCase =>
			{
				Assert.Equal($"{typeof(MultipleDataClassSkipped).FullName}.{nameof(MultipleDataClassSkipped.TheoryMethod)}(x: 42)", testCase.TestCaseDisplayName);
				Assert.Equal("Skip this attribute", testCase.SkipReason);
			}
		);
	}

	class MultipleDataClassSkipped
	{
		[Theory, MultipleData(Skip = "Skip this attribute")]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public async void ThrowingData()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ThrowingDataClass>("TheoryWithMisbehavingData");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(ThrowingDataClass).FullName}.{nameof(ThrowingDataClass.TheoryWithMisbehavingData)}", testCase.TestCaseDisplayName);
		var message = Assert.Single(discoverer.DiagnosticMessages);
		var diagnostic = Assert.IsAssignableFrom<_DiagnosticMessage>(message);
		Assert.StartsWith($"Exception thrown during theory discovery on '{typeof(ThrowingDataClass).FullName}.{nameof(ThrowingDataClass.TheoryWithMisbehavingData)}'; falling back to single test case.{Environment.NewLine}System.DivideByZeroException: Attempted to divide by zero.", diagnostic.Message);
	}

	class ThrowingDataAttribute : DataAttribute
	{
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo method)
		{
			throw new DivideByZeroException();
		}
	}

	class ThrowingDataClass
	{
		[Theory, ThrowingData]
		public void TheoryWithMisbehavingData(string a) { }
	}

	[Fact]
	public async void DataDiscovererReturningNullYieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var theoryAttribute = Mocks.TheoryAttribute();
		var dataAttribute = Mocks.DataAttribute();
		var testMethod = Mocks.TestMethod("MockTheoryType", "MockTheoryMethod", methodAttributes: new[] { theoryAttribute, dataAttribute });

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, theoryAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("MockTheoryType.MockTheoryMethod", testCase.TestCaseDisplayName);
	}

	[Fact]
	public async void NonSerializableDataYieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<NonSerializableDataClass>(nameof(NonSerializableDataClass.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(NonSerializableDataClass).FullName}.{nameof(NonSerializableDataClass.TheoryMethod)}", testCase.TestCaseDisplayName);
		var message = Assert.Single(discoverer.DiagnosticMessages);
		var diagnostic = Assert.IsAssignableFrom<_DiagnosticMessage>(message);
		Assert.Equal($"Non-serializable data (one or more of: '{typeof(NonSerializableDataAttribute).FullName}') found for '{typeof(NonSerializableDataClass).FullName}.{nameof(NonSerializableDataClass.TheoryMethod)}'; falling back to single test case.", diagnostic.Message);
	}

	class NonSerializableDataAttribute : DataAttribute
	{
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo method) =>
			new(
				new[]
				{
					new TheoryDataRow(42),
					new TheoryDataRow(new NonSerializableDataAttribute())
				}
			);
	}

	class NonSerializableDataClass
	{
		[Theory, NonSerializableData]
		public void TheoryMethod(object a) { }
	}

	[Fact]
	public async void NoSuchDataDiscoverer_ThrowsInvalidOperationException()
	{
		var results = await RunAsync<_TestFailed>(typeof(NoSuchDataDiscovererClass));

		var failure = Assert.Single(results);
		Assert.Equal(typeof(InvalidOperationException).FullName, failure.ExceptionTypes.Single());
		Assert.Equal($"Data discoverer specified for {typeof(NoSuchDataDiscovererAttribute).FullName} on {typeof(NoSuchDataDiscovererClass).FullName}.{nameof(NoSuchDataDiscovererClass.Test)} does not exist.", failure.Messages.Single());
	}

	class NoSuchDataDiscovererClass
	{
		[Theory]
		[NoSuchDataDiscoverer]
		public void Test() { }
	}

	[DataDiscoverer("Foo.Blah.ThingDiscoverer", "invalid_assembly_name")]
	public class NoSuchDataDiscovererAttribute : DataAttribute
	{
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod)
		{
			throw new NotImplementedException();
		}
	}

	[Fact]
	public async void NotADataDiscoverer_ThrowsInvalidOperationException()
	{
		var results = await RunAsync<_TestFailed>(typeof(NotADataDiscovererClass));

		var failure = Assert.Single(results);
		Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
		Assert.Equal($"Data discoverer specified for {typeof(NotADataDiscovererAttribute).FullName} on {typeof(NotADataDiscovererClass).FullName}.{nameof(NotADataDiscovererClass.Test)} does not implement IDataDiscoverer.", failure.Messages.Single());
	}

	class NotADataDiscovererClass
	{
		[Theory]
		[NotADataDiscoverer]
		public void Test() { }
	}

	[DataDiscoverer(typeof(TheoryDiscovererTests))]
	public class NotADataDiscovererAttribute : DataAttribute
	{
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod)
		{
			throw new NotImplementedException();
		}
	}

	[Fact]
	public async void DiscoveryDisabledOnTheoryAttribute_YieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<NonDiscoveryOnTheoryAttribute>(nameof(NonDiscoveryOnTheoryAttribute.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(NonDiscoveryOnTheoryAttribute).FullName}.{nameof(NonDiscoveryOnTheoryAttribute.TheoryMethod)}", testCase.TestCaseDisplayName);
	}

	class NonDiscoveryOnTheoryAttribute
	{
		public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
		public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(foo))]
		[MemberData(nameof(bar))]
		public static void TheoryMethod(int x) { }
	}

	[Fact]
	public async void DiscoveryDisabledOnMemberData_YieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<NonDiscoveryEnumeratedData>(nameof(NonDiscoveryEnumeratedData.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(NonDiscoveryEnumeratedData).FullName}.{nameof(NonDiscoveryEnumeratedData.TheoryMethod)}", testCase.TestCaseDisplayName);
	}

	class NonDiscoveryEnumeratedData
	{
		public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
		public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

		[Theory]
		[MemberData(nameof(foo), DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(bar), DisableDiscoveryEnumeration = true)]
		public static void TheoryMethod(int x) { }
	}

	[Fact]
	public async void MixedDiscoveryEnumerationOnMemberData_YieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MixedDiscoveryEnumeratedData>(nameof(MixedDiscoveryEnumeratedData.TheoryMethod));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(MixedDiscoveryEnumeratedData).FullName}.{nameof(MixedDiscoveryEnumeratedData.TheoryMethod)}", testCase.TestCaseDisplayName);
	}

	class MixedDiscoveryEnumeratedData
	{
		public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
		public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

		[Theory]
		[MemberData(nameof(foo), DisableDiscoveryEnumeration = false)]
		[MemberData(nameof(bar), DisableDiscoveryEnumeration = true)]
		public static void TheoryMethod(int x) { }
	}

	[Fact]
	public async void SkippedTheoryWithNoData()
	{
		var msgs = await RunAsync(typeof(SkippedWithNoData));

		var skip = Assert.Single(msgs.OfType<_TestSkipped>());
		var skipStarting = Assert.Single(msgs.OfType<_TestStarting>().Where(s => s.TestUniqueID == skip.TestUniqueID));
		Assert.Equal($"{typeof(SkippedWithNoData).FullName}.{nameof(SkippedWithNoData.TestMethod)}", skipStarting.TestDisplayName);
		Assert.Equal("I have no data", skip.Reason);
	}

	class SkippedWithNoData
	{
		[Theory(Skip = "I have no data")]
		public void TestMethod(int value) { }
	}

	[Fact]
	public async void SkippedTheoryWithData()
	{
		var msgs = await RunAsync(typeof(SkippedWithData));

		var skip = Assert.Single(msgs.OfType<_TestSkipped>());
		var skipStarting = Assert.Single(msgs.OfType<_TestStarting>().Where(s => s.TestUniqueID == skip.TestUniqueID));
		Assert.Equal($"{typeof(SkippedWithData).FullName}.{nameof(SkippedWithData.TestMethod)}", skipStarting.TestDisplayName);
		Assert.Equal("I have data", skip.Reason);
	}

	class SkippedWithData
	{
		[Theory(Skip = "I have data")]
		[InlineData(42)]
		[InlineData(2112)]
		public void TestMethod(int value) { }
	}

	[Fact]
	public async void TheoryWithSerializableInputDataThatIsntSerializableAfterConversion_YieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassWithExplicitConvertedData>(nameof(ClassWithExplicitConvertedData.ParameterDeclaredExplicitConversion));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal($"{typeof(ClassWithExplicitConvertedData).FullName}.{nameof(ClassWithExplicitConvertedData.ParameterDeclaredExplicitConversion)}", testCase.TestCaseDisplayName);
	}

	class ClassWithExplicitConvertedData
	{
		// Explicit conversion defined on the parameter's type
		[Theory]
		[InlineData("abc")]
		public void ParameterDeclaredExplicitConversion(Explicit e)
		{
			Assert.Equal("abc", e.Value);
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
	}

	class ClassWithSkippedTheoryDataRows
	{
		public static IEnumerable<ITheoryDataRow> Data =>
			new[]
			{
				new TheoryDataRow(42) { Skip = "Do not run this test" },
				new TheoryDataRow(2112),
			};

		[Theory]
		[MemberData(nameof(Data))]
		public void TestWithSomeSkippedTheoryRows(int x)
		{
			Assert.Equal(96, x);
		}
	}

	[Fact]
	public async void CanSkipFromTheoryDataRow_Preenumerated()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassWithSkippedTheoryDataRows>(nameof(ClassWithSkippedTheoryDataRows.TestWithSomeSkippedTheoryRows));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		Assert.Collection(
			testCases.OrderBy(tc => tc.TestCaseDisplayName),
			testCase =>
			{
				Assert.IsType<XunitPreEnumeratedTheoryTestCase>(testCase);
				Assert.Equal($"{typeof(ClassWithSkippedTheoryDataRows).FullName}.{nameof(ClassWithSkippedTheoryDataRows.TestWithSomeSkippedTheoryRows)}(x: 2112)", testCase.TestCaseDisplayName);
				Assert.Null(testCase.SkipReason);
			},
			testCase =>
			{
				Assert.IsType<XunitSkippedDataRowTestCase>(testCase);
				Assert.Equal($"{typeof(ClassWithSkippedTheoryDataRows).FullName}.{nameof(ClassWithSkippedTheoryDataRows.TestWithSomeSkippedTheoryRows)}(x: 42)", testCase.TestCaseDisplayName);
				Assert.Equal("Do not run this test", testCase.SkipReason);
			}
		);
	}

	[Trait("Class", "ClassWithTraitsOnTheoryDataRows")]
	class ClassWithTraitsOnTheoryDataRows
	{
		public static IEnumerable<ITheoryDataRow> Data =>
			new[]
			{
				new TheoryDataRow(42).WithTrait("Number", "42"),
				new TheoryDataRow(2112) { Skip = "I am skipped" }.WithTrait("Number", "2112"),
			};

		[Theory]
		[Trait("Theory", "TestWithTraits")]
		[MemberData(nameof(Data))]
		public void TestWithTraits(int x)
		{
			Assert.Equal(96, x);
		}
	}

	[Fact]
	public async void CanAddTraitsFromTheoryDataRow_Preenumerated()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassWithTraitsOnTheoryDataRows>(nameof(ClassWithTraitsOnTheoryDataRows.TestWithTraits));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		Assert.Collection(
			testCases.OrderBy(tc => tc.TestCaseDisplayName),
			testCase =>
			{
				Assert.IsType<XunitSkippedDataRowTestCase>(testCase);
				Assert.Equal($"{typeof(ClassWithTraitsOnTheoryDataRows).FullName}.{nameof(ClassWithTraitsOnTheoryDataRows.TestWithTraits)}(x: 2112)", testCase.TestCaseDisplayName);
				Assert.Collection(
					testCase.Traits.OrderBy(kvp => kvp.Key),
					kvp =>  // Assembly level trait
					{
						Assert.Equal("Assembly", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("Trait", traitValue);
					},
					kvp =>  // Class level trait
					{
						Assert.Equal("Class", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("ClassWithTraitsOnTheoryDataRows", traitValue);
					},
					kvp =>  // Row level trait
					{
						Assert.Equal("Number", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("2112", traitValue);
					},
					kvp =>  // Theory level trait
					{
						Assert.Equal("Theory", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("TestWithTraits", traitValue);
					}
				);
			},
			testCase =>
			{
				Assert.IsType<XunitPreEnumeratedTheoryTestCase>(testCase);
				Assert.Equal($"{typeof(ClassWithTraitsOnTheoryDataRows).FullName}.{nameof(ClassWithTraitsOnTheoryDataRows.TestWithTraits)}(x: 42)", testCase.TestCaseDisplayName);
				Assert.Collection(
					testCase.Traits.OrderBy(kvp => kvp.Key),
					kvp =>  // Assembly level trait
					{
						Assert.Equal("Assembly", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("Trait", traitValue);
					},
					kvp =>  // Class level trait
					{
						Assert.Equal("Class", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("ClassWithTraitsOnTheoryDataRows", traitValue);
					},
					kvp =>  // Row level trait
					{
						Assert.Equal("Number", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("42", traitValue);
					},
					kvp =>  // Theory level trait
					{
						Assert.Equal("Theory", kvp.Key);
						var traitValue = Assert.Single(kvp.Value);
						Assert.Equal("TestWithTraits", traitValue);
					}
				);
			}
		);
	}

	class ClassWithDisplayNameOnTheoryDataRows
	{
		public static IEnumerable<ITheoryDataRow> Data =>
			new[]
			{
				new TheoryDataRow(42) { TestDisplayName = "I am a special test" },
				new TheoryDataRow(2112),
				new TheoryDataRow(2600) { Skip = "I am skipped", TestDisplayName = "I am a skipped test" }
			};

		[Theory]
		[MemberData(nameof(Data))]
		public void TestWithDisplayName(int x)
		{
			Assert.Equal(96, x);
		}
	}

	[Fact]
	public async void CanSetDisplayNameFromTheoryDataRow_Preenumerated()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassWithDisplayNameOnTheoryDataRows>(nameof(ClassWithDisplayNameOnTheoryDataRows.TestWithDisplayName));
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = await discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		Assert.Collection(
			testCases.OrderBy(tc => tc.TestCaseDisplayName),
			testCase =>
			{
				Assert.IsType<XunitSkippedDataRowTestCase>(testCase);
				Assert.Equal("I am a skipped test(x: 2600)", testCase.TestCaseDisplayName);
			},
			testCase =>
			{
				Assert.IsType<XunitPreEnumeratedTheoryTestCase>(testCase);
				Assert.Equal("I am a special test(x: 42)", testCase.TestCaseDisplayName);
			},
			testCase =>
			{
				Assert.IsType<XunitPreEnumeratedTheoryTestCase>(testCase);
				Assert.Equal($"{typeof(ClassWithDisplayNameOnTheoryDataRows).FullName}.{nameof(ClassWithDisplayNameOnTheoryDataRows.TestWithDisplayName)}(x: 2112)", testCase.TestCaseDisplayName);
			}
		);
	}

	class TestableTheoryDiscoverer : TheoryDiscoverer
	{
		public List<_MessageSinkMessage> DiagnosticMessages;

		public TestableTheoryDiscoverer(List<_MessageSinkMessage> diagnosticMessages)
			: base(SpyMessageSink.Create(messages: diagnosticMessages))
		{
			DiagnosticMessages = diagnosticMessages;
		}

		public static TestableTheoryDiscoverer Create()
		{
			return new TestableTheoryDiscoverer(new List<_MessageSinkMessage>());
		}
	}
}
