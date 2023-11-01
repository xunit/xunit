using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
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

			await Assert.ThrowsAsync<ArgumentNullException>("callback", () => framework.Find(callback: null!, discoveryOptions: _TestFrameworkOptions.ForDiscovery()).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("discoveryOptions", () => framework.Find(callback: _ => new(true), discoveryOptions: null!).AsTask());
		}

		[Fact]
		public async ValueTask ExceptionDuringFindTestsForType_ReportsExceptionAsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;

			var mockType = Mocks.TypeInfo("MockType");
			var discoverer = TestableTestFrameworkDiscoverer.Create(mockType);
			discoverer.FindTestsForType_Exception = new DivideByZeroException();

			await discoverer.Find();

			var message = Assert.Single(spy.Messages.OfType<_DiagnosticMessage>());
			Assert.StartsWith($"Exception during discovery:{Environment.NewLine}System.DivideByZeroException: Attempted to divide by zero.", message.Message);
		}

		[Fact]
		public async ValueTask TestContextVisibility()
		{
			var mockType = Mocks.TypeInfo("MockType");
			var discoverer = TestableTestFrameworkDiscoverer.Create(mockType);

			await discoverer.Find();

			var context = discoverer.FindTestsForType_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Discovering, context.TestAssemblyStatus);
			Assert.Equal(TestPipelineStage.Discovery, context.PipelineStage);
		}

		public class ByAssembly
		{
			[Fact]
			public static async ValueTask NoTypes()
			{
				var discoverer = TestableTestFrameworkDiscoverer.Create();

				await discoverer.Find();

				Assert.Empty(discoverer.FindTestsForType_TestClasses);
			}

			[Fact]
			public static async ValueTask RequestsPublicTypesFromAssembly()
			{
				var framework = TestableTestFrameworkDiscoverer.Create();

				await framework.Find();

				framework.AssemblyInfo.Received(1).GetTypes(includePrivateTypes: false);
			}

			[Fact]
			public static async ValueTask IncludesNonAbstractTypes()
			{
				var objectTypeInfo = Reflector.Wrap(typeof(object));
				var intTypeInfo = Reflector.Wrap(typeof(int));
				var discoverer = TestableTestFrameworkDiscoverer.Create(objectTypeInfo, intTypeInfo);

				await discoverer.Find();

				Assert.Collection(
					discoverer.FindTestsForType_TestClasses.Select(c => c.Class.Name).OrderBy(x => x),
					typeName => Assert.Equal(typeof(int).FullName, typeName),    // System.Int32
					typeName => Assert.Equal(typeof(object).FullName, typeName)  // System.Object
				);
			}

			[Fact]
			public static async ValueTask ExcludesAbstractTypes()
			{
				var abstractClassTypeInfo = Reflector.Wrap(typeof(AbstractClass));
				var discoverer = TestableTestFrameworkDiscoverer.Create(abstractClassTypeInfo);

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

				await discoverer.Find(types: new[] { typeof(object) });

				var testClass = Assert.Single(discoverer.FindTestsForType_TestClasses);
				Assert.Equal(typeof(object).FullName, testClass.Class.Name);
			}

			[Fact]
			public static async ValueTask ExcludesAbstractTypes()
			{
				var discoverer = TestableTestFrameworkDiscoverer.Create();

				await discoverer.Find(types: new[] { typeof(AbstractClass) });

				Assert.Empty(discoverer.FindTestsForType_TestClasses);
			}
		}

		public class WithCulture
		{
			readonly _ITypeInfo mockType = Mocks.TypeInfo("MockType");

			[Fact]
			public async ValueTask DefaultCultureIsCurrentCulture()
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				var discoverer = TestableTestFrameworkDiscoverer.Create(mockType);
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: null);

				await discoverer.Find(discoveryOptions);

				Assert.NotNull(discoverer.FindTestsForType_CurrentCulture);
				Assert.Equal("en-US", discoverer.FindTestsForType_CurrentCulture.Name);
			}

			[Fact]
			public async ValueTask EmptyStringIsInvariantCulture()
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				var discoverer = TestableTestFrameworkDiscoverer.Create(mockType);
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: string.Empty);

				await discoverer.Find(discoveryOptions);

				Assert.NotNull(discoverer.FindTestsForType_CurrentCulture);
				Assert.Equal(string.Empty, discoverer.FindTestsForType_CurrentCulture.Name);
			}

			[Fact]
			public async ValueTask CustomCultureViaDiscoveryOptions()
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				var discoverer = TestableTestFrameworkDiscoverer.Create(mockType);
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: "en-GB");

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

	class TestableTestFrameworkDiscoverer : TestFrameworkDiscoverer<_ITestCase>
	{
		public TestContext? FindTestsForType_Context;
		public CultureInfo? FindTestsForType_CurrentCulture;
		public Exception? FindTestsForType_Exception = null;
		public readonly List<_ITestClass> FindTestsForType_TestClasses = new();

		TestableTestFrameworkDiscoverer(_IAssemblyInfo assemblyInfo) :
			base(assemblyInfo)
		{
			TestAssembly = Mocks.TestAssembly(assemblyInfo.AssemblyPath, uniqueID: "asm-id");
		}

		public new _IAssemblyInfo AssemblyInfo => base.AssemblyInfo;

		public override _ITestAssembly TestAssembly { get; }

		public override string TestFrameworkDisplayName => "testable-test-framework";

		protected override ValueTask<_ITestClass> CreateTestClass(_ITypeInfo @class) =>
			new(Mocks.TestClass(@class));

		public ValueTask Find(
			_ITestFrameworkDiscoveryOptions? discoveryOptions = null,
			Type[]? types = null) =>
				Find(
					testCase => new(true),
					discoveryOptions ?? _TestFrameworkOptions.ForDiscovery(),
					types
				);

		protected override ValueTask<bool> FindTestsForType(
			_ITestClass testClass,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			Func<_ITestCase, ValueTask<bool>> discoveryCallback)
		{
			FindTestsForType_Context = TestContext.Current;
			FindTestsForType_CurrentCulture = CultureInfo.CurrentCulture;
			FindTestsForType_TestClasses.Add(testClass);

			if (FindTestsForType_Exception is not null)
				throw FindTestsForType_Exception;

			return new(true);
		}

		public static TestableTestFrameworkDiscoverer Create(params _ITypeInfo[] types) =>
			new(Mocks.AssemblyInfo(types));
	}
}
