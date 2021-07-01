using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		Assert.Equal("No data found for TheoryDiscovererTests+NoDataAttributesClass.TheoryMethod", failure.Messages.Single());
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
		Assert.Equal("Test data returned null for TheoryDiscovererTests+NullDataClass.NullMemberData. Make sure it is statically initialized before this test method is called.", failure.Messages.Single());
	}

	class NullDataClass
	{
		public static IEnumerable<object[]>? InitializedInConstructor;

		public NullDataClass()
		{
			InitializedInConstructor = new List<object[]>
			{
				new object[] { "1", "2"}
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
		Assert.Equal("No data found for TheoryDiscovererTests+EmptyTheoryDataClass.TheoryMethod", failure.Messages.Single());
	}

	class EmptyTheoryDataAttribute : DataAttribute
	{
		public override IReadOnlyCollection<object[]> GetData(MethodInfo testMethod) =>
			Array.Empty<object[]>();
	}

	class EmptyTheoryDataClass
	{
		[Theory, EmptyTheoryData]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public void DiscoveryOptions_PreEnumerateTheoriesSetToTrue_YieldsTestCasePerDataRow()
	{
		discoveryOptions.SetPreEnumerateTheories(true);
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MultipleDataClass>("TheoryMethod");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute).ToList();

		Assert.Equal(2, testCases.Count);
		Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClass.TheoryMethod(x: 42)");
		Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClass.TheoryMethod(x: 2112)");
	}

	[Fact]
	public void DiscoveryOptions_PreEnumerateTheoriesSetToFalse_YieldsSingleTestCase()
	{
		discoveryOptions.SetPreEnumerateTheories(false);
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MultipleDataClass>("TheoryMethod");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("TheoryDiscovererTests+MultipleDataClass.TheoryMethod", testCase.DisplayName);
	}

	class MultipleDataAttribute : DataAttribute
	{
		public override IReadOnlyCollection<object[]> GetData(MethodInfo testMethod) =>
			new[]
			{
				new object[] { 42 },
				new object[] { 2112 }
			};
	}

	class MultipleDataClass
	{
		[Theory, MultipleDataAttribute]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public void DiscoverOptions_PreEnumerateTheoriesSetToTrueWithSkipOnData_YieldsSkippedTestCasePerDataRow()
	{
		discoveryOptions.SetPreEnumerateTheories(true);
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MultipleDataClassSkipped>("TheoryMethod");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute).ToList();

		Assert.Equal(2, testCases.Count);
		Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClassSkipped.TheoryMethod(x: 42)" && testCase.SkipReason == "Skip this attribute");
		Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClassSkipped.TheoryMethod(x: 2112)" && testCase.SkipReason == "Skip this attribute");

	}

	class MultipleDataClassSkipped
	{
		[Theory, MultipleDataAttribute(Skip = "Skip this attribute")]
		public void TheoryMethod(int x) { }
	}

	[Fact]
	public void ThrowingData()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ThrowingDataClass>("TheoryWithMisbehavingData");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("TheoryDiscovererTests+ThrowingDataClass.TheoryWithMisbehavingData", testCase.DisplayName);
		var message = Assert.Single(discoverer.DiagnosticMessages);
		var diagnostic = Assert.IsAssignableFrom<_DiagnosticMessage>(message);
		Assert.StartsWith($"Exception thrown during theory discovery on 'TheoryDiscovererTests+ThrowingDataClass.TheoryWithMisbehavingData'; falling back to single test case.{Environment.NewLine}System.DivideByZeroException: Attempted to divide by zero.", diagnostic.Message);
	}

	class ThrowingDataAttribute : DataAttribute
	{
		public override IReadOnlyCollection<object[]> GetData(MethodInfo method)
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
	public void DataDiscovererReturningNullYieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var theoryAttribute = Mocks.TheoryAttribute();
		var dataAttribute = Mocks.DataAttribute();
		var testMethod = Mocks.TestMethod("MockTheoryType", "MockTheoryMethod", methodAttributes: new[] { theoryAttribute, dataAttribute });

		var testCases = discoverer.Discover(discoveryOptions, testMethod, theoryAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("MockTheoryType.MockTheoryMethod", testCase.DisplayName);
	}

	[Fact]
	public void NonSerializableDataYieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<NonSerializableDataClass>("TheoryMethod");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("TheoryDiscovererTests+NonSerializableDataClass.TheoryMethod", testCase.DisplayName);
		var message = Assert.Single(discoverer.DiagnosticMessages);
		var diagnostic = Assert.IsAssignableFrom<_DiagnosticMessage>(message);
		Assert.Equal("Non-serializable data (one or more of: 'TheoryDiscovererTests+NonSerializableDataAttribute') found for 'TheoryDiscovererTests+NonSerializableDataClass.TheoryMethod'; falling back to single test case.", diagnostic.Message);
	}

	class NonSerializableDataAttribute : DataAttribute
	{
		public override IReadOnlyCollection<object[]> GetData(MethodInfo method) =>
			new[]
			{
				new object[] { 42 },
				new object[] { new NonSerializableDataAttribute() }
			};
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
		Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
		Assert.Equal("Data discoverer specified for TheoryDiscovererTests+NoSuchDataDiscovererAttribute on TheoryDiscovererTests+NoSuchDataDiscovererClass.Test does not exist.", failure.Messages.Single());
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
		public override IReadOnlyCollection<object[]> GetData(MethodInfo testMethod)
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
		Assert.Equal("Data discoverer specified for TheoryDiscovererTests+NotADataDiscovererAttribute on TheoryDiscovererTests+NotADataDiscovererClass.Test does not implement IDataDiscoverer.", failure.Messages.Single());
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
		public override IReadOnlyCollection<object[]> GetData(MethodInfo testMethod)
		{
			throw new NotImplementedException();
		}
	}

	[Fact]
	public void NonDiscoveryEnumeratedDataYieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<NonDiscoveryEnumeratedData>("TheoryMethod");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("TheoryDiscovererTests+NonDiscoveryEnumeratedData.TheoryMethod", testCase.DisplayName);
	}

	class NonDiscoveryEnumeratedData
	{
		public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
		public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

		[Theory]
		[MemberData("foo", DisableDiscoveryEnumeration = true)]
		[MemberData("bar", DisableDiscoveryEnumeration = true)]
		public static void TheoryMethod(int x) { }
	}

	[Fact]
	public void MixedDiscoveryEnumerationDataYieldSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<MixedDiscoveryEnumeratedData>("TheoryMethod");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("TheoryDiscovererTests+MixedDiscoveryEnumeratedData.TheoryMethod", testCase.DisplayName);
	}

	class MixedDiscoveryEnumeratedData
	{
		public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
		public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

		[Theory]
		[MemberData("foo", DisableDiscoveryEnumeration = false)]
		[MemberData("bar", DisableDiscoveryEnumeration = true)]
		public static void TheoryMethod(int x) { }
	}

	[Fact]
	public async void SkippedTheoryWithNoData()
	{
		var msgs = await RunAsync(typeof(SkippedWithNoData));

		var skip = Assert.Single(msgs.OfType<_TestSkipped>());
		var skipStarting = Assert.Single(msgs.OfType<_TestStarting>().Where(s => s.TestUniqueID == skip.TestUniqueID));
		Assert.Equal("TheoryDiscovererTests+SkippedWithNoData.TestMethod", skipStarting.TestDisplayName);
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
		Assert.Equal("TheoryDiscovererTests+SkippedWithData.TestMethod", skipStarting.TestDisplayName);
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
	public void TheoryWithSerializableInputDataThatIsntSerializableAfterConversion_YieldsSingleTheoryTestCase()
	{
		var discoverer = TestableTheoryDiscoverer.Create();
		var testMethod = Mocks.TestMethod<ClassWithExplicitConvertedData>("ParameterDeclaredExplicitConversion");
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

		var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

		var testCase = Assert.Single(testCases);
		Assert.IsType<XunitDelayEnumeratedTheoryTestCase>(testCase);
		Assert.Equal("TheoryDiscovererTests+ClassWithExplicitConvertedData.ParameterDeclaredExplicitConversion", testCase.DisplayName);
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
