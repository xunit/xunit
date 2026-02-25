using Xunit;
using Xunit.v3;

partial class TestRunnerBaseTests
{
	[Collection(typeof(SpyEventListener))]
	public class EventSourceLogging
	{
		// Dynamic skip behavior doesn't exist at this low level, so we only test static skipping.
		// It could be simulated with a custom context, but we test the actual behavior at layers above,
		// which will de-facto test the ability of the context to permit dynamic skipping.
		public static IEnumerable<TheoryDataRow<Func<ValueTask<RunSummary>>, string>> EventSourceData()
		{
			yield return new(() => new TestableTestRunnerBase().Run(), "Passed");
			yield return new(() => new TestableTestRunnerBase() { ShouldTestRun__Result = false }.Run(), "NotRun");
			yield return new(() => new TestableTestRunnerBase().Run("Don't run me"), "Skipped");
			yield return new(() => new TestableTestRunnerBase() { RunTest__Lambda = () => Assert.Fail() }.Run(), "Failed");
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(EventSourceData))]
		public async ValueTask LogsEvents(
			Func<ValueTask<RunSummary>> runner,
			string expectedResult)
		{
			var listener = new SpyEventListener();

			try
			{
				await runner();
			}
			finally
			{
				listener.Dispose();
			}

			var events = await listener.WaitForEventCount(2);
			Assert.Collection(
				events,
				@event => Assert.Equal(@"[TestStart] testName = ""test-display-name""", @event),
				@event => Assert.Equal(@$"[TestStop] testName = ""test-display-name"", result = ""{expectedResult}""", @event)
			);
		}
	}
}
