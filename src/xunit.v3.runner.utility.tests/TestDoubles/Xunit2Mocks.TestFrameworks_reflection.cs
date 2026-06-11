#pragma warning disable xUnit3000 // These derive from the "wrong" version (v3 instead of v2) of LLMBRO, which is acceptable

using Xunit.Abstractions;
using LongLivedMarshalByRefObject = Xunit.Sdk.LongLivedMarshalByRefObject;

namespace Xunit.Runner.v2;

partial class Xunit2Mocks
{
	public static ITestFrameworkDiscoverer TestFrameworkDiscoverer(params (ITestCase testCase, string serialization)[] serializations) =>
		new MockTestFrameworkDiscoverer(serializations);

	sealed class MockTestFrameworkDiscoverer((ITestCase testCase, string serialization)[] serializations) :
		LongLivedMarshalByRefObject, ITestFrameworkDiscoverer
	{
		public string TargetFramework => "mock-target-framework";
		public string TestFrameworkDisplayName => "mock-test-framework";

		public void Dispose() { }

		public void Find(
			bool includeSourceInformation,
			IMessageSink discoveryMessageSink,
			ITestFrameworkDiscoveryOptions discoveryOptions) =>
				throw new NotImplementedException();

		public void Find(
			string typeName,
			bool includeSourceInformation,
			IMessageSink discoveryMessageSink,
			ITestFrameworkDiscoveryOptions discoveryOptions) =>
				throw new NotImplementedException();

		public string Serialize(ITestCase testCase)
		{
			var match = serializations.FirstOrDefault(s => s.testCase == testCase);
			return match.serialization ?? throw new InvalidOperationException("Could not find requested test case");
		}
	}
}
