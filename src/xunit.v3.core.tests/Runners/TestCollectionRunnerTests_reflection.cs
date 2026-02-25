using Xunit;

partial class TestCollectionRunnerTests
{
	[Collection(typeof(SpyEventListener))]
	public class EventSourceLogging
	{
		[Fact]
		public async ValueTask LogsEvents()
		{
			var listener = new SpyEventListener();
			var testCase = Mocks.TestCase();
			var runner = new TestableTestCollectionRunner([testCase]);

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
				@event => Assert.Equal(@"[TestCollectionStart] testCollectionName = ""test-collection-display-name""", @event),
				@event => Assert.Equal(@"[TestCollectionStop] testCollectionName = ""test-collection-display-name""", @event)
			);
		}
	}
}
