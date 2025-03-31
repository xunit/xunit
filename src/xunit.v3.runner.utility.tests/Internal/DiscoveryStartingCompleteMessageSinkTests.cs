using Xunit;
using Xunit.Internal;
using Xunit.Sdk;

public class DiscoveryStartingCompleteMessageSinkTests
{
	[Fact]
	public void NoTestCases()
	{
		var spySink = SpyMessageSink.Capture();
		var sink = new DiscoveryStartingCompleteMessageSink("display-name", "assembly-file", "config-file", spySink);

		sink.Finish();

		// When there are no test cases, the unique ID is calculated using the constructor arguments
		var assemblyUniqueID = UniqueIDGenerator.ForAssembly("assembly-file", "config-file");
		Assert.Collection(
			spySink.Messages,
			msg =>
			{
				var starting = Assert.IsAssignableFrom<IDiscoveryStarting>(msg);
				Assert.Equal("display-name", starting.AssemblyName);
				Assert.Equal("assembly-file", starting.AssemblyPath);
				Assert.Equal(assemblyUniqueID, starting.AssemblyUniqueID);
				Assert.Equal("config-file", starting.ConfigFilePath);
			},
			msg =>
			{
				var complete = Assert.IsAssignableFrom<IDiscoveryComplete>(msg);
				Assert.Equal(assemblyUniqueID, complete.AssemblyUniqueID);
				Assert.Equal(0, complete.TestCasesToRun);
			}
		);
	}

	[Fact]
	public void TwoTestCases()
	{
		var discovery1 = TestData.TestCaseDiscovered(assemblyUniqueID: "assembly-id-1");
		var discovery2 = TestData.TestCaseDiscovered(assemblyUniqueID: "assembly-id-2");
		var spySink = SpyMessageSink.Capture();
		var sink = new DiscoveryStartingCompleteMessageSink("display-name", "assembly-file", "config-file", spySink);

		sink.OnMessage(discovery1);
		sink.OnMessage(discovery2);
		sink.Finish();

		// When there are test cases, the unique ID used for starting/complete is the one from the first discovered test case.
		// In normal usage, there will only ever be a single unique ID, so what we're testing here is that we _aren't_ using
		// the fabricated ID that is used when there were no test cases discovered.
		Assert.Collection(
			spySink.Messages,
			msg =>
			{
				var starting = Assert.IsAssignableFrom<IDiscoveryStarting>(msg);
				Assert.Equal("display-name", starting.AssemblyName);
				Assert.Equal("assembly-file", starting.AssemblyPath);
				Assert.Equal("assembly-id-1", starting.AssemblyUniqueID);
				Assert.Equal("config-file", starting.ConfigFilePath);
			},
			msg => Assert.Same(discovery1, msg),
			msg => Assert.Same(discovery2, msg),
			msg =>
			{
				var complete = Assert.IsAssignableFrom<IDiscoveryComplete>(msg);
				Assert.Equal("assembly-id-1", complete.AssemblyUniqueID);
				Assert.Equal(2, complete.TestCasesToRun);
			}
		);
	}
}
