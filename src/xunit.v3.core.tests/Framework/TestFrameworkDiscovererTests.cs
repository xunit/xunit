using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestFrameworkDiscovererTests
{
	public class Find
	{
		[Fact]
		public static async ValueTask GuardClauses()
		{
			var framework = TestableTestFrameworkDiscoverer.Create();

			await Assert.ThrowsAsync<ArgumentNullException>("callback", () => framework.Find(callback: null!, discoveryOptions: TestData.TestFrameworkDiscoveryOptions()).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("discoveryOptions", () => framework.Find(callback: _ => new(true), discoveryOptions: null!).AsTask());
		}

		[Fact]
		public async ValueTask ExceptionDuringFindTestsForType_ReportsExceptionAsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.CurrentInternal.DiagnosticMessageSink = spy;

			var discoverer = TestableTestFrameworkDiscoverer.Create();
			discoverer.FindTestsForType_Exception = new DivideByZeroException();

			await discoverer.Find();

			var message = Assert.Single(spy.Messages.OfType<IDiagnosticMessage>());
			Assert.StartsWith($"Exception during discovery:{Environment.NewLine}System.DivideByZeroException: Attempted to divide by zero.", message.Message);
		}

		[Fact]
		public async ValueTask TestContextVisibility()
		{
			var discoverer = TestableTestFrameworkDiscoverer.Create();

			await discoverer.Find();

			var context = discoverer.FindTestsForType_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Discovering, context.TestAssemblyStatus);
			Assert.Equal(TestPipelineStage.Discovery, context.PipelineStage);
		}

		public class ByAssembly
		{
			[Fact]
			public static async ValueTask IncludesNonAbstractTypes()
			{
				var discoverer = TestableTestFrameworkDiscoverer.Create(typeof(object), typeof(int));

				await discoverer.Find();

				Assert.Collection(
					discoverer.FindTestsForType_TestClasses.Select(c => c.TestClassName).OrderBy(x => x),
					typeName => Assert.Equal(typeof(int).FullName, typeName),    // System.Int32
					typeName => Assert.Equal(typeof(object).FullName, typeName)  // System.Object
				);
			}

			[Fact]
			public static async ValueTask ExcludesAbstractTypes()
			{
				var discoverer = TestableTestFrameworkDiscoverer.Create(typeof(AbstractClass));

				await discoverer.Find();

				Assert.Empty(discoverer.FindTestsForType_TestClasses);
			}
		}

		public class ByTypes
		{
			[Fact]
			public static async ValueTask IncludesNonAbstractTypes()
			{
				var discoverer = TestableTestFrameworkDiscoverer.Create();

				await discoverer.Find(types: [typeof(object)]);

				var testClass = Assert.Single(discoverer.FindTestsForType_TestClasses);
				Assert.Equal(typeof(object).FullName, testClass.TestClassName);
			}

			[Fact]
			public static async ValueTask ExcludesAbstractTypes()
			{
				var discoverer = TestableTestFrameworkDiscoverer.Create();

				await discoverer.Find(types: [typeof(AbstractClass)]);

				Assert.Empty(discoverer.FindTestsForType_TestClasses);
			}
		}

		public class WithCulture
		{
			[Fact]
			public async ValueTask DefaultCultureIsCurrentCulture()
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				var discoverer = TestableTestFrameworkDiscoverer.Create();
				var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(culture: null);

				await discoverer.Find(discoveryOptions);

				Assert.NotNull(discoverer.FindTestsForType_CurrentCulture);
				Assert.Equal("en-US", discoverer.FindTestsForType_CurrentCulture.Name);
			}

			[Fact]
			public async ValueTask EmptyStringIsInvariantCulture()
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				var discoverer = TestableTestFrameworkDiscoverer.Create();
				var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(culture: string.Empty);

				await discoverer.Find(discoveryOptions);

				Assert.NotNull(discoverer.FindTestsForType_CurrentCulture);
				Assert.Equal(string.Empty, discoverer.FindTestsForType_CurrentCulture.Name);
			}

			[Fact]
			public async ValueTask CustomCultureViaDiscoveryOptions()
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				var discoverer = TestableTestFrameworkDiscoverer.Create();
				var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(culture: "en-GB");

				await discoverer.Find(discoveryOptions);

				Assert.NotNull(discoverer.FindTestsForType_CurrentCulture);
				Assert.Equal("en-GB", discoverer.FindTestsForType_CurrentCulture.Name);
			}
		}

		abstract class AbstractClass
		{
			[Fact]
			public static void ATestNotToBeRun() { }
		}
	}

	class TestableTestFrameworkDiscoverer : TestFrameworkDiscoverer<ITestCase, ITestClass>
	{
		readonly Type[] exportedTypes;
		public ITestContext? FindTestsForType_Context;
		public CultureInfo? FindTestsForType_CurrentCulture;
		public Exception? FindTestsForType_Exception = null;
		public readonly List<ITestClass> FindTestsForType_TestClasses = [];

		TestableTestFrameworkDiscoverer(
			ITestAssembly testAssembly,
			Type[] exportedTypes) :
				base(testAssembly)
		{
			// Make sure we always have at least one type or the machinery won't spin
			this.exportedTypes = exportedTypes.Length != 0 ? exportedTypes : [typeof(object)];
		}

		protected override ValueTask<ITestClass> CreateTestClass(Type @class) =>
			new(TestData.XunitTestClass(@class));

		public ValueTask Find(
			ITestFrameworkDiscoveryOptions? discoveryOptions = null,
			Type[]? types = null) =>
				Find(
					testCase => new(true),
					discoveryOptions ?? TestData.TestFrameworkDiscoveryOptions(),
					types
				);

		protected override ValueTask<bool> FindTestsForType(
			ITestClass testClass,
			ITestFrameworkDiscoveryOptions discoveryOptions,
			Func<ITestCase, ValueTask<bool>> discoveryCallback)
		{
			FindTestsForType_Context = TestContext.Current;
			FindTestsForType_CurrentCulture = CultureInfo.CurrentCulture;
			FindTestsForType_TestClasses.Add(testClass);

			if (FindTestsForType_Exception is not null)
				throw FindTestsForType_Exception;

			return new(true);
		}

		protected override Type[] GetExportedTypes() => exportedTypes;

		public static TestableTestFrameworkDiscoverer Create(params Type[] exportedTypes) =>
			new(Mocks.XunitTestAssembly(), exportedTypes);
	}
}
