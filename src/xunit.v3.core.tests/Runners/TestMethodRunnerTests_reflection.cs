using Xunit;

partial class TestMethodRunnerTests
{
	[Collection(typeof(SpyEventListener))]
	public class EventSourceLogging
	{
		[Fact]
		public async ValueTask LogsEvents()
		{
			var listener = new SpyEventListener();
			var testCase = Mocks.TestCase();
			var runner = new TestableTestMethodRunner([testCase]);

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
				@event => Assert.Equal(@"[TestMethodStart] testMethodName = ""test-class-name.test-method""", @event),
				@event => Assert.Equal(@"[TestMethodStop] testMethodName = ""test-class-name.test-method""", @event)
			);
		}
	}
}
