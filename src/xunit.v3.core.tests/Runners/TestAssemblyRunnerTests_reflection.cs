using Xunit;

partial class TestAssemblyRunnerTests
{
	[Collection(typeof(SpyEventListener))]
	public class EventSourceLogging
	{
		[Fact]
		public async ValueTask LogsEvents()
		{
			var listener = new SpyEventListener();
			var runner = new TestableTestAssemblyRunner();

			try
			{
				await runner.RunAsync();
			}
			finally
			{
				listener.Dispose();
			}

			var events = await listener.WaitForEventCount(2);
			Assert.Collection(
				events,
				@event => Assert.Equal(@"[TestAssemblyStart] assemblyPath = ""./test-assembly.dll"", configFileName = ""<none>""", @event),
				@event => Assert.Equal(@"[TestAssemblyStop] assemblyPath = ""./test-assembly.dll"", configFileName = ""<none>""", @event)
			);
		}
	}
}
