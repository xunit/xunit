using Xunit;

partial class TestCaseRunnerBaseTests
{
	[Collection(typeof(SpyEventListener))]
	public class EventSourceLogging
	{
		[Fact]
		public async ValueTask LogsEvents()
		{
			var listener = new SpyEventListener();
			var testCase = Mocks.TestCase();
			var runner = new TestableTestCaseRunnerBase(testCase);

			try
			{
				await runner.Run();
			}
			finally
			{
				listener.Dispose();
			}

			var events = await listener.WaitForEventCount(2);
			Assert.Collection(
				events,
				@event => Assert.Equal(@"[TestCaseStart] testCaseName = ""test-case-display-name""", @event),
				@event => Assert.Equal(@"[TestCaseStop] testCaseName = ""test-case-display-name""", @event)
			);
		}
	}
}
