using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestCollectionFinished"/>.
/// </summary>
[JsonTypeID("test-collection-finished")]
sealed partial class TestCollectionFinished : TestCollectionMessage, ITestCollectionFinished
{
	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(ExecutionTime), ExecutionTime);
		serializer.Serialize(nameof(TestsFailed), TestsFailed);
		serializer.Serialize(nameof(TestsNotRun), TestsNotRun);
		serializer.Serialize(nameof(TestsSkipped), TestsSkipped);
		serializer.Serialize(nameof(TestsTotal), TestsTotal);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} total={1} failed={2} skipped={3} notRun={4} time={5}", base.ToString(), TestsTotal, TestsFailed, TestsSkipped, TestsNotRun, ExecutionTime);
}
