using Xunit;

partial class TestClassRunnerTests
{
	[Collection(typeof(SpyEventListener))]
	public class EventSourceLogging
	{
		[Fact]
		public async ValueTask LogsEvents()
		{
			var listener = new SpyEventListener();
			var testCase = Mocks.TestCase();
			var runner = new TestableTestClassRunner([testCase]);

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
				@event => Assert.Equal(@"[TestClassStart] testClassName = ""test-class-name""", @event),
				@event => Assert.Equal(@"[TestClassStop] testClassName = ""test-class-name""", @event)
			);
		}
	}
}
