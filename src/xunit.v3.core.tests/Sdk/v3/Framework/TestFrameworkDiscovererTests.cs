using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class TestFrameworkDiscovererTests
{
	public class Find_Assembly
	{
		[Fact]
		public void DefaultCulture()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			var discoverer = TestableTestFrameworkDiscoverer.Create();
			var discoverySink = new TestDiscoverySink();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: null);

			discoverer.Find(discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			Assert.NotNull(discoverer.DiscoveryCulture);
			Assert.Same(currentCulture, discoverer.DiscoveryCulture);
		}

		[Fact]
		public void InvariantCulture()
		{
			var discoverer = TestableTestFrameworkDiscoverer.Create();
			var discoverySink = new TestDiscoverySink();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: string.Empty);

			discoverer.Find(discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			Assert.NotNull(discoverer.DiscoveryCulture);
			Assert.Equal(string.Empty, discoverer.DiscoveryCulture.Name);
		}

		[Fact]
		public void CustomCulture()
		{
			var discoverer = TestableTestFrameworkDiscoverer.Create();
			var discoverySink = new TestDiscoverySink();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: "en-GB");

			discoverer.Find(discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			Assert.NotNull(discoverer.DiscoveryCulture);
			Assert.Equal("English (United Kingdom)", discoverer.DiscoveryCulture.DisplayName);
		}
	}

	public class Find_Type
	{
		[Fact]
		public void DefaultCulture()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			var discoverer = TestableTestFrameworkDiscoverer.Create();
			var discoverySink = new TestDiscoverySink();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: null);

			discoverer.Find("MockType", discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			Assert.NotNull(discoverer.DiscoveryCulture);
			Assert.Same(currentCulture, discoverer.DiscoveryCulture);
		}

		[Fact]
		public void InvariantCulture()
		{
			var discoverer = TestableTestFrameworkDiscoverer.Create();
			var discoverySink = new TestDiscoverySink();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: string.Empty);

			discoverer.Find("MockType", discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			Assert.NotNull(discoverer.DiscoveryCulture);
			Assert.Equal(string.Empty, discoverer.DiscoveryCulture.Name);
		}

		[Fact]
		public void CustomCulture()
		{
			var discoverer = TestableTestFrameworkDiscoverer.Create();
			var discoverySink = new TestDiscoverySink();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(culture: "en-GB");

			discoverer.Find("MockType", discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			Assert.NotNull(discoverer.DiscoveryCulture);
			Assert.Equal("English (United Kingdom)", discoverer.DiscoveryCulture.DisplayName);
		}
	}

	class TestableTestFrameworkDiscoverer : TestFrameworkDiscoverer
	{
		public CultureInfo? DiscoveryCulture;

		TestableTestFrameworkDiscoverer(
			_IAssemblyInfo assemblyInfo,
			string? configFileName,
			_ISourceInformationProvider sourceProvider,
			_IMessageSink diagnosticMessageSink) :
				base(assemblyInfo, configFileName, sourceProvider, diagnosticMessageSink)
		{ }

		public override string TestAssemblyUniqueID => "asm-id";

		public override string TestFrameworkDisplayName => "testable-test-framework";

		protected override ValueTask<_ITestClass> CreateTestClass(_ITypeInfo @class) =>
			new ValueTask<_ITestClass>((_ITestClass)null!);

		protected override ValueTask<bool> FindTestsForType(
			_ITestClass testClass,
			IMessageBus messageBus,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			DiscoveryCulture = CultureInfo.CurrentCulture;

			return new ValueTask<bool>(true);
		}

		public static TestableTestFrameworkDiscoverer Create()
		{
			var mockType = Mocks.TypeInfo("MockType");
			var mockAssembly = Mocks.AssemblyInfo(new[] { mockType });

			return new TestableTestFrameworkDiscoverer(
				mockAssembly,
				null,
				_NullSourceInformationProvider.Instance,
				SpyMessageSink.Create()
			);
		}
	}
}
